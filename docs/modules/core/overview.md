# AtomUI.City.Core

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Core` 是框架运行时内核，负责 Host、DI、配置、模块、生命周期基础、线程模型、应用上下文、调度抽象和全局错误处理。

Core 是所有业务无关框架能力的基础，但不承担 UI、MVVM、HTTP client、CLI、Build 或 Testing 的具体实现。

## 边界

Core 可以包含：

- Hosting
- Lifecycle
- Modularity
- Configuration
- DependencyInjection
- Threading
- Diagnostics

Core 不依赖：

- AtomUI / Avalonia
- CommunityToolkit.Mvvm
- ReactiveUI
- System.Reactive
- Microsoft.CodeAnalysis
- Spectre.Console

## 后续拆分

- [lifecycle.md](lifecycle.md)
- [hosting.md](hosting.md)
- `modularity.md`
- `configuration.md`
- `dependency-injection.md`
- [threading.md](threading.md)
