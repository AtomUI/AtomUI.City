# AtomUI.City.PluginSystem

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.PluginSystem` 负责插件发现、插件元数据、插件加载、插件模块注册和插件生命周期。

PluginSystem 的目标是让应用在可控边界内扩展模块、路由、资源、服务和 UI 集成。

## 边界

PluginSystem 负责：

- 插件元数据。
- 插件发现。
- 插件加载。
- 插件模块注册。
- 插件生命周期。
- 插件版本约束。
- 插件安全边界。

PluginSystem 不负责：

- 业务插件内容。
- 插件市场服务端。
- 任意不受控代码执行策略。

## 后续拆分

- `discovery.md`
- `loading.md`
- `metadata.md`
- `lifecycle.md`
- `security.md`
