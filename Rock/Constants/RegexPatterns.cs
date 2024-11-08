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
using Rock.Attribute;

namespace Rock.Constants
{
    /// <summary>
    /// Constant strings used with the RegularExpressionValidator Control.
    /// </summary>
    [RockInternal( "1.16.7" )]
    public static class RegexPatterns
    {
        /// <summary>
        /// A regular expression pattern string for matching strings that do not contain specific special characters.
        /// </summary>
        /// <remarks>
        /// This pattern excludes parentheses, curly braces, square brackets, and double quotes.
        /// It can be used to validate input that should not contain these specific special characters.
        /// </remarks>
        public static string SpecialCharacterPattern = @"^[^\(\{\[\)\}\]""]*$";

        /// <summary>
        /// Regular expression pattern used to identify and remove specific special characters
        /// </summary>
        public static string SpecialCharacterRemovalPattern = @"[\(\{\[\)\}\]""]";

        /// <summary>
        /// A regular expression pattern string for matching strings that do not contain emoji or special font characters.
        /// </summary>
        /// <remarks>
        /// This pattern excludes:
        /// - Copyright (©) and registered trademark (®) symbols
        /// - Various Unicode ranges including:
        ///   - General Punctuation (U+2000 to U+206F)
        ///   - Letterlike Symbols, Number Forms, Arrows, Mathematical Operators (U+2100 to U+2BFF)
        ///   - Miscellaneous Symbols, Dingbats (U+2600 to U+27BF)
        ///   - Supplemental Arrows-A, Braille Patterns, Supplemental Arrows-B (U+27F0 to U+2BFF)
        ///   - CJK Symbols and Punctuation (U+3000 to U+303F)
        ///   - Emoji and various symbol blocks (U+1F000 to U+1F9FF)
        ///   - Mathematical Alphanumeric Symbols (U+1D400 to U+1D7FF)
        /// It can be used to validate input that should not contain emoji or special font characters.
        /// </remarks>
        public static string EmojiAndSpecialFontPattern = @"^[^\u00a9\u00ae\u2000-\u3300\uD83C\uD000-\uDFFF\uD83D\uD000-\uDFFF\uD83E\uD000-\uDFFF\uD835\uDC00-\uDFFF]*$";

        /// <summary>
        /// Regular expression pattern used to identify and remove emojis and special font characters.
        /// </summary>
        public static string EmojiAndSpecialFontRemovalPattern = @"[\u00A9\u00AE\u2000-\u3300\uD83C\uD000-\uDFFF\uD83D\uD000-\uDFFF\uD83E\uD000-\uDFFF\uD835\uDC00-\uDFFF]";
    }
}