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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 100, "1.10.0" )]
    public class RegistrationInstanceGroupPlacement : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "RegistrationInstance", "Registration Instance", "", "194CC66E-8CC1-4087-AC14-955CDCB70CDF" ); // Site:Rock RMS

            // Update Layout of the RegistrationInstance page to be the Registration Instance layout
            Sql( @"update [Page] set LayoutId = ( select top 1 Id from Layout where [Guid] = '194CC66E-8CC1-4087-AC14-955CDCB70CDF') where [Guid] = '844DC54B-DAEC-47B3-A63A-712DD6D57793'" );

            // Update Zone of the RegistrationInstanceDetail block
            Sql( @"update [Block] set [Zone] = 'RegistrationInstanceDetail' where [Guid] = '5F44A3A8-500B-4C89-95CA-8C4246B53C3F'" );

            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Registrants", "", "6138DA76-BD9A-4373-A55C-F88F155E1B13", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Payments", "", "562D6252-D614-4ED4-B602-D8160066611D", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Fees", "", "B0576A70-CCB3-4E98-B6C4-3D758DD5F609", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Discounts", "", "6EE74759-D11B-4911-9BC8-CF23DE5534B2", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Linkages", "", "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Wait List", "", "E17883C2-6442-4AE5-B561-2C783F7F89C9", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "194CC66E-8CC1-4087-AC14-955CDCB70CDF", "Registration Instance - Placement Groups", "", "0CD950D7-033D-42B1-A53E-108F311DC5BF", "" ); // Site:Rock RMS
            

            Sql( "UPDATE [Page] set PageTitle = 'Registrations', InternalName = 'Registration Instance - Registrations', BrowserTitle = 'Registration Instance' where [Guid] ='844DC54B-DAEC-47B3-A63A-712DD6D57793'" );
            Sql( "UPDATE [Page] set PageTitle = 'Wait List', InternalName = 'Registration Instance - Wait List', BrowserTitle = 'Registration Instance - Wait List' where [Guid] ='E17883C2-6442-4AE5-B561-2C783F7F89C9'" );
            Sql( "UPDATE [Page] set PageTitle = 'Payments', InternalName = 'Registration Instance - Payments', BrowserTitle = 'Registration Instance - Payments' where [Guid] ='562D6252-D614-4ED4-B602-D8160066611D'" );
            Sql( "UPDATE [Page] set PageTitle = 'Discounts', InternalName = 'Registration Instance - Discounts', BrowserTitle = 'Registration Instance - Discounts' where [Guid] ='6EE74759-D11B-4911-9BC8-CF23DE5534B2'" );
            Sql( "UPDATE [Page] set PageTitle = 'Fees', InternalName = 'Registration Instance - Fees', BrowserTitle = 'Registration Instance - Fees' where [Guid] ='B0576A70-CCB3-4E98-B6C4-3D758DD5F609'" );
            Sql( "UPDATE [Page] set PageTitle = 'Linkages', InternalName = 'Registration Instance - Linkages', BrowserTitle = 'Registration Instance - Linkages' where [Guid] ='8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5'" );
            Sql( "UPDATE [Page] set PageTitle = 'Registrants', InternalName = 'Registration Instance - Registrants', BrowserTitle = 'Registration Instance - Registrants' where [Guid] ='6138DA76-BD9A-4373-A55C-F88F155E1B13'" );
            Sql( "UPDATE [Page] set PageTitle = 'Placement Groups', InternalName = 'Registration Instance - Placement Groups', BrowserTitle = 'Registration Instance - Placement Groups' where [Guid] ='0CD950D7-033D-42B1-A53E-108F311DC5BF'" );

            RockMigrationHelper.UpdateBlockType( "Registration Group Placement", "Block to manage group placement for Registration Instances.", "~/Blocks/Event/RegistrationInstanceGroupPlacement.ascx", "Event", "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Discount List", "Displays the discounts related to an event registration instance.", "~/Blocks/Event/RegistrationInstanceDiscountList.ascx", "Event", "6C8954BF-E221-4B2F-AC3B-612DC16BA27D" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Fee List", "Displays the fees related to an event registration instance.", "~/Blocks/Event/RegistrationInstanceFeeList.ascx", "Event", "41CD9629-9327-40D4-846A-1BB8135D130C" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Linkage List", "Displays the linkages associated with an event registration instance.", "~/Blocks/Event/RegistrationInstanceLinkageList.ascx", "Event", "E877FDE1-DEE6-48F8-8150-4E28D5ABB694" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Payment List", "Displays the payments related to an event registration instance.", "~/Blocks/Event/RegistrationInstancePaymentList.ascx", "Event", "762BEE39-15DF-477C-9831-DB5AA73DCB24" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Registrant List", "Displays the list of Registrants related to a Registration Instance.", "~/Blocks/Event/RegistrationInstanceRegistrantList.ascx", "Event", "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Registration List", "Displays the list of Registrations related to a Registration Instance.", "~/Blocks/Event/RegistrationInstanceRegistrationList.ascx", "Event", "A8DB2C89-F80A-43A2-AA53-36C78673F504" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Wait List", "Block for editing the wait list associated with an event registration instance.", "~/Blocks/Event/RegistrationInstanceWaitList.ascx", "Event", "671244E1-747E-436D-B866-13469723B424" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Instance Detail", "Displays the details of a Registration Instance for viewing and editing.", "~/Blocks/Event/RegistrationInstanceInstanceDetail.ascx", "Event", "779C421A-7886-4AB5-AA45-5185E7BCCF28" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance Detail (Old Big One)", "Template block for editing an event registration instance.", "~/Blocks/Event/RegistrationInstanceDetail_OldBigOne.ascx", "Event", "F1B90A23-601E-4FB5-934C-396199E17F6B" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance - Navigation", "Provides the navigation for the tabs navigation section of the Registration Instance Page/Layout", "~/Blocks/Event/RegistrationInstanceNavigation.ascx", "Event", "AF0740C9-BC60-434B-A360-EB70A7CEA108" );

            // Attrib for BlockType: Registration Instance - Registration List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 1, @"", "323B997E-E819-4668-89FA-37EC570E9D08" );
            // Attrib for BlockType: Registration Instance - Registration List:Display Discount Codes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8DB2C89-F80A-43A2-AA53-36C78673F504", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Discount Codes", "DisplayDiscountCodes", "Display Discount Codes", @"Display the discount code used with a payment", 2, @"False", "B1467FFC-FDC9-4E92-81BD-2119E512DB22" );
            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Tool Page", "GroupPlacementToolPage", "Group Placement Tool Page", @"The Page that shows Group Placements for the selected placement type", 0, @"", "D3C7AF94-FB85-45BA-8FD8-87AC9499ED56" );
            // Attrib for BlockType: Registration Instance - Navigation:WaitList Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "WaitList Page", "WaitListPage", "WaitList Page", @"The Page that shows the Wait List", 0, @"", "4269C69A-9AFA-4636-93B6-3D1013F24A2D" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Default Account Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "4FA1CCF7-9921-41EE-BC5F-DE358A1E5A89", @"2a6f9e5f-6859-44f1-ab0e-ce9cf6b08ee5" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:MergeField Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "51EB8583-55EA-4431-8B66-B5BD0F83D81E", @"{% for person in People %}{% assign remainder = forloop.index | Modulo:2 %}{% if remainder > 0 %}{{ person.SecurityCode }}-{{ person.Age }}yr\&{% endif %}{% endfor %}" );

            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Tool Page", "GroupPlacementToolPage", "Group Placement Tool Page", @"The Page that shows Group Placements for the selected placement type", 0, @"", "D3C7AF94-FB85-45BA-8FD8-87AC9499ED56" );
            // Attrib for BlockType: Registration Instance - Navigation:WaitList Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AF0740C9-BC60-434B-A360-EB70A7CEA108", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "WaitList Page", "WaitListPage", "WaitList Page", @"The Page that shows the Wait List", 0, @"", "4269C69A-9AFA-4636-93B6-3D1013F24A2D" );


            // Add Block to Layout: Registration Instance Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "194CC66E-8CC1-4087-AC14-955CDCB70CDF".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Navigation", @"", @"", 0, "3C0E6099-9FA2-4811-8B57-F101D47BD7BE" );
            // Add Block to Layout: Registration Instance Site: Rock RMS
            RockMigrationHelper.AddBlock( true, null, "194CC66E-8CC1-4087-AC14-955CDCB70CDF".AsGuid(), "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AF0740C9-BC60-434B-A360-EB70A7CEA108".AsGuid(), "Registration Instance - Navigation", "TabNavigation", @"", @"", 0, "F0E4B868-4A40-43E9-A406-782DC94313EC" );
            // Add Block to Page: Registration Instance - Registrations Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "844DC54B-DAEC-47B3-A63A-712DD6D57793".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A8DB2C89-F80A-43A2-AA53-36C78673F504".AsGuid(), "Registration Instance - Registration List", "RegistrationInstanceTabDetail", @"", @"", 0, "926890F9-555E-4DC1-994D-63CA1EC615AC" );
            // Add Block to Page: Group Placement Sandbox Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CA6E6B78-EED1-4725-90A4-077DBA509602".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A".AsGuid(), "Registration Group Placement", "Main", @"", @"", 0, "C76395C5-801D-41F7-8EB8-48269D4320AD" );
            // Add Block to Page: Registration Instance - Placement Groups Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0CD950D7-033D-42B1-A53E-108F311DC5BF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A".AsGuid(), "Registration Group Placement", "RegistrationInstanceTabDetail", @"", @"", 0, "4AD0B76F-FDBC-40D3-A3B7-0A2624B41A1A" );
            // Add Block to Page: Registration Instance - Wait List Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E17883C2-6442-4AE5-B561-2C783F7F89C9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "671244E1-747E-436D-B866-13469723B424".AsGuid(), "Registration Instance - Wait List", "RegistrationInstanceTabDetail", @"", @"", 0, "DF9127C7-E5BD-42EC-A1E2-66A7386234F9" );
            // Add Block to Page: Registration Instance - Payments Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "562D6252-D614-4ED4-B602-D8160066611D".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "762BEE39-15DF-477C-9831-DB5AA73DCB24".AsGuid(), "Registration Instance - Payment List", "RegistrationInstanceTabDetail", @"", @"", 0, "D23D394E-7F38-4D66-86D6-062631F6508C" );
            // Add Block to Page: Registration Instance - Discounts Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6EE74759-D11B-4911-9BC8-CF23DE5534B2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C8954BF-E221-4B2F-AC3B-612DC16BA27D".AsGuid(), "Registration Instance - Discount List", "RegistrationInstanceTabDetail", @"", @"", 0, "24726429-F0A3-4910-8E96-BD683E4992F8" );
            // Add Block to Page: Registration Instance - Fees Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B0576A70-CCB3-4E98-B6C4-3D758DD5F609".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "41CD9629-9327-40D4-846A-1BB8135D130C".AsGuid(), "Registration Instance - Fee List", "RegistrationInstanceTabDetail", @"", @"", 0, "8BB19A65-C821-4AFA-9A79-5C5BCF443DB0" );
            // Add Block to Page: Registration Instance - Linkages Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8C2C0EDB-60AD-4FA3-AEDA-45B972CA8CC5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E877FDE1-DEE6-48F8-8150-4E28D5ABB694".AsGuid(), "Registration Instance - Linkage List", "RegistrationInstanceTabDetail", @"", @"", 0, "9661CB22-270E-44FD-B535-D93457636A2A" );
            // Add Block to Page: Registration Instance - Registrants Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6138DA76-BD9A-4373-A55C-F88F155E1B13".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E".AsGuid(), "Registration Instance - Registrant List", "RegistrationInstanceTabDetail", @"", @"", 0, "C37F7A43-CDB9-4AD4-A713-BC4F4785E45A" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
