# 0009 每个公开功能点必须有单元测试

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 是框架项目。生命周期、模块、插件、状态、事件、路由和构建规则一旦公开，就会长期影响使用者。只依赖集成测试无法保证每个功能点的边界、错误策略和释放行为稳定。

## 决策

AtomUI.City 采用功能点测试门禁。

每个公开功能点必须有对应单元测试。集成测试不能替代单元测试。

无法单元测试的功能点必须在测试矩阵中说明原因，并提供 contract test、framework integration test、platform integration test、analyzer test 或 build test 替代。

## 影响

正向影响：

- 功能边界可验证。
- 生命周期和释放行为可回归。
- 文档中的功能点能追踪到测试。

成本：

- 每个功能点开工前需要先补测试矩阵。
- 测试工具必须先行建设。

## 执行约束

- 文档治理见 `docs/engineering/documentation-governance.md`。
- Testing 设计见 `docs/modules/testing/detailed-design.md`。
- 功能点门禁见 `docs/modules/testing/feature-test-gate.md`。
