# AtomUI.City.Data Request Pipeline 设计

版本：v0.1
状态：正式初版
适用范围：请求上下文、管线阶段、handler、OperationScope、认证注入、缓存、resilience、响应映射和诊断。

## 1. 定位

Request pipeline 是 Data 的核心执行链路。

所有 request/response 类型访问必须进入 pipeline。Streaming 和 realtime connection 也应复用 pipeline 中的认证、诊断、capability、resilience 和错误映射阶段。

## 2. Pipeline 阶段

推荐阶段：

```text
Create request context
-> Attach OperationScope
-> Validate request metadata
-> Check plugin capability
-> Resolve authentication credential
-> Build transport request
-> Cache lookup
-> Execute resilience policy
-> Send transport request
-> Map transport response
-> Map error
-> Cache write
-> Return DataResult
-> Emit diagnostics
```

第一版用固定阶段，避免复杂动态排序。

## 3. Request Context

Request context 应包含：

- OperationId。
- DataClientId。
- Operation name。
- Parent scope。
- CancellationToken。
- Transport kind。
- Auth metadata。
- Cache metadata。
- Resilience metadata。
- Plugin contribution。
- Correlation id。
- Diagnostics context。

Request context 不能包含 UI 控件实例。

## 4. OperationScope

每次请求都是 Operation。

规则：

- 调用方没有提供 OperationScope 时，Data 创建一个。
- 请求取消应联动 parent scope cancellation token。
- OperationScope 停止后禁止提交结果。
- Operation 完成、失败、取消都要记录诊断。

## 5. Handler

`IDataRequestHandler` 用于实现管线阶段。

规则：

- Handler 必须是可组合、可测试的。
- Handler 不访问 UI。
- Handler 必须尊重 cancellation token。
- Handler 异常进入 DataError mapping。
- 插件 handler 必须绑定插件 contribution。

## 6. 结果提交

结果提交前必须检查：

- OperationScope 是否仍 running。
- Parent ActivationScope / RouteScope 是否仍有效。
- Plugin contribution 是否仍 active。
- 当前 operation 是否被并发策略允许提交。

如果检查失败，结果被抑制，返回 cancelled 或 stale result 诊断。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| metadata 无效 | DataResult failed。 |
| capability 拒绝 | PolicyRejected 或 PluginUnavailable。 |
| credential 不可用 | AuthenticationRequired 或 CredentialUnavailable。 |
| transport 抛异常 | 映射为 DataError。 |
| result stale | 抑制提交，记录诊断。 |

## 8. 测试策略

测试必须覆盖：

- handler 顺序。
- credential 注入。
- cache short-circuit。
- retry 包裹 transport。
- transport error mapping。
- stale result suppression。
- OperationScope 取消。
