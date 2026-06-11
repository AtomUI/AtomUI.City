# AtomUI.City.Presentation Interaction 与 Validation 设计

版本：v0.1
状态：正式初版
适用范围：Interaction handler、Dialog/FilePicker/Toast、Validation visual state 和命令绑定

## 1. Interaction Handler

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

## 2. Validation 集成

Mvvm 定义验证状态，Presentation 负责展示。

Presentation 需要支持：

- 读取 `ObservableValidator` 或框架验证状态。
- 把错误映射到 AtomUI/Avalonia validation visual state。
- Command 与验证状态变化后的 UI 刷新。
- 插件 View 的验证资源释放。

Validation failed 不是异常，不进入 fatal error。

## 3. Command Binding

Presentation 可以增强 Command Binding。

职责：

- 把 `IRelayCommand` / `IAsyncRelayCommand` 绑定到 UI command source。
- 监听 CanExecute 变化。
- 映射 busy / executing 状态。
- 与 Security、Routing 当前状态联动后的可执行性刷新。
- 释放 UI 事件订阅。

长耗时命令仍由 Mvvm / Core Operation 管理，Presentation 不执行后台任务调度。

## 4. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| Interaction completed | Unit | handler 返回结果。 |
| Interaction canceled | Unit | Scope 停用时返回 Canceled。 |
| Interaction missing | Unit | NotHandled 并记录诊断。 |
| Validation visual state | Unit | 验证错误映射到 UI 状态。 |
| Command CanExecute | Unit | UI command source 刷新可执行状态。 |
| 插件停用 | Unit | 插件 Interaction 取消并释放 handler。 |
