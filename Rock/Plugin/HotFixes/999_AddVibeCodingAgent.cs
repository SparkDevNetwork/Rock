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

using System;
using System.Linq;

using Rock.Security;

namespace Rock.Plugin.HotFixes
{
    /*
        8/12/2026 - CLAUDE

        Seeds the Vibe Agent: an MCP agent preconfigured with the three code-based
        skills that make up the vibe-coding flow (PageBuilder, LavaData and
        ObsidianVibeCoding), so an administrator can point an MCP client at Rock
        without assembling the agent by hand.

        Three things here are easy to get wrong and are deliberate:

        1. The skill EntityTypes are registered explicitly before the AISkill rows
           reference them. EntityType registration normally happens during
           application startup, which runs AFTER migrations, so on a fresh install
           the CodeEntityTypeId lookup would resolve to NULL and produce a skill
           that silently exposes no tools.

        2. Attaching a skill to an agent does NOT enable its tools. Each
           AIAgentSkill row carries an explicit EnabledTools allowlist in its
           AdditionalSettingsJson, so every tool guid has to be enumerated.

        3. The agent row is created only when absent, never updated. There is no
           IsSystem flag on AIAgent, so an administrator is free to retune the
           instructions or the enabled tools; re-running this migration must not
           stomp that. The skills and tools ARE upserted, because their names and
           descriptions are ours to correct.

        Security is administrator-only rather than the staff-wide default the Staff
        Agent uses. These tools create pages, write code that runs in visitors'
        browsers, and can execute privileged Lava.

        Reason: Ship the vibe-coding agent preconfigured instead of hand-assembled.
    */

    /// <summary>
    /// Adds the Vibe Agent MCP server and the three code-based skills it carries.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 999, "20.0" )]
    public class AddVibeCodingAgent : Migration
    {
        #region Constants

        /// <summary>
        /// The Guid of the Vibe Agent.
        /// </summary>
        private const string VibeAgentGuid = "DC44435A-8900-4AB4-9EB3-1756FCC1B355";

        /// <summary>
        /// The MCP slug the agent answers on, at <c>/api/v2/mcp/{slug}</c>.
        /// </summary>
        private const string VibeAgentSlug = "vibe-coding";

        /// <summary>
        /// The Guid of the Obsidian Vibe Coding AISkill record.
        /// </summary>
        private const string ObsidianVibeCodingSkillGuid = "647770A9-F3D7-4924-B046-5C9C43959ECB";

        /// <summary>
        /// The Guid of the Page Builder AISkill record.
        /// </summary>
        private const string PageBuilderSkillGuid = "EE27BE5A-1276-433F-A636-1BEF3550EC1E";

        /// <summary>
        /// The Guid of the Lava Data AISkill record.
        /// </summary>
        private const string LavaDataSkillGuid = "8660E7C0-1101-4058-BAF5-20B860600027";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.ObsidianVibeCodingSkill</c>.
        /// </summary>
        private const string ObsidianVibeCodingEntityTypeGuid = "4C833FA4-A7EF-4D49-9549-B24CBB629A73";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.PageBuilderSkill</c>.
        /// </summary>
        private const string PageBuilderEntityTypeGuid = "1D5FD674-F94D-4166-BC10-F2EA86412C4B";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.LavaDataSkill</c>.
        /// </summary>
        private const string LavaDataEntityTypeGuid = "CABB72CF-DD09-48CD-9BB9-4819488BC7CA";

        #endregion Constants

        #region Instructions

        /// <summary>
        /// The instructions sent to an MCP client when it connects. Deliberately
        /// terse: this text lands in the client's context alongside every tool
        /// description, so it carries only what the tool metadata cannot. Per-tool
        /// rules live in the skills' AgentUsage and AgentGuardrail attributes.
        /// </summary>
        private const string VibeAgentInstructions = @"# Persona

You build custom UI inside this Rock instance: a page, an Obsidian Content block on it, the Vue component that block renders, and the Lava endpoints feeding that component. Everything is stored in the database. There is no repository file and no build step.

# Guardrails

Ask before building: what it shows, who the audience is, which parent page it lives under, how the data is scoped, and roughly what it should look like. Ask in one message, not one at a time. If the user says to just build something, pick defaults, state them, and produce a first version they can react to.

Confirm the parent page, route, and block type before creating anything. Those change site structure.

A successful save means the source compiled. It does not mean the component works. Never report otherwise.

# Build Order

GetRockVersion, then FindPages and CreatePage (pass a kebab-case route), then AddBlock with the ""Obsidian Content Detail"" block type. Keep the block id it returns. Look up your controls in the knowledge base, create the Lava endpoints under one application slug, then SetContentSource.

# Authoring Contract

Plain JavaScript only. `lang=""ts""` is not supported and nothing strips types, so remove every annotation when adapting a repo `.obs` file.

Imports must be plain top-level `import X from ""path"";` statements. Side-effect and dynamic imports do not resolve.

Import from `@Obsidian/*` (Controls, Core, Directives, Enums, FieldTypes, Libs, PageState, SystemGuids, Templates, Utility, ValidationRules) plus `vue`, `axios`, `luxon`, `mitt`, `ant-design-vue`, `tslib`. `@Obsidian/ViewModels/*` is unavailable because repo blocks import those as types only.

# After Saving

Give the user the page URL and tell them to check it as a normal member, not as an administrator. Components run as whoever views the page, and a new Lava application has no security rules until someone adds them, so your data may be missing or over-shared for everyone else. If they report a problem, GetContentSource, fix it, and save again.";

        #endregion Instructions

        /// <inheritdoc/>
        public override void Up()
        {
            AddPageBuilderSkill();
            AddLavaDataSkill();
            AddObsidianVibeCodingSkill();
            AddVibeAgent();
            AttachSkillsToAgent();
        }

        /// <inheritdoc/>
        public override void Down()
        {
            // Intentionally empty. Removing the agent would discard any tuning an
            // administrator has done to its instructions or enabled tools, and the
            // skills may be attached to other agents by then.
        }

        #region Skills

        /// <summary>
        /// Registers the Page Builder skill and its tools.
        /// </summary>
        private void AddPageBuilderSkill()
        {
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.PageBuilderSkill",
                PageBuilderEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Page Builder",
                "Creates CMS pages and adds blocks to them, so authored content has a place to live.",
                PageBuilderEntityTypeGuid,
                PageBuilderSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                PageBuilderSkillGuid,
                "4A95B31D-A629-4F76-B68A-6D74B7C578EE",
                "C2D66CDA-336D-4D1E-BF29-B2960A25AAB6" );

            AddOrUpdateCodeAISkillTool(
                PageBuilderSkillGuid,
                "Find Pages",
                "Finds pages by a partial name match so a parent page can be confirmed before creating anything.",
                "C668CAE0-CFA7-4AFF-87FF-5025860170BA" );

            AddOrUpdateCodeAISkillTool(
                PageBuilderSkillGuid,
                "Create Page",
                "Creates a child page under a parent, inheriting the parent's layout and authorization.",
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633" );

            AddOrUpdateCodeAISkillTool(
                PageBuilderSkillGuid,
                "Add Block",
                "Places a block in a zone on a page and returns the new block's id.",
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF" );
        }

        /// <summary>
        /// Registers the Lava Data skill and its tools.
        /// </summary>
        private void AddLavaDataSkill()
        {
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.LavaDataSkill",
                LavaDataEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Lava Data",
                "Creates the JSON data endpoints an authored component calls, by writing Lava rather than searching for an existing REST endpoint.",
                LavaDataEntityTypeGuid,
                LavaDataSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                LavaDataSkillGuid,
                "36E7ED26-E777-4E22-A133-A3148C85A9B8",
                "53F703F0-E8CA-42A3-9516-8E9935760C07" );

            AddOrUpdateCodeAISkillTool(
                LavaDataSkillGuid,
                "Create Lava Endpoint",
                "Creates a Lava endpoint that returns JSON, and test-executes the template so a broken one is caught immediately.",
                "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C" );

            AddOrUpdateCodeAISkillTool(
                LavaDataSkillGuid,
                "Get Lava Endpoint",
                "Reads an existing Lava endpoint's template and settings.",
                "11AE1557-1EF3-4E03-9E8E-FCF99F72FCD9" );

            AddOrUpdateCodeAISkillTool(
                LavaDataSkillGuid,
                "Update Lava Endpoint",
                "Replaces a Lava endpoint's template and optionally its security mode and enabled Lava commands.",
                "2F92D13B-A2A2-455C-8324-57A181D505C2" );

            AddOrUpdateCodeAISkillTool(
                LavaDataSkillGuid,
                "Delete Lava Endpoint",
                "Deletes a Lava endpoint this skill created, so scratch and diagnostic endpoints can be cleaned up.",
                "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9" );

            AddOrUpdateCodeAISkillTool(
                LavaDataSkillGuid,
                "Delete Lava Application",
                "Deletes a Lava application this skill created, once it has no endpoints left.",
                "9A47C2D1-83B5-4E60-A7F3-1B58C90D24E6" );
        }

        /// <summary>
        /// Registers the Obsidian Vibe Coding skill and its tools.
        /// </summary>
        private void AddObsidianVibeCodingSkill()
        {
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.ObsidianVibeCodingSkill",
                ObsidianVibeCodingEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Obsidian Vibe Coding",
                "Authors and edits the Vue source rendered by an Obsidian Content block placement.",
                ObsidianVibeCodingEntityTypeGuid,
                ObsidianVibeCodingSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                ObsidianVibeCodingSkillGuid,
                "4E646B3E-E483-48C0-9B5B-5D01FEFC2406",
                "E0BE5EA7-452F-4FF8-9552-545F3EDE58FC" );

            AddOrUpdateCodeAISkillTool(
                ObsidianVibeCodingSkillGuid,
                "Get Rock Version",
                "Reports the Rock version of this instance, so control and API lookups are scoped to the release actually deployed.",
                "3E7A1C42-8B95-4D06-A1F3-2C64D9B7E508" );

            AddOrUpdateCodeAISkillTool(
                ObsidianVibeCodingSkillGuid,
                "Get Content Source",
                "Reads the authored Vue source currently stored for a block placement.",
                "7D3A8200-3A90-44CC-9E30-B600383E835F" );

            AddOrUpdateCodeAISkillTool(
                ObsidianVibeCodingSkillGuid,
                "Set Content Source",
                "Compiles and stores the authored Vue source for a block placement, returning compile errors instead of saving when it fails.",
                "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8" );
        }

        #endregion Skills

        #region Agent

        /// <summary>
        /// Creates the Vibe Agent as an MCP server, if it does not already exist.
        /// </summary>
        private void AddVibeAgent()
        {
            // Create-only, never update. An administrator may retune the
            // instructions, and re-running this migration must not discard that.
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{VibeAgentGuid}')
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
        'Vibe Agent'
        , 'An MCP server that lets an AI client build custom Obsidian UI in this instance: a page, a block, the Vue component it renders, and the Lava endpoints feeding it.'
        , '{VibeAgentInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Mcp}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , '{{ ""McpAgentSettings"": {{ ""Slug"": ""{VibeAgentSlug}"", ""IsExcludingSystemSkills"": false }} }}'
        , '{VibeAgentGuid}'
    )
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                VibeAgentGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "7FB09F45-4FB1-45FE-A994-E130F6543078" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                VibeAgentGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "4D692452-3031-4854-A6AF-61A900C3D8A2" );
        }

        /// <summary>
        /// Attaches the three skills to the agent, enabling every tool on each.
        /// </summary>
        private void AttachSkillsToAgent()
        {
            AttachSkillToAgent( PageBuilderSkillGuid, new[]
            {
                "C668CAE0-CFA7-4AFF-87FF-5025860170BA", // Find Pages
                "4A64B0B9-0DF9-42CF-BF5C-8FE24EFA4633", // Create Page
                "05C9C108-4516-46B7-85FB-5C8FE6212CCF"  // Add Block
            } );

            AttachSkillToAgent( LavaDataSkillGuid, new[]
            {
                "9066DD4A-2158-4B1C-87E3-4058CBEE1E5C", // Create Lava Endpoint
                "11AE1557-1EF3-4E03-9E8E-FCF99F72FCD9", // Get Lava Endpoint
                "2F92D13B-A2A2-455C-8324-57A181D505C2", // Update Lava Endpoint
                "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9", // Delete Lava Endpoint
                "9A47C2D1-83B5-4E60-A7F3-1B58C90D24E6"  // Delete Lava Application
            } );

            AttachSkillToAgent( ObsidianVibeCodingSkillGuid, new[]
            {
                "3E7A1C42-8B95-4D06-A1F3-2C64D9B7E508", // Get Rock Version
                "7D3A8200-3A90-44CC-9E30-B600383E835F", // Get Content Source
                "26FFEE94-4868-4DEC-BE40-68FBE30DAEB8"  // Set Content Source
            } );
        }

        #endregion Agent

        #region Helper Methods

        /// <summary>
        /// Links one skill to the Vibe Agent with an explicit enabled-tool list.
        /// Attaching a skill alone does not expose its tools.
        /// </summary>
        /// <param name="skillGuid">The Guid of the AISkill to attach.</param>
        /// <param name="enabledToolGuids">The Guids of the tools to enable.</param>
        private void AttachSkillToAgent( string skillGuid, string[] enabledToolGuids )
        {
            // Produces "guid", "guid". The value is interpolated into the verbatim
            // SQL below as-is, so it must already contain single double-quotes.
            var enabledTools = string.Join( ", ", enabledToolGuids.Select( g => $"\"{g}\"" ) );

            Sql( $@"
DECLARE @AgentId INT = (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{VibeAgentGuid}')
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
