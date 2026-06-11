# AtomUI.City.Localization Plugin Integration 设计

版本：v0.1
状态：正式初版
适用范围：插件本地化贡献、语言包懒加载、assembly package、资源撤销、卸载和跨插件 contract。

## 1. 定位

插件可以贡献本地化资源，但必须受 Host 生命周期、ContributionLease、文化切换和卸载约束管理。

插件启用不等于加载所有语言包。插件只注册 localization manifest，当前 culture 和活动 UI 决定实际加载哪个语言包。

## 2. 插件贡献

插件可以贡献：

- Localization manifest。
- Language package descriptor。
- Assembly language package。
- File locpack。
- Route title key。
- Command text key。
- Validation / error message key。

所有贡献必须通过 Contribution Request 进入 registry。

## 3. 插件加载流程

```text
Plugin enable
-> register localization manifest
-> register package descriptors
-> do not load all language packages

Plugin route opened under zh-CN
-> load plugin zh-CN package
-> attach resource dictionary to plugin resource scope
-> refresh plugin UI
```

## 4. 插件停用流程

```text
Plugin stopping
-> block new localization lookup
-> detach plugin UI / route
-> remove AtomUI/Avalonia resource dictionaries
-> revoke package descriptors
-> clear plugin resource cache
-> dispose language packages
-> release ContributionLease
```

## 5. Assembly 卸载

插件语言包 assembly 必须随插件卸载。

禁止：

- Host 静态缓存插件 ResourceManager。
- Host 静态缓存插件 language assembly。
- Host 长期持有插件 localizer delegate。
- AtomUI/Avalonia 仍持有插件 ResourceDictionary。
- 插件 generated accessor 泄漏到 Host 静态缓存。

## 6. Contract 边界

跨插件边界使用的 MessageKey、ResourceKey、ErrorCode 可以是字符串或 Host 共享 contract 中的强类型 key。

插件私有资源类型不能被 Host 长期持有。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| 插件 package 缺失 | fallback 或 missing marker。 |
| 插件 package 加载失败 | 记录诊断，不影响 Host。 |
| 插件资源撤销失败 | 进入插件卸载错误聚合。 |
| 卸载后仍有引用 | 标记 UnloadPending。 |

## 8. 测试策略

测试必须覆盖：

- 插件 manifest 注册。
- 插件当前 culture package 懒加载。
- 插件未选 culture package 不加载。
- 插件 UI 资源撤销。
- 插件 package assembly 卸载。
- Host 不持有插件私有引用。
