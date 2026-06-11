# 页面模板设计

版本：v0.1
状态：正式初版
适用范围：页面路由、ViewModel Target、ViewModel、View、Outlet、Activation 和页面测试

## 1. 目标

页面模板用于创建一条完整页面进入链路，但不把业务概念写进模板。

页面链路：

```text
Route
-> ViewModel Target
-> ViewModel Activation
-> View
-> Outlet
-> VisualTree
```

## 2. 默认结构

```text
src/<ProjectName>/Routes/<PageName>/
  <PageName>Route.cs
  <PageName>ViewModel.cs
  <PageName>View.axaml
  <PageName>View.axaml.cs
tests/<ProjectName>.Tests/Routes/<PageName>/
  <PageName>RouteTests.cs
  <PageName>ViewModelTests.cs
```

## 3. 路由职责

模板生成的 route 声明只表达：

- RouteId。
- RoutePath。
- ViewModel Target。
- 参数。
- Outlet。
- 权限元数据，如果用户选择生成。
- 本地化标题 key，如果用户选择生成。

Routing 不生成 View 映射。View 映射由 Presentation 模板声明。

## 4. ViewModel

ViewModel 默认包含：

- Activation 入口。
- CancellationToken 使用示例。
- Command 示例，只有用户选择时生成。
- Validation 示例，只有用户选择时生成。
- Interaction 示例，只有用户选择时生成。

默认不生成业务字段。

## 5. View

View 默认包含最小可渲染结构。

规则：

- 不生成业务布局。
- 不生成过度装饰。
- View 和 ViewModel 绑定必须可被 Presentation source generator 识别。
- View code-behind 不写业务逻辑。

## 6. 测试

页面模板默认生成：

- route match 测试。
- ViewModel target 测试。
- activation 测试。
- cancellation 测试。
- Presentation fake commit 测试，如果启用 View。

真实 visual tree 测试放平台集成测试。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| RoutePath | Unit | match、参数、约束失败。 |
| ViewModel Target | Unit | target 解析。 |
| Activation | Unit | activate、cancel、dispose。 |
| View mapping | Generator/Unit | ViewModel -> View 映射生成。 |
| Outlet commit | Framework integration | fake outlet commit。 |
