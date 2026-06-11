# CLI 诊断和测试设计

版本：v0.1
状态：正式初版
适用范围：CLI command tests、golden output、JSON schema、template/build/plugin integration smoke、docs/tests gate 和 AI 输出验证

## 1. 目标

CLI 每个功能点必须有测试。测试必须覆盖人类输出、JSON 输出、exit code、非交互行为和跨模块调用边界。

## 2. 测试工具

Testing 包应支持：

- `CliCommandTestHost`。
- `CliOutputAssertions`。
- `CliPlanTestHost`。
- `CliWorkspaceFixture`。
- `GoldenOutputAssertions`。
- `JsonSchemaAssertions`。
- `ExitCodeAssertions`。

## 3. 测试类型

| 类型 | 用途 |
|---|---|
| Unit test | 参数解析、配置、exit code、JSON schema。 |
| Golden output test | human help、no-color、verbosity。 |
| CLI integration test | command -> Templates/Build/Plugin metadata。 |
| Template smoke test | new/generate 输出可构建。 |
| Build integration test | build/pack/publish 调用 Build。 |
| Plugin integration test | 插件 plan/install/update/pending。 |

## 4. JSON Schema 测试

必须覆盖：

- success envelope。
- failure envelope。
- diagnostics array。
- suggestedActions。
- documentationLinks。
- schemaVersion。

## 5. Plan / Apply 测试

必须覆盖：

- plan 生成。
- dry-run 不写文件。
- apply 写文件。
- apply schema 校验。
- apply 文件冲突。
- rollback 信息存在。

## 6. Non-interactive 测试

必须覆盖：

- 缺参数不 prompt。
- 需要确认但缺 `--yes` 时失败。
- `--json` 下不输出普通文本。
- exit code 稳定。

## 7. 命令测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| command parsing | Unit | 每个命令路径。 |
| generate | CLI integration | module、page、plugin、test。 |
| build | CLI/Build | Build diagnostics 透传。 |
| plugin | CLI/Plugin | install dry-run、UnloadPending。 |
| inspect | Unit/CLI | 只读 JSON。 |
| docs check | Unit | 缺文档和缺矩阵。 |
| tests check | Unit | 缺单测和释放断言。 |
| explain | Unit | 已知和未知 code。 |

## 8. 测试隔离

CLI 测试必须使用临时工作区。

规则：

- 不使用真实用户插件目录。
- 不依赖真实 NuGet feed。
- 不修改开发者全局配置。
- 不依赖真实终端颜色能力。
