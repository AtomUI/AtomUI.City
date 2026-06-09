# AtomUI.City.State

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.State` 负责状态值、可写状态、计算状态、Reaction、StateScope、Snapshot 和集合状态。

State 的目标是让框架识别状态、状态派生、状态副作用和状态生命周期，而不是把状态当作普通属性散落在 ViewModel 中。

## 边界

State Core 不要求 System.Reactive，也不依赖 ReactiveUI。

核心概念：

- `IStateValue<T>`
- `IWritableState<T>`
- `IComputedState<T>`
- `IStateReaction`
- `IStateScope`
- `StateSnapshot`
- `ICollectionState<TKey, TItem>`

## 后续拆分

- `state-values.md`
- `computed-state.md`
- `reactions.md`
- `snapshots.md`
- `collection-state.md`
