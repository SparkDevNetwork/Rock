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

using Rock.SystemGuid;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 198, "1.16.4" )]
    public class MigrationRollupsForV17_0_6: Migration
    {
        /// <summary>
        ///Up methods for the plugin migration.
        /// </summary>
        public override void Up()
        {
            FixRouteForAdaptiveMessageUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// SK: Fix Routes in Adaptive Message
        /// </summary>
        private void FixRouteForAdaptiveMessageUp()
        {
            RockMigrationHelper.UpdatePageRoute( "E612018C-FD4B-4F6F-9BCD-3B76B58CC8AB", "222ED9E3-06C0-438F-B520-C899B8835650", "admin/cms/adaptive-messages/attributes" );

#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( "73112D38-E051-4452-AEF9-E473EEDD0BCB", "admin/cms/adaptive-messages", "3B35F17E-B2DE-4512-8873-06A82F572ABD" );
#pragma warning restore CS0618 // Type or member is obsolete

            Sql( @"UPDATE 
                [Page]
            SET [DisplayInNavWhen] = 2
            WHERE [Guid]='222ED9E3-06C0-438F-B520-C899B8835650'" );
        }
    }
}
