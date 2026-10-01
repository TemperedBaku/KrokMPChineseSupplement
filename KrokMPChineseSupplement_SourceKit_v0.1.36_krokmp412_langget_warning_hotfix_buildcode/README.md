# KrokMP Chinese Supplement v0.1.36 Beta

这是用于 **Casualties Unknown Demo《未知伤亡》** 的 **KrokMP / Krokosha_MP_CU / CO-OP MOD 简体中文补充汉化补丁**。

本补丁不是 `维基中文 v1.6.5.json` 这类大型游戏内容汉化包的替代品，也不会修改它。  
本补丁只补充 KrokMP / CO-OP MOD 相关文本，包括联机菜单、服务器浏览器、规则菜单、tooltip、聊天提示、状态消息、Bug Report 页面，以及部分 KrokMP 硬编码 / 动态 UI 文本。

## 适配版本

- 游戏：Casualties Unknown Demo v7.0.1
- 联机 Mod：KrokMP / Krokosha_MP_CU / CO-OP MOD v4.0.1
- 环境：BepInEx 5

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

## v0.1.36 Beta 更新内容

- 收窄全局 UI / TMP / IMGUI 翻译范围，减少误翻 QoL Unknown 等其他 Mod 设置项。
- 默认启用 KrokMP `Lang.Get` 兜底与 `Locale.currentLang.other` 注入，用于补充 `krokosha_coop_*` 文本。
- 延迟 KrokMP 专用 patch，减少 KrokMP 本体加载前出现的无意义 TypeByName warning。
- 默认关闭不稳定的玩家交互按钮 IL patch。
- 修复右下角聊天提示 `Press "/" to chat...` 漏翻。
- 继续保留 KrokMP 4.0.1 的服务器浏览器、规则菜单、规则 tooltip、状态提示、房间密码、房间 ID 等文本补充。

## 作用域说明

本补丁只处理 KrokMP / CO-OP MOD 相关文本。

以下内容不属于本补丁范围：

- QoL Unknown 设置项
- 维基中文 / 游戏本体物品描述
- 其他 Mod 的菜单、设置项、提示文本
- 玩家名 Unicode 网络传输修复
- KrokMP 联机协议、Steamworks、房间连接、数据包、世界同步逻辑

如果 QoL Unknown 或其他 Mod 的设置项仍为英文，这是正常现象。请使用对应 Mod 的专用汉化补丁。

## 已知说明

1. Discord IPC timeout / pipe close 报错通常来自 KrokMP / Discord RPC，不是本补丁导致。
2. 玩家 Steam 中文名如果显示为 `??????`，属于 KrokMP 本体的玩家名处理限制。本补丁只负责汉化和 UI 文本兜底，不提供完整 Unicode 玩家名网络修复。
3. 如果同时安装多个汉化补丁，可能出现重复翻译、半英半中、文本被其他补丁覆盖等情况。
4. 如果发现 KrokMP 文本漏翻或翻译回退英文，请提供截图和 `BepInEx/LogOutput.log`。

## 更新记录

### v0.1.36 Beta

- 收窄 KrokMP Chinese Supplement 的动态文本捕获范围。
- 减少对 QoL Unknown 等其他 Mod UI 文本的误处理。
- 增加 KrokMP 语言表 key 的兜底处理。
- 降低无意义日志噪音。
- 修复 `Press "/" to chat...` 漏翻。

### v0.1.33 Beta

- 修复 Discord Rich Presence 设置项中的混合显示问题。
- `Discord RPC Join button` 统一为“显示 Discord 加入按钮”。
- `Discord Only Ask to Join` 统一为“Discord 仅发送加入请求”。

### v0.1.32 Beta

- 修复 `ClosedByPeer` / `关闭dByPeer` 连接状态错翻。
- 将连接关闭状态统一为“连接已关闭：对方关闭连接”。

### v0.1.31 Beta

- 将 `Hide locked` 改为“隐藏密码房”。
- 将 `Copy Lobby Id` 改为“复制房间 ID”。
- 改善房间密码相关文本。

### v0.1.30 Beta

- 统一房间密码、房间 ID、密码保护相关术语。

### v0.1.29 Beta

- 清理不属于 KrokMP 的非联机文本。

### v0.1.28 Beta

- 补充 KrokMP 4.0.1 的服务器浏览器、规则菜单、教程提示与动态状态文本。

### v0.1.27 Beta

- 修复规则菜单中 `Player` 被误处理成“播放er / 播放器”的问题。
- 补充规则 tooltip 文本。

### v0.1.25 Beta

- 适配 KrokMP / CO-OP MOD v3.1.2。
- 修复 Steam 用户名标签在部分 GUI 路径下缺少分隔符的问题。

### v0.1.23 Beta

- 适配 KrokMP / CO-OP MOD v3.1.0。
- 补充大逃杀模式、TPS / FPS、AlwaysAllowCarry、Toggle to talk 等文本。

## 版权与声明

本仓库不包含：

- 游戏本体文件
- KrokMP / CO-OP MOD 原版 DLL
- KrokMP / CO-OP MOD 原版资源
- 反编译后的 KrokMP 源码
- 游戏原始资源

KrokMP / CO-OP MOD 归其原作者所有。  
本项目仅包含独立的 BepInEx 补充汉化插件与简体中文补充词表。
