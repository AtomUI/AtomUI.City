# 诊断和断言设计

版本：v0.1
状态：正式初版
适用范围：诊断收集、错误码断言、生命周期断言、线程断言、Contribution 断言和泄漏断言

## 1. 目标

Testing 必须提供统一断言工具，让测试能够断言行为、诊断、错误策略和资源释放。

## 2. DiagnosticsCollector

测试诊断收集器记录：

- event id。
- phase。
- scope id。
- operation id。
- module id。
- plugin id。
- contribution id。
- error code。
- exception。
- policy result。

## 3. 断言类型

| 断言 | 用途 |
|---|---|
| `LifecycleAssertions` | Scope、Operation、middleware、dispose。 |
| `ContributionAssertions` | Contribution、Lease、冲突、撤销。 |
| `ThreadingAssertions` | Dispatcher target、scheduler、未完成任务。 |
| `DiagnosticsAssertions` | 诊断事件、错误码、上下文。 |
| `PluginUnloadAssertions` | 插件卸载、ALC、残留引用。 |
| `StateAssertions` | 状态版本、通知、snapshot。 |
| `EventBusAssertions` | 发布、订阅、顺序、错误。 |
| `RoutingAssertions` | 匹配、导航事务、回滚。 |

## 4. 错误码断言

承诺错误码的功能必须在测试中断言错误码。

断言内容：

- error code。
- phase。
- source。
- context ids。
- policy result。

只断言异常类型不够。

## 5. 泄漏断言

涉及生命周期和插件的测试必须断言：

- 无 active operation。
- 无 active lease。
- 无 active subscription。
- 无 dispatcher callback。
- 无 timer。
- 无 Data connection。
- 插件加载上下文可释放，如果适用。

## 6. 快照和记录

Testing 可以提供结构化 snapshot，用于断言复杂状态。

规则：

- Snapshot 必须稳定。
- Snapshot 不包含不可预测时间戳。
- Snapshot 不包含绝对临时路径，除非测试明确断言路径。
- Snapshot 不包含敏感信息。

## 7. 测试要求

断言工具自身必须有单元测试，覆盖成功断言、失败断言和错误消息质量。
