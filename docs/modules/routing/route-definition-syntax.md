# AtomUI.City.Routing Route Definition Syntax

版本：v0.1
状态：正式初版
适用范围：路由声明语法、路径模板、参数绑定、RouteReference、Source Generator 契约和诊断规则。

## 1. 定位

路由定义语法解决开发者如何声明应用页面入口的问题。

Routing 运行时负责匹配、导航、守卫、解析、RouteScope 和历史记录；本文件只定义路由如何被声明、如何被 Source Generator 分析，以及生成结果应满足哪些约束。

路由定义必须满足：

- C# / .NET 风格。
- AOT 和 trimming 友好。
- Source Generator 可静态分析。
- 支持强类型导航。
- 支持外部 Deep Link。
- 支持模块和插件贡献。
- 不依赖运行时程序集扫描。

## 2. 设计原则

路由定义采用声明式 Route Map。

核心原则：

- Route Map 是编译期输入，不是运行时扫描结果。
- 路由路径模板兼容 ASP.NET Core 10 Route Template 的主要语义。
- 路由身份以内置 `RouteId` 为准，路径只是可选外部寻址方式。
- 日常代码优先使用强类型 `RouteReference` 导航。
- `NavigateByPathAsync` 只用于 Deep Link、命令行入口、外部 URI 和测试。
- ViewModel 映射必须显式声明，不按命名约定猜测。
- Guard、Resolver、Middleware 必须显式声明。
- 插件路由必须通过 Contribution 进入 Host 路由图。

禁止：

- 启动时扫描所有程序集找路由。
- 通过字符串拼接作为主要导航方式。
- 运行时反射解析 ViewModel 构造函数。
- 通过 ViewModel 名称自动推导 View。
- 在路由定义中表达业务流程。
- 把不可序列化对象作为路由参数。

## 3. Route Map

Route Map 使用静态 `partial` 类型声明。

```csharp
[RouteMap]
public static partial class AppRoutes
{
    [LayoutRoute(typeof(ShellViewModel), Id = "app.shell")]
    public static partial RouteReference Shell();

    [IndexRoute(typeof(HomeViewModel), Parent = nameof(Shell))]
    public static partial RouteReference Home();

    [Route("settings", typeof(SettingsViewModel), Parent = nameof(Shell))]
    public static partial RouteReference Settings();
}
```

规则：

- `RouteMap` 类型必须是 `static partial class`。
- Route 方法必须是 `static partial`。
- Route 方法返回 `RouteReference` 或 `RouteReference<TParameters>`。
- Source Generator 负责生成 Route 方法实现。
- Route 方法名只服务编译期引用，不作为默认路径。
- 未显式指定 `Id` 时，默认使用 Route Map 类型完整名称加方法名生成稳定 RouteId。

默认 RouteId 示例：

```text
AtomUI.City.App.AppRoutes.Settings
```

公共 Deep Link 路由和插件路由应显式指定稳定 `Id`。

## 4. 路由类型

第一版建议提供以下声明类型：

| 声明 | 用途 |
|---|---|
| `Route` | 普通路由，有路径模板和 ViewModel。 |
| `LayoutRoute` | 布局路由，可以承载子路由和 Outlet。 |
| `IndexRoute` | 默认子路由，不增加路径段。 |
| `RouteGroup` | 只提供路径前缀或组织结构，不创建 ViewModel。 |
| `RouteExtensionPoint` | Host 开放给模块或插件挂载子路由的扩展点。 |
| `RedirectRoute` | 静态重定向路由。 |

RouteGroup 示例：

```csharp
[RouteGroup("admin", Parent = nameof(Shell))]
public static partial RouteReference Admin();

[Route("settings", typeof(AdminSettingsViewModel), Parent = nameof(Admin))]
public static partial RouteReference AdminSettings();
```

`Admin` 不创建 ViewModel，只提供路径前缀和层级关系。

## 5. Path Template

路径模板兼容 ASP.NET Core 10 Route Template 的主要语法。

| 语法 | 示例 | 语义 |
|---|---|---|
| Literal segment | `settings/profile` | 静态路径段。 |
| Parameter | `items/{id}` | 路径参数。 |
| Constraint | `items/{id:guid}` | 参数约束和类型绑定。 |
| Optional parameter | `items/{id?}` | 可选路径参数。 |
| Default value | `culture/{lang=en}` | 参数默认值。 |
| Catch-all | `files/{*path}` | 捕获剩余路径。 |
| Multiple constraints | `items/{id:int:min(1)}` | 多个约束组合。 |
| Regex constraint | `{slug:regex(^[a-z0-9_-]+$)}` | 正则约束。 |

示例：

```csharp
public readonly record struct DetailsParameters(Guid Id);

[Route("details/{id:guid}", typeof(DetailsViewModel), Parent = nameof(Shell))]
public static partial RouteReference<DetailsParameters> Details();
```

AtomUI.City 不支持以下 Web 专用语义：

- Controller / Action token，例如 `[controller]`、`[action]`。
- HTTP method 匹配。
- Endpoint routing。
- MVC action selection。
- ASP.NET Core model binding 全套规则。
- Web middleware pipeline。

Path Template 只描述导航路径，不表达 HTTP 请求。

## 6. 参数绑定

参数类型必须显式声明。

```csharp
public readonly record struct SearchParameters(
    string Keyword,
    [property: Query] int Page = 1);

[Route("search", typeof(SearchViewModel), Parent = nameof(Shell))]
public static partial RouteReference<SearchParameters> Search();
```

参数规则：

- 参数类型必须是不可变类型。
- 推荐使用 `readonly record struct` 或 `record`。
- Path token 必须能绑定到参数属性或主构造函数参数。
- Query 参数必须显式标注 `Query`。
- Fragment 参数必须显式标注 `Fragment`。
- 参数名称匹配默认大小写不敏感。
- 参数绑定失败时导航失败，不进入 ViewModel 创建阶段。

禁止作为路由参数：

- ViewModel。
- View。
- ServiceProvider。
- Stream。
- CancellationToken。
- Delegate。
- 任意 UI 对象。
- 插件私有类型跨插件边界传递。
- 大型复杂对象图。

复杂数据应进入 State、Resolver、Data 或应用自己的持久化机制。

## 7. 约束

内置约束建议第一版支持：

| 约束 | 目标类型 |
|---|---|
| `bool` | `bool` |
| `int` | `int` |
| `long` | `long` |
| `float` | `float` |
| `double` | `double` |
| `decimal` | `decimal` |
| `guid` | `Guid` |
| `datetime` | `DateTime` |
| `min(value)` | 数值 |
| `max(value)` | 数值 |
| `range(min,max)` | 数值 |
| `length(value)` | `string` |
| `minlength(value)` | `string` |
| `maxlength(value)` | `string` |
| `alpha` | `string` |
| `regex(pattern)` | `string` |

约束处理规则：

- Source Generator 必须校验约束名称。
- Source Generator 必须校验约束与参数类型是否兼容。
- Regex 约束必须在编译期解析，非法表达式报诊断。
- 运行时匹配使用生成后的强类型匹配逻辑。
- 自定义约束必须显式注册，不允许运行时按名称反射创建。

自定义约束建议：

```csharp
[RouteConstraint("culture")]
public sealed partial class CultureRouteConstraint : IRouteConstraint
{
}
```

Source Generator 应将自定义约束写入 Route Manifest。

## 8. 层级、Outlet 和布局

父子关系通过 `Parent` 显式声明。

```csharp
[LayoutRoute(typeof(SettingsLayoutViewModel), Parent = nameof(Shell))]
public static partial RouteReference SettingsLayout();

[IndexRoute(typeof(SettingsHomeViewModel), Parent = nameof(SettingsLayout))]
public static partial RouteReference SettingsHome();

[Route("advanced", typeof(SettingsAdvancedViewModel), Parent = nameof(SettingsLayout))]
public static partial RouteReference SettingsAdvanced();
```

命名 Outlet：

```csharp
[Route("help", typeof(HelpViewModel), Parent = nameof(Shell), Outlet = "side")]
public static partial RouteReference Help();
```

规则：

- 默认 Outlet 名称为 `primary`。
- Outlet 名称必须稳定，不能运行时动态变更。
- LayoutRoute 可以声明自己承载的默认 Outlet。
- Routing 只记录 Outlet 目标，具体 UI 插入由 Presentation 处理。
- 命名 Outlet 不等于独立 NavigationScope。
- 只有需要独立历史和独立并发控制时，才创建新的 NavigationScope。

## 9. Guard、Resolver 和 Middleware

Guard、Resolver、Middleware 通过 Attribute 绑定。

```csharp
[Route("profile/{id:guid}", typeof(ProfileViewModel), Parent = nameof(Shell))]
[RouteGuards(typeof(ProfileAccessGuard))]
[RouteResolvers(typeof(ProfileResolver))]
[RouteMiddleware(typeof(ProfileNavigationMiddleware))]
public static partial RouteReference<ProfileParameters> Profile();
```

规则：

- 类型必须实现对应 contract。
- Source Generator 必须校验类型可访问性。
- Source Generator 必须校验构造函数可由 DI 创建。
- 多个 Guard 按声明顺序执行。
- Resolver 默认按声明顺序执行。
- Middleware 按路由树父到子进入、子到父退出。
- 插件路由只能引用插件自身或 Host 共享 contract 中的类型。

Guard、Resolver、Middleware 不写业务流程，只表达导航阶段可复用的横切能力。

## 10. Redirect

静态重定向通过 `RedirectRoute` 声明。

```csharp
[RedirectRoute("old-settings", Target = nameof(Settings), Parent = nameof(Shell))]
public static partial RouteReference OldSettings();
```

规则：

- Source Generator 必须检测静态重定向循环。
- 重定向目标必须存在。
- 跨插件重定向只能指向 Host 共享路由或显式允许的扩展点。
- 动态重定向由 Guard 或 Resolver 返回 `NavigationRedirect`。

## 11. Route Extension Point

Host 或普通模块可以声明扩展点。

```csharp
[RouteExtensionPoint("settings.pages", Parent = nameof(SettingsLayout))]
public static partial RouteExtensionPoint SettingsPages();
```

插件或后续模块可以贡献到扩展点。

```csharp
[Route("plugin-settings", typeof(PluginSettingsViewModel), ExtensionPoint = "settings.pages")]
public static partial RouteReference PluginSettings();
```

规则：

- 扩展点 Id 必须全局稳定。
- 插件不能挂载到未开放的父路由。
- 插件贡献必须创建 ContributionLease。
- 扩展点可以限制允许的 Outlet、权限、排序和能力。
- 插件停用时，挂载到扩展点的路由必须随 Lease 撤销。

## 12. 强类型导航

日常导航使用 `RouteReference`。

```csharp
await router.NavigateAsync(AppRoutes.Settings());

await router.NavigateAsync(
    AppRoutes.Details(),
    new DetailsParameters(id));
```

推荐 API：

```csharp
public interface IRouter
{
    ValueTask<NavigationResult> NavigateAsync(
        RouteReference route,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> NavigateAsync<TParameters>(
        RouteReference<TParameters> route,
        TParameters parameters,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

Path 导航只作为适配入口：

```csharp
await router.NavigateByPathAsync("details/6f9619ff-8b86-d011-b42d-00cf4fc964ff");
```

Path 导航必须经过同一套路由匹配、Guard、Resolver 和事务流程。

## 13. Source Generator 输出

Source Generator 必须生成：

- Route Manifest。
- RouteId 常量。
- RouteReference 方法实现。
- Path formatter。
- Path parser。
- 参数 binder。
- 约束匹配器。
- Guard descriptor。
- Resolver descriptor。
- Middleware descriptor。
- ViewModel target descriptor。
- 插件 Contribution descriptor。
- 诊断信息。

运行时只消费生成后的 descriptor，不重新解析 Attribute。

生成结果应满足：

- 无运行时程序集扫描。
- 无反射发现路由。
- 无命名约定猜测。
- 可被 trimming 保留。
- 可被测试断言。
- 可被插件加载和卸载机制追踪来源。

## 14. 诊断

Source Generator 必须诊断：

- Route Map 类型不是 `static partial class`。
- Route 方法不是 `static partial`。
- Route 方法返回类型非法。
- RouteId 重复。
- 同父路由下路径冲突。
- Parent 不存在。
- ExtensionPoint 不存在。
- ExtensionPoint 不允许当前贡献。
- Path Template 语法非法。
- Path 参数和参数类型不匹配。
- Query 参数重复。
- Optional 参数绑定到非可空且无默认值成员。
- Catch-all 参数不是 `string` 或 `string[]`。
- Constraint 不存在。
- Constraint 与目标类型不兼容。
- Regex 约束非法。
- Guard、Resolver、Middleware 类型不满足 contract。
- ViewModel 类型不可由 DI 创建。
- 插件公共路由没有显式 Id。
- 公共 Deep Link 路由没有显式 Id。
- 静态重定向循环。

运行时必须诊断：

- 参数解析失败。
- 无匹配路由。
- 多路由优先级冲突。
- 插件路由已撤销。
- 路由图版本过期。
- NavigationScope 已停止。

## 15. 测试策略

Testing 包应支持：

- 加载生成的 Route Manifest。
- 断言 RouteId。
- 断言路径格式化。
- 断言路径解析。
- 断言参数绑定。
- 断言约束失败。
- 断言父子关系。
- 断言 Outlet。
- 断言扩展点挂载。
- 断言插件路由撤销后不可匹配。

路由语法测试不应启动真实 UI。

## 16. 第一版取舍

第一版暂不支持：

- 文件系统路由。
- Controller / Action token。
- 运行时动态 CLR 路由定义。
- 任意对象路由参数。
- EndpointDataSource。
- HTTP method 匹配。
- Web model binding。
- 隐式 ViewModel / View 命名约定。

第一版重点是让 Route Map、Path Template、强类型导航和 Source Generator 契约稳定下来。
