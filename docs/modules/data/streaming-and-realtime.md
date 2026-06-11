# AtomUI.City.Data Streaming and Realtime 设计

版本：v0.1
状态：正式初版
适用范围：gRPC streaming、SignalR server push、subscription、backpressure、状态投影和 EventBus 边界。

## 1. 定位

Streaming 和 realtime 是 Data 的一等访问模式。

它们不是普通请求的循环版。它们需要 subscription 生命周期、backpressure、连接状态、取消、重连和消息投影策略。

## 2. gRPC Streaming

支持：

- Server streaming。
- Client streaming。
- Bidirectional streaming。

规则：

- server stream item 必须经过 Data subscription dispatcher。
- client stream 写入必须响应 cancellation。
- bidi stream 必须同时管理读取和写入取消。
- deadline / cancellation 必须进入 call options。
- stream completed 是正常状态，不是错误。

## 3. SignalR Realtime

支持：

- Hub connection start / stop。
- Hub method invoke。
- Server event subscription。
- Reconnect policy。
- Connection state observable。
- Token refresh / reconnect。

SignalR 不替代 EventBus：

```text
SignalR receives server message
-> Data realtime transport
-> explicit mapper
-> State update or EventBus publish
```

SignalR 是外部实时数据入口。EventBus 是应用内部模块通信机制。

## 4. Backpressure

Streaming 和 realtime 必须声明 backpressure policy。

| 策略 | 说明 |
|---|---|
| `Buffer` | 有界缓冲，满了按策略处理。 |
| `DropOldest` | 丢旧消息，保留最新。 |
| `DropNewest` | 丢新消息，保护消费者。 |
| `LatestOnly` | 状态型消息只保留最后一条。 |
| `BlockProducer` | 能反压时阻塞生产方，SignalR 多数场景不适合。 |

默认不允许无限缓冲。

## 5. SubscriptionScope

每个 streaming subscription 或 SignalR server handler 都必须绑定 Scope。

Scope 停止时：

- 停止接收新消息。
- 取消 stream。
- 释放 handler。
- 清理 buffer。
- 停止结果投递。

## 6. 状态投影

Streaming 和 SignalR 默认不缓存原始消息。

允许：

- latest snapshot。
- bounded buffer。
- 显式 State projection。
- 显式 EventBus publish。

禁止：

- 无限消息缓存。
- 隐式写全局 State。
- transport callback 直接改 ViewModel。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| stream cancel | StreamCancelled，不作为失败。 |
| stream completed | StreamCompleted。 |
| backpressure drop | 记录 drop diagnostics。 |
| handler 抛异常 | 记录错误，按 subscription error policy 处理。 |
| reconnect failed | ReconnectFailed。 |

## 8. 测试策略

测试必须覆盖：

- server streaming 正常完成。
- stream cancellation。
- SignalR server push。
- backpressure DropOldest。
- LatestOnly 只投递最新状态。
- subscription scope 停止后不再投递。
