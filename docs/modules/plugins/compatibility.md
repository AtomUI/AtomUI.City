# PluginSystem 兼容性设计

版本：v0.1
状态：正式初版
适用范围：Host 版本、插件 API 版本、contract 版本、目标框架、RID、AOT 和功能兼容

## 1. 目标

兼容性检查必须在加载插件程序集之前完成。

设计目标：

- 不兼容插件不进入加载阶段。
- 兼容性结果可解释。
- 应用 patch 升级不强迫插件目录迁移。
- 插件 API 破坏性变化可以隔离。
- 兼容性判断不依赖运行时反射。

## 2. 兼容性维度

| 维度 | 说明 |
|---|---|
| Host version | 应用或框架版本范围。 |
| Plugin API version | 插件 API 兼容版本。 |
| PluginProfile | 插件目录隔离 profile。 |
| Target framework | 插件目标框架。 |
| Contract version | Host 共享 contract 版本。 |
| RID/native assets | native 资产是否匹配当前运行环境。 |
| Feature compatibility | 插件请求能力是否存在。 |
| AOT compatibility | 当前发布模式是否允许动态插件。 |

## 3. PluginProfile

`PluginProfile` 是插件安装目录和锁定文件的兼容边界：

```text
<HostPluginApiVersion>-<Channel>
```

示例：

```text
1.0-stable
1.0-dev
```

规则：

- patch 版本升级不应改变 `PluginProfile`。
- 插件 API 破坏性变化必须改变 `PluginProfile`。
- 不同渠道必须使用不同 `PluginProfile`。
- Host 启动时只扫描当前 profile 下插件。

## 4. Host 版本

插件声明：

```json
{
  "minHostVersion": "1.0.0",
  "maxHostVersion": "2.0.0"
}
```

规则：

- 当前 Host 低于 `minHostVersion`，拒绝加载。
- 当前 Host 高于 `maxHostVersion`，按策略拒绝或警告。
- 未声明 `maxHostVersion` 表示插件愿意接受兼容升级。

## 5. 插件 API 版本

`pluginApiVersion` 表示插件编程 contract 的兼容版本。

规则：

- `pluginApiVersion` 必须与当前 `PluginProfile` 匹配。
- 插件 API 破坏性变化必须提升 major。
- Host 可以支持多个旧 API 适配层，但不是第一版强要求。
- 旧 API 适配层不能破坏卸载和生命周期规则。

## 6. Contract 版本

共享 contract 版本用于跨插件边界类型。

规则：

- 事件、DTO、消息、服务 contract 必须声明版本。
- 插件私有类型不能作为跨插件 contract。
- contract 不兼容时拒绝相关插件加载或禁用对应能力。
- contract 兼容性必须在加载前从清单判断。

## 7. Target Framework 和 RID

规则：

- 插件 `targetFramework` 必须被当前 Host 支持。
- 插件不能要求高于 Host runtime 的 TFM。
- 携带 native 资产时必须声明 RID 支持范围。
- 当前 RID 不匹配时，禁用依赖 native 的能力或拒绝加载。

## 8. AOT 兼容

Native AOT 场景下默认不支持运行时动态加载插件程序集。

规则：

- CoreCLR 发布可以支持动态插件。
- Native AOT 发布只支持静态插件、资源包或 Host 显式支持的外部进程插件。
- 插件声明 `aotCompatible` 不代表可以动态加载，只表示它满足静态链接或资源模式约束。

## 9. 兼容性结果

兼容性检查结果：

| 结果 | 含义 |
|---|---|
| Compatible | 可进入验证后续阶段。 |
| Incompatible | 不可加载。 |
| Degraded | 可加载，但部分能力禁用。 |
| RequiresRestart | 安装完成，但需要重启后生效。 |
| RequiresHostUpgrade | 需要升级 Host。 |

结果必须写入诊断和锁定文件。

## 10. 测试要求

必须覆盖：

- Host 版本过低。
- Host 版本超过上限。
- 插件 API 不匹配。
- contract 版本不兼容。
- TFM 不兼容。
- RID 不兼容。
- AOT 模式拒绝动态插件。
- 兼容降级能力禁用。
