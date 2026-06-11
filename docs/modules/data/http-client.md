# AtomUI.City.Data HTTP Client 设计

版本：v0.1
状态：正式初版
适用范围：HTTP / REST / Web API、HttpClientFactory、delegating handler、认证、上传下载、进度和 HTTP 错误映射。

## 1. 定位

HTTP 是 Data 第一批一等 transport。

HTTP client 负责 REST、Web API、文件上传下载和普通 request/response 数据访问。HTTP 能力必须进入 Data pipeline，而不是让 ViewModel 直接使用裸 `HttpClient`。

## 2. HttpClientFactory

HTTP transport 基于 `IHttpClientFactory`。

规则：

- 支持 named client。
- 支持 typed client。
- 支持 delegating handler。
- handler lifetime 由 `HttpClientFactory` 管理。
- Data pipeline 不绕开 `HttpClientFactory` 自己创建长期 handler。

## 3. HTTP Pipeline

HTTP 请求经过：

```text
Data request context
-> auth metadata
-> HttpRequestMessage
-> HttpClient delegating handlers
-> response
-> DataError mapper
-> DataResult
```

Data pipeline 管理跨 transport 的生命周期、认证、缓存、resilience 和诊断；HTTP delegating handler 管理 HTTP-specific middleware。

## 4. 认证

HTTP 认证通过 Security credential 注入。

规则：

- Authorization header 由 Data/Security 管线注入。
- Token 不写日志。
- 匿名请求不强制取 token。
- 401 交给 Security refresh / challenge。
- 403 返回 authorization forbidden。

## 5. 上传下载

HTTP 必须支持大载荷场景：

- Upload progress。
- Download progress。
- Streaming content。
- Range request。
- Temporary file。
- Cancellation。

进度通知必须节流，避免 UI 高频刷新。

## 6. 错误映射

| HTTP | DataError |
|---|---|
| 400 | ValidationFailed 或 BadRequest。 |
| 401 | AuthenticationRequired / AuthenticationExpired。 |
| 403 | AuthorizationForbidden。 |
| 404 | NotFound。 |
| 409 | Conflict。 |
| 408 / timeout | Timeout。 |
| 5xx | ServerError。 |
| network error | NetworkUnavailable 或 TransportError。 |

## 7. 缓存

HTTP request/response 可以使用 Data cache。

缓存 key 必须包含 principal revision、auth scheme、client id、operation name、参数 hash 和 plugin contribution。

## 8. 测试策略

测试必须覆盖：

- typed client。
- auth header 注入。
- 401 refresh。
- 403 forbidden。
- HTTP status 映射。
- upload/download cancellation。
- progress throttle。
- cache hit/miss。
