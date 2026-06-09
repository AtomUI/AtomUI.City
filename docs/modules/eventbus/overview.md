# AtomUI.City.EventBus

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.EventBus` 负责类型事件总线、作用域订阅、事件通道、线程调度、错误策略和测试记录。

EventBus 用于模块间、组件间和框架内部的解耦通信。

## 边界

EventBus 需要支持：

- 类型事件。
- 通道或 contract。
- 强订阅和弱订阅策略。
- 订阅作用域。
- UI 线程调度。
- 后台线程调度。
- 错误处理策略。
- 测试记录和断言。

EventBus 不直接基于 CommunityToolkit WeakReferenceMessenger 作为底层实现。

## 后续拆分

- `subscriptions.md`
- `channels.md`
- `dispatching.md`
- `error-handling.md`
- `testing.md`
