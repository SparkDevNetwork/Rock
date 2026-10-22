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
    /// Gives the Document Type entity type staff-only View rules when it has none, so document types without their own rules are no longer viewable by everyone.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 335, "17.8" )]
    public class SecureDocumentTypeEntityTypeView : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/21/26 - JMH

                Document files are authorized against the document, then its document type,
                then rules on the Document Type entity type (EntityId 0), then Global Default,
                which allows All Users to View. Entity type rules close that fall-through
                for every document type without View rules of its own, including ones added later,
                while rules on a document or document type still win.

                The guard skips the insert when the entity type already has any View rule,
                so View security an administrator configured is left alone.

                Reason: Files of custom document types could be downloaded anonymously.
            */
            Sql( @"
DECLARE @DocumentTypeEntityTypeId     INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '18CF366F-46B6-49CA-B557-BCABD6BBD175' ); -- Rock.Model.DocumentType
DECLARE @RSRRockAdministrationGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' ); -- RSR - Rock Administration
DECLARE @StaffWorkersGroupId          INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' ); -- RSR - Staff Workers
DECLARE @StaffLikeWorkersGroupId      INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' ); -- RSR - Staff Like Workers

IF  @DocumentTypeEntityTypeId IS NOT NULL
AND @RSRRockAdministrationGroupId IS NOT NULL
AND NOT EXISTS
(
    SELECT 1
    FROM   [Auth]
    WHERE  [EntityTypeId] = @DocumentTypeEntityTypeId
      AND  [EntityId]     = 0
      AND  [Action]       = 'View'
)
BEGIN
    DECLARE @Now DATETIME = GETDATE();

    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT @DocumentTypeEntityTypeId, 0, [s].[Order], 'View', [s].[AllowOrDeny], [s].[SpecialRole], [s].[GroupId], NULL, [s].[Guid], @Now, @Now
    FROM
    (
        SELECT 0 AS [Order], 'A' AS [AllowOrDeny], 0 AS [SpecialRole], @RSRRockAdministrationGroupId AS [GroupId], CAST( '4C83631D-7569-4E35-B16A-083B3648347E' AS UNIQUEIDENTIFIER ) AS [Guid]
        UNION ALL SELECT 1, 'A', 0, @StaffWorkersGroupId, 'EA2B260F-5153-4B01-9A00-3E20E9B6BD82' WHERE @StaffWorkersGroupId IS NOT NULL
        UNION ALL SELECT 2, 'A', 0, @StaffLikeWorkersGroupId, '9FFDC852-828A-4C82-B24F-F18CC1FE7344' WHERE @StaffLikeWorkersGroupId IS NOT NULL
        UNION ALL SELECT 3, 'D', 1, NULL, '4F7EB6F9-2845-4E7E-AFBC-C685C52972A1'
    ) AS [s];
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
