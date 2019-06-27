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
    public partial class FundraisingParticipantAddShowAmountSetting : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Fundraising Opportunity Participant:Show Amount
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Amount", "ShowAmount", "", @"Determines if the Amount column should be displayed in the Contributions List.", 8, @"False", "47F89214-9A0D-4C83-B783-BBE09F3F1439" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Fundraising Opportunity Participant:Show Amount
            RockMigrationHelper.DeleteAttribute( "47F89214-9A0D-4C83-B783-BBE09F3F1439" );
        }
    }
}
