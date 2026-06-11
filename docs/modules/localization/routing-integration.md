# AtomUI.City.Localization Routing Integration 设计

版本：v0.1
状态：正式初版
适用范围：Route title、breadcrumb、错误路由、Resolver/Guard 文案、导航诊断和 route language package preload。

## 1. 定位

Routing 集成让路由 metadata 使用本地化 key，而不是固定显示文本。

Routing 不查找资源，不操作 UI。Localization 解析 key，Presentation 展示文本。

## 2. Route Metadata

Route 可以声明：

```text
TitleKey
DescriptionKey
BreadcrumbKey
GroupKey
ErrorTitleKey
```

Source Generator 将这些写入 Route descriptor。

## 3. 页面进入预加载

Route activated 可以触发当前 culture 的 route language package 预加载。

```text
Route matched
-> identify route localization package descriptors
-> load selected culture packages
-> continue Presentation binding
```

预加载失败按资源 criticality 决定是 fallback、missing marker 还是导航失败。

## 4. Guard / Resolver 文案

Guard、Resolver 不返回显示文本。

它们返回：

- ErrorCode。
- MessageKey。
- MessageArgs。
- Diagnostics。

Presentation 或 ViewModel 通过 Localization 渲染。

## 5. Culture 切换

文化切换后必须刷新：

- 当前 route title。
- breadcrumb。
- navigation menu。
- route error view。
- navigation diagnostics display。

Routing 的 NavigationSnapshot 不因 culture change 重新创建。

## 6. 插件路由

插件路由标题和 breadcrumb 使用插件本地化资源。

插件停用时：

- route contribution 撤销。
- language package 撤销。
- navigation UI fallback 或移除。

## 7. 测试策略

测试必须覆盖：

- Route title key。
- breadcrumb refresh。
- route language package preload。
- Guard message key。
- Resolver message key。
- 插件 route resource revoke。
