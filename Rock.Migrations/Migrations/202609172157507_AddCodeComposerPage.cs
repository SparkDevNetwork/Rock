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

    /// <summary>
    ///
    /// </summary>
    public partial class AddCodeComposerPage : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The Guid of the Code Composer Chat Agent seeded by the AddCodeComposer
        /// migration.
        /// </summary>
        private const string CodeComposerChatAgentGuid = "5A2BC280-C12E-4C13-AA1F-D169DB27D3FE";

        /// <summary>
        /// The Guid of the Code Composer MCP Agent seeded by the AddCodeComposer
        /// migration.
        /// </summary>
        private const string CodeComposerMcpAgentGuid = "DC44435A-8900-4AB4-9EB3-1756FCC1B355";

        /// <summary>
        /// The Guid of the Core Administration skill, matching its AgentSkillGuid attribute.
        /// </summary>
        private const string CoreAdministrationSkillGuid = "6DBD6867-2E0B-4D2E-9BF9-B34B77E4E94B";

        /// <summary>
        /// The Guid of the Core Administration skill's code entity type, matching
        /// its EntityTypeGuid attribute.
        /// </summary>
        private const string CoreAdministrationSkillEntityTypeGuid = "55EB1E6F-EFBF-4E9C-BA11-DBC7147DA342";

        /// <summary>
        /// The Guid of the AI Agents page that the Code Composer page is added under.
        /// </summary>
        private const string AIAgentsPageGuid = "9F7B9158-3A73-429A-A817-5909D2AED13C";

        /// <summary>
        /// The Guid of the Full Width layout on the Rock internal site.
        /// </summary>
        private const string FullWidthLayoutGuid = "D65F783D-87A9-4CC9-8110-E83466A0EADB";

        /// <summary>
        /// The Guid of the Chat Bot block type.
        /// </summary>
        private const string ChatBotBlockTypeGuid = "91A66C59-830E-49B5-A196-DCF93D0DDE92";

        /// <summary>
        /// The Guid of the Chat Bot block type's Default Agent attribute.
        /// </summary>
        private const string ChatBotDefaultAgentAttributeGuid = "44C0E822-4651-4E52-B3AD-61896D98336C";

        /// <summary>
        /// The Guid of the new Code Composer page.
        /// </summary>
        private const string CodeComposerPageGuid = "98F5C951-8AE9-4160-92B5-53FAB06C7CD5";

        /// <summary>
        /// The Guid of the Chat Bot block placed on the Code Composer page.
        /// </summary>
        private const string CodeComposerChatBotBlockGuid = "7A280908-C75F-4B6C-8B93-3B616129E529";

        #endregion Constants

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateCodeComposerChatAgentModelSettingsUp();
            AttachCoreAdministrationSkillUp();
            AddCodeComposerPageUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddCodeComposerPageDown();
            AttachCoreAdministrationSkillDown();
            UpdateCodeComposerChatAgentModelSettingsDown();
        }

        #region Core Administration Skill

        /*
            9/17/2026 - CLAUDE

            The Lava Application Builder skill no longer writes authorization
            rules itself. Who may execute an application's endpoints is authored
            with the Core Administration skill's authorization tools, so both Code
            Composer agents need that skill with those four tools enabled or a new
            Lava application stays locked to administrators.

            The three exception tools are enabled as well so the agents can read
            the server-side error behind a failing endpoint or block instead of
            guessing from the browser's response. The rest of the skill (defined
            types, categories, global attributes, system communications) is
            outside what a page builder should touch.

            The skill and tool rows are upserted first, as the AddCodeComposer
            migration did for its own skills, because on a fresh install startup
            registration has not yet run when migrations execute and the
            attachment insert requires the rows to exist. Attachments are
            create-only so an administrator's later retuning of the enabled tools
            survives a re-run.

            Reason: Let the Code Composer secure the Lava applications it creates.
        */

        /// <summary>
        /// The Core Administration tools the Code Composer agents may call.
        /// </summary>
        private static readonly string[] CoreAdministrationSkillEnabledTools = new[]
        {
            "7E4933CE-3E2E-4755-B3E8-7424F0642A5A", // List Authorization Actions For Entity
            "25CA6D47-0883-40C4-B222-BC0C64693C11", // List Authorization For Entity
            "FF3C1804-8980-4043-A35C-45830EB3336F", // Add Or Update Authorization For Entity
            "AE38A4D5-2F9D-4865-A5EC-AA7719311D50", // Delete Authorization For Entity
            "B7783D9B-E062-46A9-A47F-14D741F7B868", // List Exceptions
            "7B28303B-1081-4F93-9D5A-56F5CB98CDC7", // List Exception Instances
            "257A4CE8-3ED9-42F5-BFA1-AB11805261A6"  // Get Exception
        };

        /// <summary>
        /// Registers the Core Administration skill and its authorization tools, then
        /// attaches the skill to both Code Composer agents.
        /// </summary>
        private void AttachCoreAdministrationSkillUp()
        {
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CoreAdministrationSkill",
                CoreAdministrationSkillEntityTypeGuid,
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Core Administration Skill",
                "Provides access to Rock's core configuration metadata: defined types and values, entity types, categories, field types, attributes, and system communications.",
                CoreAdministrationSkillEntityTypeGuid,
                CoreAdministrationSkillGuid );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "List Authorization Actions For Entity",
                "Lists the actions an entity supports securing, such as View, Edit, and Administrate. Use the returned action names with the other authorization tools.",
                "7E4933CE-3E2E-4755-B3E8-7424F0642A5A" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "List Authorization For Entity",
                "Lists the authorization (security) rules that apply to an entity, including rules inherited from parent authorities.",
                "25CA6D47-0883-40C4-B222-BC0C64693C11" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "Add Or Update Authorization For Entity",
                "Adds an authorization (security) rule to an entity or updates an existing one, granting or denying one action to a person, security role, or special role.",
                "FF3C1804-8980-4043-A35C-45830EB3336F" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "Delete Authorization For Entity",
                "Deletes a single authorization (security) rule from an entity.",
                "AE38A4D5-2F9D-4865-A5EC-AA7719311D50" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "List Exceptions",
                "Lists logged exceptions grouped by the error they represent, with a count of how many times each occurred, over a date range.",
                "B7783D9B-E062-46A9-A47F-14D741F7B868" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "List Exception Instances",
                "Lists the individual occurrences of an exception, filtered by type and description, over a date range.",
                "7B28303B-1081-4F93-9D5A-56F5CB98CDC7" );

            AddOrUpdateCodeAISkillTool(
                CoreAdministrationSkillGuid,
                "Get Exception",
                "Gets a single logged exception in full detail, including its stack trace and any inner exceptions.",
                "257A4CE8-3ED9-42F5-BFA1-AB11805261A6" );

            AttachSkillToAgent( CodeComposerMcpAgentGuid, CoreAdministrationSkillGuid, CoreAdministrationSkillEnabledTools );
            AttachSkillToAgent( CodeComposerChatAgentGuid, CoreAdministrationSkillGuid, CoreAdministrationSkillEnabledTools );
        }

        /// <summary>
        /// Detaches the Core Administration skill from both Code Composer agents.
        /// The skill and tool rows are left in place because the skill is shared
        /// and startup registration maintains them.
        /// </summary>
        private void AttachCoreAdministrationSkillDown()
        {
            Sql( $@"
DELETE [aas]
FROM [AIAgentSkill] AS [aas]
INNER JOIN [AIAgent] AS [a] ON [a].[Id] = [aas].[AIAgentId]
INNER JOIN [AISkill] AS [s] ON [s].[Id] = [aas].[AISkillId]
WHERE [s].[Guid] = '{CoreAdministrationSkillGuid}'
    AND [a].[Guid] IN ( '{CodeComposerMcpAgentGuid}', '{CodeComposerChatAgentGuid}' )" );
        }

        /// <summary>
        /// Links one skill to one agent with an explicit enabled-tool list.
        /// Attaching a skill alone does not expose its tools. Copied from the
        /// AddCodeComposer migration.
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
        /// Adds or updates a code-based AI skill. Copied from the AddCodeComposer
        /// migration, which follows hotfix 309.
        /// </summary>
        /// <param name="name">The name of the AI skill.</param>
        /// <param name="description">The user-friendly description of the AI skill.</param>
        /// <param name="codeEntityTypeGuid">The GUID of the code entity type.</param>
        /// <param name="skillGuid">The GUID of the AI skill.</param>
        private void AddOrUpdateCodeAISkill( string name, string description, string codeEntityTypeGuid, string skillGuid )
        {
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
        '{name.Replace( "'", "''" )}'
        , '{description.Replace( "'", "''" )}'
        , @CodeEntityTypeId
        , '{skillGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkill]
    SET [Name] = '{name.Replace( "'", "''" )}'
        , [Description] = '{description.Replace( "'", "''" )}'
        , [CodeEntityTypeId] = @CodeEntityTypeId
    WHERE [Guid] = '{skillGuid}'
END" );
        }

        /// <summary>
        /// Adds or updates a code-based AI skill tool. Copied from the
        /// AddCodeComposer migration, which follows hotfix 309.
        /// </summary>
        /// <param name="skillGuid">The GUID of the AI skill this tool belongs to.</param>
        /// <param name="name">The name of the AI skill tool.</param>
        /// <param name="description">The user-friendly description of the AI skill tool.</param>
        /// <param name="toolGuid">The GUID of the AI skill tool.</param>
        private void AddOrUpdateCodeAISkillTool( string skillGuid, string name, string description, string toolGuid )
        {
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
        '{name.Replace( "'", "''" )}'
        , '{description.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , @SkillId
        , '{toolGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkillTool]
    SET [Name] = '{name.Replace( "'", "''" )}'
        , [Description] = '{description.Replace( "'", "''" )}'
        , [ToolType] = {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , [AISkillId] = @SkillId
    WHERE [Guid] = '{toolGuid}'
END" );
        }

        #endregion Core Administration Skill

        #region Code Composer Page

        /// <summary>
        /// Adds the Code Composer page under AI Agents with a Chat Bot block bound
        /// to the Code Composer Chat Agent. The page inherits the administrator-only
        /// security of its parent, which matches the agent's own security.
        /// </summary>
        private void AddCodeComposerPageUp()
        {
            RockMigrationHelper.AddPage( true, AIAgentsPageGuid, FullWidthLayoutGuid, "Code Composer", "", CodeComposerPageGuid, "ti ti-code" );

            RockMigrationHelper.AddBlock( true, CodeComposerPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), ChatBotBlockTypeGuid.AsGuid(), "Chat Bot", "Main", @"", @"", 0, CodeComposerChatBotBlockGuid );

            RockMigrationHelper.AddBlockAttributeValue( true, CodeComposerChatBotBlockGuid, ChatBotDefaultAgentAttributeGuid, CodeComposerChatAgentGuid );
        }

        /// <summary>
        /// Removes the Code Composer page and its Chat Bot block.
        /// </summary>
        private void AddCodeComposerPageDown()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( CodeComposerChatBotBlockGuid, ChatBotDefaultAgentAttributeGuid );
            RockMigrationHelper.DeleteBlock( CodeComposerChatBotBlockGuid );
            RockMigrationHelper.DeletePage( CodeComposerPageGuid );
        }

        #endregion Code Composer Page

        #region Code Composer Chat Agent Model Settings

        /*
            9/17/2026 - CLAUDE

            The Code Composer Chat Agent was seeded without a ChatAgentSettings
            section, so it ran on the defaults: the Medium model role and Low
            reasoning effort. Composing pages, Vue components and Lava endpoints
            is multi-step work where a weaker model and shallow reasoning produce
            broken integrations, so the agent is moved to the High role and High
            reasoning effort.

            AdditionalSettingsJson is edited in place with JSON_MODIFY rather than
            replaced so the SeededInstructionsHash and any other settings an
            administrator has saved survive. The ChatAgentSettings object is
            created first when absent because JSON_MODIFY does not create
            intermediate objects for a nested path.

            Reason: Run the Code Composer on the high model with high reasoning.
        */

        /// <summary>
        /// Sets the Code Composer Chat Agent's model role and reasoning effort to High.
        /// </summary>
        private void UpdateCodeComposerChatAgentModelSettingsUp()
        {
            Sql( $@"
UPDATE [a]
SET [a].[AdditionalSettingsJson] = JSON_MODIFY(
        JSON_MODIFY(
            CASE
                WHEN JSON_QUERY( ISNULL( [a].[AdditionalSettingsJson], '{{}}' ), '$.ChatAgentSettings' ) IS NULL
                    THEN JSON_MODIFY( ISNULL( [a].[AdditionalSettingsJson], '{{}}' ), '$.ChatAgentSettings', JSON_QUERY( '{{}}' ) )
                ELSE [a].[AdditionalSettingsJson]
            END,
            '$.ChatAgentSettings.Role',
            {( int ) Enums.AI.Agent.ModelServiceRole.High}
        ),
        '$.ChatAgentSettings.ReasoningEffort',
        {( int ) Enums.AI.Agent.ReasoningEffort.High}
    )
FROM [AIAgent] AS [a]
WHERE [a].[Guid] = '{CodeComposerChatAgentGuid}'" );
        }

        /// <summary>
        /// Removes the model role and reasoning effort from the Code Composer Chat
        /// Agent so it returns to the runtime defaults.
        /// </summary>
        private void UpdateCodeComposerChatAgentModelSettingsDown()
        {
            Sql( $@"
UPDATE [a]
SET [a].[AdditionalSettingsJson] = JSON_MODIFY(
        JSON_MODIFY( [a].[AdditionalSettingsJson], '$.ChatAgentSettings.Role', NULL ),
        '$.ChatAgentSettings.ReasoningEffort',
        NULL
    )
FROM [AIAgent] AS [a]
WHERE [a].[Guid] = '{CodeComposerChatAgentGuid}'
    AND JSON_QUERY( [a].[AdditionalSettingsJson], '$.ChatAgentSettings' ) IS NOT NULL" );
        }

        #endregion Code Composer Chat Agent Model Settings
    }
}
