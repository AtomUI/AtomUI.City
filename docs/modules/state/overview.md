# AtomUI.City.State

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.State` 负责只读状态、可写状态、计算状态、应用级共享状态、状态订阅、StateScope、Snapshot 和集合状态。

State 的目标是让框架识别状态、状态派生、应用级共享状态、状态副作用和状态生命周期，而不是把状态当作普通属性散落在 ViewModel 中，也不是做成静态全局变量。

## 边界

State Core 不要求 System.Reactive，也不依赖 ReactiveUI。

核心概念：

- `IReadOnlyState<T>`
- `IWritableState<T>`
- `IComputedState<T>`
- `IApplicationState`
- `IApplicationStateWriter`
- `StateKey<T>`
- `IStateSubscription`
- `IStateScope`
- `StateSnapshot`
- `IStateCollection<TKey, TItem>`

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | State 总体架构、边界、核心抽象、生命周期、AOT/source generator、错误策略和测试策略。 |
| [state-values.md](state-values.md) | `IReadOnlyState<T>`、`IWritableState<T>`、状态版本、相等比较、原子更新和状态定义。 |
| [application-state.md](application-state.md) | 应用级共享状态、DI 访问、写入策略、状态注册表和访问边界。 |
| [computed-state.md](computed-state.md) | 派生状态、依赖声明、缓存、失效、错误处理和 AOT 约束。 |
| [subscriptions.md](subscriptions.md) | 状态订阅、State Reaction、生命周期绑定、释放、错误策略和插件卸载。 |
| [snapshots.md](snapshots.md) | StateSnapshot、持久化策略、恢复、版本兼容和测试断言。 |
| [collection-state.md](collection-state.md) | keyed collection state、集合变更、item 版本、快照和不可变更新。 |
| [threading-and-dispatch.md](threading-and-dispatch.md) | 状态提交、通知调度、Core Threading 集成、多线程约束和 UI Dispatcher 边界。 |
| [plugin-integration.md](plugin-integration.md) | 插件状态隔离、状态写入授权、状态撤销、快照迁移和卸载安全。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | State 诊断字段、错误码、测试工具、测试矩阵和完成门禁。 |
