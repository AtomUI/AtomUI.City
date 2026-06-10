# AtomUI.City.Routing Diagnostics and Testing 设计

版本：v0.1
状态：初版草案
适用范围：Routing 诊断事件、日志、错误模型、测试工具、断言能力和无 UI 测试策略。

## 1. 定位

Routing 必须可诊断、可测试。导航失败不能只表现为页面没变化，必须能说明失败发生在哪个阶段、哪个路由、哪个 Guard、哪个 Resolver 或哪个插件贡献。

## 2. 诊断目标

诊断需要服务：

- 开发时调试。
- 自动化测试。
- 插件卸载排查。
- 现场问题定位。
- 性能分析。
- 用户可理解错误提示。

## 3. Navigation Diagnostic Record

每次导航应产生诊断记录。

字段：

| 字段 | 说明 |
|---|---|
| `NavigationId` | 导航唯一标识。 |
| `NavigationScopeId` | 所属 NavigationScope。 |
| `RouteGraphVersion` | 捕获的路由图版本。 |
| `Target` | 目标 RouteId 或 path。 |
| `Source` | RouteReference、Path、Journal、Redirect 等。 |
| `MatchedRoutes` | 匹配路由链。 |
| `Plan` | 保留、离开、新增分支。 |
| `Result` | 成功、拒绝、取消、失败等。 |
| `FailedStage` | 失败阶段。 |
| `Elapsed` | 总耗时。 |
| `Contribution` | 相关贡献。 |

## 4. Stage Diagnostic

每个阶段记录：

- 阶段名称。
- 开始时间。
- 结束时间。
- 耗时。
- 输入摘要。
- 输出结果。
- 取消状态。
- 错误。

阶段包括：

```text
NormalizeTarget
MatchRoute
BuildPlan
RunEnterGuards
ConfirmLeave
ResolveData
CreateViewModel
PrepareCommit
Commit
UpdateJournal
DisposeRemovedBranches
```

## 5. Guard / Resolver 诊断

Guard 记录：

- Guard 类型。
- RouteId。
- 结果。
- Redirect 目标。
- 耗时。

Resolver 记录：

- Resolver 类型。
- RouteId。
- 数据 key。
- 结果。
- Data request correlation id。
- 耗时。

## 6. Plugin 诊断

插件相关诊断必须包含：

- PluginId。
- Plugin version。
- ContributionId。
- Route manifest version。
- Load context id。
- 活动 RouteScope 数量。
- Journal 清理数量。
- Cache 驱逐数量。
- 未释放引用线索。

## 7. 错误模型

Routing 错误应分层：

| 类型 | 示例 |
|---|---|
| Definition error | RouteId 重复、路径冲突。 |
| Matching error | 无匹配、参数不合法。 |
| Policy error | Guard 拒绝、权限不足。 |
| Resolve error | 数据不存在、请求失败。 |
| Activation error | ViewModel 创建或激活失败。 |
| Commit error | Presentation 提交失败。 |
| Plugin error | Contribution 已撤销、插件停用。 |

错误必须可以映射到用户提示，也可以保留开发诊断详情。

## 8. EventBus 边界

Routing 可以发布只读事实事件：

- NavigationStarted。
- NavigationCompleted。
- NavigationCancelled。
- NavigationFailed。
- RouteGraphChanged。

这些事件只用于观察。不能通过 EventBus 控制导航阶段，也不能替代 Navigation Middleware。

## 9. 测试工具

Testing 包应提供：

- `TestRouteGraphBuilder`。
- `TestRouter`。
- `TestNavigationScope`。
- `FakeUiDispatcher`。
- `FakePresentationCommitter`。
- `NavigationRecorder`。
- `RouteGraphAssertions`。
- `JournalAssertions`。
- `PluginRouteTestHost`。

测试工具不依赖真实 AtomUI/Avalonia UI。

## 10. 核心测试场景

必须支持测试：

- RouteReference 格式化和解析。
- RouteGraph 构建。
- Path 匹配。
- Guard 拒绝。
- Resolver 成功和失败。
- Commit 成功和失败。
- 回滚。
- Journal。
- Reuse。
- 插件贡献和撤销。
- 并发导航。
- NavigationScope 停止。

## 11. Deterministic Dispatcher

测试中需要 deterministic dispatcher。

能力：

- 手动推进 UI queue。
- 手动推进后台 queue。
- 控制 Commit 时机。
- 模拟 Commit 中新导航。
- 捕获跨线程违规。

这可以避免路由测试依赖真实线程调度。

## 12. 断言要求

测试断言应能表达：

- 当前 RouteId。
- 当前参数。
- 当前活动 RouteScope。
- 当前 Journal stack。
- 当前 RouteGraph version。
- 已释放 RouteScope。
- Guard/Resolver 执行顺序。
- 插件 Contribution 是否清理。
- 是否没有未释放 Operation。

## 13. 性能指标

Routing 诊断应能统计：

- RouteGraph 构建耗时。
- Path 匹配耗时。
- Guard 总耗时。
- Resolver 总耗时。
- Commit 耗时。
- Journal 写入耗时。
- RouteScope 释放耗时。

性能指标不能强依赖外部 telemetry 系统。Core Diagnostics 先提供统一出口。

## 14. 文档完成标准

Routing 实现前，对应测试设计必须覆盖：

- 成功路径。
- 拒绝路径。
- 取消路径。
- 失败路径。
- 插件撤销路径。
- 线程调度路径。
- 回滚路径。

这些测试不是后补项，而是 Routing 编程模型的一部分。
