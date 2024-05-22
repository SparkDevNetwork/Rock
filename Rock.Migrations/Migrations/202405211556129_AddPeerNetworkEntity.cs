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
    public partial class AddPeerNetworkEntity : Rock.Migrations.RockMigration
    {
        private readonly string[] indexOneKeys = new[] { "SourcePersonAliasId", "TargetPersonAliasId", "RelationshipEndDate" };
        private readonly string[] indexTwoKeys = new[] { "SourcePersonAliasId", "RelationshipTypeValueId", "RelationshipEndDate" };

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Peer Network Table.
            CreateTable(
                "dbo.PeerNetwork",
                c => new
                {
                    Id = c.Long( nullable: false, identity: true ),
                    SourcePersonAliasId = c.Int( nullable: false ),
                    TargetPersonAliasId = c.Int( nullable: false ),
                    RelationshipTypeValueId = c.Int( nullable: false ),
                    RelationshipStartDate = c.DateTime( nullable: false, storeType: "date" ),
                    RelationshipEndDate = c.DateTime( storeType: "date" ),
                    RelatedEntityId = c.Int(),
                    ClassificationEntityId = c.Int(),
                    RelationshipScore = c.Decimal( nullable: false, precision: 8, scale: 1 ),
                    RelationshipScoreLastUpdateValue = c.Decimal( nullable: false, precision: 8, scale: 1 ),
                    RelationshipTrend = c.Int( nullable: false ),
                    LastUpdateDateTime = c.DateTime( nullable: false ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.DefinedValue", t => t.RelationshipTypeValueId )
                .ForeignKey( "dbo.PersonAlias", t => t.SourcePersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.TargetPersonAliasId );

            // Add Indexes.
            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                indexOneKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipTypeValueId" } );

            RockMigrationHelper.CreateIndexIfNotExists( "PeerNetwork",
                indexTwoKeys,
                new[] { "RelationshipScore", "RelationshipTrend", "RelationshipStartDate" } );

            // Update Group Table.
            AddColumn( "dbo.Group", "RelationshipGrowthEnabledOverride", c => c.Boolean() );
            AddColumn( "dbo.Group", "RelationshipStrengthOverride", c => c.Int() );
            AddColumn( "dbo.Group", "LeaderToLeaderRelationshipMultiplierOverride", c => c.Decimal( precision: 8, scale: 2 ) );
            AddColumn( "dbo.Group", "LeaderToNonLeaderRelationshipMultiplierOverride", c => c.Decimal( precision: 8, scale: 2 ) );
            AddColumn( "dbo.Group", "NonLeaderToNonLeaderRelationshipMultiplierOverride", c => c.Decimal( precision: 8, scale: 2 ) );
            AddColumn( "dbo.Group", "NonLeaderToLeaderRelationshipMultiplierOverride", c => c.Decimal( precision: 8, scale: 2 ) );

            // Update Group Type Table.
            AddColumn( "dbo.GroupType", "IsPeerNetworkEnabled", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "RelationshipGrowthEnabled", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.GroupType", "RelationshipStrength", c => c.Int( nullable: false, defaultValue: 0 ) );
            AddColumn( "dbo.GroupType", "LeaderToLeaderRelationshipMultiplier", c => c.Decimal( nullable: false, precision: 8, scale: 2, defaultValue: 1.0M ) );
            AddColumn( "dbo.GroupType", "LeaderToNonLeaderRelationshipMultiplier", c => c.Decimal( nullable: false, precision: 8, scale: 2, defaultValue: 1.0M ) );
            AddColumn( "dbo.GroupType", "NonLeaderToNonLeaderRelationshipMultiplier", c => c.Decimal( nullable: false, precision: 8, scale: 2, defaultValue: 1.0M ) );
            AddColumn( "dbo.GroupType", "NonLeaderToLeaderRelationshipMultiplier", c => c.Decimal( nullable: false, precision: 8, scale: 2, defaultValue: 1.0M ) );

            // Update Group Type Role Table.
            AddColumn( "dbo.GroupTypeRole", "IsExcludedFromPeerNetwork", c => c.Boolean( nullable: false, defaultValue: false ) );

            // Add Defined Type.
            RockMigrationHelper.AddDefinedType( "Person", "Peer Network Relationship Type", "List of different types of relationships an individual could have in their Peer Network.", SystemGuid.DefinedType.PEER_NETWORK_RELATIONSHIP_TYPE );

            // Add Defined Value.
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.PEER_NETWORK_RELATIONSHIP_TYPE, "Group Connections", "Links individuals who are in common groups with one another.", "CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.PeerNetwork", "TargetPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PeerNetwork", "SourcePersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PeerNetwork", "RelationshipTypeValueId", "dbo.DefinedValue" );

            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( indexOneKeys ) );
            RockMigrationHelper.DropIndexIfExists( "PeerNetwork", MigrationIndexHelper.GenerateIndexName( indexTwoKeys ) );

            DropColumn( "dbo.GroupTypeRole", "IsExcludedFromPeerNetwork" );

            DropColumn( "dbo.GroupType", "NonLeaderToLeaderRelationshipMultiplier" );
            DropColumn( "dbo.GroupType", "NonLeaderToNonLeaderRelationshipMultiplier" );
            DropColumn( "dbo.GroupType", "LeaderToNonLeaderRelationshipMultiplier" );
            DropColumn( "dbo.GroupType", "LeaderToLeaderRelationshipMultiplier" );
            DropColumn( "dbo.GroupType", "RelationshipStrength" );
            DropColumn( "dbo.GroupType", "RelationshipGrowthEnabled" );
            DropColumn( "dbo.GroupType", "IsPeerNetworkEnabled" );

            DropColumn( "dbo.Group", "NonLeaderToLeaderRelationshipMultiplierOverride" );
            DropColumn( "dbo.Group", "NonLeaderToNonLeaderRelationshipMultiplierOverride" );
            DropColumn( "dbo.Group", "LeaderToNonLeaderRelationshipMultiplierOverride" );
            DropColumn( "dbo.Group", "LeaderToLeaderRelationshipMultiplierOverride" );
            DropColumn( "dbo.Group", "RelationshipStrengthOverride" );
            DropColumn( "dbo.Group", "RelationshipGrowthEnabledOverride" );

            DropTable( "dbo.PeerNetwork" );

            RockMigrationHelper.DeleteDefinedValue( "CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40" );
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.PEER_NETWORK_RELATIONSHIP_TYPE );
        }
    }
}
