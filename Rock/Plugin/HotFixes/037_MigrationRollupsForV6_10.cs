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
//

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 37, "1.6.6" )]
    public class MigrationRollupsForV6_10 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // DT: Fix Rock Instance Ids
            // Moved to core migration: 201711271827181_V7Rollup
            //            Sql( @"
            //    UPDATE [Attribute] SET [Guid] = NEWID()
            //    WHERE [Key] = 'RockInstanceId'
            //    AND [Guid] = '67873C5B-F11A-4F40-8209-730C22A0F6B5'
            //" );

            // MP: Fix up Auth ordering for Auths that have duplicate order values
            // Moved to core migration: 201711271827181_V7Rollup
            //            Sql( @"
            //    UPDATE A SET [Order] = r.OrderRowNum - 1
            //    FROM [Auth] A
            //    INNER JOIN (
            //        SELECT ROW_NUMBER() OVER ( PARTITION BY EntityTypeId,EntityId,[Action] ORDER BY EntityTypeId,EntityId,[Action],[Order],Id ) OrderRowNum
            //        ,*
            //        FROM Auth
            //        WHERE CONCAT ( EntityTypeId ,'_' ,EntityId ,'_' ,[action] ) IN (
            //            SELECT CONCAT ( EntityTypeId ,'_' ,EntityId ,'_' ,[action] )
            //            FROM auth
            //            GROUP BY EntityTypeId ,EntityId ,[action] ,[order]
            //            HAVING count(*) > 1
            //        )
            //    ) r ON r.Id = a.Id
            //    AND r.OrderRowNum - 1 != a.[Order]
            //");

            // SK: Added RSR Finance roles to Contributions Edit Action
            // Moved to core migration: 201711271827181_V7Rollup
//            Sql( @"
//    DECLARE @pageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892')
//    DECLARE @entityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [name] = 'Rock.Model.Page')

//    IF NOT EXISTS ( 
//	    SELECT [Id] FROM [Auth]
//	    WHERE [EntityTypeId] = @entityTypeId
//	    AND [EntityId] = @pageId
//	    AND [Action] = 'Edit' )
//    BEGIN
//	    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId],[Guid] )
//	    VALUES
//	        (@entityTypeId , @pageId, 0, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='628C51A8-4613-43ED-A18D-4A6FB999273E'), '3F58750A-BBA6-4A51-B2C4-0DC2CA06313D'),
//	        (@entityTypeId , @pageId, 1, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='6246A7EF-B7A3-4C8C-B1E4-3FF114B84559'), '4FFB36F4-6D8E-4348-990E-7B66B7D6D92A'),
//	        (@entityTypeId , @pageId, 2, 'Edit', 'A', 0, (SELECT [Id] FROM [Group] WHERE [Guid]='2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9'), 'D5901875-4134-4E97-8C33-B9657FC81CAD'),
//	        (@entityTypeId , @pageId, 3, 'Edit', 'D', 1, NULL, NEWID())
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
