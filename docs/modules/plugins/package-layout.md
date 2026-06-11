# PluginSystem 包布局设计

版本：v0.1
状态：正式初版
适用范围：插件 NuGet 包内容、安装后目录、主程序集约束和资源布局

## 1. 目标

插件包布局需要同时服务开发、发布、安装、加载、卸载和诊断。

设计目标：

- 一个插件优先对应一个 NuGet 包。
- 一个插件包只包含一个主业务程序集。
- 插件运行目录可以脱离 NuGet 全局缓存独立工作。
- 语言包、资源、native 资产和贡献清单位置稳定。
- 目录结构支持运行时更新和回滚。

## 2. NuGet 包布局

推荐包布局：

```text
<PackageId>.<Version>.nupkg
  lib/net10.0/<MainPluginAssembly>.dll
  lib/net10.0/<MainPluginAssembly>.deps.json
  atomui-city/plugin.json
  atomui-city/manifests/plugin.manifest.json
  atomui-city/manifests/modules.json
  atomui-city/manifests/routes.json
  atomui-city/manifests/permissions.json
  atomui-city/manifests/presentation.json
  atomui-city/manifests/data.json
  atomui-city/manifests/localization.json
  atomui-city/locales/zh-CN/<Plugin>.resources.dll
  atomui-city/locales/en-US/<Plugin>.resources.dll
  atomui-city/locales/zh-CN/<Plugin>.locpack
  atomui-city/assets/icon.png
  atomui-city/assets/styles/
  runtimes/<rid>/native/
```

规则：

- `lib/<tfm>` 下第一版只允许一个主业务程序集。
- 插件私有依赖可以随包携带，但必须能被依赖解析文档定义的规则识别。
- `atomui-city/plugin.json` 必须存在。
- `atomui-city/manifests` 保存构建期生成的贡献索引。
- 本地化资源和普通资产不能混放在程序集目录下。

## 3. 主程序集约束

第一版约束：

- 一个插件包只允许一个主业务程序集。
- 主程序集内可以包含多个模块。
- 多个主业务程序集应拆成多个插件包。
- 插件依赖其他插件时，通过 `PluginId` 和版本范围表达，不通过同包多程序集表达。

这样可以降低加载上下文、卸载、诊断和发布复杂度。

## 4. 安装后布局

插件安装后布局：

```text
plugins/
  installed/
    <plugin-id>/
      <version>/
        root/
          lib/net10.0/<MainPluginAssembly>.dll
          atomui-city/plugin.json
          atomui-city/manifests/
          atomui-city/locales/
          atomui-city/assets/
          runtimes/
        install.json
```

`root` 是插件运行根目录。

规则：

- `root` 内容来自验证后的包解压结果。
- Host 只从 `root` 加载插件程序集和资源。
- `install.json` 不放在 `root` 内，避免被插件当作自身资源写入。
- 安装目录完成后不可变。

## 5. 资源布局

推荐资源类型：

| 类型 | 位置 |
|---|---|
| 插件清单 | `atomui-city/plugin.json` |
| 贡献清单 | `atomui-city/manifests/*.json` |
| 本地化程序集 | `atomui-city/locales/<culture>/*.resources.dll` |
| AOT 本地化包 | `atomui-city/locales/<culture>/*.locpack` |
| 图标 | `atomui-city/assets/` |
| 样式资源 | `atomui-city/assets/styles/` |
| native 资产 | `runtimes/<rid>/native/` |

资源必须可按插件、版本、culture 和 RID 定位。

## 6. 包内容 hash

插件包必须计算：

- package hash：`.nupkg` 原始文件 hash。
- content hash：解压后参与运行的内容 hash。
- manifest hash：`plugin.json` hash。
- contribution manifest hash：每个贡献清单 hash。

规则：

- hash 写入 `install.json` 和锁定文件。
- 构建时间戳不应影响核心 content hash。
- 验证阶段发现 hash 不匹配必须拒绝安装或加载。

## 7. 非目标

第一版不支持：

- 一个 NuGet 包包含多个独立插件。
- 一个插件包含多个主业务程序集。
- 从 NuGet 全局缓存直接作为运行时根目录加载。
- 热替换正在运行的程序集文件。

## 8. 测试要求

必须覆盖：

- 标准包布局校验。
- 缺少 `plugin.json`。
- 多主程序集拒绝。
- 资源目录缺失但非必填。
- required manifest 缺失。
- hash 稳定性。
- 安装后目录与包内容一致。
