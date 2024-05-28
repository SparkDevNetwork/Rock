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
    public partial class UpdateHistoryForConnectionRequest : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.History", "Verb", c => c.String(maxLength: 50));
            AddedConnectionRequestHistoryUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddedConnectionRequestHistoryDown();
            AlterColumn( "dbo.History", "Verb", c => c.String( maxLength: 40 ) );
        }

        private void AddedConnectionRequestHistoryUp()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route for Connection Board            
            RockMigrationHelper.AddPageRoute( "4FBCEB52-8892-4035-BDEA-112A494BE81F", "ConnectionRequest/{ConnectionRequestId}", "717F02B0-E112-4602-9AAC-2200E9212C3E" );
#pragma warning restore CS0618 // Type or member is obsolete

            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Connection Request", "", "", "4B472C6A-1ACF-481E-A2D4-9C44436BBCF5", 7, "6F09163D-7DDD-4E1E-8D18-D7CAA04451A7" );

            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C405062-72BB-4362-9738-90C9ED5ACDDE')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [Category] where [Guid] = '4B472C6A-1ACF-481E-A2D4-9C44436BBCF5')

                DELETE FROM [AttributeValue] WHERE [Guid] = 'A258B14B-6864-40D8-ADA9-66F52F0122CE'

                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'~/ConnectionRequest/{0}','A258B14B-6864-40D8-ADA9-66F52F0122CE')" );

            // Add Page Connection Request His tory to Site:Rock RMS              
            RockMigrationHelper.AddPage( true, "4FBCEB52-8892-4035-BDEA-112A494BE81F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Request History", "", "26F4DCE0-E638-4BC5-8AAB-11E75116E1FB", "" );
            // Add Block History Log to Page: Connection Request History, Site: Rock RMS   
            RockMigrationHelper.AddBlock( true, "26F4DCE0-E638-4BC5-8AAB-11E75116E1FB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0".AsGuid(), "History Log", "Main", @"", @"", 0, "5667A5A7-DFE1-4BD4-93FF-21B71E3A07EA" );
            // Attribute for BlockType: Connection Request Board:Connection Request History Page       
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28DBE708-E99B-4879-A64D-656C030D25B5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Request History Page", "ConnectionRequestHistoryPage", "Connection Request History Page", @"Page used to display history details.", 15, @"4E237286-B715-4109-A578-C1445EC02707", "A8AB6C09-D87A-4220-A801-73A43A24C3E2" );
            // Add Block Attribute Value              //   Block: Connection Request Board              //   BlockType: Connection Request Board              //   Block Location: Page=Connection Board, Site=Rock RMS              //   Attribute: Connection Request History Page              //   Attribute Value: 26f4dce0-e638-4bc5-8aab-11e75116e1fb           
            RockMigrationHelper.AddBlockAttributeValue( "7908EAD6-832B-4E38-9EDA-5FC40115DA0E", "A8AB6C09-D87A-4220-A801-73A43A24C3E2", @"26f4dce0-e638-4bc5-8aab-11e75116e1fb" );
            // Add Block Attribute Value              //   Block: History Log              //   BlockType: History Log              //   Block Location: Page=Connection Request History, Site=Rock RMS              //   Attribute: Heading              //   Attribute Value: {{ Entity.Name }} (ID:{{ Entity.Id }})        
            RockMigrationHelper.AddBlockAttributeValue( "5667A5A7-DFE1-4BD4-93FF-21B71E3A07EA", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})" );
            // Add Block Attribute Value              //   Block: History Log              //   BlockType: History Log              //   Block Location: Page=Connection Request History, Site=Rock RMS              //   Attribute: Entity Type              //   Attribute Value: 36b0d0c7-8125-48fa-9da2-729aaa65f718            
            RockMigrationHelper.AddBlockAttributeValue( "5667A5A7-DFE1-4BD4-93FF-21B71E3A07EA", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"36b0d0c7-8125-48fa-9da2-729aaa65f718" );
            // Add/Update PageContext for Page:Connection Request History, Entity: Rock.Model.ConnectionRequest, Parameter: ConnectionRequestGuid            
            RockMigrationHelper.UpdatePageContext( "26F4DCE0-E638-4BC5-8AAB-11E75116E1FB", "Rock.Model.ConnectionRequest", "ConnectionRequestGuid", "B84A9233-3930-412E-B75B-04359F35D699" );
        }

        private void AddedConnectionRequestHistoryDown()
        {
            // Connection Request History Page Attribute for BlockType: Connection Request Board        
            RockMigrationHelper.DeleteAttribute( "A8AB6C09-D87A-4220-A801-73A43A24C3E2" );
            // Remove Block: History Log, from Page: Connection Request History, Site: Rock RMS         
            RockMigrationHelper.DeleteBlock( "5667A5A7-DFE1-4BD4-93FF-21B71E3A07EA" );
            // Delete Page Connection Request History from Site:Rock RMS   
            RockMigrationHelper.DeletePage( "26F4DCE0-E638-4BC5-8AAB-11E75116E1FB" ); //  Page: Connection Request History, Layout: Full Width, Site: Rock RMS  
            // Delete PageContext for Page:Connection Request History, Entity: Rock.Model.ConnectionRequest, Parameter: ConnectionRequestGuid  
            RockMigrationHelper.DeletePageContext( "B84A9233-3930-412E-B75B-04359F35D699" );
            RockMigrationHelper.DeleteCategory( "4B472C6A-1ACF-481E-A2D4-9C44436BBCF5" );
            RockMigrationHelper.DeletePageRoute( "717F02B0-E112-4602-9AAC-2200E9212C3E" );
        }
    }
}
