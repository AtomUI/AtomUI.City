# AtomUI.City.EventBus

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.EventBus` 负责类型事件总线、作用域订阅、事件通道、线程调度、错误策略和测试记录。

EventBus 是进程内系统级事件通知基础设施，用于 Host、静态模块、运行时插件和框架组件之间发布已经发生的事实。

## 边界

EventBus 需要支持：

- 强类型事件和稳定 EventContractId。
- Shared Contract Plane 和 Plugin Private Plane。
- 强引用、生命周期托管订阅。
- Serialized、Partitioned 和 Concurrent channel。
- 有界队列和背压。
- 遵守 Core Threading 的 UI 线程调度。
- 遵守 Core Threading 的后台线程调度。
- 错误处理策略。
- 插件停用 quiescing、drain 和卸载诊断。
- AOT/source generator 注册。
- 测试记录和断言。

EventBus 不直接基于 CommunityToolkit WeakReferenceMessenger 作为底层实现。

EventBus 的线程策略必须作为订阅 contract 的一部分。发布事件时先获取订阅快照，再按每个订阅的 `DispatchPolicy` 投递。UI handler 默认投递到 UI dispatcher，后台 handler 异常进入 EventBus error policy。

线程模型见：[Core Threading 设计](../core/threading.md)。

EventBus 不负责保存当前值、请求响应、命令执行和分布式消息。需要当前值和 latest 语义时使用 State。

第一版不提供弱引用订阅、latest replay、sticky event 和同步阻塞发布。

## 详细设计

- [Detailed Design](detailed-design.md)
- [Event Contracts](contracts.md)
- [Subscriptions](subscriptions.md)
- [Dispatching](dispatching.md)
- [Plugin Integration](plugins.md)
- [Diagnostics and Testing](diagnostics-and-testing.md)
