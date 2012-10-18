using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Cms;

#pragma warning disable 1591

namespace Rock.Migrations
{
    /// <summary>
    /// Custom Migration methods for migrations prior to the 201209211556505_BlockToBlockType migration
    /// </summary>
    public abstract class RockMigration_0 : DbMigration
    {
        #region Block Methods

        public void AddBlock( string name, string description, string path, string guid )
        {
            Sql( string.Format( @"
                
                INSERT INTO [cmsBlock] (
                    [IsSystem],[Path],[Name],[Description],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,'{0}','{1}','{2}',
                    GETDATE(),GETDATE(),1,1,
                    '{3}')
",
                    path,
                    name,
                    description,
                    guid
                    ) );
        }

        public void AddBlock( BlockTypeDto block )
        {
            Sql( string.Format( @"
                INSERT INTO [cmsBlock] (
                    [IsSystem],[Path],[Name],[Description],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    {0},'{1}','{2}','{3}'
                    GETDATE(),GETDATE(),1,1,
                    '{4}')
",
                    block.IsSystem.Bit(),
                    block.Path,
                    block.Name,
                    block.Description,
                    block.Guid ) );
        }

        public void DeleteBlock( string guid )
        {
            Sql( string.Format( @"
                DELETE [cmsBlock] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public BlockTypeDto DefaultSystemBlock( string name, string description, Guid guid )
        {
            var block = new BlockTypeDto();

            block.IsSystem = true;
            block.Name = name;
            block.Description = description;
            block.Guid = guid;

            return block;
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
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [IconUrl],[Guid])
                VALUES(
                    '{1}','{1}',1,@ParentPageId,1,'Default',
                    0,1,1,0,1,0,
                    @Order,0,'{2}',1,
                    GETDATE(),GETDATE(),1,1,
                    '','{3}')
",
                    parentPageGuid,
                    name,
                    description,
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
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [IconUrl],[Guid])
                VALUES(
                    '{1}','{2}',{3},@ParentPageId,{4},'{5}',
                    {6},{7},{8},{9},{10},{11},
                    @Order,{12},'{13}',{14},
                    GETDATE(),GETDATE(),1,1,
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
                    page.Description,
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

        #region BlockInstance Methods

        public void AddBlockInstance( string pageGuid, string blockGuid, string name, string zone, string guid, int order = 0 )
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
                
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @BlockInstanceId int
                INSERT INTO [cmsBlockInstance] (
                    [IsSystem],[PageId],[Layout],[BlockId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@PageId,NULL,@BlockId,'{1}',
                    {2},'{3}',0,
                    GETDATE(),GETDATE(),1,1,
                    '{4}')
                SET @BlockInstanceId = SCOPE_IDENTITY()
",
                    blockGuid,
                    zone,
                    order,
                    name,
                    guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockInstanceId,0,'Edit','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockInstanceId,0,'Configure','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
" );
            Sql(sb.ToString());
        }

        public void AddBlockInstance( string pageGuid, string blockGuid, BlockDto blockInstance )
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

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @BlockInstanceId int
                INSERT INTO [cmsBlockInstance] (
                    [IsSystem],[PageId],[Layout],[BlockId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    {1},@PageId,{2},@BlockId,'{3}',
                    {4},'{5}',{6},
                    GETDATE(),GETDATE(),1,1,
                    '{7}')
                SET @BlockInstanceId = SCOPE_IDENTITY()
",
                    blockGuid,
                     blockInstance.IsSystem.Bit(),
                    blockInstance.Layout == null ? "NULL" : "'" + blockInstance.Layout + "'",
                    blockInstance.Zone,
                    blockInstance.Order,
                    blockInstance.Name,
                    blockInstance.OutputCacheDuration,
                    blockInstance.Guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockInstanceId,0,'Edit','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockInstanceId,0,'Configure','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
" );
            Sql( sb.ToString() );
        }

        public void DeleteBlockInstance( string guid )
        {
            Sql( string.Format( @"
                DECLARE @BlockInstanceId int
                SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '{0}')
                DELETE [cmsAuth] WHERE [EntityType] = 'Cms.BlockInstance' AND [EntityId] = @BlockInstanceId
                DELETE [cmsBlockInstance] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public BlockDto DefaultSystemBlockInstance( string name, Guid guid )
        {
            var blockInstance = new BlockDto();

            blockInstance.IsSystem = true;
            blockInstance.Zone = "Content";
            blockInstance.Name = name;
            blockInstance.Guid = guid;

            return blockInstance;
        }

        #endregion

        #region Attribute Methods

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, string name, string category, string description, int order, string defaultValue, string guid)
        {
            Sql( string.Format( @"
                
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockId',CAST(@BlockId as varchar),
                    '{2}','{3}','{4}','{5}',
                    {6},0,'{7}',0,0,
                    GETDATE(),GETDATE(),1,1,
                    '{8}')
",
                    blockGuid,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    category,
                    description,
                    order,
                    defaultValue,
                    guid)
            );
        }

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, Rock.Core.AttributeDto attribute )
        {

            Sql( string.Format( @"

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockId',CAST(@BlockId as varchar),
                    '{2}','{3}','{4}','{5}',
                    {6},{7},'{8}',{9},{10},
                    GETDATE(),GETDATE(),1,1,
                    '{11}')
",
                    blockGuid,
                    fieldTypeGuid,
                    attribute.Key,
                    attribute.Name,
                    attribute.Category,
                    attribute.Description,
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

        public void AddBlockAttributeValue( string blockInstanceGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockInstanceId int
                SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Order],[Value],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@AttributeId,@BlockInstanceId,
                    0,'{2}',
                    GETDATE(),GETDATE(),1,1,
                    NEWID())
",
                    blockInstanceGuid,
                    attributeGuid,
                    value)
            );
        }

        public void DeleteBlockAttributeValue( string blockInstanceGuid, string attributeGuid )
        {
            Sql( string.Format( @"

                DECLARE @BlockInstanceId int
                SET @BlockInstanceId = (SELECT [Id] FROM [cmsBlockInstance] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

                DELETE [coreAttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockInstanceId
",
                    blockInstanceGuid,
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
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@Order,
                    '{0}','{1}','{2}',
                    GETDATE(),GETDATE(),1,1,
                    '{3}')
",
                    category,
                    name,
                    description,
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

        public void AddDefinedValue( string definedTypeGuid, string name, string description, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [coreDefinedType] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [coreDefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                INSERT INTO [coreDefinedValue] (
                    [IsSystem],[DefinedTypeId],[Order],
                    [Name],[Description],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@DefinedTypeId,@Order,
                    '{1}','{2}',
                    GETDATE(),GETDATE(),1,1,
                    '{3}')
",
                    definedTypeGuid,
                    name,
                    description,
                    guid
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