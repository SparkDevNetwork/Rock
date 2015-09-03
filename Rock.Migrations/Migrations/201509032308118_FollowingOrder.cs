// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    
    /// <summary>
    ///
    /// </summary>
    public partial class FollowingOrder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FollowingEventType", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.FollowingSuggestionType", "Order", c => c.Int(nullable: false));

            Sql( @"
    WITH CTE 
    AS 
    ( 
	    SELECT 
		    [Id], 
		    ROW_NUMBER() OVER( ORDER BY [Name] ) AS [Order]
	    FROM [FollowingEventType]
    )

    UPDATE E SET [Order] = CTE.[Order] - 1
    FROM CTE
    INNER JOIN [FollowingEventType] E
    	ON E.[Id] = CTE.[Id]
" );

            Sql( @"
    WITH CTE 
    AS 
    ( 
	    SELECT 
		    [Id], 
		    ROW_NUMBER() OVER( ORDER BY [Name] ) AS [Order]
	    FROM [FollowingSuggestionType]
    )

    UPDATE S SET [Order] = CTE.[Order] - 1
    FROM CTE
    INNER JOIN [FollowingSuggestionType] S
    	ON S.[Id] = CTE.[Id]
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FollowingSuggestionType", "Order");
            DropColumn("dbo.FollowingEventType", "Order");
        }
    }
}
