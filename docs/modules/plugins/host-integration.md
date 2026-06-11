# PluginSystem Host 集成设计

版本：v0.1
状态：正式初版
适用范围：`AtomUI.City.PluginSystem` 与 Host、ModuleSystem、Lifecycle、DI、Registry 的交互方式

## 1. 目标

PluginSystem 不能绕过 Host 直接修改应用运行时。Host 是插件运行的唯一协调者，PluginSystem 是 Host 管理运行时扩展的专门子系统。

Host 集成设计需要保证：

- 插件能力只能通过受控 contract 进入应用。
- 插件服务不污染 Host Root ServiceProvider。
- 插件生命周期进入全局 Lifecycle。
- 插件贡献可以追踪、撤销和诊断。
- 插件错误默认不导致主应用崩溃。
- 插件卸载不会留下路由、订阅、资源、命令或静态引用。

全局架构规则见：[插件系统架构规范](../../architecture/plugin-system.md)。

## 2. Host 职责

Host 负责协调插件系统和其他框架模块。

Host 必须提供：

- Root `ApplicationScope`。
- 全局 Lifecycle Pipeline。
- 全局错误处理策略。
- 全局 ModuleGraph。
- Host contract 集合。
- 可撤销 Host Registry。
- 插件能力校验入口。
- 插件启用、停用、卸载调度入口。
- 插件诊断输出入口。

Host 不应该把 Root ServiceProvider 暴露给插件做任意修改。插件如果需要服务，只能使用插件 ServiceScope 内的 ServiceProvider 或 Host 暴露的稳定 contract。

## 3. PluginSystem 职责

PluginSystem 负责插件运行时细节：

- 插件发现。
- 插件元数据读取。
- 插件兼容性校验。
- 插件依赖解析。
- 插件加载上下文。
- 插件服务 Scope。
- 插件模块图。
- 插件贡献申请。
- 插件生命周期状态。
- 插件卸载诊断。

PluginSystem 不负责解释具体业务含义。插件贡献的路由、权限、数据客户端、UI 资源和命令应交给对应模块的 registry 或 contract 处理。

## 4. 交互边界

Host 与 PluginSystem 的关系：

```text
Host
-> PluginSystem
-> Plugin lifecycle context
-> Plugin ServiceScope
-> PluginModuleGraph
-> ContributionRequest
-> Host Registry
-> ContributionLease
```

规则：

- Host 调用 PluginSystem，不由插件直接驱动 Host。
- PluginSystem 创建插件 lifecycle context、load context 和 ServiceScope。
- 插件模块属于对应 Plugin，只能在插件 ServiceScope 内注册服务。
- PluginSystem 把插件贡献转换为 ContributionRequest。
- Host 根据 contract 校验 ContributionRequest。
- 对应 Host Registry 创建 ContributionLease。
- Host 持有 Lease，并在插件停用或卸载时按反向顺序撤销。

## 5. Host Contract

Host contract 是插件能接触 Host 的唯一稳定边界。

推荐 contract 类型：

| Contract | 用途 |
|---|---|
| Plugin context | 访问插件元数据、插件生命周期状态、诊断上下文。 |
| Contribution builder | 声明模块、路由、资源、权限、命令等贡献。 |
| Lifecycle hook | 接入插件加载、启用、停用、卸载阶段。 |
| Service accessor | 访问 Host 显式允许的共享服务。 |
| Diagnostics sink | 输出插件诊断。 |
| Capability checker | 查询 Host 是否允许某项能力。 |

Host contract 应保持窄接口。不要把 Host 内部对象、全局容器、可变注册表或具体 UI runtime 直接暴露给插件。

## 6. Registry 集成

插件贡献最终进入各模块 registry：

| 贡献 | Registry 所属模块 |
|---|---|
| 模块 | Core ModuleSystem |
| 服务 | PluginSystem / Core DI adapter |
| 路由 | Routing |
| Route 到 ViewModel Target | Routing |
| View/ViewModel 绑定 | Presentation |
| 菜单、工具栏、命令入口 | Presentation / Mvvm |
| 权限点 | Security |
| 本地化资源 | Localization |
| EventBus handler | EventBus |
| Data client | Data |
| 诊断 provider | Diagnostics contract |

所有 registry 必须支持可撤销注册。不能撤销的 registry 不能接收运行时插件贡献。

## 7. DI 集成

插件 DI 必须独立于 Host Root ServiceProvider。

推荐模型：

```text
Host Root Services
-> Plugin Shared Contracts
-> Plugin Service Scope
-> Plugin Module Services
```

规则：

- 插件不能向 Host Root ServiceProvider 添加服务。
- 插件服务注册只影响插件 ServiceScope。
- 插件可以依赖 Host 显式暴露的 contract。
- Host 可以通过受控代理调用插件服务。
- 插件服务实例不能被 Host 长期静态持有。
- 插件服务 Scope 释放后，Host 不得再访问插件服务实例。

如果某个插件能力必须被 Host 或其他模块调用，应通过稳定 contract、代理或 lease 暴露，而不是直接共享插件内部实现类型。

## 8. ModuleSystem 集成

插件可以携带一个或多个插件模块。

插件模块规则：

- 插件模块依赖必须显式声明。
- 插件模块图只在对应 Plugin 内有效。
- 插件模块初始化失败默认只禁用当前插件。
- 插件模块贡献必须生成 lease。
- 插件卸载时插件模块必须先停止，再释放插件服务 Scope。

静态应用模块和插件模块可以依赖同一套 module contract，但运行时边界不同。静态模块随应用启动，插件模块随插件加载和卸载动态变化。

## 9. Lifecycle 集成

PluginSystem 必须接入 Core 生命周期系统。

至少需要以下 pipeline：

- PluginDiscover
- PluginLoad
- PluginActivate
- PluginDeactivate
- PluginUnload
- PluginError

每个 pipeline 都应该带有：

- CancellationToken。
- 插件元数据。
- 插件生命周期状态。
- 兼容性检查结果。
- ContributionLease 集合。
- 诊断上下文。
- 错误策略。

Core 生命周期详细设计见：[Core 生命周期详细设计](../core/lifecycle.md)。

## 10. 错误处理

默认策略：

| 阶段 | 默认处理 |
|---|---|
| 发现失败 | 跳过插件并记录诊断。 |
| 元数据无效 | 禁用插件并记录原因。 |
| 依赖缺失 | 禁用插件并记录缺失依赖。 |
| 加载失败 | 禁用插件，不影响主应用启动。 |
| 启用失败 | 撤销已创建贡献，插件进入 Disabled。 |
| 停用失败 | 继续撤销剩余贡献，汇总错误。 |
| 卸载失败 | 标记 `UnloadPending`，阻止更新或删除插件文件。 |

插件错误不能默认穿透为 Host Fatal。只有 Host 自身 contract、Core 生命周期或主应用必需插件失败时，才可以升级为 Fatal。

## 11. 诊断要求

Host 与 PluginSystem 交互必须记录：

- 插件 Id、版本、路径。
- 插件状态变化。
- 加载上下文创建和卸载状态。
- 插件模块图。
- 服务 Scope 创建和释放。
- ContributionLease 创建和撤销。
- 中间件执行顺序。
- 取消来源。
- 失败阶段和错误策略结果。

诊断信息需要能回答：插件为什么没有加载、为什么不能卸载、还剩哪些贡献或引用没有释放。
