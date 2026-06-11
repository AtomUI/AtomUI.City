# AtomUI.City.Core Hosting 设计

版本：v0.1
状态：正式初版
适用范围：`AtomUI.City.Core` 中 Host、Application 构建、GenericHost 集成、启动/停止流程、Host 扩展方法 DSL

## 1. 目标

Hosting 是 AtomUI.City 应用运行时的入口。

它负责把 .NET GenericHost、Configuration、DependencyInjection、Logging、Options、ModuleSystem、Lifecycle、ContributionRegistry、PluginSystem、Presentation bridge 和 Diagnostics 串起来，形成 AtomUI.City 自己的应用框架启动模型。

Hosting 的目标：

- 使用 .NET GenericHost 作为底层容器和基础设施。
- 提供 AtomUI.City 自己的 Application Host API。
- 统一应用构建、启动、停止和释放流程。
- 驱动模块发现、模块图构建和模块生命周期。
- 创建 HostScope 和 ApplicationScope。
- 接入 Lifecycle Middleware。
- 为 Presentation 和 PluginSystem 提供受控 Host contract。
- 保持 Core 不依赖 AtomUI/Avalonia。
- 保持 AOT/trimming/source generator 友好。

## 2. 非目标

Hosting 不负责：

- UI 控件、主题、窗口和 Dispatcher 的具体实现。
- Route 到 ViewModel Target 的选择。
- View/ViewModel 绑定。
- 路由图解释。
- Data 请求管线。
- 权限策略解释。
- 插件程序集加载细节。
- CLI 交互体验。
- Build/source generator 实现。

这些能力由对应模块负责。Hosting 只负责启动边界、生命周期调度和基础设施编排。

## 3. Host 与 GenericHost 的关系

AtomUI.City 复用 .NET GenericHost 的成熟基础设施：

- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Logging`

但 GenericHost 不是 AtomUI.City 的公共编程范式本身。

关系：

```text
ApplicationHost
-> GenericHost
-> IServiceProvider
-> IConfiguration
-> ILogger
-> IOptions
```

GenericHost 负责成熟的 .NET 基础设施；AtomUI.City Host 负责自己的框架语义：

- Application。
- Module。
- Plugin。
- Contribution。
- ContributionLease。
- LifecycleScope。
- ServiceScope。
- Lifecycle middleware。
- Desktop lifetime。

允许提供 GenericHost 桥接入口，但不能绕开 AtomUI.City 的模块和生命周期约束。

## 4. 命名规范

类型名不加 `City` 前缀。命名空间已经是 `AtomUI.City.*`。

推荐类型：

| 类型 | 职责 |
|---|---|
| `ApplicationHost` | 应用 Host 静态入口和默认实现。 |
| `ApplicationHostBuilder` | 应用构建器，包装 GenericHost builder 和框架构建上下文。 |
| `ApplicationHostOptions` | Host 级配置，例如环境、关闭超时、启动模块、动态能力策略。 |
| `ApplicationContext` | 当前应用上下文。 |
| `ApplicationLifetime` | 应用 lifetime 抽象。 |
| `IApplicationHost` | Host 运行时接口。 |
| `IApplicationHostBuilder` | Host 构建期接口。 |
| `IApplicationContext` | 应用上下文接口。 |
| `IApplicationLifetime` | 桌面应用 lifetime 抽象。 |
| `IApplicationService` | Host 管理的应用服务或启动服务抽象。 |

避免：

```text
CityApplicationBuilder
ICityHost
CityHostOptions
ModuleScope
PluginScope
PluginModuleScope
```

也避免直接命名为 `IHost`、`HostOptions`、`IHostLifetime`，防止和 `Microsoft.Extensions.Hosting` 冲突。

## 5. 核心抽象

### ApplicationHost

推荐入口：

```csharp
var builder = ApplicationHost.CreateBuilder(args);
```

职责：

- 创建默认 `ApplicationHostBuilder`。
- 配置 GenericHost defaults。
- 注入 AtomUI.City Core 基础服务。
- 提供 `Build()` / `RunAsync()` 默认路径。

### ApplicationHostBuilder

构建期对象。

职责：

- 持有 GenericHost builder。
- 持有 AtomUI.City 构建期上下文。
- 收集启动模块。
- 收集配置动作。
- 收集生命周期中间件。
- 收集 framework feature descriptor。
- 输出 `IApplicationHost`。

### IApplicationHost

运行期对象。

建议能力：

```csharp
Task StartAsync(CancellationToken cancellationToken = default);
Task StopAsync(CancellationToken cancellationToken = default);
Task RunAsync(CancellationToken cancellationToken = default);
IServiceProvider Services { get; }
IApplicationContext Context { get; }
ILifecycleScope HostScope { get; }
ILifecycleScope ApplicationScope { get; }
```

### ApplicationContext

建议包含：

- ApplicationName。
- EnvironmentName。
- ContentRootPath。
- AppDataPath。
- StartupArguments。
- Configuration。
- Services。
- Host state。
- Diagnostics context。

### IApplicationLifetime

Core 定义抽象，Presentation 适配 Avalonia。

职责：

- 通知 Host UI runtime 已准备。
- 通知 Host 应用停止。
- 支持桌面应用挂起/恢复。
- 提供 shutdown cancellation。

Core 不引用 Avalonia lifetime 类型。

## 6. 扩展方法 DSL

AtomUI.City Application 构建使用 .NET 扩展方法风格。

分类：

| 前缀 | 用途 | 顺序语义 |
|---|---|---|
| `Add*` | 注册服务、能力描述、descriptor。 | 通常无顺序语义。 |
| `Use*` | 加入生命周期管线、中间件、模块。 | 有顺序语义。 |
| `Configure*` | 配置 Options、Builder、策略。 | 后者可覆盖。 |
| `Map*` | 映射路由、View、资源、命令入口。 | 需要冲突检测。 |
| `With*` | 给定义对象附加元数据。 | 链式配置。 |
| `Enable*` / `Disable*` | 开关能力。 | 最终配置生效。 |

示例：

```csharp
var builder = ApplicationHost.CreateBuilder(args);

builder
    .UseModule<AppModule>()
    .ConfigureHost(options => { })
    .ConfigureLifecycle(lifecycle => { })
    .ConfigureServices(services => { });

await builder.Build().RunAsync();
```

规则：

- 扩展方法默认只收集配置、descriptor、服务注册或中间件。
- 扩展方法不得执行真实启动逻辑。
- 扩展方法不得调用 `BuildServiceProvider()`。
- 扩展方法不得启动线程、加载插件、创建 ViewModel 或触发导航。
- `Use*` 必须保留调用顺序。
- `Add*` 应尽量幂等。
- `Map*` 必须冲突检测。
- `Configure*` 使用 Options 模式。
- 所有扩展方法返回原 builder 或更具体的 feature builder。

## 7. Application / Module / Plugin 组成模型

Host 管理 Application 的组成：

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

Module 和 Plugin 是能力贡献方，不是 Scope。

Module 可以贡献：

- Service registration。
- Configuration。
- Route。
- Permission。
- Localization resource。
- Event handler。
- Data client。
- Presentation resource。
- Plugin extension point。

Plugin 可以携带自己的 Modules，这些插件模块也通过 Contribution 向 Host 贡献能力。

## 8. Contribution 与 ContributionLease

所有可撤销能力都通过 Contribution 进入 Host。

```text
Module or Plugin Module
-> Contribution
-> Host Registry
-> ContributionLease
```

例如：

```text
RouteContribution("/sales")
  Module = SalesModule
  Plugin = SalesPlugin
  Lease = RouteContributionLease("/sales")
```

Host 必须持有 ContributionLease，用于停用、卸载、关闭和诊断。

ContributionLease 需要支持：

- 可撤销。
- 可诊断。
- 可追踪 Module / Plugin。
- 按反向顺序撤销。
- 撤销失败汇总。

## 9. Lifecycle Scope 模型

Scope 只表示运行实例的生命周期边界。

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

这里没有 `ModuleScope`、`PluginScope`。

原因：

- Module / Plugin 是组成和贡献方。
- RouteScope / OperationScope 是运行实例。
- 二者通过 Contribution 关联，而不是通过父子 Scope 关联。

例如插件路由：

```text
RouteScope("/sales")
  Parent = NavigationScope
  Contribution = RouteContribution("/sales")
  Contribution.Module = SalesModule
  Contribution.Plugin = SalesPlugin
  Services = SalesPlugin ServiceScope
```

插件卸载时：

```text
SalesPlugin stopping
-> stop new entries from SalesPlugin contributions
-> find active RouteScope where Contribution.Plugin == SalesPlugin
-> deactivate routes
-> cancel operations
-> dispose activation scopes
-> revoke contribution leases
-> dispose plugin ServiceScope
-> unload plugin assemblies
```

## 10. Host 构建流程

Build 前完成服务注册和静态模块图准备。

```text
ApplicationHost.CreateBuilder(args)
-> create GenericHost builder
-> load configuration
-> configure logging
-> register Core infrastructure
-> collect startup modules
-> load generated module manifest
-> build module graph
-> run module PreConfigureServices
-> run module ConfigureServices
-> run module PostConfigureServices
-> apply user ConfigureServices
-> build GenericHost
-> create HostScope
-> create ApplicationHost
```

原则：

- Static Module 的服务注册必须发生在 GenericHost Build 之前。
- Build 后不允许普通 Module 或 Plugin 修改 Root ServiceProvider。
- Plugin 服务必须进入自己的 ServiceScope。
- Runtime 动态能力必须走 ContributionRegistry，而不是改 Root DI。

## 11. Host 启动流程

Build 后进入运行阶段。

```text
StartAsync
-> Run HostStarting middleware
-> Create ApplicationScope
-> Initialize modules
-> Start modules
-> Apply static contributions
-> Run ApplicationStarting middleware
-> Wait Presentation lifetime ready
-> Create PresentationScope
-> Navigate initial route
-> Run ApplicationStarted middleware
-> Enter Running
```

注意两段式：

```text
Service registration phase: Build 前
Runtime initialization phase: Build 后
```

这点必须硬性规定，否则模块系统、DI 和插件系统都会混乱。

## 12. Host 停止流程

停止必须以尽可能释放为原则。

```text
Stop requested
-> reject new operations
-> Run ApplicationStopping middleware
-> stop new route activation
-> deactivate active routes
-> cancel running operations
-> deactivate plugins
-> optionally unload plugins
-> stop modules in reverse order
-> revoke remaining contribution leases
-> dispose PresentationScope
-> dispose ApplicationScope
-> stop GenericHost
-> dispose HostScope
-> Run ApplicationStopped middleware
```

规则：

- Stop 必须幂等。
- Stop 支持超时。
- Cancellation 不是 error。
- 多个 dispose 错误要汇总。
- 不能因为一个插件卸载失败阻断整个 Host 关闭。
- 插件卸载失败进入 `UnloadPending`。

## 13. ModuleSystem 集成

Hosting 驱动模块系统，但不解释模块贡献内容。

边界：

```text
Hosting
-> collect startup modules
-> build module graph
-> run service configuration stages
-> build GenericHost
-> run module initialization/start/stop stages
```

Hosting 不负责解释：

- 路由贡献。
- 权限贡献。
- 本地化贡献。
- Presentation 资源。
- EventBus handler。
- Data client。

这些贡献应由模块系统转换成 ContributionLease，并进入对应 registry。

## 14. Configuration 集成

Hosting 负责建立配置根对象。

配置来源建议：

```text
Default framework settings
-> appsettings.json
-> appsettings.{Environment}.json
-> environment variables
-> command line arguments
-> app local settings
-> user settings
```

实际顺序在 `configuration.md` 中细化。

Hosting 只定义配置入口：

```csharp
builder.ConfigureConfiguration(configuration => { });
builder.ConfigureOptions<ApplicationHostOptions>(options => { });
```

## 15. DI 集成

Hosting 负责：

- 创建服务注册阶段。
- 调用模块服务注册。
- Build Root ServiceProvider。
- 创建 HostScope 和 ApplicationScope。

DI 细节放到 `dependency-injection.md`。

Hosting 必须明确：

- 不在 Build 后修改 Root ServiceProvider。
- 不在扩展方法中调用 `BuildServiceProvider()`。
- 插件不写 Root ServiceProvider。
- Scope Tree 和 DI scope 需要明确绑定。

## 16. Presentation 集成边界

Core Hosting 不依赖 AtomUI/Avalonia。

Presentation 负责提供扩展：

```csharp
builder.UseAtomUIPresentation(...);
```

Presentation 扩展负责：

- 注册 Avalonia/AtomUI 集成服务。
- 适配 Avalonia lifetime 到 `IApplicationLifetime`。
- 提供 UI Dispatcher 实现。
- 创建 PresentationScope。
- 创建 WindowScope。
- 创建 NavigationScope。
- 提供 initial route 启动桥接。
- 提供 View/ViewModel activation 接入。

Hosting 只等待 `IApplicationLifetime` 信号，不直接操作 Avalonia 类型。

## 17. PluginSystem 集成边界

Host 是插件运行的协调者，但插件加载细节属于 PluginSystem。

Host 提供：

- Host contract。
- Contribution registry。
- Lifecycle pipeline。
- Diagnostics。
- Stop/unload 调度。

PluginSystem 负责：

- 插件发现。
- 元数据验证。
- 依赖解析。
- `AssemblyLoadContext`。
- Plugin ServiceScope。
- 插件模块图。
- ContributionRequest。
- Unload diagnostics。

Plugin 不能：

- 修改 Root ServiceProvider。
- 绕过 Host Registry。
- 绕过 Security/Data pipeline。
- 保存 Host、Scope、ServiceProvider、ViewModel 到静态字段。

## 18. AOT / Source Generator 策略

Hosting 必须 AOT-first。

默认路径：

```text
Explicit registration
Generated manifest
Generated registrar
Strongly typed descriptor
```

不默认：

```text
Assembly scanning
Naming convention reflection
Dynamic proxy
Expression tree compilation
```

推荐：

```csharp
builder.UseModule<AppModule>();
```

不推荐默认启用：

```csharp
builder.ScanAllAssemblies();
```

如果提供动态发现：

```csharp
builder.EnableDynamicDiscovery();
```

必须满足：

- Opt-in。
- Analyzer warning。
- AOT/trimming 诊断。
- Strict mode 下可报错。

## 19. 错误策略

默认策略：

| 阶段 | 策略 |
|---|---|
| Builder 创建失败 | Fatal。 |
| Configuration 加载失败 | 默认 Fatal，可配置 optional。 |
| Core service 注册失败 | Fatal。 |
| Module graph 构建失败 | Fatal。 |
| Module service registration 失败 | Fatal。 |
| Module initialization 失败 | 默认 Fatal，可配置降级。 |
| Presentation lifetime 启动失败 | Fatal。 |
| Plugin 加载失败 | Non-fatal。 |
| Plugin 卸载失败 | 标记 `UnloadPending`。 |
| Stop / Dispose 失败 | 汇总错误，继续释放。 |

## 20. 诊断要求

Hosting 必须记录：

- 应用名、环境、启动参数。
- 配置来源。
- 启动模块列表。
- 生成 manifest 使用情况。
- 动态发现是否启用。
- 模块图。
- GenericHost build 耗时。
- HostScope / ApplicationScope / PresentationScope 创建释放。
- ContributionLease 创建和撤销。
- 插件停用/卸载状态。
- Lifecycle middleware 执行顺序。
- Startup / Stop 各阶段耗时。
- Fatal / non-fatal 错误。

## 21. 测试要求

Testing 包后续要支持：

- `TestApplicationHost`。
- 无真实 Presentation 启动。
- 注入测试配置。
- 注入测试模块。
- 手动 Start/Stop。
- 断言模块顺序。
- 断言 ContributionLease 创建和撤销。
- 断言 Scope 创建和释放顺序。
- 断言插件 Contribution 对应 RouteScope 可反查。
- 断言 dynamic discovery 在 strict AOT 模式下被拒绝或 warning。
- 断言 Stop 幂等。
- 断言 Dispose 错误汇总。

## 22. 开发者约束

应用开发者应遵守：

- 通过 `ApplicationHost.CreateBuilder(args)` 创建应用。
- 通过 `UseModule<TModule>()` 显式注册启动模块。
- 不直接绕过 AtomUI.City Host 修改运行时流程。
- 不在扩展方法中执行真实运行时逻辑。
- 不依赖默认程序集扫描。
- 不在模块构造函数中启动任务或订阅事件。
- 不在 Build 后修改 Root ServiceProvider。
- Presentation、Plugin、Routing 等能力通过对应扩展点接入。
