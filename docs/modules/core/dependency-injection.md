# AtomUI.City.Core Dependency Injection 设计

版本：v0.1
状态：初版草案
适用范围：`AtomUI.City.Core` 中服务注册、服务作用域、模块服务注册、插件服务隔离、自动服务注册、AOT/source generator 约束。

## 1. 定位

Dependency Injection 是 AtomUI.City Host 的基础设施之一。AtomUI.City 默认复用 `Microsoft.Extensions.DependencyInjection` 和 GenericHost，不重造 DI 容器。

DI 模块负责定义：

- 模块如何注册服务。
- Host Root ServiceProvider 的边界。
- Application、Plugin、Route、Activation、Operation 等生命周期如何拥有服务作用域。
- 插件服务如何隔离。
- Contribution 如何绑定服务来源。
- 自动服务注册如何做到 AOT 友好。

## 2. 非目标

DI 不负责业务分层，不提供领域服务规范，不替换 `Microsoft.Extensions.DependencyInjection`，不承诺支持任意第三方容器的全部特性。

Core DI 也不负责 ViewModel 创建、路由解析、插件程序集加载和 Data client 代理生成，这些由对应模块接入服务解析能力。

## 3. 服务容器层级

AtomUI.City 第一版建议明确三类服务上下文：

```text
Host Root ServiceProvider
  -> Application ServiceScope
  -> Lifecycle-owned ServiceScopes

Plugin ServiceProvider
  -> Plugin-owned ServiceScopes
```

Host Root ServiceProvider 由 GenericHost 构建，承载框架核心服务、启动期模块服务和应用固定服务。

Application ServiceScope 随 ApplicationScope 创建和释放，用于应用生命周期内的 scoped 服务。

Lifecycle-owned ServiceScope 由 RouteScope、ActivationScope、OperationScope 等运行时 Scope 按需创建和释放。

Plugin ServiceProvider 是插件独立服务容器。插件不能修改 Host Root ServiceProvider。

## 4. 核心规则

- 启动期模块可以在 ServiceProvider 构建前注册 Root `IServiceCollection`。
- 插件模块只能注册到插件自己的 `IServiceCollection`。
- 插件服务不能自动 fallback 到 Host Root ServiceProvider。
- 插件需要访问 Host 能力时，只能通过 Host 显式暴露的 contract。
- 不允许在模块服务配置阶段调用 `BuildServiceProvider()`。
- 不允许从 Root Provider 解析 scoped 服务。
- 不允许服务实例把插件内部类型泄漏到 Host 长期持有对象中。
- `ValidateScopes` 在开发和测试环境默认开启。

## 5. 模块服务注册流程

启动期模块流程建议：

```text
PreConfigureServices(all modules)
-> Generated service registration(all modules)
-> ConfigureServices(all modules)
-> PostConfigureServices(all modules)
-> Build GenericHost
```

这样自动注册先建立默认服务，模块的 `ConfigureServices` 可以显式覆盖或调整，`PostConfigureServices` 做最终校正。

插件模块流程：

```text
Create plugin IServiceCollection
-> Register host contracts
-> Generated plugin service registration
-> Plugin module ConfigureServices
-> Build plugin ServiceProvider
```

插件服务容器释放前必须先撤销该插件产生的 ContributionLease，并关闭相关运行时 Scope。

## 6. 自动服务注册

可以提供自动服务注册，但默认必须是 source generator 自动注册，不是运行时扫描。

推荐三种注册方式：

```csharp
[ScopedService(typeof(IUserSession))]
public sealed class UserSession : IUserSession
{
}
```

```csharp
[Service(ServiceLifetime.Singleton)]
[ExposeServices(typeof(IClock))]
public sealed class SystemClock : IClock
{
}
```

```csharp
public sealed class CacheStore : ISingletonDependency
{
}
```

建议优先支持 Attribute，Marker Interface 作为简写。原因是 Attribute 更明确，能表达 exposed service、lifetime、replace、try-add、keyed service 等元数据。

## 7. Source Generator 注册模型

编译期流程：

```text
Find service candidate types
-> Read service attributes / marker interfaces
-> Validate constructors and exposed services
-> Generate service registration code
-> Emit service manifest
-> Emit diagnostics
```

生成代码示例：

```csharp
internal static class GeneratedServiceRegistrar
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<IUserSession, UserSession>();
        services.AddSingleton<IClock, SystemClock>();
    }
}
```

Strict AOT 模式下可以进一步生成强类型 factory：

```csharp
services.AddScoped<IUserSession>(sp =>
    new UserSession(sp.GetRequiredService<IClock>()));
```

这可以减少对运行时反射构造的依赖，但要求 generator 能明确选择构造函数并解析依赖。

## 8. 自动注册约束

默认禁止：

- 启动时扫描所有程序集找服务。
- 通过反射读取服务 Attribute 再注册。
- 基于命名约定运行时发现服务。
- 动态代理作为默认服务注册方式。
- Property injection 作为默认能力。

Analyzer 必须诊断：

- 多个公开构造函数但无明确构造函数选择。
- service id 或 exposed service 冲突。
- scoped 服务注入 singleton。
- 插件服务暴露为 Host 长期持有实例。
- 使用运行时扫描但未 opt-in。
- AOT Strict 下使用不可静态生成的工厂。

## 9. 服务覆盖策略

手写 `ConfigureServices` 遵循 `Microsoft.Extensions.DependencyInjection` 的基本语义。

生成注册需要更严格：

- 默认使用普通 `Add*` 还是 `TryAdd*` 需要由 attribute 指定或框架约定。
- 自动注册发现同一 service type 多个实现时，默认报诊断。
- 多实现服务必须显式声明允许 `IEnumerable<T>`。
- 替换服务必须显式使用 replace 语义。
- 静默覆盖不允许作为默认行为。

示例：

```csharp
[ScopedService(typeof(IUserSession), Replace = true)]
public sealed class CustomUserSession : IUserSession
{
}
```

## 10. Contribution 与服务来源

每个 Contribution 必须记录服务来源：

```text
Contribution
  Module
  Plugin?
  ServiceContext
  Lease
```

启动期模块贡献使用 Application 服务上下文。
插件模块贡献使用 Plugin 服务上下文。

例如插件贡献路由时，RouteScope 创建 ViewModel 应从插件 ServiceProvider 创建 route/activation service scope，而不是从 Host Root 解析。

```text
RouteContribution("/sales")
  Plugin = SalesPlugin
  Services = SalesPlugin ServiceProvider
```

插件停用时，Host 根据 Contribution 找到仍在运行的 RouteScope、ActivationScope、OperationScope，先取消和释放，再释放插件服务容器。

## 11. 公共抽象建议

| 类型 | 职责 |
|---|---|
| `ServiceConfigurationContext` | 模块服务配置上下文。 |
| `ServiceRegistrationDescriptor` | 编译期服务注册描述。 |
| `GeneratedServiceRegistrar` | SG 生成的服务注册入口。 |
| `ServiceContext` | 当前服务来源和 ServiceProvider 包装。 |
| `ServiceContextKind` | Root、Application、Plugin、Route、Activation、Operation。 |
| `IServiceContextAccessor` | 当前生命周期中访问服务上下文。 |
| `IHostContractRegistry` | Host 显式暴露给插件的 contract 集合。 |

## 12. 测试策略

Testing 包应支持：

- 构造测试 Host 并替换服务。
- 断言模块服务注册顺序。
- 断言自动注册生成结果。
- 断言重复注册和覆盖诊断。
- 断言插件不能解析未暴露 Host 服务。
- 断言插件卸载后无服务实例残留。
- 断言 scoped 服务不会从 Root Provider 解析。
