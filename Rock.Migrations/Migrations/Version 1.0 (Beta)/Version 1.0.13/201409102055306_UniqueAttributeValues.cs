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
    
    /// <summary>
    ///
    /// </summary>
    public partial class UniqueAttributeValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn("dbo.AttributeValue", "Order");

            Sql( @"
    -- Delete any attribute values for an attribute that is associated to an entity, but values are not
    DELETE V
    FROM [AttributeValue] V
    INNER JOIN [Attribute] A ON A.[Id] = V.[AttributeId]
    WHERE V.[EntityId] IS NULL
    AND A.[EntityTypeId] IS NOT NULL

    -- Delete any duplicate attribute values
    ;WITH CTE
    AS
    (
	    SELECT 
		    [AttributeId],
		    [EntityId],
		    MAX([Id]) AS [Id],
		    COUNT(*) AS [DupCount]
	    FROM [AttributeValue]
	    GROUP BY [AttributeId], [EntityId]
    )

    DELETE V
    FROM CTE
    INNER JOIN [AttributeValue] V
	    ON V.[AttributeId] = CTE.[AttributeId]
	    AND V.[EntityId] = CTE.[EntityId]
    WHERE V.[Id] <> CTE.[Id]
    AND CTE.[DupCount] > 1
" );

            CreateIndex( "dbo.AttributeValue", new string[] { "AttributeId", "EntityId" }, true, "IX_AttributeIdEntityId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.AttributeValue", "IX_AttributeIdEntityId" );

            AddColumn("dbo.AttributeValue", "Order", c => c.Int());
        }
    }
}
