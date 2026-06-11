# 配置模板设计

版本：v0.1
状态：正式初版
适用范围：Options、配置 section、PreConfigure、配置验证、reloadable 配置和配置测试

## 1. 目标

配置模板用于生成符合 Core Configuration 约定的 Options 和配置结构。

## 2. 默认结构

```text
Configuration/
  <FeatureName>Options.cs
  <FeatureName>OptionsValidator.cs
tests/<ProjectName>.Tests/Configuration/
  <FeatureName>OptionsTests.cs
```

## 3. Options

规则：

- Options 必须有明确 section。
- Options 名称使用用户命名空间。
- 默认生成 validation。
- reloadable 必须显式声明。
- 插件 Options 必须按 PluginId 分区。

## 4. PreConfigure

模板可生成 PreConfigure 入口。

规则：

- PreConfigure 用于模块默认值和提前配置。
- PreConfigure 不执行 IO。
- PreConfigure 不构建 ServiceProvider。
- 插件拥有自己的 PreConfigure store。

## 5. 配置文件

模板可以生成：

```text
appsettings.json
appsettings.Development.json
```

规则：

- 默认配置最小化。
- 不生成业务配置项。
- 插件默认配置进入插件资源或 manifest。

## 6. 测试

默认生成：

- binding test。
- validation test。
- PreConfigure test。
- reloadable test，如果启用。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| Options section | Unit | section name 稳定。 |
| binding | Unit | 配置绑定成功。 |
| validation | Unit | 成功和失败。 |
| PreConfigure | Unit | 默认值和覆盖顺序。 |
| plugin config | Unit | 插件配置隔离。 |
