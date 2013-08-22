//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Rock.Data;

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
            // Spin off a new thread to handle the real logging work so the UI is not blocked.
            Task.Run( () => LogExceptions( ex, context, pageId, siteId, personId ) );
        }

        private static void LogExceptions( Exception ex, HttpContext context, int? pageId, int? siteId, int? personId, int? parentId = null )
        {
            var exceptionLog = PopulateException( ex, context, pageId, siteId, personId, parentId );
            var exceptionLogService = new ExceptionLogService();
            exceptionLogService.Add( exceptionLog, personId );
            exceptionLogService.Save( exceptionLog, personId );

            if ( exceptionLog.HasInnerException.GetValueOrDefault( false ) )
            {
                LogExceptions( ex.InnerException, context, pageId, siteId, personId, exceptionLog.Id );
            }
        }

        private static ExceptionLog PopulateException( Exception ex, HttpContext context, int? pageId, int? siteId, int? personId, int? parentId )
        {
            var request = context.Request;
            var cookies = request.Cookies;
            var cookieString = new StringBuilder();

            for ( int i = 0; i < cookies.Count; i++ )
            {
                var cookie = cookies[i];

                if ( cookie == null )
                {
                    continue;
                }

                cookieString.AppendFormat( "Name: '{0}', Value: '{1}', Expires: '{2}'|",
                    cookie.Name,
                    cookie.Value,
                    cookie.Expires.ToString() );
            }

            var serverVars = request.ServerVariables;
            var serverVarsString = new StringBuilder();

            for ( int i = 0; i < serverVars.Count; i++ )
            {
                var variable = serverVars[i];

                if ( variable == null )
                {
                    continue;
                }

                serverVarsString.AppendFormat( "'{0}'|", variable );
            }

            var form = request.Form;
            var formString = new StringBuilder();

            for ( int i = 0; i < form.Count; i++ )
            {
                var formVar = form[i];

                if ( formVar == null )
                {
                    continue;
                }

                formString.AppendFormat( "'{0}'|", formVar );
            }

            return new ExceptionLog
                {
                    SiteId = siteId,
                    PageId = pageId,
                    ParentId = parentId,
                    CreatedByPersonId = personId,
                    Cookies = cookieString.ToString(),
                    HasInnerException = ex.InnerException != null,
                    StatusCode = context.Response.StatusCode.ToString(),
                    ExceptionType = ex.GetType().ToString(),
                    Description = ex.Message,
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    PageUrl = request.Url.ToString(),
                    ServerVariables = serverVarsString.ToString(),
                    QueryString = request.Url.Query,
                    Form = formString.ToString(),
                    Guid = Guid.NewGuid(),
                    ExceptionDateTime = DateTime.Now
                };
        }
    }
}
