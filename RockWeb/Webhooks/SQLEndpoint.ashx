<%@ WebHandler Language="C#" Class="com.blueboxmoon.DataToolkit.SQLEndpointHandler" %>

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.blueboxmoon.DataToolkit
{
    /// <summary>
    /// A webhook for processing the request with SQL.
    /// </summary>
    public class SQLEndpointHandler : IHttpHandler
    {
        /// <summary>
        /// Process the incoming http request. This is the web handler entry point.
        /// </summary>
        /// <param name="context">The context that contains all information about this request.</param>
        public void ProcessRequest( HttpContext context )
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                //
                // Find the valid api handler for this request.
                //
                var api = GetApiForRequest( context.Request, parameters );
                if ( api != null )
                {
                    var currentUser = UserLoginService.GetCurrentUser();
                    int currentPersonId = currentUser != null ? currentUser.PersonId.Value : 0;

                    //
                    // Get the necessary values from the defined value.
                    //
                    string sql = api.GetAttributeValue( "SQL" );
                    var groupGuids = api.GetAttributeValue( "SecurityRoles" ).SplitDelimitedValues().AsGuidList();

                    //
                    // Check if this API is secured.
                    //
                    if ( groupGuids.Any() )
                    {
                        bool valid = new GroupMemberService( new RockContext() ).Queryable()
                            .Where( m => m.PersonId == currentPersonId && groupGuids.Contains( m.Group.Guid ) && m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Any();

                        if ( !valid )
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.StatusCode = 403;
                            context.Response.Write( "Access denied." );
                            return;
                        }
                    }

                    //
                    // Get the raw body content.
                    //
                    using ( StreamReader reader = new StreamReader( context.Request.InputStream, Encoding.UTF8 ) )
                    {
                        parameters.AddOrReplace( "Body", reader.ReadToEnd() );
                    }

                    parameters.AddOrReplace( "PersonId", currentPersonId );

                    //
                    // Add in any optional parameters from the query string.
                    //
                    sql = AddOptionalParameters( api, context.Request, parameters, sql );

                    var results = DbService.GetDataSet( sql, CommandType.Text, parameters, null );

                    var rows = new List<Dictionary<string, object>>();
                    if ( results.Tables.Count > 0 )
                    {
                        foreach ( DataRow row in results.Tables[0].Rows )
                        {
                            var r = new Dictionary<string, object>();

                            foreach ( DataColumn column in results.Tables[0].Columns )
                            {
                                r.Add( column.ColumnName, row[column] );
                            }

                            rows.Add( r );
                        }
                    }

                    context.Response.ContentType = "application/json";
                    context.Response.Write( JsonConvert.SerializeObject( rows ) );

                    return;
                }

                // If we got here then something went wrong, probably we couldn't find a matching api.
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 404;
                context.Response.Write( "Path not found." );
            }
            catch ( Exception e )
            {
                context.Response.StatusCode = 500;
                WriteToLog( e.Message );
                ExceptionLogService.LogException( e, context );
                Dictionary<string, string> result = new Dictionary<string, string>();
                result.Add( "error", e.Message );
                context.Response.Write( JsonConvert.SerializeObject( result ) );
            }
        }

        /// <summary>
        /// These webhooks are not reusable and must only be used once.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #region Main Methods

        /// <summary>
        /// Retrieve the DefinedValueCache for this request by matching the Method and Url
        /// </summary>
        /// <param name="request">The HttpRequest object that this Api request is for.</param>
        /// <returns>
        /// A DefinedValue for the API request that was matched or null if one was not found.
        /// </returns>
        protected DefinedValueCache GetApiForRequest( HttpRequest request, Dictionary<string, object> parameters )
        {
            var url = "/" + string.Join( "", request.Url.Segments.SkipWhile( s => !s.EndsWith( ".ashx", StringComparison.InvariantCultureIgnoreCase ) && !s.EndsWith( ".ashx/", StringComparison.InvariantCultureIgnoreCase ) ).Skip( 1 ).ToArray() );

            var dt = DefinedTypeCache.Get( SystemGuid.DefinedType.SQL_API_ENDPOINTS.AsGuid() );
            if ( dt != null )
            {
                foreach ( DefinedValueCache api in dt.DefinedValues.OrderBy( h => h.Order ) )
                {
                    string apiUrl = api.Value;
                    string apiMethod = api.GetAttributeValue( "Method" );


                    List<string> variables = new List<string>();

                    if ( !apiUrl.StartsWith( "/" ) )
                    {
                        apiUrl = "/" + apiUrl;
                    }

                    //
                    // Check for match on method type, if not continue to the next item.
                    //
                    if ( !string.IsNullOrEmpty( apiMethod ) && !request.HttpMethod.ToString().Equals( apiMethod, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        continue;
                    }

                    //
                    // Check for empty url filter, match all.
                    //
                    if ( string.IsNullOrEmpty( apiUrl ) )
                    {
                        return api;
                    }

                    //
                    // Check for any {variable} style routing replacements.
                    //
                    var apiUrlMatches = Regex.Matches( apiUrl, "\\{(.+?)\\}", RegexOptions.IgnoreCase );
                    foreach ( Match routeMatch in apiUrlMatches )
                    {
                        var variable = routeMatch.Groups[1].Value;
                        variables.Add( variable );
                        apiUrl = apiUrl.Replace( string.Format( "{{{0}}}", variable ), string.Format( "(?<{0}>[^\\/]*)", variable ) );
                    }

                    //
                    // Ensure the url is a full-line match if they did not specify any
                    // regular expression options.
                    //
                    if ( !apiUrl.StartsWith( "^" ) && !apiUrl.EndsWith( "$" ) )
                    {
                        apiUrl = string.Format( "^{0}$", apiUrl );
                    }

                    //
                    // Use regular expression to see if this is a match.
                    //
                    var regex = new Regex( apiUrl, RegexOptions.IgnoreCase );
                    var match = regex.Match( url );
                    if ( match.Success )
                    {
                        foreach ( var variable in variables )
                        {
                            parameters.Add( variable, ParameterValue( match.Groups[variable].Value ) );
                        }

                        return api;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Adds in any optional parameters that were defined on the API.
        /// </summary>
        /// <param name="api">The DefinedValue API that we are processing.</param>
        /// <param name="request">The Request object that contains the query parameters.</param>
        /// <param name="parameters">The parameters that will be passed to SQL.</param>
        /// <param name="sql">The SQL string to allow us to modify the query before executing.</param>
        /// <returns>A string that contains the modified SQL query to execute.</returns>
        protected string AddOptionalParameters( DefinedValueCache api, HttpRequest request, Dictionary<string, object> parameters, string sql )
        {
            var rawValue = api.GetAttributeValue( "OptionalParameters" );
            var attribute = api.Attributes["OptionalParameters"];
            var field = ( Rock.Field.Types.KeyValueListFieldType ) attribute.FieldType.Field;
            var items = field.GetValuesFromString( null, rawValue, attribute.QualifierValues, false );

            foreach ( var kvp in items )
            {
                var values = request.QueryString.GetValues( kvp.Key );

                if ( values == null || values.Length == 0 )
                {
                    parameters.AddOrIgnore( kvp.Key, ParameterValue( kvp.Value.ToString() ) );
                }
                else
                {
                    parameters.AddOrIgnore( kvp.Key, ParameterValue( values[0] ) );

                    //
                    // If multiple values specified, process "IN (@Parameter)" formatting.
                    //
                    if ( values.Length > 1 )
                    {
                        var keys = new List<string>();

                        for ( int i = 0; i < values.Length; i++ )
                        {
                            var key = string.Format( "@{0}__{1}", kvp.Key, i );
                            parameters.Add( key, ParameterValue( values[i] ) );
                            keys.Add( key );
                        }

                        sql = Regex.Replace( sql, string.Format( @"\(\s*@{0}\s*\)", kvp.Key ), string.Format( "({0})", string.Join( ",", keys ) ) );
                    }
                }
            }

            return sql;
        }

        /// <summary>
        /// Convert the parameter value into our best guess as to what native type it should be.
        /// </summary>
        /// <param name="value">The value as a string to parse.</param>
        /// <returns>The value in the intended type.</returns>
        protected object ParameterValue( string value )
        {
            int intValue;
            double doubleValue;

            if ( int.TryParse( value, out intValue ) )
            {
                return intValue;
            }
            else if ( double.TryParse( value, out doubleValue ) )
            {
                return doubleValue;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Log a message to the LavaApi.txt file. The message is prefixed by
        /// the date and the class name.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        protected void WriteToLog( string message )
        {
            string logFile = HttpContext.Current.Server.MapPath( "~/App_Data/Logs/Sql.txt" );

            // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
            var maxRetry = 3;
            for ( int retry = 0; retry < maxRetry; retry++ )
            {
                try
                {
                    using ( FileStream fs = new FileStream( logFile, FileMode.Append, FileAccess.Write ) )
                    {
                        using ( StreamWriter sw = new StreamWriter( fs ) )
                        {
                            sw.WriteLine( string.Format( "{0} [{2}] - {1}", RockDateTime.Now.ToString(), message, GetType().Name ) );
                            break;
                        }
                    }
                }
                catch ( IOException )
                {
                    if ( retry < maxRetry - 1 )
                    {
                        System.Threading.Thread.Sleep( 2000 );
                    }
                }
            }

        }

        #endregion
    }
}