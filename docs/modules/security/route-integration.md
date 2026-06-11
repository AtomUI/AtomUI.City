# AtomUI.City.Security Route Integration 设计

版本：v0.1
状态：正式初版
适用范围：Route 授权元数据、Security Guard、Challenge、Redirect、Reject、Forbidden 和导航诊断。

## 1. 定位

Route integration 负责把路由进入决策接入 Security 授权系统。

Routing 负责执行导航事务和 Guard 顺序。Security 负责解释 route auth metadata 并返回授权结果。

## 2. Route Auth Metadata

Route 可以声明：

- 需要登录。
- 需要 permission。
- 需要 policy。
- 匿名可访问。
- 授权失败 fallback route。
- Challenge 行为。

Route metadata 必须进入 Route descriptor，由 Routing 的 Source Generator 输出。Security 不在运行时扫描路由类型。

## 3. Guard 流程

```text
Route matched
-> Routing builds Guard context
-> Security route guard reads route auth metadata
-> AuthorizationEvaluator evaluates
-> GuardResult returned
-> NavigationTransaction continues / rejects / redirects / challenges
```

Routing 传给 Security 的上下文：

- RouteId。
- Route metadata。
- 当前 principal。
- Route 参数。
- Contribution 来源。
- Navigation transaction id。
- CancellationToken。

Security 不访问 UI 对象，不创建 ViewModel，不修改 NavigationSnapshot。

## 4. 结果映射

| Authorization result | Guard result | Routing 行为 |
|---|---|---|
| Allowed | Allow | 继续导航。 |
| Challenge | Redirect 或 Reject with challenge | 进入登录流程或保持当前页面。 |
| Forbidden | Reject | 拒绝导航，可由 Presentation 展示拒绝访问。 |
| Denied | Reject | 拒绝导航。 |
| Failed | Failed | 导航失败，记录诊断。 |
| Cancelled | Cancel | 导航取消。 |

Redirect 必须由 NavigationTransaction 统一处理，Security Guard 内部不能直接调用 `IRouter`。

## 5. Challenge

Challenge 表示需要认证动作。

可能行为：

- 跳转登录路由。
- 触发登录 Interaction。
- 尝试 refresh session。
- 返回 rejected 并让应用决定。

具体策略由 Host 配置，不由 Routing 或 Presentation 私自决定。

## 6. 插件路由

插件路由授权必须携带 Contribution 信息。

规则：

- 插件 route auth metadata 必须来自插件 manifest 或 source generator descriptor。
- 插件停用后，该插件路由的授权缓存必须失效。
- 插件不能声明覆盖 Host route 的权限语义。
- 插件私有 requirement 类型不能泄漏到 Host policy contract。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| route auth metadata 无效 | Guard failed。 |
| permission 未声明 | Guard failed。 |
| policy 不存在 | Guard failed。 |
| 未登录 | Challenge。 |
| 权限不足 | Reject / Forbidden。 |
| evaluator 异常 | Guard failed。 |

## 8. 测试策略

测试必须覆盖：

- 匿名路由放行。
- 需要登录路由返回 Challenge。
- 权限不足返回 Reject。
- Redirect 策略。
- 插件路由停用后缓存失效。
- Guard cancellation。
