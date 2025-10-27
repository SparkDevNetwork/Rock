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
    public partial class PatchFileAssetManager : Rock.Migrations.RockMigration
    {
        private const string assetManagerPageGuid = "D2B919E2-3725-438F-8A86-AC87F81A72EB";
        private const string webformsAssetManagerBlockTypeGuid = "13165D92-9CCD-4071-8484-3956169CB640";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ClearBlockHtml();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        #region Up Methods

        private void ClearBlockHtml()
        {
            Sql( $@"
DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{assetManagerPageGuid}');
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{webformsAssetManagerBlockTypeGuid}');
UPDATE [Block]
SET [PreHtml] = '',
    [PostHtml] = ''
WHERE [PageId] = @PageId
  AND [BlockTypeId] = @BlockTypeId
  AND ([PreHtml] IS NOT NULL OR [PostHtml] IS NOT NULL);
" );
        }

        #endregion
    }
}
