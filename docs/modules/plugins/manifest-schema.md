# PluginSystem 清单 Schema 设计

版本：v0.1
状态：正式初版
适用范围：`atomui-city/plugin.json` 的字段、版本、校验规则和生成规则

## 1. 目标

插件清单是 Host 在加载插件程序集之前理解插件的唯一入口。

清单设计目标：

- 不执行插件代码即可完成元数据读取。
- 不加载插件程序集即可完成兼容性初筛。
- 支持构建期稳定生成。
- 支持安装、更新、回滚、诊断和审计。
- 支持 AOT 友好的静态分析。

## 2. 文件位置

插件包内必须包含：

```text
atomui-city/plugin.json
```

安装后路径：

```text
installed/<plugin-id>/<version>/root/atomui-city/plugin.json
```

规则：

- 清单必须位于 `atomui-city` 目录下。
- 清单路径大小写应在包生成时固定。
- Host 不应通过扫描程序集特性来替代清单。

## 3. 根字段

建议根字段：

| 字段 | 必填 | 说明 |
|---|---:|---|
| `schemaVersion` | 是 | 清单 schema 版本。 |
| `pluginId` | 是 | 运行时插件身份。 |
| `packageId` | 是 | 包身份。 |
| `version` | 是 | 插件版本。 |
| `displayNameKey` | 是 | 插件显示名称本地化 key。 |
| `descriptionKey` | 否 | 插件描述本地化 key。 |
| `publisher` | 否 | 发布者标识。 |
| `mainAssembly` | 是 | 主业务程序集文件名。 |
| `targetFramework` | 是 | 目标框架。 |
| `pluginApiVersion` | 是 | Host 插件 API 兼容版本。 |
| `minHostVersion` | 是 | 最小 Host 版本。 |
| `maxHostVersion` | 否 | 最大 Host 版本。 |
| `unloadable` | 是 | 是否设计为可卸载。 |
| `aotCompatible` | 是 | 是否声明 AOT 兼容。 |
| `capabilities` | 否 | 请求的能力集合。 |
| `contributions` | 否 | 贡献清单索引。 |
| `dependencies` | 否 | 插件依赖和包依赖摘要。 |
| `contracts` | 否 | 共享 contract 版本要求。 |
| `resources` | 否 | 本地化、资产、native/RID 资源摘要。 |
| `settings` | 否 | 配置 schema 和迁移摘要。 |
| `trust` | 否 | 签名、来源、hash 摘要。 |

## 4. 示例

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
  "targetFramework": "net10.0",
  "pluginApiVersion": "1.0",
  "minHostVersion": "1.0.0",
  "unloadable": true,
  "aotCompatible": false,
  "capabilities": [
    {
      "name": "routes",
      "scope": ["/sales/**"]
    }
  ],
  "contributions": {
    "routes": {
      "path": "manifests/routes.json",
      "required": true
    }
  }
}
```

## 5. Schema 版本策略

`schemaVersion` 使用 `major.minor`。

规则：

- 未知 major 版本必须拒绝。
- 已知 major、未知 minor 可以进入兼容降级策略。
- Host 不应忽略未知必填字段。
- 清单 schema 升级必须保证诊断代码稳定。

## 6. 字段校验

基础校验：

- `pluginId` 必须非空，建议使用反向域名格式。
- `version` 必须是可比较的语义化版本。
- `mainAssembly` 只能是文件名，不能包含绝对路径。
- `targetFramework` 必须在 Host 支持范围内。
- `pluginApiVersion` 必须匹配当前 `PluginProfile`。
- `displayNameKey` 不能直接使用展示文本。
- `capabilities` 的名称必须在 Host 能力目录中存在。
- `contributions` 引用的文件必须存在。

## 7. 生成规则

清单推荐由 MSBuild task 和 source generator 共同生成。

生成输入：

- MSBuild properties。
- MSBuild items。
- 模块特性。
- 路由、权限、本地化、Presentation、Data 等模块生成的贡献清单。

生成输出必须稳定：

- 字段顺序稳定。
- 数组顺序稳定。
- 路径分隔符稳定。
- 不把构建时间写入核心 hash 参与字段。

## 8. AOT 约束

清单不能依赖运行时反射。

规则：

- 不通过加载程序集发现插件 identity。
- 不通过运行时扫描类型发现模块入口。
- 构建期能生成的索引必须构建期生成。
- Native AOT 模式下，清单仍可描述静态插件和资源包。

## 9. 诊断

建议诊断代码：

| Code | 含义 |
|---|---|
| `AUCPLG0001` | 缺少 `PluginId`。 |
| `AUCPLG0002` | 包含多个主业务程序集。 |
| `AUCPLG0003` | `PluginId` 与安装记录不一致。 |
| `AUCPLG0004` | 清单 schema major 不支持。 |
| `AUCPLG0005` | 必填贡献清单缺失。 |
| `AUCPLG0006` | `mainAssembly` 路径非法。 |

诊断必须带 PluginId、PackageId、Version、manifest path 和失败字段。
