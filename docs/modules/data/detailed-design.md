# AtomUI.City.Data Detailed Design

版本：v0.1
状态：正式初版
适用范围：多传输数据访问、请求管线、HTTP、gRPC、SignalR、异步线程、并发、长连接、缓存、错误模型、认证集成、插件边界和测试策略。

## 1. 定位

`AtomUI.City.Data` 是框架级数据访问基础设施。

Data 不提供 DDD Repository 作为默认范式，不定义领域模型，不替代业务应用自己的 Application Service、Repository 或 Query Service。它只保证所有数据访问都能进入统一生命周期、统一错误处理、统一认证注入和统一诊断链路。

第一版必须把 HTTP、gRPC、SignalR 都作为一等访问方式支持：

| 访问方式 | 主要用途 |
|---|---|
| HTTP | REST、Web API、文件上传下载、普通请求响应。 |
| gRPC | 强类型 RPC、unary call、server/client/bidi streaming。 |
| SignalR | 实时连接、服务端推送、双向消息、hub method invoke。 |

核心链路：

```text
ViewModel / Command / Resolver
-> Data client
-> OperationScope
-> Data request pipeline
-> Security credential
-> Transport
-> Resilience / cache / error mapping
-> DataResult / DataStream / DataConnection
-> State / ViewModel / Resolver result
```

## 2. 设计原则

- .NET-first：优先基于 `HttpClientFactory`、Options、DI、`CancellationToken`、typed client、handler pipeline。
- Multi-transport：HTTP、gRPC、SignalR 都是一等 transport，不把 Data 设计成 HTTP wrapper。
- Pipeline-first：所有请求必须进入 Data pipeline，不能让 ViewModel 直接散落使用裸 transport。
- Lifecycle-aware：每次请求、stream、connection 必须绑定 `OperationScope`、`SubscriptionScope` 或显式 lifecycle owner。
- Security-integrated：认证凭据只通过 Security 获取，Data 不直接管理登录态。
- State-separated：Data 不隐式写全局 State；请求完成后由调用方或显式 adapter 更新 State。
- AOT-first：client descriptor、operation metadata、auth metadata、cache metadata 由 Source Generator 生成。
- Plugin-aware：插件 Data client 必须可撤销，运行中请求可取消，不能持有 Host 私有凭据。
- Thread-safe：transport callback 不直接访问 UI，不捕获 UI `SynchronizationContext`。
- Testable：请求管线、transport、认证、缓存、重试、长连接和竞态都必须可替换测试。

## 3. 非目标

Data 不负责：

- 领域模型设计。
- DDD Repository 默认实现。
- 应用服务分层。
- UI loading 展示。
- 认证状态管理。
- 权限策略解释。
- ViewModel 状态管理。
- 离线同步业务策略。
- 数据库 ORM 默认封装。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `IDataClient` | 数据客户端统一标识。 |
| `IDataClientFactory` | 创建 typed client、generated client 或 adapter client。 |
| `IDataRequestPipeline` | 执行请求管线。 |
| `IDataRequest` | 请求 descriptor。 |
| `IDataResponse` | 原始响应 descriptor。 |
| `DataResult<T>` | 标准请求结果，不返回裸异常。 |
| `DataError` | 标准错误模型。 |
| `IDataTransport` | 传输抽象根接口。 |
| `IRequestResponseTransport` | 请求/响应传输，例如 HTTP、gRPC unary。 |
| `IStreamingTransport` | streaming 传输，例如 gRPC streaming。 |
| `IRealtimeConnectionTransport` | 实时连接传输，例如 SignalR。 |
| `IDataConnection` | 长连接实例抽象。 |
| `IDataSubscription` | streaming 或 SignalR 订阅句柄。 |
| `IDataConnectionManager` | 管理长连接生命周期、重连和关闭。 |
| `IDataRequestHandler` | 管线处理器。 |
| `IDataErrorMapper` | 把 transport error 转换成 DataError。 |
| `IDataCache` | 请求缓存和结果缓存抽象。 |
| `IResiliencePolicyProvider` | timeout、retry、circuit breaker 等策略。 |
| `IDataSerializer` | 请求/响应序列化。 |
| `IDataDiagnostics` | 请求诊断、耗时、错误、correlation id。 |

命名不加 `City` 前缀。

## 5. 访问模式

Data 不能只抽象成 `SendAsync`。第一版至少区分三类访问模式：

| 模式 | 适用 | 第一批实现 |
|---|---|---|
| Request / Response | 查询、提交、命令式调用 | HTTP、gRPC unary、SignalR hub invoke。 |
| Streaming | 服务端流、客户端流、双向流 | gRPC streaming。 |
| Realtime Connection | 服务端推送、频道订阅、实时事件 | SignalR。 |

统一链路：

```text
Data client
-> Data operation context
-> Security credential
-> Transport-specific execution
-> DataResult / DataStream / DataConnection
-> State / EventBus / ViewModel
```

## 6. 请求管线

推荐管线：

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

详细规则见：[request-pipeline.md](request-pipeline.md)。

## 7. 异步和线程

Data 请求属于 Operation。所有耗时 transport 操作必须在后台或 transport 自身异步上下文中执行，不能阻塞 UI Thread。

关键约束：

- 禁止 `.Result` / `.Wait()` / sync-over-async。
- transport callback 不能直接访问 ViewModel 或 UI。
- 请求结果提交前必须检查 parent scope 是否仍然有效。
- Scope 已取消后的 late result 必须被抑制，不能更新 State 或 ViewModel。
- 结果进入 UI 前必须通过 State subscription、Presentation binding 或 dispatcher 显式调度。

详细规则见：[async-and-threading.md](async-and-threading.md)。

## 8. 并发策略

每个 Data operation 应声明并发策略：

| 策略 | 说明 |
|---|---|
| `AllowConcurrent` | 默认允许并发。 |
| `DisallowConcurrent` | 正在执行时拒绝新请求。 |
| `Queue` | 排队顺序执行。 |
| `CancelPrevious` | 新请求取消旧请求。 |
| `LatestWins` | 允许并发，但只有最新结果可提交。 |
| `KeyedSerial` | 同一个 resource key 串行，不同 key 并行。 |

详细规则见：[concurrency.md](concurrency.md)。

## 9. 长连接和实时流

HTTP 和 gRPC unary 是单次 Operation。gRPC streaming、SignalR connection 和 SignalR subscription 是长期资源，必须显式声明生命周期。

连接生命周期：

```text
Application
Window
Navigation
Route
Activation
Plugin
Manual
```

长连接不能默认挂 `ApplicationScope`。连接 owner 必须由 client metadata、Host 配置或调用方显式声明。

详细规则见：

- [connection-lifecycle.md](connection-lifecycle.md)
- [streaming-and-realtime.md](streaming-and-realtime.md)

## 10. Security 集成

Data 不直接读取 token 存储。

```text
Data request
-> auth metadata
-> IAccessTokenProvider
-> credential result
-> attach credential
-> send request
```

401 / 403 语义：

| 状态 | 默认语义 | 默认处理 |
|---|---|---|
| 401 | 认证失效或需要登录 | 通知 Security refresh / challenge。 |
| 403 | 已认证但权限不足 | 返回 forbidden，不自动重试。 |

详细规则见：[security-integration.md](security-integration.md)。

## 11. 缓存和一致性

HTTP 和 gRPC unary 可以缓存。Streaming 和 SignalR 默认不缓存原始消息，只允许显式状态投影、latest snapshot 或有界 buffer。

缓存必须按主体、权限、插件贡献和 client version 隔离。

详细规则见：

- [caching.md](caching.md)
- [consistency-and-cache-invalidation.md](consistency-and-cache-invalidation.md)

## 12. 错误模型

`DataResult<T>` 不应该让预期失败走异常。

典型错误：

```text
Cancelled
Timeout
NetworkUnavailable
CredentialUnavailable
AuthenticationRequired
AuthenticationExpired
AuthorizationForbidden
BadRequest
ServiceUnavailable
ConnectionFailed
ConnectionClosed
ReconnectFailed
StreamCancelled
DeadlineExceeded
Unavailable
SerializationError
PluginUnavailable
LocalStorageError
Unknown
```

详细规则见：[error-model.md](error-model.md)。

## 13. AOT 和 Source Generator

Data generator 负责生成：

- `HttpClientDescriptor`。
- `GrpcClientDescriptor`。
- `SignalRHubDescriptor`。
- Operation metadata。
- Auth metadata。
- Timeout / retry metadata。
- Cache metadata。
- Streaming metadata。
- Connection lifetime metadata。
- Plugin contribution metadata。
- Serializer metadata。

运行时默认不扫描程序集发现 Data client。

## 14. 测试策略

Testing 包应提供：

- Fake data client。
- Fake HTTP transport。
- Fake gRPC transport。
- Fake SignalR transport。
- Test request pipeline。
- Fake access token provider。
- Fake cache。
- Fake resilience policy。
- Data diagnostics recorder。
- Plugin data client test host。
- Deterministic scheduler。

必须覆盖竞态、取消、重试、缓存、401/403、streaming backpressure、SignalR reconnect、插件卸载和无 UI 调度环境。

详细规则见：[diagnostics-and-testing.md](diagnostics-and-testing.md)。
