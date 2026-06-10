# AtomUI.City 模块设计文档 Checklist

状态：进行中
维护规则：任何勾选、补充或调整都必须先取得用户同意。

## 0. 已有架构基线

- [x] 整体架构设计：`docs/architecture/overview.md`
- [x] 包边界：`docs/architecture/package-boundaries.md`
- [x] 编程范式：`docs/architecture/programming-model.md`
- [x] 生命周期架构：`docs/architecture/lifecycle.md`
- [x] 插件系统架构规范：`docs/architecture/plugin-system.md`
- [x] Source Generator 设计规范：`docs/architecture/source-generation.md`
- [x] 开源依赖策略：`docs/architecture/dependency-strategy.md`
- [x] 文档先行治理规范：`docs/engineering/documentation-governance.md`

## 1. Core 底座契约

- [x] Core overview：`docs/modules/core/overview.md`
- [x] Core lifecycle：`docs/modules/core/lifecycle.md`
- [x] Core hosting：`docs/modules/core/hosting.md`
- [x] Core modularity：`docs/modules/core/modularity.md`
- [x] Core dependency injection：`docs/modules/core/dependency-injection.md`
- [x] Core configuration：`docs/modules/core/configuration.md`
- [x] Core errors and diagnostics：`docs/modules/core/errors-and-diagnostics.md`
- [x] Core threading：`docs/modules/core/threading.md`

## 2. 开发者日常编程模型

- [x] Mvvm detailed design：`docs/modules/mvvm/detailed-design.md`
- [x] State detailed design：`docs/modules/state/detailed-design.md`
- [x] EventBus detailed design：`docs/modules/eventbus/detailed-design.md`

## 3. 页面进入模型

- [x] Routing detailed design：`docs/modules/routing/detailed-design.md`，并已拆分 route definition syntax、route graph、navigation、guards、resolvers、viewmodel target、journal/reuse、plugins、diagnostics/testing。
- [ ] Presentation detailed design：AtomUI/Avalonia 集成、ViewLocator、View/ViewModel 绑定、UI Dispatcher、Route Outlet、Activation 接入。

## 4. 通用应用基础设施

- [ ] Security detailed design：认证状态、权限、Policy、Route Guard、Command 权限联动。
- [ ] Data detailed design：请求管线、客户端抽象、认证注入、取消、重试、缓存、错误模型。
- [ ] Localization detailed design：模块化资源、文化切换、文本刷新、插件资源撤销。

## 5. PluginSystem 剩余细节

- [x] PluginSystem overview：`docs/modules/plugins/overview.md`
- [x] PluginSystem Host integration：`docs/modules/plugins/host-integration.md`
- [x] PluginSystem contributions：`docs/modules/plugins/contributions.md`
- [x] PluginSystem lifecycle：`docs/modules/plugins/lifecycle.md`
- [ ] PluginSystem metadata：插件清单、版本、能力声明、兼容性信息。
- [ ] PluginSystem discovery：插件目录、插件包、扫描策略、禁用策略。
- [ ] PluginSystem loading：依赖解析、加载上下文、contract 隔离。
- [ ] PluginSystem unloading：卸载重试、UnloadPending 诊断、文件更新约束。
- [ ] PluginSystem security：签名、来源、能力授权、不可信插件边界。

## 6. 工程化模块

- [ ] Testing detailed design：TestHost、FakeDispatcher、生命周期驱动器、State/Routing/EventBus/Plugin 测试工具。
- [ ] Build detailed design：输出目录、清单生成、资源生成、插件清单、MSBuild 集成。
- [ ] Cli detailed design：创建项目、模块、页面、插件、构建、打包。
- [ ] Templates detailed design：应用模板、模块模板、页面模板、插件模板、测试模板。

## 7. 每个模块文档完成标准

- [ ] 职责和非职责明确。
- [ ] 依赖和禁止依赖明确。
- [ ] 生命周期接入点明确。
- [ ] 核心概念和公共抽象明确。
- [ ] 扩展点明确。
- [ ] AOT/trimming/source generator 策略明确。
- [ ] 错误处理策略明确。
- [ ] 测试策略明确。
- [ ] 与其他模块的集成关系明确。
