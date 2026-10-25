"""Summarize recorded live evaluations without making network requests."""
import json
import statistics
import sys
from decimal import Decimal
from pathlib import Path

root = Path(__file__).resolve().parents[1]
reports = sorted((root / "artifacts").glob("evaluations-*.json"))
if len(sys.argv) > 1:
    report_path = Path(sys.argv[1])
elif reports:
    report_path = reports[-1]
else:
    raise SystemExit("No evaluation report exists yet. Run the live comparison first.")
report = json.loads(report_path.read_text(), parse_float=Decimal)
if report.get("simulated") is not False:
    raise SystemExit("Expected a real-model evaluation report.")
rows = report["results"]
lines = ["# Recorded live comparison", "", f"Source: `{report_path.name}`. Model: `{report['modelName']}`. As-of: `{report['asOf']}`.", "",
         "Automatic success is a structural/numeric grader result, not a human usefulness score. Cost is a rate-based estimate; unknown usage stays unknown.", "",
         "| Mode | Automatic passes | Median seconds | Model calls | Tool calls | Reported tokens | Estimated USD |",
         "| --- | ---: | ---: | ---: | ---: | ---: | ---: |"]
for mode in dict.fromkeys(row["mode"] for row in rows):
    group = [row for row in rows if row["mode"] == mode]
    known_cost = all(row.get("estimatedCostUsd") is not None for row in group)
    cost = f"{sum(row['estimatedCostUsd'] for row in group):.6f}" if known_cost else "unknown/partial"
    lines.append(f"| {mode} | {sum(row['passed'] for row in group)}/{len(group)} | {statistics.median(row['durationMs'] for row in group) / 1000:.3f} | {sum(row['modelCalls'] for row in group)} | {sum(row['toolCalls'] for row in group)} | {sum(row['inputTokens'] + row['outputTokens'] for row in group)} | {cost} |")
lines += ["", "## Individual results", "", "| Scenario | Mode | Repetition | Automatic result | Status | Run ID |", "| --- | --- | ---: | --- | --- | --- |"]
for row in rows:
    lines.append(f"| {row['scenarioId']} | {row['mode']} | {row['repetition']} | {'PASS' if row['passed'] else 'FAIL'} | {row['status']} | {row['runId']} |")
lines += ["", "## Interpretation boundary", "", "Inspect failed and successful traces before concluding that extra agents help. Safe clarification before a calculation error can fail the current strict grader. Review verdicts are model outputs, not proof of correctness. Small repeated samples do not establish production reliability.", ""]
output = root / "artifacts" / "comparison-summary.md"
output.write_text("\n".join(lines))
print(output.read_text())
print(f"Saved: {output}")
