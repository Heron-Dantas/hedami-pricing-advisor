# Detailed Word documentation

The final English study/portfolio guide is [HEDAMI-Pricing-Advisor-Technical-Guide.docx](HEDAMI-Pricing-Advisor-Technical-Guide.docx). It is editable in Word and contains chapter navigation, six diagrams, real code excerpts, financial derivations, agents/harness/runtime skills, evidence, limitations and a final phase-two roadmap.

The [initial Word outline](HEDAMI-Pricing-Advisor-Initial-Outline.docx), [initial Markdown outline](documentation-outline-v0.1.md) and [author self-review](documentation-review.md) preserve the requested draft → analysis → improvement process. This is not an independent external review.

Supporting artifacts: [expanded Markdown](technical-guide-final.md), [editable source template](technical-guide-source.md), [source snapshot](source-snapshot.json), [validation report](document-validation.json) and the assets directory. The evidence snapshot is October 24, 2026. The generated Word metadata records actual generation time.

Rebuild from the repository root using the dependencies in documentation-requirements.txt:

```sh
python3 scripts/build-documentation-figures.py
python3 scripts/build-word-documentation.py
textutil -convert txt -output /tmp/hedami-technical-guide.txt docs/deliverables/HEDAMI-Pricing-Advisor-Technical-Guide.docx
python3 scripts/validate-word-documentation.py
```

The diagrams use installed Arial fonts on macOS, with a DejaVu Sans fallback on Linux. textutil is a macOS extraction check. Full Word page rendering was unavailable in the restricted session; structural checks and diagram inspection are recorded accurately. Opening the file in Word will perform normal page layout and calculate page-number fields.

## Personal Brazilian Portuguese edition

Heron requested a [Brazilian Portuguese personal study guide](pt-BR/HEDAMI-Pricing-Advisor-Guia-Pessoal-pt-BR.docx). This is not the final presentation material. It retains the detailed explanations and translated figures while preserving original code/commands and the English Word artifact. See the [Portuguese package notes](pt-BR/README.md).
