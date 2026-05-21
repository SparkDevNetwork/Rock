# Prayer Documentation

Prayer is one of Rock's smaller domains by surface area but high-touch by usage. The single primary entity is `PrayerRequest`: submitted publicly or anonymously, optionally approval-gated, surfaced on prayer cards for others to pray, optionally analyzed by AI Automation.

If you are new, start with [prayer-overview.md](prayer-overview.md). The domain is small enough that a single overview covers most use cases; sub-topic docs (Approval flow, AI integration, Public/Mobile blocks) can follow if needed.

## Files in this directory

| Doc | Summary |
|---|---|
| [Prayer Domain Overview](prayer-overview.md) | Single-entity model, approval state machine, the IsUrgent default, and the AI Automation integration points. |
| [Prayer Request Approval and Publication Flow](approval-and-publication-flow.md) | Lifecycle states, approval stamping, public vs private visibility, auto-approve per category, mobile flow parity. |
