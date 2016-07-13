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
    /// 
    /// </summary>
    [MigrationNumber( 4, "1.5.0" )]
    public class FixGradeRequiredAttribute : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType' )
    DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
    IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
    BEGIN
        UPDATE [Attribute] SET [EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar) 
        WHERE [EntityTypeId] = @GroupTypeEntityTypeId
        AND [EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
        AND [Key] = 'core_checkin_GradeRequired'
    END
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
