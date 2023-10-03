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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using RestSharp;
using RestSharp.Authenticators;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Web
    /// </summary>
    public class WebRequestBlock : LavaBlockBase, ILavaSecured
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
            // first ensure that entity commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            var settings = GetAttributesFromMarkup( _markup, context );
            var parms = settings.Attributes;

            if ( !string.IsNullOrWhiteSpace( parms["url"] ) )
            {
                dynamic responseData = null;

                try {
                    var client = new RestClient( parms["url"].ToString() );

                    var request = new RestRequest( parms["method"].ToUpper().ConvertToEnum<Method>( Method.GET ) );
                    client.Timeout = parms["timeout"].AsInteger();

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
                        if ( parms.ContainsKey( "requestcontenttype" ) )
                        {
                            request.AddParameter( parms["requestcontenttype"], parms["body"], ParameterType.RequestBody );
                        }
                        else
                        {
                            result.Write( "When using the 'body' parameter you must also provide a 'requestcontenttype' also." );
                            base.OnRender( context,  result );
                            return ;
                        }
                    }

                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                    IRestResponse response = client.Execute( request );

                    if ( response.StatusCode == System.Net.HttpStatusCode.OK )
                    {

                        var content = response.Content;

                        var contentType = parms["responsecontenttype"].ToLower();

                        if ( contentType == "xml" )
                        {
                            responseData = new ExpandoObject();
                            var doc = XDocument.Parse( response.Content );
                            ExpandoObjectHelper.Parse( responseData, doc.Root );
                        }
                        else if ( contentType == "json" )
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
                    }
                    else if ( response.ErrorException != null )
                    {
                        responseData = $"Error: {response.ErrorMessage}";
                    }
                    else
                    {
                        responseData = $"{response.StatusCode}: {response.Content}";
                    }
                } catch
                {
                    throw;
                }

                context.SetMergeField( parms["return"], responseData );
            }
            else {
                result.Write( "No url parameter was found." );
            }
            base.OnRender( context, result );
        }

        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var attributes = LavaElementAttributes.NewFromMarkup( markup, context );
            attributes.AddOrIgnore( "method", "GET" );
            attributes.AddOrIgnore( "return", "results" );
            attributes.AddOrIgnore( "basicauth", "" );
            attributes.AddOrIgnore( "parameters", "" );
            attributes.AddOrIgnore( "headers", "" );
            attributes.AddOrIgnore( "responsecontenttype", "json" );
            attributes.AddOrIgnore( "body", "" );
            attributes.AddOrIgnore( "requesttype", "text/plain" );
            attributes.AddOrIgnore( "timeout", "12000" );

            return attributes;
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "WebRequest";
            }
        }

        #endregion

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