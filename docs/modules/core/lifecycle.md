# AtomUI.City.Core 生命周期详细设计

版本：v0.1
状态：初版草案
适用范围：`AtomUI.City.Core` 中 Lifecycle Kernel、Scope、Pipeline、Lease、错误策略和诊断设计

## 1. 目标

Core 生命周期系统负责提供 AtomUI.City 的运行时骨架。它不直接处理 UI、MVVM、路由、状态或插件的具体业务逻辑，但必须提供统一抽象，让这些模块可以挂入同一套生命周期树。

Core Lifecycle 的目标：

- 建立统一的 Scope Tree。
- 管理父子 Scope 的取消和释放。
- 提供生命周期中间件机制。
- 提供 Contribution Lease 机制。
- 提供阶段状态和错误策略。
- 提供生命周期诊断上下文。
- 为 PluginSystem 的运行时装载和卸载提供基础设施。
- 区分 Application/Module/Plugin 组成关系、Contribution 能力关系和 Scope 生命周期关系。

## 2. Core 边界

Core 生命周期系统负责抽象和基础机制，不依赖：

- AtomUI / Avalonia
- CommunityToolkit.Mvvm
- ReactiveUI
- System.Reactive
- Microsoft.CodeAnalysis
- Spectre.Console

Core 可以定义通用抽象，例如：

- Lifecycle scope。
- Lifecycle context。
- Lifecycle middleware。
- Lifecycle event。
- Contribution lease。
- Lifecycle diagnostic record。
- Error policy。
- Cancellation and disposal coordinator。
- Threading 和调度抽象。

具体 ViewModel Activation、Route Navigation、State Reaction、Plugin AssemblyLoadContext 由对应模块实现并接入 Core 生命周期系统。

## 3. Scope

Scope 是生命周期系统的基本单元，只表示运行实例的生命周期边界。

每个 Scope 表示一段有明确开始、运行、停止和释放边界的运行时范围。

Module 和 Plugin 不命名为 `ModuleScope` 或 `PluginScope`。Module 和 Plugin 是 Application 的组成部分和能力贡献方；它们贡献的 route、permission、resource 等能力通过 Contribution 和 ContributionLease 进入运行时 registry。

Scope 应包含：

| 属性 | 职责 |
|---|---|
| Id | 全局唯一标识，用于诊断和追踪。 |
| Kind | Scope 类型，例如 Application、Presentation、Window、Navigation、Route、Activation、Operation。 |
| Parent | 父 Scope。 |
| Children | 子 Scope 集合。 |
| Services | 当前 DI scope 或 service provider。 |
| CancellationToken | 当前 Scope 的取消令牌。 |
| Disposables | 当前 Scope 持有的可释放资源。 |
| Contributions | 当前 Scope 持有的贡献 lease。 |
| Diagnostics | 当前诊断上下文。 |
| State | 当前生命周期状态。 |

Scope 的关键约束：

- 子 Scope 不能比父 Scope 活得更久。
- 父 Scope 停止时，子 Scope 必须先停止。
- Scope 释放顺序必须和创建顺序相反。
- Scope 停止时必须取消自己的 CancellationToken。
- Scope 停止后不能再接受新的子 Scope 或 Contribution。
- Scope 停止后不能再接受新的后台任务或 Operation。

## 4. Scope 类型

Core 应定义通用 Scope 类型枚举或等价概念。

| Scope | 生命周期边界 |
|---|---|
| `HostScope` | Host 创建到 Host 释放。 |
| `ApplicationScope` | 应用启动到应用关闭。 |
| `PresentationScope` | Presentation 运行时启动到停止。 |
| `WindowScope` | 桌面窗口创建到关闭。 |
| `NavigationScope` | 独立导航上下文创建到释放，拥有 Router、当前导航状态、Journal 和导航并发边界。 |
| `RouteScope` | 路由进入到路由离开。 |
| `ActivationScope` | ViewModel 激活到停用。 |
| `StateScope` | 状态作用域创建到释放。 |
| `OperationScope` | Command/Data 请求开始到完成、失败或取消。 |
| `SubscriptionScope` | 订阅创建到解除。 |

Core 只定义通用生命周期行为，不直接创建所有具体 Scope。具体模块在需要时创建对应 Scope。

## 5. Scope 状态机

建议 Scope 状态：

```text
Created
-> Starting
-> Running
-> Stopping
-> Stopped
-> Disposing
-> Disposed
```

错误状态：

```text
Faulted
CancelRequested
UnloadPending
```

状态规则：

- `Created` 只能进入 `Starting` 或 `Disposing`。
- `Starting` 成功后进入 `Running`。
- `Running` 可以进入 `Stopping` 或 `Faulted`。
- `Stopping` 必须先停止子 Scope，再释放本 Scope 资源。
- `Disposed` 是终态。
- `UnloadPending` 主要用于插件卸载状态，表示已请求卸载但运行时仍有引用阻止完成。插件不是公共 Scope 类型，但 PluginSystem 可以在内部使用生命周期机制追踪卸载状态。

## 6. 生命周期上下文

每次生命周期管线执行时，都应创建上下文对象。

Lifecycle Context 应包含：

- 当前 Scope。
- 当前阶段。
- 当前 CancellationToken。
- 当前服务访问入口。
- 当前诊断上下文。
- 当前错误策略。
- 可写入的阶段结果。
- 可附加的用户数据。

生命周期中间件和框架默认处理器都通过 Context 通信。

## 7. Middleware Pipeline

Core 提供生命周期中间件管线，而不是只提供事件。

中间件职责：

- 在阶段前后执行逻辑。
- 短路当前阶段。
- 修改阶段输入。
- 包裹框架默认处理。
- 捕获并转换错误。
- 写入诊断信息。

管线形态：

```text
LifecycleContext
-> Middleware 1
-> Middleware 2
-> Framework Handler
-> Middleware N
```

建议 Core 支持按阶段注册中间件：

| 阶段 | 示例 |
|---|---|
| Application | Start、Suspend、Resume、Stop。 |
| Module | Initialize、Start、Stop。 |
| Plugin | Load、Activate、Deactivate、Unload。 |
| Route | Navigate、Enter、Leave。 |
| Activation | Activate、Deactivate。 |
| Operation | Execute、Cancel、Fail。 |
| Error | Handle。 |

Core 不直接实现所有阶段的业务处理，但要允许模块把自己的阶段接入统一管线。

## 8. Events 与 Middleware 的关系

事件用于观察，Middleware 用于参与流程。

事件特点：

- 不能改变核心流程。
- 不应影响框架状态机。
- 适合日志、指标、调试和外部通知。

Middleware 特点：

- 可以改变流程。
- 可以短路流程。
- 可以参与错误处理。
- 可以包装默认框架逻辑。

生命周期扩展点应优先提供 Middleware，再提供只读事件。

## 9. Contribution Lease

Contribution Lease 表示 Module 或 Plugin 向 Host 贡献能力后得到的可撤销句柄。

常见贡献：

- 路由。
- 菜单。
- 资源。
- 本地化资源。
- 权限。
- EventBus 订阅。
- Command。
- Presentation 资源。
- 数据客户端。

Lease 必须满足：

- 可追踪贡献的 Module 或 Plugin。
- 可撤销。
- 可诊断。
- 可按创建顺序反向撤销。
- 撤销失败时进入错误策略。

插件系统必须使用 Lease 贡献所有运行时能力。插件卸载时，所有 Lease 必须先撤销，再释放插件服务和加载上下文。

## 10. 取消策略

Scope 创建时应创建与父 Scope 关联的 CancellationToken。

取消传播规则：

- 父 Scope 取消会级联取消子 Scope。
- OperationScope 必须响应取消。
- Command/Data 请求必须接收 OperationScope token。
- 插件卸载时必须先取消由该插件 Contribution 创建的 OperationScope。
- RouteScope 离开时必须取消页面级 OperationScope 和 ActivationScope。
- 后台任务必须绑定 Scope token，不能脱离生命周期独立运行。

取消不是错误。取消结果应与失败结果区分，便于 UI 和日志正确表达。

## 11. 释放策略

Scope 释放顺序：

```text
Stop accepting new children
-> Cancel token
-> Stop child scopes in reverse order
-> Cancel and drain managed background tasks
-> Revoke contribution leases in reverse order
-> Dispose subscriptions
-> Dispose registered resources
-> Dispose service scope
-> Mark disposed
```

释放规则：

- 释放必须幂等。
- 支持同步和异步释放。
- 释放失败必须进入错误策略。
- 已释放 Scope 不允许再次激活。
- 释放期间不允许新建子 Scope。
- 释放期间不允许新建 Operation 或后台任务。

## 12. 错误策略

Core 需要提供默认错误策略，同时允许上层模块替换或扩展。

| 错误位置 | 默认处理 |
|---|---|
| Host 创建 | Fatal。 |
| Application 启动 | Fatal。 |
| Core Module 初始化 | Fatal。 |
| 普通 Module 初始化 | 默认 Fatal，可配置降级。 |
| Plugin 加载 | Non-fatal，禁用插件并记录诊断。 |
| Plugin 卸载 | 进入 UnloadPending，阻止更新或删除。 |
| Route 导航 | Navigation failed。 |
| ViewModel 激活 | 释放当前 ActivationScope，导航失败。 |
| Operation 执行 | Operation failed。 |
| Lease 撤销 | 记录错误，继续撤销剩余 Lease，最后汇总。 |

错误处理必须记录：

- Scope Id。
- Scope Kind。
- 生命周期阶段。
- 错误类型。
- 是否已触发取消。
- 已释放资源数量。
- 未释放资源数量。
- 后续建议。

## 13. PluginSystem 集成要求

Core 生命周期系统必须为 PluginSystem 提供基础设施。

PluginSystem 需要：

- 插件加载阶段管线。
- 插件启用阶段管线。
- 插件停用阶段管线。
- 插件卸载阶段管线。
- Contribution Lease。
- 插件 Operation 取消。
- 插件服务 Scope 释放。
- 插件卸载诊断。
- 插件 Contribution 与活动 RouteScope / OperationScope 的反查机制。

插件卸载顺序：

```text
Mark unloading
-> Stop new route activation and operations
-> Deactivate plugin routes and view models
-> Cancel plugin operations
-> Revoke plugin contributions
-> Dispose plugin subscriptions and resources
-> Dispose plugin service scope
-> Request AssemblyLoadContext unload
-> Verify unload
-> Mark unloaded or UnloadPending
```

Core 不直接依赖 `AssemblyLoadContext`，但必须提供足够的生命周期机制，让 PluginSystem 可以安全接入 .NET 可卸载程序集模型。

底层运行时机制参考：[.NET 依赖项加载参考](../../reference/dotnet/dependency-loading.md)。

## 14. 诊断

Core 生命周期诊断应覆盖：

- Scope 创建和释放。
- 生命周期阶段开始和结束。
- 阶段耗时。
- Middleware 执行顺序。
- 取消来源。
- 错误来源。
- Lease 创建和撤销。
- 资源释放数量。
- 插件卸载状态。
- 后台任务和 Operation 取消状态。

线程模型、UI Dispatcher 抽象、后台任务和调度策略见：[Threading 设计](threading.md)。

诊断信息既要服务开发调试，也要服务桌面软件现场排查。

## 15. 测试要求

生命周期系统必须可测试。

Testing 包后续应支持：

- 创建 TestHost。
- 创建 TestScope。
- 驱动 Scope 启动和停止。
- 断言释放顺序。
- 断言 CancellationToken 被触发。
- 断言 Lease 被撤销。
- 断言 Middleware 执行顺序。
- 断言错误策略结果。
- 模拟插件卸载失败。

## 16. 开发者约束

生命周期系统对开发者形成以下约束：

- 不在构造函数中启动长期任务。
- 不在构造函数中订阅长期事件。
- 长期资源必须进入 Scope。
- 页面级数据请求必须绑定 RouteScope 或 ActivationScope。
- Command/Data 执行必须绑定 OperationScope。
- State Reaction 必须绑定 StateScope 或 ActivationScope。
- 模块和插件贡献能力必须通过 Lease。
- 后台任务必须通过 Host 管理的调度入口创建。
- 插件代码不得把插件类型对象保存在全局静态结构中。

这些约束是 AtomUI.City 编程范式的一部分。
