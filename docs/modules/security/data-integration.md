# AtomUI.City.Security Data Integration 设计

版本：v0.1
状态：正式初版
适用范围：Data 请求认证注入、AccessTokenProvider、401/403 处理、认证刷新和 Data 管线边界。

## 1. 定位

Data integration 负责让 Data 请求管线使用 Security 提供的认证信息。

Security 不实现 HTTP client、重试、缓存和错误模型。Data 也不解释用户权限，只把认证失败和授权失败反馈给 Security 或调用方。

## 2. 请求认证注入

Data 请求流程：

```text
Data request
-> request auth metadata
-> IAccessTokenProvider.GetTokenAsync
-> attach header / credential
-> send request
-> handle 401 / 403
```

规则：

- Token 获取必须异步。
- Token 获取必须支持取消。
- Token 不应写入普通日志。
- 请求认证 scheme 由 Data client metadata 或 Host 配置决定。
- 匿名请求不能强制获取 token。

## 3. AccessTokenProvider

`IAccessTokenProvider` 返回的不是长期可缓存字符串，而是请求级 credential 结果。

结果建议包含：

- 成功 token。
- 不需要 token。
- 需要登录。
- Token 过期。
- 获取失败。
- 取消。

Data 管线根据结果决定继续请求、challenge、失败或取消。

## 4. 401 / 403

默认语义：

| 状态 | 说明 | 默认处理 |
|---|---|---|
| 401 | 认证无效、过期或需要登录。 | 通知 Security refresh 或 challenge。 |
| 403 | 认证有效但权限不足。 | 返回 authorization failure，不自动重试。 |

401 refresh 应有并发合并策略，避免多个请求同时刷新 token。

403 不应自动 refresh，除非 Host 显式配置。

## 5. Data Error Model

Security 只提供认证和授权语义。Data 负责把结果映射成 Data error model。

建议映射：

```text
AuthenticationRequired
AuthenticationExpired
AuthorizationForbidden
CredentialUnavailable
```

UI 表达由 Presentation 或应用决定。

## 6. 插件 Data client

插件 Data client 使用认证信息必须声明 metadata。

规则：

- 插件不能直接读取 Host token 存储。
- 插件只能通过 Security/Data contract 请求 credential。
- Host 可以按 capability 限制插件访问的 Data client。
- 插件停用时取消未完成请求。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| Token 获取取消 | 请求取消。 |
| Token 获取失败 | Data auth failure。 |
| 401 refresh 成功 | 重试一次，具体由 Data resilience 策略控制。 |
| 401 refresh 失败 | Security 状态进入 Expired 或 SignedOut。 |
| 403 | 返回 Forbidden，不自动重试。 |

## 8. 测试策略

测试必须覆盖：

- 匿名请求不获取 token。
- 受保护请求注入 token。
- Token 获取取消。
- 401 触发 refresh。
- refresh 并发合并。
- 403 不自动 refresh。
- 插件 Data client 无 capability 时被拒绝。
