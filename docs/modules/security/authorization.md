# AtomUI.City.Security Authorization 设计

版本：v0.1
状态：正式初版
适用范围：Policy、Requirement、授权结果、Challenge、Forbidden、授权执行器和跨模块授权集成。

## 1. 定位

Authorization 子模块负责回答一个问题：当前主体是否允许执行某个受保护动作。

它不定义业务权限模型，不持久化用户权限，不决定 UI 如何展示授权失败。

## 2. 授权输入

授权评估输入包含：

- 当前 `ClaimsPrincipal`。
- Permission name。
- Policy name。
- Resource descriptor。
- Route / Command / Data / Plugin 上下文。
- Contribution 信息。
- CancellationToken。

授权输入必须是可序列化诊断的 descriptor，不应直接包含 AtomUI/Avalonia 控件实例。

## 3. Policy

Policy 是授权规则组合。

典型 requirement：

| Requirement | 说明 |
|---|---|
| Authenticated | 需要登录。 |
| Permission | 需要指定权限点。 |
| Claim | 需要指定 claim。 |
| Role | 需要指定 role claim。 |
| PluginCapability | 需要 Host 授予插件 capability。 |
| CustomRequirement | 应用自定义 requirement。 |

Policy descriptor 必须由显式声明或 Source Generator manifest 提供，运行时默认不扫描程序集。

## 4. 授权结果

授权不返回裸 bool。

建议结果：

```text
Allowed
Denied
Forbidden
Challenge
Failed
Cancelled
```

语义：

| 结果 | 说明 |
|---|---|
| `Allowed` | 授权通过。 |
| `Denied` | 策略不满足，但不区分登录和权限。 |
| `Forbidden` | 已认证，但权限不足。 |
| `Challenge` | 需要登录、刷新或重新认证。 |
| `Failed` | Policy 或 evaluator 自身失败。 |
| `Cancelled` | 授权检查被取消。 |

Route、Command、Data 可以把结果映射成自己的行为，但不能改变 Security 的语义。

## 5. Evaluator

`IAuthorizationEvaluator` 负责执行 policy。

规则：

- 必须支持异步。
- 必须支持取消。
- 不能访问 UI 对象。
- 不能阻塞 UI Thread。
- 可以使用缓存，但缓存必须带认证状态 revision 和 contribution revision。
- Policy 异常返回 Failed，并记录诊断。

## 6. Challenge 和 Forbidden

`Challenge` 表示需要认证动作，例如登录或刷新。

`Forbidden` 表示当前主体已认证，但不具备所需权限。

默认映射：

| 结果 | Route | Command | Data | Presentation |
|---|---|---|---|---|
| Challenge | 登录或重定向 | 不可执行 | 401 处理 | 登录交互 |
| Forbidden | 拒绝或拒绝访问页 | 不可执行 | 403 处理 | 权限不足提示 |

Presentation 可以展示 UI，但不能重新解释授权结果。

## 7. 缓存策略

授权结果可以缓存，但必须受以下因素影响：

- Principal revision。
- Permission manifest revision。
- Policy manifest revision。
- Plugin contribution revision。
- Route / Command / resource identity。

用户切换、登录态变化、插件停用、权限贡献撤销都必须让相关缓存失效。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| Policy 不存在 | Failed，并记录 manifest 诊断。 |
| Requirement 未注册 | Failed。 |
| Evaluator 抛异常 | Failed，进入 ErrorPolicy。 |
| 授权取消 | Cancelled。 |
| 插件 requirement 已撤销 | Failed 或 Forbidden，按场景返回。 |

## 9. 测试策略

测试必须覆盖：

- authenticated requirement。
- permission requirement。
- claim / role requirement。
- Challenge 和 Forbidden 区分。
- evaluator 异常。
- 缓存随 principal revision 失效。
- 插件 contribution 撤销后授权重新计算。
