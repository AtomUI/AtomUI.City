# AtomUI.City.Data Diagnostics and Testing 设计

版本：v0.1
状态：正式初版
适用范围：Data 诊断字段、错误记录、测试替身、竞态测试、无 UI 测试和插件卸载测试。

## 1. 定位

Data 必须可诊断、可测试。

请求失败不能只表现为 ViewModel 没数据。必须能说明失败发生在哪个 client、哪个 operation、哪个 transport、哪个 retry attempt、哪个 scope 或哪个插件贡献。

## 2. 诊断字段

必须记录：

- OperationId。
- OperationScopeId。
- DataClientId。
- Operation name。
- Transport kind。
- Request correlation id。
- RouteId。
- ActivationScopeId。
- PluginId。
- ContributionId。
- Auth result。
- Cache hit/miss。
- Retry attempt。
- Timeout / deadline。
- Backpressure action。
- Connection state。
- Transport status。
- DataError kind。
- Dispatch target。
- Duration。

敏感信息不能写入日志：token、password、完整 credential、完整 authorization header。

## 3. 测试替身

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
- Test connection manager。
- Test stream producer。
- Plugin data client test host。
- Deterministic scheduler。

## 4. 竞态测试

必须覆盖：

- 请求完成时 Scope 已取消，结果不提交。
- `CancelPrevious` 旧请求返回晚于新请求。
- `LatestWins` 只提交最新结果。
- SignalR handler 在后台线程回调。
- gRPC stream 慢消费者触发 backpressure。
- token refresh 并发合并。
- 插件卸载时仍有请求、连接、订阅。
- cache 按 principal 隔离。
- mutation retry 被禁止。
- UI dispatcher 不存在时 Data 仍可测试。

## 5. Transport 测试

必须覆盖：

- HTTP 200 / 401 / 403 / 404 / 409 / 5xx。
- gRPC status mapping。
- gRPC deadline exceeded。
- gRPC server streaming。
- SignalR connect / reconnect / closed。
- SignalR server push。
- upload / download progress。

## 6. 无 UI 测试

Data 测试不得依赖真实 AtomUI/Avalonia UI。

规则：

- 不要求 UI dispatcher 存在。
- 使用 deterministic scheduler。
- 手动推进 stream 和 connection state。
- 明确断言 dispatch target。

## 7. 插件卸载测试

必须覆盖：

- 插件请求取消。
- 插件 stream 取消。
- 插件 SignalR connection stop。
- 插件 callbacks 清理。
- 插件 cache revoke。
- 无插件私有类型引用残留。
