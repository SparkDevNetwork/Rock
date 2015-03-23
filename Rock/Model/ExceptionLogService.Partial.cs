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
                var rockContext = new Rock.Data.RockContext();
                var exceptionLogService = new ExceptionLogService( rockContext );
                exceptionLogService.Add( exceptionLog );
                rockContext.SaveChanges();

                // Recurse if inner exception is found
                if ( exceptionLog.HasInnerException.GetValueOrDefault( false ) )
                {
                    LogExceptions( ex.InnerException, exceptionLog, false );
                }

                if (ex is AggregateException)
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
                        File.AppendAllText( filePath, string.Format( "{0},{1},\"{2}\"\r\n", when, ex.GetType(), ex.Message ) );
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

            var exceptionLog = new ExceptionLog
                {
                    SiteId = siteId,
                    PageId = pageId,
                    HasInnerException = ex.InnerException != null,
                    ExceptionType = ex.GetType().ToString(),
                    Description = ex.Message,
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    Guid = Guid.NewGuid(),
                    CreatedByPersonAliasId = personAliasId,
                    ModifiedByPersonAliasId = personAliasId,
                    CreatedDateTime = RockDateTime.Now,
                    ModifiedDateTime = RockDateTime.Now
                };

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
            }
            catch { 
                // Intentionally do nothing
            }

            return exceptionLog;
        }
    }
}
