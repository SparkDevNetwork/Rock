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
using System.Text.RegularExpressions;

namespace Rock.Lava
{
    /// <summary>
    /// Converts a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords with Liquid-compatible substitutes.
    /// </summary>
    public class LavaToLiquidTemplateConverter
    {
        #region Constructors

        static LavaToLiquidTemplateConverter()
        {
            // Initialize the compiled regular expressions that will be used by all instances of the converter.
            InitializeLavaCommentsRegex();
        }

        #endregion

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Lava-specific syntax and keywords.
        /// </summary>
        /// <param name="lavaTemplateText"></param>
        /// <returns></returns>
        public string ConvertToLiquid( string lavaTemplateText )
        {
            string liquidTemplateText;

            liquidTemplateText = RemoveLavaComments( lavaTemplateText );
            liquidTemplateText = ReplaceElseIfKeyword( liquidTemplateText );

            return liquidTemplateText;
        }

        internal static readonly Regex FullShortCodeToken = new Regex( @"{\[\s*(\w+)\s*(.*?)?\]}", RegexOptions.Compiled );

        public string ReplaceTemplateShortcodes( string inputTemplate )
        {
            /* The Lava shortcode syntax is not recognized as a document element by Fluid, and at present there is no way to intercept or replace the Fluid parser.
             * As a workaround, we pre-process the template to replace the Lava shortcode token "{[ ]}" with the Liquid tag token "{% %}" and add a prefix to avoid naming collisions with existing standard tags.
             * The shortcode can then be processed as a regular custom block by the Fluid templating engine.
             * As a future improvement, we could look at submitting a pull request to the Fluid project to add support for custom parsers.
             */
            var newBlockName = "{% $1<suffix> $2 %}".Replace( "<suffix>", Constants.ShortcodeInternalNameSuffix );

            inputTemplate = FullShortCodeToken.Replace( inputTemplate, newBlockName );

            return inputTemplate;
        }

        /// <summary>
        /// Convert a Lava template to a Liquid-compatible template by replacing Rock Lava-specific "elseif" keyword.
        /// </summary>
        /// <remarks>
        /// The "elseif" is not a recognized keyword in Liquid - it implements the less natural variant "elsif".
        ///  This keyword forms part of a conditional construct (if/then/else) that is processed internally by the Liquid engine,
        ///  so the most portable method of processing this alternative is to replace it with the recognized Liquid keyword.    
        /// </remarks>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        /// <param name="inputTemplate">A Lava template string</param>
        /// <returns>the template string without the "elseif" keyword</returns>
        public string ReplaceElseIfKeyword( string inputTemplate )
        {
            /*
                11/17/2025 - N.A.

                The original Regex-based approach used this Regex, but it fails to work correly inside {% lava/liquid %} style
                lava:

                    internal static readonly Regex ElseIfToken = new Regex( @"{\%(.*?\s?)elseif(\s?.*?)\%}", RegexOptions.Compiled );
                    inputTemplate = ElseIfToken.Replace( inputTemplate, "{%$1elsif$2%}" );

                We considered updating to this other Regex, but testing showed it to be significantly less efficient. It
                was approximately twice as slow and used nearly double the memory compared to a simple string replacement.
                
                    internal static readonly Regex ElseIfToken = new Regex( @"\{\%(.*?)\belseif\b(.*?)\%\}", RegexOptions.Compiled | RegexOptions.Singleline );

                Reason: Chose performance-optimized simple string replacement over Regex to reduce processing time and memory usage.
                        See https://app.asana.com/1/20866866924293/project/1208321217019996/task/1211949287939339
            */

            return inputTemplate?.Replace( "elseif", "elsif" );
        }

        #region Lava Comments

        private static string LavaTokenBlockCommentStart = @"/-";
        private static string LavaTokenBlockCommentEnd = @"-/";
        private static string LavaTokenLineComment = @"//-";

        private static Regex _lavaCommentMatchGroupsRegex = null;

        /// <summary>
        /// Build the regular expression that will be used to remove Lava-style comments from the template.
        /// </summary>
        private static void InitializeLavaCommentsRegex()
        {
            const string stringElement = @"(('|"")[^'""]*('|""))+";

            string lineCommentElement = LavaTokenLineComment + @"(.*?)\r?(\n|$)";

            var blockCommentElement = Regex.Escape( LavaTokenBlockCommentStart ) + @"(.*?)" + Regex.Escape( LavaTokenBlockCommentEnd );

            var rawBlock = @"\{%\sraw\s%\}(.*?)\{%\sendraw\s%\}";

            var templateElementMatchGroups = rawBlock + "|" + blockCommentElement + "|" + lineCommentElement + "|" + stringElement;

            // Create and compile the Regex, because it will be used very frequently.
            _lavaCommentMatchGroupsRegex = new Regex( templateElementMatchGroups, RegexOptions.Compiled | RegexOptions.Singleline );
        }

        /// <summary>
        /// Remove Lava-style comments from a Lava template.
        /// Lava comments provide a shorthand alternative to the Liquid {% comment %}{% endcomment %} block,
        /// and can can be in one of the following forms:
        /// 
        /// /- This Lava block comment style...
        ///    ... can span multiple lines -/
        ///
        /// //- This Lava line comment style can be appended to any single line.
        /// 
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <returns></returns>
        public string RemoveLavaComments( string lavaTemplate )
        {
            // Remove comments from the content.
            var lavaWithoutComments = _lavaCommentMatchGroupsRegex.Replace( lavaTemplate,
                me =>
                {
                    // If the match group is a line comment, retain the end-of-line marker.
                    if ( me.Value.StartsWith( LavaTokenBlockCommentStart ) || me.Value.StartsWith( LavaTokenLineComment ) )
                    {
                        return me.Value.StartsWith( LavaTokenLineComment ) ? Environment.NewLine : string.Empty;
                    }

                    // Keep the literal strings
                    return me.Value;
                } );

            return lavaWithoutComments;
        }

        #endregion
    }
}