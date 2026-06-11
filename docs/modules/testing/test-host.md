# TestHost 设计

版本：v0.1
状态：正式初版
适用范围：测试 Host、测试服务容器、模块组合、fake runtime 注入和 Host 生命周期驱动

## 1. 目标

`TestHost` 用于在测试中构造 AtomUI.City 运行时骨架。它应尽量接近真实 Host，但允许替换 UI、调度、数据传输、插件源和诊断输出。

## 2. 职责

`TestHost` 负责：

- 创建测试 ServiceProvider。
- 创建 `ApplicationScope`。
- 注入测试 Configuration。
- 注入测试 Module。
- 注入 fake dispatcher 和 deterministic scheduler。
- 创建 Lifecycle pipeline。
- 创建 Contribution registry。
- 收集 diagnostics。
- 按测试要求启用模块测试适配器。

`TestHost` 不负责：

- 启动真实 UI runtime。
- 访问真实用户插件目录。
- 访问真实远程服务。
- 绑定具体测试 runner。

## 3. Builder 模型

推荐 API 风格：

```csharp
var host = TestHost.CreateBuilder()
    .UseConfiguration(values => { })
    .UseModule<TestModule>()
    .UseRoutingTesting()
    .UseFakePresentation()
    .UsePluginTesting()
    .Build();
```

命名可以在实现阶段调整，但设计要求保持 builder 风格，符合 .NET 扩展方法习惯。

## 4. 默认服务

TestHost 默认包含：

- Core DI。
- Configuration。
- Options。
- Diagnostics collector。
- Lifecycle kernel。
- Fake UI dispatcher。
- Deterministic scheduler。
- Contribution test registry。

模块能力通过显式扩展方法启用。

## 5. 生命周期

TestHost 生命周期：

```text
Create builder
-> configure services
-> build service provider
-> create ApplicationScope
-> start lifecycle
-> run test
-> stop lifecycle
-> dispose scopes
-> assert no leaks
```

规则：

- `StopAsync` 必须幂等。
- `DisposeAsync` 必须释放所有 Scope。
- 默认在释放时断言没有未完成 Operation。
- 默认在释放时断言没有未撤销 Lease。

## 6. 模块组合

TestHost 支持：

- 单模块测试。
- 多模块组合测试。
- 插件模块测试。
- Host contract 测试。

模块图顺序应可断言。模块初始化失败应可注入并断言错误策略。

## 7. 测试数据隔离

每个 TestHost 必须有独立测试目录：

```text
<Temp>/AtomUI.City.Tests/<test-run-id>/
```

目录包含：

- configuration。
- plugin cache。
- plugin installed。
- generated manifests。
- diagnostics。

测试结束默认清理。失败时可按策略保留目录用于诊断。

## 8. 断言

TestHost 应提供：

- 生命周期状态断言。
- Scope 树断言。
- Diagnostics 断言。
- 未完成任务断言。
- 未释放资源断言。
- Contribution Lease 断言。

## 9. 测试要求

TestHost 自身必须测试：

- 默认 Host 创建。
- 配置注入。
- 模块注入。
- 生命周期 start/stop。
- stop 幂等。
- dispose 聚合错误。
- 测试目录隔离。
- fake runtime 替换真实 runtime。
