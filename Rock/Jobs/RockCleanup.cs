// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that executes routine cleanup tasks on Rock
    /// </summary>
    [IntegerField( "Hours to Keep Unconfirmed Accounts", "The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)", false, 48, "General", 0, "HoursKeepUnconfirmedAccounts" )]
    [IntegerField( "Days to Keep Exceptions in Log", "The number of days to keep exceptions in the exception log (default is 14 days.)", false, 14, "General", 1, "DaysKeepExceptions" )]
    [IntegerField( "Audit Log Expiration Days", "The number of days to keep items in the audit log (default is 14 days.)", false, 14, "General", 2, "AuditLogExpirationDays" )]
    [IntegerField( "Days to Keep Cached Files", "The number of days to keep cached files in the cache folder (default is 14 days.)", false, 14, "General", 3, "DaysKeepCachedFiles" )]
    [TextField( "Base Cache Folder", "The base/starting Directory for the file cache (default is ~/Cache.)", false, "~/Cache", "General", 4, "BaseCacheDirectory" )]
    [IntegerField( "Max Metaphone Names", "The maximum number of person names to process metaphone values for each time job is run (only names that have not yet been processed are checked).", false, 500, "General", 5 )]
    [DisallowConcurrentExecution]
    public class RockCleanup : IJob
    {
        /// <summary> 
        /// Empty constructor for job initilization
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

            try
            {
                CleanupUnconfirmedUserLogins( dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in CleanupUnconfirmedUserLogins", ex ) );
            }

            try
            {
                PurgeExceptionLog( dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in PurgeExceptionLog", ex ) );
            }

            try
            {
                PurgeAuditLog( dataMap );
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
                PersonCleanup( dataMap );
            }
            catch ( Exception ex )
            {
                rockCleanupExceptions.Add( new Exception( "Exception in PersonCleanup", ex ) );
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
            var personRockContext = new Rock.Data.RockContext();
            PersonService personService = new PersonService( personRockContext );
            PersonAliasService personAliasService = new PersonAliasService( personRockContext );
            var personAliasServiceQry = personAliasService.Queryable();
            foreach ( var person in personService.Queryable( "Aliases" )
                .Where( p => !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ))
                .Take( 300 ) )
            {
                person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
            }

            personRockContext.SaveChanges();

            // Add any missing metaphones
            int namesToProcess = dataMap.GetString( "MaxMetaphoneNames" ).AsInteger();
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

                personRockContext.SaveChanges();
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
                    binaryFileService.Delete( binaryFile );
                    binaryFileRockContext.SaveChanges();
                }
            }
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
        private void PurgeAuditLog( JobDataMap dataMap )
        {
            // purge audit log
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
                        int rowsDeleted = auditLogRockContext.Database.ExecuteSqlCommand( @"DELETE TOP (1000) FROM [Audit] WHERE [DateTime] < @auditExpireDate", new SqlParameter( "auditExpireDate", auditExpireDate ) );
                        keepDeleting = rowsDeleted > 0;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }

                auditLogRockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Purges the exception log.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private void PurgeExceptionLog( JobDataMap dataMap )
        {
            int? exceptionExpireDays = dataMap.GetString( "DaysKeepExceptions" ).AsIntegerOrNull();
            if ( exceptionExpireDays.HasValue )
            {
                var exceptionLogRockContext = new Rock.Data.RockContext();
                DateTime exceptionExpireDate = RockDateTime.Now.Add( new TimeSpan( exceptionExpireDays.Value * -1, 0, 0, 0 ) );

                // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = exceptionLogRockContext.Database.BeginTransaction();
                    try
                    {
                        int rowsDeleted = exceptionLogRockContext.Database.ExecuteSqlCommand( @"DELETE TOP (1000) FROM [ExceptionLog] WHERE [CreatedDateTime] < @createdDateTime", new SqlParameter( "createdDateTime", exceptionExpireDate ) );
                        keepDeleting = rowsDeleted > 0;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Cleanups the unconfirmed user logins that have not been confirmed in X hours
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        private void CleanupUnconfirmedUserLogins( JobDataMap dataMap )
        {
            int? userExpireHours = dataMap.GetString( "HoursKeepUnconfirmedAccounts" ).AsIntegerOrNull();
            if ( userExpireHours.HasValue )
            {
                var userLoginRockContext = new Rock.Data.RockContext();
                DateTime userAccountExpireDate = RockDateTime.Now.Add( new TimeSpan( userExpireHours.Value * -1, 0, 0 ) );

                // delete in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepDeleting = true;
                while ( keepDeleting )
                {
                    var dbTransaction = userLoginRockContext.Database.BeginTransaction();
                    try
                    {
                        int rowsDeleted = userLoginRockContext.Database.ExecuteSqlCommand( @"DELETE TOP (1000) FROM [UserLogin] WHERE [IsConfirmed] = 0 AND ([CreatedDateTime] is null OR [CreatedDateTime] < @createdDateTime )", new SqlParameter( "createdDateTime", userAccountExpireDate ) );
                        keepDeleting = rowsDeleted > 0;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }
                }
            }
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
                // if the directory exixts
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
                    // have thread sleep for 10 ms and retry delete
                    System.Threading.Thread.Sleep( 10 );
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
                    // have the thread sleep for 10 ms and retry delete.
                    System.Threading.Thread.Sleep( 10 );
                    DeleteFile( filePath, true );
                }
            }
        }
    }
}