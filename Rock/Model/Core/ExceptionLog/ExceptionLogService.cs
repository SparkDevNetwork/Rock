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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Logging;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.ExceptionLog"/> entity type objects.
    /// </summary>
    [RockLoggingCategory]
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
        /// This method is declared static as it is not using any properties of the class.
        /// </summary>
        public static void TruncateLog()
        {
            DbService.ExecuteCommand( "TRUNCATE TABLE ExceptionLog" );
            RockLogger.LoggerFactory.CreateLogger<ExceptionLogService>()
                .LogInformation( "The Exception Log Table has been truncated." );
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

#if WEBFORMS
            if ( request.Properties["MS_HttpContext"] is System.Web.HttpContextWrapper httpContext )
            {
                var serverVars = new StringBuilder();

                // 'serverVarList[serverVar]' throws an exception if the value is empty, even if the key exists,
                // so make a copy of the request server variables to help avoid that error
                var serverVarList = new NameValueCollection( httpContext.Request.ServerVariables );
                var serverVarListString = serverVarList.ToString();

                var serverVarKeys = httpContext.Request.ServerVariables.AllKeys;
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
                            val = RedactServerVariable( serverVar, serverVarList[serverVar].ToStringSafe(), null ).EncodeHtml();
                        }
                        catch
                        {
                            // ignore
                        }

                        serverVars.Append( $"<tr><td><b>{serverVar.EncodeHtml()}</b></td><td>{val}</td></tr>" );
                    }

                    serverVars.Append( "</table>" );

                    exceptionLog.ServerVariables = serverVars.ToString();
                }
            }
#endif

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
                            /*
                                10/19/2026 - MSE

                                Cookie values hold the .ROCK sign-in ticket, the session id and
                                other secrets, so only the cookie names are stored. Names are HTML
                                encoded because Exception Detail renders this table as HTML.

                                Reason: Exception log must not store sign-in tickets or allow stored script.
                            */
                            cookies.Append( "<tr><td><b>" + cookie.Name.EncodeHtml() + "</b></td><td>" + RedactedValue + "</td></tr>" );
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
                            nvc.Add( qKey, RedactedValue );
                        }
                        else
                        {
                            nvc.Add( qKey, queryCollection[qKey] );
                        }
                    }

                    /*
                        10/19/2026 - MSE

                        Only top level keys are checked above, so a token nested inside
                        another value (returnurl=%2Fpage%3Frckipid%3D...) is redacted here.

                        Reason: Exception log must not store impersonation tokens or REST keys.
                    */
                    queryString = RedactQueryParameters( "?" + string.Join( "&", nvc.AllKeys.Select( a => a.UrlEncode() + "=" + nvc[a].UrlEncode() ) ) );
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
            if ( key == null )
            {
                return false;
            }

            /*
                10/19/2026 - MSE

                rckipid and rckid (person action identifier) act as the person, apikey is
                the REST key, token covers access_token, id_token and similar, cc is the
                account confirmation code and code is the passwordless login and OIDC
                callback code. Their values must never reach the exception log.

                Reason: Exception log must not store impersonation tokens, REST keys or login codes.
            */
            return key.Contains( "nolog" ) ||
                key.Contains( "creditcard" ) ||
                key.Contains( "cc-number" ) ||
                key.Contains( "cvv" ) ||
                key.Contains( "ssn" ) ||
                key.Contains( "accountnumber" ) ||
                key.Contains( "account-number" ) ||
                key.Contains( "rckipid" ) ||
                key.Contains( "rckid" ) ||
                key.Contains( "personactionidentifier" ) ||
                key.Contains( "apikey" ) ||
                key.Contains( "api-key" ) ||
                key.Contains( "token" ) ||
                key.Contains( "password" ) ||
                key.Contains( "secret" ) ||
                key == "cc" ||
                key == "code";
        }

        /// <summary>
        /// The text written to the exception log in place of a sensitive value.
        /// </summary>
        internal const string RedactedValue = "obfuscated";

        /// <summary>
        /// The text written in place of an rckipid route segment. Matches
        /// <see cref="PersonToken.ObfuscateRockMagicToken(string)"/>.
        /// </summary>
        private const string RedactedRouteToken = "XXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        /// <summary>
        /// The shortest rckipid route value that is treated as a token and replaced.
        /// </summary>
        private const int MinimumRouteTokenLength = 16;

        /// <summary>
        /// Matches each key=value pair at the start of the text or after a ?, &amp; or ;.
        /// </summary>
        private static readonly Regex _queryParameterRegex = new Regex( @"(?<=^|[?&;])([^=&;#?\s]+)=([^&;#\s]*)", RegexOptions.Compiled );

        /// <summary>
        /// The characters that end a query string value as ASP.NET reads it.
        /// </summary>
        private static readonly char[] _queryValueEndCharacters = new[] { '&', '#' };

        /// <summary>
        /// Matches a sensitive key followed by a raw or URL encoded = anywhere in the
        /// text, such as inside another parameter's value. The value runs to the next
        /// &amp;, %26 or #. The short cc and code keys must start a parameter.
        /// </summary>
        private static readonly Regex _nestedSensitiveValueRegex = new Regex(
            @"(?<key>rckipid|rckid|personactionidentifier|apikey|api-key|token|password|secret|(?<=^|[?&;]|%3F|%26)(?:cc|code))(?<separator>=|%3D)(?:(?!%26)[^&#])*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase );

        /// <summary>
        /// Replaces the value of every sensitive key=value pair in a URL, query string
        /// or similar text with <see cref="RedactedValue"/>. Never throws; if the text
        /// cannot be processed then <see cref="RedactedValue"/> is returned.
        /// </summary>
        /// <param name="text">The URL, query string or other text.</param>
        /// <returns>The text with sensitive values replaced.</returns>
        internal static string RedactQueryParameters( string text )
        {
            if ( string.IsNullOrEmpty( text ) )
            {
                return text;
            }

            try
            {
                /*
                    10/19/2026 - MSE

                    ASP.NET reads a query value up to the next & or #, so a sensitive
                    value is redacted up to there, not just to the first space or ;.
                    A second pass then redacts sensitive keys nested inside another
                    value (returnurl=%2Fpage%3Frckipid%3D...), in raw or encoded form.

                    Reason: Exception log must not store any part of a token, key or password.
                */
                var result = new StringBuilder();
                var position = 0;
                var match = _queryParameterRegex.Match( text );

                while ( match.Success )
                {
                    var key = match.Groups[1].Value;

                    // Rock decodes keys when it reads them, so check the decoded key.
                    if ( IsKeySensitive( SafeUnescape( key ).ToLowerInvariant() ) )
                    {
                        var valueStart = match.Groups[2].Index;
                        var valueEnd = text.IndexOfAny( _queryValueEndCharacters, valueStart );

                        if ( valueEnd < 0 )
                        {
                            valueEnd = text.Length;
                        }

                        result.Append( text, position, valueStart - position ).Append( RedactedValue );
                        position = valueEnd;
                        match = _queryParameterRegex.Match( text, position );
                    }
                    else
                    {
                        match = match.NextMatch();
                    }
                }

                result.Append( text, position, text.Length - position );

                return _nestedSensitiveValueRegex.Replace( result.ToString(), m => m.Groups["key"].Value + m.Groups["separator"].Value + RedactedValue );
            }
            catch
            {
                return RedactedValue;
            }
        }

        /// <summary>
        /// Returns the value of a server variable as it should be written to the
        /// exception log. Variables that carry credentials are replaced entirely,
        /// others have sensitive query values and the rckipid route token replaced.
        /// Never throws; if the value cannot be processed then
        /// <see cref="RedactedValue"/> is returned.
        /// </summary>
        /// <param name="name">The server variable name.</param>
        /// <param name="value">The server variable value.</param>
        /// <param name="routeToken">The rckipid route value of the request, if any.</param>
        /// <returns>The value to write to the exception log.</returns>
        internal static string RedactServerVariable( string name, string value, string routeToken )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return value;
            }

            try
            {
                var upperName = ( name ?? string.Empty ).ToUpperInvariant();

                if ( upperName == "ALL_HTTP"
                    || upperName == "ALL_RAW"
                    || upperName.Contains( "COOKIE" )
                    || upperName.Contains( "AUTHORIZATION" )
                    || upperName.Contains( "PASSWORD" )
                    || upperName.Contains( "TOKEN" )
                    || upperName.Contains( "APIKEY" )
                    || upperName.Contains( "API_KEY" )
                    || upperName.Contains( "SECRET" )
                    || upperName.Contains( "JWT" )
                    || upperName.Contains( "OIDC" ) )
                {
                    return RedactedValue;
                }

#if WEBFORMS
                if ( upperName == "HTTP_REFERER" )
                {
                    // The referrer can be a page URL with an {rckipid} route segment.
                    value = PersonToken.ObfuscateRockMagicToken( value );
                }
#endif

                var redacted = RedactQueryParameters( value );

                /*
                    10/19/2026 - MSE

                    Variables such as UNENCODED_URL keep the route token URL encoded,
                    so the encoded forms are replaced too. Short route values are not
                    tokens and replacing them would garble the whole row.

                    Reason: Exception log must not store rckipid route tokens.
                */
                if ( !string.IsNullOrEmpty( routeToken ) && routeToken.Length >= MinimumRouteTokenLength )
                {
                    redacted = redacted.Replace( routeToken, RedactedRouteToken )
                        .Replace( Uri.EscapeDataString( routeToken ), RedactedRouteToken )
                        .Replace( System.Web.HttpUtility.UrlEncode( routeToken ), RedactedRouteToken );
                }

                return redacted;
            }
            catch
            {
                return RedactedValue;
            }
        }

        /// <summary>
        /// URL decodes the text, returning the original text if it cannot be decoded.
        /// </summary>
        /// <param name="text">The text to decode.</param>
        /// <returns>The decoded text.</returns>
        private static string SafeUnescape( string text )
        {
            try
            {
                return Uri.UnescapeDataString( text.Replace( '+', ' ' ) );
            }
            catch
            {
                return text;
            }
        }
    }
}
