# AI Documentation

AI in Rock has three layers: a Provider abstraction over external LLMs, an Agent infrastructure (ChatAgent + Skill + Tool) for tool-calling with Rock context, and an Automations layer for non-interactive AI tasks. An MCP integration is in active development for both inbound (external clients) and outbound (Rock as host) usage. The legacy `AIProviderComponent` pattern is being deprecated.

If you are new, start with [ai-overview.md](ai-overview.md). Sub-topics worth their own docs (Agent Skill Authoring, MCP Integration, AIAutomation, Result Type Conventions, Voice Agent) will be added as separate files. Note the rapid churn here; verify against current source before trusting older docs.

## Files in this directory

| Doc | Summary |
|---|---|
| [Agent Skills Authoring](agent-skills-authoring.md) | `AgentSkillComponent` subclassing, annotation-based metadata, structured Result types, security per tool. |
| [AI Domain Overview](ai-overview.md) | Provider/Agent/Automation layering, the structured Result types, MCP integration status, and the deprecation path off the legacy AI pattern. |
| [MCP Integration](mcp-integration.md) | Model Context Protocol, public anonymous endpoint, OAuth/CIMD/DCR, voice agent, evolving API surface. |
| [Vibe Coding Architecture](vibe-coding-architecture.md) | Prototype (unmerged). Rock as MCP server for AI-authored Obsidian UI: the seeded Vibe Agent, the three skills, the knowledge base dependency, the `ObsidianContent` table, and the out-of-process compile. |
