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
    public partial class RegistrationInstanceGroupPlacement : Rock.Migrations.RockMigration
    {
        public override void Up()
        {
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Registrants", "", "6138DA76-BD9A-4373-A55C-F88F155E1B13", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Payments", "", "562D6252-D614-4ED4-B602-D8160066611D", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Fees", "", "B0576A70-CCB3-4E98-B6C4-3D758DD5F609", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Discounts", "", "6EE74759-D11B-4911-9BC8-CF23DE5534B2", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Linkages", "", "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Wait List", "", "E17883C2-6442-4AE5-B561-2C783F7F89C9", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Instance - Placement Groups", "", "0CD950D7-033D-42B1-A53E-108F311DC5BF", "" ); // Site:Rock RMS

            Sql( $@"
                UPDATE [Page]
                SET PageTitle = 'Registrations'
	                ,InternalName = 'Registration Instance - Registrations'
	                ,BrowserTitle = 'Registration Instance'
	                ,BreadCrumbDisplayName = 0
	                ,LayoutId = (
		                SELECT TOP 1 [Id]
		                FROM [Layout]
		                WHERE [Guid] = '{Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE}'
		                )
                WHERE [Guid] = '844DC54B-DAEC-47B3-A63A-712DD6D57793'" );

            Sql( "UPDATE [Page] set PageTitle = 'Wait List Confirmation', InternalName = 'Wait List Confirmation', BrowserTitle = 'Wait List Confirmation' where [Guid] ='4BF84D3F-DE7B-4F8B-814A-1E728E69C105'" );
            Sql( "UPDATE [Page] set PageTitle = 'Wait List', InternalName = 'Registration Instance - Wait List', BrowserTitle = 'Registration Instance - Wait List', BreadCrumbDisplayName = 0 where [Guid] ='E17883C2-6442-4AE5-B561-2C783F7F89C9'" );
            Sql( "UPDATE [Page] set PageTitle = 'Payments', InternalName = 'Registration Instance - Payments', BrowserTitle = 'Registration Instance - Payments', BreadCrumbDisplayName = 0 where [Guid] ='562D6252-D614-4ED4-B602-D8160066611D'" );
            Sql( "UPDATE [Page] set PageTitle = 'Discounts', InternalName = 'Registration Instance - Discounts', BrowserTitle = 'Registration Instance - Discounts', BreadCrumbDisplayName = 0 where [Guid] ='6EE74759-D11B-4911-9BC8-CF23DE5534B2'" );
            Sql( "UPDATE [Page] set PageTitle = 'Fees', InternalName = 'Registration Instance - Fees', BrowserTitle = 'Registration Instance - Fees', BreadCrumbDisplayName = 0 where [Guid] ='B0576A70-CCB3-4E98-B6C4-3D758DD5F609'" );
            Sql( "UPDATE [Page] set PageTitle = 'Linkages', InternalName = 'Registration Instance - Linkages', BrowserTitle = 'Registration Instance - Linkages', BreadCrumbDisplayName = 0 where [Guid] ='8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5'" );
            Sql( "UPDATE [Page] set PageTitle = 'Registrants', InternalName = 'Registration Instance - Registrants', BrowserTitle = 'Registration Instance - Registrants', BreadCrumbDisplayName = 0 where [Guid] ='6138DA76-BD9A-4373-A55C-F88F155E1B13'" );
            Sql( "UPDATE [Page] set PageTitle = 'Placement Groups', InternalName = 'Registration Instance - Placement Groups', BrowserTitle = 'Registration Instance - Placement Groups', BreadCrumbDisplayName = 0 where [Guid] ='0CD950D7-033D-42B1-A53E-108F311DC5BF'" );

            RockMigrationHelper.UpdateBlockType( "Registration Group Placement", "Block to manage group placement for Registration Instances.", "~/Blocks/Event/RegistrationInstanceGroupPlacement.ascx", "Event", "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Discount List", "Displays the discounts related to an event registration instance.", "~/Blocks/Event/RegistrationInstanceDiscountList.ascx", "Event", "6C8954BF-E221-4B2F-AC3B-612DC16BA27D" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Fee List", "Displays the fees related to an event registration instance.", "~/Blocks/Event/RegistrationInstanceFeeList.ascx", "Event", "41CD9629-9327-40D4-846A-1BB8135D130C" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Linkage List", "Displays the linkages associated with an event registration instance.", "~/Blocks/Event/RegistrationInstanceLinkageList.ascx", "Event", "E877FDE1-DEE6-48F8-8150-4E28D5ABB694" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Payment List", "Displays the payments related to an event registration instance.", "~/Blocks/Event/RegistrationInstancePaymentList.ascx", "Event", "762BEE39-15DF-477C-9831-DB5AA73DCB24" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Registrant List", "Displays the list of Registrants related to a Registration Instance.", "~/Blocks/Event/RegistrationInstanceRegistrantList.ascx", "Event", "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Registration List", "Displays the list of Registrations related to a Registration Instance.", "~/Blocks/Event/RegistrationInstanceRegistrationList.ascx", "Event", "A8DB2C89-F80A-43A2-AA53-36C78673F504" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Wait List", "Block for editing the wait list associated with an event registration instance.", "~/Blocks/Event/RegistrationInstanceWaitList.ascx", "Event", "671244E1-747E-436D-B866-13469723B424" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Instance Detail", "Displays the details of a Registration Instance for viewing and editing.", "~/Blocks/Event/RegistrationInstanceDetail.ascx", "Event", "22B67EDB-6D13-4D29-B722-DF45367AA3CB" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Navigation", "Provides the navigation for the tabs navigation section of the Registration Instance Page/Layout", "~/Blocks/Event/RegistrationInstanceNavigation.ascx", "Event", "AF0740C9-BC60-434B-A360-EB70A7CEA108" );

            // Attrib for BlockType: Registration Instance - Registration List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 1, @"", "323B997E-E819-4668-89FA-37EC570E9D08" );
            // Attrib for BlockType: Registration Instance - Registration List:Display Discount Codes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Discount Codes", "DisplayDiscountCodes", "Display Discount Codes", @"Display the discount code used with a payment", 2, @"False", "B1467FFC-FDC9-4E92-81BD-2119E512DB22" );
            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Tool Page", "GroupPlacementToolPage", "Group Placement Tool Page", @"The Page that shows Group Placements for the selected placement type", 0, @"", "D3C7AF94-FB85-45BA-8FD8-87AC9499ED56" );
            // Attrib for BlockType: Registration Instance - Navigation:WaitList Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "WaitList Page", "WaitListPage", "WaitList Page", @"The Page that shows the Wait List", 0, @"", "4269C69A-9AFA-4636-93B6-3D1013F24A2D" );
            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Tool Page", "GroupPlacementToolPage", "Group Placement Tool Page", @"The Page that shows Group Placements for the selected placement type", 0, @"", "D3C7AF94-FB85-45BA-8FD8-87AC9499ED56" );
            // Attrib for BlockType: Registration Instance - Navigation:WaitList Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "WaitList Page", "WaitListPage", "WaitList Page", @"The Page that shows the Wait List", 0, @"", "4269C69A-9AFA-4636-93B6-3D1013F24A2D" );

            // Add Block to Layout: Registration Instance Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "844DC54B-DAEC-47B3-A63A-712DD6D57793".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 0, "F0E4B868-4A40-43E9-A406-782DC94313EC" );
            // Add Block to Page: Registration Instance - Registrations Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "844DC54B-DAEC-47B3-A63A-712DD6D57793".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A8DB2C89-F80A-43A2-AA53-36C78673F504".AsGuid(), "Registration Instance - Registration List", "Main", @"", @"", 0, "926890F9-555E-4DC1-994D-63CA1EC615AC" );
            // Add Block to Page: Registration Instance - Placement Groups Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CD950D7-033D-42B1-A53E-108F311DC5BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A".AsGuid(), "Registration Group Placement", "Main", @"", @"", 0, "4AD0B76F-FDBC-40D3-A3B7-0A2624B41A1A" );
            // Add Block to Page: Registration Instance - Wait List Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E17883C2-6442-4AE5-B561-2C783F7F89C9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "671244E1-747E-436D-B866-13469723B424".AsGuid(), "Registration Instance - Wait List", "Main", @"", @"", 0, "DF9127C7-E5BD-42EC-A1E2-66A7386234F9" );
            // Add Block to Page: Registration Instance - Payments Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "562D6252-D614-4ED4-B602-D8160066611D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "762BEE39-15DF-477C-9831-DB5AA73DCB24".AsGuid(), "Registration Instance - Payment List", "Main", @"", @"", 0, "D23D394E-7F38-4D66-86D6-062631F6508C" );
            // Add Block to Page: Registration Instance - Discounts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6EE74759-D11B-4911-9BC8-CF23DE5534B2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C8954BF-E221-4B2F-AC3B-612DC16BA27D".AsGuid(), "Registration Instance - Discount List", "Main", @"", @"", 0, "24726429-F0A3-4910-8E96-BD683E4992F8" );
            // Add Block to Page: Registration Instance - Fees Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B0576A70-CCB3-4E98-B6C4-3D758DD5F609".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "41CD9629-9327-40D4-846A-1BB8135D130C".AsGuid(), "Registration Instance - Fee List", "Main", @"", @"", 0, "8BB19A65-C821-4AFA-9A79-5C5BCF443DB0" );
            // Add Block to Page: Registration Instance - Linkages Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E877FDE1-DEE6-48F8-8150-4E28D5ABB694".AsGuid(), "Registration Instance - Linkage List", "Main", @"", @"", 0, "9661CB22-270E-44FD-B535-D93457636A2A" );
            // Add Block to Page: Registration Instance - Registrants Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6138DA76-BD9A-4373-A55C-F88F155E1B13".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E".AsGuid(), "Registration Instance - Registrant List", "Main", @"", @"", 0, "C37F7A43-CDB9-4AD4-A713-BC4F4785E45A" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page , Layout: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3C0E6099-9FA2-4811-8B57-F101D47BD7BE", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"20f97a93-7949-4c2a-8a5e-c756fe8585ca" );

            // Attrib Value for Block:Registration Instance Detail, Attribute:Default Account Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "4FA1CCF7-9921-41EE-BC5F-DE358A1E5A89", @"2a6f9e5f-6859-44f1-ab0e-ce9cf6b08ee5" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:MergeField Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder > 0 %}{{ person.SecurityCode }}-{{ person.Age }}yr\&{% endif %}{% endfor %}" );

            // Add Block to Page: Registration Instance - Registrants Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6138DA76-BD9A-4373-A55C-F88F155E1B13".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "DDCB56F8-18D7-4767-A058-6130D29AA82D" );
            // Add Block to Page: Registration Instance - Registrants Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6138DA76-BD9A-4373-A55C-F88F155E1B13".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "4C06654E-F95F-4115-ACE6-7047BFBE1F7B" );
            // Add Block to Page: Registration Instance - Payments Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "562D6252-D614-4ED4-B602-D8160066611D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "9501DE68-C7E7-4BBC-AC4E-B00783BD85E7" );
            // Add Block to Page: Registration Instance - Payments Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "562D6252-D614-4ED4-B602-D8160066611D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "B5FA8BEE-0BB8-48F2-967C-9367D61E47AB" );
            // Add Block to Page: Registration Instance - Fees Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B0576A70-CCB3-4E98-B6C4-3D758DD5F609".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance - Instance Detail", "Main", @"", @"", 0, "884075FE-0A7C-4D39-86EE-120B8A63CBB1" );
            // Add Block to Page: Registration Instance - Fees Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B0576A70-CCB3-4E98-B6C4-3D758DD5F609".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "431854A9-4430-406E-BD19-8BD0E58EE704" );
            // Add Block to Page: Registration Instance - Discounts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6EE74759-D11B-4911-9BC8-CF23DE5534B2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "C8D19431-76DA-4515-B2F9-14B27F00A89D" );
            // Add Block to Page: Registration Instance - Discounts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6EE74759-D11B-4911-9BC8-CF23DE5534B2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "B16BC19E-8B5B-4FD6-92E6-F07BD1800F74" );
            // Add Block to Page: Registration Instance - Linkages Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "CBCF0B24-D667-4919-BB63-DD2914EA4B65" );
            // Add Block to Page: Registration Instance - Linkages Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "3E051538-28BB-4620-B1AA-C3B4BCAE3DD1" );
            // Add Block to Page: Registration Instance - Wait List Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E17883C2-6442-4AE5-B561-2C783F7F89C9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "1AC1CDEE-3CA5-4464-A989-3C8FA2BADDDB" );
            // Add Block to Page: Registration Instance - Wait List Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E17883C2-6442-4AE5-B561-2C783F7F89C9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "F52235B0-5690-4EA6-A0CD-4044B264220B" );
            // Add Block to Page: Registration Instance - Placement Groups Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CD950D7-033D-42B1-A53E-108F311DC5BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "22B67EDB-6D13-4D29-B722-DF45367AA3CB".AsGuid(), "Registration Instance Detail", "Main", @"", @"", 0, "F389C67E-E2C3-44A7-A524-6CD2E5A77BDA" );
            // Add Block to Page: Registration Instance - Placement Groups Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CD950D7-033D-42B1-A53E-108F311DC5BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "Main", @"", @"", 1, "156E9533-F7E7-460A-9A80-9592F8F591F3" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '1AC1CDEE-3CA5-4464-A989-3C8FA2BADDDB'" );  // Page: Registration Instance - Wait List,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '4C06654E-F95F-4115-ACE6-7047BFBE1F7B'" );  // Page: Registration Instance - Registrants,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '884075FE-0A7C-4D39-86EE-120B8A63CBB1'" );  // Page: Registration Instance - Fees,  Zone: Main,  Block: Registration Instance - Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '9501DE68-C7E7-4BBC-AC4E-B00783BD85E7'" );  // Page: Registration Instance - Payments,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'C8D19431-76DA-4515-B2F9-14B27F00A89D'" );  // Page: Registration Instance - Discounts,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'CBCF0B24-D667-4919-BB63-DD2914EA4B65'" );  // Page: Registration Instance - Linkages,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F389C67E-E2C3-44A7-A524-6CD2E5A77BDA'" );  // Page: Registration Instance - Placement Groups,  Zone: Main,  Block: Registration Instance Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '156E9533-F7E7-460A-9A80-9592F8F591F3'" );  // Page: Registration Instance - Placement Groups,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '3E051538-28BB-4620-B1AA-C3B4BCAE3DD1'" );  // Page: Registration Instance - Linkages,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '431854A9-4430-406E-BD19-8BD0E58EE704'" );  // Page: Registration Instance - Fees,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B16BC19E-8B5B-4FD6-92E6-F07BD1800F74'" );  // Page: Registration Instance - Discounts,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B5FA8BEE-0BB8-48F2-967C-9367D61E47AB'" );  // Page: Registration Instance - Payments,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'DDCB56F8-18D7-4767-A058-6130D29AA82D'" );  // Page: Registration Instance - Registrants,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F52235B0-5690-4EA6-A0CD-4044B264220B'" );  // Page: Registration Instance - Wait List,  Zone: Main,  Block: Registration Instance - Navigation
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '24726429-F0A3-4910-8E96-BD683E4992F8'" );  // Page: Registration Instance - Discounts,  Zone: Main,  Block: Registration Instance - Discount List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '4AD0B76F-FDBC-40D3-A3B7-0A2624B41A1A'" );  // Page: Registration Instance - Placement Groups,  Zone: Main,  Block: Registration Group Placement
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '8BB19A65-C821-4AFA-9A79-5C5BCF443DB0'" );  // Page: Registration Instance - Fees,  Zone: Main,  Block: Registration Instance - Fee List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9661CB22-270E-44FD-B535-D93457636A2A'" );  // Page: Registration Instance - Linkages,  Zone: Main,  Block: Registration Instance - Linkage List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'C37F7A43-CDB9-4AD4-A713-BC4F4785E45A'" );  // Page: Registration Instance - Registrants,  Zone: Main,  Block: Registration Instance - Registrant List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'D23D394E-7F38-4D66-86D6-062631F6508C'" );  // Page: Registration Instance - Payments,  Zone: Main,  Block: Registration Instance - Payment List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'DF9127C7-E5BD-42EC-A1E2-66A7386234F9'" );  // Page: Registration Instance - Wait List,  Zone: Main,  Block: Registration Instance - Wait List
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
