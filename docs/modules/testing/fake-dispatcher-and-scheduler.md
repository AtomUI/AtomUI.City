# Fake Dispatcher 和确定性调度设计

版本：v0.1
状态：正式初版
适用范围：UI dispatcher fake、后台调度、Timer、延迟、异步回调和确定性测试

## 1. 目标

桌面应用测试不能依赖真实 UI thread、真实时钟和不可预测线程调度。Testing 必须提供可控调度环境。

## 2. FakeUiDispatcher

`FakeUiDispatcher` 模拟 UI dispatcher。

能力：

- 记录投递的 work item。
- 支持 `Drain` 执行队列。
- 支持断言是否投递到 UI target。
- 支持模拟 UI runtime 未准备。
- 支持取消排队 work item。
- 支持异常聚合。

规则：

- 默认不自动执行排队 work。
- 测试必须显式 drain。
- UI work 执行顺序必须稳定。
- Scope 停止后，对应 UI work 不应执行。

## 3. DeterministicScheduler

`DeterministicScheduler` 控制：

- background work。
- virtual time。
- Timer。
- debounce。
- throttle。
- retry delay。
- timeout。
- delayed cancellation。

测试通过推进虚拟时间触发行为。

```text
AdvanceBy(500ms)
-> run due timers
-> run scheduled callbacks
-> collect diagnostics
```

## 4. 禁止真实等待

测试中禁止使用真实 `Task.Delay` 猜测完成。

允许：

- 等待明确 completion task。
- drain scheduler。
- drain dispatcher。
- advance virtual time。
- 使用 CancellationToken 明确结束。

## 5. 线程目标断言

测试应能断言：

- UI 更新投递到 UI dispatcher。
- Data callback 未直接访问 UI。
- EventBus handler dispatch target 正确。
- State notification dispatch target 正确。
- Plugin unload 后无残留 dispatcher callback。

## 6. 错误处理

Fake dispatcher 和 scheduler 必须记录：

- work item id。
- owner scope。
- enqueue time。
- execution time。
- cancellation source。
- exception。

异常应进入 diagnostics collector，测试可以断言错误策略。

## 7. 测试要求

必须覆盖：

- UI work 排队和 drain。
- drain 顺序。
- work cancellation。
- virtual time 推进。
- Timer 触发。
- timeout。
- retry delay。
- Scope stop 后 work 不执行。
- 异常聚合。
