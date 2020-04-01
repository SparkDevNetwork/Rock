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
    [MigrationNumber( 113, "1.10.0" )]
    public class MigrationRollupsFor10_3_2 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //FixChartShortcode();
            //GroupMemberNotificationSystemEmail();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// GJ: Fix Chart Shortcode YAxis Min/Max Value (Fixes #4157)
        /// </summary>
        private void FixChartShortcode()
        {
            Sql( HotFixMigrationResource._113_MigrationRollupsFor10_3_2_FixChartYAxisShortcode );
        }

        /// <summary>
        /// SK: Updated Group Member Notification system email template to correctly define NumberTypeValueId for mobilePhone (Fixes #4156)
        /// </summary>
        private void GroupMemberNotificationSystemEmail ()
        {
            string newValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            string oldValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemEmail] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'" );

            newValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            oldValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            Sql( $@"UPDATE [dbo].[SystemEmail] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '8747131E-3EDA-4FB0-A484-C2D2BE3918BA'" );
        }

    }
}
