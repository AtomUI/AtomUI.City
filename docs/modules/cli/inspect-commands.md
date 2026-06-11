# CLI Inspect 命令设计

版本：v0.1
状态：正式初版
适用范围：workspace、project、module、route、manifest 的结构化只读检查命令

## 1. 目标

Inspect 命令为开发者、CI 和 AI Agent 提供工作区事实。Inspect 命令只读，不修改文件。

## 2. 命令

```bash
atomui city inspect workspace
atomui city inspect project <ProjectName>
atomui city inspect module <ModuleName>
atomui city inspect route <RouteIdOrPath>
atomui city inspect manifest
```

## 3. Workspace

输出：

- solution。
- projects。
- package references。
- AtomUI.City package versions。
- modules。
- routes。
- plugins。
- docs status。
- test matrix status。
- build output status。

## 4. Project

输出：

- project path。
- target frameworks。
- package references。
- project references。
- module declarations。
- generated manifests。
- test project mapping。

## 5. Module

输出：

- ModuleId。
- module type。
- dependencies。
- contributions。
- options。
- tests。
- diagnostics。

## 6. Route

输出：

- RouteId。
- path。
- parameters。
- ViewModel target。
- guards。
- resolvers。
- outlet。
- plugin source，如果适用。

## 7. Manifest

输出：

- manifest files。
- schema version。
- hash。
- validation result。
- source project。
- generated time 作为非核心诊断信息。

## 8. JSON 输出

Inspect 命令必须支持 `--json`。

JSON 输出不能依赖人类终端格式解析。

## 9. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| workspace inspect | Unit/CLI | solution、projects、docs、tests。 |
| project inspect | Unit/CLI | references、TFM、manifest。 |
| module inspect | Unit/CLI | dependencies、contributions。 |
| route inspect | Unit/CLI | path、target、guards。 |
| manifest inspect | Unit/CLI | hash、schema、validation。 |
| read-only | Unit | inspect 不写文件。 |
