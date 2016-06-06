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
    public partial class AddAttributeKeyIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
	-- Delete any duplicate attributes
    WITH CTE
    AS
    (
        SELECT 
              MIN([Id]) AS [Id]
            , [EntityTypeId]
            , [EntityTypeQualifierColumn] 
            , [EntityTypeQualifierValue]
            , [Key]
            , COUNT(*) AS [DupCount]
        FROM [Attribute]
        GROUP BY [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key]
    )
    DELETE A
    FROM CTE
    INNER JOIN [Attribute] A 
        ON A.[Id] <> CTE.[Id]
        AND A.[EntityTypeId] = CTE.[EntityTypeId]
        AND A.[EntityTypeQualifierColumn] = CTE.[EntityTypeQualifierColumn]
        AND A.[EntityTypeQualifierValue] = CTE.[EntityTypeQualifierValue]
        AND A.[Key] = CTE.[Key]
    WHERE CTE.[DupCount] > 1
" );

            CreateIndex( "Attribute", new string[] { "EntityTypeId", "EntityTypeQualifierColumn", "EntityTypeQualifierValue", "Key" }, true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "Attribute", new string[] { "EntityTypeId", "EntityTypeQualifierColumn", "EntityTypeQualifierValue", "Key" } );
        }
    }
}
