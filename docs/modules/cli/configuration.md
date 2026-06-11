# CLI 配置设计

版本：v0.1
状态：正式初版
适用范围：CLI 配置来源、工作区配置、用户配置、模板源、插件源、profile 和优先级

## 1. 目标

CLI 配置用于控制命令默认值、模板源、插件源、输出模式和自动化行为。CLI 配置不能替代应用运行时配置。

## 2. 配置来源

优先级从高到低：

```text
Command line arguments
-> Environment variables
-> Workspace CLI config
-> User CLI config
-> Defaults
```

## 3. 工作区配置

建议路径：

```text
.atomui/city/cli.json
```

用途：

- 默认 `AtomUICityOutputRoot`。
- 默认 template source。
- 默认 plugin source。
- 默认 PluginProfile。
- docs/tests gate 策略。
- CI 输出策略。

## 4. 用户配置

用户级配置用于非项目特定设置：

- 默认 template source。
- trusted plugin source。
- 输出偏好。
- telemetry 开关，如果该能力存在。

用户配置路径遵循平台约定，具体实现由 CLI 模块定义。

## 5. 环境变量

建议：

| 变量 | 用途 |
|---|---|
| `ATOMUI_CITY_OUTPUT_ROOT` | 输出根目录。 |
| `ATOMUI_CITY_NON_INTERACTIVE` | 强制非交互。 |
| `ATOMUI_CITY_NO_COLOR` | 禁用颜色。 |
| `ATOMUI_CITY_TEMPLATE_SOURCE` | 模板源。 |
| `ATOMUI_CITY_PLUGIN_SOURCE` | 插件源。 |

## 6. 配置规则

- 命令行参数优先。
- 非交互模式不能从 prompt 补配置。
- 配置读取错误必须输出诊断。
- `--json` 模式下配置诊断也使用 JSON。
- CLI 配置不能写入应用运行时配置。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 优先级 | Unit | 参数覆盖环境变量和配置文件。 |
| 工作区配置 | Unit | `.atomui/city/cli.json` 读取。 |
| 环境变量 | Unit | non-interactive、no-color。 |
| 配置错误 | Unit | JSON 无效、字段无效。 |
| 只读命令 | Unit | inspect 不修改配置。 |
