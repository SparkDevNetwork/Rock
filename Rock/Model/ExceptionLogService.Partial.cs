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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        #endregion

        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of their Parent exceptionId. 
        /// Under most instances, only one child <see cref="Rock.Model.ExceptionLog"/> entity will be returned in the collection.
        /// </summary>
        /// <param name="parentId">An <see cref="System.Int32"/> containing the Id of the parent ExceptionLog entity to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog" /> entities who's Parent ExceptionId matches the provided value..</returns>
        public IQueryable<ExceptionLog> GetByParentId( int? parentId )
        {
            return Queryable().Where( t => ( t.ParentId == parentId || ( parentId == null && t.ParentId == null ) ) );
        }
        
        // <summary>
        // Gets Exception Logs by Person Id
        // </summary>
        // <param name="personId">Person Id.</param>
        // <returns>An enumerable list of ExceptionLog objects.</returns>
        //public IQueryable<ExceptionLog> GetByPersonId( int? personId )
        //{
        //    return Queryable().Where( t => ( t.CreatedByPersonId == personId || ( personId == null && t.PersonId == null ) ) );
        //}
        
        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of the <see cref="Rock.Model.Site"/> that they occurred on.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog"/> entities who's SiteId matches the provided value.</returns>
        public IQueryable<ExceptionLog> GetBySiteId( int? siteId )
        {
            return Queryable().Where( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) );
        }


        /// <summary>
        /// Logs new <see cref="Rock.Model.ExceptionLog" /> entities.  This method serves as an interface to asynchronously log exceptions.
        /// </summary>
        /// <param name="ex">A <see cref="System.Exception" /> object to log.</param>
        /// <param name="context">The <see cref="System.Web.HttpContext" /></param>
        /// <param name="pageId">A <see cref="System.Int32" /> containing the Id of the <see cref="Rock.Model.Page" /> that the exception occurred on.
        /// This parameter is nullable..</param>
        /// <param name="siteId">A <see cref="System.Int32" /> containing the Id of the <see cref="Rock.Model.Site" /> that the exception occurred on.</param>
        /// <param name="personAlias">The person alias.</param>
        public static void LogException( Exception ex, HttpContext context, int? pageId = null, int? siteId = null, PersonAlias personAlias = null )
        {
            // Populate the initial ExceptionLog with data from HttpContext. Must capture initial
            // HttpContext details before spinning off new thread, because the current context will
            // not be the same within the context of the new thread.
            var exceptionLog = PopulateExceptionLog( ex, context, pageId, siteId, personAlias );

            // Spin off a new thread to handle the real logging work so the UI is not blocked whilst
            // recursively writing to the database.
            Task.Run( () => LogExceptions( ex, exceptionLog, true ) );
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
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
                    LogExceptions( ex.InnerException, exceptionLog, false );
                }

                if ( ex is AggregateException )
                {
                    // if an AggregateException occurs, log the exceptions individually
                    var aggregateException = ( ex as AggregateException );
                    foreach ( var innerException in aggregateException.InnerExceptions )
                    {
                        LogExceptions( innerException, exceptionLog, false );
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
                try
                {
                    string directory = AppDomain.CurrentDomain.BaseDirectory;
                    directory = Path.Combine( directory, "App_Data", "Logs" );

                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    string filePath = Path.Combine( directory, "RockExceptions.csv" );
                    string when = RockDateTime.Now.ToString();
                    while ( ex != null )
                    {
                        File.AppendAllText( filePath, string.Format( "{0},{1},\"{2}\",\"{3}\"\r\n", when, ex.GetType(), ex.Message, ex.StackTrace ) );
                        ex = ex.InnerException;
                    }
                }
                catch
                {
                    // failed to write to database and also failed to write to log file, so there is nowhere to log this error
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="Rock.Model.ExceptionLog" /> entity with the exception data.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception" /> to log.</param>
        /// <param name="context">The <see cref="System.Web.HttpContext" />.</param>
        /// <param name="pageId">An <see cref="System.Int32" /> containing the Id of the <see cref="Rock.Model.Page" /> where the exception occurred.
        /// This value is nullable.</param>
        /// <param name="siteId">An <see cref="System.Int32" /> containing the Id the <see cref="Rock.Model.Site" /> where the exception occurred.
        /// This value is nullable.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        private static ExceptionLog PopulateExceptionLog( Exception ex, HttpContext context, int? pageId, int? siteId, PersonAlias personAlias )
        {
            int? personAliasId = null;
            if (personAlias != null)
            {
                personAliasId = personAlias.Id;
            }

            string exceptionMessage = ex.Message;
            if ( ex is System.Data.SqlClient.SqlException )
            {
                var sqlEx = ex as System.Data.SqlClient.SqlException;
                var sqlErrorList = sqlEx.Errors.OfType<System.Data.SqlClient.SqlError>().ToList().Select(a => string.Format("{0}: Line {1}", a.Procedure, a.LineNumber));
                if ( sqlErrorList.Any() )
                {
                    exceptionMessage += string.Format( "[{0}]", sqlErrorList.ToList().AsDelimited(", ") );
                }
            }

            var exceptionLog = new ExceptionLog
                {
                    SiteId = siteId,
                    PageId = pageId,
                    HasInnerException = ex.InnerException != null,
                    ExceptionType = ex.GetType().ToString(),
                    Description = exceptionMessage,
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    Guid = Guid.NewGuid(),
                    CreatedByPersonAliasId = personAliasId,
                    ModifiedByPersonAliasId = personAliasId,
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
                catch { }
            }

            try
            {
                ex.Data.Add( "ExceptionLogGuid", exceptionLog.Guid );
            }
            catch { }

            try
            {
                // If current HttpContext is null, return early.
                if ( context == null )
                {
                    return exceptionLog;
                }

                // If current HttpContext is available, populate its information as well.
                var request = context.Request;

                StringBuilder cookies = new StringBuilder();
                var cookieList = request.Cookies;

                if ( cookieList.Count > 0 )
                {
                    cookies.Append( "<table class=\"cookies exception-table\">" );

                    foreach ( string cookie in cookieList )
                    {
                        var httpCookie = cookieList[cookie];
                        if ( httpCookie != null )
                            cookies.Append( "<tr><td><b>" + cookie + "</b></td><td>" + httpCookie.Value.EncodeHtml() + "</td></tr>" );
                    }

                    cookies.Append( "</table>" );
                }

                StringBuilder formItems = new StringBuilder();
                var formList = request.Form;

                if ( formList.Count > 0 )
                {
                    formItems.Append( "<table class=\"form-items exception-table\">" );
                    foreach ( string formItem in formList )
                    {
                        if ( formItem.IsNotNullOrWhiteSpace() )
                        {
                            string formValue = formList[formItem].EncodeHtml();
                            string lc = formItem.ToLower();
                            if ( lc.Contains( "nolog" ) ||
                                lc.Contains( "creditcard" ) ||
                                lc.Contains( "cc-number" ) ||
                                lc.Contains( "cvv" ) ||
                                lc.Contains( "ssn" ) ||
                                lc.Contains( "accountnumber" ) ||
                                lc.Contains( "account-number" ) )
                            {
                                formValue = "***obfuscated***";
                            }
                            formItems.Append( "<tr><td><b>" + formItem + "</b></td><td>" + formValue + "</td></tr>" );
                        }
                    }
                    formItems.Append( "</table>" );
                }

                StringBuilder serverVars = new StringBuilder();
                var serverVarList = request.ServerVariables;

                if ( serverVarList.Count > 0 )
                {
                    serverVars.Append( "<table class=\"server-variables exception-table\">" );

                    foreach ( string serverVar in serverVarList )
                    {
                        string val = string.Empty;
                        try
                        {
                            // 'serverVarList[serverVar]' throws an exception if the value is empty, even if the key exists. Was not able to find a more elegant way to avoid an exception. 
                            val = serverVarList[serverVar].ToStringSafe().EncodeHtml();
                        }
                        catch { }

                        serverVars.Append( $"<tr><td><b>{serverVar}</b></td><td>{val}</td></tr>" );
                    }

                    serverVars.Append( "</table>" );
                }

                exceptionLog.Cookies = cookies.ToString();
                exceptionLog.StatusCode = context.Response.StatusCode.ToString();
                exceptionLog.PageUrl = request.Url.ToString();
                exceptionLog.ServerVariables = serverVars.ToString();
                exceptionLog.QueryString = request.Url.Query;
                exceptionLog.Form = formItems.ToString();
            }
            catch { }

            return exceptionLog;
        }
    }
}
