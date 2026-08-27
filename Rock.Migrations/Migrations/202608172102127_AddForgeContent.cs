// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Rock.Security;

    /// <summary>
    /// Adds the ForgeContent table and the Forge Content block, then seeds the
    /// skills that build Forge Content and the two agents that expose
    /// them: the Code Composer MCP Agent for external AI clients and the Code Composer Chat Agent for
    /// Rock's own chat.
    /// </summary>
    public partial class AddForgeContent : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The EntityType Guid for the <c>Rock.Blocks.Cms.ForgeContentDetail</c> block.
        /// </summary>
        private const string BlockEntityTypeGuid = "8C7E29E5-E2C5-4331-B7F7-06EF894E7316";

        /// <summary>
        /// The BlockType Guid for the Forge Content block.
        /// </summary>
        private const string BlockTypeGuid = "D4A5F720-493C-4DE8-B4B6-D6667D7ED2A2";

        /// <summary>
        /// The Guid of the Code Composer MCP Agent AIAgent row.
        /// </summary>
        private const string CodeComposerMcpAgentGuid = "DC44435A-8900-4AB4-9EB3-1756FCC1B355";

        /// <summary>
        /// The MCP slug the Code Composer MCP Agent is served under (/api/v2/mcp/code-composer).
        /// </summary>
        private const string CodeComposerMcpAgentSlug = "code-composer";

        /// <summary>
        /// The Guid of the Code Composer Chat Agent AIAgent row.
        /// </summary>
        private const string CodeComposerChatAgentGuid = "5A2BC280-C12E-4C13-AA1F-D169DB27D3FE";

        /// <summary>
        /// The AISkill Guid of the Cms skill.
        /// </summary>
        private const string CmsSkillGuid = "613D7110-6453-4BAB-892B-064222F8397C";

        /// <summary>
        /// The AISkill Guid of the Code Builder skill.
        /// </summary>
        private const string CodeBuilderSkillGuid = "647770A9-F3D7-4924-B046-5C9C43959ECB";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CmsSkill</c>.
        /// </summary>
        private const string CmsSkillEntityTypeGuid = "7A63570D-6FC3-4573-BDF2-89CFF605D5AB";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CodeBuilderSkill</c>.
        /// </summary>
        private const string CodeBuilderSkillEntityTypeGuid = "4C833FA4-A7EF-4D49-9549-B24CBB629A73";

        /// <summary>
        /// The AISkill Guid of the Community Knowledge Base skill.
        /// </summary>
        private const string CommunityKnowledgeBaseSkillGuid = "DFCBFDE8-6BF2-4DDF-81FE-FDD436E5FD90";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CommunityKnowledgeBaseSkill</c>.
        /// </summary>
        private const string CommunityKnowledgeBaseEntityTypeGuid = "959F0B92-A3BB-4AAA-9143-CF7D77895392";

        #endregion Constants

        #region Instructions

        /// <summary>
        /// The instructions sent to an MCP client when it connects. Deliberately
        /// terse: this text lands in the client's context alongside every tool
        /// description, so it carries only what the tool metadata cannot. Per-tool
        /// rules live in the skills' AgentUsage attributes.
        /// </summary>
        private const string CodeComposerMcpAgentInstructions = @"# Persona

You build custom UI inside this Rock instance: a page, a Forge Content block on it, the Vue component that block renders, and the Lava endpoints feeding that component. Everything is stored in the database. There is no repository file and no build step.

# Guardrails

Ask before building: what it shows, who the audience is, which parent page it lives under, how the data is scoped, and roughly what it should look like. Ask in one message, not one at a time. If the user says to just build something, pick defaults, state them, and produce a first version they can react to.

Confirm the parent page, route, and block type before creating anything. Those change site structure.

A successful save means the source compiled. It does not mean the component works. Never report otherwise.

# Design Lock-In

If the user has not described the appearance concretely, do not build yet. Offer a short menu of design shapes with a one-line plain-English description each: a stat-card dashboard with a chart, a searchable table with a detail view, a single chart over time, a data entry form, or a kanban board. The user may pick one or describe their own; a design does not have to match a shape. Once the design is locked in, state your plan in two or three lines (the Panel decision, the grid mode, the key controls), then build without waiting unless the user objects. A locked-in design plus a fast first version beats a long questionnaire.

# Coding Conventions

Consult the Coding Guide topic in the community knowledge base before writing any code, and prefer what it says over your own knowledge or any other source. Before writing any component source, read the Composition Rules article: call GetArticle with articleKey 'coding-guide/conventions-and-guardrails/composition-rules'. It governs which control to use, in which mode, composed how; do not author UI without it in context this session. Retrieval keys inside the guide are stable; if that key ever returns not found, locate the article through GetTopic rather than guessing a new key. If the locked-in design matches one of the guide's recipes, also read that recipe and follow its Composition table. A recipe never overrides the Composition Rules, and a design with no matching recipe is built directly from the rules plus control lookups.

# Build Order

GetRockVersion, then SearchPages and AddOrUpdatePage (pass a kebab-case route), then AddOrUpdateBlock with the ""Forge Content"" block type resolved through ListBlockTypes. Keep the block id it returns. Read the Composition Rules article (and the matching recipe when one exists), look up your controls in the knowledge base, create the Lava application with AddOrUpdateLavaApplication and its endpoints under that one slug, then AddOrUpdateForgeContent.

# Authoring Contract

Plain JavaScript only. `lang=""ts""` is not supported and nothing strips types, so remove every annotation when adapting a repo `.obs` file.

Imports must be plain top-level `import X from ""path"";` statements. Side-effect and dynamic imports do not resolve.

Import from `@Obsidian/*` (Controls, Core, Directives, Enums, FieldTypes, Libs, PageState, SystemGuids, Templates, Utility, ValidationRules) plus `vue`, `axios`, `luxon`, `mitt`, `ant-design-vue`, `tslib`. `@Obsidian/ViewModels/*` is unavailable because repo blocks import those as types only.

# After Saving

Give the user the page URL and tell them to check it as a normal member, not as an administrator. Components run as whoever views the page, and a new Lava application has no security rules until someone adds them, so your data may be missing or over-shared for everyone else. If they report a problem, GetForgeContent, fix it, and save again.";

        /// <summary>
        /// The instructions for the chat agent. Same persona, guardrails, build
        /// order, and authoring contract as the MCP agent, reworked for Rock's own
        /// chat: markdown presentation guidance per the Staff Agent precedent, a
        /// plain statement for the unconfigured-provider state, and an explicit
        /// Control Discovery section because chat models tend to skip tool-level
        /// usage guidance more than MCP clients do.
        /// </summary>
        private const string CodeComposerChatAgentInstructions = @"# Persona

You build custom UI inside this Rock instance: a page, a Forge Content block on it, the Vue component that block renders, and the Lava endpoints feeding that component. Everything is stored in the database. There is no repository file and no build step.

If you cannot act at all, this instance's AI provider may not be configured yet; say so plainly rather than guessing at a cause.

# Guardrails

Ask before building: what it shows, who the audience is, which parent page it lives under, how the data is scoped, and roughly what it should look like. Ask in one message, not one at a time. If the user says to just build something, pick defaults, state them, and produce a first version they can react to.

Confirm the parent page, route, and block type before creating anything. Those change site structure.

A successful save means the source compiled. It does not mean the component works. Never report otherwise.

# Design Lock-In

If the user has not described the appearance concretely, do not build yet. Offer a short menu of design shapes with a one-line plain-English description each: a stat-card dashboard with a chart, a searchable table with a detail view, a single chart over time, a data entry form, or a kanban board. The user may pick one or describe their own; a design does not have to match a shape. Once the design is locked in, state your plan in two or three lines (the Panel decision, the grid mode, the key controls), then build without waiting unless the user objects. A locked-in design plus a fast first version beats a long questionnaire.

# Coding Conventions

Consult the Coding Guide topic in the community knowledge base before writing any code, and prefer what it says over your own knowledge or any other source. Before writing any component source, read the Composition Rules article: call GetArticle with articleKey 'coding-guide/conventions-and-guardrails/composition-rules'. It governs which control to use, in which mode, composed how; do not author UI without it in context this session. Retrieval keys inside the guide are stable; if that key ever returns not found, locate the article through GetTopic rather than guessing a new key. If the locked-in design matches one of the guide's recipes, also read that recipe and follow its Composition table. A recipe never overrides the Composition Rules, and a design with no matching recipe is built directly from the rules plus control lookups.

# Control Discovery

Use the Community Knowledge Base tools before writing a component. Call GetKnowledgeBaseOverview once to learn the stores and filters, SearchKnowledge for guidance and patterns, and SearchCode with sourceType 'obs' to find controls by concept, then read a control's real API with GetCodeFile. The defineProps block is the authoritative list of props; never infer a control's props from its name or from a different control, and scope lookups to the version GetRockVersion reports. If the knowledge base tools are unavailable or failing, say so, do not guess props, and build with plain HTML and Rock utility classes instead.

# Build Order

GetRockVersion, then SearchPages and AddOrUpdatePage (pass a kebab-case route), then AddOrUpdateBlock with the ""Forge Content"" block type resolved through ListBlockTypes. Keep the block id it returns. Read the Composition Rules article (and the matching recipe when one exists), create the Lava application with AddOrUpdateLavaApplication and its endpoints under that one slug, then AddOrUpdateForgeContent.

# Authoring Contract

Plain JavaScript only. `lang=""ts""` is not supported and nothing strips types, so remove every annotation when adapting a repo `.obs` file.

Imports must be plain top-level `import X from ""path"";` statements. Side-effect and dynamic imports do not resolve.

Import from `@Obsidian/*` (Controls, Core, Directives, Enums, FieldTypes, Libs, PageState, SystemGuids, Templates, Utility, ValidationRules) plus `vue`, `axios`, `luxon`, `mitt`, `ant-design-vue`, `tslib`. `@Obsidian/ViewModels/*` is unavailable because repo blocks import those as types only.

# Presentation

You are chatting inside Rock, so make a pleasant UX using markdown: short sections with headers, bold what matters, tables when listing 4 or more items, and one line per build step when reporting progress. Link to the pages you create so the user can open them.

# After Saving

Give the user the page URL and tell them to check it as a normal member, not as an administrator. Components run as whoever views the page, and a new Lava application has no security rules until someone adds them, so your data may be missing or over-shared for everyone else. If they report a problem, GetForgeContent, fix it, and save again.";

        #endregion Instructions

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ForgeContent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlockId = c.Int(),
                        Name = c.String(maxLength: 100),
                        Source = c.String(),
                        CompiledContent = c.String(),
                        CompiledVueVersion = c.String(maxLength: 50),
                        CompiledDateTime = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Block", t => t.BlockId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BlockId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RegisterForgeContentEntityTypes_Up();
            AddForgeContentBlockType_Up();
            AddCmsSkill_Up();
            AddCodeBuilderSkill_Up();
            AddCommunityKnowledgeBaseSkill_Up();
            AddCodeComposerMcpAgent_Up();
            AddCodeComposerChatAgent_Up();
            AttachSkillsToAgents_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /*
                8/17/2026 - CLAUDE

                The agent row and its security are deliberately left in place: an
                administrator may have retuned the instructions or enabled tools,
                and there is no IsSystem flag on AIAgent to protect that work, so
                a downgrade must not discard it. The skill and tool rows are
                removed because startup registration recreates them on the next
                run of a build that still contains the skill classes.

                The EntityType rows are also left in place. They are recreated by
                startup registration, carry no configuration of their own, and
                deleting them would orphan anything else that came to reference
                them.

                Reason: A downgrade must not destroy administrator tuning of the agent.
            */
            RemoveSkillsAndTools_Down();
            AddForgeContentBlockType_Down();

            DropForeignKey("dbo.ForgeContent", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ForgeContent", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ForgeContent", "BlockId", "dbo.Block");
            DropIndex("dbo.ForgeContent", new[] { "Guid" });
            DropIndex("dbo.ForgeContent", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ForgeContent", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ForgeContent", new[] { "BlockId" });
            DropTable("dbo.ForgeContent");
        }

        #region Entity Types and Block Type

        /// <summary>
        /// Registers the EntityTypes this migration references. Startup EntityType
        /// registration runs after migrations, so anything the seeding below joins
        /// against must be registered explicitly here first: a missing EntityType
        /// yields a null CodeEntityTypeId and a skill that exposes nothing, and
        /// AddOrUpdateEntityBlockType silently no-ops without the block EntityType.
        /// </summary>
        private void RegisterForgeContentEntityTypes_Up()
        {
            RockMigrationHelper.UpdateEntityType(
                "Rock.Model.ForgeContent",
                "Forge Content",
                "Rock.Model.ForgeContent, Rock, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null",
                true,
                true,
                SystemGuid.EntityType.FORGE_CONTENT );

            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Cms.ForgeContentDetail",
                "Forge Content",
                "Rock.Blocks.Cms.ForgeContentDetail, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                BlockEntityTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CmsSkill",
                CmsSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CodeBuilderSkill",
                CodeBuilderSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CommunityKnowledgeBaseSkill",
                CommunityKnowledgeBaseEntityTypeGuid,
                false,
                false );
        }

        /// <summary>
        /// Registers the entity-based (Obsidian) block type. The path-based
        /// UpdateBlockTypeByGuid is intentionally avoided because it deletes by
        /// path and can wipe entity-based block types.
        /// </summary>
        private void AddForgeContentBlockType_Up()
        {
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Forge Content",
                "Renders an authored forge content, compiled on the server, and lets an administrator edit it in place.",
                "Rock.Blocks.Cms.ForgeContentDetail",
                "CMS",
                BlockTypeGuid );
        }

        /// <summary>
        /// Removes the Forge Content block type.
        /// </summary>
        private void AddForgeContentBlockType_Down()
        {
            RockMigrationHelper.DeleteBlockType( BlockTypeGuid );
        }

        #endregion Entity Types and Block Type

        #region Skills

        /*
            8/17/2026 - CLAUDE

            Every seeded skill name below is exactly the class name split-cased, and
            every seeded description is exactly the class or method [Description]
            text, character for character. Startup re-registration derives those
            values from the classes and overwrites the rows on every application
            start, so any drift here lasts only until the first restart and then
            silently disappears.

            Reason: Seeded values must match what startup registration derives.
        */

        /// <summary>
        /// Registers the Cms skill, its tools, and administrator-only security
        /// on the two mutating tools. The skill itself carries no security
        /// rules: its read tools (LookupSites and friends) are usable by any
        /// audience, so the lockdown is per tool rather than per skill.
        /// </summary>
        private void AddCmsSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Cms Skill",
                "Explore and manage sites, pages, and blocks in Rock's CMS.",
                CmsSkillEntityTypeGuid,
                CmsSkillGuid );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Lookup Sites",
                "Retrieves all configured websites in Rock.",
                "6234BB68-99B8-4B7C-884D-0D760B1F081C" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Get Site",
                "Gets the details of a single site, including its theme, default page, and login page.",
                "16C84C00-62DC-4AE9-9A85-F7CDE7D20FC8" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Pages",
                "Lists one level of the page tree: the root pages of every site, or the immediate children of a parent page. Call repeatedly with a returned page's IdKey to walk deeper.",
                "1F7C1F00-F481-468A-860F-314D1B43A477" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Pages For Site",
                "Lists every page belonging to one site as a flat list. Each page includes its parent page so the hierarchy can be reconstructed.",
                "8968B4EF-3A1D-472A-9BC6-17A80B8F824F" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Search Pages",
                "Searches CMS pages by a partial name match so a page can be resolved and confirmed with the user before adding a child page under it or adding a block to it.",
                "C668CAE0-CFA7-4AFF-87FF-5025860170BA" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Get Page",
                "Gets the details of a single page, including its routes, layout, and the blocks already placed on it.",
                "E2CFF69F-C4B2-47F5-B322-4041D841F37C" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Add Or Update Page",
                "Adds a new child page under a parent page or updates an existing page. New pages inherit the parent's layout unless a layout is specified. Pass a kebab-case route so the page gets a friendly URL.",
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Layouts",
                "Lists the layouts pages can render with, optionally filtered to one site. Returns the layoutIdKey that AddOrUpdatePage accepts.",
                "82C06D71-800E-4064-B72D-98F1B2A684D7" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Block Types",
                "Lists the block types available to place on a page, filtered by a partial name or category. Returns the blockTypeIdKey that AddOrUpdateBlock needs.",
                "F9A5AC4D-E40C-4FAF-895D-8C0E10A37EEC" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "List Blocks",
                "Lists the blocks placed on a page, a layout, or a site. At least one filter is required.",
                "98F33433-0712-4248-9C71-EAE4D9F9CA38" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Add Or Update Block",
                "Adds a block to a page, layout, or site, or updates an existing block. Returns the block's IdKey, which is the block id the Code Builder skill's AddOrUpdateForgeContent tool needs.",
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Delete Page",
                "Deletes a page along with its blocks, routes, and optionally its interaction history. Pages with child pages are refused; delete or move the children first.",
                "BB6C42F3-C448-49D5-BB85-4072960178FC" );

            AddOrUpdateCodeAISkillTool(
                CmsSkillGuid,
                "Delete Block",
                "Deletes a block from its page, layout, or site, along with any Forge Content stored against it.",
                "B30F66EA-0D9E-4854-BB82-A96BE7719D00" );

            // Only the mutating tools are locked to administrators. The read
            // tools stay visible and rely on per-person VIEW filtering inside
            // each tool.
            AddAdministratorOnlySecurityForAISkillTool(
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633", // Add Or Update Page
                "4A95B31D-A629-4F76-B68A-6D74B7C578EE",
                "C2D66CDA-336D-4D1E-BF29-B2960A25AAB6" );

            AddAdministratorOnlySecurityForAISkillTool(
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF", // Add Or Update Block
                "3FC10ED8-FF27-4B28-9C11-19B4F1993A80",
                "70241AD4-ED95-4757-A19D-ECD27FDB430A" );

            AddAdministratorOnlySecurityForAISkillTool(
                "BB6C42F3-C448-49D5-BB85-4072960178FC", // Delete Page
                "7483110B-1155-45FC-A7F7-B77959DB3982",
                "A4118437-30B1-48C7-88D9-89E34E0C4B46" );

            AddAdministratorOnlySecurityForAISkillTool(
                "B30F66EA-0D9E-4854-BB82-A96BE7719D00", // Delete Block
                "AFBD660A-35C7-48BB-8A82-15099CF595AE",
                "557884F7-F369-4FE5-9F18-6C41FBE13900" );
        }

        /// <summary>
        /// Registers the Code Builder skill, its security, and its tools: the
        /// Forge Content authoring tools plus the Lava application tools it
        /// absorbed when the former Custom Component and Lava Application
        /// skills merged. The skill keeps the former Custom Component skill's
        /// guids, and the Lava tool rows re-parent onto it by guid.
        /// </summary>
        private void AddCodeBuilderSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Code Builder Skill",
                "Author the Vue source rendered by a Forge Content block placement and the Lava applications and endpoints that feed it data.",
                CodeBuilderSkillEntityTypeGuid,
                CodeBuilderSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                CodeBuilderSkillGuid,
                "4E646B3E-E483-48C0-9B5B-5D01FEFC2406",
                "E0BE5EA7-452F-4FF8-9552-545F3EDE58FC" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Get Rock Version",
                "Reports the Rock version this instance is running, so control and API lookups can be scoped to the release actually deployed here.",
                "3E7A1C42-8B95-4D06-A1F3-2C64D9B7E508" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Get Forge Content",
                "Reads the current authored source of a Forge Content block placement so it can be iterated on.",
                "7D3A8200-3A90-44CC-9E30-B600383E835F" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Add Or Update Forge Content",
                "Compiles and saves the authored source of a Forge Content block placement. A failed compile stores nothing and returns the compiler's errors.",
                "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Add Or Update Lava Application",
                "Adds a new Lava application or updates one this skill created. Applications group a block's endpoints and must exist before endpoints can be added.",
                "A82B55AE-16A6-4321-95E1-59762C7CED14" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Get Lava Application",
                "Reads a Lava application and lists its endpoints so existing work can be discovered before adding more.",
                "9A078C57-946C-4D5F-8EBE-5009E6390EF2" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Add Or Update Lava Endpoint",
                "Adds a new Lava endpoint or updates an existing one, keyed by slug and HTTP method, within an existing Lava application. Returns the result of test-executing the template.",
                "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Get Lava Endpoint",
                "Reads the current template and configuration of a Lava endpoint so it can be iterated on.",
                "11AE1557-1EF3-4E03-9E8E-FCF99F72FCD9" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Delete Lava Endpoint",
                "Deletes a Lava endpoint this skill previously created, so exploration and diagnostics can clean up after themselves.",
                "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9" );

            AddOrUpdateCodeAISkillTool(
                CodeBuilderSkillGuid,
                "Delete Lava Application",
                "Deletes a Lava application this skill previously created, along with any endpoints it created inside it.",
                "9A47C2D1-83B5-4E60-A7F3-1B58C90D24E6" );
        }

        /// <summary>
        /// Registers the Community Knowledge Base skill and its tools. The skill
        /// itself carries no security rules: every tool is read-only and both
        /// agents that receive it are already administrator-gated.
        /// </summary>
        private void AddCommunityKnowledgeBaseSkill_Up()
        {
            /*
                8/18/2026 - CLAUDE

                This skill ships in core, but core registers it only at startup and
                startup runs after migrations, so at this migration's point in time
                its AISkill row does not exist and attaching it below would
                silently no-op. Seeding it here is idempotent: the names and
                descriptions match the class exactly, so startup re-registration
                is a no-op, and if core later seeds it in its own migration that
                upsert lands on the same rows.

                Reason: Attach depends on rows that startup has not created yet.
            */
            AddOrUpdateCodeAISkill(
                "Community Knowledge Base Skill",
                "Provides access to the Rock community knowledge base: product documentation, community content, the Rock source code, and curated topic guides.",
                CommunityKnowledgeBaseEntityTypeGuid,
                CommunityKnowledgeBaseSkillGuid );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Get Knowledge Base Overview",
                "Describes everything the Rock community knowledge base holds: its knowledge sources and their document counts, which Rock releases have source code indexed, the curated topics available and how to open them, the valid values for every search filter, and guidance on which store answers which kind of question.",
                "7D3ED0C6-6B02-42F5-AB34-4815FE7FF00C" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Search Knowledge",
                "Searches Rock documentation, guides, and community content using combined keyword and meaning-based matching. This is the right first move for almost every question about Rock.",
                "2A6D26DA-F889-4AD7-B9F2-B26B80902229" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Search Code",
                "Searches the Rock source code by meaning, to find which files implement a given behavior. Returns file locations and metadata only, never code.",
                "A60CA1BC-5E68-481B-8561-27F6AE57D500" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Grep Code",
                "Finds exact text or a regular expression in the Rock source, returning each matching line with its line number and surrounding context.",
                "D0EA7BC3-3DAF-4481-A1B0-483FE1A4834E" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Get Code File",
                "Reads one Rock source file in full.",
                "90764482-BA27-4FC0-B9CE-1585F07A6C64" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Get Code Lines",
                "Reads a range of lines from one Rock source file.",
                "DB33743D-B2A7-4CD8-A6BA-9576EA83DD35" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Get Topic",
                "Returns one topic's table of contents: its guidance plus its top-level articles, each with the key needed to retrieve it.",
                "F0179643-6979-416B-8D30-E45CBD96E49E" );

            AddOrUpdateCodeAISkillTool(
                CommunityKnowledgeBaseSkillGuid,
                "Get Article",
                "Returns one article's full content along with the keys of its child articles.",
                "BCE7AD22-3768-4DEE-A2E1-71BC324905EE" );
        }

        /// <summary>
        /// Removes the seeded skill links, security, tools, and skills. Runs before
        /// the block type and table are removed.
        /// </summary>
        private void RemoveSkillsAndTools_Down()
        {
            var skillGuids = $"'{CmsSkillGuid}', '{CodeBuilderSkillGuid}'";

            // The Community Knowledge Base skill's links are removed with the
            // others, but its AISkill and AISkillTool rows are deliberately NOT
            // deleted below: the skill belongs to core, and another agent may
            // have attached it by the time this migration is rolled back.
            Sql( $@"
DELETE FROM [AIAgentSkill]
WHERE [AIAgentId] IN (SELECT [Id] FROM [AIAgent] WHERE [Guid] IN ('{CodeComposerMcpAgentGuid}', '{CodeComposerChatAgentGuid}'))
    AND [AISkillId] IN (SELECT [Id] FROM [AISkill] WHERE [Guid] IN ({skillGuids}, '{CommunityKnowledgeBaseSkillGuid}'))" );

            RockMigrationHelper.DeleteSecurityAuth( "4A95B31D-A629-4F76-B68A-6D74B7C578EE" );
            RockMigrationHelper.DeleteSecurityAuth( "C2D66CDA-336D-4D1E-BF29-B2960A25AAB6" );
            RockMigrationHelper.DeleteSecurityAuth( "3FC10ED8-FF27-4B28-9C11-19B4F1993A80" );
            RockMigrationHelper.DeleteSecurityAuth( "70241AD4-ED95-4757-A19D-ECD27FDB430A" );
            RockMigrationHelper.DeleteSecurityAuth( "7483110B-1155-45FC-A7F7-B77959DB3982" );
            RockMigrationHelper.DeleteSecurityAuth( "A4118437-30B1-48C7-88D9-89E34E0C4B46" );
            RockMigrationHelper.DeleteSecurityAuth( "AFBD660A-35C7-48BB-8A82-15099CF595AE" );
            RockMigrationHelper.DeleteSecurityAuth( "557884F7-F369-4FE5-9F18-6C41FBE13900" );
            RockMigrationHelper.DeleteSecurityAuth( "4E646B3E-E483-48C0-9B5B-5D01FEFC2406" );
            RockMigrationHelper.DeleteSecurityAuth( "E0BE5EA7-452F-4FF8-9552-545F3EDE58FC" );

            Sql( $@"
DELETE FROM [AISkillTool]
WHERE [AISkillId] IN (SELECT [Id] FROM [AISkill] WHERE [Guid] IN ({skillGuids}))

DELETE FROM [AISkill]
WHERE [Guid] IN ({skillGuids})" );
        }

        #endregion Skills

        #region Agent

        /// <summary>
        /// Creates the Code Composer MCP Agent as an MCP server, if it does not already exist.
        /// </summary>
        private void AddCodeComposerMcpAgent_Up()
        {
            // Create-only, never update. An administrator may retune the
            // instructions, and re-running this migration must not discard that.
            // The one exception is the rename below: a row still carrying one of
            // the pre-release default names ('Vibe Agent', 'Vibe MCP Agent') has
            // not been tuned, so it is safe to bring in line with the shipped name.
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{CodeComposerMcpAgentGuid}')
BEGIN
    INSERT INTO [AIAgent] (
        [Name]
        , [Description]
        , [Instructions]
        , [AgentType]
        , [AudienceType]
        , [AdditionalSettingsJson]
        , [Guid]
    )
    VALUES (
        'Code Composer MCP Agent'
        , 'An MCP server that lets an AI client build custom UI in this instance: a page, a Forge Content block, the Vue component it renders, and the Lava endpoints feeding it.'
        , '{CodeComposerMcpAgentInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Mcp}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , '{{ ""McpAgentSettings"": {{ ""Slug"": ""{CodeComposerMcpAgentSlug}"", ""IsExcludingSystemSkills"": false }} }}'
        , '{CodeComposerMcpAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = 'Code Composer MCP Agent'
    WHERE [Guid] = '{CodeComposerMcpAgentGuid}'
        AND [Name] IN ('Vibe Agent', 'Vibe MCP Agent')
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                CodeComposerMcpAgentGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "7FB09F45-4FB1-45FE-A994-E130F6543078" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                CodeComposerMcpAgentGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "4D692452-3031-4854-A6AF-61A900C3D8A2" );
        }

        /// <summary>
        /// Creates the Code Composer Chat Agent, if it does not already exist. Same skills
        /// and tools as the MCP agent on Rock's own chat transport, following the
        /// Staff Agent precedent for chat agents: create-only (except the
        /// pre-release rename from 'Vibe Chat Agent'), no additional settings,
        /// markdown presentation guidance in the instructions.
        /// </summary>
        private void AddCodeComposerChatAgent_Up()
        {
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{CodeComposerChatAgentGuid}')
BEGIN
    INSERT INTO [AIAgent] (
        [Name]
        , [Description]
        , [Instructions]
        , [AgentType]
        , [AudienceType]
        , [Guid]
    )
    VALUES (
        'Code Composer Chat Agent'
        , 'A chat agent that builds custom UI in this instance from Rock''s own chat: a page, a Forge Content block, the Vue component it renders, and the Lava endpoints feeding it.'
        , '{CodeComposerChatAgentInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Chat}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , '{CodeComposerChatAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = 'Code Composer Chat Agent'
    WHERE [Guid] = '{CodeComposerChatAgentGuid}'
        AND [Name] = 'Vibe Chat Agent'
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                CodeComposerChatAgentGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "555E5B64-1F3F-4117-B108-20ADB95F8A04" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                CodeComposerChatAgentGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "0CCA182B-7827-4FE7-BA69-B63C79BDE4D3" );
        }

        /// <summary>
        /// The enabled tools of the Cms skill, shared by both agents.
        /// </summary>
        private static readonly string[] CmsSkillEnabledTools = new[]
        {
            "6234BB68-99B8-4B7C-884D-0D760B1F081C", // Lookup Sites
            "16C84C00-62DC-4AE9-9A85-F7CDE7D20FC8", // Get Site
            "1F7C1F00-F481-468A-860F-314D1B43A477", // List Pages
            "8968B4EF-3A1D-472A-9BC6-17A80B8F824F", // List Pages For Site
            "C668CAE0-CFA7-4AFF-87FF-5025860170BA", // Search Pages
            "E2CFF69F-C4B2-47F5-B322-4041D841F37C", // Get Page
            "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633", // Add Or Update Page
            "82C06D71-800E-4064-B72D-98F1B2A684D7", // List Layouts
            "F9A5AC4D-E40C-4FAF-895D-8C0E10A37EEC", // List Block Types
            "98F33433-0712-4248-9C71-EAE4D9F9CA38", // List Blocks
            "05C9C108-4516-46B7-85FB-5C8FE6212CCF", // Add Or Update Block
            "BB6C42F3-C448-49D5-BB85-4072960178FC", // Delete Page
            "B30F66EA-0D9E-4854-BB82-A96BE7719D00"  // Delete Block
        };

        /// <summary>
        /// The enabled tools of the Code Builder skill, shared by both agents:
        /// the Forge Content authoring tools plus the Lava application tools
        /// absorbed from the former Lava Application skill.
        /// </summary>
        private static readonly string[] CodeBuilderSkillEnabledTools = new[]
        {
            "3E7A1C42-8B95-4D06-A1F3-2C64D9B7E508", // Get Rock Version
            "7D3A8200-3A90-44CC-9E30-B600383E835F", // Get Forge Content
            "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8", // Add Or Update Forge Content
            "A82B55AE-16A6-4321-95E1-59762C7CED14", // Add Or Update Lava Application
            "9A078C57-946C-4D5F-8EBE-5009E6390EF2", // Get Lava Application
            "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C", // Add Or Update Lava Endpoint
            "11AE1557-1EF3-4E03-9E8E-FCF99F72FCD9", // Get Lava Endpoint
            "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9", // Delete Lava Endpoint
            "9A47C2D1-83B5-4E60-A7F3-1B58C90D24E6"  // Delete Lava Application
        };

        /// <summary>
        /// The enabled tools of the Community Knowledge Base skill, shared by both agents.
        /// </summary>
        private static readonly string[] CommunityKnowledgeBaseSkillEnabledTools = new[]
        {
            "7D3ED0C6-6B02-42F5-AB34-4815FE7FF00C", // Get Knowledge Base Overview
            "2A6D26DA-F889-4AD7-B9F2-B26B80902229", // Search Knowledge
            "A60CA1BC-5E68-481B-8561-27F6AE57D500", // Search Code
            "D0EA7BC3-3DAF-4481-A1B0-483FE1A4834E", // Grep Code
            "90764482-BA27-4FC0-B9CE-1585F07A6C64", // Get Code File
            "DB33743D-B2A7-4CD8-A6BA-9576EA83DD35", // Get Code Lines
            "F0179643-6979-416B-8D30-E45CBD96E49E", // Get Topic
            "BCE7AD22-3768-4DEE-A2E1-71BC324905EE"  // Get Article
        };

        /// <summary>
        /// Attaches the three skills to both agents with explicit enabled-tool
        /// lists. Attaching a skill alone does not expose its tools.
        /// </summary>
        private void AttachSkillsToAgents_Up()
        {
            AttachSkillToAgent( CodeComposerMcpAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( CodeComposerMcpAgentGuid, CodeBuilderSkillGuid, CodeBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerMcpAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools );

            AttachSkillToAgent( CodeComposerChatAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, CodeBuilderSkillGuid, CodeBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools );
        }

        #endregion Agent

        #region Helper Methods

        /// <summary>
        /// Links one skill to one agent with an explicit enabled-tool list.
        /// Attaching a skill alone does not expose its tools.
        /// </summary>
        /// <param name="agentGuid">The Guid of the AIAgent to attach the skill to.</param>
        /// <param name="skillGuid">The Guid of the AISkill to attach.</param>
        /// <param name="enabledToolGuids">The Guids of the tools to enable.</param>
        private void AttachSkillToAgent( string agentGuid, string skillGuid, string[] enabledToolGuids )
        {
            // Produces "guid", "guid". The value is interpolated into the verbatim
            // SQL below as-is, so it must already contain the double quotes.
            var enabledTools = string.Join( ", ", enabledToolGuids.Select( g => $"\"{g}\"" ) );

            Sql( $@"
DECLARE @AgentId INT = (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{agentGuid}')
DECLARE @SkillId INT = (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')

IF @AgentId IS NOT NULL AND @SkillId IS NOT NULL
    AND NOT EXISTS (SELECT [Id] FROM [AIAgentSkill] WHERE [AIAgentId] = @AgentId AND [AISkillId] = @SkillId)
BEGIN
    INSERT INTO [AIAgentSkill] (
        [AIAgentId]
        , [AISkillId]
        , [AdditionalSettingsJson]
        , [Guid]
    )
    VALUES (
        @AgentId
        , @SkillId
        , '{{ ""AgentSkillSettings"": {{ ""EnabledTools"": [{enabledTools}] }} }}'
        , NEWID()
    )
END" );
        }

        /// <summary>
        /// Grants VIEW on a skill to Rock administrators and denies it to everyone
        /// else. These skills write code and run privileged Lava, so they are not
        /// offered to staff at large the way the sample Staff Agent skills are.
        /// </summary>
        /// <param name="skillGuid">The Guid of the AISkill to secure.</param>
        /// <param name="allowAuthGuid">The Guid of the administrator allow rule.</param>
        /// <param name="denyAuthGuid">The Guid of the all-users deny rule.</param>
        private void AddAdministratorOnlySecurityForAISkill( string skillGuid, string allowAuthGuid, string denyAuthGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                allowAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                skillGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                denyAuthGuid );
        }

        /// <summary>
        /// Grants VIEW on a single tool to Rock administrators and denies it to
        /// everyone else. Used for mutating tools on a skill whose read tools
        /// stay open, so the lockdown lands on the tool rather than the skill.
        /// A tool a person is not authorized to VIEW is simply never offered to
        /// the model.
        /// </summary>
        /// <param name="toolGuid">The Guid of the AISkillTool to secure.</param>
        /// <param name="allowAuthGuid">The Guid of the administrator allow rule.</param>
        /// <param name="denyAuthGuid">The Guid of the all-users deny rule.</param>
        private void AddAdministratorOnlySecurityForAISkillTool( string toolGuid, string allowAuthGuid, string denyAuthGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAISkillTool(
                toolGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                allowAuthGuid );

            RockMigrationHelper.AddSecurityAuthForAISkillTool(
                toolGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                denyAuthGuid );
        }

        /// <summary>
        /// Adds or updates a code-based AI skill. Copied from hotfix 309, which is
        /// the established pattern for seeding these records.
        /// </summary>
        /// <param name="name">The name of the AI skill.</param>
        /// <param name="description">The user-friendly description of the AI skill.</param>
        /// <param name="codeEntityTypeGuid">The GUID of the code entity type.</param>
        /// <param name="skillGuid">The GUID of the AI skill.</param>
        private void AddOrUpdateCodeAISkill( string name, string description, string codeEntityTypeGuid, string skillGuid )
        {
            if ( codeEntityTypeGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( codeEntityTypeGuid ), "The code entity type guid must be a valid guid." );
            }

            if ( skillGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( skillGuid ), "The skill guid must be a valid guid." );
            }

            Sql( $@"
DECLARE @CodeEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{codeEntityTypeGuid}')

IF NOT EXISTS (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')
BEGIN
    INSERT INTO [AISkill] (
        [Name]
        , [Description]
        , [CodeEntityTypeId]
        , [Guid]
    )
    VALUES (
        '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , @CodeEntityTypeId
        , '{skillGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkill]
    SET [Name] = '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , [Description] = '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , [CodeEntityTypeId] = @CodeEntityTypeId
    WHERE [Guid] = '{skillGuid}'
END
" );
        }

        /// <summary>
        /// Adds or updates a code-based AI skill tool. Copied from hotfix 309,
        /// which is the established pattern for seeding these records.
        /// </summary>
        /// <param name="skillGuid">The GUID of the AI skill this tool belongs to.</param>
        /// <param name="name">The name of the AI skill tool.</param>
        /// <param name="description">The user-friendly description of the AI skill tool.</param>
        /// <param name="toolGuid">The GUID of the AI skill tool.</param>
        private void AddOrUpdateCodeAISkillTool( string skillGuid, string name, string description, string toolGuid )
        {
            if ( skillGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( skillGuid ), "The skill guid must be a valid guid." );
            }

            if ( toolGuid.AsGuidOrNull() == null )
            {
                throw new ArgumentOutOfRangeException( nameof( toolGuid ), "The tool guid must be a valid guid." );
            }

            Sql( $@"
DECLARE @SkillId INT = (SELECT [Id] FROM [AISkill] WHERE [Guid] = '{skillGuid}')

IF NOT EXISTS (SELECT [Id] FROM [AISkillTool] WHERE [Guid] = '{toolGuid}')
BEGIN
    INSERT INTO [AISkillTool] (
        [Name]
        , [Description]
        , [ToolType]
        , [AISkillId]
        , [Guid]
    )
    VALUES (
        '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , @SkillId
        , '{toolGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkillTool]
    SET [Name] = '{name?.Replace( "'", "''" ) ?? string.Empty}'
        , [Description] = '{description?.Replace( "'", "''" ) ?? string.Empty}'
        , [ToolType] = {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , [AISkillId] = @SkillId
    WHERE [Guid] = '{toolGuid}'
END
" );
        }

        #endregion Helper Methods
    }
}
