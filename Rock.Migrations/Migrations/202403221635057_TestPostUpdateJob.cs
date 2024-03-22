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
    using System.Collections.Generic;

    /// <summary>
    ///
    /// </summary>
    public partial class TestPostUpdateJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop AccountEntry and Login",
                blockTypeReplacements: new Dictionary<string, string> {
                    { "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "E5C34503-DDAD-4881-8463-0E1E20B1675D" }, // Account Entry
                    { "7B83D513-1178-429E-93FF-E76430E038E4", "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" }, // Login
                },
                migrationStrategy: "Swap",
                jobGuid: "3B606280-AC75-49A1-83D0-74F59BFB28BE",
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                    { "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "TestAttribute" }, // Account Entry
                    { "7B83D513-1178-429E-93FF-E76430E038E4", "TestAttribute,RemoteAuthorizationTypes " }, // Login
                } );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
