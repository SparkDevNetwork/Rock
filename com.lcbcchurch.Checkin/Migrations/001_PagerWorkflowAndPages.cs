// <copyright>
// Copyright by Central Christian Church
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class PagerWorkflowAndPages : Migration
    {
        public override void Up()
        {
            // Add new entity attributes
            RockMigrationHelper.AddGroupTypeGroupAttribute( "0572A5FE-20A4-4BF1-95CD-C71DB5281392", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Are Pagers Offered", "", 3, "False", "AF8D7EA5-5A57-4796-95E4-3248FF866731" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Device", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Allow Pagers to be Offered?", "", "", 1007, "False", "482417C4-67F4-472B-82CC-A6D5CAB3C6D3", "AllowPagerstobeOffered" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Attendance", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Pager Number", "", "", 1008, "", "791A4DC9-BB89-41E6-95E9-D377ED4C2F0B" );

            // Add Workflow
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "com.lcbcchurch.Checkin.Workflow.Action.CheckIn.SaveAttendanceAndPagerNumber", "893F3D2D-DE81-4F9A-8651-E60E3F36BF6C", false, true );

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "893F3D2D-DE81-4F9A-8651-E60E3F36BF6C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "8F6F9634-3387-4627-BD62-F634ED4CAF14" ); // com.lcbcchurch.Checkin.Workflow.Action.CheckIn.SaveAttendanceAndPagerNumber:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "893F3D2D-DE81-4F9A-8651-E60E3F36BF6C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "969E4130-1FB5-4908-A041-ACF14FFD9F68" ); // com.lcbcchurch.Checkin.Workflow.Action.CheckIn.SaveAttendanceAndPagerNumber:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Check-in", "fa fa-check-square", "", "8F8B272D-D351-485E-86D6-3EE5B7C84D99", 0 ); // Check-in

            #endregion

            #region LCBC Unattended Check-in

            RockMigrationHelper.UpdateWorkflowType( false, true, "LCBC Unattended Check-in", "", "8F8B272D-D351-485E-86D6-3EE5B7C84D99", "Check-in", "fa fa-list-ol", 0, true, 3, "A0BBC045-00E5-4485-88CB-69A73AC7C78D", 0 ); // LCBC Unattended Check-in
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override", "Override", "Used to enable age/grade override.", 0, @"False", "C3A358CE-9933-430B-9540-9118958891BB", false ); // LCBC Unattended Check-in:Override
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Family Search", "", false, 0, "269AB396-149F-40C9-A9C8-2A8D2A5E3726" ); // LCBC Unattended Check-in:Family Search
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Person Search", "", false, 1, "6231233E-11C4-4FF3-8F71-F01601095722" ); // LCBC Unattended Check-in:Person Search
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Ability Level Search", "", false, 2, "A7BC860C-381E-4466-A21C-D1D95AEAB3D8" ); // LCBC Unattended Check-in:Ability Level Search
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Location Search", "", false, 3, "3A535A76-82CF-4ECA-97ED-E22F0A63F704" ); // LCBC Unattended Check-in:Location Search
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Schedule Search", "", false, 4, "C984812B-0D58-4B53-A4B6-A73616E4127B" ); // LCBC Unattended Check-in:Schedule Search
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Save Attendance", "", false, 5, "D271B9C9-FC75-4958-8AE5-B6118C806B52" ); // LCBC Unattended Check-in:Save Attendance
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Load Schedules", "Loads schedules for Family Check-In", false, 6, "2B0B1D03-9397-4EAD-B9E2-778DE0E66328" ); // LCBC Unattended Check-in:Load Schedules
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Schedule Select", "Filters the locations, groups, and group types by the selected schedule(s) in Family Check-In", false, 7, "BD2A2CFE-F92F-4028-827D-9E2C768686C2" ); // LCBC Unattended Check-in:Schedule Select
            RockMigrationHelper.UpdateWorkflowActivityType( "A0BBC045-00E5-4485-88CB-69A73AC7C78D", true, "Create Check-Out Labels", "Creates the labels to be printed during check-out", false, 8, "EBE5CA5D-109D-4B72-BA7C-DBC51B1CB55C" ); // LCBC Unattended Check-in:Create Check-Out Labels
            RockMigrationHelper.UpdateWorkflowActionType( "269AB396-149F-40C9-A9C8-2A8D2A5E3726", "Find Families", 0, "E2F172A8-88E5-4F84-9805-73164516F5FB", true, false, "", "", 1, "", "C73A232A-531C-4844-903D-9D8DFD3BE84F" ); // LCBC Unattended Check-in:Family Search:Find Families
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Find Family Members", 0, "5794B18B-8F43-43B2-8D60-6C047AB096AF", true, false, "", "", 1, "", "57CFB32F-2342-405C-9577-F83CBC73B8D8" ); // LCBC Unattended Check-in:Person Search:Find Family Members
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Find Relationships", 1, "F43099A7-E872-439B-A750-351C741BCFEF", true, false, "", "", 1, "", "F3D1539F-B7DA-4279-9E67-33037484FC69" ); // LCBC Unattended Check-in:Person Search:Find Relationships
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Load Group Types", 2, "50D5D915-074A-41FB-9EA7-0DBE52141398", true, false, "", "", 1, "", "1AB818AF-CFE8-42C9-9C9E-1E3A6C595501" ); // LCBC Unattended Check-in:Person Search:Load Group Types
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter GroupTypes by Age", 3, "3A77A36E-D613-44F7-ACA1-34666A85CD07", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "D3C20C06-F240-4DB6-9C34-AC5D675AEA1A" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Age
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter GroupTypes by Grade", 4, "BA1D77F3-CC92-4C1F-8DC5-AEBADF114E74", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "5206A1CD-C632-4BC7-A602-BB9920A8B847" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Grade
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Load Groups", 5, "008402A8-3A6C-4CB6-A230-6AD532505EDC", true, false, "", "", 1, "", "E6287756-9A79-4D16-A29F-BBF402833DD1" ); // LCBC Unattended Check-in:Person Search:Load Groups
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter Groups by Age", 6, "23F1E3FD-48AE-451F-9911-A5C7523A74B6", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "8BD38570-C881-4CC0-9096-EBF7840BD1EE" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter Groups by Grade", 7, "542DFBF4-F2F5-4C2F-9C9D-CDB00FDD5F04", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "214434DB-B91A-41B3-92A8-2A56CD79E993" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Grade
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter Groups by Gender", 8, "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "F867DFB5-B601-4765-8B59-B1DAE10D54BB" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Gender
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Load Locations", 9, "4492E36A-77C8-4DC7-8128-570FAA161ADB", true, false, "", "", 1, "", "A6ED709E-5F41-4286-8B64-CF1DD36EB424" ); // LCBC Unattended Check-in:Person Search:Load Locations
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Filter Locations by Threshold", 10, "EB9E5114-D86D-49CF-89A1-6EF52428AD2E", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "89AD4F85-A8BA-459F-B10E-F688E4A69340" ); // LCBC Unattended Check-in:Person Search:Filter Locations by Threshold
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Remove Empty Groups", 11, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 1, "", "E8EA91B8-6BAE-431A-BB67-4FC85E5B65EB" ); // LCBC Unattended Check-in:Person Search:Remove Empty Groups
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Remove Empty Group Types", 12, "E998B9A7-31C9-46F6-B91C-4E5C3F06C82F", true, false, "", "", 1, "", "173134ED-84C2-4DB8-A03F-FE842D32033B" ); // LCBC Unattended Check-in:Person Search:Remove Empty Group Types
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Remove Empty People", 13, "B8B72812-190E-4802-A63F-E693344754BD", true, false, "", "", 1, "", "2143804C-E810-478E-A315-CE32A40DA65D" ); // LCBC Unattended Check-in:Person Search:Remove Empty People
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Load Schedules", 14, "24A7E196-B50B-4BD6-A347-07CFC5ABEF9E", true, false, "", "", 1, "", "F5E174D7-6BC3-4347-A983-A32026F08E80" ); // LCBC Unattended Check-in:Person Search:Load Schedules
            RockMigrationHelper.UpdateWorkflowActionType( "6231233E-11C4-4FF3-8F71-F01601095722", "Update Last Attended", 15, "A2216790-9699-4213-8EB2-DDDCA54F2C03", true, false, "", "", 1, "", "58133AEE-0738-483A-B477-A7DDFF111D2A" ); // LCBC Unattended Check-in:Person Search:Update Last Attended
            RockMigrationHelper.UpdateWorkflowActionType( "A7BC860C-381E-4466-A21C-D1D95AEAB3D8", "Filter Groups By Ability Level", 0, "54BF0279-1FBB-4537-A933-2BAD48C43063", true, false, "", "", 1, "", "1B0F6550-1A65-48F4-9EC6-D4B7F6A133D3" ); // LCBC Unattended Check-in:Ability Level Search:Filter Groups By Ability Level
            RockMigrationHelper.UpdateWorkflowActionType( "A7BC860C-381E-4466-A21C-D1D95AEAB3D8", "Filter Active Locations", 1, "7BB371F9-A8DE-49D3-BEEA-C191F6C7D4A0", true, false, "", "", 1, "", "7E672C13-4BA6-49C3-B9A5-D3645F8C8C60" ); // LCBC Unattended Check-in:Ability Level Search:Filter Active Locations
            RockMigrationHelper.UpdateWorkflowActionType( "A7BC860C-381E-4466-A21C-D1D95AEAB3D8", "Remove Empty Groups", 2, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 1, "", "638428EC-DDCD-4270-85B0-6692462F2D4D" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Groups
            RockMigrationHelper.UpdateWorkflowActionType( "A7BC860C-381E-4466-A21C-D1D95AEAB3D8", "Remove Empty Group Types", 3, "E998B9A7-31C9-46F6-B91C-4E5C3F06C82F", true, false, "", "", 1, "", "5C072761-D694-4E6A-8531-B3A3B810BB1C" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Group Types
            RockMigrationHelper.UpdateWorkflowActionType( "A7BC860C-381E-4466-A21C-D1D95AEAB3D8", "Set Available Schedules", 4, "0F16E0C5-825A-4058-8285-6370DAAC2C19", true, false, "", "", 1, "", "0017E13E-B5B1-4C0E-A91D-36F424F9FAFE" ); // LCBC Unattended Check-in:Ability Level Search:Set Available Schedules
            RockMigrationHelper.UpdateWorkflowActionType( "3A535A76-82CF-4ECA-97ED-E22F0A63F704", "Load Locations", 0, "4492E36A-77C8-4DC7-8128-570FAA161ADB", true, false, "", "", 1, "", "8DA7A38A-9FEC-4E6D-B1B6-1EA6C47113D5" ); // LCBC Unattended Check-in:Location Search:Load Locations
            RockMigrationHelper.UpdateWorkflowActionType( "3A535A76-82CF-4ECA-97ED-E22F0A63F704", "Filter Active Locations", 1, "7BB371F9-A8DE-49D3-BEEA-C191F6C7D4A0", true, false, "", "", 1, "", "7859C094-8597-4016-B070-DA9DF660F948" ); // LCBC Unattended Check-in:Location Search:Filter Active Locations
            RockMigrationHelper.UpdateWorkflowActionType( "3A535A76-82CF-4ECA-97ED-E22F0A63F704", "Remove Empty Groups", 2, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 1, "", "EC3377EC-85EE-4D4B-9923-22FB4F5FD3B2" ); // LCBC Unattended Check-in:Location Search:Remove Empty Groups
            RockMigrationHelper.UpdateWorkflowActionType( "3A535A76-82CF-4ECA-97ED-E22F0A63F704", "Update Last Attended", 3, "A2216790-9699-4213-8EB2-DDDCA54F2C03", true, false, "", "", 1, "", "4B2AE3C4-8F74-41E7-8200-A4D6139B89DC" ); // LCBC Unattended Check-in:Location Search:Update Last Attended
            RockMigrationHelper.UpdateWorkflowActionType( "C984812B-0D58-4B53-A4B6-A73616E4127B", "Load Schedules", 0, "24A7E196-B50B-4BD6-A347-07CFC5ABEF9E", true, false, "", "", 1, "", "692D3C1D-9A10-4125-A79F-347F9FEFE262" ); // LCBC Unattended Check-in:Schedule Search:Load Schedules
            RockMigrationHelper.UpdateWorkflowActionType( "D271B9C9-FC75-4958-8AE5-B6118C806B52", "Save Attendance", 0, "893F3D2D-DE81-4F9A-8651-E60E3F36BF6C", true, false, "", "", 1, "", "83332011-177C-4241-9B11-854CA94D554F" ); // LCBC Unattended Check-in:Save Attendance:Save Attendance
            RockMigrationHelper.UpdateWorkflowActionType( "D271B9C9-FC75-4958-8AE5-B6118C806B52", "Create Labels", 1, "8F348E7B-F9FD-4600-852D-477B13B0B4EE", true, false, "", "", 1, "", "0F8B5072-7B9C-40A7-84F5-130107B57D94" ); // LCBC Unattended Check-in:Save Attendance:Create Labels
            RockMigrationHelper.UpdateWorkflowActionType( "2B0B1D03-9397-4EAD-B9E2-778DE0E66328", "Load Schedules", 0, "24A7E196-B50B-4BD6-A347-07CFC5ABEF9E", true, false, "", "", 1, "", "786E6C31-3748-4FD0-AD62-D4CF7BDC6B98" ); // LCBC Unattended Check-in:Load Schedules:Load Schedules
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Filter Locations By Schedule", 0, "DC86310C-44CF-44F5-804E-5085A29F5AAE", true, false, "", "", 1, "", "4086CA26-75F4-494B-B4FF-99A491A42096" ); // LCBC Unattended Check-in:Schedule Select:Filter Locations By Schedule
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Remove Empty Locations", 1, "51AE2690-ED00-423D-86AD-6E97054F04A9", true, false, "", "", 1, "", "2EAFD0E1-CE78-4EF5-AB2C-4071E5BEC01B" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Locations
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Remove Empty Groups", 2, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 1, "", "CB6BE78F-3B1F-4185-8029-FEBDD3EAD8BC" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Groups
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Remove Empty Group Types", 3, "E998B9A7-31C9-46F6-B91C-4E5C3F06C82F", true, false, "", "", 1, "", "5A1F4FA1-D0B6-44DC-A871-444E5BAD357D" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Group Types
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Set Available Schedules", 4, "0F16E0C5-825A-4058-8285-6370DAAC2C19", true, false, "", "", 1, "", "10B3F5FD-6F0C-4818-AEC3-F15AC7587E8B" ); // LCBC Unattended Check-in:Schedule Select:Set Available Schedules
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A2CFE-F92F-4028-827D-9E2C768686C2", "Remove Previous Checkins", 5, "5151FB64-35C6-48B3-ACCF-959BAD3A31CA", true, false, "", "C3A358CE-9933-430B-9540-9118958891BB", 1, "False", "833FF74B-76C6-4735-BA78-EBA04E873AFD" ); // LCBC Unattended Check-in:Schedule Select:Remove Previous Checkins
            RockMigrationHelper.UpdateWorkflowActionType( "EBE5CA5D-109D-4B72-BA7C-DBC51B1CB55C", "Create Labels", 0, "83B13E96-A024-4ED1-9B2D-A76911139553", true, false, "", "", 1, "", "B3AFF4FD-44DB-49D8-8ED2-C2903C45AD23" ); // LCBC Unattended Check-in:Create Check-Out Labels:Create Labels
            RockMigrationHelper.AddActionTypeAttributeValue( "C73A232A-531C-4844-903D-9D8DFD3BE84F", "3404112D-3A97-4AE8-B699-07F62BD37D81", @"" ); // LCBC Unattended Check-in:Family Search:Find Families:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C73A232A-531C-4844-903D-9D8DFD3BE84F", "1C6D8BD4-1A72-41E7-A9B5-AF37613058D8", @"False" ); // LCBC Unattended Check-in:Family Search:Find Families:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "57CFB32F-2342-405C-9577-F83CBC73B8D8", "857A277E-6824-48FA-8E7A-9988AC4BCB13", @"" ); // LCBC Unattended Check-in:Person Search:Find Family Members:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "57CFB32F-2342-405C-9577-F83CBC73B8D8", "3EF34D41-030B-411F-9D18-D331ABD89F0D", @"False" ); // LCBC Unattended Check-in:Person Search:Find Family Members:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F3D1539F-B7DA-4279-9E67-33037484FC69", "2C5535C6-80C9-4886-9A93-33A18F46AAA3", @"" ); // LCBC Unattended Check-in:Person Search:Find Relationships:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F3D1539F-B7DA-4279-9E67-33037484FC69", "6845038E-A08E-4D0A-BE1C-750034109496", @"False" ); // LCBC Unattended Check-in:Person Search:Find Relationships:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1AB818AF-CFE8-42C9-9C9E-1E3A6C595501", "1F4BD3F6-C528-4160-8478-825C3B8AC85D", @"" ); // LCBC Unattended Check-in:Person Search:Load Group Types:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1AB818AF-CFE8-42C9-9C9E-1E3A6C595501", "1C7CD28E-ACC5-4B88-BC05-E02D72919305", @"False" ); // LCBC Unattended Check-in:Person Search:Load Group Types:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D3C20C06-F240-4DB6-9C34-AC5D675AEA1A", "497F8DDB-345E-404E-9B5D-CE555EB9A572", @"" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Age:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D3C20C06-F240-4DB6-9C34-AC5D675AEA1A", "161E6748-85AE-49D8-B5BA-8135F27232FB", @"False" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Age:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D3C20C06-F240-4DB6-9C34-AC5D675AEA1A", "DFFC0499-A352-40F5-9C49-143FAC0E1475", @"True" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Age:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "5206A1CD-C632-4BC7-A602-BB9920A8B847", "7D791CDE-C9F1-45B6-A213-DB2511A69E1B", @"" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Grade:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5206A1CD-C632-4BC7-A602-BB9920A8B847", "3EC91119-FC4F-475F-B8E3-8666E86FFDF7", @"False" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Grade:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5206A1CD-C632-4BC7-A602-BB9920A8B847", "1667766D-2A03-4129-AF8D-F88D0821A074", @"True" ); // LCBC Unattended Check-in:Person Search:Filter GroupTypes by Grade:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "E6287756-9A79-4D16-A29F-BBF402833DD1", "39762EF0-91D5-4B13-BD34-FC3AC3C24897", @"True" ); // LCBC Unattended Check-in:Person Search:Load Groups:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "E6287756-9A79-4D16-A29F-BBF402833DD1", "C26C5959-7144-443B-88ED-28E4A5AE544C", @"" ); // LCBC Unattended Check-in:Person Search:Load Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E6287756-9A79-4D16-A29F-BBF402833DD1", "AD7528AD-2A3D-4C26-B452-FA9F4F48953C", @"False" ); // LCBC Unattended Check-in:Person Search:Load Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8BD38570-C881-4CC0-9096-EBF7840BD1EE", "410FF40E-EB74-415C-B5F5-9B31F5F4F1EA", @"" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8BD38570-C881-4CC0-9096-EBF7840BD1EE", "5FC15614-2DA7-41D8-B095-48DF17A974B6", @"False" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8BD38570-C881-4CC0-9096-EBF7840BD1EE", "16020443-CE2E-41B8-BFE9-2E1AA4E6E07C", @"True" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "8BD38570-C881-4CC0-9096-EBF7840BD1EE", "7F505AD9-D348-4088-91C0-F8FDB28AE39A", @"43511b8f-71d9-423a-85bf-d1cd08c1998e" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age:Group Age Range Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "8BD38570-C881-4CC0-9096-EBF7840BD1EE", "C7E69256-BA4F-436E-B1F3-B5B6B101C523", @"f1a43eab-d682-403f-a05e-ccffbf879f32" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Age:Group Birthdate Range Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "214434DB-B91A-41B3-92A8-2A56CD79E993", "78A19926-CABA-475D-8AB9-F9EB08BD6FAA", @"" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Grade:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "214434DB-B91A-41B3-92A8-2A56CD79E993", "E69E09E8-C71D-4AA3-86AD-D46DBAE60816", @"False" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Grade:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "214434DB-B91A-41B3-92A8-2A56CD79E993", "39F82A06-DD0E-4D51-A364-8D6C0EB32BC4", @"True" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Grade:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "F867DFB5-B601-4765-8B59-B1DAE10D54BB", "6EC3B2A5-E962-476F-8052-B795AE2ECEF3", @"False" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Gender:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F867DFB5-B601-4765-8B59-B1DAE10D54BB", "81957147-9C2D-424E-A9B7-386A72937892", @"" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Gender:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F867DFB5-B601-4765-8B59-B1DAE10D54BB", "1E1F7819-D6EA-4F0B-B30B-90429BDA9808", @"True" ); // LCBC Unattended Check-in:Person Search:Filter Groups by Gender:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "A6ED709E-5F41-4286-8B64-CF1DD36EB424", "70203A96-AE70-47AD-A086-FD84792DF2B6", @"True" ); // LCBC Unattended Check-in:Person Search:Load Locations:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "A6ED709E-5F41-4286-8B64-CF1DD36EB424", "6EE6128C-79BF-4333-85DB-3B0C92B27131", @"" ); // LCBC Unattended Check-in:Person Search:Load Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A6ED709E-5F41-4286-8B64-CF1DD36EB424", "2F3B6B42-A89C-443A-9008-E9E96535E815", @"False" ); // LCBC Unattended Check-in:Person Search:Load Locations:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "89AD4F85-A8BA-459F-B10E-F688E4A69340", "62AACC4A-E976-43F5-8828-A8D2D1AC5D1B", @"False" ); // LCBC Unattended Check-in:Person Search:Filter Locations by Threshold:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "89AD4F85-A8BA-459F-B10E-F688E4A69340", "097966C4-8FE0-4464-A340-C3E78B0D3693", @"True" ); // LCBC Unattended Check-in:Person Search:Filter Locations by Threshold:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "89AD4F85-A8BA-459F-B10E-F688E4A69340", "3C6C2ABA-8705-4674-9BB5-F07AEBC38EDC", @"" ); // LCBC Unattended Check-in:Person Search:Filter Locations by Threshold:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E8EA91B8-6BAE-431A-BB67-4FC85E5B65EB", "041E4A2B-90C6-4242-A7F1-ED07D9B348F2", @"" ); // LCBC Unattended Check-in:Person Search:Remove Empty Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E8EA91B8-6BAE-431A-BB67-4FC85E5B65EB", "05C329B0-3794-42BD-9467-8F3FF95D7882", @"False" ); // LCBC Unattended Check-in:Person Search:Remove Empty Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "173134ED-84C2-4DB8-A03F-FE842D32033B", "4D8C38AC-A58E-49DF-BA53-D28EC00A841A", @"" ); // LCBC Unattended Check-in:Person Search:Remove Empty Group Types:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "173134ED-84C2-4DB8-A03F-FE842D32033B", "20941740-7907-42EE-9250-40EEE2C30D75", @"False" ); // LCBC Unattended Check-in:Person Search:Remove Empty Group Types:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2143804C-E810-478E-A315-CE32A40DA65D", "CFDAD883-5FAA-4EC6-B308-30BBB2EFAA94", @"" ); // LCBC Unattended Check-in:Person Search:Remove Empty People:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "2143804C-E810-478E-A315-CE32A40DA65D", "EE892293-5B1E-4631-877E-179849F8D0FC", @"False" ); // LCBC Unattended Check-in:Person Search:Remove Empty People:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F5E174D7-6BC3-4347-A983-A32026F08E80", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"True" ); // LCBC Unattended Check-in:Person Search:Load Schedules:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "F5E174D7-6BC3-4347-A983-A32026F08E80", "F7B09469-EB3D-44A4-AB8E-C74318BD4669", @"" ); // LCBC Unattended Check-in:Person Search:Load Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F5E174D7-6BC3-4347-A983-A32026F08E80", "4DFA9F8D-F2E6-4040-A23B-2A1F8258C767", @"False" ); // LCBC Unattended Check-in:Person Search:Load Schedules:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "58133AEE-0738-483A-B477-A7DDFF111D2A", "EA5AE300-CC75-4DD1-ADCF-BEAAF71B0F4F", @"" ); // LCBC Unattended Check-in:Person Search:Update Last Attended:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "58133AEE-0738-483A-B477-A7DDFF111D2A", "21440E98-D3E9-427E-AB13-65C75D61EA22", @"False" ); // LCBC Unattended Check-in:Person Search:Update Last Attended:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1B0F6550-1A65-48F4-9EC6-D4B7F6A133D3", "85C89ADB-3B22-4F67-836C-892F9796BD34", @"" ); // LCBC Unattended Check-in:Ability Level Search:Filter Groups By Ability Level:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1B0F6550-1A65-48F4-9EC6-D4B7F6A133D3", "3F841ECB-9506-48F2-A89B-8C52C36D02E3", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Filter Groups By Ability Level:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1B0F6550-1A65-48F4-9EC6-D4B7F6A133D3", "2FBA7E72-3EC1-4C77-83D8-71DF53E113C4", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Filter Groups By Ability Level:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "7E672C13-4BA6-49C3-B9A5-D3645F8C8C60", "C8BE5BB1-9293-4FA0-B4CF-FED19B855465", @"" ); // LCBC Unattended Check-in:Ability Level Search:Filter Active Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7E672C13-4BA6-49C3-B9A5-D3645F8C8C60", "D6BCB113-0699-4D58-8002-BC919CB4BA04", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Filter Active Locations:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7E672C13-4BA6-49C3-B9A5-D3645F8C8C60", "885D28C5-A395-4A05-AEFB-6131498BDF12", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Filter Active Locations:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "638428EC-DDCD-4270-85B0-6692462F2D4D", "041E4A2B-90C6-4242-A7F1-ED07D9B348F2", @"" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "638428EC-DDCD-4270-85B0-6692462F2D4D", "05C329B0-3794-42BD-9467-8F3FF95D7882", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5C072761-D694-4E6A-8531-B3A3B810BB1C", "4D8C38AC-A58E-49DF-BA53-D28EC00A841A", @"" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Group Types:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5C072761-D694-4E6A-8531-B3A3B810BB1C", "20941740-7907-42EE-9250-40EEE2C30D75", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Remove Empty Group Types:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0017E13E-B5B1-4C0E-A91D-36F424F9FAFE", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975", @"False" ); // LCBC Unattended Check-in:Ability Level Search:Set Available Schedules:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0017E13E-B5B1-4C0E-A91D-36F424F9FAFE", "B6256274-8971-4DA2-9144-ED732B46EC5B", @"" ); // LCBC Unattended Check-in:Ability Level Search:Set Available Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8DA7A38A-9FEC-4E6D-B1B6-1EA6C47113D5", "70203A96-AE70-47AD-A086-FD84792DF2B6", @"False" ); // LCBC Unattended Check-in:Location Search:Load Locations:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "8DA7A38A-9FEC-4E6D-B1B6-1EA6C47113D5", "6EE6128C-79BF-4333-85DB-3B0C92B27131", @"" ); // LCBC Unattended Check-in:Location Search:Load Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8DA7A38A-9FEC-4E6D-B1B6-1EA6C47113D5", "2F3B6B42-A89C-443A-9008-E9E96535E815", @"False" ); // LCBC Unattended Check-in:Location Search:Load Locations:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7859C094-8597-4016-B070-DA9DF660F948", "C8BE5BB1-9293-4FA0-B4CF-FED19B855465", @"" ); // LCBC Unattended Check-in:Location Search:Filter Active Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7859C094-8597-4016-B070-DA9DF660F948", "D6BCB113-0699-4D58-8002-BC919CB4BA04", @"False" ); // LCBC Unattended Check-in:Location Search:Filter Active Locations:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7859C094-8597-4016-B070-DA9DF660F948", "885D28C5-A395-4A05-AEFB-6131498BDF12", @"False" ); // LCBC Unattended Check-in:Location Search:Filter Active Locations:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "EC3377EC-85EE-4D4B-9923-22FB4F5FD3B2", "041E4A2B-90C6-4242-A7F1-ED07D9B348F2", @"" ); // LCBC Unattended Check-in:Location Search:Remove Empty Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EC3377EC-85EE-4D4B-9923-22FB4F5FD3B2", "05C329B0-3794-42BD-9467-8F3FF95D7882", @"False" ); // LCBC Unattended Check-in:Location Search:Remove Empty Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4B2AE3C4-8F74-41E7-8200-A4D6139B89DC", "EA5AE300-CC75-4DD1-ADCF-BEAAF71B0F4F", @"" ); // LCBC Unattended Check-in:Location Search:Update Last Attended:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4B2AE3C4-8F74-41E7-8200-A4D6139B89DC", "21440E98-D3E9-427E-AB13-65C75D61EA22", @"False" ); // LCBC Unattended Check-in:Location Search:Update Last Attended:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "692D3C1D-9A10-4125-A79F-347F9FEFE262", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"False" ); // LCBC Unattended Check-in:Schedule Search:Load Schedules:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "692D3C1D-9A10-4125-A79F-347F9FEFE262", "F7B09469-EB3D-44A4-AB8E-C74318BD4669", @"" ); // LCBC Unattended Check-in:Schedule Search:Load Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "692D3C1D-9A10-4125-A79F-347F9FEFE262", "4DFA9F8D-F2E6-4040-A23B-2A1F8258C767", @"False" ); // LCBC Unattended Check-in:Schedule Search:Load Schedules:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "83332011-177C-4241-9B11-854CA94D554F", "969E4130-1FB5-4908-A041-ACF14FFD9F68", @"" ); // LCBC Unattended Check-in:Save Attendance:Save Attendance:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "83332011-177C-4241-9B11-854CA94D554F", "8F6F9634-3387-4627-BD62-F634ED4CAF14", @"False" ); // LCBC Unattended Check-in:Save Attendance:Save Attendance:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0F8B5072-7B9C-40A7-84F5-130107B57D94", "F70112C9-4D93-41B9-A3FB-1E7C866AACCF", @"" ); // LCBC Unattended Check-in:Save Attendance:Create Labels:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0F8B5072-7B9C-40A7-84F5-130107B57D94", "36EB15CE-095C-41ED-9C0F-9EA345599D54", @"False" ); // LCBC Unattended Check-in:Save Attendance:Create Labels:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "786E6C31-3748-4FD0-AD62-D4CF7BDC6B98", "B222CAF2-DF12-433C-B5D4-A8DB95B60207", @"True" ); // LCBC Unattended Check-in:Load Schedules:Load Schedules:Load All
            RockMigrationHelper.AddActionTypeAttributeValue( "786E6C31-3748-4FD0-AD62-D4CF7BDC6B98", "F7B09469-EB3D-44A4-AB8E-C74318BD4669", @"" ); // LCBC Unattended Check-in:Load Schedules:Load Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "786E6C31-3748-4FD0-AD62-D4CF7BDC6B98", "4DFA9F8D-F2E6-4040-A23B-2A1F8258C767", @"False" ); // LCBC Unattended Check-in:Load Schedules:Load Schedules:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4086CA26-75F4-494B-B4FF-99A491A42096", "5E590D32-9101-457D-9296-4FED6EA992F4", @"False" ); // LCBC Unattended Check-in:Schedule Select:Filter Locations By Schedule:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4086CA26-75F4-494B-B4FF-99A491A42096", "4C6ACFAD-F94B-43F7-AA7C-FEF48EFAA79C", @"False" ); // LCBC Unattended Check-in:Schedule Select:Filter Locations By Schedule:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "4086CA26-75F4-494B-B4FF-99A491A42096", "874AC9A3-CA6D-4C1F-8CCA-BCB7BFC74C19", @"" ); // LCBC Unattended Check-in:Schedule Select:Filter Locations By Schedule:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "2EAFD0E1-CE78-4EF5-AB2C-4071E5BEC01B", "5E8DBE2C-DD06-43C1-A587-3F3DA4423964", @"False" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Locations:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2EAFD0E1-CE78-4EF5-AB2C-4071E5BEC01B", "9CA7DD25-422D-4859-B8B4-C2A44293D485", @"" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Locations:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "CB6BE78F-3B1F-4185-8029-FEBDD3EAD8BC", "041E4A2B-90C6-4242-A7F1-ED07D9B348F2", @"" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Groups:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "CB6BE78F-3B1F-4185-8029-FEBDD3EAD8BC", "05C329B0-3794-42BD-9467-8F3FF95D7882", @"False" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Groups:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1F4FA1-D0B6-44DC-A871-444E5BAD357D", "4D8C38AC-A58E-49DF-BA53-D28EC00A841A", @"" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Group Types:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1F4FA1-D0B6-44DC-A871-444E5BAD357D", "20941740-7907-42EE-9250-40EEE2C30D75", @"False" ); // LCBC Unattended Check-in:Schedule Select:Remove Empty Group Types:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "10B3F5FD-6F0C-4818-AEC3-F15AC7587E8B", "DE072E28-C3B5-42A0-B12F-A0D8BE8F6975", @"False" ); // LCBC Unattended Check-in:Schedule Select:Set Available Schedules:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "10B3F5FD-6F0C-4818-AEC3-F15AC7587E8B", "B6256274-8971-4DA2-9144-ED732B46EC5B", @"" ); // LCBC Unattended Check-in:Schedule Select:Set Available Schedules:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "833FF74B-76C6-4735-BA78-EBA04E873AFD", "759890BC-7990-4024-BD82-DF4C3623C3AC", @"False" ); // LCBC Unattended Check-in:Schedule Select:Remove Previous Checkins:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "833FF74B-76C6-4735-BA78-EBA04E873AFD", "C134C40A-D372-44E2-A359-2268977B87C9", @"" ); // LCBC Unattended Check-in:Schedule Select:Remove Previous Checkins:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B3AFF4FD-44DB-49D8-8ED2-C2903C45AD23", "50AB642C-9AD1-4973-9A6D-067F7714DFF3", @"False" ); // LCBC Unattended Check-in:Create Check-Out Labels:Create Labels:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B3AFF4FD-44DB-49D8-8ED2-C2903C45AD23", "CFAC142C-73CB-4886-9A5A-4ED12C80A544", @"" ); // LCBC Unattended Check-in:Create Check-Out Labels:Create Labels:Order

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"     UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )     FROM [AttributeQualifier] [aq]     INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]     INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]     INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]     WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'     AND [aq].[key] = 'definedtypeguid'    " );

            #endregion

            // Add Pager Page
            // Page: Pager Select
            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Pager Select", "", "EB789391-F355-4815-B151-0775BEC4E8B6", "" ); // Site:Rock Check-in
            RockMigrationHelper.UpdateBlockType( "Idle Redirect", "Redirects user to a new url after a specific number of idle seconds.", "~/Blocks/Utility/IdleRedirect.ascx", "Utility", "49FC4B38-741E-4B0B-B395-7C1929340D88" );
            RockMigrationHelper.UpdateBlockType( "Select Select", "Displays a text box to enter a pager number", "~/Plugins/com_bemadev/Checkin/PagerSelect.ascx", "com_bemadev > Check-in", "BF35D025-2BE8-4E10-B5F2-94F5E808326E" );
            // Add Block to Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "EB789391-F355-4815-B151-0775BEC4E8B6", "", "49FC4B38-741E-4B0B-B395-7C1929340D88", "Idle Timeout", "Main", "", "", 1, "1A266D9A-DEB1-4164-BA17-AF48E323F419" );
            // Add Block to Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlock( true, "EB789391-F355-4815-B151-0775BEC4E8B6", "", "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "Select Select", "Main", "", "", 0, "96041DBC-A35D-4E7D-8A28-6D4E1E19A775" );
            // Attrib for BlockType: Idle Redirect:Idle Seconds
            RockMigrationHelper.UpdateBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Idle Seconds", "IdleSeconds", "", "How many seconds of idle time to wait before redirecting user", 0, @"20", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            // Attrib for BlockType: Idle Redirect:New Location
            RockMigrationHelper.UpdateBlockTypeAttribute( "49FC4B38-741E-4B0B-B395-7C1929340D88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "New Location", "NewLocation", "", "The new location URL to send user to after idle time", 0, @"", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4" );
            // Attrib for BlockType: Select Select:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "The workflow type to activate for check-in", 0, @"", "60A53637-0F10-4A39-8A52-CFFE4EE8842B" );
            // Attrib for BlockType: Select Select:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "58080898-0380-4AB4-8DE3-5D78249BD0AE" );
            // Attrib for BlockType: Select Select:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 2, @"", "6AE01D26-E6DA-4284-802F-A62A51A3C0D8" );
            // Attrib for BlockType: Select Select:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 3, @"", "1335CE7A-76CD-47AD-8BF3-D2A169F664EE" );
            // Attrib for BlockType: Select Select:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 4, @"", "181F6788-F854-4176-80B7-BB70001B7BDC" );
            // Attrib for BlockType: Select Select:Multi-Person First Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person First Page (Family Check-in)", "MultiPersonFirstPage", "", "The first page for each person during family check-in.", 5, @"", "263ACDC3-F2CD-4213-A8A5-2123DF3EA95C" );
            // Attrib for BlockType: Select Select:Multi-Person Last Page  (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Last Page  (Family Check-in)", "MultiPersonLastPage", "", "The last page for each person during family check-in.", 6, @"", "09D61C2B-4913-4E4F-97DA-4D9BF1EF62E2" );
            // Attrib for BlockType: Select Select:Multi-Person Done Page (Family Check-in)
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Multi-Person Done Page (Family Check-in)", "MultiPersonDonePage", "", "The page to navigate to once all people have checked in during family check-in.", 7, @"", "FC633FC2-5512-49CF-A187-161E619449F1" );
            // Attrib for BlockType: Select Select:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "7CBCD581-E89A-48ED-A828-3B2C5A53B34A" );
            // Attrib for BlockType: Select Select:Sub Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group name.", 9, @"{0}", "7DDEC250-E975-47A9-9DEC-61B9B8740EC7" );
            // Attrib for BlockType: Select Select:Caption
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Location", "A6D6BC40-858D-4020-99BA-9F1E28F9B73D" );
            // Attrib for BlockType: Select Select:No Option Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available locations that {0} can check into at {1}.", "98A73089-1A39-4C97-933B-48EF57C3BCA0" );
            // Attrib for BlockType: Select Select:No Option After Select Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "BF35D025-2BE8-4E10-B5F2-94F5E808326E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after location is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "787246D1-9DE3-410F-9949-99D12B7C6F27" );
            // Attrib Value for Block:Idle Timeout, Attribute:Idle Seconds Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "1A266D9A-DEB1-4164-BA17-AF48E323F419", "1CAC7B16-041A-4F40-8AEE-A39DFA076C14", @"20" );
            // Attrib Value for Block:Idle Timeout, Attribute:New Location Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "1A266D9A-DEB1-4164-BA17-AF48E323F419", "2254B67B-9CB1-47DE-A63D-D0B56051ECD4", @"/checkin/welcome" );
            // Attrib Value for Block:Select Select, Attribute:Title Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "7CBCD581-E89A-48ED-A828-3B2C5A53B34A", @"{0}" );
            // Attrib Value for Block:Select Select, Attribute:No Option After Select Message Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "787246D1-9DE3-410F-9949-99D12B7C6F27", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );
            // Attrib Value for Block:Select Select, Attribute:No Option Message Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "98A73089-1A39-4C97-933B-48EF57C3BCA0", @"Sorry, there are currently not any available locations that {0} can check into at {1}." );
            // Attrib Value for Block:Select Select, Attribute:Sub Title Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "7DDEC250-E975-47A9-9DEC-61B9B8740EC7", @"{0}" );
            // Attrib Value for Block:Select Select, Attribute:Caption Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "A6D6BC40-858D-4020-99BA-9F1E28F9B73D", @"Enter Pager" );
            // Attrib Value for Block:Select Select, Attribute:Multi-Person First Page (Family Check-in) Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "263ACDC3-F2CD-4213-A8A5-2123DF3EA95C", @"a1cbdaa4-94dd-4156-8260-5a3781e39fd0" );
            // Attrib Value for Block:Select Select, Attribute:Multi-Person Last Page  (Family Check-in) Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "09D61C2B-4913-4E4F-97DA-4D9BF1EF62E2", @"043bb717-5799-446f-b8da-30e575110b0c" );
            // Attrib Value for Block:Select Select, Attribute:Multi-Person Done Page (Family Check-in) Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "FC633FC2-5512-49CF-A187-161E619449F1", @"4af7a0e1-e991-4ae5-a2b5-c440f67a2e6a" );
            // Attrib Value for Block:Select Select, Attribute:Workflow Type Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "60A53637-0F10-4A39-8A52-CFFE4EE8842B", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            // Attrib Value for Block:Select Select, Attribute:Workflow Activity Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "58080898-0380-4AB4-8DE3-5D78249BD0AE", @"" );
            // Attrib Value for Block:Select Select, Attribute:Home Page Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "6AE01D26-E6DA-4284-802F-A62A51A3C0D8", @"432b615a-75ff-4b14-9c99-3e769f866950" );
            // Attrib Value for Block:Select Select, Attribute:Previous Page Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "1335CE7A-76CD-47AD-8BF3-D2A169F664EE", @"6f0cb22b-e05b-42f1-a329-9219e81f6c34" );
            // Attrib Value for Block:Select Select, Attribute:Next Page Page: Pager Select, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775", "181F6788-F854-4176-80B7-BB70001B7BDC", @"043bb717-5799-446f-b8da-30e575110b0c" );

            // Update other pages with new workflow and Pager page
            RockMigrationHelper.AddBlockAttributeValue( "87E1FCDA-41B9-4F1C-A910-BAC2918BE6DB", "162A2B82-A71F-4B29-970A-047266FE696D", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "9DBAC218-2498-4B94-B40D-45516C477C07", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "8BAD4223-13E5-4A53-8BBC-483D8AE9AE61", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "7FBE00BD-7A4E-4F2D-89F1-D62348F4F146", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "1701FD5B-4B65-4AC1-845C-3AA31DE621AE", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "829998AD-2992-4A11-932F-5C3AE5B09895", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "1B455A3D-58B0-4BB9-BF25-3F2CEAF6E49F", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "5E00309E-EC0D-4B99-A1C7-FD644361E5DD", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "5C01B3D2-781B-4A64-8E9A-9987868AD709", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", @"eb789391-f355-4815-b151-0775bec4e8b6" );
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "236ABC89-D83B-4A78-BC9B-6E273E8DD81E", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "569E033B-A2D5-4C15-8CD5-7F1336C22871", @"eb789391-f355-4815-b151-0775bec4e8b6" );
            RockMigrationHelper.AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "46C273D7-6C8A-4066-86A3-4EFEFF39A85E", @"a0bbc045-00e5-4485-88cb-69a73ac7c78d" );
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "C7ABDF19-09B1-4426-B409-CAA1BBB13A11", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "07BC8F00-2925-4CDC-8F9E-DB431B822770", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );


            //Dataview
            RockMigrationHelper.UpdateCategory( "57F8FA29-DCF1-4F74-8553-87E90F234139", "Check-In", "", "", "9C99B5BA-5028-47DA-B99D-C2BEF52DACA3", 0, "" );
            // Create [GroupAll] DataViewFilter for DataView: Attendances in the last Week with Pager Numbers
            Sql( @"  IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'D2046CA9-28CE-4396-BBD1-405A92FEA004') BEGIN          DECLARE          @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),          @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')        INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid])       values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','D2046CA9-28CE-4396-BBD1-405A92FEA004')  END  " );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Attendances in the last Week with Pager Numbers  /* NOTE to Developer. Review that the generated DataViewFilter.Selection '["Attribute_PagerNumber_791a4dc9bb8941e695e9d377ed4c2f0b","64",""]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"  IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '442DC29E-75F8-41C5-99B7-840A64DB9DEB') BEGIN          DECLARE          @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D2046CA9-28CE-4396-BBD1-405A92FEA004'),          @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')        INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid])       values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[""Attribute_PagerNumber_791a4dc9bb8941e695e9d377ed4c2f0b"",""64"",""""]','442DC29E-75F8-41C5-99B7-840A64DB9DEB')  END  " );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Attendances in the last Week with Pager Numbers  /* NOTE to Developer. Review that the generated DataViewFilter.Selection '["Property_StartDateTime","256","CURRENT:-10080\tAll||||"]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"  IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '087631A4-DE40-41DD-924F-CFEB9BA25837') BEGIN          DECLARE          @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D2046CA9-28CE-4396-BBD1-405A92FEA004'),          @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')        INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid])       values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[""Property_StartDateTime"",""256"",""CURRENT:-10080\tAll||||""]','087631A4-DE40-41DD-924F-CFEB9BA25837')  END  " );
            // Create DataView: Attendances in the last Week with Pager Numbers
            Sql( @"  IF NOT EXISTS (SELECT * FROM DataView where [Guid] = '7DF7CE90-0F47-4827-B1F9-AD8EAEB8FEDC') BEGIN  DECLARE      @categoryId int = (select top 1 [Id] from [Category] where [Guid] = '9C99B5BA-5028-47DA-B99D-C2BEF52DACA3'),      @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '4CCB856F-51E0-4E48-B94A-1705EFBA6C9E'),      @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = 'D2046CA9-28CE-4396-BBD1-405A92FEA004'),      @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')    INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])  VALUES(0,'Attendances in the last Week with Pager Numbers','',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'7DF7CE90-0F47-4827-B1F9-AD8EAEB8FEDC')  END  " );

            //Report
            RockMigrationHelper.UpdateCategory( "F1F22D3E-FEFA-4C84-9FFA-9E8ACE60FCE7", "Check-in", "", "", "EA459ECC-0C4E-40E3-8CEC-66DC36936BC9", 0, "" );
            RockMigrationHelper.AddReport( "EA459ECC-0C4E-40E3-8CEC-66DC36936BC9", "7DF7CE90-0F47-4827-B1F9-AD8EAEB8FEDC", "4CCB856F-51E0-4E48-B94A-1705EFBA6C9E", "Child Pager Numbers", "", "D57B0366-CE04-43DE-8879-6120E0175C45" );
            RockMigrationHelper.AddReportField( "D57B0366-CE04-43DE-8879-6120E0175C45", Rock.Model.ReportFieldType.DataSelectComponent, true, "C130DC52-CA31-45EE-A4F2-6C53A838EF3D", @"{% attendance id:'{{Id}}' %}
    {{attendance.PersonAlias.Person.NickName}}
{% endattendance %}", 0, "First Name", "42313F91-73E7-4AE0-870D-151AEAB7986D" );
            RockMigrationHelper.AddReportField( "D57B0366-CE04-43DE-8879-6120E0175C45", Rock.Model.ReportFieldType.DataSelectComponent, true, "C130DC52-CA31-45EE-A4F2-6C53A838EF3D", @"{% attendance id:'{{Id}}' %}
    {{attendance.PersonAlias.Person.LastName}}
{% endattendance %}", 1, "Last Name", "F4A55BF3-F32A-4510-B464-592305EB9F7B" );
            RockMigrationHelper.AddReportField( "D57B0366-CE04-43DE-8879-6120E0175C45", Rock.Model.ReportFieldType.Property, true, new Guid().ToString(), "StartDateTime", 2, "Check-in Date", "D2489667-8912-4124-BD57-AFAB6A3B91C3" );
            RockMigrationHelper.AddReportField( "D57B0366-CE04-43DE-8879-6120E0175C45", Rock.Model.ReportFieldType.DataSelectComponent, true, "C130DC52-CA31-45EE-A4F2-6C53A838EF3D", @"{% attendance id:'{{Id}}' %}
    { { attendance.Occurrence.Schedule.Name} }
            {% endattendance %}
            ", 3, "Service", "3043CE63-61D0-4A3C-A917-549FBB2E987F" );
            RockMigrationHelper.AddReportField( "D57B0366-CE04-43DE-8879-6120E0175C45", Rock.Model.ReportFieldType.Attribute, true, new Guid().ToString(), "791a4dc9bb8941e695e9d377ed4c2f0b", 4, "Pager Number", "17EBDC4E-118E-4762-94F9-C69CAB49A619" );
            Sql( @"
                Update ReportField
                Set SortDirection = 1,
                    SortOrder = 0
                Where [Guid] = 'D2489667-8912-4124-BD57-AFAB6A3B91C3'" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteReportField( "42313F91-73E7-4AE0-870D-151AEAB7986D" );
            RockMigrationHelper.DeleteReportField( "F4A55BF3-F32A-4510-B464-592305EB9F7B" );
            RockMigrationHelper.DeleteReportField( "D2489667-8912-4124-BD57-AFAB6A3B91C3" );
            RockMigrationHelper.DeleteReportField( "3043CE63-61D0-4A3C-A917-549FBB2E987F" );
            RockMigrationHelper.DeleteReportField( "17EBDC4E-118E-4762-94F9-C69CAB49A619" );
            RockMigrationHelper.DeleteReport( "D57B0366-CE04-43DE-8879-6120E0175C45" );
            RockMigrationHelper.DeleteCategory( "EA459ECC-0C4E-40E3-8CEC-66DC36936BC9" );

            // Delete DataView: Attendances in the last Week with Pager Numbers
            Sql( @"DELETE FROM DataView where [Guid] = '7DF7CE90-0F47-4827-B1F9-AD8EAEB8FEDC'" );
            // Delete DataViewFilter for DataView: Attendances in the last Week with Pager Numbers
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '087631A4-DE40-41DD-924F-CFEB9BA25837'" );
            // Delete DataViewFilter for DataView: Attendances in the last Week with Pager Numbers
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '442DC29E-75F8-41C5-99B7-840A64DB9DEB'" );
            // Delete DataViewFilter for DataView: Attendances in the last Week with Pager Numbers
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'D2046CA9-28CE-4396-BBD1-405A92FEA004'" );
            RockMigrationHelper.DeleteCategory( "9C99B5BA-5028-47DA-B99D-C2BEF52DACA3" );
            RockMigrationHelper.AddBlockAttributeValue( "87E1FCDA-41B9-4F1C-A910-BAC2918BE6DB", "162A2B82-A71F-4B29-970A-047266FE696D", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "9DBAC218-2498-4B94-B40D-45516C477C07", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "8BAD4223-13E5-4A53-8BBC-483D8AE9AE61", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "7FBE00BD-7A4E-4F2D-89F1-D62348F4F146", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "1701FD5B-4B65-4AC1-845C-3AA31DE621AE", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "829998AD-2992-4A11-932F-5C3AE5B09895", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "1B455A3D-58B0-4BB9-BF25-3F2CEAF6E49F", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "5E00309E-EC0D-4B99-A1C7-FD644361E5DD", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "1ED29421-3F3B-4A5B-AC1D-FAA27B34D23E", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "5C01B3D2-781B-4A64-8E9A-9987868AD709", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "E4F7B489-39B8-49F9-8C8C-533275FAACDF", @"043bb717-5799-446f-b8da-30e575110b0c" );
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "236ABC89-D83B-4A78-BC9B-6E273E8DD81E", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "569E033B-A2D5-4C15-8CD5-7F1336C22871", @"6f0cb22b-e05b-42f1-a329-9219e81f6c34" );
            RockMigrationHelper.AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "46C273D7-6C8A-4066-86A3-4EFEFF39A85E", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "C7ABDF19-09B1-4426-B409-CAA1BBB13A11", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "07BC8F00-2925-4CDC-8F9E-DB431B822770", "C7C8C51E-B5A0-49A5-96F3-CB23BB5F81AB", @"011E9F5A-60D4-4FF5-912A-290881E37EAF" );
            RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "108E2E9E-DC18-4D5D-80FA-5D4A90FFCE65", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "887E090F-5468-44F0-BE84-9AE21D822987", "BA9AD11A-DB90-4BF6-ACDA-6FFB56C0358A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );
            RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "E7FCFB35-0172-46DB-A38F-6C54BCA49A6A", @"011e9f5a-60d4-4ff5-912a-290881e37eaf" );

            RockMigrationHelper.DeleteAttribute( "181F6788-F854-4176-80B7-BB70001B7BDC" );
            RockMigrationHelper.DeleteAttribute( "1335CE7A-76CD-47AD-8BF3-D2A169F664EE" );
            RockMigrationHelper.DeleteAttribute( "6AE01D26-E6DA-4284-802F-A62A51A3C0D8" );
            RockMigrationHelper.DeleteAttribute( "58080898-0380-4AB4-8DE3-5D78249BD0AE" );
            RockMigrationHelper.DeleteAttribute( "60A53637-0F10-4A39-8A52-CFFE4EE8842B" );
            RockMigrationHelper.DeleteAttribute( "FC633FC2-5512-49CF-A187-161E619449F1" );
            RockMigrationHelper.DeleteAttribute( "09D61C2B-4913-4E4F-97DA-4D9BF1EF62E2" );
            RockMigrationHelper.DeleteAttribute( "263ACDC3-F2CD-4213-A8A5-2123DF3EA95C" );
            RockMigrationHelper.DeleteAttribute( "A6D6BC40-858D-4020-99BA-9F1E28F9B73D" );
            RockMigrationHelper.DeleteAttribute( "7DDEC250-E975-47A9-9DEC-61B9B8740EC7" );
            RockMigrationHelper.DeleteAttribute( "98A73089-1A39-4C97-933B-48EF57C3BCA0" );
            RockMigrationHelper.DeleteAttribute( "787246D1-9DE3-410F-9949-99D12B7C6F27" );
            RockMigrationHelper.DeleteAttribute( "7CBCD581-E89A-48ED-A828-3B2C5A53B34A" );
            RockMigrationHelper.DeleteAttribute( "2254B67B-9CB1-47DE-A63D-D0B56051ECD4" );
            RockMigrationHelper.DeleteAttribute( "1CAC7B16-041A-4F40-8AEE-A39DFA076C14" );
            RockMigrationHelper.DeleteBlock( "96041DBC-A35D-4E7D-8A28-6D4E1E19A775" );
            RockMigrationHelper.DeleteBlock( "1A266D9A-DEB1-4164-BA17-AF48E323F419" );
            RockMigrationHelper.DeleteBlockType( "BF35D025-2BE8-4E10-B5F2-94F5E808326E" );
            RockMigrationHelper.DeleteBlockType( "49FC4B38-741E-4B0B-B395-7C1929340D88" );
            RockMigrationHelper.DeletePage( "EB789391-F355-4815-B151-0775BEC4E8B6" ); //  Page: Pager Select
        }
    }
}
