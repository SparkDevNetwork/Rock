//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class CategoryTreeView : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Utility - Category Tree View", "", "~/Blocks/Utility/CategoryTreeView.ascx", "ADE003C7-649B-466A-872B-B8AC952E7841" );

            Sql( @" 
DECLARE @BlockTypeId int
SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'ADE003C7-649B-466A-872B-B8AC952E7841')
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId WHERE [Guid] = '3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA' 
" );
            DeleteAttribute( "C67C785F-4B19-410E-80FA-E5B320D07DD5" );

            UpdateFieldType( "EntityType", "EntityType Field", "Rock", "Rock.Field.Types.EntityType", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB" );

            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Parameter Key", "PageParameterKey", "", "The page parameter to look for", 0, "", "AA057D3E-00CC-42BD-9998-600873356EDB" );
            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", "The types of entities to display categories for", 0, "", "06D414F0-AA20-4D3C-B297-1530CCD64395" );
            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15" );

            // Attrib Value for Workflow Tree:Detail Page Guid
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", "dcb18a76-6dff-48a5-a66e-2caa10d2ca1a" );
            // Attrib Value for Workflow Tree:Page Parameter Key
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "AA057D3E-00CC-42BD-9998-600873356EDB", "workflowTypeId" );
            // Attrib Value for Workflow Tree:Entity Type
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "06D414F0-AA20-4D3C-B297-1530CCD64395", "D550E301-4D3F-4E72-AD19-A0E694304AF3" );

            Sql( @"
IF NOT EXISTS(SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowType')
BEGIN
    INSERT INTO [EntityType] ([Name],[Guid],[IsEntity],[IsSecured])
    VALUES ('Rock.Model.WorkflowType', newid(), 1, 1)
END

DECLARE @EntityTypeId int
SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowType')
UPDATE [AttributeValue] SET [Value] = CAST(@EntityTypeId as varchar) WHERE [Value] = 'D550E301-4D3F-4E72-AD19-A0E694304AF3'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @" 
DECLARE @BlockTypeId int
SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = 'BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C')
UPDATE [Block] SET [BlockTypeId] = @BlockTypeId WHERE [Guid] = '3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA' 
" );

            DeleteAttribute( "AA057D3E-00CC-42BD-9998-600873356EDB" );
            DeleteAttribute( "06D414F0-AA20-4D3C-B297-1530CCD64395" );
            DeleteAttribute( "AEE521D8-124D-4BB3-8A80-5F368E5CEC15" );

            DeleteBlockType( "ADE003C7-649B-466A-872B-B8AC952E7841" );

            DeleteFieldType( "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB" );

            AddBlockTypeAttribute( "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "C67C785F-4B19-410E-80FA-E5B320D07DD5" );
            AddBlockAttributeValue( "3DAAE6A6-E8AC-435F-A449-96E75D9E8ACA", "C67C785F-4B19-410E-80FA-E5B320D07DD5", "dcb18a76-6dff-48a5-a66e-2caa10d2ca1a" );

        }
    }
}
