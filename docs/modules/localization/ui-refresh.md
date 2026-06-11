# AtomUI.City.Localization UI Refresh 设计

版本：v0.1
状态：正式初版
适用范围：文化变化后的 binding refresh、Window title、Route title、Command、Validation、Interaction 和错误文本刷新。

## 1. 定位

UI refresh 负责让文化变化后现有界面自动更新文本。

开发者不应该在每个 ViewModel 手写大量 `OnPropertyChanged`。Localization 应提供 culture-aware binding/reaction 机制。

## 2. 刷新链路

```text
CultureState committed
-> LocalizationChanged notification
-> Presentation binding adapter refresh
-> AtomUI/Avalonia resources swapped
-> View text updates
-> Command / Route / Validation text refresh
```

## 3. 刷新范围

必须支持刷新：

- XAML localized binding。
- Window title。
- Route title。
- Breadcrumb。
- Command text。
- Command tooltip。
- Validation message。
- Dialog / Interaction 文案。
- Data / Security error message。
- Notification / Toast 文案。

## 4. Culture-aware Binding

XAML 目标语法：

```xml
<TextBlock Text="{loc:Text Settings.Title}" />
```

规则：

- Binding 订阅 CultureState revision。
- View detached 后释放订阅。
- 插件 View 的 binding 随插件 UI 释放。
- Binding refresh 必须在 UI Thread。

## 5. ViewModel 文本

ViewModel 可以使用 `ILocalizedText` 或强类型 accessor。

规则：

- `ILocalizedText` 可以随 culture change 刷新。
- 简单字符串属性可以由 source generator 生成 culture-aware notification。
- ViewModel 停用时释放 localization subscription。

## 6. 错误策略

| 场景 | 默认处理 |
|---|---|
| binding key missing | missing marker + diagnostics。 |
| refresh callback failed | 记录错误，不阻止其他 binding。 |
| View detached | 停止刷新。 |
| plugin resource revoked | fallback 或 clear UI。 |

## 7. 测试策略

测试必须覆盖：

- XAML binding refresh。
- Window title refresh。
- Command text refresh。
- Validation message refresh。
- plugin View detached 后不刷新。
- missing key marker。
