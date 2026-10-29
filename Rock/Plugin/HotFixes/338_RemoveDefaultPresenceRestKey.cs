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
    /// Removes a default REST key that was included with new installs.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 338, "17.8" )]
    public class RemoveDefaultPresenceRestKey : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/29/2026 - MSE

                New installs included a default REST key on the built in presence login.
                Clear it so it cannot be used. Match on the shipped value only, so a key
                an administrator has already set is left in place. Administrators who need
                REST access for this login can generate a new key.

                Reason: Removes a default REST key that was included with new installs so it cannot be used.
            */
            Sql( @"
UPDATE [UserLogin]
SET [ApiKey] = NULL
WHERE [ApiKey] = 'B421A0508102448AA50506E3452BB08D';" );
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
