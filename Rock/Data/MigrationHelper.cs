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
using System;
using System.Text;
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Helper Class for updating Rock entities during migrations
    /// </summary>
    public class MigrationHelper
    {
        private IMigration Migration = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationHelper"/> class.
        /// </summary>
        /// <param name="migration">The migration.</param>
        public MigrationHelper( IMigration migration )
        {
            Migration = migration;
        }

        /// <summary>
        /// Deletes from the table by the by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="tableName">The table.</param>
        public void DeleteByGuid( string guid, string tableName )
        {
            Migration.Sql( string.Format( "DELETE [{0}] WHERE [Guid] = '{1}'", tableName, guid ) );
        }

        #region Entity Type Methods

        /// <summary>
        /// Updates the EntityType by name (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isEntity">if set to <c>true</c> [is entity].</param>
        /// <param name="isSecured">if set to <c>true</c> [is secured].</param>
        public void UpdateEntityType( string name, string guid, bool isEntity, bool isSecured )
        {
            Migration.Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [IsEntity] = {1},
                        [IsSecured] = {2},
                        [Guid] = '{3}'
                    WHERE [Name] = '{0}'
                END
                ELSE
                BEGIN
                    INSERT INTO [EntityType] ([Name], [IsEntity], [IsSecured], [IsCommon], [Guid])
                    VALUES ('{0}', {1}, {2}, 0, '{3}')
                END
",
                name,
                isEntity ? "1" : "0",
                isSecured ? "1" : "0",
                guid ) );
        }

        /// <summary>
        /// Updates the EntityType by name (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="isEntity">if set to <c>true</c> [is entity].</param>
        /// <param name="isSecured">if set to <c>true</c> [is secured].</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateEntityType( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [EntityType] (
                        [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[IsCommon],[Guid])
                    VALUES(
                        '{0}','{1}','{2}',{3},{4},0,'{5}')
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
        /// Deletes the EntityType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteEntityType( string guid )
        {
            DeleteByGuid( guid, "EntityType" );
        }

        /// <summary>
        /// Updates the EntityType SingleValueFieldType
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        public void UpdateEntityTypeSingleValueFieldType( string entityTypeName, string fieldTypeGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                UPDATE [EntityType] SET [SingleValueFieldTypeId] = @FieldTypeId WHERE [Id] = @EntityTypeId
                ", entityTypeName, fieldTypeGuid )
            );
        }

        /// <summary>
        /// Updates the EntityType MultiValueFieldType
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        public void UpdateEntityTypeMultiValueFieldType( string entityTypeName, string fieldTypeGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                UPDATE [EntityType] SET [MultiValueFieldTypeId] = @FieldTypeId WHERE [Id] = @EntityTypeId
                ", entityTypeName, fieldTypeGuid )
            );
        }

        #endregion

        #region Field Type Methods

        /// <summary>
        /// Updates the FieldType by assembly and className (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="IsSystem">if set to <c>true</c> [is system].</param>
        public void UpdateFieldType( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Migration.Sql( string.Format( @"

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
                    IsSystem ? "1" : "0" ) );
        }

        /// <summary>
        /// Deletes the FieldType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteFieldType( string guid )
        {
            DeleteByGuid( guid, "FieldType" );
        }

        #endregion

        #region Block Type Methods

        /// <summary>
        /// Updates the BlockType by path (if it exists);
        /// otherwise it inserts a new record. In either case it will be marked IsSystem.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="path">The path.</param>
        /// <param name="category">The category.</param>
        /// <param name="guid">The GUID.</param>
        public void UpdateBlockType( string name, string description, string path, string category, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [BlockType] WHERE [Path] = '{0}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [BlockType] (
                        [IsSystem],[Path],[Category],[Name],[Description],
                        [Guid])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}',
                        '{4}')
                END
                ELSE
                BEGIN
                    UPDATE [BlockType] SET
                        [IsSystem] = 1,
                        [Category] = '{1}',
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [Guid] = '{4}'
                    WHERE [Path] = '{0}'
                END
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Adds a new BlockType.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="path"></param>
        /// <param name="category"></param>
        /// <param name="guid"></param>
        public void AddBlockType( string name, string description, string path, string category, string guid )
        {
            Migration.Sql( string.Format( @"

                INSERT INTO [BlockType] (
                    [IsSystem],[Path],[Category],[Name],[Description],
                    [Guid])
                VALUES(
                    1,'{0}','{1}','{2}','{3}',
                    '{4}')
",
                    path,
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the BlockType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockType( string guid )
        {
            DeleteByGuid( guid, "BlockType" );
        }

        #endregion

        #region Site Methods

        /// <summary>
        /// Adds a new Layout to the given site.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="theme">The theme.</param>
        /// <param name="guid">The GUID.</param>
        public void AddSite( string name, string description, string theme, string guid )
        {
            Migration.Sql( string.Format( @"

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [Site]
                    WHERE [Guid] = '{3}' )

                BEGIN

                    INSERT INTO [Site] (
                        [IsSystem],[Name],[Description],[Theme],[Guid])
                    VALUES(1,'{0}','{1}','{2}','{3}')
                END
                ELSE
                BEGIN

                    UPDATE [Site] SET
                        [Name] = '{0}',
                        [Description] = '{1}',
                        [Theme] = '{2}'
                    WHERE [Guid] = '{3}'

                END
",
                    name,
                    description.Replace( "'", "''" ),
                    theme,
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the Layout.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSite( string guid )
        {
            DeleteByGuid( guid, "Site" );
        }

        #endregion

        #region Layout Methods

        /// <summary>
        /// Adds a new Layout to the given site.
        /// </summary>
        /// <param name="siteGuid">The site GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        public void AddLayout( string siteGuid, string fileName, string name, string description, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @SiteId int
                SET @SiteId = (SELECT [Id] FROM [Site] WHERE [Guid] = '{0}')

                INSERT INTO [Layout] (
                    [IsSystem],[SiteId],[FileName],[Name],[Description],[Guid])
                VALUES(
                    1,@SiteId,'{1}','{2}','{3}','{4}')
",
                    siteGuid,
                    fileName,
                    name,
                    description.Replace( "'", "''" ),
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the Layout.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteLayout( string guid )
        {
            DeleteByGuid( guid, "Layout" );
        }

        #endregion

        #region Page Methods

        /// <summary>
        /// Adds a new Page to the given parent page.
        /// The new page will be ordered as last child of the parent page.
        /// </summary>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        /// <param name="layoutGuid">The layout GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="insertAfterPageGuid">The insert after page unique identifier.</param>
        public void AddPage( string parentPageGuid, string layoutGuid, string name, string description, string guid, string iconCssClass = "", string insertAfterPageGuid = "" )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ParentPageId int = ( SELECT [Id] FROM [Page] WHERE [Guid] = {0} )
                DECLARE @LayoutId int = ( SELECT [Id] FROM [Layout] WHERE [Guid] = '{1}' )
                DECLARE @Order int = ( SELECT [order] + 1 FROM [Page] WHERE [Guid] = {6} )

                IF @Order IS NULL
                BEGIN
                    SELECT @Order = ISNULL(MAX([order])+1,0) FROM [Page] WHERE [ParentPageId] = @ParentPageId;
                END
                ELSE
                BEGIN
                    UPDATE [Page] SET [Order] = [Order] + 1 WHERE [ParentPageId] = @ParentPageId AND [Order] >= @Order
                END

                INSERT INTO [Page] (
                    [InternalName],[PageTitle],[BrowserTitle],[IsSystem],[ParentPageId],[LayoutId],
                    [RequiresEncryption],[EnableViewState],
                    [PageDisplayTitle],[PageDisplayBreadCrumb],[PageDisplayIcon],[PageDisplayDescription],
                    [MenuDisplayDescription],[MenuDisplayIcon],[MenuDisplayChildPages],[DisplayInNavWhen],
                    [BreadCrumbDisplayName],[BreadCrumbDisplayIcon],
                    [Order],[OutputCacheDuration],[Description],[IncludeAdminFooter],
                    [IconCssClass],[Guid])
                VALUES(
                    '{2}','{2}','{2}',1,@ParentPageId,@LayoutId,
                    0,1,
                    1,1,1,1,
                    0,0,1,0,
                    1,0,
                    @Order,0,'{3}',1,
                    '{5}','{4}')
",
                    string.IsNullOrWhiteSpace( parentPageGuid ) ? "NULL" : "'" + parentPageGuid + "'",
                    layoutGuid,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    iconCssClass,
                    string.IsNullOrWhiteSpace( insertAfterPageGuid ) ? "NULL" : "'" + insertAfterPageGuid + "'"
                    ) );
        }

        /// <summary>
        /// Moves the Page to the new given parent page.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="parentPageGuid">The parent page GUID.</param>
        public void MovePage( string pageGuid, string parentPageGuid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @parentPageId int
                SET @parentPageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                UPDATE [Page] SET [ParentPageId]=@parentPageId WHERE [Guid] = '{1}'
                ", parentPageGuid, pageGuid ) );
        }

        /// <summary>
        /// Deletes the Page and any PageViews that use the page.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeletePage( string guid )
        {
            Migration.Sql( string.Format( @"

                DELETE PV
                FROM [PageView] PV
                INNER JOIN [Page] P ON P.[Id] = PV.[PageId] AND P.[Guid] = '{0}'

                DELETE [Page] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Adds a new PageRoute to the given page but only if the given route name does not exist.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="route">The route.</param>
        public void AddPageRoute( string pageGuid, string route )
        {
            Migration.Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                IF NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = '{1}')
                    INSERT INTO [PageRoute] (
                        [IsSystem],[PageId],[Route],[Guid])
                    VALUES(
                        1, @PageId, '{1}', newid())
", pageGuid, route ) );
        }

        /// <summary>
        /// Adds a new PageContext to the given page.
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="idParameter">The id parameter.</param>
        [Obsolete( "Use UpdatePageContext" )]
        public void AddPageContext( string pageGuid, string entity, string idParameter )
        {
            Migration.Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                INSERT INTO [PageContext] (
                    [IsSystem],[PageId],[Entity],[IdParameter],[Guid])
                VALUES(
                    1, @PageId, '{1}', '{2}', newid())
", pageGuid, entity, idParameter ) );
        }

        /// <summary>
        /// Adds or Updates PageContext to the given page, entity, idParameter
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="entity">The entity value.</param>
        /// <param name="idParameter">The idparameter value.</param>
        /// <param name="guid">The unique identifier for the PageContext record.</param>
        public void UpdatePageContext( string pageGuid, string entity, string idParameter, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @PageId int
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

                DECLARE @PageContextId int
                SET @PageContextId = (SELECT TOP 1 [Id] FROM [PageContext] WHERE [PageId] = @PageId and [Entity] = '{1}' and [IdParameter] = '{2}')
                IF @PageContextId IS NULL
                BEGIN
                    INSERT INTO [PageContext] (
                        [IsSystem],[PageId],[Entity],[IdParameter],[Guid])
                    VALUES(
                        1, @PageId, '{1}', '{2}', '{3}')
                END
                ELSE
                BEGIN
                    UPDATE [PageContext] set [Guid] = '{3}' where [Id] = @PageContextId
                END

", pageGuid, entity, idParameter, guid ) );
        }

        /// <summary>
        /// Deletes the page context.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeletePageContext( string guid )
        {
            Migration.Sql( string.Format( @"DELETE FROM [PageContext] WHERE [Guid] = '{0}'", guid ) );
        }

        #endregion

        #region Block Methods

        /// <summary>
        /// Adds a new Block of the given block type to the given page (optional) and layout (optional),
        /// setting its values with the given parameter values. If only the layout is given,
        /// edit/configuration authorization will also be inserted into the Auth table
        /// for the admin role (GroupId 2).
        /// </summary>
        /// <param name="pageGuid">The page GUID.</param>
        /// <param name="layoutGuid">The layout GUID.</param>
        /// <param name="blockTypeGuid">The block type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="preHtml">The pre HTML.</param>
        /// <param name="postHtml">The post HTML.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddBlock( string pageGuid, string layoutGuid, string blockTypeGuid, string name, string zone, string preHtml, string postHtml, int order, string guid )
        {
            var sb = new StringBuilder();
            sb.Append( @"
                DECLARE @PageId int
                SET @PageId = null

                DECLARE @LayoutId int
                SET @LayoutId = null
" );

            if ( !string.IsNullOrWhiteSpace( pageGuid ) )
            {
                sb.AppendFormat( @"
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')
", pageGuid );
            }

            if ( !string.IsNullOrWhiteSpace( layoutGuid ) )
            {
                sb.AppendFormat( @"
                SET @LayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '{0}')
", layoutGuid );
            }

            sb.AppendFormat( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                DECLARE @BlockId int
                INSERT INTO [Block] (
                    [IsSystem],[PageId],[LayoutId],[BlockTypeId],[Zone],
                    [Order],[Name],[PreHtml],[PostHtml],[OutputCacheDuration],
                    [Guid])
                VALUES(
                    1,@PageId,@LayoutId,@BlockTypeId,'{1}',
                    {2},'{3}','{4}','{5}',0,
                    '{6}')
                SET @BlockId = SCOPE_IDENTITY()
",
                    blockTypeGuid,
                    zone,
                    order,
                    name.Replace( "'", "''" ),
                    preHtml.Replace( "'", "''" ),
                    postHtml.Replace( "'", "''" ),
                    guid );

            // If adding a layout block, give edit/configuration authorization to admin role
            if ( string.IsNullOrWhiteSpace( pageGuid ) )
                sb.Append( @"
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Edit','A',0,2,NEWID())
                INSERT INTO [Auth] ([EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
                    VALUES(@EntityTypeId,@BlockId,0,'Configure','A',0,2,NEWID())
" );
            Migration.Sql( sb.ToString() );
        }

        /// <summary>
        /// Add or Updates the HTML content for an HTML Content Block
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateHtmlContentBlock( string blockGuid, string htmlContent, string guid )
        {
            string sqlFormat = @"
    DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '{0}')
    IF @BlockId IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT [Id]
            FROM [HtmlContent]
            WHERE [Guid] = '{2}')
        BEGIN
            UPDATE [HtmlContent] SET [Content] = '{1}' WHERE [Guid] = '{2}'
        END
        ELSE
        BEGIN
            INSERT INTO [HtmlContent] ([BlockId], [Version], [Content], [IsApproved], [Guid])
                VALUES (@BlockId, 1, '{1}', 1, '{2}')
        END
    END";

            Migration.Sql( string.Format( sqlFormat, blockGuid, htmlContent.Replace( "'", "''" ), guid ) );
        }

        /// <summary>
        /// Deletes the block and any authorization records that belonged to it.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlock( string guid )
        {
            Migration.Sql( string.Format( @"
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

        #endregion

        #region Category Methods

        /// <summary>
        /// Updates the category.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="order">The order.</param>
        public void UpdateCategory( string entityTypeGuid, string name, string iconCssClass, string description, string guid, int order = 0 )
        {
            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Category]
                    WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [Category] SET
                        [EntityTypeId] = @EntityTypeId,
                        [Name] = '{1}',
                        [IconCssClass] = '{2}',
                        [Description] = '{3}',
                        [Order] = {5}
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[IconCssClass],[Description],[Order],[Guid] )
                    VALUES( 1,@EntityTypeId,'{1}','{2}','{3}',{5},'{4}' )
                END
",
                    entityTypeGuid,
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    guid,
                    order )
            );
        }

        /// <summary>
        /// Updates the name of the category by.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="order">The order.</param>
        public void UpdateCategoryByName( string entityTypeGuid, string name, string iconCssClass, string description, string guid, int order = 0 )
        {
            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Category]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [Name] = '{1}' )
                BEGIN
                    UPDATE [Category] SET
                        [IsSystem] = 1,
                        [IconCssClass] = '{2}',
                        [Description] = '{3}',
                        [Order] = {5},
                        [Guid] = '{4}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [Name] = '{1}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[IconCssClass],[Description],[Order],[Guid] )
                    VALUES( 1,@EntityTypeId,'{1}','{2}','{3}',{5},'{4}' )
                END
",
                    entityTypeGuid,
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    guid,
                    order )
            );
        }

        /// <summary>
        /// Deletes the category.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteCategory( string guid )
        {
            DeleteByGuid( guid, "Category" );
        }

        #endregion

        #region Attribute Methods

        /// <summary>
        /// Updates the BlockType Attribute for the given blocktype and key (if it exists);
        /// otherwise it inserts a new record.
        /// </summary>
        /// <param name="blockTypeGuid"></param>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="category"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue"></param>
        /// <param name="guid"></param>
        public void UpdateBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            Migration.Sql( string.Format( @"

                DECLARE @BlockTypeId int
                SET @BlockTypeId = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [IsSystem] = 1,
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
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
                END
",
                    blockTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Adds a new BlockType Attribute for the given blocktype and key.
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
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <exception cref="System.Exception">Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!</exception>
        public void AddBlockTypeAttribute( string blockTypeGuid, string fieldTypeGuid, string name, string key, string category, string description, int order, string defaultValue, string guid, bool isRequired = false )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            Migration.Sql( string.Format( @"

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
                    {5},0,'{6}',0,{8},
                    '{7}')
",
                    blockTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid,
                    isRequired ? "1" : "0" )
            );
        }

        /// <summary>
        /// Deletes the block Attribute.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteBlockAttribute( string guid )
        {
            DeleteAttribute( guid );
        }

        /// <summary>
        /// Adds a new EntityType Attribute for the given EntityType, FieldType, and name (key).
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
        /// <param name="key">The key.  Defaults to Name without Spaces. If this is a core global attribute, specify the key with a 'core.' prefix</param>
        public void AddEntityAttribute( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string category, string description, int order, string defaultValue, string guid, string key = null )
        {
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                throw new Exception( "Attribute Category no longer supported by this helper function. You'll have to write special migration code yourself. Sorry!" );
            }

            if ( string.IsNullOrWhiteSpace( key ) )
            {
                key = name.Replace( " ", string.Empty );
            }

            EnsureEntityTypeExists( entityTypeName );

            Migration.Sql( string.Format( @"

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
                    key,
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
        /// Adds or updates a group member Attribute for the given group for storing a particular defined value.
        /// The defined values are constrained by the given defined type.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="name">The name the group member attribute. The attribute key will become the name with the whitespace removed.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="isGridColumn">if set to <c>true</c> the group member attribute will appear in the group member list grid.</param>
        /// <param name="isMultiValue">if set to <c>true</c> the attribute will allow multiple defined values to be set.</param>
        /// <param name="isRequired">if set to <c>true</c> the attribute will be required to be set.</param>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="guid">The unique identifier of the attribute.</param>
        /// <param name="isSystem">if set to <c>true</c> the attribute is considered a system attribute..</param>
        public void AddGroupMemberAttributeDefinedValue( string groupGuid, string name, string description, int order, string defaultValue, bool isGridColumn, bool isMultiValue, bool isRequired, string definedTypeGuid, string guid, bool isSystem = true )
        {
            UpdateGroupMemberAttributeDefinedValue( groupGuid, name, description, order, defaultValue, isGridColumn, isMultiValue, isRequired, definedTypeGuid, guid, isSystem );
        }

        /// <summary>
        /// Adds or updates a group member Attribute for the given group for storing a particular defined value.
        /// The defined values are constrained by the given defined type.
        /// </summary>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="name">The name the group member attribute. The attribute key will become the name with the whitespace removed.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="isGridColumn">if set to <c>true</c> the group member attribute will appear in the group member list grid.</param>
        /// <param name="isMultiValue">if set to <c>true</c> the attribute will allow multiple defined values to be set.</param>
        /// <param name="isRequired">if set to <c>true</c> the attribute will be required to be set.</param>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="guid">The unique identifier of the attribute.</param>
        /// <param name="isSystem">if set to <c>true</c> the attribute is considered a system attribute..</param>
        public void UpdateGroupMemberAttributeDefinedValue( string groupGuid, string name, string description, int order, string defaultValue, bool isGridColumn, bool isMultiValue, bool isRequired, string definedTypeGuid, string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"
                -- Add group member attribute for a group that holds a particular defined value (constrained by a defined type).

                DECLARE @GroupId int = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')
                DECLARE @GroupMemberEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{9}')
                DECLARE @DefinedValueFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupId)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [IsGridColumn] = '{15}',
                        [DefaultValue] = '{6}',
                        [IsMultiValue] = '{10}',
                        [IsRequired] = '{11}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @GroupMemberEntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = CONVERT(NVARCHAR, @GroupId)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid],[CreatedDateTime])
                    VALUES(
                        {12},@DefinedValueFieldTypeId,@GroupMemberEntityTypeId,'{8}',CONVERT(NVARCHAR, @GroupId),
                        '{2}','{3}','{4}',
                        {5},{15},'{6}',{10},{11},
                        '{7}', GETDATE() )
                END

                -- Add/Update the 'allowmultiple' and 'definedtype' attribute qualifiers

                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{7}')
                DECLARE @DefinedTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{13}')

                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple' )
                BEGIN
                    INSERT INTO [AttributeQualifier] (
                        [IsSystem],[AttributeId],[Key],[Value],[Guid])
                    VALUES(
                       {12},@AttributeId,'allowmultiple','{14}',NEWID() )
                END
                ELSE
                BEGIN
                    UPDATE [AttributeQualifier] SET
                        [Key] = 'allowmultiple',
                        [Value] = '{14}'
                    WHERE [AttributeId] = @AttributeId AND [Key] = 'allowmultiple'
                END

                IF NOT EXISTS( SELECT 1 FROM [AttributeQualifier] WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype' )
                BEGIN
                    INSERT INTO [AttributeQualifier] (
                        [IsSystem],[AttributeId],[Key],[Value],[Guid])
                    VALUES(
                       {12},@AttributeId,'definedtype',CONVERT(NVARCHAR, @DefinedTypeId),NEWID() )
                END
                ELSE
                BEGIN
                    UPDATE [AttributeQualifier] SET
                        [Key] = 'definedtype',
                        [Value] = CONVERT(NVARCHAR, @DefinedTypeId)
                    WHERE [AttributeId] = @AttributeId AND [Key] = 'definedtype'
                END
",
                    groupGuid,
                    Rock.SystemGuid.FieldType.DEFINED_VALUE,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue,
                    guid,
                    "GroupId",
                    Rock.SystemGuid.EntityType.GROUP_MEMBER,
                    ( isMultiValue ? "1" : "0" ),
                    ( isRequired ? "1" : "0" ),
                    ( isSystem ? "1" : "0" ),
                    definedTypeGuid,
                    ( isMultiValue ? "True" : "False" ),
                    ( isGridColumn ? "1" : "0" )
                    )
            );
        }

        /// <summary>
        /// Updates the Entity Attribute for the given EntityType, FieldType, and name (key).
        /// otherwise it inserts a new record.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="key">The key.  Defaults to Name without Spaces. If this is a core attribute for the entity, specify the key with a 'core.' prefix</param>
        public void UpdateEntityAttribute( string entityTypeName, string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description, int order, string defaultValue, string guid, string key = null )
        {
            EnsureEntityTypeExists( entityTypeName );

            if ( string.IsNullOrWhiteSpace( key ) )
            {
                key = name.Replace( " ", string.Empty );
            }

            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = '{9}'
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = '{8}'
                    AND [EntityTypeQualifierValue] = '{9}'
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
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
                END
",
                    entityTypeName,
                    fieldTypeGuid,
                    key,
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
        /// Adds a global Attribute for the given FieldType, entityTypeQualifierColumn, entityTypeQualifierValue and name (key).
        /// Note: This method delets the Attribute first if it had already existed.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type GUID.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="key">The key.  Defaults to Name without Spaces. If this is a core global attribute, specify the key with a 'core.' prefix</param>
        public void AddGlobalAttribute( string fieldTypeGuid, string entityTypeQualifierColumn, string entityTypeQualifierValue, string name, string description, int order, string defaultValue, string guid, string key = null )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                key = name.Replace( " ", string.Empty );
            }

            Migration.Sql( string.Format( @"

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute]
                WHERE [EntityTypeId] IS NULL
                AND [Key] = '{2}'
                AND [EntityTypeQualifierColumn] = '{8}'
                AND [EntityTypeQualifierValue] = '{9}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId,NULL,'{8}','{9}',
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    "", // no entity; keeps {#} the same as AddEntityAttribute()
                    fieldTypeGuid,
                    key,
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
        /// Ensures the entity type exists by adding it by name if it did not already exist.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        private void EnsureEntityTypeExists( string entityTypeName )
        {
            // NOTE: If it doesn't exist, add it assuming that IsEntity=True and IsSecured=True.  The framework will correct it if those assumptions are incorrect
            Migration.Sql( string.Format( @"
                if not exists (
                select id from EntityType where name = '{0}')
                begin
                INSERT INTO [EntityType]
                           ([Name]
                           ,[FriendlyName]
                           ,[IsEntity]
                           ,[IsSecured]
                           ,[IsCommon]
                           ,[Guid])
                     VALUES
                           ('{0}'
                           ,null
                           ,{1}
                           ,{2}
                           ,0
                           ,newid()
                           )
                end"
                , entityTypeName, 1, 1 )
            );
        }

        /// <summary>
        /// Adds a new attribute value for the given attributeGuid if it does not already exist.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="value">The value.</param>
        /// <param name="guid">The GUID.</param>
        public void AddAttributeValue( string attributeGuid, int entityId, string value, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                IF NOT EXISTS(Select * FROM [AttributeValue] WHERE [Guid] = '{3}')
                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,{1},'{2}','{3}')
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
            DeleteByGuid( guid, "Attribute" );
        }

        /// <summary>
        /// Adds the attribute qualifier.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddAttributeQualifier( string attributeGuid, string key, string value, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                IF NOT EXISTS(Select * FROM [AttributeQualifier] WHERE [Guid] = '{3}')
                BEGIN
                    INSERT INTO [AttributeQualifier] (
                        [IsSystem],[AttributeId],[Key],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,'{1}','{2}','{3}')
                END
                ELSE
                BEGIN
                    UPDATE [AttributeQualifier] SET
                        [Key] = '{1}',
                        [Value] = '{2}'
                    WHERE [Guid] = '{3}'
                END
",
                    attributeGuid, // {0}
                    key, // {1}
                    value, // {2}
                    guid ) // {3}
            );
        }

        /// <summary>
        /// Deletes the attributes by entity type.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        public void DeleteAttributesByEntityType( string entityTypeGuid )
        {
            Migration.Sql( string.Format( @"
                SELECT a.*
                FROM Attribute a
                WHERE a.EntityTypeId IN (
	                SELECT w.Id
	                FROM EntityType w
	                WHERE w.Guid = '{0}'
                )", entityTypeGuid ) );
        }

        /// <summary>
        /// Deletes the attribute values by workflow action guid.
        /// </summary>
        /// <param name="workflowActionGuid">The workflow action unique identifier.</param>
        public void DeleteAttributeValuesByWorkflowAction( string workflowActionGuid )
        {
            Migration.Sql( string.Format( @"
                DELETE
                FROM AttributeValue v
                WHERE v.EntityId IN (
	                SELECT t.Id
	                FROM WorkflowActionType t
	                WHERE t.Guid = '{0}'
                )", workflowActionGuid ) );
        }

        #endregion

        #region Block Attribute Value Methods

        /// <summary>
        /// Adds a new block attribute value for the given block guid and attribute guid,
        /// deleting any previously existing attribute value first.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="value">The value.</param>
        /// <param name="appendToExisting">if set to <c>true</c> appends the value to the existing value instead of replacing.</param>
        public void AddBlockAttributeValue( string blockGuid, string attributeGuid, string value, bool appendToExisting = false )
        {
            Migration.Sql( string.Format( @"

                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                DECLARE @TheValue NVARCHAR(MAX) = '{2}'

                -- If appendToExisting (and any current value exists), get the current value before we delete it...
                IF 1 = {3} AND EXISTS (SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                BEGIN
                    SET @TheValue = (SELECT [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
                    -- If the new value is not in the old value, append it.
                    IF CHARINDEX( '{2}', @TheValue ) = 0
                    BEGIN
                        SET @TheValue = (SELECT @TheValue + ',' + '{2}' )
                    END
                END

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @BlockId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@BlockId,
                    @TheValue,
                    NEWID())
",
                    blockGuid,
                    attributeGuid,
                    value.Replace( "'", "''" ),
                    ( appendToExisting ? "1" : "0" )
                )
            );
        }

        /// <summary>
        /// Deletes the block attribute value.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        public void DeleteBlockAttributeValue( string blockGuid, string attributeGuid )
        {
            Migration.Sql( string.Format( @"

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
        /// Adds a new DefinedType.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="helpText">The help text.</param>
        public void AddDefinedType( string category, string name, string description, string guid, string helpText = null )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeEntityTypeId int = (
                    SELECT TOP 1 [Id]
                    FROM [EntityType]
                    WHERE [Name] = 'Rock.Model.DefinedType' )

                DECLARE @CategoryId int = (
                    SELECT TOP 1 [Id] FROM [Category]
                    WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
                    AND [Name] = '{0}' )

                IF @CategoryId IS NULL AND @DefinedTypeEntityTypeId IS NOT NULL
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[Order],[Guid] )
                    VALUES( 0, @DefinedTypeEntityTypeId,'{0}', 0, NEWID() )
                    SET @CategoryId = SCOPE_IDENTITY()
                END

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedType];

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [DefinedType]
                    WHERE [Guid] = '{3}' )

                BEGIN

                    INSERT INTO [DefinedType] (
                        [IsSystem],[FieldTypeId],[Order],
                        [CategoryId],[Name],[Description],[HelpText],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId,@Order,
                        @CategoryId,'{1}','{2}','{4}',
                        '{3}')
                END
                ELSE
                BEGIN

                    UPDATE [DefinedType] SET
                        [IsSystem] = 1,
                        [FieldTypeId] = @FieldTypeId,
                        [CategoryId] = @CategoryId,
                        [Name] = '{1}',
                        [Description] = '{2}',
                        [HelpText] = '{4}'
                    WHERE [Guid] = '{3}'

                END
",
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    helpText ?? string.Empty
                    ) );
        }

        /// <summary>
        /// Deletes the DefinedType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedType( string guid )
        {
            DeleteByGuid( guid, "DefinedType" );
        }

        /// <summary>
        /// Adds the defined type attribute.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddDefinedTypeAttribute( string definedTypeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute]
                WHERE [EntityTypeId] = @EntityTypeId
                AND [EntityTypeQualifierColumn] = 'DefinedTypeId'
                AND [EntityTypeQualifierValue] = CAST(@DefinedTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId, @EntityTypeId,'DefinedTypeId',CAST(@DefinedTypeId as varchar),
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    definedTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        #endregion

        #region DefinedValue Methods

        /// <summary>
        /// Adds a new DefinedValue for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void AddDefinedValue( string definedTypeGuid, string value, string description, string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                INSERT INTO [DefinedValue] (
                    [IsSystem],[DefinedTypeId],[Order],
                    [Value],[Description],
                    [Guid])
                VALUES(
                    {4},@DefinedTypeId,@Order,
                    '{1}','{2}',
                    '{3}')
",
                    definedTypeGuid,
                    value,
                    description.Replace( "'", "''" ),
                    guid,
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Updates (or Adds) the defined value for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateDefinedValue( string definedTypeGuid, string value, string description, string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [DefinedValue]
                    SET
                        [IsSystem] = {4}
                        ,[DefinedTypeId] = @DefinedTypeId
                        ,[Value] = '{1}'
                        ,[Description] = '{2}'
                    WHERE
                        [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    DECLARE @Order int
                    SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                    INSERT INTO [DefinedValue]
                        ([IsSystem]
                        ,[DefinedTypeId]
                        ,[Order]
                        ,[Value]
                        ,[Description]
                        ,[Guid])
                    VALUES
                        ({4}
                        ,@DefinedTypeId
                        ,@Order
                        ,'{1}'
                        ,'{2}'
                        ,'{3}')
                END
",
                    definedTypeGuid,
                    value.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    guid,
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Updates the name of the defined value by.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateDefinedValueByValue( string definedTypeGuid, string value, string description, int order, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId AND [Name] = '{1}' )
                BEGIN
                    UPDATE [DefinedValue]
                    SET
                         [IsSystem] = {4}
                        ,[Description] = '{2}'
                        ,[Order] = {3}
                    WHERE [DefinedTypeId] = @DefinedTypeId
                    AND [Value] = '{1}'
                END
                ELSE
                BEGIN
                    INSERT INTO [DefinedValue]
                        ([IsSystem]
                        ,[DefinedTypeId]
                        ,[Value]
                        ,[Description]
                        ,[Order]
                        ,[Guid])
                    VALUES
                        ({4}
                        ,@DefinedTypeId
                        ,'{1}'
                        ,'{2}'
                        ,{3}
                        ,NEWID())
                END
",
                    definedTypeGuid,
                    value.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    order,
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Deletes the DefinedValue.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteDefinedValue( string guid )
        {
            DeleteByGuid( guid, "DefinedValue" );
        }

        /// <summary>
        /// Adds the defined value attribute value.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void UpdateDefinedValueAttributeValue( string definedValueGuid, string attributeGuid, string value )
        {
            this.AddDefinedValueAttributeValue( definedValueGuid, attributeGuid, value );
        }

        /// <summary>
        /// Adds the defined value attribute value.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void AddDefinedValueAttributeValue( string definedValueGuid, string attributeGuid, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedValueId int
                SET @DefinedValueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @DefinedValueId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@DefinedValueId,
                    '{2}',
                    NEWID())
",
                    definedValueGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        /// <summary>
        /// Adds the name of the defined value attribute value by.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="definedValueValue">The defined value value.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        public void AddDefinedValueAttributeValueByValue( string definedTypeGuid, string definedValueValue, string attributeKey, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @DefinedValueId int
                SET @DefinedValueId = (SELECT [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId AND [Value] = '{1}' )

                DECLARE @AttributeId int
                SET @AttributeId = (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeQualifierColumn] = 'DefinedTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@DefinedTypeId as varchar)
                    AND [Key] = '{2}'
                )

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @DefinedValueId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@DefinedValueId,
                    '{3}',
                    NEWID())
",
                    definedTypeGuid,
                    definedValueValue,
                    attributeKey,
                    value.Replace( "'", "''" )
                )
            );
        }

        #endregion

        #region BinaryFile Methods

        /// <summary>
        /// Updates the type of the binary file.
        /// </summary>
        /// <param name="storageEntityTypeId">The storage entity type identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="allowCaching">if set to <c>true</c> [allow caching].</param>
        /// <param name="requiresViewSecurity">if set to <c>true</c> [requires view security].</param>
        public void UpdateBinaryFileType( string storageEntityTypeId, string name, string description, string iconCssClass, string guid, bool allowCaching = false, bool requiresViewSecurity = false )
        {
            Migration.Sql( string.Format( @"

                DECLARE @StorageEntityTypeId int
                SET @StorageEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')

                IF EXISTS (
                    SELECT [Id]
                    FROM [BinaryFileType]
                    WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [BinaryFileType] SET
                        [Name] = '{1}',
                        [Description] = '{2}',
                        [IconCssClass] = '{3}',
                        [StorageEntityTypeId] = @StorageEntityTypeId,
                        [AllowCaching] = {5},
                        [RequiresViewSecurity] = {6}
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    INSERT INTO [BinaryFileType] ( [IsSystem],[Name],[Description],[IconCssClass],[StorageEntityTypeId],[AllowCaching],[RequiresViewSecurity],[Guid] )
                    VALUES( 1,'{1}','{2}','{3}',@StorageEntityTypeId,{5},{6},'{4}' )
                END
",
                    storageEntityTypeId,
                    name,
                    description.Replace( "'", "''" ),
                    iconCssClass,
                    guid,
                    ( allowCaching ? "1" : "0" ),
                    ( requiresViewSecurity ? "1" : "0" )
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
           ,[Guid]
           ,[Order])
     VALUES
           (1
           ,null
           ,@groupTypeId
           ,null
           ,'{0}'
           ,'{1}'
           ,1
           ,1
           ,'{2}'
           ,0)
";
            Migration.Sql( string.Format( sql,
                name.Replace( "'", "''" ),
                description.Replace( "'", "''" ),
                guid ) );
        }

        /// <summary>
        /// Deletes the security role group.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityRoleGroup( string guid )
        {
            Migration.Sql( string.Format( "DELETE FROM [dbo].[Group] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Adds the security auth record for the given entity type and group.
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
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,0
           ,0
           ,'{1}'
           ,'A'
           ,0
           ,@groupId
           ,'{3}')
";
            Migration.Sql( string.Format( sql, entityTypeName, action, groupGuid, authGuid ) );
        }

        /// <summary>
        /// Adds the security auth record for the given entity type and group.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForEntityType( string entityTypeName, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
        {
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = 0
    AND [Action] = '{3}'
    AND [SpecialRole] = {5}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,0
               ,{2}
               ,'{3}'
               ,'{4}'
               ,{5}
               ,@groupId
               ,'{6}')
END
";
            Migration.Sql( string.Format( sql,
                groupGuid ?? Guid.Empty.ToString(), // {0}
                entityTypeName, // {1}
                order, // {2}
                action, // {3}
                ( allow ? "A" : "D" ), // {4}
                specialRole, // {5}
                authGuid // {6}
                ) );
        }

        /// <summary>
        /// Deletes the security authentication for page.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        public void DeleteSecurityAuthForPage( string pageGuid )
        {
            string sql = @"
DECLARE @pageId int
SET @pageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

DELETE [dbo].[Auth]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @pageId
";
            Migration.Sql( string.Format( sql, pageGuid ) );
        }

        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForPage( string pageGuid, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Page";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @pageId int
SET @pageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @pageId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,@pageId
               ,{6}
               ,'{3}'
               ,'{7}'
               ,{4}
               ,@groupId
               ,'{5}')
END

";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, pageGuid, action, specialRole, authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security authentication for block.
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        public void DeleteSecurityAuthForBlock( string blockGuid )
        {
            string sql = @"
DECLARE @blockId int
SET @blockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Block')

DELETE [dbo].[Auth]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @blockId
";
            Migration.Sql( string.Format( sql, blockGuid ) );
        }

        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForBlock( string blockGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Block";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @blockId int
SET @blockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @blockId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
           ([EntityTypeId]
           ,[EntityId]
           ,[Order]
           ,[Action]
           ,[AllowOrDeny]
           ,[SpecialRole]
           ,[GroupId]
           ,[Guid])
     VALUES
           (@entityTypeId
           ,@blockId
           ,{6}
           ,'{3}'
           ,'{7}'
           ,{4}
           ,@groupId
           ,'{5}')
END
";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, blockGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Adds the binaryfiletype security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="binaryFileTypeGuid">The binary file type unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForBinaryFileType( string binaryFileTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.BinaryFileType";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @binaryFileTypeId int
SET @binaryFileTypeId = (SELECT [Id] FROM [BinaryFileType] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @binaryFileTypeId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,@binaryFileTypeId
               ,{6}
               ,'{3}'
               ,'{7}'
               ,{4}
               ,@groupId
               ,'{5}')
END
";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, binaryFileTypeGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security authentication for groupType.
        /// </summary>
        /// <param name="groupTypeGuid">The groupType unique identifier.</param>
        public void DeleteSecurityAuthForGroupType( string groupTypeGuid )
        {
            string sql = @"
DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.GroupType')

DELETE [dbo].[Auth]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @groupTypeId
";
            Migration.Sql( string.Format( sql, groupTypeGuid ) );
        }

        /// <summary>
        /// Adds the page security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForGroupType( string groupTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.GroupType";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @groupTypeId int
SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @groupTypeId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,@groupTypeId
               ,{6}
               ,'{3}'
               ,'{7}'
               ,{4}
               ,@groupId
               ,'{5}')
END
";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, groupTypeGuid, action, specialRole.ConvertToInt(), authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security authentication for page.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        public void DeleteSecurityAuthForAttribute( string attributeGuid )
        {
            string sql = @"
DECLARE @attributeId int
SET @attributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

DELETE [dbo].[Auth]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @attributeId
";
            Migration.Sql( string.Format( sql, attributeGuid ) );
        }

        /// <summary>
        /// Adds the attribute security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForAttribute( string attributeGuid, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Attribute";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @attributeId int
SET @attributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @attributeId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,@attributeId
               ,{6}
               ,'{3}'
               ,'{7}'
               ,{4}
               ,@groupId
               ,'{5}')
END
";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, attributeGuid, action, specialRole, authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security authentication for category.
        /// </summary>
        /// <param name="categoryGuid">The category unique identifier.</param>
        public void DeleteSecurityAuthForCategory( string categoryGuid )
        {
            string sql = @"
DECLARE @categoryId int
SET @categoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

DELETE [dbo].[Auth]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityId] = @categoryId
";
            Migration.Sql( string.Format( sql, categoryGuid ) );
        }

        /// <summary>
        /// Adds the category security authentication. Set GroupGuid to null when setting to a special role
        /// </summary>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForCategory( string categoryGuid, int order, string action, bool allow, string groupGuid, int specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.Category";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
DECLARE @groupId int
SET @groupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '{0}')

DECLARE @entityTypeId int
SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [name] = '{1}')

DECLARE @categoryId int
SET @categoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{2}')

IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[Auth]
    WHERE [EntityTypeId] = @entityTypeId
    AND [EntityId] = @categoryId
    AND [Action] = '{3}'
    AND [SpecialRole] = {4}
    AND [GroupId] = @groupId
)
BEGIN
    INSERT INTO [dbo].[Auth]
               ([EntityTypeId]
               ,[EntityId]
               ,[Order]
               ,[Action]
               ,[AllowOrDeny]
               ,[SpecialRole]
               ,[GroupId]
               ,[Guid])
         VALUES
               (@entityTypeId
               ,@categoryId
               ,{6}
               ,'{3}'
               ,'{7}'
               ,{4}
               ,@groupId
               ,'{5}')
END
";
            Migration.Sql( string.Format( sql, groupGuid ?? Guid.Empty.ToString(), entityTypeName, categoryGuid, action, specialRole, authGuid, order,
                ( allow ? "A" : "D" ) ) );
        }

        /// <summary>
        /// Deletes the security auth record.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSecurityAuth( string guid )
        {
            Migration.Sql( string.Format( "DELETE FROM [dbo].[Auth] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Adds the security authentication for rest controller.
        /// </summary>
        /// <param name="restControllerClass">The rest controller class.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForRestController( string restControllerClass, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.RestController";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [name] = '{0}')
    DECLARE @ControllerId int = ( SELECT TOP 1 [Id] FROM [RestController] WHERE [ClassName] = '{1}')

    IF @EntityTypeId IS NOT NULL AND @ControllerId IS NOT NULL
    BEGIN

        DECLARE @groupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '{5}')

        IF NOT EXISTS (
            SELECT [Id] FROM [dbo].[Auth]
            WHERE [EntityTypeId] = @entityTypeId
            AND [EntityId] = @ControllerId
            AND [Action] = '{3}'
            AND [SpecialRole] = {6}
            AND [GroupId] = @groupId
        )
        BEGIN
            INSERT INTO [dbo].[Auth]
                   ([EntityTypeId]
                   ,[EntityId]
                   ,[Order]
                   ,[Action]
                   ,[AllowOrDeny]
                   ,[SpecialRole]
                   ,[GroupId]
                   ,[Guid])
             VALUES
                   (@EntityTypeId
                   ,@ControllerId
                   ,{2}
                   ,'{3}'
                   ,'{4}'
                   ,{6}
                   ,@groupId
                   ,'{7}')
        END
    END
";
            Migration.Sql( string.Format( sql,
                entityTypeName,                 // 0
                restControllerClass,            // 1
                order,                          // 2
                action,                         // 3
                ( allow ? "A" : "D" ),          // 4
                groupGuid,                      // 5
                specialRole.ConvertToInt(),     // 6
                authGuid ) );                   // 7
        }

        /// <summary>
        /// Adds the security authentication for rest action.
        /// </summary>
        /// <param name="restActionMethod">The rest action method.</param>
        /// <param name="restActionPath">The rest action path.</param>
        /// <param name="order">The order.</param>
        /// <param name="action">The action.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="specialRole">The special role.</param>
        /// <param name="authGuid">The authentication unique identifier.</param>
        public void AddSecurityAuthForRestAction( string restActionMethod, string restActionPath, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            string entityTypeName = "Rock.Model.RestAction";
            EnsureEntityTypeExists( entityTypeName );

            string sql = @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [name] = '{0}')
    DECLARE @ActionId int = ( SELECT TOP 1 [Id] FROM [RestAction] WHERE [ApiId] = '{1}{2}')

    IF @EntityTypeId IS NOT NULL AND @ActionId IS NOT NULL
    BEGIN

        DECLARE @groupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '{6}')

        IF NOT EXISTS (
            SELECT [Id] FROM [dbo].[Auth]
            WHERE [EntityTypeId] = @entityTypeId
            AND [EntityId] = @ActionId
            AND [Action] = '{4}'
            AND [SpecialRole] = {7}
            AND [GroupId] = @groupId
        )
        BEGIN
            INSERT INTO [dbo].[Auth]
                   ([EntityTypeId]
                   ,[EntityId]
                   ,[Order]
                   ,[Action]
                   ,[AllowOrDeny]
                   ,[SpecialRole]
                   ,[GroupId]
                   ,[Guid])
             VALUES
                   (@EntityTypeId
                   ,@ActionId
                   ,{3}
                   ,'{4}'
                   ,'{5}'
                   ,{7}
                   ,@groupId
                   ,'{8}')
        END
    END
";
            Migration.Sql( string.Format( sql,
                entityTypeName,                 // 0
                restActionMethod,               // 1
                restActionPath,                 // 2
                order,                          // 3
                action,                         // 4
                ( allow ? "A" : "D" ),          // 5
                groupGuid,                      // 6
                specialRole.ConvertToInt(),     // 7
                authGuid ) );                   // 8
        }

        #endregion

        #region Group Type

        /// <summary>
        /// Adds or Updates the GroupType for the given guid (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="groupTerm">The group term.</param>
        /// <param name="groupMemberTerm">The group member term.</param>
        /// <param name="allowMultipleLocations">if set to <c>true</c> [allow multiple locations].</param>
        /// <param name="showInGroupList">if set to <c>true</c> [show in group list].</param>
        /// <param name="showInNavigation">if set to <c>true</c> [show in navigation].</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="order">The order.</param>
        /// <param name="inheritedGroupTypeGuid">The inherited group type unique identifier.</param>
        /// <param name="locationSelectionMode">The location selection mode.</param>
        /// <param name="groupTypePurposeValueGuid">The group type purpose value unique identifier.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void AddGroupType( string name, string description, string groupTerm, string groupMemberTerm, bool allowMultipleLocations,
            bool showInGroupList, bool showInNavigation, string iconCssClass, int order, string inheritedGroupTypeGuid, int locationSelectionMode, string groupTypePurposeValueGuid,
            string guid, bool isSystem = true )
        {
            UpdateGroupType( name, description, groupTerm, groupMemberTerm, null, allowMultipleLocations, showInGroupList, showInNavigation,
                iconCssClass, order, inheritedGroupTypeGuid, locationSelectionMode, groupTypePurposeValueGuid, guid, isSystem );
        }

        /// <summary>
        /// Adds or Updates the GroupType for the given guid (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="groupTerm">The group term.</param>
        /// <param name="groupMemberTerm">The group member term.</param>
        /// <param name="defaultGroupRoleGuid">The guid for default group role. If you don't have a role yet, use 'null' and then use the <see cref="MigrationHelper.UpdateGroupTypeRole"/> method later when creating the new role.</param>
        /// <param name="allowMultipleLocations">if set to <c>true</c> [allow multiple locations].</param>
        /// <param name="showInGroupList">if set to <c>true</c> [show in group list].</param>
        /// <param name="showInNavigation">if set to <c>true</c> [show in navigation].</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="order">The order.</param>
        /// <param name="inheritedGroupTypeGuid">The inherited group type unique identifier.</param>
        /// <param name="locationSelectionMode">The location selection mode.</param>
        /// <param name="groupTypePurposeValueGuid">The group type purpose value unique identifier.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateGroupType( string name, string description, string groupTerm, string groupMemberTerm, string defaultGroupRoleGuid, bool allowMultipleLocations,
            bool showInGroupList, bool showInNavigation, string iconCssClass, int order, string inheritedGroupTypeGuid, int locationSelectionMode, string groupTypePurposeValueGuid,
            string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                -- Update or insert a group type...

                DECLARE @DefaultGroupRoleId int = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = {6} )
                DECLARE @InheritedGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = {12} )
                DECLARE @GroupTypePurposeValueId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = {14} )

                IF EXISTS (
                    SELECT [Id]
                    FROM [GroupType]
                    WHERE [Guid] = '{0}' )
                BEGIN
                    UPDATE [GroupType] SET
                        [IsSystem] = {1}
                        ,[Name] = '{2}'
                        ,[Description] = '{3}'
                        ,[GroupTerm] = '{4}'
                        ,[GroupMemberTerm] = '{5}'
                        ,[DefaultGroupRoleId] = @DefaultGroupRoleId
                        ,[AllowMultipleLocations] = {7}
                        ,[ShowInGroupList] = {8}
                        ,[ShowInNavigation] = {9}
                        ,[IconCssClass] = '{10}'
                        ,[Order] = {11}
                        ,[InheritedGroupTypeId] = @InheritedGroupTypeId
                        ,[LocationSelectionMode] = {13}
                        ,[GroupTypePurposeValueId] = @GroupTypePurposeValueId
                    WHERE [Guid] = '{0}'
                END
                ELSE
                BEGIN
                    INSERT INTO [GroupType] (
                        [IsSystem]
                        ,[Name]
                        ,[Description]
                        ,[GroupTerm]
                        ,[GroupMemberTerm]
                        ,[DefaultGroupRoleId]
                        ,[AllowMultipleLocations]
                        ,[ShowInGroupList]
                        ,[ShowInNavigation]
                        ,[IconCssClass]
                        ,[TakesAttendance]
                        ,[AttendanceRule]
                        ,[AttendancePrintTo]
                        ,[Order]
                        ,[InheritedGroupTypeId]
                        ,[LocationSelectionMode]
                        ,[GroupTypePurposeValueId]
                        ,[Guid])
                    VALUES(
                        {1}
                        ,'{2}'
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                        ,@DefaultGroupRoleId
                        ,{7}
                        ,{8}
                        ,{9}
                        ,'{10}'
                        ,0
                        ,0
                        ,0
                        ,{11}
                        ,@InheritedGroupTypeId
                        ,{13}
                        ,@GroupTypePurposeValueId
                        ,'{0}')
                END
",
                    guid,
                    ( isSystem ? "1" : "0" ),
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    groupTerm.Replace( "'", "''" ),
                    groupMemberTerm.Replace( "'", "''" ),
                    ( defaultGroupRoleGuid == null ) ? "NULL" : "'" + defaultGroupRoleGuid + "'",
                    ( allowMultipleLocations ? "1" : "0" ),
                    ( showInGroupList ? "1" : "0" ),
                    ( showInNavigation ? "1" : "0" ),
                    iconCssClass,
                    order,
                    ( inheritedGroupTypeGuid == null ) ? "NULL" : "'" + inheritedGroupTypeGuid + "'",
                    locationSelectionMode,
                    ( groupTypePurposeValueGuid == null ) ? "NULL" : "'" + groupTypePurposeValueGuid + "'"
            ) );
        }

        /// <summary>
        /// Adds or Updates the GroupTypeRole for the given guid (if it exists); otherwise it inserts a new record.  Can also set the
        /// role as the default for the given GroupType if isDefaultGroupTypeRole is set to true.
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="guid">The unique identifier of the group type role.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        /// <param name="isLeader">if set to <c>true</c> [is leader].</param>
        /// <param name="isDefaultGroupTypeRole">if set to <c>true</c> the role will be set as the default role for the given group type.</param>
        public void AddGroupTypeRole( string groupTypeGuid, string name, string description, int order, int? maxCount, int? minCount, string guid, bool isSystem = true, bool isLeader = false, bool isDefaultGroupTypeRole = false )
        {
            UpdateGroupTypeRole( groupTypeGuid, name, description, order, maxCount, minCount, guid, isSystem, isLeader, isDefaultGroupTypeRole );
        }

        /// <summary>
        /// Adds or Updates the GroupTypeRole for the given guid (if it exists); otherwise it inserts a new record.  Can also set the
        /// role as the default for the given GroupType if isDefaultGroupTypeRole is set to true.
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="guid">The unique identifier of the group type role.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        /// <param name="isLeader">if set to <c>true</c> [is leader].</param>
        /// <param name="isDefaultGroupTypeRole">if set to <c>true</c> the role will be set as the default role for the given group type.</param>
        public void UpdateGroupTypeRole( string groupTypeGuid, string name, string description, int order, int? maxCount, int? minCount, string guid, bool isSystem = true, bool isLeader = false, bool isDefaultGroupTypeRole = false )
        {
            Migration.Sql( string.Format( @"
                -- Update or insert a group type role...

                DECLARE @GroupTypeId int = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = '{9}' )
                DECLARE @GroupTypeRoleId int

                IF EXISTS (
                    SELECT [Id]
                    FROM [GroupTypeRole]
                    WHERE [Guid] = '{0}' )
                BEGIN
                    UPDATE [GroupTypeRole] SET
                        [IsSystem] = {1}
                        ,[GroupTypeId] = @GroupTypeId
                        ,[Name] = '{2}'
                        ,[Description] = '{3}'
                        ,[Order] = {4}
                        ,[MaxCount] = {5}
                        ,[MinCount] = {6}
                        ,[IsLeader] = {7}
                    WHERE [Guid] = '{0}'

                    SET @GroupTypeRoleId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{0}')

                END
                ELSE
                BEGIN
                    INSERT INTO [GroupTypeRole]
                        ([IsSystem]
                        ,[GroupTypeId]
                        ,[Name]
                        ,[Description]
                        ,[Order]
                        ,[MaxCount]
                        ,[MinCount]
                        ,[IsLeader]
                        ,[Guid])
                    VALUES
                        ({1}
                        ,@GroupTypeId
                        ,'{2}'
                        ,'{3}'
                        ,{4}
                        ,{5}
                        ,{6}
                        ,{7}
                        ,'{0}')

                    SET @GroupTypeRoleId = SCOPE_IDENTITY()

                END

                IF {8} = 1
                BEGIN
                    -- Update the new group type with the default role id
                    UPDATE [GroupType]
                        SET [DefaultGroupRoleId] = @GroupTypeRoleId
                    WHERE
                        [Id] = @GroupTypeId
                END
",
                    guid,
                    ( isSystem ? "1" : "0" ),
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    order,
                    ( maxCount == null ) ? "NULL" : maxCount.ToString(),
                    ( minCount == null ) ? "NULL" : minCount.ToString(),
                    ( isLeader ? "1" : "0" ),
                    ( isDefaultGroupTypeRole ? "1" : "0" ),
                    groupTypeGuid
                ) );
        }

        /// <summary>
        /// Adds a new GroupType "Group Attribute" for the given GroupType using the given values.
        /// </summary>
        /// <param name="groupTypeGuid"></param>
        /// <param name="fieldTypeGuid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="order"></param>
        /// <param name="defaultValue">a string, empty string, or NULL</param>
        /// <param name="guid"></param>
        public void AddGroupTypeGroupAttribute( string groupTypeGuid, string fieldTypeGuid, string name, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Group')

                DECLARE @GroupTypeId int
                SET @GroupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute]
                WHERE
                    [EntityTypeId] = @EntityTypeId
                    AND [Key] = '{2}'
                    AND [EntityTypeQualifierColumn] = 'GroupTypeId'
                    AND [EntityTypeQualifierValue] = @GroupTypeId

                INSERT INTO [Attribute]
                    ([IsSystem]
                    ,[FieldTypeId]
                    ,[EntityTypeId]
                    ,[EntityTypeQualifierColumn]
                    ,[EntityTypeQualifierValue]
                    ,[Key]
                    ,[Name]
                    ,[Description]
                    ,[Order]
                    ,[IsGridColumn]
                    ,[DefaultValue]
                    ,[IsMultiValue]
                    ,[IsRequired]
                    ,[Guid])
                VALUES
                    (1
                    ,@FieldTypeId
                    ,@EntityTypeId
                    ,'GroupTypeId'
                    ,@GroupTypeId
                    ,'{2}'
                    ,'{3}'
                    ,'{4}'
                    ,{5}
                    ,0
                    ,{6}
                    ,0
                    ,0
                    ,'{7}')
",
                    groupTypeGuid,
                    fieldTypeGuid,
                    name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    ( defaultValue == null ) ? "NULL" : "'" + defaultValue + "'",
                    guid )
            );
        }

        /// <summary>
        /// Deletes the GroupType.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteGroupType( string guid )
        {
            Migration.Sql( string.Format( @"

                -- Delete the group type and any dangling bits
                DECLARE @GroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '{0}')
                UPDATE [GroupType] SET [InheritedGroupTypeId] = NULL, [DefaultGroupRoleId] = NULL WHERE [InheritedGroupTypeId] = @GroupTypeId
                DELETE [GroupTypeAssociation] WHERE [ChildGroupTypeId] = @GroupTypeId OR [GroupTypeId] = @GroupTypeId
                DELETE [GroupTypeRole] WHERE [GroupTypeId] = @GroupTypeId
                DELETE [GroupType] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        /// <summary>
        /// Deletes the GroupTypeRole.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteGroupTypeRole( string guid )
        {
            Migration.Sql( string.Format( @"

                -- Delete the group type role and any dangling bits
                DECLARE @GroupTypeRoleId int = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '{0}')
                UPDATE [GroupType] SET [DefaultGroupRoleId] = NULL WHERE [DefaultGroupRoleId] = @GroupTypeRoleId
                DELETE [GroupTypeRole] WHERE [Guid] = '{0}'
",
                    guid
                    ) );
        }

        #endregion

        #region Group

        /// <summary>
        /// Updates the Group for the given guid (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="parentGroupGuid">The parent group unique identifier.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="name">The name of the group.</param>
        /// <param name="description">The description.</param>
        /// <param name="campusGuid">The campus unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier of the group.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        /// <param name="isSecurityRole">if set to <c>true</c> [is security role].</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public void UpdateGroup( string parentGroupGuid, string groupTypeGuid, string name, string description, string campusGuid, int order,
                    string guid, bool isSystem = true, bool isSecurityRole = false, bool isActive = true )
        {
            Migration.Sql( string.Format( @"

                -- Update or insert a group...

                DECLARE @ParentGroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = {2} )
                DECLARE @GroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = {3} )
                DECLARE @CampusId int = ( SELECT TOP 1 [Id] FROM [Campus] WHERE [Guid] = {4} )

                IF EXISTS (
                    SELECT [Id]
                    FROM [Group]
                    WHERE [Guid] = '{0}' )
                BEGIN
                    UPDATE [Group] SET
                        [IsSystem] = {1}
                        ,[ParentGroupId] = @ParentGroupId
                        ,[GroupTypeId] = @GroupTypeId
                        ,[CampusId] = @CampusId
                        ,[Name] = '{5}'
                        ,[Description] = '{6}'
                        ,[IsSecurityRole] = {7}
                        ,[IsActive] = {8}
                        ,[Order] = {9}
                    WHERE [Guid] = '{0}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Group] (
                        [IsSystem]
                        ,[ParentGroupId]
                        ,[GroupTypeId]
                        ,[CampusId]
                        ,[Name]
                        ,[Description]
                        ,[IsSecurityRole]
                        ,[IsActive]
                        ,[Order]
                        ,[Guid])
                    VALUES(
                        {1}
                        ,@ParentGroupId
                        ,@GroupTypeId
                        ,@CampusId
                        ,'{5}'
                        ,'{6}'
                        ,{7}
                        ,{8}
                        ,{9}
                        ,'{0}')
                END
",
                    guid,
                    ( isSystem ? "1" : "0" ),
                    ( parentGroupGuid == null ) ? "NULL" : "'" + parentGroupGuid + "'",
                    ( groupTypeGuid == null ) ? "NULL" : "'" + groupTypeGuid + "'",
                    ( campusGuid == null ) ? "NULL" : "'" + campusGuid + "'",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    ( isSecurityRole ? "1" : "0" ),
                    ( isActive ? "1" : "0" ),
                    order
            ) );
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="orphanAnyChildren">if set to <c>true</c> any child groups will be orphaned.</param>
        public void DeleteGroup( string guid, bool orphanAnyChildren = true )
        {
            Migration.Sql( string.Format( @"

                -- Delete the group and any dangling bits
                DECLARE @GroupId int = (SELECT [ID] FROM [Group] WHERE [Guid] = '{0}')

                -- orphan any children?
                IF {1} = 1
                BEGIN
                    UPDATE [Group] SET [ParentGroupId] = NULL WHERE [ParentGroupId] = @GroupId
                END

                DELETE [Group] WHERE [Guid] = '{0}'
",
                    guid,
                    ( orphanAnyChildren ? "1" : "0" )
            ) );
        }

        #endregion

        #region PersonAttribute

        /// <summary>
        /// Updates the person attribute category.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="order">The order.</param>
        public void UpdatePersonAttributeCategory( string name, string iconCssClass, string description, string guid, int order = 0 )
        {
            Migration.Sql( string.Format( @"

                DECLARE @AttributeEntityTypeId int
                SET @AttributeEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '5997C8D3-8840-4591-99A5-552919F90CBD')

                DECLARE @PersonEntityTypeId int
                SET @PersonEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Category]
                    WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [Category] SET
                        [Name] = '{0}',
                        [IconCssClass] = '{1}',
                        [Description] = '{2}',
                        [Order] = {4}
                    WHERE [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[IconCssClass],[Description],[Order],[Guid] )
                    VALUES( 1,@AttributeEntityTypeId,'EntityTypeId',CAST(@PersonEntityTypeId as varchar),'{0}','{1}','{2}',{4},'{3}' )
                END
",
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    guid,
                    order )
            );
        }

        /// <summary>
        /// Updates the BlockType Attribute for the given blocktype and key (if it exists);
        /// otherwise it inserts a new record.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.  Defaults to Name without Spaces. If this is a core person attribute, specify the key with a 'core.' prefix</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdatePersonAttribute( string fieldTypeGuid, string categoryGuid, string name, string key, string iconCssClass, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{0}')

                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = ''
                    AND [EntityTypeQualifierValue] = ''
                    AND [Key] = '{1}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{2}',
                        [IconCssClass] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = ''
                    AND [EntityTypeQualifierValue] = ''
                    AND [Key] = '{1}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[IconCssClass],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'','',
                        '{1}','{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')
                END
",
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    iconCssClass,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );

            Migration.Sql( string.Format( @"

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{0}')

                DECLARE @CategoryId int
                SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{1}')

                IF NOT EXISTS (
                    SELECT *
                    FROM [AttributeCategory]
                    WHERE [AttributeId] = @AttributeId
                    AND [CategoryId] = CategoryId )
                BEGIN
                    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
                    VALUES( @AttributeId, @CategoryId )
                END
",
                    guid,
                    categoryGuid )
            );
        }

        #endregion

        #region PersonBadge

        /// <summary>
        /// Updates the PersonBadge by Guid (if it exists); otherwise it inserts a new record.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdatePersonBadge( string name, string description, string entityTypeName, int order, string guid )
        {
            Migration.Sql( string.Format( @"
                    DECLARE @EntityTypeId int = (SELECT [ID] FROM [EntityType] WHERE [Name] = '{2}')

                    IF EXISTS ( SELECT * FROM [PersonBadge] where [Guid] = '{4}')
                    BEGIN
                        UPDATE [PersonBadge] set
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [EntityTypeId] = @EntityTypeId,
                            [Order] = {3}
                        WHERE [Guid] = '{4}'

                    END
                    ELSE
                    BEGIN
                        INSERT INTO [PersonBadge] ([Name],[Description],[EntityTypeId],[Order],[Guid])
                            VALUES ('{0}', '{1}', @EntityTypeId, {3}, '{4}')
                    END

",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    entityTypeName,
                    order,
                    guid )
            );
        }

        /// <summary>
        /// Adds (or Deletes and Adds) the person badge attribute.
        /// </summary>
        /// <param name="personBadgeGuid">The person badge unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddPersonBadgeAttribute( string personBadgeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @PersonBadgeId int
                SET @PersonBadgeId = (SELECT [Id] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @PersonBadgeEntityTypeId int
                SET @PersonBadgeEntityTypeId = (SELECT [EntityTypeId] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')

                -- get the EntityTypeId for 'Rock.Model.PersonBadge'
                DECLARE @EntityTypeId int
                SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonBadge')

                -- Delete existing attribute first (might have been created by Rock system)
                DELETE [Attribute]
                WHERE [EntityTypeId] = @EntityTypeId
                AND [EntityTypeQualifierColumn] = 'EntityTypeId'
                AND [EntityTypeQualifierValue] = CAST(@PersonBadgeEntityTypeId as varchar)
                AND [Key] = '{2}'

                INSERT INTO [Attribute] (
                    [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                    [Key],[Name],[Description],
                    [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                    [Guid])
                VALUES(
                    1,@FieldTypeId, @EntityTypeId,'EntityTypeId',CAST(@PersonBadgeEntityTypeId as varchar),
                    '{2}','{3}','{4}',
                    {5},0,'{6}',0,0,
                    '{7}')
",
                    personBadgeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Adds/Updates the person badge attribute value.
        /// </summary>
        /// <param name="personBadgeGuid">The person badge unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void AddPersonBadgeAttributeValue( string personBadgeGuid, string attributeGuid, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @PersonBadgeId int
                SET @PersonBadgeId = (SELECT [Id] FROM [PersonBadge] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @PersonBadgeId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@PersonBadgeId,
                    '{2}',
                    NEWID())
",
                    personBadgeGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        #endregion

        #region SystemEmail

        /// <summary>
        /// Updates or Inserts the system email.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="title">The title.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateSystemEmail( string category, string title, string from, string fromName, string to,
            string cc, string bcc, string subject, string body, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @SystemEmailEntity int = (
                    SELECT TOP 1 [Id]
                    FROM [EntityType]
                    WHERE [Name] = 'Rock.Model.SystemEmail' )

                DECLARE @CategoryId int = (
                    SELECT TOP 1 [Id] FROM [Category]
                    WHERE [EntityTypeId] = @SystemEmailEntity
                    AND [Name] = '{0}' )

                IF @CategoryId IS NULL AND @SystemEmailEntity IS NOT NULL
                BEGIN
                    INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[Order],[Guid] )
                    VALUES( 0, @SystemEmailEntity,'{0}', 0, NEWID() )
                    SET @CategoryId = SCOPE_IDENTITY()
                END

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [SystemEmail] WHERE [guid] = '{9}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [SystemEmail] (
                        [IsSystem],[CategoryId],[Title],[From],[FromName],[To],[cc],[Bcc],[Subject],[Body],[Guid])
                    VALUES(
                        1, @CategoryId,'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')
                END
                ELSE
                BEGIN
                    UPDATE [SystemEmail] SET
                        [CategoryId] = @CategoryId,
                        [Title] = '{1}',
                        [From] = '{2}',
                        [FromName] = '{3}',
                        [To] = '{4}',
                        [Cc] = '{5}',
                        [Bcc] = '{6}',
                        [Subject] = '{7}',
                        [Body] = '{8}'
                    WHERE [Guid] = '{9}'
                END
",
                    category.Replace( "'", "''" ),
                    title.Replace( "'", "''" ),
                    from.Replace( "'", "''" ),
                    fromName.Replace( "'", "''" ),
                    to.Replace( "'", "''" ),
                    cc.Replace( "'", "''" ),
                    bcc.Replace( "'", "''" ),
                    subject.Replace( "'", "''" ),
                    body.Replace( "'", "''" ),
                    guid ) );
        }

        /// <summary>
        /// Deletes the SystemEmail.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        public void DeleteSystemEmail( string guid )
        {
            DeleteByGuid( guid, "SystemEmail" );
        }

        #endregion

        #region Workflow Methods

        /// <summary>
        /// Updates the workflow action entity attribute.
        /// </summary>
        /// <param name="actionEntityTypeGuid">The action entity type unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActionEntityAttribute( string actionEntityTypeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ActionEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')
                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActionType')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'EntityTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@ActionEntityTypeId as varchar)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [FieldTypeId] = @FieldTypeId,
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'EntityTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@ActionEntityTypeId as varchar)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'EntityTypeId',CAST(@ActionEntityTypeId as varchar),
                        '{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')
                END
",
                    actionEntityTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Updates the type of the workflow.
        /// </summary>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="workTerm">The work term.</param>
        /// <param name="iconCssClass">The icon CSS class.</param>
        /// <param name="processingIntervalSeconds">The processing interval seconds.</param>
        /// <param name="isPersisted">if set to <c>true</c> [is persisted].</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="order">The order.</param>
        public void UpdateWorkflowType( bool isSystem, bool isActive, string name, string description, string categoryGuid, string workTerm, string iconCssClass,
            int processingIntervalSeconds, bool isPersisted, int loggingLevel, string guid, int order = 0 )
        {
            Migration.Sql( string.Format( @"

                DECLARE @CategoryId int = (SELECT [Id] FROM [Category] WHERE [Guid] = '{4}')

                IF EXISTS ( SELECT [Id] FROM [WorkflowType] WHERE [Guid] =  '{10}' )
                BEGIN
                    UPDATE [WorkflowType] SET
                        [IsSystem] = {0},
                        [IsActive] = {1},
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [CategoryId] = @CategoryId,
                        [WorkTerm] = '{5}',
                        [IconCssClass] = '{6}',
                        [ProcessingIntervalSeconds] = {7},
                        [IsPersisted] = {8},
                        [LoggingLevel] = {9},
                        [Order] = {11}
                    WHERE [Guid] = '{10}'
                END
                ELSE
                BEGIN
                    INSERT INTO [WorkflowType] (
                        [IsSystem], [IsActive], [Name], [Description], [CategoryId], [WorkTerm], [IconCssClass],
                        [ProcessingIntervalSeconds], [IsPersisted], [LoggingLevel], [Guid], [Order] )
                    VALUES( {0}, {1}, '{2}', '{3}', @CategoryId, '{5}', '{6}', {7}, {8}, {9}, '{10}', {11} )
                END
",
                    ( isSystem ? "1" : "0" ),
                    ( isActive ? "1" : "0" ),
                    name,
                    description.Replace( "'", "''" ),
                    categoryGuid,
                    workTerm.Replace( "'", "''" ),
                    iconCssClass.Replace( "'", "''" ),
                    processingIntervalSeconds,
                    ( isPersisted ? "1" : "0" ),
                    loggingLevel,
                    guid,
                    order )
            );
        }

        /// <summary>
        /// Updates the workflow type attribute.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowTypeAttribute( string workflowTypeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @WorkflowTypeId int = (SELECT [Id] FROM [WorkflowType] WHERE [Guid] = '{0}')
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')
                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Workflow')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'WorkflowTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@WorkflowTypeId as varchar)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'WorkflowTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@WorkflowTypeId as varchar)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'WorkflowTypeId',CAST(@WorkflowTypeId as varchar),
                        '{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')
                END
",
                    workflowTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Updates the type of the workflow activity.
        /// </summary>
        /// <param name="WorkflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="isActivatedWithWorkflow">if set to <c>true</c> [is activated with workflow].</param>
        /// <param name="order">The order.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActivityType( string WorkflowTypeGuid, bool isActive, string name, string description,
            bool isActivatedWithWorkflow, int order, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @WorkflowTypeId int = (SELECT [Id] FROM [WorkflowType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] =  '{6}' )
                BEGIN
                    UPDATE [WorkflowActivityType] SET
                        [WorkflowTypeId] = @WorkflowTypeId,
                        [IsActive] = {1},
                        [Name] = '{2}',
                        [Description] = '{3}',
                        [IsActivatedWithWorkflow] = {4},
                        [Order] = {5}
                    WHERE [Guid] = '{6}'
                END
                ELSE
                BEGIN
                    INSERT INTO [WorkflowActivityType] ( [WorkflowTypeId], [IsActive], [Name], [Description], [IsActivatedWithWorkflow], [Order], [Guid] )
                    VALUES( @WorkflowTypeId, {1}, '{2}', '{3}', {4}, {5}, '{6}' )
                END
",
                    WorkflowTypeGuid,
                    ( isActive ? "1" : "0" ),
                    name,
                    description.Replace( "'", "''" ),
                    ( isActivatedWithWorkflow ? "1" : "0" ),
                    order,
                    guid )
            );
        }

        /// <summary>
        /// Updates the workflow activity type attribute.
        /// </summary>
        /// <param name="workflowActivityTypeGuid">The workflow activity type unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActivityTypeAttribute( string workflowActivityTypeGuid, string fieldTypeGuid, string name, string key, string description, int order, string defaultValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @WorkflowActivityTypeId int = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = '{0}')
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')
                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActivity')

                IF EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'ActivityTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@WorkflowActivityTypeId as varchar)
                    AND [Key] = '{2}' )
                BEGIN
                    UPDATE [Attribute] SET
                        [Name] = '{3}',
                        [Description] = '{4}',
                        [Order] = {5},
                        [DefaultValue] = '{6}',
                        [Guid] = '{7}'
                    WHERE [EntityTypeId] = @EntityTypeId
                    AND [EntityTypeQualifierColumn] = 'ActivityTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@WorkflowActivityTypeId as varchar)
                    AND [Key] = '{2}'
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'ActivityTypeId',CAST(@WorkflowActivityTypeId as varchar),
                        '{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        '{7}')
                END
",
                    workflowActivityTypeGuid,
                    fieldTypeGuid,
                    key ?? name.Replace( " ", string.Empty ),
                    name,
                    description.Replace( "'", "''" ),
                    order,
                    defaultValue.Replace( "'", "''" ),
                    guid )
            );
        }

        /// <summary>
        /// Updates the workflow action form.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="footer">The footer.</param>
        /// <param name="actions">The actions.</param>
        /// <param name="systemEmailGuid">The system email unique identifier.</param>
        /// <param name="includeActionsInNotification">if set to <c>true</c> [include actions in notification].</param>
        /// <param name="actionAttributeGuid">The action attribute unique identifier.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActionForm( string header, string footer, string actions, string systemEmailGuid,
            bool includeActionsInNotification, string actionAttributeGuid, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @SystemEmailId int = (SELECT [Id] FROM [SystemEmail] WHERE [Guid] = '{3}')

                IF EXISTS ( SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] =  '{6}' )
                BEGIN
                    UPDATE [WorkflowActionForm] SET
                        [Header] = '{0}',
                        [Footer] = '{1}',
                        [Actions] = '{2}',
                        [NotificationSystemEmailId] = @SystemEmailId,
                        [IncludeActionsInNotification] = {4},
                        [ActionAttributeGuid] = {5}
                    WHERE [Guid] = '{6}'
                END
                ELSE
                BEGIN
                    INSERT INTO [WorkflowActionForm] (
                        [Header], [Footer], [Actions], [NotificationSystemEmailId], [IncludeActionsInNotification], [ActionAttributeGuid], [Guid] )
                    VALUES( '{0}', '{1}', '{2}', @SystemEmailId, {4}, {5}, '{6}' )
                END
",
                    header.Replace( "'", "''" ),
                    footer.Replace( "'", "''" ),
                    actions,
                    ( string.IsNullOrWhiteSpace( systemEmailGuid ) ? Guid.Empty.ToString() : systemEmailGuid ),
                    ( includeActionsInNotification ? "1" : "0" ),
                    ( string.IsNullOrWhiteSpace( actionAttributeGuid ) ? "NULL" : "'" + actionAttributeGuid + "'" ),
                    guid )
            );
        }

        /// <summary>
        /// Updates the workflow action form attribute.
        /// </summary>
        /// <param name="actionFormGuid">The action form unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="order">The order.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="isReadOnly">if set to <c>true</c> [is read only].</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActionFormAttribute( string actionFormGuid, string attributeGuid, int order,
            bool isVisible, bool isReadOnly, bool isRequired, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ActionFormId int = (SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '{0}')
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                IF EXISTS ( SELECT [Id] FROM [WorkflowActionFormAttribute] WHERE [Guid] =  '{6}' )
                BEGIN
                    UPDATE [WorkflowActionFormAttribute] SET
                        [WorkflowActionFormId] = @ActionFormId,
                        [AttributeId] = @AttributeId,
                        [Order] = {2},
                        [IsVisible] = {3},
                        [IsReadOnly] = {4},
                        [IsRequired] = {5}
                    WHERE [Guid] = '{6}'
                END
                ELSE
                BEGIN
                    INSERT INTO [WorkflowActionFormAttribute] (
                        [WorkflowActionFormId], [AttributeId], [Order], [IsVisible], [IsReadOnly], [IsRequired], [Guid] )
                    VALUES( @ActionFormId, @AttributeId, {2}, {3}, {4}, {5}, '{6}' )
                END
",
                    actionFormGuid,
                    attributeGuid,
                    order,
                    ( isVisible ? "1" : "0" ),
                    ( isReadOnly ? "1" : "0" ),
                    ( isRequired ? "1" : "0" ),
                    guid )
            );
        }

        /// <summary>
        /// Updates the type of the workflow action.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="order">The order.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="isActionCompletedOnSuccess">if set to <c>true</c> [is action completed on success].</param>
        /// <param name="isActivityCompletedOnSuccess">if set to <c>true</c> [is activity completed on success].</param>
        /// <param name="workflowFormGuid">The workflow form unique identifier.</param>
        /// <param name="criteriaAttributeGuid">The criteria attribute unique identifier.</param>
        /// <param name="criteriaComparisonType">Type of the criteria comparison.</param>
        /// <param name="criteriaValue">The criteria value.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateWorkflowActionType( string activityTypeGuid, string name, int order, string entityTypeGuid,
            bool isActionCompletedOnSuccess, bool isActivityCompletedOnSuccess, string workflowFormGuid, string criteriaAttributeGuid,
            int criteriaComparisonType, string criteriaValue, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ActivityTypeId int = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = '{0}')
                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{3}')
                DECLARE @FormId int = (SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '{6}')

                IF EXISTS ( SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] =  '{10}' )
                BEGIN
                    UPDATE [WorkflowActionType] SET
                        [ActivityTypeId] = @ActivityTypeId,
                        [Name] = '{1}',
                        [Order] = {2},
                        [EntityTypeId] = @EntityTypeId,
                        [IsActionCompletedOnSuccess] = {4},
                        [IsActivityCompletedOnSuccess] = {5},
                        [WorkflowFormId] = @FormId,
                        [CriteriaAttributeGuid] = {7},
                        [CriteriaComparisonType] = {8},
                        [CriteriaValue] = '{9}'
                    WHERE [Guid] = '{10}'
                END
                ELSE
                BEGIN
                    INSERT INTO [WorkflowActionType] (
                        [ActivityTypeId], [Name], [Order], [EntityTypeId], [IsActionCompletedOnSuccess], [IsActivityCompletedOnSuccess],
                        [WorkflowFormId], [CriteriaAttributeGuid], [CriteriaComparisonType], [CriteriaValue], [Guid] )
                    VALUES( @ActivityTypeId, '{1}', {2}, @EntityTypeId, {4}, {5}, @FormId, {7}, {8}, '{9}', '{10}' )
                END
",
                    activityTypeGuid,
                    name.Replace( "'", "''" ),
                    order,
                    entityTypeGuid,
                    ( isActionCompletedOnSuccess ? "1" : "0" ),
                    ( isActivityCompletedOnSuccess ? "1" : "0" ),
                    ( string.IsNullOrWhiteSpace( workflowFormGuid ) ? Guid.Empty.ToString() : workflowFormGuid ),
                    ( string.IsNullOrWhiteSpace( criteriaAttributeGuid ) ? "NULL" : "'" + criteriaAttributeGuid + "'" ),
                    criteriaComparisonType,
                    criteriaValue,
                    guid )
            );
        }

        /// <summary>
        /// Adds the action type attribute value.
        /// </summary>
        /// <param name="actionTypeGuid">The action type unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void AddActionTypeAttributeValue( string actionTypeGuid, string attributeGuid, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ActionTypeId int = (SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] = '{0}')
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @ActionTypeId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@ActionTypeId,
                    '{2}',
                    NEWID())
",
                    actionTypeGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        /// <summary>
        /// Adds an action type person attribute value.  Because there's not a way to link to another person in the
        /// target database, person attribute values are just set to the first person alias record in the target
        /// database which will most likely be the Admin, Admin record.
        /// </summary>
        /// <param name="actionTypeGuid">The action type unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="value">The value.</param>
        public void AddActionTypePersonAttributeValue( string actionTypeGuid, string attributeGuid, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @ActionTypeId int = (SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] = '{0}')
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @ActionTypeId

                IF NOT EXISTS ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @ActionTypeId )
                BEGIN
                    IF '{2}' = '' OR EXISTS ( SELECT [Id] FROM [PersonAlias] WHERE [Guid] = '{2}' )
                    BEGIN
                        INSERT INTO [AttributeValue] (
                            [IsSystem],[AttributeId],[EntityId],
                            [Value],
                            [Guid])
                        VALUES(
                            1,@AttributeId,@ActionTypeId,
                            '{2}',
                            NEWID())
                    END
                    ELSE
                    BEGIN
                        INSERT INTO [AttributeValue] (
                            [IsSystem],[AttributeId],[EntityId],
                            [Value],
                            [Guid])
                        SELECT TOP 1
                            1,@AttributeId,@ActionTypeId,
                            CONVERT(nvarchar(50), [Guid]),
                            NEWID()
                        FROM [PersonAlias]
                        ORDER BY [Id]
                    END
                END
",
                    actionTypeGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }

        /// <summary>
        /// Deletes workflow triggers that reference a workflow type that has a category defined by the given guid.
        /// </summary>
        /// <param name="workflowCategoryGuid">The workflow category unique identifier.</param>
        public void DeleteWorkflowTriggersByWorkflowCategory( string workflowCategoryGuid )
        {
            Migration.Sql( string.Format( @"
                DELETE
                FROM WorkflowTrigger t
                WHERE t.WorkflowTypeId IN (
	                SELECT w.Id
	                FROM WorkflowType w
	                JOIN Category c ON c.Id = w.CategoryId
	                WHERE c.Guid = '{0}'
                )", workflowCategoryGuid ) );
        }

        /// <summary>
        /// Creates the workflow trigger.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        public void CreateWorkflowTrigger( string entityTypeName, WorkflowTriggerType triggerType, string qualifierColumn, string qualifierValue, string workflowTypeGuid, string description, string guid = null )
        {
            guid = guid != null ? string.Format( "'{0}'", guid ) : "NEWID()";

            Migration.Sql( string.Format( @"
                INSERT INTO [dbo].[WorkflowTrigger]
                   ([IsSystem]
                   ,[EntityTypeId]
                   ,[EntityTypeQualifierColumn]
                   ,[EntityTypeQualifierValue]
                   ,[WorkflowTypeId]
                   ,[WorkflowTriggerType]
                   ,[WorkflowName]
                   ,[Guid]
                   ,[IsActive])
                VALUES
                   (0
                   ,(SELECT Id FROM EntityType WHERE NAME = 'Rock.Model.{0}')
                   ,'{1}'
                   ,'{2}'
                   ,(SELECT Id FROM WorkflowType WHERE Guid = '{3}')
                   ,{4}
                   ,'{5}'
                   ,{6}
                   ,1)", entityTypeName, qualifierColumn, qualifierValue, workflowTypeGuid, (int)triggerType, description, guid ) );
        }

        /// <summary>
        /// Deletes the type of the workflow.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteWorkflowType( string guid )
        {
            DeleteByGuid( guid, "WorkflowType" );
        }

        /// <summary>
        /// Deletes the type of the workflow activity.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteWorkflowActivityType( string guid )
        {
            DeleteByGuid( guid, "WorkflowActivityType" );
        }

        /// <summary>
        /// Deletes the type of the workflow action.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteWorkflowActionType( string guid )
        {
            DeleteByGuid( guid, "WorkflowActionType" );
        }

        #endregion

        #region REST Methods

        /// <summary>
        /// Adds the rest controller.
        /// </summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="controllerClass">The controller class.</param>
        public void AddRestController( string controllerName, string controllerClass )
        {
            Migration.Sql( string.Format( @"

    DECLARE @ControllerId int = ( SELECT TOP 1 [Id] FROM [RestController] WHERE [ClassName] = '{1}' )
    IF @ControllerId IS NULL
    BEGIN

        INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
	    VALUES ( '{0}', '{1}', NEWID() )

	    SET @ControllerId = SCOPE_IDENTITY()

    END
",
                    controllerName,
                    controllerClass
                    ) );
        }

        /// <summary>
        /// Adds the rest action.
        /// </summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="controllerClass">The controller class.</param>
        /// <param name="actionMethod">The action method.</param>
        /// <param name="actionPath">The action path.</param>
        public void AddRestAction( string controllerName, string controllerClass, string actionMethod, string actionPath )
        {
            AddRestController( controllerName, controllerClass );

            Migration.Sql( string.Format( @"

    DECLARE @ControllerId int = ( SELECT TOP 1 [Id] FROM [RestController] WHERE [ClassName] = '{0}' )
    DECLARE @ActionId int = ( SELECT TOP 1 [Id] FROM [RestAction] WHERE [ApiId] = '{1}{2}' )
    IF @ActionId IS NULL
    BEGIN

	    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
	    VALUES ( @ControllerId, '{1}', '{1}{2}', '{2}', NEWID() )

    END

",
                    controllerClass,
                    actionMethod,
                    actionPath
                    ) );
        }

        #endregion

        #region Deprecated Methods

        /// <summary>
        /// Adds a new DefinedValue for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void AddDefinedValue_pre20140819( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

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
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Updates (or Adds) the defined value for the given DefinedType.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateDefinedValue_pre20140819( string definedTypeGuid, string name, string description, string guid, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{3}' )
                BEGIN
                    UPDATE [DefinedValue]
                    SET
                        [IsSystem] = {4}
                        ,[DefinedTypeId] = @DefinedTypeId
                        ,[Name] = '{1}'
                        ,[Description] = '{2}'
                    WHERE
                        [Guid] = '{3}'
                END
                ELSE
                BEGIN
                    DECLARE @Order int
                    SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId

                    INSERT INTO [DefinedValue]
                        ([IsSystem]
                        ,[DefinedTypeId]
                        ,[Order]
                        ,[Name]
                        ,[Description]
                        ,[Guid])
                    VALUES
                        ({4}
                        ,@DefinedTypeId
                        ,@Order
                        ,'{1}'
                        ,'{2}'
                        ,'{3}')
                END
",
                    definedTypeGuid,
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    guid,
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Updates the name of the defined value by.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        /// <param name="isSystem">if set to <c>true</c> [is system].</param>
        public void UpdateDefinedValueByName_pre20140819( string definedTypeGuid, string name, string description, int order, bool isSystem = true )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                IF EXISTS ( SELECT [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId AND [Name] = '{1}' )
                BEGIN
                    UPDATE [DefinedValue]
                    SET
                         [IsSystem] = {4}
                        ,[Description] = '{2}'
                        ,[Order] = {3}
                    WHERE [DefinedTypeId] = @DefinedTypeId
                    AND [Name] = '{1}'
                END
                ELSE
                BEGIN
                    INSERT INTO [DefinedValue]
                        ([IsSystem]
                        ,[DefinedTypeId]
                        ,[Name]
                        ,[Description]
                        ,[Order]
                        ,[Guid])
                    VALUES
                        ({4}
                        ,@DefinedTypeId
                        ,'{1}'
                        ,'{2}'
                        ,{3}
                        ,NEWID())
                END
",
                    definedTypeGuid,
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    order,
                    ( isSystem ? "1" : "0" )
                    ) );
        }

        /// <summary>
        /// Adds the name of the defined value attribute value by.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="definedValueName">Name of the defined value.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        public void AddDefinedValueAttributeValueByName_pre20140819( string definedTypeGuid, string definedValueName, string attributeKey, string value )
        {
            Migration.Sql( string.Format( @"

                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                DECLARE @DefinedValueId int
                SET @DefinedValueId = (SELECT [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @DefinedTypeId AND [Name] = '{1}' )

                DECLARE @AttributeId int
                SET @AttributeId = (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeQualifierColumn] = 'DefinedTypeId'
                    AND [EntityTypeQualifierValue] = CAST(@DefinedTypeId as varchar)
                    AND [Key] = '{2}'
                )

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @DefinedValueId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@DefinedValueId,
                    '{3}',
                    NEWID())
",
                    definedTypeGuid,
                    definedValueName,
                    attributeKey,
                    value.Replace( "'", "''" )
                )
            );
        }

        /// <summary>
        /// Adds the defined type_pre201409101843015.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="helpText">The help text.</param>
        public void AddDefinedType_pre201409101843015( string category, string name, string description, string guid, string helpText = null )
        {
            Migration.Sql( string.Format( @"

                DECLARE @FieldTypeId int
                SET @FieldTypeId = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                DECLARE @Order int
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedType];

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [DefinedType]
                    WHERE [Guid] = '{3}' )

                BEGIN

                    INSERT INTO [DefinedType] (
                        [IsSystem],[FieldTypeId],[Order],
                        [Category],[Name],[Description],[HelpText],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId,@Order,
                        '{0}','{1}','{2}','{4}',
                        '{3}')
                END
                ELSE
                BEGIN

                    UPDATE [DefinedType] SET
                        [IsSystem] = 1,
                        [FieldTypeId] = @FieldTypeId,
                        [Category] = '{0}',
                        [Name] = '{1}',
                        [Description] = '{2}',
                        [HelpText] = '{4}'
                    WHERE [Guid] = '{3}'

                END
",
                    category,
                    name,
                    description.Replace( "'", "''" ),
                    guid,
                    helpText ?? string.Empty
                    ) );
        }

        /// <summary>
        /// Updates the system email_pre201409101843015.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="title">The title.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="guid">The unique identifier.</param>
        public void UpdateSystemEmail_pre201409101843015( string category, string title, string from, string fromName, string to,
            string cc, string bcc, string subject, string body, string guid )
        {
            Migration.Sql( string.Format( @"

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [SystemEmail] WHERE [guid] = '{9}')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [SystemEmail] (
                        [IsSystem],[Category],[Title],[From],[FromName],[To],[cc],[Bcc],[Subject],[Body],[Guid])
                    VALUES(
                        1,'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')
                END
                ELSE
                BEGIN
                    UPDATE [SystemEmail] SET
                        [Category] = '{0}',
                        [Title] = '{1}',
                        [From] = '{2}',
                        [FromName] = '{3}',
                        [To] = '{4}',
                        [Cc] = '{5}',
                        [Bcc] = '{6}',
                        [Subject] = '{7}',
                        [Body] = '{8}'
                    WHERE [Guid] = '{9}'
                END
",
                    category.Replace( "'", "''" ),
                    title.Replace( "'", "''" ),
                    from.Replace( "'", "''" ),
                    fromName.Replace( "'", "''" ),
                    to.Replace( "'", "''" ),
                    cc.Replace( "'", "''" ),
                    bcc.Replace( "'", "''" ),
                    subject.Replace( "'", "''" ),
                    body.Replace( "'", "''" ),
                    guid ) );
        }

        #endregion

        #region Reports

        /// <summary>
        /// Adds a report.
        /// </summary>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="dataViewGuid">The data view unique identifier.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="guid">The report.Guid</param>
        /// <param name="fetchTop">The fetch top.</param>
        public void AddReport( string categoryGuid, string dataViewGuid, string entityTypeGuid, string name, string description, string guid, int? fetchTop = null )
        {
            Migration.Sql( string.Format( @"
                DECLARE @CategoryId INT = (
                        SELECT TOP 1 [Id]
                        FROM [Category]
                        WHERE [Guid] = '{0}'
                        )
                    ,@DataViewId INT = (
                        SELECT TOP 1 [Id]
                        FROM [DataView]
                        WHERE [Guid] = '{1}'
                        )
                    ,@EntityTypeId INT = (
                        SELECT TOP 1 [Id]
                        FROM [EntityType]
                        WHERE [Guid] = '{2}'
                        )

                INSERT INTO [Report] (
                    [IsSystem]
                    ,[Name]
                    ,[Description]
                    ,[CategoryId]
                    ,[EntityTypeId]
                    ,[DataViewId]
                    ,[Guid]
                    ,[FetchTop]
                    )
                VALUES (
                    0
                    ,'{3}'
                    ,'{4}'
                    ,@CategoryId
                    ,@EntityTypeId
                    ,@DataViewId
                    ,'{5}'
                    ,{6}
                    )",
                      categoryGuid, // {0}
                      dataViewGuid, // {1}
                      entityTypeGuid, // {2}
                      name, // {3}
                      description, // {4}
                      guid, // {5}
                      fetchTop.HasValue ? fetchTop.Value.ToString() : "NULL" // {6}
                      )
                      );
        }

        /// <summary>
        /// Deletes the report
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteReport( string guid )
        {
            Migration.Sql( string.Format( "DELETE FROM [Report] where [Guid] = '{0}'", guid ) );
        }

        /// <summary>
        /// Adds a report field to a report
        /// </summary>
        /// <param name="reportGuid">The report unique identifier.</param>
        /// <param name="reportFieldType">Type of the report field.</param>
        /// <param name="showInGrid">if set to <c>true</c> [show in grid].</param>
        /// <param name="dataSelectComponentEntityTypeGuid">The data select component entity type unique identifier.</param>
        /// <param name="selection">The selection.</param>
        /// <param name="order">The order.</param>
        /// <param name="columnHeaderText">The column header text.</param>
        /// <param name="guid">The unique identifier.</param>
        public void AddReportField( string reportGuid, Rock.Model.ReportFieldType reportFieldType, bool showInGrid,
            string dataSelectComponentEntityTypeGuid, string selection, int order, string columnHeaderText, string guid )
        {
            Migration.Sql( string.Format( @"
            DECLARE @ReportId INT = (
                        SELECT TOP 1 [Id]
                        FROM [Report]
                        WHERE [Guid] = '{0}'
                        )
                   ,@DataSelectComponentEntityTypeId INT = (
                        SELECT TOP 1 [Id]
                        FROM [EntityType]
                        WHERE [Guid] = '{3}'
                        )

            INSERT INTO [dbo].[ReportField] (
                [ReportId]
                ,[ReportFieldType]
                ,[ShowInGrid]
                ,[DataSelectComponentEntityTypeId]
                ,[Selection]
                ,[Order]
                ,[ColumnHeaderText]
                ,[Guid]
                )
            VALUES (
                @ReportId
                ,{1}
                ,{2}
                ,@DataSelectComponentEntityTypeId
                ,'{4}'
                ,{5}
                ,'{6}'
                ,'{7}'
                )
            ",
              reportGuid, // {0}
              reportFieldType.ConvertToInt(), // {1}
              showInGrid.Bit(), // {2}
              dataSelectComponentEntityTypeGuid, // {3}
              selection.Replace( "'", "''" ), // {4}
              order, // {5}
              columnHeaderText, // {6}
              guid // {7}
              ) );
        }

        /// <summary>
        /// Deletes the report field.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public void DeleteReportField( string guid )
        {
            Migration.Sql( string.Format( "DELETE FROM [ReportField] where [Guid] = '{0}'", guid ) );
        }

        #endregion
    }
}