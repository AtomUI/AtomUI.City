---
name: atomuicity-upgrade-dependencies
description: Use when upgrading AtomUI.City NuGet dependencies, .NET SDK/runtime targets, Avalonia, AtomUI, test packages, or third-party versions where compatibility must be evaluated before implementation.
---

# AtomUI.City Dependency Upgrade Skill

## Core Principle

Dependency upgrades are evidence-first work. Compare the current version and target version through source diffs, API changes, package metadata, and actual repository usage before editing version declarations.

Release notes and changelogs are useful secondary context, but they are not enough by themselves.

## Hard Rules

- Check the dirty worktree first and do not revert unrelated user changes.
- Do not modify dependency versions during evaluation unless the user has approved the upgrade plan.
- Upgrade one dependency family at a time unless the user explicitly approves grouping.
- Preserve AtomUI.City behavior. Compatibility fixes are allowed; unrelated refactors are not part of upgrade work.
- If discovery changes the risk profile, stop and revise the plan before continuing.
- Do not commit unless the user explicitly asks for a commit.

## Version Sources

Check these files before evaluating an upgrade:

- `build/Version.props`
- `Directory.Packages.props`
- relevant `*.csproj`
- `global.json`, if present
- restore output or lock files, if introduced later

Record the current version, requested target version, and the file that owns the version declaration.

## Reference Source Workflow

For framework-level dependencies such as Avalonia, AtomUI, ReactiveUI, Roslyn, or .NET SDK-related packages:

1. Map the package family to its upstream source repository.
2. Prefer an existing `.referenceprojects/<RepoName>` checkout if present.
3. If it is missing and source-level evaluation is needed, clone the upstream repository under `.referenceprojects`.
4. Run `git fetch --tags --prune` in the reference repository.
5. Resolve current and target versions to exact tags, branches, or commits.
6. Compare source diffs:

```bash
git diff <old-ref>..<new-ref> --name-status
git diff <old-ref>..<new-ref> -- <relevant paths>
git log --oneline <old-ref>..<new-ref>
```

If a version cannot be resolved, report the exact blocker instead of guessing.

## Evaluation Checklist

Before implementation, produce an evaluation with:

- current version and target version
- exact source refs compared, when source comparison is available
- public API changes
- behavior changes likely to affect AtomUI.City
- removed or renamed symbols
- target framework, runtime, platform, or tooling changes
- package dependency constraint changes
- repository usage impact from `rg` searches
- risk level with concrete reasons
- required AtomUI.City changes, if any
- verification commands
- changelog/release note links marked as secondary context

## Upgrade Plan Gate

After evaluation, create a concrete plan and wait for user approval before editing.

Use this shape:

```markdown
## Upgrade Plan: <Dependency> <current-version> -> <target-version>

- [ ] 1. Update version declarations in <files>.
- [ ] 2. Apply compatibility fixes for <specific API or behavior>.
- [ ] 3. Update tests or smoke tests for <specific affected area>.
- [ ] 4. Run restore/build/test verification.
- [ ] 5. Run pack/template verification when packaging is affected.
- [ ] 6. Report residual risks and pre-existing failures.
```

Approval must be explicit, such as "approved", "continue", "execute this plan", or equivalent wording.

## Execution Rules

Once approved:

- Execute the plan from top to bottom.
- Update task status incrementally.
- Keep edits scoped to the approved plan.
- If verification fails, distinguish upgrade-caused failures from pre-existing failures.
- If a new incompatibility appears, pause and revise the plan.

## Verification

At minimum, use:

```bash
dotnet restore AtomUICity.slnx
dotnet test AtomUICity.slnx
dotnet pack src/AtomUI.City.Templates/AtomUI.City.Templates.csproj --no-build --configuration Debug
```

Add targeted CLI, template install, sample app, or integration verification as those project surfaces are introduced.
