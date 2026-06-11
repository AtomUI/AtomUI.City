# AtomUI.City.Cli

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Cli` 负责项目创建、模块生成、路由生成、构建命令、模板调用和诊断输出。

Cli 是使用者接触 AtomUI.City 工程化能力的主要入口。

用户命令入口统一为：

```bash
atomui city <command>
```

CLI 同时面向开发者、CI 和 AI Agent。关键命令必须支持机器可读输出、dry-run、plan/apply、稳定 exit code 和非交互模式。

## 边界

Cli 负责：

- 命令定义。
- 参数解析。
- 模板调用。
- 项目生成。
- 模块生成。
- 诊断报告。
- 终端输出体验。
- AI 友好 JSON 输出。
- 工作区结构化 inspect。
- 文档和测试门禁检查。
- plan/apply 执行模式。

Cli 不负责：

- 模板自身内容。
- MSBuild 任务实现。
- 运行时框架逻辑。
- 运行时插件加载。
- 业务代码自由生成。
- 真实部署平台。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | CLI 定位、命令模型、AI 友好能力、模板调用、构建调用、插件管理、诊断、配置、非交互模式和测试。 |
| [commands.md](commands.md) | `atomui city` 命令树、命名规则、参数约定、通用选项和命令边界。 |
| [ai-integration.md](ai-integration.md) | AI-friendly 输出、JSON schema、plan/apply、inspect、doctor、explain、docs/tests gate 和 Agent 调用边界。 |
| [project-creation.md](project-creation.md) | `atomui city new app`、应用模板调用、参数校验、生成计划、测试项目和 Build 接入。 |
| [generation.md](generation.md) | module、page、plugin、test、config、localization 的生成命令和模板调用。 |
| [build-commands.md](build-commands.md) | `atomui city build`、`pack`、`publish` 命令、Build 调用、诊断透传和输出规则。 |
| [plugin-commands.md](plugin-commands.md) | 插件 list、inspect、install、update、remove、enable、disable、doctor 命令和 PluginSystem metadata 集成。 |
| [inspect-commands.md](inspect-commands.md) | workspace、project、module、route、manifest 的结构化只读检查命令。 |
| [docs-and-tests-gates.md](docs-and-tests-gates.md) | `docs check`、`tests check`、文档先行检查、测试矩阵检查和功能点测试门禁。 |
| [diagnostics.md](diagnostics.md) | CLI 错误码、诊断 envelope、human output、JSON output、doctor、explain 和跨模块诊断透传。 |
| [configuration.md](configuration.md) | CLI 配置来源、工作区配置、用户配置、模板源、插件源、profile 和优先级。 |
| [non-interactive-and-ci.md](non-interactive-and-ci.md) | 非交互模式、CI 模式、exit code、JSON 输出、确认策略和自动化安全。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | CLI command tests、golden output、JSON schema、template/build/plugin integration smoke、docs/tests gate 和 AI 输出验证。 |
