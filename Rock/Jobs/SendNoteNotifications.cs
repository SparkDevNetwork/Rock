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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [SystemEmailField( "Note Watch Notification Email", "", defaultSystemEmailGuid: Rock.SystemGuid.SystemEmail.NOTEWATCH_NOTIFICATION, required: false, order: 1 )]
    [SystemEmailField( "Note Approval Notification Email", "", defaultSystemEmailGuid: Rock.SystemGuid.SystemEmail.NOTE_APPROVAL_NOTIFICATION, required: false, order: 2 )]
    [IntegerField( "Cutoff Days", "Just in case the Note Notification service hasn't run for a while, this is the max number of days between the note edited date and the notification.", required: true, defaultValue: 7, order: 3 )]
    public class SendNoteNotifications : IJob
    {
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendNoteNotifications()
        {
            //
        }

        /// <summary>
        /// 
        /// </summary>
        private class NoteWatchPersonToNotifyList : List<NoteWatchPersonToNotify>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoteWatchPersonToNotifyList"/> class.
            /// </summary>
            /// <param name="person">The person.</param>
            public NoteWatchPersonToNotifyList( Person person )
            {
                this.Person = person;
            }

            /// <summary>
            /// Gets or sets the person.
            /// </summary>
            /// <value>
            /// The person.
            /// </value>
            public Person Person { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private class NoteWatchPersonToNotify
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoteWatchPersonToNotify" /> class.
            /// </summary>
            /// <param name="person">The person.</param>
            /// <param name="note">The note.</param>
            /// <param name="noteWatch">The note watch.</param>
            public NoteWatchPersonToNotify( Person person, Note note, NoteWatch noteWatch )
            {
                this.Person = person;
                this.Note = note;
                this.NoteWatch = noteWatch;
            }

            /// <summary>
            /// Gets or sets the person.
            /// </summary>
            /// <value>
            /// The person.
            /// </value>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the note that triggered the notewatch notification
            /// </summary>
            /// <value>
            /// The note.
            /// </value>
            public Note Note { get; set; }

            /// <summary>
            /// Gets or sets the note watch.
            /// </summary>
            /// <value>
            /// The note watch.
            /// </value>
            public NoteWatch NoteWatch { get; set; }
        }

        /// <summary>
        /// The cutoff note edit date time
        /// </summary>
        private DateTime _cutoffNoteEditDateTime;

        /// <summary>
        /// The default lava merge fields
        /// </summary>
        private Dictionary<string, object> _defaultMergeFields;

        /// <summary>
        /// The note watch notification email unique identifier
        /// </summary>
        private Guid? _noteWatchNotificationEmailGuid;

        /// <summary>
        /// The note approval notification email unique identifier
        /// </summary>
        private Guid? _noteApprovalNotificationEmailGuid;

        /// <summary>
        /// The note watch notifications sent
        /// </summary>
        private int _noteWatchNotificationsSent = 0;

        /// <summary>
        /// The note approval notifications sent
        /// </summary>
        private int _noteApprovalNotificationsSent = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            // get the job dataMap
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int oldestDaysOld = dataMap.GetString( "CutoffDays" ).AsIntegerOrNull() ?? 7;
            _cutoffNoteEditDateTime = RockDateTime.Now.AddDays( -oldestDaysOld );
            _defaultMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            _noteWatchNotificationEmailGuid = dataMap.GetString( "NoteWatchNotificationEmail" ).AsGuidOrNull();
            _noteApprovalNotificationEmailGuid = dataMap.GetString( "NoteApprovalNotificationEmail" ).AsGuidOrNull();
            var errors = new List<string>();

            errors.AddRange( SendNoteWatchNotifications( context ) );
            context.UpdateLastStatusMessage( $"{_noteWatchNotificationsSent} note watch notifications sent..." );
            errors.AddRange( SendNoteApprovalNotifications( context ) );
            context.UpdateLastStatusMessage( $"{_noteWatchNotificationsSent} note watch notifications sent, and {_noteApprovalNotificationsSent} note approval notifications sent" );

            if ( errors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                var exception = new Exception( errorMessage );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );
                throw exception;
            }
        }

        /// <summary>
        /// Sends the note approval notifications.
        /// </summary>
        /// <param name="context">The context.</param>
        private List<string> SendNoteApprovalNotifications( IJobExecutionContext context )
        {
            var errors = new List<string>();
            List<int> noteIdsToProcessApprovalsList = new List<int>();

            using ( var rockContext = new RockContext() )
            {
                var noteService = new Rock.Model.NoteService( rockContext );

                // get all notes that are pending approval and haven't sent approval notifications yet
                var notesThatNeedApprovalNotifyQuery = noteService.Queryable().Where( a =>
                    a.NoteType.RequiresApprovals
                    && a.NoteType.SendApprovalNotifications
                    && a.ApprovalsSent == false
                    && a.ApprovalStatus == NoteApprovalStatus.PendingApproval
                    && a.EditedDateTime > _cutoffNoteEditDateTime );

                if ( !notesThatNeedApprovalNotifyQuery.Any() )
                {
                    // there aren't any notes that haven't had approval notifications processed yet
                    return errors;
                }

                noteIdsToProcessApprovalsList = notesThatNeedApprovalNotifyQuery.Select( a => a.Id ).ToList();
            }

            using ( var rockContext = new RockContext() )
            {
                // get the approvers for each notetypeId
                Dictionary<int, List<Person>> noteTypeApprovers = new Dictionary<int, List<Person>>();

                NoteTypeService noteTypeService = new NoteTypeService( rockContext );
                var noteTypeIdsForNotes = new NoteService( rockContext ).Queryable()
                    .Where( a => noteIdsToProcessApprovalsList.Contains( a.Id ) ).Select( a => a.NoteTypeId ).Distinct().ToList();

                foreach ( var noteTypeId in noteTypeIdsForNotes )
                {
                    var approvers = noteTypeService.GetApprovers( noteTypeId ).ToList();
                    noteTypeApprovers.Add( noteTypeId, approvers );
                }

                // make a list of notes for each approver so we can send a digest of notes to approve to each approver
                Dictionary<Person, List<Note>> approverNotesToApproveList = new Dictionary<Person, List<Note>>();
                foreach ( var noteId in noteIdsToProcessApprovalsList )
                {
                    var noteService = new Rock.Model.NoteService( rockContext );
                    var note = noteService.Get( noteId );
                    var approversForNote = noteTypeApprovers.GetValueOrNull( note.NoteTypeId );
                    if ( approversForNote?.Any() == true )
                    {
                        List<Note> notesToApprove;
                        foreach ( Person approverPerson in approversForNote )
                        {
                            if ( approverNotesToApproveList.ContainsKey( approverPerson ) )
                            {
                                notesToApprove = approverNotesToApproveList[approverPerson] ?? new List<Note>();
                            }
                            else
                            {
                                notesToApprove = new List<Note>();
                                approverNotesToApproveList.Add( approverPerson, notesToApprove );
                            }

                            notesToApprove.Add( note );
                        }
                    }
                    else
                    {
                        // if there are no approvers for this note type, leave it as pending approval
                    }
                }

                if ( !approverNotesToApproveList.Any() )
                {
                    // nothing to do so exit
                    return errors;
                }

                // send approval emails
                var recipients = new List<RecipientData>();
                foreach ( var approverNotesToApprove in approverNotesToApproveList )
                {
                    Person approverPerson = approverNotesToApprove.Key;
                    List<Note> noteList = approverNotesToApprove.Value;
                    if ( !string.IsNullOrEmpty( approverPerson.Email ) && approverPerson.IsEmailActive && noteList.Any() )
                    {
                        var mergeFields = new Dictionary<string, object>( _defaultMergeFields );
                        mergeFields.Add( "ApproverPerson", approverPerson );
                        mergeFields.Add( "NoteList", noteList );
                        recipients.Add( new RecipientData( approverPerson.Email, mergeFields ) );
                    }

                    if ( _noteApprovalNotificationEmailGuid.HasValue )
                    {
                        var emailMessage = new RockEmailMessage( _noteApprovalNotificationEmailGuid.Value );
                        emailMessage.SetRecipients( recipients );
                        emailMessage.Send( out errors );
                        _noteApprovalNotificationsSent += recipients.Count();

                        using ( var rockUpdateContext = new RockContext() )
                        {
                            var noteListIds = noteList.Select( a => a.Id ).ToList();
                            var notesToMarkApprovalSent = new NoteService( rockUpdateContext ).Queryable().Where( a => noteListIds.Contains( a.Id ) );

                            // use BulkUpdate to mark all the notes that we processed to ApprovalsSent = true
                            rockUpdateContext.BulkUpdate( notesToMarkApprovalSent, n => new Note { ApprovalsSent = true } );
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Sends the note watch notifications.
        /// </summary>
        /// <param name="context">The context.</param>
        private List<string> SendNoteWatchNotifications( IJobExecutionContext context )
        {
            var errors = new List<string>();
            List<int> noteIdsToProcessNoteWatchesList = new List<int>();

            using ( var rockContext = new RockContext() )
            {
                var noteService = new NoteService( rockContext );
                var noteWatchService = new NoteWatchService( rockContext );
                var noteWatchQuery = noteWatchService.Queryable();

                if ( !noteWatchQuery.Any() )
                {
                    // there aren't any note watches, so there is nothing to do
                    return errors;
                }

                // get all notes that haven't processed notifications yet
                var notesToNotifyQuery = noteService.Queryable().Where( a =>
                    a.NotificationsSent == false
                    && a.NoteType.AllowsWatching == true
                    && a.EditedDateTime > _cutoffNoteEditDateTime );

                // limit to notes that don't require approval or are approved
                notesToNotifyQuery = notesToNotifyQuery.Where( a => a.NoteType.RequiresApprovals == false || a.ApprovalStatus == NoteApprovalStatus.Approved );

                if ( !notesToNotifyQuery.Any() )
                {
                    // there aren't any notes that haven't had notifications processed yet
                    return errors;
                }

                noteIdsToProcessNoteWatchesList = notesToNotifyQuery.Select( a => a.Id ).ToList();
            }

            // make a list of notifications to send to each personId
            Dictionary<int, NoteWatchPersonToNotifyList> personNotificationDigestList = new Dictionary<int, NoteWatchPersonToNotifyList>();
            using ( var rockContext = new RockContext() )
            {
                foreach ( int noteId in noteIdsToProcessNoteWatchesList )
                {
                    this.UpdateNoteWatchNotificationDigest( personNotificationDigestList, rockContext, noteId );
                }

                // Send NoteWatch notifications
                if ( personNotificationDigestList.Any() )
                {

                    foreach ( var personNotificationDigest in personNotificationDigestList )
                    {
                        var recipients = new List<RecipientData>();
                        Person personToNotify = personNotificationDigest.Value.Person;
                        List<Note> noteList = personNotificationDigest.Value.Select( a => a.Note ).OrderBy( a => a.EditedDateTime ).ToList();

                        // make sure a person doesn't get a notification on a note that they wrote
                        noteList = noteList.Where( a => a.EditedByPersonAlias?.PersonId != personToNotify.Id ).ToList();

                        if ( !string.IsNullOrEmpty( personToNotify.Email ) && personToNotify.IsEmailActive && personToNotify.EmailPreference != EmailPreference.DoNotEmail && noteList.Any() )
                        {
                            var mergeFields = new Dictionary<string, object>( _defaultMergeFields );
                            mergeFields.Add( "Person", personToNotify );
                            mergeFields.Add( "NoteList", noteList );
                            recipients.Add( new RecipientData( personToNotify.Email, mergeFields ) );    
                        }

                        if ( _noteWatchNotificationEmailGuid.HasValue )
                        {
                            var emailMessage = new RockEmailMessage( _noteWatchNotificationEmailGuid.Value );
                            emailMessage.SetRecipients( recipients );
                            emailMessage.Send( out errors );
                            _noteWatchNotificationsSent += recipients.Count();
                        }
                    }
                }
            }

            using ( var rockUpdateContext = new RockContext() )
            {
                var notesToMarkNotified = new NoteService( rockUpdateContext ).Queryable().Where( a => noteIdsToProcessNoteWatchesList.Contains( a.Id ) );

                // use BulkUpdate to mark all the notes that we processed to NotificationsSent = true
                rockUpdateContext.BulkUpdate( notesToMarkNotified, n => new Note { NotificationsSent = true } );
            }
            return errors;
        }

        /// <summary>
        /// Updates the note watch notification digest.
        /// </summary>
        /// <param name="personIdNotificationDigestList">The person identifier notification digest list.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="noteId">The note identifier.</param>
        private void UpdateNoteWatchNotificationDigest( Dictionary<int, NoteWatchPersonToNotifyList> personIdNotificationDigestList, RockContext rockContext, int noteId )
        {
            var noteService = new Rock.Model.NoteService( rockContext );

            var note = noteService.Queryable().Include( a => a.EditedByPersonAlias ).FirstOrDefault( a => a.Id == noteId );

            if ( note == null || !note.EntityId.HasValue )
            {
                // shouldn't' happen
                return;
            }

            var noteType = NoteTypeCache.Get( note.NoteTypeId );

            // make sure the note's notetype has an EntityTypeId (is should, but just in case it doesn't)
            int? noteEntityTypeId = noteType?.EntityTypeId;
            if ( !noteEntityTypeId.HasValue )
            {
                return;
            }

            var noteWatchService = new Rock.Model.NoteWatchService( rockContext );

            // narrow it down to NoteWatches for the same EntityType as the Note
            var noteWatchesQuery = noteWatchService.Queryable()
                .Where( a =>
                    ( a.EntityTypeId.HasValue && a.EntityTypeId.Value == noteEntityTypeId.Value )
                    || ( a.NoteTypeId.HasValue && a.NoteType.EntityTypeId == noteEntityTypeId ) );

            // narrow it down to either note watches on..
            // 1) specific Entity
            // 2) specific Note
            // 3) any note of the NoteType
            // 4) any note on the EntityType

            // specific Entity
            noteWatchesQuery = noteWatchesQuery.Where( a =>
                ( a.EntityId == null )
                || ( note.EntityId.HasValue && a.EntityId.Value == note.EntityId.Value ) );

            // or specifically for this Note's ParentNote (a reply to the Note)
            noteWatchesQuery = noteWatchesQuery.Where( a =>
                ( a.NoteId == null )
                || ( note.ParentNoteId.HasValue && a.NoteId.Value == note.ParentNoteId ) );

            // or specifically for this note's note type
            noteWatchesQuery = noteWatchesQuery.Where( a =>
                ( a.NoteTypeId == null )
                || ( a.NoteTypeId.Value == note.NoteTypeId ) );

            // if there are any NoteWatches that relate to this note, process them
            if ( noteWatchesQuery.Any() )
            {
                var noteWatchesForNote = noteWatchesQuery.Include( a => a.WatcherPersonAlias.Person ).AsNoTracking().ToList();
                List<NoteWatchPersonToNotify> noteWatchPersonToNotifyListAll = new List<NoteWatchPersonToNotify>();

                // loop thru Watches to get a list of people to possibly notify/override
                foreach ( var noteWatch in noteWatchesForNote )
                {
                    // if a specific person is the watcher, add them
                    var watcherPerson = noteWatch.WatcherPersonAlias?.Person;
                    if ( watcherPerson != null )
                    {   // Since this is iterated do not add the person to the list if they are already there. 
                        var exists = noteWatchPersonToNotifyListAll.Where( p => p.Person.Email.Contains( watcherPerson.Email ) ).Any();
                        if ( !exists )
                        {
                            noteWatchPersonToNotifyListAll.Add( new NoteWatchPersonToNotify( watcherPerson, note, noteWatch ) );
                        }
                    }

                    if ( noteWatch.WatcherGroupId.HasValue )
                    {
                        var watcherPersonsFromGroup = new GroupMemberService( rockContext ).Queryable()
                            .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active && a.GroupId == noteWatch.WatcherGroupId.Value )
                            .Select( a => a.Person ).ToList();

                        if ( watcherPersonsFromGroup.Any() )
                        {
                            // Do not add people from the group that are already added.
                            var distinctWatchers = watcherPersonsFromGroup.Where( wg => !noteWatchPersonToNotifyListAll.Where( w => w.Person.Email.Contains( wg.Email ) ).Any() );
                            noteWatchPersonToNotifyListAll.AddRange( distinctWatchers.Select( a => new NoteWatchPersonToNotify( a, note, noteWatch ) ) );
                        }
                    }
                }

                var noteWatchPersonToNotifyList = noteWatchPersonToNotifyListAll.Where( a => a.NoteWatch.IsWatching ).ToList();
                var noteWatchPersonToNotifyListWatchDisabled = noteWatchPersonToNotifyListAll.Where( a => !a.NoteWatch.IsWatching ).ToList();
                var noteWatchPersonToNotifyListNoOverride = noteWatchPersonToNotifyList.Where( a => a.NoteWatch.AllowOverride == false ).ToList();

                foreach ( var noteWatchPersonToNotify in noteWatchPersonToNotifyList )
                {
                    // check if somebody wanted to specifically NOT watch this (and there isn't another watch that prevents overrides)
                    if ( noteWatchPersonToNotifyListWatchDisabled.Any( a => a.Person.Id == noteWatchPersonToNotify.Person.Id ) )
                    {
                        // Person Requested to NOT watch, now make sure that they aren't force to watch based on Override = False
                        if ( !noteWatchPersonToNotifyListNoOverride.Any( a => a.Person.Id == noteWatchPersonToNotify.Person.Id ) )
                        {
                            // person requested to NOT watch, and there aren't any overrides to prevent that, so jump out to the next one
                            continue;
                        }
                    }

                    NoteWatchPersonToNotifyList personToNotifyList;
                    if ( personIdNotificationDigestList.ContainsKey( noteWatchPersonToNotify.Person.Id ) )
                    {
                        // This just create a place holder for the Note referring to item in the digest by key of person
                        personToNotifyList = personIdNotificationDigestList[noteWatchPersonToNotify.Person.Id] ?? new NoteWatchPersonToNotifyList( noteWatchPersonToNotify.Person );
                    }
                    else
                    {
                        // This just creates  a new place holder for the Note in the digest 
                        personToNotifyList = new NoteWatchPersonToNotifyList( noteWatchPersonToNotify.Person );
                        personIdNotificationDigestList.Add( noteWatchPersonToNotify.Person.Id, personToNotifyList );
                    }

                    // Only include the note if the watcher person is authorized to view the note
                    if ( noteWatchPersonToNotify.Note.IsAuthorized( Rock.Security.Authorization.VIEW, noteWatchPersonToNotify.Person ) )
                    {
                        // This is where the get added to the item of the digest
                        personToNotifyList.Add( noteWatchPersonToNotify );
                    }
                }
            }
        }
    }
}