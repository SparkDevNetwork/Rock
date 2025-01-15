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
    /// <summary>
    ///
    /// </summary>
    public partial class TempAddDefaultSecurityToFinance : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Default Security for FinancialPaymentDetail
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "02476fa2-6ba0-4364-833f-cabd7931a0c8" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a0ce3e2e-684a-430d-bc85-b5517bdcd93d" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "a55f551c-8591-4dfc-8018-9eb5eee939cc" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "971dc09b-f934-4238-9d83-e4754ae9343b" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "33ea051b-a040-495d-92a2-7bd0aaeac5bf" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialPaymentDetail",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "f9370185-7790-40ae-b087-4e6e03745c13" );

            // Default Security for FinancialStatementTemplate
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "010533a5-0446-4ce2-8d4e-d3853966a9ff" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a255c49f-97cf-4ba4-9c09-1ef38a44aa4e" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "f664b1af-3e89-4bdb-9539-8951b03e8b89" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "443b13b9-55c4-4183-ab68-73a100f45777" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "a2ce65ff-89e8-4f9c-8c8c-8ae72dbbd910" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialStatementTemplate",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "3a860c98-df3e-419e-96e3-4e898f0da07a" );

            // Default Security for FinancialTransactionAlertType
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "fa22c42a-1ade-40a7-8b98-8ab02f8ba009" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "be113be9-b026-4fa2-843b-324637aece50" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "20f45bb5-6960-4749-a9c7-69f60c34ea75" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "ff39c885-909c-43e4-ac7a-d8e1f862b486" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "dad3a6b4-cac5-4eed-a79b-3dd518b65fa9" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlertType",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "b1b8a1ce-fdf0-45b2-a5c4-6d19ed9b3c66" );

            // Default Security for FinancialTransactionAlert
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "c7579205-b54b-4429-9f9b-7bf30ef88532" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "78b1c098-3498-449f-93cb-f83bd896fadf" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                0,
                "247e9bb5-4756-43f6-927f-2499f1eb1549" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) Model.SpecialRole.AllUsers,
                "44d46714-855d-4c2a-9c4f-2a05d07e98e7" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                0,
                "6e530bba-8643-4917-bebb-eda7bb7e89a2" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionAlert",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "e6c1e0d9-38b0-47dd-a6c8-4017eeeceb6d" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "e6c1e0d9-38b0-47dd-a6c8-4017eeeceb6d" );
            RockMigrationHelper.DeleteSecurityAuth( "6e530bba-8643-4917-bebb-eda7bb7e89a2" );
            RockMigrationHelper.DeleteSecurityAuth( "44d46714-855d-4c2a-9c4f-2a05d07e98e7" );
            RockMigrationHelper.DeleteSecurityAuth( "247e9bb5-4756-43f6-927f-2499f1eb1549" );
            RockMigrationHelper.DeleteSecurityAuth( "78b1c098-3498-449f-93cb-f83bd896fadf" );
            RockMigrationHelper.DeleteSecurityAuth( "c7579205-b54b-4429-9f9b-7bf30ef88532" );

            RockMigrationHelper.DeleteSecurityAuth( "b1b8a1ce-fdf0-45b2-a5c4-6d19ed9b3c66" );
            RockMigrationHelper.DeleteSecurityAuth( "dad3a6b4-cac5-4eed-a79b-3dd518b65fa9" );
            RockMigrationHelper.DeleteSecurityAuth( "ff39c885-909c-43e4-ac7a-d8e1f862b486" );
            RockMigrationHelper.DeleteSecurityAuth( "20f45bb5-6960-4749-a9c7-69f60c34ea75" );
            RockMigrationHelper.DeleteSecurityAuth( "be113be9-b026-4fa2-843b-324637aece50" );
            RockMigrationHelper.DeleteSecurityAuth( "fa22c42a-1ade-40a7-8b98-8ab02f8ba009" );

            RockMigrationHelper.DeleteSecurityAuth( "3a860c98-df3e-419e-96e3-4e898f0da07a" );
            RockMigrationHelper.DeleteSecurityAuth( "a2ce65ff-89e8-4f9c-8c8c-8ae72dbbd910" );
            RockMigrationHelper.DeleteSecurityAuth( "443b13b9-55c4-4183-ab68-73a100f45777" );
            RockMigrationHelper.DeleteSecurityAuth( "f664b1af-3e89-4bdb-9539-8951b03e8b89" );
            RockMigrationHelper.DeleteSecurityAuth( "a255c49f-97cf-4ba4-9c09-1ef38a44aa4e" );
            RockMigrationHelper.DeleteSecurityAuth( "010533a5-0446-4ce2-8d4e-d3853966a9ff" );

            RockMigrationHelper.DeleteSecurityAuth( "f9370185-7790-40ae-b087-4e6e03745c13" );
            RockMigrationHelper.DeleteSecurityAuth( "33ea051b-a040-495d-92a2-7bd0aaeac5bf" );
            RockMigrationHelper.DeleteSecurityAuth( "971dc09b-f934-4238-9d83-e4754ae9343b" );
            RockMigrationHelper.DeleteSecurityAuth( "a55f551c-8591-4dfc-8018-9eb5eee939cc" );
            RockMigrationHelper.DeleteSecurityAuth( "a0ce3e2e-684a-430d-bc85-b5517bdcd93d" );
            RockMigrationHelper.DeleteSecurityAuth( "02476fa2-6ba0-4364-833f-cabd7931a0c8" );
        }
    }
}
