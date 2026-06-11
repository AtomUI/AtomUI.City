# AtomUI.City.Mvvm Activation 设计

版本：v0.1
状态：正式初版
适用范围：ViewModel 创建、激活、停用、active 状态、ActivationScope、资源释放、State/EventBus 订阅绑定。

## 1. 定位

Activation 是 AtomUI.City.Mvvm 的生命周期核心。

ViewModel 构造函数只做依赖接收和轻量字段初始化。长期订阅、Reaction、EventBus 订阅、Interaction handler 绑定、Data 请求不能放在构造函数里，必须放到 Activation 阶段。

Activation 的目标是让 ViewModel 的运行期资源有明确边界，避免重复订阅、悬挂任务和内存泄漏。

## 2. 非目标

Activation 不负责：

- 路由匹配。
- View/ViewModel 绑定。
- UI 控件生命周期。
- EventBus 核心实现。
- State 核心实现。
- 插件程序集加载。

这些由对应模块负责。Activation 只管理 ViewModel 激活期的资源、取消、诊断和释放。

## 3. 核心抽象

| 类型 | 职责 |
|---|---|
| `IActivatableViewModel` | 支持激活/停用的 ViewModel contract。 |
| `ActivationContext` | ViewModel 激活上下文。 |
| `ActivationScope` | ViewModel 激活期资源和取消边界。 |
| `IActivationScopeAccessor` | 当前 ActivationScope 访问入口。 |
| `ActivationState` | ViewModel 当前激活状态。 |

ViewModel 默认可以继承 `ViewModelBase`，也可以只实现 `IActivatableViewModel`。

## 4. 生命周期状态

ViewModel 生命周期：

```text
Constructed
-> Activating
-> Active
-> Deactivating
-> Deactivated
-> Disposed
```

规则：

- `Constructed` 阶段不允许创建长期订阅。
- `Activating` 阶段创建 ActivationScope。导航场景中它可以先作为候选 ActivationScope 存在，用于注册 Presentation binding 和 UI 订阅释放边界。
- `Active` 阶段允许执行命令、订阅状态、发起交互。
- `Deactivating` 阶段先取消 OperationScope，再释放 ActivationScope。
- `Disposed` 是终态，不允许再次激活。

## 5. ActivationScope

ActivationScope 负责：

- 收集 `IDisposable` / `IAsyncDisposable`。
- 提供 CancellationToken。
- 绑定 State Reaction。
- 绑定 EventBus subscription。
- 绑定 Interaction handler。
- 跟踪 OperationScope。
- 写入诊断上下文。

释放顺序：

```text
Stop accepting new operations
-> Cancel activation token
-> Cancel operation scopes
-> Dispose registered resources in reverse order
-> Dispose service scope
-> Mark deactivated
```

ActivationScope 释放必须幂等。

导航场景中的 ActivationScope 状态约束：

- Presentation commit 前，ActivationScope 可以收集可释放资源，但 ViewModel 不能被视为 active。
- Presentation commit 成功后，ActivationScope 进入 running，ViewModel 进入 active。
- Presentation commit 失败时，候选 ActivationScope 必须释放，ViewModel 不触发 active 生命周期。

## 6. Active 状态

active 状态是框架级概念，不只是 UI 可见性。

active 状态可能来自：

- 当前 Route。
- 当前 Tab。
- 当前 Window。
- 当前 Region / Outlet。
- 插件激活状态。

Mvvm 应提供 active 状态通知能力，使 Command、CompositeCommand、Interaction 和 State Reaction 能根据当前激活上下文启用或暂停。

## 7. Deactivation 确认

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

## 8. State 集成

State Reaction 必须绑定 ActivationScope。

```text
ActivationScope
-> State Reaction
-> Dispose on deactivation
```

ViewModel 可以持有 `IStateValue<T>`、`IWritableState<T>`、`IComputedState<T>`，但 Reaction 不能脱离 Scope 长期存在。

State 错误不应杀死 ViewModel。状态错误进入 State 错误策略，并写入当前 Activation diagnostic context。

## 9. EventBus 集成

ViewModel 订阅 EventBus 必须绑定 ActivationScope。

```text
ActivationScope
-> EventBus subscription
-> Dispose on deactivation
```

不允许 ViewModel 构造函数里创建长期订阅。这样可以避免重复激活造成重复订阅和内存泄漏。

## 10. 插件 ViewModel

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

## 11. 错误策略

| 场景 | 默认处理 |
|---|---|
| ViewModel 创建失败 | Route activation failed。 |
| Activation 失败 | 释放 ActivationScope，导航失败。 |
| Deactivation 失败 | 继续释放资源，聚合诊断。 |
| 释放失败 | 继续释放剩余资源，聚合诊断。 |

## 12. 测试策略

Testing 包应支持：

- 创建 Fake ActivationScope。
- 驱动 Activate / Deactivate。
- 断言释放顺序。
- 断言 State Reaction 自动释放。
- 断言 EventBus subscription 自动释放。
- 模拟 Deactivation 确认。
- 断言插件停用时强制取消。
