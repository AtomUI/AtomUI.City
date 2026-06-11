# AtomUI.City.Presentation ViewLocator 设计

版本：v0.1
状态：正式初版
适用范围：ViewModel 到 ViewDescriptor 的定位、View manifest、重复 View 诊断和插件 View 撤销

## 1. 定位

ViewLocator 负责 `ViewModel -> ViewDescriptor`。

第一版不依赖运行时命名约定扫描。

## 2. 声明方式

推荐声明：

```csharp
[ViewFor(typeof(SettingsViewModel))]
public sealed partial class SettingsView : UserControl
{
}
```

Source Generator 生成 View manifest：

```text
ViewModelType
-> ViewType
-> Contribution
-> Resource scope
-> Factory descriptor
```

## 3. 规则

- 一个 ViewModel 默认只能有一个默认 View。
- 多 View 场景必须显式命名，例如 `ViewKey`。
- ViewLocator 不创建 ViewModel。
- ViewLocator 不解释 Route。
- 插件 View 必须记录 PluginId 和 ContributionId。
- 插件卸载时必须撤销对应 View descriptor。

## 4. AOT 和 Source Generator

Presentation generator 负责：

- 生成 View/ViewModel binding manifest。
- 生成 View factory descriptor。
- 诊断重复默认 View。
- 诊断 ViewModel 没有 View。
- 诊断插件 View 类型泄漏。
- 诊断运行时扫描和命名约定定位。

运行时禁止扫描程序集找 View。

## 5. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| ViewLocator 命中 | Unit | ViewModel 定位到 ViewDescriptor。 |
| 找不到 View | Unit | 返回 commit failure 诊断。 |
| 多默认 View | Analyzer/Generator | 输出重复 View 诊断。 |
| 命名 View | Unit | ViewKey 能选择对应 View。 |
| 插件 View 撤销 | Unit | 撤销后不能定位插件 View。 |
