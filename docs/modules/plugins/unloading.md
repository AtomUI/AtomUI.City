# PluginSystem 卸载设计

版本：v0.1
状态：正式初版
适用范围：插件停用后卸载、引用释放、卸载重试、UnloadPending 和文件删除约束

## 1. 目标

插件卸载必须保证 Host 不再持有插件程序集、类型、对象、委托、事件订阅、资源或文件句柄。

设计目标：

- 卸载前必须停用插件。
- 所有 Contribution Lease 必须撤销。
- 所有插件 Operation 必须取消。
- 卸载失败进入可诊断的 `UnloadPending`。
- `UnloadPending` 阻止更新和删除文件。

## 2. 前置条件

卸载要求插件处于：

- `Inactive`
- `Faulted` 且已完成贡献回滚
- `Loaded` 但未启用

如果插件仍为 `Active`，Host 必须先执行停用流程。

## 3. 卸载流程

```text
Ensure plugin is inactive
-> Mark Unloading
-> Reject new plugin entry
-> Cancel plugin operations
-> Revoke remaining leases
-> Dispose EventBus subscriptions
-> Remove localization and presentation resources
-> Dispose plugin ServiceProvider
-> Clear plugin diagnostics callbacks
-> Request AssemblyLoadContext unload
-> Run cooperative GC verification
-> Mark Unloaded or UnloadPending
```

## 4. 引用释放

卸载前必须释放：

- RouteScope。
- ActivationScope。
- OperationScope。
- EventBus subscription。
- State subscription。
- Timer。
- Dispatcher callback。
- Data connection。
- SignalR connection。
- gRPC streaming call。
- Localization ResourceDictionary。
- Presentation View/ViewModel 映射。
- Plugin ServiceProvider。

任何 registry 接收插件贡献时，都必须能按 PluginId 和 ContributionId 反查并撤销。

## 5. UnloadPending

`UnloadPending` 表示 Host 已请求卸载，但运行时仍无法释放插件加载上下文或相关文件。

常见原因：

- Host 或其他模块持有插件对象。
- 静态字段保存插件委托。
- EventBus 订阅未解除。
- UI visual tree 仍引用插件 View。
- 后台任务未退出。
- native 文件被锁定。
- 反射对象被长期缓存。

进入 `UnloadPending` 后：

- 插件不能重新启用。
- 插件目录不能删除。
- 插件文件不能覆盖。
- 更新操作进入 pending。
- Host 可以在后续时机重试卸载。

## 6. 卸载重试

重试触发点：

- 路由关闭后。
- 窗口关闭后。
- 后台任务结束后。
- GC 验证后。
- 应用关闭前。
- 下次应用启动前清理。

重试必须保持幂等。已经撤销的 lease 不应重复执行副作用。

## 7. 文件清理

插件文件清理前必须满足：

- 插件状态为 `Unloaded`。
- 没有关联加载上下文。
- 没有关联 pending update。
- 没有 native 文件锁定。
- 锁定文件不再指向该版本。

清理失败不应影响 Host 关闭，但必须记录诊断。

## 8. 诊断

卸载诊断必须能回答：

- 哪个插件无法卸载。
- 卡在哪个阶段。
- 剩余多少 lease。
- 是否还有 Operation。
- 是否还有 EventBus/State subscription。
- 是否还有 UI 引用。
- 是否有 native 文件锁定。

## 9. 测试要求

必须覆盖：

- 正常卸载。
- Active 插件先停用再卸载。
- lease 未撤销导致 `UnloadPending`。
- EventBus 订阅残留。
- UI 引用残留。
- 后台任务未退出。
- native 文件锁定。
- 重试卸载成功。
- `UnloadPending` 阻止更新和删除文件。
