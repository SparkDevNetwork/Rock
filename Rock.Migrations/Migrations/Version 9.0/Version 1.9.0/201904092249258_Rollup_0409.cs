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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0409 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateCaptivePortalTnCs();
            RemoveIndexingAndConfigurationFromAssetManagerPluginPage();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Group Attendance Detail:Allow Sorting
            RockMigrationHelper.UpdateBlockTypeAttribute("FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Sorting","AllowSorting","",@"Should the block allow sorting the Member's list by First Name or Last Name?",13,@"True","184B5356-7101-43B8-B163-251422FE24CC");
        }
        
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Group Attendance Detail:Allow Sorting
            RockMigrationHelper.DeleteAttribute("184B5356-7101-43B8-B163-251422FE24CC");
        }

        /// <summary>
        /// SK: Update Captive Portal Terms and Conditions
        /// </summary>
        private void UpdateCaptivePortalTnCs()
        {
            string note = @"<li>
                    Every user is entitled to 20 continuous minutes free WiFi service every day at the Company's designated locations(s). If the connection is disconnected within the 20 minutes due to any reason, the users cannot use the Service again on the same day;
                </li>";

            note = note.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var defaultValueColumn = RockMigrationHelper.NormalizeColumnCRLF( "DefaultValue" );
            var valueColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            Sql( $@"
                DECLARE @AttributeId int
                SELECT 
	                @AttributeId = [Id]
                FROM
	                [dbo].[Attribute]
                WHERE
	                [Guid]='581389F2-90D1-4E0D-B8DD-2195D73F9A59'

                UPDATE
	                [dbo].[Attribute]
                SET
	                [DefaultValue]=REPLACE( {defaultValueColumn}
                                ,'{note}'
                                ,'' )
                WHERE
	                [Id]=@AttributeId

                UPDATE
	                [dbo].[AttributeValue]
                SET
	                [Value]=REPLACE( {valueColumn}
                                ,'{note}'
                                ,'' )
                WHERE
	                [AttributeId]=@AttributeId"
            );
        }

        private void RemoveIndexingAndConfigurationFromAssetManagerPluginPage()
        {
            Sql( @"UPDATE [dbo].[Page]
                SET [AllowIndexing] = 0, [IncludeAdminFooter] = 0
                WHERE [Guid] = 'DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389'" );
        }
    }
}
