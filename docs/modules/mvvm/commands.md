# AtomUI.City.Mvvm Commands 设计

版本：v0.1
状态：正式初版
适用范围：Command、Async Command、OperationScope、执行状态、错误策略、权限联动、组合命令和测试支持。

## 1. 定位

Command 是 ViewModel 暴露用户动作的主要方式。

AtomUI.City.Mvvm 不重新发明基础命令类型，第一版沿用 `CommunityToolkit.Mvvm` 的命令模型，并在其上补充生命周期、执行状态、取消、错误和诊断。

## 2. 底层命令类型

默认使用：

```text
IRelayCommand
IAsyncRelayCommand
RelayCommand
AsyncRelayCommand
```

不引入 `CityCommand` / `CityAsyncCommand` 这类命名。

## 3. OperationScope

每次 async command 执行都应创建 OperationScope。

OperationScope 负责：

- 提供 CancellationToken。
- 记录 Command 执行诊断。
- 关联当前 ViewModel。
- 关联当前 ActivationScope。
- 记录执行耗时和结果。
- 将错误交给 ErrorPolicy。

执行流程：

```text
CanExecute check
-> Create OperationScope
-> Execute command
-> Capture result
-> Capture error or cancellation
-> Dispose OperationScope
```

Command 失败不应导致 ViewModel 死亡。

## 4. Command 状态

Command 需要标准化运行状态：

| 状态 | 说明 |
|---|---|
| `CanExecute` | 当前是否可执行。 |
| `IsExecuting` | 当前是否正在执行。 |
| `LastResult` | 最近一次执行结果。 |
| `LastError` | 最近一次失败信息。 |
| `CancellationToken` | 当前执行取消令牌。 |

这些状态应可被 UI、Diagnostics 和 Testing 读取。

## 5. 权限和路由联动

Command 可执行状态可以接入：

- Security 权限。
- Routing 当前状态。
- ViewModel active 状态。
- Validation 状态。
- Operation 正在执行状态。

Security 和 Routing 不由 Mvvm 实现。Mvvm 只提供命令状态接入点。

## 6. CompositeCommand / CommandGroup

Mvvm 应支持组合命令，用于菜单、工具栏、全局快捷键和 Shell 级命令。

组合命令规则：

- 可以聚合多个子命令。
- 只执行当前 active 上下文中的子命令。
- 子命令可随 ActivationScope 注册和释放。
- 可执行状态由 active 子命令共同决定。
- 执行结果和错误需要进入 OperationScope 诊断。

建议类型可以命名为 `CompositeCommand` 或 `CommandGroup`，具体命名在实现前再定。

## 7. 取消策略

Command 取消不是错误。

取消来源：

- 用户取消。
- ActivationScope 停用。
- Route 离开。
- Plugin 停用。
- Host shutdown。

Command 必须区分 canceled、failed 和 completed。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| CanExecute 失败 | 记录诊断，命令不可执行。 |
| Execute 抛异常 | Operation failed，不杀死 ViewModel。 |
| Execute 被取消 | Operation canceled，不作为失败统计。 |
| CompositeCommand 子命令失败 | 聚合结果，继续策略由 command policy 决定。 |

## 9. AOT / Source Generator

Generator/Analyzer 可负责：

- 生成 command descriptor。
- 诊断 command 未绑定 ActivationScope。
- 诊断 async command 缺少取消令牌接入。
- 诊断 command id 重复。
- 输出 command manifest，供菜单、工具栏、快捷键和测试使用。

## 10. 测试策略

Testing 包应支持：

- 执行 command 并断言 OperationScope。
- 断言 `IsExecuting`。
- 断言成功、失败和取消结果。
- 断言权限状态影响 `CanExecute`。
- 断言 CompositeCommand active 行为。
- 断言错误诊断。
