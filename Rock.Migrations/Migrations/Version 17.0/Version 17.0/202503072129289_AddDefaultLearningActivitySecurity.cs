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

    using Rock.Model;
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class AddDefaultLearningActivitySecurity : Rock.Migrations.RockMigration
    {
        //private const string RSR_STAFF_GUID = "2C112948-FF4C-46E7-981A-0257681EADF4";
        //private const string RSR_STAFF_LIKE_GUID = "300BA2C8-49A3-44BA-A82A-82E3FD8C3745";
        private const string RSR_LMS_ADMINISTRATION_GUID = "5E0E02A9-F16B-437E-A9D9-C3D9D6AFABB0";
        private const string RSR_LMS_WORKER_GUID = "B5481A0E-52E3-4D7B-93B5-A8F5C908DC67";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // LMS Administration: View, Edit and Administrate.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.VIEW,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "a738683e-109b-432d-ad4c-0abd79b31946" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.EDIT,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "7daa947c-2824-4516-acba-e937319f0b81" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.ADMINISTRATE,
                true,
                RSR_LMS_ADMINISTRATION_GUID,
                0,
                "53f5c4fc-8c75-4720-aab3-775c7f4936ee" );

            // Rock Administration
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "748a58cf-0153-4d89-9345-8a5f2b0702c4" );

            // LMS Worker: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.VIEW,
                true,
                RSR_LMS_WORKER_GUID,
                0,
                "783f0ed1-4696-406d-ade4-3a5bc3e77aff" );

            // Staff: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                0,
                "9672a5c8-b32c-42a2-b878-49541f60df66" );

            // Staff-Like Workers: View.
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                0,
                "1081ea2d-e351-4d49-9151-de70c7c2031a" );

            // All Users: View [Deny].
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Model.LearningActivity",
                1,
                Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "34787327-b6c7-41e2-a5ff-7d59ba0869fa" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "34787327-b6c7-41e2-a5ff-7d59ba0869fa" );
            RockMigrationHelper.DeleteSecurityAuth( "1081ea2d-e351-4d49-9151-de70c7c2031a" );
            RockMigrationHelper.DeleteSecurityAuth( "9672a5c8-b32c-42a2-b878-49541f60df66" );
            RockMigrationHelper.DeleteSecurityAuth( "783f0ed1-4696-406d-ade4-3a5bc3e77aff" );
            RockMigrationHelper.DeleteSecurityAuth( "748a58cf-0153-4d89-9345-8a5f2b0702c4" );
            RockMigrationHelper.DeleteSecurityAuth( "53f5c4fc-8c75-4720-aab3-775c7f4936ee" );
            RockMigrationHelper.DeleteSecurityAuth( "7daa947c-2824-4516-acba-e937319f0b81" );
            RockMigrationHelper.DeleteSecurityAuth( "a738683e-109b-432d-ad4c-0abd79b31946" );
        }
    }
}
