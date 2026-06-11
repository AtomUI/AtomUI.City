# AtomUI.City.Build

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Build` 负责构建约定、资源生成、模块清单、路由清单、输出组织和构建期诊断。

Build 是框架工程化能力的一部分，用于把应用框架约定落实到构建输出。

Build 是 CLI 和 Templates 的工程规则底座。CLI 调用 Build 能力，Templates 生成的项目必须符合 Build 约定。

## 边界

Build 可以承担：

- MSBuild 集成。
- 源码生成。
- Analyzer。
- 模块清单生成。
- 路由清单生成。
- 资源清单生成。
- 插件清单生成。
- 插件包构建和校验。
- 应用发布输出约定。
- AOT/trimming 构建期诊断。
- 输出目录组织。

Build 不负责：

- CLI 交互体验。
- 模板内容维护。
- 运行时模块加载逻辑。
- 运行时插件加载逻辑。
- 真实部署平台。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Build 总体架构、职责边界、构建管线、输出、manifest、打包、发布、增量构建和测试矩阵。 |
| [output-layout.md](output-layout.md) | `output/` 根目录、构建产物、生成产物、包、发布输出、日志和清理策略。 |
| [msbuild-integration.md](msbuild-integration.md) | Build props、targets、tasks、MSBuild 属性、Item、Target 和 buildTransitive 分发。 |
| [manifest-generation.md](manifest-generation.md) | 模块、路由、权限、Presentation、Data、Localization、Plugin 和应用 manifest 的生成、校验和输出。 |
| [source-generation.md](source-generation.md) | Source Generator 接入、生成产物收敛、增量生成、输出路径、AOT 约束和测试。 |
| [analyzers.md](analyzers.md) | 构建期 analyzer 规则、诊断 ID、AOT/trimming、插件、架构和测试矩阵诊断。 |
| [plugin-packaging.md](plugin-packaging.md) | 插件 NuGet 包、plugin manifest、贡献清单、资源、hash、签名输入和包布局校验。 |
| [application-packaging.md](application-packaging.md) | 应用 publish、静态插件、bundled plugin、资源包、Native AOT、发布 manifest 和发布诊断。 |
| [incremental-build.md](incremental-build.md) | 增量生成、缓存、输入输出追踪、确定性输出和 CI 可复现性。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | 构建诊断、错误码、MSBuild target test、generator/analyzer test、manifest snapshot 和 package layout test。 |
