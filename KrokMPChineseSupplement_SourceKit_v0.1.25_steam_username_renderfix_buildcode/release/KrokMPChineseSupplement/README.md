
## Steam 用户名分隔符渲染修复说明

如果安装了 `KrokMP Chinese Name Fix` 这类会再次 patch IMGUI / GUIStyle 的中文名修复补丁，单纯用 JSON 规则有时无法把 `Steam用户名玩家名` 改成 `Steam用户名：玩家名`。

本 SourceKit 增加了代码层 postprocess 和更靠近最终渲染路径的 `GUIStyle.Draw` 覆盖，因此必须重新编译 DLL。只替换 JSON 不足以测试这个修复。

# KrokMP Chinese Supplement v0.1.25

这是用于 **Casualties Unknown Demo《未知伤亡》** 的 **KrokMP / Krokosha_MP_CU / CO-OP MOD 简体中文补充汉化补丁**。

本补丁不是 `维基中文 v1.6.5.json` 这类大型游戏内容汉化包的替代品，也不会修改它。  
本补丁只补充 KrokMP / CO-OP MOD 中未走原有语言表的硬编码英文、动态状态文本、服务器浏览器文本、规则菜单文本、聊天/提示文本和大逃杀模式文本。

## 适配版本

- 游戏：Casualties Unknown Demo v7.0.1
- 联机 Mod：KrokMP / Krokosha_MP_CU / CO-OP MOD v3.1.2
- 环境：BepInEx 5

## 说明

当前游戏/汉化环境已具备中文字体显示支持，不再需要额外推荐安装 XTMP 字体补丁。  
如果你使用的是旧版本游戏或旧版联机 Mod，可能会出现部分文本未翻译或补丁入口失效。

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

## 主要功能

本补丁会尝试补全：

- KrokMP 联机菜单
- 服务器浏览器
- 创建房间 / 加入房间界面
- Steam 大厅类型、房间状态、距离筛选等文本
- 规则设置页面
- 规则中文搜索
- 聊天栏提示与系统消息
- 最后状态信息栏
- 多人游戏内状态面板
- 多人互动相关提示
- 规则悬浮说明 tooltip
- KrokMP 3.1.2 新增文本
- 大逃杀模式相关文本
- TPS / FPS / Packet Stability 显示文本
- 新版地点名与心情状态文本
- Bug Report 页面与部分调试统计文本

## 当前补丁入口

本补丁会尝试拦截和补充以下文本入口：

- `UnityEngine.UI.Text.text`
- `TMPro.TMP_Text.text`
- `GUI` / `GUILayout` 常见文本方法
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
- 玩家名逻辑

## 配置

配置文件位于：

```text
BepInEx/config/casualtiesunknown.krokmpchinesesupplement.cfg
```

正常使用无需修改。

为了避免联机中卡顿，泛用 IMGUI 游戏内翻译默认采用保守策略。  
本补丁优先使用定向 patch、词表和动态文本兜底，而不是在游戏场景中无差别扫描所有 GUI 文本。

## 已知说明

1. Discord IPC timeout / pipe close 报错通常来自 KrokMP / Discord RPC，不是本补丁导致。
2. 大逃杀模式本身仍可能存在 KrokMP 原版逻辑问题，本补丁只负责补充相关文本。
3. 玩家 Steam 中文名如果显示为 `??????`，属于 KrokMP 本体的玩家名处理限制。本补丁不再尝试替换 Steam 中文名，以避免破坏名牌布局。
4. 如果发现未翻译文本，请提供截图和 `BepInEx/LogOutput.log`。

## 更新记录

### v0.1.25 Beta

- 适配 KrokMP / CO-OP MOD v3.1.2。
- 保持 Casualties Unknown Demo v7.0.1 适配。
- 补充新版 `Hide locked` / 隐藏锁定房间文本。
- 补充新版 `Current VC Talk Keybind` 文本。
- 补充联机设置中“已经在游戏中，请断开连接并返回主菜单”的完整提示。
- 补充 `PACKET STABILITY` / 连接稳定度显示。
- 补充网络调试 tooltip 统计文本，例如 ConnectionQuality、PacketsPerSec、BytesPerSec、PacketLoss 等。
- 补充 KrokMP 3.1.2 加载失败提示与版本相关文本。
- 修正 `FPS: {0}   TPS: {1}` 中 TPS 的中文表达为“刻率”。

### v0.1.24 Beta

- 统一 KrokMP 大厅里的 Steam 名称标签：`Steam Username` / `Steam用户名称` → `Steam用户名`（避免大厅界面重复冒号）。
- 改善 KrokMP Unicode Name Fix 恢复完整中文 Steam 名后大厅界面拥挤的问题。
- 清理 README 版本和依赖说明，删除过时的 XTMP / Steam 显示名实验说明。

### v0.1.23 Beta

- 适配 KrokMP / CO-OP MOD v3.1.0。
- 适配 Casualties Unknown Demo v7.0.1。
- 补充大逃杀模式相关文本。
- 补充 TPS / FPS 显示文本。
- 补充 `AlwaysAllowCarry` 规则项与 tooltip。
- 补充新版语音模式文本，例如 `Toggle to talk`。
- 从维基中文 v1.6.5 补充新版地点名与心情状态文本。
- 补充 KrokMP 3.1.0 新增 `krokosha_coop_*` 语言 key。
- 移除实验性 Steam 中文显示名替换逻辑，避免玩家名牌布局被挤坏。

### v0.1.22 Beta

- 补全 KrokMP 3.0.0 主要联机菜单、服务器浏览器、规则菜单、状态栏、聊天提示和 tooltip 文本。
- 保留中文规则搜索。
- 默认关闭可能导致游戏内卡顿的广域 FocusableButton patch。
- 不修改 KrokMP 网络协议、玩家名验证、网络包或同步逻辑。

## 反馈

如果发现未翻译文本、显示错位、异常卡顿或联机问题，请尽量提供：

- 问题截图
- `BepInEx/LogOutput.log`
- 使用的游戏版本
- 使用的 KrokMP / CO-OP MOD 版本
- 是否安装其他 Mod 或汉化包
- 出现问题的操作步骤

## 版权与声明

本仓库不包含：

- 游戏本体文件
- KrokMP / CO-OP MOD 原版 DLL
- KrokMP / CO-OP MOD 原版资源
- 反编译后的 KrokMP 源码
- 游戏原始资源

KrokMP / CO-OP MOD 归其原作者所有。  
本项目仅包含独立的 BepInEx 补充汉化插件与简体中文补充词表。


Hotfix note: Steam username label in the Steam lobby now uses `Steam用户名：` for label strings and keeps standalone `Steam用户名` without a separator, avoiding both `Steam用户名：:` and `Steam用户名玩家名` display issues.


Hotfix note: Added placeholder fallback rules `Steam Username{0}` / `Steam Username {0}` so KrokMP UI paths that concatenate the label and persona name without a separator render as `Steam用户名：玩家名`. This hotfix changes translation rules only.
