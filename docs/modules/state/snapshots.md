# AtomUI.City.State 快照设计

版本：v0.1
状态：正式初版
适用范围：StateSnapshot、持久化策略、恢复、版本兼容和测试断言

## 1. 定位

`StateSnapshot` 用于保存和恢复状态，也用于测试断言和诊断。

典型用途：

- 测试断言。
- Route state 恢复。
- 应用关闭前保存 UI 状态。
- 插件状态保存。
- 调试诊断。

## 2. Snapshot 内容

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

## 3. Snapshot Policy

应用级共享状态建议：

| 状态 | 建议 |
|---|---|
| Theme / Culture | 可持久化。 |
| Current user/auth runtime | 通常不直接持久化完整对象。 |
| Current workspace | 可持久化引用。 |
| Network status | 不持久化。 |
| Window layout policy | 可持久化。 |

策略必须说明：

- 是否持久化。
- 存储范围。
- 序列化方式。
- schema version。
- 是否允许插件迁移。

## 4. 恢复流程

恢复流程：

```text
Load snapshot
-> validate state id
-> validate owner/module/plugin
-> validate schema version
-> deserialize value
-> apply migration if needed
-> commit state or fallback default value
```

恢复失败不应阻止应用启动，默认使用初始值并记录诊断。

## 5. 插件快照

插件 state snapshot 必须带 PluginId。

插件 state restore 必须经过：

- 插件版本兼容检查。
- 插件 schema migration 检查。
- Host trust policy 检查。
- 插件已启用检查。

插件卸载后，其 snapshot 可以保留但不能被 Host 直接恢复为 Host 状态。

## 6. AOT 和 Source Generator

Generator 负责：

- 生成 snapshot serializer metadata。
- 生成 state snapshot manifest。
- 诊断不可序列化类型。
- 诊断缺少 schema version 的持久化状态。

默认禁止运行时反射发现 snapshot 类型。

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 保存 snapshot | Unit | 输出包含 state id、version、schema。 |
| 恢复 snapshot | Unit | 值正确恢复。 |
| schema 不兼容 | Unit | 使用默认值并记录诊断。 |
| 不持久化状态 | Unit | 不写入持久化 snapshot。 |
| 插件 snapshot | Unit | 带 PluginId 和版本信息。 |
| 反射 serializer | Analyzer/Generator | Strict AOT 下诊断。 |
