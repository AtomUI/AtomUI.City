# CLI 项目创建设计

版本：v0.1
状态：正式初版
适用范围：`atomui city new app`、应用模板调用、参数校验、生成计划、测试项目和 Build 接入

## 1. 目标

项目创建命令调用 Templates 应用模板，生成符合 AtomUI.City 编程范式的最小可运行应用。

命令：

```bash
atomui city new app <AppName>
```

## 2. 参数

建议参数：

| 参数 | 说明 |
|---|---|
| `AppName` | 应用名。 |
| `--namespace` | RootNamespace。 |
| `--target-framework` | 目标框架。 |
| `--output` | 输出目录。 |
| `--include-tests` | 是否生成测试项目，默认 true。 |
| `--use-aot` | 是否启用 AOT 友好默认设置。 |
| `--use-dynamic-plugins` | 是否启用动态插件模式。 |
| `--sample` | 是否生成 sample，默认 false。 |

## 3. 执行流程

```text
Parse arguments
-> Validate template variables
-> Detect target directory
-> Build creation plan
-> Invoke application template
-> Write files
-> Run optional restore/build if requested
-> Emit diagnostics
```

## 4. 规则

- 默认生成测试项目。
- 默认不生成业务页面。
- 默认不启用动态插件。
- `--use-aot` 和 `--use-dynamic-plugins` 冲突时必须诊断。
- 用户命名空间不能以 `AtomUI.City` 开头。
- 生成项目必须引用 `AtomUI.City.Build`。

## 5. Dry-run

```bash
atomui city new app SalesClient --dry-run --json
```

输出将创建的目录、文件、项目引用、测试项目和风险。

## 6. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 参数校验 | Unit | 缺 AppName、非法 namespace。 |
| dry-run | CLI | 不写文件，输出 plan。 |
| 应用生成 | Template smoke | 文件结构完整。 |
| 测试项目 | Template smoke | FeatureTestMatrix 存在。 |
| Build 接入 | Build smoke | 生成项目可 build。 |
