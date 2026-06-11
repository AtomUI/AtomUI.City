# AtomUI.City.State 诊断与测试设计

版本：v0.1
状态：正式初版
适用范围：State 诊断字段、错误码、测试工具、测试矩阵和完成门禁

## 1. 定位

State 的每个功能点必须可测试。涉及生命周期、线程、插件和释放的功能点必须有释放断言。

## 2. 诊断字段

State 诊断至少包含：

- StateKey。
- State type。
- Owner module。
- PluginId。
- ScopeId。
- Version。
- OperationId。
- DispatchPolicy。
- Error code。
- Snapshot schema version。

## 3. 错误策略

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

## 4. 测试工具

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

## 5. 模块测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| state set/get | Unit | 值和版本正确。 |
| 相等值不通知 | Unit | 不递增版本，不触发通知。 |
| computed invalidation | Unit | 依赖变化后重新计算。 |
| subscription dispose | Unit | 释放后不再通知。 |
| Scope stop | Unit | Scope 停止后不再通知。 |
| application state DI | Unit | 可通过 DI 读取和写入授权状态。 |
| access policy | Unit | 未授权写入被拒绝。 |
| dispatch policy | Unit | fake dispatcher 可确定推进。 |
| snapshot save/restore | Unit | 版本、schema 和值正确。 |
| plugin cleanup | Unit | 插件停用释放订阅和状态。 |
| source generator manifest | Generator | 输出稳定 state descriptor。 |
| analyzer diagnostics | Analyzer | 未绑定 Scope、动态发现、泄漏等诊断稳定。 |
