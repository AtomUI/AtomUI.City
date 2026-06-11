# CLI Build 命令设计

版本：v0.1
状态：正式初版
适用范围：`atomui city build`、`pack`、`publish` 命令、Build 调用、诊断透传和输出规则

## 1. 目标

Build 命令负责调用 `AtomUI.City.Build` 定义的 MSBuild targets，不在 CLI 内重新实现构建逻辑。

## 2. 命令

```bash
atomui city build
atomui city pack
atomui city publish
```

## 3. 参数

| 参数 | 说明 |
|---|---|
| `--configuration` | Debug、Release。 |
| `--framework` | Target Framework。 |
| `--project` | 指定项目。 |
| `--output-root` | 覆盖 Build 输出根目录。 |
| `--strict-aot` | 启用严格 AOT 检查。 |
| `--json` | 输出结构化诊断。 |

## 4. 执行流程

```text
Inspect workspace
-> Resolve project
-> Build MSBuild invocation
-> Run Build target
-> Collect Build diagnostics
-> Map exit code
-> Emit CLI output
```

## 5. 规则

- 不绕过 MSBuild target。
- 默认遵守 `output/` 布局。
- Build diagnostic code 原样透传。
- CLI 可以补充命令级诊断，但不能改写 Build 错误语义。
- `--json` 输出包含 Build diagnostics。

## 6. Pack

`pack` 根据项目类型调用：

- 普通 NuGet pack。
- 插件 package target。
- 模板 package target。

插件 pack 必须执行 package layout validation。

## 7. Publish

`publish` 调用应用发布 target。

规则：

- Native AOT 兼容性由 Build 校验。
- 动态插件和 AOT 冲突由 Build diagnostic 表达。
- 发布输出进入 Build 定义的 publish layout。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| build | CLI/Build | target 调用、diagnostics 透传。 |
| pack | CLI/Build | plugin package validation。 |
| publish | CLI/Build | publish layout、AOT diagnostic。 |
| 参数映射 | Unit | configuration、framework、project。 |
| JSON 输出 | Unit | Build diagnostics envelope。 |
