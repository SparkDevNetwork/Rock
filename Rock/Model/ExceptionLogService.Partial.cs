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
    /// Exception Log POCO Service class
    /// </summary>
    public partial class ExceptionLogService 
    {
        /// <summary>
        /// Gets Exception Logs by Parent Id
        /// </summary>
        /// <param name="parentId">Parent Id.</param>
        /// <returns>An enumerable list of ExceptionLog objects.</returns>
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
        /// Gets Exception Logs by Site Id
        /// </summary>
        /// <param name="siteId">Site Id.</param>
        /// <returns>An enumerable list of ExceptionLog objects.</returns>
        public IEnumerable<ExceptionLog> GetBySiteId( int? siteId )
        {
            return Repository.Find( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) );
        }


        /// <summary>
        /// Public static method to log an exception, serves as an interface to log asynchronously.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="context">The context.</param>
        /// <param name="pageId">The page id.</param>
        /// <param name="siteId">The site id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <param name="personId">The person id.</param>
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
        /// <param name="ex">The System.Exception to log.</param>
        /// <param name="log">The parent ExceptionLog.</param>
        /// <param name="isParent">if set to <c>true</c> [is parent].</param>
        private static void LogExceptions( Exception ex, ExceptionLog log, bool isParent )
        {
            // First, attempt to log exception to the database.
            try
            {
                ExceptionLog exceptionLog;

                // If this is a recursive call and not the originating exception being logged,
                // attempt to clone the initial one, and populate it with Exception Type and Message
                // from the inner excetpion, while retaining the contextual information from where
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
        /// Populates the ExceptionLog model with information from HttpContext and Exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="context">The context.</param>
        /// <param name="pageId">The page id.</param>
        /// <param name="siteId">The site id.</param>
        /// <param name="personId">The person id.</param>
        /// <param name="parentId">The parent id.</param>
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
