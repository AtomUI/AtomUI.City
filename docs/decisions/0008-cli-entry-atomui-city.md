# 0008 CLI 命令入口使用 atomui city

状态：Accepted
日期：2026-06-11

## 背景

CLI 是开发者、CI 和 AI Agent 使用 AtomUI.City 工程化能力的入口。命令名需要稳定、可读，并且能表达该 CLI 属于 AtomUI 生态下的 City 框架。

## 决策

CLI 顶层命令固定为：

```bash
atomui city <command>
```

不使用单独的 `city` 顶层命令作为第一版默认入口。

## 影响

正向影响：

- 品牌归属清晰。
- 后续 AtomUI 生态 CLI 可以共享 `atomui` 顶层入口。
- AI Agent 和 CI 能使用稳定命令树。

约束：

- 文档、模板和测试中统一使用 `atomui city`。
- CLI JSON 输出、dry-run、plan/apply 和 non-interactive 模式都挂在该命令树下。

## 执行约束

- CLI 设计见 `docs/modules/cli/detailed-design.md`。
- 命令树见 `docs/modules/cli/commands.md`。
