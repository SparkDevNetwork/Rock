//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of their Parent exceptionId. 
        /// Under most instances, only one child <see cref="Rock.Model.ExceptionLog"/> entity will be returned in the collection.
        /// </summary>
        /// <param name="parentId">An <see cref="System.Int32"/> containing the Id of the parent ExceptionLog entity to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog" /> entities who's Parent ExceptionId matches the provided value..</returns>
        public IEnumerable<ExceptionLog> GetByParentId( int? parentId )
        {
            return Repository.Find( t => ( t.ParentId == parentId || ( parentId == null && t.ParentId == null ) ) );
        }
        
        // <summary>
        // Gets Exception Logs by Person Id
        // </summary>
        // <param name="personId">Person Id.</param>
        // <returns>An enumerable list of ExceptionLog objects.</returns>
        //public IEnumerable<ExceptionLog> GetByPersonId( int? personId )
        //{
        //    return Repository.Find( t => ( t.CreatedByPersonId == personId || ( personId == null && t.PersonId == null ) ) );
        //}
        
        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.ExceptionLog"/> entities by the Id of the <see cref="Rock.Model.Site"/> that they occurred on.
        /// </summary>
        /// <param name="siteId">An <see cref="String.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.ExceptionLog"/> entities who's SiteId matches the provided value.</returns>
        public IEnumerable<ExceptionLog> GetBySiteId( int? siteId )
        {
            return Repository.Find( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) );
        }


        /// <summary>
        /// Logs new <see cref="Rock.Model.ExceptionLog"/> entities.  This method serves as an interface to asynchronously log exceptions.
        /// </summary>
        /// <param name="ex">A <see cref="System.Exception"/> object to log.</param>
        /// <param name="context">The <see cref="System.Web.HttpContext"/></param>
        /// <param name="pageId">A <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Page"/> that the exception occurred on.
        ///     This parameter is nullable..</param>
        /// <param name="siteId">A <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.site"/> that the exception occurred on.</param>
        /// <param name="parentId">The Id of the exception's parent <see cref="Rock.Model.ExceptionLog"/> entity.</param>
        /// <param name="personId">The Id of the <see cref="Rock.Model.Person"/> that caused the exception.</param>
        public static void LogException( Exception ex, HttpContext context, int? pageId = null, int? siteId = null, int? personId = null, int? parentId = null )
        {
            // Populate the initial ExceptionLog with data from HttpContext. Must capture initial
            // HttpContext details before spinning off new thread, because the current context will
            // not be the same within the context of the new thread.
            var exceptionLog = PopulateExceptionLog( ex, context, pageId, siteId, personId, parentId );

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
                var exceptionLogService = new ExceptionLogService();
                exceptionLogService.Add( exceptionLog, exceptionLog.CreatedByPersonId );
                exceptionLogService.Save( exceptionLog, exceptionLog.CreatedByPersonId );

                // Recurse if inner exception is found
                if ( exceptionLog.HasInnerException.GetValueOrDefault( false ) )
                {
                    LogExceptions( ex.InnerException, exceptionLog, false );
                }
            }
            catch ( Exception )
            {
                // If logging the exception fails, write the exception to a file
                try
                {
                    string directory = AppDomain.CurrentDomain.BaseDirectory;
                    directory = Path.Combine( directory, "App_Data", "Logs" );

                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    string filePath = Path.Combine( directory, "RockExceptions.csv" );
                    File.AppendAllText( filePath, string.Format( "{0},{1},\"{2}\"\r\n", DateTime.Now.ToString(), ex.GetType(), ex.Message ) );
                }
                catch
                {
                    // failed to write to database and also failed to write to log file, so there is nowhere to log this error
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="Rock.Model.ExceptionLog"/> entity with the exception data.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> to log.</param>
        /// <param name="context">The <see cref="System.Web.HttpContext"/>.</param>
        /// <param name="pageId">An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Page"/> where the exception occurred.
        ///     This value is nullable.
        /// </param>
        /// <param name="siteId">An <see cref="System.Int32"/> containing the Id the <see cref="Rock.Model.Site"/> where the exception occurred.
        ///     This value is nullable.
        /// </param>
        /// <param name="personId">The Id of the <see cref="Rock.Model.Person"/> who was logged in when the exception occurred. If the anonymous 
        /// user was logged in or if the exception was caused by an job/process this will be null..</param>
        /// <param name="parentId">The Id of the Exception's parent <see cref="Rock.Model.ExceptionLog"/> entity. If this exception does not have an 
        /// outer/parent exception this value will be null.</param>
        /// <returns></returns>
        private static ExceptionLog PopulateExceptionLog( Exception ex, HttpContext context, int? pageId, int? siteId, int? personId, int? parentId )
        {
            var exceptionLog = new ExceptionLog
                {
                    SiteId = siteId,
                    PageId = pageId,
                    ParentId = parentId,
                    CreatedByPersonId = personId,
                    HasInnerException = ex.InnerException != null,
                    ExceptionType = ex.GetType().ToString(),
                    Description = ex.Message,
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    Guid = Guid.NewGuid(),
                    ExceptionDateTime = DateTime.Now
                };

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
                        cookies.Append( "<tr><td><b>" + cookie + "</b></td><td>" + httpCookie.Value + "</td></tr>" );
                }

                cookies.Append( "</table>" );
            }
            
            StringBuilder formItems = new StringBuilder();
            var formList = request.Form;

            if ( formList.Count > 0 )
            {
                formItems.Append( "<table class=\"form-items exception-table\">" );

                foreach ( string formItem in formList )
                    formItems.Append( "<tr><td><b>" + formItem + "</b></td><td>" + formList[formItem] + "</td></tr>" );

                formItems.Append( "</table>" );
            }

            StringBuilder serverVars = new StringBuilder();
            var serverVarList = request.ServerVariables;

            if ( serverVarList.Count > 0 )
            {
                serverVars.Append( "<table class=\"server-variables exception-table\">" );

                foreach ( string serverVar in serverVarList )
                    serverVars.Append( "<tr><td><b>" + serverVar + "</b></td><td>" + serverVarList[serverVar] + "</td></tr>" );

                serverVars.Append( "</table>" );
            }

            exceptionLog.Cookies = cookies.ToString();
            exceptionLog.StatusCode = context.Response.StatusCode.ToString();
            exceptionLog.PageUrl = request.Url.ToString();
            exceptionLog.ServerVariables = serverVars.ToString();
            exceptionLog.QueryString = request.Url.Query;
            exceptionLog.Form = formItems.ToString();
            return exceptionLog;
        }
    }
}
