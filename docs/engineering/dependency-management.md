# 依赖管理规范

版本：v0.1
状态：正式初版
适用范围：NuGet 版本集中管理、依赖引入、升级流程和兼容性验证

## 1. 目标

依赖管理必须保证 Core 足够薄、包边界稳定、AOT/trimming 风险可控，并且升级过程可验证。

依赖策略见：[开源依赖策略](../architecture/dependency-strategy.md)。

## 2. 中央版本管理

仓库启用 NuGet 中央版本管理：

```xml
<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
```

规则：

- PackageReference 不写 Version。
- 版本统一放在 `Directory.Packages.props` 或版本 props。
- 升级必须说明影响范围。
- 可选适配包不能污染核心依赖链。

## 3. 依赖分层

基本边界：

- Core 只依赖 .NET 基础设施。
- Presentation 承担 AtomUI/Avalonia。
- Mvvm 承担 CommunityToolkit.Mvvm。
- Data 承担 HTTP、gRPC、SignalR、resilience。
- Build/Generators 承担 Roslyn 和 MSBuild task。
- Cli 承担命令行和终端 UI。
- Testing 承担测试框架和断言库。

## 4. 升级流程

依赖升级流程：

```text
确认影响模块
-> 更新依赖策略或模块文档
-> 更新 Directory.Packages.props
-> restore/build/test
-> 运行受影响模块测试
-> 检查 AOT/trimming 风险
-> 记录必要 ADR
```

重大依赖升级必须有 ADR 或设计文档记录。

## 5. AOT 和 trimming

新增依赖前必须检查：

- 是否依赖运行时反射扫描。
- 是否使用 dynamic code generation。
- 是否支持 trimming。
- 是否影响 Native AOT。
- 是否可放入可选适配包。

## 6. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 中央版本 | Build | 无项目内联版本。 |
| Core 依赖边界 | Build | Core 不依赖 UI、Roslyn、CLI、Testing。 |
| 可选适配隔离 | Build | Rx/ReactiveUI 等不进入核心包。 |
| 升级验证 | Build/Test | restore、build、test 通过。 |
| AOT 风险 | Analyzer/Build | 不友好依赖有诊断或文档说明。 |
