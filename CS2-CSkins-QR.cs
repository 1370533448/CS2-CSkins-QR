using System.Collections.Concurrent;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using HttpUtils;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;

namespace CS2_CSkins_QR;

// [MinimumApiVersion(160)]
public class CS2_CSkins_QR : BasePlugin, IPluginConfig<CS2_CSkins_QRConfig>
{
    public override string ModuleName => "CS2_CSkins_QR";
    public override string ModuleDescription => "Change player skin by scan QR code";
    public override string ModuleAuthor => "https://bbs.csgocn.net/";
    public override string ModuleVersion => "0.0.1";

    public class PlayerQRImageInfo
    {
        public bool IsShowing { get; set; }  // 是否显示图片
        public string ImageUrl { get; set; } // 图片 URL

        public PlayerQRImageInfo(bool isShowing, string imageUrl)
        {
            IsShowing = isShowing;
            ImageUrl = imageUrl;
        }
    }
    public bool bShowingSkinChangeQR = false;
    // 改造后的 ConcurrentDictionary
    public ConcurrentDictionary<ulong, PlayerQRImageInfo> bPlayerSeeingQRImage = new();
    public CS2_CSkins_QRConfig Config { get; set; } = new();

    public class ResponseData
    {
        [JsonPropertyName("success")]  // 显式指定 JSON 字段名
        public bool Success { get; set; }

        [JsonPropertyName("qr_url")]
        public string? QrUrl { get; set; }

        [JsonPropertyName("redirect_url")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("is_new")]
        public bool IsNew { get; set; }
    }

    public void OnConfigParsed(CS2_CSkins_QRConfig config)
	{
        Console.WriteLine($"Skin Change WebUrl: {config.WebUrl}!");
        Config = config;
    }

    public static bool IsPlayerValid(CCSPlayerController? player)
    {
        return player != null
            && player.IsValid
            && !player.IsBot
            && player.Pawn != null
            && player.Pawn.IsValid
            && player.Connected == PlayerConnectedState.PlayerConnected
            && !player.IsHLTV;
    }

    public override void Load(bool hotReload)
    {
        Console.WriteLine($"[INFO] {ModuleName} Loading +++ ");

        if (hotReload)
        {
            Console.WriteLine("[INFO] [CS2_CSkins_QR] hotReload +++ ");
            bPlayerSeeingQRImage.Clear();
            Console.WriteLine("[INFO] [CS2_CSkins_QR] hotReload --- ");
        }
        RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (!IsPlayerValid(player))
                    continue;

                if (bPlayerSeeingQRImage.TryGetValue(player.SteamID, out var value))
                {
                    bool isShowing = value.IsShowing;
                    if (!isShowing) continue;

                    string imageUrl = value.ImageUrl;
                    // Console.WriteLine($"[INFO] [CS2_CSkins_QR] Player {player.SteamID}: IsShowing={isShowing}, ImageUrl={imageUrl}");
                    var buttons = player.Buttons;
                    if (buttons.HasFlag(PlayerButtons.Attack2))
                    {
                        var newValue = new PlayerQRImageInfo(false, imageUrl);
                        bool success = bPlayerSeeingQRImage.TryUpdate(player.SteamID, newValue, value);
                        Console.WriteLine(success ? "[INFO] [CS2_CSkins_QR] 玩家关闭换肤成功" : "[ERROR] [CS2_CSkins_QR] 更新失败（可能被其他线程修改）");
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        player.PrintToCenterHtml($"<img src='{imageUrl}'>");
                        player.PrintToCenter("扫码换肤, 右键退出!");
                    }
                }
                else
                    continue;
            }
        });
        Console.WriteLine($"[INFO] {ModuleName} loaded --- ");
    }

    public void GetPlayerCSkinQRImageUrl(string queryUrl, CCSPlayerController player)
    {
        Task.Run(async () =>
        {
            try
            {
                string? response = await Utils.HttpGetAsync(queryUrl);
                if (response != null)
                {
                    // 处理响应（注意线程安全）
                    Logger.LogInformation(response);
                    // 使用 JsonSerializer 反序列化 JSON 字符串
                    // ResponseData? 表示可能为 null
                    ResponseData? data = JsonSerializer.Deserialize<ResponseData>(response);
                    if (data != null)
                    {
                        // Console.WriteLine($"Success: {data.Success}");
                        // Console.WriteLine($"QrUrl: {data.QrUrl}");
                        // Console.WriteLine($"RedirectUrl: {data.RedirectUrl}");
                        // Console.WriteLine($"IsNew: {data.IsNew}");
                        // 添加或更新某个玩家的 QR 图片信息
                        if (!string.IsNullOrWhiteSpace(data.QrUrl)) {
                            bPlayerSeeingQRImage.AddOrUpdate(
                                player.SteamID,                           // ulong 类型的 Key（玩家ID）
                                new PlayerQRImageInfo(true, data.QrUrl),   // 如果 Key 不存在，创建新对象
                                (key, existing) => new PlayerQRImageInfo(true, data.QrUrl) // 如果 Key 存在，更新对象
                            );
                        } else {
                            string fallback = $"{Config.WebUrl.TrimEnd('/')}" + $"/images_qr/{player.SteamID}.png";
                            bPlayerSeeingQRImage.AddOrUpdate(
                                player.SteamID,
                                new PlayerQRImageInfo(true, fallback),
                                (key, existing) => new PlayerQRImageInfo(true, fallback)
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"后台请求失败: {ex.Message}");
            }
        });
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (!IsPlayerValid(player))
            return HookResult.Continue;

        Console.WriteLine($"[INFO] [CS2_CSkins_QR] Player {player!.PlayerName} has disconnected!");
        bPlayerSeeingQRImage.TryRemove(player!.SteamID, out _);
        return HookResult.Continue;
    }

    [ConsoleCommand("css_cskin", "Pop the skin change QR image")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnCSkinsCommand(CCSPlayerController? player, CommandInfo command)
    {
        string args = command.ArgString;
        Console.WriteLine($"[INFO] [CS2_CSkins_QR] OnCSkinsCommand!");
        if (!IsPlayerValid(player)) return;
        Console.WriteLine($"[INFO] [CS2_CSkins_QR] Player should see skin change QRImage!");
        string baseUrl = Config.WebUrl.TrimEnd('/');
        string entry = (Config.LoginEntryPath ?? string.Empty).Trim();
        string pathSegment = string.IsNullOrEmpty(entry) ? string.Empty : (entry.StartsWith("/") ? entry : "/" + entry);
        string paramName = string.IsNullOrWhiteSpace(Config.LoginQueryParamName) ? "qr" : Config.LoginQueryParamName.Trim();
        string extra = (Config.LoginExtraParams ?? string.Empty).Trim().TrimStart('&');
        string queryParams = $"{paramName}={player!.SteamID}" + (string.IsNullOrEmpty(extra) ? string.Empty : ("&" + extra));
        string queryUrl = $"{baseUrl}{pathSegment}?{queryParams}";
        GetPlayerCSkinQRImageUrl(queryUrl, player);
    }

}
