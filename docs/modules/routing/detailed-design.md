# AtomUI.City.Routing Detailed Design

版本：v0.1
状态：正式初版
适用范围：路由图、导航事务、NavigationScope、RouteScope、Guard、Resolver、Journal、插件路由、线程模型、诊断和测试。

## 1. 定位

`AtomUI.City.Routing` 是应用页面进入模型的核心模块。

Routing 不只是路径到页面的映射，也不是简单 ViewModel 栈。它负责把应用的页面结构、导航状态、生命周期、权限守卫、数据解析、插件贡献和历史记录统一到一套可测试、可回滚、可诊断的运行时模型中。

Routing 在框架中的位置：

```text
Route Graph
-> Navigation Transaction
-> RouteScope
-> ViewModel Target
-> Mvvm Activation
-> Presentation Outlet Commit
```

Routing 只选择要进入的 ViewModel Target，不负责创建具体 UI 控件。ViewModel 到 View 的定位、View 创建、Outlet 控件承接和 AtomUI/Avalonia 线程适配由 `AtomUI.City.Presentation` 负责。

## 2. 非目标

Routing 不负责：

- AtomUI 控件实现。
- Avalonia visual tree 操作。
- ViewLocator 具体实现。
- HTTP client。
- 认证状态存储。
- 权限策略持久化。
- 业务工作台、文档区、仪表盘等应用模型。
- 文件系统路由。
- Web endpoint routing。
- ASP.NET Core MVC model binding。

这些能力由 Presentation、Data、Security 或业务应用自己实现。

## 3. 设计原则

Routing 必须遵守：

- ViewModel-first：导航目标是 ViewModel Target，不是 View 类型。
- Lifecycle-first：每个活动路由都有明确 RouteScope。
- Transactional navigation：导航提交前不破坏当前页面。
- AOT-first：路由图、参数绑定和工厂描述由 Source Generator 生成。
- Plugin-aware：插件路由可以运行时加入和撤销。
- Thread-safe：同一个 NavigationScope 内导航串行化，UI 提交只在 UI Thread。
- Business-agnostic：框架不内置业务页面形态。
- Testable：路由匹配、守卫、解析、事务回滚和插件撤销都必须可测试。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `RouteId` | 路由稳定身份。 |
| `RouteDescriptor` | 编译期生成的路由定义。 |
| `RouteGraphSnapshot` | 不可变路由图快照。 |
| `RouteRegistry` | 接收模块和插件路由贡献，发布新快照。 |
| `RouteReference` | 强类型导航引用。 |
| `NavigationTarget` | 一次导航的规范化目标。 |
| `NavigationScope` | 独立 Router、状态、Journal 和并发边界。 |
| `IRouter` | 当前 NavigationScope 的导航入口。 |
| `NavigationTransaction` | 一次导航准备、提交、回滚过程。 |
| `NavigationSnapshot` | 当前活动路由树快照。 |
| `RouteScope` | 活动路由节点生命周期边界。 |
| `IRouteContext` | 当前路由参数、解析数据、父链和诊断上下文。 |
| `NavigationResult` | 导航成功、拒绝、取消、失败或重定向结果。 |

命名不额外添加 `City` 前缀。命名空间已经表达框架归属。

## 5. Route Graph

Route Graph 是应用可进入页面的静态和动态合成结果。

来源：

- 静态应用模块。
- 启动期模块贡献。
- 运行时插件贡献。
- Host 暴露的路由扩展点。

Route Graph 必须以快照方式发布：

```text
RouteContribution
-> RouteRegistry
-> Validate
-> Build immutable RouteGraphSnapshot
-> Publish snapshot
```

导航开始时捕获当前 `RouteGraphSnapshot`。同一次导航只使用该快照，即使导航过程中插件启用或停用，也不能改变本次匹配结果。

详细规则见：[route-graph.md](route-graph.md)。

## 6. NavigationScope

`NavigationScope` 是独立导航上下文。

它拥有：

- 当前 `IRouter`。
- 当前 `NavigationSnapshot`。
- 当前 `NavigationJournal`。
- 导航并发队列。
- 运行中 `NavigationTransaction`。
- 与 WindowScope 或父 NavigationScope 的生命周期关系。

普通嵌套 Outlet 不自动创建新的 NavigationScope。只有需要独立 Back/Forward、独立当前状态和独立导航并发控制时，才创建新的 NavigationScope。

示例：

```text
WindowScope
-> NavigationScope(root)
   -> primary outlet
      -> Layout RouteScope
         -> content outlet
            -> Page RouteScope
   -> side outlet
      -> Help RouteScope
```

Tab、子窗口、独立预览面板可以创建子 NavigationScope。

## 7. Navigation Transaction

导航必须是事务式流程。

推荐流程：

```text
Accept request
-> Normalize target
-> Capture RouteGraphSnapshot
-> Match route
-> Build navigation plan
-> Run match policies
-> Create provisional RouteScopes
-> Run enter guards
-> Confirm leaving routes
-> Resolve data
-> Create ViewModels and provisional ActivationScopes
-> Prepare Presentation change
-> Commit on UI thread
-> Update NavigationSnapshot and Journal
-> Dispose removed route branches
-> Complete
```

核心约束：

- 目标路由准备完成前，当前路由树保持可用。
- 候选 RouteScope 在提交前处于 provisional 状态。
- 候选 ActivationScope 可以在 Presentation binding 前创建，用于注册 binding 和 UI 订阅释放边界，但只有 Commit 成功后才进入 running / active 状态。
- 任意准备阶段失败时，释放候选分支并保留当前页面。
- Commit 是不可抢占边界。
- Commit 失败必须尝试恢复原 Outlet 状态。
- 导航取消不是错误。

详细规则见：[navigation.md](navigation.md)。

## 8. Route Lifecycle

活动路由生命周期：

```text
Matched
-> ProvisionalScopeCreated
-> Guarded
-> Resolved
-> ViewModelCreated
-> Committed
-> Activated
-> Deactivating
-> Disposed
```

`RouteScope` 负责：

- 路由参数。
- 解析数据。
- 页面级服务作用域。
- 页面级 Operation。
- 当前 Contribution 信息。
- CancellationToken。
- 诊断上下文。
- 可释放资源集合。

RouteScope 离开时必须先拒绝新 Operation，再取消 token，再停用 ActivationScope 和子 RouteScope，最后释放服务作用域。

## 9. Guard、Resolver 和 Middleware

Guard 负责决策，Resolver 负责数据准备，Middleware 负责包裹导航流程。

| 能力 | 职责 |
|---|---|
| Match Policy | 决定候选路由是否参与匹配。 |
| Enter Guard | 决定是否允许进入候选路由。 |
| Leave Guard | 决定是否允许离开当前路由。 |
| Resolver | 在激活前准备必需数据。 |
| Navigation Middleware | 参与导航阶段、记录诊断、转换错误。 |

Guard、Resolver 和 Middleware 必须由 Source Generator 写入 descriptor，运行时不扫描程序集。

详细规则见：

- [guards.md](guards.md)
- [resolvers.md](resolvers.md)

## 10. ViewModel Target

Routing 只输出 ViewModel Target。

```text
RouteDescriptor
-> ViewModelTargetDescriptor
-> ViewModel instance
-> Presentation View binding
-> Commit
-> Mvvm Activation
```

ViewModel Target 描述：

- ViewModel 类型。
- 服务来源。
- 工厂策略。
- 参数注入规则。
- Resolver 数据注入规则。
- 插件归属。

Presentation 再根据 ViewModel 找 View。Routing 不做 ViewLocator。

详细规则见：[viewmodel-target.md](viewmodel-target.md)。

## 11. Journal 和 Reuse

每个 NavigationScope 拥有独立 Journal。

Journal 支持：

- Navigate。
- Replace。
- Reset。
- Back。
- Forward。

Journal 只保存可序列化导航状态，不保存 ViewModel、ServiceProvider、委托或插件私有类型实例。

Route Reuse 分为：

- 共同父路由保留。
- 显式 KeepAlive 分支缓存。

默认只保留共同父路由，已离开的分支默认释放。

详细规则见：[journal-and-reuse.md](journal-and-reuse.md)。

## 12. Plugin 路由

插件通过 Route Contribution 加入 RouteRegistry。

插件路由必须：

- 记录 PluginId、ModuleId 和 ContributionLease。
- 只能挂载到 Host 开放的 RouteExtensionPoint。
- 使用插件自己的 ServiceProvider。
- 可被 Host 在插件停用时反查和关闭。
- 不把插件私有类型泄漏到 Host 静态缓存。

插件停用时 Routing 必须阻止新导航、取消运行中导航、关闭活动路由、清理 Journal 和缓存，再撤销 ContributionLease。

详细规则见：[plugins.md](plugins.md)。

## 13. Threading

同一个 NavigationScope 内导航必须串行化。

默认并发策略：

- Commit 前，新导航可以取消旧导航。
- Commit 中，新导航必须等待。
- Commit 后，新导航进入下一次事务。
- 不同 NavigationScope 可以并行导航。

线程规则：

- 路由匹配和参数解析可以在后台执行。
- Resolver 默认在后台执行，不访问 UI。
- ViewModel 创建和激活遵守 Mvvm 线程策略。
- Outlet 提交必须通过 `IUiDispatcher`。
- 当前导航状态更新必须原子化。

## 14. AOT 和 Source Generator

Source Generator 必须生成：

- Route Manifest。
- RouteDescriptor。
- RouteGraph 构建代码。
- Path parser 和 formatter。
- 参数 binder。
- Guard / Resolver / Middleware descriptor。
- ViewModelTargetDescriptor。
- 插件 RouteContributionDescriptor。
- 诊断。

运行时禁止：

- 扫描程序集发现路由。
- 反射读取 Attribute 建图。
- 反射推断 ViewModel 构造函数。
- 命名约定猜测 ViewModel 或 View。
- 动态代理作为默认路径。

## 15. 错误和诊断

NavigationResult 必须区分：

- Success。
- Rejected。
- Redirected。
- Cancelled。
- Failed。
- NotFound。
- StaleRouteGraph。
- ContributionRevoked。

诊断必须记录：

- Navigation id。
- NavigationScope id。
- RouteGraph version。
- Target。
- Matched route chain。
- Guard / Resolver 耗时。
- RouteScope 创建和释放。
- Commit 耗时。
- 插件 Contribution 信息。
- 失败阶段和错误策略。

详细规则见：[diagnostics-and-testing.md](diagnostics-and-testing.md)。

## 16. 测试策略

Testing 包应提供：

- TestRouteGraphBuilder。
- TestRouter。
- FakeNavigationScope。
- FakeUiDispatcher。
- Guard / Resolver 测试工具。
- Journal 断言。
- Plugin route contribution 测试工具。
- NavigationTransaction 录制器。

Routing 测试不应依赖真实 AtomUI/Avalonia UI。

## 17. 第一版取舍

第一版不做：

- 文件系统路由。
- Web endpoint routing。
- Controller / Action token。
- Server Action。
- 任意运行时 CLR 路由定义。
- UI region framework。
- Workbench / Documents / Dashboard 模型。
- 跨进程插件安全沙箱。

第一版优先稳定：

- Route definition syntax。
- RouteGraphSnapshot。
- NavigationScope。
- NavigationTransaction。
- RouteScope。
- Plugin route contribution。
- AOT/source generator 契约。
