# PluginSystem 元数据设计

版本：v0.1
状态：正式初版
适用范围：插件身份、清单、版本、兼容性、能力声明、安装记录和锁定信息

## 1. 目标

插件元数据必须让 Host 在不执行插件代码的前提下判断插件是否可信、兼容、可安装、可加载和可启用。

元数据设计目标：

- 插件身份稳定。
- 插件包和插件运行时身份分离。
- 插件兼容性可在加载前判断。
- 插件能力可在启用前授权。
- 插件安装状态可复现。
- 插件更新和回滚可追踪。
- 插件信息对 AOT 和 trimming 友好。

本篇是元数据总览。具体拆分见：

- [清单 Schema 设计](manifest-schema.md)
- [兼容性设计](compatibility.md)
- [能力授权设计](capabilities.md)
- [贡献索引设计](contribution-index.md)
- [包布局设计](package-layout.md)
- [签名和信任设计](signing-and-trust.md)

## 2. 身份模型

插件身份分为三个层次：

| 身份 | 用途 | 稳定性 |
|---|---|---|
| `PluginId` | 运行时插件身份、能力授权、配置隔离、状态隔离。 | 必须跨版本稳定。 |
| `PackageId` | NuGet 包身份、下载、缓存、包来源追踪。 | 可与 `PluginId` 不同。 |
| `MainAssemblyName` | 插件主业务程序集名称。 | 只用于加载和诊断，不作为业务身份。 |

规则：

- 一个插件包第一版只允许声明一个 `PluginId`。
- 一个插件包第一版只允许包含一个主业务程序集。
- 一个主业务程序集可以包含多个插件模块。
- `PluginId` 推荐使用反向域名格式，例如 `com.company.sales`。
- `PluginId` 一旦发布不应变更。
- 同一个 `PluginId` 可以安装多个版本，但同一插件配置 profile 内同一时间只能启用一个版本。

`PackageId` 不应被框架当作运行时身份。包名可以因发布渠道、品牌或迁移发生变化，但 `PluginId` 必须保持稳定。

## 3. PluginProfile

插件安装目录必须按插件兼容 profile 隔离。

`PluginProfile` 由 Host 插件 API 兼容版本和渠道组成：

```text
<HostPluginApiVersion>-<Channel>
```

示例：

```text
1.0-stable
1.0-dev
2.0-stable
```

规则：

- `PluginProfile` 不等同于应用完整版本号。
- 应用 patch 升级不应导致插件目录整体迁移。
- Host 插件 API 或插件 ABI 发生破坏性变化时，应切换 `PluginProfile`。
- 不同渠道的插件目录必须隔离，例如 stable、beta、dev。

## 4. 插件清单

插件包必须包含框架清单：

```text
atomui-city/plugin.json
```

建议结构：

```json
{
  "schemaVersion": "1.0",
  "pluginId": "com.company.sales",
  "packageId": "Company.Sales.Plugin",
  "version": "1.0.0",
  "displayNameKey": "SalesPlugin.DisplayName",
  "descriptionKey": "SalesPlugin.Description",
  "publisher": "Company",
  "mainAssembly": "Company.Sales.Plugin.dll",
  "minHostVersion": "1.0.0",
  "pluginApiVersion": "1.0",
  "targetFramework": "net10.0",
  "aotCompatible": false,
  "unloadable": true,
  "capabilities": [
    {
      "name": "routes",
      "scope": ["/sales/**"]
    },
    {
      "name": "localization"
    }
  ],
  "contributions": {
    "routes": {
      "path": "manifests/routes.json",
      "required": true
    },
    "localization": {
      "path": "manifests/localization.json",
      "required": false
    }
  }
}
```

规则：

- `schemaVersion` 的未知主版本必须拒绝。
- `mainAssembly` 必须指向包内唯一主业务程序集。
- `displayNameKey` 和 `descriptionKey` 使用本地化 key，不直接写死展示文本。
- 清单读取不能要求加载插件程序集。
- 清单字段顺序由构建任务稳定生成，便于 hash 和审计。

## 5. 版本和兼容性

插件至少声明：

| 字段 | 用途 |
|---|---|
| `version` | 插件自身版本。 |
| `minHostVersion` | 最小 Host 版本。 |
| `maxHostVersion` | 最大 Host 版本，默认可省略。 |
| `pluginApiVersion` | Host 插件 API 兼容版本。 |
| `targetFramework` | 插件目标框架。 |
| `runtimeIdentifiers` | 插件携带 native/RID 资产时声明。 |
| `contractVersions` | 插件依赖的共享 contract 版本。 |

兼容性检查必须发生在加载前：

```text
Read manifest
-> Check schema version
-> Check Host version
-> Check plugin API version
-> Check target framework
-> Check contract versions
-> Check RID/native asset compatibility
```

不兼容的插件进入 `Invalid` 或 `Disabled`，不能加载程序集。

## 6. 能力声明

插件能力声明表达插件希望使用哪些扩展点。声明不等于授权。

示例：

```json
{
  "capabilities": [
    {
      "name": "routes",
      "scope": ["/sales/**"]
    },
    {
      "name": "data.http",
      "clients": ["SalesApi"]
    },
    {
      "name": "eventbus.subscribe",
      "contracts": ["SalesOrderChanged"]
    },
    {
      "name": "localization"
    }
  ]
}
```

规则：

- Host Security 或 Host policy 负责把 requested capabilities 转换为 granted capabilities。
- 插件启用时只能提交已授权能力范围内的 Contribution。
- 能力拒绝不一定导致插件安装失败，但会阻止对应 Contribution。
- 能力校验结果必须进入诊断。

## 7. Contribution Index

插件清单只描述贡献清单的位置，不直接内联所有贡献内容。

建议：

```json
{
  "contributions": {
    "routes": {
      "path": "manifests/routes.json",
      "required": true
    },
    "permissions": {
      "path": "manifests/permissions.json",
      "required": false
    },
    "presentation": {
      "path": "manifests/presentation.json",
      "required": false
    },
    "data": {
      "path": "manifests/data.json",
      "required": false
    },
    "localization": {
      "path": "manifests/localization.json",
      "required": false
    }
  }
}
```

规则：

- `required` 为 true 的贡献清单缺失时，插件验证失败。
- 贡献清单读取仍然不能执行插件代码。
- 贡献清单必须可追踪来源文件和 hash。

## 8. 安装记录

每个已安装版本目录下必须生成安装记录：

```text
install.json
```

建议记录：

- `pluginId`
- `packageId`
- `version`
- `source`
- `packageHash`
- `contentHash`
- `installedAt`
- `installedBy`
- `pluginProfile`
- `installPath`
- `manifestHash`
- `grantedCapabilities`
- `validationResult`

安装记录用于诊断、回滚、审计和清理，不参与插件业务逻辑。

## 9. 锁定文件

每个插件 profile 需要维护锁定文件：

```text
atomui-city.plugins.lock.json
```

锁定文件记录：

- 已安装插件列表。
- 当前启用版本。
- 禁用状态。
- 包来源和 hash。
- 插件依赖解析结果。
- 授权后的能力集合。
- 上次验证结果。
- pending 操作。

Host 启动时应以锁定文件为准恢复插件启用状态，而不是只根据目录扫描结果推断。

## 10. AOT 和 Source Generator 约束

元数据系统必须优先依赖构建期生成的清单。

规则：

- 不通过运行时反射扫描插件入口。
- 不通过加载程序集读取插件 identity。
- 插件模块图、路由、权限、本地化索引等优先由 source generator 或 MSBuild task 生成。
- Native AOT 场景不支持运行时动态加载插件程序集，元数据仍可用于静态插件和资源包管理。

## 11. 诊断和测试

必须覆盖：

- 清单 schema 版本不兼容。
- `PluginId` 缺失或格式无效。
- 包内存在多个主业务程序集。
- `PluginId` 与锁定文件冲突。
- Host 版本不兼容。
- 插件 API 版本不兼容。
- 缺少 required contribution manifest。
- 能力声明被拒绝。
- 安装记录和实际文件 hash 不一致。

诊断信息必须包含 PluginId、PackageId、Version、PluginProfile、Source、Path 和失败阶段。
