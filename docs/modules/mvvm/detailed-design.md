# AtomUI.City.Mvvm Detailed Design

版本：v0.1
状态：初版草案
适用范围：ViewModel、Activation、Command、Interaction、Validation，以及 MVVM 与 Lifecycle、State、Routing、Security、EventBus、Presentation 的集成边界。

## 0. 拆分文档

- [Activation](activation.md)
- [Commands](commands.md)
- [Interactions](interactions.md)
- [Validation](validation.md)

## 1. 定位

`AtomUI.City.Mvvm` 是 AtomUI.City 的开发者日常编程模型核心。它不只是 ViewModel 基类，而是定义 ViewModel 如何创建、激活、停用、执行命令、发起交互、绑定状态、接收事件和释放资源。

MVVM 层必须让开发者默认写出生命周期清晰、可测试、可释放、可诊断的桌面应用代码。

## 2. 非目标

Mvvm 不负责：

- View/ViewModel 绑定和 ViewLocator 实现。
- 路由图、导航栈和 route matching。
- Data 请求管线。
- Security policy 存储。
- UI 控件、窗口、主题。
- 全局状态容器实现。
- EventBus 核心实现。

这些由对应模块负责，Mvvm 只提供集成点。

## 3. 底层依赖

第一版直接复用 `CommunityToolkit.Mvvm`：

- `ObservableObject`
- `ObservableValidator`
- `IRelayCommand`
- `IAsyncRelayCommand`
- `RelayCommand`
- `AsyncRelayCommand`
- `[ObservableProperty]`
- `[RelayCommand]`

不重新发明命令类型，不引入 `CityCommand` / `CityViewModel` 这类命名。命名空间已经表达框架身份。

ReactiveUI / Rx 可以作为适配层，但不是默认核心依赖。

## 4. ViewModel 基础模型

建议提供：

| 类型 | 职责 |
|---|---|
| `ViewModelBase` | 默认 ViewModel 基类。 |
| `IActivatableViewModel` | 支持激活/停用的 ViewModel contract。 |
| `ActivationContext` | ViewModel 激活上下文。 |
| `ActivationScope` | ViewModel 激活期资源和取消边界。 |
| `IActivationScopeAccessor` | 当前 ActivationScope 访问入口。 |

ViewModel 构造函数只做依赖接收和轻量字段初始化。长期订阅、Reaction、EventBus 订阅、Interaction handler 绑定、Data 请求不能放在构造函数里，必须放到 Activation 阶段。

## 5. Activation 模型

ViewModel 生命周期：

```text
Constructed
-> Activating
-> Active
-> Deactivating
-> Deactivated
-> Disposed
```

ActivationScope 负责：

- 收集可释放资源。
- 提供 CancellationToken。
- 绑定 State Reaction。
- 绑定 EventBus subscription。
- 绑定 Interaction request handler。
- 跟踪 OperationScope。
- 写入诊断上下文。

ActivationScope 必须支持 disposable registration：

```text
ActivationScope
-> Register IDisposable / IAsyncDisposable
-> Dispose in reverse order on deactivation
```

Route 进入时创建 ActivationScope。Route 离开时先取消 OperationScope，再停用 ViewModel，最后释放 ActivationScope。

## 6. Active 状态

ViewModel 的 active 状态是框架级概念，不只是 UI 可见性。

active 状态可能来自：

- 当前 Route。
- 当前 Tab。
- 当前 Window。
- 当前 Region / Outlet。
- 插件激活状态。

Mvvm 应提供 active 状态通知能力，使 Command、CompositeCommand、Interaction 和 State Reaction 能根据当前激活上下文启用或暂停。

## 7. Command 模型

命令沿用 CommunityToolkit：

```text
IRelayCommand
IAsyncRelayCommand
RelayCommand
AsyncRelayCommand
```

Mvvm 在其上补充框架行为：

- 每次 async command 执行创建 OperationScope。
- OperationScope 提供 cancellation token。
- Command 错误进入 ErrorPolicy。
- Command 状态可接入 Security 权限。
- Command 状态可接入 Routing 状态。
- Command 执行诊断必须记录 ViewModel、Command、Scope、耗时、结果。

Command 需要标准化运行状态：

| 状态 | 说明 |
|---|---|
| `CanExecute` | 当前是否可执行。 |
| `IsExecuting` | 当前是否正在执行。 |
| `LastResult` | 最近一次执行结果。 |
| `LastError` | 最近一次失败信息。 |
| `CancellationToken` | 当前执行取消令牌。 |

命令失败不应导致 ViewModel 死亡。失败结果应可被 UI、Diagnostics 和测试读取。

## 8. CompositeCommand / CommandGroup

Mvvm 应支持组合命令，用于菜单、工具栏、全局快捷键和 Shell 级命令。

组合命令规则：

- 可以聚合多个子命令。
- 只执行当前 active 上下文中的子命令。
- 子命令可随 ActivationScope 注册和释放。
- 可执行状态由 active 子命令共同决定。
- 执行结果和错误需要进入 OperationScope 诊断。

建议类型可以命名为 `CompositeCommand` 或 `CommandGroup`，具体命名在实现前再定。

## 9. Deactivation 确认

桌面业务应用需要支持离开确认，例如未保存修改。

Mvvm 可以提供 ViewModel 侧 contract：

```text
ICanDeactivate
IConfirmDeactivate
```

Routing 在导航离开前调用这些 contract。Mvvm 只定义 ViewModel 能力和结果模型，不负责路由决策。

确认流程必须支持：

- 同步结果。
- 异步结果。
- 用户取消。
- 超时或异常诊断。
- 插件停用时强制取消。

## 10. Interaction 模型

ViewModel 不直接引用窗口、Dialog、MessageBox 或 AtomUI 控件。

建议提供泛型 Interaction Request：

```text
ViewModel
-> Interaction<TRequest, TResult>
-> Presentation handler
-> Result back to ViewModel
```

Interaction handler 绑定到 ActivationScope。ViewModel 停用时，未完成 interaction 应取消或返回明确的 canceled result。

Interaction 适合：

- 确认。
- 输入。
- 文件选择。
- 通知。
- 需要 UI 承接的用户交互。

## 11. Validation 模型

默认基于 `ObservableValidator`。

Mvvm 补充：

- ValidationScope。
- 同步验证和异步验证结果归一。
- Command 与验证状态联动。
- Presentation 可观察验证结果。
- Diagnostics 记录验证失败来源。

验证失败不是异常。验证失败应作为状态暴露给 UI 和测试。

## 12. 与 State 集成

State Reaction 必须绑定 ActivationScope。

```text
ActivationScope
-> State Reaction
-> Dispose on deactivation
```

ViewModel 可以持有 `IStateValue<T>`、`IWritableState<T>`、`IComputedState<T>`，但 Reaction 不能脱离 Scope 长期存在。

派生状态由 State 模块提供，Mvvm 不引入 Rx 作为默认状态表达。

State 错误不应杀死 ViewModel。状态错误进入 State 错误策略，并写入当前 Activation diagnostic context。

## 13. 与 EventBus 集成

ViewModel 订阅 EventBus 必须绑定 ActivationScope。

```text
ActivationScope
-> EventBus subscription
-> Dispose on deactivation
```

不允许 ViewModel 构造函数里创建长期订阅。这样可以避免重复激活造成重复订阅和内存泄漏。

## 14. 与 Routing / Presentation 集成

Routing 负责决定进入哪个 ViewModel，Presentation 负责 View/ViewModel 绑定。

Mvvm 负责：

- ViewModel 激活。
- ViewModel 停用。
- ActivationScope 管理。
- Command/Interaction/Validation 与当前 Scope 对齐。

ViewModel 不直接知道 View 类型。View/ViewModel binding 由 Presentation 和 source generator 处理。

## 15. 插件 ViewModel

插件 ViewModel 从插件 ServiceProvider 解析。

插件 route 激活时：

```text
RouteContribution
-> Plugin ServiceProvider
-> RouteScope
-> ActivationScope
-> ViewModel
```

插件停用时，Host 必须先关闭该插件产生的 RouteScope、ActivationScope、OperationScope，再释放插件 ServiceProvider。

## 16. AOT / Source Generator

Mvvm 默认 AOT-first。

Generator/Analyzer 负责：

- 诊断 ViewModel 构造函数不可解析。
- 诊断 ViewModel 未绑定 ActivationScope 的订阅。
- 生成 ViewModel descriptor。
- 生成 command descriptor。
- 生成 interaction descriptor。
- 与 Presentation generator 对齐 View/ViewModel binding。
- 避免运行时扫描程序集查找 ViewModel。

Mvvm 不通过反射扫描 ViewModel，不基于命名约定做默认发现。

## 17. 错误策略

| 场景 | 默认处理 |
|---|---|
| ViewModel 创建失败 | Route activation failed。 |
| Activation 失败 | 释放 ActivationScope，导航失败。 |
| Deactivation 失败 | 继续释放资源，聚合诊断。 |
| Command 执行失败 | Operation failed，不杀死 ViewModel。 |
| Interaction handler 缺失 | Interaction failed，记录诊断。 |
| Validation 失败 | 暴露验证状态，不抛异常。 |

## 18. 测试策略

Testing 包应支持：

- 构造 ViewModel 测试 Host。
- 创建 Fake ActivationScope。
- 驱动 Activate / Deactivate。
- 执行 command 并断言 OperationScope。
- 捕获 Interaction request。
- 断言 CompositeCommand active 行为。
- 断言 EventBus subscription 自动释放。
- 断言 State Reaction 自动释放。
- 断言验证状态。
- 断言错误诊断。
