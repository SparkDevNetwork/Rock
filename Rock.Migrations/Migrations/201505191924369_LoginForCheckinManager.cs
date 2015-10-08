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
    using Rock.Security;
    
    /// <summary>
    ///
    /// </summary>
    public partial class LoginForCheckinManager : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            //
            // change Sms to SMS: Nick Airdo
            //

            Sql( @"UPDATE [EntityType] SET [FriendlyName] = 'SMS' WHERE [Guid] = '4BC02764-512A-4A10-ACDE-586F71D8A8BD'" );

            //
            // add business link: Nick Airdo
            //
            RockMigrationHelper.AddBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Business Link", "AddBusinessLink", "", "Select the page where a new business can be added. If specified, a link will be shown which will open in a new window when clicked", 0, @"", "B5327385-CD67-4519-B83D-1DA1E438356F" );

            // Add Business Link value (of the existing Business Detail page) on the existing TransactionMatching block instance
            RockMigrationHelper.AddBlockAttributeValue( "A18A0A0A-0B71-43B4-B830-44B802C272D4", "B5327385-CD67-4519-B83D-1DA1E438356F", @"d2b43273-c64f-4f57-9aae-9571e1982bac" );

            //
            // add login block for checkin manager
            //

            RockMigrationHelper.AddPage( "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Login", "", "31F51DBB-AC84-4724-9219-B46FADAB9CB2", "" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( "31F51DBB-AC84-4724-9219-B46FADAB9CB2", "8305704F-928D-4379-967A-253E576E0923", "Forgot Password", "", "11A5A9D4-5BCF-44B4-9F28-B7841A33E6FB", "" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( "31F51DBB-AC84-4724-9219-B46FADAB9CB2", "8305704F-928D-4379-967A-253E576E0923", "Account Confirmation", "", "13F63F5D-23C4-4E07-868F-760B240E6023", "" ); // Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( "31F51DBB-AC84-4724-9219-B46FADAB9CB2", "8305704F-928D-4379-967A-253E576E0923", "New Account", "", "B99BF92B-E66A-4485-ACD8-E84A5551425A", "" ); // Site:Rock Check-in Manager

            // Add Block to Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "31F51DBB-AC84-4724-9219-B46FADAB9CB2", "", "7B83D513-1178-429E-93FF-E76430E038E4", "Login", "Main", "", "", 0, "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C" );

            // Add Block to Page: Forgot Password, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "11A5A9D4-5BCF-44B4-9F28-B7841A33E6FB", "", "02B3D7D1-23CE-4154-B602-F4A15B321757", "Forgot Username", "Main", "", "", 0, "3928D796-2D9F-4433-B01E-6F465FBD55D9" );

            // Add Block to Page: Account Confirmation, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "13F63F5D-23C4-4E07-868F-760B240E6023", "", "734DFF21-7465-4E02-BFC3-D40F7A65FB60", "Confirm Account", "Main", "", "", 0, "D4816739-E850-45A7-A14A-A67B47210645" );

            // Add Block to Page: New Account, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( "B99BF92B-E66A-4485-ACD8-E84A5551425A", "", "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "Account Entry", "Main", "", "", 0, "09FDF8D1-97C2-40B8-90E3-5E369C9D1A7F" );


            // Attrib Value for Block:Login, Attribute:Help Page Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C", "FCDD819E-B566-4ED5-8252-BCD37ED043C1", @"11a5a9d4-5bcf-44b4-9f28-b7841a33e6fb" );

            // Attrib Value for Block:Login, Attribute:Hide New Account Option Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C", "7D47046D-5D66-45BB-ACFA-7460DE112FC2", @"True" );

            // Attrib Value for Block:Login, Attribute:Remote Authorization Types Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C", "8A09E6E2-3A9C-4D70-B03D-43D8FCB77D78", @"" );

            // Attrib Value for Block:Login, Attribute:Prompt Message Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C", "B2ABA418-32EF-4310-A1EA-3C76A2375979", @"" );


            // Attrib Value for Block:Forgot Username, Attribute:Forgot Username Email Template Page: Forgot Password, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "3928D796-2D9F-4433-B01E-6F465FBD55D9", "D924EB46-061F-499E-AB00-B439748A1347", @"113593ff-620e-4870-86b1-7a0ec0409208" );

            // Attrib Value for Block:Forgot Username, Attribute:Confirmation Page Page: Forgot Password, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "3928D796-2D9F-4433-B01E-6F465FBD55D9", "D85C2AAD-3635-46C5-8BBC-48C5A32F4AF0", @"13f63f5d-23c4-4e07-868f-760b240e6023" );

            // Attrib Value for Block:Confirm Account, Attribute:New Account Page Page: Account Confirmation, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlockAttributeValue( "D4816739-E850-45A7-A14A-A67B47210645", "A2BD1864-BCF1-4A7C-B21B-220A46D22290", @"b99bf92b-e66a-4485-acd8-e84a5551425a" );

            // configure the login page for the site
            Sql( @"DECLARE @LoginPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '31F51DBB-AC84-4724-9219-B46FADAB9CB2')
                    UPDATE [Site] 
	                    SET [LoginPageId] = @LoginPageId
	                    WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC'" );

            // set security on the page
            RockMigrationHelper.AddSecurityAuthForPage( "31F51DBB-AC84-4724-9219-B46FADAB9CB2", 0, Authorization.VIEW, true, null, 1, "3F7D5F50-F3BB-BFAB-4BF0-DE2BE4C71957" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // remove business link
            //
            RockMigrationHelper.DeleteAttribute( "B5327385-CD67-4519-B83D-1DA1E438356F" );

            //
            // remove login pages
            //

            // blank login page on site
            Sql( @"UPDATE [Site] 
	                SET [LoginPageId] = null
	                WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC'" );

            Sql( @"DELETE FROM [Auth] WHERE [Guid] = '3F7D5F50-F3BB-BFAB-4BF0-DE2BE4C71957'" );

            // Remove Block: Account Entry, from Page: New Account, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "09FDF8D1-97C2-40B8-90E3-5E369C9D1A7F" );
            // Remove Block: Confirm Account, from Page: Account Confirmation, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "D4816739-E850-45A7-A14A-A67B47210645" );
            // Remove Block: Forgot Username, from Page: Forgot Password, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "3928D796-2D9F-4433-B01E-6F465FBD55D9" );
            // Remove Block: Login, from Page: Login, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "7BE7C09A-3C2D-4AD9-8DB6-0E77DA8D237C" );
            RockMigrationHelper.DeletePage( "B99BF92B-E66A-4485-ACD8-E84A5551425A" ); //  Page: New Account, Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeletePage( "13F63F5D-23C4-4E07-868F-760B240E6023" ); //  Page: Account Confirmation, Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeletePage( "11A5A9D4-5BCF-44B4-9F28-B7841A33E6FB" ); //  Page: Forgot Password, Layout: Full Width, Site: Rock Check-in Manager
            RockMigrationHelper.DeletePage( "31F51DBB-AC84-4724-9219-B46FADAB9CB2" ); //  Page: Login, Layout: Full Width, Site: Rock Check-in Manager
           
            
        }
    }
}
