# Claude Code for Rock RMS

A developer guide for using [Claude Code](https://code.claude.com/docs/en/overview) in the Rock RMS codebase.

> **Note:** This guide is written for the terminal (CLI) experience. Claude Code is also available as a [VS Code extension](https://code.claude.com/docs/en/vs-code) and through the [Claude desktop app](https://code.claude.com/docs/en/desktop). The core concepts -- skills, rules, MCP, context management -- apply everywhere, but the commands and examples below assume you're working in a terminal.

---

## 1. Overview

Claude Code is an agentic coding tool that reads files, writes code, runs commands, and operates within the context of this repository.

**How we use it:** Claude Code accelerates Rock development -- block conversions, entity scaffolding, migrations, SQL scripts, bug fixes, and code review. The `.claude/` directory is checked into the repo so the entire team shares the same configuration, rules, and skills.

**Philosophy:** Claude Code is a tool for leverage, not a replacement for engineering judgment. You own the output. Review what it produces, understand what it changed, and verify it works before committing.

---

## 2. Setup

### Install

```bash
npm install -g @anthropic-ai/claude-code
```

Then open a terminal at the repo root and run:

```bash
claude
```

Claude Code automatically picks up the shared `CLAUDE.md` and `.claude/` configuration. On first run it will walk you through authentication -- follow the prompts.

### LSP Setup

LSP gives Claude the same code navigation your IDE has -- go-to-definition, find references, type resolution, and diagnostics. In a codebase this large, that difference matters.

Install both from within a Claude Code session:

```
/plugin install typescript-lsp@claude-plugins-official
/plugin install csharp-lsp@claude-plugins-official
```

- **TypeScript LSP** -- Type-aware navigation across the Obsidian frontend (Vue 3 + TypeScript)
- **C# LSP** -- Cross-project resolution across Rock.Blocks, Rock.ViewModels, Rock.Model, and the rest of the solution

> **Why it matters:** With LSP, Claude can resolve types, follow inheritance chains, and find all usages -- the same way your IDE does. Without it, it relies on grep and file reads alone.
>
> **Troubleshooting:** If LSP isn't working, run `/plugin` and check the **Errors** tab. The most common issue is the language server binary not being found in your PATH.

### Prompting tips

- **Plan before you build.** For non-trivial tasks, press `Shift+Tab` to toggle Plan Mode -- Claude explores the code without making changes. Once you're aligned on the approach, switch back and let it execute.
- **Be specific.** "Fix the login bug in `AuthenticationService.cs` -- session tokens aren't refreshing after expiry" beats "fix the login bug."
- **Include test cases.** "Implement `ValidateEmail`. Test: `user@example.com` returns true, `invalid` returns false. Run tests after."
- **Scope your requests.** One task per prompt. If you need three things, do them sequentially or break them into separate prompts.
- **Provide context.** Paste error messages, stack traces, or screenshots directly into the prompt. The more Claude can verify its own work, the better the output.

---

## 3. Project Configuration

The `.claude/` directory is checked into the repo. Everything here is shared -- treat it like production code.

```
.claude/
  README.md              -- This guide
  settings.json          -- Shared permissions and hooks (team-wide)
  settings.local.json    -- Personal overrides (gitignored)
  commands/              -- Slash commands (/build, /test, /check)
  hooks/                 -- Safety hooks (block destructive git operations)
  rules/                 -- Contextual rules (auto-loaded based on file paths)
    block-architecture.md     Loads when editing block files
    code-conventions.md       Always loaded (formatting, SQL, enums, etc.)
    data-model.md             Always loaded (entities, FKs, GUIDs, etc.)
  skills/                -- On-demand workflows (/convert-block, /bugfix, etc.)
```

### How the pieces work together

| Component | Location | When it loads | Purpose |
|---|---|---|---|
| `CLAUDE.md` | Repo root | Always | Core guidelines: architecture, naming, critical rules, commit format |
| Rules | `.claude/rules/` | Always or by file path | Additional coding standards and domain conventions |
| Skills | `.claude/skills/` | On demand (`/command` or keyword) | Multi-step workflows with reference material |
| Commands | `.claude/commands/` | On demand (`/command`) | Simple one-shot operations |
| Settings | `.claude/settings.json` | Always | Permission allowlist and safety hooks |

**Rules** deserve special attention: `block-architecture.md` only activates when Claude is working on files in `Rock.Blocks/`, `Rock.JavaScript.Obsidian.Blocks/`, `RockWeb/Blocks/`, or `Rock.ViewModels/Blocks/`. The other two rules load on every session. This keeps Claude's context focused -- it gets block patterns when working on blocks, not when writing a migration.

---

## 4. Using Skills & Commands

### Skills

Skills are on-demand workflows that encode Rock-specific knowledge. They live in `.claude/skills/` and are invoked by `/` command or keyword triggers.

| Skill | Triggers | Purpose |
|---|---|---|
| `/bugfix` | "fix this bug", "debug this", error + fix intent | Root cause analysis and minimal correct fix |
| `/convert-block` | "convert block", "obsidian conversion", WebForms path | Converts WebForms (.ascx) to Obsidian (Vue 3 + C#) |
| `/css-cleanup` | "clean up css", "style audit", "use rock utilities" | Replaces inline styles with Rock utility classes |
| `/entity-model` | "create entity", "new model", "scaffold entity" | Scaffolds entity class, config, SystemGuid, service |
| `/migration` | "write migration", "EF migration", "review migration" | Writes/reviews Up() and Down() for EF migrations |
| `/plugin-migration` | "plugin migration", "hotfix", "new hotfix" | Creates plugin migration .cs files in HotFixes/ |
| `/review-conversion` | "review conversion", "check the conversion" | Audits completed Obsidian conversion against WebForms original |
| `/sql` | "write sql", "seed data", "insert data" | Generates Rock-safe SQL with proper conventions |

Skills can be invoked two ways:
- **Directly:** Type `/convert-block` in the prompt
- **By keyword:** Describe the task naturally -- "convert `RockWeb/Blocks/Core/CampusList.ascx` to Obsidian" triggers the conversion skill automatically

Each skill has a `SKILL.md` (the workflow definition) and a `references/` folder (domain knowledge, patterns, common pitfalls). **Read the skill files** to understand what each one does -- this is the best way to learn the patterns.

Full docs: https://code.claude.com/docs/en/skills

### Commands

Commands are lightweight slash commands for build and verification.

| Command | Purpose |
|---|---|
| `/build` | Build Rock.sln, report errors |
| `/test` | Run Rock.Tests, report results |
| `/check` | Pre-commit verification: build + test + diff review |

### After code generation

When Claude creates new ViewModels (bags) or block types, you need to run the **Rock.CodeGeneration** tool to generate the corresponding TypeScript types before working on the Obsidian frontend. Claude cannot run this for you -- it's a WPF app. Run it from Visual Studio after Claude finishes the C# side.

---

## 5. Permissions & Safety

### Shared permissions

`settings.json` pre-approves common operations so Claude doesn't prompt for every file read or git status. The current allowlist covers:

- **File operations** -- read, write, edit, glob, grep (all pre-approved)
- **Git read operations** -- status, diff, log, show, branch, blame (pre-approved)
- **Git write operations** -- add, create branches, push to origin (pre-approved)
- **Build and test** -- `dotnet build`, `dotnet test` (pre-approved)
- **Everything else** -- requires your approval when prompted

### Safety hooks

A `PreToolUse` hook (`hooks/prevent-destructive.sh`) intercepts every Bash command and blocks dangerous git operations before they execute:

- Force push (`--force`, `-f`) and hard reset (`--hard`)
- Rebase onto main or develop
- Amend commits, skip hooks (`--no-verify`)
- Force-delete branches (`-D`), drop/clear stashes
- Blanket discard (`checkout .`, `restore .`, `clean -f`)

These protect the team from accidental data loss in autonomous mode. If you need to do something the hook blocks, do it manually outside Claude Code.

### Personal overrides

Create `.claude/settings.local.json` (gitignored) for personal preferences -- additional MCP servers, adjusted permission prompts, or tool-specific settings that don't affect the team.

### Team guidelines

- **Do not modify skills you didn't author** without coordinating with the team. They encode tested workflows that others depend on.
- **Do not edit `settings.json`** without team review. It controls permissions and safety hooks for everyone.
- **Test skill changes before pushing.** Run the skill against a known input and verify the output.

### Contributing to shared knowledge

When Claude makes a repeatable mistake, add it to the relevant `common-pitfalls.md` in the skill's `references/` directory:

```markdown
### Pitfall: [Short name]
**Symptom:** What Claude does wrong
**Cause:** Why it happens
**Fix:** What to do instead
**Added:** [date] by [your name]
```

This builds institutional knowledge over time -- the more pitfalls documented, the fewer repeated mistakes across the team.

---

## 6. MCP (Model Context Protocol)

MCP lets Claude Code connect to external services -- design tools, databases, project management -- without custom integration. Instead of copy-pasting between tools, Claude pulls context directly from the source.

**Example -- Figma:** Share a Figma URL and Claude reads the design specs directly, implements the UI, and can compare against the original design.

### Managing servers

Type `/mcp` to manage servers -- enable, disable, reconnect, or authenticate. You can also edit `settings.json` or `settings.local.json` directly.

Scope options:
- `--scope local` (default) -- your machine only
- `--scope project` -- checked into `.mcp.json`, shared with the team
- `--scope user` -- global across all projects

Full docs: https://code.claude.com/docs/en/mcp

---

## 7. Context Management

### Choosing a model

**Opus** is the default and should be used for all work -- especially planning, where deep reasoning matters most. If you have a solid plan and want faster execution, you can switch to **Sonnet** mid-session with `/model`.

### Working with context

Opus supports ~1M tokens of context -- large enough to hold entire block conversions in a single session. But context is finite. Manage it intentionally:

- **`/clear` between unrelated tasks.** Leftover context adds noise and reduces accuracy.
- **`/compact` to reclaim space.** Add a focus: `/compact keep the migration changes` preserves what matters.
- **Reference files with `@filename`** instead of pasting contents into prompts.
- **`/context`** to see what's consuming space.

### Statusline

Optional but worth setting up. Pins context usage, model, git branch, cost, and elapsed time to the bottom of your terminal so you always know where you stand. Run `/statusline` to configure.

Full docs: https://code.claude.com/docs/en/statusline

---

## 8. Resources

### Core references

The principles and guidelines behind this setup were shaped by these resources:

- [Claude Code Best Practices](https://code.claude.com/docs/en/best-practices) -- Prompting, context management, and verification loops
- [The Complete Guide to Building Skills for Claude](https://resources.anthropic.com/hubfs/The-Complete-Guide-to-Building-Skill-for-Claude.pdf) -- Skill design, structure, and patterns
- [Real-world Claude Code workflows](https://x.com/trq212/status/2033949937936085378) -- Practical patterns from production use

### Community & extensions

The Claude Code ecosystem is growing quickly. These are worth exploring:

- [Anthropic Skills](https://github.com/anthropics/skills/) -- Anthropic's official skill library covering code review, TDD, security, and more
- [Everything Claude Code](https://github.com/affaan-m/everything-claude-code) -- Comprehensive collection of skills, agents, rules, and multi-agent patterns
- [Skills Marketplace](https://skillsmp.com/) -- Searchable directory of community-contributed skills
