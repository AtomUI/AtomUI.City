# 构建系统规范

版本：v0.1
状态：正式初版
适用范围：MSBuild props/targets、输出目录、构建约定和构建验证

## 1. 目标

构建系统负责把 AtomUI.City 的项目结构、依赖版本、输出布局、包元数据和测试门禁固化为可重复执行的规则。

模块级 Build 设计见：[AtomUI.City.Build 详细设计](../modules/build/detailed-design.md)。

## 2. 仓库级文件

| 文件 | 职责 |
|---|---|
| `Directory.Build.props` | 仓库通用编译属性和 build props 导入。 |
| `Directory.Build.targets` | 仓库通用 targets。 |
| `Directory.Packages.props` | NuGet 中央版本管理。 |
| `build/Version.props` | 版本号和版本相关属性。 |
| `build/Common.props` | 通用项目属性。 |
| `build/PackageMetaInfo.props` | 包元数据。 |
| `build/Output.props` | 输出根目录和输出约定。 |
| `build/Output.App.props` | 应用输出相关约定。 |

## 3. 基本规则

- 所有项目必须通过 `AtomUICity.slnx` 可 restore/build。
- 所有 package version 必须放在 `Directory.Packages.props` 或版本 props 中统一管理。
- 运行时项目不能依赖 Roslyn、CLI terminal UI 或测试框架。
- Build/Gene­rator/Analyzer 能力不能进入运行时主链路。
- 构建输出必须遵守 `output/` 目录约定。

## 4. 构建命令

常用验证命令：

```bash
dotnet restore AtomUICity.slnx
dotnet build AtomUICity.slnx
dotnet test AtomUICity.slnx
```

CLI 封装命令见：[CLI build commands](../modules/cli/build-commands.md)。

## 5. 输出规则

`output/` 是框架级输出根目录。

主要分区：

- `output/artifacts/`
- `output/packages/`
- `output/publish/`
- `output/logs/`
- `output/temp/`

详细布局见：[output-layout.md](../modules/build/output-layout.md)。

## 6. Source Generator 和 Analyzer

Source Generator 由 `AtomUI.City.Build` 通过 buildTransitive 接入应用项目。

规则：

- runtime package 不引用 `Microsoft.CodeAnalysis`。
- generator 输出必须确定性。
- manifest 输出必须可测试。
- AOT/trimming 不兼容路径必须产生 analyzer 或 build diagnostic。

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| restore | Build | solution restore 成功。 |
| build | Build | solution build 成功。 |
| test discovery | Test | 所有测试项目可被发现。 |
| 中央包版本 | Build | 项目不内联 package version。 |
| runtime 依赖边界 | Build | Core 等运行时包不依赖 Roslyn/CLI/Test。 |
| output layout | Build | 构建产物写入规定输出目录。 |
