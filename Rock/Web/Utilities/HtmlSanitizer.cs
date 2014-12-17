// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HtmlAgilityPack;

namespace Rock.Web.Utilities
{
    /// <summary>
    /// Sanitation method from Rick Strahl's blog...
    /// http://weblog.west-wind.com/posts/2012/Jul/19/NET-HTML-Sanitation-for-rich-HTML-Input
    /// https://github.com/RickStrahl/HtmlSanitizer/blob/master/HtmlSanitizer/HtmlSanitizer/HtmlSanitizer.cs 
    /// </summary>
    public class HtmlSanitizer
    { 
        /// <summary>
        /// The black list
        /// </summary>
        public HashSet<string> BlackList = new HashSet<string>() 
        {
                { "script" },
                { "iframe" },
                { "form" },
                { "object" },
                { "embed" },
                { "link" },                
                { "head" },
                { "meta" }
        };

        /// <summary>
        /// Cleans up an HTML string and removes HTML tags in blacklist
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="blackList">The black list.</param>
        /// <returns></returns>
        public static string SanitizeHtml( string html, params string[] blackList )
        {
            var sanitizer = new HtmlSanitizer();
            
            if ( blackList != null && blackList.Length > 0 )
            {
                sanitizer.BlackList.Clear();
                foreach ( string item in blackList )
                    sanitizer.BlackList.Add( item );
            }

            return sanitizer.Sanitize( html );
        }

        /// <summary>
        /// Cleans up an HTML string by removing elements
        /// on the blacklist and all elements that start
        /// with onXXX .
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string Sanitize( string html )
        {
            var doc = new HtmlDocument();

            doc.LoadHtml( html.Replace( "&nbsp;", "<nbsp />" ).Replace( "&#39;", "<apos />" ).Replace("&quot;", "<quote />") );

            SanitizeHtmlNode( doc.DocumentNode );

            //return doc.DocumentNode.WriteTo();

            string output = null;

            // Use an XmlTextWriter to create self-closing tags
            using ( StringWriter sw = new StringWriter() )
            {
                XmlWriter writer = new XmlTextWriter( sw );
                doc.DocumentNode.WriteTo( writer );
                output = sw.ToString();

                // strip off XML doc header
                if ( !string.IsNullOrEmpty( output ) )
                {
                    int at = output.IndexOf( "?>" );
                    output = output.Substring( at + 2 );
                }

                writer.Close();
            }
            doc = null;

            return output.Replace( "<nbsp />", "&nbsp;" ).Replace( "<apos />", "&#39;" ).Replace( "<quote />", "&quot;" );
        }

        private void SanitizeHtmlNode( HtmlNode node )
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                
            }

            if ( node.NodeType == HtmlNodeType.Element )
            {
                // check for blacklist items and remove
                if ( BlackList.Contains( node.Name ) )
                {
                    node.Remove();
                    return;
                }

                // remove CSS Expressions and embedded script links
                if ( node.Name == "style" )
                {
                    var val = node.InnerHtml;
                    if ( string.IsNullOrEmpty( node.InnerText ) )
                    {
                        if ( HasExpressionLinks( val ) || HasScriptLinks( val ) )
                            node.ParentNode.RemoveChild( node );
                    }
                }

                // remove script attributes
                if ( node.HasAttributes )
                {
                    for ( int i = node.Attributes.Count - 1; i >= 0; i-- )
                    {
                        HtmlAttribute currentAttribute = node.Attributes[i];

                        var attr = currentAttribute.Name.ToLower();
                        var val = currentAttribute.Value.ToLower();

                        // remove event handlers
                        if ( attr.StartsWith( "on" ) )
                            node.Attributes.Remove( currentAttribute );

                        // Remove CSS Expressions
                        else if ( attr == "style" &&
                                 val != null &&
                                 HasExpressionLinks( val ) || HasScriptLinks( val ) )
                            node.Attributes.Remove( currentAttribute );

                        // remove script links from all attributes
                        else if (
                            //(attr == "href" || attr== "src" || attr == "dynsrc" || attr == "lowsrc") &&
                                 val != null &&
                                 HasScriptLinks( val ) )
                            node.Attributes.Remove( currentAttribute );
                    }
                }
            }

            // Look through child nodes recursively
            if ( node.HasChildNodes )
            {
                for ( int i = node.ChildNodes.Count - 1; i >= 0; i-- )
                {
                    SanitizeHtmlNode( node.ChildNodes[i] );
                }
            }
        }

        private bool HasScriptLinks( string value )
        {
            return value.Contains( "javascript:" ) || value.Contains( "vbscript:" );
        }

        private bool HasExpressionLinks( string value )
        {
            return value.Contains( "expression" );
        }
    }
}