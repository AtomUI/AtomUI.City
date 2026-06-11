# AtomUI.City.EventBus Event Contracts 设计

版本：v0.1
状态：正式初版
适用范围：事件契约身份、共享 Contract 程序集、AssemblyLoadContext、插件跨边界事件、版本兼容、对象图约束、manifest 和卸载设计。

## 1. 定位

Event Contract 定义 EventBus 中可以发布什么数据，以及 Host、静态模块和运行时插件如何对同一个事件类型形成一致认识。

跨插件边界时，Contract 设计直接决定：

- CLR 类型身份是否一致。
- 插件能否匹配 Host 发布的事件。
- Contract 版本是否兼容。
- Host 是否会意外持有插件私有类型。
- 插件的 `AssemblyLoadContext` 能否卸载。

因此，Event Contract 不是普通 DTO 约定，而是 Host 和插件之间的稳定运行时边界。

## 2. CLR 类型身份

.NET 类型身份不仅由 namespace 和 type name 决定，还包括定义它的 Assembly 以及加载该 Assembly 的 `AssemblyLoadContext`。

即使两个类型具有完全相同的名称：

```text
MyApplication.Contracts.WorkspaceChangedEvent
```

如果它们分别由 Default AssemblyLoadContext 和 Plugin AssemblyLoadContext 加载，运行时仍然认为它们是不同类型：

```text
Default ALC::MyApplication.Contracts.WorkspaceChangedEvent
!=
Plugin ALC::MyApplication.Contracts.WorkspaceChangedEvent
```

直接后果：

- Host 发布的事件无法匹配插件订阅。
- 强制类型转换失败。
- 泛型 handler 无法调用。
- EventBus 被迫退化为反射或序列化桥接。
- Host 缓存插件 ALC 中的 `Type` 时会阻止插件卸载。
- 不同插件携带不同 Contract 版本时会产生类型冲突。

## 3. 跨边界 Contract 规则

任何需要在 Host、静态模块或多个插件之间发布和订阅的事件，其事件类型及完整对象图必须：

- 来自 Host 注册的共享 Contract 程序集。
- 由 Default AssemblyLoadContext 唯一加载。
- 在 Host Contract Registry 中有唯一描述。
- 使用稳定 EventContractId。
- 满足版本兼容规则。
- 不包含插件私有类型。

插件私有事件类型只能在插件私有 EventBus 平面内使用，不能进入 Host 共享事件总线。

## 4. 共享 Contract 程序集

共享 Contract 程序集是由 Host 管理、由多个运行边界共同引用、由 Default AssemblyLoadContext 唯一加载的稳定契约程序集。

推荐分类：

| 程序集类别 | 职责 |
|---|---|
| `AtomUI.City.EventBus.Abstractions` | EventBus 基础接口、上下文和稳定元数据类型。 |
| Application contracts | 当前应用允许模块和插件共同使用的事件契约。 |
| Extension contracts | 某个扩展点或插件生态共同使用的事件契约。 |

框架不要求所有应用事件放进一个巨大程序集。应用可以按稳定边界拆分多个 Contract 包，但每个包都必须由 Host 注册和统一加载。

业务事件不能放进 `AtomUI.City.EventBus` 框架包。框架只提供事件机制和基础 contract。

## 5. 推荐加载结构

```text
Default AssemblyLoadContext
├── AtomUI.City.EventBus.Abstractions
├── MyApplication.Contracts
└── DocumentExtension.Contracts

Plugin A AssemblyLoadContext
├── PluginA.dll
└── PluginA.Private.dll

Plugin B AssemblyLoadContext
├── PluginB.dll
└── PluginB.Private.dll
```

插件可以在构建时引用 `MyApplication.Contracts` 或 `DocumentExtension.Contracts`，但运行时不能在自己的 ALC 中再次加载这些 Assembly。

## 6. Contract 解析流程

PluginSystem 加载插件依赖时必须先查询 Host Contract Registry：

```text
Plugin requests an assembly
-> Check Host shared contract registry
-> Validate assembly identity and version
-> Return assembly already loaded by Default ALC
-> Do not load plugin-local contract copy
```

如果插件包携带共享 Contract 的本地副本：

- Plugin loader 必须忽略该副本。
- 必须验证其编译目标版本与 Host 提供版本兼容。
- 不允许插件本地副本覆盖 Host Contract。
- 不兼容时在 PluginVerify 或 PluginLoad 阶段拒绝插件。

不能等到第一次事件发布时才发现类型不匹配。

## 7. Host Contract Registry

Host 维护共享契约注册表。它可以与全局 `IHostContractRegistry` 集成，并由 EventBus 暴露专用读取模型。

每个 Contract Assembly descriptor 至少包含：

- Assembly name。
- Assembly version。
- Public key token。
- Contract package id。
- Contract version。
- 兼容版本范围。
- 来源。
- 是否允许插件引用。
- 允许的 capability。
- 包含的 EventContract descriptor。

每个 EventContract descriptor 至少包含：

- EventContractId。
- CLR type。
- Contract version。
- Declaring contract assembly。
- Schema fingerprint。
- Publish capability。
- Subscribe capability。
- Channel definitions。
- Compatibility metadata。
- Diagnostic metadata。

运行时发布热路径使用预构建 descriptor，不反复反射读取 Assembly 和 Attribute。

## 8. EventContractId

EventContractId 是事件跨版本和跨加载边界的稳定身份。

示例：

```text
workspace.changed.v1
document.saved.v1
plugin.state-changed.v1
```

规则：

- Id 在 Host 共享事件平面内唯一。
- Id 使用稳定、可读、与 CLR 类型名解耦的形式。
- Id 一旦公开不能改变语义。
- 破坏性变更创建新 Id 或新 major version。
- 未显式声明的内部事件可以由 source generator 使用类型全名生成默认 Id。
- 插件共享事件应显式声明 Id。

Contract Id 不用于把任意无类型 payload 转换为动态消息。运行时仍然保留强类型映射。

## 9. Contract 类型设计

跨边界事件类型应该是简单、不可变的数据契约。

推荐：

- `sealed record`。
- `readonly record struct`。
- 只读属性。
- 构造时完成初始化。
- 明确 nullability。
- 简单、稳定的数据结构。

可以使用：

- BCL 基础类型。
- Contract Assembly 自己定义的 enum。
- Contract Assembly 自己定义的不可变 value object。
- `DateTimeOffset`、`Guid` 等稳定值类型。
- 只读集合接口，但实际值应避免后续原地修改。

不推荐使用继承层次表达事件多态。默认只进行精确 contract 匹配，避免运行时扫描继承树和产生隐式订阅范围。

## 10. 完整对象图约束

仅仅让事件根类型位于共享 Contract Assembly 还不够。事件的完整对象图也不能包含插件私有对象。

禁止携带：

- AtomUI/Avalonia View、Control、Window。
- ViewModel。
- `IServiceProvider`。
- Delegate。
- `Task`、`CancellationTokenSource`。
- Stream 和开放资源句柄。
- 插件服务实例。
- 插件私有异常。
- 插件私有 `Type` 或反射对象。
- 可变全局集合。
- 无约束 `object` 扩展数据。

以下设计不允许作为共享 Contract：

```csharp
public sealed record DataChangedEvent(object Data);
```

即使 `DataChangedEvent` 位于共享 Assembly，`Data` 仍可能引用插件私有实例，从而把插件类型泄漏给 Host 和其他订阅者。

如果确实需要扩展字段，应使用受约束的稳定值模型，例如：

- `IReadOnlyDictionary<string,string>`。
- 明确定义的 Contract value object。
- 已注册的可序列化 schema。

第一版不建议提供任意 object property bag。

## 11. Shared Plane 与 Private Plane

EventBus 存在两个逻辑平面：

| 平面 | Contract 来源 | 可见范围 | 生命周期 |
|---|---|---|---|
| Shared Contract Plane | Host 注册的共享 Contract Assembly | Host、静态模块、授权插件 | ApplicationScope |
| Plugin Private Plane | 插件自己的 Assembly | 当前插件内部 | 插件生命周期 |

共享事件：

```text
Host or module publishes
-> Shared Contract Plane
-> Host/module/authorized plugin handlers
```

插件私有事件：

```text
Plugin publishes private event
-> Plugin Private Plane
-> Same plugin handlers only
```

插件私有事件不能因为 namespace 看起来公共就进入 Shared Plane。判断依据是加载期生成并注册的 EventContract descriptor。

## 12. 插件之间通信

Plugin A 不应直接引用 Plugin B 的实现程序集或私有 Contract。

推荐关系：

```text
Plugin A
  -> DocumentExtension.Contracts

Plugin B
  -> DocumentExtension.Contracts

Host
  -> registers and loads DocumentExtension.Contracts
```

Plugin A 发布共享事件，Plugin B 订阅共享事件。双方只依赖 Contract，不依赖彼此实现，也不要求双方同时加载。

如果某个 contract 只存在于 Plugin A 私有 Assembly，它就只能用于 Plugin A 内部通信。

## 13. Capability 与访问控制

共享 Contract 不表示所有插件都自动获得发布和订阅权限。

Contract descriptor 可以声明：

- HostOnlyPublish。
- ModulePublish。
- PluginPublish。
- HostOnlySubscribe。
- PluginSubscribe。
- Required capability。
- Allowed channel。

Plugin manifest 必须声明：

- 需要引用的 Contract Assembly。
- 需要发布的 EventContractId。
- 需要订阅的 EventContractId。
- 需要的 channel。

Host 在插件加载前完成 capability 校验。未授权插件不能获得相应 publisher/subscriber contract。

## 14. 版本兼容

共享 Contract 必须采用显式兼容策略。

通常兼容的变更：

- 增加具有默认语义的可选字段。
- 增加新的 enum value，同时订阅方能处理 unknown value。
- 增加新的事件 contract。

破坏性变更：

- 删除字段。
- 修改字段类型。
- 修改字段必填性。
- 修改字段语义。
- 重命名稳定 Contract Id。
- 把嵌套类型替换成不兼容类型。

破坏性变更必须：

- 创建新的 EventContractId 或 major version。
- 保留旧 contract 的明确兼容周期。
- 在 Host manifest 中声明支持范围。
- 由应用或适配 handler 显式转换。

EventBus Core 不自动猜测 schema 兼容性。

## 15. Host 版本选择

Host 决定最终加载的共享 Contract 版本。

加载规则：

- Host 先加载并注册共享 Contract。
- 插件声明可接受版本范围。
- PluginSystem 验证 Host 版本是否满足范围。
- 插件不能私自加载另一版本并覆盖 Host。
- 多个插件要求互不兼容版本时，拒绝不兼容插件并输出诊断。

第一版不承诺同一 Contract Assembly 的多个 major version 在同一 Shared Plane 并存。需要并存时应使用不同 Assembly identity 和不同 EventContractId。

## 16. Manifest 与 Source Generator

Source Generator 为共享 Contract 生成：

- EventContractId。
- CLR type mapping。
- Contract version。
- Schema fingerprint。
- 嵌套 contract type graph。
- Channel metadata。
- Capability metadata。
- Strongly typed publisher descriptor。
- Strongly typed handler invoker descriptor。

构建期 analyzer 应检查：

- Contract Id 重复。
- 共享事件引用插件项目类型。
- 共享事件包含 `object`、delegate、UI type 或反射 type。
- Contract 类型不是稳定可访问类型。
- Contract version 缺失或不合法。
- 插件 manifest 未声明共享 Contract 依赖。

Dynamic Plugin Mode 读取插件预生成的 event manifest，不扫描任意类型。

## 17. 发布时校验

发布 Shared Plane 事件时，EventBus 使用已注册 descriptor 校验：

- EventContractId 是否存在。
- CLR type 是否与 descriptor 一致。
- 发布方是否有 capability。
- channel 是否允许。
- 插件是否仍处于 Active。

这些校验基于 descriptor 和整数/引用比较，不在热路径遍历 Assembly metadata。

插件尝试把私有事件发布到 Shared Plane 时，EventBus 必须拒绝并记录：

- PluginId。
- Event type。
- AssemblyLoadContext。
- Requested channel。
- Missing contract descriptor。

## 18. 卸载与缓存

Host 长期缓存只能保存 Shared Contract Plane 中由 Default ALC 加载的类型。

插件停用和卸载时必须清理：

- 插件 handler delegate。
- 插件 handler instance。
- 插件 subscription descriptor。
- 插件私有 EventContract descriptor。
- 插件私有 channel。
- 插件私有事件队列。
- 包含插件私有泛型参数的缓存。
- 插件 dispatch plan。

禁止在 Host 静态泛型缓存中永久保存：

```text
PluginPrivateEvent
IEventHandler<PluginPrivateEvent>
Func<PluginPrivateEvent,...>
```

Plugin Private Plane 的所有缓存必须由插件运行时上下文持有，并在卸载前整体释放。

## 19. Contract Assembly 卸载

Shared Contract Assembly 由 Default ALC 持有，生命周期通常与 Host 一致，不随单个插件卸载。

这意味着：

- 卸载 Plugin A 不会卸载共享 Contract Assembly。
- 共享事件对象可以在 Host 中正常存在。
- 必须清理的是 Plugin A 的 handler、delegate、队列和私有类型。

如果某个 Extension Contract 需要动态卸载，它就不能被作为 Host Shared Contract 使用。第一版不支持可卸载共享 Contract Assembly。

## 20. 错误处理

| 场景 | 处理 |
|---|---|
| Host 缺少插件需要的 Contract Assembly | 插件验证失败。 |
| Contract 版本不兼容 | 插件验证失败。 |
| Contract Id 冲突 | Host 构建或插件激活失败。 |
| 插件重复加载本地 Contract 副本 | 拒绝加载并输出类型身份诊断。 |
| 私有事件进入 Shared Plane | 拒绝发布。 |
| 共享事件对象图包含私有类型 | 构建期报错；运行时防御性拒绝。 |
| 插件无 publish/subscribe capability | 拒绝贡献或发布。 |

错误不能延迟为无法解释的 handler cast exception。

## 21. 测试要求

必须测试：

- Default ALC 和 Plugin ALC 中同名类型不相等。
- 插件依赖解析返回 Default ALC 中的共享 Assembly。
- 插件本地 Contract 副本不会被再次加载。
- 兼容版本插件可以加载。
- 不兼容版本插件在验证阶段失败。
- 私有事件不能发布到 Shared Plane。
- 共享事件完整对象图不能携带插件类型。
- Plugin A 和 Plugin B 能通过共享 Contract 通信。
- 卸载插件后 Shared Contract 仍可使用。
- 卸载插件后 Host 不再持有插件私有 `Type` 和 delegate。

## 22. 最终约束

任何需要在 Host、静态模块或多个插件之间发布和订阅的事件，其事件类型及完整对象图必须来自 Host 注册的共享 Contract 程序集，并由 Default AssemblyLoadContext 唯一加载。

插件私有事件类型只能在插件私有 EventBus 平面内使用，不得进入 Host 共享事件总线。
