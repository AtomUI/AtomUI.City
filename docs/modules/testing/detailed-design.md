# AtomUI.City.Testing 详细设计

版本：v0.1
状态：正式初版
适用范围：测试治理、TestHost、确定性调度、生命周期测试、模块协作测试、插件测试、集成测试和测试断言工具

## 1. 定位

`AtomUI.City.Testing` 是 AtomUI.City 的框架级测试基础设施。它负责让框架编程模型、生命周期、模块协作、插件运行时、线程调度、状态通知和诊断行为可被稳定验证。

Testing 的目标不是替代测试框架，也不是封装应用业务测试。应用和框架测试可以继续使用 xUnit、NUnit、MSTest 等 runner。Testing 提供的是测试 Host、fake runtime、deterministic scheduler、driver、assertion 和测试矩阵规范。

## 2. 硬约束

AtomUI.City 采用功能点测试门禁。

规则：

- 每一个公开功能点必须有对应单元测试。
- 每一个模块文档中的功能点必须能追踪到测试用例或测试场景。
- 没有测试设计的功能点不能进入实现。
- 没有单元测试的功能点不能标记完成。
- 集成测试不能替代单元测试。
- 如果某个功能点无法通过单元测试验证，必须说明原因，并提供 contract test、integration test、analyzer test 或 build test 替代。
- 例外必须写入对应模块测试矩阵。

功能点包括公共 API、生命周期阶段、middleware、Options 行为、source generator 输出、Contribution 和 Lease、插件安装加载卸载、路由导航、状态变更、事件派发、数据请求、本地化、安全授权、CLI 命令和 Build target。

详细规则见：[功能点测试门禁](feature-test-gate.md)。

## 3. 设计原则

| 原则 | 说明 |
|---|---|
| Runner-neutral | 核心测试工具不绑定具体测试 runner。 |
| Deterministic-first | Dispatcher、Timer、延迟、EventBus dispatch、Data callback 必须可控推进。 |
| Lifecycle-first | 测试对象绑定明确 Scope、Operation 或 ContributionLease。 |
| No real UI by default | 默认使用 fake Presentation runtime，不启动真实 AtomUI/Avalonia UI。 |
| Plugin unload testable | 插件停用、Lease 撤销、Operation 取消、加载上下文卸载必须可断言。 |
| AOT-aware | 测试工具不能依赖运行时反射扫描作为核心机制。 |
| Documentation-traceable | 模块设计文档中的功能点必须映射到测试矩阵。 |

## 4. 边界

Testing 负责：

- TestHost。
- 测试配置和测试服务注入。
- Fake UI Dispatcher。
- Deterministic scheduler。
- 生命周期驱动器。
- 模块图测试工具。
- Contribution 和 Lease 断言。
- Routing 测试工具。
- Presentation fake runtime。
- MVVM 测试工具。
- State 测试工具。
- EventBus 测试记录器。
- Data fake transport。
- Security fake provider。
- Localization fake provider。
- PluginSystem 测试工具。
- 诊断和断言工具。
- 集成测试分层规范。
- AOT/source generator 测试规范。

Testing 不负责：

- 具体业务测试。
- 真实 UI 自动化框架封装。
- 性能测试平台。
- 线上遥测平台。
- 生产代码运行时能力。

生产代码不得依赖 `AtomUI.City.Testing`。测试项目可以依赖它。

## 5. 核心组件

| 组件 | 职责 |
|---|---|
| `TestHost` | 构造最小 Host，注入测试配置、模块、服务和 fake runtime。 |
| `TestHostBuilder` | 用 builder 风格组合测试 Host。 |
| `FakeUiDispatcher` | 模拟 UI dispatcher，支持队列推进和线程目标断言。 |
| `DeterministicScheduler` | 控制后台任务、Timer、延迟和异步回调。 |
| `LifecycleDriver` | 驱动 start、stop、dispose、Scope 创建释放和 middleware 执行。 |
| `ModuleTestHost` | 构建模块图，断言依赖顺序、配置阶段和服务注册阶段。 |
| `ContributionTestRegistry` | 记录 Contribution 和 Lease 创建、撤销、冲突和顺序。 |
| `RoutingTestHost` | 测试 route graph、navigation transaction、guard、resolver 和 journal。 |
| `PresentationTestRuntime` | Fake ViewLocator、ViewFactory、Outlet commit 和 visual lifecycle feedback。 |
| `MvvmTestKit` | ViewModel activation、command、interaction、validation 测试工具。 |
| `StateTestStore` | 状态读写、订阅、computed invalidation、snapshot 断言。 |
| `EventBusRecorder` | 记录 publish、subscribe、dispatch、error 和 backpressure。 |
| `DataTransportFakes` | Fake HTTP、gRPC、SignalR transport、stream 和 connection lifecycle。 |
| `SecurityTestKit` | Fake principal、permission checker、policy evaluator。 |
| `LocalizationTestKit` | Fake culture provider、language package provider、resource lookup。 |
| `PluginTestHost` | 构造 fake plugin package/source，驱动 install、load、activate、unload。 |
| `DiagnosticsAssertions` | 断言诊断事件、错误码、阶段和上下文。 |

## 6. TestHost 模型

```text
TestHost
-> Test ServiceProvider
-> ApplicationScope
-> Fake dispatcher/scheduler
-> Lifecycle pipeline
-> Module graph
-> Contribution registries
-> Optional module runtimes
```

默认启用：

- Core DI。
- Configuration。
- Lifecycle。
- Diagnostics。
- Fake dispatcher。
- Deterministic scheduler。

其他模块按测试需要显式加入：

```text
UseRoutingTesting
UseFakePresentation
UseStateTesting
UseEventBusTesting
UseDataTesting
UseSecurityTesting
UseLocalizationTesting
UsePluginTesting
```

## 7. 线程和调度

测试必须避免依赖真实时钟和真实线程时序。

推荐流程：

```text
Arrange
-> enqueue dispatcher work
-> enqueue background work
-> advance scheduler
-> drain dispatcher
-> assert effects
```

规则：

- 测试不使用 `Task.Delay` 猜测异步完成。
- UI 更新只断言投递目标和 drain 后效果。
- 后台任务必须绑定 Scope 或 Operation。
- 取消、超时、debounce、retry 必须通过 deterministic scheduler 推进。

详细规则见：[Fake Dispatcher 和确定性调度](fake-dispatcher-and-scheduler.md)。

## 8. 测试分层

| 层级 | 说明 |
|---|---|
| Unit | 单个功能点、服务、策略、registry、parser。 |
| Contract | 模块公共 contract、事件、Contribution、路由定义、Data client contract。 |
| Framework integration | 多模块在 TestHost 下协作，使用 fake runtime。 |
| Runtime lifecycle | Scope、Operation、Lease、middleware、取消和释放顺序。 |
| Plugin lifecycle | 安装、启用、停用、卸载、更新、回滚。 |
| Platform integration | 真实 AtomUI/Avalonia runtime，少量关键链路。 |
| Template smoke | 模板生成、构建、打包和基础运行路径。 |

集成测试策略见：[集成测试策略](integration-testing.md)。

## 9. 每个模块的测试矩阵

每个模块详细设计必须包含测试矩阵。

建议格式：

| 功能点 | 测试类型 | 测试工具 | 必测场景 |
|---|---|---|---|
| Route matching | Unit | RoutingTestHost | path match、参数绑定、约束失败。 |
| Plugin unload | Unit/Integration | PluginTestHost + UnloadAssertions | lease 撤销、operation 取消、UnloadPending。 |

测试矩阵用于实现前检查和完成前验收。

## 10. 集成测试设计

集成测试分两类：

- Framework integration：默认 CI 必跑，不启动真实 UI。
- Platform integration：真实 AtomUI/Avalonia runtime，数量少，独立分类。

框架内集成测试覆盖：

- Host 启动。
- 路由进入页面。
- 页面激活。
- 状态变更。
- 事件通知。
- 数据请求。
- 多语言切换。
- 插件启用。
- 插件卸载。

平台集成测试覆盖 fake runtime 无法证明的 UI 行为，例如 UI Dispatcher、真实 ViewLocator、binding、Outlet commit、ResourceDictionary 和 visual lifecycle。

## 11. 插件测试

PluginSystem 测试必须支持：

- fake plugin package builder。
- fake plugin source。
- fake lock file。
- fake package cache。
- staging/installed/pending 目录模拟。
- install/load/activate/deactivate/unload driver。
- Contribution Lease 反向撤销断言。
- Operation cancellation 断言。
- `UnloadPending` 原因断言。
- AssemblyLoadContext unload helper。

插件测试不能使用真实用户插件目录，也不能要求真实 NuGet feed。

详细规则见：[插件测试](plugin-testing.md)。

## 12. AOT 和 Source Generator 测试

Testing 必须支持：

- source generator snapshot 或结构化输出断言。
- analyzer diagnostics 断言。
- generated manifest 断言。
- trimming/AOT compatibility diagnostics 断言。
- build target 测试。

运行时反射扫描不能作为测试工具发现功能点的默认方式。

## 13. 完成标准

一个功能点完成必须同时满足：

- 文档中有设计说明。
- 文档中有测试矩阵条目。
- 实现存在。
- 单元测试存在。
- 成功路径、失败路径、取消或释放路径被覆盖。
- 涉及插件、线程、生命周期的功能有泄漏或释放断言。
- 对应诊断事件和错误策略有断言。

没有满足以上条件时，该功能点不能标记完成。
