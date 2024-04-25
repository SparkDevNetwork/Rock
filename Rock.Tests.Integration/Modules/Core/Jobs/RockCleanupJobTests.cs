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
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Jobs
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
                Assert.That.AreEqual( AccountProtectionProfile.Medium, actualPerson.AccountProtectionProfile );
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
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );

                actualPerson = new PersonService( rockContext ).Get( expectedHighSecurityGroupPerson.Guid );
                Assert.That.AreEqual( AccountProtectionProfile.Extreme, actualPerson.AccountProtectionProfile );
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
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
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
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
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
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
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
                Assert.That.AreEqual( AccountProtectionProfile.High, actualPerson.AccountProtectionProfile );
            }
        }

        #region Cleanup Task: Update EventItemOccurrence.NextDateTime

        private Guid testEvent1Guid = new Guid( "1DC19F1B-8FD1-41ED-80AE-6F112AEDBE8A" );
        private Guid testEventOccurrence11Guid = new Guid( "A7FD20AD-0349-4125-8ABF-04437CEA31C0" );
        private Guid testEventOccurrence12Guid = new Guid( "D9958C1F-F485-4147-87C9-A0523216A2B6" );
        private Guid testEventOccurrence13Guid = new Guid( "37CA5E63-9464-4E44-9F9B-378D22DB8300" );
        private Guid testScheduleGuid = new Guid( "8FF2529A-778F-4190-A7D8-6C0506D43D84" );
        private Guid testEvent2Guid = new Guid( "75906B33-84F3-45DD-B79E-B31B1523E573" );
        private Guid testEventOccurrence21Guid = new Guid( "96453FB1-1F8C-4E47-BFC1-C7B9DEC94446" );

        [TestMethod]
        public void RockCleanup_Execute_ShouldUpdateEventItemOccurrences()
        {
            var referenceDate = new DateTime( 2020, 1, 1 );

            // Get the sample data schedule for Saturday 4:30pm.
            var rockContext = new RockContext();

            var scheduleService = new ScheduleService( rockContext );
            var schedule = scheduleService.Get( TestGuids.Schedules.ScheduleSat1630Guid.AsGuid() );

            // Create a new inactive schedule.
            var scheduleInactive = scheduleService.Get( testScheduleGuid );
            if ( scheduleInactive == null )
            {
                scheduleInactive = new Schedule();
                scheduleService.Add( scheduleInactive );
            }
            scheduleInactive.Name = "Test Schedule";
            scheduleInactive.Guid = testScheduleGuid;
            scheduleInactive.IsActive = false;

            rockContext.SaveChanges();

            // Create the Test Events.
            var eventItemService = new EventItemService( rockContext );

            // Test Event 1 (active)
            var testEvent1 = eventItemService.Get( testEvent1Guid );
            if ( testEvent1 != null )
            {
                eventItemService.Delete( testEvent1 );
                rockContext.SaveChanges();
            }

            testEvent1 = new EventItem();
            testEvent1.Guid = testEvent1Guid;
            testEvent1.Name = "Test Event 1";
            eventItemService.Add( testEvent1 );

            // Add an occurrence with a future schedule and no NextDateTime value.
            // When the cleanup task executes, this should be updated to the next occurrence after the reference date.
            var testOccurrence11 = new EventItemOccurrence();

            testOccurrence11.ScheduleId = schedule.Id;
            testOccurrence11.Guid = testEventOccurrence11Guid;
            testOccurrence11.NextStartDateTime = null;
            testEvent1.EventItemOccurrences.Add( testOccurrence11 );

            // Add an occurrence with a NextDateTime that is prior to the reference date.
            // When the cleanup task executes, this should be updated to the next occurrence after the reference date.
            var testOccurrence12 = new EventItemOccurrence();
            testOccurrence12.ScheduleId = schedule.Id;
            testOccurrence12.Guid = testEventOccurrence12Guid;
            testOccurrence12.NextStartDateTime = referenceDate.AddDays( -1 );
            testEvent1.EventItemOccurrences.Add( testOccurrence12 );

            // Add an occurrence with a NextDateTime and an inactive Schedule.
            // When the cleanup task executes, the NextDateTime should be set to null.
            var testOccurrence13 = new EventItemOccurrence();
            testOccurrence13.ScheduleId = scheduleInactive.Id;
            testOccurrence13.Guid = testEventOccurrence13Guid;
            testOccurrence13.NextStartDateTime = referenceDate.AddDays( 7 );
            testEvent1.EventItemOccurrences.Add( testOccurrence13 );

            // Test Event 2 (inactive)
            var testEvent2 = eventItemService.Get( testEvent2Guid );
            if ( testEvent2 != null )
            {
                eventItemService.Delete( testEvent2 );
                rockContext.SaveChanges();
            }

            testEvent2 = new EventItem();
            testEvent2.Guid = testEvent2Guid;
            testEvent2.Name = "Test Event 2";
            testEvent2.IsActive = false;
            eventItemService.Add( testEvent2 );

            // Add an occurrence with a future schedule and a NextDateTime value.
            // When the cleanup task executes, the NextDateTime should be set to null.
            var testOccurrence21 = new EventItemOccurrence();

            testOccurrence21.ScheduleId = schedule.Id;
            testOccurrence21.Guid = testEventOccurrence21Guid;
            testOccurrence21.NextStartDateTime = referenceDate;
            testEvent2.EventItemOccurrences.Add( testOccurrence21 );

            // Save changes without triggering the pre-save, to avoid updating the NextEventDate field.
            rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );

            // Run the cleanup task to verify the results for the reference date.
            RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( referenceDate );

            // Re-run the task to verify that the results are adjusted for the current date.
            RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( RockDateTime.Now );
        }

        private void RunRockCleanupTaskUpdateEventNextOccurrenceDatesAndVerify( DateTime referenceDate )
        {
            // Execute the process to update the Event Occurrence next dates.
            var rockContext = new RockContext();

            Rock.Jobs.RockCleanup.UpdateEventNextOccurrenceDates( rockContext, referenceDate );

            // Verify the results of the cleanup.
            rockContext = new RockContext();

            var eventOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Event 1.1 should be updated to the next occurrence after the reference date.
            var event11 = eventOccurrenceService.Get( testEventOccurrence11Guid );
            Assert.AreEqual( event11.NextStartDateTime, event11.Schedule.GetNextStartDateTime( referenceDate ) );

            // Event 1.2 should be updated to the next occurrence after the reference date.
            var event12 = eventOccurrenceService.Get( testEventOccurrence12Guid );
            Assert.AreEqual( event12.NextStartDateTime, event12.Schedule.GetNextStartDateTime( referenceDate ) );

            // Event 1.3 should be set to null because the schedule is inactive.
            var event13 = eventOccurrenceService.Get( testEventOccurrence13Guid );
            Assert.IsNull( event13.NextStartDateTime );

            // Event 2.1 should be set to null because the Event is inactive.
            var event21 = eventOccurrenceService.Get( testEventOccurrence21Guid );
            Assert.IsNull( event21.NextStartDateTime );
        }

        #endregion

        #region Cleanup Task: ClearCache

        private void CreateCacheFilesTestData( string avatarCachePath, string imageCachePath )
        {
            // Create some test files in the cache.
            var baseDate = RockDateTime.Now;

            CreateTestFile( avatarCachePath + $"/avatar_current_1.txt", lastModifiedTime: baseDate );
            CreateTestFile( avatarCachePath + $"/avatar_current_2.txt", lastModifiedTime: baseDate.AddDays( -6 ) );
            CreateTestFile( avatarCachePath + $"/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );
            CreateTestFile( avatarCachePath + $"/avatar_future.txt", lastModifiedTime: baseDate.AddDays( 1 ) );
            CreateTestFile( avatarCachePath + $"/subdir1/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );
            CreateTestFile( avatarCachePath + $"/subdir2/avatar_old.txt", lastModifiedTime: baseDate.AddDays( -7 ) );

            CreateTestFile( imageCachePath + $"/image_current_1.txt", createdTime: baseDate );
            CreateTestFile( imageCachePath + $"/image_current_2.txt", createdTime: baseDate.AddDays( -6 ) );
            CreateTestFile( imageCachePath + $"/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
            CreateTestFile( imageCachePath + $"/image_future.txt", createdTime: baseDate.AddDays( 1 ) );
            CreateTestFile( imageCachePath + $"/subdir1/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
            CreateTestFile( imageCachePath + $"/subdir2/image_old.txt", createdTime: baseDate.AddDays( -7 ) );
        }

        [TestMethod]
        public void RockCleanup_ClearCache_RemovesExpiredFilesOnly()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "App_Data/Image/Cache";
            var imageCachePath = Path.GetTempPath() + "App_Data/Avatar/Cache";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 7,
                HostName = "test-host"
            };

            _ = job.CleanCachedFileDirectories( args );

            AssertExpectedCacheFiles( avatarCachePath );
            AssertExpectedCacheFiles( imageCachePath );
        }

        [TestMethod]
        public void RockCleanup_ClearCache_EnforcesFileRemoveLimit()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "App_Data/Image/Cache";
            var imageCachePath = Path.GetTempPath() + "App_Data/Avatar/Cache";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 7,
                HostName = "test-host",
                CacheMaximumFilesToRemove = 1
            };

            var avatarFileCount = Directory.GetFiles( avatarCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();
            var imageFileCount = Directory.GetFiles( imageCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();

            _ = job.CleanCachedFileDirectories( args );

            var newAvatarFileCount = Directory.GetFiles( avatarCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();
            var newImageFileCount = Directory.GetFiles( imageCachePath, searchPattern: "*", searchOption: SearchOption.AllDirectories ).Count();

            Assert.AreEqual( avatarFileCount - 1, newAvatarFileCount, "Invalid avatar cache file count." );
            Assert.AreEqual( imageFileCount - 1, newImageFileCount, "Invalid image cache file count." );
        }

        [TestMethod]
        public void RockCleanup_ClearCache_TargetDirectoryMustIncludeAppData()
        {
            // Create some test files in the cache.
            var avatarCachePath = Path.GetTempPath() + "/RockAvatarCacheTest";
            var imageCachePath = Path.GetTempPath() + "/RockImageCacheTest";

            CreateCacheFilesTestData( avatarCachePath, imageCachePath );

            var job = new Rock.Jobs.RockCleanup();
            var args = new Rock.Jobs.RockCleanup.RockCleanupActionArgs
            {
                AvatarCachePath = avatarCachePath,
                ImageCachePath = imageCachePath,
                CacheDurationDays = 7,
                HostName = "test-host"
            };

            try
            {
                _ = job.CleanCachedFileDirectories( args );
            }
            catch ( Exception ex )
            {
                // Verify the exception message.
                // This exception is processed internally when the Job is executed via the Rock application.
                Assert.That.MatchesWildcard( $@"%Path ""%/RockAvatarCacheTest"" does not match the required pattern ""*\App_Data\*\Cache\*""%",
                    ex.Message,
                    ignoreCase: true,
                    ignoreWhiteSpace: true,
                    wildcard: "%" );
            }
        }

        private void AssertExpectedCacheFiles( string cacheDirectory )
        {
            var remainingFiles = Directory.EnumerateFiles( cacheDirectory, searchPattern: "*", searchOption: SearchOption.AllDirectories ).ToList();

            var oldFiles = remainingFiles.Where( f => f.Contains( "_old" ) ).ToList();
            Assert.IsTrue( oldFiles.Count == 0, "Unexpected files found. Old files not removed from cache." );

            var currentFiles = remainingFiles.Where( f => f.Contains( "_current" ) ).ToList();
            Assert.IsTrue( currentFiles.Count == 2, "Expected files not found. Current files removed from cache." );

            var futureFiles = remainingFiles.Where( f => f.Contains( "_future" ) ).ToList();
            Assert.IsTrue( futureFiles.Count == 1, "Expected files not found. Future files removed from cache." );
        }

        private FileInfo CreateTestFile( string filePath, DateTime? createdTime = null, DateTime? lastModifiedTime = null )
        {
            var directory = Path.GetDirectoryName( filePath );
            Directory.CreateDirectory( directory );

            var fileInfo = new FileInfo( filePath );

            using ( var sw = fileInfo.CreateText() )
            {
                sw.WriteLine( Guid.NewGuid().ToString() );
            }

            if ( createdTime != null )
            {
                fileInfo.CreationTime = createdTime.Value;
            }

            if ( lastModifiedTime != null )
            {
                fileInfo.LastWriteTime = lastModifiedTime.Value;
            }

            return fileInfo;
        }

        #endregion

        #region Cleanup Task: Cleanup Person-Related Records

        [TestMethod]
        [IsolatedTestDatabase]
        public void RockCleanup_CleanupPersonRelatedRecords_MissingSearchKeysAreAdded()
        {
            var job = new Rock.Jobs.RockCleanup();
            var rockContext = new RockContext();
            int recordsAffected;

            // Remove the existing Person Search Keys.
            var personSearchKeyService = new PersonSearchKeyService( rockContext );

            var initialSearchKeyCount = personSearchKeyService.Queryable().Count();

            var sql = @"
DELETE FROM PersonSearchKey
";
            DbService.ExecuteCommand( sql, System.Data.CommandType.Text );

            /* Add some faulty data to verify that this will not cause a problem processing the cleanup. */

            // Remove a PersonAlias record.
            sql = $@"
DELETE FROM PersonAlias
WHERE AliasPersonId IN (SELECT Id FROM Person WHERE [Guid] = '{TestGuids.TestPeople.SamHanks}')
";
            recordsAffected = DbService.ExecuteCommand( sql, System.Data.CommandType.Text );
            Assert.AreEqual( 1, recordsAffected, "Test data is invalid." );

            // Modify a PersonAlias record to point to the wrong Person.
            sql = $@"
DECLARE @validPersonId int = ( SELECT [Id] FROM [Person] WHERE [Guid] = '{TestGuids.TestPeople.SamHanks}')
UPDATE PersonAlias
SET [AliasPersonId] = @validPersonId
WHERE PersonId IN ( SELECT Id FROM Person WHERE [Guid] = '{TestGuids.TestPeople.MaddieLowe}' )
";
            recordsAffected = DbService.ExecuteCommand( sql, System.Data.CommandType.Text );
            Assert.AreEqual( 1, recordsAffected, "Test data is invalid." );

            // Execute the cleanup job and verify that all of the search keys have been regenerated.
            job.PersonCleanup();

            var finalSearchKeyCount = personSearchKeyService.Queryable().Count();

            // Verify that the initial search keys have been restored, less the two invalid records.
            Assert.AreEqual( initialSearchKeyCount - 2, finalSearchKeyCount, "Invalid search key count." );
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
