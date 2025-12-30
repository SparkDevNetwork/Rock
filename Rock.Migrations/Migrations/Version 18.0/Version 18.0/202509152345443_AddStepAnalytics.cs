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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddStepAnalytics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.StepProgram", "CompletionFlow", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Step", "RelatedEntityTypeId", c => c.Int() );
            AddColumn( "dbo.Step", "RelatedEntityId", c => c.Int() );
            AddColumn( "dbo.StepType", "EngagementType", c => c.Int() );
            AddColumn( "dbo.StepType", "ImpactWeight", c => c.Int() );
            AddColumn( "dbo.StepType", "OrganizationalObjectiveValueId", c => c.Int() );
            AddColumn( "dbo.StepType", "CallToActionLabel", c => c.String() );
            AddColumn( "dbo.StepType", "CallToActionLink", c => c.String() );
            AddColumn( "dbo.StepType", "CallToActionDescription", c => c.String() );

            AddForeignKey( "dbo.StepType", "OrganizationalObjectiveValueId", "dbo.DefinedValue", "Id" );

            UpdateDefinedTypeUp();
            UpdateStepFlowStoredProcedure();
            UpdateKPIDefaultAttributeValues();
            RemoveStepFlowBlockAndPageUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.StepProgram", "CompletionFlow" );
            DropColumn( "dbo.Step", "RelatedEntityId" );
            DropColumn( "dbo.Step", "RelatedEntityTypeId" );

            DropForeignKey( "dbo.StepType", "OrganizationalObjectiveValueId", "dbo.DefinedValue" );

            DropColumn( "dbo.StepType", "CallToActionDescription" );
            DropColumn( "dbo.StepType", "CallToActionLink" );
            DropColumn( "dbo.StepType", "CallToActionLabel" );
            DropColumn( "dbo.StepType", "OrganizationalObjectiveValueId" );
            DropColumn( "dbo.StepType", "ImpactWeight" );
            DropColumn( "dbo.StepType", "EngagementType" );

            UpdateDefinedTypeDown();
            RemoveStepFlowBlockAndPageDown();
        }

        #region ME: Add organizational objective defined type and values

        /// <summary>
        /// Add organizational objective defined type and values - up.
        /// </summary>
        private void UpdateDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType(
                category: "Global",
                name: "Organizational Objective",
                description: "This list defines the core objectives that activities within the organization are aligned to. While you can rename these objectives to better fit your context, we highly recommend preserving their original intent. Reporting and analytics are based on the intended purpose of each objective, and changing that intent may lead to misleading results.",
                guid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Outreach",
                description: "Connecting with those outside the church to share the love and message of Christ.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_OUTREACH,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Discipleship",
                description: "Guiding individuals to grow in their faith and deepen their relationship with Jesus.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_DISCIPLESHIP,
                isSystem: true
            );

            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE,
                value: "Activation",
                description: "Empowering individuals to serve others and actively participate in God's mission.",
                guid: Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_ACTIVATION,
                isSystem: true
            );
        }

        /// <summary>
        /// Add organizational objective defined type and values - down.
        /// </summary>
        private void UpdateDefinedTypeDown()
        {
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_OUTREACH );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_DISCIPLESHIP );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ORGANIZATIONAL_OBJECTIVE_TYPE_ACTIVATION );

            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.ORGANIZATIONAL_OBJECTIVE_TYPE );
        }

        #endregion

        #region KH: Update Step Flow Stored Procedure

        private void UpdateStepFlowStoredProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spSteps_StepFlow] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spSteps_StepFlow]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spSteps_StepFlow];" );

            Sql( RockMigrationSQL._202509152345443_AddStepAnalytics_spSteps_StepFlow );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        #endregion

        #region KH: Update KPI Default Attribute Values

        private void UpdateKPIDefaultAttributeValues()
        {
            var stepProgramDetailKPILava = @"{[kpis style:''card'' iconbackground:''true'']}
  [[ kpi icon:''ti ti-user'' value:''{{IndividualsCompleting | Format:''N0''}}'' label:''Individuals Completing Program'' color:''blue-700'']][[ endkpi ]]
  [[ kpi icon:''ti ti-calendar'' value:''{{AvgDaysToComplete | Format:''N0''}}'' label:''Average Days to Complete Program'' color:''gray-700'']][[ endkpi ]]
  [[ kpi icon:''ti ti-stairs'' value:''{{StepsStarted | Format:''N0''}}'' label:''Steps Started'' color:''orange-700'']][[ endkpi ]]
  [[ kpi icon:''ti ti-circle-check'' value:''{{StepsCompleted | Format:''N0''}}'' label:''Steps Completed'' color:''green-700'']][[ endkpi ]]
{[endkpis]}";

            Sql( $@"UPDATE [Attribute]
SET [DefaultValue] = '{stepProgramDetailKPILava}'
    , [DefaultPersistedTextValue] = NULL
    , [DefaultPersistedHtmlValue] = NULL
    , [DefaultPersistedCondensedTextValue] = NULL
    , [DefaultPersistedCondensedHtmlValue] = NULL
    , [IsDefaultPersistedValueDirty] = 1
WHERE [Guid] = '824D334A-944E-4315-A0E9-35BB20EE40D2'" );

            var stepTypeDetailKPILava = @"{[kpis style:''card'' iconbackground:''true'' columncount:''4'']}
    [[ kpi icon:''ti-user'' value:''{{IndividualsCompleting | Format:''N0''}}'' label:''Individuals Completing'' color:''blue-700'']][[ endkpi ]]
    {% if StepType.HasEndDate %}
        [[ kpi icon:''ti-calendar'' value:''{{AvgDaysToComplete | Format:''N0''}}'' label:''Average Days to Complete'' color:''gray-700'']][[ endkpi ]]
        [[ kpi icon:''ti-stairs'' value:''{{StepsStarted | Format:''N0''}}'' label:''Steps Started'' color:''orange-700'']][[ endkpi ]]
    {% endif %}
    [[ kpi icon:''ti-circle-check'' value:''{{StepsCompleted | Format:''N0''}}'' label:''Steps Completed'' color:''green-700'']][[ endkpi ]]
{[endkpis]}";

            Sql( $@"UPDATE [Attribute]
SET [DefaultValue] = '{stepTypeDetailKPILava}'
    , [DefaultPersistedTextValue] = NULL
    , [DefaultPersistedHtmlValue] = NULL
    , [DefaultPersistedCondensedTextValue] = NULL
    , [DefaultPersistedCondensedHtmlValue] = NULL
    , [IsDefaultPersistedValueDirty] = 1
WHERE [Guid] = '3455C62B-C3E7-4902-A75C-A8CF62B78F2C'" );
        }

        #endregion

        #region KH: Remove Step Flow Block and Page

        private void RemoveStepFlowBlockAndPageUp()
        {
            RockMigrationHelper.DeleteBlock( "A40684E9-10DA-4CF8-815B-EBDE53624419" );
            RockMigrationHelper.DeletePageRoute( "4F75872B-EBE0-43FA-A8F3-ED716B45A1A6" );
            RockMigrationHelper.DeletePage( "A5FE5D33-C9E2-496D-AD8F-5B7AA496B2AC" );
        }

        private void RemoveStepFlowBlockAndPageDown()
        {
            // Add Page - Internal Name: Step Flow - Site: Rock RMS
            RockMigrationHelper.AddPage( true, SystemGuid.Page.STEP_PROGRAM_DETAIL, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Step Flow", "", SystemGuid.Page.STEP_FLOW, "fa-project-diagram" );
            // Type or member is obsolete
            // Add Page Route - Page:Step Flow - Route: steps/program/{ProgramId}/flow
            RockMigrationHelper.AddOrUpdatePageRoute( SystemGuid.Page.STEP_FLOW, "steps/program/{ProgramId}/flow", SystemGuid.PageRoute.STEP_FLOW );
            // Type or member is obsolete
            // Add Block - Block Name: Step Flow - Page Name: Step Flow Layout: - Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.STEP_FLOW.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "2B4E0128-BCDF-48BF-AEC9-85001169DA3E".AsGuid(), "Step Flow", "Main", @"", @"", 0, "A40684E9-10DA-4CF8-815B-EBDE53624419" );
        }

        #endregion
    }
}