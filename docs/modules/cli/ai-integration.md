# CLI AI 集成设计

版本：v0.1
状态：正式初版
适用范围：AI-friendly 输出、JSON schema、plan/apply、inspect、doctor、explain、docs/tests gate 和 Agent 调用边界

## 1. 目标

CLI 必须从第一版支持 AI Agent 和自动化工具稳定调用。

AI 友好不是增加聊天能力，而是让命令输出、计划、诊断和执行行为可被机器可靠解析。

## 2. 核心原则

| 原则 | 说明 |
|---|---|
| Machine-readable first | 关键命令支持 JSON 输出。 |
| Dry-run first | 复杂操作支持 `--dry-run`。 |
| Plan/apply | 复杂写操作可先生成 plan，再执行。 |
| Stable schema | JSON schema、参数、exit code 稳定。 |
| Explainable diagnostics | 诊断有 code、原因、修复建议和文档链接。 |
| Workspace-aware | 能输出工作区结构化事实。 |
| Gate-aware | 能检查文档和测试门禁。 |
| No hidden prompt | 自动化模式下不出现隐式交互。 |

## 3. JSON 输出

统一结构：

```json
{
  "schemaVersion": "1.0",
  "command": "atomui city generate page",
  "success": true,
  "exitCode": 0,
  "diagnostics": [],
  "data": {},
  "suggestedActions": [],
  "documentationLinks": []
}
```

规则：

- `--json` 模式不能输出彩色文本。
- 所有错误也必须输出相同 envelope。
- `schemaVersion` 变更必须兼容或提升主版本。
- `diagnostics` 必须是数组。

## 4. Plan / Apply

复杂操作支持：

```bash
atomui city plan generate page Orders/List --route /orders --json
atomui city apply .city/plans/2026-06-11-generate-page.json
```

Plan 文件包含：

```json
{
  "schemaVersion": "1.0",
  "operationId": "2026-06-11-generate-page",
  "command": "atomui city generate page",
  "inputs": {},
  "changes": [
    {
      "type": "create",
      "path": "src/App/Routes/Orders/ListViewModel.cs"
    }
  ],
  "buildTargets": [],
  "testTargets": [],
  "docsRequired": [],
  "risks": [],
  "rollback": []
}
```

规则：

- Plan 生成不修改业务文件。
- Apply 必须校验 plan schema。
- Apply 前检查文件是否被外部修改。
- Apply 结果输出 diagnostics。
- destructive 操作必须有 rollback 描述。

## 5. Inspect

AI Agent 可以使用 inspect 命令获取事实。

```bash
atomui city inspect workspace --json
atomui city inspect module Sales --json
atomui city inspect manifest --json
```

inspect 命令只读，不修改文件。

## 6. Doctor 和 Explain

```bash
atomui city doctor --json
atomui city explain AUCCLI0201 --json
```

`explain` 输出：

- diagnostic code。
- 触发原因。
- 严重级别。
- 修复建议。
- 相关文档。
- 相关命令。

## 7. Agent 调用边界

CLI 可以：

- 生成符合模板的结构。
- 输出工作区事实。
- 生成 dry-run plan。
- 检查文档和测试门禁。
- 执行明确的 build/template/plugin 操作。

CLI 不应该：

- 根据自然语言自由生成业务代码。
- 绕过文档确认和测试门禁。
- 自动修改未确认的大范围文件。
- 在非交互模式下弹 prompt。
- 输出只有人能读懂的错误。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| JSON envelope | Unit | 成功和失败输出结构。 |
| plan schema | Unit | 必填字段、非法 schema。 |
| dry-run | Unit/CLI | 不写文件。 |
| apply | CLI | 校验 plan、写文件、输出诊断。 |
| inspect | Unit/CLI | 只读、结构化输出。 |
| explain | Unit | code、建议、文档链接。 |
