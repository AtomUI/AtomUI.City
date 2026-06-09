# AtomUI.City.Security

版本：v0.1
状态：初版草案

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

Security 不负责：

- 具体登录 UI。
- 用户管理业务。
- 租户、组织、角色等具体业务模型。

## 后续拆分

- `authentication.md`
- `authorization.md`
- `permissions.md`
- `route-integration.md`
- `command-integration.md`
