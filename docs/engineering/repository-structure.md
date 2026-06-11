# 仓库结构规范

版本：v0.1
状态：正式初版
适用范围：仓库目录、源码项目、测试项目、引用项目和输出目录

## 1. 目标

本规范约束 AtomUI.City 仓库的目录和项目布局，保证源码、测试、文档、构建产物和引用项目边界清晰。

## 2. 顶层目录

| 路径 | 职责 |
|---|---|
| `src/` | 框架源码项目。 |
| `tests/` | 框架测试项目。 |
| `docs/` | 正式设计文档、工程规范、ADR 和参考资料。 |
| `build/` | 仓库级 MSBuild props/targets 和输出规则。 |
| `output/` | 框架构建、包、发布和诊断输出根目录。 |
| `.referenceprojects/` | 外部参考项目源码，只用于学习和对照，不进入产品构建。 |
| `.agents/` | Agent 技能和仓库协作规则。 |

## 3. 源码项目

运行时和工程化项目位于 `src/`。

当前项目：

- `AtomUI.City.Core`
- `AtomUI.City.Mvvm`
- `AtomUI.City.State`
- `AtomUI.City.EventBus`
- `AtomUI.City.Routing`
- `AtomUI.City.Presentation`
- `AtomUI.City.Data`
- `AtomUI.City.Security`
- `AtomUI.City.Localization`
- `AtomUI.City.PluginSystem`
- `AtomUI.City.Build`
- `AtomUI.City.Cli`
- `AtomUI.City.Templates`
- `AtomUI.City.Testing`

新增项目必须先更新包边界文档和本文件。

## 4. 测试项目

每个框架包必须有对应测试项目。

命名规则：

```text
src/AtomUI.City.<Module>/
tests/AtomUI.City.<Module>.Tests/
```

测试项目不进入生产依赖链。

## 5. 文档目录

文档分层：

- `docs/architecture/`：顶层架构约束。
- `docs/modules/`：模块详细设计。
- `docs/engineering/`：仓库工程规范。
- `docs/decisions/`：长期架构决策。
- `docs/reference/`：参考资料和外部资料摘要。
- `docs/superpowers/checklists/`：执行 checklist。

代码实现前必须确认受影响文档已对齐。

## 6. 引用项目

`.referenceprojects/` 下的项目只作为设计参考。

规则：

- 不进入 `AtomUICity.slnx`。
- 不参与 restore/build/test。
- 不从引用项目复制许可证不兼容代码。
- 正式文档不混入引用项目分析语句。

## 7. 输出目录

AtomUI.City 统一输出根为 `output/`。

`bin/obj` 可以保留 .NET 默认行为，但框架级构建、包、发布、manifest、日志和诊断快照应收敛到 `output/`。

输出布局细节见：[Build 输出布局](../modules/build/output-layout.md)。

## 8. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| solution 项目引用 | Build | `AtomUICity.slnx` 包含所有 src/tests 项目。 |
| 生产/测试分离 | Unit/Build | 测试包不被生产项目引用。 |
| 引用项目隔离 | Build | `.referenceprojects/` 不进入 solution。 |
| output 目录 | Build | 构建规则输出到 `output/`。 |
