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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 86, "1.9.0" )]
    public class MigrationRollupsForV9_2_0 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            UpdateGroupTypeForGroupConnections();
            AddNonCashAssetTypes();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// JE: Group Connections Project
        /// </summary>
        private void UpdateGroupTypeForGroupConnections()
        {
            Sql( @"
                UPDATE [GroupType]
                SET [AllowSpecificGroupMemberAttributes] = 1
                WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49'
                    AND [AllowSpecificGroupMemberAttributes] = 0" );
        }

        /// <summary>
        /// SC: Pushpay Plugin v6.0/Non-Cash Giving Project.
        /// </summary>
        private void AddNonCashAssetTypes()
        {
            RockMigrationHelper.AddDefinedType(
                category: "Financial",
                name: "Non-Cash Asset Types",
                description: "Asset types that describe various kinds of Non-Cash transactions.",
                guid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE );

            RockMigrationHelper.AddDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Property",
                description: "Non-Cash Asset Type: Property.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_PROPERTY );

            RockMigrationHelper.AddDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Stocks And Bonds",
                description: "Non-Cash Asset Type: Stocks And Bonds.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_STOCKSANDBONDS );

            RockMigrationHelper.AddDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Vehicles",
                description: "Non-Cash Asset Type: Vehicles.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_VEHICLES );

            RockMigrationHelper.AddDefinedValue(
                definedTypeGuid: Rock.SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE,
                value: "Other",
                description: "Non-Cash Asset Type: Other.",
                guid: Rock.SystemGuid.DefinedValue.NONCASH_ASSET_OTHER );
        }
    }
}
