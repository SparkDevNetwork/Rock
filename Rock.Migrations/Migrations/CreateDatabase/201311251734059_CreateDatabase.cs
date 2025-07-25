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
    public partial class CreateDatabase : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Create Database */
            Sql( RockMigration.MigrationSQL._201311251734059_CreateDatabase );

            /*
                07/17/2025 - SMC
                This migration was updated to replace the TransactionEntry block with the newer TransactionEntryV2 block in new
                Rock installations, intentionally leaving the block alone for existing instances.

                The next time the database is "squished" into the CreateDatabase script, this extra migration should be removed,
                as it will already be included.

                Reason: Use new TransactionEntryV2 block for new Rock installations.
             */

            Sql( NewInstallationGivingBlockReplacementAsOf17_3.MigrationSql );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
