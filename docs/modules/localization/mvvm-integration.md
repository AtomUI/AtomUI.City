# AtomUI.City.Localization MVVM Integration 设计

版本：v0.1
状态：正式初版
适用范围：ViewModel localizer、强类型 accessor、Command 文本、Interaction、Validation 和 ActivationScope 绑定。

## 1. 定位

MVVM 集成让 ViewModel、Command、Interaction 和 Validation 使用统一本地化能力。

Mvvm 不实现资源查找。Localization 提供文本和 culture notification。Presentation 负责 UI 展示刷新。

## 2. ViewModel Localizer

字符串 key 模式：

```csharp
public sealed partial class SettingsViewModel
{
    private readonly IStringLocalizer<SettingsViewModel> _localizer;

    public string Title => _localizer["Settings.Title"];
}
```

强类型模式：

```csharp
public string Title => _texts.Settings.Title();
```

## 3. ActivationScope

Localization subscription 必须绑定 `ActivationScope`。

规则：

- ViewModel 激活时订阅 culture change。
- ViewModel 停用时释放订阅。
- ViewModel 构造函数不启动长期订阅。
- 插件 ViewModel 的 localizer 不泄漏到 Host 静态缓存。

## 4. Command

Command metadata 可以声明：

```text
TextKey
ToolTipKey
DescriptionKey
IconKey
```

Culture 变化后：

```text
CultureChanged
-> command text provider refresh
-> Presentation updates menu / toolbar / shortcut UI
```

Command 可执行性不由 Localization 决定。

## 5. Interaction

Interaction request 不应传固定显示文本。

推荐传：

- TitleKey。
- MessageKey。
- ButtonKey。
- MessageArgs。

Presentation handler 在显示时查找当前 culture 文本。

## 6. Validation

Validation message 应使用 MessageKey + MessageArgs。

文化切换后，仍显示的 validation message 必须可刷新。

## 7. 测试策略

测试必须覆盖：

- ViewModel localizer lookup。
- strong typed accessor。
- ActivationScope 停用释放 subscription。
- Command text culture refresh。
- Interaction message refresh。
- Validation message refresh。
