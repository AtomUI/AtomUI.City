# MVVM 测试设计

版本：v0.1
状态：正式初版
适用范围：ViewModel activation、Command、Interaction、Validation、property change 和 Presentation-free ViewModel 测试

## 1. 目标

MVVM 测试必须让 ViewModel 编程模型在无真实 UI 的环境中可验证。

## 2. MvvmTestKit

Testing 提供：

- ViewModel activation driver。
- command execution helper。
- async command completion helper。
- interaction handler fake。
- validation context fake。
- property change recorder。
- activation disposal assertion。

## 3. Activation 测试

必须覆盖：

- activate。
- deactivate。
- repeated activate。
- activation cancellation。
- activation error。
- owned disposable 释放。
- OperationScope 绑定。
- Presentation commit 后激活。

## 4. Command 测试

必须覆盖：

- can execute。
- execute success。
- execute failure。
- async execute。
- cancellation。
- concurrency policy。
- permission refresh。
- diagnostics。

## 5. Interaction 测试

必须覆盖：

- handler registration。
- handler invocation。
- missing handler。
- handler cancellation。
- handler disposal。
- plugin unload 后 handler 不可用。

## 6. Validation 测试

必须覆盖：

- rule success。
- rule failure。
- async validation。
- culture change 后错误文本刷新。
- property change 触发验证。
- validation diagnostics。

## 7. 测试要求

ViewModel 测试不应依赖真实 View。Presentation 相关行为通过 fake Presentation runtime 或平台集成测试补充。
