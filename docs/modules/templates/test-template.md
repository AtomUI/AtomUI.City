# 测试模板设计

版本：v0.1
状态：正式初版
适用范围：测试项目、功能点测试矩阵、TestHost、单元测试、集成测试和模板默认测试入口

## 1. 目标

测试模板用于落实 AtomUI.City 的功能点测试门禁。

规则：

- 每个功能点必须有单元测试。
- 集成测试不能替代单元测试。
- 无法单元测试的功能点必须说明原因并提供替代测试。

## 2. 默认结构

```text
tests/<ProjectName>.Tests/
  FeatureTestMatrix.md
  <FeatureName>Tests.cs
```

可选结构：

```text
tests/<ProjectName>.FrameworkIntegrationTests/
tests/<ProjectName>.PlatformIntegrationTests/
```

## 3. FeatureTestMatrix

默认生成：

```text
| 功能点 | 测试类型 | 测试工具 | 必测场景 | 完成门禁 |
|---|---|---|---|---|
```

规则：

- 模板生成的功能点必须自动写入矩阵。
- 新增页面、模块、插件时必须补矩阵。
- 集成测试条目不能替代单元测试条目。

## 4. TestHost

测试项目默认引用：

- `AtomUI.City.Testing`
- 被测项目。

默认生成 TestHost 使用入口：

- application smoke。
- module host。
- routing host。
- plugin host，如果是插件模板。

## 5. 单元测试默认项

模板应生成最小单元测试：

- 构造测试。
- manifest/generator 输入测试。
- lifecycle 或 activation 测试。
- diagnostics 测试，如果模板生成诊断行为。

## 6. 集成测试默认项

可选生成：

- Framework integration test。
- Platform integration test。
- Template smoke test。

默认不生成真实 UI 平台集成测试，除非用户显式选择。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 测试项目生成 | Smoke | restore、build。 |
| FeatureTestMatrix | Unit | 文件存在，包含模板功能点。 |
| TestHost 引用 | Unit/Build | 测试项目能使用 TestHost。 |
| 单元测试入口 | Unit | 默认测试可运行。 |
| 集成测试开关 | Build | 显式启用时生成对应项目。 |
