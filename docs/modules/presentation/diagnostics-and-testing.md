# AtomUI.City.Presentation 诊断与测试设计

版本：v0.1
状态：正式初版
适用范围：Presentation 诊断字段、fake runtime、平台集成测试和测试矩阵

## 1. 诊断

必须记录：

- UI runtime ready / stopping。
- Dispatcher 投递失败。
- ViewLocator 命中和失败。
- View 创建耗时。
- Binding 耗时。
- Outlet commit 计划和结果。
- Activation visual adapter 执行。
- Interaction handler 执行。
- Resource contribution 和撤销。
- 插件 UI 关闭和资源清理。

诊断信息必须包含 ScopeId、WindowId、NavigationScopeId、RouteId、ViewModel type、View type、PluginId 和 ContributionId。

## 2. 测试工具

Testing 包应提供：

- FakePresentationRuntime。
- FakeUiDispatcher。
- TestViewLocator。
- TestViewFactory。
- TestRouteOutlet。
- TestPresentationCommitter。
- Interaction test handler。
- View binding recorder。
- Plugin presentation resource test host。

Presentation 测试应能在无真实 AtomUI/Avalonia UI 的环境中运行。真实 UI 集成测试单独放到平台集成测试中。

## 3. 模块测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| ViewLocator 成功 | Unit | ViewModel 定位到 View。 |
| ViewLocator 失败 | Unit | commit failed 并记录诊断。 |
| 多默认 View | Analyzer/Generator | 输出重复 View 诊断。 |
| View 创建失败 | Unit | commit failed，旧内容保留。 |
| Outlet commit 成功 | Unit | View attached 到 outlet。 |
| Outlet commit 失败回滚 | Unit | 新 View 释放，旧 content 保留。 |
| Interaction 状态 | Unit | Completed、Canceled、NotHandled、Failed 均可断言。 |
| ActivationScope 释放 | Unit | binding 和 UI 事件订阅被释放。 |
| 插件停用 | Unit | View 关闭，资源撤销。 |
| dispatcher stopped | Unit | 停止后拒绝投递。 |
| real dispatcher | Platform integration | 真实 UI dispatcher 可执行最小投递。 |
