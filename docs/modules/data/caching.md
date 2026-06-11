# AtomUI.City.Data Caching 设计

版本：v0.1
状态：正式初版
适用范围：request cache、response cache、snapshot cache、principal 隔离、插件缓存撤销和缓存诊断。

## 1. 定位

Data cache 用于减少重复请求和提升响应速度。

Cache 不是 State。Cache 是数据访问层优化；State 是应用状态表达。Data 不应把所有响应自动写入全局 State。

## 2. 缓存类型

| 类型 | 说明 |
|---|---|
| Request cache | 同一请求 key 的短期结果缓存。 |
| Response cache | HTTP response 或 transport response cache。 |
| Snapshot cache | streaming / realtime 的 latest snapshot。 |
| Entity cache | 可选高风险能力，第一版只定义扩展点。 |

## 3. 缓存 key

缓存 key 必须包含：

- DataClientId。
- Operation name。
- Request parameters hash。
- Principal revision。
- Auth scheme。
- Permission / capability revision。
- Plugin contribution id。
- Client version。
- Cache policy version。

用户 A 的缓存不能被用户 B 读到。

## 4. Streaming 和 SignalR

Streaming 和 SignalR 默认不缓存原始消息。

允许：

- latest snapshot。
- bounded buffer。
- explicit state projection。

不允许无限消息缓存。

## 5. 失效

缓存失效来源：

- Mutation success。
- Principal change。
- Permission / capability revision change。
- Plugin contribution revoked。
- Client version changed。
- Manual invalidation。
- TTL expired。

## 6. 插件缓存

插件 client 缓存必须带 PluginId 和 ContributionId。

插件停用时：

```text
Stop new plugin data operations
-> cancel running operations
-> revoke client descriptors
-> invalidate plugin cache entries
-> dispose cache handles
```

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| cache read failed | 记录诊断，继续请求 transport。 |
| cache write failed | 返回请求结果，记录诊断。 |
| cache key 缺少 principal | 拒绝缓存。 |
| 插件缓存撤销失败 | 聚合错误，继续撤销其他资源。 |

## 8. 测试策略

测试必须覆盖：

- cache hit / miss。
- principal 隔离。
- permission revision 失效。
- mutation 后失效。
- plugin cache revoke。
- streaming snapshot cache。
