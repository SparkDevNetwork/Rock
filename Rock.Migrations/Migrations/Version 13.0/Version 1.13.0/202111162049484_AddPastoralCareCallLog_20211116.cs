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
    ///
    /// </summary>
    public partial class AddPastoralCareCallLog_20211116 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BenevolenceType",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    RequestLavaTemplate = c.String(),
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
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Inserts the default benevolence type with a new generated GUID
            Sql( $@"INSERT INTO [BenevolenceType] ([Name],[Description],[IsActive],[Guid],[CreatedDateTime],[ModifiedDateTime])
                VALUES(
                    'Benevolence','The default benevolence type.',1,'{SystemGuid.BenevolenceType.BENEVOLENCE}','{RockDateTime.Now:s}','{RockDateTime.Now:s}')" );

            CreateTable(
                "dbo.BenevolenceWorkflow",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    BenevolenceTypeId = c.Int( nullable: false ),
                    WorkflowTypeId = c.Int( nullable: false ),
                    TriggerType = c.Int( nullable: false ),
                    QualifierValue = c.String(),
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
                .ForeignKey( "dbo.BenevolenceType", t => t.BenevolenceTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true )
                .Index( t => t.BenevolenceTypeId )
                .Index( t => t.WorkflowTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Adds the BenevolenceTypeId column and set default benevolence type id to 1 which is the default and should not be removed
            AddColumn( "dbo.BenevolenceRequest", "BenevolenceTypeId", ( c ) => c.Int( nullable: false, defaultValue: 1 ) );

            CreateIndex( "dbo.BenevolenceRequest", "BenevolenceTypeId" );
            AddForeignKey( "dbo.BenevolenceRequest", "BenevolenceTypeId", "dbo.BenevolenceType", "Id" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.BenevolenceRequest", "BenevolenceTypeId", "dbo.BenevolenceType" );
            DropForeignKey( "dbo.BenevolenceType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceWorkflow", "WorkflowTypeId", "dbo.WorkflowType" );
            DropForeignKey( "dbo.BenevolenceWorkflow", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceWorkflow", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceWorkflow", "BenevolenceTypeId", "dbo.BenevolenceType" );
            DropIndex( "dbo.BenevolenceWorkflow", new[] { "Guid" } );
            DropIndex( "dbo.BenevolenceWorkflow", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceWorkflow", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceWorkflow", new[] { "WorkflowTypeId" } );
            DropIndex( "dbo.BenevolenceWorkflow", new[] { "BenevolenceTypeId" } );
            DropIndex( "dbo.BenevolenceType", new[] { "Guid" } );
            DropIndex( "dbo.BenevolenceType", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceType", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "BenevolenceTypeId" } );
            DropColumn( "dbo.BenevolenceRequest", "BenevolenceTypeId" );
            DropTable( "dbo.BenevolenceWorkflow" );
            DropTable( "dbo.BenevolenceType" );
        }
    }
}
