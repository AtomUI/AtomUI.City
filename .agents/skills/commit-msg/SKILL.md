---
name: atomuicity-commit-msg
description: Generate or create a one-line commit message for AtomUI.City by reading the git staged area and recent commit style. Use when the user asks for a commit message, says "msg", "commit msg", "写提交信息", "创建 commit", "创建commit", or wants one-line text that covers staged changes.
---

# AtomUI.City Commit Message Generation Skill

## Goal

Generate a single-line commit message based on the current git staging area. The message should summarize the intent of all staged changes and match this repository's existing style.

## Trigger Conditions

Use this skill when the user says any of:

- `创建 commit` / `创建commit`
- `commit msg` / `msg`
- `提交信息` / `写提交信息`
- asks for a commit message
- asks to summarize staged changes into a commit title

If the user asks to create the commit, this skill covers reading staged changes, generating the message, creating the commit, and verifying the result.

## Rules

- Default to staged changes only.
- Do not fabricate a message when nothing is staged.
- Check recent commit style before generating the message.
- Output one line unless the user explicitly asks for an explanation or body.
- Do not add trailers such as `Co-Authored-By` unless the user explicitly asks.

## Required Inspection

Run and read:

```bash
git status --short
git diff --cached --stat
git diff --cached
git log --oneline -10 --no-merges
```

## Message Format

Use:

```text
<type>(<scope>): <subject>
```

or, when the scope is not useful:

```text
<type>: <subject>
```

## Types

- `feat` — new framework capability, API, CLI command, template, or user-facing behavior
- `fix` — bug fix, incorrect behavior, crash, packaging issue, or broken template
- `refactor` — code restructuring without external behavior change
- `docs` — documentation only
- `style` — formatting only
- `perf` — performance improvement
- `test` — tests or test infrastructure
- `build` — MSBuild props, packaging, solution, scripts, or dependency build rules
- `chore` — maintenance outside src/test behavior
- `ci` — CI/CD configuration
- `release` — version bumps, changelog, release preparation
- `revert` — revert a previous commit

## Scope Guidance

Prefer City-specific scopes:

- `Core`
- `Cli`
- `Templates`
- `Build`
- `Packaging`
- `Tests`
- `Avalonia`
- `AtomUI`
- `Runtime`
- `Compiler`
- `Generator`
- `Hosting`
- `Docs`
- `deps`

Use a precise project or module name when one dominates the staged diff. Omit scope for broad repository initialization or scattered maintenance.

## Subject Rules

- Use imperative mood: `add`, `fix`, `update`, `move`; Chinese subjects should start with a verb such as `添加`、`修复`、`调整`.
- Do not capitalize the first letter of an English subject.
- Do not end with a period or `。`.
- Keep the whole header at 72 characters or fewer when practical.
- Avoid vague subjects like `update files`, `misc`, or `wip`.

## Examples

```text
build: add shared MSBuild props for City projects
test: add smoke tests for initial project skeleton
feat(Cli): add project creation command
fix(Templates): include template config in package output
chore(deps): bump Avalonia to 12.0.5
```

## Reference

For a fuller convention, read `references/commit-message-convention.md`.
