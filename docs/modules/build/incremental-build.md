# Build 增量构建设计

版本：v0.1
状态：正式初版
适用范围：增量生成、缓存、输入输出追踪、确定性输出和 CI 可复现性

## 1. 目标

Build 必须支持稳定增量构建，避免每次构建都重复生成所有 manifest 和包。

设计目标：

- 输入不变，输出不变。
- 输出内容 deterministic。
- manifest 和 hash 可复现。
- CI 和本地行为一致。
- 增量缓存错误时可以安全重建。

## 2. 输入追踪

输入包括：

- 源码。
- AdditionalFiles。
- MSBuild properties。
- MSBuild items。
- resource files。
- language packages。
- plugin assets。
- generator version。
- build task version。

输入变化必须触发相关输出重建。

## 3. 输出追踪

输出包括：

- generated C#。
- intermediate manifest。
- final manifest。
- plugin package。
- application manifest。
- diagnostics。

输出应带内容 hash 或输入 hash 摘要。

## 4. 确定性规则

规则：

- 不把当前时间写入核心输出。
- 不把机器绝对路径写入核心输出。
- 不使用随机顺序。
- manifest 排序稳定。
- package 内容顺序稳定。
- diagnostic 输出稳定。

## 5. 缓存失效

缓存失效条件：

- 源文件变化。
- AdditionalFiles 变化。
- MSBuild 属性变化。
- generator/task 版本变化。
- manifest schema 版本变化。
- resource hash 变化。

缓存不可信时，可以删除后完整重建。

## 6. CI 复现

CI 应能断言：

- clean build 和 incremental build 输出一致。
- 同一输入在不同机器核心 manifest hash 一致。
- 输出目录不包含机器本地路径。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| input hash | Unit | 源码和 item 变化触发重建。 |
| no-op build | Build | 未变化不重写输出。 |
| deterministic manifest | Build | clean/incremental 输出一致。 |
| cache invalidation | Build | generator 版本变化触发重建。 |
| path stability | Unit/Build | 输出不包含临时绝对路径。 |
