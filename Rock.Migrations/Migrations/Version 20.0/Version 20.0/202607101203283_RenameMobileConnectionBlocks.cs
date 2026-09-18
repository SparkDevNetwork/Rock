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
    /// Renames the mobile Connection block types so the revamped blocks take over
    /// the base names and the original blocks become Legacy. All Guid values are
    /// unchanged, so existing block instances and page placements are unaffected.
    /// </summary>
    public partial class RenameMobileConnectionBlocks : Rock.Migrations.RockMigration
    {
        #region Entity Type Guids for the revamped blocks

        private const string ADD_CONNECTION_REQUEST_ENTITY_TYPE = "553609B0-49E3-4E52-9D63-7F10C03D249E";
        private const string CONNECTION_REQUEST_DETAIL_ENTITY_TYPE = "8B53B246-526F-4B3E-AF5B-4C36763E9DC9";
        private const string CONNECTION_REQUEST_LIST_ENTITY_TYPE = "CC91A1ED-7FB0-43B3-A8B4-A050DBF6BA6D";
        private const string CONNECTION_TYPE_LIST_ENTITY_TYPE = "88E9C088-5CCE-41F9-B99E-C3B03E123316";
        private const string CONNECTION_OPPORTUNITY_LIST_ENTITY_TYPE = "8DD07282-8470-426C-8F89-7390599DB37F";

        #endregion

        #region Block Type Guids for the revamped blocks

        private const string ADD_CONNECTION_REQUEST_BLOCK_TYPE = "5A198A75-177C-4A2A-8558-BFB5A4EFCB30";
        private const string CONNECTION_REQUEST_DETAIL_BLOCK_TYPE = "74DDC1A2-2025-4072-8F47-DF7A5A76CF83";
        private const string CONNECTION_REQUEST_LIST_BLOCK_TYPE = "117ADAF8-8173-4A88-8C88-2C97F88985DC";
        private const string CONNECTION_TYPE_LIST_BLOCK_TYPE = "A7FF3F7F-AC1D-4C07-A1E1-FBDE8F689F6A";
        private const string CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE = "039AB104-FDFE-4BB0-944A-2C02F4C1D73A";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PS_RenameConnectionEntityTypes_Up();
            PS_RenameConnectionBlockTypes_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PS_RenameConnectionBlockTypes_Down();
            PS_RenameConnectionEntityTypes_Down();
        }

        #region Private Methods

        /// <summary>
        /// Renames the EntityType rows in place: the original mobile Connection blocks
        /// gain a Legacy suffix, then the revamped blocks drop their V2 suffix. Renaming
        /// in place keeps the EntityTypeId links on BlockType intact so Rock start-up
        /// re-links the renamed classes without creating orphan rows.
        /// </summary>
        private void PS_RenameConnectionEntityTypes_Up()
        {
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST, "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest", "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequestLegacy" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail", "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetailLegacy" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList", "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestListLegacy" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList", "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeListLegacy" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList", "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityListLegacy" );

            RenameEntityType( ADD_CONNECTION_REQUEST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.AddConnectionRequestV2", "Rock.Blocks.Mobile.Connection.AddConnectionRequest" );
            RenameEntityType( CONNECTION_REQUEST_DETAIL_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2", "Rock.Blocks.Mobile.Connection.ConnectionRequestDetail" );
            RenameEntityType( CONNECTION_REQUEST_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionRequestListV2", "Rock.Blocks.Mobile.Connection.ConnectionRequestList" );
            RenameEntityType( CONNECTION_TYPE_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionTypeListV2", "Rock.Blocks.Mobile.Connection.ConnectionTypeList" );
            RenameEntityType( CONNECTION_OPPORTUNITY_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2", "Rock.Blocks.Mobile.Connection.ConnectionOpportunityList" );
        }

        /// <summary>
        /// Restores the original EntityType names: the revamped blocks get their V2
        /// suffix back first, which frees the base names for the Legacy blocks.
        /// </summary>
        private void PS_RenameConnectionEntityTypes_Down()
        {
            RenameEntityType( ADD_CONNECTION_REQUEST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.AddConnectionRequest", "Rock.Blocks.Mobile.Connection.AddConnectionRequestV2" );
            RenameEntityType( CONNECTION_REQUEST_DETAIL_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionRequestDetail", "Rock.Blocks.Mobile.Connection.ConnectionRequestDetailV2" );
            RenameEntityType( CONNECTION_REQUEST_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionRequestList", "Rock.Blocks.Mobile.Connection.ConnectionRequestListV2" );
            RenameEntityType( CONNECTION_TYPE_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionTypeList", "Rock.Blocks.Mobile.Connection.ConnectionTypeListV2" );
            RenameEntityType( CONNECTION_OPPORTUNITY_LIST_ENTITY_TYPE, "Rock.Blocks.Mobile.Connection.ConnectionOpportunityList", "Rock.Blocks.Mobile.Connection.ConnectionOpportunityListV2" );

            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST, "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequestLegacy", "Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetailLegacy", "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestListLegacy", "Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeListLegacy", "Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList" );
            RenameEntityType( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE, "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityListLegacy", "Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList" );
        }

        /// <summary>
        /// Updates the admin-facing BlockType names: the original blocks are marked
        /// Legacy first so the clean names are never duplicated, then the revamped
        /// blocks drop their V2 suffix.
        /// </summary>
        private void PS_RenameConnectionBlockTypes_Up()
        {
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST, "Add Connection Request (Legacy)" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL, "Connection Request Detail (Legacy)" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST, "Connection Request List (Legacy)" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST, "Connection Type List (Legacy)" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST, "Connection Opportunity List (Legacy)" );

            RenameBlockType( ADD_CONNECTION_REQUEST_BLOCK_TYPE, "Add Connection Request" );
            RenameBlockType( CONNECTION_REQUEST_DETAIL_BLOCK_TYPE, "Connection Request Detail" );
            RenameBlockType( CONNECTION_REQUEST_LIST_BLOCK_TYPE, "Connection Request List" );
            RenameBlockType( CONNECTION_TYPE_LIST_BLOCK_TYPE, "Connection Type List" );
            RenameBlockType( CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE, "Connection Opportunity List" );
        }

        /// <summary>
        /// Restores the original BlockType names: the revamped blocks get their V2
        /// suffix back first, which frees the clean names for the Legacy blocks.
        /// </summary>
        private void PS_RenameConnectionBlockTypes_Down()
        {
            RenameBlockType( ADD_CONNECTION_REQUEST_BLOCK_TYPE, "Add Connection Request V2" );
            RenameBlockType( CONNECTION_REQUEST_DETAIL_BLOCK_TYPE, "Connection Request Detail V2" );
            RenameBlockType( CONNECTION_REQUEST_LIST_BLOCK_TYPE, "Connection Request List V2" );
            RenameBlockType( CONNECTION_TYPE_LIST_BLOCK_TYPE, "Connection Type List V2" );
            RenameBlockType( CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE, "Connection Opportunity List V2" );

            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_ADD_CONNECTION_REQUEST, "Add Connection Request" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL, "Connection Request Detail" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST, "Connection Request List" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST, "Connection Type List" );
            RenameBlockType( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST, "Connection Opportunity List" );
        }

        /// <summary>
        /// Renames a single EntityType row in place, keyed by Guid, keeping the
        /// AssemblyName consistent with the new type name. The NOT EXISTS guard skips
        /// the update if a row already holds the target name, which protects the
        /// unique index on Name if a renamed build auto-registered before this
        /// migration ran.
        /// </summary>
        /// <param name="entityTypeGuid">The Guid of the EntityType row to rename.</param>
        /// <param name="oldName">The fully qualified type name being replaced.</param>
        /// <param name="newName">The fully qualified type name to apply.</param>
        private void RenameEntityType( string entityTypeGuid, string oldName, string newName )
        {
            Sql( $@"
IF EXISTS ( SELECT 1 FROM [EntityType] WHERE [Guid] = '{entityTypeGuid}' )
    AND NOT EXISTS ( SELECT 1 FROM [EntityType] WHERE [Name] = '{newName}' )
BEGIN
    UPDATE [EntityType]
    SET [Name] = '{newName}'
        , [AssemblyName] = REPLACE( [AssemblyName], '{oldName}', '{newName}' )
    WHERE [Guid] = '{entityTypeGuid}'
END" );
        }

        /// <summary>
        /// Updates the Name of a single BlockType row, keyed by Guid. A plain UPDATE
        /// is used intentionally: UpdateBlockTypeByGuid() deletes BlockType rows by
        /// empty Path, which would destroy entity-based block types.
        /// </summary>
        /// <param name="blockTypeGuid">The Guid of the BlockType row to rename.</param>
        /// <param name="newName">The name to apply.</param>
        private void RenameBlockType( string blockTypeGuid, string newName )
        {
            Sql( $@"UPDATE [BlockType] SET [Name] = '{newName}' WHERE [Guid] = '{blockTypeGuid}'" );
        }

        #endregion
    }
}
