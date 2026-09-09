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

    /*
        8/31/2026 - CLAUDE

        Seeds the Code Composer agents: an MCP agent and a chat agent preconfigured
        with the skills that make up the authoring flow (Cms, Forge Content Builder,
        Lava Application Builder and Community Knowledge Base), plus the Forge
        Content block type they place on a page.

        This migration deliberately does NOT create the ForgeContent table or seed
        the Cms skill. Both belong to the earlier AddForgeContentAndCmsSkill EF
        migration; duplicating them here would fight that migration for ownership
        of the rows. The Cms skill's guid is still referenced below, because the
        agents have to attach the skill and enumerate its enabled tools.

        9/9/2026: this seeding originally shipped as plugin hotfix 999 while the
        feature was in development. It was folded into this EF migration so the
        feature ships as schema plus seeding in EF migrations only, with no
        hotfix numbering to coordinate.

        Three things here are easy to get wrong and are deliberate:

        1. The skill and block EntityTypes are registered explicitly before the rows
           that reference them. EntityType registration normally happens during
           application startup, which runs AFTER migrations, so on a fresh install
           the CodeEntityTypeId lookup would resolve to NULL and produce a skill
           that silently exposes no tools, and AddOrUpdateEntityBlockType would
           quietly no-op.

        2. Attaching a skill to an agent does NOT enable its tools. Each
           AIAgentSkill row carries an explicit EnabledTools allowlist in its
           AdditionalSettingsJson, so every tool guid has to be enumerated.

        3. The agent rows are created only when absent, never updated apart from the
           pre-release rename and the IsSystem flag. Both agents are system agents
           (IsSystem = 1), so they cannot be deleted and their instructions cannot
           be edited through the UI; the flag is also set on rows seeded by the
           earlier hotfix, which created them with IsSystem = 0. An administrator
           can still retune the enabled tools, so re-running this migration must not
           stomp the skill attachments. The skills and tools ARE upserted, because
           their names and descriptions are ours to correct.

           So that a later migration can still ship instruction fixes safely, each
           row's AdditionalSettingsJson carries the SHA-256 of the seeded
           Instructions text under SeededInstructionsHash. A future migration hashes
           the row's current Instructions, and updates the text (and the hash) only
           when the two match. A row whose text no longer matches was changed
           outside the UI and is left alone.

        Security is administrator-only rather than the staff-wide default the Staff
        Agent uses. These tools create pages, write code that runs in visitors'
        browsers, and can execute privileged Lava.

        Reason: Ship the Code Composer agents preconfigured instead of hand-assembled.
    */

    /// <summary>
    /// Adds the Code Composer MCP and chat agents, the Forge Content block type, and
    /// the code-based skills the agents carry.
    /// </summary>
    public partial class AddCodeComposer : Rock.Migrations.RockMigration
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
        /// The display name of the Code Composer MCP Agent. Carries the
        /// Experimental suffix while the feature is in preview.
        /// </summary>
        private const string CodeComposerMcpAgentName = "Code Composer MCP Agent (Experimental)";

        /// <summary>
        /// The display name of the Code Composer Chat Agent. Carries the
        /// Experimental suffix while the feature is in preview.
        /// </summary>
        private const string CodeComposerChatAgentName = "Code Composer Chat Agent (Experimental)";

        /// <summary>
        /// The MCP slug the Code Composer MCP Agent is served under (/api/v2/mcp/code-composer).
        /// </summary>
        private const string CodeComposerMcpAgentSlug = "code-composer";

        /// <summary>
        /// The Guid of the Code Composer Chat Agent AIAgent row.
        /// </summary>
        private const string CodeComposerChatAgentGuid = "5A2BC280-C12E-4C13-AA1F-D169DB27D3FE";

        /// <summary>
        /// The AISkill Guid of the Cms skill. The skill and its tools are seeded by
        /// the AddForgeContentAndCmsSkill EF migration; this migration only attaches
        /// the skill to the two agents.
        /// </summary>
        private const string CmsSkillGuid = "613D7110-6453-4BAB-892B-064222F8397C";

        /// <summary>
        /// The AISkill Guid of the Forge Content Builder skill.
        /// </summary>
        private const string ForgeContentBuilderSkillGuid = "0F3D6B8A-52C1-4E97-A6D3-84B2E7F91C05";

        /// <summary>
        /// The AISkill Guid of the Lava Application Builder skill.
        /// </summary>
        private const string LavaApplicationBuilderSkillGuid = "71B4E9D2-C685-4A30-BF17-5D208C4E96A1";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.ForgeContentBuilderSkill</c>.
        /// </summary>
        private const string ForgeContentBuilderSkillEntityTypeGuid = "6C2E94D7-1B58-4A3F-9E60-D74A5C813F29";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.LavaApplicationBuilderSkill</c>.
        /// </summary>
        private const string LavaApplicationBuilderSkillEntityTypeGuid = "94D07F3B-6E21-45C8-A5B4-1F8E3D62C079";

        /// <summary>
        /// The AISkill Guid of the Community Knowledge Base skill.
        /// </summary>
        private const string CommunityKnowledgeBaseSkillGuid = "DFCBFDE8-6BF2-4DDF-81FE-FDD436E5FD90";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CommunityKnowledgeBaseSkill</c>.
        /// </summary>
        private const string CommunityKnowledgeBaseEntityTypeGuid = "959F0B92-A3BB-4AAA-9143-CF7D77895392";

        /// <summary>
        /// The AdditionalSettingsJson key under which each agent row stores the
        /// SHA-256 of the Instructions text this migration seeded. A later
        /// migration compares it to the row's current text to decide whether the
        /// instructions are still untouched and may be updated.
        /// </summary>
        private const string SeededInstructionsHashKey = "SeededInstructionsHash";

        #endregion Constants

        #region Instructions

        /*
            9/8/2026 - CLAUDE

            The two agents share one instruction body and differ only by a
            short transport appendix, so the body is defined once and the two
            agent constants are concatenations. An earlier version kept two
            near-identical 1,000-word constants that had already begun to
            drift.

            The body carries workflow and policy only: what to establish, what
            to default, what to ask, the build order, what counts as done, and
            how to report. Tool mechanics (useLavaApp, entity commands, the
            coding guide route, the property evidence list) live on the skills
            and in result riders, where every client sees them, and are not
            repeated here.

            Reason: One source for the shared text; each rule in one layer.
        */

        /// <summary>
        /// The instruction body both Code Composer agents share.
        /// </summary>
        private const string SharedInstructionsBody = @"# Persona

You build custom UI inside this Rock instance: a page, a Forge Content block on it, the Vue component that block renders, and, when the component needs Rock data or server-side actions, the Lava endpoints feeding it. Everything is stored in the database. There is no repository file and no build step.

# Before You Build

Establish five things: what the feature shows or does, who it is for, which parent page it lives under, how the data is scoped, and roughly what it looks like. Use what the user already told you without reconfirming it. Ask one question only when the missing answer would change the structure, the security, or the result, and cannot be resolved from the instance or from the defaults below. Otherwise state the default you are taking and proceed.

Defaults you take without asking: the block type is Forge Content; the zone is Main; a new page's route is its kebab-case name; the Lava application slug is the kebab-case feature name; read endpoints use the application's audiences. Do not ask about block type or zone.

When the feature needs a Lava application, the one decision you never take for the user is its audience. When the user names people rather than a security role, call ResolveAudience with their words and confirm any match scored under 50. Never choose Public on your own. When an endpoint must be callable by a narrower or different audience than the rest of the application, give it its own audiences. A write endpoint does not require a separate audience merely because it writes. When the same people may read and write, let it inherit the application's audience. A static or client-only component needs no application and no audience decision. The CMS tools do not configure page or block authorization. Do not invent or search for such an operation, and do not treat its absence as a blocker. Protect data and actions through the Lava application and endpoint audiences, preserve the inherited page and block security, and report that boundary to the user.

When materially different design choices remain unresolved after the coding guide's design playbook has been consulted, present them briefly, one plain-English line each, and let the user pick. Skip this when the request already implies a shape. Then state your plan in two or three lines and continue building in the same turn.

# Build Order

1. Call GetRockVersion so lookups are interpreted against the release this instance runs. Its result also carries the pointer to the coding guide route in the knowledge base; follow that route before authoring anything, and retrieve only the material it assigns for this feature.
2. Inspect the target. Use SearchPages and GetPage to find the parent page and what is already on it. If the page, block, or application already exists from an earlier session, read it first (GetForgeContent, GetLavaApplication) and reuse or update it when the corresponding tool permits. Do not create duplicates.
3. Research what the route assigns: the controls the component needs, and for any endpoint that writes, the entity's schema article and the write procedure.
4. If the component requires Rock data or server-side actions, build the data layer first.
   New application: create it with AddOrUpdateLavaApplication, passing the audiences the user chose.
   Existing application: read it with GetLavaApplication first. If its audiences are already right, reuse it without changing it. Change its audiences only when the user requested a security change; passing audiences on an update replaces every existing ExecuteView rule, including ones an administrator added by hand, so state the current audience and the new one before you do. If you cannot administrate the application, ask whether the user wants to adjust it in the Lava Applications admin pages or have you create a separate application with a new slug. Never create a second application silently.
   Then create or update each endpoint with AddOrUpdateLavaEndpoint, passing testParameters shaped exactly like the payload the component will send.
5. Page and block.
   New page: call AddOrUpdatePage with the parent, the name, and a kebab-case route. Then resolve the Forge Content block type with ListBlockTypes and call AddOrUpdateBlock to place it in zone Main.
   Existing page: do not pass route or other page properties unless the user asked to change them; supplying a route replaces every route the page has. Use GetPage to find the existing Forge Content block and keep its type and zone. Add a block only when the page has none for this feature.
6. Author the component with AddOrUpdateForgeContent.
7. Verify the contract. For every invoke call in the component, compare the application slug, endpoint slug, HTTP method, parameter names and location, and expected response shape with the saved endpoint. Testing an endpoint alone does not prove the component calls it correctly.

Keep every IdKey the tools return; you will need them to update and to clean up.

# Authoring Contract

One single-file component per block, using <script setup>. There are no partial files.

Plain JavaScript only. lang=""ts"" is not supported and nothing strips types, so remove every annotation when adapting a repo .obs file.

Imports must be static, top-level import statements. Default, named, and namespace imports are supported. Side-effect and dynamic imports do not resolve.

Import from @Obsidian/* (Controls, Core, Directives, Enums, FieldTypes, Libs, PageState, SystemGuids, Templates, Utility, ValidationRules) plus vue, axios, luxon, mitt, ant-design-vue, tslib. @Obsidian/ViewModels/* is unavailable: ViewModel bags are TypeScript types with no runtime module, so use plain objects.

# What Done Means

A successful save means the source compiled. It does not mean the component works. Never report otherwise.

Never present hardcoded, mock, or sample data as if it were real instance data.

Missing examples, dedicated recipes, or exhaustive domain contracts are not blockers. Use the available schema, focused source evidence, and established Rock patterns to make a supported decision and continue. Uncertainty is a research task, not a stopping condition.

The assigned Rock tools cannot load the rendered page or automatically execute a write endpoint. Ask the user to verify the workflow: tell them which page to open, which record they should see and in which control, or which action to take and what persisted change to look for. Treat the workflow as unverified until they confirm the persisted result.

When a correctable operation fails, use the returned error to make a materially different correction and retry. Do not retry an authorization failure, a required user decision, or a platform configuration failure that another payload cannot resolve; report those at once. For other failures, stop after three materially different corrections when no safe supported alternative remains, and report the evidence: the operation, the error, each correction you tried, and the alternatives you ruled out. Offer to remove the pages, blocks, applications, and endpoints created solely by the failed attempt, and confirm before deleting anything. Never leave those shells as the reported result of a build.

# Reporting

Give the user the page URL. Tell them to check it as a representative non-administrator, because components and endpoints run with the viewer's permissions. When the feature has a Lava application, state who can call it (its readAudiences) and any endpoint that has its own audiences. Name the page and block security you left inherited and could not configure. If the user reports a problem, read the saved component and any endpoints, correct them, and verify again.
";

        /// <summary>
        /// Appended to the shared body for the chat agent: the unconfigured
        /// provider state and markdown presentation per the Staff Agent
        /// precedent.
        /// </summary>
        private const string ChatInstructionsAppendix = @"
# Chat

If you cannot act at all, this instance's AI provider may not be configured yet; say so plainly rather than guessing at a cause.

You are chatting inside Rock, so make a pleasant UX using markdown: short sections with headers, bold what matters, tables when listing 4 or more items, and one line per build step when reporting progress. Link to the pages you create so the user can open them.";

        /// <summary>
        /// Appended to the shared body for the MCP agent, whose client may
        /// bring its own tools.
        /// </summary>
        private const string McpInstructionsAppendix = @"
# MCP

Your client may have its own file, browser, or shell tools. When it provides a browser, use the rendered page to verify the workflow yourself before asking the user: a known record reaches its intended control, or an authorized action completes and shows its persisted result. Do not write component source to disk; the only place it runs is the Forge Content block, through AddOrUpdateForgeContent.";

        /// <summary>
        /// The instructions sent to an MCP client when it connects.
        /// </summary>
        private const string CodeComposerMcpAgentInstructions = SharedInstructionsBody + McpInstructionsAppendix;

        /// <summary>
        /// The instructions for the chat agent.
        /// </summary>
        private const string CodeComposerChatAgentInstructions = SharedInstructionsBody + ChatInstructionsAppendix;

        #endregion Instructions

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RegisterEntityTypes_Up();
            AddForgeContentBlockType_Up();
            AddForgeContentBuilderSkill_Up();
            AddLavaApplicationBuilderSkill_Up();
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
                9/9/2026 - CLAUDE

                The EntityType rows are deliberately left in place, matching the
                AddForgeContentAndCmsSkill migration: startup registration recreates
                them, they carry no configuration, and deleting them would orphan
                anything else that came to reference them. The agents are removed
                first because AIAgentSkill cascades from both AIAgent and AISkill,
                then the skills (which cascade to their tools), then the block type.
                The Cms skill is not touched here; it belongs to the earlier migration.

                Reason: A downgrade must remove what this migration seeded and nothing more.
            */
            RemoveCodeComposerAgents_Down();
            RemoveSkillsAndTools_Down();
            RemoveForgeContentBlockType_Down();
        }

        #region Entity Types and Block Type

        /// <summary>
        /// Registers the EntityTypes this migration references. Startup EntityType
        /// registration runs after migrations, so anything the seeding below joins
        /// against must be registered explicitly here first: a missing EntityType
        /// yields a null CodeEntityTypeId and a skill that exposes nothing, and
        /// AddOrUpdateEntityBlockType silently no-ops without the block EntityType.
        /// The ForgeContent and Cms skill EntityTypes are registered by the
        /// AddForgeContentAndCmsSkill EF migration and are not repeated here.
        /// </summary>
        private void RegisterEntityTypes_Up()
        {
            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Cms.ForgeContentDetail",
                "Forge Content",
                "Rock.Blocks.Cms.ForgeContentDetail, Rock.Blocks, Version=20.0.5.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                BlockEntityTypeGuid );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.ForgeContentBuilderSkill",
                ForgeContentBuilderSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.LavaApplicationBuilderSkill",
                LavaApplicationBuilderSkillEntityTypeGuid,
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
        /// Removes the Forge Content block type. Any Forge Content block placements
        /// must already be gone; the BlockType foreign key would otherwise block the
        /// delete, which is the same behavior every other block type Down has.
        /// </summary>
        private void RemoveForgeContentBlockType_Down()
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
        /// Registers the Forge Content Builder skill, its security, and its
        /// tools: the component authoring loop. The skill was briefly merged
        /// with the Lava Application Builder skill as one Code Builder skill;
        /// a side-by-side evaluation kept the split, so each skill carries
        /// focused guidance and its own guids.
        /// </summary>
        private void AddForgeContentBuilderSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Forge Content Builder Skill",
                "Author and edit the Vue source rendered by a Forge Content block placement.",
                ForgeContentBuilderSkillEntityTypeGuid,
                ForgeContentBuilderSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                ForgeContentBuilderSkillGuid,
                "5D3B7A20-94E6-4C18-B0F5-27A4D9C1E863",
                "A9E64C17-3B85-4D02-96E1-C50F8B27D4A9" );

            AddOrUpdateCodeAISkillTool(
                ForgeContentBuilderSkillGuid,
                "Get Rock Version",
                "Reports the Rock version this instance is running, so control and API lookups can be scoped to the release actually deployed here.",
                "8A51C3E9-674D-4B02-93F8-2E6B9D40A715" );

            AddOrUpdateCodeAISkillTool(
                ForgeContentBuilderSkillGuid,
                "Get Forge Content",
                "Reads the current authored source of a Forge Content block placement so it can be iterated on.",
                "D24F8B61-9C07-4E5A-B173-60A4F2C8E9D3" );

            AddOrUpdateCodeAISkillTool(
                ForgeContentBuilderSkillGuid,
                "Add Or Update Forge Content",
                "Compiles and saves the authored source of a Forge Content block placement. A failed compile stores nothing and returns the compiler's errors.",
                "3E97A0C5-48D2-4F16-85B9-C1D7E63A2F40" );
        }

        /// <summary>
        /// Registers the Lava Application Builder skill, its security, and its
        /// tools: the endpoint authoring loop that feeds Forge Content
        /// components their data.
        /// </summary>
        private void AddLavaApplicationBuilderSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Lava Application Builder Skill",
                "Create and edit Lava applications and endpoints that return JSON data to authored components.",
                LavaApplicationBuilderSkillEntityTypeGuid,
                LavaApplicationBuilderSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                LavaApplicationBuilderSkillGuid,
                "1C78E5B3-D40A-4F96-82D7-63B9A0F5C214",
                "F2A91D68-7C35-4E80-B41A-09D6E3C7825F" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Add Or Update Lava Application",
                "Adds a new Lava application or updates an existing one the current person can administrate. Applications group a block's endpoints and must exist before endpoints can be added.",
                "26C5F1A8-3D94-4E67-90B2-7A45D8E1C6F3" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Get Lava Application",
                "Reads a Lava application and lists its endpoints so existing work can be discovered before adding more.",
                "B83A2D64-15F7-4C09-8E51-9C3B6F04D7A2" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Resolve Audience",
                "Lists this instance's security roles and maps a plain-English description of who a page is for onto the exact audience values AddOrUpdateLavaApplication accepts.",
                "248182EA-2B76-4127-A864-BE48F8E15585" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Add Or Update Lava Endpoint",
                "Adds a new Lava endpoint or updates an existing one, keyed by slug and HTTP method, within an existing Lava application. Returns the result of test-executing the template.",
                "5F1E8C29-A47B-4D63-B905-E26A1D79F4C8" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Get Lava Endpoint",
                "Reads the current template and configuration of a Lava endpoint so it can be iterated on.",
                "E64B9F07-2C58-41DA-A83F-05D9C7B24E61" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Delete Lava Endpoint",
                "Deletes a Lava endpoint from an application the current person can administrate, so exploration and diagnostics can clean up after themselves.",
                "49A7D3E1-8F60-4B25-96C4-B1E5A08D3F72" );

            AddOrUpdateCodeAISkillTool(
                LavaApplicationBuilderSkillGuid,
                "Delete Lava Application",
                "Deletes a Lava application the current person can administrate, along with every endpoint inside it.",
                "C08E5A93-D1B6-4F74-82D0-46F3C9E17B58" );
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
                "Searches Rock documentation, guides, and community content using combined keyword and meaning-based matching. Use a versioned data-model topic instead when exact entity fields, nullability, enums, or relationships are required.",
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
        /// Removes the seeded security, tools, and skills for the Forge Content
        /// Builder, Lava Application Builder, and Community Knowledge Base skills.
        /// Deleting an AISkill cascades to its AISkillTool and AIAgentSkill rows.
        /// Startup registration recreates the skills on the next run of a build
        /// that still contains the skill classes.
        /// </summary>
        private void RemoveSkillsAndTools_Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "5D3B7A20-94E6-4C18-B0F5-27A4D9C1E863" );
            RockMigrationHelper.DeleteSecurityAuth( "A9E64C17-3B85-4D02-96E1-C50F8B27D4A9" );
            RockMigrationHelper.DeleteSecurityAuth( "1C78E5B3-D40A-4F96-82D7-63B9A0F5C214" );
            RockMigrationHelper.DeleteSecurityAuth( "F2A91D68-7C35-4E80-B41A-09D6E3C7825F" );

            Sql( $@"
DELETE FROM [AISkill]
WHERE [Guid] IN (
    '{ForgeContentBuilderSkillGuid}'
    , '{LavaApplicationBuilderSkillGuid}'
    , '{CommunityKnowledgeBaseSkillGuid}'
)" );
        }

        #endregion Skills

        #region Agent

        /// <summary>
        /// Creates the Code Composer MCP Agent as an MCP server, if it does not already exist.
        /// </summary>
        private void AddCodeComposerMcpAgent_Up()
        {
            // Create-only, never update, apart from two corrections to rows seeded
            // by earlier pre-release builds: a row still carrying one of the earlier
            // default names ('Vibe Agent', 'Vibe MCP Agent', 'Code Composer MCP
            // Agent') has not been renamed by hand, so it is safe to bring in line
            // with the shipped name; and every existing row is marked as a system
            // agent, because the hotfix created it with IsSystem = 0.
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{CodeComposerMcpAgentGuid}')
BEGIN
    INSERT INTO [AIAgent] (
        [Name]
        , [Description]
        , [Instructions]
        , [AgentType]
        , [AudienceType]
        , [IsSystem]
        , [AdditionalSettingsJson]
        , [Guid]
    )
    VALUES (
        '{CodeComposerMcpAgentName}'
        , 'An MCP server that lets an AI client build custom UI in this instance: a page, a Forge Content block, the Vue component it renders, and the Lava endpoints feeding it.'
        , '{CodeComposerMcpAgentInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Mcp}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , 1
        , '{{ ""McpAgentSettings"": {{ ""Slug"": ""{CodeComposerMcpAgentSlug}"", ""IsExcludingSystemSkills"": false }}, ""{SeededInstructionsHashKey}"": ""{GetSha256Hex( CodeComposerMcpAgentInstructions )}"" }}'
        , '{CodeComposerMcpAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = '{CodeComposerMcpAgentName}'
    WHERE [Guid] = '{CodeComposerMcpAgentGuid}'
        AND [Name] IN ('Vibe Agent', 'Vibe MCP Agent', 'Code Composer MCP Agent')

    UPDATE [AIAgent]
    SET [IsSystem] = 1
    WHERE [Guid] = '{CodeComposerMcpAgentGuid}'
        AND [IsSystem] = 0
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
        /// pre-release rename and the IsSystem correction, as for the MCP agent),
        /// no additional settings, markdown presentation guidance in the
        /// instructions.
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
        , [IsSystem]
        , [AdditionalSettingsJson]
        , [Guid]
    )
    VALUES (
        '{CodeComposerChatAgentName}'
        , 'A chat agent that builds custom UI in this instance from Rock''s own chat: a page, a Forge Content block, the Vue component it renders, and the Lava endpoints feeding it.'
        , '{CodeComposerChatAgentInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Chat}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , 1
        , '{{ ""{SeededInstructionsHashKey}"": ""{GetSha256Hex( CodeComposerChatAgentInstructions )}"" }}'
        , '{CodeComposerChatAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = '{CodeComposerChatAgentName}'
    WHERE [Guid] = '{CodeComposerChatAgentGuid}'
        AND [Name] IN ('Vibe Chat Agent', 'Code Composer Chat Agent')

    UPDATE [AIAgent]
    SET [IsSystem] = 1
    WHERE [Guid] = '{CodeComposerChatAgentGuid}'
        AND [IsSystem] = 0
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
        /// The enabled tools of the Forge Content Builder skill, shared by both agents.
        /// </summary>
        private static readonly string[] ForgeContentBuilderSkillEnabledTools = new[]
        {
            "8A51C3E9-674D-4B02-93F8-2E6B9D40A715", // Get Rock Version
            "D24F8B61-9C07-4E5A-B173-60A4F2C8E9D3", // Get Forge Content
            "3E97A0C5-48D2-4F16-85B9-C1D7E63A2F40"  // Add Or Update Forge Content
        };

        /// <summary>
        /// The enabled tools of the Lava Application Builder skill, shared by both agents.
        /// </summary>
        private static readonly string[] LavaApplicationBuilderSkillEnabledTools = new[]
        {
            "26C5F1A8-3D94-4E67-90B2-7A45D8E1C6F3", // Add Or Update Lava Application
            "B83A2D64-15F7-4C09-8E51-9C3B6F04D7A2", // Get Lava Application
            "248182EA-2B76-4127-A864-BE48F8E15585", // Resolve Audience
            "5F1E8C29-A47B-4D63-B905-E26A1D79F4C8", // Add Or Update Lava Endpoint
            "E64B9F07-2C58-41DA-A83F-05D9C7B24E61", // Get Lava Endpoint
            "49A7D3E1-8F60-4B25-96C4-B1E5A08D3F72", // Delete Lava Endpoint
            "C08E5A93-D1B6-4F74-82D0-46F3C9E17B58"  // Delete Lava Application
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
        /// Attaches the four skills to both agents with explicit enabled-tool
        /// lists. Attaching a skill alone does not expose its tools.
        /// </summary>
        private void AttachSkillsToAgents_Up()
        {
            AttachSkillToAgent( CodeComposerMcpAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( CodeComposerMcpAgentGuid, ForgeContentBuilderSkillGuid, ForgeContentBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerMcpAgentGuid, LavaApplicationBuilderSkillGuid, LavaApplicationBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerMcpAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools );

            AttachSkillToAgent( CodeComposerChatAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, ForgeContentBuilderSkillGuid, ForgeContentBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, LavaApplicationBuilderSkillGuid, LavaApplicationBuilderSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools );
        }

        /// <summary>
        /// Removes the two Code Composer agents and their security rules. Deleting
        /// an AIAgent cascades to its AIAgentSkill rows, so the skill attachments
        /// added by <see cref="AttachSkillsToAgents_Up"/> go with it.
        /// </summary>
        private void RemoveCodeComposerAgents_Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "7FB09F45-4FB1-45FE-A994-E130F6543078" );
            RockMigrationHelper.DeleteSecurityAuth( "4D692452-3031-4854-A6AF-61A900C3D8A2" );
            RockMigrationHelper.DeleteSecurityAuth( "555E5B64-1F3F-4117-B108-20ADB95F8A04" );
            RockMigrationHelper.DeleteSecurityAuth( "0CCA182B-7827-4FE7-BA69-B63C79BDE4D3" );

            Sql( $@"
DELETE FROM [AIAgent]
WHERE [Guid] IN (
    '{CodeComposerMcpAgentGuid}'
    , '{CodeComposerChatAgentGuid}'
)" );
        }

        #endregion Agent

        #region Helper Methods

        /// <summary>
        /// Computes the lowercase hexadecimal SHA-256 of a string's UTF-8 bytes.
        /// Used to fingerprint the seeded instruction text so a later migration
        /// can tell an untouched agent from a tuned one.
        /// </summary>
        /// <param name="value">The text to hash.</param>
        /// <returns>The 64-character hexadecimal digest.</returns>
        private static string GetSha256Hex( string value )
        {
            using ( var sha256 = System.Security.Cryptography.SHA256.Create() )
            {
                var hash = sha256.ComputeHash( System.Text.Encoding.UTF8.GetBytes( value ?? string.Empty ) );

                return string.Concat( hash.Select( b => b.ToString( "x2" ) ) );
            }
        }

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
