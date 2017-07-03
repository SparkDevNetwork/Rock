using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.Util;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Model;
using System;
using Rock.Lava.Blocks;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicShortcodeBlock : RockLavaShortcodeBlockBase
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        string _markup = string.Empty;
        string _tagName = string.Empty;
        StringBuilder _blockMarkup = new StringBuilder();

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            // get all the block dynamic shortcodes and register them
            var blockShortCodes = LavaShortcodeCache.All( false ).Where( s => s.TagType == TagType.Block );

            foreach(var shortcode in blockShortCodes )
            {
                // register this shortcode
                Template.RegisterShortcode<DynamicShortcodeBlock>( shortcode.TagName );
            }
        }

        protected override void Parse( List<string> tokens )
        {
            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            var endTag = $@"{{\[\s*end{ _tagName }\s*\]}}";
            Regex rgx = new Regex( endTag );

            NodeList = NodeList ?? new List<object>();
            NodeList.Clear();

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {
                Match endTagMatch = rgx.Match( token );
                if ( endTagMatch.Success )
                {
                    return;
                }
                else
                {
                    _blockMarkup.Append(token);
                }
            }
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
            _tagName = tagName;
            
            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            var shortcode = LavaShortcodeCache.Read( _tagName );

            if ( shortcode != null )
            {
                var parms = ParseMarkup( _markup, context );

                var lavaTemplate = shortcode.Markup;
                var blockMarkup = _blockMarkup.ToString();

                // merge the block markup in
                Regex rgx = new Regex( @"{{\s*blockContent\s*}}" );
                lavaTemplate = rgx.Replace( lavaTemplate, blockMarkup );

                // next ensure they did not use any entity commands in the block that are not allowed
                // this is needed as the shortcode it configured to allow entities for processing that
                // might allow more entities than the source block, template, action, etc allows
                var enabledCommands = "";
                if ( context.Registers.ContainsKey( "EnabledCommands" ) )
                {
                    enabledCommands = context.Registers["EnabledCommands"].ToString();
                }

                var securityCheck = blockMarkup.ResolveMergeFields(new Dictionary<string, object>(), enabledCommands);

                Regex securityPattern = new Regex( String.Format(RockLavaBlockBase.NotAuthorizedMessage, ".*" ) );
                Match securityMatch = securityPattern.Match( securityCheck );

                if ( securityMatch.Success )
                {
                    result.Write( securityMatch.Value ); // return security error message
                }
                else
                {
                    if ( shortcode.EnabledLavaCommands.IsNotNullOrWhitespace() )
                    {
                        enabledCommands = shortcode.EnabledLavaCommands;
                    }

                    var results = lavaTemplate.ResolveMergeFields( parms, enabledCommands );
                    result.Write( results );
                    base.Render( context, result );
                }
            }
            else
            {
                result.Write( $"An error occurred while processing the {0} shortcode.", _tagName );
            }
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="markup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No parameters were found in your command. The syntax for a parameter is parmName:'' (note that you must use single quotes).</exception>
        private Dictionary<string, object> ParseMarkup( string markup, Context context )
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

            var parms = new Dictionary<string, object>();

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