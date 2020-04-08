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
    /// Add global attributes to enable and define when user lockouts occur from too many
    /// failed logins in a specified timeframe.
    /// </summary>
    public partial class FailedLoginTrackingAttributes : Rock.Migrations.RockMigration
    {
        private static string AttemptWindowGuid = "F492A1BD-DE8D-4EDE-82DA-1A5B1666E413";
        private static string MaxAttemptsGuid = "F5C2E99C-5FFB-4662-A90A-D33BE8C11C1A";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute(
                SystemGuid.FieldType.INTEGER,
                null,
                null,
                "Password Attempt Window",
                "The number of minutes after a user's first unsuccessful login that the user can be locked out. After this window of time, those failed logins will be forgiven. Set to 0 to disable user lockouts.",
                0,
                "1",
                AttemptWindowGuid,
                "PasswordAttemptWindow",
                false );

            RockMigrationHelper.AddGlobalAttribute(
                SystemGuid.FieldType.INTEGER,
                null,
                null,
                "Max Invalid Password Attempts",
                "The number of invalid login attempts by a user, within the Password Attempt Window, before the user's account is locked.",
                0,
                "20",
                MaxAttemptsGuid,
                "MaxInvalidPasswordAttempts",
                false );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( MaxAttemptsGuid );
            RockMigrationHelper.DeleteAttribute( AttemptWindowGuid );
        }
    }
}
