# Build Analyzer 设计

版本：v0.1
状态：正式初版
适用范围：构建期 analyzer 规则、诊断 ID、AOT/trimming、插件、架构和测试矩阵诊断

## 1. 目标

Analyzer 用于在构建期发现架构、AOT、插件、manifest 和测试门禁问题，避免问题延迟到运行时。

## 2. 诊断 ID

Build/Generator 诊断建议使用：

```text
AUCBLD0001
AUCGEN0001
AUCANL0001
```

分类：

| 前缀 | 用途 |
|---|---|
| `AUCBLD` | MSBuild task 和 package/publish 诊断。 |
| `AUCGEN` | Source generator 诊断。 |
| `AUCANL` | Analyzer 诊断。 |
| `AUCPLG` | Plugin package 诊断，和 PluginSystem 文档保持一致。 |

## 3. 诊断级别

| 级别 | 用途 |
|---|---|
| Error | 破坏构建产物、manifest、AOT 或运行时必需能力。 |
| Warning | 影响兼容性、性能、AOT/trimming 或推荐规范。 |
| Info | 优化建议或迁移提示。 |

## 4. 第一版规则

建议 analyzer 覆盖：

- 使用运行时程序集扫描但未显式 opt-in。
- `Strict` 模式下使用 dynamic discovery。
- Route 定义冲突。
- Permission id 冲突。
- View/ViewModel 映射不稳定。
- 插件私有类型泄漏到 Host contract。
- 插件缺少 `PluginId`。
- 插件包多主程序集。
- Contribution 不可撤销。
- EventBus 跨插件事件不在共享 contract。
- Source generator 无法识别需要生成的声明。
- 测试矩阵缺失。

## 5. AOT 和 trimming

必须诊断：

- 未声明反射。
- 动态代理。
- 表达式树编译路径。
- Native AOT 模式动态插件。
- 非 source-generated serializer/options binding，如果框架要求生成路径。

## 6. 测试门禁

Analyzer 可以辅助发现：

- 模块详细文档缺少测试矩阵。
- 测试项目缺少功能点测试。
- 生成 manifest 中功能点没有对应测试索引。

具体实现可以分阶段，但 Build 文档必须把测试门禁作为目标。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| diagnostic id | Analyzer test | 规则触发稳定 ID。 |
| diagnostic location | Analyzer test | 定位到声明位置。 |
| strict AOT | Analyzer test | dynamic discovery 报错。 |
| plugin leak | Analyzer test | private contract 泄漏报错。 |
| route conflict | Analyzer test | 重复 route 报错。 |
| no false positive | Analyzer test | 正确代码不报错。 |
