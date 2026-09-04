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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.Engagement.Interactions;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared;
using Rock.Tests.Shared.Constants;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core.Jobs
{
    [TestClass]
    public class RockCleanupJobTests : DatabaseTestsBase
    {
        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleWithLoginsToAccountProtectionProfileMedium()
        {
            var expectedPerson = CreateTestPerson();
            CreateTestUserLogin( expectedPerson.Id );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedPerson.Guid );
                Assert.AreEqual( AccountProtectionProfile.Medium, actualPerson.AccountProtectionProfile );
            }
        }

        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdatePeopleInSecurityGroupsWithElevatedSecurityLevelToCorrectAccountProtectionProfile()
        {
            var expectedHighSecurityGroupPerson = CreateTestPerson();
            var expectedLowGroupPerson = CreateTestPerson();

            CreateTestSecurityGroupWithPersonAsMember( expectedHighSecurityGroupPerson.Id, ElevatedSecurityLevel.Extreme );
            CreateTestSecurityGroupWithPersonAsMember( expectedLowGroupPerson.Id, ElevatedSecurityLevel.High );

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( expectedLowGroupPerson.Guid );
                Assert.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );

                actualPerson = new PersonService( rockContext ).Get( expectedHighSecurityGroupPerson.Guid );
                Assert.AreEqual( AccountProtectionProfile.Extreme, actualPerson.AccountProtectionProfile );
            }
        }

        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
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
                Assert.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
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
                Assert.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
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
                    TransactionFrequencyValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0,
                    TransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ),
                };

                var service = new FinancialScheduledTransactionService( rockContext );
                service.Add( financialScheduledTransaction );
                rockContext.SaveChanges();
            }

            ExecuteRockCleanupJob();

            using ( var rockContext = new RockContext() )
            {
                var actualPerson = new PersonService( rockContext ).Get( personGuid );
                Assert.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        [Ignore( "Fix required. Tests related to the Account Protection Profile are not returning expected results. [Last Modified by MP]" )]
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
                Assert.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        #region Cleanup Task: Cleanup Person-Related Records

        [TestMethod]
        [IsolatedTestDatabase]
        public void RockCleanup_CleanupPersonRelatedRecords_MissingSearchKeysAreAdded()
        {
            var job = new Rock.Jobs.RockCleanup();
            var rockContext = new RockContext();
            var personSearchKeyService = new PersonSearchKeyService( rockContext );

            var personSearchOptions = PersonService.PersonQueryOptions.AllRecords();
            personSearchOptions.IncludeAnonymousVisitor = false;

            // We should have a search key for every Person except the anonymous
            // visitor person.
            var expectedCount = new PersonService( rockContext ).Queryable( personSearchOptions ).Count();

            // Remove the existing Person Search Keys.
            var sql = @"
DELETE FROM PersonSearchKey
";
            DbService.ExecuteCommand( sql, System.Data.CommandType.Text );

            // Execute the cleanup job and verify that all of the search keys have been regenerated.
            job.PersonCleanup();

            var finalSearchKeyCount = personSearchKeyService.Queryable().Count();

            // Verify that search keys have been created.
            Assert.AreEqual( expectedCount, finalSearchKeyCount, "Invalid search key count." );
        }

        #endregion

        #region Cleanup Task: Cleanup Interaction Sessions

        [TestMethod]
        public void RockCleanup_CleanupInteractionSessions_RemovesSessionsWithNoInteractions()
        {
            var job = new Rock.Jobs.RockCleanup();
            var rockContext = new RockContext();

            // Add an Interaction Session with some associated interactions.
            var complete1GuidString = "2D8F4B1D-2D68-4E4D-83FD-0099DEA3C599";
            CreateEmptyInteractionSession( complete1GuidString,
                RockDateTime.New( 2022, 3, 1, 15, 1, 0, 0 ).Value,
                createInteractions: true,
                rockContext );

            // Add some empty Interaction Sessions.
            var empty1GuidString = "61B0BB6D-2B8C-469C-A697-C00C158E9CD0";
            CreateEmptyInteractionSession( empty1GuidString,
                RockDateTime.New( 2022, 4, 1, 15, 1, 0, 0 ).Value,
                createInteractions: false,
                rockContext );
            var empty2GuidString = "921EDBF9-86FD-4A2B-BF27-4CA5BF331CDB";
            CreateEmptyInteractionSession( empty2GuidString,
                RockDateTime.New( 2022, 4, 2, 15, 1, 0, 0 ).Value,
                createInteractions: false,
                rockContext );

            // Add a final session, because the RockCleanup job is configured to ignore the most recently-created session.
            var empty3GuidString = "011C6D5F-D53A-44F4-A6E6-1D06A298EC01";
            CreateEmptyInteractionSession( empty3GuidString,
                RockDateTime.New( 2022, 4, 3, 15, 1, 0, 0 ).Value,
                createInteractions: true,
                rockContext );

            // Execute the cleanup job and verify that all but the most recent Interaction Session are removed.
            var args = new RockCleanup.RockCleanupActionArgs
            {
                EnabledTaskKeys = new List<string> { RockCleanup.JobTaskKey.InteractionSessionCleanup }
            };

            job.Execute( args );

            // Verify that all but the last Interaction Session has been removed.
            var result = job.Result;
            TestHelper.Log( result );

            Assert.Contains( "2 Unused Interaction Sessions", result );

            InteractionSession interactionSession;
            var interactionSessionService = new InteractionSessionService( rockContext );
            interactionSession = interactionSessionService.Get( empty1GuidString );
            Assert.IsNull( interactionSession, "Empty Interaction session not removed." );
            interactionSession = interactionSessionService.Get( empty2GuidString );
            Assert.IsNull( interactionSession, "Empty Interaction session not removed." );

            interactionSession = interactionSessionService.Get( complete1GuidString );
            Assert.IsNotNull( interactionSession, "Populated Interaction Session removed incorrectly." );
        }

        private int CreateEmptyInteractionSession( string browserSessionGuidString, DateTime firstInteractionDateTime, bool createInteractions, RockContext rockContext )
        {
            var browserSessionGuid = new Guid( browserSessionGuidString );

            var args = new CreatePageViewInteractionActionArgs
            {
                PageIdentifier = SystemGuid.Page.EXCEPTION_LIST,
                BrowserIpAddress = "1.2.3.4",
                BrowserSessionGuid = browserSessionGuid,
            };

            args.ViewDateTime = firstInteractionDateTime;
            var interaction1 = InteractionsDataManager.Instance.CreatePageViewInteraction( args );

            args.ViewDateTime = firstInteractionDateTime.AddMinutes( 1 );
            var interaction2 = InteractionsDataManager.Instance.CreatePageViewInteraction( args );

            args.ViewDateTime = firstInteractionDateTime.AddMinutes( 2 );
            var interaction3 = InteractionsDataManager.Instance.CreatePageViewInteraction( args );

            var sessionId = interaction1.InteractionSessionId ?? 0;

            // Verify that the Interaction session exists.
            var interactionSessionService = new InteractionSessionService( rockContext );
            var interactionSession = interactionSessionService.Get( sessionId );

            Assert.IsNotNull( interactionSession, "Interaction session not found." );

            if ( !createInteractions )
            {
                // The method we have used to add the interaction session also creates an interaction, so
                // remove the interactions from the session if they are not wanted.
                string sql;
                sql = $@"
DELETE FROM [Interaction]
WHERE [InteractionSessionId] = {sessionId}
";

                var recordsAffected = DbService.ExecuteCommand( sql, System.Data.CommandType.Text );
                Assert.AreEqual( 3, recordsAffected, "Test data is invalid." );
            }

            return sessionId;
        }

        #endregion

        #region Create Test Data

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
                    , [GroupTypeId]
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
                    , 1 --GroupTypeId
                )";
                rockContext.Database.ExecuteSqlCommand( createGroupMemeberScript );
            }
        }

        #endregion

        private void ExecuteRockCleanupJob()
        {
            var jobContext = new TestJobContext();
            var job = new Rock.Jobs.RockCleanup();

            try
            {
                job.ExecuteInternal( jobContext );
            }
            catch
            {
                // ignore exceptions we just care about the overall results.
            }
        }
    }
}
