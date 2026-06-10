# .NET 依赖项加载参考

版本：v0.1
状态：参考资料
来源范围：Microsoft Learn .NET fundamentals / dependency loading
最后更新：2026-06-10

## 1. 文档定位

本文记录从 Microsoft Learn 学习到的 .NET 依赖项加载、`AssemblyLoadContext`、插件加载和程序集卸载机制。

本文是外部官方资料摘要，不是 AtomUI.City 的正式架构设计。后续生命周期、插件系统和模块系统设计可以引用本文作为底层运行时背景。

## 2. 官方资料

| 主题 | 链接 |
|---|---|
| .NET 依赖项加载概述 | <https://learn.microsoft.com/zh-cn/dotnet/core/dependency-loading/overview> |
| 了解 AssemblyLoadContext | <https://learn.microsoft.com/zh-cn/dotnet/core/dependency-loading/understanding-assemblyloadcontext> |
| 依赖项加载详细信息 | <https://learn.microsoft.com/zh-cn/dotnet/core/dependency-loading/loading-resources> |
| 托管程序集加载算法 | <https://learn.microsoft.com/zh-cn/dotnet/core/dependency-loading/loading-managed> |
| 非托管库加载算法 | <https://learn.microsoft.com/zh-cn/dotnet/core/dependency-loading/loading-unmanaged> |
| 使用插件创建 .NET 应用程序 | <https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/creating-app-with-plugin-support> |
| 程序集可卸载性 | <https://learn.microsoft.com/zh-cn/dotnet/standard/assembly/unloadability> |
| AssemblyLoadContext API | <https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.loader.assemblyloadcontext> |
| AssemblyDependencyResolver API | <https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.loader.assemblydependencyresolver> |

## 3. AssemblyLoadContext

`AssemblyLoadContext` 是 .NET 中负责定位、加载和缓存托管程序集及其他依赖项的运行时抽象。

核心能力：

- 管理托管程序集加载。
- 管理依赖项解析。
- 支持多个加载上下文。
- 支持动态代码加载。
- 通过 collectible context 支持程序集卸载。
- 允许不同上下文加载不同版本的同名依赖。

默认应用程序集和静态依赖通常位于 `AssemblyLoadContext.Default` 中。插件、脚本、扩展模块等动态能力通常需要自定义 `AssemblyLoadContext`。

## 4. 加载上下文隔离

多个 `AssemblyLoadContext` 可以隔离不同依赖集合。

这对插件系统很重要，因为：

- 插件可以携带自己的依赖版本。
- 不同插件可以使用不同版本的第三方库。
- 插件依赖不必全部进入主应用默认上下文。
- 插件卸载时可以尝试卸载对应上下文中的程序集。

但隔离不是安全沙箱。进程内加载的插件仍然运行在同一个进程中，不能把 `AssemblyLoadContext` 当作不可信代码安全边界。

## 5. AssemblyDependencyResolver

`AssemblyDependencyResolver` 用于根据组件路径解析程序集和 native library。

官方插件教程使用它来实现自定义插件加载上下文：

- 根据插件主程序集路径创建 resolver。
- 在 `AssemblyLoadContext.Load` 中解析托管程序集路径。
- 在 `LoadUnmanagedDll` 中解析非托管库路径。
- 利用插件输出目录和 `.deps.json` 完成依赖解析。

这比手写路径拼接更可靠，也更接近 .NET SDK 的发布输出模型。

## 6. 托管程序集加载

托管程序集加载通常涉及 active `AssemblyLoadContext`。

加载过程会考虑：

- 当前 active context 的缓存。
- 自定义 `AssemblyLoadContext.Load`。
- 默认上下文缓存。
- 默认探测逻辑。
- `AssemblyLoadContext.Resolving` 事件。
- `AppDomain.AssemblyResolve` 事件。

结论是：插件系统不能只依赖 `Assembly.LoadFrom` 这类简单 API。要想让插件依赖解析可控，需要显式管理加载上下文和 resolver。

## 7. 非托管库加载

插件可能包含 native library。

非托管库加载需要考虑：

- 运行时标识符。
- 平台差异。
- native asset 路径。
- `LoadUnmanagedDll` 覆盖。
- `AssemblyDependencyResolver.ResolveUnmanagedDllToPath`。

如果插件包含 native library，插件加载器必须把 native probing 纳入设计，否则会出现托管程序集加载成功但运行时调用 native 依赖失败的问题。

## 8. 附属程序集和资源加载

插件可能包含本地化资源和 satellite assemblies。

资源加载需要考虑：

- culture fallback。
- satellite assembly 位置。
- 插件上下文中的资源探测。
- 插件卸载时资源引用释放。

如果应用支持运行时插件和本地化，插件资源不能只注册到全局字典后不撤销。资源贡献需要有明确生命周期。

## 9. 插件应用模型

官方插件教程的基本模式是：

```text
Host application
-> Plugin contract assembly
-> Plugin project
-> Custom AssemblyLoadContext
-> AssemblyDependencyResolver
-> Load plugin assembly
-> Discover plugin types
-> Execute plugin contract
```

关键约束：

- 插件 contract 必须由主应用共享。
- 插件 contract 不应被插件输出复制成另一个独立副本。
- 主应用和插件必须看到同一个 contract assembly identity。
- 每个插件可以使用独立加载上下文。
- 不可信代码不能安全加载到可信 .NET 进程中。

## 10. 程序集可卸载性

.NET 的程序集卸载基于 collectible `AssemblyLoadContext`。

卸载不是强制式，而是协作式：

- 只有 collectible context 可以卸载。
- 调用 `Unload()` 只是启动卸载。
- 实际卸载发生在没有强引用阻止回收之后。
- 需要触发 GC 才能观察卸载是否完成。

常见阻止卸载的因素：

- 外部仍持有插件类型实例。
- 外部仍持有插件 `Type`、`Assembly`、`MethodInfo` 等反射对象。
- 静态字段保存了插件对象。
- 事件订阅没有解除。
- 后台线程仍在执行插件代码。
- `GCHandle` 持有强引用。
- 委托或 lambda 捕获了插件对象。

因此，插件系统如果要支持运行时卸载，必须把对象引用、事件订阅、后台任务、UI 资源、状态、命令、服务 scope 都纳入统一生命周期释放。

## 11. 诊断

.NET 提供程序集加载诊断能力，可以用于排查：

- 程序集从哪里加载。
- 加载失败原因。
- native library 解析失败。
- 资源程序集解析失败。
- AssemblyLoadContext 未能卸载的原因。

插件系统应记录自己的加载诊断信息，包括：

- 插件路径。
- 插件主程序集。
- 加载上下文名称。
- 解析到的托管依赖。
- 解析到的 native 依赖。
- 资源程序集。
- 加载失败异常。
- 卸载尝试和卸载结果。

## 12. 对框架设计的注意事项

这部分不是官方结论，而是基于官方机制得到的设计注意事项：

- 运行时插件需要独立 `AssemblyLoadContext`。
- 可卸载插件需要 collectible context。
- 插件生命周期必须先停止业务活动，再解除引用，最后触发 ALC 卸载。
- 插件贡献的服务、路由、资源、本地化、权限、事件订阅必须可撤销。
- 插件 contract 必须稳定，并由主应用默认上下文加载。
- 不可信插件需要进程隔离，不能依赖进程内 ALC。
- 插件加载器需要支持 managed、native、resource 三类依赖诊断。
- 生命周期系统需要能追踪插件创建的 Scope、Operation、Subscription 和 UI 资源。

## 13. 术语

| 术语 | 含义 |
|---|---|
| AssemblyLoadContext | .NET 中定位、加载、缓存和隔离程序集的上下文。 |
| Default context | 默认加载上下文，通常包含主应用程序集和静态依赖。 |
| Collectible context | 可被卸载的加载上下文。 |
| AssemblyDependencyResolver | 根据组件路径和发布输出解析程序集及 native library 的工具。 |
| Managed assembly | 托管程序集，例如 `.dll`。 |
| Native library | 平台相关的非托管库。 |
| Satellite assembly | 用于本地化等资源场景的附属程序集。 |
| Cooperative unload | 协作式卸载，要求引用释放后才能完成。 |
