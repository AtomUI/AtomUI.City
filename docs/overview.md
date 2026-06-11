# AtomUI.City 文档

本目录维护 AtomUI.City 框架的正式设计、工程规范、模块文档、决策记录和使用指南。

## 全局强约束

AtomUI.City 采用文档先行研发规则。

所有模块必须有完善的文档后，才能开始写代码。任何改动都必须先完成文档对齐，并确认文档没有问题后，才能进入实现。

具体规则见：[文档先行治理规范](engineering/documentation-governance.md)。

该规则是项目级工程治理要求，不是建议。

## 文档分层

| 目录 | 职责 |
|---|---|
| `architecture/` | 框架顶层架构、包边界、编程范式、生命周期和依赖策略。 |
| `modules/` | 各框架包的模块级设计文档。 |
| `engineering/` | 仓库结构、构建系统、代码风格、版本、打包和发布流程。 |
| `decisions/` | 架构决策记录，用于沉淀重要选择及其原因。 |
| `guides/` | 面向使用者的框架使用指南。 |
| `reference/` | API、包列表、术语表等查询型资料。 |

当前优先维护：

- [整体架构设计](architecture/overview.md)
- [包边界](architecture/package-boundaries.md)
- [编程范式](architecture/programming-model.md)
- [生命周期](architecture/lifecycle.md)
- [插件系统架构规范](architecture/plugin-system.md)
- [开源依赖策略](architecture/dependency-strategy.md)
- [文档先行治理规范](engineering/documentation-governance.md)
- [实现路线图](engineering/implementation-roadmap.md)
- [模块文档索引](modules/overview.md)
- [架构决策记录](decisions/overview.md)
