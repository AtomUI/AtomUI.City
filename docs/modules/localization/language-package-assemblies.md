# AtomUI.City.Localization Language Package Assemblies 设计

版本：v0.1
状态：正式初版
适用范围：独立语言包 assembly、satellite assembly、collectible AssemblyLoadContext、ResourceManager、Native AOT locpack fallback 和卸载约束。

## 1. 定位

普通 .NET 桌面运行时下，Localization 支持语言包放在独立 assembly 中运行时动态加载。

语言包 assembly 推荐是 resource-only assembly，不放可执行代码。它只承载指定 culture 的资源数据，不承载业务逻辑。

Native AOT 不支持运行时动态加载 assembly，因此必须提供 file-based locpack fallback。

## 2. Provider 模型

统一抽象：

```text
ILanguagePackageProvider
-> AssemblyLanguagePackageProvider
-> FileLanguagePackageProvider
```

| Provider | 场景 |
|---|---|
| `AssemblyLanguagePackageProvider` | CoreCLR、普通桌面运行时、插件动态加载。 |
| `FileLanguagePackageProvider` | Native AOT、严格 trimming、独立资源包。 |

两者都输出同一套 `ILanguagePackage` / `ILocalizedResourceStore` contract。

## 3. Assembly 语言包布局

推荐使用 .NET satellite assembly 风格：

```text
locales/
  zh-CN/
    AtomUI.City.App.resources.dll
    SettingsModule.resources.dll
    SalesPlugin.resources.dll
  en-US/
    AtomUI.City.App.resources.dll
    SettingsModule.resources.dll
    SalesPlugin.resources.dll
```

也允许自定义命名：

```text
SalesModule.Localization.zh-CN.dll
SalesModule.Localization.en-US.dll
```

但无论命名如何，descriptor 必须说明：

- Culture。
- PackageId。
- Assembly path。
- Resource base name。
- ContributionId。
- Version。
- Checksum。

## 4. 加载流程

```text
Language package descriptor
-> resolve package path
-> load assembly in package load context
-> create resource store
-> expose lookup table / ResourceManager adapter
-> register package lease
```

Host app 语言包可以加载到默认上下文或专用上下文。插件语言包必须跟插件加载上下文和 ContributionLease 绑定。

## 5. 卸载约束

如果语言包需要随插件卸载，必须避免外部强引用。

禁止：

- Host 静态缓存插件语言包 assembly。
- Host 静态缓存插件 `ResourceManager`。
- Host 静态缓存插件 localizer delegate。
- Host 持有插件语言包里的 Type。
- AtomUI/Avalonia ResourceDictionary 未移除就卸载插件。
- generated accessor 类型放进语言包 assembly。

强类型 accessor 应生成在模块或插件主 assembly。语言包 assembly 只提供资源数据。

## 6. Native AOT Locpack

Native AOT 模式使用 file-based locpack。

可选格式：

- `.locpack` binary。
- json。
- embedded generated table。

规则：

- locpack 不依赖动态 assembly loading。
- locpack descriptor 与 assembly package descriptor 语义一致。
- Source Generator 生成 locpack manifest。
- 运行时通过 `FileLanguagePackageProvider` 加载当前 culture 的 locpack。

## 7. 安全和完整性

语言包 assembly / locpack 应支持：

- checksum。
- version。
- culture metadata。
- package compatibility。
- plugin ownership。

语言包不作为安全边界。不可信语言包仍然必须受 Host 安全策略和插件策略约束。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| assembly 不存在 | fallback 或 culture switch rollback。 |
| assembly 加载失败 | fallback 或 rollback。 |
| checksum 不匹配 | 拒绝加载。 |
| culture 不匹配 | 拒绝加载。 |
| AOT 下请求 assembly provider | 返回不支持，使用 file provider。 |
| 卸载后仍有引用 | 标记 UnloadPending，输出诊断。 |

## 9. 测试策略

测试必须覆盖：

- assembly package 加载。
- satellite-style package 解析。
- file locpack 加载。
- AOT provider fallback。
- checksum mismatch。
- plugin package unload。
- Host 不持有插件 package 引用。
