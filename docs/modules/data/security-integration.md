# AtomUI.City.Data Security Integration 设计

版本：v0.1
状态：正式初版
适用范围：Security credential、AccessTokenProvider、401/403、single-flight refresh、用户切换、长连接认证和插件凭据边界。

## 1. 定位

Data 通过 Security 获取认证凭据。

Data 不直接读取 token 存储，不管理登录态，不解释权限策略。Data 只根据请求 metadata 获取 credential，并把认证或授权失败映射为 DataError。

## 2. Credential 获取

```text
Data request
-> auth metadata
-> IAccessTokenProvider
-> credential result
-> attach credential
-> transport send
```

规则：

- credential 获取必须异步。
- credential 获取必须支持取消。
- Token 不写入日志。
- 匿名请求不强制获取 token。
- 插件不能直接读取 Host token。

## 3. Single-flight Refresh

401 或 unauthenticated 可能触发 refresh。

```text
401 / unauthenticated
-> single-flight refresh
-> pending requests wait
-> refresh success retry
-> refresh failed fail all waiting operations
```

规则：

- 同一 principal revision 下并发 refresh 合并。
- refresh 成功后按 resilience 策略重试。
- refresh 失败后 waiting operations 返回 AuthenticationExpired 或 AuthenticationRequired。
- 用户取消不触发 refresh。

## 4. 401 / 403

| 状态 | 语义 | 默认处理 |
|---|---|---|
| 401 / Unauthenticated | 认证失效、过期或需要登录。 | refresh / challenge。 |
| 403 / PermissionDenied | 已认证但权限不足。 | AuthorizationForbidden，不自动重试。 |

403 默认不 refresh，除非 Host 显式配置。

## 5. 长连接认证

gRPC streaming 和 SignalR connection 需要特殊处理。

规则：

- token 过期后可能需要结束并重建 stream / connection。
- 用户切换账号时旧连接必须关闭。
- refresh 期间是否暂停发送由 connection policy 决定。
- reconnect 后是否重新订阅由 subscription policy 决定。

## 6. 插件凭据边界

插件 Data client 使用 credential 必须走 Security/Data contract。

禁止：

- 插件读取 Host token store。
- 插件缓存 Host token。
- 插件把 token 写入日志。
- Host 静态缓存插件 credential callback。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| credential unavailable | AuthenticationRequired。 |
| refresh failed | AuthenticationExpired 或 AuthenticationRequired。 |
| 403 | AuthorizationForbidden。 |
| 用户切换 | 取消旧用户相关 operations 和 connections。 |
| 插件 credential callback 撤销 | PluginUnavailable。 |

## 8. 测试策略

测试必须覆盖：

- credential 注入。
- 匿名请求不取 token。
- 401 single-flight refresh。
- refresh 失败唤醒等待请求。
- 403 不 refresh。
- 用户切换关闭连接。
- 插件不能读取 Host token。
