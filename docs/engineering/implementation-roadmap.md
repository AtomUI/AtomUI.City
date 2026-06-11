# AtomUI.City 实现路线图

版本：v0.1
状态：正式初版
适用范围：从设计文档进入代码实现的阶段顺序、验收标准、测试门禁和暂停条件

## 1. 目标

本路线图用于约束 AtomUI.City 的实现顺序。

它不是排期文档，而是工程推进规则：先实现内核、测试底座和生命周期模型，再逐步实现开发者编程模型、页面进入链路、插件系统和工程化能力。

核心原则：

- 文档确认后才能实现。
- 每个功能点必须有单元测试。
- 集成测试不能替代单元测试。
- 每个阶段只交付一个可验证闭环。
- 如果实现偏离文档，先回到文档确认。
- 不为了表面完整提前实现复杂能力。

## 2. 全局完成规则

任一功能点完成必须满足：

```text
Documented
-> Has test matrix row
-> Implemented
-> Unit tested
-> Diagnostics asserted if applicable
-> Lifecycle cleanup asserted if applicable
```

任一阶段完成必须满足：

- 阶段范围内的公开功能点都有测试矩阵条目。
- 阶段范围内的公开功能点都有单元测试。
- 涉及生命周期、线程、插件、订阅、连接或资源释放的功能点必须有释放断言。
- 阶段内实现不得引入未确认的公共 API。
- 阶段内实现不得改变已确认的包依赖方向。
- 阶段内实现和文档一致。

## 3. Phase 0：仓库和工程基线

目标：保证项目结构、包、测试项目、构建配置能支撑后面的实现工作。

范围：

- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`
- `AtomUICity.slnx`
- 基础项目引用关系
- 测试项目可运行
- LGPL v3 license 确认
- `output/` 目录规则预留

必须完成：

- 所有项目能 restore 和 build。
- 所有测试项目能运行空测试或 assembly smoke test。
- 包边界符合文档。
- 运行时包不引入不应出现的依赖，例如 Roslyn 进入 Core。

暂不做：

- 运行时功能实现。
- Source Generator 实现。
- CLI 命令实现。
- 插件加载实现。

必测项：

- solution build。
- test projects discovery。
- package reference 边界。
- assembly smoke test。

DoD：

- `dotnet build` 通过。
- `dotnet test` 通过。
- 每个项目至少有 assembly smoke test。
- 文档和项目结构一致。

## 4. Phase 1：Core Kernel

目标：实现 AtomUI.City 最小内核。

范围：

- `AtomUI.City.Core`
- Hosting contract
- Application context
- Lifecycle scope
- Lifecycle pipeline
- Operation scope
- Contribution lease abstraction
- Diagnostics abstraction
- Threading abstraction
- Configuration 基础 contract
- Module abstraction 最小接口

最小闭环：

```text
Host
-> ApplicationScope
-> Module initialization
-> Lifecycle start
-> Operation run
-> Contribution lease create/revoke
-> Lifecycle stop/dispose
-> Diagnostics collect
```

暂不做：

- UI runtime。
- 路由。
- 插件动态加载。
- Source Generator。
- 完整 Options binding。

必测项：

- Scope 创建和释放顺序。
- Stop 幂等。
- Operation cancellation。
- Lease revoke。
- Middleware 顺序。
- 错误聚合。
- Diagnostics 记录。

DoD：

- Core 单元测试覆盖全部公开功能点。
- Core 不依赖 UI、Roslyn、CLI、Testing 生产代码。
- 生命周期文档和实现一致。

## 5. Phase 2：Testing 基础设施

目标：让后面的所有模块实现都有可用测试工具。

范围：

- `AtomUI.City.Testing`
- TestHost
- FakeUiDispatcher
- DeterministicScheduler
- LifecycleDriver
- ContributionAssertions
- DiagnosticsAssertions
- ThreadingAssertions

最小闭环：

```text
TestHost
-> Core lifecycle
-> Fake dispatcher
-> Deterministic scheduler
-> Contribution registry
-> Assertions
```

暂不做：

- 模块专属测试工具完整实现。
- 真实 UI 平台集成测试工具。
- 插件包 fake builder 完整实现。

必测项：

- TestHost start/stop。
- Fake dispatcher drain。
- Scheduler virtual time。
- Lifecycle driver。
- Lease assertion。
- Diagnostics assertion。
- 未完成 Operation 检测。

DoD：

- 后面的模块可以不启动真实 UI 写测试。
- 所有测试不依赖 `Task.Delay` 猜时序。
- Testing 自身有单元测试。
- Testing 不进入生产运行时依赖链。

## 6. Phase 3：Modularity、Configuration 和 DI

目标：把模块系统和服务注册模型定稳。

范围：

- Module base / contract。
- Module dependency attribute。
- Module graph。
- Module lifecycle context。
- PreConfigure / Configure / PostConfigure。
- Service registration phase。
- Plugin module 作为普通 Module 的来源模型。
- AOT 友好的服务注册约束。

最小闭环：

```text
Module descriptors
-> dependency graph
-> configure services
-> lifecycle hooks
-> contribution requests
```

暂不做：

- Source Generator 自动生成模块图。
- 插件程序集加载。
- 复杂自动服务扫描。

必测项：

- 默认 ModuleId 使用类型全名。
- 显式 ModuleId。
- 依赖排序。
- 循环依赖失败。
- 缺失依赖失败。
- PreConfigure 顺序。
- Configure 不允许 BuildServiceProvider。
- Plugin module 不写 Host Root ServiceProvider。

DoD：

- 模块图可测试。
- 配置阶段可测试。
- 服务注册边界可测试。
- 后面的模块都能通过 Module 接入。

## 7. Phase 4：State 和 EventBus

目标：实现开发者日常编程模型的基础能力。

范围：

- `AtomUI.City.State`
- application state
- scoped state
- writable state
- computed state
- subscription / reaction
- snapshot 基础 contract
- `AtomUI.City.EventBus`
- typed event publish / subscribe
- lifecycle-bound subscription
- deterministic dispatch
- error policy
- diagnostics

最小闭环：

```text
State change
-> subscription
-> dispatcher target
-> lifecycle cleanup

Event publish
-> handler dispatch
-> cancellation
-> diagnostics
```

暂不做：

- 复杂持久化。
- 分布式事件。
- 高级背压策略完整实现。
- 插件跨程序集 contract 复杂验证。

必测项：

- state set/get。
- 相等值不通知。
- computed invalidation。
- subscription dispose。
- Scope stop 后不通知。
- event publish / subscribe。
- handler exception。
- dispatch target。
- lifecycle-bound unsubscribe。
- plugin-like contribution cleanup。

DoD：

- State 和 EventBus 可被 MVVM、Routing、Data 使用。
- 多线程调度规则通过 fake scheduler 可测。
- 诊断可断言。

## 8. Phase 5：MVVM

目标：实现 ViewModel 编程模型底座。

范围：

- ViewModel base。
- Activation。
- Command。
- Async command。
- Interaction。
- Validation contract。
- Property change integration。
- OperationScope 绑定。
- Dispatcher 绑定。

最小闭环：

```text
ViewModel create
-> activate
-> command execute
-> operation/cancellation
-> deactivate/dispose
```

暂不做：

- 复杂 UI binding。
- 真实 Interaction UI。
- 高级 validation UI 展示。

必测项：

- ViewModel activation。
- deactivate。
- activation disposal。
- command can execute。
- async command cancellation。
- interaction missing handler。
- validation success/failure。
- permission/state change 触发 command refresh 的 contract。

DoD：

- ViewModel 能在无真实 UI 下完整测试。
- MVVM 不直接依赖 AtomUI/Avalonia。
- Presentation 能接入 Activation。

## 9. Phase 6：Routing 和 Presentation Fake Bridge

目标：跑通页面进入链路，但仍然不依赖真实 UI。

范围：

- `AtomUI.City.Routing`
- route definition syntax
- route graph
- navigation transaction
- guard
- resolver
- ViewModel target
- journal
- `AtomUI.City.Presentation`
- fake ViewLocator contract
- fake ViewFactory contract
- outlet commit contract
- UI dispatcher bridge contract
- visual lifecycle feedback contract

最小闭环：

```text
Navigate
-> match route
-> run guard
-> run resolver
-> resolve ViewModel target
-> create/activate ViewModel
-> fake Presentation outlet commit
-> RouteScope active
```

暂不做：

- 真实 AtomUI/Avalonia View。
- 复杂 journal restore。
- 多窗口高级场景。
- 插件动态路由完整卸载。

必测项：

- route match。
- parameter binding。
- guard deny/redirect。
- resolver success/failure/cancel。
- navigation rollback。
- ViewModel target。
- fake outlet commit。
- activation after commit。
- visual detach -> deactivate。

DoD：

- `Route -> ViewModel Target -> ViewModel -> Fake View -> Outlet` 链路可跑通。
- 不启动真实 UI。
- 所有失败路径有回滚测试。

## 10. Phase 7：Security、Data 和 Localization 基础

目标：补齐通用应用基础设施的最小可用闭环。

Security 范围：

- ClaimsPrincipal state。
- permission checker。
- policy evaluator。
- route guard integration。
- command integration。
- data auth integration contract。

Data 范围：

- request pipeline。
- fake/http transport。
- gRPC contract。
- SignalR connection contract。
- cancellation。
- error model。
- cache contract。
- security token injection。

Localization 范围：

- culture state。
- resource lookup。
- fallback。
- lazy package provider contract。
- MVVM validation text integration。
- Presentation fake refresh integration。

最小闭环：

```text
Route guard checks permission
Data request injects token
Culture switch refreshes resource-backed text
```

暂不做：

- 完整真实认证 provider。
- 所有真实 gRPC/SignalR 高级功能。
- 动态 language assembly unload 完整实现。
- AtomUI ResourceDictionary 平台实现。

必测项：

- permission allow/deny。
- command permission refresh。
- data token injection。
- request cancellation。
- HTTP fake transport。
- streaming/realtime contract。
- culture switch。
- fallback。
- lazy load。
- plugin-like resource revoke。

DoD：

- Routing、MVVM、Data、Localization 的基础集成可在 TestHost 跑通。
- 不依赖真实远程服务。
- 不依赖真实 UI。

## 11. Phase 8：Build 和 Source Generator 最小闭环

目标：把手写注册逐步收敛到构建期生成。

范围：

- `AtomUI.City.Build`
- `AtomUI.City.Generators`
- buildTransitive props/targets。
- module manifest generator。
- route manifest generator。
- presentation mapping manifest generator。
- permission/localization/data manifest 起步。
- analyzer 基础规则。
- output layout。
- manifest validation。

最小闭环：

```text
Source declarations
-> generator output
-> manifest
-> Build validation
-> output/artifacts/manifests
```

暂不做：

- 所有 analyzer 规则。
- 完整 CLI。
- 完整模板包发布。
- 高级 incremental optimization。

必测项：

- generator 输入输出。
- manifest deterministic。
- duplicate route diagnostic。
- strict AOT dynamic discovery diagnostic。
- output layout。
- build target execution。

DoD：

- 一个示例项目能生成 module、routes、presentation manifest。
- Build 测试通过。
- 运行时默认路径不依赖反射扫描。

## 12. Phase 9：PluginSystem 最小闭环

目标：实现插件从包元数据到启用、停用、卸载的最小闭环。

范围：

- plugin metadata。
- manifest read。
- discovery。
- package layout validation。
- installed / staging / pending 目录。
- lock file。
- dependency resolution 基础。
- Plugin ServiceScope。
- Plugin module graph。
- Contribution apply/revoke。
- deactivate/unload。
- UnloadPending diagnostics。

最小闭环：

```text
Install fake plugin package
-> discover
-> verify
-> load
-> activate contributions
-> deactivate
-> revoke leases
-> unload
```

暂不做：

- 插件市场。
- 不可信插件沙箱。
- 复杂签名体系完整实现。
- 多插件复杂依赖更新。
- Native AOT 动态插件。

必测项：

- plugin.json validation。
- one main assembly rule。
- package install staging。
- lock file active version。
- load/activate/deactivate/unload。
- lease revoke。
- operation cancellation。
- UnloadPending。
- update pending。
- rollback。

DoD：

- 插件生命周期能在 TestHost 中跑通。
- 插件不污染 Host Root ServiceProvider。
- 插件卸载失败有诊断。
- 文件不原地覆盖规则可测试。

## 13. Phase 10：Presentation Platform Bridge

目标：接入真实 AtomUI/Avalonia UI runtime。

范围：

- real UI dispatcher。
- ViewLocator。
- ViewFactory。
- Route outlet。
- ViewModel/View binding。
- ResourceDictionary bridge。
- visual attach/detach feedback。
- platform integration tests。

最小闭环：

```text
Navigate
-> create ViewModel
-> locate real View
-> commit to outlet
-> visual lifecycle feedback
-> deactivate on detach
```

暂不做：

- 完整复杂 UI shell。
- 业务布局。
- 高级主题编辑器。
- 插件 UI 复杂热替换。

必测项：

- fake tests 继续通过。
- platform integration test 跑通真实 dispatcher。
- ViewLocator 找到 View。
- Outlet commit 到 visual tree。
- Resource refresh。
- visual detach 触发 deactivation。

DoD：

- Routing、MVVM、Presentation 真实链路可用。
- 平台集成测试独立分类。
- UI 线程规则可诊断。

## 14. Phase 11：Templates 和 CLI

目标：把已稳定的底层能力包装成开发者入口。

Templates 范围：

- application template。
- module template。
- page template。
- plugin template。
- test template。
- localization/config template。

CLI 范围：

- `atomui city new app`
- `atomui city generate module/page/plugin/test`
- `atomui city build/pack/publish`
- `atomui city inspect`
- `atomui city docs check`
- `atomui city tests check`
- `atomui city plugin inspect/install --dry-run`
- AI-friendly JSON。
- plan/apply。

暂不做：

- 自由自然语言代码生成。
- 插件市场服务端。
- 复杂交互 wizard。

必测项：

- template smoke。
- generated project build。
- generated test project test。
- CLI JSON schema。
- non-interactive。
- plan/apply。
- docs/tests gate。

DoD：

- 新用户可以用 `atomui city new app` 创建项目。
- 生成项目能 build/test。
- CLI 不绕过 Build/Templates。
- AI Agent 可以通过 inspect、docs、tests、build JSON 输出理解工作区状态。

## 15. Phase 12：Hardening

目标：收敛质量、兼容性和发布前工程细节。

范围：

- API review。
- docs review。
- analyzer 扩展。
- package metadata。
- NuGet packing。
- license。
- CI pipeline。
- performance smoke。
- trimming/AOT verification。
- examples。
- migration notes。

必测项：

- full build。
- full test。
- package validation。
- template smoke。
- plugin lifecycle。
- platform integration。
- docs link check。
- analyzer tests。
- generator tests。

DoD：

- 所有文档链接有效。
- 所有测试通过。
- 所有 package 可 pack。
- 示例能运行。
- 没有未确认的公共 API 偏离文档。
- Release notes 可生成。

## 16. 第一批落地范围

第一批实现只做：

1. Phase 0：仓库和工程基线。
2. Phase 1：Core Kernel 最小闭环。
3. Phase 2：Testing 基础设施。
4. Phase 3：Modularity、Configuration 和 DI。

这四个阶段完成后，再进入 State、EventBus、MVVM 和 Routing。

原因：

- 没有 Core，后面的模块没有稳定生命周期和诊断底座。
- 没有 Testing，后面的模块无法执行功能点测试门禁。
- 没有 Modularity、Configuration 和 DI，模块实现会直接耦合到临时代码。

## 17. 全局暂停条件

出现以下情况必须暂停实现，回到文档确认：

- 需要新增公共 API，但文档没有定义。
- 生命周期阶段需要调整。
- 包依赖方向需要改变。
- 测试矩阵无法覆盖功能点。
- 单元测试不可行，需要改成替代测试。
- 运行时需要反射扫描作为默认路径。
- 插件卸载无法按文档实现。
- CLI 或 Templates 需要改变 Build 规则。

## 18. 下一步

路线图确认后，进入第一批实现计划。

第一份实现计划应聚焦：

```text
Phase 0 + Phase 1
```

也就是仓库工程基线和 Core Kernel 最小闭环。
