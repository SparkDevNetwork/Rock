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
    public partial class RegistrationAutoPopulateFamily : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationTemplate", "ShowCurrentFamilyMembers", c => c.Boolean(nullable: false));

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Hide Photos", "", 0, "False", "18CE8076-E399-4B5F-973A-1FBFD15CB8BB", "core_checkin_HidePhotos" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Prevent Duplicate Checkin", "", 0, "False", "9915BFF2-9BE3-455F-AE96-89D95C87F966", "core_checkin_PreventDuplicateCheckin" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Prevent Inactive People", "", 0, "False", "C37F80BD-7925-48B5-A93F-CA1C3BAC15EF", "core_checkin_PreventInactivePeople" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "ShowCurrentFamilyMembers");
        }
    }
}
