#!/usr/bin/env bash
set -euo pipefail

dotnet test AtomUICity.slnx --no-build --filter "Category!=PlatformIntegration"
