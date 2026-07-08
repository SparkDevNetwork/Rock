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
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Enums.Event;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Core.Model
{
    /// <summary>
    /// Tests the capacity validation performed when a registration session is
    /// created or updated, which gates whether a registrant can reserve spots
    /// and proceed to payment.
    /// </summary>
    [TestClass]
    public class RegistrationSessionServiceTests : DatabaseTestsBase
    {
        private string _foreignKey;

        [TestInitialize]
        public void TestInitialize()
        {
            _foreignKey = $"Test {Guid.NewGuid()}";
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Remove in foreign-key-safe order: sessions and registrants reference
            // the registrations, which reference the instance and template.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( $@"
DELETE FROM [RegistrationSession] WHERE [ForeignKey] = '{_foreignKey}';
DELETE FROM [RegistrationRegistrant] WHERE [ForeignKey] = '{_foreignKey}';
DELETE FROM [Registration] WHERE [ForeignKey] = '{_foreignKey}';
DELETE FROM [RegistrationInstance] WHERE [ForeignKey] = '{_foreignKey}';
DELETE FROM [RegistrationTemplate] WHERE [ForeignKey] = '{_foreignKey}';" );
            }
        }

        /// <summary>
        /// A registrant returning to complete payment for spots they already hold
        /// is not blocked, even though the instance is at capacity.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_ExistingRegistrationAtCapacity_CompletesPayment()
        {
            var instance = CreateInstance( maxAttendees: 1 );
            var ownRegistration = CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            var errorMessage = InvokeCreateOrUpdateSession( instance, ownRegistration.Id, registrationCount: 1 );

            Assert.IsTrue( string.IsNullOrEmpty( errorMessage ), $"Expected payment to be allowed but got: {errorMessage}" );
        }

        /// <summary>
        /// A registrant moved off the wait list is over capacity by design, and
        /// must still be able to complete payment for the spot they were granted.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_RegistrationOverCapacity_CompletesPayment()
        {
            var instance = CreateInstance( maxAttendees: 1 );

            // Another registration occupies the only spot.
            CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            // This registration was promoted off the wait list, pushing the
            // instance over capacity.
            var promotedRegistration = CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            var errorMessage = InvokeCreateOrUpdateSession( instance, promotedRegistration.Id, registrationCount: 1 );

            Assert.IsTrue( string.IsNullOrEmpty( errorMessage ), $"Expected payment to be allowed but got: {errorMessage}" );
        }

        /// <summary>
        /// Wait-listed registrants do not occupy capacity, so their presence does
        /// not block an existing registrant from completing payment.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_ExistingRegistrationWithWaitListedBystander_CompletesPayment()
        {
            var instance = CreateInstance( maxAttendees: 1 );
            var ownRegistration = CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            // A separate registration sitting on the wait list should not consume a spot.
            CreateRegistration( instance, activeRegistrants: 0, waitListRegistrants: 1 );

            var errorMessage = InvokeCreateOrUpdateSession( instance, ownRegistration.Id, registrationCount: 1 );

            Assert.IsTrue( string.IsNullOrEmpty( errorMessage ), $"Expected payment to be allowed but got: {errorMessage}" );
        }

        /// <summary>
        /// A brand-new registrant is blocked when the instance is already full.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_NewRegistrantWhenFull_IsBlocked()
        {
            var instance = CreateInstance( maxAttendees: 1 );

            // The only spot is already taken.
            CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            // A new registration has no saved registrants yet, so RegistrationId is null.
            var errorMessage = InvokeCreateOrUpdateSession( instance, registrationId: null, registrationCount: 1 );

            Assert.IsFalse( string.IsNullOrEmpty( errorMessage ), "Expected a new registrant to be blocked when the instance is full." );
        }

        /// <summary>
        /// An existing registration that tries to add a new active registrant
        /// beyond the cap is blocked, since the added spot is genuinely new.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_ExistingRegistrationAddingBeyondCapacity_IsBlocked()
        {
            var instance = CreateInstance( maxAttendees: 1 );
            var ownRegistration = CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            // The registration holds 1 active spot but the session requests 2,
            // so one spot is a genuinely new reservation against a full instance.
            var errorMessage = InvokeCreateOrUpdateSession( instance, ownRegistration.Id, registrationCount: 2 );

            Assert.IsFalse( string.IsNullOrEmpty( errorMessage ), "Expected adding a registrant beyond capacity to be blocked." );
        }

        /// <summary>
        /// When the caller does not credit already-reserved spots (the single
        /// incremental-spot reservation used by the admin registrant detail block),
        /// the full requested count is checked against capacity, so reserving a
        /// spot against a full instance is blocked even for an existing registration.
        /// </summary>
        [TestMethod]
        public void CreateOrUpdateSession_WithoutCrediting_ExistingRegistrationAtCapacity_IsBlocked()
        {
            var instance = CreateInstance( maxAttendees: 1 );
            var ownRegistration = CreateRegistration( instance, activeRegistrants: 1, waitListRegistrants: 0 );

            // creditAlreadyReservedSpots: false mirrors the admin block reserving a
            // single incremental spot, which must still honor the capacity limit.
            var errorMessage = InvokeCreateOrUpdateSession( instance, ownRegistration.Id, registrationCount: 1, creditAlreadyReservedSpots: false );

            Assert.IsFalse( string.IsNullOrEmpty( errorMessage ), "Expected an uncredited single-spot reservation to be blocked when the instance is full." );
        }

        #region Test Data Helpers

        /// <summary>
        /// Creates a registration template and instance with the given capacity.
        /// Setting <paramref name="maxAttendees"/> enables the timeout/session
        /// behavior the capacity check relies on.
        /// </summary>
        private RegistrationInstance CreateInstance( int maxAttendees )
        {
            using ( var rockContext = new RockContext() )
            {
                // Several string columns on RegistrationTemplate are NOT NULL at
                // the database level even though the model does not mark them as
                // required, so they must be populated for the insert to succeed.
                var template = new RegistrationTemplate
                {
                    Name = $"Test Template {_foreignKey}",
                    Description = string.Empty,
                    RegistrantTerm = "Registrant",
                    RegistrationTerm = "Registration",
                    FeeTerm = "Additional Options",
                    DiscountCodeTerm = "Discount Code",
                    ConfirmationFromName = string.Empty,
                    ConfirmationFromEmail = string.Empty,
                    ConfirmationSubject = string.Empty,
                    ConfirmationEmailTemplate = string.Empty,
                    ReminderFromName = string.Empty,
                    ReminderFromEmail = string.Empty,
                    ReminderSubject = string.Empty,
                    ReminderEmailTemplate = string.Empty,
                    SuccessTitle = string.Empty,
                    SuccessText = string.Empty,
                    ForeignKey = _foreignKey
                };
                new RegistrationTemplateService( rockContext ).Add( template );
                rockContext.SaveChanges();

                var instance = new RegistrationInstance
                {
                    Name = $"Test Instance {_foreignKey}",
                    RegistrationTemplateId = template.Id,
                    MaxAttendees = maxAttendees,
                    TimeoutIsEnabled = true,
                    TimeoutLengthMinutes = 30,
                    Details = string.Empty,
                    AdditionalReminderDetails = string.Empty,
                    AdditionalConfirmationDetails = string.Empty,
                    ForeignKey = _foreignKey
                };
                new RegistrationInstanceService( rockContext ).Add( instance );
                rockContext.SaveChanges();

                return instance;
            }
        }

        /// <summary>
        /// Creates a saved registration with the requested number of active and
        /// wait-listed registrants.
        /// </summary>
        private Registration CreateRegistration( RegistrationInstance instance, int activeRegistrants, int waitListRegistrants )
        {
            using ( var rockContext = new RockContext() )
            {
                var registration = new Registration
                {
                    RegistrationInstanceId = instance.Id,
                    RegistrationTemplateId = instance.RegistrationTemplateId,
                    ForeignKey = _foreignKey
                };
                new RegistrationService( rockContext ).Add( registration );
                rockContext.SaveChanges();

                var registrantService = new RegistrationRegistrantService( rockContext );

                for ( var i = 0; i < activeRegistrants; i++ )
                {
                    registrantService.Add( BuildRegistrant( registration, instance.RegistrationTemplateId, onWaitList: false ) );
                }

                for ( var i = 0; i < waitListRegistrants; i++ )
                {
                    registrantService.Add( BuildRegistrant( registration, instance.RegistrationTemplateId, onWaitList: true ) );
                }

                rockContext.SaveChanges();

                return registration;
            }
        }

        private RegistrationRegistrant BuildRegistrant( Registration registration, int registrationTemplateId, bool onWaitList )
        {
            return new RegistrationRegistrant
            {
                RegistrationId = registration.Id,
                RegistrationTemplateId = registrationTemplateId,
                OnWaitList = onWaitList,
                ForeignKey = _foreignKey
            };
        }

        /// <summary>
        /// Invokes the capacity check for a new session and returns the resulting
        /// error message (empty when the session is permitted). Mirrors the
        /// registration entry flow by crediting the registration's already-held
        /// spots unless <paramref name="creditAlreadyReservedSpots"/> is false.
        /// </summary>
        private string InvokeCreateOrUpdateSession( RegistrationInstance instance, int? registrationId, int registrationCount, bool creditAlreadyReservedSpots = true )
        {
            var sessionGuid = Guid.NewGuid();

            RegistrationSessionService.CreateOrUpdateSession(
                sessionGuid,
                () => new RegistrationSession
                {
                    Guid = sessionGuid,
                    RegistrationInstanceId = instance.Id,
                    RegistrationId = registrationId,
                    RegistrationCount = registrationCount,
                    SessionStartDateTime = RockDateTime.Now,
                    SessionStatus = SessionStatus.PaymentPending,
                    RegistrationData = string.Empty,
                    ForeignKey = _foreignKey
                },
                session =>
                {
                    session.RegistrationCount = registrationCount;
                },
                creditAlreadyReservedSpots,
                out var errorMessage );

            return errorMessage;
        }

        #endregion
    }
}
