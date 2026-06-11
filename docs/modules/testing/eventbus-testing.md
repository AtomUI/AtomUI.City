# EventBus 测试设计

版本：v0.1
状态：正式初版
适用范围：事件发布、订阅、dispatch、背压、错误、生命周期、插件和事件链诊断

## 1. 目标

EventBus 测试必须证明事件通知在模块和插件之间可诊断、可释放、可确定性调度。

## 2. EventBusRecorder

Testing 提供：

- publish recorder。
- subscription recorder。
- handler invocation recorder。
- dispatch target assertion。
- backpressure assertion。
- error aggregation assertion。
- event chain assertion。
- plugin subscription assertion。

## 3. 单元测试范围

必须覆盖：

- subscribe。
- unsubscribe。
- publish。
- no subscriber。
- multiple subscribers。
- ordered dispatch。
- concurrent dispatch policy。
- handler exception。
- cancellation。
- backpressure。
- diagnostics。

## 4. Lifecycle 测试

必须覆盖：

- subscription 绑定 Scope。
- Scope stop 后自动退订。
- Operation cancellation。
- handler 执行中 stop。
- dispose 幂等。

## 5. 插件测试

必须覆盖：

- 插件订阅注册。
- 插件停用后退订。
- 插件卸载前无订阅残留。
- 跨插件事件 contract 必须来自 Host shared contract。
- 插件 private event type 泄漏被拒绝。

## 6. 确定性调度

EventBus 测试不能依赖真实线程顺序。必须使用 deterministic scheduler 推进 dispatch。
