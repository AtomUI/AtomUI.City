# 0003 Core 保持 UI 无关

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 面向 AtomUI/Avalonia，但 Core 是 Host、DI、配置、模块、生命周期、线程和诊断内核。Core 如果直接依赖 UI，会导致测试困难、包依赖膨胀，并破坏 Presentation 作为 UI 适配层的边界。

## 决策

`AtomUI.City.Core` 不依赖 AtomUI、Avalonia、Presentation、MVVM、ReactiveUI、System.Reactive、Roslyn、CLI 或测试框架。

UI 运行时桥接由 `AtomUI.City.Presentation` 承担。

## 影响

正向影响：

- Core 可在无 UI 环境下测试。
- 生命周期、模块、线程和诊断可以独立演进。
- AOT/trimming 风险更容易控制。

约束：

- UI Dispatcher 在 Core 中只能是抽象。
- AtomUI/Avalonia 类型不能出现在 Core 公共 API 中。
- Presentation 负责把 Avalonia Dispatcher、VisualTree 和 Resource 系统接入 Core 抽象。

## 执行约束

- 包边界见 `docs/architecture/package-boundaries.md`。
- Presentation 设计见 `docs/modules/presentation/detailed-design.md`。
- 构建测试必须检查 Core 依赖边界。
