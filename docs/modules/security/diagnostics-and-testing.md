# AtomUI.City.Security Diagnostics and Testing 设计

版本：v0.1
状态：正式初版
适用范围：认证诊断、授权诊断、错误策略、测试替身和集成测试工具。

## 1. 定位

Security 必须能解释每一次认证和授权结果。授权失败不能只表现为按钮不可用或导航没发生。

Diagnostics and Testing 子模块负责定义诊断字段、错误分类和测试工具。

## 2. 诊断字段

认证诊断应包含：

- AuthenticationState。
- Principal id 或匿名标记。
- Scheme。
- Token expiry。
- Refresh attempt id。
- Failure reason。
- ScopeId。
- ContributionId。

授权诊断应包含：

- Authorization result。
- Permission name。
- Policy name。
- Requirement name。
- Resource type。
- RouteId / CommandId / DataClientId。
- PluginId。
- ContributionId。
- Principal revision。
- Policy manifest revision。

敏感信息不能写入日志，例如 access token、refresh token、密码、完整 credential。

## 3. 错误分类

建议分类：

| 分类 | 说明 |
|---|---|
| AuthenticationRequired | 需要登录或重新认证。 |
| AuthenticationExpired | 认证过期。 |
| Forbidden | 已认证但权限不足。 |
| PolicyNotFound | Policy 不存在。 |
| PermissionNotFound | Permission 未声明。 |
| RequirementFailed | Requirement 不满足。 |
| EvaluatorFailed | Evaluator 异常。 |
| ContributionRevoked | 来源贡献已撤销。 |
| CapabilityDenied | 插件 capability 被拒绝。 |

## 4. ErrorPolicy 集成

Security 错误处理规则：

- 授权不通过不是 fatal error。
- Policy/evaluator 异常进入 ErrorPolicy，但返回明确 Failed。
- 认证 refresh 失败不直接杀死应用。
- 插件撤销失败聚合错误并继续清理。
- 敏感信息必须脱敏。

## 5. Testing 包

Testing 包应提供：

- `TestPrincipalBuilder`。
- `FakeAuthenticationStateProvider`。
- `FakeAuthenticationService`。
- `FakeAccessTokenProvider`。
- `FakePermissionChecker`。
- `FakeAuthorizationEvaluator`。
- `RouteAuthorizationTestHost`。
- `CommandAuthorizationTestHost`。
- `DataAuthPipelineTestHost`。
- `PluginSecurityContributionTestHost`。

命名最终以实现阶段 API 规范为准，但能力必须覆盖这些场景。

## 6. 测试场景

必须覆盖：

- anonymous / authenticated / expired / signed out。
- 登录成功、登录取消、登录失败。
- refresh 成功、失败、并发合并。
- Route allow / reject / redirect / challenge。
- Command `CanExecute` 随权限变化刷新。
- Data 401 / 403 映射。
- 插件权限贡献、冲突、撤销。
- Capability deny。
- Source Generator 重复权限诊断。
- 未声明权限引用诊断。

## 7. 无 UI 测试

Security 测试必须能在无真实 AtomUI/Avalonia UI 的环境中运行。

Presentation 相关行为只验证 Security 输出：

- Challenge result。
- Forbidden result。
- Command disabled state。
- Diagnostic record。

不验证具体 UI 控件样式。
