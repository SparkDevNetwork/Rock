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
    public partial class AddSPForNullingPageViewPageIds : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            @Sql( @"
/*
<doc>
	<summary>
 		This function nulls out the page ids on the [PageView] table for a page that 
		is about to be deleted. This is done in SQL since a single page can have
		thousands of [PageView] records.
	</summary>

	<returns>
		
	</returns>
	<param name=""PageId"" datatype=""int"">Page Id of the page that is about to be deleted</param>
	
	<code>
		EXEC [dbo].[spCore_PageViewNullPageId] 2 
	</code>
</doc>
*/
CREATE PROCEDURE spCore_PageViewNullPageId 
	@PageId int 
AS
BEGIN

	SET NOCOUNT ON;

	UPDATE
		[PageView]
	SET
		[PageId] = null
	WHERE
		[PageId] = @PageId
    
END


" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DROP PROCEDURE spCore_PageViewNullPageId" );
        }
    }
}
