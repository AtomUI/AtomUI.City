# 发布流程规范

版本：v0.1
状态：正式初版
适用范围：发布流程、发布前验证、包发布、模板发布和发布记录

## 1. 目标

发布流程必须保证文档、实现、测试、包和模板一致。没有文档确认和测试门禁通过，不允许发布。

## 2. 发布前检查

发布前必须完成：

- 文档链接检查。
- 设计文档和实现一致性检查。
- full build。
- full test。
- package validation。
- template smoke test。
- plugin lifecycle smoke test。
- platform integration test。
- analyzer/generator tests。
- license 检查。

## 3. 发布流程

```text
Confirm docs
-> run full verification
-> pack packages
-> validate package layout
-> generate release notes
-> tag release
-> publish packages
-> publish templates/tool
-> archive diagnostics
```

## 4. 暂停条件

出现以下情况必须暂停发布：

- 文档和实现不一致。
- 公共 API 未记录。
- 单元测试缺失。
- 集成测试失败。
- AOT/trimming 诊断未处理。
- package layout 不符合规范。
- 插件安装/卸载 smoke test 失败。
- License 元数据不正确。

## 5. Release notes

Release notes 至少包含：

- 版本号。
- 新增功能。
- 破坏性变更。
- 修复。
- 已知限制。
- 迁移说明。
- 插件 API 兼容性说明。

## 6. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| full build | Build | solution build 成功。 |
| full test | Test | 全部测试通过。 |
| package validation | Pack test | 包布局和元数据有效。 |
| template smoke | Template test | 生成项目可 build/test。 |
| plugin smoke | Integration | 插件安装、启用、停用、卸载链路可跑。 |
| release notes | Docs | 版本变更可追踪。 |
