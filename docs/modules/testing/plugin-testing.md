# PluginSystem 测试设计

版本：v0.1
状态：正式初版
适用范围：插件包、发现、安装、加载、启用、停用、卸载、更新、回滚、UnloadPending 和安全策略

## 1. 目标

PluginSystem 测试必须证明插件运行时扩展可安装、可加载、可撤销、可卸载、可回滚，并且不会污染 Host。

## 2. PluginTestHost

Testing 提供：

- fake plugin package builder。
- fake plugin source。
- fake package cache。
- fake installed directory。
- fake lock file。
- fake plugin manifest。
- fake contribution manifest。
- plugin lifecycle driver。
- unload assertion helper。
- trust policy fake。

## 3. 包和安装测试

必须覆盖：

- 标准包安装。
- 本地包安装。
- staging。
- package hash。
- content hash。
- manifest validation。
- required contribution manifest。
- install record。
- lock file。
- 安装失败恢复。

## 4. 生命周期测试

必须覆盖：

- discover。
- verify。
- load。
- activate。
- deactivate。
- unload。
- disabled。
- faulted。
- invalid。
- UnloadPending。

状态机必须拒绝非法转换。

## 5. 卸载测试

必须断言：

- 新入口被阻止。
- active route 被关闭。
- Operation 被取消。
- EventBus subscription 被释放。
- State subscription 被释放。
- Data connection 被停止。
- Localization resource 被撤销。
- Presentation resource 被撤销。
- Contribution Lease 全部撤销。
- ServiceProvider 被释放。
- AssemblyLoadContext 可释放。

## 6. 更新和回滚测试

必须覆盖：

- side-by-side version install。
- active version switch。
- 更新成功。
- 更新失败回滚。
- rollback failure。
- pending update。
- UnloadPending 阻止删除和覆盖文件。

## 7. 安全测试

必须覆盖：

- unknown source。
- hash mismatch。
- invalid signature。
- capability denied。
- unauthorized contribution。
- private contract leakage。

## 8. 测试隔离

插件测试必须使用测试临时目录，不得使用真实用户插件目录。

插件测试不要求真实 NuGet feed。
