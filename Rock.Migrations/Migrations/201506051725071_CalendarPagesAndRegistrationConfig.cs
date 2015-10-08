// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class CalendarPagesAndRegistrationConfig : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // #897 Binary File Types Duplicate Name rollups
            DropIndex("dbo.BinaryFileType", new[] { "Name" });

            // #893 Office Information orphan block removal
            RockMigrationHelper.DeleteBlock("3BFEF2CC-AEA9-457E-A552-C14D69AD93FE");

            //
            // add defined type for registration configuration
            //

            RockMigrationHelper.AddDefinedType("Check-in", "Family Registration Configuration", "Configuration templates for the Family Registration application.", "0F48CB3F-8A48-249A-412A-2DCA7648706F");
            RockMigrationHelper.AddDefinedTypeAttribute("0F48CB3F-8A48-249A-412A-2DCA7648706F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Configuration Template", "ConfigurationTemplate", "JSON configuration file.", 0, "", "492D1E2D-5DB5-53BF-46BF-C1F95D17A72B");
            RockMigrationHelper.AddAttributeQualifier("492D1E2D-5DB5-53BF-46BF-C1F95D17A72B", "editorHeight", "600", "8A687566-5E0C-5C9B-4A95-CAE04EF4F947");
            RockMigrationHelper.AddAttributeQualifier("492D1E2D-5DB5-53BF-46BF-C1F95D17A72B", "editorTheme", "0", "3E912DED-21A6-9398-4BC3-DC44EA3249A5");
            RockMigrationHelper.AddAttributeQualifier("492D1E2D-5DB5-53BF-46BF-C1F95D17A72B", "editorMode", "4", "C557305C-84DD-5191-4A9B-C873A487AD05");

            // 
            // pages for calendar
            //

            RockMigrationHelper.AddPage("F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Calendars", "", "63990874-0DFF-45FC-9F09-81B0B0D375B4", ""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Event Calendar List", "Block to display the event calendars.", "~/Blocks/Calendar/EventCalendarList.ascx", "Event Calendar", "041B5C23-5F1F-4B02-A767-FB7F4B1A5345");
            RockMigrationHelper.AddBlock("63990874-0DFF-45FC-9F09-81B0B0D375B4", "", "041B5C23-5F1F-4B02-A767-FB7F4B1A5345", "Event Calendar List", "Main", "", "", 0, "367B36C6-2779-451D-BF8C-BC6318D42AA1");

            RockMigrationHelper.AddBlockTypeAttribute("041B5C23-5F1F-4B02-A767-FB7F4B1A5345", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view status of an event calendar.", 0, @"", "E7907FA7-B586-439B-8DE1-C0185EC4F790");

            RockMigrationHelper.AddBlockAttributeValue("367B36C6-2779-451D-BF8C-BC6318D42AA1", "E7907FA7-B586-439B-8DE1-C0185EC4F790", @"b54725e1-3640-4419-b580-2af77daf6568"); // Detail Page

            // Page: Event Calendar Detail
            RockMigrationHelper.AddPage("63990874-0DFF-45FC-9F09-81B0B0D375B4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Event Calendar Detail", "", "B54725E1-3640-4419-B580-2AF77DAF6568", ""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Event Calendar Detail", "Displays the details of the given Event Calendar for editing.", "~/Blocks/Calendar/EventCalendarDetail.ascx", "Event Calendar", "0320DFB9-7A5A-4DAC-8234-3D504E496D71");
            RockMigrationHelper.UpdateBlockType("Event Calendar Item List", "Lists all the items in the given calendar.", "~/Blocks/Calendar/EventCalendarItemList.ascx", "Event Calendar", "EC8DFDC5-C177-4208-8ABA-1F85010FBBFF");
            RockMigrationHelper.AddBlock("B54725E1-3640-4419-B580-2AF77DAF6568", "", "0320DFB9-7A5A-4DAC-8234-3D504E496D71", "Event Calendar Detail", "Main", "", "", 0, "0C94B3DE-4FA8-4683-B4E9-52BE40D266D9");

            RockMigrationHelper.AddBlock("B54725E1-3640-4419-B580-2AF77DAF6568", "", "EC8DFDC5-C177-4208-8ABA-1F85010FBBFF", "Event Calendar Item List", "Main", "", "", 1, "87D0A3C6-D32B-4840-8D1A-B6030B72EA95");

            RockMigrationHelper.AddBlockTypeAttribute("EC8DFDC5-C177-4208-8ABA-1F85010FBBFF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "AA08DCED-940E-45B9-AB9B-7B93A609BF13");

            RockMigrationHelper.AddBlockAttributeValue("87D0A3C6-D32B-4840-8D1A-B6030B72EA95", "AA08DCED-940E-45B9-AB9B-7B93A609BF13", @"7fb33834-f40a-4221-8849-bb8c06903b04"); // Detail Page

            // Page: Event Item Detail Page
            RockMigrationHelper.AddPage("B54725E1-3640-4419-B580-2AF77DAF6568", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Event Item Detail Page", "", "7FB33834-F40A-4221-8849-BB8C06903B04", ""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Event Item Detail", "Displays the details of the given eventItem.", "~/Blocks/Calendar/EventItemDetail.ascx", "Event Calendar", "39E3476D-1BA1-438D-887F-03DD23639221");
            RockMigrationHelper.AddBlock("7FB33834-F40A-4221-8849-BB8C06903B04", "", "39E3476D-1BA1-438D-887F-03DD23639221", "Event Item Detail", "Main", "", "", 0, "2F577D8E-7A9C-46DC-A4AB-4732E217F7D0");

            RockMigrationHelper.AddBlockTypeAttribute("39E3476D-1BA1-438D-887F-03DD23639221", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 2, @"True", "95CB7E85-F83D-47BB-84B3-77A73C640304");
            // Page: Calendar
            RockMigrationHelper.AddPage("85F25819-E948-4960-9DDF-00F54D32444E", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Calendar", "", "2E6FED28-683F-4726-8CF1-2822E8E73B03", ""); // Site:External Website
            RockMigrationHelper.UpdateBlockType("Event Calendar Lava", "Displays details for a specific package.", "~/Blocks/Calendar/ExternalCalendarLava.ascx", "Event Calendar", "8760D668-8ADF-48C8-9D90-09461FB75B88");
            RockMigrationHelper.AddBlock("2E6FED28-683F-4726-8CF1-2822E8E73B03", "", "8760D668-8ADF-48C8-9D90-09461FB75B88", "Event Calendar Lava", "Main", "", "", 0, "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the calendar name.", 0, @"False", "4A53E8C5-EE4E-4321-9226-3EEA32A9379D");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Event Calendar Id", "EventCalendarId", "", "The Id of the event calendar to be displayed", 0, @"1", "BF06F16A-E40F-497A-B1A9-409D4FFCD972");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "", "Determines whether the campus filters are shown", 0, @"False", "C1259B83-C1AF-4880-81A3-16204D502DC9");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Filter", "ShowCategoryFilter", "", "Determines whether the campus filters are shown", 0, @"False", "9DEA73E9-E5AF-4386-BFBA-4C1440D315C8");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "", "Determines whether the campus filters are shown", 0, @"False", "0DA63111-AFE3-4D77-BE00-05DE5D69B536");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of events.", 2, @"{% include '~/Assets/Lava/EventCalendar/ExternalCalendar.lava' %}", "1D3EC083-581E-4435-8FC8-930C48AC50F4");

            RockMigrationHelper.AddBlockTypeAttribute("8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "1704176B-F79A-4586-8B11-8460CCFB44BE");

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "1D3EC083-581E-4435-8FC8-930C48AC50F4", @"{% include '~/Assets/Lava/EventCalendar/ExternalCalendar.lava' %}"); // Lava Template

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "1704176B-F79A-4586-8B11-8460CCFB44BE", @"False"); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "4A53E8C5-EE4E-4321-9226-3EEA32A9379D", @"False"); // Set Page Title

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "BF06F16A-E40F-497A-B1A9-409D4FFCD972", @"1"); // Event Calendar Id

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "C1259B83-C1AF-4880-81A3-16204D502DC9", @"False"); // Show Date Range Filter

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "9DEA73E9-E5AF-4386-BFBA-4C1440D315C8", @"False"); // Show Category Filter

            RockMigrationHelper.AddBlockAttributeValue("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "0DA63111-AFE3-4D77-BE00-05DE5D69B536", @"False"); // Show Campus Filter


            Sql(@"
    INSERT INTO [dbo].[EventCalendar]
           ([Name]
           ,[Description]
           ,[IconCssClass]
           ,[IsActive]
           ,[Guid])
     VALUES
           ('Public'
           ,'A calendar for public events that the church hosts and promotes.'
           ,'fa fa-calendar'
           ,'true'
           ,'8a444668-19af-4417-9c74-09f842572974')
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // #897 Binary File Types Duplicate Name rollups
            CreateIndex("dbo.BinaryFileType", "Name", unique: true);

            //
            // family reg config
            // 

            RockMigrationHelper.DeleteAttribute("492D1E2D-5DB5-53BF-46BF-C1F95D17A72B");
            RockMigrationHelper.DeleteDefinedType("0F48CB3F-8A48-249A-412A-2DCA7648706F");

            //
            // calendar pages
            //

            Sql(@"DELETE FROM [EventCalendar] WHERE [Guid] = '8a444668-19af-4417-9c74-09f842572974'");

            RockMigrationHelper.DeleteAttribute("0DA63111-AFE3-4D77-BE00-05DE5D69B536");
            RockMigrationHelper.DeleteAttribute("9DEA73E9-E5AF-4386-BFBA-4C1440D315C8");
            RockMigrationHelper.DeleteAttribute("C1259B83-C1AF-4880-81A3-16204D502DC9");
            RockMigrationHelper.DeleteAttribute("BF06F16A-E40F-497A-B1A9-409D4FFCD972");
            RockMigrationHelper.DeleteAttribute("4A53E8C5-EE4E-4321-9226-3EEA32A9379D");
            RockMigrationHelper.DeleteAttribute("1704176B-F79A-4586-8B11-8460CCFB44BE");
            RockMigrationHelper.DeleteAttribute("1D3EC083-581E-4435-8FC8-930C48AC50F4");
            RockMigrationHelper.DeleteBlock("0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7");
            RockMigrationHelper.DeleteBlockType("8760D668-8ADF-48C8-9D90-09461FB75B88");
            RockMigrationHelper.DeletePage("2E6FED28-683F-4726-8CF1-2822E8E73B03"); //  Page: Calendar
            RockMigrationHelper.DeleteAttribute("95CB7E85-F83D-47BB-84B3-77A73C640304");
            RockMigrationHelper.DeleteBlock("2F577D8E-7A9C-46DC-A4AB-4732E217F7D0");
            RockMigrationHelper.DeleteBlockType("39E3476D-1BA1-438D-887F-03DD23639221");
            RockMigrationHelper.DeletePage("7FB33834-F40A-4221-8849-BB8C06903B04"); //  Page: Event Item Detail Page
            RockMigrationHelper.DeleteAttribute("AA08DCED-940E-45B9-AB9B-7B93A609BF13");
            RockMigrationHelper.DeleteBlock("87D0A3C6-D32B-4840-8D1A-B6030B72EA95");
            RockMigrationHelper.DeleteBlock("0C94B3DE-4FA8-4683-B4E9-52BE40D266D9");
            RockMigrationHelper.DeleteBlockType("EC8DFDC5-C177-4208-8ABA-1F85010FBBFF");
            RockMigrationHelper.DeleteBlockType("0320DFB9-7A5A-4DAC-8234-3D504E496D71");
            RockMigrationHelper.DeletePage("B54725E1-3640-4419-B580-2AF77DAF6568"); //  Page: Event Calendar Detail
            RockMigrationHelper.DeleteAttribute("E7907FA7-B586-439B-8DE1-C0185EC4F790");
            RockMigrationHelper.DeleteBlock("367B36C6-2779-451D-BF8C-BC6318D42AA1");
            RockMigrationHelper.DeleteBlockType("041B5C23-5F1F-4B02-A767-FB7F4B1A5345");
            RockMigrationHelper.DeletePage("63990874-0DFF-45FC-9F09-81B0B0D375B4"); //  Page: Calendars
        }
    }
}
