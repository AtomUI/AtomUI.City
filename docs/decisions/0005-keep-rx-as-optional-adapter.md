# 0005 Rx 和 ReactiveUI 只作为可选适配

状态：Accepted
日期：2026-06-11

## 背景

Rx 和 ReactiveUI 在复杂事件流、调度和响应式组合方面能力成熟。但 AtomUI.City 的核心公共 API 需要保持 C#/.NET 桌面开发者容易理解，并且避免把 Rx 类型扩散到 State、Routing、Command 和 EventBus 主路径。

## 决策

AtomUI.City 不把 `IObservable<T>`、ReactiveUI ViewModel、ReactiveCommand 或 RoutingState 作为核心公共 API。

Rx、ReactiveUI、DynamicData 等能力通过可选适配包提供。

## 影响

正向影响：

- 核心 API 更符合普通 .NET 桌面应用习惯。
- Core、State、Routing、EventBus 不被 Rx 依赖绑定。
- 已有 ReactiveUI 应用仍可通过适配包迁移。

约束：

- State Core 不依赖 System.Reactive。
- EventBus Core 不依赖 Rx。
- MVVM 默认命令使用 CommunityToolkit.Mvvm。
- 可选适配包不得改变核心生命周期语义。

## 执行约束

- 依赖策略见 `docs/architecture/dependency-strategy.md`。
- State 设计见 `docs/modules/state/detailed-design.md`。
- EventBus 设计见 `docs/modules/eventbus/detailed-design.md`。
