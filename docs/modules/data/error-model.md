# AtomUI.City.Data Error Model 设计

版本：v0.1
状态：正式初版
适用范围：DataResult、DataError、HTTP/gRPC/SignalR 错误映射、取消语义和诊断。

## 1. 定位

Data 不应该把预期失败都作为异常抛给 ViewModel。

`DataResult<T>` 表达请求成功、失败、取消和部分结果。异常用于不可预期的框架错误，进入 DataError mapping 后返回调用方。

## 2. DataResult

结果建议：

```text
Success
Failed
Cancelled
Partial
StaleSuppressed
```

`StaleSuppressed` 表示请求完成时 parent scope、operation sequence 或 plugin contribution 已失效，结果未提交。

## 3. DataError

建议错误类型：

```text
Cancelled
Timeout
NetworkUnavailable
CredentialUnavailable
AuthenticationRequired
AuthenticationExpired
AuthorizationForbidden
BadRequest
NotFound
Conflict
ValidationFailed
ServerError
ServiceUnavailable
TransportError
SerializationError
PolicyRejected
ConnectionFailed
ConnectionClosed
ReconnectFailed
StreamCancelled
StreamCompleted
StreamProtocolError
DeadlineExceeded
Unavailable
PluginUnavailable
LocalStorageError
Unknown
```

## 4. Transport 映射

| 来源 | 映射 |
|---|---|
| HTTP 401 | AuthenticationRequired / AuthenticationExpired。 |
| HTTP 403 | AuthorizationForbidden。 |
| gRPC Unauthenticated | AuthenticationRequired / AuthenticationExpired。 |
| gRPC PermissionDenied | AuthorizationForbidden。 |
| gRPC DeadlineExceeded | DeadlineExceeded / Timeout。 |
| gRPC Unavailable | NetworkUnavailable / Unavailable。 |
| SignalR reconnect failed | ReconnectFailed。 |
| SignalR closed | ConnectionClosed。 |
| Scope cancellation | Cancelled。 |

## 5. 取消语义

取消不是错误。

取消来源：

- 用户取消。
- Scope 停止。
- Navigation 离开。
- ViewModel 停用。
- Plugin 停用。
- Host 关闭。
- `CancelPrevious` 并发策略。

取消必须可诊断，但不进入 fatal error。

## 6. 错误边界

DataError 不直接决定 UI 展示。

调用方可以映射到：

- Resolver `ResolveResult`。
- Command result。
- State update。
- Presentation notification。
- Diagnostics record。

## 7. 测试策略

测试必须覆盖：

- HTTP status 映射。
- gRPC status 映射。
- SignalR closed / reconnect failed。
- cancellation。
- stale result suppression。
- serialization error。
- plugin unavailable。
