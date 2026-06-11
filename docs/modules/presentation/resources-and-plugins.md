# AtomUI.City.Presentation 资源与插件设计

版本：v0.1
状态：正式初版
适用范围：AtomUI/Avalonia 资源、主题、插件 View、插件资源贡献和撤销

## 1. Resource 和 Theme 集成

Presentation 接入 AtomUI/Avalonia 资源系统。

资源类型：

- Styles。
- Themes。
- Icons。
- Templates。
- Fonts。
- Images。
- Localization resource bridge。

插件资源必须通过 ContributionLease 进入 `IPresentationResourceRegistry`。

## 2. 插件贡献

插件可以贡献：

- View。
- Style。
- Theme resource。
- Icon。
- Data template。
- Interaction handler。
- Presentation resource。

## 3. 插件贡献规则

- 必须有 ContributionLease。
- 必须记录 PluginId。
- 必须可撤销。
- 不能污染 Host Root resource registry。
- 不能让 Host 静态缓存持有插件私有 View 类型实例。
- 停用时必须先停止新入口，再关闭活动 UI，再撤销资源。

插件 View/ViewModel 绑定中跨边界传递的公共类型必须位于 Host 共享 contract 程序集。

## 4. 停用流程

```text
Stop new view creation from plugin
-> Detach active plugin views
-> Remove plugin resources
-> Clear resource cache
-> Dispose plugin resource scope
```

## 5. AOT 和 Source Generator

Presentation generator 负责：

- 生成 Resource manifest。
- 生成 Interaction handler descriptor。
- 生成 Validation binding descriptor。
- 诊断插件 View 类型泄漏。
- 诊断运行时资源扫描。

## 6. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| resource contribution | Unit | 资源通过 lease 注册。 |
| resource revoke | Unit | lease 撤销后资源不可用。 |
| plugin View close | Unit | 插件停用关闭活动 View。 |
| Host root 污染 | Unit/Analyzer | 插件资源不进入 Host root registry。 |
| 插件类型泄漏 | Analyzer/Generator | 输出稳定诊断。 |
