# PlayerBadge

Language: [English](#english) | [中文](#中文)

## English

`PlayerBadge` is a LabAPI `net48` custom title plugin. It assigns per-player server role text and color through LabAPI `GroupName`/`GroupColor`, including single-color titles, inline multi-color titles, and a `rainbow` color mode that cycles colors.

### Installation

1. Build `PlayerBadge.dll`.
2. Place it in `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\<port>\` or `global`.
3. Start the server once so LabAPI creates `config.yml`.

### Configuration

If `config_file_path` is empty, the title list is stored as `PlayerBadge.txt` under this plugin's LabAPI config folder.

Title file format:

```text
playerId@platform:color:title
76561198000000000@steam:red:Admin
123456789@discord:rainbow:VIP
76561198111111111@steam:multi:red=Admin|cyan=Helper
76561198222222222@steam:rich:<color=#ff5577>Pink</color><color=#55ddff>Blue</color>
```

Supported colors: `red`, `yellow`, `cyan`, `green`, `aqua`, `pink`, `white`, `orange`, `rainbow`, `multi`, `rich`.

Use `multi` for one or more compact segments in `color=text|color=text` form. Use `rich` when the title already contains SCP:SL/Unity rich text such as `<color=#ff5577>Pink</color>`.

### Commands

Remote Admin and game console:

```text
pbadge set <player|userId> <color> <title...>
pbadge remove <player|userId>
pbadge reload
pbadge list
```

Aliases: `playerbadge`, `ptag`. RA players need `playerbadge.manage`; the game console is allowed. `set` persists the title to `PlayerBadge.txt`, reloads the file, and immediately applies it to online matching players. If the target is online, a nickname or partial user ID can be used; otherwise use a full ID such as `76561198000000000@steam`.

### Known Conflicts

This plugin owns `GroupName`/`GroupColor` while a matching title is applied. It stores and restores the previous group text/color on disable or when a player no longer matches a title entry. Do not run another plugin that continuously writes group text for the same players.

## 中文

`PlayerBadge` 是 LabAPI `net48` 自定义称号插件。它通过 LabAPI 的 `GroupName`/`GroupColor` 为玩家设置服务器称号和颜色，支持单色称号、称号内部多重颜色，以及 `rainbow` 彩色循环。

### 安装

1. 构建 `PlayerBadge.dll`。
2. 放入 `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\<端口>\` 或 `global`。
3. 启动一次服务器，让 LabAPI 创建 `config.yml`。

### 配置

如果 `config_file_path` 留空，称号列表会保存在本插件 LabAPI 配置目录下的 `PlayerBadge.txt`。

称号文件格式：

```text
玩家ID@平台:颜色:称号
76561198000000000@steam:red:管理员
123456789@discord:rainbow:VIP
76561198111111111@steam:multi:red=管理员|cyan=助手
76561198222222222@steam:rich:<color=#ff5577>粉色</color><color=#55ddff>蓝色</color>
```

支持颜色：`red`、`yellow`、`cyan`、`green`、`aqua`、`pink`、`white`、`orange`、`rainbow`、`multi`、`rich`。

`multi` 使用一个或多个 `颜色=文字|颜色=文字` 简写片段。`rich` 用于已经写好 SCP:SL/Unity 富文本的称号，例如 `<color=#ff5577>粉色</color>`。

### 命令

Remote Admin 和游戏控制台：

```text
pbadge set <玩家|用户ID> <颜色> <称号...>
pbadge remove <玩家|用户ID>
pbadge reload
pbadge list
```

别名：`playerbadge`、`ptag`。RA 玩家需要 `playerbadge.manage` 权限；游戏控制台允许使用。`set` 会把称号持久化到 `PlayerBadge.txt`，重新加载文件，并立即应用到在线匹配玩家。如果目标在线，可以使用昵称或部分用户 ID；离线玩家请使用完整 ID，例如 `76561198000000000@steam`。

### 已知冲突

玩家匹配称号时，本插件会接管该玩家的 `GroupName`/`GroupColor`。插件禁用或玩家不再匹配称号时，会恢复原称号和颜色。不要同时运行另一个持续写入同一玩家服务器称号的插件。
