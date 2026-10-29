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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Links existing document files to their Document so file delivery applies Document security.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 337, "17.8" )]
    public class LinkDocumentBinaryFilesToDocument : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @DocumentEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '7CEFA340-9D98-4B2C-B462-313C61944B6C' ); -- Rock.Model.Document

IF @DocumentEntityTypeId IS NOT NULL
BEGIN
    UPDATE [bf]
    SET [bf].[ParentEntityTypeId] = @DocumentEntityTypeId
        , [bf].[ParentEntityId] = [d].[DocumentId]
    FROM [BinaryFile] AS [bf]
    INNER JOIN
    (
        SELECT [BinaryFileId], MIN( [Id] ) AS [DocumentId]
        FROM [Document]
        GROUP BY [BinaryFileId]
    ) AS [d] ON [d].[BinaryFileId] = [bf].[Id]
    WHERE [bf].[ParentEntityTypeId] IS NULL
        AND [bf].[ParentEntityId] IS NULL;
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
