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
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 42, "1.7.0" )]
    public class FixShortLinkUrlInteractionChannel : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // This migration is not needed in dev branch as future installs would not have the issue it is fixing
//            Sql( @"
//    DECLARE @OldEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SiteUrlMap' )
//    DECLARE @NewEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PageShortLink' )

//    IF @OldEntityTypeId IS NOT NULL AND @NewEntityTypeId IS NOT NULL
//    BEGIN
//	    UPDATE [InteractionChannel]
//	    SET [ComponentEntityTypeId] = @NewEntityTypeId
//	    WHERE [ComponentEntityTypeId] = @OldEntityTypeId

//	    DELETE [EntityType]
//	    WHERE [Id] = @OldEntityTypeId
//    END
//" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
