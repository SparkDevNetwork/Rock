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
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Humanizer;
using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that executes routine cleanup tasks on Rock.
    /// Cleanup tasks are tasks that fixes (add, update or purge) invalid, missing or obsolete data.
    /// </summary>
    [DisplayName( "Rock Cleanup" )]
    [Description( "General job to clean up various areas of Rock." )]

    [IntegerField( "Days to Keep Exceptions in Log",
        Description = "The number of days to keep exceptions in the exception log (default is 14 days.)",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = "General",
        Order = 1,
        Key = AttributeKey.DaysKeepExceptions )]

    [IntegerField( "Audit Log Expiration Days",
        Description = "The number of days to keep items in the audit log (default is 14 days.)",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = "General",
        Order = 2,
        Key = AttributeKey.AuditLogExpirationDays )]

    [IntegerField( "Days to Keep Cached Files",
        Description = "The number of days to keep cached files in the cache folder (default is 14 days.)",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = "General",
        Order = 3,
        Key = AttributeKey.DaysKeepCachedFiles )]

    [TextField( "Base Cache Folder",
        Key = AttributeKey.BaseCacheDirectory,
        Description = "The base/starting Directory for the file cache (default is ~/Cache.)",
        IsRequired = false,
        DefaultValue = "~/Cache",
        Category = "General",
        Order = 4 )]

    [IntegerField( "Max Metaphone Names",
        Key = AttributeKey.MaxMetaphoneNames,
        Description = "The maximum number of person names to process metaphone values for each time job is run (only names that have not yet been processed are checked).",
        IsRequired = false,
        DefaultIntegerValue = 500,
        Category = "General",
        Order = 5 )]

    [IntegerField( "Batch Cleanup Amount",
        Key = AttributeKey.BatchCleanupAmount,
        Description = "The number of records to delete at a time dependent on infrastructure. Recommended range is 1000 to 10,000.",
        IsRequired = false,
        DefaultIntegerValue = 1000,
        Category = "General",
        Order = 6 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (900). Note, some operations could take several minutes, so you might want to set it at 900 (15 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = 60 * 15,
        Category = "General",
        Order = 7 )]

    [DisallowConcurrentExecution]
    public class RockCleanup : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DaysKeepExceptions = "DaysKeepExceptions";
            public const string AuditLogExpirationDays = "AuditLogExpirationDays";
            public const string DaysKeepCachedFiles = "DaysKeepCachedFiles";
            public const string BaseCacheDirectory = "BaseCacheDirectory";
            public const string MaxMetaphoneNames = "MaxMetaphoneNames";
            public const string BatchCleanupAmount = "BatchCleanupAmount";
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RockCleanup()
        {
        }

        private List<RockCleanupJobResult> rockCleanupJobResultList = new List<RockCleanupJobResult>();
        private IJobExecutionContext jobContext;

        private int commandTimeout;
        private int batchAmount;

        /// <summary>
        /// Job that executes routine Rock cleanup tasks
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            jobContext = context;

            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;

            batchAmount = dataMap.GetString( AttributeKey.BatchCleanupAmount ).AsIntegerOrNull() ?? 1000;
            commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 900;

            /* IMPORTANT!! MDP 2020-05-05

            1 ) Whenever you do a new RockContext() in RockCleanup make sure to set the commandtimeout, like this:

                var rockContext = new RockContext();
                rockContext.Database.CommandTimeout = commandTimeout;

            2) The cleanupTitle parameter on RunCleanupTask should short. The should be short enough so that the summary of all job tasks
               only shows a one line summary of each task (doesn't wrap)

            3) The cleanupTitle parameter should be in {Verb} [adjective] {noun} format (look below for examples)


            */

            RunCleanupTask( "exception log", () => this.CleanupExceptionLog( dataMap ) );

            RunCleanupTask( "expired entity set", () => CleanupExpiredEntitySets( dataMap ) );

            RunCleanupTask( "median page load time", () => UpdateMedianPageLoadTimes() );

            RunCleanupTask( "old interaction", () => CleanupOldInteractions( dataMap ) );

            RunCleanupTask( "unused interaction session", () => CleanupUnusedInteractionSessions() );

            RunCleanupTask( "audit log", () => PurgeAuditLog( dataMap ) );

            RunCleanupTask( "cached file", () => CleanCachedFileDirectory( context, dataMap ) );

            RunCleanupTask( "temporary binary file", () => CleanupTemporaryBinaryFiles() );

            // updates missing person aliases, metaphones, etc (doesn't delete any records)
            RunCleanupTask( "person", () => PersonCleanup( dataMap ) );

            RunCleanupTask( "anonymous giver login", () => RemoveAnonymousGiverUserLogins() );

            RunCleanupTask( "temporary registration", () => CleanUpTemporaryRegistrations() );

            RunCleanupTask( "workflow log", () => CleanUpWorkflowLogs( dataMap ) );

            // Note run Workflow Log Cleanup before Workflow Cleanup to avoid timing out if a Workflow has lots of workflow logs (there is a cascade delete)
            RunCleanupTask( "workflow", () => CleanUpWorkflows( dataMap ) );

            RunCleanupTask( "unused attribute value", () => CleanupOrphanedAttributes( dataMap ) );

            RunCleanupTask( "transient communication", () => CleanupTransientCommunications( dataMap ) );

            RunCleanupTask( "financial transaction", () => CleanupFinancialTransactionNullCurrency( dataMap ) );

            RunCleanupTask( "person token", () => CleanupPersonTokens( dataMap ) );

            // Reduce the job history to max size
            RunCleanupTask( "job history", () => CleanupJobHistory() );

            // Search for and delete group memberships duplicates (same person, group, and role)
            RunCleanupTask( "group membership", () => GroupMembershipCleanup() );

            RunCleanupTask( "attendance label data", () => AttendanceDataCleanup( dataMap ) );

            // Search for locations with no country and assign USA or Canada if it match any of the country's states
            RunCleanupTask( "location", () => LocationCleanup( dataMap ) );

            // Does any cleanup on AttributeValue, such as making sure as ValueAsNumeric column has the correct value
            RunCleanupTask( "attribute value", () => CleanupAttributeValues( dataMap ) );

            RunCleanupTask( "merge streak data", () => MergeStreaks() );

            RunCleanupTask( "refresh streak data", () => RefreshStreaksDenormalizedData() );

            RunCleanupTask( "validate schedule", () => EnsureScheduleEffectiveStartEndDates() );

            RunCleanupTask( "set nameless SMS response", () => EnsureNamelessPersonForSMSResponses() );

            RunCleanupTask( "merge nameless to person", () => MatchNamelessPersonToRegularPerson() );

            //// ***********************
            ////  Final count and report
            //// ***********************

            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine( string.Empty );
            foreach ( var rockCleanupJobResult in rockCleanupJobResultList )
            {
                jobSummaryBuilder.AppendLine( GetFormattedResult( rockCleanupJobResult ) );
            }

            if ( rockCleanupJobResultList.Any( a => a.HasException ) )
            {
                jobSummaryBuilder.AppendLine( "\n<i class='fa fa-circle text-warning'></i> Some jobs have errors. See exception log for details." );
            }

            context.Result = jobSummaryBuilder.ToString();

            var rockCleanupExceptions = rockCleanupJobResultList.Where( a => a.HasException ).Select( a => a.Exception ).ToList();

            if ( rockCleanupExceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in RockCleanup.", rockCleanupExceptions );
                throw new RockJobWarningException( "RockCleanup completed with warnings", exceptionList );
            }
        }

        /// <summary>
        /// Get a cleanup job result as a formatted string
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetFormattedResult( RockCleanupJobResult result )
        {
            if ( result.HasException )
            {
                return $"<i class='fa fa-circle text-danger'></i> { result.Title}";
            }
            else
            {
                var icon = "<i class='fa fa-circle text-success'></i>";
                var title = result.Title.PluralizeIf( result.RowsAffected != 1 ).ApplyCase( LetterCasing.Title );
                return $"{icon} {result.RowsAffected} {title} ({result.Elapsed.TotalMilliseconds:N0}ms)";
            }
        }

        /// <summary>
        /// Runs the cleanup task.
        /// </summary>
        /// <param name="cleanupTitle">The cleanup title.</param>
        /// <param name="cleanupMethod">The cleanup method.</param>
        private void RunCleanupTask( string cleanupTitle, Func<int> cleanupMethod )
        {
            var stopwatch = new Stopwatch();
            try
            {
                jobContext.UpdateLastStatusMessage( $"{cleanupTitle.Pluralize().ApplyCase( LetterCasing.Title )}..." );
                stopwatch.Start();
                var cleanupRowsAffected = cleanupMethod();
                stopwatch.Stop();

                rockCleanupJobResultList.Add( new RockCleanupJobResult
                {
                    Title = cleanupTitle,
                    RowsAffected = cleanupRowsAffected,
                    Elapsed = stopwatch.Elapsed
                } );
            }
            catch ( Exception ex )
            {
                stopwatch.Stop();
                rockCleanupJobResultList.Add( new RockCleanupJobResult
                {
                    Title = cleanupTitle,
                    RowsAffected = 0,
                    Elapsed = stopwatch.Elapsed,
                    Exception = new RockCleanupException( cleanupTitle, ex )
                } );
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="System.Exception" />
        private class RockCleanupException : Exception
        {
            /// <summary>
            /// The exception
            /// </summary>
            private Exception _exception;

            /// <summary>
            /// The title
            /// </summary>
            private string _title;

            /// <summary>
            /// Initializes a new instance of the <see cref="RockCleanupException"/> class.
            /// </summary>
            /// <param name="title">The title.</param>
            /// <param name="ex">The ex.</param>
            public RockCleanupException( string title, Exception ex ) : base( ex.Message, ex.InnerException )
            {
                _title = title;
                _exception = ex;
            }

            /// <summary>
            /// Gets a message that describes the current exception.
            /// </summary>
            public override string Message => $"{_title}:{_exception.Message}";

            /// <summary>
            /// Gets a string representation of the immediate frames on the call stack.
            /// </summary>
            public override string StackTrace
            {
                get
                {
                    string stackTrace = _exception.StackTrace;
                    var innerException = _exception.InnerException;
                    while ( innerException != null )
                    {
                        stackTrace += "\n\n" + innerException.Message;
                        stackTrace += "\n" + innerException.StackTrace;
                        innerException = innerException.InnerException;
                    }

                    return stackTrace;
                }
            }
        }

        /// <summary>
        /// Does cleanup of Person Aliases and Metaphones
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int PersonCleanup( JobDataMap dataMap )
        {
            int resultCount = 0;

            // Add any missing person aliases
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;

                PersonService personService = new PersonService( personRockContext );
                PersonAliasService personAliasService = new PersonAliasService( personRockContext );
                var personAliasServiceQry = personAliasService.Queryable();
                foreach ( var person in personService.Queryable( "Aliases" )
                    .Where( p => !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                    .Take( 300 ) )
                {
                    person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                    resultCount++;
                }

                personRockContext.SaveChanges();
            }

            resultCount += AddMissingAlternateIds();

            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;

                PersonService personService = new PersonService( personRockContext );

                // Add any missing metaphones
                int namesToProcess = dataMap.GetString( AttributeKey.MaxMetaphoneNames ).AsIntegerOrNull() ?? 500;
                if ( namesToProcess > 0 )
                {
                    var firstNameQry = personService.Queryable().Select( p => p.FirstName ).Where( p => p != null );
                    var nickNameQry = personService.Queryable().Select( p => p.NickName ).Where( p => p != null );
                    var lastNameQry = personService.Queryable().Select( p => p.LastName ).Where( p => p != null );
                    var nameQry = firstNameQry.Union( nickNameQry.Union( lastNameQry ) );

                    var metaphones = personRockContext.Metaphones;
                    var existingNames = metaphones.Select( m => m.Name ).Distinct();

                    // Get the names that have not yet been processed
                    var namesToUpdate = nameQry
                        .Where( n => !existingNames.Contains( n ) )
                        .Take( namesToProcess )
                        .ToList();

                    foreach ( string name in namesToUpdate )
                    {
                        string mp1 = string.Empty;
                        string mp2 = string.Empty;
                        Rock.Utility.DoubleMetaphone.doubleMetaphone( name, ref mp1, ref mp2 );

                        var metaphone = new Metaphone();
                        metaphone.Name = name;
                        metaphone.Metaphone1 = mp1;
                        metaphone.Metaphone2 = mp2;

                        metaphones.Add( metaphone );
                        resultCount++;
                    }

                    personRockContext.SaveChanges( disablePrePostProcessing: true );
                }
            }

            // Ensures the PrimaryFamily is correct for all person records in the database
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int primaryFamilyUpdates = PersonService.UpdatePrimaryFamilyAll( personRockContext );
                resultCount += primaryFamilyUpdates;
            }

            // Ensures the GivingLeaderId is correct for all person records in the database
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int givingLeaderUpdates = PersonService.UpdateGivingLeaderIdAll( personRockContext );
                resultCount += givingLeaderUpdates;
            }

            // Ensures the GivingId is correct for all person records in the database
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int givingLeaderUpdates = PersonService.UpdateGivingIdAll( personRockContext );
                resultCount += givingLeaderUpdates;
            }

            // update any updated or incorrect age classifications on persons
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int ageClassificationUpdates = PersonService.UpdatePersonAgeClassificationAll( personRockContext );
                resultCount += ageClassificationUpdates;
            }

            // update any PhoneNumber.FullNumber's that aren't correct.
            using ( var phoneNumberRockContext = new RockContext() )
            {
                phoneNumberRockContext.Database.CommandTimeout = commandTimeout;
                int phoneNumberUpdates = phoneNumberRockContext.Database.ExecuteSqlCommand( @"UPDATE [PhoneNumber] SET [FullNumber] = CONCAT([CountryCode], [Number]) where [FullNumber] is null OR [FullNumber] != CONCAT([CountryCode], [Number])" );
                resultCount += phoneNumberUpdates;
            }

            // update the BirthDate with a computed value
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                PersonService.UpdateBirthDateAll( personRockContext );
            }

            //// Add any missing Implied/Known relationship groups
            // Known Relationship Group
            resultCount += AddMissingRelationshipGroups( GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ), Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid(), commandTimeout );

            // Implied Relationship Group
            resultCount += AddMissingRelationshipGroups( GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK ), Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid(), commandTimeout );

            // Find family groups that have no members or that have only 'inactive' people (record status) and mark the groups inactive.
            using ( var familyRockContext = new Rock.Data.RockContext() )
            {
                familyRockContext.Database.CommandTimeout = commandTimeout;

                int familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                int recordStatusInactiveValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

                var activeFamilyWithNoActiveMembers = new GroupService( familyRockContext ).Queryable()
                    .Where( a => a.GroupTypeId == familyGroupTypeId && a.IsActive == true )
                    .Where( a => !a.Members.Where( m => m.Person.RecordStatusValueId != recordStatusInactiveValueId ).Any() );

                var currentDateTime = RockDateTime.Now;

                familyRockContext.BulkUpdate( activeFamilyWithNoActiveMembers, x => new Rock.Model.Group { IsActive = false } );
            }

            return resultCount;
        }

        /// <summary>
        /// Removes any UserLogin records associated with the Anonymous Giver.
        /// </summary>
        private int RemoveAnonymousGiverUserLogins()
        {
            int loginCount = 0;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                var userLoginService = new UserLoginService( rockContext );
                var anonymousGiver = new PersonService( rockContext ).GetOrCreateAnonymousGiverPerson();
                if ( anonymousGiver == null )
                {
                    return 0; // This shouldn't ever happen.
                }

                var logins = userLoginService.Queryable().Where( l => l.PersonId == anonymousGiver.Id ).ToList();

                loginCount = logins.Count();

                if ( loginCount > 0 )
                {
                    userLoginService.DeleteRange( logins );
                    rockContext.SaveChanges();
                }
            }

            return loginCount;
        }

        /// <summary>
        /// Adds any missing person alternate ids; limited to 150k records per run
        /// to avoid any possible memory issues. Processes about 150k records
        /// in 52 seconds.
        /// </summary>
        private static int AddMissingAlternateIds()
        {
            int resultCount = 0;
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                var personService = new PersonService( personRockContext );
                int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                var personSearchKeyService = new PersonSearchKeyService( personRockContext );
                var alternateKeyQuery = personSearchKeyService.Queryable().AsNoTracking().Where( a => a.SearchTypeValueId == alternateValueId );

                IQueryable<Person> personQuery = personService.Queryable( includeDeceased: true ).AsNoTracking();

                // Make a list of items that we're going to bulk insert.
                var itemsToInsert = new List<PersonSearchKey>();

                // Get all existing keys so we can keep track and quickly check them while we're bulk adding new ones.
                var keys = new HashSet<string>( personSearchKeyService.Queryable().AsNoTracking()
                    .Where( a => a.SearchTypeValueId == alternateValueId )
                    .Select( a => a.SearchValue )
                    .ToList() );

                string alternateId = string.Empty;

                // Find everyone who does not yet have an alternateKey.
                foreach ( var person in personQuery = personQuery
                    .Where( p => !alternateKeyQuery.Any( f => f.PersonAlias.PersonId == p.Id ) )
                    .Take( 150000 ) )
                {
                    // Regenerate key if it already exists.
                    do
                    {
                        alternateId = PersonSearchKeyService.GenerateRandomAlternateId();
                    } while ( keys.Contains( alternateId ) );

                    keys.Add( alternateId );

                    itemsToInsert.Add(
                        new PersonSearchKey()
                        {
                            PersonAliasId = person.PrimaryAliasId,
                            SearchTypeValueId = alternateValueId,
                            SearchValue = alternateId
                        } );
                }

                if ( itemsToInsert.Count > 0 )
                {
                    // Now add them in one bulk insert.
                    personRockContext.BulkInsert( itemsToInsert );
                }

                resultCount += itemsToInsert.Count;
            }

            return resultCount;
        }

        /// <summary>
        /// Adds the missing relationship groups.
        /// </summary>
        /// <param name="relationshipGroupType">Type of the relationship group.</param>
        /// <param name="ownerRoleGuid">The owner role unique identifier.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        private static int AddMissingRelationshipGroups( GroupTypeCache relationshipGroupType, Guid ownerRoleGuid, int commandTimeout )
        {
            int resultCount = 0;
            if ( relationshipGroupType != null )
            {
                var ownerRoleId = relationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( ownerRoleGuid ) ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                if ( ownerRoleId.HasValue )
                {
                    var rockContext = new RockContext();
                    rockContext.Database.CommandTimeout = commandTimeout;
                    var personService = new PersonService( rockContext );
                    var memberService = new GroupMemberService( rockContext );

                    var qryGroupOwnerPersonIds = memberService.Queryable( true )
                        .Where( m => m.GroupRoleId == ownerRoleId.Value ).Select( a => a.PersonId );

                    var personIdsWithoutKnownRelationshipGroup = personService.Queryable().Where( p => !qryGroupOwnerPersonIds.Contains( p.Id ) ).Select( a => a.Id ).ToList();

                    var groupsToInsert = new List<Group>();
                    var groupGroupMembersToInsert = new Dictionary<Guid, GroupMember>();
                    foreach ( var personId in personIdsWithoutKnownRelationshipGroup )
                    {
                        var groupMember = new GroupMember();
                        groupMember.PersonId = personId;
                        groupMember.GroupRoleId = ownerRoleId.Value;

                        var group = new Group();
                        group.Name = relationshipGroupType.Name;
                        group.Guid = Guid.NewGuid();
                        group.GroupTypeId = relationshipGroupType.Id;

                        groupGroupMembersToInsert.Add( group.Guid, groupMember );

                        groupsToInsert.Add( group );
                    }

                    if ( groupsToInsert.Any() )
                    {
                        // use BulkInsert just in case there are a large number of groups and group members to insert
                        rockContext.BulkInsert( groupsToInsert );

                        Dictionary<Guid, int> groupIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.GroupTypeId == relationshipGroupType.Id ).Select( a => new { a.Id, a.Guid } ).ToDictionary( k => k.Guid, v => v.Id );
                        var groupMembersToInsert = new List<GroupMember>();
                        foreach ( var groupGroupMember in groupGroupMembersToInsert )
                        {
                            var groupMember = groupGroupMember.Value;
                            groupMember.GroupId = groupIdLookup[groupGroupMember.Key];
                            groupMembersToInsert.Add( groupMember );
                        }

                        rockContext.BulkInsert( groupMembersToInsert );
                        resultCount += groupGroupMembersToInsert.Count();
                    }
                }
            }

            return resultCount;
        }

        /// <summary>
        /// Cleanups the temporary binary files.
        /// </summary>
        private int CleanupTemporaryBinaryFiles()
        {
            var binaryFileRockContext = new Rock.Data.RockContext();
            binaryFileRockContext.Database.CommandTimeout = commandTimeout;

            // clean out any temporary binary files
            BinaryFileService binaryFileService = new BinaryFileService( binaryFileRockContext );
            int resultCount = 0;
            foreach ( var binaryFile in binaryFileService.Queryable().Where( bf => bf.IsTemporary == true ).ToList() )
            {
                if ( binaryFile.ModifiedDateTime < RockDateTime.Now.AddDays( -1 ) )
                {
                    string errorMessage;
                    if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                    {
                        resultCount++;
                        binaryFileService.Delete( binaryFile );
                        binaryFileRockContext.SaveChanges();
                    }
                }
            }

            return resultCount;
        }

        /// <summary>
        /// Cleans up temporary registrations.
        /// </summary>
        private int CleanUpTemporaryRegistrations()
        {
            var registrationRockContext = new Rock.Data.RockContext();
            registrationRockContext.Database.CommandTimeout = commandTimeout;

            int totalRowsDeleted = 0;

            // clean out any temporary registrations
            RegistrationService registrationService = new RegistrationService( registrationRockContext );
            foreach ( var registration in registrationService.Queryable().Where( bf => bf.IsTemporary == true ).ToList() )
            {
                if ( registration.ModifiedDateTime < RockDateTime.Now.AddHours( -1 ) )
                {
                    string errorMessage;
                    if ( registrationService.CanDelete( registration, out errorMessage ) )
                    {
                        registrationService.Delete( registration );
                        registrationRockContext.SaveChanges();
                        totalRowsDeleted++;
                    }
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up completed workflows.
        /// </summary>
        private int CleanUpWorkflows( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            var workflowContext = new RockContext();
            workflowContext.Database.CommandTimeout = commandTimeout;

            var workflowService = new WorkflowService( workflowContext );

            var completedWorkflows = workflowService.Queryable().AsNoTracking()
                .Where( w => w.WorkflowType.CompletedWorkflowRetentionPeriod.HasValue && w.Status.Equals( "Completed" )
                && DateTime.Now > DbFunctions.AddDays( w.ModifiedDateTime, w.WorkflowType.CompletedWorkflowRetentionPeriod ) )
                .ToList();

            List<int> workflowIdsSafeToDelete = new List<int>();

            // Now, verify and build a list of workflow Ids that are OK to be deleted.
            // And start deleting them in batches
            foreach ( var workflow in completedWorkflows )
            {
                // Verify that the workflow is not being used by something important by letting CanDelete tell
                // us if it's OK to delete.
                if ( workflowService.CanDelete( workflow, out _ ) )
                {
                    workflowIdsSafeToDelete.Add( workflow.Id );
                }

                // to prevent a SQL complexity exception, do a bulk delete anytime the workflowIdsSafeToDelete gets too big
                if ( workflowIdsSafeToDelete.Count >= batchAmount )
                {
                    totalRowsDeleted += BulkDeleteInChunks( workflowService.Queryable().Where( a => workflowIdsSafeToDelete.Contains( a.Id ) ), batchAmount, commandTimeout );
                    workflowIdsSafeToDelete = new List<int>();
                }
            }

            if ( workflowIdsSafeToDelete.Any() )
            {
                totalRowsDeleted += BulkDeleteInChunks( workflowService.Queryable().Where( a => workflowIdsSafeToDelete.Contains( a.Id ) ), batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up workflow logs by removing logs in batches for workflows with a log retention period that has passed.
        /// </summary>
        private int CleanUpWorkflowLogs( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;

            // Limit the number of workflow logs to delete for this run (20M records could take around 20 minutes).
            int maxRowDeleteLimit = 20000000;
            var workflowContext = new RockContext();
            workflowContext.Database.CommandTimeout = commandTimeout;

            var workflowService = new WorkflowService( workflowContext );
            var workflowLogQuery = new WorkflowLogService( workflowContext ).Queryable();

            // Get the list of workflows that haven't been modified since X days
            // and have at least one workflow log (narrowing it down to ones with Logs improves performance of this cleanup)
            var workflowIdsOlderThanLogRetentionPeriodQuery = workflowService.Queryable()
                .Where( w =>
                    w.WorkflowType.LogRetentionPeriod.HasValue
                    && DateTime.Now > DbFunctions.AddDays( w.ModifiedDateTime, w.WorkflowType.LogRetentionPeriod )
                    && workflowLogQuery.Any( wl => wl.WorkflowId == w.Id ) )
                .Select( w => w.Id );

            // WorkflowLogService.CanDelete( log ) always returns true, so no need to check
            var workflowLogsToDeleteQuery = new WorkflowLogService( workflowContext ).Queryable().Where( a => workflowIdsOlderThanLogRetentionPeriodQuery.Contains( a.WorkflowId ) );
            BulkDeleteInChunks( workflowLogsToDeleteQuery, batchAmount, commandTimeout, maxRowDeleteLimit );

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans the cached file directory.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private int CleanCachedFileDirectory( IJobExecutionContext context, JobDataMap dataMap )
        {
            string cacheDirectoryPath = dataMap.GetString( AttributeKey.BaseCacheDirectory );
            int? cacheExpirationDays = dataMap.GetString( AttributeKey.DaysKeepCachedFiles ).AsIntegerOrNull();

            int resultCount = 0;
            if ( cacheExpirationDays.HasValue )
            {
                DateTime cacheExpirationDate = RockDateTime.Now.Add( new TimeSpan( cacheExpirationDays.Value * -1, 0, 0, 0 ) );

                // if job is being run by the IIS scheduler and path is not null
                if ( context.Scheduler.SchedulerName == "RockSchedulerIIS" && !string.IsNullOrEmpty( cacheDirectoryPath ) )
                {
                    // get the physical path of the cache directory
                    cacheDirectoryPath = System.Web.Hosting.HostingEnvironment.MapPath( cacheDirectoryPath );
                }

                // if directory is not blank and cache expiration date not in the future
                if ( !string.IsNullOrEmpty( cacheDirectoryPath ) && cacheExpirationDate <= RockDateTime.Now )
                {
                    // Clean cache directory
                    resultCount += CleanCacheDirectory( cacheDirectoryPath, cacheExpirationDate );
                }
            }

            return resultCount;
        }

        /// <summary>
        /// Purges the audit log.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int PurgeAuditLog( JobDataMap dataMap )
        {
            // purge audit log
            int totalRowsDeleted = 0;
            int? auditExpireDays = dataMap.GetString( AttributeKey.AuditLogExpirationDays ).AsIntegerOrNull();
            if ( auditExpireDays.HasValue )
            {
                var auditLogRockContext = new Rock.Data.RockContext();
                auditLogRockContext.Database.CommandTimeout = commandTimeout;

                DateTime auditExpireDate = RockDateTime.Now.Add( new TimeSpan( auditExpireDays.Value * -1, 0, 0, 0 ) );
                totalRowsDeleted += BulkDeleteInChunks( new AuditService( auditLogRockContext ).Queryable().Where( a => a.DateTime < auditExpireDate ), batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Uses the DaysKeepExceptions setting to remove old exception logs
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupExceptionLog( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            int? exceptionExpireDays = dataMap.GetString( AttributeKey.DaysKeepExceptions ).AsIntegerOrNull();
            if ( exceptionExpireDays.HasValue )
            {
                var exceptionLogRockContext = new Rock.Data.RockContext();

                // Assuming a 10 minute minimum CommandTimeout for this process.
                exceptionLogRockContext.Database.CommandTimeout = commandTimeout >= 600 ? commandTimeout : 600;
                DateTime exceptionExpireDate = RockDateTime.Now.Add( new TimeSpan( exceptionExpireDays.Value * -1, 0, 0, 0 ) );
                var exceptionLogsToDelete = new ExceptionLogService( exceptionLogRockContext ).Queryable().Where( a => a.CreatedDateTime < exceptionExpireDate );

                totalRowsDeleted = BulkDeleteInChunks( exceptionLogsToDelete, batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up expired entity sets.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupExpiredEntitySets( JobDataMap dataMap )
        {
            var entitySetRockContext = new RockContext();
            entitySetRockContext.Database.CommandTimeout = commandTimeout;

            var currentDateTime = RockDateTime.Now;
            var entitySetService = new EntitySetService( entitySetRockContext );

            var qry = entitySetService.Queryable().Where( a => a.ExpireDateTime.HasValue && a.ExpireDateTime < currentDateTime );
            int totalRowsDeleted = 0;

            foreach ( var entitySet in qry.ToList() )
            {
                string deleteWarning;
                if ( entitySetService.CanDelete( entitySet, out deleteWarning ) )
                {
                    var entitySetItemsToDeleteQuery = new EntitySetItemService( entitySetRockContext ).Queryable().Where( a => a.EntitySetId == entitySet.Id );
                    BulkDeleteInChunks( entitySetItemsToDeleteQuery, batchAmount, commandTimeout );
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up Interactions for Interaction Channels that have a retention period
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupOldInteractions( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            var currentDateTime = RockDateTime.Now;

            var interactionSessionIdsOfDeletedInteractions = new List<int>();
            var interactionChannelsWithRentionDurations = InteractionChannelCache.All().Where( ic => ic.RetentionDuration.HasValue );

            using ( var interactionRockContext = new Rock.Data.RockContext() )
            {
                interactionRockContext.Database.CommandTimeout = commandTimeout;

                foreach ( var interactionChannel in interactionChannelsWithRentionDurations )
                {
                    var retentionCutoffDateTime = currentDateTime.AddDays( -interactionChannel.RetentionDuration.Value );

                    if ( retentionCutoffDateTime < System.Data.SqlTypes.SqlDateTime.MinValue.Value )
                    {
                        continue;
                    }

                    var interactionsToDeleteQuery = new InteractionService( interactionRockContext ).Queryable().Where( i =>
                        i.InteractionComponent.InteractionChannelId == interactionChannel.Id &&
                        i.InteractionDateTime < retentionCutoffDateTime );

                    var interactionSessionIdsForInteractionChannel = interactionsToDeleteQuery
                        .Where( i => i.InteractionSessionId != null )
                        .Where( i => !interactionSessionIdsOfDeletedInteractions.Contains( i.Id ) )
                        .Select( i => ( int ) i.InteractionSessionId )
                        .Distinct()
                        .ToList();

                    interactionSessionIdsOfDeletedInteractions.AddRange( interactionSessionIdsForInteractionChannel );

                    totalRowsDeleted += BulkDeleteInChunks( interactionsToDeleteQuery, batchAmount, commandTimeout );
                }
            }

            if ( interactionSessionIdsOfDeletedInteractions.Any() )
            {
                RunCleanupTask( "Unused Interaction Session Cleanup", () => CleanupUnusedInteractionSessions( interactionSessionIdsOfDeletedInteractions ) );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleanups the unused interactions.
        /// </summary>
        /// <param name="interactionSessionIds">The interaction session ids.</param>
        /// <returns></returns>
        private int CleanupUnusedInteractionSessions( List<int> interactionSessionIds )
        {
            if ( !interactionSessionIds.Any() )
            {
                return 0;
            }

            int totalRowsDeleted = 0;
            var currentDateTime = RockDateTime.Now;

            // delete any InteractionSession records that are no longer used.
            var rockContext = new Rock.Data.RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            // process 1K at a time to prevent the exception "Query processor ran out of internal resources".
            for ( int x = 0; x < interactionSessionIds.Count / 1000; x++ )
            {
                var interactionSessionIdChunk = interactionSessionIds.Skip( x * 1000 ).Take( 1000 );

                // Find a list of session IDs in the delete list that are being used for other interactions
                var interactionSessionsIdsToKeep = new InteractionService( rockContext )
                    .Queryable()
                    .Where( s => interactionSessionIdChunk.Contains( s.InteractionSessionId.Value ) )
                    .Select( s => s.InteractionSessionId.Value )
                    .ToList();

                // filter list to remove InteractionSessionIds that are still being used
                var interactionSessionsIdsToRemove = interactionSessionIdChunk.Where( i => !interactionSessionsIdsToKeep.Contains( i ) );
                var interactionSessionQueryable = new InteractionSessionService( rockContext ).Queryable().Where( s => interactionSessionsIdsToRemove.Contains( s.Id ) );

                // take a snapshot of the most recent session id so we don't have to worry about deleting a session id that might be right in the middle of getting used
                int maxInteractionSessionId = interactionSessionQueryable.Max( a => ( int? ) a.Id ) ?? 0;

                // put the batchCount in the where clause to make sure that the BulkDeleteInChunks puts its Take *after* we've batched it
                var batchUnusedInteractionSessionsQuery = interactionSessionQueryable
                        .Where( a => a.Id < maxInteractionSessionId )
                        .OrderBy( a => a.Id )
                        .Take( batchAmount );

                var unusedInteractionSessionsQueryToRemove = new InteractionSessionService( rockContext )
                    .Queryable()
                    .Where( a => batchUnusedInteractionSessionsQuery.Any( u => u.Id == a.Id ) );

                totalRowsDeleted += BulkDeleteInChunks( unusedInteractionSessionsQueryToRemove, batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// This method will look for any orphaned InteractionSession rows and delete them.
        /// </summary>
        /// <returns></returns>
        private int CleanupUnusedInteractionSessions()
        {
            int totalRowsDeleted = 0;
            var currentDateTime = RockDateTime.Now;

            // If there are no channels with a retention policy then don't bother looking for orphans.
            var interactionChannelsWithRentionDurations = InteractionChannelCache.All().Where( ic => ic.RetentionDuration.HasValue );
            if ( !interactionChannelsWithRentionDurations.Any() )
            {
                return 0;
            }

            // delete any InteractionSession records that are no longer used.
            using ( var interactionSessionRockContext = new Rock.Data.RockContext() )
            {
                interactionSessionRockContext.Database.CommandTimeout = commandTimeout;

                var interactionQueryable = new InteractionService( interactionSessionRockContext ).Queryable().Where( a => a.InteractionSessionId.HasValue );
                var interactionSessionQueryable = new InteractionSessionService( interactionSessionRockContext ).Queryable();

                // take a snapshot of the most recent session id so we don't have to worry about deleting a session id that might be right in the middle of getting used
                int maxInteractionSessionId = interactionSessionQueryable.Max( a => ( int? ) a.Id ) ?? 0;

                // put the batchCount in the where clause to make sure that the BulkDeleteInChunks puts its Take *after* we've batched it
                var batchUnusedInteractionSessionsQuery = interactionSessionQueryable
                        .Where( s => !interactionQueryable.Any( i => i.InteractionSessionId == s.Id ) )
                        .Where( a => a.Id < maxInteractionSessionId ).OrderBy( a => a.Id ).Take( batchAmount );

                var unusedInteractionSessionsQueryToRemove = new InteractionSessionService( interactionSessionRockContext ).Queryable().Where( a => batchUnusedInteractionSessionsQuery.Any( u => u.Id == a.Id ) );

                totalRowsDeleted += BulkDeleteInChunks( unusedInteractionSessionsQueryToRemove, batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Does a <see cref="M:RockContext.BulkDelete"></see> on the records listed in the query, but does it in chunks to help prevent timeouts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recordsToDeleteQuery">The records to delete query.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <returns>
        /// The number of records deleted
        /// </returns>
        private static int BulkDeleteInChunks<T>( IQueryable<T> recordsToDeleteQuery, int chunkSize, int commandTimeout ) where T : class
        {
            return BulkDeleteInChunks( recordsToDeleteQuery, chunkSize, commandTimeout, int.MaxValue );
        }

        /// <summary>
        /// Does a <see cref="M:RockContext.BulkDelete"></see> on the records listed in the query, but does it in chunks to help prevent timeouts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recordsToDeleteQuery">The records to delete query.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="maxNumberOfRecordsToDelete">Stops bulk deleting if the total amount exceeds the maximum number of records to delete.</param>
        /// <returns>
        /// The number of records deleted
        /// </returns>
        private static int BulkDeleteInChunks<T>( IQueryable<T> recordsToDeleteQuery, int chunkSize, int commandTimeout, int maxNumberOfRecordsToDelete ) where T : class
        {
            int totalRowsDeleted = 0;

            // Event though BulkDelete has a batch amount, that could exceed our command time out since that'll just be one command for the whole thing, so let's break it up into multiple commands
            // Also, this helps prevent new record inserts waiting the batch operation (if Snapshot Isolation is disabled)
            var chunkQuery = recordsToDeleteQuery.Take( chunkSize );

            using ( var bulkDeleteContext = new RockContext() )
            {
                bulkDeleteContext.Database.CommandTimeout = commandTimeout;
                var keepDeleting = true;
                while ( keepDeleting )
                {
                    var rowsDeleted = bulkDeleteContext.BulkDelete( chunkQuery );
                    keepDeleting = rowsDeleted > 0;
                    totalRowsDeleted += rowsDeleted;
                    if ( totalRowsDeleted >= maxNumberOfRecordsToDelete )
                    {
                        break;
                    }
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleanups the orphaned attributes.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private int CleanupOrphanedAttributes( JobDataMap dataMap )
        {
            int recordsDeleted = 0;

            // Cleanup AttributeMatrix records that are no longer associated with an attribute value
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                AttributeMatrixService attributeMatrixService = new AttributeMatrixService( rockContext );
                AttributeMatrixItemService attributeMatrixItemService = new AttributeMatrixItemService( rockContext );

                var orphanedAttributeMatrices = attributeMatrixService.GetOrphanedAttributeMatrices().ToList();

                if ( orphanedAttributeMatrices.Any() )
                {
                    recordsDeleted += orphanedAttributeMatrices.Count;
                    attributeMatrixItemService.DeleteRange( orphanedAttributeMatrices.SelectMany( a => a.AttributeMatrixItems ) );
                    attributeMatrixService.DeleteRange( orphanedAttributeMatrices );
                    rockContext.SaveChanges();
                }
            }

            // clean up other orphaned entity attributes
            Type rockContextType = typeof( Rock.Data.RockContext );
            foreach ( var cachedType in EntityTypeCache.All().Where( e => e.IsEntity ) )
            {
                Type entityType = cachedType.GetEntityType();
                if ( entityType != null &&
                    typeof( IEntity ).IsAssignableFrom( entityType ) &&
                    typeof( Attribute.IHasAttributes ).IsAssignableFrom( entityType ) &&
                    !entityType.Namespace.Equals( "Rock.Rest.Controllers" ) )
                {
                    try
                    {
                        bool ignore = false;
                        if ( entityType.Assembly != rockContextType.Assembly )
                        {
                            // If the model is from a custom project, verify that it is using RockContext, if not, ignore it since an
                            // exception will occur due to the AttributeValue query using RockContext.
                            var entityContextType = Reflection.SearchAssembly( entityType.Assembly, typeof( System.Data.Entity.DbContext ) );
                            ignore = entityContextType.Any() && !entityContextType.First().Value.Equals( rockContextType );
                        }

                        if ( !ignore )
                        {
                            var classMethod = this.GetType().GetMethods( BindingFlags.Instance | BindingFlags.NonPublic ).First( m => m.Name == nameof( CleanupOrphanedAttributeValuesForEntityType ) );

                            var genericMethod = classMethod.MakeGenericMethod( entityType );
                            var result = genericMethod.Invoke( this, null ) as int?;
                            if ( result.HasValue )
                            {
                                recordsDeleted += ( int ) result;
                            }
                        }
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            return recordsDeleted;
        }

        /// <summary>
        /// Cleanups the orphaned attribute values for entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private int CleanupOrphanedAttributeValuesForEntityType<T>() where T : Rock.Data.Entity<T>, Attribute.IHasAttributes, new()
        {
            int recordsDeleted = 0;

            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                var attributeValueService = new AttributeValueService( rockContext );
                int? entityTypeId = EntityTypeCache.GetId<T>();
                var entityIdsQuery = new Service<T>( rockContext ).AsNoFilter().Select( a => a.Id );
                var orphanedAttributeValuesQuery = attributeValueService.Queryable().Where( a => a.EntityId.HasValue && a.Attribute.EntityTypeId == entityTypeId.Value && !entityIdsQuery.Contains( a.EntityId.Value ) );
                recordsDeleted += BulkDeleteInChunks( orphanedAttributeValuesQuery, batchAmount, commandTimeout );
            }

            return recordsDeleted;
        }

        /// <summary>
        /// Cleanups the transient communications.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupTransientCommunications( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;

            var rockContext = new Rock.Data.RockContext();

            // Set a 10 minute minimum timeout here.
            rockContext.Database.CommandTimeout = commandTimeout >= 600 ? commandTimeout : 600;

            DateTime transientCommunicationExpireDate = RockDateTime.Now.Add( new TimeSpan( 7 * -1, 0, 0, 0 ) );
            var communicationsToDelete = new CommunicationService( rockContext ).Queryable().Where( a => a.CreatedDateTime < transientCommunicationExpireDate && a.Status == CommunicationStatus.Transient );

            totalRowsDeleted += BulkDeleteInChunks( communicationsToDelete, batchAmount, commandTimeout );

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleanups the financial transaction null currency.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private int CleanupFinancialTransactionNullCurrency( JobDataMap dataMap )
        {
            int totalRowsUpdated = 0;
            var rockContext = new Rock.Data.RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            int? currencyTypeUnknownId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid() )?.Id;

            if ( currencyTypeUnknownId.HasValue )
            {
                var financialPaymentDetailsToUpdate = new FinancialPaymentDetailService( rockContext ).Queryable().Where( a => a.CurrencyTypeValueId == null );

                totalRowsUpdated = rockContext.BulkUpdate( financialPaymentDetailsToUpdate, a => new FinancialPaymentDetail { CurrencyTypeValueId = currencyTypeUnknownId.Value } );
            }

            return totalRowsUpdated;
        }

        /// <summary>
        /// Cleanups the person tokens.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns></returns>
        private int CleanupPersonTokens( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            var currentDateTime = RockDateTime.Now;

            // Cleanup PersonTokens records that are expired
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                PersonTokenService personTokenService = new PersonTokenService( rockContext );

                var personTokensToDeleteQuery = personTokenService.Queryable().Where( a => a.ExpireDateTime.HasValue && a.ExpireDateTime < currentDateTime );
                totalRowsDeleted += BulkDeleteInChunks( personTokensToDeleteQuery, batchAmount, commandTimeout );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans expired cached files from the cache folder
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="expirationDate">The file expiration date. Files older than this date will be deleted</param>
        private int CleanCacheDirectory( string directoryPath, DateTime expirationDate )
        {
            int resultCount = 0;

            // verify that the directory exists
            if ( !Directory.Exists( directoryPath ) )
            {
                // if directory doesn't exist return
                return 0;
            }

            // loop through each file in the directory
            foreach ( string filePath in Directory.GetFiles( directoryPath ) )
            {
                // if the file creation date is older than the expiration date
                DateTime adjustedFileDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( File.GetCreationTime( filePath ) );
                if ( adjustedFileDateTime < expirationDate )
                {
                    // delete the file
                    resultCount++;
                    DeleteFile( filePath, false );
                }
            }

            // loop through each subdirectory in the current directory
            foreach ( string subDirectory in Directory.GetDirectories( directoryPath ) )
            {
                // if the directory is not a reparse point
                if ( ( File.GetAttributes( subDirectory ) & FileAttributes.ReparsePoint ) != FileAttributes.ReparsePoint )
                {
                    // clean the directory
                    resultCount += CleanCacheDirectory( subDirectory, expirationDate );
                }
            }

            // get subdirectory and file count
            int directoryCount = Directory.GetDirectories( directoryPath ).Length;
            int fileCount = Directory.GetFiles( directoryPath ).Length;

            // if directory is empty
            if ( ( directoryCount + fileCount ) == 0 )
            {
                // delete the directory
                DeleteDirectory( directoryPath, false );
            }

            return resultCount;
        }

        /// <summary>
        /// Deletes the specified directory.
        /// </summary>
        /// <param name="directoryPath">The path the directory that you would like to delete.</param>
        /// <param name="isRetryAttempt">Is this execution a retry attempt.  If <c>true</c> then don't retry on failure.</param>
        private void DeleteDirectory( string directoryPath, bool isRetryAttempt )
        {
            try
            {
                // if the directory exists
                if ( Directory.Exists( directoryPath ) )
                {
                    // delete the directory
                    Directory.Delete( directoryPath );
                }
            }
            catch ( System.IO.IOException )
            {
                // if IO Exception thrown and this is not a retry attempt
                if ( !isRetryAttempt )
                {
                    // wait for 10 ms and retry delete
                    System.Threading.Tasks.Task.Delay( 10 ).Wait();
                    DeleteDirectory( directoryPath, true );
                }
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file that you would like to delete</param>
        /// <param name="isRetryAttempt">Indicates if this execution is a retry attempt. IF <c>true</c> don't retry on failure</param>
        private void DeleteFile( string filePath, bool isRetryAttempt )
        {
            try
            {
                // verify that the file still exists
                if ( File.Exists( filePath ) )
                {
                    // if the file exists, delete it
                    File.Delete( filePath );
                }
            }
            catch ( System.IO.IOException )
            {
                // If an IO exception has occurred and this is not a retry attempt
                if ( !isRetryAttempt )
                {
                    // wait for 10 ms and retry delete.
                    System.Threading.Tasks.Task.Delay( 10 ).Wait();
                    DeleteFile( filePath, true );
                }
            }
        }

        /// <summary>
        /// Cleanups the job history
        /// </summary>
        private int CleanupJobHistory()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                ServiceJobHistoryService serviceJobHistoryService = new ServiceJobHistoryService( rockContext );
                serviceJobHistoryService.DeleteMoreThanMax();
            }

            return 0;
        }

        /// <summary>
        /// Delete old attendance data (as of today, this is just label data) and
        /// return the number of records deleted.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        /// <returns>The number of records deleted</returns>
        private int AttendanceDataCleanup( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var attendanceService = new AttendanceService( rockContext );

                // 1 day (24 hrs) ago
                DateTime olderThanDate = RockDateTime.Now.Add( new TimeSpan( -1, 0, 0, 0 ) );
                var attendanceDataToDelete = attendanceService.Queryable()
                    .Where( a => a.CreatedDateTime.HasValue && a.CreatedDateTime <= olderThanDate && a.AttendanceData != null && a.AttendanceData.LabelData != null ).Select( a => a.AttendanceData );

                if ( attendanceDataToDelete.Any() )
                {
                    totalRowsDeleted += BulkDeleteInChunks( attendanceDataToDelete, batchAmount, commandTimeout );
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Does cleanup of Attribute Values
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupAttributeValues( JobDataMap dataMap )
        {
            AttributeValueCleanup( commandTimeout );

            return 0;
        }

        /// <summary>
        /// Does cleanup of Attribute Values
        /// </summary>
        /// <param name="commandTimeout">The command timeout.</param>
        internal static void AttributeValueCleanup( int commandTimeout )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                // Ensure AttributeValue.ValueAsNumeric is in sync with AttributeValue.Value, just in case Value got updated without also updating ValueAsNumeric
                rockContext.Database.CommandTimeout = commandTimeout;
                rockContext.Database.ExecuteSqlCommand( @"
UPDATE AttributeValue
SET ValueAsNumeric = CASE
		WHEN LEN([value]) < (100)
			THEN CASE
					WHEN ISNUMERIC([value]) = (1)
						AND NOT [value] LIKE '%[^-0-9.]%'
						THEN TRY_CAST([value] AS [decimal](18, 2))
					END
		END
where ISNULL(ValueAsNumeric, 0) != ISNULL((case WHEN LEN([value]) < (100)
			THEN CASE
					WHEN ISNUMERIC([value]) = (1)
						AND NOT [value] LIKE '%[^-0-9.]%'
						THEN TRY_CAST([value] AS [decimal](18, 2))
					END
		END), 0)
" );
            }
        }

        /// <summary>
        /// Does cleanup of Locations
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int LocationCleanup( JobDataMap dataMap )
        {
            int resultCount = 0;
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );

                // Update states from state name to abbreviation.
                var stateNameList = definedType
                    .DefinedValues
                    .Where( v => v.ContainsKey( "Country" ) && v["Country"] != null )
                    .Select( v => new { State = v.Value, Country = v["Country"], StateName = v.Description } ).ToLookup( v => v.StateName, StringComparer.CurrentCultureIgnoreCase );

                LocationService locationService = new LocationService( rockContext );
                var locations = locationService
                    .Queryable()
                    .Where( l => l.State != null && l.State != string.Empty && l.State.Length > 3 )
                    .ToList();

                foreach ( var location in locations )
                {
                    if ( stateNameList.Contains( location.State ) )
                    {
                        var state = stateNameList[location.State];
                        if ( state.Count() == 1 )
                        {
                            resultCount++;
                            location.State = state.First().State;
                            location.Country = state.First().Country.ToStringSafe();
                        }
                    }
                }

                rockContext.SaveChanges();

                // Populate empty country name if state is a known state
                var stateList = definedType
                    .DefinedValues
                    .Where( v => v.ContainsKey( "Country" ) && v["Country"] != null )
                    .Select( v => new { State = v.Value, Country = v["Country"] } ).ToLookup( v => v.State, StringComparer.CurrentCultureIgnoreCase );

                locationService = new LocationService( rockContext );
                locations = locationService
                    .Queryable()
                    .Where( l => ( l.Country == null || l.Country == string.Empty ) && l.State != null && l.State != string.Empty )
                    .ToList();

                foreach ( var location in locations )
                {
                    if ( stateList.Contains( location.State ) )
                    {
                        var state = stateList[location.State];
                        if ( state.Count() == 1 )
                        {
                            resultCount++;
                            location.State = state.First().State;
                            location.Country = state.First().Country.ToStringSafe();
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            return resultCount;
        }

        /// <summary>
        /// Delete group membership duplicates if they are not allowed by web.config and return the
        /// number of records deleted.
        /// </summary>
        /// <returns>The number of records deleted</returns>
        private int GroupMembershipCleanup()
        {
            // There is a web.config setting to allow duplicate memberships
            // If that is set to allow, then don't cleanup duplicates
            var allowDuplicates = GroupService.AllowsDuplicateMembers();

            if ( allowDuplicates )
            {
                return 0;
            }

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );

            var duplicateQuery = groupMemberService.Queryable()

                // Duplicates are the same person, group, and role occurring more than once
                .GroupBy( m => new { m.PersonId, m.GroupId, m.GroupRoleId } )

                // Filter out sets with only one occurrence because those are not duplicates
                .Where( g => g.Count() > 1 )

                // Leave the oldest membership and delete the others
                .SelectMany( g => g.OrderBy( gm => gm.CreatedDateTime ).Skip( 1 ) );

            // Get the IDs to delete the history
            var groupMemberIds = duplicateQuery.Select( d => d.Id );
            var historyQuery = groupMemberHistoricalService.Queryable()
                .Where( gmh => groupMemberIds.Contains( gmh.GroupMemberId ) );

            // Delete the history and duplicate memberships
            groupMemberHistoricalService.DeleteRange( historyQuery );
            groupMemberService.DeleteRange( duplicateQuery );
            rockContext.SaveChanges();

            // Return the count of memberships deleted
            return groupMemberIds.Count();
        }

        /// <summary>
        /// Merges the streaks.
        /// </summary>
        /// <returns></returns>
        private int MergeStreaks()
        {
            var recordsDeleted = 0;

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            var streakService = new StreakService( rockContext );
            var attemptService = new StreakAchievementAttemptService( rockContext );
            var duplicateGroups = streakService.Queryable()
                .GroupBy( s => new { s.PersonAlias.PersonId, s.StreakTypeId } )
                .Where( g => g.Count() > 1 )
                .ToList();

            foreach ( var duplicateGroup in duplicateGroups )
            {
                var recordToKeep = duplicateGroup.OrderByDescending( s => s.ModifiedDateTime ).First();
                var recordsToDelete = duplicateGroup.Where( s => s.Id != recordToKeep.Id );

                recordToKeep.InactiveDateTime = duplicateGroup.Min( s => s.InactiveDateTime );
                recordToKeep.EnrollmentDate = duplicateGroup.Min( s => s.EnrollmentDate );

                var engagementMaps = duplicateGroup.Select( s => s.EngagementMap ?? new byte[0] ).ToArray();
                recordToKeep.EngagementMap = StreakTypeService.GetAggregateMap( engagementMaps );

                var exclusionMaps = duplicateGroup.Select( s => s.ExclusionMap ?? new byte[0] ).ToArray();
                recordToKeep.ExclusionMap = StreakTypeService.GetAggregateMap( exclusionMaps );

                var recordsToDeleteIds = recordsToDelete.Select( s => s.Id ).ToList();
                var attempts = attemptService.Queryable().Where( saa => recordsToDeleteIds.Contains( saa.StreakId ) ).ToList();
                attempts.ForEach( saa => saa.StreakId = recordToKeep.Id );

                streakService.DeleteRange( recordsToDelete );
                rockContext.SaveChanges( true );
                recordsDeleted += recordsToDelete.Count();
            }

            return recordsDeleted;
        }

        /// <summary>
        /// Refreshes the streaks denormalized data.
        /// </summary>
        /// <returns></returns>
        private int RefreshStreaksDenormalizedData()
        {
            var recordsUpdated = 0;

            foreach ( var streakTypeCache in StreakTypeCache.All().Where( st => st.IsActive ) )
            {
                recordsUpdated += StreakTypeService.HandlePostSaveChanges( streakTypeCache.Id );
            }

            return recordsUpdated;
        }

        /// <summary>
        /// Ensures the schedule effective start end dates.
        /// </summary>
        /// <returns></returns>
        private int EnsureScheduleEffectiveStartEndDates()
        {
            int rowsUpdated = 0;
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var scheduleService = new ScheduleService( rockContext );
                var scheduleList = scheduleService.Queryable().ToList();
                foreach ( var schedule in scheduleList )
                {
                    if ( schedule.EnsureEffectiveStartEndDates() )
                    {
                        rowsUpdated++;
                    }
                }

                if ( rowsUpdated > 0 )
                {
                    rockContext.SaveChanges();
                }
            }

            return rowsUpdated;
        }

        /// <summary>
        /// Ensures the nameless person for SMS responses that have a NULL fromPersonAliasId
        /// </summary>
        /// <returns></returns>
        private int EnsureNamelessPersonForSMSResponses()
        {
            int rowsUpdated = 0;
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var communicationResponseService = new CommunicationResponseService( rockContext );
                var personService = new PersonService( rockContext );

                var nullPersonCommunicationReponseQry = communicationResponseService.Queryable().Where( a => !a.FromPersonAliasId.HasValue && !string.IsNullOrEmpty( a.MessageKey ) );
                var messageKeysWithoutFromPersonList = nullPersonCommunicationReponseQry.Select( a => a.MessageKey ).Distinct().ToList();
                foreach ( var messageKey in messageKeysWithoutFromPersonList )
                {
                    var fromPerson = personService.GetPersonFromMobilePhoneNumber( messageKey, true );

                    var communicationsResponsesToUpdate = nullPersonCommunicationReponseQry.Where( a => a.MessageKey == messageKey );
                    var fromPersonAliasId = fromPerson.PrimaryAliasId;
                    rowsUpdated += rockContext.BulkUpdate( communicationsResponsesToUpdate, a => new CommunicationResponse { FromPersonAliasId = fromPersonAliasId } );
                }
            }

            return rowsUpdated;
        }

        /// <summary>
        /// Matches the nameless person to regular person.
        /// </summary>
        /// <returns></returns>
        private int MatchNamelessPersonToRegularPerson()
        {
            int rowsUpdated = 0;
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var personService = new PersonService( rockContext );
                var phoneNumberService = new PhoneNumberService( rockContext );

                var namelessPersonRecordTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );

                int numberTypeMobileValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                var namelessPersonPhoneNumberQry = phoneNumberService.Queryable()
                    .Where( pn => pn.Person.RecordTypeValueId == namelessPersonRecordTypeId )
                    .AsNoTracking();

                var personPhoneNumberQry = phoneNumberService.Queryable()
                    .Where( a => a.Person.RecordTypeValueId != namelessPersonRecordTypeId );

                // match nameless person records to regular person records by comparing phone numbers.
                // order so that the non-nameless person phones with an SMS number with messaging enabled are listed first
                // then sort by the oldest person record in case there are multiple people with the same number
                var matchedPhoneNumbersJoinQry = namelessPersonPhoneNumberQry.Join(
                    personPhoneNumberQry,
                    np => np.FullNumber,
                    pp => pp.FullNumber,
                    ( np, pp ) => new
                    {
                        NamelessPersonPhoneNumber = np,
                        PersonPhoneNumber = pp
                    } )
                    .OrderByDescending( j => j.PersonPhoneNumber.IsMessagingEnabled )
                    .ThenByDescending( j => j.PersonPhoneNumber.NumberTypeValueId == numberTypeMobileValueId )
                    .ThenByDescending( j => j.PersonPhoneNumber.Person.RecordTypeValueId != namelessPersonRecordTypeId )
                    .ThenBy( j => j.PersonPhoneNumber.PersonId );

                var matchedPhoneNumberList = matchedPhoneNumbersJoinQry.ToList();

                HashSet<int> mergedNamelessPersonIds = new HashSet<int>();

                foreach ( var matchedPhoneNumber in matchedPhoneNumberList.ToList() )
                {
                    var namelessPersonId = matchedPhoneNumber.NamelessPersonPhoneNumber.PersonId;
                    if ( !mergedNamelessPersonIds.Contains( namelessPersonId ) )
                    {
                        var existingPersonId = matchedPhoneNumber.PersonPhoneNumber.PersonId;
                        using ( var mergeContext = new RockContext() )
                        {
                            mergeContext.Database.CommandTimeout = commandTimeout;
                            var mergePersonService = new PersonService( mergeContext );
                            var namelessPerson = mergePersonService.Get( namelessPersonId );
                            var existingPerson = mergePersonService.Get( existingPersonId );
                            mergePersonService.MergeNamelessPersonToExistingPerson( namelessPerson, existingPerson );
                            mergeContext.SaveChanges();
                            mergedNamelessPersonIds.Add( namelessPersonId );
                            rowsUpdated++;
                        }
                    }
                }
            }

            return rowsUpdated;
        }

        /// <summary>
        /// Updates the median page load times. Returns the count of pages that had their MedianPageLoadTime updated.
        /// </summary>
        private int UpdateMedianPageLoadTimes()
        {
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeout;

            var interactionService = new InteractionService( rockContext );
            var pageService = new PageService( rockContext );
            var serviceJobService = new ServiceJobService( rockContext );

            // Get the last successful job run date
            var serviceJob = serviceJobService.Get( SystemGuid.ServiceJob.ROCK_CLEANUP.AsGuid() );
            var minDate = serviceJob?.LastSuccessfulRunDateTime ?? DateTime.MinValue;

            /* 2020-04-21 MDP
             *
             * NOTE: When testing this, set the minDate to DateTime.MinValue to make sure it can still perform even when the job hasn't run before, or in a long time
             *
             * Querying the Interaction table (which can be very large) can easily take a very long time out. So some optimizations might be needed.
             * In this case, the query was timing out in a way that was hard to avoid, so we broke it into several simplier individual queries
             * This results in more roundtrips, but easy roundtrip should be pretty fast, so the net time to do the task ends up taking much less time.
             *
             * 2020-04-27 ETD
             * Also for the same reason as above this job will process all of the page IDs. Trying to query them from recent interactions has a high
             * likelyhood of getting a SQL command timeout exception. The overall job takes longer but no single transaction is long enough to timeout.
             */

            // Un-comment this out when debugging, and make sure to comment it back out when checking in (see above note)
            ////minDate = DateTime.MinValue;

            var channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ).Id;
            var updateCount = 0;

            // Get interaction components to page map - this eliminates some joins for the query within the loop
            var pageIdToComponentIdMap = InteractionComponentCache.All()
                .Where( ic => ic.InteractionChannel.ChannelTypeMediumValueId == channelMediumTypeValueId )
                .Where( ic => ic.EntityId.HasValue )
                .GroupBy( ic => ic.EntityId.Value )
                .ToDictionary( g => g.Key, g => g.Select( ic => ic.Id ).ToList() );

            // The pages we can calculate load time for are those that have interaction components
            var uniquePageIds = pageIdToComponentIdMap.Keys.ToList();

            foreach ( var pageId in uniquePageIds )
            {
                // Get the components for this page
                var componentIds = pageIdToComponentIdMap.GetValueOrNull( pageId );

                // Get the page (sometimes it doesn't exist if the page was deleted)
                var page = pageService.Get( pageId );

                if ( componentIds == null || !componentIds.Any() || page == null )
                {
                    continue;
                }

                // Query to check if this page has had any views since the last time the job ran. This is very fast and much cheaper
                // than one big query to get all pages, which was timing out.
                var hasViewsSinceMinDate = interactionService.Queryable().AsNoTracking().Any( i =>
                    componentIds.Contains( i.InteractionComponentId ) &&
                    i.InteractionDateTime >= minDate );

                if ( !hasViewsSinceMinDate )
                {
                    continue;
                }

                // We want the last 100 interactions included in the median calculation **(reguardless of the minDate)**
                var recentTimesToServe = interactionService.Queryable().AsNoTracking()
                    .Where( i => componentIds.Contains( i.InteractionComponentId ) )
                    .OrderByDescending( i => i.InteractionDateTime )
                    .Take( 100 )
                    .Select( i => i.InteractionTimeToServe )
                    .ToList()
                    .Where( i => i.HasValue )
                    .Select( i => i.Value )
                    .OrderBy( i => i )
                    .ToList();

                var count = recentTimesToServe.Count;
                if ( count < 1 )
                {
                    continue;
                }

                var firstMiddleValue = recentTimesToServe.ElementAt( ( count - 1 ) / 2 );
                var secondMiddleValue = recentTimesToServe.ElementAt( count / 2 );
                var median = ( firstMiddleValue + secondMiddleValue ) / 2;
                page.MedianPageLoadTimeDurationSeconds = median;

                rockContext.SaveChanges();
                updateCount++;
            }

            return updateCount;
        }

        /// <summary>
        /// The result data from a cleanup task
        /// </summary>
        private class RockCleanupJobResult
        {
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the rows affected.
            /// </summary>
            /// <value>
            /// The rows affected.
            /// </value>
            public int RowsAffected { get; set; }

            /// <summary>
            /// Gets or sets the amount of time taken
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public TimeSpan Elapsed { get; set; }

            public bool HasException => Exception != null;

            public Exception Exception { get; set; }
        }
    }
}
