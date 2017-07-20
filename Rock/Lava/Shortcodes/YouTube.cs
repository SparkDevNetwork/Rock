using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.Util;
using Rock.Data;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// 
    /// </summary>
    public class YouTube : RockLavaShortcodeBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterShortcode<YouTube>( "youtube" );
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
            Uri playerUrl;


            using ( TextWriter writer = new StringWriter() )
            {
                var playerId = Guid.NewGuid().ToString().ToLower(); 

                base.Render( context, writer );

                var parms = ParseMarkup( _markup, context );

                StringBuilder markup = new StringBuilder();

                if ( parms.Any( p => p.Key == "url" ) ) {
                    
                    if ( !Uri.TryCreate( parms["url"], UriKind.Absolute, out playerUrl ) )
                    {
                        result.Write( "Invalid YouTube URL provided." );
                        return;
                    }
                }
                else
                {
                    result.Write( "A YouTube URL must be provided using the 'url' parmameter." );
                    return;
                }

                Page page = HttpContext.Current.Handler as Page;
                ScriptManager.RegisterClientScriptInclude( page, page.GetType(), "youtube-playerapi", "https://www.youtube.com/player_api" );

                markup.Append( $"<div id='ytplayer-{ playerId }'></div>" );

                markup.Append( $@"
                    <script>

                    //var tag = document.createElement('script');
                    //tag.src = 'https://www.youtube.com/player_api';
                    //var firstScriptTag = document.getElementsByTagName( 'script' )[0];
                    //firstScriptTag.parentNode.insertBefore( tag, firstScriptTag );

                    var player{ ( playerId.Replace("-", "") ) };
                    function onYouTubePlayerAPIReady() {{
                        player{ (playerId.Replace( "-", "" )) } = new YT.Player( 'ytplayer-{ playerId }', {{
                                    height: '360',
                                    width: '640',
                                    videoId: '{ playerUrl.LocalPath.TrimStart('/') }'
                            }});
                        }}
                    </script> " );
                // http://stackoverflow.com/questions/17012886/loading-multiple-video-players-with-youtube-api
                result.Write( writer.ToString() + " " + markup.ToString() );
            }
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
            parms.Add( "return", "results" );
            parms.Add( "statement", "select" );

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
    }
}