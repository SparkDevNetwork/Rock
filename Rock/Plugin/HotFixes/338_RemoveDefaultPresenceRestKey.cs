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
    /// Removes the default presence login and its default credentials that were included with new installs.
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

                New installs included a built in presence login with a default REST key
                and a default password. Clear the default key, remove the login while it
                is still exactly as shipped, and clear the default password from any login
                that still has it. A login an administrator changed (new key or new password)
                is kept. The Presence person, its alias and its security rules are kept.
                Each statement matches only the shipped values, so it is safe to run again.

                Reason: Removes the default presence login and its default credentials that were included with new installs.
            */
            Sql( @"
UPDATE [UserLogin]
SET [ApiKey] = NULL
WHERE [ApiKey] = 'B421A0508102448AA50506E3452BB08D';

DELETE FROM [UserLogin]
WHERE [Guid] = '0CEB5F7F-AD1D-493E-8E7F-B6C12DCB5646'
    AND [UserName] = 'presence'
    AND [Password] = '$2a$11$bRBPqjG.Lzalcf6EN8msrebJEmzshVZdUIBgiVvqHgUzwRZDhfd46'
    AND ( [ApiKey] IS NULL OR [ApiKey] = 'B421A0508102448AA50506E3452BB08D' );

UPDATE [UserLogin]
SET [Password] = NULL
WHERE [Password] = '$2a$11$bRBPqjG.Lzalcf6EN8msrebJEmzshVZdUIBgiVvqHgUzwRZDhfd46';" );
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
