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
    public partial class SiteCommunicationPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Site", "CommunicationPageId", c => c.Int());
            AddColumn("dbo.Site", "CommunicationPageRouteId", c => c.Int());
            CreateIndex("dbo.Site", "CommunicationPageId");
            CreateIndex("dbo.Site", "CommunicationPageRouteId");
            AddForeignKey("dbo.Site", "CommunicationPageId", "dbo.Page", "Id");
            AddForeignKey("dbo.Site", "CommunicationPageRouteId", "dbo.PageRoute", "Id");

            RockMigrationHelper.UpdateFieldType( "Rating", "", "Rock", "Rock.Field.Types.RatingFieldType", "24BC2DD2-5745-4A97-A0F9-C1EC0E6E1862" );
            RockMigrationHelper.UpdateFieldType( "System Email", "", "Rock", "Rock.Field.Types.SystemEmailFieldType", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF" );
            RockMigrationHelper.DeletePageContext( "E904932A-4551-4A5A-B6BF-EF60AD8E90E6" );

            RockMigrationHelper.AddPage( "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Communication", "", "60002BC0-790A-4052-8F8D-B08C2C5D261C", "" ); // Site:External Website
            // Add Block to Page: Communication, Site: External Website
            RockMigrationHelper.AddBlock( "60002BC0-790A-4052-8F8D-B08C2C5D261C", "", "D9834641-7F39-4CFA-8CB2-E64068127565", "Communication Entry", "Main", "", "", 0, "690193A4-9EF5-4915-B034-969CF84EF703" );
            // Attrib for BlockType: Communication Entry:Mediums
            RockMigrationHelper.UpdateBlockTypeAttribute("D9834641-7F39-4CFA-8CB2-E64068127565", "039E2E97-3682-4B29-8748-7132287A2059", "Mediums", "Mediums", "", "The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).", 0, @"", "7879C156-A2AD-429B-A107-25069022C7B2" );
            // Attrib for BlockType: Communication Entry:Mode
            RockMigrationHelper.AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "", "The mode to use ( 'Simple' mode will prevent uers from searching/adding new people to communication).", 5, @"Full", "F678AD23-7185-4E54-96CC-9D988B2654D8" );

            // Attrib Value for Block:Communication Entry, Attribute:Default Template Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "73B61EB5-F6B7-495B-A781-8EE2D5717D14", @"" );
            // Attrib Value for Block:Communication Entry, Attribute:Mediums Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "7879C156-A2AD-429B-A107-25069022C7B2", @"5a653ebe-6803-44b4-85d2-fb7b8146d55d" );
            // Attrib Value for Block:Communication Entry, Attribute:Mode Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "F678AD23-7185-4E54-96CC-9D988B2654D8", @"Simple" );
            // Attrib Value for Block:Communication Entry, Attribute:Display Count Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "39B75C2A-879F-4C5A-A7D7-B18710B4681C", @"0" );
            // Attrib Value for Block:Communication Entry, Attribute:Maximum Recipients Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "EA534449-2BD0-4AA3-836C-627FA22576E4", @"25" );
            // Attrib Value for Block:Communication Entry, Attribute:Send When Approved Page: Communication, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "690193A4-9EF5-4915-B034-969CF84EF703", "04C3D08F-CAB5-49DC-91B4-3F81CCF40211", @"True" );

            Sql( @"
    DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE GUID = '2A22D08D-73A8-4AAF-AC7E-220E8B2E7857' )
    DECLARE @RouteId int = ( SELECT TOP 1 [Id] FROM [PageRoute] WHERE GUID = '79C0C1A7-41B6-4B40-954D-660A4B39B8CE' )

    UPDATE [Site] SET 
        [CommunicationPageId] = @PageId,
        [CommunicationPageRouteId] = @RouteId
    WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4' 

    SET @PageId = ( SELECT TOP 1 [Id] FROM [Page] WHERE GUID = '60002BC0-790A-4052-8F8D-B08C2C5D261C' )
    UPDATE [Site] SET 
        [CommunicationPageId] = @PageId 
    WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B' 
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "CommunicationPageRouteId", "dbo.PageRoute");
            DropForeignKey("dbo.Site", "CommunicationPageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "CommunicationPageRouteId" });
            DropIndex("dbo.Site", new[] { "CommunicationPageId" });
            DropColumn("dbo.Site", "CommunicationPageRouteId");
            DropColumn("dbo.Site", "CommunicationPageId");
        }
    }
}
