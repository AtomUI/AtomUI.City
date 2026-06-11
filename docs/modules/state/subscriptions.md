# AtomUI.City.State 订阅与 Reaction 设计

版本：v0.1
状态：正式初版
适用范围：状态订阅、State Reaction、生命周期绑定、释放、错误策略和插件卸载

## 1. 定位

状态副作用不命名为 `Effect`。公共 API 优先使用 `IStateSubscription` 或 State Reaction 语义。

```text
state.OnChange(...)
-> returns IDisposable / IStateSubscription
-> registered in StateScope / ActivationScope
```

## 2. 生命周期绑定

所有 subscription 必须绑定 Scope。

常见绑定：

| 创建位置 | 默认绑定 |
|---|---|
| Application service | ApplicationScope |
| Route resolver | RouteScope |
| ViewModel activation | ActivationScope |
| Operation callback | OperationScope |
| Plugin contribution | Plugin contribution lease |

ViewModel 构造函数不得建立长期订阅。长期订阅必须在 Activation 阶段创建，并随 ActivationScope 停用释放。

## 3. 释放规则

规则：

- subscription 释放必须幂等。
- Scope 停止时按反向顺序释放 subscription。
- StateScope 释放时释放所有 state subscriptions。
- 插件 subscription 必须可被插件卸载流程找到并释放。
- 释放失败进入错误聚合，但不能阻断其他释放。

## 4. 错误策略

subscription 抛异常时：

- 进入 State ErrorPolicy。
- 写入 Diagnostics。
- 不杀死 state。
- 不阻止其他订阅者接收通知，除非策略显式要求 fail-fast。
- UI 线程订阅异常不得逃逸到 Dispatcher 形成未处理异常。

## 5. 调度

订阅必须声明或继承调度策略。

| 策略 | 说明 |
|---|---|
| Immediate | 当前线程通知。 |
| Queued | 排队后统一通知。 |
| Dispatcher | 切到 UI dispatcher。 |
| Background | 后台调度。 |

调度语义见：[threading-and-dispatch.md](threading-and-dispatch.md)。

## 6. 插件卸载

插件停用时必须：

```text
Stop new plugin state subscriptions
-> cancel plugin operations
-> drain or reject pending notifications
-> dispose plugin subscriptions
-> revoke contribution leases
-> release plugin state objects
```

Host 不允许长期持有插件私有 subscription。

## 7. AOT 和 Source Generator

Generator/Analyzer 负责：

- 生成 subscription descriptor。
- 诊断未绑定 Scope 的订阅。
- 诊断插件 subscription 泄漏。
- 诊断 UI 订阅缺少 Dispatcher 策略。

## 8. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| OnChange 通知 | Unit | 状态提交后收到通知。 |
| 相等值不通知 | Unit | 相等提交不触发 handler。 |
| 手动释放 | Unit | Dispose 后不再收到通知。 |
| Scope 自动释放 | Unit | Scope 停止后不再收到通知。 |
| handler 异常 | Unit | 诊断记录，不杀死 state。 |
| UI Dispatcher 策略 | Unit | 通知投递到 fake UI dispatcher。 |
| 插件停用 | Unit | 插件 subscription 被释放。 |
