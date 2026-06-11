# AtomUI.City.PluginSystem

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.PluginSystem` 负责插件发现、插件元数据、插件加载、插件模块注册和插件生命周期。

PluginSystem 的目标是让应用在可控边界内扩展模块、路由、资源、服务和 UI 集成。

架构级规范见：[插件系统架构规范](../../architecture/plugin-system.md)。

## 边界

PluginSystem 负责：

- 插件元数据。
- 插件发现。
- 插件加载。
- 插件模块注册。
- 插件生命周期。
- 插件版本约束。
- 插件安全边界。
- 插件贡献申请。
- 插件 Contribution Lease。
- 插件停用和卸载诊断。

PluginSystem 支持插件贡献：

- 模块。
- 服务。
- 路由。
- View 和 ViewModel。
- Presentation 资源。
- 命令或动作。
- 权限。
- 本地化资源。
- EventBus handler。
- Data client。
- 设置页面。
- 诊断 provider。

PluginSystem 不负责：

- 业务插件内容。
- 插件市场服务端。
- 任意不受控代码执行策略。
- Host Root ServiceProvider 直接修改。
- 不可信插件的进程内安全沙箱。

## 详细设计

| 文档 | 内容 |
|---|---|
| [host-integration.md](host-integration.md) | PluginSystem 与 Host、ModuleSystem、Lifecycle、DI 和 Registry 的交互方式。 |
| [contributions.md](contributions.md) | 插件可以贡献的能力、贡献申请、Contribution Lease 和撤销规则。 |
| [lifecycle.md](lifecycle.md) | 插件发现、加载、启用、停用、卸载和 UnloadPending 状态设计。 |
| [metadata.md](metadata.md) | 插件身份、清单、版本、兼容性、能力声明、安装记录和锁定信息。 |
| [manifest-schema.md](manifest-schema.md) | `atomui-city/plugin.json` 的字段、版本、校验规则和生成规则。 |
| [package-layout.md](package-layout.md) | 插件 NuGet 包内容、安装后目录、主程序集约束和资源布局。 |
| [msbuild-integration.md](msbuild-integration.md) | 插件项目属性、Item、Target、清单生成、包验证和本地开发安装。 |
| [discovery.md](discovery.md) | 插件目录、插件包扫描、来源优先级、禁用策略和发现诊断。 |
| [compatibility.md](compatibility.md) | Host 版本、插件 API 版本、contract 版本、目标框架、RID、AOT 和功能兼容。 |
| [capabilities.md](capabilities.md) | 插件能力声明、授权、能力范围、Contribution 校验和诊断。 |
| [contribution-index.md](contribution-index.md) | 插件贡献清单索引、贡献清单文件、必填策略和构建期生成。 |
| [dependency-resolution.md](dependency-resolution.md) | 插件依赖、程序集依赖、共享 contract、私有依赖、native/RID 资产和加载上下文解析。 |
| [loading.md](loading.md) | 插件验证后加载、加载上下文创建、模块图、服务 Scope 和启用前准备。 |
| [unloading.md](unloading.md) | 插件停用后卸载、引用释放、卸载重试、UnloadPending 和文件删除约束。 |
| [package-installation.md](package-installation.md) | 插件包下载、缓存、校验、staging、安装目录布局和安装失败恢复。 |
| [update-and-rollback.md](update-and-rollback.md) | 插件版本切换、运行时更新、Pending 操作、回滚和文件更新约束。 |
| [settings-and-state-migration.md](settings-and-state-migration.md) | 插件配置、用户状态、版本升级、回滚、迁移声明和降级策略。 |
| [aot-and-static-plugins.md](aot-and-static-plugins.md) | Native AOT、trimming、source generator、静态插件、资源包和运行时动态插件限制。 |
| [security.md](security.md) | 插件来源、签名、hash、能力授权、加载边界和不可信代码约束。 |
| [signing-and-trust.md](signing-and-trust.md) | 插件包来源、签名、hash、发布者、信任策略和审计记录。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | 插件诊断事件、错误码、测试工具、状态机测试和卸载验证。 |
