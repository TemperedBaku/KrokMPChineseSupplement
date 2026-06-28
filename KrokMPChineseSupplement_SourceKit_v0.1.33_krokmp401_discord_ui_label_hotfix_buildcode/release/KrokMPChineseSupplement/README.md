# KrokMP Chinese Supplement v0.1.33 Beta

## v0.1.33 重点

- 修复 Steam 连接关闭状态中的 `ClosedByPeer` / `关闭dByPeer` 半翻译问题。
- 将 `Steam: Connection closed: ClosedByPeer` 统一显示为“Steam：连接已关闭：对方关闭连接”。
- 保留 v0.1.31 的房间密码、隐藏密码房、房间ID 术语修正。
- 继续保持作用域边界：只处理 KrokMP / CO-OP MOD 相关文本，不加入未确认属于 KrokMP 的游戏本体或其他 Mod 文本。



这是用于 **Casualties Unknown Demo《未知伤亡》** 的 **KrokMP / Krokosha_MP_CU / CO-OP MOD 简体中文补充汉化补丁**。

本补丁不是 `维基中文 v1.6.5.json` 这类大型游戏内容汉化包的替代品，也不会修改它。  
本补丁只补充 KrokMP / CO-OP MOD 中未走原有语言表的硬编码英文、动态状态文本、服务器浏览器文本、规则菜单文本、聊天/提示文本、Bug Report 页面文本以及部分调试/状态文本。

## 适配版本

- 游戏：Casualties Unknown Demo v7.0.1
- 联机 Mod：KrokMP / Krokosha_MP_CU / CO-OP MOD v4.0.1
- 环境：BepInEx 5

## v0.1.33 重点

- 适配 KrokMP / CO-OP MOD v4.0.1。
- 补全 KrokMP 4.0.1 的房间浏览器、规则菜单、规则 tooltip、状态提示和联机教程相关文本。
- 将 `Last Stand` 相关译名统一为“背水一战”。
- 移除 v0.1.28 中不属于 KrokMP 的通用 UI fallback，例如 `You're almost there. Just a couple more steps.` 和 `Also try CUCoreLib!`。
- 本补丁只覆盖 KrokMP / CO-OP MOD 相关文本；非 KrokMP、非联机 Mod 本体的内容不再主动加入。


## 安装

下载 Release 中的发布包，将整个 `KrokMPChineseSupplement` 文件夹放入：

```text
BepInEx/plugins/
```

最终结构应类似：

```text
BepInEx/plugins/KrokMPChineseSupplement/
├─ KrokMPChineseSupplement.dll
├─ translations.zh-CN.json
├─ phrases.zh-CN.json
└─ README.md
```

## 卸载

删除以下文件夹即可：

```text
BepInEx/plugins/KrokMPChineseSupplement/
```

如需彻底清理，也可以删除配置文件：

```text
BepInEx/config/casualtiesunknown.krokmpchinesesupplement.cfg
```

## 词表

- `translations.zh-CN.json`：精确翻译、带占位符翻译、动态文本兜底。
- `phrases.zh-CN.json`：短语替换、状态栏/聊天/tooltip 兜底。

多数普通补词只需要修改 JSON，不需要重新编译 DLL。  
如果新增的是新的 patch 入口、动态格式规则或版本号，则需要重新编译 DLL。

## 编译

Windows PowerShell 示例：

```powershell
cd "D:\ModWork\KrokMPChineseSupplement_SourceKit_v0.1.33_krokmp401_discord_ui_label_hotfix_buildcode"
.\build.ps1 -GameDir "D:\steam\steamapps\common\Casualties Unknown Demo"
```

如果你的游戏路径不同，请把 `-GameDir` 改成实际游戏目录。

编译成功后会生成：

```text
release/KrokMPChineseSupplement/KrokMPChineseSupplement.dll
release/KrokMPChineseSupplement/translations.zh-CN.json
release/KrokMPChineseSupplement/phrases.zh-CN.json
KrokMPChineseSupplement_v0.1.33_Beta_krokmp401_discord_ui_label_hotfix.zip
```

## 当前补丁入口

本补丁会尝试拦截和补充以下文本入口：

- `UnityEngine.UI.Text.text`
- `TMPro.TMP_Text.text`
- `GUI` / `GUILayout` 常见文本方法
- `GUIStyle.Draw` / `DrawCursor` 后段渲染路径
- `PlayerCamera` 的 Alert / DoAlert 类方法
- `ConsoleScript` 的 Log / Console 类方法
- KrokMP 语言表与 `krokosha_coop_*` 键的兜底翻译
- `KrokoshaScavMultiplayer.DoMultiplayerStatusMessageLog/Error` 最后状态信息栏
- `Chat.Server_ChatAnnouncement` / 系统聊天消息
- `GUILayout_DropdownMenu.Dropdown` 下拉菜单选项

本补丁不会修改：

- KrokMP 联机协议
- Steamworks 初始化
- 房间创建 / 加入逻辑
- 网络包
- 玩家验证
- 世界同步
- 菜单对象生命周期
- 玩家名网络传输逻辑

## 已知说明

1. Discord IPC timeout / pipe close 报错通常来自 KrokMP / Discord RPC，不是本补丁导致。
2. 玩家 Steam 中文名如果显示为 `??????`，属于 KrokMP 本体的玩家名处理限制。本补丁只负责汉化和 UI 文本兜底，不提供完整 Unicode 玩家名网络修复。
3. 如果安装了单独的 KrokMP 中文名修复补丁，Steam 用户名标签的最终显示可能还会受该补丁的 GUI patch 影响；v0.1.25 起已加入渲染层分隔符 fallback。
4. 如果发现未翻译文本，请提供截图和 `BepInEx/LogOutput.log`。

## 更新记录

### v0.1.33 Beta

- 适配 KrokMP / CO-OP MOD v4.0.1。
- 补全 KrokMP 4.0.1 新版语言表缺失项。
- 新增/修正复制房间 ID、返回主菜单、规则变更、规则默认、鼠标移开提示等文本。
- 新增/修正 IP 封禁、文字聊天静音、语音聊天静音、玩家静音状态文本。
- 新增/修正强制模组列表一致及其 tooltip。
- 新增/修正服务器浏览器中的隐藏不兼容房间、调试世界、模组/玩家/规则列表、加载中、层级、总玩家数等文本。
- 新增/修正 Discord RPC 加入请求相关文本。
- 新增 KrokMP 4.0.1 版本、加载失败、资源加载失败、存档版本不匹配等文本。
- 修正 `Found Lobbies: {0}   Shown: {1}   Total Players: {2}` 的中文格式。

### v0.1.25 Beta

- 适配 KrokMP / CO-OP MOD v3.1.2。
- 补充 `Hide locked`、`Current VC Talk Keybind`、`PACKET STABILITY` 等文本。
- 补充网络调试 tooltip 统计文本。
- 修复 Steam 用户名标签在部分 GUI 路径下缺少分隔符的问题。

### v0.1.24 Beta

- 统一 KrokMP 大厅里的 Steam 名称标签。
- 改善 KrokMP Unicode Name Fix 恢复完整中文 Steam 名后大厅界面拥挤的问题。

### v0.1.23 Beta

- 适配 KrokMP / CO-OP MOD v3.1.0。
- 适配 Casualties Unknown Demo v7.0.1。
- 补充大逃杀模式、TPS / FPS、AlwaysAllowCarry、Toggle to talk 等文本。

### v0.1.22 Beta

- 补全 KrokMP 3.0.0 主要联机菜单、服务器浏览器、规则菜单、状态栏、聊天提示和 tooltip 文本。
- 保留中文规则搜索。
- 默认关闭可能导致游戏内卡顿的广域 FocusableButton patch。

## 版权与声明

本仓库不包含：

- 游戏本体文件
- KrokMP / CO-OP MOD 原版 DLL
- KrokMP / CO-OP MOD 原版资源
- 反编译后的 KrokMP 源码
- 游戏原始资源

KrokMP / CO-OP MOD 归其原作者所有。  
本项目仅包含独立的 BepInEx 补充汉化插件与简体中文补充词表。