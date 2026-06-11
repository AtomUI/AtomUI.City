# PluginSystem 生命周期设计

版本：v0.1
状态：正式初版
适用范围：插件发现、验证、加载、启用、停用、卸载和 UnloadPending 状态

## 1. 目标

插件生命周期必须服务桌面应用长期运行模型。插件可能在应用运行期间被安装、启用、停用、更新或卸载，因此生命周期必须可控、可取消、可诊断、可回滚。

本设计只描述 PluginSystem 模块内的生命周期实现策略。全局生命周期规则见：[AtomUI.City 生命周期](../../architecture/lifecycle.md)。

## 2. 状态机

插件状态建议：

```text
Discovered
-> Verified
-> Loaded
-> Initialized
-> ContributionsApplied
-> Active
-> Deactivating
-> Inactive
-> Unloading
-> Unloaded
```

错误或特殊状态：

```text
Invalid
Disabled
Faulted
UnloadPending
```

状态规则：

- `Discovered` 只表示发现插件位置，不表示可以加载。
- `Verified` 表示元数据、版本和能力声明通过校验。
- `Loaded` 表示程序集已进入插件加载上下文。
- `ContributionsApplied` 表示贡献已注册并生成 lease。
- `Active` 表示插件入口可被路由、命令、事件等系统使用。
- `Inactive` 表示插件已停用，但加载上下文可以仍存在。
- `UnloadPending` 表示已请求卸载，但仍有引用阻止加载上下文释放。

## 3. 发现与验证

发现流程：

```text
Scan plugin locations
-> Read metadata
-> Build plugin descriptor
-> Check duplicate plugin id
-> Check version and compatibility
-> Check declared capabilities
-> Check dependencies
-> Mark Verified or Invalid
```

验证失败默认不影响主应用启动。失败原因必须进入诊断。

验证阶段不应执行插件代码。元数据读取应尽量基于清单文件或包元数据，避免为了读取信息而加载插件程序集。

## 4. 加载

加载流程：

```text
Create plugin lifecycle context
-> Create plugin load context
-> Resolve dependencies
-> Load plugin assemblies
-> Build plugin module graph
-> Build plugin service scope
-> Initialize plugin modules
```

规则：

- 每个可卸载插件使用独立加载上下文。
- 插件 contract 由 Host 默认上下文加载。
- 插件依赖解析必须记录来源和版本。
- 插件 service scope 只服务当前 Plugin。
- 加载失败时释放已创建资源并标记 Disabled 或 Faulted。

.NET 依赖加载机制参考：[.NET 依赖项加载参考](../../reference/dotnet/dependency-loading.md)。

## 5. 启用

启用流程：

```text
Run PluginActivate pipeline
-> Collect contribution requests
-> Validate contribution requests
-> Apply contributions to Host registries
-> Store contribution leases
-> Mark Active
```

启用失败时：

```text
Reject new plugin entry
-> Revoke already-created leases
-> Dispose activation resources
-> Mark Disabled or Faulted
```

启用完成后，插件能力才可以被路由、命令、EventBus、Data、Security、Localization 和 Presentation 使用。

## 6. 停用

停用不是卸载。停用后插件可以保留加载上下文和服务 Scope，以便后续重新启用。

停用流程：

```text
Mark Deactivating
-> Stop accepting new plugin entry
-> Deactivate plugin routes and view models
-> Cancel plugin operations
-> Revoke contribution leases in reverse order
-> Stop plugin modules
-> Mark Inactive
```

停用期间：

- 新路由不能进入插件页面。
- 新命令不能执行插件动作。
- 新后台任务不能启动。
- 已运行 Operation 必须收到取消。
- EventBus 订阅必须撤销或禁用。
- UI 入口必须移除或禁用。
- Dispatcher callback、Timer 和后台任务必须解除或结束。

## 7. 卸载

卸载要求插件已停用。如果插件仍处于 Active，Host 必须先执行停用流程。

卸载流程：

```text
Ensure Inactive
-> Mark Unloading
-> Dispose plugin subscriptions and resources
-> Dispose plugin service scope
-> Request AssemblyLoadContext unload
-> Force cooperative GC verification
-> Mark Unloaded or UnloadPending
```

卸载成功表示插件加载上下文已经释放，Host 不再持有插件程序集、类型、实例、委托、反射对象或资源引用。

## 8. UnloadPending

`UnloadPending` 表示运行时暂时无法释放插件加载上下文。

常见原因：

- Host 或其他模块仍持有插件类型实例。
- EventBus 订阅没有解除。
- 静态字段保存插件对象或委托。
- 后台线程仍在运行。
- Timer、Task、Dispatcher 回调未释放。
- 反射对象、Assembly、Type、MethodInfo 被长期持有。
- UI 对象仍在 visual tree 或资源字典中。

进入 `UnloadPending` 后：

- 插件不能更新或删除文件。
- 插件不能重新启用。
- Host 必须输出剩余引用相关诊断。
- 可以允许后续重试卸载。

插件线程模型必须遵守 Core Threading 设计。插件不能启动非受控线程，后台任务必须通过 Host 管理的调度入口创建并绑定插件生命周期。

线程模型见：[Core Threading 设计](../core/threading.md)。

## 9. 生命周期 Middleware

PluginSystem 应接入 Lifecycle Middleware。

建议阶段：

| 阶段 | 用途 |
|---|---|
| PluginDiscover | 插件发现、元数据过滤、来源策略。 |
| PluginVerify | 版本、签名、能力和依赖校验。 |
| PluginLoad | 加载上下文、依赖解析、服务 Scope 创建。 |
| PluginActivate | 贡献申请、贡献校验、注册到 Host。 |
| PluginDeactivate | 停止入口、取消操作、撤销贡献。 |
| PluginUnload | 释放服务、请求卸载、诊断 UnloadPending。 |
| PluginError | 插件错误统一处理。 |

Middleware 可以拒绝插件、修改诊断、执行审计、增加策略校验或包裹默认处理，但不能破坏 Scope Tree 的释放规则。

## 10. 错误策略

默认策略：

| 阶段 | 默认策略 |
|---|---|
| Discover | 跳过当前插件。 |
| Verify | 标记 Invalid。 |
| Load | 标记 Disabled 或 Faulted，并释放已创建资源。 |
| Activate | 撤销已创建 lease，标记 Disabled 或 Faulted。 |
| Deactivate | 尽力撤销剩余 lease，汇总错误。 |
| Unload | 标记 UnloadPending。 |

插件错误默认 Non-fatal。Host 可把必需插件失败升级为 Fatal，但这应是显式策略。

## 11. 测试要求

Testing 包后续应支持：

- 驱动插件状态机。
- 模拟插件加载失败。
- 模拟插件启用失败后的 lease 回滚。
- 断言停用顺序。
- 断言卸载前所有 Operation 被取消。
- 断言卸载前所有 lease 被撤销。
- 模拟 `UnloadPending`。
- 断言诊断记录包含插件 Id、阶段和失败原因。
