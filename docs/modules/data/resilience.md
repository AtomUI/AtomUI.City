# AtomUI.City.Data Resilience 设计

版本：v0.1
状态：正式初版
适用范围：timeout、retry、circuit breaker、fallback、rate limit、mutation 重试约束和重试诊断。

## 1. 定位

Resilience 负责提高 Data operation 对网络抖动、服务暂时不可用和超时的容错能力。

Data 可以使用 Polly 作为策略实现，但 Data API 不应把 Polly 类型扩散到 ViewModel、Routing 或 State。

## 2. 策略类型

第一版支持：

- Timeout。
- Retry。
- Circuit breaker。
- Fallback。
- Rate limit。

策略由 operation descriptor、client metadata 或 Host 配置提供。

## 3. Timeout / Deadline

规则：

- HTTP timeout 映射为 Data timeout。
- gRPC timeout 映射为 deadline。
- SignalR hub invoke timeout 映射为 operation timeout。
- Streaming 和 SignalR connection 不能只有总 timeout，还需要 idle timeout 或 keepalive 诊断。

## 4. Retry

默认规则：

- Query 可以按策略 retry。
- Mutation 默认不自动 retry。
- 取消不 retry。
- 403 不 retry。
- 401 refresh 成功后最多按策略重试一次。
- streaming item handler 不按普通 request retry。

Mutation 只有声明幂等或提供 idempotency key 时才允许自动 retry。

## 5. Circuit Breaker

Circuit breaker 绑定 client 或 operation。

规则：

- breaker 状态进入 diagnostics。
- breaker open 时返回 PolicyRejected 或 ServiceUnavailable。
- 插件 client 的 breaker 随插件 contribution 撤销。

## 6. Fallback

Fallback 必须显式声明。

允许：

- 返回本地缓存。
- 返回降级数据。
- 返回默认空结果。

禁止：

- 隐式吞掉认证失败。
- 隐式吞掉权限不足。
- 隐式写 State。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| timeout | Timeout 或 DeadlineExceeded。 |
| retry exhausted | 保留最后错误并记录 attempts。 |
| circuit open | PolicyRejected / ServiceUnavailable。 |
| fallback failed | 返回原始错误和 fallback 诊断。 |

## 8. 测试策略

测试必须覆盖：

- query retry。
- mutation 默认不 retry。
- idempotent mutation retry。
- timeout。
- circuit open。
- fallback cache。
- retry attempts diagnostics。
