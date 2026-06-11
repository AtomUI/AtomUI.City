# AtomUI.City.Presentation Detailed Design

版本：v0.1
状态：正式初版
适用范围：AtomUI/Avalonia 集成、UI Runtime、ViewLocator、View/ViewModel 绑定、Route Outlet、UI Dispatcher、Activation 接入、Interaction Handler、State/Localization/UI 更新、插件资源、AOT/source generator 和测试策略。

## 1. 定位

`AtomUI.City.Presentation` 是 AtomUI.City 框架运行时与 AtomUI/Avalonia UI 运行时之间的隔离层。

Presentation 不定义业务应用形态，不提供 Workbench、Documents、Dashboard 等业务模型。它只负责把框架已经确定的运行时对象安全、可诊断、可释放地接入 UI：

```text
Routing
-> ViewModel Target / ViewModel instance
-> Presentation ViewLocator
-> View/ViewModel binding
-> Route Outlet commit
-> AtomUI / Avalonia visual tree
```

## 1.1 拆分文档

Presentation 的详细设计按职责拆分维护：

| 文档 | 内容 |
|---|---|
| [ui-runtime.md](ui-runtime.md) | UI runtime、PresentationScope、WindowScope 和 AtomUI/Avalonia bridge。 |
| [dispatcher.md](dispatcher.md) | UI Dispatcher、UI thread access、投递、停止和异常。 |
| [view-locator.md](view-locator.md) | ViewModel 到 ViewDescriptor 的定位、View manifest 和插件 View 撤销。 |
| [view-binding.md](view-binding.md) | ViewFactory、DataContext、binding handle 和释放。 |
| [route-outlet.md](route-outlet.md) | Outlet commit plan、attach/detach/replace 和失败回滚。 |
| [activation-integration.md](activation-integration.md) | Visual lifecycle、ActivationScope、attached/detached 和 close intent。 |
| [interaction-and-validation.md](interaction-and-validation.md) | Interaction handler、Validation visual state 和 Command binding。 |
| [state-and-localization.md](state-and-localization.md) | State UI 更新、culture refresh 和本地化 binding 刷新。 |
| [resources-and-plugins.md](resources-and-plugins.md) | AtomUI/Avalonia 资源、主题、插件 UI contribution 和撤销。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | Presentation 诊断、fake runtime、平台集成测试和测试矩阵。 |

边界必须明确：

- Routing 负责 `Route -> ViewModel Target`。
- Mvvm 负责 ViewModel Activation、Command、Interaction、Validation contract。
- Presentation 负责 `ViewModel -> View`、UI Dispatcher、Outlet 提交和 UI 运行时桥接。
- AtomUI/Avalonia 负责控件、样式、主题和底层 UI 行为。

### 1.1 模块数据流

Presentation 处在页面进入链路的 UI 提交段。完整数据流必须闭合到 VisualTree 反馈和 UI 元素状态反馈：

```text
User / Command / Startup
-> Routing NavigationRequest
-> RouteGraph match
-> Guards / Resolvers
-> ViewModel Target
-> provisional RouteScope + ActivationScope
-> ViewModel instance
-> Presentation ViewLocator
-> View creation + binding
-> Outlet commit
-> AtomUI/Avalonia VisualTree
-> Presentation feedback
-> Routing / Mvvm / Lifecycle / Diagnostics
```

模块之间只交换框架 contract，不传播 AtomUI/Avalonia 原始事件：

| From | To | 传递内容 | 说明 |
|---|---|---|---|
| Routing | Mvvm | `ViewModelTarget`、Route 参数、Resolved data、RouteScope | Routing 只决定进入哪个 ViewModel，不知道 View。 |
| Mvvm | Presentation | ViewModel instance、ActivationScope、Command/Interaction/Validation contract | Mvvm 提供可绑定对象和生命周期边界。 |
| Routing | Presentation | Outlet commit plan、NavigationTransactionId、Reuse/KeepAlive 指令、Contribution 信息 | Presentation 执行 UI 提交，不解释路由图。 |
| Presentation | AtomUI/Avalonia | View instance、DataContext、Styles、Resources、Outlet content | 进入真实 visual tree。 |
| AtomUI/Avalonia | Presentation | Attached/Detached、Loaded/Unloaded、UI events、Dispatcher signal | 原始 UI 事件只进入 Presentation。 |
| Presentation | Routing | CommitResult、失败阶段、创建/复用/释放诊断 | Routing 据此更新 NavigationSnapshot、Journal 或回滚。 |
| Presentation | Mvvm | visual activation/deactivation、Interaction result、validation display feedback | 不反向控制业务，只反馈 UI 生命周期和交互结果。 |
| Presentation | Core Lifecycle | Scope 诊断、UI runtime 状态、错误事件 | Core 只接收归一化后的生命周期和错误信息。 |

核心约束：

- Presentation 是 UI 提交者，不是导航决策者。
- AtomUI/Avalonia 是视觉和控件运行时，不知道 AtomUI.City 的路由、模块和插件语义。
- Mvvm 是 ViewModel 生命周期和交互契约层，不知道具体 View。
- Routing 是页面进入决策层，不创建或持有 UI 控件。

### 1.2 导航提交阶段

导航进入 UI 前分为 prepare、commit、activate 三段。

```text
Prepare phase:
Create provisional RouteScope / ActivationScope
Create ViewModel
Resolve ViewDescriptor
Create View
Bind View and ViewModel

Commit phase:
Apply Outlet commit plan
Attach / replace / detach views
Update AtomUI/Avalonia VisualTree

Activate phase:
Mark ActivationScope running
Activate ViewModel
Attach visual lifecycle adapter
Update NavigationSnapshot / Journal
```

ActivationScope 需要在 binding 前可用，因为 UI 事件订阅、binding disposable、Interaction handler 和 validation binding 都需要注册释放边界。但此时它仍属于候选路由分支，不能被视为已经 active。只有 Outlet commit 成功后，ViewModel 才进入 active 状态。

如果 commit 失败：

```text
Presentation commit failed
-> detach newly created view
-> dispose binding
-> dispose provisional ActivationScope
-> dispose provisional RouteScope
-> keep old outlet content
-> navigation failed with diagnostics
```

这保证旧 VisualTree 在新路由准备失败时仍保持可用。

### 1.3 VisualTree 反馈

VisualTree 变化必须通过 Presentation 归一化后反馈。Routing、Mvvm、Core 不直接订阅 AtomUI/Avalonia 原始 visual tree 事件。

反馈分为四类：

| 类型 | 反馈目标 | 说明 |
|---|---|---|
| Outlet commit 反馈 | Routing | 返回 UI 提交成功、失败阶段、创建/复用/释放信息。 |
| Visual lifecycle 反馈 | Mvvm Activation | 把 attached/detached/loaded/unloaded 转换成 visual activation state。 |
| Leave / close intent | Routing / Mvvm | 用户关闭窗口、关闭页面、系统退出时转换为 leave request 或 interaction。 |
| Diagnostics | Core diagnostics | 高频布局、测量、普通 resize、资源查找等默认只进入诊断。 |

Visual lifecycle 和 ViewModel active 状态不能混为一谈：

| 概念 | 来源 | 含义 |
|---|---|---|
| ActivationScope | Routing / Mvvm | ViewModel 逻辑上进入当前路由或激活上下文。 |
| VisualAttachmentState | Presentation / AtomUI/Avalonia | View 当前是否挂在 visual tree 或处于可见生命周期。 |

例如 KeepAlive、Tab 缓存、隐藏区域可能让 ViewModel 保持 active，但 View 暂时 detached 或不可见。Presentation 只能反馈 visual state，不应直接停用 ViewModel；是否停用由 Routing/Mvvm 的生命周期策略决定。

关闭类事件必须表达为意图，而不是直接释放对象：

```text
Window Closing / View close gesture
-> Presentation captures close intent
-> Routing LeaveGuard / Mvvm Interaction confirmation
-> if allowed: deactivate route or window
-> Presentation detaches VisualTree
```

### 1.4 UI 元素状态反馈

UI 元素状态变化通知 ViewModel，必须走 MVVM Binding / Command / Interaction 通道，而不是 VisualTree feedback。

```text
AtomUI/Avalonia control state
-> Binding / Command / Interaction
-> ViewModel property / command / interaction continuation
-> ViewModel state changes
-> Presentation updates UI
```

允许反馈给 ViewModel 的 UI 状态必须有业务语义：

| UI 变化 | 反馈方式 | 示例 |
|---|---|---|
| 控件值变化 | TwoWay binding | `TextBox.Text`、`CheckBox.IsChecked`、`ComboBox.SelectedItem`、`Slider.Value`。 |
| 用户动作 | Command binding | Button click、Menu item click、shortcut、double click、drag drop committed。 |
| UI 交互结果 | Interaction result | Dialog result、FilePicker result、Toast action、close confirmation。 |
| 验证触发 | Validation binding/update trigger | LostFocus、submit、explicit validation request。 |
| 语义选择状态 | Binding 或 Command | selected item、current item、checked item。 |

默认不反馈给 ViewModel 的 UI 状态：

- Hover。
- Pressed。
- Pointer move。
- Layout measure / arrange。
- Scroll offset。
- Theme resource lookup。
- Animation state。
- 普通 resize。

这些状态属于 View/Presentation，除非应用显式把它们建模成有业务语义的 ViewModel 属性或命令参数。

### 1.5 运行时闭环

常见闭环如下：

```text
View binding:
ViewModel instance
-> IViewLocator locate ViewDescriptor
-> IViewFactory create View on UI Thread
-> IViewBinder set DataContext
-> attach lifecycle adapter
-> register disposables into ActivationScope
-> return BoundViewHandle
```

```text
State update:
State change
-> scoped subscription / reaction
-> DispatchPolicy.UiThread
-> ViewModel property change or Presentation binding update
-> AtomUI/Avalonia visual refresh
```

```text
Plugin unloading:
Plugin stopping
-> block new plugin contributions
-> find active RouteScope by Contribution.PluginId
-> navigate away / close active UI
-> deactivate ViewModel
-> dispose ActivationScope
-> detach plugin views
-> revoke ViewDescriptor / resources / handlers
-> dispose plugin service scope
-> unload plugin assemblies
```

一句话边界：VisualTree 反馈生命周期和提交结果；UI 元素状态反馈业务语义。原始 UI 事件默认不跨越 Presentation 边界。

## 2. 非目标

Presentation 不负责：

- 业务布局模型。
- 工作台、文档区、仪表盘等具体应用结构。
- 路由图解释。
- ViewModel 创建策略。
- Data 请求。
- Security policy。
- State 核心实现。
- EventBus 核心实现。
- AtomUI 控件库实现。
- Avalonia 框架内部生命周期实现。

这些由 Routing、Mvvm、Data、Security、State、EventBus、AtomUI/Avalonia 或业务应用负责。

## 3. 设计原则

Presentation 必须遵守：

- UI-affine：所有 UI 对象访问必须发生在 UI Thread。
- ViewModel-first：以 ViewModel 作为 View 定位和绑定入口。
- Lifecycle-aware：View、Binding、Interaction handler 和 UI 订阅必须绑定 Scope。
- Transaction-aware：Route Outlet commit 必须支持成功、失败和回滚。
- AOT-first：View/ViewModel binding manifest 由 Source Generator 生成。
- Plugin-aware：插件 View 和资源必须可撤销、可释放、可卸载。
- Business-agnostic：不内置业务页面形态。
- Testable：支持无真实 UI 的 Presentation 测试替身。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `IPresentationRuntime` | 管理 UI runtime ready、shutdown 和诊断状态。 |
| `IUiDispatcher` | Core 定义的 UI Thread 调度抽象，Presentation 提供 Avalonia 实现。 |
| `IViewLocator` | 根据 ViewModel 定位 View descriptor。 |
| `IViewFactory` | 创建 View 实例。 |
| `IViewBinder` | 建立 View 和 ViewModel 的绑定。 |
| `IRouteOutlet` | Presentation 侧路由承载点。 |
| `IRouteOutletRegistry` | 当前 Window / NavigationScope 的 Outlet 注册表。 |
| `IPresentationCommitter` | 执行 Routing 的 Outlet commit plan。 |
| `IInteractionHandlerRegistry` | 注册和查找 Interaction handler。 |
| `IViewActivationAdapter` | 把 View 的 visual lifecycle 接入 ViewModel Activation。 |
| `IPresentationResourceRegistry` | 管理 View、样式、图标、模板和插件资源贡献。 |

命名不加 `City` 前缀。

## 5. UI Runtime

Presentation 负责启动和连接 AtomUI/Avalonia UI runtime。

职责：

- 初始化 Avalonia application bridge。
- 接入 AtomUI 资源和主题。
- 注册 Core `IUiDispatcher`。
- 报告 UI runtime ready。
- 创建 PresentationScope。
- 创建 WindowScope。
- 在 UI runtime 停止时拒绝新的 UI 投递。
- 输出 UI runtime 诊断。

启动关系：

```text
ApplicationHost
-> StartPresentation
-> Initialize AtomUI/Avalonia runtime
-> Register IUiDispatcher
-> Create PresentationScope
-> Open initial WindowScope
-> Routing navigates initial route
```

Core 不依赖 Avalonia。Presentation 是 Core 和 Avalonia 之间的适配层。

## 6. UI Dispatcher

Presentation 提供 `IUiDispatcher` 的 Avalonia 实现。

规则：

- `CheckAccess` 映射 Avalonia UI thread access。
- `InvokeAsync` 返回执行结果或异常。
- `PostAsync` 表示异步投递。
- UI runtime 未 ready 时，按 Host 策略等待或返回明确错误。
- UI runtime stopping 后拒绝新投递。
- Dispatcher callback 异常进入 ErrorPolicy。
- 插件不能长期静态保存 dispatcher callback。

调度策略见：[Core Threading 设计](../core/threading.md)。

## 7. ViewLocator

ViewLocator 负责 `ViewModel -> ViewDescriptor`。

第一版不依赖运行时命名约定扫描。

推荐声明：

```csharp
[ViewFor(typeof(SettingsViewModel))]
public sealed partial class SettingsView : UserControl
{
}
```

Source Generator 生成 View manifest：

```text
ViewModelType
-> ViewType
-> Contribution
-> Resource scope
-> Factory descriptor
```

规则：

- 一个 ViewModel 默认只能有一个默认 View。
- 多 View 场景必须显式命名，例如 `ViewKey`。
- ViewLocator 不创建 ViewModel。
- ViewLocator 不解释 Route。
- 插件 View 必须记录 PluginId 和 ContributionId。
- 插件卸载时必须撤销对应 View descriptor。

## 8. ViewFactory

ViewFactory 负责创建 View。

规则：

- View 创建必须在 UI Thread。
- View 可以从 Application 或 Plugin service context 创建。
- View 构造函数不应启动长期任务。
- View 不能持有插件服务到 Host 静态对象。
- 创建失败返回 Presentation commit failure。

Strict AOT 模式下，ViewFactory 应由 Source Generator 生成强类型工厂，避免反射构造。

## 9. View/ViewModel 绑定

Binding 过程：

```text
ViewModel instance
-> Locate ViewDescriptor
-> Create View
-> Set DataContext / binding context
-> Attach lifecycle adapter
-> Register view-side disposables in ActivationScope
```

规则：

- ViewModel 不知道 View 类型。
- View 不负责导航决策。
- Binding 必须可释放。
- ViewDataContext 变化必须受控，不能被外部任意覆盖。
- View 和 ViewModel 生命周期不完全等同，但必须有关联释放策略。
- RouteScope 是导航上层生命周期；UI 事件订阅、binding disposable 和 visual adapter 默认挂 ActivationScope，Route 离开时通过 ActivationScope 停用和释放。

Presentation 应提供诊断：

- 找不到 View。
- 找到多个默认 View。
- View 创建失败。
- Binding 失败。
- 插件 View descriptor 已撤销。

## 10. Route Outlet

Route Outlet 是 Routing 和 Presentation 的提交边界。

Routing 输出 Outlet commit plan，Presentation 执行：

```text
Commit plan
-> Find outlets
-> Create/bind new views
-> Attach or detach reused views
-> Update visual tree
-> Return commit result
```

`IRouteOutlet` 应支持：

- Outlet name。
- 当前 content。
- Attach。
- Detach。
- Replace。
- Clear。
- Commit diagnostics。

规则：

- 默认 Outlet 名为 `primary`。
- Outlet 名称稳定，不能运行时动态变更。
- 命名 Outlet 不自动创建新的 NavigationScope。
- Commit 必须在 UI Thread。
- Commit 失败时必须尽量恢复旧 content。
- Presentation 不决定导航成功，只返回 commit result。

## 11. Routing 集成

Routing 与 Presentation 通过明确 contract 交互。

Routing 提供：

- NavigationTransaction id。
- Outlet commit plan。
- ViewModel instance。
- RouteContext。
- Reuse / KeepAlive 指令。
- Contribution 信息。

Presentation 返回：

- Commit success。
- Commit failed。
- Failure stage。
- Created views。
- Attached / detached views。
- Disposal diagnostics。

Routing 根据结果更新 NavigationSnapshot、Journal 或执行回滚。

## 12. Activation 集成

Mvvm Activation 是 ViewModel 生命周期。Presentation 负责把 visual lifecycle 接入 Activation。

规则：

- ViewModel 激活由 Routing/Mvvm 驱动。
- View attached 后可以触发 visual activation adapter。
- View detached 时触发 visual deactivation adapter。
- ActivationScope 释放时释放 View 侧 binding、UI 事件订阅和 Interaction handler。
- View 重用时不能重复注册同一 handler。

Presentation 不应该在 View 构造阶段激活 ViewModel。

## 13. Interaction Handler

Presentation 负责把 MVVM Interaction Request 映射到 UI。

支持场景：

- 确认。
- 输入。
- 文件选择。
- Dialog。
- Toast / Notification。
- Window 选择。

规则：

- Handler 运行在 UI Thread。
- Handler 注册绑定 ActivationScope、WindowScope 或 ApplicationScope。
- ViewModel 停用时，未完成 Interaction 返回 Canceled。
- 插件停用时，插件 Interaction 返回 Canceled。
- Handler 缺失返回 NotHandled，并记录诊断。

Presentation 不把具体 Dialog 业务模型强加给应用。

## 14. Validation 集成

Mvvm 定义验证状态，Presentation 负责展示。

Presentation 需要支持：

- 读取 `ObservableValidator` 或框架验证状态。
- 把错误映射到 AtomUI/Avalonia validation visual state。
- Command 与验证状态变化后的 UI 刷新。
- 插件 View 的验证资源释放。

Validation failed 不是异常，不进入 fatal error。

## 15. Command Binding

Presentation 可以增强 Command Binding。

职责：

- 把 `IRelayCommand` / `IAsyncRelayCommand` 绑定到 UI command source。
- 监听 CanExecute 变化。
- 映射 busy / executing 状态。
- 与 Security、Routing 当前状态联动后的可执行性刷新。
- 释放 UI 事件订阅。

长耗时命令仍由 Mvvm / Core Operation 管理，Presentation 不执行后台任务调度。

## 16. State 和 UI 更新

State Core 不直接依赖 UI。Presentation 负责 UI 线程安全更新。

规则：

- UI 订阅使用 `DispatchPolicy.UiThread`。
- State 到 UI 的订阅必须绑定 Scope。
- View detached 后停止 UI 更新。
- 插件 View 相关订阅随插件停用释放。
- UI 更新异常进入 Presentation diagnostics。

Presentation 不保存应用状态；状态仍归 State 模块管理。

## 17. Localization 集成

Presentation 负责把 Localization 的文化变化反映到 UI。

职责：

- 注册本地化 binding adapter。
- 当前文化变化后刷新文本。
- 路由标题、命令文本、验证消息、错误消息刷新。
- 插件本地化资源撤销后更新或清理对应 UI。

Localization 负责资源查找和文化状态，Presentation 负责 UI 展示刷新。

## 18. Resource 和 Theme 集成

Presentation 接入 AtomUI/Avalonia 资源系统。

资源类型：

- Styles。
- Themes。
- Icons。
- Templates。
- Fonts。
- Images。
- Localization resource bridge。

插件资源必须通过 ContributionLease 进入 `IPresentationResourceRegistry`。

插件停用时：

```text
Stop new view creation from plugin
-> Detach active plugin views
-> Remove plugin resources
-> Clear resource cache
-> Dispose plugin resource scope
```

## 19. WindowScope 集成

Presentation 创建和管理 WindowScope。

规则：

- 每个窗口有独立 WindowScope。
- WindowScope 下可以有一个或多个 NavigationScope。
- 窗口关闭先请求 Routing/Mvvm leave confirmation。
- 关闭确认通过 Mvvm Interaction 或 Leave Guard 处理。
- WindowScope 停止时释放所有 Outlet、View、Binding、Interaction handler 和 UI 订阅。

Presentation 不定义业务窗口模型，但提供窗口生命周期桥接。

## 20. 插件边界

插件可以贡献：

- View。
- Style。
- Theme resource。
- Icon。
- Data template。
- Interaction handler。
- Presentation resource。

插件贡献规则：

- 必须有 ContributionLease。
- 必须记录 PluginId。
- 必须可撤销。
- 不能污染 Host Root resource registry。
- 不能让 Host 静态缓存持有插件私有 View 类型实例。
- 停用时必须先停止新入口，再关闭活动 UI，再撤销资源。

插件 View/ViewModel 绑定中跨边界传递的公共类型必须位于 Host 共享 contract 程序集。

## 21. AOT 和 Source Generator

Presentation generator 负责：

- 生成 View/ViewModel binding manifest。
- 生成 View factory descriptor。
- 生成 Resource manifest。
- 生成 Interaction handler descriptor。
- 生成 Validation binding descriptor。
- 诊断重复默认 View。
- 诊断 ViewModel 没有 View。
- 诊断插件 View 类型泄漏。
- 诊断运行时扫描和命名约定定位。

运行时禁止：

- 扫描程序集找 View。
- 按命名约定反射查找 View。
- 反射构造 View 作为默认路径。
- 动态代理绑定 UI 生命周期。

## 22. 错误策略

| 场景 | 默认处理 |
|---|---|
| UI runtime 启动失败 | Application fatal。 |
| UI dispatcher 未 ready | 等待 ready 或返回明确错误。 |
| ViewLocator 找不到 View | Presentation commit failed。 |
| View 创建失败 | Presentation commit failed。 |
| Binding 失败 | Presentation commit failed。 |
| Outlet 不存在 | Navigation failed with diagnostics。 |
| Interaction handler 缺失 | Interaction NotHandled。 |
| Plugin View 释放失败 | 插件卸载错误聚合。 |
| Resource 撤销失败 | 记录错误，继续撤销其他资源。 |

Presentation 错误不能静默吞掉。必须返回给 Routing、Mvvm 或 Lifecycle 的错误策略。

## 23. 诊断

必须记录：

- UI runtime ready / stopping。
- Dispatcher 投递失败。
- ViewLocator 命中和失败。
- View 创建耗时。
- Binding 耗时。
- Outlet commit 计划和结果。
- Activation visual adapter 执行。
- Interaction handler 执行。
- Resource contribution 和撤销。
- 插件 UI 关闭和资源清理。

诊断信息必须包含 ScopeId、WindowId、NavigationScopeId、RouteId、ViewModel type、View type、PluginId 和 ContributionId。

## 24. 测试策略

Testing 包应提供：

- FakePresentationRuntime。
- FakeUiDispatcher。
- TestViewLocator。
- TestViewFactory。
- TestRouteOutlet。
- TestPresentationCommitter。
- Interaction test handler。
- View binding recorder。
- Plugin presentation resource test host。

测试必须覆盖：

- ViewLocator 成功和失败。
- 多默认 View 诊断。
- View 创建失败。
- Outlet commit 成功。
- Outlet commit 失败回滚。
- Interaction Completed / Canceled / NotHandled / Failed。
- ActivationScope 释放时释放 binding。
- 插件停用时关闭 View 并撤销资源。
- UI dispatcher stopped 后拒绝投递。

Presentation 测试应能在无真实 AtomUI/Avalonia UI 的环境中运行。真实 UI 集成测试单独放到平台集成测试中。

## 25. 第一版取舍

第一版不做：

- Workbench / Documents / Dashboard 模型。
- 复杂 dock layout 管理。
- 任意运行时 View 扫描。
- 主题设计器。
- 通用 Dialog 框架业务模型。
- 跨进程 UI 插件沙箱。

第一版优先稳定：

- UI Dispatcher。
- ViewLocator。
- View/ViewModel binding。
- Route Outlet commit。
- Activation integration。
- Interaction handler。
- Plugin resource contribution。
- Source Generator 契约。
