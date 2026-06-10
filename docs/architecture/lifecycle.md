# AtomUI.City 生命周期

版本：v0.2
状态：初版草案
适用范围：Application、Module、Plugin、Route、ViewModel、State、EventBus、Command 生命周期设计

## 1. 目标

生命周期是 AtomUI.City 的核心设计重心。AtomUI.City 不是一组零散工具类，而是应用框架。它必须明确管理创建、初始化、激活、停用、取消、释放、错误处理和诊断流程，并通过这些约束影响开发者组织应用的方式。

生命周期设计需要解决以下问题：

- 应用启动、挂起、恢复、关闭的顺序。
- 模块发现、配置、初始化、启动、停止的顺序。
- 路由进入、离开和 ViewModel 激活的边界。
- State Reaction、EventBus 订阅、Command/Data 请求的释放和取消。
- 运行时插件装载、停用、卸载和依赖释放。
- 开发者在生命周期关键点注入自定义逻辑。
- 生命周期错误的传播、降级和诊断。

## 2. 核心模型

AtomUI.City 生命周期模型由四个核心概念组成：

```text
Lifecycle Kernel
= Scope Tree
+ Middleware Pipeline
+ Contribution Lease
+ Diagnostics
```

| 概念 | 职责 |
|---|---|
| Scope Tree | 只表达应用、窗口、导航、路由、ViewModel、操作和订阅等运行实例之间的父子生命周期。 |
| Middleware Pipeline | 允许开发者在生命周期关键点注入自定义逻辑，并可以影响流程。 |
| Contribution Lease | 记录 Module 和 Plugin 贡献的路由、资源、权限、订阅等能力，并支持撤销。 |
| Diagnostics | 记录生命周期阶段、耗时、错误、取消、资源释放和插件卸载状态。 |

生命周期不是简单事件回调。事件只适合观察；中间件用于参与流程、改变流程、短路流程和包裹框架默认处理。

## 3. Application、Contribution 与 Scope

AtomUI.City 需要区分三类概念，不能全部命名为 `*Scope`。

Application 组成描述应用由哪些 Module 和 Plugin 构成：

```text
Application
  Modules
    AppModule
    RoutingModule
    SecurityModule

  Plugins
    SalesPlugin
      Modules
        SalesModule
        SalesReportModule
```

Contribution 描述 Module 或 Plugin 向 Host 贡献的能力：

```text
RouteContribution("/sales")
  Module = SalesModule
  Plugin = SalesPlugin
  Lease = RouteContributionLease("/sales")
```

Scope Tree 只表达运行实例之间的生命周期父子关系：

```text
HostScope
  -> ApplicationScope
    -> PresentationScope
      -> WindowScope
        -> NavigationScope
          -> RouteScope
            -> ActivationScope
              -> StateScope
              -> OperationScope
              -> SubscriptionScope
```

父 Scope 停止时，子 Scope 必须按反向顺序停止和释放。子 Scope 不能比父 Scope 活得更久。

Scope 不用于表达 Module 或 Plugin 的能力来源。运行实例通过 Contribution 关联到对应 Module 和 Plugin：

```text
RouteScope("/sales")
  Parent = NavigationScope
  Contribution = RouteContribution("/sales")
  Contribution.Module = SalesModule
  Contribution.Plugin = SalesPlugin
  Services = SalesPlugin ServiceScope
```

这意味着：

- `Module` 和 `Plugin` 是应用组成和能力贡献方，不命名为 `ModuleScope` 或 `PluginScope`。
- `RouteScope` 是导航运行实例，不是 Module 或 Plugin 的子 Scope。
- 插件卸载时通过 ContributionLease 和 Contribution 上的 Module/Plugin 信息找到相关运行实例。
- `ServiceScope` 只表示 DI 范围，不和生命周期 Scope 混用。

每个 Scope 都应该携带：

- 当前 `IServiceProvider` 或 DI scope。
- 当前 `CancellationToken`。
- 当前 Dispatcher 策略。
- 当前错误处理策略。
- 当前诊断上下文。
- 当前可释放资源集合。
- 当前贡献能力集合。
- 当前生命周期状态。

## 4. 应用生命周期

AtomUI.City 的应用生命周期面向桌面软件长期运行模型：

```text
CreateHost
-> LoadConfiguration
-> DiscoverModules
-> BuildModuleGraph
-> PreConfigureServices
-> ConfigureServices
-> PostConfigureServices
-> BuildServiceProvider
-> InitializeModules
-> StartModules
-> StartPresentation
-> NavigateInitialRoute
-> Running
-> Suspending / Resuming
-> Stopping
-> Stopped
-> Disposed
```

与 Web 应用不同，桌面应用没有 request scope 作为主要业务边界。AtomUI.City 的主要运行边界是：

- `ApplicationScope`
- `PresentationScope`
- `WindowScope`
- `NavigationScope`
- `RouteScope`
- `ActivationScope`
- `OperationScope`

这些 Scope 决定了应用开发者应该把页面进入、状态订阅、命令执行和资源释放放在哪里。Module 和 Plugin 的服务注册、能力贡献和生命周期回调由 ModuleSystem 和 PluginSystem 管理，并通过 ContributionLease 与运行实例关联。

## 5. 模块生命周期

模块生命周期：

```text
Discovered
-> PreConfigure
-> ConfigureServices
-> PostConfigureServices
-> Initialize
-> Started
-> Stopping
-> Stopped
-> Disposed
```

模块可以贡献：

- 服务注册。
- 配置。
- 路由。
- 权限。
- 本地化资源。
- 数据客户端。
- EventBus handler。
- 插件扩展点。
- Presentation 资源。

模块构造函数不应执行真实工作。真实工作应进入明确生命周期阶段，长期资源必须进入对应 Scope。

模块不命名为 `ModuleScope`。模块是应用组成和能力贡献方；模块贡献的路由、权限、资源等能力必须通过 Contribution 和 ContributionLease 进入对应 registry。

## 6. 路由生命周期

路由生命周期：

```text
AcceptNavigationRequest
-> NormalizeTarget
-> CaptureRouteGraphSnapshot
-> MatchRoute
-> BuildNavigationPlan
-> RunMatchPolicies
-> CreateProvisionalRouteScopes
-> RunEnterGuards
-> ConfirmLeavingRoutes
-> ResolveData
-> CreateViewModels
-> PreparePresentationChange
-> CommitOnUiThread
-> UpdateNavigationStateAndJournal
-> DisposeRemovedRouteBranches
-> NavigationCompleted
```

路由是页面进入的正式入口。权限检查、数据预取、ViewModel 创建和 Activation 都应由路由生命周期驱动。

开发者不应在 ViewModel 构造函数中隐式发起页面级数据请求、权限判断或长期订阅。

当前路由树不能在候选路由准备完成前被破坏。候选路由先创建 provisional `RouteScope`，守卫、解析器和 ViewModel 创建成功后再进入 UI Thread 提交。准备阶段失败时释放候选分支，当前页面保持活动。

如果路由来自插件，`RouteScope` 仍然挂在 `NavigationScope` 下，但其 `Contribution` 指向对应插件模块。插件停用或卸载时，Host 通过该 Contribution 找到并关闭相关 RouteScope。

## 7. ViewModel 生命周期

ViewModel 生命周期：

```text
Constructed
-> Initialized
-> Activated
-> Deactivated
-> Disposed
```

ViewModel 构造函数只接收依赖和初始化普通字段。

以下资源必须进入 `ActivationScope`：

- State Reaction。
- EventBus 订阅。
- Interaction Handler。
- 定时器。
- 后台任务。
- UI 事件订阅。
- 可释放资源。

这样 ViewModel 可以被安全创建、缓存、重新激活、停用和释放。

## 8. Operation 生命周期

Command 和 Data 请求都属于 Operation。

每次 Operation 执行都创建 `OperationScope`：

```text
CanExecute
-> Executing
-> Completed
```

失败或取消路径：

```text
CanExecute
-> Executing
-> Failed / Cancelled
```

`OperationScope` 需要绑定到父 Scope。页面离开、ViewModel 停用、应用关闭或插件卸载时，正在执行的 Operation 应收到取消信号。

## 9. State 生命周期

State 不应作为无边界全局对象散落在应用中。

推荐 Scope：

| State 类型 | 推荐 Scope |
|---|---|
| 应用级状态 | `ApplicationScope` |
| 模块级持久状态 | Module 服务或 ApplicationScope 管理的模块状态服务 |
| 插件级持久状态 | Plugin service scope 管理的插件状态服务 |
| 页面级状态 | `RouteScope` |
| ViewModel 临时状态 | `ActivationScope` |
| 请求/命令临时状态 | `OperationScope` |

Computed 和 Reaction 必须注册到 Scope。Scope 停止时，Reaction 自动释放。需要恢复的状态通过 Snapshot 策略保存；不需要恢复的状态随 Scope 销毁。

## 10. PluginSystem 生命周期

PluginSystem 是全局一等生命周期对象，不能只是 ModuleSystem 的附属品。

插件生命周期：

```text
Discover
-> Verify
-> CreatePluginLoadContext
-> LoadAssemblies
-> BuildPluginModuleGraph
-> BuildPluginServiceProvider
-> Initialize
-> ApplyContributions
-> Activate
-> Running
-> Deactivate
-> StopNewEntries
-> DeactivateRoutesAndViewModels
-> StopOperations
-> RevokeContributions
-> Inactive
-> Unload
-> DisposeServices
-> UnloadAssemblyLoadContext
-> VerifyUnloaded
```

关键约束：

- 每个插件独立加载上下文。
- 可卸载插件使用 collectible `AssemblyLoadContext`。
- 插件 contract 由主应用默认上下文加载。
- 插件服务不能污染 Root ServiceProvider。
- 插件贡献必须通过 Contribution Lease 管理。
- 插件卸载时必须先阻止新入口、停用插件 UI、取消插件操作、撤销贡献、释放服务，最后卸载加载上下文。
- 插件卸载失败时进入 `UnloadPending` 状态，并输出诊断。

.NET 的 `AssemblyLoadContext.Unload()` 是协作式卸载。只要外部仍持有插件对象、类型、反射对象、事件订阅、静态字段、后台线程或委托引用，卸载就可能无法完成。

底层运行时机制参考：[.NET 依赖项加载参考](../reference/dotnet/dependency-loading.md)。

## 11. Contribution Lease

模块和插件不能直接永久注册运行时能力，而应通过 Contribution Lease 贡献能力。

常见 Lease：

- `RouteContributionLease`
- `ResourceContributionLease`
- `PermissionContributionLease`
- `LocalizationContributionLease`
- `EventSubscriptionLease`
- `CommandContributionLease`
- `PresentationResourceLease`

Scope 停止时，Lease 按反向顺序撤销。插件卸载时，所有插件贡献必须撤销完成后才能进入加载上下文卸载阶段。

## 12. Middleware Pipeline

生命周期关键点必须支持中间件，而不仅是事件。

中间件模型：

```text
LifecycleContext -> Middleware1 -> Middleware2 -> Framework Handler -> MiddlewareN
```

建议提供以下 Pipeline：

| Pipeline | 用途 |
|---|---|
| Application pipeline | 启动、挂起、恢复、关闭。 |
| Module pipeline | 模块初始化、启动、停止。 |
| Plugin pipeline | 插件加载、启用、停用、卸载。 |
| Route pipeline | 导航、守卫、解析、进入、离开。 |
| ViewModel pipeline | 创建、激活、停用、释放。 |
| State pipeline | Reaction、Snapshot、恢复、释放。 |
| Operation pipeline | Command/Data 请求执行、取消、失败。 |
| Error pipeline | 生命周期错误统一处理。 |

开发者可以在这些点注入日志、审计、权限、性能跟踪、资源预热、诊断、插件校验和自定义降级逻辑。

## 13. 错误策略

生命周期错误不能一刀切。

| 阶段 | 默认策略 |
|---|---|
| Host 创建失败 | Fatal，应用不能继续。 |
| Core Module 初始化失败 | Fatal。 |
| 静态 Module 初始化失败 | 默认 Fatal。 |
| Plugin 加载失败 | 非 Fatal，插件禁用并记录诊断。 |
| Route Guard 失败 | Navigation rejected。 |
| Resolver 失败 | Navigation failed 或进入错误路由。 |
| ViewModel Activation 失败 | 释放当前 Scope，导航失败。 |
| Command/Data 失败 | Operation failed，不崩应用。 |
| Plugin 卸载失败 | 标记 `UnloadPending`，阻止更新或删除。 |

错误策略必须和 Scope 释放顺序绑定。某个阶段失败时，已经创建的子 Scope 和 Lease 必须被释放或撤销。

## 14. 设计原因

这种设计会直接塑造开发者的研发模式：

- 模块负责注册和贡献能力，不负责偷偷启动长期任务。
- 路由负责页面进入，权限和数据预取不散落在 ViewModel。
- ViewModel 构造函数只接依赖，长期订阅必须进 ActivationScope。
- Command/Data 每次执行都有 OperationScope，可取消、可诊断。
- State Reaction 绑定 Scope，页面离开自动释放。
- Plugin 运行时装载卸载可控，不污染全局容器。
- 生命周期变成可测试对象，而不是隐式副作用。

这套模型同时支持桌面应用长期运行、MVVM 激活/停用、运行时插件卸载和开发者自定义生命周期中间件。
