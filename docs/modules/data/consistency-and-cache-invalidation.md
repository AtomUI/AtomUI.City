# AtomUI.City.Data Consistency and Cache Invalidation 设计

版本：v0.1
状态：正式初版
适用范围：query、mutation、subscription、一致性策略、idempotency、optimistic update、rollback 和缓存失效。

## 1. 定位

Data 必须区分查询、写入和订阅。

查询可以缓存和重试。写入需要一致性和幂等性约束。订阅是长期数据流，需要投影和失效策略。

## 2. Operation 类型

| 类型 | 说明 |
|---|---|
| Query | 可缓存、可重试。 |
| Mutation | 默认不自动重试，除非声明幂等。 |
| Subscription | 长期推送。 |
| Upload / Download | 有进度、可取消、可恢复。 |

## 3. Mutation

Mutation 规则：

- 默认不自动 retry。
- 可以声明 idempotency key。
- success 后可以触发 cache invalidation。
- 可以声明 affected cache keys。
- 可以显式触发 State update。
- conflict 必须返回 DataError。

## 4. Optimistic Update

Optimistic update 必须显式声明。

```text
Apply optimistic state
-> execute mutation
-> success: confirm
-> failure: rollback
```

规则：

- rollback 必须可执行。
- Operation cancelled 时按策略 rollback。
- 插件 mutation 不能修改未授权 Host state。

## 5. Subscription Consistency

Subscription 推送可以用于维护本地状态投影。

规则：

- 消息必须有顺序或版本策略。
- 乱序消息需要按 policy 处理。
- 缺失消息需要重新同步或标记 stale。
- 重连后是否 replay 由 transport 和 Host policy 决定。

## 6. Cache Invalidation

失效来源：

- mutation success。
- subscription message。
- manual invalidation。
- principal change。
- plugin contribution revoked。
- route leave。
- TTL expired。

失效动作必须进入诊断。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| mutation conflict | Conflict。 |
| optimistic rollback failed | 记录 ErrorPolicy，保留诊断。 |
| invalidation failed | 记录诊断，不吞掉 mutation result。 |
| subscription gap | 标记 stale，按策略重新同步。 |

## 8. 测试策略

测试必须覆盖：

- query cache。
- mutation 不自动 retry。
- idempotency key retry。
- optimistic success / rollback。
- mutation invalidates cache。
- subscription message invalidates cache。
- principal change invalidates cache。
