# 0006 AOT-first 和 Source Generator-first

状态：Accepted
日期：2026-06-11

## 背景

AtomUI.City 需要支持桌面应用启动性能、包体积、trimming 和 Native AOT 场景。运行时反射扫描、动态代理、表达式树编译和命名约定发现会增加 AOT 风险。

## 决策

AtomUI.City 采用 AOT-first / Source Generator-first 设计。

默认路径优先使用：

- 显式注册。
- 强类型 descriptor。
- 构建期 manifest。
- Source Generator。
- Analyzer 诊断。

## 影响

正向影响：

- 启动时扫描更少。
- Native AOT 和 trimming 风险更可控。
- 错误尽量前移到构建期。
- Manifest 可测试、可诊断。

约束：

- 运行时程序集扫描不是默认发现机制。
- 动态插件和动态 assembly loading 必须明确声明 AOT 限制。
- Runtime package 不依赖 Roslyn。
- Generator/Analyzer 通过 Build 接入。

## 执行约束

- Source Generator 规范见 `docs/architecture/source-generation.md`。
- Build source generation 见 `docs/modules/build/source-generation.md`。
- Testing 的 AOT/SG 测试见 `docs/modules/testing/aot-and-source-generation-testing.md`。
