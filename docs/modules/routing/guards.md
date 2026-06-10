# AtomUI.City.Routing Guards 设计

版本：v0.1
状态：初版草案
适用范围：Match Policy、Enter Guard、Leave Guard、离开确认、重定向、权限集成、线程和诊断。

## 1. 定位

Guard 是导航决策点。它决定某个路由是否可以匹配、是否可以进入、当前路由是否可以离开。

Guard 不负责数据加载，不负责 UI 渲染，不保存业务状态。

## 2. Guard 类型

第一版建议三类：

| 类型 | 执行时机 | 用途 |
|---|---|---|
| `IRouteMatchPolicy` | 路由匹配阶段 | 功能开关、插件状态、环境条件。 |
| `IRouteEnterGuard` | 进入候选路由前 | 登录、权限、运行条件。 |
| `IRouteLeaveGuard` | 离开当前路由前 | 未保存修改、任务中断确认。 |

Match Policy 返回 false 时，该路由不参与匹配。Enter/Leave Guard 返回拒绝时，本次导航失败或重定向。

## 3. 结果模型

Guard 不返回裸 bool。

建议结果：

```text
Allow
Reject(reason)
Redirect(target)
Cancel
Failed(error)
```

Reject 是业务或策略拒绝。Failed 是 Guard 自身异常或不可恢复错误。Cancel 表示用户取消或外部取消。

## 4. 执行顺序

进入目标路由：

```text
Match root policy
-> Match child policy
-> Enter root guard
-> Enter child guard
```

离开当前路由：

```text
Leave leaf guard
-> Leave parent guard
```

这样更符合桌面页面退出语义：最内层页面先确认是否能离开。

## 5. Leave Guard 与 MVVM

Mvvm 提供 ViewModel 侧能力，例如：

```text
ICanDeactivate
IConfirmDeactivate
```

Routing 在 Leave Guard 阶段调用这些能力。

规则：

- Mvvm 只定义 ViewModel 能力和结果。
- Routing 决定是否继续导航。
- Presentation 只承接需要 UI 的 Interaction。
- 用户取消返回 Cancel，不是异常。
- 插件强制停用时可以进入强制离开策略。

## 6. 权限集成

Security 模块可以提供 Guard。

Routing 只传入：

- RouteId。
- Route metadata。
- 当前 principal。
- 参数。
- Contribution 来源。

Routing 不存储权限，也不解释业务权限语义。

## 7. 重定向

Guard 可以返回 redirect。

规则：

- Guard 内部不直接调用 `IRouter`。
- Redirect 由 NavigationTransaction 统一处理。
- Redirect 目标重新进入完整导航流程。
- Redirect 必须带来源诊断。
- Redirect 次数有限制。

典型场景：

- 未登录跳转登录路由。
- 无权限跳转拒绝访问路由。
- 插件路由失效跳转安全 fallback。

## 8. 线程和取消

Guard 必须接收 CancellationToken。

规则：

- Guard 不访问 UI 对象。
- 需要用户确认时通过 MVVM Interaction 进入 Presentation。
- 长耗时 Guard 应通过 OperationScope 或受管后台任务执行。
- 插件停用会取消插件 Guard。
- 导航取消时 Guard 应尽快返回 Cancel。

## 9. AOT 和注册

Guard 通过 Route Map 显式声明。

```csharp
[RouteGuards(typeof(ProfileAccessGuard))]
```

Source Generator 必须校验：

- 类型实现对应 contract。
- 类型可由 DI 创建。
- 插件路由引用类型不越界。
- Guard 顺序稳定。

运行时不扫描程序集找 Guard。

## 10. 错误策略

| 场景 | 默认处理 |
|---|---|
| Match Policy false | 尝试其他候选路由。 |
| Enter Guard Reject | Navigation rejected。 |
| Leave Guard Reject | 保持当前页面。 |
| Guard Cancel | Navigation cancelled。 |
| Guard 抛异常 | Navigation failed。 |
| Redirect 循环 | Navigation failed with diagnostics。 |

## 11. 诊断

必须记录：

- Guard 类型。
- 所属 RouteId。
- 所属 Contribution。
- 执行顺序。
- 耗时。
- 结果。
- Redirect 目标。
- 错误信息。

## 12. 测试要求

测试必须覆盖：

- Match Policy 排除候选路由。
- Enter Guard 拒绝。
- Leave Guard 拒绝。
- 用户取消离开。
- Guard redirect。
- Redirect 循环。
- Guard 异常。
- 插件停用取消 Guard。
