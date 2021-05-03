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
    public partial class BusinessConversion : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpAddBusinessConversion();

            UpIcal();

            UpPersonalDevicesFixes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DownAddBusinessConversion();

            DownIcal();
        }

        private void UpAddBusinessConversion()
        {
            // Page: Business Conversion              
            RockMigrationHelper.AddPage( true, "F4DF4899-2D44-4997-BA9B-9D2C64958A20","D65F783D-87A9-4CC9-8110-E83466A0EADB","Business Conversion","","94B07FB1-41C1-4755-87E4-0892406D1F3D",""); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "94B07FB1-41C1-4755-87E4-0892406D1F3D", "convertbusiness" );
            RockMigrationHelper.UpdateBlockType( "Convert Business", "Allows you to convert a Person record into a Business and vice-versa.", "~/Blocks/Finance/ConvertBusiness.ascx", "Finance", "155BC217-1B29-4EFA-A7EA-29C075AE96B3" );
            // Add Block to Page: Business Conversion, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "94B07FB1-41C1-4755-87E4-0892406D1F3D","","155BC217-1B29-4EFA-A7EA-29C075AE96B3","Convert Business","Main","","",0,"A4C78D56-34F7-4E8B-A8D2-A2CB66CDA515");
            // Attrib for BlockType: Convert Business:Default Connection Status              
            RockMigrationHelper.UpdateBlockTypeAttribute("155BC217-1B29-4EFA-A7EA-29C075AE96B3","59D5A94C-94A0-4630-B80A-BB25697D74C7","Default Connection Status","DefaultConnectionStatus","","The default connection status to use when converting a business to a person.",0,@"","D5A83572-EA10-4951-A860-B2E26017EF6D");
            // Attrib Value for Block:Convert Business, Attribute:Default Connection Status Page: Business Conversion, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("A4C78D56-34F7-4E8B-A8D2-A2CB66CDA515","D5A83572-EA10-4951-A860-B2E26017EF6D",@"39f491c5-d6ac-4a9b-8ac0-c431cb17d588");

            // Add Block to Page: Businesses, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "F4DF4899-2D44-4997-BA9B-9D2C64958A20","","19B61D65-37E3-459F-A44F-DEF0089118A3","Action Links","Main",@"",@"",0,"D9B9E00F-399E-4A13-84FF-643E1C90118A");   

            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'D9B9E00F-399E-4A13-84FF-643E1C90118A'" );  // Page: Businesses,  Zone: Main,  Block: Action Links
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '04E68378-E2F6-465B-925A-D8B124858C44'" );  // Page: Businesses,  Zone: Main,  Block: Business List
            // Add/Update HtmlContent for Block: Action Links              
            RockMigrationHelper.UpdateHtmlContentBlock("D9B9E00F-399E-4A13-84FF-643E1C90118A",@"<div class=""clearfix margin-b-sm"">     <a href=""/convertbusiness"" class=""btn btn-default btn-xs pull-right""><i class=""fa fa-exchange""></i> Convert Person/Business</a> </div>","19496B8B-84BB-4C90-BD75-E4D5DE8DBE48");   
        }


        private void DownAddBusinessConversion()
        {
            RockMigrationHelper.DeleteBlockType( "19496B8B-84BB-4C90-BD75-E4D5DE8DBE48" ); // Convert Business Action Link

            RockMigrationHelper.DeleteAttribute( "D5A83572-EA10-4951-A860-B2E26017EF6D" );
            RockMigrationHelper.DeleteBlock( "A4C78D56-34F7-4E8B-A8D2-A2CB66CDA515" );
            RockMigrationHelper.DeleteBlockType( "155BC217-1B29-4EFA-A7EA-29C075AE96B3" );
            RockMigrationHelper.DeletePage( "94B07FB1-41C1-4755-87E4-0892406D1F3D" ); //  Page: Business Conversion
        }

        private void UpIcal()
        {
            RockMigrationHelper.AddDefinedValue( "C3D44004-6951-44D9-8560-8567D705A48B", "Default iCal Description", "The default iCal description used by the Event Calendar Feed if one is not specified. All iCal templates will use the MIME Type of 'text/calendar'.", "DCBA4862-73E9-49B5-8AD5-08E17BE68025", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DCBA4862-73E9-49B5-8AD5-08E17BE68025", "1E13E409-B568-45D0-B4B6-556C87D61232", @"{{ EventItem.Description }}
                {{ EventItemOccurrence.Note }}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DCBA4862-73E9-49B5-8AD5-08E17BE68025", "4FBF9D1A-06A4-4941-B9F4-85D2C4C12F2A", @"text/calendar" );
        }

        private void DownIcal()
        {
            RockMigrationHelper.DeleteAttribute( "1E13E409-B568-45D0-B4B6-556C87D61232" ); // Template
            RockMigrationHelper.DeleteAttribute( "4FBF9D1A-06A4-4941-B9F4-85D2C4C12F2A" ); // MimeType
            RockMigrationHelper.DeleteDefinedValue( "DCBA4862-73E9-49B5-8AD5-08E17BE68025" ); // Default iCal Description
        }

        private void UpPersonalDevicesFixes()
        {
            // add missing device icon css classes
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_MOBILE, "DC0E00D2-7694-410E-82C0-E99A097D0A30", "fa fa-mobile-alt" );
            RockMigrationHelper.AddDefinedValueAttributeValue( SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER, "DC0E00D2-7694-410E-82C0-E99A097D0A30", "fa fa-desktop" );
        }
    }
}
