# AtomUI.City 包边界

版本：v0.1
状态：正式初版
适用范围：AtomUI.City 源码项目、NuGet 包、模块职责和依赖方向

## 1. 目标

包边界用于约束 AtomUI.City 的实现结构，避免所有能力集中到单个运行时包，也避免过早拆分导致使用成本过高。

第一版包结构遵循：

- Core 保持薄内核。
- UI 依赖隔离到 Presentation。
- MVVM、State、Routing、Data、Security、EventBus 各自承担业务无关的框架能力。
- Build、Cli、Templates、Testing 支撑完整研发生命周期。
- 可选生态能力通过适配包进入，不污染主公共 API。

## 2. 包清单

| 包 | 职责 |
|---|---|
| `AtomUI.City.Core` | Host、DI、配置、模块、生命周期基础、应用上下文、调度抽象、全局错误处理。 |
| `AtomUI.City.Mvvm` | ViewModel、命令、Activation、Interaction、验证、CommunityToolkit.Mvvm 集成。 |
| `AtomUI.City.State` | 状态值、可写状态、计算状态、Reaction、StateScope、Snapshot、集合状态。 |
| `AtomUI.City.Routing` | 路由定义、导航、守卫、解析器、路由生命周期、Route 到 ViewModel Target 的映射。 |
| `AtomUI.City.Data` | 数据请求、客户端代理、请求管线、缓存、错误模型、认证集成。 |
| `AtomUI.City.Security` | 认证状态、权限检查、授权策略、路由和命令权限联动。 |
| `AtomUI.City.EventBus` | 类型事件总线、作用域订阅、事件通道、线程调度、错误策略。 |
| `AtomUI.City.Localization` | 本地化资源、文化切换、文本刷新、模块化资源注册。 |
| `AtomUI.City.Presentation` | AtomUI/Avalonia 集成、ViewLocator、UI Dispatcher、Activation 接入、Interaction Handler。 |
| `AtomUI.City.PluginSystem` | 插件发现、插件元数据、插件加载、插件模块注册、插件生命周期。 |
| `AtomUI.City.Build` | 构建约定、资源生成、模块清单、路由清单、输出组织。 |
| `AtomUI.City.Cli` | 项目创建、模块生成、路由生成、构建命令、模板调用。 |
| `AtomUI.City.Templates` | 应用模板、模块模板、页面模板、插件模板、测试模板。 |
| `AtomUI.City.Testing` | 测试 Host、测试 Dispatcher、生命周期驱动、状态/路由/EventBus 测试工具。 |

## 3. 依赖方向

推荐依赖方向：

```text
Application
-> Core
-> Mvvm / State / Routing / Data / Security / EventBus / Localization / PluginSystem
-> Presentation
-> AtomUI / Avalonia
```

工程层依赖方向：

```text
Cli -> Templates / Build
Build -> Core metadata / manifests / generators
Testing -> Core and selected framework packages
```

## 4. Core 边界

`AtomUI.City.Core` 可以包含以下命名空间：

- `AtomUI.City.Hosting`
- `AtomUI.City.Lifecycle`
- `AtomUI.City.Modularity`
- `AtomUI.City.Configuration`
- `AtomUI.City.DependencyInjection`
- `AtomUI.City.Threading`
- `AtomUI.City.Diagnostics`

Core 不应依赖：

- AtomUI / Avalonia
- CommunityToolkit.Mvvm
- ReactiveUI
- System.Reactive
- Microsoft.CodeAnalysis
- Spectre.Console
- 任何具体 HTTP client proxy 框架

## 5. 拆包规则

新增包前必须满足至少一个条件：

- 会引入不适合进入现有包的重量级依赖。
- 需要隔离可选生态适配。
- 有独立发布和版本兼容需求。
- 会形成清晰的用户安装边界。

不满足以上条件时，优先放入已有包的命名空间中。
