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
namespace Rock.Tests.Lava.Shared
{
    /// <summary>
    /// String helpers for writing Lava template tests.
    /// </summary>
    public static class LavaTestStringExtensions
    {
        /// <summary>
        /// Converts every line terminator in the string to a single line feed.
        /// </summary>
        /// <remarks>
        /// A multi-line string literal carries whatever line terminator the source
        /// file happens to use, and the repository has no <c>.gitattributes</c> to
        /// pin that down - a Windows working copy checked out with
        /// <c>core.autocrlf=true</c> holds CRLF while a Linux or CI checkout holds
        /// LF. Comparing a rendered template against such a literal would therefore
        /// pass on one machine and fail on another. Apply this to a multi-line
        /// expected value; rendered output from
        /// <see cref="LavaRenderTestHelper.Render"/> is already normalized.
        /// </remarks>
        /// <param name="input">The string to normalize.</param>
        /// <returns>The string with all line terminators converted to "\n".</returns>
        public static string NormalizeLineEndings( this string input )
        {
            if ( input == null )
            {
                return null;
            }

            return input.Replace( "\r\n", "\n" ).Replace( "\r", "\n" );
        }
    }
}
