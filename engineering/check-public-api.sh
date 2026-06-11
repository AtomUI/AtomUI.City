#!/usr/bin/env bash
set -euo pipefail

configuration="${CONFIGURATION:-Debug}"
output_dir="output/public-api"
output_file="$output_dir/public-api.txt"

mkdir -p "$output_dir"

if ! rg -n '^[[:space:]]*public[[:space:]]+((abstract|sealed|static|partial|readonly|record)[[:space:]]+)*(class|interface|enum|struct|record|delegate)[[:space:]]+' src --glob '*.cs' > "$output_file"; then
  printf 'No public API declarations found.\n' >&2
  exit 1
fi

missing_docs=()

while IFS= read -r project; do
  project_name="$(basename "$project" .csproj)"
  project_xml_docs="$(find "output/bin/$configuration/$project_name" -name "$project_name.xml" -print -quit 2>/dev/null || true)"

  if [[ -z "$project_xml_docs" ]]; then
    missing_docs+=("$project_name")
  fi
done < <(find src/AtomUI.City.* -name 'AtomUI.City.*.csproj' | sort)

if [[ ${#missing_docs[@]} -gt 0 ]]; then
  printf 'Missing XML API documentation. Ensure GenerateDocumentationFile is enabled for:\n' >&2
  printf '  %s\n' "${missing_docs[@]}" >&2
  exit 1
fi

printf '%s\n' "$output_file"
