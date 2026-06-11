# 模块模板设计

版本：v0.1
状态：正式初版
适用范围：模块类、模块依赖、服务注册、配置、Contribution、source generator 输入和模块测试

## 1. 目标

模块模板用于创建 AtomUI.City 模块骨架。模块是应用组成单元和能力贡献方，不是生命周期 Scope，也不是插件。

## 2. 默认结构

```text
src/<ProjectName>/Modules/<ModuleName>/
  <ModuleName>Module.cs
  <ModuleName>Options.cs
  <ModuleName>Contributions.cs
tests/<ProjectName>.Tests/Modules/<ModuleName>/
  <ModuleName>ModuleTests.cs
```

## 3. Module Id

规则：

- 默认 Module Id 使用模块类全名。
- 只有需要公开稳定 Id、跨版本兼容、插件发布或清单对外暴露时，才显式指定。
- 不强制要求用户写 `[Module("...")]`。

## 4. 模块内容

模板生成：

- Module 类。
- 模块依赖声明入口。
- 服务注册入口。
- 配置入口。
- Contribution 入口。
- source generator 可识别声明。
- 模块单元测试。
- 测试矩阵条目。

## 5. 服务注册

规则：

- 模块服务注册进入对应 ServiceCollection。
- 不在服务注册阶段构建 ServiceProvider。
- 插件模块不能修改 Host Root ServiceProvider。
- 自动服务注册必须 AOT 友好。

## 6. 配置

如果生成 Options：

- Options 有明确 section。
- 支持 validation。
- 支持 PreConfigure。
- 默认生成 Options 单元测试。

## 7. Contribution

如果模块贡献路由、权限、本地化、事件或 Data client：

- Contribution 必须可撤销。
- Contribution 必须能进入 manifest。
- ContributionId 必须稳定。
- 测试必须覆盖 lease 创建和撤销。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| Module Id | Unit | 默认全名、显式 Id。 |
| 依赖声明 | Unit | 拓扑排序、缺失依赖、循环依赖。 |
| 服务注册 | Unit | 服务进入正确容器，不构建 ServiceProvider。 |
| Options | Unit | binding、validation、PreConfigure。 |
| Contribution | Unit | request、lease、revoke。 |
| generator 输入 | Generator | 模块清单可生成。 |
