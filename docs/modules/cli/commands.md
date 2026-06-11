# CLI 命令模型设计

版本：v0.1
状态：正式初版
适用范围：`atomui city` 命令树、命名规则、参数约定、通用选项和命令边界

## 1. 目标

命令模型必须稳定、清晰、可被人和 AI Agent 使用。

用户入口：

```bash
atomui city <command>
```

## 2. 命名规则

规则：

- 顶层命令组固定为 `atomui city`。
- 动作用动词：`new`、`generate`、`build`、`pack`、`publish`、`inspect`。
- 资源用名词：`module`、`page`、`plugin`、`test`、`manifest`。
- 插件管理命令位于 `plugin` 子命令下。
- 检查类命令使用 `check`。
- 解释诊断使用 `explain`。

## 3. 命令树

```text
atomui city
  new app
  generate module
  generate page
  generate plugin
  generate test
  generate config
  generate localization
  build
  pack
  publish
  plugin list
  plugin inspect
  plugin install
  plugin update
  plugin remove
  plugin enable
  plugin disable
  plugin doctor
  inspect workspace
  inspect project
  inspect module
  inspect route
  inspect manifest
  docs check
  tests check
  doctor
  explain <diagnostic-code>
  plan <operation>
  apply <plan-file>
```

## 4. 通用选项

所有关键命令支持：

| 选项 | 说明 |
|---|---|
| `--json` | 输出机器可读 JSON。 |
| `--pretty` | JSON 格式化输出。 |
| `--no-color` | 禁止颜色。 |
| `--verbosity` | `quiet`、`normal`、`detailed`、`diagnostic`。 |
| `--non-interactive` | 禁止 prompt。 |
| `--dry-run` | 只输出计划，不写文件。 |
| `--yes` | 对需要确认的操作显式确认。 |
| `--working-directory` | 指定工作目录。 |

## 5. 参数规则

规则：

- 必填参数缺失时返回参数错误。
- 非交互模式不提示补参。
- 参数名稳定，不随输出文本变化。
- 文件路径可相对工作目录解析。
- `--json` 模式错误也输出 JSON。

## 6. 命令边界

CLI 不做：

- 不实现 MSBuild task。
- 不维护模板文件内容。
- 不直接运行插件业务代码。
- 不扫描运行时程序集推断框架结构。
- 不绕过文档和测试门禁。

CLI 做：

- 调用 Templates。
- 调用 Build。
- 读取 manifest。
- 读取 PluginSystem metadata。
- 输出诊断。
- 生成 plan。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 命令解析 | Unit | 每个命令路径解析。 |
| 通用选项 | Unit | json、dry-run、non-interactive。 |
| 非法参数 | Unit | 缺失、未知、冲突参数。 |
| 命令边界 | Unit | generate 不直接调用 Build task 实现。 |
| 帮助输出 | Golden output | 命令帮助稳定。 |
