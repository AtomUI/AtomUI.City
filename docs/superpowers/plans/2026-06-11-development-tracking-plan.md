# AtomUI.City Development Tracking Plan

**目标：** 用可精确跟踪的阶段计划推进 AtomUI.City 从当前骨架到可发布的第一版框架内核。

**当前基线：**

- 分支：`develop`
- 当前提交：`d54bde4 fix(Cli): protect dotnet invocation arguments`
- 当前测试：`dotnet build AtomUICity.slnx --no-restore` 通过；`bash engineering/test-ci.sh` 通过，638 tests passed；`dotnet format AtomUICity.slnx --verify-no-changes --no-restore` 通过；`bash engineering/check-docs.sh` 通过；`git diff --check` 通过
- 当前状态：已完成 Generator localization readonly collections

**最新实现检查点：**

- [x] Security command authorization source
- [x] Security command authorization refresh on authentication / permission / descriptor changes
- [x] Security command authorization cancellation does not allow execution
- [x] Data access token credential provider
- [x] Data credential provider DI registration
- [x] Data bearer request fails gracefully when full Security registration is absent
- [x] Localization message key formatting
- [x] Localization message format failure diagnostics
- [x] Localization service DI registration
- [x] Data error localizable message metadata
- [x] Security authorization localizable message metadata
- [x] MVVM validation localizable message metadata
- [x] Localization culture-aware text handle
- [x] Localization text refresh on culture switch
- [x] Localization text refresh diagnostics
- [x] Presentation localized text target binding
- [x] Presentation localized text refresh through UI dispatcher
- [x] Presentation localized text binding disposal through ActivationScope
- [x] Presentation command text descriptor
- [x] Presentation command text localization binding
- [x] Presentation command text culture refresh
- [x] Presentation interaction text descriptor
- [x] Presentation interaction text localization binding
- [x] Presentation interaction text culture refresh
- [x] Localization localized message text handle
- [x] Localization localized message text culture refresh
- [x] Presentation validation message localization binding
- [x] Presentation validation message literal fallback
- [x] Presentation validation message culture refresh
- [x] Presentation validation message disposal through ActivationScope
- [x] Presentation generic error message descriptor
- [x] Presentation error message localization binding
- [x] Presentation Security authorization error message binding
- [x] Presentation error message culture refresh
- [x] Presentation error message disposal through ActivationScope
- [x] Routing route localization metadata descriptor
- [x] Routing route attributes carry localization keys
- [x] Generator route metadata carries localization keys into manifests
- [x] Presentation route text localization binding
- [x] Presentation route title and breadcrumb culture refresh
- [x] Presentation route text disposal through ActivationScope
- [x] Presentation window text descriptor
- [x] Presentation window title localization binding
- [x] Presentation window title literal fallback
- [x] Presentation window title culture refresh
- [x] Presentation window title disposal through ActivationScope
- [x] Presentation notification text descriptor
- [x] Presentation notification text localization binding
- [x] Presentation notification text literal fallback
- [x] Presentation notification text culture refresh
- [x] Presentation notification text disposal through ActivationScope
- [x] Presentation localization bridge applier pipeline
- [x] Presentation localization bridge UI dispatcher dispatch
- [x] Presentation localization bridge DI registration
- [x] Presentation current thread culture applier
- [x] Presentation default culture applier DI registration
- [x] Presentation culture flow direction applier
- [x] Presentation flow direction target DI registration
- [x] Presentation resource dictionary applier
- [x] Presentation resource dictionary target DI registration
- [x] Presentation plugin resource dictionary revoker
- [x] Presentation plugin resource dictionary revoke UI dispatcher dispatch
- [x] Presentation Avalonia UI dispatcher adapter
- [x] Presentation Avalonia UI dispatcher DI registration
- [x] Presentation Avalonia UI dispatcher cancellation handling
- [x] Presentation runtime state contract
- [x] Presentation runtime PresentationScope creation
- [x] Presentation runtime WindowScope creation
- [x] Presentation runtime stopping rejects new windows
- [x] Presentation runtime DI registration
- [x] Presentation runtime ready diagnostics
- [x] Presentation runtime stopping diagnostics
- [x] Presentation runtime diagnostics DI integration
- [x] Presentation dispatcher runtime not-ready gate
- [x] Presentation dispatcher runtime stopping gate
- [x] Presentation dispatcher runtime DI integration
- [x] Presentation dispatcher rejected operation diagnostics
- [x] Presentation dispatcher callback failure diagnostics
- [x] Presentation dispatcher diagnostics DI integration
- [x] Presentation ViewLocator matched diagnostics
- [x] Presentation ViewLocator failure diagnostics
- [x] Presentation ViewFactory created diagnostics
- [x] Presentation ViewFactory creation failure diagnostics
- [x] Presentation ViewBinder binding diagnostics
- [x] Presentation ViewBinder binding failure diagnostics
- [x] Presentation RouteOutlet commit plan diagnostics
- [x] Presentation RouteOutlet commit result diagnostics
- [x] Presentation visual lifecycle adapter execution diagnostics
- [x] Presentation visual lifecycle adapter failure diagnostics
- [x] Presentation resource dictionary revoke diagnostics
- [x] Presentation resource dictionary revoke failure diagnostics
- [x] Presentation resource dictionary apply diagnostics
- [x] Presentation resource dictionary apply failure diagnostics
- [x] Presentation interaction handler registry
- [x] Presentation interaction handler DI registration
- [x] Presentation interaction handler UI dispatcher execution
- [x] Presentation interaction handled diagnostics
- [x] Presentation interaction missing handler diagnostics
- [x] Presentation interaction handler failure diagnostics
- [x] Presentation interaction handler activation-scope cancellation
- [x] Presentation plugin interaction handler registration metadata
- [x] Presentation plugin interaction handler revoke by PluginId
- [x] Presentation plugin interaction handler revoke by ContributionId
- [x] Presentation plugin interaction handler revocation cancels pending requests
- [x] Presentation plugin interaction handler revoke diagnostics
- [x] Presentation validation visual state target contract
- [x] Presentation validation visual state snapshot
- [x] Presentation validation visual state UI dispatcher application
- [x] Presentation validation visual state diagnostics
- [x] Presentation validation visual state failure diagnostics
- [x] Presentation command source target contract
- [x] Presentation command binding initial CanExecute state
- [x] Presentation command binding CanExecute refresh
- [x] Presentation command binding execution request
- [x] Presentation command binding async executing state
- [x] Presentation command binding disposal through ActivationScope
- [x] Presentation command binding diagnostics
- [x] Presentation resource contribution registry DI registration
- [x] Presentation resource contribution lease registration
- [x] Presentation resource contribution lease disposal
- [x] Presentation resource contribution revoke by PluginId
- [x] Presentation resource contribution revoke by ContributionId
- [x] Presentation resource contribution revoke failure diagnostics
- [x] Presentation active plugin view registry DI registration
- [x] Presentation active plugin view lease tracking
- [x] Presentation active plugin view close by PluginId
- [x] Presentation active plugin view close by ContributionId
- [x] Presentation active plugin view stale outlet protection
- [x] Presentation active plugin view close failure diagnostics
- [x] Presentation plugin unload cleanup coordinator DI registration
- [x] Presentation plugin unload cleanup closes active plugin views before revocation
- [x] Presentation plugin unload cleanup by ContributionId
- [x] Presentation plugin unload cleanup stops when active views remain
- [x] Presentation plugin unload cleanup records resource dictionary failure
- [x] Presentation plugin unload cleanup diagnostics
- [x] Generator Presentation ViewFor metadata reader
- [x] Generator Presentation view manifest model
- [x] Generator Presentation view manifest deterministic ordering
- [x] Generator Presentation duplicate ViewModel/ViewKey diagnostics
- [x] Generator Presentation view contribution metadata
- [x] Presentation ViewDescriptor PluginId metadata
- [x] Presentation ViewForAttribute PluginId metadata
- [x] Presentation ViewRegistry revoke by PluginId
- [x] Generator Presentation view PluginId metadata
- [x] Generator Presentation plugin view requires PluginId and ContributionId pair
- [x] Presentation ViewRegistry DI registration
- [x] Presentation plugin unload cleanup revokes View descriptors
- [x] Presentation plugin unload cleanup records View descriptor revoke failure
- [x] Generator Presentation view registrar source builder
- [x] Generator Presentation view registrar incremental output
- [x] Generator Presentation manifest diagnostics reporting
- [x] Generator Presentation generated registrar compile verification
- [x] Generator Presentation diagnostics source location
- [x] Presentation ViewFactory service provider context
- [x] Generator Presentation view constructor dependency factory
- [x] State subscription diagnostics
- [x] State application registry diagnostics
- [x] State computed failure diagnostics
- [x] State writable update failure diagnostics
- [x] State snapshot restore failure diagnostics
- [x] State duplicate registration diagnostics
- [x] State background subscription option
- [x] State queued subscription option
- [x] State scope dispose failure diagnostics
- [x] State disposed scope subscription diagnostics
- [x] State computed dispose lifecycle guard
- [x] State computed failure retry guard
- [x] State computed dispose failure diagnostics
- [x] State snapshot restore unchanged value guard
- [x] State collection add or update change records
- [x] State collection remove change records
- [x] State collection clear change records
- [x] State collection batch add or update notifications
- [x] State collection item version query
- [x] State collection snapshot creation
- [x] State collection snapshot restore
- [x] State collection empty snapshot restore
- [x] State collection unchanged snapshot restore guard
- [x] State collection snapshot defensive copy
- [x] State collection changed event args defensive copy
- [x] State collection no-op update coverage
- [x] State collection batch failure atomicity coverage
- [x] EventBus subscription error policy configuration
- [x] EventBus stop publication error policy
- [x] EventBus fail publisher error policy
- [x] EventBus PostAsync canceled publication rejection
- [x] EventBus PostAsync accepted diagnostic
- [x] EventBus PostAsync rejected diagnostic
- [x] EventBus subscription in-flight drain
- [x] EventBus typed handler subscription overload
- [x] EventBus owned typed handler subscription overload
- [x] EventBus DI registration
- [x] EventBus contract DI registration
- [x] EventBus default contract descriptor registration
- [x] Data request completed diagnostic
- [x] Data request failed diagnostic
- [x] Data credential failure diagnostic
- [x] Data request cache contract
- [x] Data request cache hit
- [x] Data request cache write
- [x] Data cache read failure diagnostic
- [x] Data cache write failure diagnostic
- [x] Data in-memory request cache
- [x] Data request cache DI registration
- [x] Data cache hit diagnostic
- [x] Data cache miss diagnostic
- [x] Data request cache key invalidation
- [x] Data request cache invalidation diagnostic
- [x] Data request cache nullable value hit
- [x] Data connection registered diagnostic
- [x] Data connection stopped diagnostic
- [x] Data connection manager DI registration
- [x] Data typed client unregister
- [x] Data missing client diagnostic
- [x] Data HTTP 503 service unavailable mapping
- [x] Data HTTP 429 rate limit mapping
- [x] Data connection stop failure diagnostic
- [x] Data connection start failure diagnostic
- [x] Data connection started diagnostic
- [x] Data connection registration rejection diagnostic
- [x] Data client registration diagnostic
- [x] Data client unregistration diagnostic
- [x] Data client unregistration missing diagnostic
- [x] Data request retry diagnostic metadata
- [x] Data request retry diagnostic error kind
- [x] Data stale suppression diagnostic
- [x] Data gRPC transport cancellation mapping
- [x] Data HTTP transport cancellation mapping
- [x] Data HTTP response mapper serialization error mapping
- [x] Data gRPC invoker transport error mapping
- [x] Data HTTP send transport error mapping
- [x] Data request cancellation diagnostic
- [x] Data gRPC cancelled status mapping
- [x] Data HTTP transport timeout mapping
- [x] Data SignalR invoke timeout mapping
- [x] Data pipeline timeout cancellation result mapping
- [x] Data credential timeout cancellation result mapping
- [x] Data cache read timeout cancellation result mapping
- [x] Data request token late cancellation guard
- [x] Data credential late cancellation guard
- [x] Data cache read late cancellation guard
- [x] Data credential late timeout guard
- [x] Data cache read late timeout guard
- [x] Data transport late timeout guard
- [x] State application state type mismatch diagnostic
- [x] State snapshot plugin mismatch restore diagnostic
- [x] State snapshot owner module mismatch restore diagnostic
- [x] State snapshot value type mismatch restore diagnostic
- [x] State snapshot missing state restore diagnostic
- [x] State snapshot deterministic entry ordering
- [x] State snapshot null value restore
- [x] State snapshot readonly entries
- [x] State collection snapshot readonly items
- [x] State collection changed event readonly changes
- [x] EventBus publish result readonly deliveries
- [x] EventBus delivery cancellation accounting
- [x] PluginSystem result diagnostics readonly
- [x] PluginManifest readonly top-level collections
- [x] PluginManifest readonly nested collections
- [x] Routing descriptor and graph readonly collections
- [x] Routing template readonly collections
- [x] Security policy requirements readonly collection
- [x] CLI envelope diagnostics readonly collection
- [x] Template plan readonly collections
- [x] Template render diagnostics readonly collection
- [x] Presentation plugin unload result readonly errors
- [x] Localization culture state readonly collections
- [x] Core module descriptor readonly dependencies
- [x] Core host diagnostics readonly records
- [x] Core lifecycle scope readonly children
- [x] Core module registry readonly modules
- [x] Core application context readonly startup arguments
- [x] Data diagnostics readonly records
- [x] Localization diagnostics readonly records
- [x] Presentation validation visual state readonly snapshot
- [x] MVVM validation scope readonly collections
- [x] Testing source generation test case readonly collections
- [x] Testing diagnostics readonly entries
- [x] Testing routing test host readonly routes
- [x] Testing module test host readonly modules
- [x] Generator diagnostics readonly definitions
- [x] CLI dotnet invocation readonly arguments
- [x] Generator localization readonly collections

## 状态定义

- `[x]` 已完成
- `[ ]` 未完成
- `Blocked` 表示被外部条件阻塞
- `Review` 表示需要文档或设计 review 后才能继续
- `Parallel` 表示可以与其他 phase 并行推进

## 文档门禁

- [x] 全局架构文档完成
- [x] Lifecycle 设计文档完成
- [x] Hosting 设计文档完成
- [x] Module 设计文档完成
- [x] Routing 设计文档完成
- [x] Presentation 设计文档完成
- [x] MVVM 设计文档完成
- [x] State 设计文档完成
- [x] EventBus 设计文档完成
- [x] PluginSystem 设计文档完成
- [x] Data Access 设计文档完成
- [x] Localization 设计文档完成
- [x] Testing 设计文档完成
- [x] CLI / Templates 设计文档完成
- [x] Roadmap 文档完成
- [x] 开发计划文档确认并落盘

## Phase 0: 工程基线

目标：先把工程约束固定住，避免后续代码风格、许可证、构建产物、测试约束返工。

- [x] `AtomUICity.slnx` 创建完成
- [x] 核心项目骨架创建完成
- [x] 测试项目骨架创建完成
- [x] 输出目录统一到 `output`
- [x] 基础测试可运行
- [x] 项目清单测试：验证 solution 中必须包含的项目
- [x] 项目依赖边界测试：验证禁止的反向依赖
- [x] 输出目录测试：验证构建产物进入 `output`
- [x] CI 工作流：restore / build / test / format / docs check
- [x] format 检查：统一 `.editorconfig` 与格式化命令
- [x] license header 检查：LGPL v3 头部或集中式许可证策略
- [x] NuGet metadata 检查：license、repository、package id、description
- [x] Phase 0 验收

## Phase 1: Testing Platform

目标：提前建立测试基础设施，后续每个功能点都必须能被单元测试和必要的集成测试覆盖。

- [x] 测试分层规范落地：unit / integration / analyzer / generator / runtime
- [x] 测试命名规范落地
- [x] 测试基类与 shared test utilities
- [x] Host 测试夹具
- [x] Module 测试夹具
- [x] Routing 测试夹具
- [x] UI dispatcher 测试夹具
- [x] Plugin runtime 测试夹具
- [x] Source Generator snapshot / compilation test 基础设施
- [x] AOT 友好检查测试入口
- [x] CI 中接入测试分类
- [x] Phase 1 验收

## Phase 2: Source Generator Platform

目标：在 Module / Routing 前完成 SG 总体框架，避免后续依赖反射扫描。

- [x] SG 项目结构确认
- [x] Analyzer / Generator 命名规范
- [x] 增量生成器基础设施
- [x] 诊断 ID 规划
- [x] 生成代码命名规范
- [x] Module metadata 生成能力
- [x] Module dependency graph 生成能力
- [x] Service registration manifest 生成能力
- [x] Route manifest 生成能力
- [x] Localization manifest 生成能力
- [x] Plugin manifest 辅助生成能力
- [x] 生成器测试覆盖
- [x] Phase 2 验收

## Phase 3: Core Host / Lifecycle

目标：确定应用框架最核心的启动、运行、停止、异常和生命周期扩展模型。

- [x] Host builder contract
- [x] Application host runtime
- [x] Lifecycle stage model
- [x] Lifecycle middleware pipeline
- [x] Application / Module / Plugin / Navigation 生命周期关系
- [x] UI dispatcher 抽象接入点
- [x] Host options 与配置入口
- [x] Host diagnostics
- [x] Host integration tests
- [x] Phase 3 验收

## Phase 4: Modularity

目标：实现模块声明、模块依赖、模块初始化顺序和模块服务注册。

- [x] `IModule` / `ModuleBase` 接口完善
- [x] Module attribute 设计落地
- [x] 默认模块名规则：未指定时使用模块类型全名
- [x] 模块依赖声明
- [x] SG 生成模块依赖图
- [x] 循环依赖诊断
- [x] 模块初始化阶段：PreConfigure / Configure / PostConfigure
- [x] 异步初始化阶段
- [x] 模块服务注册
- [x] 模块生命周期测试
- [x] Phase 4 验收

## Phase 5: Routing

目标：实现 Route -> ViewModel Target 的稳定链路，并保证路由表 AOT 友好。

- [x] 路由语法实现
- [x] 路由模板解析
- [x] 路由参数绑定
- [x] Route target model
- [x] Route manifest SG
- [x] Route matcher
- [x] Navigation context
- [x] Route guard / filter
- [x] Navigation error model
- [x] Routing tests
- [x] Phase 5 验收

## Phase 6: Presentation

目标：打通 ViewModel -> View -> Outlet -> VisualTree，并定义 UI 状态回流机制。

- [x] View locator contract
- [x] View registration model
- [x] ViewModel to View resolution
- [x] Route outlet contract
- [x] Outlet commit pipeline
- [x] UI dispatcher integration
- [x] VisualTree 变化通知
- [x] UI element state 到 ViewModel 的反馈规范
- [x] Presentation tests
- [x] Phase 6 验收

## Phase 7: MVVM

目标：提供框架自己的 MVVM 编程模型，但不重复底层 UI 框架职责。

- [x] ViewModel activation
- [x] Activation scope
- [x] Command contract
- [x] Async command contract
- [x] Interaction contract
- [x] Validation contract
- [x] Observable / notification 基础约束
- [x] 与 Presentation 生命周期对齐
- [x] MVVM tests
- [x] Phase 7 验收

## Phase 8: State

目标：提供桌面应用可注入、可监听、线程安全、AOT 友好的状态管理能力。

- [x] 全局状态 contract
- [x] 局部状态 contract
- [x] 状态容器生命周期
- [x] 状态监听机制
- [x] UI 线程调度约束
- [x] 后台线程写入约束
- [x] 派生状态
- [x] 状态快照
- [x] 状态恢复
- [x] State tests
- [x] Phase 8 验收

## Phase 9: EventBus

目标：提供模块和插件之间低耦合、高性能、线程模型清晰的系统级事件机制。

- [x] 事件 contract 规范
- [x] 进程内事件总线
- [x] 同步 / 异步 handler
- [x] UI dispatcher 投递策略
- [x] 后台线程投递策略
- [x] 事件订阅生命周期
- [x] 跨插件边界 contract assembly 规范
- [x] 弱引用或显式释放策略
- [x] 事件诊断能力
- [x] EventBus tests
- [x] Phase 9 验收

## Phase 10: PluginSystem

目标：实现运行时可安装、加载、卸载的插件体系，并与 Host / Module / EventBus 生命周期一致。

- [x] Plugin manifest
- [x] Plugin assembly loading
- [x] Plugin module model
- [x] Plugin install directory 规范
- [x] Plugin package layout
- [x] Plugin NuGet packaging
- [x] Plugin MSBuild properties
- [x] Plugin MSBuild tasks
- [x] Plugin unload lifecycle
- [x] Plugin dependency validation
- [x] Plugin tests
- [x] Phase 10 验收

## Phase 11: Data Access

目标：提供统一的数据访问抽象，首批支持 HttpClient、gRPC、SignalR。

- [x] Data client contract
- [x] HttpClient integration
- [x] gRPC integration
- [x] SignalR integration
- [x] Connection lifecycle
- [x] Retry / timeout / cancellation
- [x] Authentication integration point
- [x] Threading policy
- [x] Data access tests
- [x] Phase 11 验收

## Phase 12: Localization

目标：实现强大的桌面端多语言能力，支持按语言包懒加载和 assembly 动态加载。

- [x] Localization contract
- [x] Resource provider
- [x] Culture switching
- [x] Per-language lazy loading
- [x] Assembly language package loading
- [x] AtomUI integration point
- [x] Missing resource diagnostics
- [x] Localization tests
- [x] Phase 12 验收

## Phase 13: CLI / Templates

目标：提供 `atomui city ...` 命令体系，支撑创建、开发、插件、模板、诊断和 AI 友好工作流。

- [x] CLI command architecture
- [x] `atomui city new`
- [x] `atomui city build`
- [x] `atomui city test`
- [x] `atomui city plugin`
- [x] `atomui city doctor`
- [x] AI-friendly project inspection output
- [x] Templates package
- [x] CLI tests
- [x] Phase 13 验收

## Phase 14: Packaging / Release

目标：形成可发布、可验证、可维护的第一版包体系。

- [x] Package dependency review
- [x] Public API review
- [x] XML docs / API docs check
- [x] NuGet package generation
- [x] Symbols package
- [x] SourceLink
- [x] License verification
- [x] Release notes generation
- [x] Versioning policy
- [x] Phase 14 验收

## 推荐立即执行顺序

1. 完成 `Phase 0` 剩余工程基线，特别是 CI、format、license header。
2. 并行启动 `Phase 1 Testing Platform`，先把测试夹具定下来。
3. 开始 `Phase 2 Source Generator Platform`，优先 Module metadata、dependency graph、Route manifest。
4. 再进入 `Phase 3 Host / Lifecycle`。
5. 随后依次推进 `Modularity`、`Routing`、`Presentation`，保证主链路闭环。
