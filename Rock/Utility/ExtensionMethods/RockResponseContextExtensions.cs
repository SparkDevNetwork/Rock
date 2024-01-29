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

using Rock.Enums.Net;
using Rock.Net;
using Rock.Web;

namespace Rock.Utility.ExtensionMethods
{
    /// <summary>
    /// Extension methods for instances of <see cref="IRockResponseContext"/>.
    /// </summary>
    public static class RockResponseContextExtensions
    {
        /// <summary>
        /// Adds a JavaScript script block to the &lt;head&gt; section of the
        /// page. This will always wrap the <paramref name="script"/> content
        /// inside a &lt;script&gt; tag.
        /// </summary>
        /// <param name="context">The response context.</param>
        /// <param name="id">A unique identifier for this script to prevent duplicates.</param>
        /// <param name="script">The body of the script tag.</param>
        public static void AddScriptToHead( this IRockResponseContext context, string id, string script )
        {
            context.AddHtmlElement( id, "script", script, null, ResponseElementLocation.Header );
        }

        /// <summary>
        /// Adds a link to a JavaScript file to the &lt;head&gt; section of
        /// the page.
        /// </summary>
        /// <param name="context">The response context.</param>
        /// <param name="url">The URL to the internal or external JavaScript file. This must have any <c>~</c> characters resolved.</param>
        /// <param name="fingerprint"><c>true</c> if the URL should be modified to add fingerprinting for a local file.</param>
        public static void AddScriptLinkToHead( this IRockResponseContext context, string url, bool fingerprint )
        {
            var attributes = new Dictionary<string, string>();

            if ( fingerprint )
            {
                attributes.Add( "src", Fingerprint.Tag( url ) );
            }
            else
            {
                attributes.Add( "src", url );
            }

            context.AddHtmlElement( $"script-{url}", "script", null, attributes, ResponseElementLocation.Header );
        }

        /// <summary>
        /// Adds a JavaScript script block to the page near the bottom. This
        /// will always wrap the <paramref name="script"/> content inside a
        /// &lt;script&gt; tag.
        /// </summary>
        /// <param name="context">The response context.</param>
        /// <param name="id">A unique identifier for this script to prevent duplicates.</param>
        /// <param name="script">The body of the script tag.</param>
        public static void AddScript( this IRockResponseContext context, string id, string script )
        {
            context.AddHtmlElement( id, "script", script, null, ResponseElementLocation.Footer );
        }

        /// <summary>
        /// Adds a link to a CSS file to the &lt;head&gt; section of the page.
        /// </summary>
        /// <param name="context">The response context.</param>
        /// <param name="url">The URL to the internal or external CSS file. This must have any <c>~</c> characters resolved.</param>
        /// <param name="fingerprint"><c>true</c> if the URL should be modified to add fingerprinting for a local file.</param>
        public static void AddCssLink( this IRockResponseContext context, string url, bool fingerprint )
        {
            var attributes = new Dictionary<string, string>();

            if ( fingerprint )
            {
                attributes.Add( "href", Fingerprint.Tag( url ) );
            }
            else
            {
                attributes.Add( "href", url );
            }

            attributes.Add( "type", "text/css" );
            attributes.Add( "rel", "stylesheet" );

            context.AddHtmlElement( $"script-{url}", "link", null, attributes, ResponseElementLocation.Header );
        }

        /// <summary>
        /// Adds a &lt;meta&gt; tag to the page. If this method has already been
        /// used to add a conflicting meta tag then it will be replaced with this one.
        /// </summary>
        /// <param name="context">The response context.</param>
        /// <param name="name">The <c>name</c> attribute of the meta tag, may be null.</param>
        /// <param name="httpEquiv">The <c>http-equiv</c> attribute of the meta tag, may be null.</param>
        /// <param name="content">The <c>content</c> attribute of the meta tag.</param>
        public static void AddMetaTag( this IRockResponseContext context, string name, string httpEquiv, string content )
        {
            if ( name.IsNotNullOrWhiteSpace() && httpEquiv.IsNotNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "It is not valid to add a meta tag with both a name and http-equiv identifier." );
            }

            var attributes = new Dictionary<string, string>
            {
                ["content"] = content
            };

            if ( name.IsNotNullOrWhiteSpace() )
            {
                attributes.Add( "name", name );
            }
            else if ( httpEquiv.IsNotNullOrWhiteSpace() )
            {
                attributes.Add( "http-equiv", httpEquiv );
            }

            context.AddHtmlElement( $"meta-{name}-{httpEquiv}", "meta", null, attributes, ResponseElementLocation.Header );
        }
    }
}
