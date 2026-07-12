# Workflow Documentation

Workflow is Rock's process automation and form-building engine. A `WorkflowType` is a template, a `Workflow` is one execution. Form Builder is the workflow UI's other face: a visual designer that produces a WorkflowType whose first activity is a form-presentation step.

If you are new, start with [workflow-overview.md](workflow-overview.md). Sub-topics worth their own docs (Form Builder, Workflow Triggers, Custom Action Components, Persistence Models) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Form Builder](form-builder.md) | Form-as-WorkflowType, shareable links, Category-based security inheritance, the v20 Settings / Automations / Person Entry refresh, and post-submit automations (Confirmation Email with Recipient = Both, Notification Email, Connection Requests with Attribute Matching). |
| [The Workflow Runtime](the-runtime.md) | Synchronous vs async execution, the `ProcessWorkflows` job, persistent vs transient, activity branching. |
| [Workflow Domain Overview](workflow-overview.md) | Template-vs-instance model, the runtime, action component pluggability, Form Builder, and the trigger system. |
| [Workflow Triggers](workflow-triggers.md) | Entity-save event hooks, PreSave vs PostSave, qualifier-based filtering, domain-specific trigger entities. |
| [Writing Workflow Action Components](writing-action-components.md) | Subclassing `ActionComponent`, attribute-based configuration, idempotency, the `Execute` contract. |
