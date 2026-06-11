# 版本管理规范

版本：v0.1
状态：正式初版
适用范围：框架版本、NuGet 包版本、模板版本、插件 API 版本和兼容性策略

## 1. 目标

版本管理需要让运行时包、Build、CLI、Templates、Plugin API 和文档状态保持一致，避免不同入口使用不兼容能力。

## 2. 版本来源

仓库版本属性集中在：

- `build/Version.props`
- `build/PackageMetaInfo.props`

项目不单独声明版本。

## 3. 包版本

第一版所有主包使用统一版本号。

主包包括：

- Runtime packages。
- Engineering packages。
- Testing package。
- Templates package。
- CLI tool package。

可选适配包可以跟随主版本，也可以在形成独立兼容边界后单独版本化。

## 4. 插件 API 版本

PluginSystem 必须区分：

- Host package version。
- Plugin package version。
- Plugin API version。
- Contract assembly version。

插件兼容性规则见：[Plugin compatibility](../modules/plugins/compatibility.md)。

## 5. 模板版本

模板版本必须与支持的框架版本范围一致。

CLI 创建项目时应能输出：

- 使用的模板版本。
- 目标框架版本。
- AtomUI.City package version。
- Build 规则版本。

## 6. 版本变更规则

破坏性变更必须：

- 更新设计文档。
- 更新 ADR。
- 更新 release notes。
- 更新模板。
- 更新 analyzer 或迁移诊断。

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 统一版本 | Build | 主包版本来自统一 props。 |
| 模板版本 | Template test | 生成项目引用正确版本。 |
| 插件 API 版本 | Unit | Plugin compatibility 可判定版本范围。 |
| 破坏性变更 | Docs/Build | 文档、ADR、release notes 均可追踪。 |
