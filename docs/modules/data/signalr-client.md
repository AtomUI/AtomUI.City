# AtomUI.City.Data SignalR Client 设计

版本：v0.1
状态：正式初版
适用范围：SignalR connection、hub method invoke、server push、subscription、reconnect、access token provider 和插件卸载。

## 1. 定位

SignalR 是 Data 第一批一等 realtime transport。

SignalR client 负责实时连接、服务端推送、hub method invoke 和双向消息。它是外部实时数据入口，不替代应用内部 EventBus。

## 2. Connection

SignalR connection 必须由 `IDataConnectionManager` 或等价能力管理。

规则：

- connection owner 必须显式声明。
- start/stop 必须可诊断。
- reconnect 策略必须显式配置。
- 默认不假设自动重连。
- connection state 必须可观察。
- owner scope 停止时必须 stop connection。

## 3. Hub Invoke

Hub method invoke 是 request/response 操作。

```text
Hub invoke
-> OperationScope
-> Security credential
-> SignalR transport
-> DataResult<T>
```

invoke 失败必须映射为 DataError。

## 4. Server Push

Server push 必须通过 subscription 注册。

规则：

- handler 绑定 SubscriptionScope。
- handler 不能直接访问 UI。
- handler 不能无限缓冲消息。
- handler 必须可撤销。
- 插件 handler 不能被 Host 静态缓存持有。

## 5. Token 和重连

SignalR access token 通过 Security 提供。

规则：

- AccessTokenProvider 每次需要 token 时从 Security 获取。
- token 过期时可触发 refresh。
- 用户切换账号时旧 connection 必须关闭并重建。
- reconnect 期间是否排队消息由 policy 决定。
- reconnect 失败返回 ReconnectFailed。

## 6. SignalR 与 EventBus

边界：

```text
SignalR receives server message
-> Data realtime transport
-> explicit mapper
-> State update or EventBus publish
```

SignalR 不直接广播到所有模块。是否转为 EventBus 事件必须显式声明。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| connection closed | ConnectionClosed。 |
| reconnect failed | ReconnectFailed。 |
| hub invoke failed | TransportError 或 ServerError。 |
| token unavailable | AuthenticationRequired。 |
| owner stopped | stop connection and subscriptions。 |

## 8. 测试策略

测试必须覆盖：

- connection start / stop。
- hub invoke success / failure。
- server push delivery。
- subscription disposal。
- reconnect failed。
- token refresh。
- plugin unload stops connection。
