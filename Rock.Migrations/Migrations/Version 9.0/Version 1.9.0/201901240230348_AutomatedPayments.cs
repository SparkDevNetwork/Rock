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
    public partial class AutomatedPayments : RockMigration
    {
        private const string TransactionApiPath = "api/FinancialTransactions/Process?ignoreRepeatChargeProtection={ignoreRepeatChargeProtection}&ignoreScheduleAdherenceProtection={ignoreScheduleAdherenceProtection}";
        private const string TransactionAllowGuid = "2A0D1438-EB20-4570-A61C-991CF9780B1A";
        private const string TransactionDenyGuid = "3F786410-99D8-41C2-AE2B-15BF73D1EB58";

        private const string ScheduledApiPath = "api/FinancialScheduledTransactions/Process/{scheduledTransactionId}?ignoreRepeatChargeProtection={ignoreRepeatChargeProtection}&ignoreScheduleAdherenceProtection={ignoreScheduleAdherenceProtection}";
        private const string ScheduleAllowGuid = "D23FDBF9-100E-4DF5-9661-A7A5877B7539";
        private const string ScheduleDenyGuid = "546686A6-885C-455A-B3E5-9FE1CDD93A19";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE,
                "Apple Pay",
                "Apple Pay",
                Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY,
                true,
                null,
                string.Empty,
                4 );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE,
                "Android Pay",
                "Android Pay",
                Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY,
                true,
                null,
                string.Empty,
                5 );

            AddColumn( "dbo.FinancialTransactionDetail", "FeeAmount", c => c.Decimal( nullable: false, precision: 18, scale: 2 ) );

            AddColumn( "dbo.FinancialPersonSavedAccount", "GatewayPersonIdentifier", c => c.String( maxLength: 50 ) );
            AddColumn( "dbo.FinancialPersonSavedAccount", "IsSystem", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.FinancialPersonSavedAccount", "IsDefault", c => c.Boolean( nullable: false ) );

            Sql( GetAddAuthSql( TransactionApiPath, TransactionAllowGuid, TransactionDenyGuid ) );
            Sql( GetAddAuthSql( ScheduledApiPath, ScheduleAllowGuid, ScheduleDenyGuid ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( TransactionAllowGuid );
            RockMigrationHelper.DeleteSecurityAuth( TransactionDenyGuid );
            RockMigrationHelper.DeleteSecurityAuth( ScheduleAllowGuid );
            RockMigrationHelper.DeleteSecurityAuth( ScheduleDenyGuid );

            DropColumn( "dbo.FinancialPersonSavedAccount", "IsDefault" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "IsSystem" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "GatewayPersonIdentifier" );

            DropColumn( "dbo.FinancialTransactionDetail", "FeeAmount" );

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY );
        }

        private string GetAddAuthSql( string apiPath, string allowGuid, string denyGuid )
        {
            return string.Format( @"
                DECLARE @apiPath AS NVARCHAR(300) = '{0}';
                DECLARE @apiMethod AS NVARCHAR(10) = 'POST';
                DECLARE @apiID AS NVARCHAR(350) = @apiMethod + @apiPath;
                DECLARE @controllerClassName AS NVARCHAR(100) = 'Rock.Rest.Controllers.FinancialTransactionsController';
                DECLARE @authAllowGuid AS UNIQUEIDENTIFIER = '{1}'; -- Hardcode in migration
                DECLARE @authDenyGuid AS UNIQUEIDENTIFIER = '{2}'; -- Hardcode in migration

                IF NOT EXISTS ( SELECT [Id] FROM [RestAction] WHERE [ApiId] = @apiID ) 
	                INSERT INTO [RestAction] ( 
                        [ControllerId], 
                        [Method], 
                        [ApiId], 
                        [Path], 
                        [Guid] 
                    )
	                SELECT 
                        [Id], -- ControllerId
                        @apiMethod, 
                        @apiID, 
                        @apiPath, 
                        NEWID()
                    FROM [RestController] 
                    WHERE [ClassName] = @controllerClassName;

                IF NOT EXISTS ( SELECT * FROM [Auth] WHERE [Guid] = @authAllowGuid )
	                INSERT INTO [Auth] ( 
                        [EntityTypeId], 
                        [EntityId], 
                        [Order], 
                        [Action], 
                        [AllowOrDeny], 
                        [SpecialRole], 
                        [GroupId], 
                        [Guid] 
                    ) VALUES (
                        (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), -- Rest action
                        (SELECT [Id] FROM [RestAction] WHERE [ApiId] = @apiID), 
                        0, -- Order
                        'View', 
                        'A', -- Allow
                        0, -- Special role
                        (SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'), -- Admins
                        @authAllowGuid
                    );

                IF NOT EXISTS (SELECT * FROM [Auth] WHERE [Guid] = @authDenyGuid)
	                INSERT INTO [Auth] ( 
                        [EntityTypeId], 
                        [EntityId], 
                        [Order], 
                        [Action], 
                        [AllowOrDeny], 
                        [SpecialRole], 
                        [GroupId], 
                        [Guid] 
                    ) VALUES (
                        (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D'), -- Rest action
                        (SELECT [Id] FROM [RestAction] WHERE [ApiId] = @apiID), 
                        1, -- Order
                        'View', 
                        'D', -- Deny
                        1, -- Special Role
                        NULL, -- All users
                        @authDenyGuid
                    );
            ", apiPath, allowGuid, denyGuid );
        }
    }
}
