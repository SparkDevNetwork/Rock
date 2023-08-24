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
    /// Roll up Migrations for pre-alpha release 1.16.0.9
    /// </summary>
    public partial class Rollup_20230810 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MarkPostUpdateJobsAsSystemJobs();
            NoteMentionSearchSecurityUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            NoteMentionSearchSecurityDown();
        }

        /// <summary>
        /// PA: Mark all Post Update Jobs as System Jobs
        /// </summary>
        private void MarkPostUpdateJobsAsSystemJobs()
        {
            Sql( @"UPDATE [ServiceJob] SET [IsSystem] = 1 WHERE [Guid] IN (
                    'E7C54AAB-451E-4E89-8083-CF398D37416E', -- Rock Update Helper v12.5 - Update Step Program Completion
                    'D96BD1F7-6A4A-4DC0-B10D-40031F709573', -- Rock Update Helper v14.0 - Add FK indexes
                    '6DFE731E-F28B-40B3-8383-84212A301214', -- Rock Update Helper v15.0 - System Phone Numbers
                    '480E996E-6A31-40DB-AE98-BFF85CDED506', -- Rock Update Helper v15.0 - Mobile Application Users Security Group
                    'EA00D1D4-709A-4102-863D-08471AA2C345', -- Rock Update Helper v15.0 - Replace WebForms Blocks with Obsidian Blocks
                    'D3D60B90-48D1-4718-905E-39638B44C665', -- Rock Update Helper v15.1 - Mobile Duplicate Interaction Cleanup
                    'C8591D15-9D37-49D3-8DF8-1DB72EE42D29', -- Rock Update Helper v16.0 - Migrate Person Preferences
                    '4232194C-90AE-4B44-93E7-1E5DE984A9E1'  -- Rock Update Helper v15.2 - Replace WebForms Blocks with Obsidian Blocks
				)" );
        }

        /// <summary>
        /// DH: Set default security on new Mention Search API endpoint
        /// </summary>
        private void NoteMentionSearchSecurityUp()
        {
            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                Rock.Security.Authorization.EDIT, // For POST method
                true,
                string.Empty,
                Rock.Model.SpecialRole.AllUsers,
                "11615413-430e-4579-8e6c-26537e6fd473" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                Rock.Model.SpecialRole.None,
                "0764dae6-8915-452a-95d3-f2cdf5999892" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None,
                "575c92ac-1602-4d75-9c02-1a8259dc2ca9" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                Rock.Model.SpecialRole.None,
                "0858c8b1-1359-49d8-b3a6-6c6e03fa3643" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None,
                "7a3dc6a3-c9d7-48c9-ac36-f64ada7aaf07" );

            RockMigrationHelper.AddSecurityAuthForRestAction( "dca338b6-9749-427e-8238-1686c9587d16",
                0,
                "FullSearch",
                false,
                string.Empty,
                Rock.Model.SpecialRole.AllUsers,
                "de33af64-a72a-4ba7-a8ee-9825ba6e73f4" );
        }

        /// <summary>
        /// DH: Set default security on new Mention Search API endpoint
        /// </summary>
        private void NoteMentionSearchSecurityDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "de33af64-a72a-4ba7-a8ee-9825ba6e73f4" );
            RockMigrationHelper.DeleteSecurityAuth( "7a3dc6a3-c9d7-48c9-ac36-f64ada7aaf07" );
            RockMigrationHelper.DeleteSecurityAuth( "0858c8b1-1359-49d8-b3a6-6c6e03fa3643" );
            RockMigrationHelper.DeleteSecurityAuth( "575c92ac-1602-4d75-9c02-1a8259dc2ca9" );
            RockMigrationHelper.DeleteSecurityAuth( "0764dae6-8915-452a-95d3-f2cdf5999892" );
            RockMigrationHelper.DeleteSecurityAuth( "11615413-430e-4579-8e6c-26537e6fd473" );
        }
    }
}
