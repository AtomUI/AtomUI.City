# AtomUI.City.Data Transport 设计

版本：v0.1
状态：正式初版
适用范围：Data transport 抽象、request/response、streaming、realtime connection、transport metadata 和生命周期差异。

## 1. 定位

Transport 是 Data 与外部数据源交互的传输层抽象。

Data 第一版必须支持：

- HTTP request / response。
- gRPC unary 和 streaming。
- SignalR realtime connection。

Transport 只负责传输，不负责业务状态，不解释权限，不直接更新 UI。

## 2. Transport 分类

| 类型 | 说明 | 代表实现 |
|---|---|---|
| `IRequestResponseTransport` | 单次请求/响应。 | HTTP、gRPC unary、SignalR hub invoke。 |
| `IStreamingTransport` | 有开始和结束的 stream。 | gRPC server/client/bidi streaming。 |
| `IRealtimeConnectionTransport` | 长连接和实时推送。 | SignalR HubConnection。 |

Transport 分类影响生命周期、取消、错误映射、缓存和测试策略。

## 3. Request / Response

Request / Response transport 输出 `DataResult<T>`。

规则：

- 每次调用绑定 OperationScope。
- 必须接收 `CancellationToken`。
- 必须支持 timeout。
- 可以使用 retry、cache 和 error mapping。
- 结果提交前必须检查 parent scope。

## 4. Streaming

Streaming transport 输出 stream handle 或 async stream abstraction。

规则：

- stream 必须有 owner scope。
- stream 必须支持取消。
- stream item 回调不能直接访问 UI。
- stream 必须有 backpressure policy。
- stream 完成、取消和失败都要进入诊断。

## 5. Realtime Connection

Realtime connection transport 输出 `IDataConnection`。

规则：

- 连接生命周期必须显式声明。
- 连接状态变化必须可观察。
- reconnect 策略必须显式配置。
- 用户切换或插件停用必须关闭相关连接。
- server push message 进入 State 或 EventBus 必须通过显式 mapper。

## 6. Transport Metadata

Transport descriptor 应包含：

- Transport kind。
- Client id。
- Operation id。
- Auth scheme。
- Timeout / deadline。
- Retry policy。
- Cache policy。
- Streaming metadata。
- Connection lifetime。
- Plugin contribution。
- Serializer。

## 7. 错误映射

Transport 层错误必须映射成 DataError。

| Transport | 典型错误 |
|---|---|
| HTTP | status code、network error、timeout、serialization error。 |
| gRPC | status code、deadline exceeded、stream cancelled、metadata error。 |
| SignalR | connection closed、reconnect failed、hub invoke error、protocol error。 |

## 8. 测试策略

Testing 包应提供：

- Fake request/response transport。
- Fake streaming transport。
- Fake realtime connection transport。
- Controlled connection state。
- Controlled stream producer。
- Transport error injection。

测试必须覆盖三类 transport 的成功、失败、取消、超时和 lifecycle stop。
