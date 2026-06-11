#!/usr/bin/env bash
set -euo pipefail

if [[ ! -f LICENSE ]]; then
  printf 'LICENSE file is missing\n' >&2
  exit 1
fi

if ! grep -q 'GNU LESSER GENERAL PUBLIC LICENSE' LICENSE; then
  printf 'LICENSE must contain GNU Lesser General Public License text\n' >&2
  exit 1
fi

if ! grep -q 'Version 3' LICENSE; then
  printf 'LICENSE must use LGPL version 3 text\n' >&2
  exit 1
fi

if ! grep -q '<PackageLicenseExpression>LGPL-3.0-only</PackageLicenseExpression>' build/PackageMetaInfo.props; then
  printf 'PackageLicenseExpression must be LGPL-3.0-only\n' >&2
  exit 1
fi
