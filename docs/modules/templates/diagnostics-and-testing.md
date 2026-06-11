# Templates 诊断和测试设计

版本：v0.1
状态：正式初版
适用范围：模板诊断、变量校验、模板生成测试、构建测试、smoke test 和功能点测试矩阵

## 1. 目标

Templates 的生成结果必须可构建、可测试、符合 Build 约定，并且不生成违反框架编程范式的结构。

## 2. 诊断

模板诊断至少包含：

- template id。
- variable name。
- output path。
- project name。
- diagnostic code。
- severity。
- remediation message。

## 3. 错误码建议

| Code | 含义 |
|---|---|
| `AUCTPL0001` | 模板变量无效。 |
| `AUCTPL0002` | `RootNamespace` 不能使用框架命名空间。 |
| `AUCTPL0101` | RoutePath 无效。 |
| `AUCTPL0201` | PluginId 无效。 |
| `AUCTPL0301` | AOT 模式和动态插件配置冲突。 |
| `AUCTPL0401` | 生成结果缺少测试矩阵。 |
| `AUCTPL0501` | 生成结果不符合 Build 输出约定。 |

## 4. 测试类型

| 类型 | 用途 |
|---|---|
| Unit test | 变量校验、路径计算、命名规则。 |
| Snapshot test | 输出文件结构和关键文件内容。 |
| Build test | 生成项目 restore/build。 |
| Smoke test | 应用模板、插件模板、测试模板端到端生成。 |
| Plugin package test | 插件模板打包和 layout validation。 |

## 5. Smoke 测试

必须覆盖：

- 应用模板生成。
- 模块模板生成。
- 页面模板生成。
- 插件模板生成。
- 测试模板生成。
- 生成后 restore。
- 生成后 build。
- manifest 生成。
- 插件包 layout validation。

## 6. 禁止事项测试

必须断言模板不会生成：

- 默认业务概念。
- 用户代码中的 `AtomUI.City.*` 命名空间。
- 未进入测试矩阵的功能点。
- 不符合 Build 文档的输出配置。
- 运行时扫描作为默认发现机制。
- 插件修改 Host Root ServiceProvider 的代码。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| variable validation | Unit | 缺失、非法、冲突变量。 |
| application template | Smoke/Build | 生成、restore、build。 |
| module template | Snapshot/Build | 文件结构、模块 manifest 输入。 |
| page template | Snapshot/Unit | route、ViewModel、View、测试入口。 |
| plugin template | Build/Plugin | package、manifest、unload test 入口。 |
| test template | Snapshot | `FeatureTestMatrix.md` 生成。 |
| no business defaults | Snapshot | 默认无业务页面。 |
