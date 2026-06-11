# PluginSystem MSBuild 集成设计

版本：v0.1
状态：正式初版
适用范围：插件项目属性、Item、Target、清单生成、包验证和本地开发安装

## 1. 目标

插件开发不能要求开发者手写大量清单和包结构。框架应通过 MSBuild targets、tasks 和 source generator 生成稳定产物。

设计目标：

- 插件项目声明少量属性即可生成标准包。
- 清单、贡献索引、资源索引由构建期生成。
- 构建时发现包布局、能力、AOT、依赖问题。
- 打包结果与运行时安装规则一致。
- 支持本地开发安装到插件目录。

## 2. 插件项目属性

推荐插件项目：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AtomUICityPlugin>true</AtomUICityPlugin>
    <AtomUICityPluginId>com.company.sales</AtomUICityPluginId>
    <AtomUICityPluginDisplayNameKey>SalesPlugin.DisplayName</AtomUICityPluginDisplayNameKey>
    <AtomUICityPluginDescriptionKey>SalesPlugin.Description</AtomUICityPluginDescriptionKey>
    <AtomUICityMinHostVersion>1.0.0</AtomUICityMinHostVersion>
    <AtomUICityPluginApiVersion>1.0</AtomUICityPluginApiVersion>
    <AtomUICityPackageAsPlugin>true</AtomUICityPackageAsPlugin>
  </PropertyGroup>
</Project>
```

属性名使用 `AtomUICity` 前缀是为了避免和普通 NuGet/MSBuild 属性冲突，不代表公共 API 类型必须带 `City` 后缀或前缀。

## 3. 推荐属性

| 属性 | 说明 |
|---|---|
| `AtomUICityPlugin` | 标记项目为插件项目。 |
| `AtomUICityPluginId` | 插件运行时身份。 |
| `AtomUICityPluginVersion` | 插件版本，默认可来自 `Version`。 |
| `AtomUICityPluginPublisher` | 发布者。 |
| `AtomUICityPluginDisplayNameKey` | 显示名称本地化 key。 |
| `AtomUICityPluginDescriptionKey` | 描述本地化 key。 |
| `AtomUICityMinHostVersion` | 最小 Host 版本。 |
| `AtomUICityMaxHostVersion` | 最大 Host 版本。 |
| `AtomUICityPluginApiVersion` | 插件 API 版本。 |
| `AtomUICityPluginUnloadable` | 是否设计为可卸载。 |
| `AtomUICityPluginNativeAotCompatible` | 是否声明 AOT 兼容。 |
| `AtomUICityPluginResourceMode` | `Assembly`、`LocPack` 或 `Both`。 |
| `AtomUICityPluginGenerateManifest` | 是否生成清单。 |
| `AtomUICityPluginValidateManifest` | 是否验证清单。 |
| `AtomUICityPackageAsPlugin` | 是否按插件包布局打包。 |
| `AtomUICityPluginDevelopmentMode` | 是否启用开发期本地安装辅助。 |

## 4. 推荐 Item

| Item | 说明 |
|---|---|
| `AtomUICityPluginCapability` | 声明请求能力。 |
| `AtomUICityPluginDependency` | 声明插件依赖。 |
| `AtomUICityPluginContract` | 声明共享 contract 版本。 |
| `AtomUICityLanguagePackage` | 声明语言包。 |
| `AtomUICityPluginAsset` | 声明插件资产。 |
| `AtomUICityPluginNativeAsset` | 声明 native/RID 资产。 |
| `AtomUICityContributionManifest` | 声明额外贡献清单。 |

示例：

```xml
<ItemGroup>
  <AtomUICityPluginCapability Include="routes">
    <Scope>/sales/**</Scope>
  </AtomUICityPluginCapability>
  <AtomUICityPluginDependency Include="com.company.identity">
    <VersionRange>[1.0.0,2.0.0)</VersionRange>
  </AtomUICityPluginDependency>
  <AtomUICityLanguagePackage Include="Resources\zh-CN\*.resx">
    <Culture>zh-CN</Culture>
  </AtomUICityLanguagePackage>
</ItemGroup>
```

## 5. 推荐 Target

| Target | 说明 |
|---|---|
| `GenerateAtomUICityPluginManifest` | 生成 `plugin.json`。 |
| `GenerateAtomUICityContributionManifests` | 生成模块贡献清单。 |
| `ValidateAtomUICityPluginManifest` | 校验清单 schema 和字段。 |
| `ValidateAtomUICityPluginPackage` | 校验包布局和单主程序集规则。 |
| `PackAtomUICityPlugin` | 生成插件 NuGet 包。 |
| `InstallAtomUICityPluginToLocalCache` | 开发期安装到本机插件目录。 |
| `CleanAtomUICityPluginArtifacts` | 清理插件生成产物。 |

Target 应可被普通 `dotnet build`、`dotnet pack` 和 CI 调用。

## 6. Source Generator 分工

Source generator 负责生成编译期可知的索引：

- 模块清单。
- 路由清单。
- 权限清单。
- 本地化 key 索引。
- ViewModel/View 映射索引。
- Data client 代理索引。

MSBuild task 负责整合最终包级清单：

```text
Source generators
-> intermediate manifests
-> MSBuild task merges manifests
-> atomui-city/plugin.json
-> atomui-city/manifests/*.json
```

## 7. 诊断代码

建议诊断：

| Code | 含义 |
|---|---|
| `AUCPLG1001` | 插件项目缺少 `AtomUICityPluginId`。 |
| `AUCPLG1002` | 插件包包含多个主程序集。 |
| `AUCPLG1003` | `DisplayNameKey` 缺失。 |
| `AUCPLG1101` | 能力声明格式无效。 |
| `AUCPLG1201` | 插件依赖版本范围无效。 |
| `AUCPLG1301` | AOT 兼容声明与使用能力冲突。 |
| `AUCPLG1401` | required contribution manifest 未生成。 |

## 8. 开发体验

开发期本地安装流程：

```text
dotnet pack
-> Validate plugin package
-> Install to development PluginProfile
-> Update development lock file
```

规则：

- 开发期安装必须进入 `dev` profile。
- 不应覆盖 stable profile。
- 开发期插件可以来自项目输出目录，但必须显式开启。
- 开发期路径必须进入诊断。

## 9. 测试要求

必须覆盖：

- 最小插件项目生成标准清单。
- 多模块插件生成模块索引。
- 缺少必填属性时报诊断。
- 包布局不合法时报诊断。
- 本地开发安装不会污染 stable profile。
- 生成清单字段顺序稳定。
