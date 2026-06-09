# AtomUI.City.Testing

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Testing` 负责测试 Host、测试 Dispatcher、生命周期驱动、状态断言、路由测试、EventBus 记录和 Presentation-free ViewModel 测试支持。

Testing 的目标是让应用可以测试框架编程模型本身，而不是只测试孤立工具类。

## 边界

Testing 负责：

- Test Host。
- Test Module。
- Fake Dispatcher。
- 生命周期驱动器。
- 路由测试工具。
- State Snapshot 断言。
- EventBus 测试记录器。
- Command 执行测试工具。
- Presentation-free ViewModel 测试支持。

Testing 不负责：

- 具体应用业务测试。
- UI 自动化测试框架封装。
- 性能测试平台。

## 后续拆分

- `test-host.md`
- `fake-dispatcher.md`
- `lifecycle-testing.md`
- `routing-testing.md`
- `state-testing.md`
