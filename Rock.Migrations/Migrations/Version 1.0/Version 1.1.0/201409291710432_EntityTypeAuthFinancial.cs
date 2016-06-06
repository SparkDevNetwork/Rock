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
    public partial class EntityTypeAuthFinancial : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Grant Rock.Model.FinancialAccount to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialAccount ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "AA158D2D-D753-4584-A24D-8701034C7C7D" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialAccount ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "14B68AA5-1CEB-45F2-8756-2BF3F2B7B2DF" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialAccount ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "3CC8225C-C697-4774-A704-14948328A628" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialAccount ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "51B22F55-26EC-4B68-94B9-6B4598BD61D5" );


            // Grant Rock.Model.FinancialBatch to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialBatch ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "80DA94D9-AD28-41CA-889F-8CADFC9CBF6B" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialBatch ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "D98B99A2-8995-4241-B155-B0DBC5C73C9F" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialBatch ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "0EFBE419-2C37-4E4D-8C64-77C6C09E56C7" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialBatch ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "EDDEA75C-3BF8-4C7F-B3FB-E6E13B2DACC7" );

            // Grant Rock.Model.FinancialPersonBankAccount to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonBankAccount ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "821B9436-7D2B-40C9-98A2-7A405E79B0BE" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonBankAccount ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "D91432BD-C94C-4CC3-AD2D-414ADD77ADDA" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonBankAccount ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "360C5CD0-241A-49C6-8BAC-6784601AD4A7" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonBankAccount ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "60456963-8044-42CF-81E5-9FE216F59F59" );

            // Grant Rock.Model.FinancialPersonSavedAccount to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonSavedAccount ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "1856A372-D0B7-4996-95A2-28295A745B06" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonSavedAccount ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "0D770437-C1E8-4CF3-9E0E-BB0C72CB1538" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonSavedAccount ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "20F07C25-86CC-41A5-9A53-BC8E4C97CD28" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPersonSavedAccount ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "FA37C4CE-0510-4D8D-9363-DB61C5CB1FA9" );

            // Grant Rock.Model.FinancialPledge to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPledge ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "E9655A34-2459-430F-90FC-491FA49FC8C4" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPledge ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "1A1A4565-1AD5-4BCD-92D1-BB0394CCEC50" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPledge ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "CC51E41C-4C88-4E1F-A18A-5E9D374CB922" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialPledge ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "880B5B23-D463-40F0-9E05-9CE4B466263F" );

            // Grant Rock.Model.FinancialScheduledTransaction to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransaction ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "882E23B4-BFD8-466C-9611-5F923D851624" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransaction ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "75850DEA-E618-4A37-8426-09C0E4A520DA" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransaction ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "4274DD23-C4F6-4DF4-A65F-C8431F4B013B" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransaction ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "19C8047E-776A-40B1-BA6E-CE831627A3E4" );

            // Grant Rock.Model.FinancialScheduledTransactionDetail to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransactionDetail ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "A6A402B6-C08E-4612-B0B6-A9082F67876E" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransactionDetail ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "C4E58A2A-9C96-4AC9-8379-7913A3E1564C" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransactionDetail ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "7A0E20B3-4D93-45AD-9982-E5991578FC43" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialScheduledTransactionDetail ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "D9496B22-2A2E-4D9F-A13D-3A4DC540B47E" );

            // Grant Rock.Model.FinancialTransaction to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransaction ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "49FDDD6B-8705-4AE4-9EAE-5E3B64F71306" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransaction ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "5A2F0D7C-F5DF-4475-B042-162C4FC709D1" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransaction ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "6659C59B-D2E0-460C-A348-1CA5F3899844" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransaction ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "1D641963-34A3-4081-96F4-DC81DA84BFB2" );

            // Grant Rock.Model.FinancialTransactionDetail to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionDetail ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "7A7E3D4F-9469-4014-BF0A-8C547597A516" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionDetail ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "F4011747-BCA1-42FC-9850-E42AA1F82BFD" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionDetail ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "74B36FF1-198E-44CF-A21F-AAF970A29E7D" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionDetail ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "74D5C0D3-0C7A-4021-A474-51482D046940" );

            // Grant Rock.Model.FinancialTransactionImage to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionImage ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "74BB126B-8BD9-4F39-952B-FE00EF10BF71" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionImage ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "C4F88006-0BE7-4179-8EB6-5EA78940A904" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionImage ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "6F1FE3D4-CB9D-46CF-A42B-D0A64141E712" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionImage ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "3AFC5506-DCAD-42FA-A791-D5815663EF44" );

            // Grant Rock.Model.FinancialTransactionRefund to Finance Administrators, Finance Users, and Rock Admins, but not anybody else
            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionRefund ).FullName,
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "7AA9C97B-4143-4AA7-B2AC-58671FB285BC" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionRefund ).FullName,
                1,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_FINANCE_USERS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "BB90697E-1323-44F8-BCB6-D3659CC664E2" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionRefund ).FullName,
                2,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                Rock.Model.SpecialRole.None.ConvertToInt(),
                "28F9FAD0-1AB2-4E25-9FC3-7203F40333CE" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                typeof( Rock.Model.FinancialTransactionRefund ).FullName,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                Rock.Model.SpecialRole.AllUsers.ConvertToInt(),
                "2B105BA2-5129-4570-AC43-0A4A25C840F1" );

            // update new location of checkscanner installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.1.0/checkscanner.exe' where [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
            
            // update new location of jobscheduler installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.1.0/jobscheduler.exe' where [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'" );

            // update new location of statementgenerator installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.1.0/statementgenerator.exe' where [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'" );

            // update new location of checkinclient installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.1.0/checkinclient.exe' where [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'" );

            Sql( @"UPDATE [Attribute] SET [Name] = 'Animate Bars', [Key] = 'AnimateBars' WHERE [key] = 'AnnimateBars'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "2B105BA2-5129-4570-AC43-0A4A25C840F1" );
            RockMigrationHelper.DeleteSecurityAuth( "28F9FAD0-1AB2-4E25-9FC3-7203F40333CE" );
            RockMigrationHelper.DeleteSecurityAuth( "BB90697E-1323-44F8-BCB6-D3659CC664E2" );
            RockMigrationHelper.DeleteSecurityAuth( "7AA9C97B-4143-4AA7-B2AC-58671FB285BC" );
            RockMigrationHelper.DeleteSecurityAuth( "3AFC5506-DCAD-42FA-A791-D5815663EF44" );
            RockMigrationHelper.DeleteSecurityAuth( "6F1FE3D4-CB9D-46CF-A42B-D0A64141E712" );
            RockMigrationHelper.DeleteSecurityAuth( "C4F88006-0BE7-4179-8EB6-5EA78940A904" );
            RockMigrationHelper.DeleteSecurityAuth( "74BB126B-8BD9-4F39-952B-FE00EF10BF71" );
            RockMigrationHelper.DeleteSecurityAuth( "74D5C0D3-0C7A-4021-A474-51482D046940" );
            RockMigrationHelper.DeleteSecurityAuth( "74B36FF1-198E-44CF-A21F-AAF970A29E7D" );
            RockMigrationHelper.DeleteSecurityAuth( "F4011747-BCA1-42FC-9850-E42AA1F82BFD" );
            RockMigrationHelper.DeleteSecurityAuth( "7A7E3D4F-9469-4014-BF0A-8C547597A516" );
            RockMigrationHelper.DeleteSecurityAuth( "1D641963-34A3-4081-96F4-DC81DA84BFB2" );
            RockMigrationHelper.DeleteSecurityAuth( "6659C59B-D2E0-460C-A348-1CA5F3899844" );
            RockMigrationHelper.DeleteSecurityAuth( "5A2F0D7C-F5DF-4475-B042-162C4FC709D1" );
            RockMigrationHelper.DeleteSecurityAuth( "49FDDD6B-8705-4AE4-9EAE-5E3B64F71306" );
            RockMigrationHelper.DeleteSecurityAuth( "D9496B22-2A2E-4D9F-A13D-3A4DC540B47E" );
            RockMigrationHelper.DeleteSecurityAuth( "7A0E20B3-4D93-45AD-9982-E5991578FC43" );
            RockMigrationHelper.DeleteSecurityAuth( "C4E58A2A-9C96-4AC9-8379-7913A3E1564C" );
            RockMigrationHelper.DeleteSecurityAuth( "A6A402B6-C08E-4612-B0B6-A9082F67876E" );
            RockMigrationHelper.DeleteSecurityAuth( "19C8047E-776A-40B1-BA6E-CE831627A3E4" );
            RockMigrationHelper.DeleteSecurityAuth( "4274DD23-C4F6-4DF4-A65F-C8431F4B013B" );
            RockMigrationHelper.DeleteSecurityAuth( "75850DEA-E618-4A37-8426-09C0E4A520DA" );
            RockMigrationHelper.DeleteSecurityAuth( "882E23B4-BFD8-466C-9611-5F923D851624" );
            RockMigrationHelper.DeleteSecurityAuth( "880B5B23-D463-40F0-9E05-9CE4B466263F" );
            RockMigrationHelper.DeleteSecurityAuth( "CC51E41C-4C88-4E1F-A18A-5E9D374CB922" );
            RockMigrationHelper.DeleteSecurityAuth( "1A1A4565-1AD5-4BCD-92D1-BB0394CCEC50" );
            RockMigrationHelper.DeleteSecurityAuth( "E9655A34-2459-430F-90FC-491FA49FC8C4" );
            RockMigrationHelper.DeleteSecurityAuth( "FA37C4CE-0510-4D8D-9363-DB61C5CB1FA9" );
            RockMigrationHelper.DeleteSecurityAuth( "20F07C25-86CC-41A5-9A53-BC8E4C97CD28" );
            RockMigrationHelper.DeleteSecurityAuth( "0D770437-C1E8-4CF3-9E0E-BB0C72CB1538" );
            RockMigrationHelper.DeleteSecurityAuth( "1856A372-D0B7-4996-95A2-28295A745B06" );
            RockMigrationHelper.DeleteSecurityAuth( "60456963-8044-42CF-81E5-9FE216F59F59" );
            RockMigrationHelper.DeleteSecurityAuth( "360C5CD0-241A-49C6-8BAC-6784601AD4A7" );
            RockMigrationHelper.DeleteSecurityAuth( "D91432BD-C94C-4CC3-AD2D-414ADD77ADDA" );
            RockMigrationHelper.DeleteSecurityAuth( "821B9436-7D2B-40C9-98A2-7A405E79B0BE" );
            RockMigrationHelper.DeleteSecurityAuth( "EDDEA75C-3BF8-4C7F-B3FB-E6E13B2DACC7" );
            RockMigrationHelper.DeleteSecurityAuth( "0EFBE419-2C37-4E4D-8C64-77C6C09E56C7" );
            RockMigrationHelper.DeleteSecurityAuth( "D98B99A2-8995-4241-B155-B0DBC5C73C9F" );
            RockMigrationHelper.DeleteSecurityAuth( "80DA94D9-AD28-41CA-889F-8CADFC9CBF6B" );
            RockMigrationHelper.DeleteSecurityAuth( "51B22F55-26EC-4B68-94B9-6B4598BD61D5" );
            RockMigrationHelper.DeleteSecurityAuth( "3CC8225C-C697-4774-A704-14948328A628" );
            RockMigrationHelper.DeleteSecurityAuth( "14B68AA5-1CEB-45F2-8756-2BF3F2B7B2DF" );
            RockMigrationHelper.DeleteSecurityAuth( "AA158D2D-D753-4584-A24D-8701034C7C7D" );
        }
    }
}
