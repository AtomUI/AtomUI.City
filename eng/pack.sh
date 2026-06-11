#!/usr/bin/env bash
set -euo pipefail

configuration="${CONFIGURATION:-Debug}"
no_build=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --configuration|-c)
      configuration="$2"
      shift 2
      ;;
    --no-build)
      no_build=(--no-build)
      shift
      ;;
    *)
      printf 'Unknown argument: %s\n' "$1" >&2
      exit 2
      ;;
  esac
done

package_output="output/NuGet/$configuration"
mkdir -p "$package_output"

while IFS= read -r project; do
  dotnet pack "$project" --configuration "$configuration" --output "$package_output" "${no_build[@]}"
done < <(find src/AtomUI.City.* -name 'AtomUI.City.*.csproj' | sort)
