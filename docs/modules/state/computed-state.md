# AtomUI.City.State 计算状态设计

版本：v0.1
状态：正式初版
适用范围：派生状态、依赖声明、缓存、失效、错误处理和 AOT 约束

## 1. 定位

`IComputedState<T>` 表达由一个或多个状态派生出来的只读状态。

```text
Dependencies
-> Compute
-> Cache value
-> Invalidate on dependency change
-> Notify subscribers
```

计算状态用于减少 ViewModel 中重复派生属性和手工通知逻辑。

## 2. API 语义

计算状态是只读状态。

```csharp
public interface IComputedState<T> : IReadOnlyState<T>
{
}
```

计算函数不应执行 IO，也不应启动异步任务。异步结果应先进入 Data 或 OperationScope，再提交普通状态。

## 3. 依赖声明

依赖必须显式声明或由 source generator 静态分析。

允许：

```text
ComputedState
  Dependencies:
    ThemeStates.CurrentTheme
    AuthStates.CurrentPrincipal
```

默认不允许：

- 运行时反射扫描依赖。
- expression-tree 依赖分析作为默认路径。
- 通过闭包捕获未知状态对象。

## 4. 缓存和失效

规则：

- 计算结果应缓存。
- 依赖变化后标记失效。
- 有订阅或读取时才重新计算。
- 计算结果相等时不通知。
- 依赖通知顺序应保持确定性。

## 5. 错误策略

计算异常不能杀死依赖状态。

默认处理：

- 保留上一有效值，或进入 failed 状态。
- 记录 Diagnostics。
- 通知订阅者计算失败状态。
- 不重复无限重算同一个失败状态。

## 6. 生命周期

计算状态绑定创建它的 StateScope。

规则：

- Scope 停止后不能继续计算。
- 依赖订阅随计算状态释放。
- 插件计算状态不能被 Host 长期持有。
- RouteScope 或 ActivationScope 中的计算状态随对应 Scope 释放。

## 7. AOT 和 Source Generator

Generator 负责：

- 生成 computed descriptor。
- 生成依赖列表。
- 诊断无法静态分析的依赖。
- 诊断 computed 捕获插件私有类型泄漏到 Host。

Analyzer 应提示：

- 计算函数执行 IO。
- 计算函数返回可变集合且未声明比较策略。
- 计算状态缺少生命周期绑定。

## 8. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 初次计算 | Unit | 首次读取返回计算值。 |
| 缓存 | Unit | 依赖未变时不重复计算。 |
| 依赖失效 | Unit | 依赖变化后重新计算。 |
| 相等结果 | Unit | 结果相等时不通知。 |
| 计算异常 | Unit | 保留旧值或 failed 状态，诊断记录。 |
| Scope 释放 | Unit | 释放后不再计算或通知。 |
| 依赖无法静态分析 | Analyzer/Generator | 输出稳定诊断。 |
