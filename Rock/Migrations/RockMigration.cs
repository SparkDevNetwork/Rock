using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Model;


namespace Rock.Migrations
{
    /// <summary>
    /// Custom Migration methods
    /// </summary>
    public abstract class RockMigration : DbMigration
    {

        #region Entity Type Methods

        /// <summary>
        /// Updates the type of the entity.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="isEntity">if set to <c>true</c> [is entity].</param>
        /// <param name="isSecured">if set to <c>true</c> [is secured].</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateEntityType( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [EntityType] (
                        [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[Guid])
                    VALUES(
                        '{0}','{1}','{2}',{3},{4},'{5}')
                END
                ELSE
                BEGIN
                    UPDATE [EntityType] SET 
                        [FriendlyName] = '{1}',
                        [AssemblyName] = '{2}',
                        [IsEntity] = {3},
                        [IsSecured] = {4},
                        [Guid] = '{5}'
                    WHERE [Name] = '{0}'
                END
",
                    name.Replace( "'", "''" ),
                    friendlyName.Replace( "'", "''" ),
                    assemblyName.Replace( "'", "''" ),
                    isEntity ? "1" : "0",
                    isSecured ? "1" : "0",
                    guid ) );
        }

        /// <summary>
        /// Deletes the type of the entity.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteEntityType( string guid )
        {
            Sql( string.Format( @"
                DELETE [EntityType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion


        #region Field Type Methods

        /// <summary>
        /// Updates the type of the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="IsSystem">if set to <c>true</c> [is system].</param>
        public void UpdateFieldType( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = '{2}' AND [Class] = '{3}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [FieldType] (
                        [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                    VALUES(
                        '{0}','{1}','{2}','{3}','{4}',{5})
                END
                ELSE
                BEGIN
                    UPDATE [FieldType] SET 
                        [Name] = '{0}', 
                        [Description] = '{1}',
                        [Guid] = '{4}',
                        [IsSystem] = {5}
                    WHERE [Assembly] = '{2}'
                    AND [Class] = '{3}'
                END
",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    assembly,
                    className,
                    guid,
                    IsSystem ? "1" : "0") );
        }

        /// <summary>
        /// Adds the type of the field.
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        public void AddFieldType( FieldType fieldType )
        {
            Sql( string.Format( @"
                INSERT INTO [FieldType] (
                    [IsSystem],[Name],[Description],[Assembly],[Class],
                    [Guid])
                VALUES(
                    {0},'{1}','{2}','{3}','{4}',
                    '{5}')
",
                    fieldType.IsSystem.Bit(),
                    fieldType.Name,
                    fieldType.Assembly,
                    fieldType.Class,
                    fieldType.Description.Replace( "'", "''" ),
                    fieldType.Guid ) );
        }

        /// <summary>
        /// Deletes the type of the field.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteFieldType( string guid )
        {
            Sql( string.Format( @"
                DELETE [FieldType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

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
                
                INSERT INTO [BlockType] (
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

        /// <summary>
        /// Adds the type of the block.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        public void AddBlockType( BlockType blockType )
        {
            Sql( string.Format( @"
                INSERT INTO [BlockType] (
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

        /// <summary>
        /// Deletes the type of the block.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockType( string guid )
        {
            Sql( string.Format( @"
                DELETE [BlockType] WHERE [Guid] = '{0}'
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
        public BlockType DefaultSystemBlockType( string name, string description, Guid guid )
        {
            var blockType = new BlockType();

            blockType.IsSystem = true;
            blockType.Name = name;
            blockType.Description = description;
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
        /// <param name="layout">The layout.</param>
        /// <param name="guid">The GUID.</param>
        public void AddPage( string parentPageGuid, string name, string description, string layout, string guid, string iconCssClass = ""  )
        {
            Sql( string.Format( @"
                
                DECLARE @ParentPageId int
                SET @ParentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [Page] (
                    [Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
                    [RequiresEncryption],[EnableViewState],
                    [PageDisplayTitle],[PageDisplayBreadCrumb],[PageDisplayIcon],[PageDisplayDescription],
                    [MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [BreadCrumbDisplayName],[BreadCrumbDisplayIcon],
                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconFileId],[IconCssClass],[Guid])
                VALUES(
                    '{1}','{1}',1,@ParentPageId,1,'{3}',
                    0,1,
                    1,1,1,1,
                    0,0,1,0,
                    1,0,
                    @Order,0,'{2}',1,
                    null,'{5}','{4}')
",
                    parentPageGuid,
                    name,
                    description.Replace( "'", "''" ),
                    layout,
                    guid,
                    iconCssClass
                    ) );
        }

        /// <summary>
        /// Adds the page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddPage( string parentPageGuid, string name, string description, string guid )
        {
            AddPage( parentPageGuid, name, description, "Default", guid );
        }

        /// <summary>
        /// Adds the page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="page">The page.</param>
        public void AddPage( string parentPageGuid, Page page )
        {

            Sql( string.Format( @"

                DECLARE @ParentPageId int
                SET @ParentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [Page] (
                    [Name],[Title],[IsSystem],[ParentPageId],[SiteId],[Layout],
                    [RequiresEncryption],[EnableViewState],
                    [PageDisplayTitle],[PageDisplayBreadCrumb],[PageDisplayIcon],[PageDisplayDescription],
                    [MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [BreadCrumbDisplayName],[BreadCrumbDisplayIcon],

                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconFileId],[Guid])
                VALUES(
                    '{1}','{2}',{3},@ParentPageId,{4},'{5}',
                    {6},{7},
                    {17},{18},{19},{20}.
                    {8},{9},{10},{11},
                    {21},{22},
                    @Order,{12},'{13}',{14},
                    {15},'{16}')
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
                    page.IconFileId.HasValue ? page.IconFileId.Value.ToString() : "NULL",
                    page.Guid,
                    page.PageDisplayTitle.Bit(),
                    page.PageDisplayBreadCrumb.Bit(),
                    page.PageDisplayIcon.Bit(),
                    page.PageDisplayDescription.Bit(),
                    page.BreadCrumbDisplayName.Bit(),
                    page.BreadCrumbDisplayIcon.Bit()
                    ) );
        }

        /// <summary>
        /// Moves the page.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        public void MovePage( string pageGuid, string parentPageGuid )
        {
            Sql( string.Format( @"

                DECLARE @parentPageId int
                SET @parentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                UPDATE [Page] SET [ParentPageId]=@parentPageId WHERE [Guid] = '{1}'
                ", parentPageGuid, pageGuid ) );
        }

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeletePage( string guid )
        {
            Sql( string.Format( @"
                DELETE [Page] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Adds the page route.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="route">The route.</param>
        public void AddPageRoute( string pageGuid, string route )
        {
            Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                INSERT INTO [PageRoute] (
                    [IsSystem],[PageId],[Route],[Guid])
                VALUES(
                    1, @PageId, '{1}', newid())
", pageGuid, route ) );

        }

        /// <summary>
        /// Adds the page context.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="idParameter">The id parameter.</param>
        public void AddPageContext( string pageGuid, string entity, string idParameter )
        {
            Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                INSERT INTO [PageContext] (
                    [IsSystem],[PageId],[Entity],[IdParameter],[Guid])
                VALUES(
                    1, @PageId, '{1}', '{2}', newid())
", pageGuid, entity, idParameter ) );

        }
        
        /// <summary>
        /// Defaults the system page.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public Page DefaultSystemPage( string name, string description, Guid guid )
        {
            var page = new Page();

            page.Name = name;
            page.Title = name;
            page.IsSystem = true;
            page.SiteId = 1;
            page.Layout = "Default";
            page.EnableViewState = true; ;
            page.PageDisplayTitle = true;
            page.PageDisplayBreadCrumb = true;
            page.PageDisplayIcon = true;
            page.PageDisplayDescription = true;
            page.MenuDisplayDescription = true;
            page.MenuDisplayChildPages = true;
            page.DisplayInNavWhen = DisplayInNavWhen.WhenAllowed;
            page.BreadCrumbDisplayName = true;
            page.BreadCrumbDisplayIcon = false;
            page.Description = description;
            page.IncludeAdminFooter = true;
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
        /// <param name="layout">The layout.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The GUID.</param>
        public void AddBlock( string pageGuid, string blockTypeGuid, string name, string layout, string zone, int order, string guid )
        {
            var sb = new StringBuilder();
            string layoutParam;
            if ( string.IsNullOrWhiteSpace( layout ) )
            {
                layoutParam = "NULL";
            }
            else
            {
                layoutParam = string.Format( "'{0}'", layout );
            }

            sb.Append( @"
                DECLARE @PageId int
" );
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                SET @PageId = NULL
" );
            else
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );

            sb.AppendFormat( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
                INSERT INTO [Block] (
                    [IsSystem],[PageId],[Layout],[BlockTypeId],[Zone],
                    [Order],[Name],[OutputCacheDuration],
                    [Guid])
                VALUES(
                    1,@PageId,{5},@BlockTypeId,'{1}',
                    {2},'{3}',0,
                    '{4}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                    zone,
                    order,
                    name,
                    guid,
                    layoutParam );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
" );
            Sql( sb.ToString() );
        }

        /// <summary>
        /// Adds the block.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="blockTypeGuid">The block type GUID.</param>
        /// <param name="block">The block.</param>
        public void AddBlock( string pageGuid, string blockTypeGuid, Block block )
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
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
                INSERT INTO [Block] (
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
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
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
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
                DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
                DELETE [Block] WHERE [Guid] = '{0}'
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
        public Block DefaultSystemBlock( string name, Guid guid )
        {
            var block = new Block();

            block.IsSystem = true;
            block.Zone = "Content";
            block.Name = name;
            block.Guid = guid;

            return block;
        }

        #endregion

        #region Attribute Methods

        /// <summary>
        /// Adds the block attribute.
        /// </summary>
        /// <param name="blockTypeGuid">The block GUID.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }
            
            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                DECLARE @EntityTypeId int                
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] = @EntityTypeId
                AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId, @EntityTypeId,'BlockTypeId',CAST(@BlockTypeId as varchar),
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    blockTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid )
            );
        }

        /// <summary>
        /// Deletes the block attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockAttribute( string guid )
        {
            DeleteAttribute( guid );
        }

        /// <summary>
        /// Adds the entity attribute.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="category">The category.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddEntityAttribute( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }
            
            EnsureEntityTypeExists( entityTypeName );

            Sql( string.Format( @"
                 
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [EntityTypeId] = @EntityTypeId
                AND [Key] = '{2}'
                AND [EntityTypeQualifierColumn] = '{8}'
                AND [EntityTypeQualifierValue] = '{9}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@EntityTypeid,'{8}','{9}',
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    entityTypeName,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
        }

        /// <summary>
        /// Updates the type of the entity.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateEntityType(string name, string guid)
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}' )
                BEGIN
                    UPDATE [EntityType] SET [Guid] = '{1}' WHERE [Name] = '{0}'
                END
                ELSE
                BEGIN
                    INSERT INTO [EntityType] ([Name], [FriendlyName], [Guid])
                    VALUES ('{0}', null, '{1}')
                END
", 
                name, guid ) );
        }

        /// <summary>
        /// Ensures the entity type exists.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        private void EnsureEntityTypeExists( string entityTypeName )
        {
            Sql( string.Format( @"
                if not exists (
                select id from EntityType where name = '{0}')
                begin
                INSERT INTO [EntityType]
                           ([Name]
                           ,[FriendlyName]
                           ,[Guid])
                     VALUES
                           ('{0}'
                           ,null
                           ,newid()
                           )
                end"
                , entityTypeName )
            );
        }

        /// <summary>
        /// Adds the attribute value.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="value">The value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddAttributeValue( string attributeGuid, int entityId, string value, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                IF NOT EXISTS(Select * FROM [AttributeValue] WHERE [Guid] = '{3}')
                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,{1},0,'{2}','{3}')
",
                    attributeGuid,
                    entityId,
                    value,
                    guid )
            );
        }
        /// <summary>
        /// Deletes the attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteAttribute( string guid )
        {
            Sql( string.Format( @"
                DELETE [Attribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
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
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @BlockId

                INSERT INTO [AttributeValue] (
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
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
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
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedType];

                INSERT INTO [DefinedType] (
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

        /// <summary>
        /// Deletes the type of the defined.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedType( string guid )
        {
            Sql( string.Format( @"
                DELETE [DefinedType] WHERE [Guid] = '{0}'
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
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void AddDefinedValue( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Sql( string.Format( @"
                
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                INSERT INTO [DefinedValue] (
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

        /// <summary>
        /// Deletes the defined value.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedValue( string guid )
        {
            Sql( string.Format( @"
                DELETE [DefinedValue] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Security/Auth

        /// <summary>
        /// Adds the security role group.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddSecurityRoleGroup( string name, string description, string guid )
        {
            string sql = @"

DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = 'AECE949F-704C-483E-A4FB-93D5E4720C4C')

INSERT INTO [dbo].[Group]
           ([IsSystem]
           ,[ParentGroupId]
           ,[GroupTypeId]
           ,[CampusId]
           ,[Name]
           ,[Description]
           ,[IsSecurityRole]
           ,[IsActive]
           ,[Guid])
     VALUES
           (1
           ,null
           ,@groupTypeId
           ,null
           ,'{0}'
           ,'{1}'
           ,1
           ,1
           ,'{2}')
";
            Sql( string.Format( sql, name, description, guid ) );
        }

        /// <summary>
        /// Deletes the security role group.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityRoleGroup( string guid )
        {
            Sql( string.Format( "DELETE FROM [dbo].[Group] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Adds the security auth.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="action">The action.</param>
        /// <param name="groupGuid">The group GUID.</param>
        /// <param name="authGuid">The auth GUID.</param>
        public void AddSecurityAuth( string entityTypeName, string action, string groupGuid, string authGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{2}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{0}')

INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[PersonId]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,0
           ,0
           ,'{1}'
           ,'A'
           ,0
           ,null
           ,@groupId
           ,'{3}')
";
            @Sql( string.Format( sql, entityTypeName, action, groupGuid, authGuid ) );
        }

        /// <summary>
        /// Deletes the security auth.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityAuth( string guid )
        {
            Sql( string.Format( "DELETE FROM [dbo].[Auth] where [Guid] = '{0}'", guid ) );
        }

        #endregion

    }
}