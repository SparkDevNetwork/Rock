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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rock.Model
{
    /// <summary>
    /// ExceptionLog Properties that use System.Web.
    /// </summary>
    public partial class ExceptionLogService
    {
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
            if ( personAlias != null )
            {
                personAliasId = personAlias.Id;
            }

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
                // If current HttpContext is null, return early.
                if ( context == null )
                {
                    return exceptionLog;
                }

                // If current HttpContext is available, populate its information as well.
                var request = context.Request;
                var routeToken = request.RequestContext?.RouteData?.Values["rckipid"] as string;

                StringBuilder cookies = new StringBuilder();
                var cookieList = request.Cookies;

                if ( cookieList.Count > 0 )
                {
                    cookies.Append( "<table class=\"cookies exception-table\">" );

                    foreach ( string cookie in cookieList )
                    {
                        var httpCookie = cookieList[cookie];
                        if ( httpCookie != null )
                        {
                            /*
                                10/19/2026 - MSE

                                Cookie values hold the .ROCK sign-in ticket, the session id and
                                other secrets, so only the cookie names are stored. Names are HTML
                                encoded because Exception Detail renders this table as HTML.

                                Reason: Exception log must not store sign-in tickets or allow stored script.
                            */
                            cookies.Append( "<tr><td><b>" + cookie.EncodeHtml() + "</b></td><td>" + RedactedValue + "</td></tr>" );
                        }
                    }

                    cookies.Append( "</table>" );
                }

                StringBuilder serverVars = new StringBuilder();

                // 'serverVarList[serverVar]' throws an exception if the value is empty, even if the key exists,
                // so make a copy of the request server variables to help avoid that error
                var serverVarList = new NameValueCollection( request.ServerVariables );
                var serverVarListString = serverVarList.ToString();

                var serverVarKeys = request.ServerVariables.AllKeys;

                if ( serverVarList.Count > 0 )
                {
                    serverVars.Append( "<table class=\"server-variables exception-table\">" );

                    foreach ( string serverVar in serverVarList )
                    {
                        string val = string.Empty;
                        try
                        {
                            /*
                                10/19/2026 - MSE

                                Server variables include the cookie and authorization headers
                                (HTTP_COOKIE, ALL_RAW, HTTP_AUTHORIZATION_TOKEN...) and URLs that
                                can carry an rckipid or API key. Staff can read the exception log,
                                so credential values are redacted before they are stored.

                                Reason: Exception log must not store sign-in tickets, REST keys or rckipid tokens.
                            */
                            val = RedactServerVariable( serverVar, serverVarList[serverVar].ToStringSafe(), routeToken ).EncodeHtml();
                        }
                        catch
                        {
                            // ignore
                        }

                        serverVars.Append( $"<tr><td><b>{serverVar.EncodeHtml()}</b></td><td>{val}</td></tr>" );
                    }

                    serverVars.Append( "</table>" );
                }

                exceptionLog.Cookies = cookies.ToString();
                exceptionLog.StatusCode = context.Response.StatusCode.ToString();

                /*
                    10/19/2026 - MSE

                    The URL and query string can carry an rckipid token, an API key or
                    a login code, so sensitive values are redacted before they are stored.
                    AbsoluteUri is used because ToString() decodes %26 inside a value,
                    which would end the redaction early and keep part of the secret.

                    Reason: Exception log must not store impersonation tokens or REST keys.
                */
                exceptionLog.PageUrl = RedactUrl( request.UrlProxySafe().AbsoluteUri );
                exceptionLog.ServerVariables = serverVars.ToString();
                exceptionLog.QueryString = RedactQueryParameters( request.UrlProxySafe().Query );
                /*
                     SK - 11/24/2021
                     We are commenting out below line as we have decided not to store form data from now on as it may contain sensative data.
                     exceptionLog.Form = formItems.ToString();
                */
            }
            catch
            {
            }

            return exceptionLog;
        }

        /// <summary>
        /// Hides sensitive query values and rckipid tokens in a URL before it is
        /// written to the exception log. Never throws; if the URL cannot be
        /// processed then the part before the query string is returned.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The URL with sensitive values replaced.</returns>
        [Rock.Attribute.RockInternal( "17.12", true )]
        public static string RedactUrl( string url )
        {
            if ( string.IsNullOrEmpty( url ) )
            {
                return url;
            }

            try
            {
                return RedactQueryParameters( PersonToken.ObfuscateRockMagicToken( url ) );
            }
            catch
            {
                var queryIndex = url.IndexOf( '?' );

                return queryIndex >= 0 ? url.Substring( 0, queryIndex ) : url;
            }
        }
    }
}
