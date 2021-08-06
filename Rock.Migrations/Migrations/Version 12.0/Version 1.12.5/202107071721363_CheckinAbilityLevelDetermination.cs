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
    public partial class CheckinAbilityLevelDetermination : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Entity: Rock.Model.GroupType Attribute: Ability Level Determination
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "GroupTypePurposeValueId", "142", "Ability Level Determination", "Ability Level Determination", @"", 2006, @"0", "37766623-0784-4D32-8AD2-C141342A4655", "core_checkin_AbilityLevelDetermination");
            
            // Qualifier for attribute: core_checkin_AbilityLevelDetermination
            RockMigrationHelper.UpdateAttributeQualifier( "37766623-0784-4D32-8AD2-C141342A4655", "fieldtype", @"rb", "3CD38113-1763-4310-98EC-1AFCF1A60C32");
            // Qualifier for attribute: core_checkin_AbilityLevelDetermination
            RockMigrationHelper.UpdateAttributeQualifier( "37766623-0784-4D32-8AD2-C141342A4655", "repeatColumns", @"", "D0C72110-F6ED-4F05-A8C7-A50ECDDC05C7");
            // Qualifier for attribute: core_checkin_AbilityLevelDetermination
            RockMigrationHelper.UpdateAttributeQualifier( "37766623-0784-4D32-8AD2-C141342A4655", "values", @"0^Ask,1^Don't Ask", "EE72E367-933F-406E-A8DA-1498AFA35CFE");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "37766623-0784-4D32-8AD2-C141342A4655"); // Rock.Model.GroupType: Ability Level Determination
        }
    }
}
