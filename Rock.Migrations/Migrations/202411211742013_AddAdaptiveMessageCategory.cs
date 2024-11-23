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
    public partial class AddAdaptiveMessageCategory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.AdaptiveMessageCategory", "Adaptive Message Category", "", true, true, Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE_CATEGORY );
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE_CATEGORY, "All Church", "", "", "2512EC06-CF27-402C-9CCC-EFD97174F6E4" );
            DropIndex( "dbo.AdaptiveMessageCategory", new[] { "AdaptiveMessageId" });
            DropIndex("dbo.AdaptiveMessageCategory", new[] { "CategoryId" });
            DropPrimaryKey("dbo.AdaptiveMessageCategory");
            AddColumn("dbo.AdaptiveMessageCategory", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.AdaptiveMessageCategory", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.AdaptiveMessageCategory", "Guid", c => c.Guid(nullable: false));
            AddColumn("dbo.AdaptiveMessageCategory", "ForeignId", c => c.Int());
            AddColumn("dbo.AdaptiveMessageCategory", "ForeignGuid", c => c.Guid());
            AddColumn("dbo.AdaptiveMessageCategory", "ForeignKey", c => c.String(maxLength: 100));
            AddPrimaryKey("dbo.AdaptiveMessageCategory", "Id");
            Sql( @"
DECLARE @AdaptiveMsgCategoryEntityTypeId INT = (SELECT Id FROM [EntityType] WHERE [Guid]='D47BDA25-03A3-46EE-A0A6-F8B220E39E4A')
DECLARE @AdaptiveMsgEntityTypeId INT = (SELECT Id FROM [EntityType] WHERE [Guid]='63D98F58-DA81-46AE-AE0C-662A7BFAA7D0')
DECLARE @AllChurchCategory INT = (SELECT Id FROM [Category] WHERE [Guid] = '2512EC06-CF27-402C-9CCC-EFD97174F6E4')
UPDATE
	[Category]
SET [EntityTypeId] = @AdaptiveMsgCategoryEntityTypeId
WHERE [EntityTypeId] = @AdaptiveMsgEntityTypeId

UPDATE [AdaptiveMessageCategory] SET [Guid] = NEWID()

INSERT INTO [AdaptiveMessageCategory]
([AdaptiveMessageId],[CategoryId],[Guid],[Order])
SELECT [Id], @AllChurchCategory, NEWID(), 0 FROM [AdaptiveMessage] WHERE [Id] NOT IN (SELECT [AdaptiveMessageId] FROM [AdaptiveMessageCategory])" );

            CreateIndex( "dbo.AdaptiveMessageCategory", new[] { "AdaptiveMessageId", "CategoryId" }, name: "IX_AdaptiveMessageCategory");
            CreateIndex("dbo.AdaptiveMessageCategory", "Guid", unique: true);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.AdaptiveMessageCategory", new[] { "Guid" });
            DropIndex("dbo.AdaptiveMessageCategory", "IX_AdaptiveMessageCategory");
            DropPrimaryKey("dbo.AdaptiveMessageCategory");
            DropColumn("dbo.AdaptiveMessageCategory", "ForeignKey");
            DropColumn("dbo.AdaptiveMessageCategory", "ForeignGuid");
            DropColumn("dbo.AdaptiveMessageCategory", "ForeignId");
            DropColumn("dbo.AdaptiveMessageCategory", "Guid");
            DropColumn("dbo.AdaptiveMessageCategory", "Order");
            DropColumn("dbo.AdaptiveMessageCategory", "Id");
            AddPrimaryKey("dbo.AdaptiveMessageCategory", new[] { "AdaptiveMessageId", "CategoryId" });
            CreateIndex("dbo.AdaptiveMessageCategory", "CategoryId");
            CreateIndex("dbo.AdaptiveMessageCategory", "AdaptiveMessageId");
        }
    }
}
