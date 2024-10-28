﻿// <copyright>
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
    [MigrationNumber( 214, "1.17.0" )]
    public class UpdateGradingScaleDescriptions : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateGradingScaleDescriptionsUp();
            UpdateCourseAndClassDetailPageMenuMargin();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Updates the description for LearningGradingSystemScales.
        /// </summary>
        private void UpdateGradingScaleDescriptionsUp()
        {
            Sql( @"
-- Update descriptions for Grading Scales.
-- Pass/Fail should have more descriptive values
-- while A - F will be removed.
UPDATE s SET
	[Description] = [updates].[NewDescription]
from (
	SELECT 'Pass' [Name], 'A ""Pass"" grade indicates that the individual has met all the basic requirements and demonstrated sufficient understanding of the material.' NewDescription, 'C07A3227-7188-4D61-AC02-FF6AB8380AAD' [Guid]
	UNION SELECT 'Fail' [Name], 'A ""Fail"" grade indicates that the individual did not meet the minimum requirements or demonstrate an adequate understanding of the material.' NewDescription, 'BD209F2D-22E0-41A9-B425-ED42D515E13B' [Guid]
	UNION SELECT 'A' [Name], '' NewDescription, 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26' [Guid]
	UNION SELECT 'B' [Name], '' NewDescription, 'E8128844-04B0-4772-AB59-55F17645AB7A' [Guid]
	UNION SELECT 'C' [Name], '' NewDescription, 'A99DC539-D363-416F-BDA8-00163D186919' [Guid]
	UNION SELECT 'D' [Name], '' NewDescription, '6E6A61C3-3305-491D-80C6-1C3723468460' [Guid]
	UNION SELECT 'F' [Name], '' NewDescription, '2F7885F5-4DFB-4057-92D7-2684B4542BF7' [Guid]
) [updates]
JOIN [dbo].[LearningGradingSystemScale] s ON s.[Guid] = [updates].[Guid]
" );
        }

        private void UpdateCourseAndClassDetailPageMenuMargin()
        {
            Sql( $@"
DECLARE 
	@blockEntityTypeId INT = (SELECT Id FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}'),

    -- The Attribute Key for the Page Menu 'Template' block setting.
    @templateAttributeKey NVARCHAR(2000) = 'Template',

	-- The 4 pages that use the PageMenu block which should be updated.
	@programCompletionsPageGuid NVARCHAR(40) = '395BE5DD-E524-4B75-A4CA-5A0548645647',
	@coursesPageGuid NVARCHAR(40) = '0E5103B8-EF4A-46C9-8F76-313A259B0A3C',
	@semestersPageGuid NVARCHAR(40) = '0D89CFE6-BA23-4AC0-AF95-1016BAEF734C',
	@currentClassesPageGuid NVARCHAR(40) = '56459D93-32DF-4151-8F6D-003B9AFF0F94';

UPDATE av SET
	[Value] = '<div class=""mb-3"">
    {{% include ''~~/Assets/Lava/PageListAsTabs.lava'' %}}
</div>'
from  [dbo].[Block] b
JOIN [dbo].[Page] p ON p.Id = b.PageId
JOIN [dbo].[BlockType] bt ON bt.[Id] = b.[BlockTypeId]
JOIN [dbo].[AttributeValue] av ON av.EntityId = b.Id
JOIN [dbo].[Attribute] a ON a.Id = av.AttributeId
WHERE a.EntityTypeId = @blockEntityTypeId
	AND a.EntityTypeQualifierColumn = 'BlockTypeId'
	AND a.EntityTypeQualifierValue = CONVERT(NVARCHAR(MAX), bt.Id)
	AND a.[Key] = @templateAttributeKey
	AND p.[Guid] IN (
		@programCompletionsPageGuid,
		@coursesPageGuid,
		@semestersPageGuid,
		@currentClassesPageGuid
	)
	AND av.[Value] = '{{% include ''~~/Assets/Lava/PageListAsTabs.lava'' %}}'
" );
        }
    }
}
