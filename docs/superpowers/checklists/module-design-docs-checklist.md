# AtomUI.City 模块设计文档 Checklist

状态：第一轮完成
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
- [x] 实现路线图：`docs/engineering/implementation-roadmap.md`

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
- [x] State detailed design：`docs/modules/state/detailed-design.md`，并已拆分 state values、application state、computed state、subscriptions、snapshots、collection state、threading/dispatch、plugin integration、diagnostics/testing。
- [x] EventBus detailed design：`docs/modules/eventbus/detailed-design.md`

## 3. 页面进入模型

- [x] Routing detailed design：`docs/modules/routing/detailed-design.md`，并已拆分 route definition syntax、route graph、navigation、guards、resolvers、viewmodel target、journal/reuse、plugins、diagnostics/testing。
- [x] Presentation detailed design：`docs/modules/presentation/detailed-design.md`，并已拆分 UI runtime、dispatcher、view locator、view binding、route outlet、activation integration、interaction/validation、state/localization、resources/plugins、diagnostics/testing。

## 4. 通用应用基础设施

- [x] Security detailed design：`docs/modules/security/detailed-design.md`，并已拆分 authentication、authorization、permissions、route integration、command integration、data integration、plugin integration、diagnostics/testing。
- [x] Data detailed design：`docs/modules/data/detailed-design.md`，并已拆分 request pipeline、transport、HTTP、gRPC、SignalR、client proxy、security integration、async/threading、concurrency、connection lifecycle、streaming/realtime、resilience、caching、consistency、large payload、error model、state/routing/plugin integration、diagnostics/testing。
- [x] Localization detailed design：`docs/modules/localization/detailed-design.md`，并已拆分 resource model、culture management、lazy loading、language package assemblies、lookup/fallback、AtomUI integration、UI refresh、MVVM/Routing integration、validation/errors、plugin integration、source generation、diagnostics/testing。

## 5. PluginSystem 剩余细节

- [x] PluginSystem overview：`docs/modules/plugins/overview.md`
- [x] PluginSystem Host integration：`docs/modules/plugins/host-integration.md`
- [x] PluginSystem contributions：`docs/modules/plugins/contributions.md`
- [x] PluginSystem lifecycle：`docs/modules/plugins/lifecycle.md`
- [x] PluginSystem metadata：`docs/modules/plugins/metadata.md`
- [x] PluginSystem manifest schema：`docs/modules/plugins/manifest-schema.md`
- [x] PluginSystem package layout：`docs/modules/plugins/package-layout.md`
- [x] PluginSystem MSBuild integration：`docs/modules/plugins/msbuild-integration.md`
- [x] PluginSystem discovery：`docs/modules/plugins/discovery.md`
- [x] PluginSystem compatibility：`docs/modules/plugins/compatibility.md`
- [x] PluginSystem capabilities：`docs/modules/plugins/capabilities.md`
- [x] PluginSystem contribution index：`docs/modules/plugins/contribution-index.md`
- [x] PluginSystem dependency resolution：`docs/modules/plugins/dependency-resolution.md`
- [x] PluginSystem package installation：`docs/modules/plugins/package-installation.md`
- [x] PluginSystem update and rollback：`docs/modules/plugins/update-and-rollback.md`
- [x] PluginSystem loading：`docs/modules/plugins/loading.md`
- [x] PluginSystem unloading：`docs/modules/plugins/unloading.md`
- [x] PluginSystem settings and state migration：`docs/modules/plugins/settings-and-state-migration.md`
- [x] PluginSystem AOT and static plugins：`docs/modules/plugins/aot-and-static-plugins.md`
- [x] PluginSystem security：`docs/modules/plugins/security.md`
- [x] PluginSystem signing and trust：`docs/modules/plugins/signing-and-trust.md`
- [x] PluginSystem diagnostics and testing：`docs/modules/plugins/diagnostics-and-testing.md`

## 6. 工程化模块

- [x] Testing detailed design：`docs/modules/testing/detailed-design.md`，并已拆分 feature test gate、TestHost、fake dispatcher/scheduler、lifecycle、module、contribution、routing、presentation、MVVM、state、EventBus、Data、Security、Localization、Plugin、integration、diagnostics/assertions、AOT/source generation testing。
- [x] Build detailed design：`docs/modules/build/detailed-design.md`，并已拆分 output layout、MSBuild integration、manifest generation、source generation、analyzers、plugin packaging、application packaging、incremental build、diagnostics/testing。
- [x] Cli detailed design：`docs/modules/cli/detailed-design.md`，并已拆分 commands、AI integration、project creation、generation、build commands、plugin commands、inspect、docs/tests gates、diagnostics、configuration、non-interactive/CI、diagnostics/testing。
- [x] Templates detailed design：`docs/modules/templates/detailed-design.md`，并已拆分 application、module、page、plugin、test、localization、configuration、variables、diagnostics/testing。

## 7. 每个模块文档完成标准

- [x] 职责和非职责明确。
- [x] 依赖和禁止依赖明确。
- [x] 生命周期接入点明确。
- [x] 核心概念和公共抽象明确。
- [x] 扩展点明确。
- [x] AOT/trimming/source generator 策略明确。
- [x] 错误处理策略明确。
- [x] 测试策略明确。
- [x] 测试矩阵明确，并覆盖每个功能点。
- [x] 与其他模块的集成关系明确。
