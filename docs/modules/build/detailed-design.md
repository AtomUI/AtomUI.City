# AtomUI.City.Build 详细设计

版本：v0.1
状态：正式初版
适用范围：构建约定、输出目录、MSBuild 集成、manifest 生成、source generator、analyzer、打包、发布、增量构建、诊断和测试

## 1. 定位

`AtomUI.City.Build` 是 AtomUI.City 的构建期基础设施。它负责把框架的编程范式、模块系统、路由、插件、本地化、Presentation、Data、Security、Source Generator、Analyzer 和发布约定落实到确定性的构建输出。

Build 不是 CLI，也不是 Templates。CLI 调用 Build 能力，Templates 生成的项目必须符合 Build 约定。

## 2. 职责

Build 负责：

- MSBuild props、targets、tasks。
- `output/` 目录组织。
- source generator 产物收敛。
- analyzer 规则接入。
- module manifest 生成。
- route manifest 生成。
- permission manifest 生成。
- presentation manifest 生成。
- data client manifest 生成。
- localization manifest 生成。
- plugin manifest 生成。
- plugin package 构建和校验。
- application package 和 publish 输出约定。
- AOT/trimming 诊断。
- 构建期错误码和测试工具。

Build 不负责：

- CLI 命令交互。
- 模板内容本身。
- 运行时插件加载。
- 运行时路由解析。
- 业务代码生成。
- 真实部署平台。

## 3. 构建管线

建议 Build 管线：

```text
Restore
-> Compile source generators
-> Generate module/source indexes
-> Run analyzers
-> Generate manifests
-> Validate manifests
-> Build assemblies
-> Collect resources
-> Package plugins if enabled
-> Publish application if enabled
-> Write output layout
-> Emit diagnostics
```

规则：

- 构建输出必须 deterministic。
- 构建任务不能依赖运行时反射扫描。
- 构建期可知道的信息必须通过 source generator 或 MSBuild task 生成。
- 所有生成产物必须能被测试断言。
- AOT 不友好行为必须尽早报 analyzer 或 build diagnostic。
- 构建失败必须带稳定 diagnostic code。

## 4. 包边界

工程层依赖方向：

```text
Cli -> Templates / Build
Build -> Core metadata / manifests / generators
Testing -> Build test utilities
```

规则：

- `AtomUI.City.Build` 可以通过 build assets 引入 generator/analyzer。
- `AtomUI.City.Build` 不进入运行时主链路。
- 运行时包不能依赖 Roslyn。
- Build 可以引用 manifest contract，但不能依赖运行时 Host 执行插件加载。

## 5. 输出目录

Build 统一输出到仓库或项目配置的 `output/` 根目录：

```text
output/
  artifacts/
    bin/
    obj/
    generated/
    manifests/
    diagnostics/
  packages/
    nuget/
    plugins/
    templates/
  publish/
    apps/
    plugins/
    resources/
  logs/
```

详细规则见：[输出目录设计](output-layout.md)。

## 6. MSBuild 集成

Build 提供：

- buildTransitive props/targets。
- 应用构建 targets。
- 插件构建 targets。
- manifest 生成 task。
- package layout 校验 task。
- AOT 兼容校验 task。
- diagnostics 输出 task。

详细规则见：[MSBuild 集成设计](msbuild-integration.md)。

## 7. Manifest 生成

Build 汇总各模块 source generator 和 MSBuild item 产物，生成最终 manifest。

| Manifest | 来源 |
|---|---|
| `modules.json` | Module source generator。 |
| `routes.json` | Routing source generator。 |
| `permissions.json` | Security source generator。 |
| `presentation.json` | Presentation source generator。 |
| `data.json` | Data source generator。 |
| `localization.json` | Localization source generator。 |
| `plugin.json` | MSBuild task 汇总。 |
| `plugin.manifest.json` | 插件贡献总索引。 |

详细规则见：[Manifest 生成设计](manifest-generation.md)。

## 8. Source Generator 和 Analyzer

Source Generator 负责生成编译期索引和 registrar。Analyzer 负责构建期诊断。

Build 负责把 generator/analyzer 作为构建资产接入项目，并收敛输出到统一目录。

详细规则见：

- [Source Generation 集成设计](source-generation.md)
- [Analyzer 设计](analyzers.md)

## 9. 打包和发布

Build 支持：

- 插件 NuGet package。
- 应用 publish 输出。
- 静态插件发布。
- 资源包发布。
- 本地化语言包。
- package hash 和 manifest hash。

详细规则见：

- [插件打包设计](plugin-packaging.md)
- [应用发布设计](application-packaging.md)

## 10. 增量构建

Build 必须支持稳定增量构建：

- 输入未变化时不重复生成。
- manifest 顺序稳定。
- hash 稳定。
- diagnostics 稳定。
- generated 文件路径稳定。

详细规则见：[增量构建设计](incremental-build.md)。

## 11. 测试矩阵

| 功能点 | 测试类型 | 测试工具 | 必测场景 |
|---|---|---|---|
| output layout | Unit/Build test | BuildTestHost | 路径、清理、产物分类。 |
| manifest generation | Generator/Build test | ManifestAssertions | 字段稳定、hash、validation。 |
| plugin packaging | Build test | PackageLayoutAssertions | 单主程序集、资源、plugin.json。 |
| analyzer rules | Analyzer test | AnalyzerTestHost | diagnostic id、location。 |
| AOT diagnostics | Build test | BuildDiagnosticsAssertions | dynamic plugin 被拒绝。 |
| incremental build | Build test | IncrementalBuildDriver | 未变化不重复生成。 |
| application publish | Build test | PublishLayoutAssertions | publish layout、manifest。 |

完整规则见：[诊断和测试设计](diagnostics-and-testing.md)。

## 12. 完成标准

Build 实现任何功能点前必须满足：

- 文档已确认。
- 测试矩阵已有条目。
- 对应 MSBuild/generator/analyzer 行为有测试设计。
- 输出路径和诊断码已定义。

功能点完成时必须有单元测试或 build/generator/analyzer test。
