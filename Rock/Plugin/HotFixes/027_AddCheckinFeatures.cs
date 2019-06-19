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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add additional check-in features (check-out/auto-select/barcode check-in)
    /// .
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 27, "1.6.4" )]
    public class AddCheckinFeatures : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
// Moved to core migration: 201711271827181_V7Rollup
//            RockMigrationHelper.UpdateDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Scanned Id", "Search for family based on a barcode, proximity card, etc.", "7668CE15-E372-47EE-8FF8-6FEE09F7C858", true );
//            RockMigrationHelper.UpdateDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Family Id", "Search for family based on a Family Id", "111385BB-DAEB-4CE3-A945-0B50DC15EE02", true );

//            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Group", Rock.SystemGuid.FieldType.VALUE_LIST, "GroupTypeId", "10", "Check-in Identifiers", "One or more identifiers such as a barcode, or proximity card value that can be used during check-in.", 0, "", "8F528431-A438-4488-8DC3-CA42E66C1B37", "CheckinId" );

//            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "values", "0^Family,1^Person,2^Location,3^Check-Out", "E77DF4E6-A995-4C82-BBB7-DB57739D66F3" );

//            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypePurposeValueId", "", "Allow Checkout", "", 0, "False", "37EB8C83-A5DC-4A9B-8816-D93F07B2A7C5", "core_checkin_AllowCheckout" );
//            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.SINGLE_SELECT, "GroupTypePurposeValueId", "", "Auto Select Options", "", 0, "0", "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "core_checkin_AutoSelectOptions" );
//            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "fieldtype", "ddl", "F614DAFE-18BC-4FFE-A0D7-90A59DAF79AA" );
//            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "values", "0^People Only,1^People and Their Area/Group/Location", "542F273C-A5DB-4268-9D32-29DADA386E74" );

//            Sql( @"
//        DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
//        DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
//        IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
//        BEGIN

//            UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
//            WHERE[EntityTypeId] = @GroupTypeEntityTypeId
//            AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
//            AND[Key] LIKE 'core_checkin_%'

//        END
//" );

//            // Add the new action type entity for creating check-out labels
//            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.CreateCheckoutLabels", "83B13E96-A024-4ED1-9B2D-A76911139553", false, false );
//            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "83B13E96-A024-4ED1-9B2D-A76911139553", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50AB642C-9AD1-4973-9A6D-067F7714DFF3" ); // Rock.Workflow.Action.CheckIn.CreateCheckoutLabels:Active
//            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "83B13E96-A024-4ED1-9B2D-A76911139553", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "CFAC142C-73CB-4886-9A5A-4ED12C80A544" ); // Rock.Workflow.Action.CheckIn.CreateCheckoutLabels:Order

//            // Change the "Set Available Schedule" action in the "Person Search" activity to use the "Load Schedule" action instead.
//            Sql( @"
//    DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '24A7E196-B50B-4BD6-A347-07CFC5ABEF9E')
//    UPDATE [WorkflowActionType] SET [Name] = 'Load Schedules', [EntityTypeId] = @EntityTypeId WHERE [Guid] = '79CB608D-ED25-4526-A0F5-132D13642CDA'
//" );
//            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"True" ); // Unattended Check-in:Person Search:Load Schedules:Load All
//            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "F7B09469-EB3D-44A4-AB8E-C74318BD4669", @"" ); // Unattended Check-in:Person Search:Load Schedules:Order
//            RockMigrationHelper.AddActionTypeAttributeValue( "79CB608D-ED25-4526-A0F5-132D13642CDA", "4DFA9F8D-F2E6-4040-A23B-2A1F8258C767", @"False" ); // Unattended Check-in:Person Search:Load Schedules:Active

//            // Remove the "Preselect Recent Attendance" action from the "Person Search" activity
//            RockMigrationHelper.DeleteWorkflowActionType( "08D15C7A-4421-420A-BCA8-D6EE532E659F" ); 

//            // Add a new activity type for creating check-out labels
//            RockMigrationHelper.UpdateWorkflowActivityType( "011E9F5A-60D4-4FF5-912A-290881E37EAF", true, "Create Check-Out Labels", "Creates the labels to be printed during check-out", false, 8, "1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE" ); // Unattended Check-in:Create Check-Out Labels
//            RockMigrationHelper.UpdateWorkflowActionType( "1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE", "Create Labels", 0, "83B13E96-A024-4ED1-9B2D-A76911139553", true, false, "", "", 1, "", "B015DEB3-678A-478B-9DF4-AB4059C9A29B" ); // Unattended Check-in:Create Check-Out Labels:Create Labels
//            Sql( @"
//    DECLARE @WorkflowTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowType] WHERE [Guid] = '011E9F5A-60D4-4FF5-912A-290881E37EAF' )
//    DECLARE @Order int = (ISNULL(( SELECT MAX([Order]) FROM [WorkflowActivityType] WHERE [WorkflowTypeId] = @WorkflowTypeId ),-1) + 1)
//    UPDATE [WorkflowActivityType] SET [Order] = @Order WHERE [Guid] = '1AF64D9F-9AD2-4A2B-B8B3-8D079567AEAE'
//" );
//            RockMigrationHelper.AddActionTypeAttributeValue( "B015DEB3-678A-478B-9DF4-AB4059C9A29B", "CFAC142C-73CB-4886-9A5A-4ED12C80A544", @"" ); // Unattended Check-in:Create Check-Out Labels:Create Labels:Order
//            RockMigrationHelper.AddActionTypeAttributeValue( "B015DEB3-678A-478B-9DF4-AB4059C9A29B", "50AB642C-9AD1-4973-9A6D-067F7714DFF3", @"False" ); // Unattended Check-in:Create Check-Out Labels:Create Labels:Active


//            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Action Select", "", "0586648B-9490-43C6-B18D-7F403458C080", "" ); // Site:Rock Check-in
//            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Check Out Person Select", "", "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "" ); // Site:Rock Check-in
//            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Check Out Success", "", "21A855BA-6D68-4504-97B4-D787452CEC29", "" ); // Site:Rock Check-in

//            RockMigrationHelper.UpdateBlockType( "Action Select", "Displays option for family to Check In or Check Out.", "~/Blocks/CheckIn/ActionSelect.ascx", "Check-in", "66DDB050-8F60-4DF3-9AED-5CE283E22350" );
//            RockMigrationHelper.UpdateBlockType( "Check Out Person Select", "Lists people who match the selected family and provides option of selecting multiple people to check-out.", "~/Blocks/CheckIn/CheckOutPersonSelect.ascx", "Check-in", "54EB0252-6FE4-49C5-8716-14A3A06C3EC5" );
//            RockMigrationHelper.UpdateBlockType( "Check Out Success", "Displays the details of a successful check out.", "~/Blocks/CheckIn/CheckoutSuccess.ascx", "Check-in", "F499C4A9-9A60-404B-9383-B950EE6D7821" );

//            // Add Block to Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "0586648B-9490-43C6-B18D-7F403458C080", "", "66DDB050-8F60-4DF3-9AED-5CE283E22350", "Action Select", "Main", "", "", 0, "F5C21AE7-4BB4-4628-9B15-7CF761C66891" );
//            // Add Block to Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "", "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "Check Out Person Select", "Main", "", "", 0, "1F33702B-C26C-4EAA-AD0E-8510384EACBC" );
//            // Add Block to Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "21A855BA-6D68-4504-97B4-D787452CEC29", "", "F499C4A9-9A60-404B-9383-B950EE6D7821", "Check Out Success", "Main", "", "", 0, "32B345DD-0EF4-480E-B82A-7D7191CC374B" );

//            // Attrib for BlockType: Action Select:Workflow Type
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "31BAADBE-0E12-4EC4-B05D-472EBAD9C1B5" );
//            // Attrib for BlockType: Action Select:Workflow Activity
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "C0EEDB49-6B69-47B0-98DE-2A1A28188C5D" );
//            // Attrib for BlockType: Action Select:Home Page
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "DE27E0C8-5BEF-48FE-88D9-3E8300B4988E" );
//            // Attrib for BlockType: Action Select:Previous Page
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "5751A6B9-1155-4BAC-BA2D-84C6A419D6E7" );
//            // Attrib for BlockType: Action Select:Next Page
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "A161CC4A-F521-49A3-B648-165A7AE4EFE0" );
//            // Attrib for BlockType: Action Select:Next Page (Family Check-in)
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page (Family Check-in)", "FamilyNextPage", "", "", 5, @"", "83450920-66B3-46FD-AEA5-35EC43F96C9D" );
//            // Attrib for BlockType: Action Select:Check Out Page
//            RockMigrationHelper.AddBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Check Out Page", "CheckOutPage", "", "", 6, @"", "F70CFDEC-1131-4127-A6B8-A1A9AEE02D71" );

//            // Attrib for BlockType: Check Out Person Select:Workflow Type
//            RockMigrationHelper.AddBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A" );
//            // Attrib for BlockType: Check Out Person Select:Workflow Activity
//            RockMigrationHelper.AddBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "A4FB35E5-8A62-47FE-AE49-6E447DA8CF82" );
//            // Attrib for BlockType: Check Out Person Select:Home Page
//            RockMigrationHelper.AddBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "843FB186-90E8-4DCE-B138-B23E891E2CFF" );
//            // Attrib for BlockType: Check Out Person Select:Previous Page
//            RockMigrationHelper.AddBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "67C256C2-753F-410B-B683-F64368AC8497" );
//            // Attrib for BlockType: Check Out Person Select:Next Page
//            RockMigrationHelper.AddBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "18BAC460-2630-4651-A320-7927A3078A87" );

//            // Attrib for BlockType: Check Out Success:Workflow Type
//            RockMigrationHelper.AddBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "7328D95B-D9BB-49D0-943B-B374EBC664DD" );
//            // Attrib for BlockType: Check Out Success:Workflow Activity
//            RockMigrationHelper.AddBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "76C4F5AC-7EA8-45ED-8B7C-1974361FDEE5" );
//            // Attrib for BlockType: Check Out Success:Home Page
//            RockMigrationHelper.AddBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "ADA3C354-42ED-4F28-8F68-38FBC2926CBF" );
//            // Attrib for BlockType: Check Out Success:Previous Page
//            RockMigrationHelper.AddBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "735D4AAB-F8F4-4388-9A00-2132356187A6" );
//            // Attrib for BlockType: Check Out Success:Next Page
//            RockMigrationHelper.AddBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "223C5BA3-B6B0-4EC6-9D38-5607837410D6" );

//            // Attrib for BlockType: Welcome:Check-in Button Text
//            RockMigrationHelper.AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Check-in Button Text", "CheckinButtonText", "", "The text to display on the check-in button.", 7, @"", "C211328D-3F66-4F5D-902A-2A7AF1985209" );

//            // Attrib for BlockType: Person Select (Family Check-in):Auto Select Next Page
//            RockMigrationHelper.AddBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Auto Select Next Page", "AutoSelectNextPage", "", "The page to navigate to after selecting people in auto-select mode.", 5, @"", "4302646B-F6CD-492D-8850-96B9CA1CEA59" );
//            // Attrib for BlockType: Person Select (Family Check-in):Pre-Selected Options Format
//            RockMigrationHelper.AddBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Pre-Selected Options Format", "OptionFormat", "", "", 6, @"<span class='auto-select-schedule'>{{ Schedule.Name }}:</span>
//<span class='auto-select-group'>{{ Group.Name }}</span>
//<span class='auto-select-location'>{{ Location.Name }}</span>", "55580865-E792-469F-B45C-45713477D033" );

//            // Update Family Select Next Page to direct to Action Page
//            RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "90ECD00A-9570-4986-B32F-02F32B656A2A", @"0586648b-9490-43c6-b18d-7f403458c080" ); // Next Page

//            // Attrib Value for Block:Action Select, Attribute:Workflow Activity Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "C0EEDB49-6B69-47B0-98DE-2A1A28188C5D", @"" );
//            // Attrib Value for Block:Action Select, Attribute:Home Page Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "DE27E0C8-5BEF-48FE-88D9-3E8300B4988E", @"432b615a-75ff-4b14-9c99-3e769f866950" );
//            // Attrib Value for Block:Action Select, Attribute:Previous Page Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "5751A6B9-1155-4BAC-BA2D-84C6A419D6E7", @"10c97379-f719-4acb-b8c6-651957b660a4" );
//            // Attrib Value for Block:Action Select, Attribute:Next Page Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "A161CC4A-F521-49A3-B648-165A7AE4EFE0", @"bb8cf87f-680f-48f9-9147-f4951e033d17" );
//            // Attrib Value for Block:Action Select, Attribute:Next Page (Family Check-in) Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "83450920-66B3-46FD-AEA5-35EC43F96C9D", @"d14154ba-2f2c-41c3-b380-f833252cbb13" );
//            // Attrib Value for Block:Action Select, Attribute:Check Out Page Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "F70CFDEC-1131-4127-A6B8-A1A9AEE02D71", @"d54fc289-df7d-48c5-91be-38bcfdebc6af" );

//            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Type Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
//            // Attrib Value for Block:Check Out Person Select, Attribute:Workflow Activity Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "A4FB35E5-8A62-47FE-AE49-6E447DA8CF82", @"Create Check-Out Labels" );
//            // Attrib Value for Block:Check Out Person Select, Attribute:Home Page Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "843FB186-90E8-4DCE-B138-B23E891E2CFF", @"432b615a-75ff-4b14-9c99-3e769f866950" );
//            // Attrib Value for Block:Check Out Person Select, Attribute:Previous Page Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "67C256C2-753F-410B-B683-F64368AC8497", @"0586648b-9490-43c6-b18d-7f403458c080" );
//            // Attrib Value for Block:Check Out Person Select, Attribute:Next Page Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "18BAC460-2630-4651-A320-7927A3078A87", @"21a855ba-6d68-4504-97b4-d787452cec29" );

//            // Attrib Value for Block:Check Out Success, Attribute:Workflow Activity Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "32B345DD-0EF4-480E-B82A-7D7191CC374B", "76C4F5AC-7EA8-45ED-8B7C-1974361FDEE5", @"" );
//            // Attrib Value for Block:Check Out Success, Attribute:Home Page Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "32B345DD-0EF4-480E-B82A-7D7191CC374B", "ADA3C354-42ED-4F28-8F68-38FBC2926CBF", @"432b615a-75ff-4b14-9c99-3e769f866950" );
//            // Attrib Value for Block:Check Out Success, Attribute:Previous Page Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "32B345DD-0EF4-480E-B82A-7D7191CC374B", "735D4AAB-F8F4-4388-9A00-2132356187A6", @"d54fc289-df7d-48c5-91be-38bcfdebc6af" );

//            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "4302646B-F6CD-492D-8850-96B9CA1CEA59", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" ); // Auto Select Next Page
//            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "55580865-E792-469F-B45C-45713477D033", @"{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }}" ); // Pre-Selected Options Format

//            // Add Block to Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "0586648B-9490-43C6-B18D-7F403458C080", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "7A293980-9E28-4115-85EB-DA197734EED2" );
//            // Add Block to Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "D54FC289-DF7D-48C5-91BE-38BCFDEBC6AF", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "258CC6B9-CA88-41E7-B578-2514FCF245B4" );
//            // Add Block to Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlock( "21A855BA-6D68-4504-97B4-D787452CEC29", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Redirect", "Main", "", "", 1, "04BF66EF-66E5-465D-A590-D8BA02E217B7" );
//            // update block order for pages with new blocks if the page,zone has multiple blocks

//            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "7A293980-9E28-4115-85EB-DA197734EED2", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
//            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Action Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "7A293980-9E28-4115-85EB-DA197734EED2", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
//            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "258CC6B9-CA88-41E7-B578-2514FCF245B4", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
//            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Person Select, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "258CC6B9-CA88-41E7-B578-2514FCF245B4", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
//            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "04BF66EF-66E5-465D-A590-D8BA02E217B7", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
//            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Check Out Success, Site: Rock Check-in
//            RockMigrationHelper.AddBlockAttributeValue( "04BF66EF-66E5-465D-A590-D8BA02E217B7", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );

//            Sql( @"
//update DataViewFilter
//set selection = '[
//	""SecondaryAudiences"",
//    ""8"",
//    ""b364cdee-f000-4965-ae67-0c80dda365dc""
//]' 
//where selection = '[
//    ""SecondaryAudiences"",
//	""8"",
//	""4cdee-f000-4965-ae67-0c80dda365dc\""\""
//]'

//update DataViewFilter
//set selection = '[
//    ""SecondaryAudiences"",
//	""8"",
//	""57b2a23f-3b0c-43a8-9f45-332120dcd0ee""
//]' 
//where selection = '[
//    ""SecondaryAudiences"",
//	""8"",
//	""2a23f-3b0c-43a8-9f45-332120dcd0ee\""\"",
//]'" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
