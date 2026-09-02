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
    using System.Linq;

    using Rock.Model;
    using Rock.Plugin.HotFixes;
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20260819 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20/develop):
            // Note: These are ordered by the Rock version they are expected to
            //       run in.
            // ----------------------------------------------------------------
            // v19.4; 308_AddConnectedServices.cs
            DH_AddConnectedServices_Up();

            // v19.4; 309_EnableRockIntelligence.cs
            DH_EnableRockIntelligence_Up();

            // v19.4; 314_FixEventItemAttributeCorruption6962.cs
            NA_CleanupEventItemAttributeCorruption_6962_Up();

            // v19.5; 310_FixPaymentEntryInstructionsDefault6951.cs
            NA_FixPaymentEntryInstructionsDefault6951_Up();

            // v19.5; 311_FixEraFamilyAnalyticsWeekBoundariesFollowup6902.cs
            NA_FixEraFamilyAnalyticsWeekBoundariesFollowup6902_Up();

            // v19.5; 312_FixSignatureDocumentBinaryFileParentEntity6928.cs
            NA_FixSignatureDocumentBinaryFileParentEntity6928_Up();

            // v19.5; 313_FixPersonDuplicateFinderNullSelfJoinPerformance6959.cs
            NA_FixPersonDuplicateFinderNullSelfJoinPerformance6959_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Adds the Spark Connected Services page and block.
        /// </summary>
        private void DH_AddConnectedServices_Up()
        {
            // Add the /link-organization route to the existing Link
            // Organization page.
            RockMigrationHelper.AddOrUpdatePageRoute(
                SystemGuid.Page.LINK_ORGANIZATION,
                "link-organization",
                "42c6994a-76e0-48e1-87a3-a011e238b906" );

            // Add the new Spark Connect Services page.
            RockMigrationHelper.AddPage(
                skipIfAlreadyExists: true,
                SystemGuid.Page.SYSTEM_SETTINGS,
                SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE,
                "Spark Connect Services",
                string.Empty,
                SystemGuid.Page.SPARK_CONNECTED_SERVICES );

            RockMigrationHelper.AddOrUpdatePageRoute(
                SystemGuid.Page.SPARK_CONNECTED_SERVICES,
                "admin/settings/spark-connected-services",
                "49213723-3711-4ecd-866a-f15a85e2cbaf" );

            RockMigrationHelper.UpdatePageIcon(
                SystemGuid.Page.SPARK_CONNECTED_SERVICES,
                "ti ti-affiliate" );

            // Add the new Spark Connected Services block type.
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.Blocks.Administration.SparkConnectedServices",
                "af86a425-26ab-4254-b525-46d007d4b97e",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Spark Connected Services",
                "Configures the connected services provided by Spark for use in Rock.",
                "Rock.Blocks.Administration.SparkConnectedServices",
                "Administration",
                "4dfa65ad-ba8e-4634-baa2-6cf300987822" );

            // Add the Spark Connected Services block to the Spark Connected
            // Services page.
            RockMigrationHelper.AddBlock(
                skipIfAlreadyExists: true,
                SystemGuid.Page.SPARK_CONNECTED_SERVICES,
                null,
                "4dfa65ad-ba8e-4634-baa2-6cf300987822",
                "Spark Connected Services",
                "Main",
                string.Empty,
                string.Empty,
                0,
                "1a8035c5-7e04-407f-a486-ae193bbd13f7" );
        }

        /// <summary>
        /// Enables the Rock Intelligence AI provider and makes final CMS changes.
        /// </summary>
        private void DH_EnableRockIntelligence_Up()
        {
            // ------------------------------------------------------
            // Register the Rock Intelligence provider and make sure it is active.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Providers.RockIntelligenceProvider",
                "485db97f-37d1-480b-b536-f0e609f599be",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityAttribute(
                "Rock.AI.Agent.Providers.RockIntelligenceProvider",
                SystemGuid.FieldType.BOOLEAN,
                string.Empty,
                string.Empty,
                "Active",
                "Active",
                "Should Service be used?",
                0,
                "False",
                "59cc2a69-bef5-477a-b8dc-f1e85e800964",
                "Active" );

            // RockMigrationHelper does not have a generic "add or update
            // attribute value" method. Use custom SQL to make sure the
            // provider is active.
            Sql( @"
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '59cc2a69-bef5-477a-b8dc-f1e85e800964')

-- Now check if the attribute/entity pair already has a row and insert it if not
IF NOT EXISTS (SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = 0)
BEGIN
    INSERT INTO [AttributeValue] (
            [IsSystem]
        , [AttributeId]
        , [EntityId]
        , [Value]
        , [Guid])
    VALUES(
            1
        , @AttributeId
        , 0
        , 'True'
        , '4cc84b1a-6de7-46d8-b531-57759e2f78ee')
END
ELSE
BEGIN
    UPDATE [AttributeValue]
    SET [Value] = 'True'
    WHERE [AttributeId] = @AttributeId AND [EntityId] = 0
END" );

            // ------------------------------------------------------
            // Turn off the UserSelectable flag on the Quick Note note type.

            Sql( @"
UPDATE [NoteType]
SET [UserSelectable] = 0
WHERE [Guid] = 'A3F5982F-C4D0-4345-8021-EB38C4C9AA18'
" );

            // ------------------------------------------------------
            // Register the Person skill so it can be used by the default
            // Chat Agent.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.PersonSkill",
                "12e7bdea-b67a-48d7-8d1e-245bf8e9b555",
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Person Skill",
                "This skill provides a holistic view of a person’s profile, connections, and overall engagement.",
                "12e7bdea-b67a-48d7-8d1e-245bf8e9b555",
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "2defb3ed-51f1-4473-9524-25e9fd7e9fbc" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                101,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) SpecialRole.None,
                "6a3eb7ea-4831-47c8-a917-677cd1e5929a" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                102,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) SpecialRole.None,
                "50e721a2-32e6-474b-a6b9-de5e31bd0c8c" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                103,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "f9310ed2-cc9c-49fa-8c02-0eaab426f9b0" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Add Or Update Address",
                "Updates a person's address.",
                "d34e7821-36e0-f2bc-4496-7a82e1ce4475" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Get Family Service Attendance Summary",
                "Provide attendance information for members of the selected person.",
                "544f23d7-6d28-41ea-bd43-249c976beba0" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Get Page Visit Summary For Person",
                "Lists page visits for a specific person.",
                "efdbc338-cc1c-46d2-a7f6-7ae5081147ae" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Get Person Profile",
                "Returns a comprehensive profile for a single person, including contact details, demographics, household, and key insights.",
                "2142a382-6ab2-0995-4480-69b641ae2cdc" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "List Communication History For Person",
                "Lists communication records for a specific person.",
                "dd7510bb-9176-4463-9b23-665000992a62" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "List Group Memberships For Person",
                "Lists group membership records for a specific person.",
                "a02698ca-3c3a-48ed-adba-36f6f9b29cae" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "List Media Views For Person",
                "Lists media element views for a specific person.",
                "ab6cb80c-352a-f895-4233-09ba9da69ccc" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "List Peer Network For Person",
                "Lists people in the provided person's peer network.",
                "39244a1e-57bf-476b-af88-65ebc205f25d" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "List Personal Devices For Person",
                "Lists personal devices for the provided person.",
                "29b7a989-59c4-4956-9c45-1d1297d3e673" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Search Person",
                "Does a full name sounds like search for the person.",
                "03093b11-a02d-f794-4a5e-9aea2c6ef63e" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Search Person Partial",
                "Does a name search based on a partial search (e.g. 't dec').",
                "873afc46-1872-999f-4e6c-94409654f6bc" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Update Person",
                "Updates properties on the person.",
                "a1198a34-fcf2-4f58-83fa-7d02dd69830e" );

            AddOrUpdateCodeAISkillTool(
                "dd5fa7dd-3277-4c31-848d-285cd67ac7ca",
                "Update Phone Number",
                "Updates a person's phone number.",
                "89a9f9c5-87f2-9197-46da-5c96d0bda628" );

            // ------------------------------------------------------
            // Create the default Chat Agent.

            Sql( @"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '4cc84b1a-6de7-46d8-b531-57759e2f78ee')
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
        'Staff Agent'
        , 'This agent provides a starting point to begin building a chat agent suited for use by your staff.'
        , 'You are configured as a sample agent which has not been fully configured yet. If you can''t do something, answer that the agent may need to be configured.

When displaying data make a pleasant UX using markdown. Below are some guidelines:
* Structure responses like a web UI: Start with a # Header summarizing the query, followed by ## Sections with tables or lists for content.
* Display friendly intro above the information you are displaying.
* Use emoji when it makes sense to add color and visual hierarchy.
* Use <hr> as separators between content sections.
* Bold information that would make a good title.
* Show only the information that you believe the user needs to see. 
* When searching for a person, only show their name, age, age classification, email and spouse if one exists. 
* When showing information about a person consider the context and display information that might be helpful. For example if the person is a child you might show their age.
* Don''t display missing information unless it''s relevant to what the user needs.
* Add a link to a person''s profile if it''s likely that the user would want to get to the person''s profile. Links should go next to the name. Format this as <a href=""url"" target=""_blank"">Profile</a>
* When there are 4 or more options consider showing the results in a table.'
        , 0
        , 0
        , '4cc84b1a-6de7-46d8-b531-57759e2f78ee'
    )
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                "4cc84b1a-6de7-46d8-b531-57759e2f78ee",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "4e43b632-06a6-4318-825a-38acb39bfa18" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                "4cc84b1a-6de7-46d8-b531-57759e2f78ee",
                1,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) SpecialRole.None,
                "28e32a29-8a77-4c52-814e-4a324d88630e" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                "4cc84b1a-6de7-46d8-b531-57759e2f78ee",
                2,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) SpecialRole.None,
                "21833356-9936-47f6-8268-df58630441b2" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                "4cc84b1a-6de7-46d8-b531-57759e2f78ee",
                3,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "2dc863f8-bccc-4643-a436-51b6bcdc8ab2" );

            var enabledToolGuids = new string[]
            {
                "d34e7821-36e0-f2bc-4496-7a82e1ce4475", // Add Or Update Address
                "544f23d7-6d28-41ea-bd43-249c976beba0", // Get Family Service Attendance Summary
                "efdbc338-cc1c-46d2-a7f6-7ae5081147ae", // Get Page Visit Summary For Person
                "2142a382-6ab2-0995-4480-69b641ae2cdc", // Get Person Profile
                "dd7510bb-9176-4463-9b23-665000992a62", // List Communication History For Person
                "a02698ca-3c3a-48ed-adba-36f6f9b29cae", // List Group Memberships For Person
                "ab6cb80c-352a-f895-4233-09ba9da69ccc", // List Media Views For Person
                "39244a1e-57bf-476b-af88-65ebc205f25d", // List Peer Network For Person
                "29b7a989-59c4-4956-9c45-1d1297d3e673", // List Personal Devices For Person
                "03093b11-a02d-f794-4a5e-9aea2c6ef63e", // Search Person
                "873afc46-1872-999f-4e6c-94409654f6bc", // Search Person Partial
                "a1198a34-fcf2-4f58-83fa-7d02dd69830e", // Update Person
                "89a9f9c5-87f2-9197-46da-5c96d0bda628"  // Update Phone Number
            };
            var enabledTools = string.Join( ", ", enabledToolGuids.Select( g => $"\"{g}\"" ) );

            Sql( $@"
DECLARE @AgentId INT = (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '4cc84b1a-6de7-46d8-b531-57759e2f78ee')
DECLARE @SkillId INT = (SELECT [Id] FROM [AISkill] WHERE [Guid] = 'dd5fa7dd-3277-4c31-848d-285cd67ac7ca')

IF NOT EXISTS (SELECT [Id] FROM [AIAgentSkill] WHERE [AIAgentId] = @AgentId AND [AISkillId] = @SkillId)
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
END
" );

            // ------------------------------------------------------
            // Register the Finance skill and set default security.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.FinanceSkill",
                "92c9469f-c158-4476-8854-ef4805ea0970",
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Finance Skill",
                "This skill provides access to financial data.",
                "92c9469f-c158-4476-8854-ef4805ea0970",
                "4fc57368-8362-49f0-a1a2-ebc9efdd947c" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "4fc57368-8362-49f0-a1a2-ebc9efdd947c",
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "ac9ed7f2-86ad-45fb-b13f-34e1488e2268" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "4fc57368-8362-49f0-a1a2-ebc9efdd947c",
                101,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "13e56551-f61a-4ef9-9430-df57c7319fd8" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "4fc57368-8362-49f0-a1a2-ebc9efdd947c",
                102,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                ( int ) SpecialRole.None,
                "1d53e2a2-55cb-42c1-8c21-4cd2ec1870a5" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "4fc57368-8362-49f0-a1a2-ebc9efdd947c",
                103,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "8429707b-b1f6-43c0-b1f0-e5c459bee7cb" );

            // ------------------------------------------------------
            // Register the Benevolence skill and set default security.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.BenevolenceSkill",
                "43f23f97-2360-4089-ad6e-c1ddcdf4665b",
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Benevolence Skill",
                "This skill provides access to benevolence requests.",
                "43f23f97-2360-4089-ad6e-c1ddcdf4665b",
                "d7340fae-917c-4a96-8958-99ec8361328a" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "d7340fae-917c-4a96-8958-99ec8361328a",
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "1517552b-36ee-4b4d-9762-9b20e5813385" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "d7340fae-917c-4a96-8958-99ec8361328a",
                101,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_BENEVOLENCE,
                ( int ) SpecialRole.None,
                "0ddf5ce8-4a0d-45a5-a2c9-ba74de7b3338" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "d7340fae-917c-4a96-8958-99ec8361328a",
                102,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "7f2ad0ac-4b35-4947-8c8e-e76a36c15c26" );

            // ------------------------------------------------------
            // Register the Prayer skill and set default security.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.PrayerSkill",
                "6033d65e-c782-45ba-9a74-23f9b9353a27",
                false,
                false );

            AddOrUpdateCodeAISkill(
                "Prayer Skill",
                "This skill provides functionality to manage prayer.",
                "6033d65e-c782-45ba-9a74-23f9b9353a27",
                "0ef2bbfd-52d9-441b-9be5-f4c5d2b42ed0" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "0ef2bbfd-52d9-441b-9be5-f4c5d2b42ed0",
                100,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "0ea20856-e4e9-4c99-bbc7-165dd72fa074" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "0ef2bbfd-52d9-441b-9be5-f4c5d2b42ed0",
                101,
                Authorization.VIEW,
                true,
                "59da53fc-ae18-4052-bc90-ce75b3979943", // Prayer Administration
                ( int ) SpecialRole.None,
                "30d49805-3ced-4d9a-9dc2-6af68ee8e17f" );

            RockMigrationHelper.AddSecurityAuthForAISkill(
                "0ef2bbfd-52d9-441b-9be5-f4c5d2b42ed0",
                102,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "0f4a333e-68e0-4311-bbb9-9f35584152cb" );

            // ------------------------------------------------------
            // Show the AI Agents page.

            Sql( @"
UPDATE [Page]
SET [DisplayInNavWhen] = 0 -- Always
WHERE [Guid] = '9f7b9158-3a73-429a-a817-5909d2aed13c'" );

            // ------------------------------------------------------
            // Hide the AI Agent Providers page.

            Sql( @"
UPDATE [Page]
SET [DisplayInNavWhen] = 2 -- Never
WHERE [Guid] = '74b916c7-256a-42e1-8e4e-951450d23152'" );

            // ------------------------------------------------------
            // Add page routes for existing AI Agent pages.

            RockMigrationHelper.AddOrUpdatePageRoute(
                "9f7b9158-3a73-429a-a817-5909d2aed13c", // Page: AI Agents
                "admin/system/ai-agents",
                "56ef5233-c003-40e5-85f3-a771333e4165" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "fc231eb9-6fd6-40b1-a678-fb4e85c70cd1", // Page: AI Agents > Settings
                "admin/system/ai-agents/settings",
                "98f8b545-00ee-4b1d-91fb-2cd69de14f05" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "74b916c7-256a-42e1-8e4e-951450d23152", // Page: AI Agents > AI Agent Providers
                "admin/system/ai-agents/providers",
                "c378369d-7c3c-46e0-b6f9-6041e1e7215e" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "db33d4a6-c5c3-4cae-b121-37588a513e29", // Page: AI Agents > Agents
                "admin/system/ai-agents/agents",
                "f34311c5-ca4c-4ca7-8029-df66021133f4" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "c7bca1fb-b627-4a8c-8c9f-43ae69fa69fc", // Page: AI Agents > Agents > AI Agent Detail
                "admin/system/ai-agents/agents/{AIAgentId}",
                "31936207-b8fd-4d90-b383-ca205fc53491" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "e1c14e52-9e06-4618-ab15-63261f9ba79b", // Page: AI Agents > AI Skills
                "admin/system/ai-agents/skills",
                "def90397-9494-48d2-8646-46480b706958" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "6f89544f-50c0-42d6-b925-fb6e404b434c", // Page: AI Agents > AI Skills > AI Skill Detail
                "admin/system/ai-agents/skills/{AISkillId}",
                "21460e43-2f12-48c3-87e9-e8c41965213d" );

            // ------------------------------------------------------
            // Register the Chat Bot block type.

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.Blocks.AI.ChatBot",
                "c08511a6-d9f5-40f4-a9cc-50cbe40a4ab8",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Chat Bot",
                "Allows the user to try out the chat agent.",
                "Rock.Blocks.AI.ChatBot",
                "AI",
                "91a66c59-830e-49b5-a196-dcf93d0dde92" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "91a66c59-830e-49b5-a196-dcf93d0dde92", // BlockType: Chat Bot
                SystemGuid.FieldType.BOOLEAN,
                "Docked Mode",
                "DockedMode",
                "Docked Mode",
                "In Docked mode, the chat bot will appear as a docked panel on the page.",
                1,
                "False",
                "039390ce-59b3-461b-b3c8-a06b812761e1" );

            // ------------------------------------------------------
            // Add the new AI Chat page to the AI Agents page.

            RockMigrationHelper.AddPage(
                skipIfAlreadyExists: true,
                "9f7b9158-3a73-429a-a817-5909d2aed13c", // Page: AI Agents
                "c2467799-bb45-4251-8ee6-f0bf27201535", // Layout: Full Worksurface
                "AI Chat",
                string.Empty,
                "da5ac7d1-fbd7-4e1d-81d7-ef28bbb6fcf2" );

            RockMigrationHelper.AddOrUpdatePageRoute(
                "da5ac7d1-fbd7-4e1d-81d7-ef28bbb6fcf2", // Page: AI Chat
                "admin/system/ai-agents/chat",
                "3a222137-456b-48a2-86f9-fcc18b4ef3d1" );

            RockMigrationHelper.UpdatePageIcon(
                "da5ac7d1-fbd7-4e1d-81d7-ef28bbb6fcf2", // Page: AI Chat
                "ti ti-message-dots" );

            // Add the Spark Connected Services block to the Spark Connected
            // Services page.
            RockMigrationHelper.AddBlock(
                skipIfAlreadyExists: true,
                "da5ac7d1-fbd7-4e1d-81d7-ef28bbb6fcf2", // Page: AI Chat
                null,
                "91a66c59-830e-49b5-a196-dcf93d0dde92", // Block Type: Chat Bot
                "Chat Bot",
                "Main",
                string.Empty,
                string.Empty,
                0,
                "1a25dc3c-e1d1-4aa8-bbf1-7a36e11a33f7" );

            // ------------------------------------------------------
            // Add the Chat Bot to the Internal Site header.

            // Add the Spark Connected Services block to the Spark Connected
            // Services page.
            RockMigrationHelper.AddBlock(
                true,
                null,
                null,
                SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), // Site: Rock Internal
                "91a66c59-830e-49b5-a196-dcf93d0dde92".AsGuid(), // Block Type: Chat Bot
                "Chat Bot",
                "Header",
                string.Empty,
                string.Empty,
                0,
                "2416603a-7632-4dfe-a34f-59f09886a4b1" );

            RockMigrationHelper.AddBlockAttributeValue(
                "2416603a-7632-4dfe-a34f-59f09886a4b1", // Block: Chat Bot in Site Header
                "039390ce-59b3-461b-b3c8-a06b812761e1", // Attribute: Docked Mode
                "True" );

            // Move the Chat Bot block to be the last block in the header.
            Sql( @"
DECLARE @SiteId INT = (SELECT [Id] FROM [Site] WHERE [Guid] = 'c2d29296-6a87-47a9-a753-ee4e9159c4c4')
DECLARE @Order INT = (SELECT ISNULL(MAX([Order]), 0) FROM [Block] WHERE [SiteId] = @SiteId) + 1

UPDATE [Block]
SET [Order] = @Order
WHERE [Guid] = '2416603a-7632-4dfe-a34f-59f09886a4b1'
" );
        }

        /// <summary>
        /// Adds or updates the AI skill with the specified parameters. If the
        /// skill already exists for the specified <paramref name="skillGuid"/>
        /// then it will be updated with the new values. If it does not exist,
        /// it will be created with the specified values.
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
        /// Adds or updates the AI skill tool with the specified parameters. If the
        /// skill tool already exists for the specified <paramref name="skillGuid"/>
        /// then it will be updated with the new values. If it does not exist,
        /// it will be created with the specified values.
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

        /// <summary>
        /// Deletes AttributeValue rows written with EntityId = EventItem.Id for
        /// EventCalendarItem-scoped attributes by the Obsidian Event Item Detail block
        /// regression (fixed in this branch). Only rows whose EntityId does not match a
        /// real EventCalendarItem on the attribute's qualifying calendar are removed.
        ///
        /// Reason: https://github.com/SparkDevNetwork/Rock/issues/6962
        /// </summary>
        private void NA_CleanupEventItemAttributeCorruption_6962_Up()
        {
            Sql( @"
DECLARE @EventCalendarItemEntityTypeId INT = (
    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.EventCalendarItem'
);

IF @EventCalendarItemEntityTypeId IS NOT NULL
BEGIN
    DECLARE @BatchSize INT = 5000;
    DECLARE @Deleted   INT = 1;

    WHILE @Deleted > 0
    BEGIN
        DELETE TOP (@BatchSize) av
        FROM [AttributeValue] av
        INNER JOIN [Attribute] a
            ON a.[Id] = av.[AttributeId]
        LEFT JOIN [EventCalendarItem] eci
            ON eci.[Id] = av.[EntityId]
           AND eci.[EventCalendarId] = TRY_CAST( a.[EntityTypeQualifierValue] AS INT )
        WHERE a.[EntityTypeId] = @EventCalendarItemEntityTypeId
          AND a.[EntityTypeQualifierColumn] = 'EventCalendarId'
          AND av.[EntityId] IS NOT NULL
          AND eci.[Id] IS NULL;

        SET @Deleted = @@ROWCOUNT;
    END
END
" );
        }

        /// <summary>
        /// Updates the default "Payment Information Instructions" for the Payment Entry
        /// workflow action so the "Amount: $X" line is only rendered when an amount has
        /// been supplied. Without this guard, the individual-entered amount case renders
        /// both a stale "Amount: $0.00" line and the CurrencyBox "Amount" label, which
        /// visually collide. Fix for issue #6951.
        /// </summary>
        private void NA_FixPaymentEntryInstructionsDefault6951_Up()
        {
            /*
                8/4/26 - NA

                The default value for the PaymentInformationInstructions attribute on the
                Payment Entry workflow action unconditionally rendered "<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}".
                When the workflow is configured to let the individual enter the amount
                (Amount attribute is blank), that line still rendered as "Amount: ..."
                and stacked visually with the CurrencyBox "Amount" label below it.

                Wrapping the line in {% if PaymentConfiguration.Amount %}...{% endif %}
                makes the two mutually exclusive. Both the Attribute default and any
                AttributeValue rows still holding the old shipped default are updated;
                customized values are left alone. The DefaultPersisted* columns are
                cleared and IsDefaultPersistedValueDirty is set so Rock re-renders them
                from the new DefaultValue on the next persistence pass.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6951
            */
            Sql( @"
DECLARE @WorkflowActionTypeEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActionType' );
DECLARE @EntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.PaymentEntry' );
DECLARE @AttributeId INT = ( SELECT [Id] FROM [Attribute]
    WHERE [Key] = 'PaymentInformationInstructions'
      AND [EntityTypeId] = @WorkflowActionTypeEntityTypeId
      AND [EntityTypeQualifierColumn] = 'EntityTypeId'
      AND [EntityTypeQualifierValue] = CAST( @EntityTypeId AS NVARCHAR( 100 ) ) );

DECLARE @OldFragment NVARCHAR(MAX) = '""<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}""';
DECLARE @NewFragment NVARCHAR(MAX) = '""{% if PaymentConfiguration.Amount %}<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}{% endif %}""';

IF @AttributeId IS NOT NULL
BEGIN
    -- Update the Attribute default so newly created Payment Entry actions inherit the corrected default.
    UPDATE [Attribute]
    SET [DefaultValue] = REPLACE( [DefaultValue], @OldFragment, @NewFragment ),
        [DefaultPersistedTextValue] = NULL,
        [DefaultPersistedHtmlValue] = NULL,
        [DefaultPersistedCondensedTextValue] = NULL,
        [DefaultPersistedCondensedHtmlValue] = NULL,
        [IsDefaultPersistedValueDirty] = 1
    WHERE [Id] = @AttributeId

    -- Update any existing AttributeValue rows still holding the old shipped default.
    UPDATE [AttributeValue]
    SET [Value] = REPLACE( [Value], @OldFragment, @NewFragment ),
        [PersistedTextValue] = NULL,
        [PersistedHtmlValue] = NULL,
        [PersistedCondensedTextValue] = NULL,
        [PersistedCondensedHtmlValue] = NULL,
        [IsPersistedValueDirty] = 1
    WHERE [AttributeId] = @AttributeId
      AND [Value] LIKE '%' + @OldFragment + '%';
END
" );
        }

        /// <summary>
        /// Follow-up to hotfix 306 for issue #6902. Two cleanup items remained in
        /// the [spCrm_FamilyAnalyticsEraDataset] stored procedure after that fix:
        /// the outer pre-filter WHERE clauses that populate the giving and
        /// attendance temp tables still compared the raw TransactionDateTime /
        /// StartDateTime columns against a Sunday-midnight variable, which is the
        /// same buggy pattern that was corrected elsewhere. Those two
        /// lines are now aligned to use SundayDate week-boundary logic. This
        /// migration also switches the two GETDATE() calls in the procedure to
        /// dbo.RockGetDate() so eRA start/created timestamps honor Rock's
        /// configured time zone (often matters for Azure-hosted SQL databases whose
        /// server time zone may not match the organization's).
        /// </summary>
        private void NA_FixEraFamilyAnalyticsWeekBoundariesFollowup6902_Up()
        {
            Sql( HotFixMigrationResource._311_FixEraFamilyAnalyticsWeekBoundariesFollowup6902_spCrm_FamilyAnalyticsEraDataset );
        }

        /// <summary>
        /// Backfills the ParentEntityId and ParentEntityTypeId columns on BinaryFile rows that
        /// belong to a SignatureDocument but were created without those parent-entity references,
        /// so that BinaryFile.ParentEntityAllowsView (used by GetFile.ashx) can delegate authorization
        /// to the SignatureDocumentTemplate ACL. Fix for issue #6928.
        /// </summary>
        private void NA_FixSignatureDocumentBinaryFileParentEntity6928_Up()
        {
            /*
                8/5/26 - NA

                The 2023 fix for issue #5599 (commit fe56f8318f, Rock v16.1) established that a
                signed document's BinaryFile should link back to its SignatureDocumentTemplate via
                ParentEntityId / ParentEntityTypeId so that BinaryFile.ParentEntityAllowsView can
                delegate authorization to the template. That fix only patched
                Rock.Blocks/Event/RegistrationEntry.cs, leaving two other creation paths --
                RockWeb/Blocks/WorkFlow/WorkflowEntry.ascx.cs and
                Rock/Workflow/Action/WorkflowControl/ElectronicSignature.cs.CreateSignedDocumentPdf
                -- still emitting BinaryFiles with NULL parent-entity columns. As a result, direct
                downloads via GetFile.ashx bypassed the template ACL for any file created through
                those paths (and for any pre-v16.1 file regardless of path), letting a person view
                a signed document that the template ACL had explicitly denied them.

                Both source paths are fixed in this branch. This migration backfills the historical
                BinaryFile rows so their parent-entity link matches what future rows will have. It
                only touches rows where at least one of the two columns is NULL, so any manual
                customization is preserved.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6928
            */
            Sql( @"
DECLARE @SignatureDocumentTemplateEntityTypeId INT = (
    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SignatureDocumentTemplate'
);

IF @SignatureDocumentTemplateEntityTypeId IS NOT NULL
BEGIN
    UPDATE bf
    SET    bf.[ParentEntityTypeId] = @SignatureDocumentTemplateEntityTypeId,
           bf.[ParentEntityId]     = sd.[SignatureDocumentTemplateId]
    FROM   [BinaryFile] bf
    INNER JOIN [SignatureDocument] sd ON sd.[BinaryFileId] = bf.[Id]
    WHERE  ( bf.[ParentEntityTypeId] IS NULL OR bf.[ParentEntityId] IS NULL )
        AND sd.[SignatureDocumentTemplateId] IS NOT NULL;
END
" );
        }

        /// <summary>
        /// Fix for issue #6959. In [spCrm_PersonDuplicateFinder] there are three
        /// UPDATE statements that join the Person table to itself to match on
        /// BirthDate, MaritalStatusValueId, and SuffixValueId. Those columns are
        /// nullable, and when a lot of rows have NULL in them (which can happen
        /// on larger databases where these fields aren't consistently captured)
        /// the query slows down badly. SQL Server can pick a join plan that
        /// walks through all the NULL rows first and only afterward realizes
        /// none of them can match (in SQL, NULL never equals NULL). On the
        /// database that triggered this issue, one of the three statements ran
        /// for about two hours.
        ///
        /// The fix adds "AND column IS NOT NULL" for both sides of each of the
        /// three joins. That tells SQL Server up front to skip the NULL rows,
        /// so it can use the existing index on the column and finish quickly.
        /// It doesn't change what rows are returned (NULLs were already being
        /// filtered out by the equality check); it just gets there faster. The
        /// same pattern is already used a few lines earlier in the procedure
        /// for LastName and Campus, so this brings BirthDate, MaritalStatus,
        /// and Suffix into line with them.
        /// </summary>
        private void NA_FixPersonDuplicateFinderNullSelfJoinPerformance6959_Up()
        {
            Sql( HotFixMigrationResource._313_FixPersonDuplicateFinderNullSelfJoinPerformance6959_spCrm_PersonDuplicateFinder );
        }

    }
}
