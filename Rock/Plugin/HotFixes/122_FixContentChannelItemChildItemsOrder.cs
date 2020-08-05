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
    [MigrationNumber( 122, "1.10.2" )]
    public class FixContentChannelItemChildItemsOrder : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //FixContentChannelItemAssociationOrder();
        }

        /// <summary>
        /// Fixes the content channel item association order.
        /// </summary>
        private void FixContentChannelItemAssociationOrder()
        {
            // There was a bug in ContentChannelItemDetail where ChildItems were updating and sorting child items on the ContentChannelItemAssociation.ContentChannelItem.Order
            // instead of the ContentChannelItemAssociation.Order. Now that ContentChannelItemDetail is fixed, we'll need to update the ContentChannelItemAssociation.Order
            // values to what order they were seeing when it was ordering by ContentChannelItem.Order
            Sql( @"
UPDATE a
SET a.[Order] = i.[Order]
FROM ContentChannelItemAssociation a
JOIN ContentChannelItem i
	ON a.ChildContentChannelItemId = i.Id
WHERE a.[Order] != i.[Order]" );
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
