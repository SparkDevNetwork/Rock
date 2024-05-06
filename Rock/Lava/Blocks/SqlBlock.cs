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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Observability;
using Rock.Web.Cache.NonEntities;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Sql stores the result of provided SQL query into a variable.
    ///
    /// {% sql results %}
    /// SELECT [FirstName], [LastName] FROM [Person]
    /// {% endsql %}
    /// </summary>
    public class SqlBlock : LavaBlockBase, ILavaSecured
    {
        string _markup = string.Empty;

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // first ensure that sql commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                return;
            }

            // Get the SQL statement from the block content.
            string sql;
            using ( TextWriter sqlWriter = new StringWriter() )
            {
                base.OnRender( context, sqlWriter );
                sql = sqlWriter.ToString();
            }

            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;

            var sqlTimeout = (int?)null;
            if ( parms.ContainsKey( "timeout" ) )
            {
                sqlTimeout = parms["timeout"].AsIntegerOrNull();
            }

            var summary = $"[Params]=\"{_markup}\", [Sql]=\"{sql}\"";
            if ( sqlTimeout != null )
            {
                summary += $", [Timeout]={sqlTimeout}s";
            }

            using ( var activity = ObservabilityHelper.StartActivity( "Database Command", ActivityKind.Client ) )
            {
                DbCommandObservabilityCache.UpdateActivity( activity, sql, parms, p => p );

                Exception queryException = null;

                switch ( parms["statement"] )
                {
                    case "select":
                        var stopWatch = new Stopwatch();
                        DataSet results = null;

                        try
                        {
                            stopWatch.Start();
                            results = DbService.GetDataSet( sql, CommandType.Text, parms.ToDictionary( i => i.Key, i => ( object ) i.Value ), sqlTimeout );
                            stopWatch.Stop();
                        }
                        catch ( Exception ex )
                        {
                            queryException = ex;
                        }
                        finally
                        {
                            summary += $", [Elapsed]={stopWatch.Elapsed.TotalSeconds}";
                        }

                        if ( queryException != null )
                        {
                            // Throw the SQL error message as the topmost exception, and include the diagnostic detail as an inner exception for logging purposes.
                            var detailException = new Exception( "SqlBlock Render failed.\n" + summary, queryException );
                            throw new Exception( queryException.Message, detailException );
                        }

                        context.SetMergeField( parms["return"], results.Tables[0].ToDynamicTypeCollection() );

                        // Manually add query timings
                        var rockMockContext = LavaHelper.GetRockContextFromLavaContext( context );
                        rockMockContext.QueryCount++;
                        if ( rockMockContext.QueryMetricDetailLevel == QueryMetricDetailLevel.Full )
                        {
                            rockMockContext.QueryMetricDetails.Add( new QueryMetricDetail
                            {
                                Sql = sql,
                                Duration = stopWatch.ElapsedTicks,
                                Database = rockMockContext.Database.Connection.Database,
                                Server = rockMockContext.Database.Connection.DataSource
                            } );
                        }
                        break;
                    case "command":
                        var sqlParameters = new List<System.Data.SqlClient.SqlParameter>();

                        foreach ( var p in parms )
                        {
                            sqlParameters.Add( new System.Data.SqlClient.SqlParameter( p.Key, p.Value ) );
                        }

                        var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

                        // Save the orginal command timeout as we're about to change it
                        var originalCommandTimeout = rockContext.Database.CommandTimeout;

                        if ( sqlTimeout != null )
                        {
                            rockContext.Database.CommandTimeout = sqlTimeout;
                        }

                        var numOfRowsAffected = 0;
                        try
                        {
                            numOfRowsAffected = rockContext.Database.ExecuteSqlCommand( sql, sqlParameters.ToArray() );
                        }
                        catch ( Exception ex )
                        {
                            queryException = ex;
                        }
                        finally
                        {
                            // Put the command timeout back to the setting before we changed it... there is nothing to see here... move along...
                            rockContext.Database.CommandTimeout = originalCommandTimeout;
                        }
                        if ( queryException != null )
                        {
                            // Throw the SQL error message as the topmost exception, and include the diagnostic detail as an inner exception for logging purposes.
                            var detailException = new Exception( "SqlBlock Render failed.\n" + summary, queryException );
                            throw new Exception( queryException.Message, detailException );
                        }

                        context.SetMergeField( parms["return"], numOfRowsAffected );

                        break;
                    default:
                        break;
                }
            }
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            settings.AddOrIgnore( "return", "results" );
            settings.AddOrIgnore( "statement", "select" );

            return settings;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "Sql";
            }
        }

        #endregion
    }
}