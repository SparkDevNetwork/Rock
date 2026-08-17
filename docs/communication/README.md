# Communication Documentation

Communication is Rock's outbound messaging: bulk emails, SMS conversations, push notifications, transactional system communications, and (since 2025-08) multi-step Communication Flows. The split between `Communication` (one-time/scheduled bulk), `SystemCommunication` (templated/triggered), and `CommunicationFlow` (multi-step sequences) reflects the different reliability and lifecycle requirements of each.

If you are new, start with [communication-overview.md](communication-overview.md). Sub-topics worth their own docs (SMS Pipeline, Communication Flows, Email Editor, Compliance/Opt-In) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Bulk vs System vs Flow Communication](bulk-vs-system-vs-flow.md) | Decision matrix for picking the right construct: one-off bulk, triggered transactional, or multi-step orchestrator. |
| [Communication Entry List Selection](communication-entry-list-selection.md) | Sending to a Communication List from the simple Communication Entry block: the Enable Communication List Selection setting, Full-mode gating, the per-medium reachable count, and ListGroupId persistence. |
| [Communication Domain Overview](communication-overview.md) | Three parallel constructs: bulk Communication, SystemCommunication for transactional, and CommunicationFlow for multi-step sequences with conversion tracking. |
| [Communication Flows](communication-flows.md) | Multi-step orchestrator: enrollment, per-step Communication generation, conversion tracking, multi-channel sequences. |
| [Email Editor and Sections](email-editor-and-sections.md) | The drag-and-drop section model, CommunicationTemplate vs EmailSection vs Snippet, Lava merge fields at send time. |
| [Push Notifications](push-notifications.md) | CommunicationType.Push, PersonalDevice token routing, fallback to email/SMS, mobile-shell registration. |
| [SMS Pipeline](sms-pipeline.md) | Inbound SMS chain-of-responsibility, SmsAction components, opt-in/out compliance per SystemPhoneNumber. |
