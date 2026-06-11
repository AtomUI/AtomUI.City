# AtomUI.City.Data Async and Threading 设计

版本：v0.1
状态：正式初版
适用范围：异步执行、线程边界、UI 调度、late result suppression、transport callback 和 sync-over-async 禁止规则。

## 1. 定位

Data 请求天然运行在多线程环境中。

HTTP、gRPC、SignalR 回调都不能假设发生在 UI Thread。Data 必须遵守 Core Threading 模型，保证请求不会阻塞 UI，不会在 Scope 已释放后回写旧结果，不会让插件线程或回调阻止卸载。

线程模型见：[Core Threading 设计](../core/threading.md)。

## 2. 基本规则

- Data 模块不能直接假设 UI dispatcher 存在。
- 请求必须接收 `CancellationToken`。
- 请求不能阻塞 UI Thread。
- 禁止 `.Result` / `.Wait()` / sync-over-async。
- transport callback 不能直接更新 ViewModel 或 UI。
- SignalR handler 不能直接写 UI。
- gRPC stream item handler 不能直接写 UI。
- 请求结果进入 State 或 ViewModel 前必须按目标调度策略投递。

## 3. Operation 语义

每个请求必须有：

- OperationId。
- ParentScope。
- CancellationToken。
- Timeout。
- ConcurrencyPolicy。
- Diagnostics。
- Result commit policy。

```text
Data request starts
-> OperationScope running
-> transport executes asynchronously
-> result returns
-> validate scope and concurrency state
-> commit or suppress result
```

## 4. Late Result Suppression

late result suppression 是强制规则。

```text
Data request starts
-> user navigates away
-> ActivationScope / RouteScope cancelled
-> request returns later
-> result must be ignored
```

被抑制的结果不能：

- 更新 ViewModel。
- 写入 State。
- 触发 Presentation UI 更新。
- 触发成功通知。

它只能记录诊断，必要时释放 transport resource。

## 5. DispatchPolicy

Data pipeline 默认在后台或 transport async context 中运行。

结果投递：

| 目标 | 推荐方式 |
|---|---|
| ViewModel property | 通过 UI dispatcher 或 Presentation binding 间接更新。 |
| State | 通过 State writer，并由 State subscription 决定 dispatch。 |
| EventBus | 发布事件时由 EventBus subscription 的 DispatchPolicy 决定。 |
| Diagnostics | 后台记录，不访问 UI。 |

## 6. Streaming 回调

Streaming item 和 SignalR message 必须先进入 Data subscription dispatcher。

```text
Transport callback
-> Data subscription dispatcher
-> backpressure policy
-> mapper
-> State / EventBus / ViewModel boundary
```

不能在 transport callback 内执行业务 UI 逻辑。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| UI dispatcher 不存在 | Data 仍可运行，结果不直接投递 UI。 |
| Scope 已取消 | 抑制结果。 |
| callback 抛异常 | 进入 Data diagnostics 和 ErrorPolicy。 |
| 取消 | 返回 Cancelled，不作为失败。 |
| sync-over-async 检测 | Analyzer 诊断或运行时警告。 |

## 8. 测试策略

测试必须覆盖：

- 请求在后台完成。
- Scope 取消后结果不提交。
- `CancelPrevious` 旧请求晚返回。
- SignalR handler 在后台线程回调。
- gRPC stream item 在后台线程回调。
- 无 UI dispatcher 的 Data 测试。
