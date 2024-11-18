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
    [MigrationNumber( 220, "1.17.0" )]
    public class CompletionGradingSystem : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @completeGradingSystemGuid NVARCHAR(40) = '48A4EFCD-6B59-481F-9E09-0C06193A77FC';
DECLARE @completeGradingSystemId INT = (SELECT [Id] FROM [dbo].[LearningGradingSystem] WHERE [Guid] = @completeGradingSystemGuid);

IF @completeGradingSystemId IS NULL
BEGIN
	INSERT [dbo].[LearningGradingSystem] (
		  [Name]
		, [Description]
		, [IsActive]
		, [CreatedDateTime]
		, [ModifiedDateTime]
		, [Guid]
	)
	VALUES ('Completion', 'The Completion grading system focuses solely on whether an activity has been completed. Any completion is considered passing.', 1, @now, @now, @completeGradingSystemGuid)
	SET @completeGradingSystemId = SCOPE_IDENTITY();
END

DECLARE @completeGradingSystemScaleGuid NVARCHAR(40) = '9D17F412-25A8-4638-9CD7-5343017620D6';

IF NOT EXISTS (SELECT 1 FROM [dbo].[LearningGradingSystemScale] WHERE [Guid] = @completeGradingSystemScaleGuid)
BEGIN
	INSERT [dbo].[LearningGradingSystemScale] (
		  [Name]
		, [Description]
		, [ThresholdPercentage]
		, [IsPassing]
		, [Order]
		, [LearningGradingSystemId]
        , [HighlightColor] 
		, [CreatedDateTime]
		, [ModifiedDateTime]
		, [Guid]
	)
	VALUES ('', 'Has been completed.', 0, 1, 1, @completeGradingSystemId, '#34D87D', @now, @now, @completeGradingSystemScaleGuid)
END
" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }
    }
}
