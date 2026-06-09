# AtomUI.City 开源依赖策略

版本：v0.1
状态：初版草案
适用范围：AtomUI.City 框架实现、包边界、依赖引入和可选适配设计
最后更新：2026-06-10

## 1. 目标

AtomUI.City 需要依赖成熟的开源基础设施，但不能让外部库决定框架的公共编程模型。

依赖策略的目标是：

- 降低框架基础设施实现成本。
- 保持 Core 依赖链足够薄。
- 将 UI、MVVM、Data、CLI、Build、Testing 等能力分别放到对应包中。
- 将 Rx、ReactiveUI、DynamicData 等高级生态能力作为可选适配，而不是默认核心范式。
- 避免为了短期实现方便，把反射扫描、状态流、HTTP client、验证框架、CLI UI 等能力直接扩散到所有包。

## 2. 分层原则

依赖引入必须遵循以下原则：

- `AtomUI.City.Core` 只依赖 .NET 标准基础设施，不依赖 AtomUI/Avalonia、CommunityToolkit.Mvvm、ReactiveUI、System.Reactive。
- `AtomUI.City.Presentation` 承担 AtomUI/Avalonia 依赖。
- `AtomUI.City.Mvvm` 承担 CommunityToolkit.Mvvm 依赖。
- `AtomUI.City.Data` 承担 HTTP、resilience、client proxy 等依赖。
- `AtomUI.City.Build` 和 `AtomUI.City.Templates` 可以依赖 Roslyn、模板引擎和 NuGet SDK。
- `AtomUI.City.Cli` 可以依赖 CLI parser 和 terminal UI 库。
- Rx、ReactiveUI、DynamicData、OpenTelemetry 等能力优先放入可选扩展包。

## 3. 推荐依赖表

| 开源库 / 生态 | 建议放入 | 依赖级别 | 原因 | 主要约束 |
|---|---|---:|---|---|
| [Microsoft.Extensions.Hosting / DependencyInjection / Configuration / Options / Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview) | `AtomUI.City.Core` | 必选 | 作为 Host、DI、配置、Options、日志和生命周期的基础设施，符合 .NET 应用框架习惯。 | Core 只暴露 AtomUI.City 自己的生命周期语义，不把所有 Microsoft.Extensions 类型直接扩散为框架概念。 |
| [Avalonia](https://docs.avaloniaui.net/docs/welcome) | `AtomUI.City.Presentation` | 必选 | AtomUI.City 面向 Avalonia 应用，Presentation 层需要 View、Dispatcher、资源和应用生命周期集成。 | 不进入 Core；所有 UI 依赖必须隔离在 Presentation 或更上层。 |
| [AtomUI](https://github.com/AtomUI/AtomUI) | `AtomUI.City.Presentation` | 必选 | AtomUI 承担控件、主题、视觉系统和基础样式能力，AtomUI.City 不重造 UI 控件库。 | AtomUI.City 只做应用框架层，不把业务无关框架能力写成控件库能力。 |
| [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/overview) | `AtomUI.City.Mvvm` | 必选 | 提供 `ObservableObject`、`ObservableValidator`、`IRelayCommand`、`IAsyncRelayCommand` 和 Source Generator，减少 ViewModel 样板代码。 | 不进入 Core；不使用 `WeakReferenceMessenger` 作为框架 EventBus 底层。 |
| [Microsoft.Extensions.Http](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory) | `AtomUI.City.Data` | 强推荐 | `IHttpClientFactory` 适合统一管理 HTTP client、DI、日志、配置和请求生命周期。 | Data 应提供自己的请求管线和错误模型，不直接暴露裸 HttpClient 作为唯一抽象。 |
| [Polly](https://www.pollydocs.org/) | `AtomUI.City.Data` | 强推荐 | Data 层需要 retry、timeout、circuit breaker、fallback 等 resilience 能力。 | 作为请求管线策略实现，不污染 ViewModel 或 Core API。 |
| [Refit](https://github.com/reactiveui/refit) | `AtomUI.City.Data.Refit` | 可选强推荐 | 适合把 REST API 声明为 C# interface，降低客户端代理实现成本。 | 不作为 Data Core 的唯一 client proxy 方案；需要保留手写 client 和生成式 client 空间。 |
| [FluentValidation](https://fluentvalidation.net/) | `AtomUI.City.Mvvm.Validation` / `AtomUI.City.Data` | 可选 | 适合复杂表单、DTO、命令参数和请求模型验证。 | 基础 ViewModel 验证优先使用 CommunityToolkit.Mvvm 的 `ObservableValidator`；FluentValidation 作为增强集成。 |
| [System.Reactive](https://github.com/dotnet/reactive) | `AtomUI.City.Reactive` | 可选适配 | 适合事件流、调度、异步组合和测试调度。 | 不把 `IObservable<T>` 作为 State、Routing、Command、EventBus 的主公共 API。 |
| [ReactiveUI](https://www.reactiveui.net/) | `AtomUI.City.ReactiveUI` | 可选适配 | 方便已有 ReactiveUI 应用迁移，并提供 activation、command、interaction 等互操作能力。 | 不作为默认 ViewModel 基类；不让 ReactiveUI RoutingState 成为 AtomUI.City 路由模型。 |
| [DynamicData](https://github.com/reactivemarbles/DynamicData) | `AtomUI.City.State.DynamicData` | 可选适配 | 适合复杂集合状态的过滤、排序、分组、增量更新和派生集合。 | 依赖 Rx，不能进入 State Core；应作为高级集合状态扩展。 |
| [Scrutor](https://github.com/khellang/Scrutor) | Core 内部实现或 `AtomUI.City.Modularity` 内部能力 | 谨慎 | 可以补足 Microsoft.Extensions.DependencyInjection 的 assembly scanning 和 decorator 能力。 | 反射扫描必须考虑 AOT、trimming 和启动性能；模块注册优先支持显式注册。 |
| [Microsoft.CodeAnalysis](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/) | `AtomUI.City.Build` / Generator | 强推荐 | 路由清单、模块清单、模板诊断、代码约定检查、source generator 和 analyzer 都需要 Roslyn 能力。 | 只进入 Build/Generator 相关包，不进入应用运行时主链路。 |
| [Scriban](https://github.com/scriban/scriban) | `AtomUI.City.Templates` / `AtomUI.City.Cli` | 强推荐 | 适合作为项目、模块、页面、插件、测试模板的文本模板引擎。 | 模板语法必须保持稳定；模板输出要经过测试验证。 |
| [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) | `AtomUI.City.Cli` | 推荐 | 适合构建命令、参数、帮助文本和 CLI command model。 | CLI 命令模型不应泄漏到 Core。 |
| [Spectre.Console](https://spectreconsole.net/) | `AtomUI.City.Cli` | 推荐 | 适合输出表格、树、进度、诊断信息和交互式提示，提高 CLI 可用性。 | 只负责终端体验，不负责业务逻辑和构建逻辑。 |
| [NuGet.Protocol](https://learn.microsoft.com/en-us/nuget/reference/nuget-client-sdk) | `AtomUI.City.PluginSystem` / `AtomUI.City.Build` / `AtomUI.City.Cli` | 推荐 | 插件包、模板包、模块包、feed 查询和包元数据读取需要 NuGet Client SDK。 | 插件加载和包下载必须有安全边界、版本策略和缓存策略。 |
| [OpenTelemetry](https://opentelemetry.io/docs/) | `AtomUI.City.Diagnostics` 或可选集成 | 可选 | Host、Routing、Data、EventBus、PluginSystem 都需要 tracing、metrics、logs 等可观测性扩展点。 | 不作为 v1 Core 必选依赖；先保留诊断抽象，再提供 OTel 适配。 |
| [xUnit.net](https://xunit.net/) / [NSubstitute](https://nsubstitute.github.io/) / [Shouldly](https://docs.shouldly.org/) | `AtomUI.City.Testing` 和测试项目 | 推荐 | 分别承担测试框架、替身对象和可读断言，适合框架级行为测试。 | 这些依赖只进入测试包和测试项目，不进入运行时包。 |

## 4. 第一版硬依赖建议

第一版可以进入主依赖链的库：

- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Http`
- `Avalonia`
- `AtomUI`
- `CommunityToolkit.Mvvm`
- `Microsoft.CodeAnalysis`
- `Scriban`
- `System.CommandLine`
- `Spectre.Console`
- `xUnit.net`
- `NSubstitute`
- `Shouldly`

这些依赖分别进入对应包，不应该全部集中到 Core。

## 5. 第一版可选适配建议

以下依赖不进入 v1 核心依赖链：

- `System.Reactive`
- `ReactiveUI`
- `DynamicData`
- `Refit`
- `FluentValidation`
- `OpenTelemetry`
- `Scrutor`
- `NuGet.Protocol`

它们可以通过独立适配包或内部实现逐步引入。

建议的适配包命名：

- `AtomUI.City.Reactive`
- `AtomUI.City.ReactiveUI`
- `AtomUI.City.State.DynamicData`
- `AtomUI.City.Data.Refit`
- `AtomUI.City.Validation.FluentValidation`
- `AtomUI.City.Diagnostics.OpenTelemetry`

是否创建这些包，应由实际功能落地需求驱动，不提前拆包。

## 6. 不建议进入 Core 的依赖

以下依赖不应进入 `AtomUI.City.Core`：

- `Avalonia`
- `AtomUI`
- `CommunityToolkit.Mvvm`
- `System.Reactive`
- `ReactiveUI`
- `DynamicData`
- `Refit`
- `FluentValidation`
- `Spectre.Console`
- `Microsoft.CodeAnalysis`
- `NuGet.Protocol`

Core 的职责是 Host、Module、Lifecycle、DI、Configuration、ApplicationContext 和框架级错误处理。任何和 UI、MVVM、HTTP client、CLI、Build、Template、Testing 强相关的依赖都应该下沉到对应包。

## 7. 维护规则

新增依赖前必须回答以下问题：

- 这个依赖属于哪个 AtomUI.City 包？
- 是否会进入 Core 依赖链？
- 是否会影响 AOT、trimming、启动性能或包体积？
- 是否会把第三方库的编程模型泄漏为 AtomUI.City 的公共 API？
- 是否可以通过 adapter 包隔离？
- 是否已有 .NET 官方或 AtomUI/Avalonia 生态内置方案？
- 是否需要在 `Directory.Packages.props` 中集中管理版本？
- 是否需要补测试基础设施或模板覆盖？

默认策略：

- Core 依赖从严。
- Presentation、Mvvm、Data 按职责引入。
- Build、Cli、Templates、Testing 可以更务实。
- Rx、ReactiveUI、DynamicData、OpenTelemetry 走适配包。
- 第三方库可以作为实现细节，但不要轻易成为 AtomUI.City 的编程范式。
