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

            liquidTemplateText = ReplaceTemplateShortcodes( liquidTemplateText );
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

        internal static readonly Regex ElseIfToken = new Regex( @"{\%(.*?\s?)elseif(\s?.*?)\%}", RegexOptions.Compiled );

        public string ReplaceElseIfKeyword( string inputTemplate )
        {
            // "elseif" is not a recognized keyword in Liquid - it implements the less natural variant "elsif".
            // This keyword forms part of a conditional construct (if/then/else) that is processed internally by the Liquid engine,
            // so the most portable method of processing this alternative is to replace it with the recognized Liquid keyword.            
            inputTemplate = ElseIfToken.Replace( inputTemplate, "{%$1elsif$2%}" );

            return inputTemplate;
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

            string lineCommentElement = LavaTokenLineComment + @"(.*?)\r?\n";

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