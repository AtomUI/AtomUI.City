# AtomUI.City.EventBus Subscriptions 设计

版本：v0.1
状态：正式初版
适用范围：事件处理器、静态和动态订阅、订阅所有权、生命周期状态、DI 创建、并发与重入、撤销和插件贡献设计。

## 1. 定位

Subscription 表示一个事件 contract、handler、调度策略和生命周期 owner 之间的受管关系。

EventBus 必须能明确回答：

- 谁创建了订阅。
- 订阅由哪个 Scope 或 ContributionLease 持有。
- Handler 从哪个 ServiceProvider 创建。
- Handler 在哪个线程执行。
- 是否允许并发和重入。
- Scope 停止或插件卸载时如何停止投递。
- 当前是否还有排队或执行中的 handler。

订阅不是一个无法追踪的 delegate。

## 2. 核心抽象

建议抽象：

```csharp
public interface IEventHandler<in TEvent>
{
    ValueTask HandleAsync(
        EventContext<TEvent> context);
}
```

动态订阅入口：

```csharp
public interface IEventSubscriber
{
    IEventSubscription Subscribe<TEvent>(
        ILifecycleScope owner,
        Func<EventContext<TEvent>, ValueTask> handler,
        EventSubscriptionOptions? options = null);
}
```

订阅句柄：

```csharp
public interface IEventSubscription :
    IDisposable,
    IAsyncDisposable
{
    EventSubscriptionId Id { get; }
    EventSubscriptionState State { get; }
    ValueTask StopAsync(
        CancellationToken cancellationToken = default);
}
```

`Dispose` 用于无异步等待的快速撤销入口；需要 drain handler 时必须使用 `StopAsync` 或 `DisposeAsync`。

## 3. 订阅类型

EventBus 支持两类订阅。

### 静态订阅

静态订阅由应用或静态模块在构建期声明：

```text
Module declaration
-> Source Generator
-> EventSubscriptionDescriptor
-> Host startup registration
```

特点：

- 随 ApplicationScope 启动。
- Descriptor 在编译期生成。
- Handler 由 DI 创建。
- 不依赖运行时程序集扫描。
- 随 Host 停止释放。

### 动态订阅

动态订阅在运行时创建：

- ViewModel ActivationScope。
- RouteScope。
- WindowScope。
- 应用运行期功能。
- 插件 Contribution。

动态订阅必须提供明确 owner，并返回 `IEventSubscription`。

## 4. Subscription Descriptor

订阅 descriptor 至少包含：

- SubscriptionId。
- EventContractId。
- CLR event type descriptor。
- Channel。
- Owner kind。
- ModuleId。
- PluginId。
- Handler service type 或 generated invoker。
- Service context。
- DispatchPolicy。
- ConcurrencyPolicy。
- Queue capacity。
- BackpressurePolicy。
- ErrorPolicy。
- Diagnostic metadata。

Descriptor 不保存不必要的运行时对象。静态 descriptor 可以长时间缓存；插件 descriptor 必须由插件运行时上下文持有。

## 5. Owner

每个订阅必须绑定以下 owner 之一：

| Owner | 使用场景 |
|---|---|
| ApplicationScope | 应用级静态 handler。 |
| WindowScope | 窗口级 handler。 |
| RouteScope | 当前路由存续期间的 handler。 |
| ActivationScope | ViewModel 激活期间的 handler。 |
| ContributionLease | 插件或动态模块贡献的 handler。 |

禁止：

- 无 owner 的全局动态订阅。
- 仅依赖 finalizer 解除订阅。
- 把 subscription 保存到静态字段。
- 让子 Scope 订阅比父 Scope 活得更久。

## 6. 强引用策略

第一版只提供强引用订阅。

原因：

- 生命周期应该显式管理。
- Scope 停止时可以确定性撤销。
- 插件卸载时可以诊断仍存活的 handler。
- 弱引用会让订阅在不可预测的 GC 时间失效。
- 弱引用不能解决队列、delegate、closure 对插件类型的持有。
- 弱引用容易掩盖遗漏 Dispose 的错误。

需要“对象释放后自动退订”的场景，应通过 ActivationScope、RouteScope 或其他 Lifecycle Scope 表达。

## 7. Subscription 状态机

建议状态：

```text
Created
-> Active
-> Quiescing
-> Draining
-> Disposed
```

错误状态：

```text
Faulted
StopTimedOut
```

语义：

- `Created`：descriptor 已创建，尚未进入发布快照。
- `Active`：可以接收新投递。
- `Quiescing`：已从新发布快照移除，不接收新事件。
- `Draining`：等待已排队或执行中的 handler 完成或取消。
- `Disposed`：队列、handler、service scope 和引用已释放。
- `StopTimedOut`：停止超过 timeout，进入错误策略。

## 8. 注册流程

```text
Validate owner
-> Resolve event contract descriptor
-> Validate channel and capability
-> Build subscription descriptor
-> Allocate subscription runtime
-> Atomically add to immutable snapshot
-> Attach subscription to owner
-> Mark Active
```

如果注册中途失败：

- 不得留下半注册快照。
- 已创建资源必须释放。
- 插件 Contribution 不得创建 Active lease。
- 错误进入 Diagnostics。

## 9. 撤销流程

```text
Mark Quiescing
-> Atomically remove from publication snapshots
-> Reject new deliveries
-> Cancel or drain queued deliveries
-> Wait for in-flight handlers
-> Dispose handler resources
-> Detach from owner
-> Mark Disposed
```

关键语义：

- 发布已经捕获旧快照时，可能已经持有该订阅的 delivery token。
- `Quiescing` 状态必须在真正调用 handler 前再次检查。
- 插件停用可以要求取消旧快照中的未开始投递。
- 正在执行的 handler 通过 cancellation token 协作停止。
- EventBus 不能强制终止线程。

## 10. 发布与撤销竞争

多线程环境下，发布和撤销可能同时发生。

EventBus 采用：

- 不可变订阅快照。
- 订阅状态原子转换。
- 每次投递的执行计数。
- Quiescing barrier。

推荐流程：

```text
Publisher reads snapshot
-> TryAcquireDelivery(subscription)
-> If Active, increment in-flight count
-> Dispatch handler
-> Decrement in-flight count

Stop subscription
-> Change Active to Quiescing
-> Replace snapshot without subscription
-> Wait until in-flight count is zero
```

这保证：

- 发布热路径不需要 registry 写锁。
- 停止后不会开始新的 handler。
- 已经开始的 handler 可以被等待和取消。

## 11. Handler 创建

Handler 可以是：

- DI service handler。
- 生成的静态 invoker。
- 动态 delegate handler。

DI handler 创建规则：

- 使用 descriptor 绑定的 service context。
- Host handler 从 Host/Application service context 创建。
- Route/Activation handler 从对应 lifecycle service scope 创建。
- Plugin handler 从插件 ServiceProvider 创建。
- 不允许插件 handler fallback 到 Host Root ServiceProvider。

如果 handler 注册为 transient：

- 可以每次 delivery 创建。
- Handler 的同步或异步释放必须在本次 delivery 后执行。

如果 handler 绑定 Scope：

- Handler 随 owner Scope 创建和释放。
- EventBus 只持有 Scope 可控引用。

第一版不允许 singleton handler 隐式持有 route、ViewModel 或插件服务。

## 12. Handler 调用

Handler 调用必须通过预构建 invoker：

```text
Event delivery
-> Validate subscription state
-> Resolve/create handler
-> Enter diagnostic context
-> Dispatch according to policy
-> Invoke strongly typed handler
-> Apply error policy
-> Dispose transient handler
-> Complete delivery
```

运行时禁止使用 `MethodInfo.Invoke` 调用 handler。

## 13. 并发和重入

建议订阅并发策略：

| 策略 | 语义 |
|---|---|
| `Serial` | 同一订阅一次只执行一个事件。默认。 |
| `Concurrent` | 同一订阅允许并发执行。 |
| `Partitioned` | 相同 partition key 串行，不同 key 并行。 |
| `LatestWins` | 新投递取消同订阅尚未完成的旧投递。谨慎使用。 |

默认 `Serial`。

只有满足以下条件时才能使用 `Concurrent`：

- Handler 明确线程安全。
- Handler 不依赖事件顺序。
- Handler 不直接修改 UI。
- Handler 不共享非线程安全状态。
- 插件停用能够等待全部并发调用结束。

## 14. Filter 设计

第一版不把任意订阅 predicate 作为核心 API。

原因：

- Predicate 也属于用户代码，需要线程、错误和生命周期定义。
- 过滤器容易被用来承担业务规则。
- 动态 closure 可能持有 ViewModel 或插件实例。
- Channel 和 handler 内快速判断通常已经足够。

如果未来增加 filter：

- 必须同步、无副作用、快速完成。
- 在调度前执行。
- 错误进入订阅错误策略。
- 插件 filter 必须随订阅释放。

## 15. Handler 顺序

不同 handler 之间不提供业务顺序保证。

可以为诊断和确定性快照定义稳定 registration sequence，但它不能成为业务依赖。

禁止使用：

```text
Order = 10
Order = 20
```

来表达必须完成的业务工作流。

存在强依赖时，应：

- 在同一个 handler 中显式调用服务。
- 使用 Command/Operation 编排。
- 发布后续事实事件。
- 使用明确的流程协调器。

## 16. 生命周期取消

每次 delivery 的 cancellation token 至少关联：

- 发布方 cancellation token。
- Subscription owner token。
- EventBus shutdown token。
- Plugin deactivation token。

取消来源必须进入 DiagnosticContext。

Handler 应把 cancellation token 传递给：

- Data 请求。
- 延迟和队列等待。
- 异步 IO。
- 子 Operation。

取消不计入 handler failure。

## 17. ContributionLease

插件和运行时模块注册 handler 时，订阅必须作为 Contribution 进入 EventBus。

```text
Plugin module
-> EventHandlerContributionRequest
-> EventBus validation
-> Subscription runtime
-> ContributionLease
```

Lease 记录：

- PluginId。
- ModuleId。
- SubscriptionId。
- EventContractId。
- Channel。
- Handler descriptor。
- 当前状态。
- 撤销动作。
- in-flight 数量。

撤销 Lease 必须触发 subscription quiesce 和 drain。

## 18. 闭包和引用安全

动态 delegate handler 可能通过 closure 持有：

- ViewModel。
- Window。
- Plugin service。
- ServiceProvider。
- 大对象。

因此：

- 动态订阅必须绑定最小可用 Scope。
- 不应把 ApplicationScope 用于页面级 closure。
- Plugin delegate 必须由插件 runtime context 持有。
- 诊断应显示 handler target type 和 owner Scope。
- Analyzer 可以提示静态 subscription 捕获局部对象。

## 19. 错误处理

订阅级错误包括：

- Handler 创建失败。
- Handler 执行失败。
- Dispatcher 拒绝。
- Queue overflow。
- Stop timeout。
- Handler dispose 失败。

默认：

- 当前 delivery 标记失败。
- 继续其他独立订阅。
- 记录 SubscriptionId、EventId、Owner、PluginId。
- 连续失败可触发 `DisableSubscription`。

禁用订阅后：

- 从新发布快照移除。
- 保留诊断记录。
- 插件订阅失败可以降低插件能力或触发插件停用策略。

## 20. AOT 与 Source Generator

Source Generator 负责生成：

- 静态 handler descriptor。
- Handler service type registration。
- 强类型 invoker。
- DispatchPolicy metadata。
- EventContractId mapping。
- Plugin manifest handler declaration。

Analyzer 应检查：

- Handler 未绑定 Scope。
- Handler event type 不是注册 contract。
- 插件 handler 使用私有事件却注册到 Shared Plane。
- UI-affine handler 未声明 UI dispatch。
- Concurrent handler 未显式声明线程安全意图。
- Dynamic handler 被静态字段持有。

## 21. 测试要求

必须测试：

- 静态和动态订阅注册。
- Scope 停止自动撤销。
- 强引用订阅确定性释放。
- 发布和撤销竞争。
- Quiescing 后不开始新 handler。
- Drain 等待 in-flight handler。
- Stop timeout。
- Serial handler 不重入。
- Concurrent handler 并发执行。
- Handler 从正确 ServiceProvider 创建。
- Plugin handler 不从 Host Root 创建。
- Handler 异步释放。
- ContributionLease 撤销订阅。

## 22. 第一版明确决策

- 默认强引用订阅。
- 不提供弱引用订阅。
- 默认绑定 Lifecycle Scope。
- 插件订阅必须绑定 ContributionLease。
- 默认单订阅串行执行。
- 不提供业务 handler order。
- 不提供任意 predicate filter。
- 不通过反射调用 handler。
