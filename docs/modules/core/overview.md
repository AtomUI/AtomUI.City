# AtomUI.City.Core

版本：v0.1
状态：正式初版

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

## 详细设计

| 文档 | 内容 |
|---|---|
| [lifecycle.md](lifecycle.md) | 应用、模块、插件、路由、ViewModel、状态、事件、命令和资源释放生命周期。 |
| [hosting.md](hosting.md) | Generic Host、应用构建、扩展方法、Host 生命周期和 PluginSystem 接入。 |
| [modularity.md](modularity.md) | 模块定义、依赖声明、模块图、配置阶段、生命周期和插件模块。 |
| [configuration.md](configuration.md) | 配置源、Options、PreConfigure、验证、热更新、插件配置隔离和 AOT 约束。 |
| [dependency-injection.md](dependency-injection.md) | 服务注册、服务作用域、模块服务注册、插件服务隔离和 AOT 友好自动注册。 |
| [threading.md](threading.md) | UI 线程、后台调度、OperationScope、取消、late result 和线程诊断。 |
| [errors-and-diagnostics.md](errors-and-diagnostics.md) | 错误模型、诊断事件、错误聚合、生命周期诊断和测试断言。 |
