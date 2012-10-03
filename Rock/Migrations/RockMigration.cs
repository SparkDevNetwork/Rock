//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Data.Entity.Migrations;
using System.Text;
using Rock.Cms;

namespace Rock.Migrations
{
    /// <summary>
    /// Custom Migration methods
    /// </summary>
    public abstract class RockMigration : DbMigration
    {
        #region Block Type Methods

        /// <summary>
        /// Adds the type of the block.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="path">The path.</param>
        /// <param name="guid">The GUID.</param>
        public void AddBlockType( string name, string description, string path, string guid )
        {
            Sql( string.Format( @"
                
                INSERT INTO [cmsBlockType] (
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

        /// <summary>
        /// Adds the type of the block.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        public void AddBlockType( BlockTypeDto blockType )
        {
            Sql( string.Format( @"
                INSERT INTO [cmsBlockType] (
                    [IsSystem],[Path],[Name],[Description],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    {0},'{1}','{2}','{3}'
                    '{4}','{5}',{6},{7},
                    '{8}')
",
                    blockType.IsSystem.Bit(),
                    blockType.Path,
                    blockType.Name,
                    blockType.Description,
                    blockType.CreatedDateTime,
                    blockType.ModifiedDateTime,
                    blockType.CreatedByPersonId,
                    blockType.ModifiedByPersonId,
                    blockType.Guid ) );
        }

        /// <summary>
        /// Deletes the type of the block.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockType( string guid )
        {
            Sql( string.Format( @"
                DELETE [cmsBlockType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Defaults the type of the system block.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public BlockTypeDto DefaultSystemBlockType( string name, string description, Guid guid )
        {
            var blockType = new BlockTypeDto();

            blockType.IsSystem = true;
            blockType.Name = name;
            blockType.Description = description;
            blockType.CreatedDateTime = DateTime.Now;
            blockType.ModifiedDateTime = DateTime.Now;
            blockType.CreatedByPersonId = 1;
            blockType.ModifiedByPersonId = 1;
            blockType.Guid = guid;

            return blockType;
        }

        #endregion

        #region Page Methods

        /// <summary>
        /// Adds the page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
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

        /// <summary>
        /// Adds the page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="page">The page.</param>
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
                    '{15}','{16}',{17},{18},
                    '{19}','{20}')
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
                    page.CreatedDateTime,
                    page.ModifiedDateTime,
                    page.CreatedByPersonId,
                    page.ModifiedByPersonId,
                    page.IconUrl,
                    page.Guid ) );
        }

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeletePage( string guid )
        {
            Sql( string.Format( @"
                DELETE [cmsPage] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Defaults the system page.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
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
            page.CreatedDateTime = DateTime.Now;
            page.ModifiedDateTime = DateTime.Now;
            page.CreatedByPersonId = 1;
            page.ModifiedByPersonId = 1;
            page.Guid = guid;

            return page;
        }

        #endregion

        #region Block Methods

        /// <summary>
        /// Adds the block.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="blockTypeGuid">The block type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="order">The order.</param>
        public void AddBlock( string pageGuid, string blockTypeGuid, string name, string zone, string guid, int order = 0 )
        {
            var sb = new StringBuilder();

            sb.Append( @"
                DECLARE @PageId int
" );
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                SET @PageId = NULL
" );
            else
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')
", pageGuid );

            sb.AppendFormat( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @BlockId int
                INSERT INTO [cmsBlock] (
                    [IsSystem],[PageId],[Layout],[BlockTypeId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@PageId,NULL,@BlockTypeId,'{1}',
                    {2},'{3}',0,
                    GETDATE(),GETDATE(),1,1,
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
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockId,0,'Edit','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockId,0,'Configure','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
" );
            Sql( sb.ToString() );
        }

        /// <summary>
        /// Adds the block.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="blockTypeGuid">The block type GUID.</param>
        /// <param name="block">The block.</param>
        public void AddBlock( string pageGuid, string blockTypeGuid, BlockDto block )
        {
            var sb = new StringBuilder();

            sb.Append( @"
                DECLARE @PageId int
" );
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                SET @PageId = NULL
" );
            else
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [cmsPage] WHERE [Guid] = '{0}')
", pageGuid );

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @BlockId int
                INSERT INTO [cmsBlock] (
                    [IsSystem],[PageId],[Layout],[BlockTypeId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    {1},@PageId,{2},@BlockTypeId,'{3}',
                    {4},'{5}',{6},
                    '{7}','{8}',{9},{10},
                    '{11}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                    block.IsSystem.Bit(),
                    block.Layout == null ? "NULL" : "'" + block.Layout + "'",
                    block.Zone,
                    block.Order,
                    block.Name,
                    block.OutputCacheDuration,
                    block.CreatedDateTime,
                    block.ModifiedDateTime,
                    block.CreatedByPersonId,
                    block.ModifiedByPersonId,
                    block.Guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockId,0,'Edit','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
                INSERT INTO [cmsAuth] ([EntityType],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],[Guid])
                    VALUES('Cms.BlockInstance',@BlockId,0,'Configure','A',0,NULL,2,GETDATE(),GETDATE(),1,1,NEWID())
" );
            Sql( sb.ToString() );
        }

        /// <summary>
        /// Deletes the block.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlock( string guid )
        {
            Sql( string.Format( @"
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')
                DELETE [cmsAuth] WHERE [EntityType] = 'Cms.BlockInstance' AND [EntityId] = @BlockId
                DELETE [cmsBlock] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Defaults the system block.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public BlockDto DefaultSystemBlock( string name, Guid guid )
        {
            var block = new BlockDto();

            block.IsSystem = true;
            block.Zone = "Content";
            block.Name = name;
            block.CreatedDateTime = DateTime.Now;
            block.ModifiedDateTime = DateTime.Now;
            block.CreatedByPersonId = 1;
            block.ModifiedByPersonId = 1;
            block.Guid = guid;

            return block;
        }

        #endregion

        #region Attribute Methods

        /// <summary>
        /// Adds the block attribute.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, string name, string category, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockTypeId',CAST(@BlockTypeId as varchar),
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
                    guid )
            );
        }

        /// <summary>
        /// Adds the block attribute.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="attribute">The attribute.</param>
        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, Rock.Core.AttributeDto attribute )
        {

            Sql( string.Format( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [cmsBlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [coreFieldType] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttribute] (
                    [IsSystem],[FieldTypeId],[Entity],[EntityQualifierColumn],[EntityQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,'Rock.Cms.BlockInstance','BlockTypeId',CAST(@BlockTypeId as varchar),
                    '{2}','{3}','{4}','{5}',
                    {6},{7},'{8}',{9},{10},
                    '{11}','{12}',{13},{14},
                    '{15}')
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
                    attribute.CreatedDateTime,
                    attribute.ModifiedDateTime,
                    attribute.CreatedByPersonId,
                    attribute.ModifiedByPersonId,
                    attribute.Guid )
            );
        }

        /// <summary>
        /// Deletes the block attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockAttribute( string guid )
        {
            Sql( string.Format( @"
                DELETE [coreAttribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Defaults the block attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public Rock.Core.AttributeDto DefaultBlockAttribute( string name, string category, string description, int order, string defaultValue, Guid guid )
        {
            var attribute = new Rock.Core.AttributeDto();

            attribute.IsSystem = true;
            attribute.Key = name.Replace( " ", string.Empty );
            attribute.Name = name;
            attribute.Category = category;
            attribute.Description = description;
            attribute.Order = order;
            attribute.CreatedDateTime = DateTime.Now;
            attribute.ModifiedDateTime = DateTime.Now;
            attribute.CreatedByPersonId = 1;
            attribute.ModifiedByPersonId = 1;
            attribute.Guid = guid;

            return attribute;
        }

        #endregion

        #region Block Attribute Value Methods

        /// <summary>
        /// Adds the block attribute value.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="value">The value.</param>
        public void AddBlockAttributeValue( string blockGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [cmsBlock] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [coreAttribute] WHERE [Guid] = '{1}')

                INSERT INTO [coreAttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Order],[Value],
                    [CreatedDateTime],[ModifiedDateTime],[CreatedByPersonId],[ModifiedByPersonId],
                    [Guid])
                VALUES(
                    1,@AttributeId,@BlockId,
                    0,'{2}',
                    GETDATE(),GETDATE(),1,1,
                    NEWID())
",
                    blockGuid,
                    attributeGuid,
                    value )
            );
        }

        /// <summary>
        /// Deletes the block attribute value.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
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
                    attributeGuid )
            );
        }

        #endregion

        #region DefinedType Methods

        /// <summary>
        /// Adds the type of the defined.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
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

        /// <summary>
        /// Deletes the type of the defined.
        /// </summary>
        /// <param name="guid">The GUID.</param>
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

        /// <summary>
        /// Adds the defined value.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
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

        /// <summary>
        /// Deletes the defined value.
        /// </summary>
        /// <param name="guid">The GUID.</param>
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