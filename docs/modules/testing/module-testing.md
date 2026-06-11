# Module 测试设计

版本：v0.1
状态：正式初版
适用范围：模块定义、模块依赖、模块图、配置阶段、服务注册阶段、模块生命周期和插件模块

## 1. 目标

Module 测试用于验证模块作为应用组成单元的行为。模块测试不能通过真实应用启动掩盖模块依赖、顺序和贡献问题。

## 2. ModuleTestHost

`ModuleTestHost` 提供：

- 测试模块注册。
- 模块图构建。
- 拓扑排序断言。
- 模块生命周期阶段驱动。
- 配置阶段断言。
- 服务注册阶段断言。
- Contribution 记录。

## 3. 依赖图测试

必须覆盖：

- 显式依赖。
- 默认模块 Id。
- 重复模块。
- 缺失依赖。
- 循环依赖。
- 拓扑排序。
- 插件模块依赖。

## 4. 配置阶段测试

必须覆盖：

- PreConfigure。
- Configure。
- PostConfigure。
- Options validation。
- 插件配置隔离。
- 配置失败错误策略。

## 5. 服务注册测试

必须覆盖：

- 服务注册进入正确 ServiceCollection。
- 模块服务不提前构建 ServiceProvider。
- 插件模块不能修改 Host Root ServiceProvider。
- 自动服务注册清单生效。
- AOT 模式拒绝运行时扫描。

## 6. 生命周期测试

必须覆盖：

- 模块初始化顺序。
- 模块启动顺序。
- 模块停止顺序。
- 停止反向顺序。
- 失败回滚。
- 诊断记录。

## 7. 测试要求

每个模块功能点必须有单元测试。模块组合行为使用 ModuleTestHost 做 framework integration test。
