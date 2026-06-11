# 0002 使用 Microsoft.Extensions.Hosting 作为 Host 基础

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 需要统一管理应用启动、DI、配置、日志、生命周期和关闭流程。桌面应用虽然不是 Web 应用，但仍然需要稳定的 Host 模型承载模块和运行时服务。

## 决策

AtomUI.City 使用 Microsoft.Extensions.Hosting / DependencyInjection / Configuration / Options / Logging 作为 Host 基础设施。

AtomUI.City 会在其上定义自己的桌面应用生命周期、模块初始化、Presentation 接入、插件生命周期和诊断语义。

## 影响

正向影响：

- 符合 .NET 应用框架习惯。
- DI、配置、Options 和日志基础设施成熟。
- 测试 Host 更容易构建。

约束：

- Core 可以依赖 Microsoft.Extensions 基础设施。
- AtomUI.City 不直接照搬 Web Host 生命周期。
- 桌面 UI runtime、WindowScope、RouteScope 和 Plugin lifecycle 由 AtomUI.City 自己定义。

## 执行约束

- Host 设计见 `docs/modules/core/hosting.md`。
- 生命周期设计见 `docs/modules/core/lifecycle.md`。
- 实现偏离 Host 设计时必须先更新文档并重新确认。
