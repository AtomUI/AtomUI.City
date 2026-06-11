# AtomUI.City Source Generator 设计规范

版本：v0.1
状态：正式初版
适用范围：AtomUI.City 框架级 Source Generator、Analyzer、Manifest、AOT/trimming 设计约束

## 1. 定位

Source Generator 是 AtomUI.City AOT-first 设计的核心基础设施。

它的目标不是减少少量样板代码，而是把框架中原本可能依赖运行时反射、程序集扫描、命名约定发现、动态注册的能力，尽可能移动到编译期完成。

核心目标：

- 生成模块清单。
- 生成路由清单。
- 生成 Route 到 ViewModel Target 清单。
- 生成 Presentation 的 View/ViewModel 绑定清单。
- 生成权限清单。
- 生成本地化资源清单。
- 生成静态插件清单。
- 生成运行时 registrar。
- 提供 AOT/trimming 诊断。
- 降低启动时扫描成本。
- 避免框架默认依赖运行时反射。

## 2. 基本原则

AtomUI.City Source Generator 遵循以下原则：

- Incremental generator only：只使用 `IIncrementalGenerator`。
- Runtime reflection avoidance：默认路径不依赖运行时反射发现。
- Explicit registration preferred：显式注册优先，约定扫描只能作为 opt-in。
- Manifest-first：跨模块、跨程序集信息优先沉淀为 manifest。
- Runtime package clean：运行时包不依赖 Roslyn。
- Deterministic output：生成结果稳定，不包含时间戳、机器路径、随机值。
- Diagnostics first：不能生成时给出 analyzer 诊断，而不是延迟到运行时报错。
- AOT-safe generated code：生成代码使用 `typeof`、强类型调用和静态 registrar，不生成动态代理和表达式树编译路径。

## 3. 项目组织

第一版采用一个生成器包、多 feature 分区的方式，避免项目过早碎片化。

```text
src/
  AtomUI.City.Build/
    buildTransitive/
    targets/
    props/

  AtomUI.City.Generators/
    Common/
    Diagnostics/
    Modularity/
    Routing/
    Presentation/
    Security/
    EventBus/
    Localization/
    PluginSystem/
    Manifest/
```

职责划分：

| 项目 | 职责 |
|---|---|
| `AtomUI.City.Build` | MSBuild 集成、buildTransitive、manifest 输出、generator/analyzer 引入。 |
| `AtomUI.City.Generators` | Source Generator 和 Analyzer 实现。 |
| Runtime packages | 定义 attributes、descriptor、builder API、runtime registrar contract。 |

`Microsoft.CodeAnalysis` 只能进入 `AtomUI.City.Generators` 或 Build/Analyzer 相关项目，不能进入 `AtomUI.City.Core`、`AtomUI.City.Routing`、`AtomUI.City.Presentation` 等运行时主链路。

## 4. 包分发策略

应用开发者不应该手动引用多个 generator 包。

推荐：

```text
Application
-> PackageReference AtomUI.City.Build
-> buildTransitive 引入 AtomUI.City.Generators
```

`AtomUI.City.Build` 负责把 generator/analyzer 作为 analyzer asset 注入编译过程。

运行时包只提供 descriptor、builder API 和 registrar contract，不直接携带 Roslyn 实现。

## 5. Generator 分类

第一版规划以下 generator：

| Generator | 输出 | 主要替代 |
|---|---|---|
| Modularity generator | Module manifest、module dependency graph input | 运行时扫描模块类型。 |
| Routing generator | Route manifest、route registrar | 运行时扫描 ViewModel 或 route attribute。 |
| Presentation generator | View/ViewModel binding manifest | 命名约定反射查找 View。 |
| Security generator | Permission manifest | 启动时汇总权限声明。 |
| EventBus generator | Event contract、handler descriptor、强类型 invoker、channel metadata | 运行时扫描 handler 和反射调用。 |
| Localization generator | Localization resource manifest | 运行时资源扫描。 |
| Plugin static generator | Static plugin manifest | AOT 模式下动态插件发现。 |
| Diagnostics analyzer | AOT/trimming/API 使用诊断 | 运行时才发现不兼容问题。 |

## 6. 生成产物

生成产物分为 C# registrar 和构建期 manifest。

C# registrar：

```text
obj/.../generated/
  AtomUI.City.Generated.Modularity.g.cs
  AtomUI.City.Generated.Routing.g.cs
  AtomUI.City.Generated.Presentation.g.cs
```

构建期 manifest：

```text
obj/AtomUI.City/manifests/
  modules.json
  routes.json
  views.json
  permissions.json
  events.json
  localization.json
  plugins.json
```

运行时优先使用生成的 C# registrar。

JSON manifest 主要服务 Build、CLI、诊断、测试和插件打包。

## 7. 命名规范

生成器内部类型可以使用清晰的框架前缀，但公共 API 不滥用 `City`。

推荐 hint name：

```text
AtomUI.City/Modularity/{AssemblyName}.Modules.g.cs
AtomUI.City/Routing/{AssemblyName}.Routes.g.cs
AtomUI.City/Presentation/{AssemblyName}.Views.g.cs
AtomUI.City/EventBus/{AssemblyName}.Events.g.cs
```

生成类型建议：

```csharp
namespace AtomUI.City.Generated;

internal static partial class GeneratedModuleManifest
{
}
```

生成类型默认 `internal`。只有确实需要跨程序集调用时，才通过运行时 contract 暴露。

## 8. 输入模型

Generator 应优先读取：

- 显式 API。
- Attribute。
- Partial declaration。
- AdditionalFiles。
- MSBuild properties。

不推荐读取：

- 任意程序集反射结果。
- 运行期配置文件。
- 不稳定命名约定。
- 需要执行用户代码才能得到的结果。

扩展方法 DSL 可以用于开发体验，但如果某个能力需要进入 manifest，必须有 generator 能稳定识别的声明形态。

## 9. Analyzer 诊断

诊断 ID 统一使用：

```text
AUCGEN001
AUCGEN002
AUCGEN003
```

诊断级别：

| 级别 | 用途 |
|---|---|
| Error | 会破坏生成结果或运行时必需 manifest。 |
| Warning | 影响 AOT/trimming 或启动性能。 |
| Info | 可优化建议。 |

典型诊断：

- 使用运行时程序集扫描但未显式 opt-in。
- Route/ViewModel 无法生成稳定映射。
- Plugin 使用 Dynamic Plugin Mode 但项目开启 AOT strict。
- 生成 manifest 时发现重复 route、permission、command id。
- Public API 需要反射保留但缺少显式声明。

## 10. MSBuild 配置

提供统一属性：

```xml
<PropertyGroup>
  <AtomUICitySourceGenerationMode>Strict</AtomUICitySourceGenerationMode>
  <AtomUICityEmitManifests>true</AtomUICityEmitManifests>
  <AtomUICityAllowDynamicDiscovery>false</AtomUICityAllowDynamicDiscovery>
</PropertyGroup>
```

模式：

| 模式 | 行为 |
|---|---|
| `Strict` | AOT-first，禁止默认动态发现，问题尽量报错。 |
| `Compatible` | 允许部分 opt-in 动态能力，输出 warning。 |
| `Off` | 关闭框架生成器，仅用于特殊调试。 |

## 11. 运行时集成

运行时 Host 不应该自己扫描程序集找模块、路由或 ViewModel。

推荐启动路径：

```text
ApplicationHost
-> Load generated manifests
-> Register generated module descriptors
-> Register generated routes
-> Register generated view mappings
-> Register generated permissions
-> Register generated event contracts and handlers
-> Start lifecycle
```

如果没有生成结果，框架可以提供开发期 fallback，但必须明确标记为非默认、非 AOT-first。

## 12. PluginSystem 关系

PluginSystem 必须区分：

| 模式 | 说明 |
|---|---|
| Static Plugin Mode | 构建期确定插件，生成 plugin manifest，适合 AOT。 |
| Dynamic Plugin Mode | 运行时加载插件程序集，适合 JIT 桌面部署，不承诺完整 NativeAOT。 |

Dynamic Plugin Mode 必须 opt-in，并输出 AOT/trimming 兼容性诊断。

## 13. 设计红线

- 不在 generator 中执行用户代码。
- 不在 generator 中做网络访问。
- 不生成非确定性代码。
- 不把 Roslyn 依赖带入运行时包。
- 不把运行时反射扫描作为默认路径。
- 不用 source generator 生成复杂业务代码。
- 不让一个巨大 generator 文件承载所有 feature。
- 不在生成代码里隐藏不可诊断的 fallback 行为。

## 14. 测试要求

每个 generator 必须有：

- 输入源码测试。
- 生成源码快照测试。
- 诊断测试。
- Incremental 更新测试。
- AOT/trimming 相关 analyzer 测试。
- 多项目/多程序集 manifest 合并测试。
- 冲突检测测试。

## 15. 第一版优先级

第一版建议顺序：

1. Modularity generator。
2. Routing generator。
3. Presentation mapping generator。
4. Security permission generator。
5. Localization manifest generator。
6. Plugin static manifest generator。
7. AOT diagnostics analyzer。

Source Generator 是全局架构能力，不只是 Build 模块的实现细节。Build 负责实现和 MSBuild 集成，但 Source Generator 会影响 Core、Routing、Presentation、Security、Localization、PluginSystem 的设计方式。
