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
    using Rock.Model;
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class TempAddDefaultRestrictedSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Security.GlobalRestrictedDefault",
                "0a326a00-6f00-45b8-bb39-ec6ebe53ce88",
                false,
                true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Security.GlobalRestrictedDefault",
                0,
                Authorization.ADMINISTRATE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "7c01fdb4-7e70-45c5-9f51-ef3b45f5b9f5" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Security.GlobalRestrictedDefault",
                1,
                Authorization.ADMINISTRATE,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "5266bd3b-bcf2-433a-82c7-eff06f430d3c" );

            // Add additional security auths to the Check-in Controller to mimic
            // what it was before becomming restricted by default.
            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "7ed56e29-b07c-46d7-b7ad-1d2b54d6fd8a" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "98b917ed-0d9a-4d0e-bd5d-11b95e96c90f" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "06787034-6808-4b29-983f-8ba30b3773c2" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                false,
                null,
                SpecialRole.AllUsers,
                "183659f2-ac00-420d-8813-6e3c59c59a9c" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "5fccefae-cb5a-48a2-ab50-5f0901df47ca" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "b0be618d-fb3f-4890-8af5-734b27e7bc5d" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "5880ef39-f135-4760-a69f-9568edd8f2f1" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.CheckInController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                false,
                null,
                SpecialRole.AllUsers,
                "735acdc1-1922-484c-baf7-3e8b53abfdcf" );

            // Add additional security auths to the Controls controller
            // to mimic what it was before becomming restricted by default.
            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "63b1a279-5637-4133-89c1-5b51d2a28155" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "0b7c8e65-1094-4032-a49d-d42209a62210" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "951f95b7-81bc-4cd0-95b1-8e0438a59c96" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_READ,
                false,
                null,
                SpecialRole.AllUsers,
                "8ed5b612-4a31-4167-8bf5-79b666a34aef" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                SpecialRole.None,
                "6d303c10-ef8f-4901-8b85-334f4ccec44d" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                SpecialRole.None,
                "313bccf7-fbba-411a-96bc-52814703f612" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "72ec0496-1066-407e-8d96-40490af6ddfb" );

            RockMigrationHelper.AddSecurityAuthForRestControllerByFullClassName( "Rock.Rest.v2.ControlsController",
                int.MaxValue,
                Authorization.EXECUTE_WRITE,
                false,
                null,
                SpecialRole.AllUsers,
                "539f0406-fcad-4f1c-b9b8-c9e24f3ee06a" );

            // Add default EXECUTE permissions for administrators on EntitySearch.
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.EntitySearch",
                Rock.SystemGuid.EntityType.ENTITY_SEARCH,
                true,
                true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EntitySearch",
                0,
                Authorization.EXECUTE,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "83113606-57f3-4262-b92f-f1865780e425" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.EntitySearch",
                1,
                Authorization.EXECUTE,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "3d603aed-38c6-4837-b830-bd47166de794" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // EntitySearch
            RockMigrationHelper.DeleteSecurityAuth( "3d603aed-38c6-4837-b830-bd47166de794" );
            RockMigrationHelper.DeleteSecurityAuth( "83113606-57f3-4262-b92f-f1865780e425" );

            // Controls Controller
            RockMigrationHelper.DeleteSecurityAuth( "539f0406-fcad-4f1c-b9b8-c9e24f3ee06a" );
            RockMigrationHelper.DeleteSecurityAuth( "72ec0496-1066-407e-8d96-40490af6ddfb" );
            RockMigrationHelper.DeleteSecurityAuth( "313bccf7-fbba-411a-96bc-52814703f612" );
            RockMigrationHelper.DeleteSecurityAuth( "6d303c10-ef8f-4901-8b85-334f4ccec44d" );
            RockMigrationHelper.DeleteSecurityAuth( "8ed5b612-4a31-4167-8bf5-79b666a34aef" );
            RockMigrationHelper.DeleteSecurityAuth( "951f95b7-81bc-4cd0-95b1-8e0438a59c96" );
            RockMigrationHelper.DeleteSecurityAuth( "0b7c8e65-1094-4032-a49d-d42209a62210" );
            RockMigrationHelper.DeleteSecurityAuth( "63b1a279-5637-4133-89c1-5b51d2a28155" );

            // Check-in Controller
            RockMigrationHelper.DeleteSecurityAuth( "735acdc1-1922-484c-baf7-3e8b53abfdcf" );
            RockMigrationHelper.DeleteSecurityAuth( "5880ef39-f135-4760-a69f-9568edd8f2f1" );
            RockMigrationHelper.DeleteSecurityAuth( "b0be618d-fb3f-4890-8af5-734b27e7bc5d" );
            RockMigrationHelper.DeleteSecurityAuth( "5fccefae-cb5a-48a2-ab50-5f0901df47ca" );
            RockMigrationHelper.DeleteSecurityAuth( "183659f2-ac00-420d-8813-6e3c59c59a9c" );
            RockMigrationHelper.DeleteSecurityAuth( "06787034-6808-4b29-983f-8ba30b3773c2" );
            RockMigrationHelper.DeleteSecurityAuth( "98b917ed-0d9a-4d0e-bd5d-11b95e96c90f" );
            RockMigrationHelper.DeleteSecurityAuth( "7ed56e29-b07c-46d7-b7ad-1d2b54d6fd8a" );

            // GlobalRestrictedDefault
            RockMigrationHelper.DeleteSecurityAuth( "5266bd3b-bcf2-433a-82c7-eff06f430d3c" );
            RockMigrationHelper.DeleteSecurityAuth( "7c01fdb4-7e70-45c5-9f51-ef3b45f5b9f5" );
        }
    }
}
