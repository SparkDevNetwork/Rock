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
    using System.Collections.Generic;
    using Rock.Model;

    /// <summary>
    /// Class UpdateBackgroundCheckWorkflowActions.
    /// </summary>
    public partial class UpdateBackgroundCheckWorkflowMissingAttributeActions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateBackgroundCheckWorkflowActions();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void UpdateBackgroundCheckWorkflowActions()
        {
            const string CHECKR_NAME_KEY = "Background Check (Checkr)";
            const string PMM_NAME_KEY = "Background Check";

            var backgroundCheck_WorkflowActivityTypeGuids = new Dictionary<string, string>();
            var bgCheckInitalRequestGuidsSql = @"SELECT STUFF(
                                            (SELECT ',' + CONVERT(nvarchar(50),wfat.[guid])+'^'+wft.[Name]
                                             FROM [WorkflowActivityType] wfat
                                             JOIN [WorkflowType] wft ON wft.[Id] = wfat.[WorkflowTypeId]
                                             WHERE wft.[Name] IN ('Background Check (Checkr)','Background Check') AND wfat.[Name] = 'Initial Request'
                                             FOR
                                              XML PATH('')
                                            ), 1, 1, '') AS [Guids]";

            var bgCheckInitalRequestGuidsString = SqlScalar( bgCheckInitalRequestGuidsSql ).ToStringSafe();

            if ( bgCheckInitalRequestGuidsString.IsNullOrWhiteSpace() )
            {
                // Nothing to do.
                return;
            }

            var arr = bgCheckInitalRequestGuidsString.Split( ',' );
            if ( arr != null && arr.Length > 0 )
            {
                foreach ( var arrVal in arr )
                {
                    if ( !arrVal.Contains( "^" ) )
                    {
                        continue;
                    }

                    var data = arrVal.Split( '^' );
                    if ( data != null && data.Length == 2 )
                    {
                        var guid = data[0];
                        var bgcheckType = data[1];
                        if ( !backgroundCheck_WorkflowActivityTypeGuids.ContainsKey( guid ) )
                        {
                            backgroundCheck_WorkflowActivityTypeGuids.Add( guid, bgcheckType );
                        }
                    }
                }
            }

            if ( backgroundCheck_WorkflowActivityTypeGuids.Count == 0 )
            {
                // Nothing to do.
                return;
            }

            foreach ( var activityTypeKvp in backgroundCheck_WorkflowActivityTypeGuids )
            {
                var bgCheck_ActivityTypeIdSql = $"SELECT TOP 1 [Id] FROM WorkflowActivityType WHERE [Guid] = '{activityTypeKvp.Key}'";
                var bgCheck_ActivityTypeId = SqlScalar( bgCheck_ActivityTypeIdSql ).ToIntSafe();

                if ( bgCheck_ActivityTypeId > 0 )
                {
                    // PersonEmailValid - Attribute - Guid
                    var personEmailValid_AttributeGuid = Guid.NewGuid().ToString().ToUpper();

                    // Add Activity [PersonEmailValid] Entity Attribute
                    RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.WorkflowActivity",
                        "9C204CD0-1233-41C5-818A-C5DA439445AA", "ActivityTypeId",
                        bgCheck_ActivityTypeId.ToString(),
                        "PersonEmailValid",
                        "PersonEmailValid",
                        "Used to indicate if a person has a valid and active email before executing a background check.", 0, "No",
                        personEmailValid_AttributeGuid, "PersonEmailValid" );

                    // Reorder Workflow Activity - Action entries
                    if ( activityTypeKvp.Value == PMM_NAME_KEY )
                    {
                        // Set Person is already order 0

                        // Set Status
                        Sql( $"Update [WorkflowActionType] SET [Order] = 4 WHERE [Guid] = '10E8C6CB-A72C-4254-A1F5-E43B6C7B404B' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        // Set Requester 
                        Sql( $"Update [WorkflowActionType] SET [Order] = 5 WHERE [Guid] = '3136A135-4836-4C09-BD81-326CA21C6AA5' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        // Set Name
                        Sql( $"Update [WorkflowActionType] SET [Order] = 6 WHERE [Guid] = '6A779AB3-3223-411B-9AEE-87A5EE1EDF12' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        // Set Warning
                        Sql( $"Update [WorkflowActionType] SET [Order] = 7 WHERE [Guid] = '81D1FB3E-5017-4A53-A4EF-F6618F782935' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        // Get Details
                        Sql( $"Update [WorkflowActionType] SET [Order] = 8 WHERE [Guid] = 'A3EAF2A3-97FB-47A6-9844-F7F0755FC5BE' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );
                    }
                    else if ( activityTypeKvp.Value == CHECKR_NAME_KEY )
                    {
                        // Set Person 
                        Sql( $"Update [WorkflowActionType] SET [Order] = 0 WHERE [Guid] = '407C6B9C-2E89-400F-8345-A062B1083F66' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        //// Set Status
                        Sql( $"Update [WorkflowActionType] SET [Order] = 4 WHERE [Guid] = '5556D7BA-4EFF-400A-A7A9-E8EFE4519985' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        //// Set Requester 
                        Sql( $"Update [WorkflowActionType] SET [Order] = 5 WHERE [Guid] = '46BF8484-6386-4A45-A332-A107185E7353' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        //// Set Name
                        Sql( $"Update [WorkflowActionType] SET [Order] = 6 WHERE [Guid] = '8C59CA7A-4FF1-410F-9CBC-81FA7BDEE610' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        //// Set Warning
                        Sql( $"Update [WorkflowActionType] SET [Order] = 7 WHERE [Guid] = '2EE62F7C-EABE-4C9C-B00B-045984C462E9' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );

                        //// Get Details
                        Sql( $"Update [WorkflowActionType] SET [Order] = 8 WHERE [Guid] = '9751CA4C-A648-4567-B31E-4B077DDFF124' AND ActivityTypeId = {bgCheck_ActivityTypeId}" );
                    }

                    // Add/Inject new Workflow Activity - Action entries

                    // Add [Set PersonEmailValid Attribute] Action - Order 1

                    var setPersonEmailValidAttributeActionTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [WorkflowActionType] WHERE [Name] = 'Set PersonEmailValid Attribute' AND ActivityTypeId = {bgCheck_ActivityTypeId}" )
                        .ToStringOrDefault( Guid.NewGuid().ToString() ).ToUpper();

                    var runLavaEntityTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.RunLava'" )
                        .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( runLavaEntityTypeGuid.IsNotNullOrWhiteSpace() )
                    {
                        RockMigrationHelper.UpdateWorkflowActionType( activityTypeKvp.Key, "Set PersonEmailValid Attribute", 1,
                            runLavaEntityTypeGuid, true, false, "",
                            "", ComparisonType.EqualTo.ConvertToInt(), "",
                            setPersonEmailValidAttributeActionTypeGuid );
                    }

                    // Add [Show Email Not Found Message] Action - Order 2
                    var showEmailNotFoundMessageActionTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [WorkflowActionType] WHERE [Name] = 'Show Email Not Found Message' AND ActivityTypeId = {bgCheck_ActivityTypeId}" )
                        .ToStringOrDefault( Guid.NewGuid().ToString() ).ToUpper();

                    var showHtmlEntityTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.ShowHtml'" )
                        .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( showHtmlEntityTypeGuid.IsNotNullOrWhiteSpace() )
                    {
                        RockMigrationHelper.UpdateWorkflowActionType( activityTypeKvp.Key, "Show Email Not Found Message", 2,
                            showHtmlEntityTypeGuid, true, false, "",
                            personEmailValid_AttributeGuid, ComparisonType.EqualTo.ConvertToInt(), "No",
                            showEmailNotFoundMessageActionTypeGuid );
                    }

                    // Add [Set Email Not Found Status] Action - Order 3
                    var setEmailNotFoundStatusActionTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [WorkflowActionType] WHERE [Name] = 'Set Email Not Found Status' AND ActivityTypeId = {bgCheck_ActivityTypeId}" )
                        .ToStringOrDefault( Guid.NewGuid().ToString() ).ToUpper();

                    var completeWorkflowEntityTypeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CompleteWorkflow'" )
                       .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( completeWorkflowEntityTypeGuid.IsNotNullOrWhiteSpace() )
                    {
                        RockMigrationHelper.UpdateWorkflowActionType( activityTypeKvp.Key, "Set Email Not Found Status", 3,
                        completeWorkflowEntityTypeGuid, true, true, "",
                        personEmailValid_AttributeGuid, ComparisonType.EqualTo.ConvertToInt(), "No",
                        setEmailNotFoundStatusActionTypeGuid );
                    }

                    #region Add [Set PersonEmailValid Attribute] Value
                    // Add [Set PersonEmailValid Attribute] Value
                    const string setPersonEmailValidLava = @"{% assign person = Workflow | Attribute:'Person','Object' %}
{% assign emailLength = person.Email | Size %}
{% assign emailActive = person.IsEmailActive | AsBoolean %}
{% if emailLength > 0 and emailActive == true %}
Yes
{% else %}
No
{% endif %}";
                    #endregion Add [Set PersonEmailValid Attribute] Lava Value

                    // Set PersonEmailValid Attribute - Action - Lava
                    var codeEditorFieldTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.CodeEditorFieldType'" )
                      .ToIntSafe( 0 );

                    var personEntityTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person'" )
                       .ToIntSafe( 0 );

                    if ( codeEditorFieldTypeId > 0 && personEntityTypeId > 0 )
                    {
                        var lavaAttributeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [Attribute] WHERE [Name] = 'Lava' AND [FieldTypeId] = {codeEditorFieldTypeId} AND [EntityTypeId] = {personEntityTypeId}" )
                          .ToStringOrDefault( string.Empty ).ToUpper();

                        if ( lavaAttributeGuid.IsNotNullOrWhiteSpace() )
                        {
                            RockMigrationHelper.AddActionTypeAttributeValue( setPersonEmailValidAttributeActionTypeGuid, lavaAttributeGuid,
                                setPersonEmailValidLava );
                        }
                    }

                    // Set PersonEmailValid Attribute - Action - Select Attribute
                    var workflowAttributeFieldTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.WorkflowAttributeFieldType'" )
                                          .ToIntSafe( 0 );
                    var workflowActionEntityTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActionType'" )
                       .ToIntSafe( 0 );

                    if ( workflowAttributeFieldTypeId > 0 && workflowActionEntityTypeId > 0 )
                    {
                        var attributeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [Attribute] WHERE [Name] = 'Attribute' AND [FieldTypeId] = {workflowAttributeFieldTypeId} AND [EntityTypeId] = {workflowActionEntityTypeId}" )
                          .ToStringOrDefault( string.Empty ).ToUpper();

                        if ( attributeGuid.IsNotNullOrWhiteSpace() )
                        {
                            RockMigrationHelper.AddActionTypeAttributeValue( setPersonEmailValidAttributeActionTypeGuid, attributeGuid, personEmailValid_AttributeGuid );
                        }
                    }

                    #region [Show Email Not Found Message] Lava Value
                    // [Show Email Not Found Message] Value
                    const string personEmailNotFoundMessage = @"{% assign person = Workflow | Attribute:'Person','Object' %}
{% assign personProfilePage = '~/Person/' %}

<div class='alert alert-danger margin-t-lg js-workflow-entry-message-notification-box'>
    <strong> Error: Email Required</strong>
    <span class='js-notification-text'>
        <ul>
            <li><strong><a href='{{ personProfilePage | ResolveRockUrl }}{{ person.Id }}' target='_blank'>{{ Workflow | Attribute:'Person','FullName' }}</a></strong> has an empty or inactive email address. A background check can not be run unless a valid email address is provided.</li>
                    <ul>
                        <li>Enter a valid email address and/or set it to active.</li>
                    </ul>
        </ul>
    </span>
</div>";
                    #endregion [Show Email Not Found Message] Value
                    // Set Email Not Found Message - Action - HTML\Lava
                    var htmlAttributeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [Attribute] WHERE [Name] = 'HTML' AND [FieldTypeId] = {codeEditorFieldTypeId} AND [EntityTypeId] = {workflowActionEntityTypeId}" )
                      .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( codeEditorFieldTypeId > 0 && workflowActionEntityTypeId > 0 )
                    {
                        if ( htmlAttributeGuid.IsNotNullOrWhiteSpace() )
                        {
                            RockMigrationHelper.AddActionTypeAttributeValue( showEmailNotFoundMessageActionTypeGuid, htmlAttributeGuid, personEmailNotFoundMessage );
                        }
                    }

                    // Set Email Not Found Message - Action - Hide Status Message
                    var boolAttributeFieldTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.BooleanFieldType'" ).ToIntSafe( 0 );
                    var hideStatusMessageAttributeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [Attribute] WHERE [Name] = 'Hide Status Message' AND [FieldTypeId] = {boolAttributeFieldTypeId} AND [EntityTypeId] = {workflowActionEntityTypeId}" )
                      .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( boolAttributeFieldTypeId > 0 && workflowActionEntityTypeId > 0 )
                    {
                        if ( hideStatusMessageAttributeGuid.IsNotNullOrWhiteSpace() )
                        {
                            RockMigrationHelper.AddActionTypeAttributeValue( showEmailNotFoundMessageActionTypeGuid, hideStatusMessageAttributeGuid, "True" );
                        }
                    }

                    // Set Email Not Found Status - Action
                    var workflowTextAttributeFieldTypeId = SqlScalar( $"SELECT TOP 1 [ID] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.WorkflowTextOrAttributeFieldType'" ).ToIntSafe( 0 );
                    var statusAttributeGuid = SqlScalar( $"SELECT TOP 1 [Guid] FROM [Attribute] WHERE [Name] = 'Status|Status Attribute' AND [FieldTypeId] = {workflowTextAttributeFieldTypeId} AND [EntityTypeId] = {workflowActionEntityTypeId}" )
                      .ToStringOrDefault( string.Empty ).ToUpper();

                    if ( workflowTextAttributeFieldTypeId > 0 && workflowActionEntityTypeId > 0 )
                    {
                        if ( statusAttributeGuid.IsNotNullOrWhiteSpace() )
                        {
                            RockMigrationHelper.AddActionTypeAttributeValue( setEmailNotFoundStatusActionTypeGuid, statusAttributeGuid, "Person Email Not Found" );
                        }
                    }

                    // Fix Lava in the My Ministry background check [Get Details] - Action
                    if ( activityTypeKvp.Value == PMM_NAME_KEY )
                    {
                        const string header = @"{% assign WarnOfRecent = Workflow | Attribute:'WarnOfRecent' %}
<h1>Background Request Details</h1>
<p>{{ CurrentPerson.NickName }}, please complete the form below to start the background request process.</p>
{% if WarnOfRecent == 'Yes' %}
    <div class='alert alert-warning'>
        Notice: It's been less than a year since this person's last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr/>";

                        const string actions = "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|";

                        // Update using the fixed lava and the current Action values for the Action Form
                        RockMigrationHelper.UpdateWorkflowActionForm( header, "", actions, "", false, "", "328B74E5-6058-4C4E-9EF8-EC10985F18A8" );
                    }
                }

            } //foreach
        }
    }
}
