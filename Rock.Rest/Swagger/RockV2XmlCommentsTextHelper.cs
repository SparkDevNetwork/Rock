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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Helper class to convert the XML comments to a format that renders
    /// decently in Swagger UI since it expects Markdown.
    /// </summary>
    /// <remarks>
    /// Taken from MIT licensed <see href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b6c528c0b7c1315263cf78565c30afe1e2935f65/src/Swashbuckle.AspNetCore.SwaggerGen/XmlComments/XmlCommentsTextHelper.cs"/>.
    /// </remarks>
    internal static class RockV2XmlCommentsTextHelper
    {
        public static string Humanize( string text )
        {
            if ( text == null )
                throw new ArgumentNullException( "text" );

            //Call DecodeXml at last to avoid entities like &lt and &gt to break valid xml

            text = NormalizeIndentation( text );
            text = HumanizeRefTags( text );
            text = HumanizeHrefTags( text );
            text = HumanizeCodeTags( text );
            text = HumanizeMultilineCodeTags( text );
            text = HumanizeParaTags( text );
            text = HumanizeBrTags( text ); // must be called after HumanizeParaTags() so that it replaces any additional <br> tags
            text = text.Trim( new[] { '\r', '\n' } );
            text = DecodeXml( text );

            return text;
        }

        private static string NormalizeIndentation( string text )
        {
            string[] lines = text.Split( '\n' );
            string padding = GetCommonLeadingWhitespace( lines );

            int padLen = padding == null ? 0 : padding.Length;

            // remove leading padding from each line
            for ( int i = 0, l = lines.Length; i < l; ++i )
            {
                string line = lines[i].TrimEnd( '\r' ); // remove trailing '\r'

                if ( padLen != 0 && line.Length >= padLen && line.Substring( 0, padLen ) == padding )
                {
                    line = line.Substring( padLen );
                }

                lines[i] = line;
            }

            // remove leading empty lines, but not all leading padding
            // remove all trailing whitespace, regardless
            return string.Join( "\r\n", lines.SkipWhile( x => string.IsNullOrWhiteSpace( x ) ) ).TrimEnd();
        }

        private static string GetCommonLeadingWhitespace( string[] lines )
        {
            if ( null == lines )
            {
                throw new ArgumentException( "lines" );
            }

            if ( lines.Length == 0 )
            {
                return null;
            }

            string[] nonEmptyLines = lines
                .Where( x => !string.IsNullOrWhiteSpace( x ) )
                .ToArray();

            if ( nonEmptyLines.Length < 1 )
            {
                return null;
            }

            int padLen = 0;

            // use the first line as a seed, and see what is shared over all nonEmptyLines
            string seed = nonEmptyLines[0];
            for ( int i = 0, l = seed.Length; i < l; ++i )
            {
                if ( !char.IsWhiteSpace( seed, i ) )
                {
                    break;
                }

                if ( nonEmptyLines.Any( line => line[i] != seed[i] ) )
                {
                    break;
                }

                ++padLen;
            }

            if ( padLen > 0 )
            {
                return seed.Substring( 0, padLen );
            }

            return null;
        }

        private static string HumanizeRefTags( string text )
        {
            return _refTag.Replace( text, ( match ) => match.Groups["display"].Value );
        }

        private static string HumanizeHrefTags( string text )
        {
            return _hrefTag.Replace( text, m => $"[{m.Groups[2].Value}]({m.Groups[1].Value})" );
        }

        private static string HumanizeCodeTags( string text )
        {
            return _codeTag.Replace( text, ( match ) => "`" + match.Groups["display"].Value + "`" );
        }

        private static string HumanizeMultilineCodeTags( string text )
        {
            return _multilineCodeTag.Replace( text, ( match ) => "```" + match.Groups["display"].Value + "```" );
        }

        private static string HumanizeParaTags( string text )
        {
            return _paraTag.Replace( text, ( match ) =>
            {
                var displayText = match.Groups["display"].Value;

                displayText = NormalizeIndentation( displayText );
                displayText = _newlineWhitespace.Replace( displayText, " " );

                return "<br>" + displayText;
            } );
        }

        private static string HumanizeBrTags( string text )
        {
            return _brTag.Replace( text, m => Environment.NewLine );
        }

        private static string DecodeXml( string text )
        {
            return WebUtility.HtmlDecode( text );
        }

        private const string RefTagPattern = @"<(see|paramref) (name|cref|langword)=""([TPF]{1}:)?(?<display>.+?)"" ?/>";
        private const string CodeTagPattern = @"<c>(?<display>.+?)</c>";
        private const string MultilineCodeTagPattern = @"<code>(?<display>.+?)</code>";
        private const string ParaTagPattern = @"<para>(?<display>.+?)</para>";
        private const string HrefPattern = @"<see href=\""(.*)\"">(.*)<\/see>";
        private const string BrPattern = @"(<br ?\/?>)"; // handles <br>, <br/>, <br />
        private const string NewlineWhitespacePattern = @"[\r\n]+\s*";

        private static readonly Regex _refTag = new Regex( RefTagPattern, RegexOptions.Compiled );
        private static readonly Regex _codeTag = new Regex( CodeTagPattern, RegexOptions.Compiled );
        private static readonly Regex _multilineCodeTag = new Regex( MultilineCodeTagPattern, RegexOptions.Singleline | RegexOptions.Compiled );
        private static readonly Regex _paraTag = new Regex( ParaTagPattern, RegexOptions.Singleline | RegexOptions.Compiled );
        private static readonly Regex _hrefTag = new Regex( HrefPattern, RegexOptions.Compiled );
        private static readonly Regex _brTag = new Regex( BrPattern, RegexOptions.Compiled );
        private static readonly Regex _newlineWhitespace = new Regex( NewlineWhitespacePattern, RegexOptions.Compiled );
    }
}
