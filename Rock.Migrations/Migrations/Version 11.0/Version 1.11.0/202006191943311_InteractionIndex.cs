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
    public partial class InteractionIndex : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // This index will be covered by the new index created by the next statement
            RockMigrationHelper.DropIndexIfExists( "Interaction", "IX_InteractionComponentId" );

            // This index is primarily for the median page load times, but also includes operation which is often used
            RockMigrationHelper.CreateIndexIfNotExists( "Interaction",
                new[] { "InteractionComponentId", "InteractionDateTime" },
                new[] { "InteractionTimeToServe", "Operation" } );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // There is no need to remove the new index or re-add unnecessary indexes
        }
    }
}
