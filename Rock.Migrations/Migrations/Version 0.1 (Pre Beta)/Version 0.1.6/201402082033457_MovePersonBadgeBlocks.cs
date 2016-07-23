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
    public partial class MovePersonBadgeBlocks : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
                      UPDATE [BlockType]
                          SET 
	                        [Path] = '~/Blocks/Crm/PersonBadgeList.ascx'
	                        , [Category] = 'CRM' 
                          WHERE [Guid] = 'D8CCD577-2200-44C5-9073-FD16F174D364'");

          Sql(@"                 
                        UPDATE [BlockType]
                          SET 
	                        [Path] = '~/Blocks/Crm/PersonBadgeDetail.ascx'
	                        , [Category] = 'CRM' 
                          WHERE [Guid] = 'A79336CD-2265-4E36-B915-CF49956FD689'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
