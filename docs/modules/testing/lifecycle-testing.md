# Lifecycle 测试设计

版本：v0.1
状态：正式初版
适用范围：Lifecycle Scope、Lifecycle pipeline、middleware、Operation、Lease、取消、释放和错误聚合

## 1. 目标

生命周期是 AtomUI.City 的运行骨架。Testing 必须能确定性验证 Scope、Operation、Lease 和 middleware 的顺序与释放。

## 2. LifecycleDriver

`LifecycleDriver` 负责驱动：

- Host start。
- Application start。
- Scope create。
- Scope stop。
- Operation run。
- Lease create/revoke。
- Middleware execution。
- Dispose。

## 3. Scope 断言

必须支持断言：

- Scope 创建顺序。
- Scope 父子关系。
- Scope 状态。
- Scope stop 顺序。
- Scope dispose 顺序。
- CancellationToken 是否触发。
- Stop 幂等。

## 4. Middleware 断言

必须支持：

- 执行顺序断言。
- 短路断言。
- 异常策略断言。
- cancellation 传递断言。
- diagnostics 断言。

## 5. Operation 测试

Operation 测试必须覆盖：

- 成功完成。
- 失败完成。
- 取消。
- owner scope stop 后自动取消。
- late result suppression。
- 诊断记录。

## 6. Lease 测试

Lease 测试必须覆盖：

- 创建。
- revoke。
- 反向撤销顺序。
- revoke 幂等。
- revoke 失败聚合。
- owner scope stop 后自动撤销。

## 7. 错误聚合

释放阶段不能因为单个错误阻断其他释放动作。

测试必须断言：

- 所有释放动作都被尝试。
- 错误被聚合。
- 错误上下文包含 phase 和 scope id。
- cancellation 不被当成普通失败。

## 8. 测试要求

必须覆盖：

- Scope tree。
- Middleware 顺序。
- Operation cancellation。
- Lease revoke。
- Stop 幂等。
- Dispose 错误聚合。
- Plugin unload 生命周期路径。
