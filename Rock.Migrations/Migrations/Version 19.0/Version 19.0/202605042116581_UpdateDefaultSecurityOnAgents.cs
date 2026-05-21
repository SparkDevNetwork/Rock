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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateDefaultSecurityOnAgents : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Ensure the AI Agent entity type exists.
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.AIAgent", "ee3fe609-5c7c-492e-b0e9-5461045fc825", true, true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.AIAgent",
                0,
                Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.None,
                "0c850a21-e3e5-4238-a0e8-499b8f656e29" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.AIAgent",
                0,
                Authorization.VIEW,
                false,
                string.Empty,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "f2f4f1a0-c702-46c2-b4c8-0a5a8b82ec66" );

            // Ensure the rest controller and rest action exist.
            RockMigrationHelper.AddRestAction( "2c6194af-095a-42fa-9288-27e8b3494231", "Mcp", "Rock.Rest.v2.McpController" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "2c6194af-095a-42fa-9288-27e8b3494231",
                0,
                Authorization.EXECUTE_READ,
                true,
                string.Empty,
                Rock.Model.SpecialRole.AllAuthenticatedUsers,
                "075332c5-c5ef-4eab-b6bc-8bc1a1915569" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "2c6194af-095a-42fa-9288-27e8b3494231",
                1,
                Authorization.EXECUTE_READ,
                false,
                string.Empty,
                Rock.Model.SpecialRole.AllUsers,
                "b9479698-7ce0-4135-b95d-e0c92a9e57a7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "b9479698-7ce0-4135-b95d-e0c92a9e57a7" );
            RockMigrationHelper.DeleteSecurityAuth( "075332c5-c5ef-4eab-b6bc-8bc1a1915569" );

            RockMigrationHelper.DeleteSecurityAuth( "f2f4f1a0-c702-46c2-b4c8-0a5a8b82ec66" );
            RockMigrationHelper.DeleteSecurityAuth( "0c850a21-e3e5-4238-a0e8-499b8f656e29" );
        }
    }
}
