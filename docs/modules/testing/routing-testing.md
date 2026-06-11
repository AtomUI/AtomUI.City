# Routing 测试设计

版本：v0.1
状态：正式初版
适用范围：路由语法、RouteGraph、导航事务、Guard、Resolver、ViewModel Target、Journal、插件路由和诊断

## 1. 目标

Routing 测试必须证明页面进入模型可预测、可回滚、可诊断，不依赖真实 UI runtime。

## 2. RoutingTestHost

`RoutingTestHost` 提供：

- route definition builder。
- route graph builder。
- navigation driver。
- fake guard。
- fake resolver。
- fake ViewModel target registry。
- fake presentation committer。
- journal assertions。
- plugin route contribution helper。

## 3. 单元测试范围

必须覆盖：

- route pattern 解析。
- path formatting。
- path matching。
- 参数绑定。
- route constraints。
- route id。
- route graph 父子关系。
- outlet metadata。
- ViewModel target 解析。
- guard allow/deny/redirect。
- resolver success/failure/cancel。
- journal push/replace/back。

## 4. 导航事务测试

必须覆盖：

- 导航成功。
- guard 拒绝。
- resolver 失败。
- presentation commit 失败。
- ViewModel activation 失败。
- 回滚。
- cancellation。
- diagnostics。

## 5. 插件路由测试

必须覆盖：

- 插件路由注册。
- 插件路由匹配。
- 插件路由撤销后不可匹配。
- 插件路由 active scope 关闭。
- 插件停用阻止新导航。

## 6. 集成测试范围

Framework integration test 覆盖：

```text
Routing
-> Security guard
-> Resolver
-> ViewModel target
-> Fake Presentation outlet
-> Lifecycle scope
```

真实 UI commit 只放平台集成测试。
