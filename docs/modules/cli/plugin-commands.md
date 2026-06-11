# CLI 插件命令设计

版本：v0.1
状态：正式初版
适用范围：插件 list、inspect、install、update、remove、enable、disable、doctor 命令和 PluginSystem metadata 集成

## 1. 目标

插件命令用于管理本地插件包和插件状态，但不能绕过 PluginSystem 生命周期和安装规则。

CLI 不直接加载插件业务代码。

## 2. 命令

```bash
atomui city plugin list
atomui city plugin inspect <PluginId>
atomui city plugin install <PackagePathOrSource>
atomui city plugin update <PluginId>
atomui city plugin remove <PluginId>
atomui city plugin enable <PluginId>
atomui city plugin disable <PluginId>
atomui city plugin doctor <PluginId>
```

## 3. List 和 Inspect

只读命令：

- 读取插件目录。
- 读取 lock file。
- 读取 install record。
- 读取 manifest。
- 输出 PluginId、version、state、source、capabilities、diagnostics。

## 4. Install 和 Update

写操作必须支持：

- `--dry-run`。
- plan/apply。
- `--json`。
- `--yes`。

流程：

```text
Resolve package
-> Verify package metadata
-> Build install/update plan
-> Check running plugin state
-> Execute PluginSystem installation operation
-> Emit diagnostics
```

规则：

- 不覆盖运行中插件目录。
- `UnloadPending` 时进入 pending 操作。
- hash、签名、来源、capability 授权结果必须进入诊断。

## 5. Enable 和 Disable

规则：

- 修改插件启用状态必须通过 PluginSystem metadata/lock file 规则。
- 禁用不删除安装目录和用户配置。
- 启用前必须执行兼容性和能力检查。

## 6. Remove

规则：

- remove 默认卸载插件包，但保留用户配置和状态。
- 清理用户数据必须显式参数。
- 插件处于 `UnloadPending` 时不能删除文件。

## 7. Doctor

`plugin doctor` 输出：

- manifest 状态。
- lock file 状态。
- package hash。
- signature/trust。
- dependency。
- capability。
- unload pending reason。
- suggested actions。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| plugin list | Unit/CLI | 读取 lock file 和 installed 目录。 |
| plugin inspect | Unit/CLI | 输出 manifest、capabilities、state。 |
| install dry-run | CLI | 不写文件，输出 plan。 |
| update pending | Plugin integration | UnloadPending 进入 pending。 |
| disable | Unit/Plugin | 禁用不删除文件。 |
| remove | Plugin integration | active/unload pending 策略。 |
| doctor | Unit | 诊断字段完整。 |
