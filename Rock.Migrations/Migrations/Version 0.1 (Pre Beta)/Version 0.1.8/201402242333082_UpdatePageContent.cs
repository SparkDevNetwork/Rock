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
    public partial class UpdatePageContent : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
    // change the 404 message on internal site
    Sql(@"
UPDATE [HtmlContent]
SET
	[Content] = '<div class=""error-wrap"">
    <h1>We Can''t Find That Page</h1>

    <p class=""error-icon info"">
        <i class=""fa fa-question-circle""></i>
    </p>

    <p>
        Sorry, but the page you are looking for can not be found. Check the address of the page
        and see your adminstrator if you still need assistance.
    </p>
</div>'
WHERE
	[Guid] = 'E4E4FE76-C47E-4DD4-AF94-637FF30E3FDC'" );

    // change layout of system config
    Sql( @"
UPDATE [Page]
SET
	[LayoutId] = 13
	,[IconCssClass] = 'fa fa-wrench'
WHERE
	[Guid] = '7BFD28F2-6B90-4191-B7C1-2CDBFA23C3FA'" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
