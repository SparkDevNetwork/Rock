using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

using Rock;
using Rock.Model;


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

        public void AddBlockType( BlockTypeDto blockType )
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

        public void DeleteBlockType( string guid )
        {
            Sql( string.Format( @"
                DELETE [BlockType] WHERE [Guid] = '{0}'
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
                SET @ParentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [Page] (
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
                SET @ParentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;

                INSERT INTO [Page] (
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

        public void MovePage( string pageGuid, string parentPageGuid )
        {
            Sql( string.Format( @"

                DECLARE @parentPageId int
                SET @parentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                UPDATE [Page] SET [ParentPageId]=@parentPageId WHERE [Guid] = '{1}'
                ", parentPageGuid, pageGuid ) );
        }

        public void DeletePage( string guid )
        {
            Sql( string.Format( @"
                DELETE [Page] WHERE [Guid] = '{0}'
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
                DECLARE @EntityTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')

                DECLARE @BlockId int
                INSERT INTO [Block] (
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
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,NULL,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[PersonId],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,NULL,2,NEWID())
" );
            Sql( sb.ToString() );
        }

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
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                DECLARE @EntityTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')

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

        public void DeleteBlock( string guid )
        {
            Sql( string.Format( @"
                DECLARE @BlockId int
                DECLARE @EntityTypeId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'B2DE7D41-EA40-42A9-B212-9DD2ADE2DDAE')
                DELETE [Auth] WHERE [EntityTypeId] = @EntityTypeId AND [EntityId] = @BlockId
                DELETE [Block] WHERE [Guid] = '{0}'
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

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, string name, string category, string description, int order, string defaultValue, string guid )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [Entity] = 'Rock.Cms.Block'
                AND [EntityQualifierColumn] = 'BlockTypeId'
                AND [EntityQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
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
                    guid )
            );
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
                AND [EntityTypeQualifierColumn] = '{9}'
                AND [EntityTypeQualifierValue] = '{10}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Category],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,@EntityTypeid,'{9}','{10}',
                    '{2}','{3}','{4}','{5}',
                    {6},0,'{7}',0,0,
                    '{8}')
",
                    entityTypeName,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    category,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    entityTypeQualifierColumn,
                    entityTypeQualifierValue )
            );
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

        public void DeleteAttribute( string guid )
        {
            Sql( string.Format( @"
                DELETE [Attribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public void AddBlockAttribute( string blockGuid, string fieldTypeGuid, Rock.Model.AttributeDto attribute )
        {

            Sql( string.Format( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute] 
                WHERE [Entity] = 'Rock.Cms.Block'
                AND [EntityQualifierColumn] = 'BlockTypeId'
                AND [EntityQualifierValue] = CAST(@BlockTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
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
                    attribute.Description.Replace( "'", "''" ),
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
                DELETE [Attribute] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        public Rock.Model.AttributeDto DefaultBlockAttribute( string name, string category, string description, int order, string defaultValue, Guid guid )
        {
            var attribute = new Rock.Model.AttributeDto();

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
        public void AddSecurityRoleGroup(string name, string description, string guid)
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
            Sql(string.Format(sql, name, description, guid));
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
            Sql(string.Format("DELETE FROM [dbo].[Auth] where [Guid] = '{0}'", guid));
        }

        #endregion
    }
}