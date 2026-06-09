# AtomUI.City.Mvvm

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Mvvm` 负责 ViewModel、Command、Activation、Interaction 和验证基础设施。

它建立 AtomUI.City 的 MVVM 编程模型，并与 Lifecycle、State、Routing、Security、EventBus 形成清晰集成点。

## 边界

Mvvm 依赖 CommunityToolkit.Mvvm，复用：

- `ObservableObject`
- `ObservableValidator`
- `IRelayCommand`
- `IAsyncRelayCommand`
- `RelayCommand`
- `AsyncRelayCommand`
- Source generators

Mvvm 不负责：

- 路由图和导航状态。
- HTTP 请求管线。
- 权限策略存储。
- UI 控件和 ViewLocator 实现。

## 后续拆分

- `activation.md`
- `commands.md`
- `interactions.md`
- `validation.md`
