# AtomUI.City Git Commit Message Convention

## Format

```text
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

Scope may be omitted when it adds no value:

```text
build: add shared output path configuration
```

## Types

| Type | When to Use |
|---|---|
| `feat` | New framework API, CLI command, template, runtime behavior, or user-facing capability |
| `fix` | Bug fix, broken template output, packaging issue, CLI behavior fix, crash, or incorrect runtime behavior |
| `refactor` | Code restructuring with no behavior change |
| `docs` | Documentation only |
| `style` | Formatting only |
| `perf` | Performance improvement |
| `test` | Tests, test helpers, smoke tests, or test infrastructure |
| `build` | MSBuild props, solution files, package metadata, scripts, or dependency build rules |
| `chore` | Maintenance outside src/test behavior |
| `ci` | CI/CD workflows |
| `release` | Version bumps, changelog, release preparation |
| `revert` | Reverting a previous commit |

## Scopes

Use PascalCase for product areas:

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

Use lowercase for conventional maintenance scopes:

- `deps`
- `scripts`

## Subject Rules

- Use imperative mood: `add`, `fix`, `update`, `move`; Chinese should start with a verb.
- Do not capitalize the first letter of an English subject.
- Do not end with a period or `。`.
- Keep the header concise and preferably no longer than 72 characters.
- Describe the intent, not a file list.

## Examples

```text
build: add centralized MSBuild output configuration
build(Packaging): add template package metadata
test: add smoke tests for initial City projects
feat(Cli): add new app scaffold command
fix(Templates): include template config in nupkg
chore(deps): update xUnit package versions
```
