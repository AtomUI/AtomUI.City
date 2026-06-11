# AtomUI.City.Presentation Route Outlet 设计

版本：v0.1
状态：正式初版
适用范围：Route Outlet、commit plan、attach/detach/replace、提交失败回滚和诊断

## 1. 定位

Route Outlet 是 Routing 和 Presentation 的提交边界。

Routing 输出 Outlet commit plan，Presentation 执行 UI 提交。

```text
Commit plan
-> Find outlets
-> Create/bind new views
-> Attach or detach reused views
-> Update visual tree
-> Return commit result
```

## 2. IRouteOutlet

`IRouteOutlet` 应支持：

- Outlet name。
- 当前 content。
- Attach。
- Detach。
- Replace。
- Clear。
- Commit diagnostics。

默认 Outlet 名为 `primary`。

## 3. 规则

- Outlet 名称稳定，不能运行时动态变更。
- 命名 Outlet 不自动创建新的 NavigationScope。
- Commit 必须在 UI Thread。
- Commit 失败时必须尽量恢复旧 content。
- Presentation 不决定导航成功，只返回 commit result。

## 4. 失败回滚

```text
Presentation commit failed
-> detach newly created view
-> dispose binding
-> dispose provisional ActivationScope
-> dispose provisional RouteScope
-> keep old outlet content
-> navigation failed with diagnostics
```

## 5. Routing 集成

Routing 提供：

- NavigationTransaction id。
- Outlet commit plan。
- ViewModel instance。
- RouteContext。
- Reuse / KeepAlive 指令。
- Contribution 信息。

Presentation 返回：

- Commit success。
- Commit failed。
- Failure stage。
- Created views。
- Attached / detached views。
- Disposal diagnostics。

## 6. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| primary outlet | Unit | 默认 Outlet 可提交内容。 |
| named outlet | Unit | 按名称找到目标 Outlet。 |
| replace | Unit | 旧 View detach，新 View attach。 |
| commit failure | Unit | 旧 content 保留。 |
| disposal diagnostics | Unit | detach/dispose 失败被聚合。 |
