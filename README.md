# CS2-CSkins-QR

一个基于 Counter-Strike 2 (CS2) 的插件，允许玩家通过扫描二维码来更换游戏内皮肤。

## 功能特性

- 🎮 **游戏内二维码显示** - 在游戏中心显示皮肤更换二维码
- 📱 **手机扫码换肤** - 玩家可使用手机扫描二维码更换皮肤
- 🔄 **实时皮肤更新** - 扫码后皮肤立即生效
- 🎯 **玩家专属二维码** - 每个玩家都有独立的二维码
- ⚡ **高性能** - 使用异步HTTP请求，不影响游戏性能

## 安装要求

- Counter-Strike 2 服务器
- CounterStrikeSharp API
- .NET 8.0 Runtime
- 皮肤更换Web服务（需要配置WebUrl）

## 安装步骤

### 1. 编译插件

```bash
# 使用提供的构建脚本
build.bat
```

或者手动编译：

```bash
dotnet build CS2-CSkins-QR.csproj
```

### 2. 部署插件

将以下文件复制到CS2服务器插件目录：
- `CS2-CSkins-QR.dll`
- `CS2-CSkins-QR.pdb` 
- `CS2-CSkins-QR.deps.json`

默认目标目录：`F:\csgoserver_win\cs2\game\csgo\addons\counterstrikesharp\plugins\CS2-CSkins-QR\`

### 3. 配置插件

在 `addons/counterstrikesharp/configs/plugins/CS2-CSkins-QR` 目录下创建配置文件：

```json
{
  "WebUrl": "https://your-skin-service.com",
  "Version": 1
}
```

**配置说明：**
- `WebUrl`: 皮肤更换服务的URL地址

## 使用方法

### 玩家命令

在游戏聊天框中输入：

```
css_cskin
```

### 操作流程

1. 玩家输入 `css_cskin` 命令
2. 游戏中心显示专属二维码
3. 玩家使用手机扫描二维码
4. 在网页中选择想要更换的皮肤
5. 皮肤立即在游戏中生效
6. 按鼠标右键退出二维码显示

## 项目结构

```
CS2-CSkins-QR/
├── CS2-CSkins-QR.cs          # 主插件逻辑
├── Config.cs                 # 配置文件类
├── HttpUtils.cs             # HTTP工具类
├── CS2-CSkins-QR.csproj     # 项目文件
├── CS2-CSkins-QR.sln        # 解决方案文件
├── build.bat                # 构建脚本
└── .gitignore              # Git忽略文件
```

## 技术实现

### 核心组件

- **CS2-CSkins-QR.cs**: 主插件类，处理游戏事件和玩家交互
- **HttpUtils.cs**: 提供HTTP GET/POST请求功能
- **Config.cs**: 插件配置管理

### 主要功能

1. **二维码生成**: 通过Web服务为每个玩家生成专属二维码
2. **实时显示**: 在游戏中心持续显示二维码图片
3. **玩家状态管理**: 使用线程安全的ConcurrentDictionary管理玩家状态
4. **事件处理**: 处理玩家连接/断开事件

## 开发环境

- **目标框架**: .NET 8.0
- **依赖**: CounterStrikeSharp.API
- **开发工具**: Visual Studio / VS Code

## 构建说明

项目使用标准的.NET构建流程：

```bash
dotnet build CS2-CSkins-QR.csproj
```

构建脚本 `build.bat` 会自动编译并将文件复制到目标目录。

## 许可证

本项目基于 CounterStrikeSharp API 开发。

## 支持与反馈

如有问题或建议，请访问：https://bbs.csgocn.net/

## 版本信息

- 当前版本: 0.0.1
- 作者: https://bbs.csgocn.net/
- 最后更新: 2025-11-22