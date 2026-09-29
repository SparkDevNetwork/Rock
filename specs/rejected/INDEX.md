# Rejected Specs Index

This index lists every spec that has been moved into `specs/rejected/`. It is maintained by the `spec` skill, please do not edit by hand. Specs land here when a proposal is dismissed; the goal is to preserve the historical reasoning so future contributors can find it before re-proposing the same idea.

| Spec | Domain | Author | Summary | Rejected On | Rejection Reason |
|------|--------|--------|---------|-------------|------------------|
| [MJML Email Builder Adoption](communication/260910-mjml-email-builder-adoption.md) | Communication | Joshua Henninger | Evaluation of replacing the Email Builder's hand-built table markup with MJML; it would eliminate the nesting-depth bug class but requires rewriting the editor core and accepting fidelity loss. | 2026-09-10 | MJML is a one-way compiler and the Builder is a live-DOM WYSIWYG; the editor rewrite and margin/border/Lava regressions outweigh the rendering bugs it would fix. |
