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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.ViewModels.Blocks.Crm.BulkUpdate;
using Rock.Web.Cache;

namespace Rock.Crm.BulkUpdate
{
    /// <summary>
    /// Applies a bulk update to a list of <see cref="Person"/> records on a background
    /// thread. The processor partitions the persons into batches and processes each batch
    /// in parallel using its own <see cref="RockContext"/>, streaming progress back to the
    /// caller through an optional <see cref="TaskActivityProgress"/> wrapper.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Within each batch the pipeline runs in the following order: Person fields, Family
    /// Campus, Following, Person Attributes, Note, Group (add / remove / update), Step
    /// (add / remove / modify), and Tag. Workflow launches run last, after the batch
    /// transaction commits, since an enqueued workflow cannot be rolled back.
    /// </para>
    /// <para>
    /// Each per-batch worker owns its own <see cref="RockContext"/>; a context is never
    /// shared across workers (EF6 <c>DbContext</c> is not thread-safe).
    /// </para>
    /// </remarks>
    internal class BulkUpdateProcessor
    {
        #region Keys

        /// <summary>
        /// The canonical keys recognized in <see cref="BulkUpdateBag.UpdatedFields"/>.
        /// Mirror of the client-side <c>UpdatedFieldKey</c> in <c>types.partial.ts</c>;
        /// keep the two in sync.
        /// </summary>
        /// <remarks>
        /// A key without a pipeline handler is a silent no-op. <c>Grade</c> is
        /// permanently client-only: the UI computes a GraduationYear via the
        /// <c>GetGraduationYearFromGrade</c> block action and writes it through
        /// the <c>GraduationYear</c> key.
        /// </remarks>
        private static class UpdatedFieldKey
        {
            public const string Title = "title";
            public const string Suffix = "suffix";
            public const string Gender = "gender";
            public const string MaritalStatus = "maritalStatus";
            public const string Grade = "grade";
            public const string GraduationYear = "graduationYear";
            public const string Campus = "campus";
            public const string ConnectionStatus = "connectionStatus";
            public const string RecordStatus = "recordStatus";
            public const string RecordSource = "recordSource";
            public const string CommunicationPreference = "communicationPreference";
            public const string IsEmailActive = "isEmailActive";
            public const string EmailPreference = "emailPreference";
            public const string EmailNote = "emailNote";
            public const string Following = "following";
            public const string ReviewReason = "reviewReason";
            public const string ReviewReasonNote = "reviewReasonNote";
            public const string SystemNote = "systemNote";
        }

        /// <summary>
        /// The keys recognized in <see cref="BulkUpdateGroupBag.UpdatedFields"/> for the
        /// Group <c>Update</c> branch. Mirror of the client-side <c>isUpdatingGroup</c>
        /// reactive in <c>bulkUpdate.obs</c>; keep the two in sync.
        /// </summary>
        private static class GroupUpdatedFieldKey
        {
            public const string Role = "role";
            public const string MemberStatus = "memberStatus";
        }

        /// <summary>
        /// The keys recognized in <see cref="BulkUpdateStepBag.UpdatedFields"/> for the
        /// Step <c>Update</c> (Modify) branch. Mirror of the client-side
        /// <c>isUpdatingStep</c> reactive in <c>bulkUpdate.obs</c>; keep the two in sync.
        /// </summary>
        private static class StepUpdatedFieldKey
        {
            public const string Status = "status";
            public const string StartDate = "startDate";
            public const string EndDate = "endDate";
            public const string Campus = "campus";
            public const string Note = "note";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The hard cap on the degree of parallelism.
        /// </summary>
        private const int MaxAllowedTaskCount = 64;

        private readonly BulkUpdateSettings _settings;
        private readonly BulkUpdateBag _bag;
        private readonly UpdatedFieldFlags _updatedFields;
        private readonly TaskActivityProgress _progress;

        private readonly int _personAliasEntityTypeId;
        private readonly int _personEntityTypeId;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkUpdateProcessor"/> class.
        /// </summary>
        /// <param name="settings">The bulk update payload and authorization context.</param>
        /// <param name="progress">
        /// Optional progress reporter. When supplied, the processor reports per-batch
        /// completion via <see cref="TaskActivityProgress.ReportProgressUpdate(long, long, string)"/>
        /// and the caller is responsible for calling
        /// <see cref="TaskActivityProgress.StopTask"/> with the returned
        /// <see cref="BulkUpdateResultBag"/>.
        /// </param>
        internal BulkUpdateProcessor( BulkUpdateSettings settings, TaskActivityProgress progress = null )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
            _bag = _settings.Bag;
            _updatedFields = new UpdatedFieldFlags( _bag?.UpdatedFields );
            _progress = progress;
            _personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            _personEntityTypeId = EntityTypeCache.Get<Person>().Id;
        }

        #endregion

        #region Process

        /// <summary>
        /// Runs the bulk update across all persons in <see cref="BulkUpdateSettings.Bag"/>.
        /// Resolves the <see cref="BulkUpdatePersonBag.PersonAliasGuid"/> values to person
        /// identifiers, partitions the work, and executes each batch in parallel.
        /// </summary>
        /// <returns>A <see cref="BulkUpdateResultBag"/> summarizing the run.</returns>
        internal BulkUpdateResultBag Process()
        {
            var result = new BulkUpdateResultBag();

            if ( _bag == null || _bag.UpdatePersons == null || !_bag.UpdatePersons.Any() )
            {
                return result;
            }

            var personIds = ResolvePersonIds( _bag.UpdatePersons );
            result.TotalCount = personIds.Count;

            if ( personIds.Count == 0 )
            {
                return result;
            }

            var taskCount = ResolveTaskCount();
            var batchSize = _settings.BatchSize.GetValueOrDefault();

            var partitioner = batchSize > 0
                ? Partitioner.Create( 0, personIds.Count, batchSize )
                : Partitioner.Create( 0, personIds.Count );

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = taskCount };

            var personResults = new ConcurrentBag<BulkUpdatePersonResultBag>();
            var errors = new ConcurrentBag<string>();
            long successCount = 0;
            long issuesCount = 0;
            long failedCount = 0;
            long processedCount = 0;

            try
            {
                Parallel.ForEach( partitioner, parallelOptions, range =>
                {
                    var batchPersonIds = new List<int>( range.Item2 - range.Item1 );
                    for ( var i = range.Item1; i < range.Item2; i++ )
                    {
                        batchPersonIds.Add( personIds[i] );
                    }

                    var batchResult = ProcessBatch( batchPersonIds );
                    Interlocked.Add( ref successCount, batchResult.SuccessCount );
                    Interlocked.Add( ref issuesCount, batchResult.IssuesCount );
                    Interlocked.Add( ref failedCount, batchResult.FailedCount );

                    foreach ( var personResult in batchResult.PersonResults )
                    {
                        personResults.Add( personResult );
                    }

                    foreach ( var error in batchResult.Errors )
                    {
                        errors.Add( error );
                    }

                    var processed = Interlocked.Add( ref processedCount, batchPersonIds.Count );
                    _progress?.ReportProgressUpdate( processed, personIds.Count );
                } );
            }
            catch ( AggregateException ex )
            {
                /*
                    5/27/2026 - MSE

                    Per-batch lambdas already trap their own exceptions and account for
                    every person they touch. This outer catch handles partitioner /
                    scheduler failures (rare, defensive only). Count any persons that never
                    made it through a batch as failed so SuccessCount + IssuesCount +
                    FailedCount == TotalCount.
                */
                foreach ( var inner in ex.InnerExceptions )
                {
                    ExceptionLogService.LogException( inner );
                    errors.Add( inner.Message );
                }

                var accounted = Interlocked.Read( ref successCount )
                    + Interlocked.Read( ref issuesCount )
                    + Interlocked.Read( ref failedCount );
                var unprocessed = personIds.Count - accounted;
                if ( unprocessed > 0 )
                {
                    Interlocked.Add( ref failedCount, unprocessed );
                }
            }

            result.SuccessCount = ( int ) Interlocked.Read( ref successCount );
            result.IssuesCount = ( int ) Interlocked.Read( ref issuesCount );
            result.FailedCount = ( int ) Interlocked.Read( ref failedCount );
            result.PersonResults = personResults.OrderBy( r => r.PersonName ).ToList();
            result.Errors = errors.ToList();

            // Force the progress bar to land on 100% even if a batch reported short.
            _progress?.ReportProgressUpdate( personIds.Count, personIds.Count );

            return result;
        }

        /// <summary>
        /// Processes a single batch of persons inside its own <see cref="RockContext"/>. The
        /// pipeline pieces that write via <c>SaveChanges</c> run inside one
        /// <see cref="Rock.Data.DbContext.WrapTransaction(System.Action)"/>, so a hard
        /// failure rolls the whole batch back rather than leaving partial writes. Only the
        /// Workflow launch (an irreversible queue enqueue) cannot participate in a DbContext
        /// transaction, so it runs after the core commits. Per-person business-rule
        /// rejections are collected on a <see cref="BatchOutcomeTracker"/> and translated
        /// into the result's three buckets (Updated / CompletedWithIssues / Failed).
        /// </summary>
        private BulkUpdateResultBag ProcessBatch( List<int> personIds )
        {
            var batchResult = new BulkUpdateResultBag { TotalCount = personIds.Count };

            using ( var rockContext = new RockContext() )
            {
                List<Person> persons;
                var outcomes = new BatchOutcomeTracker();

                /*
                    5/28/2026 - MSE

                    Load the batch and commit the SaveChanges-based pieces as one unit, inside a
                    single transaction. A hard failure anywhere in here (the load, a DB
                    constraint, a deadlock) rolls the whole batch back, so every person is
                    reported as Failed with no partial writes.
                */
                try
                {
                    persons = new PersonService( rockContext ).Queryable( true )
                        .Where( p => personIds.Contains( p.Id ) )
                        .ToList();

                    rockContext.WrapTransaction( () =>
                    {
                        ApplyPersonFields( persons );
                        ApplyFamilyCampus( persons, rockContext, outcomes );
                        ApplyFollowing( persons, rockContext );

                        rockContext.SaveChanges();

                        ApplyPersonAttributes( persons, rockContext );
                        ApplyNote( persons, rockContext );
                        ApplyGroup( persons, rockContext, outcomes );
                        ApplyStep( persons, rockContext, outcomes );
                        ApplyTag( persons, rockContext );
                    } );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    batchResult.FailedCount = personIds.Count;
                    batchResult.Errors.Add( ex.Message );
                    return batchResult;
                }

                /*
                    5/28/2026 - MSE

                    Workflow launch is the only piece that cannot join the transaction above: an
                    enqueued workflow is handed to a background queue and cannot be recalled if a
                    later step were to roll back. It runs after the core commits and is isolated so
                    a failure is logged and recorded as a run-level note rather than failing the
                    persons whose committed changes succeeded.
                */
                try
                {
                    ApplyWorkflows( persons );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    batchResult.Errors.Add( $"Workflow launch could not be completed: {ex.Message}" );
                }

                // Translate the per-person issues into the three outcome buckets.
                foreach ( var person in persons )
                {
                    if ( outcomes.TryGetIssues( person.Id, out var issues ) )
                    {
                        batchResult.IssuesCount++;
                        batchResult.PersonResults.Add( new BulkUpdatePersonResultBag
                        {
                            PersonId = person.Id,
                            IdKey = person.IdKey,
                            PersonName = person.FullName,
                            Issues = issues
                        } );
                    }
                    else
                    {
                        batchResult.SuccessCount++;
                    }
                }

                // Persons whose id no longer resolves to a row (deleted or merged between
                // selection and this batch) could not be updated.
                var notFoundCount = personIds.Count - persons.Count;
                if ( notFoundCount > 0 )
                {
                    batchResult.FailedCount += notFoundCount;
                    batchResult.Errors.Add( $"{notFoundCount} selected {( notFoundCount == 1 ? "individual was" : "individuals were" )} not found (deleted or merged) and could not be updated." );
                }
            }

            return batchResult;
        }

        #endregion

        #region Pipeline

        #region Pipeline: Person Fields

        /// <summary>
        /// Applies the simple scalar Person field updates from
        /// <see cref="BulkUpdateBag"/>. Fields not toggled on in
        /// <see cref="BulkUpdateBag.UpdatedFields"/> are left alone. Connection Status,
        /// Record Status, and Record Source are skipped when the caller lacks the
        /// corresponding per-field security action.
        /// </summary>
        private void ApplyPersonFields( List<Person> persons )
        {
            if ( !_updatedFields.HasAny() )
            {
                return;
            }

            foreach ( var person in persons )
            {
                if ( _updatedFields.IsActive( UpdatedFieldKey.Title ) )
                {
                    person.TitleValueId = ResolveDefinedValueId( _bag.TitleValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.Suffix ) )
                {
                    person.SuffixValueId = ResolveDefinedValueId( _bag.SuffixValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.ConnectionStatus ) && _settings.CanEditConnectionStatus )
                {
                    person.ConnectionStatusValueId = ResolveDefinedValueId( _bag.ConnectionStatusValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.RecordStatus ) && _settings.CanEditRecordStatus )
                {
                    var recordStatusValueId = ResolveDefinedValueId( _bag.RecordStatusValueGuid );
                    person.RecordStatusValueId = recordStatusValueId;

                    var inactiveRecordStatusId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )?.Id;
                    if ( recordStatusValueId.HasValue && recordStatusValueId == inactiveRecordStatusId )
                    {
                        person.RecordStatusReasonValueId = ResolveDefinedValueId( _bag.InactiveReasonValueGuid );

                        if ( !string.IsNullOrWhiteSpace( _bag.InactiveReasonNote ) )
                        {
                            person.InactiveReasonNote = _bag.InactiveReasonNote;
                        }
                    }
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.RecordSource ) && _settings.CanEditRecordSource )
                {
                    person.RecordSourceValueId = ResolveDefinedValueId( _bag.RecordSourceValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.Gender ) )
                {
                    // A blank selection clears the gender, which for this non-nullable
                    // enum means Unknown. Honors the client's "(clear)" option.
                    person.Gender = _bag.Gender ?? Gender.Unknown;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.MaritalStatus ) )
                {
                    person.MaritalStatusValueId = ResolveDefinedValueId( _bag.MaritalStatusValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.GraduationYear ) )
                {
                    person.GraduationYear = _bag.GraduationYear;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.IsEmailActive ) && _bag.IsEmailActive.HasValue )
                {
                    person.IsEmailActive = _bag.IsEmailActive.Value;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.CommunicationPreference ) && _bag.CommunicationPreference.HasValue )
                {
                    // Bag uses Rock.Enums.Communication.CommunicationType; Person uses
                    // Rock.Model.CommunicationType. Same integer values.
                    person.CommunicationPreference = ( Rock.Model.CommunicationType ) _bag.CommunicationPreference.Value;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.EmailPreference ) && _bag.EmailPreference.HasValue )
                {
                    person.EmailPreference = _bag.EmailPreference.Value;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.EmailNote ) )
                {
                    person.EmailNote = _bag.EmailNote;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.SystemNote ) )
                {
                    person.SystemNote = _bag.SystemNote;
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.ReviewReason ) )
                {
                    person.ReviewReasonValueId = ResolveDefinedValueId( _bag.ReviewReasonValueGuid );
                }

                if ( _updatedFields.IsActive( UpdatedFieldKey.ReviewReasonNote ) )
                {
                    person.ReviewReasonNote = _bag.ReviewReasonNote;
                }
            }
        }

        #endregion

        #region Pipeline: Family Campus

        /// <summary>
        /// Applies the family campus update. Each selected person's single family receives
        /// <see cref="BulkUpdateBag.CampusGuid"/> (or null to clear). Persons that belong
        /// to multiple families are skipped, recorded as a per-person issue. Persons with no
        /// family memberships are skipped silently.
        /// </summary>
        private void ApplyFamilyCampus( List<Person> persons, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            if ( !_updatedFields.IsActive( UpdatedFieldKey.Campus ) || persons.Count == 0 )
            {
                return;
            }

            var newCampusId = _bag.CampusGuid.HasValue && _bag.CampusGuid.Value != Guid.Empty
                ? CampusCache.Get( _bag.CampusGuid.Value )?.Id
                : null;

            var personIds = persons.Select( p => p.Id ).ToList();
            var familyGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var familyMemberships = new GroupMemberService( rockContext ).Queryable()
                .Where( m => personIds.Contains( m.PersonId ) && m.Group.GroupType.Guid == familyGuid )
                .Select( m => new FamilyMembership { PersonId = m.PersonId, GroupId = m.GroupId } )
                .Distinct()
                .ToList();

            if ( familyMemberships.Count == 0 )
            {
                return;
            }

            var familyIds = familyMemberships.Select( m => m.GroupId ).Distinct().ToList();
            var families = new GroupService( rockContext ).Queryable()
                .Where( g => familyIds.Contains( g.Id ) )
                .ToList();

            /*
                5/27/2026 - MSE

                Persons in the same family share one family Group. Track which
                families we've already mutated so we don't overwrite the campus
                a second time when two selected persons share a single family.
            */
            var updatedFamilyIds = new HashSet<int>();

            foreach ( var person in persons )
            {
                var personFamilyIds = familyMemberships
                    .Where( m => m.PersonId == person.Id )
                    .Select( m => m.GroupId )
                    .ToList();

                if ( personFamilyIds.Count > 1 )
                {
                    outcomes.RecordIssue( person.Id, "Campus not updated (belongs to multiple families)." );
                    continue;
                }

                if ( personFamilyIds.Count != 1 )
                {
                    continue;
                }

                var familyId = personFamilyIds[0];
                if ( !updatedFamilyIds.Add( familyId ) )
                {
                    continue;
                }

                var family = families.FirstOrDefault( f => f.Id == familyId );
                if ( family != null )
                {
                    family.CampusId = newCampusId;
                }
            }
        }

        #endregion

        #region Pipeline: Following

        /// <summary>
        /// Adds or removes <see cref="Following"/> rows for the selected persons. The
        /// follower is always the user running the bulk update. Dedup (Add) and the
        /// delete filter (Remove) operate across <strong>every</strong> alias of each
        /// selected person — not just <c>PrimaryAlias</c>. New follows are inserted on
        /// <c>PrimaryAlias</c>.
        /// </summary>
        private void ApplyFollowing( List<Person> persons, RockContext rockContext )
        {
            if ( !_updatedFields.IsActive( UpdatedFieldKey.Following )
                || !_bag.Following.HasValue
                || !_settings.CurrentPersonAliasId.HasValue )
            {
                return;
            }

            var personIds = persons.Select( p => p.Id ).ToList();
            if ( personIds.Count == 0 )
            {
                return;
            }

            var aliasService = new PersonAliasService( rockContext );
            var followingService = new FollowingService( rockContext );
            var currentPersonAliasId = _settings.CurrentPersonAliasId.Value;

            /*
                5/28/2026 - MSE

                Joining the alias set in SQL keeps alias resolution and the Following
                filter in a single round-trip. Scoping across every alias (not just
                PrimaryAlias) catches follows on non-primary aliases from prior
                merges or API inserts.

                Reason: One round-trip; dedup / delete cover every alias of each person.
            */
            var aliasesForSelectedPersons = aliasService.Queryable()
                .Where( pa => personIds.Contains( pa.PersonId ) );

            if ( _bag.Following.Value == Rock.Enums.Crm.BulkUpdateActionSpecifier.Add )
            {
                // PersonIds whose any alias the current user already follows.
                var alreadyFollowedPersonIds = new HashSet<int>(
                    followingService.Queryable()
                        .Where( f =>
                            f.EntityTypeId == _personAliasEntityTypeId
                            && ( f.PurposeKey == null || f.PurposeKey == "" )
                            && f.PersonAliasId == currentPersonAliasId )
                        .Join(
                            aliasesForSelectedPersons,
                            f => f.EntityId,
                            pa => pa.Id,
                            ( f, pa ) => pa.PersonId )
                        .Distinct() );

                foreach ( var person in persons )
                {
                    if ( !person.PrimaryAliasId.HasValue )
                    {
                        continue;
                    }

                    if ( alreadyFollowedPersonIds.Contains( person.Id ) )
                    {
                        continue;
                    }

                    followingService.Add( new Following
                    {
                        EntityTypeId = _personAliasEntityTypeId,
                        EntityId = person.PrimaryAliasId.Value,
                        PersonAliasId = currentPersonAliasId
                    } );
                }
            }
            else if ( _bag.Following.Value == Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove )
            {
                var followsToDelete = followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == _personAliasEntityTypeId
                        && ( f.PurposeKey == null || f.PurposeKey == "" )
                        && f.PersonAliasId == currentPersonAliasId )
                    .Join(
                        aliasesForSelectedPersons,
                        f => f.EntityId,
                        pa => pa.Id,
                        ( f, pa ) => f )
                    .ToList();

                followingService.DeleteRange( followsToDelete );
            }
        }

        #endregion

        #region Pipeline: Person Attributes

        /// <summary>
        /// Applies the per-attribute writes from <see cref="BulkUpdateBag.PersonAttributes"/>,
        /// fenced by <see cref="BulkUpdateSettings.AuthorizedPersonAttributes"/> — the set the
        /// block resolved from the admin-configured attribute categories and the current
        /// user's <c>Authorization.EDIT</c>. Keys outside the fence are dropped silently. The
        /// shared <see cref="ApplyAttributeValues{T}"/> does the load / diff / save.
        /// </summary>
        private void ApplyPersonAttributes( List<Person> persons, RockContext rockContext )
        {
            ApplyAttributeValues( persons, _bag.PersonAttributes, _settings.AuthorizedPersonAttributes, rockContext );
        }

        #endregion

        #region Pipeline: Note

        /// <summary>
        /// Adds one <see cref="Note"/> per selected person against
        /// <see cref="BulkUpdateSettings.AuthorizedNoteTypeId"/>. Caption is
        /// <c>"You - Personal Note"</c> when private, empty otherwise. Skipped entirely
        /// when no NoteType is authorized or the text is blank.
        /// </summary>
        private void ApplyNote( List<Person> persons, RockContext rockContext )
        {
            if ( _bag.NoteUpdate == null
                || string.IsNullOrWhiteSpace( _bag.NoteUpdate.NoteText )
                || !_settings.AuthorizedNoteTypeId.HasValue
                || persons.Count == 0 )
            {
                return;
            }

            var noteTypeId = _settings.AuthorizedNoteTypeId.Value;
            var noteText = _bag.NoteUpdate.NoteText;
            var isPrivate = _bag.NoteUpdate.IsPrivate;
            var isAlert = _bag.NoteUpdate.IsAlert;
            var caption = isPrivate ? "You - Personal Note" : string.Empty;

            var noteService = new NoteService( rockContext );
            var notes = new List<Note>( persons.Count );

            foreach ( var person in persons )
            {
                notes.Add( new Note
                {
                    IsSystem = false,
                    EntityId = person.Id,
                    Caption = caption,
                    Text = noteText,
                    IsAlert = isAlert,
                    IsPrivateNote = isPrivate,
                    NoteTypeId = noteTypeId
                } );
            }

            noteService.AddRange( notes );
            rockContext.SaveChanges();
        }

        #endregion

        #region Pipeline: Group

        /// <summary>
        /// Adds members to, removes members from, or updates memberships on the authorized
        /// group (<see cref="BulkUpdateSettings.AuthorizedGroupId"/>) for the selected
        /// persons. Three branches:
        /// <list type="bullet">
        /// <item><description><b>Add</b>: skips persons already holding the target role,
        /// validates each candidate via <c>IsValidGroupMember</c>, and surfaces validation
        /// failures as per-person issues.</description></item>
        /// <item><description><b>Remove</b>: archives members that carry group-member
        /// history (when the group type enables history), otherwise deletes them; the
        /// archive-eligibility check is one batched query.</description></item>
        /// <item><description><b>Update</b>: honors the per-field toggles
        /// (<c>role</c>, <c>memberStatus</c>), guarding the role change against the unique
        /// <c>(GroupId, PersonId, GroupRoleId)</c> key.</description></item>
        /// </list>
        /// Member attribute writes (Add and Update) are fenced by
        /// <see cref="BulkUpdateSettings.AuthorizedGroupMemberAttributes"/>. Skipped
        /// entirely when no group is authorized.
        /// </summary>
        private void ApplyGroup( List<Person> persons, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            if ( _bag.GroupUpdate == null || !_settings.AuthorizedGroupId.HasValue || persons.Count == 0 )
            {
                return;
            }

            var groupId = _settings.AuthorizedGroupId.Value;
            var group = new GroupService( rockContext ).Get( groupId );
            if ( group == null )
            {
                return;
            }

            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupTypeCache == null )
            {
                return;
            }

            var personIds = persons.Select( p => p.Id ).ToList();
            var groupMemberService = new GroupMemberService( rockContext );

            var existingMembersQuery = groupMemberService.Queryable( true ).Include( m => m.Group )
                .Where( m => m.GroupId == groupId && personIds.Contains( m.PersonId ) );

            if ( _bag.GroupUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove )
            {
                ApplyGroupRemove( group, groupTypeCache, groupMemberService, existingMembersQuery, rockContext, outcomes );
            }
            else if ( _bag.GroupUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Add )
            {
                ApplyGroupAdd( group, groupTypeCache, groupMemberService, existingMembersQuery, personIds, rockContext, outcomes );
            }
            else if ( _bag.GroupUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Update )
            {
                ApplyGroupUpdate( groupTypeCache, existingMembersQuery, rockContext, outcomes );
            }
        }

        /// <summary>
        /// Removes the selected persons from the group. Members carrying group-member
        /// history are archived (soft-deleted) so the snapshots survive; the rest are
        /// hard-deleted after a <c>CanDelete</c> check. A single batched query resolves
        /// which members have history, and a single <c>SaveChanges</c> commits the whole
        /// batch. Per-member failures surface as per-person issues.
        /// </summary>
        private void ApplyGroupRemove( Group group, GroupTypeCache groupTypeCache, GroupMemberService groupMemberService, IQueryable<GroupMember> existingMembersQuery, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            var membersToRemove = existingMembersQuery.ToList();
            if ( membersToRemove.Count == 0 )
            {
                return;
            }

            var memberIds = membersToRemove.Select( m => m.Id ).ToList();
            var memberIdsWithHistory = groupTypeCache.EnableGroupHistory
                ? new HashSet<int>( new GroupMemberHistoricalService( rockContext ).Queryable()
                    .Where( h => memberIds.Contains( h.GroupMemberId ) )
                    .Select( h => h.GroupMemberId )
                    .Distinct() )
                : new HashSet<int>();

            foreach ( var groupMember in membersToRemove )
            {
                try
                {
                    if ( groupTypeCache.EnableGroupHistory && memberIdsWithHistory.Contains( groupMember.Id ) )
                    {
                        // History snapshots exist; soft-delete (archive) to preserve them.
                        groupMemberService.Archive( groupMember, _settings.CurrentPersonAliasId, true );
                    }
                    else
                    {
                        if ( !groupMemberService.CanDelete( groupMember, out var errorMessage ) )
                        {
                            outcomes.RecordIssue( groupMember.PersonId, errorMessage );
                            continue;
                        }

                        groupMemberService.Delete( groupMember, true );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    outcomes.RecordIssue( groupMember.PersonId, $"Could not be removed from {group.Name}." );
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Adds the selected persons to the group in the configured role and member status.
        /// Persons already holding the target role are skipped (the unique
        /// <c>(GroupId, PersonId, GroupRoleId)</c> key would reject them); the rest are
        /// validated via <c>IsValidGroupMember</c>, with validation failures surfaced as
        /// warnings. Member attribute values are applied to the newly added members.
        /// </summary>
        private void ApplyGroupAdd( Group group, GroupTypeCache groupTypeCache, GroupMemberService groupMemberService, IQueryable<GroupMember> existingMembersQuery, List<int> personIds, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            var groupRoleId = ResolveGroupRoleId( groupTypeCache, _bag.GroupUpdate.GroupRole?.Value );
            if ( !groupRoleId.HasValue )
            {
                return;
            }

            var memberStatus = _bag.GroupUpdate.MemberStatus ?? GroupMemberStatus.Active;

            // Persons who already hold the target role; adding them again would trip the
            // unique (GroupId, PersonId, GroupRoleId) key.
            var existingRolePersonIds = new HashSet<int>( existingMembersQuery
                .Where( m => m.GroupRoleId == groupRoleId.Value )
                .Select( m => m.PersonId ) );

            var newMembers = new List<GroupMember>();

            foreach ( var personId in personIds )
            {
                if ( existingRolePersonIds.Contains( personId ) )
                {
                    continue;
                }

                var groupMember = new GroupMember
                {
                    Group = group,
                    GroupId = group.Id,
                    GroupRoleId = groupRoleId.Value,
                    GroupMemberStatus = memberStatus,
                    PersonId = personId
                };

                if ( groupMember.IsValidGroupMember( rockContext ) )
                {
                    groupMemberService.Add( groupMember );
                    newMembers.Add( groupMember );
                }
                else
                {
                    var validationMessage = string.Join( ", ", groupMember.ValidationResults.Select( r => r.ErrorMessage ) );
                    outcomes.RecordIssue( personId, validationMessage );
                }
            }

            rockContext.SaveChanges();

            ApplyGroupMemberAttributes( newMembers, rockContext );
        }

        /// <summary>
        /// Updates the existing memberships for the selected persons. The role and member
        /// status are each applied only when their toggle is set in
        /// <see cref="BulkUpdateGroupBag.UpdatedFields"/>. The role change skips any person
        /// who already holds the target role. A mutated member that fails
        /// <c>IsValidGroupMember</c> (role-max, group capacity, unmet add requirements) is
        /// skipped with a per-person issue so it does not fail the whole batch. Member
        /// attribute values are applied to the surviving matched members.
        /// </summary>
        private void ApplyGroupUpdate( GroupTypeCache groupTypeCache, IQueryable<GroupMember> existingMembersQuery, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            var members = existingMembersQuery.ToList();
            if ( members.Count == 0 )
            {
                return;
            }

            var groupUpdatedFields = new UpdatedFieldFlags( _bag.GroupUpdate.UpdatedFields );

            // Members whose role or status this pass actually changed (i.e. will be in the
            // EF Modified state at SaveChanges). Only these need the validity re-check below.
            var mutatedMembers = new HashSet<GroupMember>();

            var groupRoleId = ResolveGroupRoleId( groupTypeCache, _bag.GroupUpdate.GroupRole?.Value );
            if ( groupUpdatedFields.IsActive( GroupUpdatedFieldKey.Role ) && groupRoleId.HasValue )
            {
                /*
                    5/28/2026 - MSE

                    Skip moving a person into the target role when they already hold it,
                    which would trip the unique (GroupId, PersonId, GroupRoleId) key. The set
                    seeds from members already in the target role and grows as we move each
                    person in, so a person who appears in the batch under two non-target roles
                    gets one row moved and the rest left in place.

                    Reason: Prevent duplicate target-role rows without failing the batch.
                */
                var personIdsInTargetRole = new HashSet<int>(
                    members.Where( m => m.GroupRoleId == groupRoleId.Value ).Select( m => m.PersonId ) );

                foreach ( var member in members.Where( m => m.GroupRoleId != groupRoleId.Value ).ToList() )
                {
                    // Already holds the target role (originally, or moved earlier this pass).
                    if ( personIdsInTargetRole.Contains( member.PersonId ) )
                    {
                        continue;
                    }

                    member.GroupRoleId = groupRoleId.Value;
                    personIdsInTargetRole.Add( member.PersonId );
                    mutatedMembers.Add( member );
                }
            }

            if ( groupUpdatedFields.IsActive( GroupUpdatedFieldKey.MemberStatus ) && _bag.GroupUpdate.MemberStatus.HasValue )
            {
                foreach ( var member in members )
                {
                    member.GroupMemberStatus = _bag.GroupUpdate.MemberStatus.Value;
                    mutatedMembers.Add( member );
                }
            }

            /*
                5/28/2026 - MSE

                GroupMember.SaveHook.PreSave runs IsValidGroupMember on every non-archived,
                non-inactive member it is about to save and THROWS GroupMemberValidationException
                when it fails (role-max, hard group capacity, unmet add requirements). Under our
                single per-batch SaveChanges one such member would abort the entire batch (every
                person becomes an error). Re-check the members this pass mutated and detach any
                that would be rejected, mirroring ApplyStepModify, so the bad row is reported as a
                per-person warning and the rest commit. This is an intentional improvement over
                WebForms, whose Update branch let the save throw.

                Reason: Isolate a per-member validation failure without failing the batch.
            */
            var skippedMembers = new List<GroupMember>();
            foreach ( var member in mutatedMembers )
            {
                if ( member.IsArchived || member.GroupMemberStatus == GroupMemberStatus.Inactive )
                {
                    continue;
                }

                if ( member.IsValidGroupMember( rockContext ) )
                {
                    continue;
                }

                var validationMessage = string.Join( ", ", member.ValidationResults.Select( r => r.ErrorMessage ) );
                outcomes.RecordIssue( member.PersonId, validationMessage );

                rockContext.Entry( member ).State = EntityState.Detached;
                skippedMembers.Add( member );
            }

            rockContext.SaveChanges();

            // Detached members were dropped from the unit of work; skip their attribute writes too.
            var membersForAttributes = skippedMembers.Count == 0
                ? members
                : members.Where( m => !skippedMembers.Contains( m ) ).ToList();

            ApplyGroupMemberAttributes( membersForAttributes, rockContext );
        }

        /// <summary>
        /// Applies the submitted group-member attribute values to <paramref name="members"/>,
        /// fenced by <see cref="BulkUpdateSettings.AuthorizedGroupMemberAttributes"/> (the
        /// <c>ShowOnBulk</c> set for the authorized group). The shared
        /// <see cref="ApplyAttributeValues{T}"/> does the load / diff / save.
        /// </summary>
        private void ApplyGroupMemberAttributes( List<GroupMember> members, RockContext rockContext )
        {
            ApplyAttributeValues( members, _bag.GroupUpdate.MemberAttributes, _settings.AuthorizedGroupMemberAttributes, rockContext );
        }

        #endregion

        #region Pipeline: Step

        /// <summary>
        /// Adds, removes, or modifies steps of the authorized type
        /// (<see cref="BulkUpdateSettings.AuthorizedStepTypeId"/>) for the selected persons,
        /// dispatching on <see cref="BulkUpdateStepBag.Action"/> across the Add, Remove, and
        /// Modify branches. Per-person / per-step failures (Allow Multiple, unmet
        /// prerequisites, validation) surface as per-person issues rather than aborting the
        /// batch. Skipped entirely when no step type is authorized.
        /// </summary>
        private void ApplyStep( List<Person> persons, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            if ( _bag.StepUpdate == null || !_settings.AuthorizedStepTypeId.HasValue || persons.Count == 0 )
            {
                return;
            }

            var stepTypeId = _settings.AuthorizedStepTypeId.Value;

            if ( _bag.StepUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove )
            {
                ApplyStepRemove( persons, stepTypeId, rockContext );
            }
            else if ( _bag.StepUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Add )
            {
                ApplyStepAdd( persons, stepTypeId, rockContext, outcomes );
            }
            else if ( _bag.StepUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Update )
            {
                ApplyStepModify( persons, stepTypeId, rockContext, outcomes );
            }
        }

        /// <summary>
        /// Removes every step of the authorized type held by any selected person in a single
        /// <c>DeleteRange</c> + <c>SaveChanges</c> for the whole batch.
        /// </summary>
        private void ApplyStepRemove( List<Person> persons, int stepTypeId, RockContext rockContext )
        {
            var personIds = persons.Select( p => p.Id ).ToList();
            var stepService = new StepService( rockContext );

            // Unmaterialized alias subquery so EF emits a single IN ( SELECT ... ).
            var aliasIdsForPersons = GetPersonAliasIdsQuery( personIds, rockContext );

            var stepsToRemove = stepService.Queryable()
                .Where( s => s.StepTypeId == stepTypeId && aliasIdsForPersons.Contains( s.PersonAliasId ) )
                .ToList();

            if ( stepsToRemove.Count == 0 )
            {
                return;
            }

            // Materialize before DeleteRange to avoid modifying the set during enumeration
            // (matches the Following / Tag delete pattern).
            stepService.DeleteRange( stepsToRemove );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Adds a step of the authorized type to each selected person. Persons who cannot
        /// receive another step (Allow Multiple), who have unmet prerequisites, or whose
        /// step fails validation are skipped with a warning. Valid steps are added and
        /// committed in one <c>SaveChanges</c>, then their attribute values are written —
        /// the two-phase save the new Step.Id requires.
        /// </summary>
        private void ApplyStepAdd( List<Person> persons, int stepTypeId, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            var stepService = new StepService( rockContext );
            var stepType = new StepTypeService( rockContext ).Get( stepTypeId );
            if ( stepType == null )
            {
                return;
            }

            var stepStatus = ResolveStepStatus( _bag.StepUpdate.StepStatus?.Value, rockContext );
            var stepStatusId = stepStatus?.Id;
            var isCompleteStatus = stepStatus?.IsCompleteStatus ?? false;
            var campusId = ResolveCampusId( _bag.StepUpdate.Campus?.Value );
            var startDate = _bag.StepUpdate.StartDate;
            var endDate = stepType.HasEndDate ? _bag.StepUpdate.EndDate : null;
            var note = _bag.StepUpdate.Note;

            var addedSteps = new List<Step>();

            foreach ( var person in persons )
            {
                if ( !person.PrimaryAliasId.HasValue )
                {
                    continue;
                }

                var step = new Step
                {
                    StepTypeId = stepType.Id,
                    PersonAliasId = person.PrimaryAliasId.Value,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    StepStatusId = stepStatusId,
                    CampusId = campusId,
                    Note = note
                };

                if ( isCompleteStatus )
                {
                    // A completed status stamps the completion date from the step's dates.
                    step.CompletedDateTime = step.EndDateTime ?? step.StartDateTime;
                }

                if ( !stepService.CanAddBecauseMeetsAllowMultipleRule( person.PrimaryAliasId.Value, stepType ) )
                {
                    outcomes.RecordIssue( person.Id, $"Not able to complete {stepType.Name} again because of the 'Allow Multiple' setting." );
                    continue;
                }

                if ( !stepService.CanAddBecausePrereqsAreMet( person.PrimaryAliasId.Value, stepType ) )
                {
                    outcomes.RecordIssue( person.Id, $"Not able to complete {stepType.Name} as there are unmet prerequisites." );
                    continue;
                }

                if ( !step.IsValid )
                {
                    var validationMessage = string.Join( ", ", step.ValidationResults.Select( r => r.ErrorMessage ) );
                    outcomes.RecordIssue( person.Id, validationMessage );
                    continue;
                }

                stepService.Add( step );
                addedSteps.Add( step );
            }

            rockContext.SaveChanges();

            ApplyStepAttributes( addedSteps, rockContext );
        }

        /// <summary>
        /// Modifies the existing steps of the authorized type held by the selected persons.
        /// Each field (status, start/end date, campus, note) is applied only when its toggle
        /// is set in <see cref="BulkUpdateStepBag.UpdatedFields"/>. The completion date is
        /// recomputed from the (possibly updated) status whenever a status- or date-bearing
        /// field changed. Steps that fail validation are detached so the batch commit skips
        /// them, and are surfaced as per-person issues. Like the other branches this runs
        /// once per batch.
        /// </summary>
        private void ApplyStepModify( List<Person> persons, int stepTypeId, RockContext rockContext, BatchOutcomeTracker outcomes )
        {
            var stepUpdatedFields = new UpdatedFieldFlags( _bag.StepUpdate.UpdatedFields );

            var personIds = persons.Select( p => p.Id ).ToList();
            var stepService = new StepService( rockContext );

            var aliasIdsForPersons = GetPersonAliasIdsQuery( personIds, rockContext );

            var steps = stepService.Queryable()
                .Where( s => s.StepTypeId == stepTypeId && aliasIdsForPersons.Contains( s.PersonAliasId ) )
                .ToList();

            if ( steps.Count == 0 )
            {
                return;
            }

            // Resolve the scalar values once; they are constant across the batch.
            var submittedStatusId = ResolveStepStatus( _bag.StepUpdate.StepStatus?.Value, rockContext )?.Id;
            var campusId = ResolveCampusId( _bag.StepUpdate.Campus?.Value );
            var note = _bag.StepUpdate.Note;
            var startDate = _bag.StepUpdate.StartDate;
            var endDate = _bag.StepUpdate.EndDate;

            var changesStartDate = stepUpdatedFields.IsActive( StepUpdatedFieldKey.StartDate );
            var changesEndDate = stepUpdatedFields.IsActive( StepUpdatedFieldKey.EndDate );
            var changesStatus = stepUpdatedFields.IsActive( StepUpdatedFieldKey.Status ) && submittedStatusId.HasValue;
            var changesCampus = stepUpdatedFields.IsActive( StepUpdatedFieldKey.Campus );
            var changesNote = stepUpdatedFields.IsActive( StepUpdatedFieldKey.Note );

            // A date- or status-bearing change means the completion date must be recomputed.
            var recomputeCompletionDate = changesStartDate || changesEndDate || changesStatus;

            foreach ( var step in steps )
            {
                if ( changesCampus )
                {
                    step.CampusId = campusId;
                }

                if ( changesNote )
                {
                    step.Note = note;
                }

                if ( changesEndDate )
                {
                    step.EndDateTime = endDate;
                }

                if ( changesStartDate )
                {
                    step.StartDateTime = startDate;
                }

                if ( changesStatus )
                {
                    step.StepStatusId = submittedStatusId;
                }
            }

            if ( recomputeCompletionDate )
            {
                RecomputeStepCompletionDates( steps, rockContext );
            }

            var validSteps = new List<Step>( steps.Count );

            // Resolved lazily: the alias -> person map is only needed to attribute an issue
            // when a step fails validation, which is the uncommon path. The common all-valid
            // batch issues no extra query.
            Dictionary<int, int> personIdByAliasId = null;

            foreach ( var step in steps )
            {
                if ( step.IsValid )
                {
                    validSteps.Add( step );
                    continue;
                }

                /*
                    5/28/2026 - MSE

                    Step's date-order rules (StartDateTime vs End / Completed) live only in the
                    Step.IsValid getter. Step does not implement IValidatableObject and EF's
                    SaveChanges validation only covers DataAnnotations, so EF never evaluates
                    those rules: an invalid step left tracked would be silently committed by the
                    single batch SaveChanges (EF saves every tracked Modified entity regardless
                    of the validSteps list). Detach it to drop its bad values from the batch
                    commit and surface it as a per-step warning instead.

                    Reason: Keep a date-invalid step out of the shared save without failing the batch.
                */
                personIdByAliasId = personIdByAliasId ?? new PersonAliasService( rockContext ).Queryable()
                    .Where( a => personIds.Contains( a.PersonId ) )
                    .ToDictionary( a => a.Id, a => a.PersonId );

                var validationMessage = string.Join( ", ", step.ValidationResults.Select( r => r.ErrorMessage ) );
                if ( personIdByAliasId.TryGetValue( step.PersonAliasId, out var personId ) )
                {
                    outcomes.RecordIssue( personId, validationMessage );
                }

                rockContext.Entry( step ).State = EntityState.Detached;
            }

            rockContext.SaveChanges();

            ApplyStepAttributes( validSteps, rockContext );
        }

        /// <summary>
        /// Applies the submitted step attribute values to <paramref name="steps"/>, fenced by
        /// <see cref="BulkUpdateSettings.AuthorizedStepAttributes"/>. The Update branch's
        /// per-attribute opt-in is enforced client-side (only toggled attributes appear in the
        /// submitted set), so no extra "was this field selected" check is needed here. The
        /// shared <see cref="ApplyAttributeValues{T}"/> does the load / diff / save.
        /// </summary>
        private void ApplyStepAttributes( List<Step> steps, RockContext rockContext )
        {
            ApplyAttributeValues( steps, _bag.StepUpdate.StepAttributes, _settings.AuthorizedStepAttributes, rockContext );
        }

        /// <summary>
        /// Recomputes <see cref="Step.CompletedDateTime"/> for each step that carries a
        /// status: cleared when the status is not a completed status, otherwise stamped from
        /// the step's end date (or start date when there is no end date). Resolves the
        /// <c>IsCompleteStatus</c> flag for every distinct status in one query — there is no
        /// StepStatusCache to hit. Steps without a status are left untouched.
        /// </summary>
        private static void RecomputeStepCompletionDates( List<Step> steps, RockContext rockContext )
        {
            var statusIds = steps
                .Where( s => s.StepStatusId.HasValue )
                .Select( s => s.StepStatusId.Value )
                .Distinct()
                .ToList();

            if ( statusIds.Count == 0 )
            {
                return;
            }

            var isCompleteByStatusId = new StepStatusService( rockContext ).Queryable()
                .Where( s => statusIds.Contains( s.Id ) )
                .Select( s => new { s.Id, s.IsCompleteStatus } )
                .ToDictionary( s => s.Id, s => s.IsCompleteStatus );

            foreach ( var step in steps )
            {
                if ( !step.StepStatusId.HasValue )
                {
                    continue;
                }

                if ( isCompleteByStatusId.TryGetValue( step.StepStatusId.Value, out var isComplete ) && isComplete )
                {
                    step.CompletedDateTime = step.EndDateTime ?? step.StartDateTime;
                }
                else
                {
                    step.CompletedDateTime = null;
                }
            }
        }

        #endregion

        #region Pipeline: Tag

        /// <summary>
        /// Adds or removes <see cref="TaggedItem"/> rows for the selected persons against
        /// <see cref="BulkUpdateSettings.AuthorizedTagId"/>. Both branches resolve their full
        /// row set in one query (pre-query dedup for Add, an <c>(TagId, EntityGuid IN ...)</c>
        /// query for Remove) and commit with a single <c>SaveChanges</c>, so the piece runs
        /// inside the per-batch transaction. Skipped entirely when no Tag is authorized.
        /// </summary>
        private void ApplyTag( List<Person> persons, RockContext rockContext )
        {
            if ( _bag.TagUpdate == null
                || !_settings.AuthorizedTagId.HasValue
                || persons.Count == 0 )
            {
                return;
            }

            var tagId = _settings.AuthorizedTagId.Value;
            var personGuids = persons.Select( p => p.Guid ).ToList();

            var taggedItemService = new TaggedItemService( rockContext );

            if ( _bag.TagUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Add )
            {
                var existingTaggedGuids = new HashSet<Guid>(
                    taggedItemService.Queryable()
                        .Where( t => t.TagId == tagId && personGuids.Contains( t.EntityGuid ) )
                        .Select( t => t.EntityGuid ) );

                var taggedItemsToAdd = new List<TaggedItem>();
                foreach ( var personGuid in personGuids )
                {
                    if ( existingTaggedGuids.Contains( personGuid ) )
                    {
                        continue;
                    }

                    taggedItemsToAdd.Add( new TaggedItem
                    {
                        TagId = tagId,
                        EntityTypeId = _personEntityTypeId,
                        EntityGuid = personGuid
                    } );
                }

                if ( taggedItemsToAdd.Count > 0 )
                {
                    taggedItemService.AddRange( taggedItemsToAdd );
                    rockContext.SaveChanges();
                }
            }
            else if ( _bag.TagUpdate.Action == Rock.Enums.Crm.BulkUpdateActionSpecifier.Remove )
            {
                var taggedItemsToDelete = taggedItemService.Queryable()
                    .Where( t => t.TagId == tagId && personGuids.Contains( t.EntityGuid ) )
                    .ToList();

                if ( taggedItemsToDelete.Count > 0 )
                {
                    taggedItemService.DeleteRange( taggedItemsToDelete );
                    rockContext.SaveChanges();
                }
            }
        }

        #endregion

        #region Pipeline: Workflow

        /// <summary>
        /// Enqueues a <see cref="LaunchWorkflowsTransaction"/> for each authorized
        /// WorkflowType (<see cref="BulkUpdateSettings.AuthorizedWorkflowTypeIds"/>),
        /// launching one workflow per (workflow type × selected person). The transactions
        /// are fire-and-forget — the workflows are activated later by the transaction
        /// queue — so this piece intentionally does not touch the per-batch
        /// <see cref="RockContext"/> or share its name with the other pipeline methods.
        /// </summary>
        private void ApplyWorkflows( List<Person> persons )
        {
            if ( _settings.AuthorizedWorkflowTypeIds == null
                || _settings.AuthorizedWorkflowTypeIds.Count == 0
                || persons.Count == 0 )
            {
                return;
            }

            var workflowDetails = persons.Select( p => new LaunchWorkflowDetails( p ) ).ToList();

            foreach ( var workflowTypeId in _settings.AuthorizedWorkflowTypeIds )
            {
                var launchWorkflowsTransaction = new LaunchWorkflowsTransaction( workflowTypeId, workflowDetails )
                {
                    InitiatorPersonAliasId = _settings.CurrentPersonAliasId
                };

                // Enqueue is already async and caches WorkflowTypeCache.Get internally;
                // do not await or inline the launch.
                launchWorkflowsTransaction.Enqueue();
            }
        }

        #endregion

        #endregion Pipeline

        #region Helpers

        /// <summary>
        /// Applies the submitted attribute values to a batch of attribute-bearing entities,
        /// fenced by the authorized attribute set. Keeps only the submitted keys that survive
        /// the fence, loads the batch's attributes in one round trip, writes only changed
        /// values (trimmed comparison, converting the client's public "edit" value to the
        /// private database representation), and commits per entity via
        /// <c>SaveAttributeValues</c>. Shared by the Person, group-member, and Step attribute
        /// pipelines. No-op when there are no entities, no submitted values, or none survive
        /// the fence.
        /// </summary>
        private static void ApplyAttributeValues<T>( List<T> entities, IDictionary<string, string> submittedValues, IReadOnlyDictionary<string, AttributeCache> authorizedAttributes, RockContext rockContext )
            where T : class, IHasAttributes, new()
        {
            if ( entities.Count == 0
                || submittedValues == null || submittedValues.Count == 0
                || authorizedAttributes == null || authorizedAttributes.Count == 0 )
            {
                return;
            }

            // Keep only the submitted keys that survive the authorization fence.
            var attributesToApply = new Dictionary<string, AttributeCache>();
            foreach ( var key in submittedValues.Keys )
            {
                if ( authorizedAttributes.TryGetValue( key, out var attribute ) )
                {
                    attributesToApply[key] = attribute;
                }
            }

            if ( attributesToApply.Count == 0 )
            {
                return;
            }

            // Single round trip to load every entity's attribute set (required for
            // GetAttributeValue / SetAttributeValue to operate against the in-memory
            // AttributeValues dictionary).
            entities.LoadAttributes( rockContext );

            foreach ( var entity in entities )
            {
                var hasChanges = false;

                foreach ( var kvp in attributesToApply )
                {
                    // Convert the client's public "edit" value to the private database
                    // representation before comparing and storing (matches the standard
                    // SetPublicAttributeValues path).
                    var newValue = PublicAttributeHelper.GetPrivateValue( kvp.Value, submittedValues[kvp.Key] ?? string.Empty );
                    var originalValue = entity.GetAttributeValue( kvp.Key ) ?? string.Empty;

                    if ( originalValue.Trim() != newValue.Trim() )
                    {
                        entity.SetAttributeValue( kvp.Key, newValue );
                        hasChanges = true;
                    }
                }

                if ( hasChanges )
                {
                    entity.SaveAttributeValues( rockContext );
                }
            }
        }

        /// <summary>
        /// Builds an unmaterialized query of the PersonAlias ids belonging to the given
        /// persons, so a consuming query can embed it as a single <c>IN ( SELECT ... )</c>
        /// subquery rather than materializing the id list. Used by the Step Remove / Modify
        /// branches to scope steps across every alias of each selected person.
        /// </summary>
        private static IQueryable<int> GetPersonAliasIdsQuery( List<int> personIds, RockContext rockContext )
        {
            return new PersonAliasService( rockContext ).Queryable()
                .Where( a => personIds.Contains( a.PersonId ) )
                .Select( a => a.Id );
        }

        /// <summary>
        /// Resolves the submitted primary-alias GUIDs to person identifiers in a single
        /// query. Missing aliases are dropped silently; the caller can compare
        /// <see cref="BulkUpdateResultBag.TotalCount"/> against the request size to detect
        /// the gap.
        /// </summary>
        private List<int> ResolvePersonIds( IEnumerable<BulkUpdatePersonBag> personBags )
        {
            var aliasGuids = personBags
                .Where( p => p.PersonAliasGuid != Guid.Empty )
                .Select( p => p.PersonAliasGuid )
                .Distinct()
                .ToList();

            if ( aliasGuids.Count == 0 )
            {
                return new List<int>();
            }

            using ( var rockContext = new RockContext() )
            {
                return new PersonAliasService( rockContext ).Queryable()
                    .Where( pa => aliasGuids.Contains( pa.Guid ) )
                    .Select( pa => pa.PersonId )
                    .Distinct()
                    .ToList();
            }
        }

        /// <summary>
        /// Resolves the effective MaxDegreeOfParallelism. Honors the configured
        /// <see cref="BulkUpdateSettings.TaskCount"/>; falls back to
        /// <see cref="Environment.ProcessorCount"/> when blank or non-positive; caps at
        /// <see cref="MaxAllowedTaskCount"/>.
        /// </summary>
        private int ResolveTaskCount()
        {
            var requested = _settings.TaskCount.GetValueOrDefault();
            if ( requested < 1 )
            {
                requested = Environment.ProcessorCount;
            }

            return Math.Min( requested, MaxAllowedTaskCount );
        }

        /// <summary>
        /// Resolves a DefinedValue GUID to its integer identifier via
        /// <see cref="DefinedValueCache"/>. Returns <c>null</c> when the GUID is null,
        /// <see cref="Guid.Empty"/>, or does not match a cached value.
        /// </summary>
        private static int? ResolveDefinedValueId( Guid? definedValueGuid )
        {
            if ( !definedValueGuid.HasValue || definedValueGuid.Value == Guid.Empty )
            {
                return null;
            }

            return DefinedValueCache.Get( definedValueGuid.Value )?.Id;
        }

        /// <summary>
        /// Resolves a campus GUID string to its integer identifier via
        /// <see cref="CampusCache"/>. Returns <c>null</c> when the string is null/blank,
        /// <see cref="Guid.Empty"/>, or does not match a cached campus.
        /// </summary>
        private static int? ResolveCampusId( string campusGuidString )
        {
            var campusGuid = campusGuidString.AsGuidOrNull();
            if ( !campusGuid.HasValue || campusGuid.Value == Guid.Empty )
            {
                return null;
            }

            return CampusCache.Get( campusGuid.Value )?.Id;
        }

        /// <summary>
        /// Resolves a submitted step-status GUID string to its <see cref="StepStatus"/>
        /// entity (for the Id and the <c>IsCompleteStatus</c> flag), or <c>null</c> when the
        /// string is null/blank or not a GUID. There is no StepStatusCache, so this is a
        /// service lookup.
        /// </summary>
        private static StepStatus ResolveStepStatus( string statusGuidString, RockContext rockContext )
        {
            var statusGuid = statusGuidString.AsGuidOrNull();
            if ( !statusGuid.HasValue )
            {
                return null;
            }

            return new StepStatusService( rockContext ).Get( statusGuid.Value );
        }

        /// <summary>
        /// Resolves a group-role GUID string to its integer identifier within the supplied
        /// group type. Returns <c>null</c> when the string is null/blank, not a GUID, or
        /// names a role that does not belong to the group type.
        /// </summary>
        private static int? ResolveGroupRoleId( GroupTypeCache groupTypeCache, string roleGuidString )
        {
            var roleGuid = roleGuidString.AsGuidOrNull();
            if ( !roleGuid.HasValue )
            {
                return null;
            }

            return groupTypeCache.Roles.FirstOrDefault( r => r.Guid == roleGuid.Value )?.Id;
        }

        /// <summary>
        /// Projection target for the (PersonId, GroupId) pair returned by the Family
        /// Campus pipeline's family-membership query. A named type so the query's intent
        /// is visible in stack traces and the debugger; the SQL projection is identical
        /// to the anonymous-type form.
        /// </summary>
        private sealed class FamilyMembership
        {
            public int PersonId { get; set; }

            public int GroupId { get; set; }
        }

        /// <summary>
        /// Collects per-person issues (requested actions that could not be applied) within a
        /// single batch, keyed by person id. <see cref="ProcessBatch"/> reads these after the
        /// pipeline runs and translates them into the result's outcome buckets. A batch is
        /// processed by a single thread, so no synchronization is needed.
        /// </summary>
        private sealed class BatchOutcomeTracker
        {
            private readonly Dictionary<int, List<string>> _issuesByPersonId = new Dictionary<int, List<string>>();

            /// <summary>
            /// Records a reason a requested action could not be applied to the person. The
            /// reason must not include the person's name; the result carries the name
            /// separately so the UI can render and link it.
            /// </summary>
            public void RecordIssue( int personId, string issue )
            {
                if ( !_issuesByPersonId.TryGetValue( personId, out var issues ) )
                {
                    issues = new List<string>();
                    _issuesByPersonId[personId] = issues;
                }

                issues.Add( issue );
            }

            /// <summary>
            /// Gets the issues recorded for the person, if any were.
            /// </summary>
            public bool TryGetIssues( int personId, out List<string> issues )
            {
                return _issuesByPersonId.TryGetValue( personId, out issues );
            }
        }

        /// <summary>
        /// Lightweight wrapper that lets the pipeline ask "is this field toggled on?" in
        /// one line, treating both a missing key and a <c>false</c> value as "leave alone."
        /// </summary>
        private sealed class UpdatedFieldFlags
        {
            private readonly Dictionary<string, bool> _inner;

            public UpdatedFieldFlags( Dictionary<string, bool> source )
            {
                _inner = source != null
                    ? new Dictionary<string, bool>( source, StringComparer.OrdinalIgnoreCase )
                    : new Dictionary<string, bool>( StringComparer.OrdinalIgnoreCase );
            }

            public bool HasAny()
            {
                return _inner.Any( kvp => kvp.Value );
            }

            public bool IsActive( string key )
            {
                return _inner.TryGetValue( key, out var value ) && value;
            }
        }

        #endregion
    }
}
