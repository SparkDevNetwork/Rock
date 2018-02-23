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
    
    /// <summary>
    ///
    /// </summary>
    public partial class V7Rollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Migration Rollups */
            // DT: Add new "Register" security action to registration templates
            Sql( @"

DECLARE @TemplateEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.RegistrationTemplate' )


    INSERT INTO[Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
    SELECT @TemplateEntityTypeId, [EntityId], [Order], 'Register', [AllowOrDeny], [SpecialRole], [GroupId], NEWID()

    FROM[Auth]
    WHERE[EntityTypeId] = @TemplateEntityTypeId

    AND[Action] = 'Edit'
" );

            /* Converted Plugin Migrations */

            //// from 016_SetInactiveFamilies
            Sql( @"
	-- Clean up migration for issue #1103.
	-- Find family groups that have only 'inactive' people (record status) and mark the groups inactive.
	DECLARE @cFAMILY_GROUPTYPE_GUID uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = @cFAMILY_GROUPTYPE_GUID)

	DECLARE @cPERSON_RECORD_STATUS_INACTIVE_GUID uniqueidentifier = '1DAD99D5-41A9-4865-8366-F269902B80A4';
	DECLARE @RecordStatusInactiveId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = @cPERSON_RECORD_STATUS_INACTIVE_GUID)

	-- All groups that are currently active, but don't have a single member whose record status is not inactive
	UPDATE [Group]
	SET
		[IsActive] = 0
	WHERE NOT EXISTS (
		-- All family groups whose members are NOT inactive
		SELECT 1 FROM [Group] g 
		INNER JOIN [GroupMember] gm ON gm.[GroupId] = g.[Id]
		INNER JOIN [Person] p on p.[Id] = gm.[PersonId]
		WHERE 
			g.[Id] = [Group].[Id]
			AND g.[GroupTypeId] = @FamilyGroupTypeId 
			AND p.[RecordStatusValueId] <> @RecordStatusInactiveId
	)
	AND [Group].[GroupTypeId] = @FamilyGroupTypeId 
	AND [Group].[IsActive] = 1
" );
            //// FROM 017_FixBackgroundCheckOptionalCampus
            Sql( @"
	-- Fixes for issue #1701

    -- Change Campus optional form field to be *not* required if it has not yet been modified by the admin
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionFormAttribute] WHERE [Guid] = '18346714-25CC-4673-A4AF-BDDA3C2F1BDA' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionFormAttribute] SET
			[IsRequired] = 0
		WHERE [Guid] = '18346714-25CC-4673-A4AF-BDDA3C2F1BDA'
	END

" );

            // Add new Cancel type button
            // cancel button
            RockMigrationHelper.UpdateDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Cancel", "Used for Cancel when you don't want the page/form to validate.", "5683E775-B9F3-408C-80AC-94DE0E51CF3A", true );

            // cancel button html (attribute value)
            RockMigrationHelper.AddDefinedValueAttributeValue( "5683E775-B9F3-408C-80AC-94DE0E51CF3A", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""return true;"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-default"" >
    {{ ButtonText }}
</a>", false );

            // cancel button email html (attribute value)
            RockMigrationHelper.AddDefinedValueAttributeValue( "5683E775-B9F3-408C-80AC-94DE0E51CF3A", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#adadad""  fillcolor=""#e6e6e6"">
			<w:anchorlock/>
			<center style=""color:#333333;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#e6e6e6;border:1px solid #adadad;border-radius:4px;color:#333333;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>", false );

            // Background Check:Initial Request:Get Details
            // Background Check:Review Denial:Review
            // Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
            Sql( @"
    -- Background Check:Initial Request:Get Details
    -- Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '328B74E5-6058-4C4E-9EF8-EC10985F18A8' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionForm] SET
			[Actions] = 'Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|'
		WHERE [Guid] = '328B74E5-6058-4C4E-9EF8-EC10985F18A8'
	END

    -- Background Check:Review Denial:Review
    -- Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '91C87731-05BC-44FA-AB84-881F73EDDA20' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionForm] SET
            [Actions] = 'Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^The request has been submitted again to the security team for approval.|Cancel Request^5683E775-B9F3-408C-80AC-94DE0E51CF3A^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|'
		WHERE [Guid] = '91C87731-05BC-44FA-AB84-881F73EDDA20'
	END
" );

            //// From 019_FixIpadClientPrinting
            // NOTE: If they customized the checking label, it would be a different BinaryFile.Guid, so we are safe to update it 
            Sql( @"
    DECLARE @LabelFileId INT = ( SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = 'C2E6B0A0-3991-4FAF-9E2F-49CF321CBB0D' )
    UPDATE [BinaryFileData] 
    SET [Content] =	0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534432345E4A55535E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E46543435322C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530A5E465431322C3235345E41304E2C3133352C3134365E46485C5E4644355E46530A5E465431342C3330395E41304E2C34352C34355E46485C5E4644365E46530A5E43575A2C453A524F433030302E464E545E46543239332C38325E415A4E2C37332C36340A5E46485C5E4644425E46530A5E43575A2C453A524F433030302E464E545E46543337382C38315E415A4E2C37332C36340A5E46485C5E4644465E46530A5E46543239392C3132305E41304E2C32382C32385E46485C5E4644345E46530A5E46423333302C322C302C4C5E4654382C3338325E41304E2C32382C32385E46485C5E4644395E46530A5E43575A2C453A524F433030302E464E545E46543630352C3338335E415A4E2C37332C36345E46485C5E4644375E46530A5E43575A2C453A524F433030302E464E545E46543731352C3338365E415A4E2C37332C36345E46485C5E4644385E46530A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0A5E5051312C302C312C595E585A0A
    WHERE [Id] = @LabelFileId

    UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' %}{{ personAllergy | Truncate:100,''...'' }}{% endif %}'
    WHERE [Guid] = '4315A58E-6514-49A8-B80C-22AC7710AC19' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}{{ personLegalNotes | Truncate:100,''...'' }}{% endif %}'
    WHERE [Guid] = '89C604FA-61A9-4255-AE1F-B6381B23603F' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' -%}A{% endif -%}'
    WHERE [Guid] = '5DD35431-D22D-4410-9A55-55EAC9859C35' AND [ModifiedDateTime] IS NULL

    UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}L{% endif %}'
    WHERE [Guid] = '872DBF30-E0C0-4810-A36E-D28FC3124A51' AND [ModifiedDateTime] IS NULL
" );

            //// From 020_FixCommunicationTemplate
            Sql( @"
    UPDATE [CommunicationTemplate]
    SET [MediumDataJson] = REPLACE([MediumDataJson], 'a:visited {\ncolor: #2ba6cb !important;\n}', 'a:visited {\ncolor: #2ba6cb;\n}')
" );
            //// From 021_UpdateCheckInMergefieldDebugInfo
            Sql( MigrationSQL._201711271827181_V7Rollup_021_UpdateCheckInMergefieldDebugInfo );

            //// From 023_SecurityCodeLength
            // NOTE: Update the EntityTypeQualifierValue to string.Empty just in case the HotFix migration already ran so that it can be run again
            Sql( "UPDATE [Attribute] SET [EntityTypeQualifierValue] = '' WHERE [Guid] IN ('B76A6877-FD96-4A00-9470-AEFC3788D795','90980CBA-9842-40AB-A258-880087973258', 'FD72C08A-81E9-4D93-9370-2BA1B4192601' ) " );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Alpha Length", "", 0, "0", "B76A6877-FD96-4A00-9470-AEFC3788D795", "core_checkin_SecurityCodeAlphaLength" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Numeric Length", "", 0, "0", "90980CBA-9842-40AB-A258-880087973258", "core_checkin_SecurityCodeNumericLength" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Numeric Value Randomized", "", 0, "True", "FD72C08A-81E9-4D93-9370-2BA1B4192601", "core_checkin_SecurityCodeNumericRandom" );

            Sql( @"
        UPDATE [Attribute] SET [Name] = 'Security Code Alpha-Numeric Length' WHERE [Guid] = '712CFC8A-7B67-4793-A71E-E2EEB2D1048D'

        DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
        DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
        IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
        BEGIN

            UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
            WHERE[EntityTypeId] = @GroupTypeEntityTypeId
            AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
            AND[Key] LIKE 'core_checkin_%'

        END
" );

            //// From 025_PersonGivingEnvelopeAttribute
            RockMigrationHelper.UpdatePersonAttributeCategory( "Finance Internal", "fa fa-money", "Internal Finance Attributes", SystemGuid.Category.PERSON_ATTRIBUTES_FINANCE_INTERNAL );

            RockMigrationHelper.EnsureAttributeByGuid( SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER, "core_GivingEnvelopeNumber", SystemGuid.EntityType.PERSON, SystemGuid.FieldType.INTEGER, "", "" );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ATTRIBUTES_FINANCE_INTERNAL, "Envelope Number", "core_GivingEnvelopeNumber", "fa fa-money", "The Giving Envelope Number that is associated with this Person", 1, "", SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.BOOLEAN, null, null, "Enable Giving Envelope Number", "Enables the Giving Envelope Number feature", 0, false.ToString(), Rock.SystemGuid.Attribute.GLOBAL_ENABLE_GIVING_ENVELOPE, "core.EnableGivingEnvelopeNumber", false );

            //// From 026_PersonEditConnectionRecordStatus
            Sql( MigrationSQL._201711271827181_V7Rollup_026_PersonEditConnectionRecordStatus );

            //// From 027_AddCheckinFeatures
            RockMigrationHelper.UpdateDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Scanned Id", "Search for family based on a barcode, proximity card, etc.", "7668CE15-E372-47EE-8FF8-6FEE09F7C858", true );
            RockMigrationHelper.UpdateDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Family Id", "Search for family based on a Family Id", "111385BB-DAEB-4CE3-A945-0B50DC15EE02", true );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Group", Rock.SystemGuid.FieldType.VALUE_LIST, "GroupTypeId", "10", "Check-in Identifiers", "One or more identifiers such as a barcode, or proximity card value that can be used during check-in.", 0, "", "8F528431-A438-4488-8DC3-CA42E66C1B37", "CheckinId" );

            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "values", "0^Family,1^Person,2^Location,3^Check-Out", "E77DF4E6-A995-4C82-BBB7-DB57739D66F3" );

            // NOTE: Update the EntityTypeQualifierValue to string.Empty just in case the HotFix migration already ran so that it can be run again
            Sql( "UPDATE [Attribute] SET [EntityTypeQualifierValue] = '' WHERE [Guid] IN ('37EB8C83-A5DC-4A9B-8816-D93F07B2A7C5','C053648F-8A85-4C0C-BB24-41E9ABB56EEF' ) " );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypePurposeValueId", "", "Allow Checkout", "", 0, "False", "37EB8C83-A5DC-4A9B-8816-D93F07B2A7C5", "core_checkin_AllowCheckout" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.SINGLE_SELECT, "GroupTypePurposeValueId", "", "Auto Select Options", "", 0, "0", "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "core_checkin_AutoSelectOptions" );
            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "fieldtype", "ddl", "F614DAFE-18BC-4FFE-A0D7-90A59DAF79AA" );
            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "values", "0^People Only,1^People and Their Area/Group/Location", "542F273C-A5DB-4268-9D32-29DADA386E74" );

            Sql( @"
        DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
        DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
        IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
        BEGIN

            UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
            WHERE[EntityTypeId] = @GroupTypeEntityTypeId
            AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
            AND[Key] LIKE 'core_checkin_%'

        END
" );

            // Add the new action type entity for creating check-out labels
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.CreateCheckoutLabels", "83B13E96-A024-4ED1-9B2D-A76911139553", false, false );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "83B13E96-A024-4ED1-9B2D-A76911139553", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50AB642C-9AD1-4973-9A6D-067F7714DFF3" ); // Rock.Workflow.Action.CheckIn.CreateCheckoutLabels:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "83B13E96-A024-4ED1-9B2D-A76911139553", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "CFAC142C-73CB-4886-9A5A-4ED12C80A544" ); // Rock.Workflow.Action.CheckIn.CreateCheckoutLabels:Order

            // Change the "Set Available Schedule" action in the "Person Search" activity to use the "Load Schedule" action instead.
            Sql( @"
    DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '24A7E196-B50B-4BD6-A347-07CFC5ABEF9E')
    UPDATE [WorkflowActionType] SET [Name] = 'Load Schedules', [EntityTypeId] = @EntityTypeId WHERE [Guid] = '79CB608D-ED25-4526-A0F5-132D13642CDA'
" );
            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"True" ); // Unattended Check-in:Person Search:Load Schedules:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "F7B09469-EB3D-44A4-AB8E-C74318BD4669", @"" ); // Unattended Check-in:Person Search:Load Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "4DFA9F8D-F2E6-4040-A23B-2A1F8258C767", @"False" ); // Unattended Check-in:Person Search:Load Schedules:Active

            // Remove the "Preselect Recent Attendance" action from the "Person Search" activity
            RockMigrationHelper.DeleteWorkflowActionType( "08D15C7A-4421-420A-BCA8-D6EE532E659F" );

            // Add a new activity type for creating check-out labels
            RockMigrationHelper.UpdateWorkflowActivityType( "011E9F5A-60D4-4FF5-912A-290881E37EAF", true, "Create Check-Out Labels", "Creates the labels to be printed during check-out", false, 8, "1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE" ); // Unattended Check-in:Create Check-Out Labels
            RockMigrationHelper.UpdateWorkflowActionType( "1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE", "Create Labels", 0, "83B13E96-A024-4ED1-9B2D-A76911139553", true, false, "", "", 1, "", "B015DEB3-678A-478B-9DF4-AB4059C9A29B" ); // Unattended Check-in:Create Check-Out Labels:Create Labels
            Sql( @"
    DECLARE @WorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '011E9F5A-60D4-4FF5-912A-290881E37EAF' )
    DECLARE @Order int = (ISNULL(( SELECT MAX([Order]) FROM [WorkflowActivityType] WHERE [WorkflowTypeId] = @WorkflowTypeId ),-1) + 1)
    UPDATE [WorkflowActivityType] SET [Order] = @Order WHERE [Guid] = '1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE'
" );
            RockMigrationHelper.AddActionTypeAttributeValue( "B015DEB3-678A-478B-9DF4-AB4059C9A29B", "CFAC142C-73CB-4886-9A5A-4ED12C80A544", @"" ); // Unattended Check-in:Create Check-Out Labels:Create Labels:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B015DEB3-678A-478B-9DF4-AB4059C9A29B", "50AB642C-9AD1-4973-9A6D-067F7714DFF3", @"False" ); // Unattended Check-in:Create Check-Out Labels:Create Labels:Active


            RockMigrationHelper.AddPage( true, "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Action Select", "", "0586648B-9490-43C6-B18D-7F403458C080", "" ); // Site:Rock Check-in
            RockMigrationHelper.AddPage( true, "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Check Out Person Select", "", "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "" ); // Site:Rock Check-in
            RockMigrationHelper.AddPage( true, "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Check Out Success", "", "21A855BA-6D68-4504-97B4-D787452CEC29", "" ); // Site:Rock Check-in

            RockMigrationHelper.UpdateBlockType( "Action Select", "Displays option for family to Check In or Check Out.", "~/Blocks/CheckIn/ActionSelect.ascx", "Check-in", "66DDB050-8F60-4DF3-9AED-5CE283E22350" );
            RockMigrationHelper.UpdateBlockType( "Check Out Person Select", "Lists people who match the selected family and provides option of selecting multiple people to check-out.", "~/Blocks/CheckIn/CheckOutPersonSelect.ascx", "Check-in", "54EB0252-6FE4-49C5-8716-14A3A06C3EC5" );
            RockMigrationHelper.UpdateBlockType( "Check Out Success", "Displays the details of a successful check out.", "~/Blocks/CheckIn/CheckoutSuccess.ascx", "Check-in", "F499C4A9-9A60-404B-9383-B950EE6D7821" );

            // Add Block to Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "0586648B-9490-43C6-B18D-7F403458C080", "", "66DDB050-8F60-4DF3-9AED-5CE283E22350", "Action Select", "Main", "", "", 0, "F5C21AE7-4BB4-4628-9B15-7CF761C66891" );
            // Add Block to Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "", "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "Check Out Person Select", "Main", "", "", 0, "1F33702B-C26C-4EAA-AD0E-8510384EACBC" );
            // Add Block to Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "21A855BA-6D68-4504-97B4-D787452CEC29", "", "F499C4A9-9A60-404B-9383-B950EE6D7821", "Check Out Success", "Main", "", "", 0, "32B345DD-0EF4-480E-B82A-7D7191CC374B" );

            // Attrib for BlockType: Action Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "31BAADBE-0E12-4EC4-B05D-472EBAD9C1B5" );
            // Attrib for BlockType: Action Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "C0EEDB49-6B69-47B0-98DE-2A1A28188C5D" );
            // Attrib for BlockType: Action Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "DE27E0C8-5BEF-48FE-88D9-3E8300B4988E" );
            // Attrib for BlockType: Action Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "5751A6B9-1155-4BAC-BA2D-84C6A419D6E7" );
            // Attrib for BlockType: Action Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "A161CC4A-F521-49A3-B648-165A7AE4EFE0" );
            // Attrib for BlockType: Action Select:Next Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page (Family Check-in)", "FamilyNextPage", "", "", 5, @"", "83450920-66B3-46FD-AEA5-35EC43F96C9D" );
            // Attrib for BlockType: Action Select:Check Out Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Check Out Page", "CheckOutPage", "", "", 6, @"", "F70CFDEC-1131-4127-A6B8-A1A9AEE02D71" );

            // Attrib for BlockType: Check Out Person Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A" );
            // Attrib for BlockType: Check Out Person Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "A4FB35E5-8A62-47FE-AE49-6E447DA8CF82" );
            // Attrib for BlockType: Check Out Person Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "843FB186-90E8-4DCE-B138-B23E891E2CFF" );
            // Attrib for BlockType: Check Out Person Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "67C256C2-753F-410B-B683-F64368AC8497" );
            // Attrib for BlockType: Check Out Person Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "18BAC460-2630-4651-A320-7927A3078A87" );

            // Attrib for BlockType: Check Out Success:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "7328D95B-D9BB-49D0-943B-B374EBC664DD" );
            // Attrib for BlockType: Check Out Success:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "76C4F5AC-7EA8-45ED-8B7C-1974361FDEE5" );
            // Attrib for BlockType: Check Out Success:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "ADA3C354-42ED-4F28-8F68-38FBC2926CBF" );
            // Attrib for BlockType: Check Out Success:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "735D4AAB-F8F4-4388-9A00-2132356187A6" );
            // Attrib for BlockType: Check Out Success:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "223C5BA3-B6B0-4EC6-9D38-5607837410D6" );

            // Attrib for BlockType: Welcome:Check-in Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Button Text", "CheckinButtonText", "", "The text to display on the check-in button.", 7, @"", "C211328D-3F66-4F5D-902A-2A7AF1985209" );

            // Attrib for BlockType: Person Select (Family Check-in):Auto Select Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Auto Select Next Page", "AutoSelectNextPage", "", "The page to navigate to after selecting people in auto-select mode.", 5, @"", "4302646B-F6CD-492D-8850-96B9CA1CEA59" );
            // Attrib for BlockType: Person Select (Family Check-in):Pre-Selected Options Format
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pre-Selected Options Format", "OptionFormat", "", "", 6, @"<span class='auto-select-schedule'>{{ Schedule.Name }}:</span>
<span class='auto-select-group'>{{ Group.Name }}</span>
<span class='auto-select-location'>{{ Location.Name }}</span>", "55580865-E792-469F-B45C-45713477D033" );

            // Update Family Select Next Page to direct to Action Page
            // NOTE: We want to overwrite the attribute value if already exists (the attribute was introduced in a prior EF Migration and we want it to be updated to this now)
            RockMigrationHelper.AddBlockAttributeValue( false, "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "90ECD00A-9570-4986-B32F-02F32B656A2A", @"0586648b-9490-43c6-b18d-7f403458c080" ); // Next Page

            /* START: All of these were introduced in the Hotfix, so skip if they already exist */
            // Attrib Value for Block:Action Select, Attribute:Workflow Activity Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "C0EEDB49-6B69-47B0-98DE-2A1A28188C5D", @"" );
            // Attrib Value for Block:Action Select, Attribute:Home Page Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "DE27E0C8-5BEF-48FE-88D9-3E8300B4988E", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Action Select, Attribute:Previous Page Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "5751A6B9-1155-4BAC-BA2D-84C6A419D6E7", @"10c97379-f719-4acb-b8c6-651957b660a4" );
            // Attrib Value for Block:Action Select, Attribute:Next Page Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "A161CC4A-F521-49A3-B648-165A7AE4EFE0", @"bb8cf87f-680f-48f9-9147-f4951e033d17" );
            // Attrib Value for Block:Action Select, Attribute:Next Page (Family Check-in) Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "83450920-66B3-46FD-AEA5-35EC43F96C9D", @"d14154ba-2f2c-41c3-b380-f833252cbb13" );
            // Attrib Value for Block:Action Select, Attribute:Check Out Page Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "F70CFDEC-1131-4127-A6B8-A1A9AEE02D71", @"d54fc289-df7d-48c5-91be-38bcfdebc6af" );

            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Type Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Activity Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "A4FB35E5-8A62-47FE-AE49-6E447DA8CF82", @"Create Check-Out Labels" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Home Page Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "843FB186-90E8-4DCE-B138-B23E891E2CFF", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Previous Page Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "67C256C2-753F-410B-B683-F64368AC8497", @"0586648b-9490-43c6-b18d-7f403458c080" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Next Page Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "18BAC460-2630-4651-A320-7927A3078A87", @"21a855ba-6d68-4504-97b4-d787452cec29" );

            // Attrib Value for Block:Check Out Success, Attribute:Workflow Activity Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "32B345DD-0EF4-480E-B82A-7D7191CC374B", "76C4F5AC-7EA8-45ED-8B7C-1974361FDEE5", @"" );
            // Attrib Value for Block:Check Out Success, Attribute:Home Page Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "32B345DD-0EF4-480E-B82A-7D7191CC374B", "ADA3C354-42ED-4F28-8F68-38FBC2926CBF", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Check Out Success, Attribute:Previous Page Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "32B345DD-0EF4-480E-B82A-7D7191CC374B", "735D4AAB-F8F4-4388-9A00-2132356187A6", @"d54fc289-df7d-48c5-91be-38bcfdebc6af" );

            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "4302646B-F6CD-492D-8850-96B9CA1CEA59", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Auto Select Next Page
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "55580865-E792-469F-B45C-45713477D033", @"{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }}" ); // Pre-Selected Options Format
            /* END: All of these were introduced in the Hotfix, so skip if they already exist */

            // Add Block to Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "0586648B-9490-43C6-B18D-7F403458C080", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "7A293980-9E28-4115-85EB-DA197734EED2" );
            // Add Block to Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "258CC6B9-CA88-41E7-B578-2514FCF245B4" );
            // Add Block to Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "21A855BA-6D68-4504-97B4-D787452CEC29", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "04BF66EF-66E5-465D-A590-D8BA02E217B7" );
            // update block order for pages with new blocks if the page,zone has multiple blocks

            
            /* START: All of these are for a Block that was introduced in the HotFix, so skip if it already exists  */
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "7A293980-9E28-4115-85EB-DA197734EED2", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "7A293980-9E28-4115-85EB-DA197734EED2", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "258CC6B9-CA88-41E7-B578-2514FCF245B4", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "258CC6B9-CA88-41E7-B578-2514FCF245B4", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "04BF66EF-66E5-465D-A590-D8BA02E217B7", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "04BF66EF-66E5-465D-A590-D8BA02E217B7", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            /* END: All of these are for a Block that was introduced in HotFix, so skip if it already exists  */

            Sql( @"
update DataViewFilter
set selection = '[
	""SecondaryAudiences"",
    ""8"",
    ""b364cdee-f000-4965-ae67-0c80dda365dc""
]' 
where selection = '[
    ""SecondaryAudiences"",
	""8"",
	""4cdee-f000-4965-ae67-0c80dda365dc\""\""
]'

update DataViewFilter
set selection = '[
    ""SecondaryAudiences"",
	""8"",
	""57b2a23f-3b0c-43a8-9f45-332120dcd0ee""
]' 
where selection = '[
    ""SecondaryAudiences"",
	""8"",
	""2a23f-3b0c-43a8-9f45-332120dcd0ee\""\"",
]'" );

            //// From 028_CheckinTextSettings
            // Attrib for BlockType: Person Search:Phone Number Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Number Types", "PhoneNumberTypes", "", "Types of phone numbers to include with person detail", 1, @"", "05E58531-95AF-4883-BE0D-1EC034BB81DD" );
            // Attrib for BlockType: Person Search:Show Gender
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "", "Should a gender column be displayed?", 4, @"False", "0EB7AFCB-A274-4D9C-9D5B-7C9613BF9A27" );
            // Attrib for BlockType: Person Search:Show Envelope Number
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Envelope Number", "ShowEnvelopeNumber", "", "Should an envelope # column be displayed?", 6, @"False", "B6B8F559-BFDE-43DB-9BD7-20EF8B4DAB48" );
            // Attrib for BlockType: Person Search:Show Spouse
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Spouse", "ShowSpouse", "", "Should a spouse column be displayed?", 5, @"False", "18EA8DDF-0DC5-471E-8A6B-ED978FD1F2BC" );
            // Attrib for BlockType: Person Search:Show Birthdate
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birthdate", "ShowBirthdate", "", "Should a birthdate column be displayed?", 2, @"False", "B57C80CA-69F2-4B55-B909-B59EAEAF10F4" );
            // Attrib for BlockType: Person Search:Show Age
            RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "", "Should an age column be displayed?", 3, @"True", "F18A1DEE-C0DA-40B1-A75A-9808780F27BF" );
            // Attrib for BlockType: Welcome:Not Active Yet Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Yet Caption", "NotActiveYetCaption", "", "Caption displayed when there are active options today, but none are active now. Use {0} for a countdown timer.", 10, @"This kiosk is not active yet.  Countdown until active: {0}.", "4ACAEE3E-1388-485E-A641-07C69562D317" );
            // Attrib for BlockType: Welcome:Not Active Yet Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Yet Title", "NotActiveYetTitle", "", "Title displayed when there are active options today, but none are active now.", 9, @"Check-in Is Not Active Yet", "151B4CD4-C5CA-47F0-B2EB-348408FC8AE1" );
            // Attrib for BlockType: Welcome:No Option Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Caption", "NoOptionCaption", "", "The text to display when there are not any families found matching a scanned identifier (barcode, etc).", 14, @"Sorry, there were not any families found with the selected identifier.", "35727C8E-71A2-4272-ACB3-5D407194D728" );
            // Attrib for BlockType: Welcome:Not Active Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Title", "NotActiveTitle", "", "Title displayed when there are not any active options today.", 7, @"Check-in Is Not Active", "32470663-4CCE-4FA8-9AAC-CF7B5C6346D1" );
            // Attrib for BlockType: Welcome:Not Active Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Caption", "NotActiveCaption", "", "Caption displayed when there are not any active options today.", 8, @"There are no current or future schedules for this kiosk today!", "1D832AB9-DA71-47B3-B4E8-6661A316BD7B" );
            // Attrib for BlockType: Welcome:Closed Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Closed Title", "ClosedTitle", "", "", 11, @"Closed", "C4642C7B-049C-4A00-A2EE-1ABE7EF05E61" );
            // Attrib for BlockType: Welcome:Closed Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Closed Caption", "ClosedCaption", "", "", 12, @"This location is currently closed.", "4AC7E21F-7805-4E43-9301-597E45EA5211" );
            // Attrib for BlockType: Search:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 6, @"There were not any families that match the search criteria.", "E4AFD216-5386-4E38-B98F-9436601F7B1B" );
            // Attrib for BlockType: Search:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for search type.", 5, @"Search By {0}", "837B34E3-D140-44CD-8456-9D222325E42E" );
            // Attrib for BlockType: Family Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 6, @"Select Your Family", "F1911A0A-AC69-474F-9D99-A5022D6E129C" );
            // Attrib for BlockType: Family Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display.", 5, @"Families", "AF6A6A8B-981A-4ACB-A42C-1D576917C724" );
            // Attrib for BlockType: Family Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 7, @"Sorry, no one in your family is eligible to check-in at this location.", "E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF" );
            // Attrib for BlockType: Person Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "The option to display when there are not any people that match. Use {0} for the current action ('into' or 'out of').", 10, @"Sorry, there are currently not any available areas that the selected person can check {0}.", "9B51C224-E03E-498F-A3A2-855FFF71A103" );
            // Attrib for BlockType: Person Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name.", 8, @"{0}", "8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B" );
            // Attrib for BlockType: Person Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 9, @"Select Person", "50595A53-E5FE-4515-9DBD-EA4B006A5AFF" );
            // Attrib for BlockType: Person Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "3747242A-6852-4B60-BB63-49DEB8A20CF1" );
            // Attrib for BlockType: Group Type Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after group type is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "E19DD14C-80DD-4427-B73D-58187E1BE8AD" );
            // Attrib for BlockType: Group Type Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 9, @"{0}", "9DDF3190-E07F-4964-9CDC-69AF675FCF2E" );
            // Attrib for BlockType: Group Type Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Area", "314506D3-FCC0-42AC-86A2-77EE921C0CCD" );
            // Attrib for BlockType: Group Type Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available areas that {0} can check into at {1}.", "444058FF-0C4D-4D2E-9FDA-6036AD572C7E" );
            // Attrib for BlockType: Location Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after location is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "C14C7143-5D9C-463F-9C8B-2680509C22A5" );
            // Attrib for BlockType: Location Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available locations that {0} can check into at {1}.", "C94E5760-4EF1-40B4-84B9-B75EFAA1030B" );
            // Attrib for BlockType: Location Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "F95CAB1D-37A4-4A53-B63F-BF9D275FBA27" );
            // Attrib for BlockType: Location Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group name.", 9, @"{0}", "A85DEF5A-D6CE-41FE-891E-36880DE5CD9C" );
            // Attrib for BlockType: Location Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Location", "3CD2A392-3A06-42BE-A3A9-8324D5FCC810" );
            // Attrib for BlockType: Group Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "256D4CC4-9F09-47D3-B167-46F876F0ACD3" );
            // Attrib for BlockType: Group Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Group", "B36D25F5-7A36-4901-9ACE-72ED355F5C6C" );
            // Attrib for BlockType: Group Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 11, @"Sorry, no one in your family is eligible to check-in at this location.", "05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4" );
            // Attrib for BlockType: Group Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group type name.", 9, @"{0}", "342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376" );
            // Attrib for BlockType: Time Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family/person name.", 5, @"{0}", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E" );
            // Attrib for BlockType: Time Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group/location name.", 6, @"{0}", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837" );
            // Attrib for BlockType: Time Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 7, @"Select Time(s)", "16091831-474A-4618-872F-E9257F7E9948" );
            // Attrib for BlockType: Success:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "", 6, @"Checked-in", "E1A36C9B-9D35-4BA2-81D5-87E242D36999" );
            // Attrib for BlockType: Success:Detail Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Detail Message", "DetailMessage", "", "The message to display indicating person has been checked in. Use {0} for person, {1} for group, {2} for schedule, and {3} for the security code", 7, @"{0} was checked into {1} in {2} at {3}", "2DF1D330-3724-4F2E-B297-D6EB6654398E" );
            // Attrib for BlockType: Ability Level Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person's name.", 9, @"{0}", "085D8CA9-82EE-40C2-8985-F3DB36DC4370" );
            // Attrib for BlockType: Ability Level Select:No Option Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Caption", "NoOptionCaption", "", "", 12, @"Sorry, there are currently not any available options to check into.", "6886E63B-F961-4088-9FD9-72D1A5C84DD7" );
            // Attrib for BlockType: Ability Level Select:Selection No Option
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selection No Option", "SelectionNoOption", "", "Text displayed if there are not any options after selecting an ability level. Use {0} for person's name.", 13, @"Sorry, based on your selection, there are currently not any available locations that {0} can check into.", "FDF2C27E-5D91-44EF-AEB7-B559A7711EE5" );
            // Attrib for BlockType: Ability Level Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Ability Level", "DE63023F-10BC-4D71-94EA-A9E020016E97" );
            // Attrib for BlockType: Ability Level Select:No Option Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Title", "NoOptionTitle", "", "", 11, @"Sorry", "634FE416-500A-4C32-A1E0-123C19681574" );
            // Attrib for BlockType: Person Select (Family Check-in):Option Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Option Title", "OptionTitle", "", "Title to display on option screen. Use {0} for person's full name.", 9, @"{0}", "816DA5E9-2865-4312-B704-05EC9D83FAB2" );
            // Attrib for BlockType: Person Select (Family Check-in):Option Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Option Sub Title", "OptionSubTitle", "", "Sub-title to display on option screen. Use {0} for person's nick name.", 10, @"Please select the options that {0} would like to attend.", "5C67A905-7DD5-438E-A60E-BCB5078C0686" );
            // Attrib for BlockType: Person Select (Family Check-in):No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 11, @"Sorry, there are currently not any available areas that the selected people can check into.", "A0893943-F77A-42D2-AD21-BEBAF2573A54" );
            // Attrib for BlockType: Person Select (Family Check-in):Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 8, @"Select People", "3CE01A6B-8F4B-4050-8201-8AAB5F3A79D4" );
            // Attrib for BlockType: Person Select (Family Check-in):Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name.", 7, @"{0}", "88D0B3A3-9EFA-45DF-97BF-7F200AFA80BD" );
            // Attrib for BlockType: Action Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 8, @"Select Action", "1E7D1586-9636-4144-9019-61DE1CFF576F" );
            // Attrib for BlockType: Action Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name", 7, @"{0}", "0D5B5B30-E1CE-402E-B7FB-760F8E4975B2" );
            // Attrib for BlockType: Check Out Person Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 6, @"Select People", "45B1D388-9DBC-4A8A-8AC7-70423E672624" );
            // Attrib for BlockType: Check Out Person Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name", 5, @"{0} Check Out", "05CA1B42-C5F1-407C-9F35-2CF4104BC96D" );
            // Attrib for BlockType: Check Out Success:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display.", 5, @"Checked Out", "3784B16E-BF23-42A4-8E04-0E93EB71C0D4" );
            // Attrib for BlockType: Check Out Success:Detail Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Detail Message", "DetailMessage", "", "The message to display indicating person has been checked out. Use {0} for person, {1} for group, {2} for location, and {3} for schedule.", 6, @"{0} was checked out of {1} in {2} at {3}.", "A6C1FF95-43D8-4602-9175-B6F0B0523E61" );

            /* START: All of these were introduced in the HotFix, so skip if they already exists */
            // Attrib Value for Block:Welcome, Attribute:Not Active Yet Caption Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "4ACAEE3E-1388-485E-A641-07C69562D317", @"This kiosk is not active yet.  Countdown until active: {0}." );
            // Attrib Value for Block:Welcome, Attribute:Not Active Yet Title Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "151B4CD4-C5CA-47F0-B2EB-348408FC8AE1", @"Check-in Is Not Active Yet" );
            // Attrib Value for Block:Welcome, Attribute:No Option Caption Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "35727C8E-71A2-4272-ACB3-5D407194D728", @"Sorry, there were not any families found with the selected identifier." );
            // Attrib Value for Block:Welcome, Attribute:Not Active Title Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "32470663-4CCE-4FA8-9AAC-CF7B5C6346D1", @"Check-in Is Not Active" );
            // Attrib Value for Block:Welcome, Attribute:Not Active Caption Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "1D832AB9-DA71-47B3-B4E8-6661A316BD7B", @"There are no current or future schedules for this kiosk today!" );
            // Attrib Value for Block:Welcome, Attribute:Closed Title Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "C4642C7B-049C-4A00-A2EE-1ABE7EF05E61", @"Closed" );
            // Attrib Value for Block:Welcome, Attribute:Closed Caption Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "4AC7E21F-7805-4E43-9301-597E45EA5211", @"This location is currently closed." );
            // Attrib Value for Block:Search, Attribute:No Option Message Page: Search, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "E4AFD216-5386-4E38-B98F-9436601F7B1B", @"There were not any families that match the search criteria." );
            // Attrib Value for Block:Search, Attribute:Title Page: Search, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "837B34E3-D140-44CD-8456-9D222325E42E", @"Search By {0}" );
            // Attrib Value for Block:Family Select, Attribute:Caption Page: Family Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "F1911A0A-AC69-474F-9D99-A5022D6E129C", @"Select Your Family" );
            // Attrib Value for Block:Family Select, Attribute:Title Page: Family Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "AF6A6A8B-981A-4ACB-A42C-1D576917C724", @"Families" );
            // Attrib Value for Block:Family Select, Attribute:No Option Message Page: Family Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF", @"Sorry, no one in your family is eligible to check-in at this location." );
            // Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "9B51C224-E03E-498F-A3A2-855FFF71A103", @"Sorry, there are currently not any available areas that the selected person can check {0}." );
            // Attrib Value for Block:Person Select, Attribute:Title Page: Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B", @"{0}" );
            // Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "50595A53-E5FE-4515-9DBD-EA4B006A5AFF", @"Select Person" );
            // Attrib Value for Block:Person Select, Attribute:Workflow Activity Page: Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "3747242A-6852-4B60-BB63-49DEB8A20CF1", @"" );
            // Attrib Value for Block:Group Type Select, Attribute:No Option After Select Message Page: Group Type Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "E19DD14C-80DD-4427-B73D-58187E1BE8AD", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );
            // Attrib Value for Block:Group Type Select, Attribute:Title Page: Group Type Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "9DDF3190-E07F-4964-9CDC-69AF675FCF2E", @"{0}" );
            // Attrib Value for Block:Group Type Select, Attribute:Caption Page: Group Type Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "314506D3-FCC0-42AC-86A2-77EE921C0CCD", @"Select Area" );
            // Attrib Value for Block:Group Type Select, Attribute:No Option Message Page: Group Type Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "444058FF-0C4D-4D2E-9FDA-6036AD572C7E", @"Sorry, there are currently not any available areas that {0} can check into at {1}." );
            // Attrib Value for Block:Location Select, Attribute:No Option After Select Message Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9D876B07-DF35-4355-85B0-638F65C367C4", "C14C7143-5D9C-463F-9C8B-2680509C22A5", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );
            // Attrib Value for Block:Location Select, Attribute:No Option Message Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9D876B07-DF35-4355-85B0-638F65C367C4", "C94E5760-4EF1-40B4-84B9-B75EFAA1030B", @"Sorry, there are currently not any available locations that {0} can check into at {1}." );
            // Attrib Value for Block:Location Select, Attribute:Title Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9D876B07-DF35-4355-85B0-638F65C367C4", "F95CAB1D-37A4-4A53-B63F-BF9D275FBA27", @"{0}" );
            // Attrib Value for Block:Location Select, Attribute:Sub Title Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9D876B07-DF35-4355-85B0-638F65C367C4", "A85DEF5A-D6CE-41FE-891E-36880DE5CD9C", @"{0}" );
            // Attrib Value for Block:Location Select, Attribute:Caption Page: Location Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9D876B07-DF35-4355-85B0-638F65C367C4", "3CD2A392-3A06-42BE-A3A9-8324D5FCC810", @"Select Location" );
            // Attrib Value for Block:Group Select, Attribute:Title Page: Group Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "256D4CC4-9F09-47D3-B167-46F876F0ACD3", @"{0}" );
            // Attrib Value for Block:Group Select, Attribute:Caption Page: Group Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "B36D25F5-7A36-4901-9ACE-72ED355F5C6C", @"Select Group" );
            // Attrib Value for Block:Group Select, Attribute:No Option Message Page: Group Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4", @"Sorry, no one in your family is eligible to check-in at this location." );
            // Attrib Value for Block:Group Select, Attribute:Sub Title Page: Group Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Title Page: Time Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "472E00D1-BD9B-407A-92C6-05132039DB65", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "472E00D1-BD9B-407A-92C6-05132039DB65", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "472E00D1-BD9B-407A-92C6-05132039DB65", "16091831-474A-4618-872F-E9257F7E9948", @"Select Time(s)" );
            // Attrib Value for Block:Success, Attribute:Title Page: Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "E1A36C9B-9D35-4BA2-81D5-87E242D36999", @"Checked-in" );
            // Attrib Value for Block:Success, Attribute:Detail Message Page: Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "2DF1D330-3724-4F2E-B297-D6EB6654398E", @"{0} was checked into {1} in {2} at {3}" );
            // Attrib Value for Block:Ability Level Select, Attribute:Title Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "C175A9ED-612E-4B25-BED4-CF713D922179", "085D8CA9-82EE-40C2-8985-F3DB36DC4370", @"{0}" );
            // Attrib Value for Block:Ability Level Select, Attribute:No Option Caption Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "C175A9ED-612E-4B25-BED4-CF713D922179", "6886E63B-F961-4088-9FD9-72D1A5C84DD7", @"Sorry, there are currently not any available options to check into." );
            // Attrib Value for Block:Ability Level Select, Attribute:Selection No Option Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "C175A9ED-612E-4B25-BED4-CF713D922179", "FDF2C27E-5D91-44EF-AEB7-B559A7711EE5", @"Sorry, based on your selection, there are currently not any available locations that {0} can check into." );
            // Attrib Value for Block:Ability Level Select, Attribute:Caption Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "C175A9ED-612E-4B25-BED4-CF713D922179", "DE63023F-10BC-4D71-94EA-A9E020016E97", @"Select Ability Level" );
            // Attrib Value for Block:Ability Level Select, Attribute:No Option Title Page: Ability Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "C175A9ED-612E-4B25-BED4-CF713D922179", "634FE416-500A-4C32-A1E0-123C19681574", @"Sorry" );
            // Attrib Value for Block:Person Select, Attribute:Option Title Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "816DA5E9-2865-4312-B704-05EC9D83FAB2", @"{0}" );
            // Attrib Value for Block:Person Select, Attribute:Option Sub Title Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "5C67A905-7DD5-438E-A60E-BCB5078C0686", @"Please select the options that {0} would like to attend." );
            // Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "A0893943-F77A-42D2-AD21-BEBAF2573A54", @"Sorry, there are currently not any available areas that the selected people can check into." );
            // Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "3CE01A6B-8F4B-4050-8201-8AAB5F3A79D4", @"Select People" );
            // Attrib Value for Block:Person Select, Attribute:Title Page: Person Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "88D0B3A3-9EFA-45DF-97BF-7F200AFA80BD", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Title Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "558C15C1-47F7-4232-A069-89463B17924F", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "558C15C1-47F7-4232-A069-89463B17924F", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837", @"{0}" );
            // Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select (Family Check-in), Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "558C15C1-47F7-4232-A069-89463B17924F", "16091831-474A-4618-872F-E9257F7E9948", @"Select Time(s)" );
            // Attrib Value for Block:Action Select, Attribute:Caption Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "1E7D1586-9636-4144-9019-61DE1CFF576F", @"Select Action" );
            // Attrib Value for Block:Action Select, Attribute:Title Page: Action Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "0D5B5B30-E1CE-402E-B7FB-760F8E4975B2", @"{0}" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Caption Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "45B1D388-9DBC-4A8A-8AC7-70423E672624", @"Select People" );
            // Attrib Value for Block:Check Out Person Select, Attribute:Title Page: Check Out Person Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "05CA1B42-C5F1-407C-9F35-2CF4104BC96D", @"{0} Check Out" );
            // Attrib Value for Block:Check Out Success, Attribute:Title Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "32B345DD-0EF4-480E-B82A-7D7191CC374B", "3784B16E-BF23-42A4-8E04-0E93EB71C0D4", @"Checked Out" );
            // Attrib Value for Block:Check Out Success, Attribute:Detail Message Page: Check Out Success, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( true, "32B345DD-0EF4-480E-B82A-7D7191CC374B", "A6C1FF95-43D8-4602-9175-B6F0B0523E61", @"{0} was checked out of {1} in {2} at {3}." );
            /* END: All of these were introduced in the HotFix, so skip if they already exists */

            //// From 029_BatchDetailReopenBatchSecurity
            Sql( MigrationSQL._201711271827181_V7Rollup_029_BatchDetailReopenBatchSecurity );

            /// From 030_MyConnectionOpportunitiesLava
            RockMigrationHelper.UpdateBlockType( "My Connection Opportunities Lava", "Block to display connection opportunities that are assigned to the current user. The display format is controlled by a lava template.", "~/Blocks/Connection/MyConnectionOpportunitiesLava.ascx", "Connection", "1B8E50A0-7AC4-475F-857C-50D0809A3F04" );
            RockMigrationHelper.AddBlock( true, "AE1818D8-581C-4599-97B9-509EA450376A", "", "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "My Connection Opportunities Lava", "Main", "", "", 2, "35B7FF3C-969E-44BE-BACA-EDB490450DFF" );

            /* This block was introduced in the hotfix, so don't update the AttributeValue if it already exists*/
            RockMigrationHelper.AddBlockAttributeValue( true, "35B7FF3C-969E-44BE-BACA-EDB490450DFF", "9E6887CA-6D20-47EE-8158-3EC9F06F063D", @"50f04e77-8d3b-4268-80ab-bc15dd6cb262" ); // Detail Page

            // Attrib for BlockType: My Connection Opportunities Lava:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"Page used to view details of a request.", 1, @"50f04e77-8d3b-4268-80ab-bc15dd6cb262", "848484B1-0666-4B2A-B63B-22CFBD00540E" );
            // Attrib for BlockType: My Connection Opportunities Lava:Contents
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Contents", "Contents", "", @"The Lava template to use for displaying connection opportunities assigned to current user.", 3, @"{% include '~~/Assets/Lava/MyConnectionOpportunitiesSortable.lava' %}", "03C85B59-C957-4216-A750-B667188B4CB9" );
            // Attrib for BlockType: My Connection Opportunities Lava:Connection Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "", @"Optional list of connection types to limit the display to (All will be displayed by default).", 2, @"", "2687FEFB-9012-499A-A37D-32838C952D3D" );

            //// From 032_MigrationRollupsForV6_9
            // DT: Update American Express Value for Pushpay
            Sql( @"
    UPDATE [DefinedValue] SET [Value] = 'Amex' WHERE [Guid] = '696A54E3-352C-49FB-88A1-BCDBD81AA9EC'
" );

            //// From 034_FinancialSecurityActions
            Sql( @"
    DECLARE @RockAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
    DECLARE @FinanceAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '6246A7EF-B7A3-4C8C-B1E4-3FF114B84559' )
    DECLARE @FinanceUsersGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9' )

    -- Batch Entity Type 'Delete' Action
    DECLARE @EntityTypeIdFinancialBatch INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.FinancialBatch' )
    IF NOT EXISTS ( SELECT [Id] FROM [Auth] WHERE [EntityTypeId] = @EntityTypeIdFinancialBatch AND [EntityId] = 0 AND [Action] = 'Delete' )
    BEGIN
	    INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
        VALUES
            ( @EntityTypeIdFinancialBatch, 0, 0, 'Delete', 'A', 0, @FinanceUsersGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 1, 'Delete', 'A', 0, @FinanceAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 2, 'Delete', 'A', 0, @RockAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialBatch, 0, 3, 'Delete', 'D', 1, NULL, NEWID() )
    END

    -- Financial Transaction Entity Type 'Refund' Action
    DECLARE @EntityTypeIdFinancialTxn INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.FinancialTransaction' )
    IF NOT EXISTS ( SELECT [Id] FROM [Auth] WHERE [EntityTypeId] = @EntityTypeIdFinancialTxn AND [EntityId] = 0 AND [Action] = 'Refund' )
    BEGIN
	    INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
        VALUES
            ( @EntityTypeIdFinancialTxn, 0, 0, 'Refund', 'A', 0, @FinanceUsersGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 1, 'Refund', 'A', 0, @FinanceAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 2, 'Refund', 'A', 0, @RockAdminGroupId, NEWID() ), 
            ( @EntityTypeIdFinancialTxn, 0, 3, 'Refund', 'D', 1, NULL, NEWID() )
    END

	-- Transaction List Block 'Filter By Person' Action
    DECLARE @EntityTypeIdBlock INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block' )
    DECLARE @TransactionListBlockTypeId INT = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'E04320BC-67C3-452D-9EF6-D74D8C177154' )
    IF NOT EXISTS ( SELECT A.[Id] FROM [Auth] A INNER JOIN [Block] B ON B.[Id] = A.[EntityId] 
        WHERE EntityTypeId = @EntityTypeIdBlock AND B.[BlockTypeId] = @TransactionListBlockTypeId AND [Action] = 'FilterByPerson' )
    BEGIN
	
        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 0, 'FilterByPerson', 'A', 0, @FinanceUsersGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 1, 'FilterByPerson', 'A', 0, @FinanceAdminGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 2, 'FilterByPerson', 'A', 0, @RockAdminGroupId, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

        INSERT INTO [Auth] ( [EntityTypeid], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
	    SELECT @EntityTypeIdBlock, [Id], 3, 'FilterByPerson', 'D', 1, NULL, NEWID()
	    FROM [Block] WHERE [BlockTypeId] = @TransactionListBlockTypeId

    END
" );

            //// From 036_PrayerRequestFollowingEvent
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonPrayerRequest", "DAE05FAE-A26F-465A-836C-BAA0EFA1267B", false, true );

            Sql( @"
    IF NOT EXISTS ( SELECT [Id] FROM [FollowingEventType] WHERE [Guid] = '0323D1DE-616B-4060-AF72-1F17FEEA648F' )
    BEGIN

        DECLARE @PersonAliasEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonAlias' )
        DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'DAE05FAE-A26F-465A-836C-BAA0EFA1267B' )

        INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	    VALUES 
	        ( 'Prayer Requests', 'Person submitted a public prayer request', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, 0, 
'<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px; min-width: 87px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign=""top"" style=''padding-bottom: 12px; min-width: 300px;''>
        <strong><a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}"">{{ Entity.Person.FullName }}</a> 
        recently submitted a public prayer request.</strong><br />
        
        {% if Entity.Person.Email != empty %}
            Email: <a href=""mailto:{{ Entity.Person.Email }}"">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>', '0323D1DE-616B-4060-AF72-1F17FEEA648F' )

    END
" );

            // SK:  Typo on Person Badge
            Sql( @"
    DECLARE @PersonBadgeId INT = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid]='7FC986B9-CA1E-CBB7-4E63-C79EAC34F39D')
    DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')
    UPDATE [AttributeValue] SET [Value] = REPLACE(Value,'{{ Person.NickName}} become an eRA on','{{ Person.NickName}} became an eRA on') 
    WHERE [AttributeId] = @AttributeId AND [EntityId] = @PersonBadgeId
" );
            // JE: Rename 'Giving Analysis' -> 'Giving Analytics'
            Sql( @"
    UPDATE [BlockType] SET [Name] = 'Giving Analytics'
    WHERE [Path] = '~/Blocks/Finance/GivingAnalytics.ascx'
" );
            // DT: Fix GroupFinder Map values
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '9DA54BB6-986C-4723-8FE1-E3EF53119C6A' )
    UPDATE [AttributeValue] SET [Value] = 
	    REPLACE( REPLACE( REPLACE( [Value], 
		    '{% if LinkedPages.GroupDetailPage != '''' %}', '{% if LinkedPages.GroupDetailPage and LinkedPages.GroupDetailPage != '''' %}' ),
		    '{% if LinkedPages.RegisterPage != '''' %}', '{% if LinkedPages.RegisterPage and LinkedPages.RegisterPage != '''' %}' ),
		    '{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '''' %}', '{% if Location.FormattedHtmlAddress and Location.FormattedHtmlAddress != '''' %}' )
    WHERE [AttributeId] = @AttributeId
" );

            // JE: Update the CronExpression for Download Payments
            Sql( @"
    UPDATE [ServiceJob] SET [CronExpression] = '0 0 5 1/1 * ? *'
    WHERE [Guid] = '43044F38-F357-4CF4-995D-C60D4724C97E' 
    AND [CronExpression] = '0 0 5 ? * MON-FRI *'
" );

            //// From 037_MigrationRollupsForV6_10
            // DT: Fix Rock Instance Ids
            Sql( @"
    UPDATE [Attribute] SET [Guid] = NEWID()
    WHERE [Key] = 'RockInstanceId'
    AND [Guid] = '67873C5B-F11A-4F40-8209-730C22A0F6B5'
" );
            // MP: Fix up Auth ordering for Auths that have duplicate order values
            Sql( @"
    UPDATE A SET [Order] = r.OrderRowNum - 1
    FROM [Auth] A
    INNER JOIN (
        SELECT ROW_NUMBER() OVER ( PARTITION BY EntityTypeId,EntityId,[Action] ORDER BY EntityTypeId,EntityId,[Action],[Order],Id ) OrderRowNum
        ,*
        FROM Auth
        WHERE CONCAT ( EntityTypeId ,'_' ,EntityId ,'_' ,[action] ) IN (
            SELECT CONCAT ( EntityTypeId ,'_' ,EntityId ,'_' ,[action] )
            FROM auth
            GROUP BY EntityTypeId ,EntityId ,[action] ,[order]
            HAVING count(*) > 1
        )
    ) r ON r.Id = a.Id
    AND r.OrderRowNum - 1 != a.[Order]
" );
            // SK: Added RSR Finance roles to Contributions Edit Action
            Sql( @"
    DECLARE @pageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892')
    DECLARE @entityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

    IF NOT EXISTS ( 
	    SELECT [Id] FROM [Auth]
	    WHERE [EntityTypeId] = @entityTypeId
	    AND [EntityId] = @pageId
	    AND [Action] = 'Edit' )
    BEGIN
	    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId],[Guid] )
	    VALUES
	        (@entityTypeId , @pageId, 0, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='628C51A8-4613-43ED-A18D-4A6FB999273E'), '3F58750A-BBA6-4A51-B2C4-0DC2CA06313D'),
	        (@entityTypeId , @pageId, 1, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='6246A7EF-B7A3-4C8C-B1E4-3FF114B84559'), '4FFB36F4-6D8E-4348-990E-7B66B7D6D92A'),
	        (@entityTypeId , @pageId, 2, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9'), 'D5901875-4134-4E97-8C33-B9657FC81CAD'),
	        (@entityTypeId , @pageId, 3, 'Edit', 'D', 1, NULL, NEWID())
    END
" );

            // clear migration table
            Sql( @"
    UPDATE [__MigrationHistory] SET [Model] = 0x
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
