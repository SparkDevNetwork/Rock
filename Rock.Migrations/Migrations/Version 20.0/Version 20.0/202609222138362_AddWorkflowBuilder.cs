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
        9/22/2026 - CLAUDE

        Seeds the Workflow Composer agents: an MCP agent and a chat agent
        preconfigured with the skills a workflow author needs (Workflow Builder,
        Core Administration, Community Knowledge Base, and the lookups from the
        Group, Connection, Communication, Content Channel, Cms, Reporting, Note,
        Step, Reminder, Benevolence and Person skills for the records workflow
        action settings reference), plus a Workflow Composer page under AI Agents that
        hosts the chat agent. This follows the AddCodeComposer and
        AddCodeComposerPage migrations, combined into one.

        The same deliberate choices apply here:

        1. The skill EntityTypes, skills and the enabled tools are registered
           before the rows that reference them. Startup registration runs AFTER
           migrations, so on a fresh install the skills would not exist yet and
           every attachment below would silently no-op. Only the tools this
           migration enables are seeded; startup registers the rest.

        2. Attaching a skill to an agent does NOT enable its tools. Each
           AIAgentSkill row carries an explicit EnabledTools allowlist.

        3. The agent rows are created only when absent, never updated apart from
           the IsSystem flag and a rename of rows still carrying the development
           name (Workflow Builder), so an administrator's retuning survives a re-run.
           Each row's AdditionalSettingsJson carries the SHA-256 of the seeded
           Instructions under SeededInstructionsHash so a later migration can
           ship instruction fixes to untouched rows only.

        Security is administrator-only on both agents and on the Workflow
        Builder skill. These tools create and delete workflow types, and a
        workflow can run SQL, send communications, and change people's records.
        The shared skills are left with whatever security they already have.

        Reason: Ship the Workflow Composer agents preconfigured instead of hand-assembled.
    */

    /// <summary>
    /// Adds the Workflow Composer MCP and chat agents, the skills they carry, and
    /// the Workflow Composer page that hosts the chat agent.
    /// </summary>
    public partial class AddWorkflowBuilder : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The Guid of the Workflow Composer MCP Agent AIAgent row.
        /// </summary>
        private const string WorkflowComposerMcpAgentGuid = "5EDECB28-22DD-4807-80D2-342862FCEED2";

        /// <summary>
        /// The display name of the Workflow Composer MCP Agent.
        /// </summary>
        private const string WorkflowComposerMcpAgentName = "Workflow Composer (MCP)";

        /// <summary>
        /// The MCP slug the Workflow Composer MCP Agent is served under (/api/v2/mcp/workflow-composer).
        /// </summary>
        private const string WorkflowComposerMcpAgentSlug = "workflow-composer";

        /// <summary>
        /// The Guid of the Workflow Composer Chat Agent AIAgent row.
        /// </summary>
        private const string WorkflowComposerChatAgentGuid = "2585435A-01F8-4E43-8AB7-5135101A66DB";

        /// <summary>
        /// The display name of the Workflow Composer Chat Agent.
        /// </summary>
        private const string WorkflowComposerChatAgentName = "Workflow Composer (Chat)";

        /// <summary>
        /// The AdditionalSettingsJson key under which each agent row stores the
        /// SHA-256 of the Instructions text this migration seeded.
        /// </summary>
        private const string SeededInstructionsHashKey = "SeededInstructionsHash";

        /// <summary>
        /// The AISkill Guid of the Workflow Builder skill.
        /// </summary>
        private const string WorkflowBuilderSkillGuid = "A74514DD-9955-49D6-8DC3-A33033797B0A";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.WorkflowBuilderSkill</c>.
        /// </summary>
        private const string WorkflowBuilderSkillEntityTypeGuid = "7A9C6D45-947B-4B32-A09E-23718F0C8A08";

        /// <summary>
        /// The AISkill Guid of the Core Administration skill.
        /// </summary>
        private const string CoreAdministrationSkillGuid = "6DBD6867-2E0B-4D2E-9BF9-B34B77E4E94B";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CoreAdministrationSkill</c>.
        /// </summary>
        private const string CoreAdministrationSkillEntityTypeGuid = "55EB1E6F-EFBF-4E9C-BA11-DBC7147DA342";

        /// <summary>
        /// The AISkill Guid of the Group skill.
        /// </summary>
        private const string GroupSkillGuid = "FA40A5E9-DF52-4645-B3ED-CF9BBF79B12F";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.GroupSkill</c>.
        /// </summary>
        private const string GroupSkillEntityTypeGuid = "EC39756F-44BA-4000-BB75-4335DDC95BC6";

        /// <summary>
        /// The AISkill Guid of the Connection skill.
        /// </summary>
        private const string ConnectionSkillGuid = "02214EF2-B1AB-52A4-42FE-C722262925EE";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.ConnectionSkill</c>.
        /// </summary>
        private const string ConnectionSkillEntityTypeGuid = "FE485F5E-7422-78BB-4973-692975860393";

        /// <summary>
        /// The AISkill Guid of the Communication skill.
        /// </summary>
        private const string CommunicationSkillGuid = "37DF3637-9775-4A89-9A77-BF6744232991";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.CommunicationSkill</c>.
        /// </summary>
        private const string CommunicationSkillEntityTypeGuid = "F67D0B02-B59F-475F-A005-8F2A5CCCA91C";

        /// <summary>
        /// The AISkill Guid of the Content Channel skill.
        /// </summary>
        private const string ContentChannelSkillGuid = "D450C4D3-1DEB-4B2D-A8E6-4F46CD722A0B";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.ContentChannelSkill</c>.
        /// </summary>
        private const string ContentChannelSkillEntityTypeGuid = "38FA0FC0-CF91-4F68-8B32-88DB87058953";

        /// <summary>
        /// The AISkill Guid of the Reporting skill.
        /// </summary>
        private const string ReportingSkillGuid = "39BB9DB1-569A-44C1-9F1D-61E8B16D8846";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.ReportingSkill</c>.
        /// </summary>
        private const string ReportingSkillEntityTypeGuid = "F8E8E905-6893-442F-8331-6DFC352C86C1";

        /// <summary>
        /// The AISkill Guid of the Note skill.
        /// </summary>
        private const string NoteSkillGuid = "216E5428-DE1A-4458-A22C-22812955264A";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.NoteSkill</c>.
        /// </summary>
        private const string NoteSkillEntityTypeGuid = "76DD142A-FB37-4B9E-A1F0-305A5B675B76";

        /// <summary>
        /// The AISkill Guid of the Step skill.
        /// </summary>
        private const string StepSkillGuid = "644CAFF4-73EF-43A3-9864-1B08614036C0";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.StepSkill</c>.
        /// </summary>
        private const string StepSkillEntityTypeGuid = "4490B637-10F7-4912-8008-AF6A061587C1";

        /// <summary>
        /// The AISkill Guid of the Reminder skill.
        /// </summary>
        private const string ReminderSkillGuid = "A7CDC0C6-DCA6-4E77-9295-245B18556BB1";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.ReminderSkill</c>.
        /// </summary>
        private const string ReminderSkillEntityTypeGuid = "41179AB0-702C-435D-94BA-EC6EAE22E39B";

        /// <summary>
        /// The AISkill Guid of the Benevolence skill.
        /// </summary>
        private const string BenevolenceSkillGuid = "D7340FAE-917C-4A96-8958-99EC8361328A";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.BenevolenceSkill</c>.
        /// </summary>
        private const string BenevolenceSkillEntityTypeGuid = "43F23F97-2360-4089-AD6E-C1DDCDF4665B";

        /// <summary>
        /// The AISkill Guid of the Person skill.
        /// </summary>
        private const string PersonSkillGuid = "DD5FA7DD-3277-4C31-848D-285CD67AC7CA";

        /// <summary>
        /// The EntityType Guid of <c>Rock.AI.Agent.Skills.PersonSkill</c>.
        /// </summary>
        private const string PersonSkillEntityTypeGuid = "12E7BDEA-B67A-48D7-8D1E-245BF8E9B555";

        /// <summary>
        /// The AISkill Guid of the Community Knowledge Base skill. The skill and
        /// its tools are seeded by the AddCodeComposer migration.
        /// </summary>
        private const string CommunityKnowledgeBaseSkillGuid = "DFCBFDE8-6BF2-4DDF-81FE-FDD436E5FD90";

        /// <summary>
        /// The AISkill Guid of the Cms skill. The skill and its tools are seeded
        /// by the AddForgeContentAndCmsSkill migration.
        /// </summary>
        private const string CmsSkillGuid = "613D7110-6453-4BAB-892B-064222F8397C";

        /// <summary>
        /// The Guid of the AI Agents page that the Workflow Composer page is added under.
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
        /// The Guid of the new Workflow Composer page.
        /// </summary>
        private const string WorkflowComposerPageGuid = "3F441C0C-33C8-4BB0-ADA9-78BA13C455C0";

        /// <summary>
        /// The Guid of the Chat Bot block placed on the Workflow Composer page.
        /// </summary>
        private const string WorkflowComposerChatBotBlockGuid = "9B470FF0-D56F-4979-B588-4CD96D113128";

        /// <summary>
        /// The Guid of the Workflow Composer page's route.
        /// </summary>
        private const string WorkflowComposerPageRouteGuid = "F8C1E42F-4D63-4272-AF0B-575C887E621A";

        /// <summary>
        /// The Guid of the Code Composer page added by the AddCodeComposerPage migration.
        /// </summary>
        private const string CodeComposerPageGuid = "98F5C951-8AE9-4160-92B5-53FAB06C7CD5";

        /// <summary>
        /// The Guid of the Code Composer page's route. The AddCodeComposerPage
        /// migration created the page without one.
        /// </summary>
        private const string CodeComposerPageRouteGuid = "1F6D6F26-399B-44B6-A9A6-85368DB2E9A8";

        #endregion Constants

        #region Instructions

        /*
            9/22/2026 - CLAUDE

            Both agents carry the same instructions, taken from the agents that
            were tuned by hand during development. The text contains an emoji,
            so it is inserted as an N'' literal; a plain '' literal would store
            it as question marks. The emoji is written as an escape sequence so
            the source file's encoding cannot corrupt it.

            Reason: One source for the shared text, stored without losing characters.
        */

        /// <summary>
        /// The instructions both Workflow Composer agents share.
        /// </summary>
        private const string WorkflowComposerInstructions = @"# Workflow Builder Agent Instructions

## Workflow building rules

* Before starting any request to build or edit a workflow's configuration, you must use the Community Knowledge Base and read the topic `rock_workflow_builder`. Detailed field type information is in the Rock Coding Guide at `coding-guide/field-types`. The field type tools also return a `valueFormat` describing how each value must be stored, so trust that inline hint when writing values (for example, a Person attribute stores a person alias guid, not the person's own guid).

* Read the Community Knowledge Base documentation for every action, field type, and Lava command or filter you use before configuring it.

* If it's unclear how the workflow will be initiated, ask. Most of the time it will be user-initiated, but Rock supports workflows launched from several areas (grids, connection requests, automations, etc.). The launch mechanism decides whether the workflow receives an entity that must be captured with an Attribute Set from Entity action, so settle this before creating attributes.

* Never invent values for critical workflow settings, and never offer example values that don't exist in this Rock instance. A setting is critical when it decides who is affected or what happens to real data: dates, statuses, approval states, recipients, connection statuses, campuses, groups, and content channels. Before building, identify every critical setting the requester has not provided. Ask for it rather than selecting a default, using a value such as TODAY, or making an assumption.

* Choose the best option yourself for non-critical configuration the requester did not specify, rather than asking. This covers things like the workflow type's name, description, icon, and category, attribute names and keys, activity and action names, and form button labels. Pick values that fit the request and match what already exists in this Rock instance, such as an existing category that suits the workflow.

* When asking for additional requirements from the user, separate your ask into its own section noted with a " + "\U0001F6A9" + @" in the title and number the items you need.

* Before telling the user a workflow is done, read it back with GetWorkflowTypeConfiguration and resolve any warnings it returns.

* When a workflow collects or looks up personal data, or reads values from a URL, state the security implication in your summary before the user asks, including whether the page needs to be access-restricted.

* Remind the user that a new workflow type inherits its security from the category it is filed under and may need explicit permissions set so the right people can use and manage it: **View**, **Edit**, **View List**, and **Administrate**.

* After creating or updating a workflow, show a confirmation section that lists every value you chose on the user's behalf, such as the name, category, and attribute keys, along with any assumptions you made. Keep it short so the user can scan it and ask for changes.

* If you need styling information, see the knowledge base topic `rock_styling`.

* When you're done creating or modifying a workflow, provide a link to it: `/page/136?WorkflowTypeId={idKey}`.

---

## Display and formatting

When displaying data, make a pleasant UX using markdown:

* Structure responses like a web UI: start with a `### Header` summarizing the query, followed by sections with tables or lists for content.
* Show a friendly intro above the information you are displaying.
* Use emoji when it adds color and visual hierarchy.
* Use `<hr>` as separators between content sections.
* Bold information that would make a good title.
* Show only the information you believe the user needs to see.
* When there are 4 or more options, consider showing the results in a table.
* NEVER use block quotes in your response.
* NEVER use h1 or h2 tags. Start your hierarchy at h3.
";

        #endregion Instructions

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RegisterEntityTypes_Up();
            AddWorkflowBuilderSkill_Up();
            AddSharedSkillsAndTools_Up();
            AddWorkflowComposerMcpAgent_Up();
            AddWorkflowComposerChatAgent_Up();
            AttachSkillsToAgents_Up();
            AddWorkflowComposerPage_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /*
                9/22/2026 - CLAUDE

                The page goes first because its block points at the chat agent.
                The agents go next because AIAgentSkill cascades from AIAgent, then
                the Workflow Builder skill (which cascades to its tools). The
                shared skills, their tools, and the EntityType rows are left in
                place: other agents use them and startup registration maintains
                them.

                Reason: A downgrade must remove what this migration seeded and nothing more.
            */
            AddWorkflowComposerPage_Down();
            RemoveWorkflowComposerAgents_Down();
            RemoveWorkflowBuilderSkill_Down();
        }

        #region Entity Types

        /// <summary>
        /// Registers the skill EntityTypes this migration references. Startup
        /// EntityType registration runs after migrations, so a missing EntityType
        /// would yield a null CodeEntityTypeId and a skill that exposes nothing.
        /// The Cms and Community Knowledge Base skill EntityTypes are registered
        /// by earlier migrations and are not repeated here.
        /// </summary>
        private void RegisterEntityTypes_Up()
        {
            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.WorkflowBuilderSkill",
                WorkflowBuilderSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CoreAdministrationSkill",
                CoreAdministrationSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.GroupSkill",
                GroupSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.ConnectionSkill",
                ConnectionSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.CommunicationSkill",
                CommunicationSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.ContentChannelSkill",
                ContentChannelSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.ReportingSkill",
                ReportingSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.NoteSkill",
                NoteSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.StepSkill",
                StepSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.ReminderSkill",
                ReminderSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.BenevolenceSkill",
                BenevolenceSkillEntityTypeGuid,
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityType(
                "Rock.AI.Agent.Skills.PersonSkill",
                PersonSkillEntityTypeGuid,
                false,
                false );
        }

        #endregion Entity Types

        #region Skills

        /*
            9/22/2026 - CLAUDE

            Every seeded skill and tool name and description below is exactly
            what startup registration derives from the class and method
            [Description] text. Startup overwrites these rows on every
            application start, so any drift here would last only until the first
            restart.

            Reason: Seeded values must match what startup registration derives.
        */

        /// <summary>
        /// Registers the Workflow Builder skill, its security, and its tools.
        /// </summary>
        private void AddWorkflowBuilderSkill_Up()
        {
            AddOrUpdateCodeAISkill(
                "Workflow Builder Skill",
                "Provides the ability to author Rock workflow types: discovering actions and their settings, and creating, editing, and removing workflow structure.\n\nIdentifiers: every parameter in this skill takes an idKey, and the skill converts it to whatever Rock stores internally. Never put a guid in a parameter. A few values are different, because what you send is written into Rock's own configuration unchanged. Those must hold a record's guid rather than its idKey when they reference another record, and each one says so in its own description.",
                WorkflowBuilderSkillEntityTypeGuid,
                WorkflowBuilderSkillGuid );

            AddAdministratorOnlySecurityForAISkill(
                WorkflowBuilderSkillGuid,
                "598A03F5-59E3-44C5-B4F4-FCED42618FFE",
                "CDFC6AEE-0323-40FD-8824-778E43E0C6AB" );

            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Action Form", "Adds the form shown by a user entry action, or updates an existing one, including its fields and buttons.", "AEF3A669-4696-42E0-AC97-FF109CC72FE2" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Action Type", "Adds an action to a workflow activity, or updates an existing one. An action is one step, such as sending an email or setting a value.", "8C1147F0-7A46-4274-9A6D-668ECD052B87" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Activity Type", "Adds an activity to a workflow type, or updates an existing one. An activity is a group of actions that run together.", "85129AA8-724E-4BAB-BBAB-7DF9253F11DE" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Attribute", "Adds a workflow attribute, which is a variable the workflow's actions read and write, or updates an existing one.", "8C371846-1F64-4A47-AB68-416CC584C85E" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Form Person Entry", "Turns on the person entry block of a user entry form, or updates its settings. Person entry collects a real person, matching or creating the record, rather than collecting plain attribute values.", "3E7B1A4C-5D26-4F98-9C03-8B41D5E6720F" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Add Or Update Workflow Type", "Adds a new workflow type or updates an existing one. This creates the workflow shell only; its attributes, activities, and actions are added separately.", "DD2120CD-0FD6-45FC-8633-60FFA69B16CC" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Delete Workflow Action Type", "Removes one action from a workflow activity, along with its form and its execution history across every existing workflow instance.", "31D70F62-B03A-47F7-8086-1C00637847CB" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Delete Workflow Activity Type", "Removes an activity from a workflow type, along with every action inside it and its execution history across every existing workflow instance.", "02164475-C6BB-4F8A-822C-BC37FEA77F03" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Delete Workflow Attribute", "Removes a workflow attribute, every form field that edits it, and every value stored for it across all existing workflow instances.", "A8A8FA55-A8D5-4791-991C-691E5D8279C3" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Delete Workflow Type", "Removes a whole workflow type: its attributes and their stored values, its activities and actions, their forms, and every workflow instance with its entire execution history.", "6C0A6C0E-9E24-4C1B-B60E-2B2A2A5FA7F1" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Get Workflow Action Type", "Gets one configured workflow action with its settings and form markup returned in full, not clipped.", "B6EB95E5-80EA-4E77-A4DE-89BB327D382F" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Get Workflow Action Type Available Attributes", "Gets the settings a workflow action component accepts, along with each setting's field type and allowed values.", "99871B11-0F69-4E0D-BCBA-446317F8B5B6" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Get Workflow Type Configuration", "Gets a workflow type in full, including its attributes and every activity and action. Long values are clipped.", "01D84BCE-8B18-4200-9435-3D1F5572BD92" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "List Workflow Types", "Lists the workflow types in Rock, with the count of activities in each.", "5B0804A3-F2F4-4F5E-AB3C-E0F618370BE1" );
            AddOrUpdateCodeAISkillTool( WorkflowBuilderSkillGuid, "Lookup Workflow Action Components", "Looks up the workflow action components installed in Rock, meaning the kinds of action that can be added to a workflow activity.", "D319EB2C-F2CE-44F2-80E1-0705C6AC68DF" );
        }

        /// <summary>
        /// Registers the shared skills the Workflow Composer agents carry, and the
        /// tools of each that the agents enable. Their security is left alone
        /// because other agents already use them.
        /// </summary>
        private void AddSharedSkillsAndTools_Up()
        {
            // Core Administration.
            AddOrUpdateCodeAISkill(
                "Core Administration Skill",
                "Provides access to Rock's core configuration metadata: defined types and values, entity types, categories, field types, attributes, and system communications.",
                CoreAdministrationSkillEntityTypeGuid,
                CoreAdministrationSkillGuid );

            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Category", "Gets a single category in full detail.", "9E3E5A2C-6D67-4F3B-B0AC-11A02C43B0E1" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Defined Type", "Gets the configuration of a single defined type. This does not include the type's values.", "366B42BD-9D92-4042-8B20-04EE6B0142C7" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Defined Value", "Gets a single defined value in full detail, including its unique identifier.", "BF14C7EA-98DC-4FFF-8485-F9952B2F4B8B" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Defined Value Available Attributes", "Gets the available attributes that can be set on the values of a defined type. Supply either the defined type or any one of its values.", "542ED067-19EA-4DEE-B8DA-47FBB47C467D" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Entity Available Attributes", "Gets the attributes that apply to every entity of a given type. Returns only unqualified attributes; entities whose attributes depend on a qualifier have their own tool.", "2A0EF1D6-8C10-4E9C-BCDB-0FCA3FEC0998" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get Field Type", "Gets a single field type in full detail, including the configuration qualifiers accepted when creating an attribute with it.", "CD8C8E44-F60C-4F1A-A480-683C600C526E" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Get System Communication", "Gets a single system communication in full detail. The message body itself is never returned.", "1D1D0F7C-6B22-4C4E-9E4B-BC5A0A9F1D74" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "List Categories", "Lists the categories defined for one entity type, such as the categories a workflow type can be filed under.", "8B1EFF0E-AAE0-43BF-A2DA-D1C71EADF28B" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "List Defined Types", "Lists the defined types configured in Rock. A defined type is a named set of values, such as Marital Status or Connection Status.", "53DDA7C1-00A5-4531-8A5D-07FBC6721798" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "List Defined Values", "Lists the values of a single defined type.", "0351DA93-E519-48D6-BB05-21D93A9583CA" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "List Entity Types", "Lists the entity types registered in Rock, such as Person, Group, or Workflow. Filter by partial name to find the one you need.", "7BD8DF7C-09AA-4809-8364-37D594370E99" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "List System Communications", "Lists the system communication templates configured in Rock, such as the templates used by workflow actions to send email.", "83AFE4C8-F8BC-4BF8-A7D6-6FDCF8AD8561" );
            AddOrUpdateCodeAISkillTool( CoreAdministrationSkillGuid, "Lookup Field Types", "Looks up the field types installed in Rock, such as Text, Person, or Single Select. Use the returned key whenever a field type must be specified.", "04F39FBF-A3B4-4F1F-88E7-49E1D3AE73A7" );

            // Group.
            AddOrUpdateCodeAISkill(
                "Group Skill",
                "This skill provides access to group related data.",
                GroupSkillEntityTypeGuid,
                GroupSkillGuid );

            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "Add Or Update Group Member", "Adds a new or updates an existing group member.", "085141CC-09C1-40D8-831C-FAF4DA96D604" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "Delete Group Member", "Removes a group member from a group.", "C2800552-0BEB-4F54-8F1D-5EEB01DF4192" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "Get Group Type", "Gets a single group type in full configuration detail.", "2846FCA1-4B7B-4623-903A-2293C17436A3" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "List Group Members", "Returns a list of group members.", "66580B04-14E6-4FA8-8367-EFE8F2A7E7ED" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "List Groups", "Returns a list of groups.", "94FDE11F-6243-4F4E-8854-66C9625B9DE1" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "List Scheduled Attendance", "Returns a list of scheduled attendance for people to serve as part of a group.", "E317AD31-3230-4D16-B039-752C7BEB68C6" );
            AddOrUpdateCodeAISkillTool( GroupSkillGuid, "Lookup Group Types", "Retrieves the group types configured in Rock.", "23C5AD96-68F8-4D3E-B2E9-96E179A08E5A" );

            // Connection.
            AddOrUpdateCodeAISkill(
                "Connection Skill",
                "This skill provides an overview of connection features.",
                ConnectionSkillEntityTypeGuid,
                ConnectionSkillGuid );

            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Add Or Update Connection Request", "Adds new or updates existing connection request.", "8EE3913A-9BCA-4971-A490-90ABFC1690C3" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Get Connection Request", "Gets the details of an existing connection request.", "3E00B0EF-9AA8-4E77-8BCC-961A6EA6FD9C" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Get Connection Request Available Attributes", "Gets the available attributes that can be set when adding or updating a connection request.", "C660989A-BA62-42F8-8EED-49C0BF7E8BF6" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Get Connection Request Insights", "Returns the insights of connection requests.", "51E14E2D-09A4-440E-9E7D-DF1BF22BD918" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Get Connection Request Summary", "Returns a summary of connection requests for the user.", "B3DF0351-AA63-44BF-98FD-16FC56AD2D39" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "List Connection Requests", "Returns a list of connection requests that match the filters.", "DC03271E-2C54-D5AF-4F18-9CCC69F25202" );
            AddOrUpdateCodeAISkillTool( ConnectionSkillGuid, "Lookup Connection Types And Opportunities", "Retrieves all configured connection types and opportunities in Rock.", "21870C06-126F-0882-47E3-DBFC1846BD92" );

            // Communication.
            AddOrUpdateCodeAISkill(
                "Communication Skill",
                "This skill helps author and send communications, and track their impact.",
                CommunicationSkillEntityTypeGuid,
                CommunicationSkillGuid );

            AddOrUpdateCodeAISkillTool( CommunicationSkillGuid, "Get System Phone Number", "Gets one system phone number in full, including its unique identifier and its SMS configuration.", "7B4E6C15-9D2A-4F83-A0E1-3C5B8D2F41A6" );
            AddOrUpdateCodeAISkillTool( CommunicationSkillGuid, "Lookup System Phone Numbers", "Looks up system phone numbers.", "FD3F160F-ABCA-4A18-B69F-0E21D61B6874" );

            // Content Channel.
            AddOrUpdateCodeAISkill(
                "Content Channel Skill",
                "This skill provides access to content channel and item details.",
                ContentChannelSkillEntityTypeGuid,
                ContentChannelSkillGuid );

            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "Add Or Update Content Channel Item", "Adds new or updates existing content channel item.", "90023821-BA55-4DE3-99C1-3DA8E8F123BD" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "Get Content Channel", "Gets the details of an existing content channel.", "7C90154A-2752-4850-A825-144BDDABC978" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "Get Content Channel Item", "Gets the details of an existing content channel item.", "3A721EB2-757D-4236-97CF-B7CDF8C11357" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "Get Content Channel Item Available Attributes", "Gets the available attributes that can be set when adding or updating a content channel item.", "DC8E603D-5F97-47CD-B87F-3F128E512BF9" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "List Content Channel Items", "Lists content channel items that match the filters.", "0782BD98-5EC4-4B88-9784-9D122AD3CBB1" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "List Content Channels", "Lists content channels that match the filters.", "780523E8-4AC9-414F-BA0C-0F6A6471F37F" );
            AddOrUpdateCodeAISkillTool( ContentChannelSkillGuid, "Lookup Content Channel Types", "Retrieves all configured content channel types in Rock.", "2A28DE77-675B-4A7A-B38E-6A22D148B2B0" );

            // Reporting.
            AddOrUpdateCodeAISkill(
                "Reporting Skill",
                "Provides access to Rock's reports and data views: listing them, reading their configuration, and running data views and reports to get their results.",
                ReportingSkillEntityTypeGuid,
                ReportingSkillGuid );

            AddOrUpdateCodeAISkillTool( ReportingSkillGuid, "Get Data View", "Gets a single data view in full detail, including a human-readable summary of its filters.", "61E0F59C-9885-4224-8D0D-7E24BD71E3D2" );
            AddOrUpdateCodeAISkillTool( ReportingSkillGuid, "List Data Views", "Lists the data views configured in Rock for a given entity type. A data view is a saved, reusable filter that selects a set of records.", "B7EA3A42-8C2A-42AA-B244-FA14F6551DD6" );

            // Note.
            AddOrUpdateCodeAISkill(
                "Note Skill",
                "This skill provides functionality to manage notes.",
                NoteSkillEntityTypeGuid,
                NoteSkillGuid );

            AddOrUpdateCodeAISkillTool( NoteSkillGuid, "Lookup Note Types", "Provides a list of all note types available for use.", "51046397-D246-4296-A1C0-EC6BF0D01FAA" );

            // Step.
            AddOrUpdateCodeAISkill(
                "Step Skill",
                "This skill provides access to step related data.",
                StepSkillEntityTypeGuid,
                StepSkillGuid );

            AddOrUpdateCodeAISkillTool( StepSkillGuid, "Lookup Step Programs", "Retrieves the step programs and step types for each program.", "70DFBFEE-7231-4762-90B9-916C1C0108BD" );

            // Reminder.
            AddOrUpdateCodeAISkill(
                "Reminder Skill",
                "This skill provides functionality to manage reminders.",
                ReminderSkillEntityTypeGuid,
                ReminderSkillGuid );

            AddOrUpdateCodeAISkillTool( ReminderSkillGuid, "Lookup Reminder Types", "Lists all reminder types that are available.", "2452B308-F805-4DE6-83DE-1E340767A4EF" );

            // Benevolence.
            AddOrUpdateCodeAISkill(
                "Benevolence Skill",
                "This skill provides access to benevolence requests.",
                BenevolenceSkillEntityTypeGuid,
                BenevolenceSkillGuid );

            AddOrUpdateCodeAISkillTool( BenevolenceSkillGuid, "Lookup Benevolence Types", "Retrieves all configured benevolence types in Rock.", "A7A059F6-08A2-4032-A91D-A787D1857752" );

            // Person. The description's apostrophe is a typographic one (U+2019),
            // matching the class text, which is why the helpers use N'' literals.
            AddOrUpdateCodeAISkill(
                "Person Skill",
                "This skill provides a holistic view of a person’s profile, connections, and overall engagement.",
                PersonSkillEntityTypeGuid,
                PersonSkillGuid );

            AddOrUpdateCodeAISkillTool( PersonSkillGuid, "Search Person", "Does a full name sounds like search for the person.", "03093B11-A02D-F794-4A5E-9AEA2C6EF63E" );
        }

        /// <summary>
        /// Removes the Workflow Builder skill's security and the skill itself.
        /// Deleting an AISkill cascades to its AISkillTool and AIAgentSkill rows.
        /// Startup registration recreates the skill on the next run of a build
        /// that still contains the skill class.
        /// </summary>
        private void RemoveWorkflowBuilderSkill_Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "598A03F5-59E3-44C5-B4F4-FCED42618FFE" );
            RockMigrationHelper.DeleteSecurityAuth( "CDFC6AEE-0323-40FD-8824-778E43E0C6AB" );

            Sql( $@"
DELETE FROM [AISkill]
WHERE [Guid] = '{WorkflowBuilderSkillGuid}'" );
        }

        #endregion Skills

        #region Agents

        /// <summary>
        /// Creates the Workflow Composer MCP Agent as an MCP server, if it does not
        /// already exist. An existing row is only marked as a system agent, because
        /// the rows made during development were created with IsSystem = 0.
        /// </summary>
        private void AddWorkflowComposerMcpAgent_Up()
        {
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{WorkflowComposerMcpAgentGuid}')
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
        N'{WorkflowComposerMcpAgentName}'
        , N'An MCP server that lets an AI client build workflows in this instance: the workflow type, its attributes, the activities and actions that drive it, and the entry forms people fill out.'
        , N'{WorkflowComposerInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Mcp}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , 1
        , N'{{ ""McpAgentSettings"": {{ ""Slug"": ""{WorkflowComposerMcpAgentSlug}"", ""IsExcludingSystemSkills"": false }}, ""{SeededInstructionsHashKey}"": ""{GetSha256Hex( WorkflowComposerInstructions )}"" }}'
        , '{WorkflowComposerMcpAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = N'{WorkflowComposerMcpAgentName}'
    WHERE [Guid] = '{WorkflowComposerMcpAgentGuid}'
        AND [Name] = N'Workflow Builder (MCP)'

    UPDATE [AIAgent]
    SET [IsSystem] = 1
    WHERE [Guid] = '{WorkflowComposerMcpAgentGuid}'
        AND [IsSystem] = 0
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                WorkflowComposerMcpAgentGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "29BD4B02-F9D2-4305-9FFA-7F1A8590E660" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                WorkflowComposerMcpAgentGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "4A3F221E-E4B1-4DC1-B7EB-5BDABAD5A8FE" );
        }

        /// <summary>
        /// Creates the Workflow Composer Chat Agent, if it does not already exist.
        /// It runs on the High model role with High reasoning effort, because
        /// building a workflow is long, multi-step work where a weaker model
        /// produces broken configuration. An existing row is only marked as a
        /// system agent, as for the MCP agent.
        /// </summary>
        private void AddWorkflowComposerChatAgent_Up()
        {
            Sql( $@"
IF NOT EXISTS (SELECT [Id] FROM [AIAgent] WHERE [Guid] = '{WorkflowComposerChatAgentGuid}')
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
        N'{WorkflowComposerChatAgentName}'
        , N'A chat agent that builds workflows in this instance from Rock''s own chat: the workflow type, its attributes, the activities and actions that drive it, and the entry forms people fill out.'
        , N'{WorkflowComposerInstructions.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.AgentType.Chat}
        , {( int ) Enums.AI.Agent.AudienceType.Internal}
        , 1
        , N'{{ ""ChatAgentSettings"": {{ ""AutoSummarizeThreshold"": 400000, ""IsExcludingSystemSkills"": false, ""Role"": {( int ) Enums.AI.Agent.ModelServiceRole.High}, ""ReasoningEffort"": {( int ) Enums.AI.Agent.ReasoningEffort.High} }}, ""{SeededInstructionsHashKey}"": ""{GetSha256Hex( WorkflowComposerInstructions )}"" }}'
        , '{WorkflowComposerChatAgentGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AIAgent]
    SET [Name] = N'{WorkflowComposerChatAgentName}'
    WHERE [Guid] = '{WorkflowComposerChatAgentGuid}'
        AND [Name] = N'Workflow Builder (Chat)'

    UPDATE [AIAgent]
    SET [IsSystem] = 1
    WHERE [Guid] = '{WorkflowComposerChatAgentGuid}'
        AND [IsSystem] = 0
END" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                WorkflowComposerChatAgentGuid,
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "4FB792F5-0446-4675-9C9B-1EBEEA04B81F" );

            RockMigrationHelper.AddSecurityAuthForAIAgent(
                WorkflowComposerChatAgentGuid,
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "5F0C05F4-9302-45F7-94CC-9E1DD042D959" );
        }

        /// <summary>
        /// The enabled tools of the Workflow Builder skill, shared by both agents.
        /// </summary>
        private static readonly string[] WorkflowBuilderSkillEnabledTools = new[]
        {
            "AEF3A669-4696-42E0-AC97-FF109CC72FE2", // Add Or Update Workflow Action Form
            "8C1147F0-7A46-4274-9A6D-668ECD052B87", // Add Or Update Workflow Action Type
            "85129AA8-724E-4BAB-BBAB-7DF9253F11DE", // Add Or Update Workflow Activity Type
            "8C371846-1F64-4A47-AB68-416CC584C85E", // Add Or Update Workflow Attribute
            "3E7B1A4C-5D26-4F98-9C03-8B41D5E6720F", // Add Or Update Workflow Form Person Entry
            "DD2120CD-0FD6-45FC-8633-60FFA69B16CC", // Add Or Update Workflow Type
            "31D70F62-B03A-47F7-8086-1C00637847CB", // Delete Workflow Action Type
            "02164475-C6BB-4F8A-822C-BC37FEA77F03", // Delete Workflow Activity Type
            "A8A8FA55-A8D5-4791-991C-691E5D8279C3", // Delete Workflow Attribute
            "6C0A6C0E-9E24-4C1B-B60E-2B2A2A5FA7F1", // Delete Workflow Type
            "B6EB95E5-80EA-4E77-A4DE-89BB327D382F", // Get Workflow Action Type
            "99871B11-0F69-4E0D-BCBA-446317F8B5B6", // Get Workflow Action Type Available Attributes
            "01D84BCE-8B18-4200-9435-3D1F5572BD92", // Get Workflow Type Configuration
            "5B0804A3-F2F4-4F5E-AB3C-E0F618370BE1", // List Workflow Types
            "D319EB2C-F2CE-44F2-80E1-0705C6AC68DF"  // Lookup Workflow Action Components
        };

        /// <summary>
        /// The enabled tools of the Core Administration skill, shared by both agents.
        /// </summary>
        private static readonly string[] CoreAdministrationSkillEnabledTools = new[]
        {
            "9E3E5A2C-6D67-4F3B-B0AC-11A02C43B0E1", // Get Category
            "366B42BD-9D92-4042-8B20-04EE6B0142C7", // Get Defined Type
            "BF14C7EA-98DC-4FFF-8485-F9952B2F4B8B", // Get Defined Value
            "542ED067-19EA-4DEE-B8DA-47FBB47C467D", // Get Defined Value Available Attributes
            "2A0EF1D6-8C10-4E9C-BCDB-0FCA3FEC0998", // Get Entity Available Attributes
            "CD8C8E44-F60C-4F1A-A480-683C600C526E", // Get Field Type
            "1D1D0F7C-6B22-4C4E-9E4B-BC5A0A9F1D74", // Get System Communication
            "8B1EFF0E-AAE0-43BF-A2DA-D1C71EADF28B", // List Categories
            "53DDA7C1-00A5-4531-8A5D-07FBC6721798", // List Defined Types
            "0351DA93-E519-48D6-BB05-21D93A9583CA", // List Defined Values
            "7BD8DF7C-09AA-4809-8364-37D594370E99", // List Entity Types
            "83AFE4C8-F8BC-4BF8-A7D6-6FDCF8AD8561", // List System Communications
            "04F39FBF-A3B4-4F1F-88E7-49E1D3AE73A7"  // Lookup Field Types
        };

        /// <summary>
        /// The enabled tools of the Group skill, shared by both agents.
        /// </summary>
        private static readonly string[] GroupSkillEnabledTools = new[]
        {
            "085141CC-09C1-40D8-831C-FAF4DA96D604", // Add Or Update Group Member
            "C2800552-0BEB-4F54-8F1D-5EEB01DF4192", // Delete Group Member
            "2846FCA1-4B7B-4623-903A-2293C17436A3", // Get Group Type
            "66580B04-14E6-4FA8-8367-EFE8F2A7E7ED", // List Group Members
            "94FDE11F-6243-4F4E-8854-66C9625B9DE1", // List Groups
            "E317AD31-3230-4D16-B039-752C7BEB68C6", // List Scheduled Attendance
            "23C5AD96-68F8-4D3E-B2E9-96E179A08E5A"  // Lookup Group Types
        };

        /// <summary>
        /// The enabled tools of the Connection skill, shared by both agents.
        /// </summary>
        private static readonly string[] ConnectionSkillEnabledTools = new[]
        {
            "8EE3913A-9BCA-4971-A490-90ABFC1690C3", // Add Or Update Connection Request
            "3E00B0EF-9AA8-4E77-8BCC-961A6EA6FD9C", // Get Connection Request
            "C660989A-BA62-42F8-8EED-49C0BF7E8BF6", // Get Connection Request Available Attributes
            "51E14E2D-09A4-440E-9E7D-DF1BF22BD918", // Get Connection Request Insights
            "B3DF0351-AA63-44BF-98FD-16FC56AD2D39", // Get Connection Request Summary
            "DC03271E-2C54-D5AF-4F18-9CCC69F25202", // List Connection Requests
            "21870C06-126F-0882-47E3-DBFC1846BD92"  // Lookup Connection Types And Opportunities
        };

        /// <summary>
        /// The enabled tools of the Communication skill, shared by both agents.
        /// Only the system phone number lookups are enabled, so the agent can
        /// fill in the From number of an SMS action. The agents build workflows
        /// and never draft or send a communication themselves.
        /// </summary>
        private static readonly string[] CommunicationSkillEnabledTools = new[]
        {
            "7B4E6C15-9D2A-4F83-A0E1-3C5B8D2F41A6", // Get System Phone Number
            "FD3F160F-ABCA-4A18-B69F-0E21D61B6874"  // Lookup System Phone Numbers
        };

        /// <summary>
        /// The enabled tools of the Content Channel skill, shared by both agents.
        /// </summary>
        private static readonly string[] ContentChannelSkillEnabledTools = new[]
        {
            "90023821-BA55-4DE3-99C1-3DA8E8F123BD", // Add Or Update Content Channel Item
            "7C90154A-2752-4850-A825-144BDDABC978", // Get Content Channel
            "3A721EB2-757D-4236-97CF-B7CDF8C11357", // Get Content Channel Item
            "DC8E603D-5F97-47CD-B87F-3F128E512BF9", // Get Content Channel Item Available Attributes
            "0782BD98-5EC4-4B88-9784-9D122AD3CBB1", // List Content Channel Items
            "780523E8-4AC9-414F-BA0C-0F6A6471F37F", // List Content Channels
            "2A28DE77-675B-4A7A-B38E-6A22D148B2B0"  // Lookup Content Channel Types
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
        /// The Community Knowledge Base skill configuration both agents use: search
        /// limited to the developer reference and product documentation categories.
        /// </summary>
        private const string CommunityKnowledgeBaseSkillConfigurationValues = @"{ ""categories"": ""developer-reference,product-documentation"" }";

        /// <summary>
        /// The enabled tools of the Cms skill, shared by both agents.
        /// </summary>
        private static readonly string[] CmsSkillEnabledTools = new[]
        {
            "6234BB68-99B8-4B7C-884D-0D760B1F081C", // Lookup Sites
            "C668CAE0-CFA7-4AFF-87FF-5025860170BA", // Search Pages
            "E2CFF69F-C4B2-47F5-B322-4041D841F37C"  // Get Page
        };

        /*
            9/22/2026 - CLAUDE

            The Reporting, Note, Step, Reminder, Benevolence and Person skills are
            attached for their lookups only. Workflow action settings reference
            data views (Person In Data View, Filter Groups By Data View), note
            types (Add Workflow Note, Person Note Add), step types (Step Add),
            reminder types (Reminder Add), benevolence types (Benevolence Request
            Add) and specific people (Assign Activity To Person, Set Attribute
            From Person), and without these tools the agent could only guess the
            values. None of the tools that change or return people's records
            beyond a name search are enabled.

            Reason: Let the agent resolve the records workflow action settings point to.
        */

        /// <summary>
        /// The enabled tools of the Reporting skill, shared by both agents.
        /// </summary>
        private static readonly string[] ReportingSkillEnabledTools = new[]
        {
            "B7EA3A42-8C2A-42AA-B244-FA14F6551DD6", // List Data Views
            "61E0F59C-9885-4224-8D0D-7E24BD71E3D2"  // Get Data View
        };

        /// <summary>
        /// The enabled tools of the Note skill, shared by both agents.
        /// </summary>
        private static readonly string[] NoteSkillEnabledTools = new[]
        {
            "51046397-D246-4296-A1C0-EC6BF0D01FAA"  // Lookup Note Types
        };

        /// <summary>
        /// The enabled tools of the Step skill, shared by both agents.
        /// </summary>
        private static readonly string[] StepSkillEnabledTools = new[]
        {
            "70DFBFEE-7231-4762-90B9-916C1C0108BD"  // Lookup Step Programs
        };

        /// <summary>
        /// The enabled tools of the Reminder skill, shared by both agents.
        /// </summary>
        private static readonly string[] ReminderSkillEnabledTools = new[]
        {
            "2452B308-F805-4DE6-83DE-1E340767A4EF"  // Lookup Reminder Types
        };

        /// <summary>
        /// The enabled tools of the Benevolence skill, shared by both agents.
        /// </summary>
        private static readonly string[] BenevolenceSkillEnabledTools = new[]
        {
            "A7A059F6-08A2-4032-A91D-A787D1857752"  // Lookup Benevolence Types
        };

        /// <summary>
        /// The enabled tools of the Person skill, shared by both agents.
        /// </summary>
        private static readonly string[] PersonSkillEnabledTools = new[]
        {
            "03093B11-A02D-F794-4A5E-9AEA2C6EF63E"  // Search Person
        };

        /// <summary>
        /// Attaches the fourteen skills to both agents with explicit enabled-tool
        /// lists. Attaching a skill alone does not expose its tools.
        /// </summary>
        private void AttachSkillsToAgents_Up()
        {
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, WorkflowBuilderSkillGuid, WorkflowBuilderSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, CoreAdministrationSkillGuid, CoreAdministrationSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, GroupSkillGuid, GroupSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, ConnectionSkillGuid, ConnectionSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, CommunicationSkillGuid, CommunicationSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, ContentChannelSkillGuid, ContentChannelSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools, CommunityKnowledgeBaseSkillConfigurationValues );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, ReportingSkillGuid, ReportingSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, NoteSkillGuid, NoteSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, StepSkillGuid, StepSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, ReminderSkillGuid, ReminderSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, BenevolenceSkillGuid, BenevolenceSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerMcpAgentGuid, PersonSkillGuid, PersonSkillEnabledTools );

            AttachSkillToAgent( WorkflowComposerChatAgentGuid, WorkflowBuilderSkillGuid, WorkflowBuilderSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, CoreAdministrationSkillGuid, CoreAdministrationSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, GroupSkillGuid, GroupSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, ConnectionSkillGuid, ConnectionSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, CommunicationSkillGuid, CommunicationSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, ContentChannelSkillGuid, ContentChannelSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, CommunityKnowledgeBaseSkillGuid, CommunityKnowledgeBaseSkillEnabledTools, CommunityKnowledgeBaseSkillConfigurationValues );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, CmsSkillGuid, CmsSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, ReportingSkillGuid, ReportingSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, NoteSkillGuid, NoteSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, StepSkillGuid, StepSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, ReminderSkillGuid, ReminderSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, BenevolenceSkillGuid, BenevolenceSkillEnabledTools );
            AttachSkillToAgent( WorkflowComposerChatAgentGuid, PersonSkillGuid, PersonSkillEnabledTools );
        }

        /// <summary>
        /// Removes the two Workflow Composer agents and their security rules.
        /// Deleting an AIAgent cascades to its AIAgentSkill rows, so the skill
        /// attachments added by <see cref="AttachSkillsToAgents_Up"/> go with it.
        /// </summary>
        private void RemoveWorkflowComposerAgents_Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "29BD4B02-F9D2-4305-9FFA-7F1A8590E660" );
            RockMigrationHelper.DeleteSecurityAuth( "4A3F221E-E4B1-4DC1-B7EB-5BDABAD5A8FE" );
            RockMigrationHelper.DeleteSecurityAuth( "4FB792F5-0446-4675-9C9B-1EBEEA04B81F" );
            RockMigrationHelper.DeleteSecurityAuth( "5F0C05F4-9302-45F7-94CC-9E1DD042D959" );

            Sql( $@"
DELETE FROM [AIAgent]
WHERE [Guid] IN (
    '{WorkflowComposerMcpAgentGuid}'
    , '{WorkflowComposerChatAgentGuid}'
)" );
        }

        #endregion Agents

        #region Workflow Composer Page

        /// <summary>
        /// Adds the Workflow Composer page under AI Agents, routed at
        /// ai/workflow-composer, with a Chat Bot block bound to the Workflow
        /// Composer Chat Agent, and routes the Code Composer page at
        /// ai/code-composer. The page inherits the administrator-only security
        /// of its parent, which matches the agent's own security.
        /// </summary>
        private void AddWorkflowComposerPage_Up()
        {
            RockMigrationHelper.AddPage( true, AIAgentsPageGuid, FullWidthLayoutGuid, "Workflow Composer (Experimental)", "", WorkflowComposerPageGuid, "ti ti-settings-ai" );

            RockMigrationHelper.AddOrUpdatePageRoute( WorkflowComposerPageGuid, "ai/workflow-composer", WorkflowComposerPageRouteGuid );

            // The Code Composer page shipped without a route; give it the matching one.
            RockMigrationHelper.AddOrUpdatePageRoute( CodeComposerPageGuid, "ai/code-composer", CodeComposerPageRouteGuid );

            RockMigrationHelper.AddBlock( true, WorkflowComposerPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), ChatBotBlockTypeGuid.AsGuid(), "Chat Bot", "Main", @"", @"", 0, WorkflowComposerChatBotBlockGuid );

            RockMigrationHelper.AddBlockAttributeValue( true, WorkflowComposerChatBotBlockGuid, ChatBotDefaultAgentAttributeGuid, WorkflowComposerChatAgentGuid );
        }

        /// <summary>
        /// Removes the Workflow Composer page, its Chat Bot block, and both routes.
        /// </summary>
        private void AddWorkflowComposerPage_Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( WorkflowComposerChatBotBlockGuid, ChatBotDefaultAgentAttributeGuid );
            RockMigrationHelper.DeleteBlock( WorkflowComposerChatBotBlockGuid );
            RockMigrationHelper.DeletePageRoute( CodeComposerPageRouteGuid );
            RockMigrationHelper.DeletePageRoute( WorkflowComposerPageRouteGuid );
            RockMigrationHelper.DeletePage( WorkflowComposerPageGuid );
        }

        #endregion Workflow Composer Page

        #region Helper Methods

        /// <summary>
        /// Computes the lowercase hexadecimal SHA-256 of a string's UTF-8 bytes.
        /// Used to fingerprint the seeded instruction text so a later migration
        /// can tell an untouched agent from a tuned one. Copied from the
        /// AddCodeComposer migration.
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
        /// Links one skill to one agent with an explicit enabled-tool list and
        /// optional skill configuration. Attaching a skill alone does not expose
        /// its tools. Adapted from the AddCodeComposer migration to also carry
        /// configuration values.
        /// </summary>
        /// <param name="agentGuid">The Guid of the AIAgent to attach the skill to.</param>
        /// <param name="skillGuid">The Guid of the AISkill to attach.</param>
        /// <param name="enabledToolGuids">The Guids of the tools to enable.</param>
        /// <param name="configurationValuesJson">The JSON object of skill configuration values, or <c>null</c> for none.</param>
        private void AttachSkillToAgent( string agentGuid, string skillGuid, string[] enabledToolGuids, string configurationValuesJson = null )
        {
            // Produces "guid", "guid". The value is interpolated into the verbatim
            // SQL below as-is, so it must already contain the double quotes.
            var enabledTools = string.Join( ", ", enabledToolGuids.Select( g => $"\"{g}\"" ) );
            var configurationValues = ( configurationValuesJson ?? "{}" ).Replace( "'", "''" );

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
        , '{{ ""AgentSkillSettings"": {{ ""EnabledTools"": [{enabledTools}], ""ConfigurationValues"": {configurationValues} }} }}'
        , NEWID()
    )
END" );
        }

        /// <summary>
        /// Grants VIEW on a skill to Rock administrators and denies it to everyone
        /// else. Copied from the AddCodeComposer migration.
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
        N'{name.Replace( "'", "''" )}'
        , N'{description.Replace( "'", "''" )}'
        , @CodeEntityTypeId
        , '{skillGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkill]
    SET [Name] = N'{name.Replace( "'", "''" )}'
        , [Description] = N'{description.Replace( "'", "''" )}'
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
        N'{name.Replace( "'", "''" )}'
        , N'{description.Replace( "'", "''" )}'
        , {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , @SkillId
        , '{toolGuid}'
    )
END
ELSE
BEGIN
    UPDATE [AISkillTool]
    SET [Name] = N'{name.Replace( "'", "''" )}'
        , [Description] = N'{description.Replace( "'", "''" )}'
        , [ToolType] = {( int ) Enums.AI.Agent.ToolType.ExecuteCode}
        , [AISkillId] = @SkillId
    WHERE [Guid] = '{toolGuid}'
END" );
        }

        #endregion Helper Methods
    }
}
