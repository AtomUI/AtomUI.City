# 集成测试策略

版本：v0.1
状态：正式初版
适用范围：框架内集成测试、平台集成测试、模板 smoke 测试和 CI 分类

## 1. 目标

集成测试用于验证模块协作和真实运行时风险。它不替代单元测试。

核心规则：

- 单元测试覆盖每个功能点。
- 集成测试覆盖跨模块链路。
- 平台集成测试覆盖 fake runtime 无法证明的真实 UI 行为。

## 2. 框架内集成测试

框架内集成测试使用 `FrameworkIntegrationTestHost`，默认不启动真实 AtomUI/Avalonia UI。

覆盖链路：

| 场景 | 覆盖链路 |
|---|---|
| Host 启动 | Configuration -> DI -> ModuleGraph -> Lifecycle |
| 路由进入页面 | Routing -> Security Guard -> Resolver -> ViewModel Target -> Fake Presentation Outlet |
| 页面激活 | Routing -> MVVM Activation -> Fake Presentation Commit -> Lifecycle Scope |
| 状态变更 | State -> Subscription -> Dispatcher -> ViewModel reaction |
| 事件通知 | EventBus -> Handler -> Lifecycle cancellation -> Diagnostics |
| 数据请求 | Data -> Security token -> Request pipeline -> Fake HTTP/gRPC/SignalR |
| 多语言切换 | Localization -> Culture state -> Fake Presentation resource refresh |
| 插件启用 | PluginSystem -> Module -> Contribution -> Routing/Presentation/EventBus/Data |
| 插件卸载 | Stop entry -> Cancel operations -> Revoke leases -> Unload assertions |

这些测试应稳定、快速、可并行，CI 每次执行。

## 3. 平台集成测试

平台集成测试使用真实 AtomUI/Avalonia runtime。

只覆盖 fake runtime 无法证明的行为：

- UI Dispatcher 回到真实 UI thread。
- ViewLocator 找到真实 View。
- ViewModel 到 View binding。
- Route Outlet 提交到 visual tree。
- ResourceDictionary 和主题刷新。
- Window lifecycle。
- visual attach/detach feedback。
- 插件 View/Resource 卸载后无残留 UI 引用。

平台集成测试应独立分类：

```text
Category=PlatformIntegration
Category=RequiresUiRuntime
```

普通 PR 可以只跑框架内集成测试，夜间或 release pipeline 跑平台集成测试。

## 4. 模板 Smoke 测试

模板 smoke 测试证明工程化入口可用。

覆盖：

- 创建应用模板。
- 构建应用。
- 生成模块。
- 生成页面。
- 生成插件。
- 插件打包。
- 插件安装到测试 Host。
- 执行一次导航。
- 执行一次插件卸载。

Smoke 测试不追求覆盖所有边界，边界由单元测试和集成测试覆盖。

## 5. 项目建议

建议测试项目：

```text
tests/
  AtomUI.City.FrameworkIntegrationTests/
  AtomUI.City.PlatformIntegrationTests/
  AtomUI.City.TemplateSmokeTests/
```

现有模块测试项目继续承载单元测试和模块内 contract test。

## 6. 失败回补规则

集成测试发现 bug 后：

- 如果 bug 可在单模块复现，必须补单元测试。
- 如果 bug 是 contract 不一致，必须补 contract test。
- 如果 bug 只存在真实 UI runtime，补平台集成测试。
- 如果 bug 涉及构建或模板，补 build test 或 smoke test。

集成测试不能成为缺少单元测试的理由。
