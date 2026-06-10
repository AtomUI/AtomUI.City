# AtomUI.City.Core Configuration 设计

版本：v0.1
状态：初版草案
适用范围：`AtomUI.City.Core` 中配置源、Options、PreConfigure、配置验证、热更新、插件配置隔离、AOT/source generator 约束。

## 1. 定位

Configuration 是 Host、Module、DI、PluginSystem 和运行时能力的基础输入。

AtomUI.City 默认复用：

- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Options`

AtomUI.City 在其上补充桌面应用需要的配置分层、模块配置阶段、插件配置隔离、AOT 友好的 options binding 和配置诊断。

## 2. 非目标

Configuration 不负责：

- 用户偏好 UI。
- 业务数据持久化。
- 状态管理。
- 插件包发现。
- 权限策略执行。
- 远程配置中心默认实现。

这些可以由上层模块扩展，但不是 Core Configuration 的第一职责。

## 3. 配置来源

建议默认配置分层：

```text
Framework defaults
-> Module defaults
-> appsettings.json
-> appsettings.{Environment}.json
-> machine configuration
-> user configuration
-> workspace configuration
-> plugin default configuration
-> plugin user configuration
-> environment variables
-> command line
```

后加载覆盖先加载。

桌面应用需要明确三类本地配置：

| 配置 | 说明 |
|---|---|
| Machine configuration | 当前机器级别配置。 |
| User configuration | 当前用户配置，适合偏好、认证缓存引用、窗口布局策略。 |
| Workspace configuration | 当前工作区或项目级配置。 |

插件配置必须独立分区，不允许默认写入 Host 全局配置根。

## 4. 模块配置阶段

模块配置阶段分为：

```text
PreConfigureServices
-> ConfigureServices
-> PostConfigureServices
-> Build ServiceProvider
-> ConfigureContributions
-> Application Initialization
```

配置相关规则：

- `PreConfigureServices`：声明早期 options 默认值和模块约定。
- `ConfigureServices`：绑定 options、注册 options validation、注册配置相关服务。
- `PostConfigureServices`：做最终修正和兼容性处理。
- `Application Initialization`：只读取最终配置，不再修改服务注册。

## 5. PreConfigure Options

AtomUI.City 需要提供 `PreConfigure<TOptions>`。

它用于模块之间传递早期默认值和约定，不替代 `IOptions<T>`。

阶段顺序：

```text
PreConfigure<TOptions>
-> Configure<TOptions>
-> PostConfigure<TOptions>
```

建议 API：

```csharp
context.PreConfigure<RoutingOptions>(options => { });

context.ExecutePreConfigure(existingOptions);
```

底层也可以提供 `IServiceCollection` 扩展：

```csharp
services.PreConfigure<RoutingOptions>(options => { });
services.ExecutePreConfigure<RoutingOptions>();
```

设计规则：

- `PreConfigure<TOptions>` action 是同步的。
- 不允许 IO。
- 不依赖 ServiceProvider。
- 按 ModuleGraph 拓扑顺序执行。
- 插件拥有独立 PreConfigure store。
- 插件不能修改 Host 全局 PreConfigure store。

## 6. Options 模型

第一版支持：

| API | 用途 |
|---|---|
| `IOptions<T>` | 稳定配置读取。 |
| `IOptionsMonitor<T>` | 支持 reloadable 配置。 |
| `IOptionsSnapshot<T>` | 桌面应用默认不推荐，除非特定 scope 明确需要。 |
| `PreConfigure<T>` | 早期默认值和模块约定。 |
| `Configure<T>` | 正常绑定和配置。 |
| `PostConfigure<T>` | 最终修正。 |
| `Validate<T>` | 启动期或插件激活期验证。 |

Options 类型建议显式声明：

```csharp
[Options("AtomUI:Routing")]
public sealed partial class RoutingOptions
{
}
```

## 7. 插件配置隔离

插件配置结构建议：

```text
Plugins
  {PluginId}
    defaults
    user
    workspace
```

插件只能访问自己的配置 section。访问 Host 配置必须通过 Host 暴露的 contract 或显式授权。

插件加载流程：

```text
Read plugin metadata
-> Load plugin default configuration
-> Load plugin user/workspace configuration
-> Create plugin configuration context
-> Run plugin module PreConfigureServices
-> Bind plugin options
-> Validate plugin options
-> Build plugin ServiceProvider
```

插件停用或卸载时：

- 停止配置监听。
- 释放 options monitor subscriptions。
- 释放插件 configuration context。
- 不删除用户配置，除非用户明确卸载并清理。

## 8. 热更新边界

默认只有明确声明为 reloadable 的 options 支持热更新。

```csharp
[Options("AtomUI:Routing", Reloadable = true)]
public sealed partial class RoutingOptions
{
}
```

配置分三类：

| 类型 | 热更新策略 |
|---|---|
| Startup-only | 需要重启 Host。 |
| Plugin-activation | 需要重载插件。 |
| Reloadable | 可通过 `IOptionsMonitor<T>` 通知更新。 |

热更新不能自动重建 DI 容器，不能修改模块图，不能修改已经加载的插件服务注册。

## 9. AOT / Source Generator

Configuration 默认 AOT-first。

Generator 负责：

- 生成 options descriptor。
- 生成 configuration section 映射。
- 生成 binding 代码。
- 生成 validation 调用。
- 生成 manifest。
- 诊断反射式 binding。
- 诊断未声明 options 类型。

默认禁止：

- 运行时扫描程序集寻找 options。
- 反射式 binding 作为 Strict 模式默认路径。
- 动态生成 options proxy。
- 配置 action 中执行用户代码发现类型。

## 10. 错误策略

启动期 required options 验证失败：Host 启动失败。

插件 options 验证失败：插件激活失败，主应用继续运行。

reloadable options 更新失败：保留上一份有效配置，记录 diagnostics，并通知订阅方更新失败。

配置缺失、格式错误、绑定失败、验证失败都必须进入 Diagnostics，包含：

- options 类型。
- configuration section。
- 模块。
- 插件。
- 配置来源。
- 错误阶段。

## 11. 公共抽象建议

| 类型 | 职责 |
|---|---|
| `ConfigurationContext` | 当前配置上下文。 |
| `OptionsDescriptor` | Options 元数据。 |
| `PreConfigureActionStore` | 构建期 PreConfigure action 存储。 |
| `IOptionsDescriptorRegistry` | Options 描述注册表。 |
| `IConfigurationScope` | Application 或 Plugin 的配置边界。 |
| `ConfigurationReloadPolicy` | 热更新策略。 |
| `ConfigurationValidationResult` | 配置验证结果。 |

## 12. 测试策略

Testing 包应支持：

- 构造测试 configuration context。
- 覆盖 options。
- 断言 PreConfigure 执行顺序。
- 断言 Configure/PostConfigure 结果。
- 模拟插件配置隔离。
- 模拟 reloadable options 更新。
- 断言配置验证失败诊断。
- 断言 AOT Strict 下禁止反射式 binding。
