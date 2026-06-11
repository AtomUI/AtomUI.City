#!/usr/bin/env bash
set -euo pipefail

version="${1:-}"

if [[ -z "$version" ]]; then
  version="$(sed -n 's:.*<AtomUICityVersion>\(.*\)</AtomUICityVersion>.*:\1:p' build/Version.props | head -n 1)"
fi

if [[ -z "$version" ]]; then
  printf 'Unable to determine AtomUI.City version.\n' >&2
  exit 2
fi

output_dir="output/release-notes"
output_file="$output_dir/AtomUI.City.$version.md"
mkdir -p "$output_dir"

cat > "$output_file" <<NOTES
# AtomUI.City $version

## New features

- See RELEASE_NOTES.md for the curated release summary.

## Breaking changes

- See RELEASE_NOTES.md for breaking change notes.

## Fixes

- See RELEASE_NOTES.md for fixes.

## Known limitations

- See RELEASE_NOTES.md for known limitations.

## Migration notes

- See RELEASE_NOTES.md for migration notes.

## Plugin API compatibility

- See RELEASE_NOTES.md for plugin API compatibility notes.
NOTES

printf '%s\n' "$output_file"
