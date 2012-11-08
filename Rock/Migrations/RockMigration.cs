using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Cms;


namespace Rock.Migrations
{
#pragma warning disable 1591
    /// <summary>
    /// Custom Migration methods
    /// </summary>
    public abstract class RockMigration : DbMigration
    {
        #region Block Type Methods

        public void AddBlockType( string name, string description, string path, string guid )
        {
            Sql( string.Format( @"
                
                INSERT INTO [cmsBlockType] (
                    [IsSystem],[Path],[Name],[Description],
                    [Guid])
                VALUES(
                    1,'{0}','{1}','{2}',
                    '{3}')
",
                    path,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        public void AddBlockType( BlockTypeDto blockType )
        {
            Sql( string.Format( @"
                INSERT INTO [cmsBlockType] (
                    [IsSystem],[Path],[Name],[Description],
                    [Guid])
                VALUES(
                    {0},'{1}','{2}','{3}'
                    '{4}')
",
                    blockType.IsSystem.Bit(),
                    blockType.Path,
                    blockType.Name,
                    blockType.Description.Replace( "'", "''" ),
                    blockType.Guid ) );
        }

        public void DeleteBlockType( string guid )
        {
            Sql( string.Format( @"
                DELETE [cmsBlockType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public BlockTypeDto DefaultSystemBlockType( string name, string description, Guid guid )
        {
            var blockType = new BlockTypeDto();

            blockType.IsSystem = true;
            blockType.Name = name;
            blockType.Description = description;
            blockType.Guid = guid;

            return blockType;
        }

        #endregion        
        
        #region Page Methods

        public void AddPage( string parentPageGuid, string name, string description, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @ParentPageId int
                SET @ParentPageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [cmsPage] (
                    [Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
                    [RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconUrl],[Guid])
                VALUES(
                    '{1}','{1}',1,@ParentPageId,1,'Default',
                    0,1,1,0,1,0,
                    @Order,0,'{2}',1,
                    '','{3}')
",
                    parentPageGuid,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        public void AddPage( string parentPageGuid, PageDto page )
        {

            Sql( string.Format( @"

                DECLARE @ParentPageId int
                SET @ParentPageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [cmsPage] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [cmsPage] (
                    [Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
                    [RequiresEncryption],[EnableViewState],[MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconUrl],[Guid])
                VALUES(
                    '{1}','{2}',{3},@ParentPageId,{4},'{5}',
                    {6},{7},{8},{9},{10},{11},
                    @Order,{12},'{13}',{14},
                    '{15}','{16}')
",
                    parentPageGuid,
                    page.Name,
                    page.Title,
                    page.IsSystem.Bit(),
                    page.SiteId,
                    page.Layout,
                    page.RequiresEncryption.Bit(),
                    page.EnableViewState.Bit(),
                    page.MenuDisplayDescription.Bit(),
                    page.MenuDisplayIcon.Bit(),
                    page.MenuDisplayChildPages.Bit(),
                    (int)page.DisplayInNavWhen,
                    page.OutputCacheDuration,
                    page.Description.Replace( "'", "''" ),
                    page.IncludeAdminFooter.Bit(),
                    page.IconUrl,
                    page.Guid ) );
        }

        public void DeletePage( string guid )
        {
            Sql( string.Format( @"
                DELETE [cmsPage] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public PageDto DefaultSystemPage( string name, string description, Guid guid )
        {
            var page = new PageDto();

            page.Name = name;
            page.Title = name;
            page.IsSystem = true;
            page.SiteId = 1;
            page.Layout = "Default";
            page.EnableViewState = true; ;
            page.MenuDisplayDescription = true;
            page.MenuDisplayChildPages = true;
            page.DisplayInNavWhen = DisplayInNavWhen.WhenAllowed;
            page.Description = description;
            page.IncludeAdminFooter = true;
            page.Guid = guid;

            return page;
        }

        #endregion

        #region Block Methods

        public void AddBlock( string pageGuid, string blockTypeGuid, string name, string zone, string guid, int order = 0 )
        {
            var sb = new StringBuilder();

            sb.Append(@"
                DECLARE @PageId int
");
            if (string.IsNullOrWhiteSpace(pageGuid))
                sb.Append( @"
                SET @PageId = NULL
");
            else
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')
",                     pageGuid);

            sb.AppendFormat( @"
                
                DECLARE @BlockTypeId int
                DECLARE @EntityTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [coreEntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')

                DECLARE @BlockId int
                INSERT INTO [cmsBlock] (
                    [IsSystem],[PageId],[Layout],[BlockTypeId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [Guid])
                VALUES(
                    1,@PageId,NULL,@BlockTypeId,'{1}',
                    {2},'{3}',0,
                    '{4}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                    zone,
                    order,
                    name,
                    guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [cmsAuth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
" );
            Sql(sb.ToString());
        }

        public void AddBlock( string pageGuid, string blockTypeGuid, BlockDto block )
        {
            var sb = new StringBuilder();

            sb.Append(@"
                DECLARE @PageId int
");
            if (string.IsNullOrWhiteSpace(pageGuid))
                sb.Append( @"
                SET @PageId = NULL
");
            else
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')
",                     pageGuid);

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                DECLARE @EntityTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [coreEntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')

                DECLARE @BlockId int
                INSERT INTO [cmsBlock] (
                    [IsSystem],[PageId],[Layout],[BlockTypeId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [Guid])
                VALUES(
                    {1},@PageId,{2},@BlockTypeId,'{3}',
                    {4},'{5}',{6},
                    '{7}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                     block.IsSystem.Bit(),
                    block.Layout == null ? "NULL" : "'" + block.Layout + "'",
                    block.Zone,
                    block.Order,
                    block.Name,
                    block.OutputCacheDuration,
                    block.Guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [cmsAuth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [cmsAuth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
" );
            Sql( sb.ToString() );
        }

        public void DeleteBlock( string guid )
        {
            Sql( string.Format( @"
                DECLARE @BlockId int
                DECLARE @EntityTypeId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [coreEntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')
                DELETE [cmsAuth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
                DELETE [cmsBlock] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public BlockDto DefaultSystemBlock( string name, Guid guid )
        {
            var block = new BlockDto();

            block.IsSystem = true;
            block.Zone = "Content";
            block.Name = name;
            block.Guid = guid;

            return block;
        }

        #endregion

        #region Attribute Methods

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, string name, string category, string description, int order, string defaultValue, string guid)
        {
            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [coreAttribute] 
                WHERE [Entity] = 'Rock.Cms.Block'
                AND [EntityQualifierColumn] = 'BlockTypeId'
                AND [EntityQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.Block','BlockTypeId',CAST(@BlockTypeId as varchar),
                    '{2}','{3}','{4}','{5}',
                    {6},0,'{7}',0,0,
                    '{8}')
",
                    blockGuid,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    category,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid)
            );
        }

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, Rock.Core.AttributeDto attribute )
        {

            Sql( string.Format( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [coreAttribute] 
                WHERE [Entity] = 'Rock.Cms.Block'
                AND [EntityQualifierColumn] = 'BlockTypeId'
                AND [EntityQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.Block','BlockTypeId',CAST(@BlockTypeId as varchar),
                    '{2}','{3}','{4}','{5}',
                    {6},{7},'{8}',{9},{10},
                    '{11}')
",
                    blockGuid,
                    fieldTypeGuid,
                    attribute.Key,
                    attribute.Name,
                    attribute.Category,
                    attribute.Description.Replace("'","''"),
                    attribute.Order,
                    attribute.IsGridColumn.Bit(),
                    attribute.DefaultValue,
                    attribute.IsMultiValue.Bit(),
                    attribute.IsRequired.Bit(),
                    attribute.Guid )
            );
        }

        public void DeleteBlockAttribute( string guid )
        {
            Sql( string.Format( @"
                DELETE [coreAttribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public Rock.Core.AttributeDto DefaultBlockAttribute( string name, string category, string description, int order, string defaultValue, Guid guid )
        {
            var attribute = new Rock.Core.AttributeDto();

            attribute.IsSystem = true;
            attribute.Key = name.Replace( " ", string.Empty );
            attribute.Name = name;
            attribute.Category = category;
            attribute.Description = description;
            attribute.Order = order;
            attribute.Guid = guid;

            return attribute;
        }

        #endregion

        #region Block Attribute Value Methods

        public void AddBlockAttributeValue( string blockGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [coreAttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @BlockId

                INSERT INTO [coreAttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Order],[Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@BlockId,
                    0,'{2}',
                    NEWID())
",
                    blockGuid,
                    attributeGuid,
                    value)
            );
        }

        public void DeleteBlockAttributeValue( string blockGuid, string attributeGuid )
        {
            Sql( string.Format( @"

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

                DELETE [coreAttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
",
                    blockGuid,
                    attributeGuid)
            );
        }

        #endregion

        #region DefinedType Methods

        public void AddDefinedType( string category, string name, string description, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [coreDefinedType];

                INSERT INTO [coreDefinedType] (
                    [IsSystem],[FieldTypeId],[Order],
                    [Category],[Name],[Description],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@Order,
                    '{0}','{1}','{2}',
                    '{3}')
",
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        public void DeleteDefinedType( string guid )
        {
            Sql( string.Format( @"
                DELETE [coreDefinedType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region DefinedType Methods

        public void AddDefinedValue( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"
                
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [coreDefinedType] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [coreDefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                INSERT INTO [coreDefinedValue] (
                    [IsSystem],[DefinedTypeId],[Order],
                    [Name],[Description],
                    [Guid])
                VALUES(
                    {4},@DefinedTypeId,@Order,
                    '{1}','{2}',
                    '{3}')
",
                    definedTypeGuid,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    isSystem.Bit().ToString()
                    ) );
        }

        public void DeleteDefinedValue( string guid )
        {
            Sql( string.Format( @"
                DELETE [coreDefinedValue] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion
    }
}