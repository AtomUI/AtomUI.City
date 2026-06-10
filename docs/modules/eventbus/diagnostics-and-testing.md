# AtomUI.City.EventBus Diagnostics and Testing 设计

版本：v0.1
状态：初版草案
适用范围：事件诊断上下文、结构化记录、错误策略、指标、事件链、测试工具、确定性调度、插件卸载断言和性能验证。

## 1. 定位

EventBus 是高度解耦的系统。发布方通常不知道订阅方，订阅方也可能位于另一个模块、线程或插件中。如果缺少结构化诊断，事件丢失、顺序异常、handler 阻塞和插件卸载失败会很难定位。

EventBus 诊断必须回答：

- 谁发布了事件。
- 事件进入了哪个 channel。
- 捕获了哪些订阅。
- 每个 handler 在哪个执行目标运行。
- 是否排队、丢弃、取消或超时。
- 哪个插件或 Scope 持有订阅。
- 当前为什么无法停止或卸载。

## 2. DiagnosticContext 集成

EventBus 复用 Core `DiagnosticContext`，并增加事件维度：

- EventId。
- EventContractId。
- Channel。
- PartitionKey hash。
- CorrelationId。
- CausationId。
- PublishDepth。
- PublisherModuleId。
- PublisherPluginId。
- PublisherScopeId。
- SubscriptionId。
- HandlerTypeId。
- SubscriberModuleId。
- SubscriberPluginId。
- DispatchTarget。
- QueueWaitDuration。
- HandlerDuration。
- DeliveryResult。

不应默认记录敏感 partition key 原文。

## 3. EventId、CorrelationId 与 CausationId

每次发布生成唯一 `EventId`。

规则：

- 外部调用没有 CorrelationId 时创建新值。
- Handler 发布子事件时继承 CorrelationId。
- 子事件的 CausationId 指向父 EventId。
- PublishDepth 加一。
- `PostAsync` 仍保留 correlation/causation 关系。

事件链：

```text
Command operation
-> Event A
   -> Handler A1
      -> Event B
   -> Handler A2
```

诊断系统可以通过 CorrelationId 重建事件因果链。

## 4. Diagnostic Record

建议事件总线产生结构化 record：

| Record | 说明 |
|---|---|
| `EventPublished` | 发布开始和 publisher 信息。 |
| `EventAccepted` | PostAsync 事件进入 queue。 |
| `EventRejected` | Contract、capability、queue 或 lifecycle 拒绝。 |
| `EventDeliveryStarted` | Handler 开始。 |
| `EventDeliveryCompleted` | Handler 成功完成。 |
| `EventDeliveryFailed` | Handler 失败。 |
| `EventDeliveryCancelled` | Handler 取消。 |
| `EventDropped` | 背压策略丢弃。 |
| `EventSubscriptionAdded` | 新订阅进入 snapshot。 |
| `EventSubscriptionQuiescing` | 订阅停止接收新投递。 |
| `EventSubscriptionDisposed` | 订阅完全释放。 |
| `EventChannelBackpressure` | Queue 达到阈值。 |
| `EventPluginDrainTimedOut` | 插件 handler 无法在时限内结束。 |

## 5. Payload 记录

默认不记录完整事件 payload。

原因：

- 可能包含敏感信息。
- 可能很大。
- 可能包含插件私有对象。
- 长期保存 payload 会阻止插件卸载。
- `ToString()` 可能执行用户代码。

默认只记录：

- EventContractId。
- EventId。
- Schema version。
- Payload size estimate。
- 明确标记为安全的摘要字段。

Payload diagnostics 必须 opt-in，并通过 contract 提供的稳定 formatter 生成 Host-owned snapshot。

## 6. Exception 记录

静态模块异常可以进入 Core diagnostic sink。

插件异常：

- 在当前调用链中可用于错误策略。
- 长期缓冲只保存稳定异常摘要。
- 不长期保存插件 Exception 实例。
- 不保存插件 stack object references。

稳定摘要包括：

- Error code。
- Exception type name string。
- Message。
- Sanitized stack trace string。
- PluginId。
- EventId。
- SubscriptionId。

## 7. 错误策略

EventBus 错误策略：

| 策略 | 行为 |
|---|---|
| `ContinueAndReport` | 继续独立 handler，聚合失败。默认。 |
| `StopPublication` | 不再开始剩余 delivery。 |
| `FailPublisher` | EventPublishResult 为 failed。 |
| `DisableSubscription` | 订阅进入 quiescing 并移除。 |

错误策略分层：

- Global default。
- Event contract。
- Channel。
- Subscription。
- Plugin policy。

更具体策略可以收紧错误处理，但插件不能自行把 Host 系统错误降级为忽略。

## 8. 连续失败

EventBus 可以维护订阅健康状态：

- ConsecutiveFailureCount。
- LastFailureTime。
- LastSuccessTime。
- DisabledReason。

达到阈值时：

- 记录 health diagnostic。
- 根据策略禁用订阅。
- 插件 handler 可以触发插件降级。

失败计数必须有恢复窗口，避免一次历史错误永久污染订阅状态。

## 9. 指标

建议指标：

- Publications total。
- Publications by contract/channel。
- Active subscriptions。
- Queue depth。
- Queue capacity utilization。
- Dropped/rejected events。
- Handler duration。
- Queue wait duration。
- Handler failures。
- Handler cancellations。
- In-flight handlers。
- Plugin drain duration。
- Snapshot replacements。

指标标签必须受控，不能把 EventId、用户 id 或任意 partition key 作为高基数标签。

## 10. 日志级别

建议：

| 场景 | 级别 |
|---|---|
| 普通发布完成 | Trace/Debug。 |
| Queue 高水位 | Warning。 |
| Event rejected | Warning 或 Error，取决于原因。 |
| Handler failed | Error。 |
| Handler cancelled | Debug。 |
| Event dropped | Warning。 |
| Subscription disabled | Warning/Error。 |
| Plugin drain timeout | Error。 |
| Shared Contract 冲突 | Error/Fatal。 |

高频事件不能默认逐条输出 Information 日志。

## 11. 诊断缓冲

桌面应用现场排查可以使用有界内存诊断缓冲。

规则：

- 缓冲必须有容量上限。
- 保存稳定 record，不保存 handler 和 payload 对象。
- 支持按 CorrelationId 查询。
- 支持按 PluginId、EventContractId、SubscriptionId 查询。
- 插件卸载前不需要清空共享稳定摘要，但必须清除插件对象引用。

## 12. Testing 包能力

`AtomUI.City.Testing` 应提供：

| 工具 | 职责 |
|---|---|
| `TestEventBus` | 使用确定性运行时的 EventBus。 |
| `EventRecorder` | 记录发布、投递和结果。 |
| `TestEventPublisher` | 驱动事件发布。 |
| `TestEventSubscription` | 控制 handler 完成、失败和取消。 |
| `DeterministicEventDispatcher` | 手动推进 dispatch queue。 |
| `EventBusAssertions` | 顺序、线程目标、错误和生命周期断言。 |
| `PluginEventBusProbe` | 检查插件残留订阅、queue 和 handler。 |

## 13. EventRecorder

EventRecorder 记录稳定事件事实：

- EventId。
- ContractId。
- Channel。
- Publish sequence。
- SubscriptionId。
- DispatchTarget。
- Start/end sequence。
- Result。

默认不记录完整 payload。测试可以显式提供安全 projector。

## 14. 确定性调度

测试不能依赖真实线程和 `Task.Delay` 猜测时序。

Deterministic dispatcher 应支持：

- 查看待执行队列。
- 执行下一项。
- 执行指定 channel。
- 执行指定 partition。
- 执行到 idle。
- 模拟 UI dispatcher。
- 模拟 handler 阻塞。
- 模拟 cancellation。
- 模拟 timeout。

这样可以稳定断言多线程顺序。

## 15. 顺序测试

必须覆盖：

- Serialized channel 接受顺序。
- 同一 subscription 不重入。
- Partition 内顺序。
- Partition 间并行。
- Concurrent 最大并发度。
- UI 和 Background handler 的独立完成顺序。
- 注册/撤销与发布竞争。
- Quiescing 后没有新 handler 开始。

测试应断言框架承诺的顺序，不应断言文档明确不保证的跨订阅完成顺序。

## 16. 背压测试

每种策略都需要：

- Queue 恰好满。
- Queue 超过容量。
- 发布方取消等待。
- EventBus shutdown。
- Plugin deactivation。

断言：

- Wait 确实等待且可取消。
- Reject 返回明确结果。
- DropOldest 记录被丢弃 EventId。
- DropNewest 拒绝当前事件。
- CoalesceLatest 只保留指定 key 的最新事件。

## 17. 错误测试

需要覆盖：

- Handler 创建失败。
- Handler 同步抛出。
- Handler 异步失败。
- Handler timeout。
- Handler dispose 失败。
- Dispatcher 拒绝。
- 多 handler 错误聚合。
- ContinueAndReport。
- StopPublication。
- FailPublisher。
- DisableSubscription。

## 18. Lifecycle 测试

需要覆盖：

- ApplicationScope 停止。
- WindowScope 停止。
- RouteScope 离开。
- ActivationScope 停用。
- EventBus shutdown。
- Subscription dispose 幂等。
- StopAsync drain。
- Shutdown timeout。

所有测试必须断言：

- CancellationToken 已触发。
- Queue 已停止接受新事件。
- Handler 已释放。
- Snapshot 不再包含订阅。

## 19. Plugin 测试

需要覆盖：

- Shared Contract 由 Default ALC 加载。
- Plugin Private Plane 隔离。
- Capability 拒绝。
- Handler Contribution 创建 Lease。
- 插件停用建立 quiescing barrier。
- 旧 snapshot 无法启动新 handler。
- In-flight handler drain。
- Queue 清理。
- Plugin private descriptor 清理。
- Diagnostic buffer 不持有插件对象。
- AssemblyLoadContext 可以被 GC 回收。
- UnloadPending 输出具体 EventBus 残留。

## 20. Contract 测试

需要覆盖：

- Contract Id 唯一。
- Schema fingerprint 稳定。
- Compatible version。
- Breaking version。
- 私有类型对象图检测。
- `object` payload 风险诊断。
- Plugin-to-plugin 共享 Contract。

Generator golden tests 应验证 manifest 和 generated invoker 的确定性。

## 21. 性能测试

Benchmark 与 correctness test 分开。

Benchmark 场景：

- 0/1/10/100 subscriptions。
- 同步完成 ValueTask。
- 异步 handler。
- Serialized/Partitioned/Concurrent。
- Diagnostics off/on。
- Snapshot 频繁更新。
- Plugin 批量撤销。

性能回归不能只看吞吐，还要检查：

- Allocations。
- Tail latency。
- Queue growth。
- Drain time。
- Cancellation responsiveness。

## 22. 测试完成标准

EventBus 开始实现前，测试设计必须能覆盖：

- 公共 API contract。
- 线程和顺序保证。
- 所有背压策略。
- Scope 生命周期。
- Plugin unload。
- AOT generated registration。
- 结构化诊断。
- 发布热路径 benchmark。
