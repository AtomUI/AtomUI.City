# AtomUI.City.Data State Integration 设计

版本：v0.1
状态：正式初版
适用范围：Data 与 State 的显式更新、状态投影、OperationScope、UI 线程和错误边界。

## 1. 定位

Data 负责异步请求和缓存策略。State 负责应用状态表达。

Data 不隐式写全局 State。请求完成后是否写 State 必须由 ViewModel、service、resolver 或显式 adapter 决定。

## 2. 默认链路

```text
Command / Data request
-> OperationScope
-> DataResult<T>
-> explicit decision
-> State writer
-> State notification
-> Presentation update
```

## 3. 显式写入

允许：

- ViewModel 调用 State writer。
- Resolver 初始化 RouteScope state。
- Mutation success 后写 State。
- Subscription mapper 投影到 State。

禁止：

- Data client 自动把所有响应写入全局 State。
- transport callback 直接写 ViewModel。
- State update 中执行 IO 或再次发 Data 请求。

## 4. Late Result

OperationScope 取消后不应继续提交状态更新。

规则：

- 提交 State 前检查 OperationScope。
- `LatestWins` 旧结果不能提交 State。
- Plugin contribution revoked 后不能提交 Host state。

## 5. Streaming 投影

Streaming / SignalR 消息可以显式投影到 State。

规则：

- 投影 mapper 绑定 SubscriptionScope。
- mapper 不能访问 UI。
- mapper 错误进入 diagnostics。
- backpressure drop 必须可诊断。

## 6. 错误策略

| 场景 | 默认处理 |
|---|---|
| State writer 拒绝 | DataResult 保留，State 写入失败进入诊断。 |
| Operation 已取消 | 抑制 State update。 |
| mapper 抛异常 | 记录错误，按 subscription policy。 |
| 插件未授权写 Host state | 拒绝写入。 |

## 7. 测试策略

测试必须覆盖：

- DataResult 显式写入 State。
- cancelled operation 不写 State。
- LatestWins 旧结果不写 State。
- SignalR message 投影。
- plugin state 写入授权。
