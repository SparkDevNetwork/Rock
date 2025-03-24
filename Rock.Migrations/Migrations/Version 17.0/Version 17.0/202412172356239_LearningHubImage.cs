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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class LearningHubImage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLearningHubDefaultImage();
            SetDefaultRoleTypeForLMS();
            UpdateParentEntityTypeForAIAutomationAttibutes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void UpdateLearningHubDefaultImage()
        {
            var fileData = BitConverter.ToString( RockMigrationSQL.lms_header_min ).Replace( "-", "" );

            Sql( $@"
-- Add the Banner Image for the External Learn Page.
DECLARE @binaryFileGuid NVARCHAR(40) = '605FD4B7-2DCA-4782-8826-95AAC6C6BAB6';
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

UPDATE [BinaryFile] SET
        [FileName] = 'lms-header-min.jpg',
        [Description] = 'The default image for the Learning Hub.',
        [MimeType] = 'image/png',
        [FileSize] = 193735,
        [Width] = 1140,
        [Height] = 300,
        [ModifiedDateTime] = @now,
        [ContentLastModified] = @now
WHERE [Guid] = @binaryFileGuid

DECLARE @binaryFileId INT = (SELECT [Id] FROM [dbo].[BinaryFile] WHERE [Guid] = @binaryFileGuid);
DECLARE @binaryFileDataGuid NVARCHAR(40) = '85FAE5A9-C28D-41FB-B5AA-5B5BB0499B3C';

DELETE d
from BinaryFile f
join BinaryFileData d on d.Id = f.Id
where f.[Guid] = @binaryFileGuid

INSERT [BinaryFileData] ( [Id], [Guid], [Content], [CreatedDateTime], [ModifiedDateTime] )
SELECT @binaryFileId [Id],  @binaryFileDataGuid [Guid], 0x{fileData} [Content], @now [CreatedDateTime], @now [ModifiedDateTime]
WHERE @binaryFileId IS NOT NULL
" );
        }

        private void SetDefaultRoleTypeForLMS()
        {
            Sql( @"
DECLARE @studentRoleId INT = (
    SELECT Id 
    FROM GroupTypeRole 
    WHERE [Guid] = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2'
);

UPDATE [dbo].[GroupType] SET
	[DefaultGroupRoleId] = @studentRoleId
WHERE [Guid] = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775'

" );
        }

        private void UpdateParentEntityTypeForAIAutomationAttibutes()
        {
            Sql( @"
DECLARE @categoryEntityTypeId NVARCHAR(400) = (
	SELECT Id
	FROM [dbo].[EntityType]
	WHERE [Guid] = '1D68154E-EC76-44C8-9813-7736B27AECF9'
);

UPDATE [Category] SET
	EntityTypeQualifierColumn = 'EntityTypeId',
	EntityTypeQualifierValue = @categoryEntityTypeId
WHERE [Guid] = '571B1191-7F6A-4C9F-8953-1C5B14274F3F'
" );
        }
    }
}
