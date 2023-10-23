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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.UniversalSearch;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Universal Search Lava Command
    /// </summary>
    public class SearchBlock : LavaBlockBase, ILavaSecured
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

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
            // first ensure that search commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;

            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();

            SearchType searchType = SearchType.Wildcard;

            List<int> entityIds = new List<int>();
            string query = string.Empty;

            int limit = 50;
            int offset = 0;

            if (parms.Any( p => p.Key == "query" ) )
            {
                query = parms["query"];
            }

            if ( parms.Any( p => p.Key == "limit" ) )
            {
                Int32.TryParse( parms["limit"], out limit );
            }

            if ( parms.Any( p => p.Key == "offset" ) )
            {
                Int32.TryParse( parms["offset"], out offset );
            }

            if ( parms.Any( p => p.Key == "fieldcriteria" ) )
            {
                foreach ( var queryString in parms["fieldcriteria"].ToKeyValuePairList() )
                {
                    // check that multiple values were not passed
                    var values = queryString.Value.ToString().Split( ',' );

                    foreach ( var value in values )
                    {
                        // the first letter of the field name should be lowercase
                        string fieldName = Char.ToLowerInvariant( queryString.Key[0] ) + queryString.Key.Substring( 1 );
                        fieldCriteria.FieldValues.Add( new FieldValue { Field = fieldName, Value = value } );
                    }
                }
            }

            if ( parms.Any( p => p.Key == "searchtype" ) )
            {
                switch( parms["searchtype"] )
                {
                    case "exactmatch":
                        {
                            searchType = SearchType.ExactMatch;
                            break;
                        }
                    case "fuzzy":
                        {
                            searchType = SearchType.Fuzzy;
                            break;
                        }
                    case "wildcard":
                        {
                            searchType = SearchType.Wildcard;
                            break;
                        }
                }
            }

            if ( parms.Any( p => p.Key == "criteriasearchtype" ) )
            {
                if (parms["criteriasearchtype"].ToLower() == "and" )
                {
                    fieldCriteria.SearchType = CriteriaSearchType.And;
                }
            }

            if ( parms.Any( p => p.Key == "entities" ) )
            {
                var entities = parms["entities"].Split( ',' );

                foreach(var entity in entities )
                {
                    foreach(var entityType in EntityTypeCache.All() )
                    {
                        if (entityType.FriendlyName?.ToLower() == entity )
                        {
                            entityIds.Add( entityType.Id );
                        }
                    }
                }
            }

            var client = IndexContainer.GetActiveComponent();

            if ( client == null )
            {
                throw new Exception( "Search results not available. Universal search is not enabled for this Rock instance." );
            }

            var results = client.Search( query, searchType, entityIds, fieldCriteria, limit, offset );

            context.SetMergeField( parms["iterator"], results );

            base.OnRender( context, result );
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            // Create default settings
            var settings = LavaElementAttributes.NewFromMarkup( markup, context );

            if ( settings.Count == 0 )
            {
                throw new Exception( "No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes)." );
            }

            settings.AddOrIgnore( "iterator", "results" );
            settings.AddOrIgnore( "searchtype", "wildcard" );

            return settings;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "Search";
            }
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : RockDynamic
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override bool TryGetMember( GetMemberBinder binder, out object result )
            {
                if ( _dataRow.Table.Columns.Contains( binder.Name ) )
                {
                    result = _dataRow[binder.Name];
                    return true;
                }

                result = null;
                return false;
            }
        }
    }
}