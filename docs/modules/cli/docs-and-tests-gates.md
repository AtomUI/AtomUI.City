# CLI 文档和测试门禁命令设计

版本：v0.1
状态：正式初版
适用范围：`docs check`、`tests check`、文档先行检查、测试矩阵检查和功能点测试门禁

## 1. 目标

CLI 必须能检查 AtomUI.City 的工程治理规则，帮助开发者和 AI Agent 在实现前发现文档和测试缺口。

## 2. 命令

```bash
atomui city docs check
atomui city tests check
```

## 3. Docs Check

检查：

- 模块是否有 overview。
- 复杂模块是否有 detailed design。
- 文档链接是否有效。
- 模块文档是否包含测试矩阵。
- 功能点是否进入测试矩阵。
- 公共 API 是否缺设计文档。
- 文档中是否存在明显占位内容。

## 4. Tests Check

检查：

- 功能点是否有单元测试。
- 是否存在只用集成测试替代单元测试的情况。
- 生命周期功能是否有取消和释放断言。
- 插件功能是否有 Lease、Operation、UnloadPending 断言。
- source generator 是否有 generator test。
- analyzer 是否有 analyzer test。
- Build target 是否有 build test。

## 5. 输出

JSON diagnostics 示例：

```json
{
  "code": "AUCCLI0201",
  "severity": "Error",
  "message": "Feature test matrix is missing.",
  "details": {
    "module": "Routing",
    "document": "docs/modules/routing/detailed-design.md"
  },
  "suggestedActions": [
    "Add a test matrix row for the feature."
  ],
  "documentationLinks": [
    "docs/modules/testing/feature-test-gate.md"
  ]
}
```

## 6. 规则

- check 命令默认只读。
- check 命令不自动修复。
- `--json` 输出可被 CI 和 AI Agent 解析。
- 失败时返回非零 exit code。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| docs check | Unit | 缺 overview、缺矩阵、断链。 |
| tests check | Unit | 缺单测、缺释放断言。 |
| JSON diagnostics | Unit | code、details、links。 |
| exit code | Unit | 有错误返回非零。 |
| read-only | Unit | check 不写文件。 |
