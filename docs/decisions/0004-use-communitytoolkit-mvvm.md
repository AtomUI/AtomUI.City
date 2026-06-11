# 0004 MVVM 基础使用 CommunityToolkit.Mvvm

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 需要提供 ViewModel、属性通知、命令和验证基础能力。重复实现这些成熟能力会增加维护成本，并让开发者远离 .NET MVVM 生态习惯。

## 决策

`AtomUI.City.Mvvm` 使用 CommunityToolkit.Mvvm 作为默认 MVVM 基础设施。

框架复用：

- `ObservableObject`
- `ObservableValidator`
- `IRelayCommand`
- `IAsyncRelayCommand`
- `RelayCommand`
- `AsyncRelayCommand`
- CommunityToolkit.Mvvm source generators

## 影响

正向影响：

- 降低样板代码。
- 命令和属性通知符合 .NET MVVM 生态习惯。
- 避免引入 `CityCommand`、`CityViewModel` 这类重复命名。

约束：

- CommunityToolkit.Mvvm 不进入 Core。
- EventBus 不使用 WeakReferenceMessenger 作为默认底层。
- ReactiveUI 可作为可选适配，不作为默认 ViewModel 基类。

## 执行约束

- MVVM 设计见 `docs/modules/mvvm/detailed-design.md`。
- 命令设计见 `docs/modules/mvvm/commands.md`。
- 新增 MVVM 公共 API 前必须先更新文档和测试矩阵。
