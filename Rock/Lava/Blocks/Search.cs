﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{

    /// <summary>
    /// Universal Search Lava Command
    /// </summary>
    public class Search : RockLavaBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<Search>( "search" );
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
            // first ensure that search commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( "The Lava command '{0}' is not configured for this template.", this.Name ) );
                base.Render( context, result );
                return;
            }

            var parms = ParseMarkup( _markup, context );

            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();

            SearchType searchType = SearchType.Wildcard;

            List<int> entityIds = new List<int>();
            string query = string.Empty;

            if (parms.Any( p => p.Key == "query" ) )
            {
                query = parms["query"];
            }

            if ( parms.Any( p => p.Key == "fieldcriteria" ) )
            {
                foreach ( var queryString in parms["fieldcriteria"].ToKeyValuePairList() )
                {
                    // check that multiple values were not passed
                    var values = queryString.Value.ToString().Split( ',' );

                    foreach ( var value in values )
                    {

                        fieldCriteria.FieldValues.Add( new FieldValue { Field = queryString.Key, Value = value } );
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
                        if (entityType.FriendlyName.ToLower() == entity )
                        {
                            entityIds.Add( entityType.Id );
                        }
                    }
                }
            }

            var client = IndexContainer.GetActiveComponent();
            var results = client.Search( query, searchType, entityIds, fieldCriteria );

            context.Scopes.Last()[parms["iterator"]] = results;

            base.Render( context, result );
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes).</exception>
        private Dictionary<string, string> ParseMarkup( string markup, Context context )
        {
            // first run lava across the inputted markup
            var internalMergeFields = new Dictionary<string, object>();

            // get variables defined in the lava source
            foreach ( var scope in context.Scopes )
            {
                foreach ( var item in scope )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }

            // get merge fields loaded by the block or container
            if ( context.Environments.Count > 0 )
            {
                foreach ( var item in context.Environments[0] )
                {
                    internalMergeFields.AddOrReplace( item.Key, item.Value );
                }
            }
            var resolvedMarkup = markup.ResolveMergeFields( internalMergeFields );

            var parms = new Dictionary<string, string>();
            parms.Add( "iterator", "results" );

            var markupItems = Regex.Matches( resolvedMarkup, "(.*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            if ( markupItems.Count == 0 )
            {
                throw new Exception( "No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes)." );
            }

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parms.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }
            return parms;
        }


        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }

                return null;
            }
        }
    }
}