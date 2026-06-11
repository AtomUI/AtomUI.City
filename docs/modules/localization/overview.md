# AtomUI.City.Localization

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Localization` 负责本地化资源、文化切换、语言包懒加载、文本刷新、模块化资源注册和插件资源撤销。

Localization 需要支持模块和插件独立贡献资源，并让 Presentation 把文化变化同步到 AtomUI/Avalonia UI。

Localization 第一版必须支持：

- 按当前文化懒加载语言包。
- 语言包放在独立 assembly 中运行时动态加载。
- Native AOT 场景下使用 file-based locpack fallback。
- 文化切换后 UI 热刷新。
- 插件语言包撤销和缓存清理。

## 边界

Localization 负责：

- 资源注册。
- 资源查找。
- 当前文化状态。
- 文化切换。
- 语言包懒加载。
- 文本刷新通知。
- 模块资源隔离。
- 插件资源撤销。
- 缺失资源诊断。

Localization 不负责：

- 业务翻译内容。
- 在线翻译服务。
- UI 控件渲染。
- AtomUI/Avalonia 控件实现。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Localization 总体架构、语言包懒加载、文化切换、AtomUI 集成、插件资源和测试策略。 |
| [resource-model.md](resource-model.md) | Resource descriptor、resource scope、语言包、资源类型和资源分层。 |
| [culture-management.md](culture-management.md) | 当前文化状态、用户偏好、系统文化、事务式文化切换和失败回滚。 |
| [lazy-loading.md](lazy-loading.md) | manifest-only startup、按当前语言包懒加载、active scope 资源加载和缓存。 |
| [language-package-assemblies.md](language-package-assemblies.md) | 独立语言包 assembly、satellite assembly、collectible ALC、Native AOT locpack fallback。 |
| [lookup-and-fallback.md](lookup-and-fallback.md) | 查找优先级、fallback culture、missing marker、格式化和资源撤销。 |
| [atomui-integration.md](atomui-integration.md) | Presentation bridge、AtomUI culture adapter、Avalonia ResourceDictionary 和 UI Thread 规则。 |
| [ui-refresh.md](ui-refresh.md) | 文化变化后的 binding refresh、Window title、Route title、Command、Validation 和 Interaction 文本刷新。 |
| [mvvm-integration.md](mvvm-integration.md) | ViewModel localizer、强类型 accessor、Command 文本、Interaction 和 culture-aware notification。 |
| [routing-integration.md](routing-integration.md) | Route title、breadcrumb、错误路由、Resolver/Guard 文案和导航诊断本地化。 |
| [validation-and-errors.md](validation-and-errors.md) | Validation、Data/Security error、MessageKey/MessageArgs 和运行时文化切换刷新。 |
| [plugin-integration.md](plugin-integration.md) | 插件本地化贡献、语言包懒加载、撤销、卸载和跨插件 contract。 |
| [source-generation.md](source-generation.md) | Resource manifest、强类型 key、descriptor、缺失 key、重复 key 和 AOT 诊断。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | 缺失资源诊断、加载失败、fake culture provider、插件撤销和 UI 刷新测试。 |

## 可选增强文档

- `pluralization.md`
- `formatting.md`
- `design-time-tooling.md`
