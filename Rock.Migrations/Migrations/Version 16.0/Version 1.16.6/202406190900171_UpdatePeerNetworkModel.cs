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
    using Rock.Data;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdatePeerNetworkModel : Rock.Migrations.RockMigration
    {
        private readonly string[] oldIndexOneKeys = new[] { "SourcePersonAliasId", "TargetPersonAliasId", "RelationshipEndDate" };
        private readonly string[] oldIndexTwoKeys = new[] { "SourcePersonAliasId", "RelationshipTypeValueId", "RelationshipEndDate" };

        private readonly string[] newIndexOneKeys = new[] { "SourcePersonId", "TargetPersonId", "RelationshipEndDate" };
        private readonly string[] newIndexTwoKeys = new[] { "SourcePersonId", "RelationshipTypeValueId", "RelationshipEndDate" };

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Drop foreign keys to PersonAlias table.
            DropForeignKey( "dbo.PeerNetwork", "SourcePersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PeerNetwork", "TargetPersonAliasId", "dbo.PersonAlias" );

            // Drop old index using PersonAlias table.
            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( oldIndexOneKeys ) );
            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( oldIndexTwoKeys ) );

            // Add new columns.
            AddColumn( "dbo.PeerNetwork", "SourcePersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.PeerNetwork", "TargetPersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.PeerNetwork", "Caption", c => c.String( maxLength: 200 ) );

            // Create new indexes with new columns
            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                newIndexOneKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipTypeValueId", "Caption" } );

            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                newIndexTwoKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipStartDate", "Caption" } );

            // Add foreign key to Person table.
            AddForeignKey( "dbo.PeerNetwork", "SourcePersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.PeerNetwork", "TargetPersonId", "dbo.Person", "Id" );

            // Drop PersonAlias columns.
            DropColumn( "dbo.PeerNetwork", "SourcePersonAliasId" );
            DropColumn( "dbo.PeerNetwork", "TargetPersonAliasId" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.PeerNetwork", "TargetPersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.PeerNetwork", "SourcePersonAliasId", c => c.Int( nullable: false ) );

            DropForeignKey( "dbo.PeerNetwork", "TargetPersonId", "dbo.Person" );
            DropForeignKey( "dbo.PeerNetwork", "SourcePersonId", "dbo.Person" );

            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( newIndexOneKeys ) );
            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( newIndexTwoKeys ) );

            DropColumn( "dbo.PeerNetwork", "Caption" );
            DropColumn( "dbo.PeerNetwork", "TargetPersonId" );
            DropColumn( "dbo.PeerNetwork", "SourcePersonId" );

            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                oldIndexOneKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipTypeValueId" } );

            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                newIndexTwoKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipStartDate" } );

            AddForeignKey( "dbo.PeerNetwork", "TargetPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.PeerNetwork", "SourcePersonAliasId", "dbo.PersonAlias", "Id" );
        }
    }
}
