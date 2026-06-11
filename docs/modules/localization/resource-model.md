# AtomUI.City.Localization Resource Model 设计

版本：v0.1
状态：正式初版
适用范围：Resource descriptor、resource scope、language package、资源类型、资源分层和 contribution。

## 1. 定位

Resource model 描述本地化资源如何被声明、索引、加载和查找。

Localization 不是简单的字符串字典。它需要表达 Host、Module、Plugin、Route、Theme 和 Presentation 的资源贡献关系，并支持按 culture 懒加载。

## 2. Resource Descriptor

Resource descriptor 应包含：

| 字段 | 说明 |
|---|---|
| ResourceId | 稳定资源标识。 |
| Key | 资源 key。 |
| ResourceType | String、FormattedString、Object、FlowDirection 等类型。 |
| Culture | 所属 culture。 |
| Scope | Host、Module、Plugin、Route、Window 等资源范围。 |
| Contribution | 来源 Contribution。 |
| PackageId | 语言包 id。 |
| Version | 资源版本。 |
| FallbackPolicy | fallback 策略。 |

运行时不通过扫描程序集发现 descriptor，默认消费 Source Generator manifest。

## 3. Resource Scope

资源 Scope：

| Scope | 说明 |
|---|---|
| Host | 应用全局资源。 |
| Module | 模块资源。 |
| Plugin | 插件资源。 |
| Route | 页面或路由资源。 |
| Window | 窗口级资源。 |
| Presentation | AtomUI/Avalonia UI 资源桥。 |

资源 Scope 决定加载时机、查找优先级和撤销边界。

## 4. Language Package

Language package 是懒加载基本单位。

规则：

- 每个 package 只包含一个 culture。
- package 可以来自独立 assembly 或 file-based locpack。
- package 必须有 descriptor。
- package 加载后产生 `ILocalizedResourceStore`。
- package 必须支持释放。

推荐命名：

```text
Host.zh-CN
SettingsModule.zh-CN
SalesPlugin.zh-CN
```

## 5. 资源类型

第一版必须支持字符串，架构预留更多类型：

| 类型 | 用途 |
|---|---|
| String | 普通文本。 |
| FormattedString | 参数化文本。 |
| Pluralization | 数量规则，后续增强。 |
| ResourceObject | 图片、图标、字体、文档片段。 |
| FlowDirection | RTL / LTR。 |
| CultureMetadata | 日期、数字、货币格式 metadata。 |
| ValidationMessage | 表单验证消息。 |
| ErrorMessage | 错误展示。 |
| CommandText | 菜单、按钮、快捷入口。 |
| RouteTitle | 页面标题、面包屑。 |

## 6. 资源分层

查找层级：

```text
Current feature / plugin
-> owning module
-> application host
-> shared framework
-> fallback culture
-> invariant fallback
-> missing resource marker
```

插件资源不能覆盖 Host 内置资源，除非 Host 显式允许 extension point。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| descriptor 重复 | 构建期诊断。 |
| resource type 不匹配 | fallback，并记录诊断。 |
| package version 不兼容 | 拒绝加载 package。 |
| contribution 已撤销 | 拒绝查找或 fallback。 |

## 8. 测试策略

测试必须覆盖：

- Host / Module / Plugin descriptor。
- Resource scope 查找优先级。
- package 版本不兼容。
- 插件资源撤销。
- resource type mismatch。
