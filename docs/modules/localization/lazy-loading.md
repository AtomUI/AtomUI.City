# AtomUI.City.Localization Lazy Loading 设计

版本：v0.1
状态：正式初版
适用范围：manifest-only startup、按当前语言包懒加载、active scope 资源加载、fallback 按需加载和缓存。

## 1. 定位

Localization 懒加载以语言包为单位。

它不是按单个 key 零散加载，也不是启动时加载所有语言包。启动阶段只加载 manifest，当前选择语言决定实际加载哪些 language package。

## 2. 启动流程

```text
Application startup
-> load localization manifests only
-> register package descriptors
-> select initial culture
-> load required active packages for selected culture
```

启动不加载：

- 未选择 culture 的 package。
- 未激活 Route 的 package。
- 未使用插件的 package。
- 所有 fallback package。

## 3. 加载策略

| 策略 | 说明 |
|---|---|
| Eager | Host 核心资源启动加载。 |
| OnDemand | 模块首次访问时加载。 |
| RouteActivated | 路由进入时预加载页面资源。 |
| PluginActivated | 插件启用时只注册 manifest，资源本体按需加载。 |
| CultureSwitch | 切换文化时按当前活动资源集合加载。 |
| PreloadHint | 模块声明预加载 hint。 |

## 4. 当前文化懒加载

示例：

```text
CurrentCulture = zh-CN
Active modules = Host, SettingsModule
Active plugin = SalesPlugin

Load:
Host.zh-CN
SettingsModule.zh-CN
SalesPlugin.zh-CN

Do not load:
Host.en-US
SettingsModule.ja-JP
SalesPlugin.en-US
```

## 5. Fallback 按需加载

Fallback chain 示例：

```text
zh-CN -> zh-Hans -> zh -> invariant
```

规则：

- 只有当前层缺失 key 时，才加载下一层 fallback package。
- fallback package 也按 contribution 加载。
- fallback 加载失败必须记录诊断。

## 6. Active Package Set

Active package set 由当前运行时状态决定：

- ApplicationScope。
- WindowScope。
- NavigationScope。
- RouteScope。
- ActivationScope。
- Plugin contribution。
- Presentation resource scope。

Route 离开、Window 关闭、Plugin 停用时，对应 package 可释放或降级为 weak cache。

## 7. 缓存

缓存维度：

```text
Culture
ContributionId
PackageId
PackageVersion
ResourceRevision
```

规则：

- 当前活动 package 优先保留。
- 非活动 package 可被内存压力释放。
- culture switch 使 active cache revision 变化。
- plugin unload 清理插件 package cache。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| package not found | fallback 或 missing marker。 |
| package load failed | culture switch 阶段 rollback；普通 lookup 阶段 fallback。 |
| cache entry stale | 重新加载。 |
| plugin package revoked | 清理 cache 并 fallback。 |

## 9. 测试策略

测试必须覆盖：

- startup 只加载 manifest。
- 当前 culture package 加载。
- 未选择 culture package 不加载。
- fallback 按需加载。
- route activated 预加载。
- plugin activated 不加载所有语言包。
- plugin route opened 加载当前 culture package。
