# AtomUI.City.Routing

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Routing` 负责应用结构、页面进入路径、导航状态、路由生命周期、权限守卫、数据解析和 Route 到 ViewModel Target 的映射。

Routing 是页面进入、数据预取、权限校验和 ViewModel 激活的重要入口。

## 边界

Routing 负责：

- 路由定义。
- 路由参数。
- 嵌套路由。
- 布局。
- 守卫。
- 解析器。
- 导航结果。
- 当前路由状态。
- Route 到 ViewModel Target 的映射。

Routing 不负责：

- 具体 UI 控件渲染。
- ViewLocator 实现。
- View 创建。
- HTTP client 实现。
- 权限策略持久化。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Routing 总体架构、RouteGraph、NavigationScope、事务式导航、插件路由、线程和测试策略。 |
| [route-definition-syntax.md](route-definition-syntax.md) | 路由声明语法、路径模板、参数绑定、RouteReference、Source Generator 契约和诊断规则。 |
| [route-graph.md](route-graph.md) | RouteDescriptor、RouteRegistry、RouteGraphSnapshot、贡献归属和冲突检测。 |
| [navigation.md](navigation.md) | IRouter、NavigationScope、NavigationTarget、NavigationTransaction、提交和回滚。 |
| [guards.md](guards.md) | Match Policy、Enter Guard、Leave Guard、离开确认、重定向和权限集成。 |
| [resolvers.md](resolvers.md) | 路由数据解析、Resolver 生命周期、Data/State 集成、取消和错误策略。 |
| [viewmodel-target.md](viewmodel-target.md) | Route 到 ViewModel Target 的映射、参数注入、解析数据注入和 Presentation 边界。 |
| [journal-and-reuse.md](journal-and-reuse.md) | NavigationJournal、Back/Forward、RouteReusePolicy、KeepAlive 和插件清理。 |
| [plugins.md](plugins.md) | 插件路由贡献、扩展点、动态停用、活动路由关闭和卸载安全。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | Routing 诊断、错误模型、测试工具和无 UI 测试策略。 |
