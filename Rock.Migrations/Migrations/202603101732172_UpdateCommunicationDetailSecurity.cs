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

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateCommunicationDetailSecurity : Rock.Migrations.RockMigration
    {
        private const string CommunicationDetailBlockTypeGuid = "2B63C6ED-20D5-467E-9A6A-C608E1D953E5";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Default allow VIEW_ALL permission for RSR - Rock Administration.
            AddSecurityAuthForBlockType(
                blockTypeGuid: CommunicationDetailBlockTypeGuid,
                order: 0,
                action: Security.Authorization.VIEW_ALL,
                allow: true,
                groupGuid: Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                specialRole: Model.SpecialRole.None
            );

            // Attribute for BlockType
            //   BlockType: Communication Detail
            //   Category: Communication
            //   Attribute: Communication Access Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( CommunicationDetailBlockTypeGuid, "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Communication Access Mode", "CommunicationAccessMode", "Communication Access Mode", @"Controls the level of visibility filtering applied to the communication list. ""Lax"" allows all individuals to view all communications. ""Moderate"" filters the list to only show communications where the individual has ""View"" rights to the associated communication template or system communication. ""Strict"" limits visibility to communications the individual authored or is listed as the sender, unless they have ""View All"" security on this block.", 1, @"strict", "0DD0AB7A-39D6-4028-97D8-A756932F8A98" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Communication Detail
            //   Category: Communication
            //   Attribute: Communication Access Mode
            RockMigrationHelper.DeleteAttribute( "0DD0AB7A-39D6-4028-97D8-A756932F8A98" );
        }

        /// <summary>
        /// Adds the block security authentication for all instances of a block type. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="blockTypeGuid">The block type unique identifier.</param>
        /// <param name="order">The order of the auth record. Specify <see cref="int.MaxValue"/> to append to the end of the rule list.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        private void AddSecurityAuthForBlockType( string blockTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole )
        {
            // Get a count of block instances, so we can confidently loop over them without a risk of infinitely looping.
            var blockInstanceCount = ( int ) SqlScalar( $@"
SELECT COUNT(1)
FROM [Block]
WHERE [BlockTypeId] = (
    SELECT TOP 1 [Id]
    FROM [BlockType]
    WHERE [Guid] = '{blockTypeGuid}'
);" );

            if ( blockInstanceCount == 0 )
            {
                return;
            }

            // Loop only as many times as there are block instances.
            var blockInstanceIndex = 0;
            while ( blockInstanceIndex < blockInstanceCount )
            {
                var blockInstanceGuid = ( Guid? ) SqlScalar( $@"
SELECT [Guid]
FROM [Block]
WHERE [BlockTypeId] = (
    SELECT TOP 1 [Id]
    FROM [BlockType]
    WHERE [Guid] = '{blockTypeGuid}'
)
ORDER BY [Id]
OFFSET {blockInstanceIndex} ROWS
FETCH NEXT 1 ROWS ONLY;" );

                if ( !blockInstanceGuid.HasValue )
                {
                    // This should never happen since we are looping based on the count of instances, but just in case,
                    // break out of the loop if we can't get a guid for the current index.
                    break;
                }

                // Add the security auth record for the block instance.
                // The following method will only add the record if it doesn't already exist.
                RockMigrationHelper.AddSecurityAuthForBlock(
                    blockGuid: blockInstanceGuid.ToString(),
                    order: order,
                    action: action,
                    allow: allow,
                    groupGuid: groupGuid,
                    specialRole: specialRole,
                    authGuid: Guid.NewGuid().ToString()
                );

                blockInstanceIndex++;
            }
        }
    }
}
