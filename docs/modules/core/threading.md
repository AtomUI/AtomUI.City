# AtomUI.City.Core Threading 设计

版本：v0.1
状态：正式初版
适用范围：`AtomUI.City.Core` 中线程模型、UI Dispatcher 抽象、后台任务调度、Operation 执行、调度策略、插件线程约束、诊断和测试设计。

## 1. 定位

Threading 是 AtomUI.City 内核契约的一部分。

桌面应用天然是多线程环境。UI Thread、ThreadPool、后台任务、Timer、异步命令、数据请求、事件派发、状态通知和插件卸载会同时存在。如果线程模型不由 Core 统一约束，State、EventBus、Data、MVVM、Presentation 和 PluginSystem 会各自实现调度规则，最终导致跨线程 UI 访问、后台任务泄漏、插件无法卸载和测试不可控。

Threading 的目标：

- 定义框架统一线程模型。
- 抽象 UI Dispatcher，但不依赖 AtomUI/Avalonia。
- 提供受生命周期管理的后台任务调度。
- 统一 Operation 执行、取消、错误和诊断。
- 为 State、EventBus、MVVM、Data、Routing、PluginSystem 提供一致调度契约。
- 支持测试环境中的 fake dispatcher 和 deterministic scheduler。
- 保持 AOT/trimming 友好。

## 2. 非目标

Threading 不负责：

- 实现 Avalonia Dispatcher。
- 实现 AtomUI 控件线程访问规则。
- 替代 .NET ThreadPool。
- 替代 GenericHost 的 `IHostedService`。
- 提供完整 actor framework。
- 强制应用所有业务代码都只能通过框架调度。
- 提供 Rx Scheduler 作为核心 API。

Presentation 负责把 Avalonia Dispatcher 适配给 Core。Core 只定义抽象和规则。

## 3. 设计原则

Threading 必须遵守以下原则：

- UI 对象只能在 UI Thread 访问。
- ViewModel 默认按 UI-affine 对象处理。
- 生命周期阶段必须串行化。
- 同一个 Scope 不能并发 Start、Stop 或 Dispose。
- 后台任务必须绑定 Lifecycle Scope。
- Command、Data 请求和插件后台任务必须绑定 OperationScope。
- State 写入必须原子化，通知必须在提交后触发。
- EventBus 订阅必须声明投递策略。
- 插件不能启动非受控线程。
- 取消是正常结果，不是错误。
- 后台异常必须进入 Diagnostics 和 ErrorPolicy。
- 测试中必须可以替换真实线程调度。

## 4. 线程边界

AtomUI.City 运行时至少存在以下线程边界：

| 边界 | 说明 |
|---|---|
| Host startup/shutdown | Host 构建、启动和停止流程。 |
| UI Thread | AtomUI/Avalonia UI 对象访问线程。 |
| Background | ThreadPool、数据请求、长耗时命令和后台任务。 |
| Operation | Command/Data/Plugin task 的执行边界。 |
| Serialized dispatch | 针对同一个 owner/key 的顺序执行队列。 |
| Test scheduler | 测试中可控的调度环境。 |

Core 不假设 UI Thread 一定存在。无 UI 测试、CLI 工具和构建设备可以只使用后台和测试调度。

## 5. 核心抽象

建议核心抽象：

| 类型 | 职责 |
|---|---|
| `IUiDispatcher` | UI Thread 调度抽象。 |
| `IBackgroundTaskScheduler` | 受生命周期管理的后台任务调度。 |
| `IOperationRunner` | 创建 OperationScope 并执行 Command/Data/Plugin task。 |
| `IExecutionDispatcher` | State/EventBus/Reaction 等框架通知的统一调度入口。 |
| `IDispatcherAccessor` | 获取当前运行时 dispatcher 组合。 |
| `DispatchPolicy` | 描述回调投递目标和执行模式。 |
| `DispatchTarget` | 当前线程、UI 线程、后台或序列队列。 |
| `DispatchMode` | 允许内联或强制异步投递。 |
| `BackgroundTaskHandle` | 后台任务的可取消、可诊断句柄。 |

命名不加 `City` 前缀。命名空间已经表达框架身份。

## 6. IUiDispatcher

`IUiDispatcher` 是 Core 对 UI Thread 的唯一认知。

建议语义：

```csharp
public interface IUiDispatcher
{
    bool CheckAccess();

    ValueTask InvokeAsync(
        Action callback,
        CancellationToken cancellationToken = default);

    ValueTask<T> InvokeAsync<T>(
        Func<T> callback,
        CancellationToken cancellationToken = default);

    ValueTask PostAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default);
}
```

规则：

- `CheckAccess` 只判断当前是否在 UI Thread。
- `InvokeAsync` 需要返回执行结果或异常。
- `PostAsync` 表示异步投递，不要求调用方等待 UI 回调立即完成。
- 如果 UI runtime 尚未启动，调用 UI dispatcher 必须返回明确错误或进入等待策略。
- Core 不引用 Avalonia 类型。
- Presentation 提供 Avalonia 实现。
- Testing 提供 fake implementation。

## 7. DispatchPolicy

`DispatchPolicy` 用于统一描述回调投递规则。

推荐目标：

| Target | 说明 |
|---|---|
| `Current` | 在当前线程执行。 |
| `UiThread` | 投递到 UI dispatcher。 |
| `Background` | 投递到后台任务调度器。 |
| `Serialized` | 按 owner/key 顺序执行。 |

推荐模式：

| Mode | 说明 |
|---|---|
| `InlineIfAllowed` | 已经位于目标上下文时可以直接执行。 |
| `Post` | 总是异步投递，避免重入。 |

第一版不引入复杂 priority 模型。Presentation 可以在内部把默认策略映射到 Avalonia dispatcher priority。

## 8. IExecutionDispatcher

`IExecutionDispatcher` 是框架内部通知调度入口，主要服务 State、EventBus、Reaction、Interaction 和诊断事件。

建议职责：

- 根据 `DispatchPolicy` 投递回调。
- 处理 UI dispatcher 缺失或停止。
- 处理后台调度失败。
- 对 `Serialized` target 提供按 owner/key 的顺序执行。
- 捕获回调异常并交给 ErrorPolicy。
- 记录调度耗时和队列诊断。

`IExecutionDispatcher` 不应该成为业务任务编排框架。长耗时业务任务应通过 `IOperationRunner` 或 Data/Command 管线执行。

## 9. IBackgroundTaskScheduler

后台任务必须受生命周期管理。

建议语义：

```csharp
public interface IBackgroundTaskScheduler
{
    BackgroundTaskHandle Schedule(
        ILifecycleScope owner,
        Func<BackgroundTaskContext, ValueTask> callback,
        BackgroundTaskOptions? options = null);
}
```

规则：

- `owner` 必须是有效 Lifecycle Scope。
- 父 Scope 停止时，后台任务必须收到取消。
- 任务完成、失败、取消都必须进入诊断。
- 任务异常不能成为 unobserved task。
- 任务不允许无限期阻塞关闭流程。
- 关闭超时后进入 ErrorPolicy。

插件、模块和应用服务不应直接启动长期裸线程。需要长期后台任务时，必须通过 Host 管理。

## 10. IOperationRunner

`IOperationRunner` 用于 Command、Data 请求、插件后台动作等有明确开始和结束的任务。

职责：

- 创建 OperationScope。
- 关联父 Scope cancellation token。
- 设置忙碌状态和执行状态。
- 捕获异常并映射为 Operation result。
- 区分 Success、Cancelled、Failed。
- 写入诊断上下文。
- 按策略决定是否在后台执行。

Operation 默认可以并发运行。具体 Command 或 Data client 可以声明重入策略：

| 策略 | 说明 |
|---|---|
| `AllowConcurrent` | 允许并发执行。 |
| `DisallowConcurrent` | 正在执行时拒绝新请求。 |
| `Queue` | 顺序排队执行。 |
| `CancelPrevious` | 新请求取消上一次执行。 |

## 11. Lifecycle 集成

Threading 必须接入 Lifecycle。

生命周期规则：

- Scope 启动、停止和释放必须串行化。
- 同一个 Scope 不能并发执行生命周期阶段。
- 父 Scope 停止时先拒绝新任务，再取消子 Scope。
- OperationScope 必须响应取消。
- Scope Dispose 必须等待受管任务完成或进入关闭超时。
- 释放期间不允许创建新的子 Scope、Operation 或后台任务。

释放顺序中，线程相关步骤应包括：

```text
Stop accepting new work
-> Cancel scope token
-> Cancel child operations
-> Drain or stop background tasks
-> Revoke contribution leases
-> Dispose subscriptions
-> Dispose service scope
```

## 12. State 集成

State 必须遵守线程模型。

规则：

- `SetValue` 和 `Update` 必须原子化。
- `Update` 中不能执行 IO 或长耗时逻辑。
- 状态提交和订阅通知必须分离。
- 不允许在状态锁内调用订阅者。
- 相同 state key 的变更通知必须保持顺序。
- 通知投递由 `DispatchPolicy` 决定。
- 应用级共享状态绑定 ApplicationScope。
- 插件状态绑定插件生命周期或插件贡献 lease。

推荐状态值使用 immutable 或 replace-only 风格。框架只能保证状态引用的替换原子性，不能保护调用方在对象内部进行跨线程原地修改。

## 13. EventBus 集成

EventBus 必须把线程策略作为订阅 contract 的一部分。

订阅时声明：

- Scope。
- Channel。
- Handler。
- DispatchPolicy。
- ErrorPolicy。

发布规则：

- 发布时先获取订阅快照。
- 当前发布不持有全局锁调用 handler。
- 每个订阅按自己的 `DispatchPolicy` 投递。
- UI handler 默认投递到 `UiThread`。
- 后台 handler 异常进入 EventBus error policy。
- 订阅 Scope 停止后不再接收新事件。

已投递但尚未执行的事件是否取消，由订阅 Scope cancellation token 决定。

## 14. MVVM 集成

MVVM 默认 UI-affine。

规则：

- ViewModel 激活和停用在 UI Thread。
- `PropertyChanged` 默认在 UI Thread 触发。
- Interaction handler 默认在 UI Thread。
- Command 启动可以发生在 UI Thread。
- 长耗时 Command 必须通过 `IOperationRunner` 进入后台。
- Command 完成后通过 dispatcher 回到 UI 更新状态。
- ActivationScope 停用时释放状态订阅、事件订阅和命令 Operation。

框架不强制所有 ViewModel 方法都只能在 UI Thread 调用，但框架提供的激活、绑定、交互和通知入口必须遵守 UI-affine 规则。

## 15. Data 集成

Data 请求属于 Operation。

规则：

- 每次请求绑定 OperationScope。
- 请求必须接收 cancellation token。
- HTTP、RPC、本地 IO 等耗时动作不能阻塞 UI Thread。
- 请求结果进入状态或 ViewModel 前需要按目标调度策略回到合适线程。
- 重试、超时和取消必须进入 Operation diagnostics。

Data 模块不能直接假设 UI dispatcher 存在。

## 16. Presentation 集成

Presentation 负责提供 UI dispatcher 实现。

职责：

- 在 AtomUI/Avalonia runtime 准备完成后注册 `IUiDispatcher`。
- 把 View/ViewModel 激活接入 UI Thread。
- 让 Interaction handler 在 UI Thread 执行。
- 把 State 和 EventBus 的 UI 投递映射到 Avalonia Dispatcher。
- 在 UI dispatcher 停止后拒绝新的 UI 投递并输出诊断。

Core 只等待 Presentation 通过 Host lifetime 信号报告 UI runtime ready。

## 17. PluginSystem 集成

插件线程模型必须严格受 Host 管理。

插件禁止：

- 启动非受控长期线程。
- 创建无法被 Host 追踪的长期 Timer。
- 把 Dispatcher callback 长期保存为静态引用。
- 在静态字段保存 Host、Scope、ServiceProvider、ViewModel、handler 或插件类型实例。
- 绕过 Host 创建不可取消后台任务。

插件停用流程必须：

- 阻止新的插件 route、command、event handler 和后台任务进入。
- 取消插件 Operation。
- 等待受管后台任务结束或超时。
- 撤销 EventBus 订阅。
- 释放 State subscription。
- 移除 UI 入口和 dispatcher callback。

插件卸载前必须确认没有后台任务、Timer、Dispatcher callback、EventBus handler 或 State subscription 继续持有插件类型。

## 18. Module 初始化并发

模块生命周期默认按依赖图拓扑顺序串行执行。

第一版不建议并行初始化模块。原因：

- 启动顺序更可诊断。
- 配置和 PreConfigure 结果更确定。
- 插件加载和贡献回滚更容易控制。
- 桌面应用启动瓶颈通常不在模块初始化并行度。

未来如果支持并行模块初始化，必须由 ModuleSystem 根据 source generator 产物和显式声明判断安全性，不能运行时猜测。

## 19. AOT 与 Source Generator

Threading 设计必须 AOT 友好。

规则：

- 不通过反射扫描线程特性。
- 不依赖动态代理拦截方法切换线程。
- 调度策略通过 descriptor、Options 或 source generator 产物表达。
- EventBus handler、State definition、Command descriptor 可以由 source generator 生成 dispatch metadata。
- Analyzer 可以检查框架已知入口是否缺少 dispatch policy。

运行时只消费强类型 descriptor，不做隐式程序集扫描。

## 20. 错误处理

线程相关错误必须进入统一诊断。

| 场景 | 处理 |
|---|---|
| UI dispatcher 未准备 | 返回明确错误或等待 ready 策略。 |
| UI dispatcher 已停止 | 拒绝投递并记录诊断。 |
| 后台任务异常 | 捕获并交给 ErrorPolicy。 |
| Operation 取消 | 标记 Cancelled，不按失败处理。 |
| Operation 异常 | 标记 Failed，并记录 Scope、Owner、阶段。 |
| 关闭超时 | 进入 ErrorPolicy，并记录未完成任务。 |
| 插件任务未退出 | 插件进入 UnloadPending。 |

错误诊断必须包含：

- Scope Id。
- Scope Kind。
- Owner module。
- Plugin id。
- DispatchTarget。
- Operation id。
- Cancellation 状态。
- 执行耗时。
- 异常信息。

## 21. 测试要求

Testing 包必须支持线程模型测试。

建议提供：

- `TestUiDispatcher`。
- `ImmediateExecutionDispatcher`。
- `DeterministicExecutionDispatcher`。
- `TestBackgroundTaskScheduler`。
- `TestOperationRunner`。
- 调度队列断言工具。
- 未完成任务断言工具。
- 插件卸载线程引用诊断断言。

测试目标：

- 不依赖真实 UI Thread。
- 不依赖真实时间等待。
- 可以手动推进调度队列。
- 可以断言回调投递目标。
- 可以断言 Scope 停止后任务被取消。
- 可以断言插件卸载前无受管任务残留。

## 22. 开发者约束

开发者使用 AtomUI.City 时必须遵守：

- 不在构造函数中启动长期任务。
- 不在构造函数中订阅长期事件。
- 不在 UI Thread 执行长耗时 IO。
- 不在 `Update` 回调中执行异步或 IO。
- 不在后台线程直接访问 UI 对象。
- 长期后台任务必须绑定 Scope。
- Command/Data 必须接收取消令牌。
- Plugin 代码必须使用 Host 提供的调度入口。
- State/EventBus subscription 必须绑定生命周期。

这些约束属于 AtomUI.City 编程范式的一部分。
