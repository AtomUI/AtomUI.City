# AtomUI.City.Cli

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Cli` 负责项目创建、模块生成、路由生成、构建命令、模板调用和诊断输出。

Cli 是使用者接触 AtomUI.City 工程化能力的主要入口。

## 边界

Cli 负责：

- 命令定义。
- 参数解析。
- 模板调用。
- 项目生成。
- 模块生成。
- 诊断报告。
- 终端输出体验。

Cli 不负责：

- 模板自身内容。
- MSBuild 任务实现。
- 运行时框架逻辑。

## 后续拆分

- `commands.md`
- `project-generation.md`
- `diagnostics.md`
