﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DotLiquid;
using DotLiquid.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Rock.Data;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class WebRequest : RockLavaBlockBase
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
            // first ensure that entity commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( "The Lava command '{0}' is not configured for this template.", this.Name ) );
                base.Render( context, result );
                return;
            }

            var parms = ParseMarkup( _markup, context );

            if ( !string.IsNullOrWhiteSpace( parms["url"] ) )
            {
                dynamic responseData = null;

                try {
                    var client = new RestClient( parms["url"].ToString() );

                    var request = new RestRequest( parms["method"].ToUpper().ConvertToEnum<Method>( Method.GET ) );
                    client.Timeout = 12000;

                    // handle basic auth
                    if ( !string.IsNullOrWhiteSpace( parms["basicauth"] ) )
                    {
                        string[] authParts = parms["basicauth"].Split( ',' );
                        if ( authParts.Length == 2 )
                        {
                            client.Authenticator = new HttpBasicAuthenticator( authParts[0], authParts[1] );
                        }
                    }

                    // add query string parms
                    if ( !string.IsNullOrWhiteSpace( parms["parameters"] ) ) {
                        foreach ( var queryString in parms["parameters"].ToKeyValuePairList() )
                        {
                            request.AddParameter( queryString.Key, queryString.Value );
                        }
                    }

                    // add headers
                    if ( !string.IsNullOrWhiteSpace( parms["headers"] ) )
                    {
                        foreach ( var header in parms["headers"].ToKeyValuePairList() )
                        {
                            request.AddHeader( header.Key, header.Value.ToString() );
                        }
                    }

                    // add body, this will be ignored if other parameters exist
                    if ( !string.IsNullOrWhiteSpace( parms["body"] ) )
                    {
                        request.AddParameter( parms["requestcontenttype"], parms["body"], ParameterType.RequestBody );
                    }

                    IRestResponse response = client.Execute( request );
                    var content = response.Content;

                    var contentType = parms["responsecontenttype"].ToLower();

                    if ( contentType == "xml" )
                    {
                        responseData = new ExpandoObject();
                        var doc = XDocument.Parse( response.Content );
                        ExpandoObjectHelper.Parse( responseData, doc.Root );
                    }
                    else if (contentType == "json" )
                    {
                        var converter = new ExpandoObjectConverter();

                        // determine if the return type is an array or not
                        if ( content.Trim().Substring( 0, 1 ) == "[" )
                        {
                            responseData = JsonConvert.DeserializeObject<List<ExpandoObject>>( content, converter ); // array
                        }
                        else
                        {
                            responseData = JsonConvert.DeserializeObject<ExpandoObject>( content, converter ); // not an array
                        }
                    }
                    else // otherwise assume html and just throw the contents out to the screen
                    {
                        responseData = content;
                    }

                    context.Scopes.Last()[parms["return"]] = responseData;
                } catch(Exception ex )
                {
                    result.Write( string.Format("An error occurred: {0}", ex.Message ) );
                }

                context.Scopes.Last()[parms["return"]] = responseData;
            }
            else {
                result.Write( "No url parameter was found." );
            }
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
            if ( context.Scopes.Count > 0 )
            {
                foreach ( var item in context.Scopes[0] )
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
            parms.Add( "method", "GET" );
            parms.Add( "return", "results" );
            parms.Add( "basicauth", "" );
            parms.Add( "parameters", "" );
            parms.Add( "headers", "" );
            parms.Add( "responsecontenttype", "json" );
            parms.Add( "body", "" );
            parms.Add( "requesttype", "text/plain" );

            var markupItems = Regex.Matches( resolvedMarkup, "(.*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

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
        /// Helper class to turn XML in an expando
        /// </summary>
        private class ExpandoObjectHelper
        {
            private static List<string> KnownLists;
            public static void Parse( dynamic parent, XElement node, List<string> knownLists = null )
            {
                if ( knownLists != null )
                {
                    KnownLists = knownLists;
                }
                IEnumerable<XElement> sorted = from XElement elt in node.Elements() orderby node.Elements( elt.Name.LocalName ).Count() descending select elt;

                if ( node.HasElements )
                {
                    int nodeCount = node.Elements( sorted.First().Name.LocalName ).Count();
                    bool foundNode = false;
                    if ( KnownLists != null && KnownLists.Count > 0 )
                    {
                        foundNode = (from XElement el in node.Elements() where KnownLists.Contains( el.Name.LocalName ) select el).Count() > 0;
                    }

                    if ( nodeCount > 1 || foundNode == true )
                    {
                        // At least one of the child elements is a list
                        var item = new ExpandoObject();
                        List<dynamic> list = null;
                        string elementName = string.Empty;
                        foreach ( var element in sorted )
                        {
                            if ( element.Name.LocalName != elementName )
                            {
                                list = new List<dynamic>();
                                elementName = element.Name.LocalName;
                            }

                            if ( element.HasElements ||
                                (KnownLists != null && KnownLists.Contains( element.Name.LocalName )) )
                            {
                                Parse( list, element );
                                AddProperty( item, element.Name.LocalName, list );
                            }
                            else
                            {
                                Parse( item, element );
                            }
                        }

                        foreach ( var attribute in node.Attributes() )
                        {
                            AddProperty( item, attribute.Name.ToString(), attribute.Value.Trim() );
                        }

                        AddProperty( parent, node.Name.ToString(), item );
                    }
                    else
                    {
                        var item = new ExpandoObject();

                        foreach ( var attribute in node.Attributes() )
                        {
                            AddProperty( item, attribute.Name.ToString(), attribute.Value.Trim() );
                        }

                        //element
                        foreach ( var element in sorted )
                        {
                            Parse( item, element );
                        }
                        AddProperty( parent, node.Name.ToString(), item );
                    }
                }
                else
                {
                    if ( node.HasAttributes )
                    {
                        var item = new ExpandoObject();
                        foreach ( var attribute in node.Attributes() )
                        {
                            AddProperty( item, attribute.Name.ToString(), attribute.Value.Trim() );
                        }

                        if ( node.Value != null && !string.IsNullOrWhiteSpace( node.Value ) )
                        {
                            AddProperty( item, "Value", node.Value.Trim() );
                        }

                        AddProperty( parent, node.Name.ToString(), item );
                    }
                    else
                    {
                        AddProperty( parent, node.Name.ToString(), node.Value.Trim() );
                    }

                }
            }

            private static void AddProperty( dynamic parent, string name, object value )
            {
                if ( parent is List<dynamic> )
                {
                    (parent as List<dynamic>).Add( value );
                }
                else
                {
                    (parent as IDictionary<string, object>)[name] = value;
                }
            }
        }
    }
}