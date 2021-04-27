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
    public partial class CheckinManagerPhase3 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page En Route to Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "En Route", "", Rock.SystemGuid.Page.CHECK_IN_MANAGER_EN_ROUTE, "fa fa-walking" );

            // Remove page display options for Check-in En Route page
            Sql( $@"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 0
                WHERE [Guid] IN ( 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_EN_ROUTE}'
                )" );

            // Add/Update BlockType En Route
            RockMigrationHelper.UpdateBlockType( "En Route", "Lists the people who are checked-in but not yet marked present.", "~/Blocks/CheckIn/Manager/EnRoute.ascx", "Check-in > Manager", Rock.SystemGuid.BlockType.CHECK_IN_MANAGER_EN_ROUTE );

            // Add Block En Route to Page: En Route, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "F6466964-6593-4B20-A49B-D2386D8A260C".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D".AsGuid(), "En Route", "Main", @"", @"", 0, Rock.SystemGuid.Block.CHECKIN_MANAGER_EN_ROUTE );

            Sql( $@"
Update [Page] set [Order] = 
    (select [Order]+1 from [Page] where Guid = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_EN_ROUTE}')
where [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_SETTINGS}'
" );

            // Add/Update HtmlContent for Block: Back Button 
            RockMigrationHelper.UpdateHtmlContentBlock( "B62CBF17-7FD1-42C8-9E98-00270A34400D", @"<a href=""javascript:history.back();"" class=""btn btn-nav-zone""><i class=""fa fa-chevron-left""></i></a>", "26988382-5547-41E4-B737-99F0C079A788" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: En Route, from Page: En Route, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( Rock.SystemGuid.Block.CHECKIN_MANAGER_EN_ROUTE );
     
            // Delete BlockType En Route
            RockMigrationHelper.DeleteBlockType( Rock.SystemGuid.BlockType.CHECK_IN_MANAGER_EN_ROUTE );

            // Delete Page En Route from Site:Rock Check-in Manager
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CHECK_IN_MANAGER_EN_ROUTE );

        }
    }
}
