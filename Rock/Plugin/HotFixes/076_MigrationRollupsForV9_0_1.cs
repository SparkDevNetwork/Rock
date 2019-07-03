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
    [MigrationNumber( 76, "1.9.0" )]
    public class MigrationRollupsForV9_0_1 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            AssessmentBadges();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// The previous AssessmentBadge migration will not work if the attribute value was deleted and recreated.
        /// This will catch those and update the data correctly.
        /// </summary>
        private void AssessmentBadges()
        {
            Sql( @"
                -- Get the badges block attribute ID
                DECLARE @badgesBlockAttribute INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A')

                -- Get the block ID
                DECLARE @entityId INT = (SELECT [Id] FROM [dbo].[Block] WHERE [Guid] = 'F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B')

                -- Get the attribute value guid
                DECLARE @avGuid UNIQUEIDENTIFIER = (select [Guid] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @badgesBlockAttribute AND [EntityId] = @entityId)

                -- Remove the DISC attribute
                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], '6C491A10-E942-4CA5-8D13-ACBC28511714', '') 
                WHERE [Guid] = @avGuid

                -- See if the assessment badge is already included, if so then skip
                IF( ( SELECT COUNT(*) FROM [dbo].[AttributeValue] WHERE [Guid] = @avGuid AND [Value] LIKE '%CCE09793-89F6-4042-A98A-ED38392BCFCC%' ) = 0 )
                BEGIN
                    -- Check if the value is blank or null, if not then preceed new value with a comma
                    IF( ( SELECT COUNT(*) FROM [dbo].[AttributeValue] WHERE [Guid] = @avGuid AND RTRIM(LTRIM(COALESCE([Value], '') )) = '' ) = 0 )
                    BEGIN
                        UPDATE [dbo].[AttributeValue]
                        SET [Value] = [Value] + ', CCE09793-89F6-4042-A98A-ED38392BCFCC'
                        WHERE [Guid] = @avGuid
                    END
                    ELSE BEGIN
                        UPDATE [dbo].[AttributeValue]
                        SET [Value] = [Value] + 'CCE09793-89F6-4042-A98A-ED38392BCFCC'
                        WHERE [Guid] = @avGuid
                    END
                END

                -- Remove any double commas
                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], ',,', ', ')
                WHERE [Guid] = @avGuid

                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], ', ,', ', ')
                WHERE [Guid] = @avGuid" );
        }
        
    }
}
