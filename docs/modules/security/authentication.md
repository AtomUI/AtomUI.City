# AtomUI.City.Security Authentication 设计

版本：v0.1
状态：正式初版
适用范围：认证状态、当前主体、登录、登出、刷新、恢复会话、Token 获取和状态通知。

## 1. 定位

Authentication 子模块负责描述当前应用是否有已认证主体，以及认证状态如何变化。

它不实现具体身份协议，不提供登录 UI，不决定用户管理业务。应用可以接入本地账号、企业 SSO、OIDC、设备认证或自定义认证方式，但必须统一汇入 Security 的认证状态模型。

## 2. 认证状态机

建议状态：

```text
Unknown
-> Anonymous
-> Authenticating
-> Authenticated
-> Refreshing
-> Expired
-> SignedOut
-> Failed
```

状态说明：

| 状态 | 说明 |
|---|---|
| `Unknown` | 应用刚启动，认证缓存或会话尚未恢复。 |
| `Anonymous` | 明确无登录主体。 |
| `Authenticating` | 正在执行登录或认证恢复。 |
| `Authenticated` | 当前有有效主体。 |
| `Refreshing` | 正在刷新 token 或会话。 |
| `Expired` | 当前主体或凭据过期。 |
| `SignedOut` | 已登出，凭据已清理。 |
| `Failed` | 认证流程失败。 |

认证状态变化必须可诊断、可订阅、可测试。

## 3. 当前主体

当前主体使用 `ClaimsPrincipal` 表达。

规则：

- 未登录时返回空主体或 anonymous principal，但不能返回 null。
- `ClaimsPrincipal` 表达身份、claim、role 等通用信息。
- Security 不内置业务用户模型。
- 业务用户信息应由应用或 Data 模块按需加载。
- 插件不能修改 Host root principal。

`ICurrentPrincipalAccessor` 只提供读取入口。写入必须通过认证状态服务完成。

## 4. 认证服务

`IAuthenticationService` 负责统一入口：

```text
SignInAsync
SignOutAsync
RefreshAsync
RestoreAsync
ChallengeAsync
```

规则：

- 所有方法接收 `CancellationToken`。
- 登录流程不能阻塞 UI Thread。
- 登录 UI 由 Presentation 或应用提供，Security 只发起 challenge 或返回认证请求。
- 登出必须取消未完成 refresh，并清理 token 引用。
- 恢复会话发生在 Application 启动或解锁时。

## 5. Token 和凭据

Data 管线通过 `IAccessTokenProvider` 获取认证信息。

规则：

- Token 不能作为普通全局状态随意暴露。
- Token 获取必须支持取消。
- Token 快过期时可以触发 refresh。
- Refresh 期间并发请求应共享同一次 refresh，避免重复刷新。
- Refresh 失败后状态进入 Expired 或 SignedOut。
- 具体存储方式由应用或平台实现，Security 只定义抽象。

## 6. 状态通知

认证状态变化需要通知：

- Route Guard。
- Command authorization source。
- Data auth pipeline。
- Presentation 用户信息展示。
- State 同步器。
- Plugin capability checker。

通知必须带版本号或 revision，避免旧状态覆盖新状态。

```text
Authentication state changed
-> update SecurityStateStore
-> publish scoped notification
-> recompute route / command / data authorization
-> update Presentation display
```

## 7. Desktop 场景

桌面应用必须考虑：

- 应用启动时恢复认证状态。
- 应用从休眠或锁屏恢复后重新校验。
- 用户主动切换账号。
- 离线状态下保留有限认证信息。
- 本地缓存损坏。
- 多窗口共享当前主体。

Security 不决定这些策略的具体 UI，只提供状态、错误和扩展点。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| 登录取消 | 返回 canceled，不进入 fatal error。 |
| 登录失败 | 状态进入 Failed，记录诊断。 |
| Refresh 失败 | 状态进入 Expired 或 SignedOut。 |
| Token 不可用 | Data 管线收到 authentication unavailable。 |
| 恢复会话失败 | 进入 Anonymous 或 Failed，按 Host 策略决定。 |

## 9. 测试策略

测试替身：

- Fake authentication state provider。
- Fake authentication service。
- Fake access token provider。
- Test principal builder。

必须覆盖：

- 启动恢复为 anonymous。
- 登录成功后主体变化。
- 登出清理 token。
- Refresh 并发合并。
- Refresh 失败触发状态变化。
- 状态变化触发 Command 和 Guard 重新计算。
