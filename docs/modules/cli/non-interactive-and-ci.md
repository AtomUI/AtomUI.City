# CLI 非交互和 CI 设计

版本：v0.1
状态：正式初版
适用范围：非交互模式、CI 模式、exit code、JSON 输出、确认策略和自动化安全

## 1. 目标

CLI 必须适合 CI 和 AI Agent 自动化执行。自动化模式下不能出现隐藏 prompt、不可预测输出或不稳定 exit code。

## 2. 非交互模式

启用方式：

```bash
atomui city build --non-interactive
```

或环境变量：

```text
ATOMUI_CITY_NON_INTERACTIVE=true
```

规则：

- 不允许 prompt。
- 缺少必填参数直接失败。
- 需要确认的操作必须要求 `--yes` 或 apply plan。
- 错误必须输出诊断。

## 3. CI 模式

CI 模式建议组合：

```bash
atomui city docs check --json --non-interactive --no-color
atomui city tests check --json --non-interactive --no-color
atomui city build --json --non-interactive --no-color
```

规则：

- 输出可被日志系统保存。
- JSON 输出不混入普通文本。
- exit code 稳定。
- 不使用交互选择。

## 4. Exit Code

建议：

| Exit Code | 含义 |
|---|---|
| `0` | 成功。 |
| `1` | 一般错误。 |
| `2` | 参数错误。 |
| `3` | 文档门禁失败。 |
| `4` | 测试门禁失败。 |
| `5` | Build 失败。 |
| `6` | Template 失败。 |
| `7` | Plugin 操作失败。 |
| `8` | Plan/apply 失败。 |

## 5. 确认策略

写操作分级：

| 操作 | 自动化要求 |
|---|---|
| 创建文件 | 支持 dry-run，非交互可执行。 |
| 修改文件 | 支持 plan/apply 或 `--yes`。 |
| 删除文件 | 必须 dry-run 或 apply plan。 |
| 插件 install/update/remove | 必须支持 plan/apply。 |
| 清理 output | 必须显式命令和确认。 |

## 6. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| non-interactive | Unit | 不 prompt。 |
| missing args | Unit | 失败并输出诊断。 |
| exit code | Unit | docs/tests/build/plugin failure。 |
| JSON only | Unit | 无彩色文本混入。 |
| confirmation | Unit | destructive 操作需要确认。 |
