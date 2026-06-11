# AOT 和 Source Generator 测试设计

版本：v0.1
状态：正式初版
适用范围：source generator、analyzer、manifest 生成、AOT/trimming diagnostics、Build target 和静态插件测试

## 1. 目标

AtomUI.City 把 AOT 友好作为全局约束。Testing 必须支持编译期能力的验证，避免实现依赖运行时反射扫描。

## 2. Source Generator 测试

必须支持：

- 输入源码。
- 运行 generator。
- 断言生成源码。
- 断言生成 manifest。
- 断言 diagnostics。
- 断言 incremental generator cache 行为。

生成结果应优先做结构化断言。源码 snapshot 可作为补充。

## 3. Analyzer 测试

必须覆盖：

- 正确代码无诊断。
- 错误代码有诊断。
- diagnostic id。
- diagnostic location。
- code fix，如果该能力存在。

## 4. Manifest 测试

必须覆盖：

- module manifest。
- route manifest。
- plugin manifest。
- localization index。
- permission index。
- data client index。
- presentation mapping。

Manifest 字段顺序必须稳定。

## 5. AOT 和 trimming 测试

必须覆盖：

- dynamic discovery 在 strict AOT 模式下被拒绝或诊断。
- 未声明反射被诊断。
- source-generated options binding。
- source-generated serialization context。
- Native AOT 模式拒绝动态插件。
- 静态插件 manifest 生效。

## 6. Build Target 测试

Build 模块实现后，Testing 应支持：

- MSBuild target 执行。
- package layout 验证。
- output 目录验证。
- generated artifact 验证。
- diagnostic code 验证。

## 7. 测试要求

编译期功能点必须有 generator/analyzer/build test。运行时集成测试不能替代编译期测试。
