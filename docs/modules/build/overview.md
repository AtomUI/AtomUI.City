# AtomUI.City.Build

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Build` 负责构建约定、资源生成、模块清单、路由清单、输出组织和构建期诊断。

Build 是框架工程化能力的一部分，用于把应用框架约定落实到构建输出。

## 边界

Build 可以承担：

- MSBuild 集成。
- 源码生成。
- Analyzer。
- 模块清单生成。
- 路由清单生成。
- 资源清单生成。
- 输出目录组织。

Build 不负责：

- CLI 交互体验。
- 模板内容维护。
- 运行时模块加载逻辑。

## 后续拆分

- `output-layout.md`
- `manifests.md`
- `packaging.md`
- `source-generation.md`
