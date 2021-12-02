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
    public partial class Rollup_20211202 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateDefaultLavaFormatOptions();
            AddDaysSinceTransactionOption_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// SK: Updated Pre Selected Options Format Lava
        /// </summary>
        private void UpdateDefaultLavaFormatOptions()
        {
            string newValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }} {% if DisplayLocationCount == true %} &nbsp;&nbsp;&nbsp; Count: {{ LocationCount }} {% endif %}".Replace( "'", "''" );
            string oldValue = "{{ Schedule.Name }} - {{ Group.Name }} - {{ Location.Name }}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            Sql( $@"DECLARE @attributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '55580865-E792-469F-B45C-45713477D033')
                    UPDATE [dbo].[AttributeValue] 
                    SET [Value] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%' AND [AttributeId] = @attributeId" );
        }

        /// <summary>
        /// MP: Add 'Show Days Since Last Transaction' block setting
        /// </summary>
        private void AddDaysSinceTransactionOption_Up()
        {
            // Attribute for BlockType
            //   BlockType: Transaction List
            //   Category: Finance
            //   Attribute: Show Days Since Last Transaction
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Days Since Last Transaction", "ShowDaysSinceLastTransaction", "Show Days Since Last Transaction", @"Show the number of days between the transaction and the transaction listed next to the transaction", 12, @"False", "D9A0DB03-1E45-46EE-A352-DA04E9A1F96A" );

            // Set ShowDaysSinceLastTransaction to true only the Person Contribution Tab
            RockMigrationHelper.UpdateBlockAttributeValue( "9382B285-3EF6-47F7-94BB-A47C498196A3", "D9A0DB03-1E45-46EE-A352-DA04E9A1F96A", "True" );
        }
    }
}
