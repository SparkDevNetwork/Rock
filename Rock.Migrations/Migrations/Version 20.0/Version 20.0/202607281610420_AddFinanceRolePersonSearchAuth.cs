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

using Rock.Security;

namespace Rock.Migrations
{
    /// <summary>
    /// Grants Finance Administration and Finance Worker roles ExecuteRead on the
    /// PersonPickerSearch REST action so the person picker returns results on finance
    /// pages (e.g. Transaction Detail) they are already authorized to use.
    /// </summary>
    public partial class AddFinanceRolePersonSearchAuth : Rock.Migrations.RockMigration
    {
        // ControlsController.PersonPickerSearch RestActionGuid.
        private const string PersonPickerSearchRestActionGuid = "1947578D-B28F-4956-8666-DCC8C0F2B945";

        public override void Up()
        {
            // Ensure the RestAction row exists before granting auth on it
            RockMigrationHelper.AddRestAction( PersonPickerSearchRestActionGuid, "Controls", "Rock.Rest.v2.ControlsController" );

            // RSR - Finance Administration
            RockMigrationHelper.AddSecurityAuthForRestAction(
                PersonPickerSearchRestActionGuid,
                0,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Model.SpecialRole.None,
                "C9CB61F3-7D33-4420-BCA5-B7CFBB206122" );

            // RSR - Finance Worker (a.k.a. Finance Users)
            RockMigrationHelper.AddSecurityAuthForRestAction(
                PersonPickerSearchRestActionGuid,
                1,
                Authorization.EXECUTE_READ,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                Model.SpecialRole.None,
                "3B0A8963-171D-4C28-BBCE-5ABBEB652D12" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "C9CB61F3-7D33-4420-BCA5-B7CFBB206122" );
            RockMigrationHelper.DeleteSecurityAuth( "3B0A8963-171D-4C28-BBCE-5ABBEB652D12" );
        }
    }
}
