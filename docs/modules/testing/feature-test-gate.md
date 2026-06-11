# 功能点测试门禁

版本：v0.1
状态：正式初版
适用范围：所有模块功能点、公共 API、生命周期行为、构建行为和 CLI 行为

## 1. 目标

功能点测试门禁用于确保 AtomUI.City 的每个功能点在进入实现和完成时都有明确测试依据。

规则很简单：

```text
No test design, no implementation.
No unit test, no done.
```

## 2. 功能点定义

功能点包括但不限于：

- 公共 API。
- 生命周期阶段。
- Middleware 行为。
- Options 和 Configuration 行为。
- Source generator 输出。
- Analyzer diagnostics。
- Contribution 和 Lease 行为。
- 插件安装、加载、启用、停用、卸载、更新、回滚。
- 路由匹配、导航事务、Guard、Resolver、Journal。
- ViewModel activation、Command、Interaction、Validation。
- State 读写、订阅、computed、snapshot。
- EventBus 发布、订阅、调度、背压、错误。
- Data 请求、连接、取消、缓存、认证。
- Localization 查找、fallback、懒加载、culture 切换。
- Security 认证、授权、权限刷新。
- CLI 命令。
- Build/MSBuild target。
- Template 生成行为。

## 3. 单元测试要求

每个功能点必须有单元测试。

单元测试必须覆盖：

- 成功路径。
- 失败路径。
- 边界条件。
- 取消、释放或清理路径，如果功能点涉及生命周期。
- 诊断事件或错误码，如果功能点承诺诊断。

对于状态、事件、线程、插件和生命周期相关功能，还必须覆盖：

- 订阅释放。
- Scope stop 后不再执行。
- Operation cancellation。
- Lease revoke。
- 无泄漏断言。

## 4. 替代测试例外

确实无法通过单元测试覆盖的功能点，必须使用替代测试。

可接受替代类型：

| 替代类型 | 适用场景 |
|---|---|
| Contract test | 公共 contract、跨模块 DTO、事件 contract。 |
| Framework integration test | 多模块协作。 |
| Platform integration test | 真实 UI runtime、visual tree、binding。 |
| Analyzer test | 编译期诊断。 |
| Build test | MSBuild target、打包、模板生成。 |
| Snapshot/manifest test | source generator 输出和清单。 |

替代测试必须在测试矩阵中说明原因。替代测试不能成为跳过单元测试的默认理由。

## 5. 测试矩阵

每个模块详细设计必须包含测试矩阵。

格式：

| 功能点 | 测试类型 | 测试工具 | 必测场景 | 完成门禁 |
|---|---|---|---|---|
| Route matching | Unit | RoutingTestHost | path、参数、约束失败 | 单测通过 |
| Plugin unload | Unit/Integration | PluginTestHost | lease、operation、UnloadPending | 单测和集成测试通过 |

规则：

- 测试矩阵必须覆盖文档声明的全部功能点。
- 测试矩阵必须在实现前确认。
- 实现完成后必须回查测试矩阵。
- 如果实现新增功能点，必须先补测试矩阵。

## 6. 完成判定

功能点完成条件：

```text
Documented
-> Has test matrix row
-> Implemented
-> Unit tested
-> Diagnostics asserted if applicable
-> Lifecycle cleanup asserted if applicable
```

缺少任一项，不能标记完成。

## 7. 禁止事项

禁止：

- 用一个大集成测试替代多个功能点单元测试。
- 实现完成后再补测试矩阵。
- 只测成功路径。
- 用真实时间等待异步完成。
- 不断言错误码和诊断上下文。
- 不断言生命周期释放。
- 插件卸载测试不检查 Lease、Operation 和引用释放。
