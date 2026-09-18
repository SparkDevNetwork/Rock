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
    public partial class RestructureConnectionsPages : Rock.Migrations.RockMigration
    {
        private const string MyConnectionOpportunitiesPageRouteGuid = "569E3B0A-6F09-447C-BC4B-FCDD3F0BFEEE";
        private const string MyConnectionOpportunitiesBlockGuid = "80710A2C-9B90-40AE-B887-B885AAA43538";

        private const string AddCampaignRequestsBlockTypeGuid = "11630BB9-E685-4582-91F8-620448AA34B0";
        private const string AddCampaignRequestsBlockGuid = "BF39BE49-B4F6-4A5B-BDA2-EB343FC80CCA";

        private const string BulkUpdateRequestsBlockTypeGuid = "175158F8-F10E-476F-809E-A76825E0AC5D";
        private const string BulkUpdateRequestsPreviousPageAttributeGuid = "80783AF9-3C03-4DC7-BDFF-9940E6338DB8";

        private const string CelebrationsReportPageGuid = "E59810B6-5225-4CF6-A239-F2757A4369B1";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_RemoveLegacyConnectionBlocks_Up();
            JPH_AddMyConnectionOpportunitiesPage_Up();
            JPH_RestructureConnectionsPageTree_Up();
            JPH_RenameMyConnectionsPage_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_RenameMyConnectionsPage_Down();
            JPH_RestructureConnectionsPageTree_Down();
            JPH_AddMyConnectionOpportunitiesPage_Down();
            JPH_RemoveLegacyConnectionBlocks_Down();
        }

        #region Remove Legacy Connection Blocks

        /// <summary>
        /// JPH: Removes the legacy Add Campaign Requests and Bulk Update Requests blocks - up.
        /// </summary>
        private void JPH_RemoveLegacyConnectionBlocks_Up()
        {
            // BlockType: Add Campaign Requests. The Connections Hub's "Add From Campaign" modal covers this.
            DeleteLegacyBlockTypeAndAllInstances( AddCampaignRequestsBlockTypeGuid, "~/Blocks/Connection/AddCampaignRequests.ascx" );

            // ----------------------------------

            // Delete this before its block type, so the attribute's values go with it.
            RockMigrationHelper.DeleteAttribute( BulkUpdateRequestsPreviousPageAttributeGuid );

            // BlockType: Connection Requests Bulk Update. Its only launcher was the WebForms Connection Request
            // Board block type, deleted in Rollup_20260520.
            DeleteLegacyBlockTypeAndAllInstances( BulkUpdateRequestsBlockTypeGuid, "~/Blocks/Connection/BulkUpdateRequests.ascx" );
        }

        /// <summary>
        /// Deletes every instance of a legacy block type, along with each instance's security authorizations,
        /// then deletes the block type itself.
        /// </summary>
        /// <param name="blockTypeGuid">The block type unique identifier.</param>
        /// <param name="path">The block type path, matched as a fallback for records carrying a different identifier.</param>
        private void DeleteLegacyBlockTypeAndAllInstances( string blockTypeGuid, string path )
        {
            Sql( $@"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}' OR [Path] = '{path}');
DECLARE @BlockEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}');

IF @BlockTypeId IS NOT NULL
BEGIN
    DELETE [Auth]
    WHERE [EntityTypeId] = @BlockEntityTypeId
        AND [EntityId] IN (SELECT [Id] FROM [Block] WHERE [BlockTypeId] = @BlockTypeId);

    DELETE [Block]
    WHERE [BlockTypeId] = @BlockTypeId;

    DELETE [BlockType]
    WHERE [Id] = @BlockTypeId;
END" );
        }

        /// <summary>
        /// JPH: Removes the legacy Add Campaign Requests and Bulk Update Requests blocks - down.
        /// </summary>
        private void JPH_RemoveLegacyConnectionBlocks_Down()
        {
            // Re-add Legacy Block Type
            //   Name:Add Campaign Requests
            //   Category:Connection Campaign
            RockMigrationHelper.AddBlockType( "Add Campaign Requests", "Adds Campaign Connection Requests", "~/Blocks/Connection/AddCampaignRequests.ascx", "Connection Campaign", AddCampaignRequestsBlockTypeGuid );

            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CONNECTIONS.AsGuid(), Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE.AsGuid(), Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), AddCampaignRequestsBlockTypeGuid.AsGuid(), "Add Campaign Requests", "Main", @"", @"", 0, AddCampaignRequestsBlockGuid );

            // ----------------------------------

            // Re-add Legacy Block Type
            //   Name:Connection Requests Bulk Update
            //   Category:Connection
            RockMigrationHelper.AddBlockType( "Connection Requests Bulk Update", "Used for updating information about several Connection Requests at once. The QueryString must have both the EntitySetId as well as the ConnectionTypeId, and all the connection requests must be for the same opportunity.", "~/Blocks/Connection/BulkUpdateRequests.ascx", "Connection", BulkUpdateRequestsBlockTypeGuid );

            // Recreated here rather than left to the block type registration scan, which would mint a new identifier
            // because the block declares this as a [LinkedPage] without one.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( BulkUpdateRequestsBlockTypeGuid, Rock.SystemGuid.FieldType.PAGE_REFERENCE, "Previous Page", "PreviousPage", "Previous Page", "", 1, Rock.SystemGuid.Page.CONNECTIONS_BOARD, BulkUpdateRequestsPreviousPageAttributeGuid );

            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE.AsGuid(), Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE.AsGuid(), Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), BulkUpdateRequestsBlockTypeGuid.AsGuid(), "Connection Requests Bulk Update", "Main", @"", @"", 0, Rock.SystemGuid.Block.CONNECTION_REQUESTS_BULK_UPDATE );
        }

        #endregion Remove Legacy Connection Blocks

        #region Add My Connection Opportunities Page

        /// <summary>
        /// JPH: Adds the My Connection Opportunities page and moves the existing block onto it - up.
        /// </summary>
        private void JPH_AddMyConnectionOpportunitiesPage_Up()
        {
            // Add Page
            //  Internal Name: My Connection Opportunities
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "My Connection Opportunities", "", Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES, "" );

            // Add Page Route
            //   Page:My Connection Opportunities
            //   Route:people/connections/my-opportunities
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES, "people/connections/my-opportunities", MyConnectionOpportunitiesPageRouteGuid );

            // ----------------------------------

            // Move the existing instance rather than adding a new one, so its configured attribute values come along.
            Sql( $@"
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES}');

IF @PageId IS NOT NULL
BEGIN
    UPDATE [Block]
    SET [PageId] = @PageId
        , [Order] = 0
    WHERE [Guid] = '{MyConnectionOpportunitiesBlockGuid}';
END" );
        }

        /// <summary>
        /// JPH: Adds the My Connection Opportunities page and moves the existing block onto it - down.
        /// </summary>
        private void JPH_AddMyConnectionOpportunitiesPage_Down()
        {
            Sql( $@"
DECLARE @PageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS}');

IF @PageId IS NOT NULL
BEGIN
    UPDATE [Block]
    SET [PageId] = @PageId
        , [Order] = 1
    WHERE [Guid] = '{MyConnectionOpportunitiesBlockGuid}';
END" );

            // ----------------------------------

            // Delete Page Route
            //   Page:My Connection Opportunities
            //   Route:people/connections/my-opportunities
            RockMigrationHelper.DeletePageRoute( MyConnectionOpportunitiesPageRouteGuid );

            // Delete Page
            //  Internal Name: My Connection Opportunities
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES );
        }

        #endregion Add My Connection Opportunities Page

        #region Restructure Connections Page Tree

        /// <summary>
        /// JPH: Flattens the Connections page tree under a single Connections root - up.
        /// </summary>
        private void JPH_RestructureConnectionsPageTree_Up()
        {
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_TYPES, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT );

            // ----------------------------------

            // Delete Page
            //  Internal Name: Connection Requests Bulk Update
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE );

            // Delete Page
            //  Internal Name: Connections (the duplicate page, a container only)
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS );

            // ----------------------------------

            // Connection Request Detail is a landing target, not a page staff browse to.
            Sql( $@"
UPDATE [Page]
SET [DisplayInNavWhen] = 2 -- Never
WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL}';" );

            // ----------------------------------

            Sql( $@"
UPDATE [Page] SET [Order] = 0 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_HUB}';
UPDATE [Page] SET [Order] = 1 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT}';
UPDATE [Page] SET [Order] = 2 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES}';
UPDATE [Page] SET [Order] = 3 WHERE [Guid] = '{Rock.SystemGuid.Page.MY_CONNECTIONS}';
UPDATE [Page] SET [Order] = 4 WHERE [Guid] = '{Rock.SystemGuid.Page.MY_CONNECTION_OPPORTUNITIES}';
UPDATE [Page] SET [Order] = 5 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_TYPES}';
UPDATE [Page] SET [Order] = 6 WHERE [Guid] = '{CelebrationsReportPageGuid}';
UPDATE [Page] SET [Order] = 7 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL}';" );
        }

        /// <summary>
        /// JPH: Flattens the Connections page tree under a single Connections root - down.
        /// </summary>
        private void JPH_RestructureConnectionsPageTree_Down()
        {
            Sql( $@"
UPDATE [Page]
SET [DisplayInNavWhen] = 0 -- When Allowed
WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL}';" );

            // ----------------------------------

            // Add Page
            //  Internal Name: Connections (the duplicate page)
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Connections", "", Rock.SystemGuid.Page.CONNECTIONS, "ti ti-plug" );

            // Add Page
            //  Internal Name: Connection Requests Bulk Update
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTIONS, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Connection Requests Bulk Update", "", Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE, "ti ti-truck" );

            // AddPage always writes a breadcrumb display name; this page did not have one.
            Sql( $@"
UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS}';" );

            // ----------------------------------

            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL, Rock.SystemGuid.Page.CONNECTIONS );
            RockMigrationHelper.MovePage( Rock.SystemGuid.Page.CONNECTION_TYPES, Rock.SystemGuid.Page.CONNECTIONS );

            Sql( $@"
UPDATE [Page] SET [Order] = 0 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_REQUEST_DETAIL}';
UPDATE [Page] SET [Order] = 1 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_TYPES}';
UPDATE [Page] SET [Order] = 2 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTION_REQUESTS_BULK_UPDATE}';" );

            // ----------------------------------

            Sql( $@"
UPDATE [Page] SET [Order] = 7 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS}';
UPDATE [Page] SET [Order] = 8 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_HUB}';
UPDATE [Page] SET [Order] = 9 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT}';
UPDATE [Page] SET [Order] = 10 WHERE [Guid] = '{Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES}';
UPDATE [Page] SET [Order] = 11 WHERE [Guid] = '{Rock.SystemGuid.Page.MY_CONNECTIONS}';
UPDATE [Page] SET [Order] = 12 WHERE [Guid] = '{CelebrationsReportPageGuid}';" );
        }

        #endregion Restructure Connections Page Tree

        #region Rename My Connections Page

        /// <summary>
        /// JPH: Renames the My Connections page to My Connection Requests - up.
        /// </summary>
        private void JPH_RenameMyConnectionsPage_Up()
        {
            RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.MY_CONNECTIONS, "My Connection Requests" );
        }

        /// <summary>
        /// JPH: Renames the My Connections page to My Connection Requests - down.
        /// </summary>
        private void JPH_RenameMyConnectionsPage_Down()
        {
            RockMigrationHelper.RenamePage( Rock.SystemGuid.Page.MY_CONNECTIONS, "My Connections" );
        }

        #endregion Rename My Connections Page
    }
}
