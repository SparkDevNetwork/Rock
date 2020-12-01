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
    public partial class BusAndFarmPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            BusUp();
            FarmUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FarmDown();
            BusDown();
        }

        private void FarmUp()
        {
            // Add Page Web Farm to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Web Farm", "", "249BE98D-9DDE-4B19-9D97-9C76D9EA3056", "fa fa-network-wired" );

            // Add/Update BlockType Log
            RockMigrationHelper.UpdateBlockType( "Log", "Shows a list of Web Farm logs.", "~/Blocks/Farm/Log.ascx", "Farm", "63ADDB5A-75D6-4E86-A031-98B3451C49A3" );

            // Add/Update BlockType Web Farm Settings
            RockMigrationHelper.UpdateBlockType( "Web Farm Settings", "Displays the details of the Web Farm.", "~/Blocks/Farm/WebFarmSettings.ascx", "Farm", "4280625A-C69A-4B47-A4D3-89B61F43C967" );

            // Add Block Web Farm Settings to Page: Web Farm, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "249BE98D-9DDE-4B19-9D97-9C76D9EA3056".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4280625A-C69A-4B47-A4D3-89B61F43C967".AsGuid(), "Web Farm Settings", "Main", @"", @"", 0, "E6880262-1A1C-4857-929C-958CAD8A6DEF" );

            // Add Block Log to Page: Web Farm, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "249BE98D-9DDE-4B19-9D97-9C76D9EA3056".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "63ADDB5A-75D6-4E86-A031-98B3451C49A3".AsGuid(), "Log", "Main", @"", @"", 1, "A99E9D67-3B9F-40D0-8C64-E95ECFE17D95" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Web Farm,  Zone: Main,  Block: Log
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'A99E9D67-3B9F-40D0-8C64-E95ECFE17D95'" );

            // Update Order for Page: Web Farm,  Zone: Main,  Block: Web Farm Settings
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'E6880262-1A1C-4857-929C-958CAD8A6DEF'" );

            // Hide pages in system settings
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Model.DisplayInNavWhen.Never} WHERE [Guid] = '{SystemGuid.Page.WEB_FARM}';" );
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) Model.DisplayInNavWhen.Never} WHERE [Guid] = '{SystemGuid.Page.BUS}';" );
        }

        private void FarmDown()
        {
            // Remove Block: Log, from Page: Web Farm, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A99E9D67-3B9F-40D0-8C64-E95ECFE17D95" );

            // Remove Block: Web Farm Settings, from Page: Web Farm, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E6880262-1A1C-4857-929C-958CAD8A6DEF" );

            // Delete BlockType Web Farm Settings
            RockMigrationHelper.DeleteBlockType( "4280625A-C69A-4B47-A4D3-89B61F43C967" ); // Web Farm Settings

            // Delete BlockType Log
            RockMigrationHelper.DeleteBlockType( "63ADDB5A-75D6-4E86-A031-98B3451C49A3" ); // Log

            // Delete Page Web Farm from Site:Rock RMS
            RockMigrationHelper.DeletePage( "249BE98D-9DDE-4B19-9D97-9C76D9EA3056" ); //  Page: Web Farm, Layout: Full Width, Site: Rock RMS
        }

        private void BusUp()
        {
            // Add Page Message Bus to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Message Bus", "", Rock.SystemGuid.Page.BUS, "fa fa-bus" );

            // Add Page Transport to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.BUS, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Transport", "", Rock.SystemGuid.Page.BUS_TRANSPORT, "fa fa-bus" );

            // Add Page Queue to Site:Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.BUS, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Queue", "", Rock.SystemGuid.Page.BUS_QUEUE, "fa fa-bus" );

            // Add/Update BlockType Bus Status
            RockMigrationHelper.UpdateBlockType( "Bus Status", "Status of the Rock Message bus", "~/Blocks/Bus/BusStatus.ascx", "Bus", "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" );

            // Add bus status block to bus page
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB".AsGuid(), "Bus Status", "Main", @"", @"", 1, "86978AA5-BA0A-4593-89DA-3E9F3486D531" );

            // Add Block Transports to Page: Message Bus Transport, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS_TRANSPORT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "21F5F466-59BC-40B2-8D73-7314D936C3CB".AsGuid(), "Transports", "Main", @"", @"", 1, "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC" );

            // Add Block Restart Required Notification to Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS_TRANSPORT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Restart Required Notification", "Main", @"", @"", 0, "80E9E506-8E74-4D89-A1BC-345449618D18" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Message Bus,  Zone: Main,  Block: Restart Required Notification
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '80E9E506-8E74-4D89-A1BC-345449618D18'" );

            // Update Order for Page: Message Bus,  Zone: Main,  Block: Transports
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC'" );

            // Add/Update HtmlContent for Block: Restart Required Notification
            RockMigrationHelper.UpdateHtmlContentBlock(
                "80E9E506-8E74-4D89-A1BC-345449618D18",
@"<div class=""alert alert-warning"">
    Any changes made to the Message Bus configuration require a Rock restart to take affect.
</div>",
                "15833A55-B710-463B-8060-6FD368E56968" );

            // Block Attribute Value for Transports ( Page: Message Bus Transports, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.Bus.Transport.TransportContainer, Rock" );

            // Block Attribute Value for Transports ( Page: Message Bus Transports, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "A4889D7B-87AA-419D-846C-3E618E79D875", @"False" );

            // Block Attribute Value for Transports ( Page: Message Bus Transports, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "A8F1D1B8-0709-497C-9DCB-44826F26AE7A", @"False" );

            // Block Attribute Value for Transports ( Page: Message Bus Transports, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF", @"True" );

            // Block Attribute Value for Transports ( Page: Message Bus Transports, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC", "07BBF752-6CB5-4591-989F-05BCE78BC73C", @"False" );

            // Add/Update BlockType Consumer List
            RockMigrationHelper.UpdateBlockType( "Consumer List", "Shows a list of all message bus consumers.", "~/Blocks/Bus/ConsumerList.ascx", "Bus", "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5" );

            // Add Block Consumer List to Page: Queue, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS_QUEUE.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5".AsGuid(), "Consumer List", "Main", @"", @"", 2, "E9BDBA5E-264C-4227-B304-338A788A730C" );

            // Add/Update BlockType Consumer List
            RockMigrationHelper.UpdateBlockType( "Queue List", "Shows a list of all message bus queues.", "~/Blocks/Bus/QueueList.ascx", "Bus", "F9872CD9-EF32-4791-B0A9-1D104250AB18" );

            // Add Queue List to Page: Bus, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "F9872CD9-EF32-4791-B0A9-1D104250AB18".AsGuid(), "Queue List", "Main", @"", @"", 2, "8C772DE3-6DD3-46DB-9FD2-5BDFAC36EB59" );

            // Add/Update BlockType Queue Detail
            RockMigrationHelper.UpdateBlockType( "Queue Detail", "Shows details of a bus queue.", "~/Blocks/Bus/QueueDetail.ascx", "Bus", "8D6C81EB-2FFE-41CB-9A2B-FB70857E5761" );

            // Add Queue Detail to Page: Queue, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.BUS_QUEUE.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D6C81EB-2FFE-41CB-9A2B-FB70857E5761".AsGuid(), "Queue List", "Main", @"", @"", 2, "9A11DD0C-6F56-42A5-B149-D8B5F804E8EE" );

            // Update Order for Page: Queue,  Zone: Main,  Block: Consumer List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '9A11DD0C-6F56-42A5-B149-D8B5F804E8EE'" );
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E9BDBA5E-264C-4227-B304-338A788A730C'" );

            // Attribute for BlockType: Bus Status:Transport Select Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transport Select Page", "TransportSelectPage", "Transport Select Page", @"The page where the transport for the bus can be selected", 1, @"10E34A5D-D967-457D-9DF1-A1D33DA9D100", "D7ED6C04-A239-4197-9548-E3FC351E3B63" );

            // Attribute for BlockType: Queue List:Queue Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9872CD9-EF32-4791-B0A9-1D104250AB18", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Queue Detail Page", "QueueDetailPage", "Queue Detail Page", @"The page where the queue can be configured", 1, @"45E865C0-CD2D-43CD-AA8A-BF5DBF537587", "330BC581-277C-4987-8AC8-35A4A0E41E3C" );
        }

        private void BusDown()
        {
            // Queue Detail Page Attribute for BlockType: Queue List
            RockMigrationHelper.DeleteAttribute( "330BC581-277C-4987-8AC8-35A4A0E41E3C" );

            // Transport Select Page Attribute for BlockType: Bus Status
            RockMigrationHelper.DeleteAttribute( "D7ED6C04-A239-4197-9548-E3FC351E3B63" );

            // Remove Block: Restart Required Notification, from Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "80E9E506-8E74-4D89-A1BC-345449618D18" );

            // Remove Block: Transports, from Page: Message Bus, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F1337CC0-D8A4-49EA-9877-B7CCB4D76DEC" );

            // Delete BlockType Bus Controls
            RockMigrationHelper.DeleteBlockType( "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" ); // Bus Controls

            // Delete BlockType Consumer List
            RockMigrationHelper.DeleteBlockType( "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5" ); // Consumer List

            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.BUS_QUEUE );
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.BUS_TRANSPORT );

            // Delete Page Message Bus from Site:Rock RMS
            RockMigrationHelper.DeletePage( "0FF43CC8-1C29-4882-B2F6-7B6F4C25FE41" ); //  Page: Message Bus, Layout: Full Width, Site: Rock RMS
        }
    }
}
