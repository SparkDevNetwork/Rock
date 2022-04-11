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
    /// Add CampusTopic model and supporting Defined Type.
    /// </summary>
    public partial class AddCampusTopic : Rock.Migrations.RockMigration
    {
        #region Up

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCampusTopicTable();
            AddTopicTypeDefinedType();
        }

        private void AddCampusTopicTable()
        {
            CreateTable(
                "dbo.CampusTopic",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    TopicTypeValueId = c.Int( nullable: false ),
                    Email = c.String( maxLength: 254 ),
                    IsPublic = c.Boolean( nullable: false ),
                    CampusId = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Campus", t => t.CampusId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.DefinedValue", t => t.TopicTypeValueId )
                .Index( t => t.TopicTypeValueId )
                .Index( t => t.CampusId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void AddTopicTypeDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Topic Type", "List of general topics that are associated with the campus.", Rock.SystemGuid.DefinedType.TOPIC_TYPE );
        }

        #endregion Up

        #region Down

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveCampusTopicTable();
            RemoveTopicTypeDefinedType();
        }

        private void RemoveCampusTopicTable()
        {
            DropForeignKey( "dbo.CampusTopic", "TopicTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.CampusTopic", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CampusTopic", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CampusTopic", "CampusId", "dbo.Campus" );
            DropIndex( "dbo.CampusTopic", new[] { "Guid" } );
            DropIndex( "dbo.CampusTopic", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CampusTopic", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CampusTopic", new[] { "CampusId" } );
            DropIndex( "dbo.CampusTopic", new[] { "TopicTypeValueId" } );
            DropTable( "dbo.CampusTopic" );
        }

        private void RemoveTopicTypeDefinedType()
        {
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.TOPIC_TYPE );
        }

        #endregion Down
    }
}
