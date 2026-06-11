# PluginSystem 包安装设计

版本：v0.1
状态：正式初版
适用范围：插件包下载、缓存、校验、staging、安装目录布局和安装失败恢复

## 1. 目标

插件安装负责把一个外部插件包变成 Host 可以发现和加载的本地插件版本。

安装设计目标：

- 插件包下载和运行时加载目录分离。
- 安装过程可验证、可回滚、可诊断。
- 不覆盖运行中的插件文件。
- 不从 NuGet 全局缓存直接加载插件。
- 支持从插件源下载和从本地文件安装。

包内容和安装后目录结构见：[包布局设计](package-layout.md)。
更新、回滚和 pending 操作见：[更新和回滚设计](update-and-rollback.md)。

## 2. 包模型

第一版插件推荐发布为一个独立 NuGet 包。

规则：

- 一个插件包对应一个 `PluginId`。
- 一个插件包包含一个主业务程序集。
- 语言包、`.locpack`、图标、样式、manifest、native asset 可以作为包资源存在。
- 多个独立业务能力应拆成多个插件包，并通过插件依赖表达关系。

推荐包结构：

```text
SalesPlugin.nupkg
  lib/net10.0/SalesPlugin.dll
  atomui-city/plugin.json
  atomui-city/manifests/plugin.manifest.json
  atomui-city/manifests/routes.json
  atomui-city/manifests/permissions.json
  atomui-city/manifests/presentation.json
  atomui-city/manifests/data.json
  atomui-city/manifests/localization.json
  atomui-city/locales/zh-CN/SalesPlugin.resources.dll
  atomui-city/locales/en-US/SalesPlugin.resources.dll
  atomui-city/locales/zh-CN/SalesPlugin.locpack
  atomui-city/assets/icon.png
```

## 3. 安装目录

安装目标目录：

```text
plugins/
  installed/
    <plugin-id>/
      <version>/
        root/
        install.json
```

`root` 是插件运行时加载根目录。插件加载上下文只能从 `root` 解析插件主程序集和插件私有依赖。

规则：

- `installed/<plugin-id>/<version>` 一旦完成安装后视为不可变。
- 更新不能覆盖已有版本目录。
- 删除只能发生在插件未加载且没有 pending 操作时。
- 安装目录不能依赖 NuGet 全局包缓存存在。

## 4. 包缓存

下载包进入包缓存：

```text
plugin-cache/
  packages/
    <package-id>/
      <version>/
        <sha256>.nupkg
```

规则：

- 包缓存用于避免重复下载。
- 包缓存不是插件加载目录。
- 包缓存损坏可以删除后重新下载。
- 包 hash 必须来自受信任源或安装流程重新计算。

## 5. 安装流程

安装流程：

```text
Resolve package source
-> Download package to package cache
-> Verify package hash and source
-> Extract package to staging
-> Read plugin manifest
-> Validate schema and compatibility
-> Validate one main assembly rule
-> Validate required contribution manifests
-> Validate native/RID assets
-> Compute content hash
-> Move staging to installed/<plugin-id>/<version>
-> Write install.json
-> Update lock file
```

规则：

- 验证失败不能写入 `installed`。
- `staging` 目录中的内容不能被加载。
- 移动到 `installed` 应尽可能使用原子目录移动。
- 锁定文件更新必须在文件落盘后执行。
- 安装成功不等于启用成功。

## 6. Staging

所有安装和更新必须先进入 staging：

```text
plugins/
  staging/
    <operation-id>/
      package.nupkg
      extract/
      validation.json
```

规则：

- 每个安装操作使用独立 `operation-id`。
- Host 启动时可以清理过期 staging。
- staging 清理不能影响已安装插件。
- staging 验证结果进入诊断。

## 7. 本地文件安装

从本地文件安装时，流程与下载包一致，只是 package source 不同：

```text
Read local package
-> Copy to package cache or staging
-> Verify hash
-> Extract and validate
-> Install
```

规则：

- 本地文件安装也必须生成 `install.json`。
- 本地文件安装也必须进入锁定文件。
- 本地文件安装可以被 Host policy 禁止。
- 本地文件安装来源必须进入诊断。

## 8. 安装失败恢复

安装失败处理：

| 阶段 | 恢复策略 |
|---|---|
| 下载失败 | 删除不完整包，保留错误诊断。 |
| hash 校验失败 | 删除包缓存中的可疑包。 |
| 解压失败 | 删除 staging。 |
| 清单验证失败 | 删除 staging，记录 Invalid。 |
| 移动到 installed 失败 | 保留 staging 供诊断或清理。 |
| 锁定文件更新失败 | 回滚安装状态或标记 pending repair。 |

安装失败不能影响已启用的旧版本插件。

## 9. 卸载和清理

卸载分为禁用、卸载和清理：

| 操作 | 效果 |
|---|---|
| Disable | 不加载插件，保留安装目录、配置和状态。 |
| Uninstall | 移除 active 记录，并在插件未加载时删除安装目录。 |
| Cleanup | 清理旧版本、缓存包和过期 staging。 |

规则：

- 插件处于 `Active` 时不能删除文件。
- 插件处于 `UnloadPending` 时不能删除文件。
- 用户配置和状态默认不随 uninstall 删除，除非用户显式选择清理数据。
- 清理缓存不能影响已安装插件。

## 10. 诊断和测试

必须覆盖：

- 从远程源下载安装。
- 从本地文件安装。
- hash 校验失败。
- 清单验证失败。
- required contribution manifest 缺失。
- 安装过程中断后的 staging 清理。
- 已存在同版本目录。
- 安装成功但启用失败。
- 不从 package cache 加载程序集。

诊断信息必须包含 operation id、package source、package hash、staging path、install path、PluginId、Version 和失败阶段。
