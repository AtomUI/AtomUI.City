# AtomUI.City.Routing

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Routing` 负责应用结构、页面进入路径、导航状态、路由生命周期、权限守卫、数据解析和 View/ViewModel 映射。

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

Routing 不负责：

- 具体 UI 控件渲染。
- HTTP client 实现。
- 权限策略持久化。

## 后续拆分

- `route-graph.md`
- `navigation.md`
- `guards.md`
- `resolvers.md`
- `view-mapping.md`
