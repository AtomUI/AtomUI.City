# AtomUI.City.Presentation UI Dispatcher 设计

版本：v0.1
状态：正式初版
适用范围：`IUiDispatcher`、UI thread access、投递、停止、异常和测试

## 1. 定位

Presentation 提供 `IUiDispatcher` 的 Avalonia 实现。

Core 只定义调度抽象，不依赖 Avalonia Dispatcher。

## 2. 基本规则

- `CheckAccess` 映射 Avalonia UI thread access。
- `InvokeAsync` 返回执行结果或异常。
- `PostAsync` 表示异步投递。
- UI runtime 未 ready 时，按 Host 策略等待或返回明确错误。
- UI runtime stopping 后拒绝新投递。
- Dispatcher callback 异常进入 ErrorPolicy。
- 插件不能长期静态保存 dispatcher callback。

调度策略见：[Core Threading 设计](../core/threading.md)。

## 3. 与 State/EventBus 集成

State 和 EventBus 不直接依赖 Avalonia。

```text
State/EventBus DispatchPolicy.UiThread
-> IUiDispatcher
-> Presentation Avalonia dispatcher
-> UI callback
```

UI callback 必须绑定 Scope。Scope 停止后，未执行 callback 应取消或跳过。

## 4. 错误策略

| 场景 | 默认处理 |
|---|---|
| UI dispatcher 未 ready | 等待 ready 或返回明确错误。 |
| UI dispatcher stopping | 拒绝投递。 |
| callback 抛异常 | 进入 Presentation diagnostics 和 ErrorPolicy。 |
| callback 所属 Scope 已停止 | 跳过并记录 trace 级诊断。 |

## 5. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| CheckAccess | Unit | UI 线程和非 UI 线程结果正确。 |
| InvokeAsync | Unit | 返回 callback 结果。 |
| PostAsync | Unit | fake dispatcher 可 drain。 |
| stopping 拒绝 | Unit | 停止后投递失败。 |
| callback 异常 | Unit | 诊断记录。 |
| Scope 停止 | Unit | 停止后的 callback 不执行。 |
