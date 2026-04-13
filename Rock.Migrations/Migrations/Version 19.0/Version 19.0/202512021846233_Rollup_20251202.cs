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

    using Rock.Model;
    using Rock.Plugin.HotFixes;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20251202 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddSynchronizeListRecipientsStoredProcedureUp();
            JPH_AddFinancialBatchIndex_20251023_Up();
            ME_UpdateFamilyAanalyticsStoredProcedureUp();
            JPH_UpdateSynchronizeListRecipientsStoredProcedureUp_20251117();
            JPH_AddIndexesForCommunicationPrep_20251117();
            KH_UpdateGroupPlacementStoredProcedure();
            UpdateCommunicationPageBlockAttributeForPersonBioBlock();
            NA_ConvertBlockSettingsToNotUseObsidianComponentsUp();
            NA_FixBrokenCampusPickerLavaShortcodeUp();
            DH_FixMissingTablerIconInNextGenThemes();
            JMH_UpdateRegistrationInstanceTimeoutSettings_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddSynchronizeListRecipientsStoredProcedureDown();
            ME_UpdateFamilyAanalyticsStoredProcedureDown();
            JPH_UpdateSynchronizeListRecipientsStoredProcedureDown_20251117();
            NA_ConvertBlockSettingsToNotUseObsidianComponentsDown();
            JMH_UpdateRegistrationInstanceTimeoutSettings_Down();
        }

        #region 262_ImproveCommunicationPrepPerformance

        /// <summary>
        /// JPH - Add spCommunication_SynchronizeListRecipients stored procedure - up.
        /// </summary>
        private void JPH_AddSynchronizeListRecipientsStoredProcedureUp()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( HotFixMigrationResource._262_ImproveCommunicationPrepPerformance_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// JPH - Add spCommunication_SynchronizeListRecipients stored procedure - down.
        /// </summary>
        private void JPH_AddSynchronizeListRecipientsStoredProcedureDown()
        {
            // Delete [spCommunication_SynchronizeListRecipients].
            Sql( "DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );
        }

        #endregion

        #region 265_MigrationRollupsForV18_1_0

        /// <summary>
        /// JPH - Add a new index to the FinancialBatch table - up.
        /// </summary>
        private void JPH_AddFinancialBatchIndex_20251023_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.1 - Add FinancialBatch Index",
                description: "This job will add a new index to the FinancialBatch table.",
                jobType: "Rock.Jobs.PostV181AddFinancialBatchIndex",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_181_ADD_FINANCIALBATCH_INDEX );
        }

        #endregion

        #region 267_UpdateFamilyAnalyticsStoredProcedure

        private void ME_UpdateFamilyAanalyticsStoredProcedureUp()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCrm_FamilyAnalyticsGiving] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCrm_FamilyAnalyticsGiving]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving];" );

            Sql( HotFixMigrationResource._267_UpdateFamilyAnalyticsStoredProcedure_spCrm_FamilyAnalyticsGiving );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        private void ME_UpdateFamilyAanalyticsStoredProcedureDown()
        {
            // Delete [dbo].[spCrm_FamilyAnalyticsGiving]
            Sql( "DROP PROCEDURE [dbo].[spCrm_FamilyAnalyticsGiving];" );
        }

        #endregion

        #region 268_ImproveSyncListRecipientsPerformance

        /// <summary>
        /// JPH - Update spCommunication_SynchronizeListRecipients stored procedure - up.
        /// </summary>
        private void JPH_UpdateSynchronizeListRecipientsStoredProcedureUp_20251117()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( HotFixMigrationResource._268_ImproveSyncListRecipientsPerformance_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// JPH - Update spCommunication_SynchronizeListRecipients stored procedure - down.
        /// </summary>
        private void JPH_UpdateSynchronizeListRecipientsStoredProcedureDown_20251117()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spCommunication_SynchronizeListRecipients] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCommunication_SynchronizeListRecipients]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCommunication_SynchronizeListRecipients];" );

            Sql( HotFixMigrationResource._262_ImproveCommunicationPrepPerformance_spCommunication_SynchronizeListRecipients );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        /// <summary>
        /// JPH - Add indexes for communication prep - up.
        /// </summary>
        private void JPH_AddIndexesForCommunicationPrep_20251117()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.1 - Add Indexes For Communication Prep",
                description: "This job will add indexes to improve communication prep performance.",
                jobType: "Rock.Jobs.PostV181AddIndexesForCommunicationPrep",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_181_ADD_INDEXES_FOR_COMMUNICATION_PREP );
        }

        #endregion

        #region 269_UpdateGoupPlacementStoredProcedure_spGetGroupPlacementPeople

        private void KH_UpdateGroupPlacementStoredProcedure()
        {
            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spGetGroupPlacementPeople] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGetGroupPlacementPeople]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGetGroupPlacementPeople];" );

            Sql( HotFixMigrationResource._269_UpdateGroupPlacementStoredProcedure_spGetGroupPlacementPeople );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );
        }

        #endregion

        #region 270_MigrationRollupsForV18_1_1

        #region KH: Update Communication Page Block Attribute for Person Bio Block

        private void UpdateCommunicationPageBlockAttributeForPersonBioBlock()
        {
            // Update the Communication Page Attribute Value if it is set to the old "Simple Communication" page.
            Sql( @"
UPDATE [dbo].[AttributeValue]
SET [Value] = '9F7AE226-CC95-4E6A-B333-C0294A2024BC,01A3891B-9998-7E30-20DC-58081A239D65',
    [IsPersistedValueDirty] = 1
WHERE [Value] = '7e8408b2-354c-4a5a-8707-36754ae80b9a'
    AND [AttributeId] = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '66CFDF24-8D19-4885-8C09-31DBE8C4126D' )
" );
        }

        #endregion

        #region [Beta v18.0.14] NA: v18.1 Migration to Set Data View Controls back to Webforms for DataViewDetail, ReportDetail, and DynamicReport block

        /// <summary>
        /// Change any existing UseObsidianComponents block setting value for the 
        /// Data View Detail, Report Detail and Dynamic Report blocks to False.
        /// </summary>
        public void NA_ConvertBlockSettingsToNotUseObsidianComponentsUp()
        {
            Sql( @"
UPDATE av
SET
    av.[Value] = 'False',
    av.[PersistedTextValue] = 'No',
    av.[PersistedCondensedTextValue] = 'N',
    av.[PersistedCondensedHtmlValue] = 'N',
    av.[IsPersistedValueDirty] = 1,
    av.ValueAsBoolean = 0
FROM dbo.[AttributeValue] AS av
INNER JOIN dbo.[Attribute] AS a
    ON a.[Id] = av.[AttributeId]
INNER JOIN dbo.[BlockType] AS bt
    ON bt.[Id] = TRY_CAST(a.[EntityTypeQualifierValue] AS INT)
WHERE
    a.[Key] = 'UseObsidianComponents'
    AND a.[EntityTypeId] = 9
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND bt.[Guid] IN (
        'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
        'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
        'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB'  -- Dynamic Report
    )
    AND av.[Value] = 'True';
" );
        }

        /// <summary>
        /// Change any existing UseObsidianComponents block setting value for the 
        /// Data View Detail, Report Detail and Dynamic Report blocks back to True.
        /// </summary>
        public void NA_ConvertBlockSettingsToNotUseObsidianComponentsDown()
        {
            Sql( @"
UPDATE av
SET
    av.[Value] = 'True',
    av.[PersistedTextValue] = 'Yes',
    av.[PersistedCondensedTextValue] = 'Y',
    av.[PersistedCondensedHtmlValue] = 'Y',
    av.[IsPersistedValueDirty] = 1,
    av.ValueAsBoolean = 1
FROM dbo.[AttributeValue] AS av
INNER JOIN dbo.[Attribute] AS a
    ON a.[Id] = av.[AttributeId]
INNER JOIN dbo.[BlockType] AS bt
    ON bt.[Id] = TRY_CAST(a.[EntityTypeQualifierValue] AS INT)
WHERE
    a.[Key] = 'UseObsidianComponents'
    AND a.[EntityTypeId] = 9
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND bt.[Guid] IN (
        'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
        'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
        'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB'  -- Dynamic Report
    )
    AND av.[Value] = 'False';
" );
        }

        #endregion

        #region NA: Fix broken CampusPicker Lava Shortcode

        private const string HelixCategoryGuid = "5874AE45-B5EE-4D10-A274-26B5D69E6283";

        #region Campus Picker

        private const string CampusPickerTagName = "campuspicker";
        private const string CampusPickerGuid = "E787B188-2E0F-479E-A855-0E4ABA75C91B";
        private const string CampusPickerMarkup = @"//- Prep configuration settings
{% assign sc-statusList = campusstatuses | Split:',' %}
{% assign sc-typeList = campustypes | Split:',',true %}
{% assign sc-selectableList = selectablecampuses | Split:',' %}
{% assign sc-currentValues = value | Split:',' %} //- Note we're supporting the possibility that there could be multiple values.
{% assign includeinactive = includeinactive | AsBoolean %}
{% assign allowmultiple = allowmultiple | AsBoolean %}

//- Get source data
{% assign sc-campuses = 'All' | FromCache:'Campus'  %}

//- Filtering works by selecting all campuses that should not be shown
//- and then removing them from the list. This means that configuration
//- filters are AND not OR (which matches the C# logic).

//- Filter by type
{% if sc-typeList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusTypeId = campus.CampusTypeValueId | ToString %}
        {% assign isConfiguredType = sc-typeList | Contains:campusTypeId %}
        {% if isConfiguredType == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by status
{% if sc-statusList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusStatusId = campus.CampusStatusValueId | ToString %}
        {% assign isConfiguredStatus = sc-statusList | Contains:campusStatusId %}
        {% if isConfiguredStatus == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by selected
{% if sc-selectableList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusId = campus.Id | ToString %}
        {% assign isSelected = sc-selectableList | Contains:campusId %}
        {% if isSelected == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Remove inactive campuses
{% if includeinactive == false %}
    {% assign sc-campuses = sc-campuses | Where:'IsActive',true %}
{% endif %}

//- Ensure current values are still in the list, the value can be either a campus id or guid
{% assign allCampuses = 'All' | FromCache:'Campus'  %}
{% for currentValue in sc-currentValues %}
    {% for campus in allCampuses %}
        {% assign campusId = campus.Id | ToString %}
        {% assign campusGuid = campus.Guid | ToString %}
        {% if campusId == currentValue or campusGuid == currentValue %}
            //- Ensure the campus list has this campus, if not add it
            {% assign isInCampusList = sc-campuses | Contains:campus %}

            {% if isInCampusList == false %}
                {% assign sc-campuses = sc-campuses | AddToArray:campus %}
            {% endif %}
        {% endif %}
    {% endfor %}
{% endfor %}

//- Sort Campuses
{% assign sc-campuses = sc-campuses | OrderBy:'Order' %}

//- Control formatting
{% if allowmultiple %}
    {[ checkboxlist label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' isrequired:'{{ isrequired }}' value:'{{ value }}' columns:'4' controltype:'campus-picker' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ endcheckboxlist ]}
{% else %}
    {[ dropdown label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' longlistenabled:'{{ longlistenabled }}' value:'{{ value }}' controltype:'campus-picker' isrequired:'{{ isrequired }}' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ enddropdown ]}
{% endif %}";
        private const string CampusPickerParameters = @"value^|includeinactive^false|campustypes^|campusstatuses^|selectablecampuses^|name^campus|isrequired^false|label^Campus|validationmessage^Please select a campus.|longlistenabled^false|valuefield^id|allowmultiple^false|additionalattributes^|showlabel^true";
        private const string CampusPickerDocumentation = @"<p>This control allows you to select a campus.</p>

<h5>Example Usage</h5>
<pre>{[ campuspicker label:'Primary Campus' value:'1,2' allowmultiple:'true' campustypes:'768' campusstatuses:'765' selectablecampuses:'1,2,5' ]}</pre>

<h5>Parameters</h5>
<p>Below are the parameters for the campus picker shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (campus) - The name for the campus picker control.</li>
    <li><strong>value</strong> - The ID or Guid of the currently selected campus(es).</li>
    <li><strong>valuefield</strong> (id) - Specifies whether the picker's value should correspond to the campus' <code>id</code> or <code>guid</code>.</li>
    <li><strong>includeinactive</strong> (false) - Determines if inactive campuses should be displayed.</li>
    <li><strong>campustypes</strong> - Filters the campus list by type (comma separated list of defined value ids).</li>
    <li><strong>campusstatuses</strong> - Filters the campus list by status (comma separated list of defined value ids).</li>
    <li><strong>selectablecampuses</strong> -  List of specific campuses to display (comma separated list of campus ids).</li>
    <li><strong>longlistenabled</strong> (false) -  Enhances the functionality to include a search feature, facilitating swift and efficient selection of the preferred item from the list.</li>
    <li><strong>allowmultiple</strong> (false) -  Determines if the selection of multiple values is allowed.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please provide a campus.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>

<p>
    The above settings enable a wide range of filtering options for the list. Regardless of the filter configurations, the
    current value will consistently be shown.
</p>";
        private const string CampusPickerDescription = "Displays a campus picker.";
        private const int CampusPickerTagType = ( int ) TagType.Inline;

        #endregion

        /// <summary>
        /// Change the typo in the CampusPicker Lava Shortcode to fix issue #6574.
        /// </summary>
        public void NA_FixBrokenCampusPickerLavaShortcodeUp()
        {
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Campus Picker", CampusPickerTagName, CampusPickerDescription, CampusPickerDocumentation, CampusPickerMarkup, CampusPickerParameters, CampusPickerTagType, HelixCategoryGuid, CampusPickerGuid );
        }

        #endregion

        #region DH: Fix missing Tabler icon in next-gen themes.

        private void DH_FixMissingTablerIconInNextGenThemes()
        {
            Sql( @"UPDATE [Theme]
SET [AdditionalSettingsJson] = JSON_MODIFY([AdditionalSettingsJson], '$.ThemeCustomizationSettings.EnabledIconSets', 2 | JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets'))
WHERE ISJSON([AdditionalSettingsJson]) = 1
  AND JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets') IS NOT NULL
  AND JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets') & 2 = 0" );
        }

        #endregion

        #endregion

        #region JMH: Update Registration Instance Timeout Threshold and Length  

        /// <summary>
        /// JMH - Updates the TimeoutThreshold and TimeoutIsEnabled columns of the RegistrationInstance table - up.
        /// </summary>
        /// <remarks>
        /// - TimeoutThreshold is now a percent of remaining attendees instead of the number of remaining attendees.
        /// - TimeoutIsEnabled is now determined by the existence of MaxAttendees instead of the existence of TimeoutLengthMinutes.
        /// </remarks>
        private void JMH_UpdateRegistrationInstanceTimeoutSettings_Up()
        {
            Sql( @"
UPDATE [dbo].[RegistrationInstance]
    /* 
        TimeoutThreshold should either be:
        - NULL (the default threshold will be used)
        - or converted to a percent of slots remaining
    */
   SET [TimeoutThreshold] = 
           CASE WHEN [TimeoutThreshold] IS NULL THEN NULL
           ELSE CEILING(100.0 * [TimeoutThreshold] / [MaxAttendees]) END
    /*
        TimeoutIsEnabled is now determined by the existence of MaxAttendees
        so set it to true since the WHERE clause already filters by those records.
    */
       , [TimeoutIsEnabled] = 1
 WHERE [MaxAttendees] IS NOT NULL 
       AND [MaxAttendees] > 0" );
        }

        /// <summary>
        /// JMH - Updates the TimeoutThreshold and TimeoutIsEnabled columns of the RegistrationInstance table - down.
        /// </summary>
        /// <remarks>
        /// - TimeoutThreshold is reverted from a percent of remaining attendees to the number of remaining attendees.
        /// - TimeoutIsEnabled is reverted from the existence of MaxAttendees to the existence of TimeoutLengthMinutes.
        /// </remarks>
        private void JMH_UpdateRegistrationInstanceTimeoutSettings_Down()
        {
            Sql( @"
UPDATE [dbo].[RegistrationInstance]
    /*
        TimeoutThreshold should be converted back to the number of slots remaining
        or left as NULL (no special meaning before).
    */
   SET [TimeoutThreshold] = 
        CASE WHEN [TimeoutThreshold] IS NULL THEN NULL
        ElSE FLOOR([TimeoutThreshold] / 100.0 * [MaxAttendees]) END
    /*
        TimeoutIsEnabled used to be determined by the existence of TimeoutLengthMinutes
        so use that for the rollback logic.
    */
    , [TimeoutIsEnabled] = 
        CASE WHEN [TimeoutLengthMinutes] IS NULL THEN 0
        ELSE 1 END
 WHERE [MaxAttendees] IS NOT NULL 
       AND [MaxAttendees] > 0" );
        }

        #endregion
    }
}
