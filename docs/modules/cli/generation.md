# CLI 生成命令设计

版本：v0.1
状态：正式初版
适用范围：module、page、plugin、test、config、localization 的生成命令和模板调用

## 1. 目标

生成命令调用 Templates，不在 CLI 内硬编码模板结构。

命令：

```bash
atomui city generate module Sales
atomui city generate page Sales/List --route /sales
atomui city generate plugin com.company.sales
atomui city generate test Sales/List
atomui city generate config Sales
atomui city generate localization Sales
```

## 2. 通用流程

```text
Parse command
-> Inspect workspace
-> Resolve target project
-> Validate template variables
-> Build generation plan
-> Apply template
-> Update test matrix
-> Emit diagnostics
```

## 3. Module

`generate module` 生成：

- Module 类。
- Options，如果请求。
- Contribution 入口，如果请求。
- 模块测试。
- 测试矩阵条目。

## 4. Page

`generate page` 生成：

- Route。
- ViewModel。
- View。
- route test。
- activation test。
- View mapping 生成输入。

规则：

- 必须明确 RoutePath。
- Routing 只生成 Route -> ViewModel Target。
- Presentation 负责 ViewModel -> View。

## 5. Plugin

`generate plugin` 生成：

- 插件项目。
- 插件模块。
- 插件测试项目。
- plugin package test。
- lifecycle/unload test。

规则：

- 一个插件一个主业务程序集。
- 默认生成 `AtomUICityPluginId`。
- 默认启用 package layout validation。

## 6. Test

`generate test` 生成：

- 功能点测试文件。
- FeatureTestMatrix 条目。
- TestHost 使用入口。

## 7. Config 和 Localization

`generate config` 生成 Options、validator 和测试。

`generate localization` 生成 culture 目录、resource key 和本地化测试入口。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| module generation | CLI/Template | 文件、测试、矩阵。 |
| page generation | CLI/Template | route、VM、View、测试。 |
| plugin generation | CLI/Template | plugin csproj、manifest 输入、测试。 |
| test generation | CLI/Template | 测试文件和矩阵条目。 |
| dry-run | Unit/CLI | 不写文件。 |
| plan/apply | CLI | plan 可执行。 |
