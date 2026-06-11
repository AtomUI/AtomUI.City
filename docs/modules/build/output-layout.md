# Build 输出目录设计

版本：v0.1
状态：正式初版
适用范围：`output/` 根目录、构建产物、生成产物、包、发布输出、日志和清理策略

## 1. 目标

Build 必须把框架构建输出集中到稳定目录中，避免构建产物散落在项目根目录。

设计目标：

- 本地开发、CLI 和 CI 使用同一套输出布局。
- 构建产物可清理、可诊断、可测试。
- 插件包、应用发布和 manifest 快照有固定位置。
- 路径可配置，但默认一致。

## 2. 默认根目录

默认输出根目录：

```text
output/
```

可以通过 MSBuild 属性覆盖：

```xml
<AtomUICityOutputRoot>output</AtomUICityOutputRoot>
```

规则：

- 相对路径基于 repo root 或 solution root 解析。
- CLI 调用 Build 时必须尊重同一属性。
- CI 可以覆盖到工作区 artifact 目录。
- 自定义路径必须进入 diagnostics。

## 3. 推荐布局

```text
output/
  artifacts/
    bin/
    obj/
    generated/
    manifests/
    diagnostics/
  packages/
    nuget/
    plugins/
    templates/
  publish/
    apps/
    plugins/
    resources/
  logs/
```

## 4. artifacts

`artifacts` 保存构建中间产物和可诊断快照。

| 路径 | 内容 |
|---|---|
| `artifacts/bin` | 构建输出程序集副本或统一收敛视图。 |
| `artifacts/obj` | 框架 task 中间产物。 |
| `artifacts/generated` | source generator 和 MSBuild 生成文件。 |
| `artifacts/manifests` | 最终 manifest 快照。 |
| `artifacts/diagnostics` | 结构化构建诊断。 |

项目自己的 `bin/obj` 可以保留 .NET 默认行为，但 AtomUI.City 需要复制或生成可诊断快照到 `output/artifacts`。

## 5. packages

`packages` 保存可发布包：

| 路径 | 内容 |
|---|---|
| `packages/nuget` | 框架或普通 NuGet 包。 |
| `packages/plugins` | 插件 NuGet 包。 |
| `packages/templates` | 模板包。 |

插件包必须经过 package layout validation 后进入 `packages/plugins`。

## 6. publish

`publish` 保存发布输出：

| 路径 | 内容 |
|---|---|
| `publish/apps` | 应用发布目录。 |
| `publish/plugins` | bundled/static plugin 输出。 |
| `publish/resources` | 资源包、语言包、`.locpack`。 |

发布目录不等同于运行时用户插件安装目录。用户插件安装目录由 PluginSystem 运行时管理。

## 7. logs

`logs` 保存人类可读构建日志。

结构化诊断优先写入 `artifacts/diagnostics`，`logs` 用于开发者排查。

## 8. 清理策略

建议 target：

- `CleanAtomUICityOutput`
- `CleanAtomUICityGenerated`
- `CleanAtomUICityPackages`
- `CleanAtomUICityPublish`

规则：

- 默认 clean 只清理当前项目相关产物。
- 全量清理必须显式执行。
- 清理不能删除用户插件安装目录。
- 清理不能删除 PluginSystem 运行时 cache。

## 9. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 默认输出根目录 | Unit | 未配置时解析到 `output/`。 |
| 自定义输出根目录 | Unit | 属性覆盖生效。 |
| artifacts 分类 | Unit/Build | generated、manifests、diagnostics 路径正确。 |
| packages 分类 | Build | plugin package 输出到 `packages/plugins`。 |
| publish 分类 | Build | app/resource 输出到 publish 子目录。 |
| clean | Build | 只清理目标目录，不删除运行时插件目录。 |
