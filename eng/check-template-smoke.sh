#!/usr/bin/env bash
set -euo pipefail

configuration="${CONFIGURATION:-Debug}"
command_name="atomui city new app"
repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
package_source="$repository_root/output/NuGet/$configuration"
cli_path="$repository_root/output/bin/$configuration/AtomUI.City.Cli/net10.0/AtomUI.City.Cli.dll"
workspace="$(mktemp -d "${TMPDIR:-/tmp}/atomuicity-template-smoke.XXXXXX")"

cleanup() {
  rm -rf "$workspace"
}
trap cleanup EXIT

if [[ ! -d "$package_source" ]]; then
  printf 'Package source does not exist: %s\n' "$package_source" >&2
  exit 1
fi

if [[ ! -f "$cli_path" ]]; then
  printf 'CLI assembly does not exist: %s\n' "$cli_path" >&2
  exit 1
fi

cat > "$workspace/NuGet.Config" <<CONFIG
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="AtomUICityLocal" value="$package_source" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
CONFIG

printf 'Running %s smoke test in %s\n' "$command_name" "$workspace"

dotnet "$cli_path" city new app TemplateSmoke \
  --namespace Company.TemplateSmoke \
  --output "$workspace" \
  --json > "$workspace/new-app.json"

dotnet restore "$workspace/tests/TemplateSmoke.Tests/TemplateSmoke.Tests.csproj" \
  --configfile "$workspace/NuGet.Config"

dotnet build "$workspace/src/TemplateSmoke/TemplateSmoke.csproj" \
  --no-restore

dotnet test "$workspace/tests/TemplateSmoke.Tests/TemplateSmoke.Tests.csproj" \
  --no-restore
