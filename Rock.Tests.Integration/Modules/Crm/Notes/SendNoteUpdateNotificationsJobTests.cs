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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Jobs;
using Rock.Logging;
using Rock.Tests.Integration.Modules.Communications.Transport;
using Rock.Tests.Integration.TestData.Core;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;
using static Rock.Jobs.SendNoteNotifications;

namespace Rock.Tests.Integration.Modules.Crm.Notes
{
    /// <summary>
    /// Integration tests for the "Send Note Update Notifications" Job.
    /// </summary>
    [TestClass]
    [TestCategory( "Core.Crm.Notes" )]
    [TestCategory( "Core.Jobs" )]
    public class SendNoteUpdateNotificationsJobTests : DatabaseTestsBase
    {
        #region Initialization

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            CreateNotesTestData();
        }

        #endregion

        #region Configuration

        private DateTime _TestPeriodStartDate = new DateTime( 2019, 1, 1 );

        #endregion

        #region Tests

        private const string _noteWatchTestGuid = "afb62b37-4b56-4068-912e-d978be38a5a7";

        private const string _noteTypeGeneralTestGuid = "00cdb542-1ab5-4313-91fb-1cafc6fe3419";
        private const string _noteTypePrivateTestGuid = "8ca0bf49-a95e-459c-82e0-46b65e4249a2";

        [TestMethod]
        public void SendNoteUpdateNotifications_WithInvalidEmailTemplate_DoesNotProcess()
        {
            var settings = new SendNoteNotifications.SendNoteNotificationsJobSettings
            {
                // Ensure no system email template is specified.
                SystemCommunicationTemplateGuid = null,
            };

            ExecuteSendNoteNotificationsJob( settings, out var sendResults, out var events );

            // Verify that the error message is as expected.
            var message = events.FirstOrDefault( m => m.Level <= RockLogLevel.Warning );

            Assert.IsFalse( sendResults.Any(), "Processing messages found but not expected." );
        }

        /// <summary>
        /// This test documents the current behavior.
        /// However, it seems more likely that the author should receive notifications for replies to their watched note.
        /// </summary>
        [TestMethod]
        public void SendNoteUpdateNotifications_WatchForSpecificNote_DoesNotSendNotificationToAuthor()
        {
            // Execute the job to clear out any existing notifications.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out _, logEvents: out _ );

            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var person1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BrianJones );
            var personStaff1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personStaff2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var generalNoteType = NoteTypeCache.Get( _noteTypeGeneralTestGuid.AsGuid() );

            // Add Note Watch 1: Note Type "General" Watched by Staff1.
            var watch1 = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchingPerson( personStaff1.PrimaryAliasId )
                .ForWatchedNoteType( generalNoteType.Id );
            noteManager.SaveNoteWatch( watch1, rockContext );

            // Add Note Watch 2: Note Type "General" Watched by Staff2.
            var watch2 = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchingPerson( personStaff2.PrimaryAliasId )
                .ForWatchedNoteType( generalNoteType.Id );
            noteManager.SaveNoteWatch( watch2, rockContext );

            // Add Note: Note Type "General" for Person1 by Staff1.
            var note1 = noteManager.NewNote( "2f2e7215-90ee-4971-8078-94d2b11fbb78".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff1.PrimaryAliasId );
            noteManager.SaveNote( note1, rockContext );

            // Execute the job and get the results.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out var sendResults, logEvents: out _ );

            // Verify that an email is sent to Staff2, but not Staff1 as the author of the message.
            Assert.That.AreEqual( 1, sendResults.Count );

            var recipientEmail = sendResults.First()?.EmailMessage.To.First()?.Address;

            Assert.That.AreEqual( personStaff2.Email, recipientEmail, "Incorrect recipient." );

            var messageText = sendResults.First()?.RockMessage.Message;
            Assert.That.IsTrue( messageText.Contains( note1.Guid.ToString() ), "Reference to Note1 expected but not found." );
        }

        [TestMethod]
        public void SendNoteUpdateNotifications_WatchForSpecificNoteType_SendsNotificationForAllMatchedNotes()
        {
            // Execute the job to clear out any existing notifications.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out _, logEvents: out _ );

            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var person1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BrianJones );
            var person2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.ThomasMiller );
            var personStaff1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personStaff2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var generalNoteType = NoteTypeCache.Get( _noteTypeGeneralTestGuid.AsGuid() );
            var privateNoteType = NoteTypeCache.Get( _noteTypePrivateTestGuid.AsGuid() );

            // Add Note: Note Type "General" for Person1 by Staff2.
            var note1 = noteManager.NewNote( "f9d9b466-4685-4ce6-a03f-c93c402a282b".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note1, rockContext );

            // Add Note: Note Type "General" for Person2 by Staff2.
            var note2 = noteManager.NewNote( "d175728f-f76a-463e-86c6-825d5206b8a2".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person2.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note2, rockContext );

            // Add Note: Note Type "Private" For Person1 By Staff2.
            var note3 = noteManager.NewNote( "b38b0723-6fe7-4658-b64c-b50d6988dfcd".AsGuid(), privateNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note3, rockContext );

            // Add Note Watch: Note Type "General" Watched by Staff1.
            var watch = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchingPerson( personStaff1.PrimaryAliasId )
                .ForWatchedNoteType( generalNoteType.Id );
            noteManager.SaveNoteWatch( watch, rockContext );

            // Execute the job and get the results.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out var sendResults, logEvents: out _ );

            // Verify that the message contains notifications for Note Type "General" but excludes Note Type "Private".
            var message = sendResults.First().RockMessage;
            var messageText = message.Message;

            Assert.That.IsTrue( messageText.Contains( note1.Guid.ToString() ), "Reference to Note1 expected but not found." );
            Assert.That.IsTrue( messageText.Contains( note2.Guid.ToString() ), "Reference to Note2 expected but not found." );
            Assert.That.IsFalse( messageText.Contains( note3.Guid.ToString() ), "Reference to Note3 found but not expected." );
        }

        [TestMethod]
        public void SendNoteUpdateNotifications_WatchForSpecificNote_SendsNotificationForNoteReplyOfAnyType()
        {
            // Execute the job to clear out any existing notifications.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out _, logEvents: out _ );

            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var person1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BrianJones );
            var personStaff1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personStaff2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var generalNoteType = NoteTypeCache.Get( _noteTypeGeneralTestGuid.AsGuid() );

            // Add Note1: Note Type "General" for Person1 by Staff2.
            var note1 = noteManager.NewNote( "e62cc9f8-cd76-4214-ae1d-c09266f86878".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note1, rockContext );

            // Add Note2: Note Type "General" for Person1 by Staff2.
            var note2 = noteManager.NewNote( "a43a6205-ec6e-487e-aec0-76c9fcc91b87".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note2, rockContext );

            // Add Note Watch: Note1 Watched by Staff1.
            var watch = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchedNote( note1.Id )
                .ForWatchingPerson( personStaff1.PrimaryAliasId );
            noteManager.SaveNoteWatch( watch, rockContext );

            // Execute the job and get the results.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out var sendResults, logEvents: out _ );

            // Verify that the message contains notifications for Note1, but not Note2
            var message = sendResults.First().RockMessage;
            var messageText = message.Message;

            Assert.That.IsTrue( messageText.Contains( note1.Guid.ToString() ), "Reference to Note1 expected but not found." );
            Assert.That.IsFalse( messageText.Contains( note2.Guid.ToString() ), "Reference to Note2 found but not expected." );
        }

        [TestMethod]
        public void SendNoteUpdateNotifications_WatchForSpecificEntityType_NotifiesNewNoteForAnyEntityOfSpecifiedType()
        {
            // Execute the job to clear out any existing notifications.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out _, logEvents: out _ );

            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var person1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BrianJones );
            var personStaff1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personStaff2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var generalNoteType = NoteTypeCache.Get( _noteTypeGeneralTestGuid.AsGuid() );
            var workflowNoteType = NoteTypeCache.Get( SystemGuid.NoteType.WORKFLOW_CHANGE_LOG_NOTE.AsGuid() );
            var assessmentWorkflowType = WorkflowTypeCache.Get( SystemGuid.WorkflowType.REQUEST_ASSESSMENT.AsGuid() );
            var workflowTypeEntityType = EntityTypeCache.Get( SystemGuid.EntityType.WORKFLOW_TYPE );

            // Add Note Watch: Entity Type "Workflow Type" Watched by Staff1.
            var watch = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchingPerson( personStaff1.PrimaryAliasId )
                .ForWatchedEntityType( workflowTypeEntityType.Id );
            noteManager.SaveNoteWatch( watch, rockContext );

            // Add Note: Note Type "Workflow Type Change Log" for Workflow Type "Request Assessment" by Staff2.
            var note1 = noteManager.NewNote( "670345a6-a95a-42e4-a8ac-fee8ccaa6fec".AsGuid(), workflowNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( assessmentWorkflowType.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note1, rockContext );

            // Add Note: Note Type "General" for Person1 by Staff2.
            var note2 = noteManager.NewNote( "f9d9b466-4685-4ce6-a03f-c93c402a282b".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note2, rockContext );

            // Execute the job and get the results.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out var sendResults, logEvents: out _ );

            // Verify that the message contains notifications for new Workflow Type Notes, but excludes Person Notes.
            var message = sendResults.First().RockMessage;
            var messageText = message.Message;

            Assert.That.IsTrue( messageText.Contains( note1.Guid.ToString() ), "Reference to Note1 expected but not found." );
            Assert.That.IsFalse( messageText.Contains( note2.Guid.ToString() ), "Reference to Note2 found but not expected." );
        }

        [TestMethod]
        public void SendNoteUpdateNotifications_ReplyToWatchedNoteHavingDifferentNoteType_GeneratesNotification()
        {
            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var person1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BrianJones );
            var personStaff1 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personStaff2 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );
            var personStaff3 = TestDataHelper.GetTestPerson( TestGuids.TestPeople.CindyDecker );

            var generalNoteType = NoteTypeCache.Get( _noteTypeGeneralTestGuid.AsGuid() );
            var privateNoteType = NoteTypeCache.Get( _noteTypePrivateTestGuid.AsGuid() );

            // Add Note1: Note Type "General" for Person1 by Staff1.
            var note1 = noteManager.NewNote( "f9d9b466-4685-4ce6-a03f-c93c402a282b".AsGuid(), generalNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff1.PrimaryAliasId );
            noteManager.SaveNote( note1, rockContext );

            // Add Note Watch: Note1 Watched by Staff3.
            var watch = noteManager.NewNoteWatch( _noteWatchTestGuid.AsGuid() )
                .ForWatchedNote( note1.Id )
                .ForWatchingPerson( personStaff3.PrimaryAliasId );
            noteManager.SaveNoteWatch( watch, rockContext );

            // Execute the job to clear pending notifications.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out _, logEvents: out _ );

            // Add Note11: Reply to Note1, Note Type "Private" by Staff2.
            var note11 = noteManager.NewNote( "b38b0723-6fe7-4658-b64c-b50d6988dfcd".AsGuid(), privateNoteType.Id )
                .SetTestTextAndCaption()
                .ForEntity( person1.Id )
                .AsReplyTo( note1.Id )
                .SetEditActionAuditInfo( RockDateTime.Now, personStaff2.PrimaryAliasId );
            noteManager.SaveNote( note11, rockContext );

            // Execute the job and get the results.
            ExecuteSendNoteNotificationsJob( settings: null, sendResults: out var sendResults, logEvents: out _ );

            // Verify that the message contains a notification for the Reply.
            var message = sendResults.First().RockMessage;
            var messageText = message.Message;

            Assert.That.IsTrue( messageText.Contains( note11.Guid.ToString() ), "Reference to Note11 expected but not found." );
        }

        #endregion

        #region Support Methods

        private void ExecuteSendNoteNotificationsJob( SendNoteNotificationsJobSettings settings, out List<MockSmtpSendResult> sendResults, out List<RockLogEvent> logEvents )
        {
            var job = new SendNoteNotifications();

            var smtpTransport = new MockSmtpTransport();
            job.EmailTransport = smtpTransport;

            var logger = new RockLoggerMemoryBuffer();
            job.Logger = logger;

            if ( settings == null )
            {
                settings = new SendNoteNotifications.SendNoteNotificationsJobSettings
                {
                    // Ensure no system email template is specified.
                    SystemCommunicationTemplateGuid = SystemGuid.SystemCommunication.NOTE_WATCH_NOTIFICATION.AsGuid(),
                    EffectiveDate = _TestPeriodStartDate
                };
            }

            job.ExecuteInternal( settings );

            logEvents = logger.GetLogEvents().ToList();

            sendResults = smtpTransport.ProcessedItems.ToList();
        }

        private static void CreateNotesTestData()
        {
            var noteManager = NoteDataManager.Instance;
            var rockContext = new RockContext();

            var personAlex = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlexDecker );
            var personAlisha = TestDataHelper.GetTestPerson( TestGuids.TestPeople.AlishaMarble );
            var personTed = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );
            var personCindy = TestDataHelper.GetTestPerson( TestGuids.TestPeople.CindyDecker );

            // Add Note Type: General Note (Person)
            var generalNoteType = noteManager.NewNoteType( _noteTypeGeneralTestGuid.AsGuid(),
                "General Note",
                typeof( Rock.Model.Person ) );

            generalNoteType.UserSelectable = true;
            generalNoteType.AllowsWatching = true;
            generalNoteType.AllowsReplies = true;

            noteManager.SaveNoteType( generalNoteType, rockContext );
            rockContext.SaveChanges();

            // Add Note Type: Private Note (Person)
            var pastoralNoteType = noteManager.NewNoteType( _noteTypePrivateTestGuid.AsGuid(),
                "Private Note",
                typeof( Rock.Model.Person ) );

            pastoralNoteType.UserSelectable = true;
            pastoralNoteType.AllowsWatching = true;
            pastoralNoteType.AllowsReplies = true;

            noteManager.SaveNoteType( pastoralNoteType, rockContext );
            rockContext.SaveChanges();

            // Add Note: For Alex Decker By Alisha Marble (1--Note)
            var note1 = noteManager.NewNote( "89c2b3fb-bfa7-4206-abcb-fb5f4dafcbd6".AsGuid(), pastoralNoteType.Id, "1--Note" )
                .ForEntity( personAlex.Id );

            noteManager.SaveNote( note1, rockContext );
            rockContext.SaveChanges();

            // Add Note Watch: Watched by Alisha Marble.
            noteManager.SetNoteWatchStatusForPerson( note1.Id,
                personAlisha.PrimaryAliasId.GetValueOrDefault(),
                isWatched: true,
                rockContext );
            rockContext.SaveChanges();

            // Add Note Reply: For Alex Decker By Ted Decker (1.1--Reply)
            var note11 = noteManager.NewNote( "d0478817-23f5-43ef-a712-a95ef988f942".AsGuid(), generalNoteType.Id, "1.1--Reply" )
                .ForEntity( personAlex.Id )
                .AsReplyTo( note1.Id );

            noteManager.SaveNote( note11, rockContext );
            rockContext.SaveChanges();

            // Add Note Watch: Watched by Ted Decker.
            noteManager.SetNoteWatchStatusForPerson( note11.Id,
                personTed.PrimaryAliasId.GetValueOrDefault(),
                isWatched: true,
                rockContext );

            rockContext.SaveChanges();

            // Add Note Reply: Alisha Marble (1.1.1--Reply)
            var note111 = noteManager.NewNote( "40081b03-6be8-44c6-bb0c-b2c82e7e67c4".AsGuid(), pastoralNoteType.Id, "1.1.1--Reply" )
                .ForEntity( personAlex.Id )
                .AsReplyTo( note11.Id );

            noteManager.SaveNote( note111, rockContext );
            rockContext.SaveChanges();

            // Add Note Watch: Alex Decker Notes Watched By Cindy Decker.
            var watchByCindy = noteManager.NewNoteWatch( "b89865f1-5a56-4bc0-94e7-f088c8a59992".AsGuid() )
                .ForWatchedPerson( personAlex.PrimaryAliasId )
                .ForWatchingPerson( personCindy.PrimaryAliasId )
                .ForWatchedNoteType( pastoralNoteType.Id );

            noteManager.SaveNoteWatch( watchByCindy, rockContext );
            rockContext.SaveChanges();
        }

        #endregion
    }
}
