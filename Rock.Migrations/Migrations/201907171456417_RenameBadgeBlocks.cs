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
    public partial class RenameBadgeBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockTypeByGuid( "Badge Detail", "Shows the details of a particular badge.", "~/Blocks/Crm/BadgeTypeDetail.ascx", "CRM", "A79336CD-2265-4E36-B915-CF49956FD689" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Badge List", "Shows a list of all entity badges.", "~/Blocks/Crm/BadgeTypeList.ascx", "CRM", "D8CCD577-2200-44C5-9073-FD16F174D364" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Badge Detail", "Shows the details of a particular person badge.", "~/Blocks/Crm/PersonBadgeDetail.ascx", "CRM", "A79336CD-2265-4E36-B915-CF49956FD689" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Person Badge List", "Shows a list of all person badges.", "~/Blocks/Crm/PersonBadgeList.ascx", "CRM", "D8CCD577-2200-44C5-9073-FD16F174D364" );
        }
    }
}
