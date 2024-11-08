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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotLiquid;

using Rock.Data;
using Rock.Lava.Blocks;
using Rock.Lava.DotLiquid;
using Rock.Observability;
using Rock.Web.Cache.NonEntities;

namespace Rock.Lava.RockLiquid.Blocks
{
    /// <summary>
    /// Sql stores the result of provided SQL query into a variable.
    ///
    /// {% sql results %}
    /// SELECT [FirstName], [LastName] FROM [Person]
    /// {% endsql %}
    /// </summary>
    public class Sql : RockLavaBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<Sql>( "sql" );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            // first ensure that sql commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( RockLavaBlockBase.NotAuthorizedMessage, this.Name ) );
                return;
            }

            using ( TextWriter sql = new StringWriter() )
            {
                base.Render( context, sql );

                var settings = SqlBlock.GetAttributesFromMarkup( _markup, new RockLiquidRenderContext( context ) );
                var parms = settings.Attributes;

                var sqlTimeout = ( int? ) null;
                if ( parms.ContainsKey( "timeout" ) )
                {
                    sqlTimeout = parms["timeout"].AsIntegerOrNull();
                }

                var sqlText = sql.ToString();

                using ( var activity = ObservabilityHelper.StartActivity( "Database Command", ActivityKind.Client ) )
                {
                    DbCommandObservabilityCache.UpdateActivity( activity, sqlText, parms, p => p );

                    switch ( parms["statement"] )
                    {
                        case "select":
                            var results = DbService.GetDataSet( sqlText, CommandType.Text, parms.ToDictionary( i => i.Key, i => ( object ) i.Value ), sqlTimeout );

                            context.Scopes.Last()[parms["return"]] = results.Tables[0].ToDynamicTypeCollection();
                            break;
                        case "command":
                            var sqlParameters = new List<System.Data.SqlClient.SqlParameter>();

                            foreach ( var p in parms )
                            {
                                sqlParameters.Add( new System.Data.SqlClient.SqlParameter( p.Key, p.Value ) );
                            }

                            using ( var rockContext = new RockContext() )
                            {
                                if ( sqlTimeout != null )
                                {
                                    rockContext.Database.CommandTimeout = sqlTimeout;
                                }
                                int numOfRowEffected = rockContext.Database.ExecuteSqlCommand( sqlText, sqlParameters.ToArray() );

                                context.Scopes.Last()[parms["return"]] = numOfRowEffected;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}