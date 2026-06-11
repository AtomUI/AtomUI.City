#!/usr/bin/env bash
set -euo pipefail

awk 'FNR==1{if(NR>1 && c%2!=0){print f ": odd code fences"; bad=1} f=FILENAME; c=0} /^```/{c++} END{if(c%2!=0){print f ": odd code fences"; bad=1} exit bad}' $(find docs -name '*.md')

missing_links="$(
  perl -ne 'while(/\[[^\]]+\]\(([^)#]+\.md)\)/g){print "$ARGV\t$1\n"}' $(find docs -name '*.md') |
    while IFS=$'\t' read -r source link; do
      base="$(dirname "$source")"
      target="$base/$link"

      if [[ ! -f "$target" ]]; then
        printf '%s -> %s missing\n' "$source" "$link"
      fi
    done
)"

if [[ -n "$missing_links" ]]; then
  printf 'missing markdown links:\n%s\n' "$missing_links" >&2
  exit 1
fi

if rg -n "README\.md|README|TODO|TBD|待定|FIXME|：text|:text" docs; then
  printf 'forbidden documentation tokens found\n' >&2
  exit 1
fi
