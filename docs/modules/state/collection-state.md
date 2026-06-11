# AtomUI.City.State 集合状态设计

版本：v0.1
状态：正式初版
适用范围：keyed collection state、集合变更、item 版本、快照和不可变更新

## 1. 定位

集合状态使用 .NET 风格命名：

```text
IStateCollection<TKey, TItem>
```

它用于表示需要增量变化通知、快照和诊断的 keyed collection state。

## 2. 能力

第一版能力：

- 按 key 添加或更新。
- 按 key 删除。
- 清空。
- 查询只读快照。
- 发出集合级变更通知。
- 支持 item 级版本。
- 支持 snapshot。

不建议直接暴露可变 `List<T>` 或 `Dictionary<TKey,T>`。

## 3. 更新规则

集合变更必须通过状态 API，以便触发通知、诊断和快照。

规则：

- 不允许外部拿到可变内部集合。
- item 更新必须产生明确 change record。
- 相同 key 的变更保持顺序。
- 批量更新应合并通知。
- 更新失败时保留旧集合。

## 4. 变更记录

集合变更记录应表达：

- Added。
- Updated。
- Removed。
- Cleared。
- Reset。

每条记录包含 key、旧值、新值、集合版本和 item 版本。

## 5. Snapshot

集合 snapshot 必须包含：

- collection key。
- schema version。
- collection version。
- item count。
- serialized items。
- item version metadata。

大型集合应支持分页或分块 snapshot，避免一次性占用过多内存。

## 6. AOT 和 Source Generator

Generator/Analyzer 负责：

- 生成 collection descriptor。
- 生成 item serializer metadata。
- 诊断可变集合直接暴露。
- 诊断缺少 key comparer。
- 诊断 snapshot item 不可序列化。

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| AddOrUpdate | Unit | 添加和更新产生正确 change record。 |
| Remove | Unit | 删除后快照不包含 item。 |
| Clear | Unit | 清空产生 clear 记录。 |
| 只读快照 | Unit | 外部不能修改内部集合。 |
| 批量更新 | Unit | 通知合并且顺序稳定。 |
| item version | Unit | item 更新递增版本。 |
| collection snapshot | Unit | 保存和恢复集合。 |
