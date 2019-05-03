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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that executes routine cleanup tasks on Rock.
    /// Cleanup tasks are tasks that fixes (add, update or purge) invalid, missing or obsolete data.
    /// </summary>
    [IntegerField( "Days to Keep Exceptions in Log", "The number of days to keep exceptions in the exception log (default is 14 days.)", false, 14, "General", 1, "DaysKeepExceptions" )]
    [IntegerField( "Audit Log Expiration Days", "The number of days to keep items in the audit log (default is 14 days.)", false, 14, "General", 2, "AuditLogExpirationDays" )]
    [IntegerField( "Days to Keep Cached Files", "The number of days to keep cached files in the cache folder (default is 14 days.)", false, 14, "General", 3, "DaysKeepCachedFiles" )]
    [TextField( "Base Cache Folder", "The base/starting Directory for the file cache (default is ~/Cache.)", false, "~/Cache", "General", 4, "BaseCacheDirectory" )]
    [IntegerField( "Max Metaphone Names", "The maximum number of person names to process metaphone values for each time job is run (only names that have not yet been processed are checked).", false, 500, "General", 5 )]
    [IntegerField( "Batch Cleanup Amount", "The number of records to delete at a time dependent on infrastructure. Recommended range is 1000 to 10,000.", false, 1000, "General", 6 )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (900). Note, some operations could take several minutes, so you might want to set it at 900 (15 minutes) or higher", false, 60 * 15, "General", 7, "CommandTimeout" )]
    [DisallowConcurrentExecution]
    public class RockCleanup : IJob
    {
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

        /// <summary>
        /// Job that executes routine Rock cleanup tasks
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            // get the job map
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            List<Exception> rockCleanupExceptions = new List<Exception>();

            Dictionary<string, int> databaseRowsCleanedUp = new Dictionary<string, int>();

            try
            {
                databaseRowsCleanedUp.Add( "Exception Log", PurgeExceptionLog( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in PurgeExceptionLog", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Expired Entity Set", CleanupExpiredEntitySets( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupExpiredEntitySets", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Old Interaction", CleanupInteractions( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupInteractions", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Audit Log", PurgeAuditLog( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in PurgeAuditLog", ex ) );
            }

            try
            {
                CleanCachedFileDirectory( context, dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanCachedFileDirectory", ex ) );
            }

            try
            {
                CleanupTemporaryBinaryFiles();
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupTemporaryBinaryFiles", ex ) );
            }

            try
            {
                // updates missing person aliases, metaphones, etc (doesn't delete any records)
                PersonCleanup( dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in PersonCleanup", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Temporary Registration", CleanUpTemporaryRegistrations() );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanUpTemporaryRegistrations", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Workflow", CleanUpWorkflows( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanUpWorkflows", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Workflow Log", CleanUpWorkflowLogs( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanUpWorkflowLogs", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Orphaned Attribute Value", CleanupOrphanedAttributes( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupOrphanedAttributes", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Transient Communication", CleanupTransientCommunications( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupTransientCommunications", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Missing Financial Transaction Currency", CleanupFinancialTransactionNullCurrency( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupFinancialTransactionNullCurrency", ex ) );
            }

            try
            {
                databaseRowsCleanedUp.Add( "Person Token", CleanupPersonTokens( dataMap ) );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupPersonTokens", ex ) );
            }

            try
            {
                // Reduce the job history to max size
                CleanupJobHistory();
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupJobHistory", ex ) );
            }

            try
            {
                // Search for and delete group memberships duplicates (same person, group, and role)
                var rowsDeleted = GroupMembershipCleanup();
                databaseRowsCleanedUp.Add( "Group Membership", rowsDeleted );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in GroupMembershipCleanup", ex ) );
            }

            if ( databaseRowsCleanedUp.Any( a => a.Value > 0 ) )
            {
                context.Result = string.Format( "Rock Cleanup cleaned up {0}", databaseRowsCleanedUp.Where( a => a.Value > 0 ).Select( a => $"{a.Value} {a.Key.PluralizeIf( a.Value != 1 )}" ).ToList().AsDelimited( ", ", " and " ) );
            }
            else
            {
                context.Result = "Rock Cleanup completed";
            }

            try
            {
                // Search for locations with no country and assign USA or Canada if it match any of the country's states
                LocationCleanup( dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in LocationCleanup", ex ) );
            }

            if ( rockCleanupExceptions.Count > 0 )
            {
                throw new AggregateException( "One or more exceptions occurred in RockCleanup.", rockCleanupExceptions );
            }
        }

        /// <summary>
        /// Does cleanup of Person Aliases and Metaphones
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private void PersonCleanup( JobDataMap dataMap )
        {
            // Add any missing person aliases
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                PersonService personService = new PersonService( personRockContext );
                PersonAliasService personAliasService = new PersonAliasService( personRockContext );
                var personAliasServiceQry = personAliasService.Queryable();
                foreach ( var person in personService.Queryable( "Aliases" )
                    .Where( p => !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                    .Take( 300 ) )
                {
                    person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                }

                personRockContext.SaveChanges();
            }

            AddMissingAlternateIds();

            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                PersonService personService = new PersonService( personRockContext );
                // Add any missing metaphones
                int namesToProcess = dataMap.GetString( "MaxMetaphoneNames" ).AsIntegerOrNull() ?? 500;
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
                    }

                    personRockContext.SaveChanges( disablePrePostProcessing: true );
                }
            }

            int commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 900;
            // Ensures the PrimaryFamily is correct for all person records in the database
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int primaryFamilyUpdates = PersonService.UpdatePrimaryFamilyAll( personRockContext );
            }

            // Ensures the GivingLeaderId is correct for all person records in the database
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int primaryFamilyUpdates = PersonService.UpdateGivingLeaderIdAll( personRockContext );
            }

            // update any updated or incorrect age classifications on persons
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                personRockContext.Database.CommandTimeout = commandTimeout;
                int ageClassificationUpdates = PersonService.UpdatePersonAgeClassificationAll( personRockContext );
            }

            // update the BirthDate with a computed value
            using ( var personRockContext = new Rock.Data.RockContext() )
            {
                PersonService.UpdateBirthDateAll( personRockContext );
            }

            //// Add any missing Implied/Known relationship groups
            // Known Relationship Group
            AddMissingRelationshipGroups( GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ), Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid(), commandTimeout );

            // Implied Relationship Group
            AddMissingRelationshipGroups( GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_PEER_NETWORK ), Rock.SystemGuid.GroupRole.GROUPROLE_PEER_NETWORK_OWNER.AsGuid(), commandTimeout );

            // Find family groups that have no members or that have only 'inactive' people (record status) and mark the groups inactive.
            using ( var familyRockContext = new Rock.Data.RockContext() )
            {
                int familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                int recordStatusInactiveValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

                var activeFamilyWithNoActiveMembers = new GroupService( familyRockContext ).Queryable()
                    .Where( a => a.GroupTypeId == familyGroupTypeId && a.IsActive == true )
                    .Where( a => !a.Members.Where( m => m.Person.RecordStatusValueId != recordStatusInactiveValueId ).Any() );

                var currentDateTime = RockDateTime.Now;

                familyRockContext.BulkUpdate( activeFamilyWithNoActiveMembers, x => new Rock.Model.Group
                {
                    IsActive = false
                } );
            }
        }

        /// <summary>
        /// Adds any missing person alternate ids; limited to 150k records per run
        /// to avoid any possible memory issues. Processes about 150k records
        /// in 52 seconds.
        /// </summary>
        private static void AddMissingAlternateIds()
        {
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
                        }
                    );
                }

                if ( itemsToInsert.Count > 0 )
                {
                    // Now add them in one bulk insert.
                    personRockContext.BulkInsert( itemsToInsert );
                }
            }
        }

        /// <summary>
        /// Adds the missing relationship groups.
        /// </summary>
        /// <param name="relationshipGroupType">Type of the relationship group.</param>
        /// <param name="ownerRoleGuid">The owner role unique identifier.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        private static void AddMissingRelationshipGroups( GroupTypeCache relationshipGroupType, Guid ownerRoleGuid, int commandTimeout )
        {
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
                    }
                }
            }
        }

        /// <summary>
        /// Cleanups the temporary binary files.
        /// </summary>
        private void CleanupTemporaryBinaryFiles()
        {
            var binaryFileRockContext = new Rock.Data.RockContext();
            // clean out any temporary binary files
            BinaryFileService binaryFileService = new BinaryFileService( binaryFileRockContext );
            foreach ( var binaryFile in binaryFileService.Queryable().Where( bf => bf.IsTemporary == true ).ToList() )
            {
                if ( binaryFile.ModifiedDateTime < RockDateTime.Now.AddDays( -1 ) )
                {
                    string errorMessage;
                    if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                    {
                        binaryFileService.Delete( binaryFile );
                        binaryFileRockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Cleans up temporary registrations.
        /// </summary>
        private int CleanUpTemporaryRegistrations()
        {
            var registrationRockContext = new Rock.Data.RockContext();
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
            var workflowService = new WorkflowService( workflowContext );
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;

            var completedWorkflows = workflowService.Queryable().AsNoTracking()
                .Where( w => w.WorkflowType.CompletedWorkflowRetentionPeriod.HasValue && w.Status.Equals( "Completed" )
                && DateTime.Now > DbFunctions.AddDays( w.ModifiedDateTime, w.WorkflowType.CompletedWorkflowRetentionPeriod ) )
                .ToList();

            List<int> workflowIdsToDelete = new List<int>();

            foreach ( var workflow in completedWorkflows )
            {
                if ( workflowService.CanDelete( workflow, out _ ) )
                {
                    workflowIdsToDelete.Add( workflow.Id );
                    totalRowsDeleted++;
                }

                // delete in batches
                if ( workflowIdsToDelete.Count >= batchAmount )
                {
                    workflowContext.BulkDelete( workflowService.GetByIds( workflowIdsToDelete ) );
                    workflowIdsToDelete = new List<int>();
                }
            }

            // Delete any remaining items in the list
            if ( workflowIdsToDelete.Count > 0 )
            {
                workflowContext.BulkDelete( workflowService.GetByIds( workflowIdsToDelete ) );
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up workflow logs by removing logs in batches for workflows with a log retention period that has passed.
        /// see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses"
        /// </summary>
        private int CleanUpWorkflowLogs( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            // Limit the number of workflow logs to delete for this run (20M records will take about 20 minutes).
            int maxRowDeleteLimit = 20000000;
            var workflowContext = new RockContext();
            var workflowService = new WorkflowService( workflowContext );
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;

            var workflowsOlderThanLogRetentionPeriod = workflowService.Queryable()
                .Where( w => w.WorkflowType.LogRetentionPeriod.HasValue
                && DateTime.Now > DbFunctions.AddDays( w.ModifiedDateTime, w.WorkflowType.LogRetentionPeriod ) )
                .Select( w => w.Id )
                .ToList();

            foreach ( var workflowId in workflowsOlderThanLogRetentionPeriod )
            {
                // WorkflowLogService.CanDelete( log ) always returns true, so no need to check
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = workflowContext.Database.BeginTransaction();
                    try
                    {
                        string sqlCommand = @"DELETE TOP (@batchAmount) FROM [WorkflowLog] WHERE [WorkflowId] = @workflowId";

                        int rowsDeleted = workflowContext.Database.ExecuteSqlCommand( sqlCommand,
                            new SqlParameter( "batchAmount", batchAmount ),
                            new SqlParameter( "workflowId", workflowId )
                        );
                        keepDeleting = rowsDeleted > 0;
                        totalRowsDeleted += rowsDeleted;

                        // If we hit the limit, short circuit and break out this while loop.
                        if ( totalRowsDeleted >= maxRowDeleteLimit )
                        {
                            keepDeleting = false;
                        }
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }

                // If we hit the limit, short circuit and break out of this foreach loop.
                if ( totalRowsDeleted >= maxRowDeleteLimit )
                {
                    break;
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans the cached file directory.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataMap">The data map.</param>
        private void CleanCachedFileDirectory( IJobExecutionContext context, JobDataMap dataMap )
        {
            string cacheDirectoryPath = dataMap.GetString( "BaseCacheDirectory" );
            int? cacheExpirationDays = dataMap.GetString( "DaysKeepCachedFiles" ).AsIntegerOrNull();
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
                    CleanCacheDirectory( cacheDirectoryPath, cacheExpirationDate );
                }
            }
        }

        /// <summary>
        /// Purges the audit log.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int PurgeAuditLog( JobDataMap dataMap )
        {
            // purge audit log
            int totalRowsDeleted = 0;
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;
            int? auditExpireDays = dataMap.GetString( "AuditLogExpirationDays" ).AsIntegerOrNull();
            if ( auditExpireDays.HasValue )
            {
                var auditLogRockContext = new Rock.Data.RockContext();
                DateTime auditExpireDate = RockDateTime.Now.Add( new TimeSpan( auditExpireDays.Value * -1, 0, 0, 0 ) );

                // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = auditLogRockContext.Database.BeginTransaction();
                    try
                    {
                        int rowsDeleted = auditLogRockContext.Database.ExecuteSqlCommand( @"DELETE TOP (@batchAmount) FROM [Audit] WHERE [DateTime] < @auditExpireDate",
                            new SqlParameter( "batchAmount", batchAmount ),
                            new SqlParameter( "auditExpireDate", auditExpireDate )
                        );
                        keepDeleting = rowsDeleted > 0;
                        totalRowsDeleted += rowsDeleted;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }

                auditLogRockContext.SaveChanges();
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Purges the exception log.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int PurgeExceptionLog( JobDataMap dataMap )
        {
            int totalRowsDeleted = 0;
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;
            int? exceptionExpireDays = dataMap.GetString( "DaysKeepExceptions" ).AsIntegerOrNull();
            if ( exceptionExpireDays.HasValue )
            {
                var exceptionLogRockContext = new Rock.Data.RockContext();
                exceptionLogRockContext.Database.CommandTimeout = ( int ) TimeSpan.FromMinutes( 10 ).TotalSeconds;
                DateTime exceptionExpireDate = RockDateTime.Now.Add( new TimeSpan( exceptionExpireDays.Value * -1, 0, 0, 0 ) );
                var exceptionLogsToDelete = new ExceptionLogService( exceptionLogRockContext ).Queryable().Where( a => a.CreatedDateTime < exceptionExpireDate );

                totalRowsDeleted = exceptionLogRockContext.BulkDelete( exceptionLogsToDelete, batchAmount );
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
            var currentDateTime = RockDateTime.Now;
            var entitySetService = new EntitySetService( entitySetRockContext );
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;
            var qry = entitySetService.Queryable().Where( a => a.ExpireDateTime.HasValue && a.ExpireDateTime < currentDateTime );
            int totalRowsDeleted = 0;

            foreach ( var entitySet in qry.ToList() )
            {
                string deleteWarning;
                if ( entitySetService.CanDelete( entitySet, out deleteWarning ) )
                {
                    // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                    bool keepDeleting = true;
                    while ( keepDeleting )
                    {
                        var dbTransaction = entitySetRockContext.Database.BeginTransaction();
                        try
                        {
                            string sqlCommand = @"DELETE TOP (@batchAmount) FROM [EntitySetItem] WHERE [EntitySetId] = @entitySetId";

                            int rowsDeleted = entitySetRockContext.Database.ExecuteSqlCommand( sqlCommand,
                                new SqlParameter( "batchAmount", batchAmount ),
                                new SqlParameter( "entitySetId", entitySet.Id )
                            );
                            keepDeleting = rowsDeleted > 0;
                            totalRowsDeleted += rowsDeleted;
                        }
                        finally
                        {
                            dbTransaction.Commit();
                        }
                    }

                    entitySetService.Delete( entitySet );
                    entitySetRockContext.SaveChanges();
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans up Interactions for Interaction Channels that have a retention period
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private int CleanupInteractions( JobDataMap dataMap )
        {
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;
            var interactionRockContext = new Rock.Data.RockContext();
            var currentDateTime = RockDateTime.Now;
            var interactionChannelService = new InteractionChannelService( interactionRockContext );
            var interactionChannelQry = interactionChannelService.Queryable().Where( a => a.RetentionDuration.HasValue );
            int totalRowsDeleted = 0;

            foreach ( var interactionChannel in interactionChannelQry.ToList() )
            {
                var retentionCutoffDateTime = currentDateTime.AddDays( -interactionChannel.RetentionDuration.Value );
                if ( retentionCutoffDateTime < System.Data.SqlTypes.SqlDateTime.MinValue.Value )
                {
                    retentionCutoffDateTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                }

                // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = interactionRockContext.Database.BeginTransaction();
                    try
                    {
                        string sqlCommand = @"
DELETE TOP (@batchAmount)
FROM ia
FROM [Interaction] ia
INNER JOIN [InteractionComponent] ic ON ia.InteractionComponentId = ic.Id
WHERE ic.ChannelId = @channelId
	AND ia.InteractionDateTime < @retentionCutoffDateTime
";
                        int rowsDeleted = interactionRockContext.Database.ExecuteSqlCommand( sqlCommand, new SqlParameter( "batchAmount", batchAmount ), new SqlParameter( "channelId", interactionChannel.Id ), new SqlParameter( "retentionCutoffDateTime", retentionCutoffDateTime ) );
                        keepDeleting = rowsDeleted > 0;
                        totalRowsDeleted += rowsDeleted;
                    }
                    finally
                    {
                        dbTransaction.Commit();
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
                            ignore = ( entityContextType.Any() && !entityContextType.First().Value.Equals( rockContextType ) );
                        }

                        if ( !ignore )
                        {
                            var classMethod = this.GetType().GetMethods( BindingFlags.Instance | BindingFlags.NonPublic )
                            .First( m => m.Name == "CleanupOrphanedAttributeValuesForEntityType" );
                            var genericMethod = classMethod.MakeGenericMethod( entityType );
                            var result = genericMethod.Invoke( this, null ) as int?;
                            if ( result.HasValue )
                            {
                                recordsDeleted += ( int ) result;
                            }
                        }
                    }
                    catch { }
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
                var attributeValueService = new AttributeValueService( rockContext );
                int? entityTypeId = EntityTypeCache.GetId<T>();
                var entityIdsQuery = new Service<T>( rockContext ).Queryable().Select( a => a.Id );
                var orphanedAttributeValuesQuery = attributeValueService.Queryable().Where( a => a.EntityId.HasValue && a.Attribute.EntityTypeId == entityTypeId.Value && !entityIdsQuery.Contains( a.EntityId.Value ) );
                recordsDeleted += rockContext.BulkDelete( orphanedAttributeValuesQuery );
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
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;
            var rockContext = new Rock.Data.RockContext();
            rockContext.Database.CommandTimeout = ( int ) TimeSpan.FromMinutes( 10 ).TotalSeconds;
            DateTime transientCommunicationExpireDate = RockDateTime.Now.Add( new TimeSpan( 7 * -1, 0, 0, 0 ) );
            var communicationsToDelete = new CommunicationService( rockContext ).Queryable().Where( a => a.CreatedDateTime < transientCommunicationExpireDate && a.Status == CommunicationStatus.Transient );

            totalRowsDeleted = rockContext.BulkDelete( communicationsToDelete, batchAmount );

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
            int? batchAmount = dataMap.GetString( "BatchCleanupAmount" ).AsIntegerOrNull() ?? 1000;

            // Cleanup PersonTokens records that are expired
            using ( RockContext rockContext = new RockContext() )
            {
                PersonTokenService personTokenService = new PersonTokenService( rockContext );

                // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = rockContext.Database.BeginTransaction();
                    try
                    {
                        string sqlCommand = @"
DELETE TOP (@batchAmount)
FROM [PersonToken]
WHERE ExpireDateTime IS NOT NULL
	AND ExpireDateTime < GetDate()

";
                        int rowsDeleted = rockContext.Database.ExecuteSqlCommand( sqlCommand, new SqlParameter( "batchAmount", batchAmount ) );
                        keepDeleting = rowsDeleted > 0;
                        totalRowsDeleted += rowsDeleted;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Cleans expired cached files from the cache folder
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="expirationDate">The file expiration date. Files older than this date will be deleted</param>
        private void CleanCacheDirectory( string directoryPath, DateTime expirationDate )
        {
            // verify that the directory exists
            if ( !Directory.Exists( directoryPath ) )
            {
                // if directory doesn't exist return
                return;
            }

            // loop through each file in the directory
            foreach ( string filePath in Directory.GetFiles( directoryPath ) )
            {
                // if the file creation date is older than the expiration date
                DateTime adjustedFileDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( File.GetCreationTime( filePath ) );
                if ( adjustedFileDateTime < expirationDate )
                {
                    // delete the file
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
                    CleanCacheDirectory( subDirectory, expirationDate );
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
        private void CleanupJobHistory()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                ServiceJobHistoryService serviceJobHistoryService = new ServiceJobHistoryService( rockContext );
                serviceJobHistoryService.DeleteMoreThanMax();
            }
        }

        /// <summary>
        /// Does cleanup of Locations
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private void LocationCleanup( JobDataMap dataMap )
        {
            // Add any missing person aliases
            using ( var rockContext = new Rock.Data.RockContext() )
            {
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
                            location.State = state.First().State;
                            location.Country = state.First().Country.ToStringSafe();
                        }
                    }
                }

                rockContext.SaveChanges();
            }
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
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );

            var duplicateQuery = groupMemberService.Queryable()
                // Duplicates are the same person, group, and role occuring more than once
                .GroupBy( m => new { m.PersonId, m.GroupId, m.GroupRoleId } )
                // Filter out sets with only one occurence because those are not duplicates
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
    }
}
