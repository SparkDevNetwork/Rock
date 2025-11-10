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
    public partial class UpdateFileAssetManager : Rock.Migrations.RockMigration
    {
        private const string assetManagerPageGuid = "D2B919E2-3725-438F-8A86-AC87F81A72EB";
        private const string fileAssetManagerBlockTypeGuid = "535500A7-967F-4DA3-8FCA-CB844203CB3D";
        private const string heightModeAttributeGuid = "67ECB409-F5C5-4487-A60B-FD572B99D95B";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateHeightModeDefaultToFull();
            ClearBlockHtml();
            DeleteBlockAttributeValue( assetManagerPageGuid, fileAssetManagerBlockTypeGuid, heightModeAttributeGuid );
        }

        #region Up Methods

        private void UpdateHeightModeDefaultToFull()
        {
            Sql( $@"
UPDATE [Attribute]
SET [DefaultValue] = 'full', [IsDefaultPersistedValueDirty] = 1
WHERE [Guid] = '{heightModeAttributeGuid}'
" );
        }

        private void ClearBlockHtml()
        {
            Sql( $@"
DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{assetManagerPageGuid}');
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{fileAssetManagerBlockTypeGuid}');
UPDATE [Block]
SET [PreHtml] = '',
    [PostHtml] = ''
WHERE [PageId] = @PageId
  AND [BlockTypeId] = @BlockTypeId
  AND ([PreHtml] IS NOT NULL OR [PostHtml] IS NOT NULL);
" );
        }

        private void DeleteBlockAttributeValue( string pageGuid, string blockTypeGuid, string attributeGuid )
        {
            Sql( $@"
DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{pageGuid}');
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}');
DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{attributeGuid}');
DECLARE @BlockId INT = (SELECT TOP 1 [Id] FROM [Block] WHERE [PageId] = @PageId AND [BlockTypeId] = @BlockTypeId);
IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
END
" );
        }

        #endregion
    }
}