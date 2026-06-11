# AtomUI.City.Localization AtomUI Integration 设计

版本：v0.1
状态：正式初版
适用范围：Presentation bridge、AtomUI culture adapter、Avalonia ResourceDictionary、UI Thread、FlowDirection 和资源撤销。

## 1. 定位

Localization 最终需要反映到 AtomUI/Avalonia UI。

Localization Core 不直接引用 AtomUI/Avalonia。Presentation 负责提供 `IPresentationLocalizationBridge`，把文化状态和资源变化同步到 AtomUI/Avalonia。

## 2. 集成链路

```text
LocalizationService.SetCultureAsync
-> load selected language packages
-> commit CultureState
-> IPresentationLocalizationBridge.ApplyCultureAsync
-> update AtomUI culture
-> update Avalonia ResourceDictionary
-> notify localized bindings
```

AtomUI/Avalonia resource apply 必须发生在 UI Thread。

## 3. Bridge 职责

`IPresentationLocalizationBridge` 负责：

- 接收 CultureState。
- 构建或更新 localized ResourceDictionary。
- 同步 AtomUI culture。
- 应用 FlowDirection。
- 刷新 localized binding。
- 撤销插件资源字典。
- 输出 UI refresh diagnostics。

## 4. ResourceDictionary Scope

资源字典挂载建议：

| 来源 | 挂载位置 |
|---|---|
| Host 核心语言包 | Application resources。 |
| Window 语言资源 | Window resources。 |
| Route 页面语言资源 | Route / View resource scope。 |
| Plugin 语言资源 | Plugin contribution resource scope。 |
| Theme / AtomUI 文案 | Presentation / AtomUI bridge。 |

禁止把所有模块和插件语言资源都塞进全局 Application resources。

## 5. FlowDirection

Culture metadata 可以影响 FlowDirection。

规则：

- RTL/LTR 变化由 Presentation bridge 应用。
- FlowDirection 变化必须触发布局刷新。
- 不支持 RTL 的 View 可以声明限制，诊断必须可见。

## 6. 插件撤销

插件停用时：

```text
Block new localization lookup
-> detach plugin UI
-> remove plugin ResourceDictionary
-> clear AtomUI resource references
-> revoke language package
-> emit diagnostics
```

撤销后 AtomUI/Avalonia 不得继续持有插件语言包 assembly 或 ResourceDictionary。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| UI dispatcher 未 ready | 等待 ready 或返回明确错误。 |
| ResourceDictionary apply failed | rollback Presentation resources。 |
| FlowDirection apply failed | 保留旧方向并记录诊断。 |
| 插件资源字典移除失败 | 进入插件卸载错误聚合。 |

## 8. 测试策略

测试必须覆盖：

- bridge apply culture。
- resource dictionary swap。
- UI Thread enforcement。
- FlowDirection change。
- plugin resource dictionary revoke。
- apply failed rollback。
