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
    public partial class Rollup_0325 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumnFollowing_PurposeKey();
            UpdateExternalCommunicationPageSecurity();
            ReorderCheckingManagerLoginZone();
            MobileOnboardPersonBlockUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Following", "PurposeKey");
            MobileOnboardPersonBlockDown();
        }

        /// <summary>
        /// Adds the column "PurposeKey" to the Following table
        /// </summary>
        private void AddColumnFollowing_PurposeKey()
        {
            AddColumn("dbo.Following", "PurposeKey", c => c.String(maxLength: 100));
        }

        /// <summary>
        /// Updates the external communication page security.
        /// </summary>
        private void UpdateExternalCommunicationPageSecurity()
        {
            // Update View Security on the external communication page be just RSR - Admin and Staff
            RockMigrationHelper.AddSecurityAuthForPage(
               Rock.SystemGuid.Page.COMMUNICATION,
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForPage(
                Rock.SystemGuid.Page.COMMUNICATION,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) Rock.Model.SpecialRole.None,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForPage(
                Rock.SystemGuid.Page.COMMUNICATION,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// GJ: Reorder checkin manager login zone
        /// </summary>
        private void ReorderCheckingManagerLoginZone()
        {
            string sql = @"
                -- Update Order Block Back Button on Layout: Full Width, Site: Rock Check-in Manager 
                UPDATE [Block] SET [Order]='0' WHERE ([Guid]='B62CBF17-7FD1-42C8-9E98-00270A34400D')
                -- Update Order Block Campus Context Setter on Layout: Full Width, Site: Rock Check-in Manager 
                UPDATE [Block] SET [Order]='1' WHERE ([Guid]='8B940F43-C38A-4086-80D8-7C33961518E3')";

            Sql( sql );
        }

        public void MobileOnboardPersonBlockUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Security.OnboardPerson", "Onboard Person", "Rock.Blocks.Types.Mobile.Security.OnboardPerson, Rock, Version=1.13.0.5, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.MOBILE_SECURITY_ONBOARD_PERSON );

            RockMigrationHelper.UpdateMobileBlockType( "Onboard Person", "Provides an interface for the user to safely identify themselves and create a login.", "Rock.Blocks.Types.Mobile.Security.OnboardPerson", "Mobile > Security", "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47" );
        }

        public void MobileOnboardPersonBlockDown()
        {
            RockMigrationHelper.DeleteBlockType( "9544EE9E-07C2-4F14-9C93-3B16EBF0CC47" );

            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_SECURITY_ONBOARD_PERSON );
        }

    }
}
