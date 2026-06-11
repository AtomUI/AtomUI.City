# 插件模板设计

版本：v0.1
状态：正式初版
适用范围：插件项目、PluginId、主程序集、模块、manifest、资源、打包配置和插件测试

## 1. 目标

插件模板用于创建符合 PluginSystem 和 Build 约定的插件项目。

第一版规则：

- 一个插件一个主业务程序集。
- 一个插件发布成一个独立 NuGet 包。
- 插件通过普通模块贡献能力。
- 插件不修改 Host Root ServiceProvider。

## 2. 默认结构

```text
src/<PluginName>/
  <PluginName>.csproj
  <PluginName>Module.cs
  Resources/
  Localization/
  atomui-city/
tests/<PluginName>.Tests/
  FeatureTestMatrix.md
  PluginPackageTests.cs
  PluginLifecycleTests.cs
```

## 3. 项目属性

插件项目默认包含：

```xml
<AtomUICityPlugin>true</AtomUICityPlugin>
<AtomUICityPluginId>...</AtomUICityPluginId>
<AtomUICityPluginDisplayNameKey>...</AtomUICityPluginDisplayNameKey>
<AtomUICityPluginDescriptionKey>...</AtomUICityPluginDescriptionKey>
<AtomUICityPluginApiVersion>1.0</AtomUICityPluginApiVersion>
<AtomUICityPackageAsPlugin>true</AtomUICityPackageAsPlugin>
```

规则：

- `PluginId` 默认由模板变量生成。
- `DisplayName` 和 `Description` 使用本地化 key。
- 默认启用 manifest 生成和 package layout validation。

## 4. 插件模块

插件模块使用普通 Module 抽象。

规则：

- 不生成单独的公共 `PluginModule` 基类。
- 插件模块依赖通过模块依赖声明表达。
- 插件模块贡献必须生成 lease。
- 插件模块服务进入插件 ServiceScope。

## 5. 资源和本地化

默认结构：

```text
Localization/
  en-US/
  zh-CN/
Resources/
  assets/
```

规则：

- 本地化资源必须可懒加载。
- 语言包可生成 assembly 或 `.locpack`。
- 插件卸载时资源必须可撤销。

## 6. 打包

插件模板必须生成符合 Build 插件打包规则的项目：

- `atomui-city/plugin.json` 由 Build 生成。
- contribution manifests 由 source generator 和 Build task 生成。
- package 输出到 `output/packages/plugins`。
- 包布局必须通过 validation。

## 7. 测试

插件模板默认生成：

- package layout test。
- plugin manifest test。
- plugin lifecycle test。
- contribution lease revoke test。
- operation cancellation test。
- unload assertion test。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| PluginId | Unit/Build | 生成、格式、manifest 字段。 |
| 包布局 | Build | 单主程序集、plugin.json、资源。 |
| 模块贡献 | Unit | contribution request、lease。 |
| 插件生命周期 | Plugin test | load、activate、deactivate、unload。 |
| 卸载 | Plugin test | operation、subscription、lease、ALC。 |
