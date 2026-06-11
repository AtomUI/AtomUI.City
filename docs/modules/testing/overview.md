# AtomUI.City.Testing

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Testing` 负责测试 Host、测试 Dispatcher、生命周期驱动、状态断言、路由测试、EventBus 记录和 Presentation-free ViewModel 测试支持。

Testing 的目标是让应用可以测试框架编程模型本身，而不是只测试孤立工具类。

Testing 是测试基础设施，不是业务测试框架。它提供 TestHost、fake runtime、确定性调度、driver、assertion 和测试矩阵规范。

## 边界

Testing 负责：

- Test Host。
- Test Module。
- Fake Dispatcher。
- Deterministic Scheduler。
- 生命周期驱动器。
- Contribution 和 Lease 断言。
- 路由测试工具。
- Presentation fake runtime。
- MVVM 测试工具。
- State Snapshot 断言。
- EventBus 测试记录器。
- Data fake transport。
- Security fake provider。
- Localization fake provider。
- PluginSystem 测试工具。
- Command 执行测试工具。
- Presentation-free ViewModel 测试支持。
- 集成测试分层规范。
- AOT/source generator 测试规范。

Testing 不负责：

- 具体应用业务测试。
- UI 自动化测试框架封装。
- 性能测试平台。
- 生产代码运行时能力。

## 硬约束

AtomUI.City 要求每一个公开功能点都有对应单元测试。

规则：

- 没有测试设计的功能点不能进入实现。
- 没有单元测试的功能点不能标记完成。
- 集成测试不能替代单元测试。
- 无法单元测试的功能点必须在测试矩阵中说明原因，并提供 contract test、integration test、analyzer test 或 build test 替代。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Testing 总体架构、硬约束、TestHost、确定性调度、集成测试、插件测试和完成标准。 |
| [feature-test-gate.md](feature-test-gate.md) | 功能点测试门禁、功能点定义、单元测试要求、替代测试例外和测试矩阵。 |
| [test-host.md](test-host.md) | 测试 Host、测试服务容器、模块组合、fake runtime 注入和 Host 生命周期驱动。 |
| [fake-dispatcher-and-scheduler.md](fake-dispatcher-and-scheduler.md) | UI dispatcher fake、后台调度、Timer、延迟、异步回调和确定性测试。 |
| [lifecycle-testing.md](lifecycle-testing.md) | Lifecycle Scope、pipeline、middleware、Operation、Lease、取消、释放和错误聚合。 |
| [module-testing.md](module-testing.md) | 模块定义、模块依赖、模块图、配置阶段、服务注册阶段、模块生命周期和插件模块。 |
| [contribution-testing.md](contribution-testing.md) | ContributionRequest、ContributionLease、Registry、冲突、撤销、插件来源和诊断。 |
| [routing-testing.md](routing-testing.md) | 路由语法、RouteGraph、导航事务、Guard、Resolver、ViewModel Target、Journal 和插件路由。 |
| [presentation-testing.md](presentation-testing.md) | Presentation fake runtime、ViewLocator、ViewFactory、Outlet commit、UI Dispatcher、visual lifecycle 和平台集成。 |
| [mvvm-testing.md](mvvm-testing.md) | ViewModel activation、Command、Interaction、Validation、property change 和无 ViewModel UI 测试。 |
| [state-testing.md](state-testing.md) | 应用状态、作用域状态、订阅、computed、snapshot、dispatcher、插件状态和线程安全。 |
| [eventbus-testing.md](eventbus-testing.md) | 事件发布、订阅、dispatch、背压、错误、生命周期、插件和事件链诊断。 |
| [data-testing.md](data-testing.md) | HTTP、gRPC、SignalR、request pipeline、streaming、realtime、缓存、认证、并发、取消和插件连接。 |
| [security-testing.md](security-testing.md) | 认证状态、ClaimsPrincipal、权限、Policy、Route Guard、Command、Data、Plugin capability 和诊断。 |
| [localization-testing.md](localization-testing.md) | culture state、资源查找、fallback、懒加载、语言包 assembly、locpack、UI refresh 和插件本地化。 |
| [plugin-testing.md](plugin-testing.md) | 插件包、发现、安装、加载、启用、停用、卸载、更新、回滚、UnloadPending 和安全策略。 |
| [integration-testing.md](integration-testing.md) | 框架内集成测试、平台集成测试、模板 smoke 测试和 CI 分类。 |
| [diagnostics-and-assertions.md](diagnostics-and-assertions.md) | 诊断收集、错误码断言、生命周期断言、线程断言、Contribution 断言和泄漏断言。 |
| [aot-and-source-generation-testing.md](aot-and-source-generation-testing.md) | source generator、analyzer、manifest 生成、AOT/trimming diagnostics、Build target 和静态插件测试。 |
