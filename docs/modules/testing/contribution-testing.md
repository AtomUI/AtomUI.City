# Contribution 测试设计

版本：v0.1
状态：正式初版
适用范围：ContributionRequest、ContributionLease、Registry、冲突、撤销、插件来源和诊断

## 1. 目标

Contribution 是模块和插件向 Host 增加能力的统一入口。Testing 必须能验证贡献创建、校验、冲突、撤销和来源追踪。

## 2. ContributionTestRegistry

测试 registry 负责：

- 记录 ContributionRequest。
- 记录 accepted/rejected。
- 创建测试 Lease。
- 记录 revoke 顺序。
- 模拟冲突。
- 模拟 revoke 失败。
- 生成诊断事件。

## 3. 必测场景

必须覆盖：

- Contribution 被接受。
- Contribution 被拒绝。
- 同一插件重复 Contribution。
- 不同插件 Contribution 冲突。
- Lease 创建。
- Lease revoke。
- Lease 反向撤销顺序。
- Revoke 失败聚合。
- PluginId、ModuleId、ContributionId 来源追踪。

## 4. 插件集成

插件测试必须断言：

- 插件启用后 Contribution 可见。
- 插件停用后 Contribution 全部撤销。
- 插件卸载前无 active lease。
- 撤销失败导致 `UnloadPending` 或错误聚合。

## 5. 诊断断言

必须断言：

- ContributionId。
- PluginId。
- ModuleId。
- Registry name。
- 冲突对象。
- 错误策略结果。

## 6. 测试要求

每个接收插件贡献的 registry 都必须有单元测试证明其 Contribution 可撤销。
