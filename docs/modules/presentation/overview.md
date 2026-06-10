# AtomUI.City.Presentation

版本：v0.1
状态：初版草案

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

- View 与 ViewModel 映射。
- UI 线程调度，并提供 Core `IUiDispatcher` 的 Avalonia 实现。
- ViewModel 激活接入。
- Interaction Handler 接入。
- AtomUI/Avalonia 应用生命周期接入。

Presentation 不负责：

- 提供业务控件。
- 承担应用业务布局模型。
- 替代 AtomUI 控件库。

## 后续拆分

- `avalonia-integration.md`
- `atomui-integration.md`
- `view-locator.md`
- `dispatcher.md`
- `activation-integration.md`

UI Dispatcher 必须遵守 Core 线程模型。线程模型见：[Core Threading 设计](../core/threading.md)。
