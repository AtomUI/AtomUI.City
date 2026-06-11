# AtomUI.City.Data Concurrency 设计

版本：v0.1
状态：正式初版
适用范围：Data operation 并发策略、请求去重、排队、取消旧请求、最新结果提交和 keyed serial。

## 1. 定位

Data operation 必须明确并发行为。

桌面应用中搜索、保存、自动刷新、导航预取、SignalR 重连和插件请求经常并发发生。如果没有统一策略，就会出现旧结果覆盖新结果、重复保存、重复刷新和不可诊断的竞态。

## 2. 并发策略

| 策略 | 说明 |
|---|---|
| `AllowConcurrent` | 默认允许并发。 |
| `DisallowConcurrent` | 正在执行时拒绝新请求。 |
| `Queue` | 排队顺序执行。 |
| `CancelPrevious` | 新请求取消旧请求。 |
| `LatestWins` | 允许并发，但只有最新结果可提交。 |
| `KeyedSerial` | 同一个 resource key 串行，不同 key 并行。 |

策略由 operation descriptor、client metadata 或调用方显式指定。

## 3. LatestWins

`LatestWins` 适合搜索、筛选、自动补全。

规则：

- 允许多个请求并发。
- 每次请求分配 monotonic sequence。
- 只有最新 sequence 的结果允许提交。
- 旧请求完成后结果被抑制。
- 旧请求可以选择不取消，但不能写状态。

## 4. CancelPrevious

`CancelPrevious` 适合最新请求完全替代旧请求的场景。

规则：

- 新请求到来时取消旧 OperationScope。
- 旧请求返回 Cancelled。
- 如果 transport 无法立即取消，返回后也必须 suppress result。

## 5. Queue

`Queue` 适合必须顺序执行的 mutation。

规则：

- 同一 operation key 下顺序执行。
- 队列绑定 owner scope。
- owner scope 停止时取消未执行项。
- 队列长度应有限制或可诊断。

## 6. KeyedSerial

`KeyedSerial` 适合按资源串行，例如同一 document 的保存。

规则：

- key 相同串行。
- key 不同可以并行。
- key 必须可诊断。
- key 不能包含敏感信息。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| DisallowConcurrent 冲突 | 返回 PolicyRejected。 |
| Queue owner 停止 | 取消队列项。 |
| LatestWins 旧结果返回 | suppress result。 |
| KeyedSerial key 无效 | DataResult failed。 |

## 8. 测试策略

测试必须覆盖：

- 并发请求允许。
- DisallowConcurrent 拒绝。
- Queue 顺序。
- CancelPrevious 取消旧请求。
- LatestWins 只提交最新结果。
- KeyedSerial 同 key 串行、不同 key 并行。
