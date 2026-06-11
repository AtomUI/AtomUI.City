# Build MSBuild 集成设计

版本：v0.1
状态：正式初版
适用范围：Build props、targets、tasks、MSBuild 属性、Item、Target 和 buildTransitive 分发

## 1. 目标

Build 通过 MSBuild 集成把框架约定接入标准 `dotnet build`、`dotnet pack`、`dotnet publish` 和 CI。

设计目标：

- 应用项目少配置即可获得框架构建能力。
- 插件项目声明属性即可生成标准插件包。
- generator/analyzer 通过 build assets 自动接入。
- 所有 target 可被 CLI 和 CI 调用。
- 构建失败有稳定 diagnostic code。

## 2. Build Assets

建议包内布局：

```text
buildTransitive/
  AtomUI.City.Build.props
  AtomUI.City.Build.targets
  AtomUI.City.Application.targets
  AtomUI.City.Plugin.targets
  AtomUI.City.Diagnostics.targets
analyzers/
  dotnet/cs/AtomUI.City.Generators.dll
tools/
  net10.0/AtomUI.City.Build.Tasks.dll
```

规则：

- `buildTransitive` 用于应用和插件项目自动获得构建规则。
- Roslyn generator/analyzer 作为 analyzer asset 引入。
- MSBuild task 不进入运行时包主链路。

## 3. 推荐属性

| 属性 | 默认值 | 说明 |
|---|---|---|
| `AtomUICityOutputRoot` | `output` | 框架输出根目录。 |
| `AtomUICityGenerateManifests` | `true` | 是否生成 manifest。 |
| `AtomUICityValidateManifests` | `true` | 是否校验 manifest。 |
| `AtomUICityEnableAnalyzers` | `true` | 是否启用 analyzer。 |
| `AtomUICitySourceGenerationMode` | `Strict` | `Strict`、`Compatible`、`Off`。 |
| `AtomUICityAllowDynamicDiscovery` | `false` | 是否允许动态发现。 |
| `AtomUICityStrictAot` | `false` | 是否启用严格 AOT 检查。 |
| `AtomUICityPackagePlugin` | `false` | 是否打包插件。 |
| `AtomUICityPackageApplication` | `false` | 是否打包应用。 |
| `AtomUICityPluginProfile` | 空 | 插件兼容 profile。 |
| `AtomUICityBuildDiagnosticsLevel` | `Normal` | 诊断详细程度。 |

## 4. 插件属性

插件属性沿用 PluginSystem 设计：

- `AtomUICityPlugin`
- `AtomUICityPluginId`
- `AtomUICityPluginVersion`
- `AtomUICityPluginPublisher`
- `AtomUICityPluginDisplayNameKey`
- `AtomUICityPluginDescriptionKey`
- `AtomUICityMinHostVersion`
- `AtomUICityMaxHostVersion`
- `AtomUICityPluginApiVersion`
- `AtomUICityPluginUnloadable`
- `AtomUICityPluginNativeAotCompatible`
- `AtomUICityPluginResourceMode`
- `AtomUICityPackageAsPlugin`

属性名使用 `AtomUICity` 前缀是为了避免和普通 MSBuild 属性冲突。

## 5. 推荐 Item

| Item | 说明 |
|---|---|
| `AtomUICityPluginCapability` | 插件请求能力。 |
| `AtomUICityPluginDependency` | 插件依赖。 |
| `AtomUICityPluginContract` | 共享 contract 版本。 |
| `AtomUICityLanguagePackage` | 语言包。 |
| `AtomUICityPluginAsset` | 插件资产。 |
| `AtomUICityPluginNativeAsset` | native/RID 资产。 |
| `AtomUICityContributionManifest` | 额外贡献清单。 |
| `AtomUICityStaticPlugin` | 应用静态插件引用。 |
| `AtomUICityResourcePack` | 资源包。 |

## 6. 推荐 Target

| Target | 说明 |
|---|---|
| `GenerateAtomUICityManifests` | 生成模块和应用级 manifest。 |
| `ValidateAtomUICityManifests` | 校验 manifest。 |
| `GenerateAtomUICityPluginManifest` | 生成插件 `plugin.json`。 |
| `ValidateAtomUICityPluginPackage` | 校验插件包布局。 |
| `PackAtomUICityPlugin` | 生成插件 NuGet 包。 |
| `PublishAtomUICityApplication` | 发布应用。 |
| `ValidateAtomUICityAotCompatibility` | AOT/trimming 兼容检查。 |
| `WriteAtomUICityBuildDiagnostics` | 写入构建诊断。 |
| `CleanAtomUICityOutput` | 清理框架输出。 |

## 7. Target 顺序

推荐顺序：

```text
CoreCompile
-> GenerateAtomUICityManifests
-> ValidateAtomUICityManifests
-> ValidateAtomUICityAotCompatibility
-> PackAtomUICityPlugin
-> PublishAtomUICityApplication
-> WriteAtomUICityBuildDiagnostics
```

具体接入点以 .NET SDK target graph 为准，但 Build 文档必须保证顺序语义稳定。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| props 导入 | Build test | 应用项目自动获得默认属性。 |
| targets 导入 | Build test | target 可被调用。 |
| 属性覆盖 | Build test | output、strict AOT、plugin profile 生效。 |
| Item 收集 | Build test | language package、asset、capability 被收集。 |
| target 顺序 | Build test | manifest 先于 package validation。 |
| diagnostics | Build test | 失败 target 输出稳定 code。 |
