# AtomUI.City.Data gRPC Client 设计

版本：v0.1
状态：正式初版
适用范围：gRPC unary、server streaming、client streaming、bidirectional streaming、deadline、metadata、channel lifecycle 和 status 映射。

## 1. 定位

gRPC 是 Data 第一批一等 transport。

gRPC client 负责强类型 RPC、低延迟服务调用和 streaming。Data 统一管理 gRPC 调用的生命周期、认证、deadline、错误映射和诊断。

## 2. 调用类型

支持：

| 类型 | Data 抽象 |
|---|---|
| Unary call | `DataResult<T>`。 |
| Server streaming | `IDataStream<T>` 或等价 stream handle。 |
| Client streaming | client stream writer + final `DataResult<T>`。 |
| Bidirectional streaming | duplex stream handle。 |

## 3. Deadline 和取消

gRPC call 必须支持 deadline 和 cancellation。

规则：

- operation timeout 映射到 gRPC deadline。
- parent scope cancellation token 传入 call options。
- deadline exceeded 映射为 `Timeout` 或 `DeadlineExceeded`。
- 用户取消映射为 `Cancelled`。
- 插件停用取消插件 gRPC call。

## 4. Metadata 和认证

认证信息通过 gRPC metadata 注入。

规则：

- credential 来自 Security。
- metadata 不能记录敏感值。
- token refresh 后是否重试由 Data resilience 策略决定。
- streaming call 中 token 过期通常需要结束并重新建立 stream，不能假设原 stream 自动续期。

## 5. Channel Lifecycle

gRPC channel 可以跨多个 call。

规则：

- channel owner 必须明确。
- channel fault 进入 connection diagnostics。
- Plugin owner 停止时关闭插件 channel。
- Host 不长期持有插件私有 channel callback。

## 6. Status 映射

| gRPC status | DataError |
|---|---|
| Cancelled | Cancelled / StreamCancelled。 |
| DeadlineExceeded | DeadlineExceeded / Timeout。 |
| Unauthenticated | AuthenticationRequired / AuthenticationExpired。 |
| PermissionDenied | AuthorizationForbidden。 |
| NotFound | NotFound。 |
| AlreadyExists | Conflict。 |
| Unavailable | NetworkUnavailable / ServiceUnavailable。 |
| Internal | ServerError。 |
| Unknown | Unknown。 |

## 7. Streaming

gRPC streaming 必须遵守：

- SubscriptionScope。
- Backpressure policy。
- Stream completion diagnostics。
- Cancellation diagnostics。
- No direct UI update from stream callback。

## 8. 测试策略

测试必须覆盖：

- unary success。
- deadline exceeded。
- cancellation。
- unauthenticated / permission denied。
- server streaming completion。
- stream cancellation。
- plugin unload cancellation。
