# AtomUI.City.EventBus Detailed Design

版本：v0.1
状态：初版草案
适用范围：进程内系统级事件总线、事件发布、事件通道、顺序与并发、背压、生命周期、插件集成、AOT/source generator、诊断和测试设计。

## 1. 定位

`AtomUI.City.EventBus` 是 AtomUI.City 的进程内系统级事件通知基础设施。

它用于 Host、静态模块、运行时插件和框架组件之间发布已经发生的事实，使发布方不需要依赖订阅方，从而降低模块之间的直接耦合。

EventBus 必须同时满足：

- 强类型事件契约。
- 模块和插件之间受控通信。
- 生命周期托管订阅。
- 明确的线程与顺序语义。
- 有界队列和背压。
- 插件动态停用和卸载。
- 高性能发布热路径。
- AOT/trimming/source generator 友好。
- 可诊断和可确定性测试。

EventBus 不是一个简单的 delegate 集合。它是建立在 Core Lifecycle、Threading、Diagnostics 和 PluginSystem Contribution 之上的框架级运行时能力。

## 2. 非目标

EventBus 不负责：

- 保存当前值或最新状态。当前值由 `AtomUI.City.State` 管理。
- UI 请求响应。UI 交互由 `Interaction<TRequest,TResult>` 管理。
- 命令执行。命令和耗时操作由 MVVM Command 与 OperationScope 管理。
- 远程或分布式消息传输。
- 持久化消息、Inbox、Outbox 和跨进程可靠投递。
- 通用工作流编排。
- 高频遥测流和无限数据流。
- 事件溯源。
- 默认 latest replay 或 sticky event。
- 默认 Rx API。

未来可以增加分布式 EventBus 适配层，但它不能改变本模块进程内事件总线的生命周期、线程和插件边界。

## 3. 依赖关系

`AtomUI.City.EventBus` 依赖：

- `AtomUI.City.Core`
  - Lifecycle
  - Threading
  - Dependency Injection
  - Errors and Diagnostics
  - Contribution / ContributionLease

EventBus 不依赖：

- AtomUI / Avalonia
- CommunityToolkit.Mvvm
- ReactiveUI
- System.Reactive
- PluginSystem 的具体加载实现
- Microsoft.CodeAnalysis

PluginSystem 可以依赖 EventBus contract，并通过 Contribution 向 EventBus 注册插件 handler。

## 4. 核心原则

EventBus 遵守以下原则：

- 事件表示已经发生的事实，不表示请求执行某个动作。
- 默认使用不可变事件对象。
- 发布主路径是异步的，不提供阻塞式同步发布。
- 订阅必须绑定 Lifecycle Scope 或 ContributionLease。
- 发布时不持有 registry 写锁调用用户代码。
- 同一个订阅默认不并发执行多个事件。
- 不提供全局事件总顺序。
- 默认可靠、有序、防重入。
- 高并发、事件丢弃和无序处理必须显式开启。
- 所有异步队列必须有界。
- 插件私有类型不能进入 Host 共享事件平面。
- Handler 错误不能静默丢失。
- 运行时发布热路径不做程序集扫描和反射调用。

## 5. 核心抽象

建议公共抽象：

| 类型 | 职责 |
|---|---|
| `IEventBus` | 组合发布和订阅能力的应用内部入口。 |
| `IEventPublisher` | 只提供事件发布能力。 |
| `IEventSubscriber` | 只提供事件订阅能力。 |
| `IEventHandler<TEvent>` | 强类型异步事件处理器。 |
| `IEventSubscription` | 可停止、可释放、可诊断的订阅句柄。 |
| `EventContext<TEvent>` | 事件数据、诊断上下文和取消令牌。 |
| `EventChannel<TEvent>` | 强类型事件通道定义。 |
| `EventContractId` | 稳定事件契约标识。 |
| `EventPublishOptions` | 发布完成语义、channel、partition 和错误策略。 |
| `EventSubscriptionOptions` | 调度、并发、背压和错误策略。 |
| `EventPublishResult` | 当前发布的结构化结果。 |
| `EventPostResult` | 事件是否被受管 channel 接受的结构化结果。 |
| `EventSubscriptionDescriptor` | 静态或动态订阅描述。 |
| `IEventContractRegistry` | Host 可见事件契约注册表。 |
| `IEventSubscriptionRegistry` | 当前 EventBus 的订阅注册表。 |

插件可以只获得受限的 `IEventPublisher` 或 `IEventSubscriber`，不应默认获得可管理整个 registry 的接口。

## 6. 发布接口

主发布接口建议为：

```csharp
public interface IEventPublisher
{
    ValueTask<EventPublishResult> PublishAsync<TEvent>(
        TEvent eventData,
        EventPublishOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<EventPostResult> PostAsync<TEvent>(
        TEvent eventData,
        EventPublishOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

### PublishAsync

`PublishAsync` 默认语义：

- 捕获发布开始时的有效订阅快照。
- 按 channel、partition 和订阅策略进行投递。
- 等待当前发布快照中的 handler 完成、取消或失败。
- 返回结构化发布结果。
- 发布方取消只影响尚未开始或允许取消的投递。
- Handler 失败是否使发布失败，由错误策略决定。

### PostAsync

`PostAsync` 默认语义：

- 把事件写入受管有界队列。
- 事件被队列接受后返回。
- 不等待 handler 执行完成。
- 队列消费错误仍由 EventBus 观察并进入 Diagnostics。
- 不是裸 fire-and-forget。
- 队列关闭、容量策略拒绝或 owner Scope 停止时返回失败结果。

第一版不提供同步 `Publish`。同步等待 UI Dispatcher 或后台 handler 容易造成死锁和不可控阻塞。

## 7. EventContext

EventBus 不把裸事件对象作为唯一 handler 输入。Handler 应接收事件上下文：

```csharp
public sealed class EventContext<TEvent>
{
    public required TEvent Event { get; init; }
    public required EventContractId ContractId { get; init; }
    public required Guid EventId { get; init; }
    public required string CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public required DateTimeOffset PublishedAt { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}
```

完整上下文还应关联：

- Channel。
- Partition key。
- Publisher module。
- Publisher plugin。
- Publisher Scope。
- Delivery attempt。
- Publish depth。
- DiagnosticContext。

EventContext 是只读执行上下文，不允许 handler 修改事件全局元数据。

## 8. 事件契约

事件类型默认作为强类型 contract，但运行时识别不能只依赖 CLR 类型全名。

事件契约需要稳定 `EventContractId`：

```text
workspace.changed.v1
authentication.state-changed.v1
plugin.state-changed.v1
```

规则：

- 未显式声明 Id 时，可以由 source generator 从类型全名生成默认 Id。
- 对外共享或长期兼容的 contract 应显式声明稳定 Id。
- Contract Id 在当前 Host 中必须唯一。
- Contract Id 与 CLR type 的映射由生成 descriptor 注册。
- 运行时不扫描程序集查找事件类型。
- 破坏性变更应创建新的 contract version。

跨 Host、静态模块和插件边界的完整规则见：[Event Contracts 设计](contracts.md)。

## 9. EventChannel

Channel 用于表达同一个事件类型在不同语义通道中的投递边界。

建议定义：

```csharp
public readonly record struct EventChannel<TEvent>(string Name);
```

Channel 不是字符串 topic 的无类型替代品。它仍然与 `TEvent` 绑定。

Channel 可用于：

- 区分 Host 系统事件和应用事件。
- 区分同一事件类型的不同来源。
- 隔离插件私有事件平面。
- 定义顺序、容量和背压策略。
- 定义授权边界。

默认 channel 为事件 contract 的 application channel。只有需要不同调度、隔离或授权语义时才创建额外 channel。

## 10. Channel 执行模式

EventBus 不提供全局总顺序。顺序在 channel 范围内定义。

| 模式 | 语义 |
|---|---|
| `Serialized` | 同一 channel 的事件按接受顺序处理。默认模式。 |
| `Partitioned` | 相同 partition key 内有序，不同 key 可以并行。 |
| `Concurrent` | 允许并发处理，不保证事件完成顺序。 |

默认选择 `Serialized`，原因：

- 桌面应用中的系统通知通常要求状态观察顺序稳定。
- 可以避免同一订阅重入。
- 更容易调试和测试。
- 插件停用时更容易 drain。

高吞吐场景可以显式使用 `Partitioned` 或 `Concurrent`。

## 11. 顺序保证

EventBus 明确保证：

- `Serialized` channel 中，事件按进入 channel 的顺序开始处理。
- 同一个订阅默认按事件顺序执行，不并发重入。
- `Partitioned` channel 中，相同 key 保持顺序。
- 同一次发布捕获的订阅快照保持稳定。

EventBus不保证：

- 不同 channel 之间的顺序。
- 不同 partition key 之间的顺序。
- 不同订阅之间的完成顺序。
- `Concurrent` channel 中的处理顺序。
- UI Dispatcher 和后台 handler 之间的完成顺序。

业务流程不能依赖订阅注册顺序。如果处理器 A 必须先于处理器 B，应使用显式工作流、Command 或服务调用，而不是 EventBus handler order。

## 12. 调度模型

每个订阅必须声明 Core `DispatchPolicy`：

| Target | EventBus 语义 |
|---|---|
| `Current` | 在当前发布执行上下文运行。只适合轻量、确定性处理。 |
| `UiThread` | 投递到 Presentation 提供的 UI Dispatcher。 |
| `Background` | 通过 Core 后台调度器执行。 |
| `Serialized` | 在订阅或指定 key 的串行队列执行。 |

默认策略：

- UI handler 使用 `UiThread`。
- 非 UI handler 默认使用受管 `Serialized` 调度。
- 明确无状态且线程安全的 handler 可以使用 `Background` 并允许并发。
- `Current` 不作为应用 handler 的默认值。

完整调度、队列和背压规则见：[Dispatching 设计](dispatching.md)。

## 13. 订阅生命周期

订阅必须有 owner：

```text
Subscription
-> Lifecycle Scope
or
-> ContributionLease
```

允许的 owner 示例：

- ApplicationScope。
- WindowScope。
- RouteScope。
- ActivationScope。
- 插件 ContributionLease。

Owner 停止后：

- 新发布不能捕获该订阅。
- 已排队但尚未执行的投递按策略取消。
- 正在执行的 handler 收到 cancellation token。
- EventBus 等待 drain 或进入超时错误策略。
- 订阅从 registry 和所有快照缓存中移除。

第一版使用强引用订阅，不提供弱引用订阅。弱引用会掩盖生命周期管理错误，并降低插件卸载问题的可诊断性。

完整规则见：[Subscriptions 设计](subscriptions.md)。

## 14. 背压

所有异步 channel 和订阅队列必须有容量上限。

支持策略：

| 策略 | 说明 |
|---|---|
| `Wait` | 等待容量，适合可靠系统事件。默认。 |
| `Reject` | 立即拒绝当前事件。 |
| `DropOldest` | 丢弃最早未处理事件。 |
| `DropNewest` | 丢弃最新未处理事件。 |
| `CoalesceLatest` | 相同 contract/partition 只保留最新事件。 |

规则：

- 可靠系统事件默认 `Wait`，不能静默丢弃。
- 丢弃策略必须显式配置并记录诊断。
- `CoalesceLatest` 只适合可合并的刷新通知。
- 需要表达当前值时应使用 State，而不是依赖 replay 或不断发布事件。
- UI 高频更新应优先写入 State，再由 UI 订阅状态变化。

## 15. 错误策略

EventBus 需要定义 handler 错误策略：

| 策略 | 说明 |
|---|---|
| `ContinueAndReport` | 记录失败并继续其他订阅。默认。 |
| `StopPublication` | 停止当前事件剩余投递。 |
| `FailPublisher` | 发布结果标记失败，由发布方处理。 |
| `DisableSubscription` | 达到连续失败阈值后禁用订阅。 |

错误规则：

- 取消不按失败统计。
- 一个 handler 失败默认不阻止其他独立 handler。
- UI handler 错误不能逃逸到 Dispatcher 形成未处理异常。
- `PostAsync` handler 错误必须进入诊断和错误策略。
- 插件 handler 失败默认隔离在当前插件。
- 错误策略不能破坏 Scope 停止和插件卸载流程。

## 16. 递归发布

Handler 可以发布新的事件，但必须防止无界递归。

EventContext 维护：

- CorrelationId。
- CausationId。
- Publish depth。
- 当前 contract chain。

默认规则：

- 子事件继承 CorrelationId。
- 子事件 CausationId 指向父事件 EventId。
- 超过最大 publish depth 时拒绝继续发布并记录错误。
- 检测到明显 contract 循环时输出诊断。
- 同一个 Serialized channel 的 handler 不允许再次等待该 channel 的 `PublishAsync`。
- 需要异步解耦递归链时使用 `PostAsync`。

## 17. 高性能设计

EventBus 发布热路径采用：

- 每个 contract/channel 的不可变订阅快照。
- 注册和撤销时加锁，发布读取无全局写锁。
- 原子替换快照。
- 泛型发布和强类型 invoker。
- `ValueTask` 支持同步完成路径。
- 预构建 dispatch plan。
- Source Generator 生成 handler descriptor 和 invoker。
- Channel 队列使用有界异步结构。
- 诊断采样和 payload 日志分离。

发布热路径禁止：

- 程序集扫描。
- 反射查找 handler。
- `MethodInfo.Invoke`。
- 为每个 handler 创建临时 DI 容器。
- 持有 registry 锁执行用户代码。
- 无界 `Task.Run`。
- 无界队列。

性能优化必须通过 benchmark 验证，并同时验证正确性、顺序和卸载能力。

## 18. DI 集成

静态 handler 可以由 DI 创建。

建议生命周期：

- Stateless handler：Transient。
- 应用级 handler：ApplicationScope scoped。
- Route/ViewModel handler：绑定 RouteScope 或 ActivationScope。
- Plugin handler：从插件 ServiceProvider 创建。

规则：

- EventBus 不从 Host Root ServiceProvider 创建插件 handler。
- Handler 执行使用订阅 descriptor 记录的 service context。
- Scope 已停止时不能再创建 handler。
- Handler 实例不能被全局静态缓存。
- 异步释放 handler 时必须等待 `IAsyncDisposable`。

## 19. Lifecycle 集成

EventBus 注册为 ApplicationScope 内的运行时服务。

启动：

```text
Load generated event contract descriptors
-> Register static handler descriptors
-> Build channel definitions
-> Start channel workers
-> Accept publications
```

停止：

```text
Reject new publications
-> Stop accepting new subscriptions
-> Cancel queued deliveries
-> Drain in-flight handlers
-> Dispose subscriptions
-> Stop channel workers
-> Clear snapshots and dispatch plans
```

EventBus 停止必须幂等，并受 Host shutdown timeout 约束。

## 20. PluginSystem 集成

插件 EventBus handler 属于运行时 Contribution。

插件启用：

```text
Validate event contracts and capabilities
-> Register plugin-private contracts
-> Apply handler contribution
-> Create subscription leases
-> Publish plugin active state if allowed
```

插件停用：

```text
Reject new plugin deliveries
-> Remove subscriptions from publication snapshots
-> Cancel queued deliveries
-> Drain in-flight handlers
-> Revoke subscription leases
-> Clear plugin dispatch plans and private registry
```

完整插件规则见：[Plugin Integration 设计](plugins.md)。

## 21. 与 State 的边界

EventBus 和 State 的职责必须明确：

| 需求 | 使用 |
|---|---|
| 通知某件事已经发生 | EventBus |
| 获取当前值 | State |
| 监听当前值后续变化 | State |
| 新订阅者立即得到当前值 | State |
| 模块之间广播一次性事实 | EventBus |
| 高频刷新并只关心最新值 | State |

EventBus v1 不提供 latest replay。需要最新值的场景必须建模为 State。

## 22. AOT 与 Source Generator

EventBus 默认路径必须 AOT 友好。

Source Generator 负责：

- 发现显式声明的事件 contract。
- 生成 contract id/type descriptor。
- 生成静态 handler descriptor。
- 生成强类型 handler invoker。
- 生成 channel 和 dispatch metadata。
- 生成插件 event manifest。
- 检查 contract id 冲突。
- 检查共享 contract 对象图风险。
- 检查插件私有类型是否错误进入共享 contract。

运行时只消费生成 descriptor，不扫描程序集。

Dynamic Plugin Mode 可以在插件加载时读取预生成 manifest，但不应通过运行时扫描任意类型发现 handler。

## 23. 公共扩展方法

建议使用 .NET 扩展方法风格：

```text
AddEventBus
AddEventContract<TEvent>
AddEventHandler<TEvent,THandler>
ConfigureEventChannel<TEvent>
UseEventBusMiddleware
```

规则：

- `Add*` 只收集 descriptor 和服务注册。
- 不在扩展方法调用时启动 worker。
- 不调用 `BuildServiceProvider()`。
- 动态插件 handler 通过 Contribution 注册，不直接修改 Host Root services。

## 24. 测试要求

EventBus 必须支持：

- 确定性发布和队列推进。
- 订阅快照断言。
- 顺序和并发断言。
- DispatchTarget 断言。
- 背压策略断言。
- Handler 错误聚合断言。
- Scope 停止后的取消断言。
- 插件停用 drain 和卸载断言。
- Contract 版本兼容断言。
- 发布记录和事件链断言。

完整诊断和测试规则见：[Diagnostics and Testing 设计](diagnostics-and-testing.md)。

## 25. 第一版明确决策

AtomUI.City.EventBus v1 明确：

- 只做进程内事件总线。
- 默认强类型事件。
- 默认异步发布。
- 默认 `Serialized` channel。
- 默认单订阅不并发。
- 默认有界队列。
- 默认 `ContinueAndReport`。
- 默认强引用并绑定生命周期。
- 不提供弱引用订阅。
- 不提供 latest replay 或 sticky event。
- 不依赖 Rx。
- 不运行时扫描程序集。
- 跨插件边界只允许 Host 注册的共享 contract。

## 26. 文档拆分

- [Event Contracts](contracts.md)
- [Subscriptions](subscriptions.md)
- [Dispatching](dispatching.md)
- [Plugin Integration](plugins.md)
- [Diagnostics and Testing](diagnostics-and-testing.md)
