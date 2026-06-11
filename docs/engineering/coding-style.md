# 代码风格规范

版本：v0.1
状态：正式初版
适用范围：C# 代码风格、项目文件、Rider/ReSharper 设置、换行符和格式化规则

## 1. 目标

AtomUI.City 的代码风格需要稳定、可自动化、符合 .NET 生态习惯，并尽量与 AtomUIV6 的工程风格保持一致。

## 2. 基本规则

- C# 使用 file-scoped namespace。
- 启用 nullable reference types。
- 启用 implicit usings。
- 公共 API 命名不刻意增加 `City` 前缀。
- 命名空间统一位于 `AtomUI.City.*`。
- 用户模板生成代码不得使用 `AtomUI.City.*` 作为用户命名空间。
- 默认使用 ASCII，已有文件明确使用中文文档时除外。

## 3. 公共 API 命名

命名原则：

- 类型名表达职责，不重复框架名。
- 命名空间表达框架身份。
- 不使用 `ICityCommand`、`IAsyncCityCommand` 这类前缀。
- 命令优先复用 .NET MVVM 生态已有命名。

## 4. 文件组织

规则：

- 一个公共类型优先一个文件。
- 模块内按职责建目录。
- 复杂实现先写模块文档，再拆代码文件。
- Source Generator、Analyzer、MSBuild task 不进入运行时包目录。

## 5. 项目文件

项目文件规则：

- 使用 SDK-style project。
- 不在项目文件内写 package version。
- 包元数据从仓库级 props 继承。
- 测试项目显式标记测试依赖。

## 6. 格式化

格式化来源：

- `.editorconfig`
- `AtomUICity.sln.DotSettings`
- Rider/ReSharper 设置

提交前至少运行：

```bash
dotnet build AtomUICity.slnx
dotnet test AtomUICity.slnx
```

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| nullable | Build | 项目启用 nullable。 |
| package version | Build | PackageReference 不内联版本。 |
| 命名空间 | Analyzer/Build | 框架代码位于 `AtomUI.City.*`。 |
| 用户模板命名空间 | Template test | 模板不生成用户 `AtomUI.City.*` 命名空间。 |
