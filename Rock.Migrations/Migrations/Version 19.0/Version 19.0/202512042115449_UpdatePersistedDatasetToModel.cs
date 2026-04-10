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
    public partial class UpdatePersistedDatasetToModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.PersistedDataset", "CreatedDateTime", c => c.DateTime() );
            AddColumn( "dbo.PersistedDataset", "ModifiedDateTime", c => c.DateTime() );
            AddColumn( "dbo.PersistedDataset", "CreatedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.PersistedDataset", "ModifiedByPersonAliasId", c => c.Int() );
            //CreateIndex( "dbo.PersistedDataset", "CreatedByPersonAliasId" );
            //CreateIndex( "dbo.PersistedDataset", "ModifiedByPersonAliasId" );
            AddForeignKey( "dbo.PersistedDataset", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.PersistedDataset", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id" );

            // Migrate existing data from RefreshIntervalMinutes to PersistedScheduleIntervalMinutes
            Sql( @"UPDATE [PersistedDataset]
                    SET [PersistedScheduleIntervalMinutes] = [RefreshIntervalMinutes]
                    WHERE [RefreshIntervalMinutes] IS NOT NULL
                    " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Undo the data migration
            Sql( @"UPDATE [PersistedDataset]
                    SET [RefreshIntervalMinutes] = [PersistedScheduleIntervalMinutes]
                    , [PersistedScheduleIntervalMinutes] = NULL
                    WHERE [PersistedScheduleIntervalMinutes] IS NOT NULL
                    " );

            DropForeignKey( "dbo.PersistedDataset", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersistedDataset", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            //DropIndex( "dbo.PersistedDataset", new[] { "ModifiedByPersonAliasId" } );
            //DropIndex( "dbo.PersistedDataset", new[] { "CreatedByPersonAliasId" } );
            DropColumn( "dbo.PersistedDataset", "ModifiedByPersonAliasId" );
            DropColumn( "dbo.PersistedDataset", "CreatedByPersonAliasId" );
            DropColumn( "dbo.PersistedDataset", "ModifiedDateTime" );
            DropColumn( "dbo.PersistedDataset", "CreatedDateTime" );
        }
    }
}
