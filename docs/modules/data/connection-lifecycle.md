# AtomUI.City.Data Connection Lifecycle 设计

版本：v0.1
状态：正式初版
适用范围：HTTP、gRPC channel、gRPC streaming、SignalR connection、连接 owner、重连、关闭和插件卸载。

## 1. 定位

连接生命周期必须显式声明。

HTTP 通常是单次 Operation；gRPC channel、gRPC streaming 和 SignalR connection 可能跨越多个请求或持续运行。Data 必须明确连接挂在哪个生命周期边界下。

## 2. 生命周期选项

连接 owner 可以是：

```text
Application
Window
Navigation
Route
Activation
Plugin
Manual
```

规则：

- 长连接不能默认挂 `ApplicationScope`。
- SignalR connection 必须声明 owner。
- gRPC channel 可以由 client factory 管理，但使用方必须声明关闭策略。
- Plugin owner 停止时必须关闭插件连接。
- Manual owner 需要调用方显式 dispose，并进入诊断。

## 3. HTTP

HTTP 使用 `HttpClientFactory` 管理底层 handler lifetime。

规则：

- 业务请求仍然是 OperationScope。
- Data 不手工长期持有裸 `HttpClientHandler`。
- named/typed client 配置通过 descriptor 或 DI 完成。

## 4. gRPC Channel

gRPC channel 生命周期可以长于单次 call。

规则：

- channel owner 必须明确。
- unary call 绑定 OperationScope。
- streaming call 绑定 OperationScope + SubscriptionScope。
- deadline 和 cancellation 必须传入 call options。
- channel fault 进入 connection diagnostics。

## 5. SignalR Connection

SignalR connection 是显式长连接。

规则：

- start/stop 必须受 owner scope 管理。
- reconnect 策略显式配置。
- 默认不假设自动重连。
- token 变化、用户切换、插件停用必须关闭或重建连接。
- server handler 订阅必须可撤销。

## 6. 关闭顺序

连接 owner 停止时：

```text
Stop accepting new operations
-> stop subscriptions
-> cancel streams
-> stop realtime connection
-> dispose connection resources
-> clear callbacks
-> emit diagnostics
```

插件连接必须保证没有 callback 持有插件私有类型。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| owner 已停止还创建连接 | 拒绝并记录诊断。 |
| reconnect 失败 | ReconnectFailed。 |
| stop 超时 | 进入 ErrorPolicy。 |
| 插件连接未释放 | 插件 UnloadPending。 |

## 8. 测试策略

测试必须覆盖：

- Route owner 停止关闭连接。
- Plugin owner 停止关闭连接。
- SignalR reconnect failed。
- gRPC stream owner 取消。
- Manual owner 泄漏诊断。
