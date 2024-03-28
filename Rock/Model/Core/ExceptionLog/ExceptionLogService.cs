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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.ExceptionLog"/> entity type objects.
    /// </summary>
    public partial class ExceptionLogService 
    {
        #region Fields

        /// <summary>
        /// When true, indicates that exceptions should always be logged to file in addition to the database.
        /// </summary>
        public static bool AlwaysLogToFile = false;

        #endregion Fields

        #region Queries

        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of their Parent exceptionId. 
        /// Under most instances, only one child <see cref="Rock.Model.ExceptionLog"/> entity will be returned in the collection.
        /// </summary>
        /// <param name="parentId">An <see cref="System.Int32"/> containing the Id of the parent ExceptionLog entity to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog" /> entities whose Parent ExceptionId matches the provided value.</returns>
        public IQueryable<ExceptionLog> GetByParentId( int? parentId )
        {
            return Queryable().Where( t => ( t.ParentId == parentId || ( parentId == null && t.ParentId == null ) ) );
        }
        
        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of the <see cref="Rock.Model.Site"/> that they occurred on.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog"/> entities who's SiteId matches the provided value.</returns>
        public IQueryable<ExceptionLog> GetBySiteId( int? siteId )
        {
            return Queryable().Where( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) );
        }

        #endregion Queries

        #region Filters

        ///
        /// TODO: To allow a fluent style of filter composition, these filters should be implemented as extension methods in a separate class.
        ///
        /// <summary>
        /// Specifies the number of prefix characters of the Exception Message property that are examined when grouping similar exceptions.
        /// </summary>
        public static readonly int DescriptionGroupingPrefixLength = 95;

        /// <summary>
        /// Filter a query for exceptions at the innermost or lowest level of the exception hierarchy.
        /// </summary>
        /// <returns></returns>
        public IQueryable<ExceptionLog> FilterByInnermost( IQueryable<ExceptionLog> query )
        {
            query = query.Where( e => e.HasInnerException == null || e.HasInnerException == false );

            return query;
        }

        /// <summary>
        /// Filter a query for exceptions at the outermost or highest level of the exception hierarchy.
        /// </summary>
        /// <returns></returns>
        public IQueryable<ExceptionLog> FilterByOutermost( IQueryable<ExceptionLog> query )
        {
            query = query.Where( e => e.ParentId == null );

            return query;
        }

        /// <summary>
        /// Filter a query for exceptions having a description matching the specified prefix.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="descriptionPrefix">The description prefix.</param>
        /// <returns></returns>
        public IQueryable<ExceptionLog> FilterByDescriptionPrefix( IQueryable<ExceptionLog> query, string descriptionPrefix )
        {
            if ( !string.IsNullOrEmpty( descriptionPrefix ) )
            {
                descriptionPrefix = descriptionPrefix.Substring( 0, Math.Min( descriptionPrefix.Length, DescriptionGroupingPrefixLength ) );

                query = query.Where( e => e.Description.Substring( 0, DescriptionGroupingPrefixLength ) == descriptionPrefix );
            }

            return query;
        }

        #endregion Filters

        #region Operations

        /// <summary>
        /// Remove all records from the Exception Log.
        /// </summary>
        public void TruncateLog()
        {
            int recordsDeleted = DbService.ExecuteCommand( "TRUNCATE TABLE ExceptionLog" );

            // TODO: We should record the log truncation action in an appropriate application log.
        }

        /// <summary>
        /// Logs new <see cref="Rock.Model.ExceptionLog" /> entities.  This method serves as an interface to asynchronously log exceptions.
        /// </summary>
        /// <param name="ex">A <see cref="System.Exception" /> object to log.</param>
        /// <param name="request">The <see cref="T:System.Net.HttpRequestMessage" /></param>
        /// <param name="personAlias">The person alias.</param>
        public static void LogApiException( Exception ex, HttpRequestMessage request, PersonAlias personAlias = null )
        {
            // Populate the initial ExceptionLog with data from HttpContext. Must capture initial
            // HttpContext details before spinning off new thread, because the current context will
            // not be the same within the context of the new thread.
            var exceptionLog = PopulateExceptionLog( ex, request, personAlias );

            // Spin off a new thread to handle the real logging work so the UI is not blocked whilst
            // recursively writing to the database.
            Task.Run( () => LogExceptions( ex, exceptionLog, true ) );
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void LogException( Exception ex )
        {
            // create a new exception model
            var exceptionLog = new ExceptionLog();
            exceptionLog.HasInnerException = ex.InnerException != null;
            exceptionLog.ExceptionType = ex.GetType().ToString();
            exceptionLog.Description = ex.Message;
            exceptionLog.Source = ex.Source;
            exceptionLog.StackTrace = ex.StackTrace;

            // Spin off a new thread to handle the real logging work so the UI is not blocked whilst
            // recursively writing to the database.
            Task.Run( () => LogExceptions( ex, exceptionLog, true ) );
        }

        /// <summary>
        /// Log an exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public static void LogException( string message )
        {
            LogException( new Exception( message ) );
        }

        /// <summary>
        /// Recursively logs exception and any children.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> to log.</param>
        /// <param name="log">The parent <see cref="Rock.Model.ExceptionLog"/> of the exception being logged. This value is nullable.</param>
        /// <param name="isParent">A <see cref="System.Boolean"/> flag indicating if this Exception is a parent exception. This value is 
        ///     <c>true</c> if the exception that is being logged is a parent exception, otherwise <c>false</c>.
        /// </param>
        private static void LogExceptions( Exception ex, ExceptionLog log, bool isParent )
        {
            bool logToFile = AlwaysLogToFile;

            // First, attempt to log exception to the database.
            try
            {
                ExceptionLog exceptionLog;

                // If this is a recursive call and not the originating exception being logged,
                // attempt to clone the initial one, and populate it with Exception Type and Message
                // from the inner exception, while retaining the contextual information from where
                // the exception originated.
                if ( !isParent )
                {
                    exceptionLog = log.Clone( false );

                    if ( exceptionLog != null )
                    {
                        // Populate with inner exception type, message and update whether or not there is another inner exception.
                        exceptionLog.ExceptionType = ex.GetType().ToString();
                        exceptionLog.Description = ex.Message;
                        exceptionLog.Source = ex.Source;
                        exceptionLog.StackTrace = ex.StackTrace;
                        exceptionLog.HasInnerException = ex.InnerException != null;

                        // Ensure EF properly recognizes this as a new record.
                        exceptionLog.Id = 0;
                        exceptionLog.Guid = Guid.NewGuid();
                        exceptionLog.ParentId = log.Id;
                    }
                }
                else
                {
                    exceptionLog = log;
                }

                // The only reason this should happen is if the `log.Clone()` operation failed. Compiler sugar.
                if ( exceptionLog == null )
                {
                    return;
                }

                // Write ExceptionLog record to database.
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    var exceptionLogService = new ExceptionLogService( rockContext );
                    exceptionLogService.Add( exceptionLog );

                    // make sure to call the regular SaveChanges so that CreatedBy,CreatedByDateTime, etc get set properly. If any of the post processing happens to also create an exception, we can just log to the exception file instead
                    rockContext.SaveChanges();
                }

                // Recurse if inner exception is found
                if ( exceptionLog.HasInnerException.GetValueOrDefault( false ) )
                {
                    if ( ex is AggregateException aggregateException )
                    {
                        foreach ( var innerException in aggregateException.InnerExceptions )
                        {
                            LogExceptions( innerException, exceptionLog, false );
                        }
                    }
                    else
                    {
                        LogExceptions( ex.InnerException, exceptionLog, false );
                    }
                }
            }
            catch ( Exception )
            {
                // If logging the exception fails, write the exceptions to a file
                logToFile = true;
            }

            if ( logToFile )
            {
                // Construct the log message.
                var sbLogMessage = new StringBuilder();
                var when = RockDateTime.Now.ToString();
                while ( ex != null )
                {
                    sbLogMessage.Append( string.Format( "{0},{1},\"{2}\",\"{3}\"\r\n", when, ex.GetType(), ex.Message, ex.StackTrace ) );
                    ex = ex.InnerException;
                }

                // Write to the log file, after ensuring that this thread has exclusive access.
                var canWriteToLogFile = _logWriterWaitHandle.WaitOne( 1000 );
                var writeToTrace = !canWriteToLogFile;
                if ( canWriteToLogFile )
                {
                    try
                    {
                        // Write the error to the log file.
                        var directory = AppDomain.CurrentDomain.BaseDirectory;
                        directory = Path.Combine( directory, "App_Data", "Logs" );

                        if ( !Directory.Exists( directory ) )
                        {
                            Directory.CreateDirectory( directory );
                        }

                        var filePath = Path.Combine( directory, "RockExceptions.csv" );
                        File.AppendAllText( filePath, sbLogMessage.ToString() );
                    }
                    catch ( Exception exLog )
                    {
                        sbLogMessage.Insert( 0, $"** Error Logging Failed.\n{exLog}\nThe Exception that could not be logged is:\n" );
                        writeToTrace = true;
                    }
                    finally
                    {
                        _logWriterWaitHandle.Set();
                    }
                }
                else
                {
                    sbLogMessage.AppendLine( "** Error Logging Failed. The log file is in use by another process." );
                }

                // If error logging has failed, write the error to Trace output.
                if ( writeToTrace )
                {
                    DebugHelper.Log( sbLogMessage.ToString() );
                }
            }
        }

        // A mutex to synchronise log file write operations for this Rock instance.
        private static EventWaitHandle _logWriterWaitHandle = new EventWaitHandle( true, EventResetMode.AutoReset, $"ROCK_EXCEPTION_LOG_{Guid.NewGuid()}" );

        #endregion Operations

        /// <summary>
        /// Populates the <see cref="Rock.Model.ExceptionLog" /> entity with the exception data.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception" /> to log.</param>
        /// <param name="request">The <see cref="T:System.Net.HttpRequestMessage" />.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        private static ExceptionLog PopulateExceptionLog( Exception ex, HttpRequestMessage request, PersonAlias personAlias )
        {
            string exceptionMessage = ex.Message;
            if ( ex is System.Data.SqlClient.SqlException )
            {
                var sqlEx = ex as System.Data.SqlClient.SqlException;
                var sqlErrorList = sqlEx.Errors.OfType<System.Data.SqlClient.SqlError>().ToList().Select( a => string.Format( "{0}: Line {1}", a.Procedure, a.LineNumber ) );
                if ( sqlErrorList.Any() )
                {
                    exceptionMessage += string.Format( "[{0}]", sqlErrorList.ToList().AsDelimited( ", " ) );
                }
            }

            var exceptionLog = new ExceptionLog
            {
                HasInnerException = ex.InnerException != null,
                ExceptionType = ex.GetType().ToString(),
                Description = exceptionMessage,
                Source = ex.Source,
                StackTrace = ex.StackTrace,
                Guid = Guid.NewGuid(),
                CreatedByPersonAliasId = personAlias?.Id,
                ModifiedByPersonAliasId = personAlias?.Id,
                CreatedDateTime = RockDateTime.Now,
                ModifiedDateTime = RockDateTime.Now,
                ModifiedAuditValuesAlreadyUpdated = true
            };

            if ( exceptionLog.StackTrace == null )
            {
                try
                {
                    // if the Exception didn't include a StackTrace, manually grab it
                    var stackTrace = new System.Diagnostics.StackTrace( 2 );
                    exceptionLog.StackTrace = stackTrace.ToString();
                }
                catch
                {
                }
            }

            try
            {
                ex.Data.Add( "ExceptionLogGuid", exceptionLog.Guid );
            }
            catch
            {
            }

            try
            {
                // If current HttpRequestMessage is null, return early.
                if ( request == null )
                {
                    return exceptionLog;
                }

                StringBuilder cookies = new StringBuilder();
                var cookieList = request.Headers.GetCookies();

                if ( cookieList.Count > 0 )
                {
                    cookies.Append( "<table class=\"cookies exception-table\">" );

                    foreach ( var cookieHeaderValue in cookieList )
                    {
                        foreach ( var cookie in cookieHeaderValue.Cookies )
                        {
                            cookies.Append( "<tr><td><b>" + cookie.Name + "</b></td><td>" + cookie.Value.EncodeHtml() + "</td></tr>" );
                        }
                    }

                    cookies.Append( "</table>" );
                }

                //
                // Check query string parameters for sensitive data.
                //
                string queryString = null;
                var queryCollection = request.RequestUri.ParseQueryString();
                if ( queryCollection.Count > 0 )
                {
                    var nvc = new NameValueCollection();
                    foreach ( string qKey in queryCollection.Keys )
                    {
                        if ( IsKeySensitive( qKey.ToLower() ) )
                        {
                            nvc.Add( qKey, "obfuscated" );
                        }
                        else
                        {
                            nvc.Add( qKey, queryCollection[qKey] );
                        }
                    }

                    queryString = "?" + string.Join( "&", nvc.AllKeys.Select( a => a.UrlEncode() + "=" + nvc[a].UrlEncode() ) );
                }

                exceptionLog.Cookies = cookies.ToString();
                exceptionLog.PageUrl = request.RequestUri.GetLeftPart( UriPartial.Path );
                exceptionLog.QueryString = queryString;
            }
            catch
            {
            }

            return exceptionLog;
        }

        /// <summary>
        /// Determines whether the value for the specific key is sensitive data..
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the value should be considered sensitive data; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsKeySensitive( string key )
        {
            return key.Contains( "nolog" ) ||
                key.Contains( "creditcard" ) ||
                key.Contains( "cc-number" ) ||
                key.Contains( "cvv" ) ||
                key.Contains( "ssn" ) ||
                key.Contains( "accountnumber" ) ||
                key.Contains( "account-number" );
        }
    }
}
