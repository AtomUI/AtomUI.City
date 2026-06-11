# Build 诊断和测试设计

版本：v0.1
状态：正式初版
适用范围：构建诊断、错误码、MSBuild target test、generator/analyzer test、manifest snapshot、package layout test 和测试矩阵

## 1. 目标

Build 的失败必须可解释、可测试、可定位。构建期错误不能只表现为 MSBuild target 失败，必须有稳定 diagnostic code 和上下文。

## 2. 诊断上下文

诊断至少包含：

- diagnostic code。
- severity。
- target name。
- project path。
- item identity。
- manifest path。
- plugin id，如果适用。
- package id，如果适用。
- source file 和 location，如果适用。
- output path。
- remediation message。

## 3. 错误码建议

| Code | 含义 |
|---|---|
| `AUCBLD0001` | 输出目录无效。 |
| `AUCBLD0101` | Manifest 生成失败。 |
| `AUCBLD0102` | Manifest 校验失败。 |
| `AUCBLD0201` | 插件包布局无效。 |
| `AUCBLD0202` | 插件包多主程序集。 |
| `AUCBLD0301` | AOT 模式不支持动态插件。 |
| `AUCBLD0401` | 发布输出布局无效。 |
| `AUCGEN0001` | Source generator 输入无效。 |
| `AUCANL0001` | Analyzer 规则违反。 |

## 4. 测试工具

Testing 包应支持：

- `BuildTestHost`。
- `MsBuildProjectFixture`。
- `GeneratorTestHost`。
- `AnalyzerTestHost`。
- `ManifestAssertions`。
- `PackageLayoutAssertions`。
- `PublishLayoutAssertions`。
- `BuildDiagnosticsAssertions`。
- `IncrementalBuildDriver`。

## 5. 测试类型

| 类型 | 用途 |
|---|---|
| Unit test | 路径解析、hash、manifest merge、validation。 |
| Generator test | source generator 输入输出。 |
| Analyzer test | diagnostic id、location、severity。 |
| Build test | MSBuild target、package、publish。 |
| Snapshot test | manifest 和 generated artifact 稳定性。 |

## 6. 功能点测试门禁

Build 每个功能点必须有测试矩阵条目。

规则：

- MSBuild target 必须有 build test。
- Source generator 必须有 generator test。
- Analyzer 必须有 analyzer test。
- Manifest 必须有 validation test。
- Package layout 必须有 package test。
- AOT 诊断必须有 build/analyzer test。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| output root | Unit | 默认和覆盖路径。 |
| manifest generation | Generator/Build | 生成、校验、hash。 |
| analyzer | Analyzer | diagnostic id 和 location。 |
| plugin package | Build | 布局、单主程序集、资源。 |
| application publish | Build | 发布目录、manifest、AOT。 |
| incremental | Build | no-op 和缓存失效。 |
| diagnostics | Unit/Build | 错误码和上下文。 |

## 8. 无真实外部依赖

Build 测试不能依赖真实 NuGet feed、真实用户插件目录或真实部署平台。

需要包源时使用本地临时 feed。需要插件安装目录时使用测试临时目录。
