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
    ///Migration 
    /// </summary>
    [MigrationNumber( 80, "1.9.0" )]
    public class MigrationRollupsForV9_0_5 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            FixDefinedTypeCategoryPersonalityAssessments();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// ED: Fix the duplicate defined type category "Personality Assessments"
        /// </summary>
        private void FixDefinedTypeCategoryPersonalityAssessments()
        {
            Sql( @"
                DECLARE @badGuid UNIQUEIDENTIFIER = (SELECT [Guid] FROM [dbo].[Category] WHERE [Name] = 'Personality Assessments' AND [Guid] <> '6A259E9A-232F-4835-B3F0-B06376A13997')
                DECLARE @badId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = @badGuid)

                UPDATE DefinedType
                SET CategoryId = (
	                SELECT [Id] 
	                FROM [dbo].[Category] 
	                WHERE [Guid] = '6A259E9A-232F-4835-B3F0-B06376A13997')
                WHERE [CategoryId] = @badId

                DELETE FROM [dbo].[Category] WHERE [Guid] = @badGuid" );
        }
    }
}
