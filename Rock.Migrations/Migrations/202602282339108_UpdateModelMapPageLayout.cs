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
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateModelMapPageLayout : Rock.Migrations.RockMigration
    {
        private const string ModelMapPageGuid = "67DBC902-BCD5-449E-8A1F-888A3CF9875E";
        private const string FullWorkspaceLayoutGuid = "C2467799-BB45-4251-8EE6-F0BF27201535";
        private const string FullWidthLayoutGuid = "D65F783D-87A9-4CC9-8110-E83466A0EADB";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
DECLARE @PageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{ModelMapPageGuid}' );
DECLARE @LayoutId INT = ( SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '{FullWorkspaceLayoutGuid}' );

IF @PageId IS NOT NULL AND @LayoutId IS NOT NULL
BEGIN
    UPDATE [Page]
    SET [LayoutId] = @LayoutId
    WHERE [Id] = @PageId;
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $@"
DECLARE @PageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{ModelMapPageGuid}' );
DECLARE @LayoutId INT = ( SELECT TOP 1 [Id] FROM [Layout] WHERE [Guid] = '{FullWidthLayoutGuid}' );

IF @PageId IS NOT NULL AND @LayoutId IS NOT NULL
BEGIN
    UPDATE [Page]
    SET [LayoutId] = @LayoutId
    WHERE [Id] = @PageId;
END
" );
        }
    }
}