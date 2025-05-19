using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace CS2_CSkins_QR
{
    public class CS2_CSkins_QRConfig : BasePluginConfig
    {
        public override int Version { get; set; } = 1;

		[JsonPropertyName("WebUrl")]
		public string WebUrl { get; set; } = "YourSkinChangeUrl";

    }
}
