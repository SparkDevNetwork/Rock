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
    public partial class PersonSearchKeyIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.PersonSearchKey", new[] { "SearchTypeValueId" });

            Sql( @"
INSERT INTO [dbo].[ExceptionLog] (
	[Description]
	,[ExceptionType]
	,CreatedDateTime
	,[Guid]
	)
SELECT CONCAT (
		'PersonSearchKey SearchValue: '
		,k.SearchValue
		,' is longer than the max length of 255 for PersonId '
		,pa.PersonId
		) [Description]
	,'SearchKey over 255' [ExceptionType]
	,GetDate()KD
	,newid() [Guid]
FROM PersonSearchKey k
JOIN PersonAlias pa ON k.PersonAliasId = pa.Id
JOIN Person p ON pa.PersonId = p.Id
WHERE len(k.SearchValue) > 255

DELETE
FROM PersonSearchKey
WHERE len(SearchValue) > 255
" );

            AlterColumn("dbo.PersonSearchKey", "SearchValue", c => c.String(maxLength: 255));
            CreateIndex("dbo.PersonSearchKey", new[] { "SearchTypeValueId", "SearchValue" }, name: "IDX_SearchTypeValueIdSearchValue");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.PersonSearchKey", "IDX_SearchTypeValueIdSearchValue");
            AlterColumn("dbo.PersonSearchKey", "SearchValue", c => c.String());
            CreateIndex("dbo.PersonSearchKey", "SearchTypeValueId");
        }
    }
}
