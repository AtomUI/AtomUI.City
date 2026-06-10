# AtomUI.City.Routing Plugin 集成设计

版本：v0.1
状态：初版草案
适用范围：插件路由贡献、扩展点、动态启用和停用、活动路由关闭、Journal 清理、缓存驱逐和卸载安全。

## 1. 定位

插件路由是 Routing 主设计的一部分，不是附加能力。

桌面应用长期运行期间，插件可能启用、停用、更新或卸载。Routing 必须保证插件路由加入和撤销不会破坏 Host 路由图，也不会留下阻止插件卸载的引用。

## 2. 贡献方式

插件通过 PluginSystem 提交 RouteContribution。

流程：

```text
Plugin load
-> Build plugin module graph
-> Read generated route manifest
-> Create RouteContributionRequest
-> Validate extension points
-> Register with RouteRegistry
-> Receive ContributionLease
```

插件不能直接写 RouteRegistry 内部结构。

## 3. 挂载边界

插件路由只能挂载到：

- Host 显式开放的 RouteExtensionPoint。
- Host 配置允许的插件路由根。
- 插件自身内部父路由。

插件不能静默覆盖 Host 路由，也不能挂载到未开放的内部页面节点。

## 4. Contribution 信息

每个插件 RouteDescriptor 必须记录：

- PluginId。
- Plugin version。
- ModuleId。
- ContributionId。
- ContributionLease。
- Plugin ServiceContext。
- Plugin load context id。
- Route manifest version。

这些信息用于诊断、RouteScope 创建、Journal 清理和卸载反查。

## 5. 服务边界

插件路由 ViewModel、Guard、Resolver、Middleware 从插件 ServiceProvider 创建。

插件可以访问 Host 能力的唯一方式是 Host 暴露的共享 contract。

禁止：

- 插件修改 Host Root ServiceProvider。
- Host 静态缓存保存插件私有类型实例。
- 插件路由参数跨边界暴露插件私有类型。
- 插件 Resolver 返回插件私有类型给 Host 长期持有。

## 6. 启用

插件路由启用步骤：

```text
Validate manifest
-> Validate extension point
-> Validate route ids and path conflicts
-> Validate guard/resolver/viewmodel contracts
-> Build candidate RouteGraphSnapshot
-> Publish snapshot
-> Activate ContributionLease
```

启用失败时：

- 不发布新 snapshot。
- 不影响当前 Host 路由图。
- 插件进入禁用或 Faulted 状态。
- 输出诊断。

## 7. 停用

插件停用步骤：

```text
Mark contribution draining
-> Reject new navigation to plugin routes
-> Cancel in-flight navigation entering plugin routes
-> Request active plugin routes to close or redirect
-> Force close after policy timeout if required
-> Remove plugin journal entries
-> Evict plugin reuse cache
-> Revoke route ContributionLease
-> Publish new RouteGraphSnapshot
-> Dispose plugin RouteScopes
```

停用必须和 PluginSystem 生命周期协同。

## 8. 活动路由处理

如果当前页面来自插件，停用策略可配置：

| 策略 | 说明 |
|---|---|
| `RedirectToFallback` | 跳转到 Host fallback route。 |
| `CloseWindow` | 关闭对应 WindowScope。 |
| `RejectUnload` | 阻止插件停用。 |
| `ForceClose` | 超时后强制关闭。 |

默认建议 `RedirectToFallback`。业务应用可以为关键插件选择 `RejectUnload`。

## 9. 运行中导航处理

插件停用时：

- 进入插件路由的 transaction 被取消。
- 离开插件路由的 transaction 可以继续，但不得调用已停用插件新逻辑。
- Commit 中 transaction 等待 Commit 结束后再执行停用清理。
- 已捕获旧 snapshot 的 transaction 如果目标 contribution 已 draining，应返回 ContributionRevoked。

## 10. Journal 和 Reuse 清理

必须按 ContributionId 清理：

- Back stack。
- Forward stack。
- Current entry。
- KeepAlive cache。
- RouteScope registry。
- State snapshot 引用。

清理后不能再通过 Back/Forward 回到插件路由。

## 11. 扩展点能力

RouteExtensionPoint 可以限制插件：

- 允许的 Outlet。
- 允许的子路由类型。
- 最大贡献数量。
- 默认排序。
- 所需权限。
- 是否允许 Deep Link。
- 是否允许 KeepAlive。

RouteRegistry 必须校验这些限制。

## 12. 跨插件边界类型

跨插件边界的事件、参数、Resolver 数据和公共 contract 必须位于 Host 共享 contract 程序集。

原因：

- Host 可以稳定加载共享类型。
- 插件卸载后 Host 不持有插件加载上下文中的 Type。
- 不同插件可以通过共享 contract 互操作。
- 诊断和序列化不依赖插件私有程序集。

插件私有类型只能在插件内部 RouteScope、ViewModel、Resolver 或服务中使用。

## 13. 错误策略

| 场景 | 默认处理 |
|---|---|
| 插件 route manifest 无效 | 插件贡献失败。 |
| ExtensionPoint 不存在 | 插件贡献失败。 |
| 路径冲突 | 插件贡献失败。 |
| 活动路由拒绝关闭 | 按 Host 策略处理。 |
| Journal 清理失败 | 记录错误，继续清理其他 entry。 |
| RouteScope 释放失败 | 聚合诊断，阻止完成卸载。 |

## 14. 诊断

必须记录：

- PluginId。
- ContributionId。
- RouteGraph version。
- 贡献路由数量。
- 扩展点目标。
- 活动 RouteScope 数量。
- 取消 transaction 数量。
- 清理 Journal entry 数量。
- 驱逐 cache 数量。
- 卸载阻塞引用。

## 15. 测试要求

测试必须覆盖：

- 插件贡献路由。
- 插件挂载未开放扩展点失败。
- 插件路径冲突失败。
- 插件停用阻止新导航。
- 插件停用取消运行中导航。
- 活动插件路由 redirect fallback。
- Journal 清理。
- KeepAlive cache 驱逐。
- Host 不保留插件私有类型实例。
