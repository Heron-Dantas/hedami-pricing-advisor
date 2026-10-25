#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

# No shell tracing, environment dumps or credential output.
dotnet build Hedami.PricingAdvisor.slnx -m:1 -p:UseSharedCompilation=false -nodeReuse:false --nologo
dotnet run --project tests/Hedami.PricingAdvisor.Verification -m:1 -p:UseSharedCompilation=false -nodeReuse:false
app="src/Hedami.PricingAdvisor.Console/bin/Debug/net10.0/Hedami.PricingAdvisor.Console.dll"
dotnet "$app" doctor

# One real tools-only request must succeed before launching a larger comparison.
dotnet "$app" analyze --demo --as-of 2026-10-24 --mode tools --max-calls 8 --max-tokens 12000 --request "Analyze productId=headset-stand using ruleId=demo-direct. Compare 30% revenue margin with and without a 10% discount."

if [[ "${1:-smoke}" != "compare" ]]; then
  echo "Smoke test finished. Inspect the run before using: bash scripts/validate-live.sh compare"
  exit 0
fi

# Persistent request reservations share the USD 2 ceiling across all commands.
# Two repetitions x seven scenarios x three modes = 42 runs, subject to the ceiling.
# A failed scenario exits nonzero after saving its comparison report.
dotnet "$app" evaluate --live --demo --as-of 2026-10-24 --mode all --repeat 2 --max-runs 42
