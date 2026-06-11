# AtomUI.City.Security Detailed Design

版本：v0.1
状态：正式初版
适用范围：认证状态、当前主体、权限声明、授权策略、Route Guard、Command 权限联动、Data 认证集成、Plugin capability、AOT/source generator 和测试策略。

## 1. 定位

`AtomUI.City.Security` 是框架级认证状态与授权决策模块。

Security 不实现具体身份系统，不提供登录 UI，不内置用户、租户、组织和角色管理业务。它只提供统一 contract，让 Routing、Mvvm Command、Data、PluginSystem、Presentation 都使用同一套认证和权限判断。

核心链路：

```text
Authentication state
-> Principal / claims
-> Permission / policy evaluation
-> Routing Guard / Command CanExecute / Data auth / Plugin capability
-> Presentation display feedback
```

模块边界：

- Security 负责认证状态、当前主体、权限、Policy 和授权结果。
- Routing 负责导航事务和 Guard 执行，不解释权限语义。
- Mvvm 提供 Command 状态接入点，不解释权限语义。
- Data 负责请求管线、重试、缓存和错误模型，通过 Security 获取认证信息。
- PluginSystem 负责插件发现、加载和卸载，通过 Security 约束插件权限和 capability。
- Presentation 负责登录、拒绝访问、命令禁用和权限提示等 UI 表达，不做授权决策。

## 2. 设计原则

- .NET-first：优先使用 `ClaimsPrincipal`、`Claim`、Options、DI、Hosted service、CancellationToken。
- Security makes decisions：认证和授权判断只能由 Security 统一执行。
- UI-independent：Security 不直接引用 AtomUI/Avalonia，不打开窗口，不显示对话框。
- Observable：认证状态、主体变化和权限变化必须可观察。
- AOT-first：权限、Policy、Route/Command 授权声明由 Source Generator 生成 manifest。
- Plugin-aware：插件可以贡献权限点和授权元数据，但 Host 解释、授权和撤销。
- Desktop-aware：支持本地会话、Token 缓存引用、离线状态、锁定/解锁、用户切换和应用恢复。
- Diagnostics-first：授权失败必须能解释是未登录、权限不足、Policy 异常、插件撤销还是认证过期。

## 3. 非目标

Security 不负责：

- 登录界面。
- 用户管理业务。
- 角色管理业务。
- 租户、组织、部门等业务模型。
- 具体 OAuth/OIDC/SAML 客户端实现。
- 密码存储策略实现。
- 插件沙箱。
- Data 请求重试、缓存和错误模型。
- UI 可见性具体样式。

这些由应用、Data、PluginSystem、Presentation 或平台层实现。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `IAuthenticationStateProvider` | 提供当前认证状态，并发布变化通知。 |
| `IAuthenticationService` | 登录、登出、刷新、恢复会话的抽象入口。 |
| `ICurrentPrincipalAccessor` | 读取当前 `ClaimsPrincipal`。 |
| `IPermissionChecker` | 检查 permission 是否满足。 |
| `IAuthorizationPolicyProvider` | 提供 policy descriptor。 |
| `IAuthorizationEvaluator` | 执行 policy 判断。 |
| `IAccessTokenProvider` | 为 Data 管线提供 token 或 credential。 |
| `ISecurityStateStore` | 保存当前认证状态快照。 |
| `ISecurityContributionRegistry` | 管理模块/插件贡献的权限、Policy 和授权元数据。 |
| `ISecurityDiagnostics` | 输出认证和授权诊断。 |

命名不加 `City` 前缀。

## 5. 认证状态

认证状态建议统一建模为：

```text
Unknown
Anonymous
Authenticating
Authenticated
Refreshing
Expired
SignedOut
Failed
```

状态快照包含：

- `ClaimsPrincipal`。
- Authentication scheme。
- Access token 引用或获取句柄。
- Refresh 状态。
- 过期时间。
- 来源 Contribution。
- 诊断信息。

Security 是认证状态源。State 模块可以同步认证状态，用于 ViewModel 和 UI 订阅，但不能成为认证状态的权威写入方。

详细规则见：[authentication.md](authentication.md)。

## 6. 权限、Policy 和 Capability

第一版区分三层：

| 概念 | 说明 |
|---|---|
| Permission | 稳定权限点，例如 `settings.read`、`project.build`。 |
| Policy | 组合规则，例如需要登录、需要某权限、需要 claim、需要插件 capability。 |
| Capability | Host 授权插件可使用的框架能力，例如贡献路由、发事件、访问 Data client。 |

Permission 是可声明、可本地化、可诊断的稳定标识。Policy 是运行时决策规则。Capability 是插件和 Host 的安全边界声明。

详细规则见：

- [permissions.md](permissions.md)
- [authorization.md](authorization.md)
- [plugin-integration.md](plugin-integration.md)

## 7. Routing 集成

Routing 不解释权限，只执行 Security 提供或 Security 驱动的 Guard。

```text
Route matched
-> Route auth metadata
-> Security route guard
-> AuthorizationEvaluator
-> Allow / Reject / Redirect / Challenge
```

结果语义：

| 结果 | Routing 行为 |
|---|---|
| Allow | 继续导航。 |
| Reject | 导航 rejected，保持当前页面。 |
| Redirect | 交给 NavigationTransaction 统一重定向。 |
| Challenge | 触发登录或认证恢复流程。 |
| Failed | 导航 failed，进入诊断。 |

详细规则见：[route-integration.md](route-integration.md)。

## 8. Command 集成

Command 的 `CanExecute` 可以接入 Security，但 Mvvm 不实现权限逻辑。

```text
Command auth metadata
-> Security command authorization source
-> CanExecute recompute
-> Presentation updates enabled / disabled state
```

权限变化、登录态变化、当前路由变化都应触发相关 Command 状态刷新。Presentation 只展示禁用、隐藏或提示，不做权限判断。

详细规则见：[command-integration.md](command-integration.md)。

## 9. Data 集成

Data 管线通过 Security 获取认证信息：

```text
Data request
-> AccessTokenProvider
-> attach auth header / credential
-> send request
-> 401 / 403 handling
-> Security state refresh or authorization failure
```

401 默认表示认证失效或需要刷新。403 默认表示认证有效但权限不足。具体 UI 反馈由 Presentation 或应用决定。

详细规则见：[data-integration.md](data-integration.md)。

## 10. PluginSystem 集成

插件可以贡献：

- Permission descriptor。
- Policy requirement descriptor。
- Route auth metadata。
- Command auth metadata。
- Data client auth metadata。

插件不能：

- 自己解释全局权限。
- 绕过 Host Security。
- 修改 Host root principal。
- 把权限结果静态缓存到 Host。
- 把插件私有类型泄漏到 Host policy contract。

插件停用时必须撤销它贡献的权限、Policy、Route/Command 授权元数据，并触发相关 Guard/Command 重新计算。

详细规则见：[plugin-integration.md](plugin-integration.md)。

## 11. Presentation 集成

Presentation 只消费 Security 结果：

- Route 被拒绝后的拒绝访问视图。
- Challenge 后的登录交互。
- Command 禁用、隐藏或提示。
- 权限不足提示。
- 用户信息展示。
- 登录态切换后的 UI 刷新。

Presentation 不直接读取权限存储，不解释 Policy。

## 12. AOT 和 Source Generator

Security generator 负责生成：

- Permission manifest。
- Policy manifest。
- Route auth metadata。
- Command auth metadata。
- Plugin permission contribution descriptor。
- 重复权限点诊断。
- 未声明权限引用诊断。
- 插件权限泄漏诊断。

运行时默认不扫描程序集找权限声明。

## 13. 错误策略

| 场景 | 默认处理 |
|---|---|
| 未登录访问受保护路由 | Challenge 或 Redirect。 |
| 权限不足 | Reject / Forbidden。 |
| Token 过期 | 尝试 Refresh，失败后 SignedOut。 |
| Policy 抛异常 | Failed，进入 diagnostics。 |
| 插件权限撤销失败 | 聚合错误，继续撤销其他 contribution。 |
| Data 401 | 通知 Security 刷新或退出登录。 |
| Data 403 | 返回 authorization failure，不自动重试。 |

Security 错误不能静默吞掉，必须进入授权结果或诊断。

## 14. 测试策略

Testing 包应提供：

- Fake principal。
- Fake authentication state provider。
- Fake permission checker。
- Fake policy evaluator。
- Route guard test helper。
- Command authorization test helper。
- Data auth pipeline test helper。
- Plugin permission contribution test host。

必须覆盖：

- 匿名、已登录、过期、刷新失败。
- Route allow / reject / redirect / challenge。
- Command `CanExecute` 随权限变化刷新。
- Data 401 / 403 映射。
- 插件权限贡献和撤销。
- Source Generator 重复权限诊断。

详细规则见：[diagnostics-and-testing.md](diagnostics-and-testing.md)。
