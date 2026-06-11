# AtomUI.City.Localization Diagnostics and Testing 设计

版本：v0.1
状态：正式初版
适用范围：缺失资源诊断、加载失败、fallback、culture switch、AtomUI bridge、插件撤销和测试工具。

## 1. 定位

Localization 必须可诊断、可测试。

缺失文本不能只表现为 UI 空白。必须能说明缺的是哪个 key、哪个 culture、哪个 package、哪个 contribution 和哪个 fallback 阶段。

## 2. 诊断字段

必须记录：

- Culture。
- Fallback culture。
- Resource key。
- Resource type。
- PackageId。
- Package version。
- Scope。
- ModuleId。
- PluginId。
- ContributionId。
- Lookup stage。
- Missing reason。
- Load duration。
- Apply duration。
- Culture revision。

敏感信息通常不应放入本地化资源 key。错误参数写入诊断时需要脱敏。

## 3. 诊断分类

| 分类 | 说明 |
|---|---|
| ResourceMissing | 当前 culture 缺 key。 |
| FallbackMissing | fallback 也缺 key。 |
| PackageLoadFailed | 语言包加载失败。 |
| PackageVersionMismatch | 语言包版本不兼容。 |
| FormatFailed | 格式化失败。 |
| ResourceRevoked | 资源 contribution 已撤销。 |
| AtomUiApplyFailed | AtomUI/Avalonia 资源应用失败。 |
| CultureSwitchRolledBack | 文化切换已回滚。 |
| PluginResourceLeak | 插件资源仍被引用。 |

## 4. Testing 包

Testing 包应提供：

- Fake culture state provider。
- Fake language package provider。
- Fake assembly package provider。
- Fake locpack provider。
- Test localization service。
- Test presentation localization bridge。
- Missing resource recorder。
- Culture switch driver。
- Plugin localization test host。
- Resource leak assertion helper。

## 5. 测试场景

必须覆盖：

- manifest-only startup。
- selected culture package lazy load。
- fallback package lazy load。
- culture switch success。
- culture switch rollback。
- missing marker。
- format error。
- AtomUI bridge apply。
- UI binding refresh。
- plugin package revoke。
- plugin assembly unload。
- AOT locpack provider。

## 6. 无 UI 测试

Localization Core 测试不依赖真实 AtomUI/Avalonia。

Presentation integration 测试使用 fake bridge。真实 UI resource dictionary 测试放到 Presentation 平台集成测试中。
