# AtomUI.City.State Detailed Design

版本：v0.1
状态：初版草案
适用范围：状态值、可写状态、计算状态、状态订阅、应用级共享状态、StateScope、Snapshot、集合状态、调度、插件隔离、AOT/source generator 约束。

## 1. 定位

`AtomUI.City.State` 是 AtomUI.City 的状态管理基础设施。

它的目标不是照搬 Web 框架里的 Store、Signal、Action、Effect 命名，而是用 .NET/C# 风格提供一套可组合、可观察、可派生、可释放、可诊断、可快照的状态模型。

State 模块要解决：

- ViewModel 本地状态如何表达。
- 应用级共享状态如何通过 DI 使用。
- 模块级共享状态如何表达。
- 路由级状态如何随 RouteScope 释放。
- 插件状态如何隔离和卸载。
- 派生状态如何缓存和失效。
- 状态副作用如何绑定生命周期。
- 状态快照如何持久化、恢复和测试断言。

## 2. 非目标

State 不负责：

- 业务领域建模。
- Data 请求管线。
- EventBus 核心实现。
- ViewModel 基类。
- UI 控件刷新机制。
- 全局单 Store。
- Redux 风格 Action/Reducer 编程模型。
- Rx 作为默认公开 API。

Rx / ReactiveUI 可以作为适配层，但不是 State 核心依赖。

## 3. 命名原则

State API 必须坚持 .NET 风格。

建议核心命名：

```text
IReadOnlyState<T>
IWritableState<T>
IComputedState<T>
IStateScope
IStateSubscription
StateSnapshot
IStateCollection<TKey, TItem>
```

命名原则：

- 不叫 Signal，叫 `IReadOnlyState<T>`。
- 不叫 WritableSignal，叫 `IWritableState<T>`。
- 不叫 Effect，叫 `IStateSubscription` 或 State Reaction。
- 不把 Store 作为核心抽象。
- 不默认暴露 Rx 类型。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `IReadOnlyState<T>` | 只读状态，提供当前值、版本和变化订阅。 |
| `IWritableState<T>` | 可写状态，支持 `SetValue` / `Update`。 |
| `IComputedState<T>` | 派生状态，基于依赖状态计算并缓存。 |
| `IStateScope` | 状态生命周期边界。 |
| `IStateSubscription` | 状态变化订阅或副作用句柄。 |
| `IStateFactory` | 创建 state/computed/subscription。 |
| `IStateRegistry` | 当前 scope 内的状态注册表。 |
| `StateKey<T>` | 强类型状态键。 |
| `IApplicationState` | 应用级共享状态读取和监听入口。 |
| `IApplicationStateWriter` | 应用级共享状态写入入口。 |
| `StateDefinition<T>` | 状态定义，包括默认值、生命周期、访问策略、快照策略。 |
| `StateSnapshot` | 状态快照。 |
| `IStateCollection<TKey,TItem>` | keyed collection state。 |

## 5. IReadOnlyState<T>

建议语义：

```csharp
public interface IReadOnlyState<T>
{
    T Value { get; }

    long Version { get; }

    IDisposable OnChange(Action<StateChangedEventArgs<T>> handler);
}
```

规则：

- `Value` 表示当前值。
- `Version` 每次有效变更递增。
- 相等值不触发变更，比较策略可配置。
- 变化通知在状态提交后触发。
- 订阅必须可释放。
- 默认不暴露 Rx 类型。

## 6. IWritableState<T>

建议语义：

```csharp
public interface IWritableState<T> : IReadOnlyState<T>
{
    bool SetValue(T value);

    bool Update(Func<T, T> updater);
}
```

规则：

- `SetValue` 直接设置新值。
- `Update` 基于旧值计算新值。
- 返回 `false` 表示值未变化。
- 更新必须是原子的。
- 更新失败时保留旧值。
- 不建议在 updater 中执行 IO 或长耗时逻辑。

异步操作不直接放进 state。异步请求属于 Data/Command/OperationScope，完成后再提交状态更新。

## 7. 应用级共享状态

桌面软件需要方便读取、监听和设置应用级共享状态，例如主题、语言、当前用户、当前工作区、网络状态、窗口布局策略、全局忙碌状态和授权状态。

全局状态不能做成静态全局变量，而应该是 Host 管理的、ApplicationScope 绑定的状态注册表。

```text
ApplicationScope
-> Application State Registry
   -> Theme state
   -> Culture state
   -> Auth state
   -> Current workspace state
   -> Network status state
```

Host 启动时注册：

```text
IApplicationState
IApplicationStateWriter
IStateRegistry
IStateScopeAccessor
```

推荐语义：

| 服务 | 职责 |
|---|---|
| `IApplicationState` | 读取和监听应用级共享状态。 |
| `IApplicationStateWriter` | 写入应用级共享状态。 |
| `IStateRegistry` | 底层状态注册表。 |
| `IStateScopeAccessor` | 获取当前生命周期状态作用域。 |

使用方通过构造函数注入状态服务，而不是访问静态对象。

## 8. StateKey<T> 与 StateDefinition<T>

应用级共享状态使用强类型 key。

```csharp
public readonly record struct StateKey<T>(string Name);
```

模块可以声明状态 key：

```csharp
public static class ThemeStates
{
    public static readonly StateKey<ThemeMode> CurrentTheme =
        new("AtomUI.City.Theme.Current");
}
```

注册时声明默认值和策略：

```csharp
context.States.Add(
    StateDefinition.Create(
        ThemeStates.CurrentTheme,
        defaultValue: ThemeMode.System,
        lifetime: StateLifetime.Application,
        access: StateAccessPolicy.HostWrite));
```

状态定义应包含：

- Key。
- 默认值。
- 生命周期。
- Owner module。
- Plugin id。
- 访问策略。
- 快照策略。
- 诊断元数据。

## 9. 应用级状态访问

建议 API：

```csharp
public interface IApplicationState
{
    IReadOnlyState<T> Get<T>(StateKey<T> key);

    IDisposable OnChange<T>(
        StateKey<T> key,
        Action<StateChangedEventArgs<T>> handler);
}

public interface IApplicationStateWriter
{
    IWritableState<T> GetWritable<T>(StateKey<T> key);

    bool Set<T>(StateKey<T> key, T value);

    bool Update<T>(StateKey<T> key, Func<T, T> updater);
}
```

监听仍然必须绑定生命周期 Scope。

```csharp
activationScope.OnStateChanged(ThemeStates.CurrentTheme, args => { });
```

这样监听会随 ViewModel 停用自动释放。

## 10. 应用级状态访问策略

全局状态必须有写入规则。

| 策略 | 说明 |
|---|---|
| `ReadOnly` | 所有模块可读，只有 Owner 可初始化。 |
| `OwnerWrite` | 只有声明模块可写。 |
| `HostWrite` | Host 或授权服务可写。 |
| `AuthorizedWrite` | 通过权限或 capability 授权后可写。 |
| `PluginIsolated` | 插件只能写自己的状态分区。 |

`IApplicationState` 和 `IApplicationStateWriter` 分离，方便 Host 给插件只暴露只读接口。

## 11. IComputedState<T>

`IComputedState<T>` 是只读派生状态。

```text
Dependencies
-> Compute
-> Cache value
-> Invalidate on dependency change
-> Notify subscribers
```

设计规则：

- 依赖必须显式声明或由 source generator 可静态分析。
- 计算结果应缓存。
- 依赖变化后失效。
- 有订阅或读取时才重新计算。
- 计算异常不应杀死依赖状态。
- 计算错误进入 Diagnostics。
- 计算不应执行 IO。

第一版不建议依赖表达式树自动解析属性路径，因为这对 AOT/trimming 不友好。更推荐显式依赖或 generator 可识别声明。

## 12. StateScope

StateScope 是状态生命周期边界。

它不是 ModuleScope 或 PluginScope。它绑定到已有 Lifecycle Scope：

```text
ApplicationScope
RouteScope
ActivationScope
OperationScope
Plugin service context
```

规则：

- StateScope 释放时释放所有 state subscriptions。
- ActivationScope 中创建的 subscription 必须随 ViewModel 停用释放。
- RouteScope 中创建的 state 随路由离开释放。
- Plugin 相关 state 随插件停用或卸载释放。
- Application 级 state 随应用关闭释放。

## 13. StateSubscription

状态副作用不叫 `Effect`，建议称为 `StateSubscription` 或 State Reaction。公共 API 优先使用 subscription 语义。

```text
state.OnChange(...)
-> returns IDisposable / IStateSubscription
-> registered in StateScope / ActivationScope
```

规则：

- 所有 subscription 必须绑定 scope。
- subscription 抛异常进入 ErrorPolicy。
- subscription 错误不应导致 state 死亡。
- subscription 释放必须幂等。
- 插件 subscription 必须可被插件卸载流程找到并释放。

## 14. StateCollection

集合状态建议用 .NET 风格命名：

```text
IStateCollection<TKey, TItem>
```

能力：

- 按 key 添加或更新。
- 按 key 删除。
- 清空。
- 查询只读快照。
- 发出集合级变更通知。
- 支持 item 级版本。
- 支持 snapshot。

不建议直接暴露可变 `List<T>` / `Dictionary<TKey,T>`。集合变更必须通过状态 API，以便触发通知、诊断和快照。

## 15. StateSnapshot

StateSnapshot 用于：

- 测试断言。
- Route state 恢复。
- 应用关闭前保存 UI 状态。
- 插件状态保存。
- 调试诊断。

Snapshot 必须包含：

- State id。
- Owner module。
- Plugin id。
- Scope kind。
- Version。
- Schema version。
- Serialized value。
- Timestamp。

不是所有 state 都默认可持久化。需要显式声明 snapshot policy。

应用级共享状态的 snapshot 建议：

| 状态 | 建议 |
|---|---|
| Theme / Culture | 可持久化。 |
| Current user/auth runtime | 通常不直接持久化完整对象。 |
| Current workspace | 可持久化引用。 |
| Network status | 不持久化。 |
| Window layout policy | 可持久化。 |

## 16. 调度策略

State Core 不直接依赖 Avalonia Dispatcher。

State 调度必须遵守 Core 线程模型。线程模型、`IExecutionDispatcher` 和 `DispatchPolicy` 见：[Core Threading 设计](../core/threading.md)。

建议提供调度抽象：

```text
State change committed
-> Scheduler policy
-> Notify subscribers
-> Presentation updates UI
```

调度策略：

| 策略 | 说明 |
|---|---|
| Immediate | 当前线程通知。 |
| Queued | 排队后统一通知。 |
| Dispatcher | 切到 UI dispatcher。 |
| Background | 后台调度。 |

Presentation 负责把 Dispatcher 接入 State。Core 只定义抽象。

State 必须满足：

- `SetValue` 和 `Update` 原子化。
- 状态提交和订阅通知分离。
- 不在状态锁内调用订阅者。
- 相同 state key 的变更通知保持顺序。
- 应用级共享状态绑定 ApplicationScope。
- 插件状态绑定插件生命周期或插件贡献 lease。
- 推荐状态值使用 immutable 或 replace-only 风格。

## 17. 与 MVVM 集成

ViewModel 可以通过 DI 接收 `IApplicationState`、`IApplicationStateWriter` 或具体 state 服务。

ViewModel 可以持有：

```text
IReadOnlyState<T>
IWritableState<T>
IComputedState<T>
IStateCollection<TKey,TItem>
```

规则：

- ViewModel 构造函数可以接收 state，但不创建长期 subscription。
- State subscription 必须绑定 ActivationScope。
- State 变化可以桥接 `INotifyPropertyChanged`。
- ViewModel 停用时释放 subscription。
- State 错误不应杀死 ViewModel。

## 18. 与 Data / EventBus 集成

Data 模块负责异步请求和缓存策略。State 只接收最终状态更新。

```text
Command/Data request
-> OperationScope
-> result
-> state.Update(...)
```

EventBus 不应该自动监听所有 state 变化。状态变化发布事件必须显式声明，避免隐式循环。

## 19. 插件状态

插件状态必须隔离。

规则：

- 插件 state 创建在插件服务上下文或插件贡献产生的 Scope 中。
- 插件默认只能注入只读 `IApplicationState`。
- 插件获得 Host 授权后才可注入 `IApplicationStateWriter`。
- 即使暴露 writer，也必须经过 `StateAccessPolicy` 检查。
- 插件不能把内部 state 实例暴露给 Host 长期持有。
- 插件卸载前必须释放 state subscriptions。
- 插件 state snapshot 必须带 PluginId。
- 插件 state restore 必须经过插件版本兼容检查。

## 20. AOT / Source Generator

State 默认 AOT-first。

Generator/Analyzer 负责：

- 生成 state descriptor。
- 生成 state key manifest。
- 生成 snapshot serializer metadata。
- 生成 state manifest。
- 诊断不可序列化 snapshot 类型。
- 诊断未绑定 scope 的 subscription。
- 诊断运行时反射式 state discovery。
- 诊断 computed dependency 无法静态分析。
- 诊断插件 state 泄漏到 Host。

默认禁止：

- 运行时扫描程序集找 state。
- 运行时反射发现 snapshot 类型。
- 动态代理 state。
- expression-tree 依赖分析作为默认路径。

## 21. 错误策略

| 场景 | 默认处理 |
|---|---|
| Set/Update 失败 | 保留旧值，记录诊断。 |
| 应用级状态未注册 | 返回诊断错误，不创建隐式全局状态。 |
| 应用级状态未授权写入 | 拒绝写入，记录诊断。 |
| Computed 计算失败 | 保留上一有效值或标记 failed，记录诊断。 |
| Subscription 失败 | 记录错误，不杀死 state。 |
| Snapshot 保存失败 | 当前保存失败，不影响运行 state。 |
| Snapshot 恢复失败 | 使用默认值，记录诊断。 |
| Plugin state 释放失败 | 进入插件卸载错误聚合。 |

取消不是错误。OperationScope 取消后不应继续提交状态更新。

## 22. 测试策略

Testing 包应支持：

- 创建 TestStateScope。
- 创建 IReadOnlyState / IWritableState。
- 注入测试 `IApplicationState`。
- 断言应用级状态读写。
- 断言应用级状态访问策略。
- 断言 Version。
- 断言 OnChange 通知。
- 断言相等值不通知。
- 断言 computed cache / invalidation。
- 断言 subscription 自动释放。
- 断言 snapshot 保存和恢复。
- 断言插件 state 卸载。
- 断言调度策略。
