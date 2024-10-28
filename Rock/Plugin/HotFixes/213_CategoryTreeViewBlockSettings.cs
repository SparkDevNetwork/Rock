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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 213, "1.17.0" )]
    public class CategoryTreeViewBlockSettings : Migration
    {
        private const string ALL_CHURCH_CATEGORY_GUID = "5A94E584-35F0-4214-91F1-D72531CC6325";
        private const string CATEGORY_TREEVIEW_BLOCK_TYPE_GUID = "ADE003C7-649B-466A-872B-B8AC952E7841";
        private const string CATEGORY_TREEVIEW_INSTANCE_GUID = "42E90A50-D8EC-4370-B970-83E48518BC26";
        private const string ENTITY_TYPE_FRIENDLY_NAME_ATTRIBUTE_GUID = "07213E2C-C239-47CA-A781-F7A908756DC2";
        private const string GENERAL_CATEGORY_GUID = "4B2D88F5-6E45-4B4B-8776-11118C8E8269";
        private const string PAGE_PARAMETER_KEY_ATTRIBUTE_GUID = "AA057D3E-00CC-42BD-9998-600873356EDB";
        private const string SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID = "845AC4E4-ACD1-40CC-96F6-8D22136C30CC";

        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateCategoryTreeViewBlockSettings();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            RevertCategoryTreeViewBlockSettings();
        }

        private void RevertCategoryTreeViewBlockSettings()
        {
            Sql( $@"
DECLARE @categoryTreeViewBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '{CATEGORY_TREEVIEW_INSTANCE_GUID}');
DECLARE @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID NVARCHAR(40) = '{SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID}';
DECLARE @showOnlyCategoriesAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID);
DECLARE @pageParameterKeyAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '{PAGE_PARAMETER_KEY_ATTRIBUTE_GUID}');

-- Change the value of the Page Parameter Key attribute back to 'CategoryId'
-- where it was changed to 'PrayerRequestId'.
UPDATE av SET
	[Value] = 'CategoryId'
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @pageParameterKeyAttributeId
	AND EntityId = @categoryTreeViewBlockId
	AND av.[Value] = 'PrayerRequestId'

DELETE [dbo].[AttributeValue] WHERE [AttributeId] = @showOnlyCategoriesAttributeId
DELETE [dbo].[Attribute] WHERE [Id] = @showOnlyCategoriesAttributeId

/* Reset the 'General' and 'All Church' categories back to system. */
UPDATE [dbo].[Category] SET
    IsSystem = 1
WHERE [Guid] IN ('{GENERAL_CATEGORY_GUID}', '{ALL_CHURCH_CATEGORY_GUID}')
" );
        }

        private void UpdateCategoryTreeViewBlockSettings()
        {
            Sql( $@"
DECLARE @blockEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}');
DECLARE @booleanFieldTypeId INT = (SELECT [Id] FROM FieldType WHERE [Guid] = '{SystemGuid.FieldType.BOOLEAN}');
DECLARE @categoryTreeViewBlockTypeId INT = (SELECT Id FROM [BlockType] WHERE [Guid] = '{CATEGORY_TREEVIEW_BLOCK_TYPE_GUID}');
DECLARE @categoryTreeViewBlockId INT = (SELECT [Id] FROM [Block] WHERE [Guid] = '{CATEGORY_TREEVIEW_INSTANCE_GUID}');

DECLARE @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID NVARCHAR(40) = '{SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID}';
DECLARE @showOnlyCategoriesAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = @SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID);
DECLARE @pageParameterKeyAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '{PAGE_PARAMETER_KEY_ATTRIBUTE_GUID}');
DECLARE @entityTypeFriendlyNameAttributeId INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = '{ENTITY_TYPE_FRIENDLY_NAME_ATTRIBUTE_GUID}');
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

/* Change the 'General' and 'All Church' categories to non-system. */
UPDATE [dbo].[Category] SET
    IsSystem = 0
WHERE [Guid] IN ('{GENERAL_CATEGORY_GUID}', '{ALL_CHURCH_CATEGORY_GUID}')

/* If the Show Only Categories attribute doesn't yet exist - create it. */
IF @showOnlyCategoriesAttributeId IS NULL
BEGIN
	INSERT [Attribute] ( [IsSystem], [FieldTypeId], [Key], [Name], [Order], [IsGridColumn], [IsMultiValue], [IsRequired], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Description], [Guid] )
	SELECT 
		1 [IsSystem], 
		@booleanFieldTypeId [FieldTypeId], 
		'ShowOnlyCategories' [Key], 
		'Show Only Categories' [Name], 
		12 [Order], 
		0 [IsGridColumn], 
		0 [IsMultiValue], 
		0 [IsRequired], 
		@blockEntityTypeId [EntityTypeId], 
		'BlockTypeId' [EntityTypeQualifierColumn],
		CONVERT(NVARCHAR(40), @categoryTreeViewBlockTypeId) [EntityTypeQualifierValue], 
		'Set to true to show only the categories (rather than the categorized entities) for the configured entity type.' [Description], 
		@SHOW_ONLY_CATEGORIES_ATTRIBUTE_GUID[Guid]

    -- Update the Id of the variable to the newly inserted identity.
	SET @showOnlyCategoriesAttributeId = SCOPE_IDENTITY();
END

/* Add a value to the 'Show Only Categories' AttributeValue for the CategoryTreeView on the Prayer Categories page. */
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[ValueAsBoolean],
	[Guid],
	[IsPersistedValueDirty],
	[CreatedDateTime],
	[ModifiedDateTime]
)
SELECT 0, @showOnlyCategoriesAttributeId, @categoryTreeViewBlockId, 'True', 1, NEWID(), 1, @now, @now
WHERE NOT EXISTS (
	    SELECT *
	    FROM [dbo].[AttributeValue] ex
	    WHERE ex.[AttributeId] = @showOnlyCategoriesAttributeId
		    AND ex.[EntityId] = @categoryTreeViewBlockId
)
AND ISNULL(@showOnlyCategoriesAttributeId, 0) > 0

/*
    Change the value of the Page Parameter Key attribute to anything except CategoryId.
    The Page Parameter Key is for identifying the selected entity who's category should be shown
    in this case it would be a Prayer Request so use PrayerRequestId even though we don't need it.
    If set to 'CategoryId' it conflicts with the behavior of the Category Tree View control.
*/
UPDATE av SET
	[Value] = 'PrayerRequestId'
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @pageParameterKeyAttributeId
	AND EntityId = @categoryTreeViewBlockId
	AND av.[Value] = 'CategoryId'

/* Update the title of the Category Tree View to 'Prayer Request Cateogries'. */
DECLARE @categoryTreeViewTitle NVARCHAR(200) = 'Prayer Request Categories';

IF NOT EXISTS (
    SELECT *
    FROM AttributeValue
    WHERE EntityId = @categoryTreeViewBlockId
        AND AttributeId = @entityTypeFriendlyNameAttributeId
)
BEGIN
	INSERT [AttributeValue] (
		[IsSystem],
		[AttributeId],
		[EntityId],
		[Value],
		[Guid],
		[IsPersistedValueDirty],
		[CreatedDateTime],
		[ModifiedDateTime]
	)
	SELECT 
		0 [IsSystem],
		@entityTypeFriendlyNameAttributeId [AttributeId],
		@categoryTreeViewBlockId [EntityId],
		@categoryTreeViewTitle [Value],
		NEWID() [Guid],
		1 [IsPersistedValueDirty],
		@now [CreatedDateTime],
		@now [ModifiedDateTime]
END
ELSE
BEGIN
	UPDATE av SET
		[Value] = @categoryTreeViewTitle,
		ModifiedDateTime = @now
	FROM [dbo].[AttributeValue] av
	WHERE av.AttributeId = @entityTypeFriendlyNameAttributeId
		AND av.EntityId = @categoryTreeViewBlockId
        AND av.[Value] = ''
END
" );
        }
    }
}
