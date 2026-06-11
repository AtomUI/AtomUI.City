# AtomUI.City.Routing Route Graph 设计

版本：v0.1
状态：正式初版
适用范围：RouteDescriptor、RouteRegistry、RouteGraphSnapshot、路由贡献、优先级、冲突检测和插件动态变更。

## 1. 定位

Route Graph 表示应用当前可导航结构。

它不是简单列表，而是一棵由 Host、模块和插件共同贡献的不可变快照。导航匹配、Deep Link 解析、扩展点挂载、插件撤销和诊断都依赖 Route Graph。

## 2. 输入来源

Route Graph 输入来自：

- 应用启动期静态模块。
- 模块 Route Map 生成的 manifest。
- 插件 Route Map 生成的 manifest。
- Host 显式开放的 RouteExtensionPoint。
- Redirect route。

所有输入都必须转换为 `RouteContribution`，再进入 `RouteRegistry`。

## 3. RouteDescriptor

`RouteDescriptor` 是运行时消费的路由描述。

建议字段：

| 字段 | 职责 |
|---|---|
| `RouteId` | 稳定身份。 |
| `Template` | 路径模板。 |
| `ParentRouteId` | 父路由。 |
| `OutletName` | 目标 Outlet。 |
| `Kind` | Route、Layout、Index、Group、Redirect、ExtensionPoint。 |
| `ViewModelTarget` | ViewModel 目标描述。 |
| `Parameters` | 参数绑定描述。 |
| `Guards` | Guard descriptor。 |
| `Resolvers` | Resolver descriptor。 |
| `Middleware` | Middleware descriptor。 |
| `Metadata` | 标题、本地化 key、排序、能力等元数据。 |
| `Contribution` | 来源贡献。 |
| `ReusePolicy` | 复用策略。 |

RouteDescriptor 必须不可变。

## 4. RouteRegistry

`RouteRegistry` 负责接收贡献并发布快照。

流程：

```text
Accept RouteContribution
-> Validate contribution
-> Merge with active contributions
-> Build RouteGraphSnapshot
-> Publish snapshot
-> Return ContributionLease
```

规则：

- Registry 不能直接持有插件私有实例。
- Registry 只能保存 descriptor 和 service context 引用。
- ContributionLease 撤销时必须重建快照。
- 快照发布必须原子化。
- 快照版本单调递增。

## 5. RouteGraphSnapshot

`RouteGraphSnapshot` 是不可变结构。

应包含：

- Snapshot id。
- Version。
- RouteDescriptor collection。
- RouteId 索引。
- Parent/children 索引。
- ExtensionPoint 索引。
- Path matcher。
- Redirect 索引。
- Contribution 索引。
- Diagnostics summary。

导航开始时捕获一个 snapshot。本次导航不得访问全局 mutable graph。

## 6. 层级规则

父子关系必须显式。

规则：

- ParentRouteId 必须存在，除非是 root route。
- IndexRoute 必须有 Parent。
- IndexRoute 不能有路径模板。
- RouteGroup 可以没有 ViewModelTarget。
- LayoutRoute 可以有 ViewModelTarget 和子 Outlet。
- RedirectRoute 不能有 ViewModelTarget。
- ExtensionPoint 不能直接被导航进入。
- 插件路由只能挂到 ExtensionPoint 或 Host 显式允许的父节点。

## 7. Path 匹配

Path Template 兼容 ASP.NET Core 10 Route Template 的主要语义。

匹配优先级建议：

1. Literal segment。
2. Constrained parameter。
3. Parameter。
4. Optional parameter。
5. Catch-all。

同级路由如果优先级相同且可能匹配同一路径，Source Generator 或 RouteRegistry 必须报冲突。

运行时不做模糊选择。

## 8. ExtensionPoint

扩展点是 Host 或模块开放给后续模块和插件的挂载位置。

ExtensionPoint 应声明：

- ExtensionPoint id。
- 所属 route。
- 允许 Outlet。
- 允许贡献类型。
- 默认排序规则。
- 能力要求。
- 是否允许插件贡献。

插件贡献到扩展点时，RouteRegistry 必须校验这些规则。

## 9. Route Metadata

Route Metadata 只表达框架级导航信息，不表达业务流程。

推荐元数据：

- Title key。
- Icon key。
- Order。
- Required capability。
- Requires authentication。
- Journal policy。
- Reuse policy。
- Preload policy。
- Diagnostics tags。

权限检查由 Security 和 Guard 承接，Routing 只保存需要交给 Guard 的 metadata。

## 10. Contribution 归属

每个 descriptor 必须记录来源。

```text
RouteDescriptor
  ContributionId
  ModuleId
  PluginId?
  ServiceContext
  Lease
```

用途：

- 插件停用时反查路由。
- RouteScope 创建时选择服务来源。
- 诊断显示来源。
- Journal 清理插件路由。
- 缓存驱逐插件 ViewModel。

## 11. 冲突检测

必须检测：

- RouteId 重复。
- ExtensionPoint id 重复。
- 同级路径冲突。
- Parent 不存在。
- Redirect 目标不存在。
- 静态 Redirect 循环。
- 插件挂载未开放父节点。
- Outlet 名称非法。
- 参数约束不兼容。
- ViewModelTarget 缺失。

冲突默认阻止贡献生效。插件贡献冲突不应影响 Host 已运行路由图。

## 12. 快照更新策略

静态模块启动期贡献失败，默认视为应用启动失败。

插件贡献失败，默认只禁用当前插件贡献并记录诊断。

快照更新顺序：

```text
Build candidate graph
-> Validate candidate graph
-> Atomically swap snapshot
-> Notify observers
-> Store old snapshot for diagnostics window
```

旧 snapshot 可能被正在执行的导航持有。Registry 不能提前释放旧 snapshot 引用的必要 descriptor。

## 13. 测试要求

测试必须覆盖：

- 静态 route graph 构建。
- Parent/children 索引。
- Path 匹配优先级。
- 重复 RouteId 诊断。
- 路径冲突诊断。
- ExtensionPoint 挂载规则。
- 插件贡献和撤销。
- Snapshot version 单调递增。
- 正在导航时 graph 更新不影响当前事务。
