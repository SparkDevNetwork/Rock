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

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Jobs
{
    [TestClass]
    public class RockCleanupJobTests
    {
        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithLoginsToAccountProtectionProfileMedium()
        {
            var expectedPerson = CreateTestPerson();
            CreateTestUserLogin( expectedPerson.Id );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedPerson.Guid );
                Assert.That.AreEqual( AccountProtectionProfile.Medium, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleInSecurityGroupsWithElevatedSecurityLevelToCorrectAccountProtectionProfile()
        {
            var expectedHighSecurityGroupPerson = CreateTestPerson();
            var expectedLowGroupPerson = CreateTestPerson();

            CreateTestSecurityGroupWithPersonAsMember( expectedHighSecurityGroupPerson.Id, ElevatedSecurityLevel.High );
            CreateTestSecurityGroupWithPersonAsMember( expectedLowGroupPerson.Id, ElevatedSecurityLevel.Low );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedLowGroupPerson.Guid );
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );

                actualPerson = new PersonService( rockContext ).Get( expectedHighSecurityGroupPerson.Guid );
                Assert.That.AreEqual( AccountProtectionProfile.Extreme, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialPersonBankAccountToAccountProtectionProfileHigh()
        {
            var personGuid = Guid.NewGuid();
            var personWithFinancialPersonBankAccount = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( personWithFinancialPersonBankAccount );
                rockContext.SaveChanges();

                personWithFinancialPersonBankAccount = personService.Get( personWithFinancialPersonBankAccount.Id );

                var financialPersonBankAccount = new FinancialPersonBankAccount
                {
                    PersonAliasId = personWithFinancialPersonBankAccount.PrimaryAliasId.Value,
                    AccountNumberMasked = "1111",
                    AccountNumberSecured = "1111-111-11"
                };

                var service = new FinancialPersonBankAccountService( rockContext );
                service.Add( financialPersonBankAccount );
                rockContext.SaveChanges();
            }

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( personGuid );
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialPersonSavedAccountToAccountProtectionProfileHigh()
        {
            var personGuid = Guid.NewGuid();
            var personWithFinancialPersonBankAccount = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( personWithFinancialPersonBankAccount );
                rockContext.SaveChanges();

                personWithFinancialPersonBankAccount = personService.Get( personWithFinancialPersonBankAccount.Id );

                var financialGateway = new FinancialGatewayService( rockContext ).Get( "6432D2D2-32FF-443D-B5B3-FB6C8414C3AD".AsGuid() );
                var creditCardTypeValue = DefinedTypeCache.Get( SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid() ).DefinedValues.OrderBy( a => Guid.NewGuid() ).First().Id;
                var currencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ).Id;
                var definedValueService = new DefinedValueService( rockContext );

                var financialPersonSavedAccount = new FinancialPersonSavedAccount
                {
                    Name = "Test Saved Account",
                    PersonAliasId = personWithFinancialPersonBankAccount.PrimaryAliasId.Value,
                    FinancialGateway = financialGateway,
                    FinancialPaymentDetail = new FinancialPaymentDetail
                    {
                        AccountNumberMasked = "1111",
                        CreditCardTypeValue = definedValueService.Get( creditCardTypeValue ),
                        CurrencyTypeValue = definedValueService.Get( currencyTypeValue ),
                        NameOnCard = "Test User"
                    }
                };

                var service = new FinancialPersonSavedAccountService( rockContext );
                service.Add( financialPersonSavedAccount );
                rockContext.SaveChanges();
            }

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( personGuid );
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialScheduledTransactionToAccountProtectionProfileHigh()
        {
            var personGuid = Guid.NewGuid();
            var personWithFinancialScheduledTransaction = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( personWithFinancialScheduledTransaction );
                rockContext.SaveChanges();

                personWithFinancialScheduledTransaction = personService.Get( personWithFinancialScheduledTransaction.Id );

                var financialScheduledTransaction = new FinancialScheduledTransaction
                {
                    AuthorizedPersonAliasId = personWithFinancialScheduledTransaction.PrimaryAliasId.Value,
                    TransactionFrequencyValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0
                };

                var service = new FinancialScheduledTransactionService( rockContext );
                service.Add( financialScheduledTransaction );
                rockContext.SaveChanges();
            }

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( personGuid );
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithFinancialTransactionToAccountProtectionProfileHigh()
        {
            var personGuid = Guid.NewGuid();
            var personWithFinancialTransaction = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( personWithFinancialTransaction );
                rockContext.SaveChanges();

                personWithFinancialTransaction = personService.Get( personWithFinancialTransaction.Id );

                var financialScheduledTransaction = new FinancialTransaction
                {
                    AuthorizedPersonAliasId = personWithFinancialTransaction.PrimaryAliasId.Value,
                    TransactionTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id
                };

                var service = new FinancialTransactionService( rockContext );
                service.Add( financialScheduledTransaction );
                rockContext.SaveChanges();
            }

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( personGuid );
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        private Person CreateTestPerson()
        {
            var personGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                // We have to manually add the records to the database so that we can ensure the post save methods don't updated the data.
                var createPersonScript = $@"INSERT INTO [Person] (
	                [IsSystem]
	                , [IsDeceased]
	                , [Gender]
	                , [IsEmailActive]
	                , [Guid]
	                , [EmailPreference]
	                , [CommunicationPreference]
	                , [AgeClassification]
	                , [IsLockedAsChild]
	                , [GivingLeaderId]
	                , [AccountProtectionProfile]
                    , [FirstName]
                    , [LastName]
                    , [Email]
                    , [RecordTypeValueId]
                ) VALUES (
	                0 --@IsSystem
	                , 0 --@IsDeceased
	                , 1 --@Gender
	                , 0 --@IsEmailActive
	                , '{personGuid}' --@Guid
	                , 0 --@EmailPreference
	                , 0 --@CommunicationPreference
	                , 0 --@AgeClassification
	                , 0 --@IsLockedAsChild
	                , 0 --@GivingLeaderId
	                , 0 --@AccountProtectionProfile)
                    , 'Test' --[FirstName]
                    , '{personGuid}' --[LastName]
                    , '{personGuid}@test.com' --[Email]
                    , 1 --[RecordTypeValueId]
                )";
                rockContext.Database.ExecuteSqlCommand( createPersonScript );

                return new PersonService( rockContext ).Get( personGuid );
            }
        }

        private UserLogin CreateTestUserLogin( int personId )
        {
            var userLoginGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                // We have to manually add the records to the database so that we can ensure the post save methods don't updated the data.
                var createUserLoginScript = $@"INSERT INTO [UserLogin] (
                    [UserName]
                    , [Guid]
                    , [EntityTypeId]
                    , [Password]
                    , [PersonId]
                ) VALUES (
                    '{userLoginGuid}' -- UserName
                    , '{userLoginGuid}' -- Guid
                    , 27 -- EntityTypeId
                    , '$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C' -- [Password]
                    , {personId} -- [PersonId]
                )";

                rockContext.Database.ExecuteSqlCommand( createUserLoginScript );
                return new UserLoginService( rockContext ).Get( userLoginGuid );
            }
        }

        private void CreateTestSecurityGroupWithPersonAsMember( int personId, ElevatedSecurityLevel securityLevel )
        {
            var securityGroupGuid = Guid.NewGuid();
            var createGroupScript = $@"INSERT INTO [Group] (
	            [IsSystem]
	            , [GroupTypeId]
	            , [Name]
	            , [IsSecurityRole]
	            , [IsActive]
	            , [Order]
	            , [Guid]
	            , [IsPublic]
	            , [IsArchived]
	            , [SchedulingMustMeetRequirements]
	            , [AttendanceRecordRequiredForCheckIn]
	            , [DisableScheduleToolboxAccess]
	            , [DisableScheduling]
	            , [ElevatedSecurityLevel]
            ) VALUES (
	            0 --IsSystem
	            , 1 --GroupTypeId
	            , '{securityLevel} Security Group'--Name
	            , 1--IsSecurityRole
	            , 1--IsActive
	            , 5--Order
	            , '{securityGroupGuid}'--Guid
	            , 0--IsPublic
	            , 0--IsArchived
	            , 0--SchedulingMustMeetRequirements
	            , 0--AttendanceRecordRequiredForCheckIn
	            , 1--DisableScheduleToolboxAccess
	            , 1--DisableScheduling
	            , ${securityLevel.ConvertToInt()}--ElevatedSecurityLevel
            )";

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( createGroupScript );
                var group = new GroupService( rockContext ).Get( securityGroupGuid );

                var createGroupMemeberScript = $@"INSERT INTO [GroupMember] (
	                [IsSystem]
	                , [GroupId]
	                , [PersonId]
	                , [GroupRoleId]
	                , [GroupMemberStatus]
	                , [Guid]
	                , [IsNotified]
	                , [IsArchived]
	                , [CommunicationPreference]
                ) VALUES (
	                0 --IsSystem
	                , {group.Id}--GroupId
	                , {personId}--PersonId
	                , 1 --GroupRoleId
	                , 1 --GroupMemberStatus
	                , '{Guid.NewGuid()}'--Guid
	                , 0--IsNotified
	                , 0--IsArchived
	                , 0--CommunicationPreference
                )";
                rockContext.Database.ExecuteSqlCommand( createGroupMemeberScript );
            }
        }

        private void ExecuteRockCleanupJob()
        {
            var jobContext = new TestJobContext();
            var job = new Rock.Jobs.RockCleanup();

            try
            {
                job.Execute( jobContext );
            }
            catch
            {
                // ignore exceptions we just care about the overall results.
            }
        }
    }
}
