# Security 测试设计

版本：v0.1
状态：正式初版
适用范围：认证状态、ClaimsPrincipal、权限、Policy、Route Guard、Command、Data、Plugin capability 和诊断

## 1. 目标

Security 测试必须证明认证状态和授权策略在路由、命令、数据访问和插件能力中一致生效。

## 2. SecurityTestKit

Testing 提供：

- fake principal。
- fake authentication state provider。
- fake authentication service。
- fake access token provider。
- fake permission checker。
- fake authorization evaluator。
- route authorization helper。
- command authorization helper。
- data auth pipeline helper。
- plugin capability authorization helper。

## 3. 单元测试范围

必须覆盖：

- anonymous principal。
- authenticated principal。
- claims lookup。
- permission allow/deny。
- policy allow/deny。
- authentication state change。
- permission refresh。
- diagnostics。

## 4. 集成测试范围

必须覆盖：

- Routing guard 授权。
- Command can execute 授权。
- Data token 注入。
- Data 认证失败映射。
- Plugin capability 授权。
- 权限变化触发 UI 入口刷新。

## 5. 插件安全测试

必须覆盖：

- capability requested。
- capability granted。
- capability denied。
- 未授权 Contribution 被拒绝。
- 插件 private contract 泄漏。

## 6. 测试要求

Security 测试不依赖真实认证服务。真实身份提供方只放应用级集成测试。
