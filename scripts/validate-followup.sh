#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

dotnet build Hedami.PricingAdvisor.slnx -m:1 -p:UseSharedCompilation=false -nodeReuse:false --nologo
dotnet run --project tests/Hedami.PricingAdvisor.Verification -m:1 -p:UseSharedCompilation=false -nodeReuse:false
app="src/Hedami.PricingAdvisor.Console/bin/Debug/net10.0/Hedami.PricingAdvisor.Console.dll"
comparison_status=0
dotnet "$app" evaluate --live --demo --as-of 2026-10-24 --mode all --scenarios complete,harmful-discount --repeat 2 --max-runs 12 || comparison_status=$?

# Link a real prior successful draft to a new explicit human objective.
parent_run=$(python3 - <<'PYTHON'
import json
from pathlib import Path
for path in sorted(Path('artifacts/runs').glob('*.json'),key=lambda p:p.stat().st_mtime,reverse=True):
    run=json.loads(path.read_text())
    if run['status']=='draft_ready' and not run['simulated']:
        print(run['id'])
        break
else:
    raise SystemExit('No successful real tools-only draft exists for feedback.')
PYTHON
)
dotnet "$app" analyze --demo --as-of 2026-10-24 --mode tools --feedback-from "$parent_run" --feedback "Replace all prior margin targets with exactly 25% revenue margin. Keep the same product/rule and compare with and without a 10% discount. Use the authoritative synthetic data again."
python3 scripts/summarize-evaluations.py
exit "$comparison_status"
