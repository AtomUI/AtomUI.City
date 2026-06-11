# 0007 插件采用 one plugin / one main assembly / one NuGet package 模型

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 支持运行时插件装载、停用、卸载和更新。插件包如果结构过于自由，会增加依赖解析、资源撤销、版本兼容和卸载诊断复杂度。

## 决策

第一版插件推荐模型：

```text
one plugin
-> one main assembly
-> one independent NuGet package
```

插件通过 NuGet 包分发，插件清单位于 `atomui-city/plugin.json`。

## 影响

正向影响：

- 包布局容易校验。
- 插件身份、版本和主程序集边界清晰。
- 安装、更新、回滚和卸载更容易做成确定性流程。

约束：

- 插件依赖必须通过 manifest 和 package metadata 明确声明。
- 插件 active files 不能被运行时更新原地覆盖。
- 插件安装目录按 AppId 和 PluginProfile 隔离。
- Native AOT 下动态插件能力受限。

## 执行约束

- 插件架构规范见 `docs/architecture/plugin-system.md`。
- 插件包布局见 `docs/modules/plugins/package-layout.md`。
- 插件安装更新见 `docs/modules/plugins/package-installation.md` 和 `docs/modules/plugins/update-and-rollback.md`。
