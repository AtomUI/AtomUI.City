# AtomUI.City.State 状态值设计

版本：v0.1
状态：正式初版
适用范围：`IReadOnlyState<T>`、`IWritableState<T>`、状态版本、相等比较、原子更新和状态定义

## 1. 定位

状态值是 State 模块的最小运行单元。

State 不把状态做成静态全局变量，也不要求开发者使用 Web 风格的 Store、Signal、Action 或 Reducer。第一版使用符合 .NET 习惯的强类型状态对象：

```text
IReadOnlyState<T>
IWritableState<T>
StateKey<T>
StateDefinition<T>
```

## 2. IReadOnlyState<T>

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

- `Value` 表示当前已提交值。
- `Version` 每次有效变更递增。
- 相等值不触发变更。
- 变化通知在状态提交后触发。
- 订阅必须可释放。
- 默认不暴露 Rx 类型。

## 3. IWritableState<T>

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
- 更新必须原子化。
- 更新失败时保留旧值。
- updater 中禁止执行 IO 或长耗时逻辑。

异步请求不直接进入 state。异步请求属于 Data、Command 或 OperationScope，完成后再提交状态更新。

## 4. StateKey<T>

应用级和模块级共享状态必须使用强类型 key。

```csharp
public readonly record struct StateKey<T>(string Name);
```

模块声明状态 key：

```csharp
public static class ThemeStates
{
    public static readonly StateKey<ThemeMode> CurrentTheme =
        new("AtomUI.City.Theme.Current");
}
```

命名规则：

- key 必须稳定。
- key 必须可诊断。
- 插件 key 必须带插件或 package 前缀。
- 不允许不同类型复用同一个 key 名称。

## 5. StateDefinition<T>

状态注册必须显式声明定义。

```csharp
context.States.Add(
    StateDefinition.Create(
        ThemeStates.CurrentTheme,
        defaultValue: ThemeMode.System,
        lifetime: StateLifetime.Application,
        access: StateAccessPolicy.HostWrite));
```

定义内容：

- Key。
- 默认值。
- 生命周期。
- Owner module。
- Plugin id。
- 访问策略。
- 快照策略。
- 相等比较策略。
- 诊断元数据。

## 6. 相等比较

相等值不触发通知。

默认策略：

- 值类型使用默认 equality。
- 引用类型使用 `EqualityComparer<T>.Default`。
- 集合状态不能依赖可变集合引用相等。
- 需要深比较时必须显式声明 comparer。

不允许通过原地修改可变对象绕过状态提交。推荐状态值使用 immutable 或 replace-only 风格。

## 7. 原子更新

`SetValue` 和 `Update` 必须满足：

- 状态提交原子化。
- 版本递增和当前值替换不可分离。
- 不在状态锁内调用订阅者。
- 更新失败不改变当前值。
- 取消后的 OperationScope 不应继续提交状态更新。

## 8. AOT 和 Source Generator

Source Generator 负责生成：

- state key manifest。
- state definition descriptor。
- snapshot serializer metadata。
- 重复 key 诊断。
- 不可序列化 snapshot 类型诊断。

默认禁止运行时扫描程序集找状态定义。

## 9. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 读取当前值 | Unit | 初始值和更新后值正确。 |
| SetValue | Unit | 值变化时返回 true，Version 递增。 |
| 相等值提交 | Unit | 返回 false，不递增 Version，不通知。 |
| Update 原子性 | Unit | updater 成功才替换值。 |
| updater 异常 | Unit | 旧值保留，诊断记录。 |
| 重复 StateKey | Analyzer/Generator | 输出稳定诊断。 |
| 不可序列化 snapshot 类型 | Analyzer/Generator | 输出构建期诊断。 |
