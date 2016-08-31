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
    public partial class HTMLCacheDuration : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- HTML Cache Duration Attribute
    DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4')

    -- Update existing values
    UPDATE AV
	    SET [Value] = '3600'
    FROM [block] B
	    INNER JOIN [BlockType] BT ON BT.[Id] = B.[BlockTypeId] AND BT.[Guid] = '19B61D65-37E3-459F-A44F-DEF0089118A3'
	    INNER JOIN [AttributeValue] AV ON AV.[AttributeId] = @AttributeId AND AV.[EntityId] = B.[Id]

    -- Add missing values
    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid] )
    SELECT 0, @AttributeId, B.[Id], 0, '3600', NEWID()
    FROM [block] B
	    INNER JOIN [BlockType] BT ON BT.[Id] = B.[BlockTypeId] AND BT.[Guid] = '19B61D65-37E3-459F-A44F-DEF0089118A3'
	    LEFT OUTER JOIN [AttributeValue] AV ON AV.[AttributeId] = @AttributeId AND AV.[EntityId] = B.[Id]
    WHERE AV.[Id] IS NULL
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
