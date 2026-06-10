# AtomUI.City.State

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.State` 负责只读状态、可写状态、计算状态、应用级共享状态、状态订阅、StateScope、Snapshot 和集合状态。

State 的目标是让框架识别状态、状态派生、应用级共享状态、状态副作用和状态生命周期，而不是把状态当作普通属性散落在 ViewModel 中，也不是做成静态全局变量。

## 边界

State Core 不要求 System.Reactive，也不依赖 ReactiveUI。

核心概念：

- `IReadOnlyState<T>`
- `IWritableState<T>`
- `IComputedState<T>`
- `IApplicationState`
- `IApplicationStateWriter`
- `StateKey<T>`
- `IStateSubscription`
- `IStateScope`
- `StateSnapshot`
- `IStateCollection<TKey, TItem>`

## 后续拆分

- `detailed-design.md`
- `state-values.md`
- `computed-state.md`
- `reactions.md`
- `snapshots.md`
- `collection-state.md`
