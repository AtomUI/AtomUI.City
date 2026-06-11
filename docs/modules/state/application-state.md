# AtomUI.City.State 应用级共享状态设计

版本：v0.1
状态：正式初版
适用范围：应用级共享状态、DI 访问、写入策略、状态注册表和访问边界

## 1. 定位

桌面软件天然存在应用级共享状态，例如主题、语言、当前用户、当前工作区、网络状态、窗口布局策略、全局忙碌状态和授权状态。

这些状态必须由 Host 管理并绑定 `ApplicationScope`，不能做成静态全局变量。

```text
ApplicationScope
-> Application State Registry
   -> Theme state
   -> Culture state
   -> Auth state
   -> Current workspace state
   -> Network status state
```

## 2. DI 入口

Host 启动时注册：

| 服务 | 职责 |
|---|---|
| `IApplicationState` | 读取和监听应用级共享状态。 |
| `IApplicationStateWriter` | 写入应用级共享状态。 |
| `IStateRegistry` | 底层状态注册表。 |
| `IStateScopeAccessor` | 获取当前生命周期状态作用域。 |

使用方通过构造函数注入状态服务，不允许访问静态对象。

## 3. 读取和监听 API

建议语义：

```csharp
public interface IApplicationState
{
    IReadOnlyState<T> Get<T>(StateKey<T> key);

    IDisposable OnChange<T>(
        StateKey<T> key,
        Action<StateChangedEventArgs<T>> handler);
}
```

监听必须绑定生命周期 Scope。

```csharp
activationScope.OnStateChanged(ThemeStates.CurrentTheme, args => { });
```

这样监听会随 ViewModel 停用自动释放。

## 4. 写入 API

建议语义：

```csharp
public interface IApplicationStateWriter
{
    IWritableState<T> GetWritable<T>(StateKey<T> key);

    bool Set<T>(StateKey<T> key, T value);

    bool Update<T>(StateKey<T> key, Func<T, T> updater);
}
```

`IApplicationState` 和 `IApplicationStateWriter` 分离，方便 Host 给插件或普通模块只暴露只读接口。

## 5. 写入策略

全局状态必须有写入规则。

| 策略 | 说明 |
|---|---|
| `ReadOnly` | 所有模块可读，只有 Owner 可初始化。 |
| `OwnerWrite` | 只有声明模块可写。 |
| `HostWrite` | Host 或授权服务可写。 |
| `AuthorizedWrite` | 通过权限或 capability 授权后可写。 |
| `PluginIsolated` | 插件只能写自己的状态分区。 |

默认不允许隐式创建应用级状态。读取未注册 key 必须返回诊断错误。

## 6. 生命周期

应用级状态绑定 `ApplicationScope`。

规则：

- ApplicationScope 停止时释放所有应用级状态订阅。
- 应用关闭前可以按 snapshot policy 保存状态。
- 运行时插件不能让自己的私有 state 升级成 Host 应用级状态。
- 当前用户、认证状态等安全敏感状态必须由 Security 模块控制写入。

## 7. 模块和插件边界

模块可以贡献应用级状态定义。

插件默认只能读取应用级状态。插件需要写入时必须同时满足：

- 插件 manifest 声明 capability。
- Host 授权该 capability。
- 目标状态的 `StateAccessPolicy` 允许写入。
- 写入过程进入诊断链路。

## 8. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| DI 读取应用状态 | Unit | 构造函数注入可读取注册状态。 |
| 未注册状态 | Unit | 返回诊断错误，不隐式创建。 |
| 只读入口 | Unit | `IApplicationState` 不能写入。 |
| Writer 写入 | Unit | 授权 writer 可更新状态。 |
| 写入策略拒绝 | Unit | 拒绝写入并记录诊断。 |
| ActivationScope 监听 | Unit | Scope 停用后自动解除订阅。 |
| 插件只读访问 | Unit | 插件默认不能写 Host 状态。 |
