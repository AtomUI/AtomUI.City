# AtomUI.City.State 插件集成设计

版本：v0.1
状态：正式初版
适用范围：插件状态隔离、状态写入授权、状态撤销、快照迁移和卸载安全

## 1. 定位

插件可以使用 State，但插件状态必须可隔离、可撤销、可释放、可诊断。

插件不能通过 State 绕过 Host 生命周期和权限边界。

## 2. 插件状态创建

插件 state 创建在：

- 插件服务上下文。
- 插件贡献产生的 Scope。
- 插件拥有的 RouteScope 或 ActivationScope。

插件默认只能注入只读 `IApplicationState`。

## 3. 写入授权

插件需要写入应用级状态时必须满足：

- 插件 manifest 声明 capability。
- Host 授权 capability。
- 目标状态允许 `AuthorizedWrite` 或 `PluginIsolated`。
- 写入过程写入诊断。

即使暴露 writer，也必须经过 `StateAccessPolicy` 检查。

## 4. 泄漏约束

禁止：

- 插件把内部 state 实例暴露给 Host 长期持有。
- Host 静态缓存插件私有 state 类型。
- 插件 subscription 脱离插件生命周期。
- 插件 state 使用全局静态变量保存当前值。

## 5. 停用和卸载

插件停用流程：

```text
Stop new plugin state access
-> cancel plugin operations
-> dispose plugin subscriptions
-> snapshot plugin state if policy allows
-> revoke state contributions
-> release plugin state registry
```

释放失败进入插件卸载错误聚合。

## 6. Snapshot 和迁移

插件 state snapshot 必须带：

- PluginId。
- Plugin version。
- State schema version。
- Owner module。

恢复前必须检查插件版本兼容。降级或回滚时必须按插件声明的迁移策略执行。

## 7. AOT 和 Source Generator

Generator/Analyzer 负责：

- 生成插件 state descriptor。
- 生成插件 snapshot manifest。
- 诊断插件私有类型泄漏。
- 诊断未绑定插件生命周期的 state。

## 8. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 插件只读状态访问 | Unit | 默认只读。 |
| 插件授权写入 | Unit | capability 允许后可写。 |
| 未授权写入 | Unit | 拒绝并记录诊断。 |
| 插件停用释放 | Unit | subscriptions 和 registry 被释放。 |
| 插件 snapshot | Unit | 带 PluginId 和版本。 |
| 插件 state 泄漏 | Analyzer/Generator | 输出稳定诊断。 |
