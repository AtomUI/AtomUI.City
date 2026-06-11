# AtomUI.City.Security

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Security` 负责认证状态、权限检查、授权策略、路由权限联动和命令权限联动。

Security 提供业务无关的认证与权限基础设施，不内置具体身份系统。

## 边界

Security 负责：

- 当前认证状态。
- 权限声明。
- 权限检查。
- 授权策略。
- 路由守卫集成。
- 命令可执行状态集成。
- Data 请求认证集成。
- 插件权限和能力声明集成。

Security 不负责：

- 具体登录 UI。
- 用户管理业务。
- 租户、组织、角色等具体业务模型。
- 具体 OAuth/OIDC/SAML 客户端实现。
- Data 请求重试、缓存和错误模型。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Security 总体架构、认证状态、权限、Policy、Route/Command/Data/Plugin 集成、AOT 和测试策略。 |
| [authentication.md](authentication.md) | 认证状态、当前主体、登录/登出/刷新/恢复会话、Token 访问和状态通知。 |
| [authorization.md](authorization.md) | Policy、Requirement、AuthorizationEvaluator、Challenge/Forbidden 语义和授权结果。 |
| [permissions.md](permissions.md) | Permission descriptor、模块/插件权限贡献、命名、撤销和 Source Generator 诊断。 |
| [route-integration.md](route-integration.md) | Route Guard、Route auth metadata、Challenge、Redirect、Reject 和导航诊断。 |
| [command-integration.md](command-integration.md) | Command 权限元数据、CanExecute 联动、权限变化刷新和 Presentation 表达。 |
| [data-integration.md](data-integration.md) | AccessTokenProvider、请求认证注入、401/403 处理和 Data 管线边界。 |
| [plugin-integration.md](plugin-integration.md) | 插件权限贡献、Capability、Host 授权、撤销和跨插件 contract 约束。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | Security 诊断字段、错误策略、Fake provider 和授权测试工具。 |

## 可选增强文档

- `claims.md`
- `token-management.md`
- `presentation-integration.md`
