# AtomUI.City.Data

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Data` 负责数据请求、客户端代理、请求管线、缓存、错误模型、取消、重试和认证集成。

Data 的目标是让应用数据访问具备统一入口、统一错误处理、统一生命周期和统一诊断。

Data 第一版必须支持多种访问方式：

- HTTP / REST / Web API。
- gRPC unary / streaming。
- SignalR realtime connection。

## 边界

Data 可以依赖：

- Microsoft.Extensions.Http
- Polly
- Security 抽象
- State 抽象

Data 不负责：

- 领域模型设计。
- 仓储模式。
- 应用服务分层。
- UI 状态展示。
- 认证状态管理。
- 权限策略解释。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Data 总体架构、多传输访问、请求管线、生命周期、认证、缓存、错误和测试策略。 |
| [request-pipeline.md](request-pipeline.md) | 请求上下文、管线阶段、handler、OperationScope、响应映射和诊断。 |
| [transport.md](transport.md) | Transport 抽象、request/response、streaming、realtime connection 和生命周期差异。 |
| [http-client.md](http-client.md) | HTTP / REST / Web API、HttpClientFactory、delegating handler、上传下载和 HTTP 错误映射。 |
| [grpc-client.md](grpc-client.md) | gRPC unary、server/client/bidi streaming、deadline、metadata、channel 和 status 映射。 |
| [signalr-client.md](signalr-client.md) | SignalR connection、hub invoke、server push、subscription、reconnect 和 token refresh。 |
| [client-proxy.md](client-proxy.md) | Typed client、generated client、adapter client、Refit 可选适配和 descriptor。 |
| [security-integration.md](security-integration.md) | Security credential、401/403、single-flight refresh、用户切换和插件凭据边界。 |
| [async-and-threading.md](async-and-threading.md) | 异步、线程、late result suppression、回调调度和 sync-over-async 禁止规则。 |
| [concurrency.md](concurrency.md) | AllowConcurrent、Queue、CancelPrevious、LatestWins、KeyedSerial 等并发策略。 |
| [connection-lifecycle.md](connection-lifecycle.md) | HTTP、gRPC channel、gRPC streaming、SignalR 长连接和显式连接生命周期。 |
| [streaming-and-realtime.md](streaming-and-realtime.md) | gRPC streaming、SignalR server push、subscription、backpressure 和状态投影。 |
| [resilience.md](resilience.md) | Timeout、retry、circuit breaker、fallback、rate limit 和 mutation 重试约束。 |
| [caching.md](caching.md) | Request cache、response cache、snapshot cache、principal 隔离和插件缓存撤销。 |
| [consistency-and-cache-invalidation.md](consistency-and-cache-invalidation.md) | Query/mutation/subscription、一致性、idempotency、optimistic update 和失效策略。 |
| [large-payload-and-progress.md](large-payload-and-progress.md) | 上传、下载、进度、range、临时文件、节流和大载荷内存约束。 |
| [error-model.md](error-model.md) | DataResult、DataError、HTTP/gRPC/SignalR 错误映射和取消语义。 |
| [state-integration.md](state-integration.md) | Data 与 State 的显式更新、状态投影、OperationScope 和 UI 线程边界。 |
| [routing-integration.md](routing-integration.md) | Resolver 调用 Data、导航取消、ResolveResult 映射和预取诊断。 |
| [plugin-integration.md](plugin-integration.md) | 插件 Data client、capability、请求取消、连接停止、缓存撤销和 contract 隔离。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | Data 诊断字段、测试替身、竞态测试、无 UI 测试和插件卸载测试。 |

## 可选增强文档

- `offline-sync.md`
- `refit-integration.md`
- `otel-integration.md`
