# CLI 诊断设计

版本：v0.1
状态：正式初版
适用范围：CLI 错误码、诊断 envelope、human output、JSON output、doctor、explain 和跨模块诊断透传

## 1. 目标

CLI 诊断必须稳定、可解释、可被机器解析。错误不能只是一段人类可读文本。

## 2. 诊断 Envelope

```json
{
  "code": "AUCCLI0001",
  "severity": "Error",
  "message": "Invalid command argument.",
  "details": {},
  "suggestedActions": [],
  "documentationLinks": []
}
```

## 3. 诊断前缀

| 前缀 | 来源 |
|---|---|
| `AUCCLI` | CLI 自身错误。 |
| `AUCBLD` | Build 透传错误。 |
| `AUCTPL` | Templates 透传错误。 |
| `AUCPLG` | PluginSystem 透传错误。 |
| `AUCGEN` | Source Generator 透传错误。 |
| `AUCANL` | Analyzer 透传错误。 |

## 4. CLI 错误码建议

| Code | 含义 |
|---|---|
| `AUCCLI0001` | 参数无效。 |
| `AUCCLI0002` | 工作区无法识别。 |
| `AUCCLI0003` | 非交互模式缺少必填参数。 |
| `AUCCLI0101` | 模板调用失败。 |
| `AUCCLI0102` | Build 调用失败。 |
| `AUCCLI0201` | 功能点测试矩阵缺失。 |
| `AUCCLI0202` | 功能点单元测试缺失。 |
| `AUCCLI0301` | Plan schema 无效。 |
| `AUCCLI0302` | Apply 时文件状态不匹配。 |
| `AUCCLI0401` | 插件操作被 PluginSystem 拒绝。 |

## 5. Doctor

```bash
atomui city doctor --json
```

检查：

- workspace。
- package versions。
- Build 配置。
- Templates 可用性。
- Plugin profile。
- output 目录。
- manifest 状态。
- docs/tests gate。

## 6. Explain

```bash
atomui city explain AUCCLI0201 --json
```

输出：

- code。
- severity。
- 触发条件。
- 修复建议。
- 相关文档。
- 相关命令。

## 7. Human Output

人类输出可以使用颜色和表格，但必须由同一诊断模型渲染。

`--no-color` 禁用颜色。

`--json` 禁止输出非 JSON 文本。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| diagnostic envelope | Unit | code、severity、details。 |
| JSON output | Unit | 成功和失败。 |
| human output | Golden output | no-color、verbosity。 |
| doctor | Unit/CLI | workspace、manifest、gates。 |
| explain | Unit | 已知和未知 code。 |
| passthrough | Unit | Build/Templates/Plugin code 不丢失。 |
