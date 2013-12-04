//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.IO;
using System.Linq;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Jobs
{
    
    /// <summary>
    /// Job that executes routine cleanup tasks on RockChMS
    /// </summary>
    /// <author>Jon Edmiston/Chris Funk</author>
    /// <author>Spark Development Network</author>
    [IntegerField("Hours to Keep Unconfirmed Accounts", "The number of hours to keep user accounts that have not been confirmed (default is 48 hours.)", false, 48, "General", 0, "HoursKeepUnconfirmedAccounts" )]
    [IntegerField("Days to Keep Exceptions in Log", "The number of days to keep exceptions in the exception log (default is 14 days.)", false, 14, "General", 1,"DaysKeepExceptions" )]
    [IntegerField( "Audit Log Expiration Days", "The number of days to keep items in the audit log (default is 14 days.)", false, 14, "General", 2, "AuditLogExpirationDays" )]
    [IntegerField( "Days to Keep Cached Files", "The number of days to keep cached files in the cache folder (default is 14 days.)", false, 14, "General", 3, "DaysKeepCachedFiles" )]
    [TextField( "Base Cache Folder", "The base/starting Directory for the file cache (default is ~/Cache.)", false, "~/Cache", "General", 4, "BaseCacheDirectory" )]
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
        /// Job that executes routine RockChMS cleanup tasks
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void  Execute(IJobExecutionContext context)
        {
            
            // get the job map
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // delete accounts that have not been confirmed in X hours
            int userExpireHours = Int32.Parse( dataMap.GetString( "HoursKeepUnconfirmedAccounts" ) );
            DateTime userAccountExpireDate = DateTime.Now.Add( new TimeSpan( userExpireHours * -1,0,0 ) );

            var userLoginService = new UserLoginService();

            foreach (var user in userLoginService.Queryable().Where(u => u.IsConfirmed == false && u.CreationDateTime < userAccountExpireDate).ToList() )
            {
                userLoginService.Delete( user, null );
                userLoginService.Save( user, null );
            }

            // purge exception log
            int exceptionExpireDays = Int32.Parse( dataMap.GetString( "DaysKeepExceptions" ) );
            DateTime exceptionExpireDate = DateTime.Now.Add( new TimeSpan( exceptionExpireDays * -1, 0, 0, 0 ) );

            ExceptionLogService exceptionLogService = new ExceptionLogService();

            foreach ( var exception in exceptionLogService.Queryable().Where( e => e.ExceptionDateTime < exceptionExpireDate ).ToList() )
            {
                exceptionLogService.Delete( exception, null );
                exceptionLogService.Save( exception, null );
            }

            // purge audit log
            int auditExpireDays = Int32.Parse( dataMap.GetString( "AuditLogExpirationDays" ) );
            DateTime auditExpireDate = DateTime.Now.Add( new TimeSpan( auditExpireDays * -1, 0, 0, 0 ) );
            AuditService auditService = new AuditService();
            foreach( var audit in auditService.Queryable().Where( a => a.DateTime < auditExpireDate ).ToList() )
            {
                auditService.Delete( audit, null );
                auditService.Save( audit, null );
            }

            // clean the cached file directory

            //get the attributes
            string cacheDirectoryPath = dataMap.GetString( "BaseCacheDirectory" );
            int cacheExpirationDays = int.Parse( dataMap.GetString( "DaysKeepCachedFiles" ) );
            DateTime cacheExpirationDate = DateTime.Now.Add( new TimeSpan( cacheExpirationDays * -1, 0, 0, 0 ) );

            //if job is being run by the IIS scheduler and path is not null
            if ( context.Scheduler.SchedulerName == "RockSchedulerIIS" && !String.IsNullOrEmpty( cacheDirectoryPath ) )
            {
                //get the physical path of the cache directory
                cacheDirectoryPath = System.Web.Hosting.HostingEnvironment.MapPath( cacheDirectoryPath );
            }

            //if directory is not blank and cache expiration date not in the future
            if ( !String.IsNullOrEmpty( cacheDirectoryPath ) && cacheExpirationDate <= DateTime.Now )
            {
                //Clean cache directory
                CleanCacheDirectory( cacheDirectoryPath, cacheExpirationDate );
            }

            // clean out any temporary binary files
            BinaryFileService binaryFileService = new BinaryFileService();
            foreach( var binaryFile in binaryFileService.Queryable().Where( bf => bf.IsTemporary == true ).ToList() )
            {
                if ( binaryFile.LastModifiedDateTime < DateTime.Now.AddDays(-1) )
                {
                    binaryFileService.Delete( binaryFile, null );
                    binaryFileService.Save( binaryFile, null );
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

            //verify that the directory exists
            if ( !Directory.Exists( directoryPath ) )
            {
                //if directory doesn't exist return
                return;
            }

            // loop through each file in the directory
            foreach ( string filePath in Directory.GetFiles( directoryPath ) )
            {
                //if the file creation date is older than the expiration date
                if ( File.GetCreationTime( filePath ) < expirationDate )
                {
                    //delete the file
                    DeleteFile( filePath, false );
                }
            }

            //loop through each subdirectory in the current directory
            foreach ( string subDirectory in Directory.GetDirectories( directoryPath ) )
            {
                //if the directory is not a reparse point
                if ( ( File.GetAttributes( subDirectory ) & FileAttributes.ReparsePoint ) != FileAttributes.ReparsePoint )
                {
                    //clean the directory
                    CleanCacheDirectory( subDirectory, expirationDate );
                }
            }

            //get subdirectory and file count
            int directoryCount = Directory.GetDirectories( directoryPath ).Length;
            int fileCount = Directory.GetFiles( directoryPath ).Length;

            // if directory is empty
            if ( ( directoryCount + fileCount ) == 0 )
            {
                //delete the directory
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
                //if the directory exixts
                if ( Directory.Exists( directoryPath ) )
                {
                    //delete the directory
                    Directory.Delete( directoryPath );
                }
            }
            catch ( System.IO.IOException )
            {
                //if IO Exception thrown and this is not a retry attempt
                if ( !isRetryAttempt )
                {
                    //have thread sleep for 10 ms and retry delete
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
                //verify that the file still exists
                if ( File.Exists( filePath ) )
                {
                    //if the file exists, delete it
                    File.Delete( filePath );
                }
            }
            catch ( System.IO.IOException )
            {
                //If an IO exception has occurred and this is not a retry attempt
                if ( !isRetryAttempt )
                {
                    //have the thread sleep for 10 ms and retry delete.
                    System.Threading.Thread.Sleep( 10 );
                    DeleteFile( filePath, true );
                }
            }
        }

    }
}