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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotLiquid;

using Rock.Utility;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// Lava shortcode for displaying scripture links
    /// </summary>
    [LavaShortcodeMetadata(
        "Scripturize ",
        "scripturize",
        "Scripturize reads through text finding scripture references and converts them into links to popular Bible websites.",
        @"<p>Many blog posts and articles contain references to scriptures. Using this
            shortcode you can easily convert those references to links to popular Bible
            websites. Let's take a look at how simple this shortcode is to use.
         </p>
         <pre>{[ scripturize defaulttranslation:'NLT' landingsite:'YouVersion' cssclass:'scripture' ]}
{{ item.Title }}

{{ item | Attribute:'Summary' }}

{{ item.Content }}
{[ endscripturize ]}</pre>

        <p>It's that easy! The format of the reference can take many forms. Below are a few examples.</p>

        <ul>
            <li>John 3:16</li>
            <li>Jn 3:16</li>
            <li>Jn 3:16 (NIV)</li>
            <li>1 Peter 1:1-10</li>
        </ul>

        <p>Let's take a look at some of the parameters and options that are available.</p>

        <ul>
            <li><strong>defaulttranslation</strong> (NLT) - Scripture references that do not provide a translation will use the default value you assign. A listing of supported translations can be found below.</li>
            <li><strong>landingsite</strong> (YouVersion) - This is the landing site that you want the links to refer to. Valid values are 'YouVersion' and 'BibleGateway'.</li>
            <li><strong>cssclass</strong> - The optional CSS class you would like to have added to the anchor tag.</li>
        </ul>

        <ul>
            <li>AMP</li><li>ASV</li><li>CEB</li><li>CEV</li><li>CEVUS06</li><li>CPDV</li><li>DARBY</li><li>DRA</li><li>ESV</li><li>GNBDC</li><li>GWT</li><li>GNB</li><li>GNT</li><li>HCSB</li><li>KJV</li><li>MSG</li><li>NASB</li><li>NCV</li><li>NIV</li><li>NET</li><li>NIRV</li><li>NKJV</li><li>NLT</li><li>OJB</li><li>RSV</li><li>TLV</li><li>WEB</li>
        </ul>",
        "defaulttranslation,landingsite,cssclass",
        "" )]
    public class Scripturize : RockLavaShortcodeBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterShortcode<Scripturize>( "scripturize" );
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

            using ( TextWriter writer = new StringWriter() )
            {
                base.Render( context, writer );

                var parms = ParseMarkup( _markup, context );

                LandingSite? landingSite = (LandingSite)Enum.Parse( typeof( LandingSite ), parms["landingsite"], true );

                if ( landingSite == null )
                {
                    result.Write( "<!-- the landing site provided to the scripturize shortcode was not correct -->" + writer.ToString() );
                    return;
                }

                result.Write( Rock.Utility.Scripturize.Parse( writer.ToString(), parms["defaulttranslation"], landingSite.Value, parms["cssclass"] ) );
            }
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
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
            parms.Add( "defaulttranslation", "NLT" );
            parms.Add( "cssclass", "" );
            parms.Add( "landingsite", "YouVersion" );

            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
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
    }
}