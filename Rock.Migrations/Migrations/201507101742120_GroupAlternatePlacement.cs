// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupAlternatePlacement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "AcceptAlternatePlacements", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "EnableAlternatePlacements", c => c.Boolean(nullable: false));

            // MP: PageViews DateTime Index
            Sql( @"
    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_DateTimeViewed' AND object_id = OBJECT_ID('PageView'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_DateTimeViewed] ON [dbo].[PageView]
        (
         [DateTimeViewed] ASC
        )
    END" );

            // JE: Missing Block settings on timeline
            Sql( @"
 DECLARE @NoteBlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '0B2B550C-B0C9-420E-9CF3-BEC8979108F2')
  DECLARE @HeadingAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69')
  DECLARE @IconAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'B69937BE-000A-4B94-852F-16DE92344392')
  DECLARE @NoteHeadingAttribute int = (SELECT TOP 1 [Id] FROM [AttributeValue] WHERE [AttributeId] = @HeadingAttributeId AND EntityId = @NoteBlockId)

  IF (NOT EXISTS(SELECT * FROM [AttributeValue] WHERE [AttributeId] = @IconAttributeId AND [EntityId] = @NoteBlockId))
  BEGIN
		INSERT INTO [AttributeValue]
			([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
			(0, @IconAttributeId, @NoteBlockId, 'fa fa-quote-left', 'EA705B06-E7D8-41AF-4B04-045430795700')
  END

  IF (NOT EXISTS(SELECT * FROM [AttributeValue] WHERE [AttributeId] = @HeadingAttributeId AND [EntityId] = @NoteBlockId))
  BEGIN
		INSERT INTO [AttributeValue]
			([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
		VALUES
			(0, @HeadingAttributeId, @NoteBlockId, 'Timeline', 'F5B28AEF-574B-C695-4C37-AB12393620B2')
  END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupType", "EnableAlternatePlacements");
            DropColumn("dbo.Group", "AcceptAlternatePlacements");
        }
    }
}
