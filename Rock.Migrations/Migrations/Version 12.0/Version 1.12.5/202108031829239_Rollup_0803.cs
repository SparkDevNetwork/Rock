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
    public partial class Rollup_0803 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixTypoURLShortner();
            LavaDocumentationUpdates();
            CleanGivingHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// NA: Fixes the typo URL shortner.
        /// </summary>
        private void FixTypoURLShortner()
        {
            Sql( @"
                UPDATE [DefinedValue] SET [Value] = 'URL Shortner' WHERE [Guid] = '371066D5-C5F9-4783-88C8-D9AC8DC67468' AND [Value] = 'Url Shortner'
            " );
        }

        /// <summary>
        /// GJ: Lava Documentation Updates
        /// </summary>
        private void LavaDocumentationUpdates()
        {
            Sql( MigrationSQL._202108031829239_Rollup_0803_LavaDocumentationUpdates );
        }

        /// <summary>
        /// GJ: Delete Giving History JSON from Giving Analytics attribute category
        /// </summary>
        private void CleanGivingHistory()
        {
            Sql( @"
                DELETE ac
                FROM AttributeCategory AS ac
                INNER JOIN Attribute AS a ON ac.AttributeId = a.Id
                INNER JOIN Category AS c ON ac.CategoryId = c.Id
                WHERE c.Guid = '61823196-8EA1-4C2B-A7DF-1654BD085667' AND a.Guid = '3BF34F25-4D50-4417-B436-37FEA3FA5473'" );
        }
    }
}
