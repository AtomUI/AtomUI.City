# AtomUI.City.Localization Detailed Design

版本：v0.1
状态：正式初版
适用范围：多语言资源、文化切换、语言包懒加载、独立语言包 assembly、AtomUI/Avalonia 集成、UI 热刷新、插件资源、AOT/source generator 和测试策略。

## 1. 定位

`AtomUI.City.Localization` 是应用级多语言资源运行时。

Localization 负责文化状态、资源包管理、按当前语言懒加载语言包、资源查找、UI 热刷新、插件资源撤销、缺失诊断和 source generator 资源索引。

Localization 不决定业务文案，不直接渲染 UI，不替代 Presentation 的 UI 绑定。它必须让 View、ViewModel、Route、Command、Validation、Data/Security 错误都能用统一方式表达本地化文本。

核心链路：

```text
Localization manifest
-> selected culture
-> lazy load active language packages
-> localized resource store
-> Presentation localization bridge
-> AtomUI/Avalonia resources and bindings
```

## 2. 设计原则

- Culture-first lazy loading：懒加载以当前 culture 的语言包为单位，不按单个 key 零散加载。
- Manifest-only startup：启动只加载 manifest，不加载所有语言包。
- Assembly package capable：普通 .NET 运行时支持语言包独立 assembly 动态加载。
- AOT compatible：Native AOT 模式使用 file-based locpack provider，不依赖动态 assembly loading。
- AtomUI-integrated：文化变化最终通过 Presentation bridge 同步到 AtomUI/Avalonia。
- Transactional culture switch：文化切换必须先准备资源，再提交状态，失败回滚。
- Plugin-aware：插件语言包必须可撤销、可释放、可卸载。
- Strong diagnostics：缺失 key、重复 key、fallback 失败和格式化错误必须可诊断。
- Source-generator-first：资源 manifest、强类型 key 和 descriptor 由 source generator 生成。
- Testable：支持无真实 UI 的文化切换、查找、fallback、插件撤销和 UI refresh 测试。

## 3. 非目标

Localization 不负责：

- 业务翻译内容。
- 在线翻译服务。
- 翻译工作流系统。
- AtomUI/Avalonia 控件实现。
- UI 布局自适应策略。
- 业务错误模型。
- 具体资源编辑器。

## 4. 核心抽象

| 类型 | 职责 |
|---|---|
| `ILocalizationService` | 当前文化、语言包加载、文化切换和资源查找入口。 |
| `ICultureStateProvider` | 提供当前文化状态和 revision。 |
| `ICultureManager` | 处理文化选择、用户偏好、系统文化和事务式切换。 |
| `ILanguagePackageProvider` | 加载指定 culture 的语言包。 |
| `ILanguagePackage` | 已加载语言包。 |
| `ILocalizedResourceStore` | 按 culture、scope、key 查找资源。 |
| `IStringLocalizer` | 字符串查找和格式化入口。 |
| `ILocalizedText` | 可随文化变化刷新显示值的本地化文本句柄。 |
| `ILocalizationContributionRegistry` | 管理 Host、Module、Plugin 的资源贡献。 |
| `IPresentationLocalizationBridge` | Presentation 侧 AtomUI/Avalonia 同步桥。 |
| `ILocalizationDiagnostics` | 缺失资源、加载失败、fallback 和刷新诊断。 |

命名不加 `City` 前缀。

## 5. 资源分层

资源来源：

```text
Host resources
Module resources
Plugin resources
Theme / Presentation resources
Feature resources
```

查找优先级：

```text
Current feature / plugin
-> owning module
-> application host
-> shared framework
-> fallback culture
-> invariant fallback
-> missing resource marker
```

详细规则见：[resource-model.md](resource-model.md) 和 [lookup-and-fallback.md](lookup-and-fallback.md)。

## 6. 语言包懒加载

Localization 懒加载以 language package 为单位。

```text
Startup
-> load localization manifests only
-> know available cultures and package descriptors
-> do not load language packages

Current culture = zh-CN
-> load active zh-CN packages
-> load fallback packages only when needed
-> commit culture
-> refresh UI
```

关键约束：

- 每个语言包只服务一个 culture。
- 当前选择语言决定实际加载哪些 language package。
- 插件启用不等于加载所有语言资源。
- 模块注册不等于加载所有语言资源。
- Fallback culture 按需加载，不能一次性加载全部 fallback。

详细规则见：[lazy-loading.md](lazy-loading.md)。

## 7. 语言包 Assembly

普通 .NET 运行时支持语言包放在独立 assembly 中运行时动态加载。

推荐模式：

```text
AssemblyLanguagePackageProvider
FileLanguagePackageProvider
```

- `AssemblyLanguagePackageProvider`：普通 .NET / CoreCLR / 插件动态加载场景。
- `FileLanguagePackageProvider`：Native AOT 或严格 AOT 模式，使用 `.locpack`、json 或 binary resource pack。

语言包 assembly 应尽量是 resource-only，不放可执行代码。强类型 accessor 生成在模块或插件主 assembly 中，语言包 assembly 只提供资源数据。

详细规则见：[language-package-assemblies.md](language-package-assemblies.md)。

## 8. 文化切换

文化切换必须事务式。

```text
SetCultureAsync("ja-JP")
-> calculate active package set
-> load ja-JP packages
-> load fallback packages
-> validate critical resources
-> prepare AtomUI resource dictionaries
-> commit CurrentCultureState
-> swap resource dictionaries on UI Thread
-> notify localized bindings
```

加载失败时：

```text
Load failed
-> keep old culture
-> release partially loaded packages
-> emit diagnostics
```

详细规则见：[culture-management.md](culture-management.md)。

## 9. AtomUI/Avalonia 集成

Localization 不直接操作控件。文化变化通过 Presentation bridge 接入 AtomUI/Avalonia。

```text
LocalizationService.SetCultureAsync
-> load selected language packages
-> commit culture state
-> IPresentationLocalizationBridge.ApplyCultureAsync
-> update AtomUI culture
-> update Avalonia ResourceDictionary
-> notify localized bindings
```

AtomUI/Avalonia 资源更新必须在 UI Thread。

详细规则见：

- [atomui-integration.md](atomui-integration.md)
- [ui-refresh.md](ui-refresh.md)

## 10. 开发者体验

字符串 key 模式：

```csharp
public sealed partial class SettingsViewModel
{
    private readonly IStringLocalizer<SettingsViewModel> _localizer;

    public string Title => _localizer["Settings.Title"];
}
```

强类型 accessor 模式：

```csharp
public string Title => _texts.Settings.Title();
```

声明式 metadata：

```csharp
[Route("settings", TitleKey = "Settings.Title")]
public sealed partial class SettingsViewModel
{
}
```

XAML 目标语法：

```xml
<TextBlock Text="{loc:Text Settings.Title}" />
```

最终 API 细节在实现前确认，但必须同时支持字符串 key 和强类型 generated accessor。

详细规则见：[mvvm-integration.md](mvvm-integration.md)。

## 11. 插件资源

插件本地化资源必须绑定 Contribution。

```text
Plugin enable
-> register localization manifest
-> do not load all language packs
-> active plugin route requests selected culture package
-> load package
-> plugin stopping
-> block new lookup
-> detach plugin UI
-> revoke resource descriptors
-> clear resource cache
-> unload package
```

插件卸载后，Host 不能持有插件 ResourceManager、assembly、localizer delegate、generated accessor 实例或 ResourceDictionary。

详细规则见：[plugin-integration.md](plugin-integration.md)。

## 12. AOT 和 Source Generator

Localization generator 负责：

- 生成 resource manifest。
- 生成 language package descriptor。
- 生成 strongly typed accessor。
- 生成 key constants。
- 生成 module/plugin resource descriptor。
- 诊断缺失 key。
- 诊断 fallback 不完整。
- 诊断重复 key。
- 诊断未声明资源引用。
- 诊断插件资源类型泄漏。

运行时默认不扫描程序集找资源。

详细规则见：[source-generation.md](source-generation.md)。

## 13. 错误策略

| 场景 | 默认处理 |
|---|---|
| 当前文化缺 key | 查找 fallback culture。 |
| fallback 也缺 | 查找 invariant。 |
| invariant 也缺 | missing marker + diagnostics。 |
| 格式参数错误 | fallback raw template + diagnostics。 |
| 语言包加载失败 | rollback 到旧 culture。 |
| 插件资源已撤销 | fallback 或清理对应 UI。 |
| AtomUI resource apply 失败 | rollback UI resource swap 并记录错误。 |

## 14. 测试策略

Testing 包应提供：

- Fake culture state provider。
- Fake language package provider。
- Test localization service。
- Test presentation localization bridge。
- Missing resource recorder。
- Plugin localization test host。
- Deterministic culture switch driver。

必须覆盖：

- manifest-only startup。
- selected culture language package lazy load。
- fallback package lazy load。
- culture switch rollback。
- AtomUI bridge apply。
- binding refresh。
- plugin package revoke。
- AOT locpack provider。
- missing key diagnostics。

详细规则见：[diagnostics-and-testing.md](diagnostics-and-testing.md)。
