# Build 插件打包设计

版本：v0.1
状态：正式初版
适用范围：插件 NuGet 包、plugin manifest、贡献清单、资源、hash、签名输入和包布局校验

## 1. 目标

Build 负责把插件项目打包为符合 PluginSystem 规范的独立 NuGet 包。

插件系统包布局见：[PluginSystem 包布局设计](../plugins/package-layout.md)。

## 2. 打包流程

```text
Build plugin project
-> Run source generators
-> Generate contribution manifests
-> Generate plugin.json
-> Validate one main assembly
-> Collect resources
-> Collect native/RID assets
-> Compute manifest hashes
-> Pack nupkg
-> Validate package layout
-> Copy to output/packages/plugins
-> Write diagnostics
```

## 3. 第一版规则

- 一个插件包一个 `PluginId`。
- 一个插件包一个主业务程序集。
- 插件包必须包含 `atomui-city/plugin.json`。
- 插件包必须包含 required contribution manifests。
- 语言包、`.locpack`、图标、样式、native asset 可以作为资源。
- 插件包不能依赖运行时目录结构补齐缺失 manifest。
- 包内容 hash 必须稳定。

## 4. 包输出

插件包输出：

```text
output/packages/plugins/
  <PackageId>.<Version>.nupkg
```

诊断输出：

```text
output/artifacts/diagnostics/plugins/
```

Manifest 快照：

```text
output/artifacts/manifests/plugins/<PluginId>/<Version>/
```

## 5. 校验

必须校验：

- `PluginId` 存在。
- `PackageId` 存在。
- 主程序集唯一。
- `plugin.json` schema。
- required manifest 存在。
- capability 声明格式。
- dependency version range。
- language package culture。
- native/RID asset 声明。
- content hash。

## 6. 本地开发安装

Build 可以提供 target：

```text
InstallAtomUICityPluginToLocalCache
```

规则：

- 只安装到 development profile。
- 不覆盖 stable profile。
- 不使用真实用户目录，除非用户显式配置。
- 安装结果必须更新 development lock file。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| plugin.json 生成 | Build test | 字段完整、schema 正确。 |
| 单主程序集 | Build test | 多主程序集失败。 |
| required manifest | Build test | 缺失失败。 |
| resource collection | Build test | language、asset、native 进入包。 |
| hash | Unit/Build | 内容变化 hash 变化。 |
| package output | Build | nupkg 进入 `output/packages/plugins`。 |
| dev install | Build | 只写入 development profile。 |
