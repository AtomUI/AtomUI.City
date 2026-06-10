# AtomUI.City.PluginSystem

版本：v0.1
状态：初版草案

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

后续可继续拆分：

- `discovery.md`
- `loading.md`
- `metadata.md`
- `unloading.md`
- `security.md`
