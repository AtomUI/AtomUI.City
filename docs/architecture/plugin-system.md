# AtomUI.City 插件系统架构规范

版本：v0.1
状态：初版草案
适用范围：插件系统的架构级边界、可扩展范围、推荐规范和 Host 交互模型

## 1. 定位

PluginSystem 是 AtomUI.City 的运行时扩展机制。它允许应用在不修改主程序代码的情况下，通过插件增加模块、路由、资源、命令、权限、本地化、数据客户端和 UI 集成能力。

PluginSystem 是全局一等架构能力，不只是 ModuleSystem 的附属工具。

架构级目标：

- 允许应用在运行时发现、加载、启用、停用和卸载插件。
- 允许插件以受控方式向 Host 贡献能力。
- 保证插件贡献可以撤销。
- 保证插件服务不污染 Host Root ServiceProvider。
- 保证插件生命周期纳入全局 Lifecycle。
- 保证插件失败不会默认导致主应用崩溃。

## 2. 插件可以扩展什么

插件推荐扩展范围：

| 扩展点 | 是否推荐 | 说明 |
|---|---:|---|
| Module | 推荐 | 插件可以携带一个或多个模块，并通过模块贡献服务、配置和资源。 |
| Services | 推荐 | 插件可以注册插件私有服务或通过受控 contract 暴露服务。 |
| Routes | 推荐 | 插件可以贡献页面路由、子路由、导航元数据和 route guard。 |
| ViewModel | 推荐 | 插件可以提供自己的 ViewModel，并通过路由或 View 映射进入应用。 |
| Views / Presentation Resources | 推荐 | 插件可以贡献 View、样式、图标、菜单项、命令入口等 UI 资源。 |
| Commands / Actions | 推荐 | 插件可以贡献命令、动作、菜单动作、工具栏动作。 |
| Permissions | 推荐 | 插件可以声明权限点，并由 Host Security 统一解释和授权。 |
| Localization | 推荐 | 插件可以贡献本地化资源，必须支持撤销和文化切换刷新。 |
| EventBus handlers | 推荐 | 插件可以订阅或处理事件，订阅必须绑定插件生命周期并可撤销。 |
| Data clients | 可选推荐 | 插件可以贡献数据客户端或 API client，但必须走 Data 管线。 |
| Background tasks | 谨慎 | 插件可以启动后台任务，但必须绑定插件生命周期，并支持取消。 |
| Settings pages | 推荐 | 插件可以贡献配置页面和配置模型。 |
| Diagnostics providers | 可选 | 插件可以贡献诊断信息，但不能绕过 Host 诊断管线。 |

插件不推荐或禁止扩展：

| 扩展点 | 结论 | 原因 |
|---|---|---|
| Host Root ServiceProvider | 禁止直接修改 | 防止污染全局容器和破坏卸载。 |
| Core 生命周期状态机 | 禁止直接替换 | 生命周期是框架内核，只允许通过中间件扩展。 |
| 全局静态状态 | 禁止 | 会阻止插件卸载并制造隐式耦合。 |
| 非受控线程 | 禁止 | 必须使用受生命周期管理的后台任务。 |
| 绕过 Security 的权限逻辑 | 禁止 | 权限必须由 Host Security 统一解释。 |
| 绕过 Data 管线的共享数据访问 | 不推荐 | 会绕过认证、错误处理、resilience 和诊断。 |
| 进程内不可信代码沙箱 | 禁止承诺 | `AssemblyLoadContext` 不是安全边界。 |

## 3. 插件贡献模型

插件不能直接把能力永久注册到 Host。

插件必须通过 Contribution Lease 贡献能力：

```text
Plugin
-> Module
-> Contribution
-> Host Registry
-> ContributionLease
```

常见 Lease：

- `ModuleContributionLease`
- `ServiceContributionLease`
- `RouteContributionLease`
- `ViewContributionLease`
- `ResourceContributionLease`
- `PermissionContributionLease`
- `LocalizationContributionLease`
- `EventSubscriptionLease`
- `CommandContributionLease`
- `PresentationResourceLease`
- `DataClientContributionLease`

插件停用时，Host 按反向顺序撤销 Lease。

```text
Deactivate plugin
-> Stop new plugin entry
-> Deactivate plugin routes and view models
-> Cancel plugin operations
-> Revoke contribution leases
-> Stop plugin modules
```

插件卸载必须建立在插件已经停用的基础上：

```text
Unload plugin
-> Ensure plugin deactivated
-> Dispose plugin ServiceScope and lifecycle context
-> Unload plugin assemblies
```

## 4. PluginSystem 与 Host 的关系

Host 是插件运行的唯一协调者。

Host 负责：

- 创建 Root ApplicationScope。
- 维护全局 ModuleGraph。
- 维护生命周期管线。
- 暴露受控插件扩展点。
- 接收插件贡献。
- 持有贡献 Lease。
- 统一处理插件错误。
- 统一协调插件停用和卸载。

PluginSystem 负责：

- 插件发现。
- 插件元数据读取。
- 插件依赖解析。
- 插件加载上下文。
- 插件服务 Scope。
- 插件模块图。
- 插件生命周期。
- 插件贡献申请。
- 插件卸载诊断。

插件本身负责：

- 声明元数据。
- 声明依赖。
- 声明需要贡献的模块和扩展点。
- 使用 Host 提供的 contract。
- 遵守生命周期取消和释放规则。

插件不能绕过 Host 直接修改全局注册表。

## 5. Host 交互流程

插件加载流程：

```text
Host
-> Ask PluginSystem to discover plugin
-> Verify metadata and compatibility
-> Create plugin lifecycle context
-> Create plugin load context
-> Load plugin assemblies
-> Build plugin module graph
-> Build plugin service scope
-> Initialize plugin modules
-> Apply contributions through Host registries
-> Activate plugin
```

插件停用流程：

```text
Host
-> Mark plugin deactivating
-> Reject new plugin routes and operations
-> Deactivate plugin routes and view models
-> Cancel plugin operations
-> Revoke plugin contribution leases
-> Stop plugin modules
-> Mark plugin inactive
```

插件卸载流程：

```text
Host
-> Ensure plugin deactivated
-> Dispose plugin services
-> Dispose plugin ServiceScope and lifecycle context
-> Request AssemblyLoadContext unload
-> Verify unload
-> Mark unloaded or UnloadPending
```

## 6. 插件与 ModuleSystem 的关系

模块是应用内部组织单元。

插件是运行时外部扩展单元。

插件加载后通常通过模块向 Host 贡献能力，但插件和模块不是同一个概念。

关系：

```text
Plugin
-> PluginLoadContext
-> Plugin ServiceScope
-> PluginModuleGraph
-> Module contributions
-> Host registries
```

约束：

- 静态模块随应用启动加载。
- 插件模块随插件加载和卸载动态变化。
- 插件模块属于对应 Plugin。
- 插件模块贡献必须可撤销。
- 插件模块不能修改 Host Root ServiceProvider。

## 7. 插件与生命周期的关系

插件生命周期必须接入全局 Lifecycle。

插件不是公共 Scope 树中的 `PluginScope`。插件和插件模块是应用组成和能力贡献方：

```text
Application
  Plugins
    SalesPlugin
      Modules
        SalesModule
        SalesReportModule
```

插件贡献的能力通过 Contribution 和 ContributionLease 进入 Host：

```text
RouteContribution("/sales")
  Module = SalesModule
  Plugin = SalesPlugin
  Lease = RouteContributionLease("/sales")
```

路由、ViewModel、Operation 等运行实例仍然挂在生命周期 Scope 树中：

```text
ApplicationScope
  -> PresentationScope
    -> WindowScope
      -> NavigationScope
        -> RouteScope("/sales")
          -> ActivationScope
            -> OperationScope
```

`RouteScope` 不挂在插件下面。它通过 Contribution 记录自己来自哪个 Plugin 和 Module：

```text
RouteScope("/sales")
  Contribution = RouteContribution("/sales")
  Contribution.Plugin = SalesPlugin
  Contribution.Module = SalesModule
  Services = SalesPlugin ServiceScope
```

规则：

- 插件停用时必须阻止新的插件入口。
- 插件停用或卸载时必须找到由该插件 Contribution 创建的活动 RouteScope、ActivationScope、OperationScope 并关闭。
- 插件 Operation 必须响应取消。
- 插件 EventBus 订阅必须自动释放。
- 插件 State Reaction 必须自动释放。
- 插件资源贡献必须撤销。
- 插件卸载必须等待可释放资源处理完成。

## 8. 插件安全边界

PluginSystem 提供的是生命周期、依赖、版本、贡献和错误隔离，不提供进程内安全沙箱。

`AssemblyLoadContext` 不是安全边界。对于不可信插件，应使用进程隔离或其他宿主隔离策略。

PluginSystem 应提供：

- 插件签名或来源校验扩展点。
- 插件权限声明。
- 插件能力声明。
- 插件兼容性校验。
- 插件禁用和隔离诊断。

安全策略由 Host 统一执行，插件不能自行绕过。

## 9. 推荐规范

插件开发推荐遵循：

- 一个插件有明确插件元数据。
- 一个插件可以包含多个模块，但模块依赖必须显式。
- 插件所有贡献必须通过 Host contract。
- 插件所有长期任务必须绑定插件生命周期。
- 插件所有订阅必须绑定插件生命周期或相关运行 Scope。
- 插件所有 UI 入口必须可撤销。
- 插件所有路由必须可禁用和可撤销。
- 插件所有服务必须位于插件服务 Scope。
- 插件不得在静态字段保存 Host、Scope、ServiceProvider、ViewModel 或插件类型实例。
- 插件卸载失败必须可以诊断。

## 10. 后续模块文档

本文件只定义架构级规范。

PluginSystem 模块级设计已拆分到：

- [Host 集成设计](../modules/plugins/host-integration.md)
- [贡献模型设计](../modules/plugins/contributions.md)
- [生命周期设计](../modules/plugins/lifecycle.md)

后续可继续补充发现、加载、元数据、卸载和安全策略细节文档。
