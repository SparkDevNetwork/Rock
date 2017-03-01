// <copyright>
// Copyright by the Central Christian Church
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

using Rock.Plugin;
namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 2, "1.4.5" )]
    public class Pages : Migration
    {
        public override void Up()
        {
            // Page: Room Management
            RockMigrationHelper.AddPage( "7F2581A1-941E-4D51-8A9D-5BE9B881B003", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Room Management", "", "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation Lava", "Renders a list of reservations in lava.", "~/Plugins/com_centralaz/RoomManagement/ReservationLava.ascx", "com_centralaz > Room Management", "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635" );
            RockMigrationHelper.AddBlock( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Feature", "", "", 0, "AF897B42-21AA-4A56-B0D7-9E5303D4CE53" );
            RockMigrationHelper.AddBlock( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "", "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "Reservation Lava", "Main", "", "", 0, "F71B7715-EBF5-4CDF-867E-B1018B2AECD5" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default View Option", "DefaultViewOption", "", "Determines the default view option", 1, @"Week", "FFCF0C2C-8FEA-4851-AB0D-D72F50B375EC" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Details Page", "DetailsPage", "", "Detail page for events", 2, @"", "DB7FBF03-B31F-4695-804C-EE93DC411621" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Campus Filter Display Mode", "CampusFilterDisplayMode", "", "", 3, @"1", "C9C37A37-06E1-4EB7-A63F-9F7C51319A94" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Audience Filter Display Mode", "MinistryFilterDisplayMode", "", "", 4, @"1", "68966FA5-6BF9-460D-AC1D-9FF7A52F5AF2" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Filter Audiences", "FilterCategories", "", "Determines which audiences should be displayed in the filter.", 5, @"", "B4EAC33E-9DC2-495C-97D4-99C4345599EF" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "", "Determines whether the date range filters are shown", 6, @"False", "927C546B-305F-4491-A2F2-9E05C2446E4E" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Small Calendar", "ShowSmallCalendar", "", "Determines whether the calendar widget is shown", 7, @"True", "7FE686C0-DE82-4D0D-AB39-99D15681D248" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Day View", "ShowDayView", "", "Determines whether the day view option is shown", 8, @"False", "8BC51BF9-C4FC-4B08-A889-69F6A1C11230" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Week View", "ShowWeekView", "", "Determines whether the week view option is shown", 9, @"True", "5B8F6E28-588C-451F-8BEC-2A5EC4800132" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Month View", "ShowMonthView", "", "Determines whether the month view option is shown", 10, @"True", "6CB8FAC9-2CAC-49D2-9316-48360A8845D2" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of events.", 12, @"{% include '~/Themes/Stark/Assets/Lava/CalendarGroupedOccurrence.lava' %}", "52C3F839-A092-441F-B3F9-10617BE391EC" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "7EDFA2DE-FDD3-4AC1-B356-1F5BFC231DAE", "Start of Week Day", "StartofWeekDay", "", "Determines what day is the start of a week.", 13, @"0", "90B5D912-8506-4C3D-89E2-8A91512BB30D" );
            RockMigrationHelper.AddBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 14, @"False", "EE50A389-5909-44DF-84E6-F84085CD827E" );
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" ); // Template
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7638af8b-e4c0-4c02-93b8-72a829ecacdb" ); // Root Page
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
            RockMigrationHelper.AddBlockAttributeValue( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "B4EAC33E-9DC2-495C-97D4-99C4345599EF", @"" ); // Filter Audiences
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "EE50A389-5909-44DF-84E6-F84085CD827E", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "FFCF0C2C-8FEA-4851-AB0D-D72F50B375EC", @"Week" ); // Default View Option
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "DB7FBF03-B31F-4695-804C-EE93DC411621", @"4cbd2b96-e076-46df-a576-356bca5e577f" ); // Details Page
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "C9C37A37-06E1-4EB7-A63F-9F7C51319A94", @"1" ); // Campus Filter Display Mode
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "68966FA5-6BF9-460D-AC1D-9FF7A52F5AF2", @"1" ); // Audience Filter Display Mode
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "927C546B-305F-4491-A2F2-9E05C2446E4E", @"False" ); // Show Date Range Filter
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "7FE686C0-DE82-4D0D-AB39-99D15681D248", @"True" ); // Show Small Calendar
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "8BC51BF9-C4FC-4B08-A889-69F6A1C11230", @"False" ); // Show Day View
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "5B8F6E28-588C-451F-8BEC-2A5EC4800132", @"True" ); // Show Week View
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "6CB8FAC9-2CAC-49D2-9316-48360A8845D2", @"True" ); // Show Month View
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "52C3F839-A092-441F-B3F9-10617BE391EC", @"{% include '~/Plugins/com_centralaz/RoomManagement/Assets/Lava/Reservation.lava' %}" ); // Lava Template
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "90B5D912-8506-4C3D-89E2-8A91512BB30D", @"0" ); // Start of Week Day

            // Page: New Reservation
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "New Reservation", "", "4CBD2B96-E076-46DF-A576-356BCA5E577F", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation Detail", "Block for viewing a reservation detail", "~/Plugins/com_centralaz/RoomManagement/ReservationDetail.ascx", "com_centralaz > Room Management", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            RockMigrationHelper.AddBlock( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "Reservation Detail", "Main", "", "", 0, "65091E04-77CE-411C-989F-EAD7D15778A0" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '4CBD2B96-E076-46DF-A576-356BCA5E577F'" );

            // Page: Search Reservations
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Search Reservations", "", "1C58D731-F590-4AAC-8B8C-FD42B428B69A", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation List", "Block for viewing a list of reservations.", "~/Plugins/com_centralaz/RoomManagement/ReservationList.ascx", "com_centralaz > Room Management", "8169F541-9544-4A41-BD90-0DC2D0144AFD" );
            RockMigrationHelper.AddBlock( "1C58D731-F590-4AAC-8B8C-FD42B428B69A", "", "8169F541-9544-4A41-BD90-0DC2D0144AFD", "Reservation List", "Main", "", "", 0, "4D4882F8-5ACC-4AE1-BC75-4FFDDA26F270" );
            RockMigrationHelper.AddBlockTypeAttribute( "8169F541-9544-4A41-BD90-0DC2D0144AFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"4cbd2b96-e076-46df-a576-356bca5e577f", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8" );

            // Page: Available Resources
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Available Resources", "", "81CC9A85-06F6-43B9-8476-9DF8A987EF55", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Availability List", "Block for viewing the availability of resources.", "~/Plugins/com_centralaz/RoomManagement/AvailabilityList.ascx", "com_centralaz > Room Management", "2A01E437-AB13-47B0-B3D4-96915801B693" );
            RockMigrationHelper.AddBlock( "81CC9A85-06F6-43B9-8476-9DF8A987EF55", "", "2A01E437-AB13-47B0-B3D4-96915801B693", "Availability List", "Main", "", "", 0, "1B4F3A33-656B-4FCB-A446-D481782DE8B4" );
            RockMigrationHelper.AddBlockTypeAttribute( "2A01E437-AB13-47B0-B3D4-96915801B693", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"4cbd2b96-e076-46df-a576-356bca5e577f", "85ECB608-B64E-43C0-986C-FC8FD38F9D81" );

            // Page: Admin Tools
            RockMigrationHelper.AddPage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Admin Tools", "", "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "41639D13-F7A6-45FE-BF32-2F17371A181C" );
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" ); // Template
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"0ff1d7f4-bf6d-444a-bd71-645bd764ec40" ); // Root Page
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
            RockMigrationHelper.AddBlockAttributeValue( "41639D13-F7A6-45FE-BF32-2F17371A181C", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List

            // Page: Resources
            RockMigrationHelper.AddPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resources", "", "15EDB2B6-BB6B-431E-A9AA-829489D87EDD", "fa fa-lightbulb-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Resource List", "Block for viewing a list of resources.", "~/Plugins/com_centralaz/RoomManagement/ResourceList.ascx", "com_centralaz > Room Management", "84F92545-49C5-4FF6-A7B1-099A9662F42D" );
            RockMigrationHelper.AddBlock( "15EDB2B6-BB6B-431E-A9AA-829489D87EDD", "", "84F92545-49C5-4FF6-A7B1-099A9662F42D", "Resource List", "Main", "", "", 0, "BFFDFD88-EA8D-47D1-80F0-4B0D05523E69" );
            RockMigrationHelper.AddBlockTypeAttribute( "84F92545-49C5-4FF6-A7B1-099A9662F42D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "0C023434-43B7-4086-B469-B541FE47561C" );
            RockMigrationHelper.AddBlockAttributeValue( "BFFDFD88-EA8D-47D1-80F0-4B0D05523E69", "0C023434-43B7-4086-B469-B541FE47561C", @"b75a0c7e-4a15-4892-a857-bade8b5dd4ca" ); // Detail Page
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '15EDB2B6-BB6B-431E-A9AA-829489D87EDD'" );

            // Page: Resource Categories
            RockMigrationHelper.AddPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resource Categories", "", "455FFF96-AE2A-435A-B3E2-F6C32754E53A", "fa fa-folder" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Categories", "Block for managing categories for a specific, configured entity type.", "~/Blocks/Core/Categories.ascx", "Core", "620FC4A2-6587-409F-8972-22065919D9AC" );
            RockMigrationHelper.AddBlock( "455FFF96-AE2A-435A-B3E2-F6C32754E53A", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", "", "", 0, "07FFD3C4-5E22-4026-AAE3-EABE608D316A" );
            RockMigrationHelper.AddBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", "The entity type to manage categories for.", 0, @"", "C405A507-7889-4287-8342-105B89710044" );
            RockMigrationHelper.AddBlockAttributeValue( "07FFD3C4-5E22-4026-AAE3-EABE608D316A", "C405A507-7889-4287-8342-105B89710044", @"35584736-8fe2-48da-9121-3afd07a2da8d" ); // Entity Type

            // Page: Resource Detail
            RockMigrationHelper.AddPage( "15EDB2B6-BB6B-431E-A9AA-829489D87EDD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Resource Detail", "", "B75A0C7E-4A15-4892-A857-BADE8B5DD4CA", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Resource Detail", "Displays the details of the resource.", "~/Plugins/com_centralaz/RoomManagement/ResourceDetail.ascx", "com_centralaz > Room Management", "88C8A452-6878-4938-913F-CA3EF87D50ED" );
            RockMigrationHelper.AddBlock( "B75A0C7E-4A15-4892-A857-BADE8B5DD4CA", "", "88C8A452-6878-4938-913F-CA3EF87D50ED", "Resource Detail", "Main", "", "", 0, "89E210D9-7645-4CB8-9AE1-CB5512074D69" );

            // Page: Reservation Configuration
            RockMigrationHelper.AddPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reservation Configuration", "", "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "fa fa-gear" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Reservation Configuration", "Displays the details of the given Connection Type for editing.", "~/Plugins/com_centralaz/RoomManagement/ReservationConfiguration.ascx", "com_centralaz > Room Management", "6931E212-A76A-4DBB-9B97-86E5CDD0793A" );
            RockMigrationHelper.AddBlock( "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "", "6931E212-A76A-4DBB-9B97-86E5CDD0793A", "Reservation Configuration", "Main", "", "", 0, "2B864E89-27DE-41F9-A24B-8D2EA5C40D10" );

            // Add Security
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Room Management Administration", "Group of individuals who can administration the custom Room Management module.", "FBE0324F-F29A-4ACF-8EC3-5386C5562D70" );
            RockMigrationHelper.AddSecurityAuthForPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", 0, "View", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, "BB566890-2A22-40CA-ACD3-F8EB3EC51860" );
            RockMigrationHelper.AddSecurityAuthForPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", 1, "View", true, "FBE0324F-F29A-4ACF-8EC3-5386C5562D70", 0, "404EEA33-5551-4048-9A04-D9AEAA0B18B0" );
            RockMigrationHelper.AddSecurityAuthForPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", 2, "View", false, "", 1, "56FF0CCC-8129-4228-9060-206589A62B23" );
            RockMigrationHelper.AddSecurityAuthForPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", 0, "Edit", true, "FBE0324F-F29A-4ACF-8EC3-5386C5562D70", 0, "1EE92B5E-E44C-434C-AA50-2D6C22388A4D" );
            RockMigrationHelper.AddSecurityAuthForPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", 0, "Administrate", true, "FBE0324F-F29A-4ACF-8EC3-5386C5562D70", 0, "70D9BC19-4328-4158-A110-46DE8E816F4E" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityRoleGroup( "FBE0324F-F29A-4ACF-8EC3-5386C5562D70" );

            RockMigrationHelper.DeleteBlock( "2B864E89-27DE-41F9-A24B-8D2EA5C40D10" );
            RockMigrationHelper.DeleteBlockType( "6931E212-A76A-4DBB-9B97-86E5CDD0793A" );
            RockMigrationHelper.DeletePage( "CFF84B6D-C852-4FC4-B602-9F045EDC8854" ); //  Page: Reservation Configuration

            RockMigrationHelper.DeleteBlock( "89E210D9-7645-4CB8-9AE1-CB5512074D69" );
            RockMigrationHelper.DeleteBlockType( "88C8A452-6878-4938-913F-CA3EF87D50ED" );
            RockMigrationHelper.DeletePage( "B75A0C7E-4A15-4892-A857-BADE8B5DD4CA" ); //  Page: Resource Detail

            RockMigrationHelper.DeleteAttribute( "C405A507-7889-4287-8342-105B89710044" );
            RockMigrationHelper.DeleteBlock( "07FFD3C4-5E22-4026-AAE3-EABE608D316A" );
            RockMigrationHelper.DeleteBlockType( "620FC4A2-6587-409F-8972-22065919D9AC" );
            RockMigrationHelper.DeletePage( "455FFF96-AE2A-435A-B3E2-F6C32754E53A" ); //  Page: Resource Categories

            RockMigrationHelper.DeleteAttribute( "0C023434-43B7-4086-B469-B541FE47561C" );
            RockMigrationHelper.DeleteBlock( "BFFDFD88-EA8D-47D1-80F0-4B0D05523E69" );
            RockMigrationHelper.DeleteBlockType( "84F92545-49C5-4FF6-A7B1-099A9662F42D" );
            RockMigrationHelper.DeletePage( "15EDB2B6-BB6B-431E-A9AA-829489D87EDD" ); //  Page: Resources
                     
            RockMigrationHelper.DeleteBlock( "41639D13-F7A6-45FE-BF32-2F17371A181C" );
            RockMigrationHelper.DeletePage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40" ); //  Page: Admin Tools

            RockMigrationHelper.DeleteAttribute( "85ECB608-B64E-43C0-986C-FC8FD38F9D81" );
            RockMigrationHelper.DeleteBlock( "1B4F3A33-656B-4FCB-A446-D481782DE8B4" );
            RockMigrationHelper.DeleteBlockType( "2A01E437-AB13-47B0-B3D4-96915801B693" );
            RockMigrationHelper.DeletePage( "81CC9A85-06F6-43B9-8476-9DF8A987EF55" ); //  Page: Available Resources

            RockMigrationHelper.DeleteAttribute( "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8" );
            RockMigrationHelper.DeleteBlock( "4D4882F8-5ACC-4AE1-BC75-4FFDDA26F270" );
            RockMigrationHelper.DeleteBlockType( "8169F541-9544-4A41-BD90-0DC2D0144AFD" );
            RockMigrationHelper.DeletePage( "1C58D731-F590-4AAC-8B8C-FD42B428B69A" ); //  Page: Search Reservations

            RockMigrationHelper.DeleteBlock( "65091E04-77CE-411C-989F-EAD7D15778A0" );
            RockMigrationHelper.DeleteBlockType( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB" );
            RockMigrationHelper.DeletePage( "4CBD2B96-E076-46DF-A576-356BCA5E577F" ); //  Page: New Reservation

            RockMigrationHelper.DeleteAttribute( "90B5D912-8506-4C3D-89E2-8A91512BB30D" );
            RockMigrationHelper.DeleteAttribute( "52C3F839-A092-441F-B3F9-10617BE391EC" );
            RockMigrationHelper.DeleteAttribute( "6CB8FAC9-2CAC-49D2-9316-48360A8845D2" );
            RockMigrationHelper.DeleteAttribute( "5B8F6E28-588C-451F-8BEC-2A5EC4800132" );
            RockMigrationHelper.DeleteAttribute( "8BC51BF9-C4FC-4B08-A889-69F6A1C11230" );
            RockMigrationHelper.DeleteAttribute( "7FE686C0-DE82-4D0D-AB39-99D15681D248" );
            RockMigrationHelper.DeleteAttribute( "927C546B-305F-4491-A2F2-9E05C2446E4E" );
            RockMigrationHelper.DeleteAttribute( "68966FA5-6BF9-460D-AC1D-9FF7A52F5AF2" );
            RockMigrationHelper.DeleteAttribute( "C9C37A37-06E1-4EB7-A63F-9F7C51319A94" );
            RockMigrationHelper.DeleteAttribute( "DB7FBF03-B31F-4695-804C-EE93DC411621" );
            RockMigrationHelper.DeleteAttribute( "FFCF0C2C-8FEA-4851-AB0D-D72F50B375EC" );
            RockMigrationHelper.DeleteAttribute( "EE50A389-5909-44DF-84E6-F84085CD827E" );
            RockMigrationHelper.DeleteAttribute( "B4EAC33E-9DC2-495C-97D4-99C4345599EF" );
            RockMigrationHelper.DeleteBlock( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5" );
            RockMigrationHelper.DeleteBlock( "AF897B42-21AA-4A56-B0D7-9E5303D4CE53" );
            RockMigrationHelper.DeleteBlockType( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635" );
            RockMigrationHelper.DeletePage( "7638AF8B-E4C0-4C02-93B8-72A829ECACDB" ); //  Page: Room Management
        }
    }
}
