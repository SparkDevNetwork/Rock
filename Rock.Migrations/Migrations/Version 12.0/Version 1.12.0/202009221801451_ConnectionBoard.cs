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
    /// <summary>
    ///
    /// </summary>
    public partial class ConnectionBoard : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelChangesUp();
            CmsChangesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CmsChangesDown();
            ModelChangesDown();
        }

        /// <summary>
        /// Models the changes up.
        /// </summary>
        private void ModelChangesUp()
        {
            AddColumn( "dbo.ConnectionType", "DefaultView", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionType", "RequestHeaderLava", c => c.String() );
            AddColumn( "dbo.ConnectionType", "RequestBadgeLava", c => c.String() );
            AddColumn( "dbo.ConnectionType", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionOpportunity", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionRequest", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionStatus", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionStatus", "HighlightColor", c => c.String( maxLength: 50 ) );
        }

        /// <summary>
        /// Models the changes down.
        /// </summary>
        private void ModelChangesDown()
        {
            DropColumn( "dbo.ConnectionStatus", "HighlightColor" );
            DropColumn( "dbo.ConnectionStatus", "Order" );
            DropColumn( "dbo.ConnectionRequest", "Order" );
            DropColumn( "dbo.ConnectionOpportunity", "Order" );
            DropColumn( "dbo.ConnectionType", "Order" );
            DropColumn( "dbo.ConnectionType", "RequestBadgeLava" );
            DropColumn( "dbo.ConnectionType", "RequestHeaderLava" );
            DropColumn( "dbo.ConnectionType", "DefaultView" );
        }

        /// <summary>
        /// CMSs the changes up.
        /// </summary>
        private void CmsChangesUp()
        {
            // Add connection opportunity select
            RockMigrationHelper.AddPage( true, SystemGuid.Page.ENGAGEMENT, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Connections", "", SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, "" );

            // Add connection board page
            RockMigrationHelper.AddPage( true, SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Connection Board", "", SystemGuid.Page.CONNECTIONS_BOARD, "" );

            // Move the existing connection page underneath opportunity select, sibling to the board
            RockMigrationHelper.MovePage( SystemGuid.Page.CONNECTIONS, SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );

            // Add/Update BlockType Connection Opportunity Select
            var opportunitySelectBlockTypeGuid = "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643";
            RockMigrationHelper.UpdateBlockType( "Connection Opportunity Select", "Block to display the connection opportunities that the user is authorized to view.", "~/Blocks//Connection/ConnectionOpportunitySelect.ascx", "Connection", opportunitySelectBlockTypeGuid );

            // Add Connection Opportunity Select to Page: CONNECTION_OPPORTUNITY_SELECT, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), opportunitySelectBlockTypeGuid.AsGuid(), "Connection Opportunity Select", "Main", @"", @"", 0, "340FBA54-FC54-4EA1-8DD2-301536405034" );

            // Add/Update BlockType Connection Request Board
            var boardBlockTypeGuid = "28DBE708-E99B-4879-A64D-656C030D25B5";
            RockMigrationHelper.UpdateBlockType( "Connection Request Board", "Display the Connection Requests for a selected Connection Opportunity as a list or board view.", "~/Blocks//Connection/ConnectionRequestBoard.ascx", "Connection", boardBlockTypeGuid );

            // Add Block Connection Request Board to Page: Connection Board, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CONNECTIONS_BOARD.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), boardBlockTypeGuid.AsGuid(), "Connection Request Board", "Main", @"", @"", 0, "7908EAD6-832B-4E38-9EDA-5FC40115DA0E" );

            // Hide the original connections page breadcrumb
            Sql( $"UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '{SystemGuid.Page.CONNECTIONS}'" );
        }

        /// <summary>
        /// CMSs the changes down.
        /// </summary>
        private void CmsChangesDown()
        {
            // Show the original connections page breadcrumb
            Sql( $"UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = '{SystemGuid.Page.CONNECTIONS}'" );

            RockMigrationHelper.DeleteBlockType( "28DBE708-E99B-4879-A64D-656C030D25B5" ); // Connection Request Board  
            RockMigrationHelper.DeleteBlockType( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643" ); // Opportunity Select

            RockMigrationHelper.MovePage( SystemGuid.Page.CONNECTIONS, SystemGuid.Page.ENGAGEMENT );

            RockMigrationHelper.DeletePage( SystemGuid.Page.CONNECTIONS_BOARD );
            RockMigrationHelper.DeletePage( SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );
        }
    }
}
