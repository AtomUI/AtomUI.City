# AtomUI.City.Routing Journal and Reuse 设计

版本：v0.1
状态：初版草案
适用范围：NavigationJournal、Back/Forward、Replace/Reset、路由状态恢复、RouteReusePolicy、KeepAlive 和插件路由清理。

## 1. 定位

Journal 记录 NavigationScope 内的导航历史。Reuse 控制路由分支是否保留或缓存。

两者都必须服务桌面长期运行模型，避免无边界缓存和插件卸载失败。

## 2. NavigationJournal

每个 NavigationScope 拥有独立 Journal。

Journal 记录：

- Back stack。
- Current entry。
- Forward stack。

不跨 NavigationScope 共享。

## 3. JournalEntry

JournalEntry 只保存可恢复导航状态。

建议字段：

| 字段 | 职责 |
|---|---|
| `RouteId` | 目标路由。 |
| `Parameters` | 可序列化参数。 |
| `Query` | Query 参数。 |
| `Fragment` | Fragment。 |
| `RouteGraphVersion` | 创建时图版本。 |
| `ContributionId` | 来源贡献。 |
| `StateSnapshotKey` | 可选状态快照引用。 |
| `Title` | 可选标题快照。 |

JournalEntry 禁止保存：

- ViewModel。
- View。
- ServiceProvider。
- Delegate。
- Stream。
- 插件私有类型实例。

## 4. 导航模式

支持：

| 模式 | 说明 |
|---|---|
| `Push` | 新 entry 入栈。 |
| `Replace` | 替换当前 entry。 |
| `Reset` | 清空历史并设置当前 entry。 |
| `Skip` | 不记录历史。 |

默认普通导航使用 Push。

## 5. Back / Forward

Back 流程：

```text
Read previous JournalEntry
-> Validate entry
-> Navigate with RestoreState
-> Move current to forward stack
```

Forward 类似。

如果 entry 的 route contribution 已撤销：

- 默认跳过该 entry。
- 从 Journal 中清除。
- 记录诊断。
- 继续寻找下一个可用 entry。

## 6. 状态恢复

Journal 不直接保存 ViewModel 状态。

需要恢复时：

- RouteScope 状态通过 StateSnapshot 保存。
- JournalEntry 保存 snapshot key。
- 导航恢复时由 State 模块读取 snapshot。

不可序列化状态不进入 Journal。

## 7. Route Reuse

复用策略分两类：

1. 共同父路由保留。
2. 已离开分支缓存。

共同父路由保留是默认行为。例如从 `settings/profile` 到 `settings/security`，`settings` 布局路由可以保留。

已离开分支缓存默认关闭，必须显式启用。

## 8. RouteReusePolicy

建议策略：

| 策略 | 说明 |
|---|---|
| `DisposeOnLeave` | 默认，离开即释放。 |
| `KeepAliveInNavigationScope` | 在当前 NavigationScope 内缓存。 |
| `KeepAliveUntilMemoryPressure` | 可选，受容量和内存策略约束。 |
| `NeverReuse` | 参数相同也重新创建。 |

缓存必须有容量限制和诊断。

## 9. KeepAlive

KeepAlive 分支必须保留：

- RouteScope。
- ActivationScope 或可重新激活状态。
- ViewModel。
- Resolved data。
- StateScope。

KeepAlive 分支不能保留：

- 已取消 Operation。
- 插件卸载中的资源。
- 无边界后台任务。
- Presentation 不允许保留的 View。

Presentation 可以拒绝某些 View 保留，Routing 必须按结果降级为释放。

## 10. 参数变化

同一 RouteId 参数变化时，策略决定：

- 复用实例并更新 RouteContext。
- 重新运行 Resolver。
- 重建 ViewModel。

默认规则：

- RouteId 相同且参数相同：可保留。
- RouteId 相同但 path 参数变化：重新解析数据。
- RouteId 不同：按路由树 diff 处理。

## 11. 插件清理

插件停用时必须：

```text
Find Journal entries by ContributionId
-> Remove entries
-> Evict reuse cache branches
-> Dispose related RouteScopes
-> Clear snapshot references
```

不允许 Journal 或 Reuse cache 保留插件类型实例，否则插件 AssemblyLoadContext 无法卸载。

## 12. 诊断

必须记录：

- Journal push/replace/reset。
- Back/Forward 目标。
- Entry 无效原因。
- Reuse 命中。
- Reuse 驱逐。
- KeepAlive 容量。
- 插件 entry 清理数量。

## 13. 测试要求

测试必须覆盖：

- Push / Replace / Reset。
- Back / Forward。
- Skip history。
- 无效 entry 跳过。
- 共同父路由保留。
- DisposeOnLeave。
- KeepAlive cache 命中。
- 插件停用清理 Journal 和缓存。
- Journal 不保存不可序列化对象。
