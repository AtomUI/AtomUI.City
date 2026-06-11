# 打包规范

版本：v0.1
状态：正式初版
适用范围：NuGet 包、模板包、CLI tool、插件包和应用发布包

## 1. 目标

打包规范确保 AtomUI.City 的 runtime、Build、CLI、Templates、Testing 和 Plugin 包边界清晰，输出可验证、可诊断、可发布。

## 2. 包类型

| 包类型 | 示例 | 说明 |
|---|---|---|
| Runtime package | `AtomUI.City.Core` | 运行时框架能力。 |
| Engineering package | `AtomUI.City.Build` | buildTransitive、targets、generator/analyzer 接入。 |
| CLI tool | `AtomUI.City.Cli` | `atomui city ...` 命令入口。 |
| Template package | `AtomUI.City.Templates` | 应用、模块、页面、插件、测试模板。 |
| Testing package | `AtomUI.City.Testing` | TestHost、fake runtime、断言工具。 |
| Plugin package | 第三方插件 NuGet | 一个插件一个主程序集，一个独立 NuGet 包。 |

## 3. NuGet 包规则

- 包元数据从 `build/PackageMetaInfo.props` 继承。
- License 必须为 LGPL v3。
- Runtime 包不包含测试资产。
- Build 包可以包含 buildTransitive assets。
- Generator/Analyzer 作为 build/analyzer 资产进入应用项目，不进入 runtime 主链路。

## 4. 插件包规则

插件包规则见：

- [Plugin package layout](../modules/plugins/package-layout.md)
- [Plugin MSBuild integration](../modules/plugins/msbuild-integration.md)
- [Build plugin packaging](../modules/build/plugin-packaging.md)

第一版推荐：

- 一个 plugin 一个主 assembly。
- 一个 plugin 发布为一个独立 NuGet 包。
- 插件清单位于 `atomui-city/plugin.json`。
- 插件安装后不原地覆盖 active files。

## 5. 应用发布

应用发布规则见：[application-packaging.md](../modules/build/application-packaging.md)。

必须支持：

- CoreCLR 桌面部署。
- 动态插件部署。
- 静态插件部署。
- Native AOT 部署约束。

## 6. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| runtime package | Pack test | 包含 runtime assets，不包含测试资产。 |
| Build package | Pack test | buildTransitive assets 正确。 |
| Template package | Template smoke | 模板可安装并生成项目。 |
| CLI package | Tool smoke | `atomui city` 可执行。 |
| Plugin package | Package layout test | one main assembly 和 plugin.json 有效。 |
