# 模板变量设计

版本：v0.1
状态：正式初版
适用范围：模板变量、命名规则、命名空间、路径规则、默认值和参数校验

## 1. 目标

模板变量必须稳定、可验证，并且不把框架命名空间误用到用户项目中。

## 2. 核心变量

| 变量 | 说明 |
|---|---|
| `AppName` | 应用名。 |
| `RootNamespace` | 用户项目根命名空间。 |
| `ModuleName` | 模块名。 |
| `PageName` | 页面名。 |
| `RoutePath` | 路由路径。 |
| `PluginId` | 插件运行时 Id。 |
| `PackageId` | NuGet 包 Id。 |
| `TargetFramework` | 目标框架。 |
| `UseAot` | 是否启用 AOT 友好默认设置。 |
| `UseDynamicPlugins` | 是否启用动态插件模式。 |
| `IncludeTests` | 是否生成测试项目，默认 true。 |
| `IncludeSample` | 是否生成示例内容，默认 false。 |

## 3. 命名空间规则

规则：

- 用户代码使用 `RootNamespace`。
- 用户项目不使用 `AtomUI.City.*` 命名空间。
- 生成测试项目使用 `<RootNamespace>.Tests`。
- 插件项目使用用户指定或派生的命名空间。
- 框架扩展方法来自 `AtomUI.City.*` 包。

## 4. 路径规则

规则：

- 应用代码进入 `src/`。
- 测试代码进入 `tests/`。
- 模板不直接写 `output/`。
- Build 负责生成 `output/`。
- 插件模板可以包含 `atomui-city/` 资源目录，但最终 manifest 由 Build 生成。

## 5. 默认值

建议默认：

| 变量 | 默认值 |
|---|---|
| `TargetFramework` | 当前框架支持的默认 TFM。 |
| `IncludeTests` | `true`。 |
| `IncludeSample` | `false`。 |
| `UseAot` | `false`。 |
| `UseDynamicPlugins` | `false`。 |

## 6. 校验规则

必须校验：

- 名称是合法 C# identifier 或可转换。
- `RoutePath` 符合路由语法。
- `PluginId` 符合反向域名建议格式。
- `PackageId` 符合 NuGet 包名要求。
- `RootNamespace` 不以 `AtomUI.City` 开头。
- `UseAot=true` 时不默认启用动态插件。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 名称校验 | Unit | 合法和非法 identifier。 |
| 命名空间 | Unit | 不生成 `AtomUI.City.*` 用户命名空间。 |
| RoutePath | Unit | 路由语法校验。 |
| PluginId | Unit | 格式校验。 |
| AOT 变量 | Unit | AOT 与动态插件冲突诊断。 |
