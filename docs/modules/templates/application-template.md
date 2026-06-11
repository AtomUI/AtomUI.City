# 应用模板设计

版本：v0.1
状态：正式初版
适用范围：AtomUI.City 应用模板、Host 启动、模块入口、配置、本地化、测试项目和 Build 接入

## 1. 目标

应用模板用于创建一个可运行的 AtomUI.City 桌面应用。它必须体现框架默认编程范式，但不引入业务概念。

## 2. 默认结构

```text
src/<AppName>/
  <AppName>.csproj
  Program.cs
  App.axaml
  App.axaml.cs
  Modules/
  Routes/
  Resources/
  Configuration/
  Localization/
tests/<AppName>.Tests/
  FeatureTestMatrix.md
  ApplicationSmokeTests.cs
```

## 3. 默认能力

应用模板默认包含：

- GenericHost 启动入口。
- AtomUI.City Core 配置。
- Lifecycle 接入。
- ModuleSystem 接入。
- Routing 接入。
- Presentation 接入。
- Localization 接入。
- `AtomUI.City.Build` 引用。
- 测试项目。
- 最小 App root。

可选启用：

- Security。
- Data。
- EventBus。
- PluginSystem。
- Dynamic plugins。
- Native AOT strict。

## 4. Program 入口

模板生成的入口必须使用 Host builder 风格。

设计要求：

- 使用 .NET 扩展方法组织配置。
- 不在入口中写业务代码。
- 不直接构建 ServiceProvider 做服务解析。
- 不在入口中执行插件加载细节。
- 生命周期和 Presentation runtime 通过 Host 接入。

## 5. 项目文件

应用项目必须引用：

- `AtomUI.City.Core`
- `AtomUI.City.Mvvm`
- `AtomUI.City.Routing`
- `AtomUI.City.Presentation`
- `AtomUI.City.Localization`
- `AtomUI.City.Build`

可选引用：

- `AtomUI.City.State`
- `AtomUI.City.EventBus`
- `AtomUI.City.Data`
- `AtomUI.City.Security`
- `AtomUI.City.PluginSystem`

## 6. Build 接入

项目默认启用：

- manifest generation。
- manifest validation。
- source generation strict mode。
- analyzer。
- `output/` 输出约定。

Native AOT 是否开启由模板变量决定。

## 7. 测试项目

默认生成测试项目：

```text
tests/<AppName>.Tests/
```

测试项目必须包含：

- `FeatureTestMatrix.md`
- TestHost smoke test。
- Host startup test。
- Manifest generation smoke test。

## 8. Sample 策略

默认不生成业务页面。

如果提供 `IncludeSample=true`，示例必须明确标记为 sample，不能污染默认编程范式，也不能引入业务域概念作为默认结构。

## 9. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 应用生成 | Smoke | 文件结构完整。 |
| Host 启动 | Framework integration | TestHost 能启动最小应用。 |
| Build 接入 | Build | manifest 生成、analyzer 启用。 |
| 测试项目 | Unit | `FeatureTestMatrix.md` 和 smoke test 存在。 |
| 可选能力 | Unit/Build | 开关影响引用和配置。 |
