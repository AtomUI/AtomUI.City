# AtomUI.City.Presentation

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Presentation` 负责 AtomUI/Avalonia 集成、ViewLocator、UI Dispatcher、Activation 接入和 Interaction Handler。

Presentation 是框架运行时和 UI 基础设施之间的隔离层。

## 边界

Presentation 依赖：

- Avalonia
- AtomUI
- Core
- Mvvm
- Routing
- State

Presentation 负责：

- View 与 ViewModel 绑定。
- UI 线程调度，并提供 Core `IUiDispatcher` 的 Avalonia 实现。
- ViewModel 激活接入。
- Interaction Handler 接入。
- AtomUI/Avalonia 应用生命周期接入。

Presentation 不负责：

- 提供业务控件。
- 承担应用业务布局模型。
- 替代 AtomUI 控件库。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | AtomUI/Avalonia 集成、UI Runtime、ViewLocator、View/ViewModel 绑定、Route Outlet、UI Dispatcher、Activation、Interaction、插件资源和测试策略。 |
| [ui-runtime.md](ui-runtime.md) | UI runtime ready/stopping、PresentationScope、WindowScope 和 AtomUI/Avalonia runtime bridge。 |
| [dispatcher.md](dispatcher.md) | `IUiDispatcher`、UI thread access、投递、停止、异常和测试。 |
| [view-locator.md](view-locator.md) | ViewModel 到 ViewDescriptor 的定位、View manifest、重复 View 诊断和插件 View 撤销。 |
| [view-binding.md](view-binding.md) | View 创建、DataContext、binding handle、View/ViewModel 生命周期和释放。 |
| [route-outlet.md](route-outlet.md) | Route Outlet、commit plan、attach/detach/replace、提交失败回滚和诊断。 |
| [activation-integration.md](activation-integration.md) | Visual lifecycle、ActivationScope、attached/detached、close intent 和 ViewModel 激活边界。 |
| [interaction-and-validation.md](interaction-and-validation.md) | Interaction handler、Dialog/FilePicker/Toast、Validation visual state 和命令绑定。 |
| [state-and-localization.md](state-and-localization.md) | State UI 更新、culture refresh、binding refresh、路由标题和错误文本刷新。 |
| [resources-and-plugins.md](resources-and-plugins.md) | AtomUI/Avalonia 资源、主题、插件 View、插件资源贡献和撤销。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | Presentation 诊断字段、fake runtime、平台集成测试和测试矩阵。 |

UI Dispatcher 必须遵守 Core 线程模型。线程模型见：[Core Threading 设计](../core/threading.md)。
