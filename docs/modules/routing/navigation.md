# AtomUI.City.Routing Navigation 设计

版本：v0.1
状态：初版草案
适用范围：IRouter、NavigationScope、NavigationTarget、NavigationTransaction、NavigationResult、并发策略、提交和回滚。

## 1. 定位

Navigation 是 Routing 的运行时核心。它把开发者的导航请求转换为事务式路由切换，并保证当前页面在候选页面准备好之前不被破坏。

## 2. IRouter

`IRouter` 是某个 NavigationScope 内的导航入口。

建议接口：

```csharp
public interface IRouter
{
    ValueTask<NavigationResult> NavigateAsync(
        RouteReference route,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> NavigateAsync<TParameters>(
        RouteReference<TParameters> route,
        TParameters parameters,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> NavigateByPathAsync(
        string path,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> BackAsync(
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> ForwardAsync(
        CancellationToken cancellationToken = default);
}
```

`NavigateByPathAsync` 只作为 Deep Link、命令行入口、外部 URI、测试和兼容入口，不作为日常代码主路径。

## 3. NavigationTarget

所有请求先规范化为 `NavigationTarget`。

来源：

- RouteReference。
- RouteReference + parameters。
- Path。
- Deep Link URI。
- Journal entry。
- Redirect result。

`NavigationTarget` 应包含：

- Target kind。
- RouteId 或 path。
- 强类型参数。
- Query 参数。
- Fragment。
- NavigationOptions。
- 来源诊断信息。

## 4. NavigationOptions

建议选项：

| 选项 | 说明 |
|---|---|
| `Mode` | Push、Replace、Reset。 |
| `HistoryBehavior` | Record、Skip、ReplaceCurrent。 |
| `ConcurrencyPolicy` | CancelPrevious、Queue、RejectIfBusy。 |
| `RestoreState` | 是否从 Journal 恢复 route state。 |
| `ForceReload` | 是否忽略可复用分支。 |
| `AllowRedirect` | 是否允许 Guard / Resolver 重定向。 |
| `Timeout` | 导航超时。 |

默认模式为 Push，默认并发策略为同 Scope 内 Commit 前取消旧导航，Commit 中排队。

## 5. NavigationScope

NavigationScope 生命周期通常挂在 WindowScope 下。

它负责：

- 持有 Router。
- 持有当前 NavigationSnapshot。
- 持有 Journal。
- 串行化导航。
- 跟踪活动 RouteScope。
- 接入 UI Dispatcher。
- 接入诊断。

NavigationScope 停止时：

```text
Reject new navigation
-> Cancel running transaction
-> Dispose active route tree
-> Clear journal
-> Mark stopped
```

## 6. NavigationTransaction

一次导航创建一个 transaction。

阶段：

```text
Created
-> Matching
-> Planning
-> Guarding
-> ConfirmingLeave
-> Resolving
-> CreatingViewModels
-> PreparingCommit
-> Committing
-> Completed
```

失败终态：

```text
Rejected
Cancelled
Failed
RolledBack
```

Transaction 必须记录阶段耗时和阶段结果。

## 7. 导航计划

NavigationPlan 由当前路由树和目标路由树 diff 得出。

应包含：

- 保留的 route branch。
- 离开的 route branch。
- 新增的 route branch。
- 参数变化但 route id 相同的 branch。
- 需要重新解析数据的 branch。
- 需要重新激活 ViewModel 的 branch。
- Outlet commit plan。

共同父路由默认保留，减少不必要的 ViewModel 重建。

## 8. Provisional RouteScope

新增路由分支先创建 provisional RouteScope。

规则：

- Provisional RouteScope 不进入当前 NavigationSnapshot。
- Provisional RouteScope 可以创建服务作用域。
- Provisional RouteScope 可以运行 Resolver。
- Provisional RouteScope 可以创建候选 ViewModel。
- Commit 失败或准备失败时必须释放。

这样可以保证当前页面在候选页面准备完成前仍保持活动。

## 9. Commit

Commit 必须在 UI Thread 上执行。

Commit 步骤：

```text
Stop accepting current route operations that will leave
-> Apply Presentation outlet changes
-> Switch current NavigationSnapshot atomically
-> Mark candidate RouteScopes active
-> Activate added ViewModels
-> Deactivate removed ViewModels
-> Update Journal
```

如果 Presentation commit 失败：

- 恢复原 Outlet 状态。
- 释放候选 RouteScope。
- 保持原 NavigationSnapshot。
- 返回 Failed。

Commit 开始后不允许被新导航打断。

## 10. 回滚

准备阶段回滚：

```text
Cancel provisional scopes
-> Dispose provisional ViewModels
-> Dispose provisional service scopes
-> Keep current snapshot unchanged
```

Commit 阶段失败回滚：

```text
Try restore old outlet content
-> Reactivate old branch if needed
-> Dispose candidate branch
-> Report commit failure
```

回滚失败必须进入 ErrorPolicy，并保留最大诊断信息。

## 11. 并发策略

同一个 NavigationScope 内默认串行。

策略：

| 策略 | 行为 |
|---|---|
| `CancelPrevious` | 新请求取消 Commit 前旧请求。 |
| `Queue` | 新请求排队。 |
| `RejectIfBusy` | 正在导航时直接拒绝。 |

Commit 中始终排队或拒绝，不取消。

不同 NavigationScope 可以并行导航，但如果共享插件卸载、全局配置切换等外部操作，需要通过对应模块的生命周期锁协调。

## 12. Redirect

Guard 或 Resolver 可以返回 redirect。

规则：

- Redirect 生成新的 NavigationTarget。
- Redirect 继承原导航的诊断链。
- Redirect 计数必须有限制。
- 静态 redirect 循环由 Source Generator 诊断。
- 动态 redirect 循环由运行时检测。

## 13. NavigationResult

建议结果：

| 结果 | 说明 |
|---|---|
| `Success` | 导航完成。 |
| `Rejected` | Guard 或策略拒绝。 |
| `Redirected` | 已重定向并完成或交给后续目标。 |
| `Cancelled` | 被取消。 |
| `Failed` | 异常失败。 |
| `NotFound` | 无匹配路由。 |
| `StaleRouteGraph` | 使用的路由图已不可用。 |
| `ContributionRevoked` | 目标贡献已撤销。 |

结果必须包含 NavigationId、目标、失败阶段和诊断信息。

## 14. 状态暴露

NavigationScope 应暴露当前状态：

```text
IStateValue<NavigationSnapshot>
IStateValue<NavigationStatus>
```

外部观察当前路由状态使用 State。EventBus 只发布导航完成、失败、取消等事实事件，不作为控制流。

## 15. 测试要求

测试必须覆盖：

- RouteReference 导航。
- Path 导航。
- Replace / Reset。
- Back / Forward。
- Guard 拒绝后当前页面不变。
- Resolver 失败后释放候选 scope。
- Commit 失败回滚。
- Commit 中新导航排队。
- 不同 NavigationScope 并行。
- Redirect 循环检测。
