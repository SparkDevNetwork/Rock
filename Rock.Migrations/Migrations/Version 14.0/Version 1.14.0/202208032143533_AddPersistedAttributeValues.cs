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
    public partial class AddPersistedAttributeValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AttributeReferencedEntity",
                c => new
                {
                    Id = c.Long( nullable: false, identity: true ),
                    AttributeId = c.Int( nullable: false ),
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Attribute", t => t.AttributeId, cascadeDelete: true )
                .Index( t => t.AttributeId )
                .Index( t => new { t.EntityTypeId, t.EntityId } );

            CreateTable(
                "dbo.AttributeValueReferencedEntity",
                c => new
                {
                    Id = c.Long( nullable: false, identity: true ),
                    AttributeValueId = c.Int( nullable: false ),
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AttributeValue", t => t.AttributeValueId, cascadeDelete: true )
                .Index( t => t.AttributeValueId )
                .Index( t => new { t.EntityTypeId, t.EntityId } );

            AddColumn( "dbo.Attribute", "DefaultPersistedTextValue", c => c.String() );
            AddColumn( "dbo.Attribute", "DefaultPersistedHtmlValue", c => c.String() );
            AddColumn( "dbo.Attribute", "DefaultPersistedCondensedTextValue", c => c.String() );
            AddColumn( "dbo.Attribute", "DefaultPersistedCondensedHtmlValue", c => c.String() );
            AddColumn( "dbo.Attribute", "IsDefaultPersistedValueDirty", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.AttributeValue", "PersistedTextValue", c => c.String() );
            AddColumn( "dbo.AttributeValue", "PersistedHtmlValue", c => c.String() );
            AddColumn( "dbo.AttributeValue", "PersistedCondensedTextValue", c => c.String() );
            AddColumn( "dbo.AttributeValue", "PersistedCondensedHtmlValue", c => c.String() );
            AddColumn( "dbo.AttributeValue", "IsPersistedValueDirty", c => c.Boolean( nullable: false, defaultValue: true ) );
            // EF logic commented out so we can build the computed column properly below.
            //AddColumn("dbo.AttributeValue", "ValueChecksum", c => c.Int(nullable: false));

            Sql( $"ALTER TABLE [dbo].[AttributeValue] ADD [ValueChecksum] AS CHECKSUM([Value])" );

            // add ServiceJob: Update Persisted Attribute Values
            Sql( @"
IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.UpdatePersistedAttributeValues' AND [Guid] = 'A7DDA4B0-BA1D-49F1-8749-5E7A9876AE70' ) 
BEGIN 
    INSERT INTO [ServiceJob] ([IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus], [Guid]) 
    VALUES (0, 1, 'Update Persisted Attribute Values', 'Updates the persisted attribute values.', 'Rock.Jobs.UpdatePersistedAttributeValues', '0 15 2 1/1 * ? *', 1, 'A7DDA4B0-BA1D-49F1-8749-5E7A9876AE70');
END" );

            // Add the custom type that will be used to update the referenced entities.
            Sql( @"
CREATE TYPE [dbo].[AttributeReferencedEntityList] AS TABLE(
	[ReferencedEntityTypeId] [int] NULL,
	[ReferencedEntityId] [int] NULL,
	[EntityId] [int] NULL
)" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DROP TYPE [dbo].[AttributeReferencedEntityList]" );

            Sql( "DELETE FROM [ServiceJob] WHERE [Guid] = 'A7DDA4B0-BA1D-49F1-8749-5E7A9876AE70'" );

            DropForeignKey( "dbo.AttributeValueReferencedEntity", "AttributeValueId", "dbo.AttributeValue" );
            DropForeignKey( "dbo.AttributeReferencedEntity", "AttributeId", "dbo.Attribute" );
            DropIndex( "dbo.AttributeValueReferencedEntity", new[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.AttributeValueReferencedEntity", new[] { "AttributeValueId" } );
            DropIndex( "dbo.AttributeReferencedEntity", new[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.AttributeReferencedEntity", new[] { "AttributeId" } );
            DropColumn( "dbo.AttributeValue", "ValueChecksum" );
            DropColumn( "dbo.AttributeValue", "IsPersistedValueDirty" );
            DropColumn( "dbo.AttributeValue", "PersistedCondensedHtmlValue" );
            DropColumn( "dbo.AttributeValue", "PersistedCondensedTextValue" );
            DropColumn( "dbo.AttributeValue", "PersistedHtmlValue" );
            DropColumn( "dbo.AttributeValue", "PersistedTextValue" );
            DropColumn( "dbo.Attribute", "IsDefaultPersistedValueDirty" );
            DropColumn( "dbo.Attribute", "DefaultPersistedCondensedHtmlValue" );
            DropColumn( "dbo.Attribute", "DefaultPersistedCondensedTextValue" );
            DropColumn( "dbo.Attribute", "DefaultPersistedHtmlValue" );
            DropColumn( "dbo.Attribute", "DefaultPersistedTextValue" );
            DropTable( "dbo.AttributeValueReferencedEntity" );
            DropTable( "dbo.AttributeReferencedEntity" );
        }
    }
}
