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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Jobs;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What the shipped projection queries say about themselves, read out of their own text.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is scaffolding for the tests below it and nothing in the run path uses it. The
    ///         job takes its column names from the data reader at the moment it reads, so a second
    ///         copy of the order in shipped code would be a third place for the order to be wrong.
    ///     </para>
    ///     <para>
    ///         The scrape is a stopgap. Once the queries become stored procedures the text is no
    ///         longer here to read, and the check becomes the integration suite asking each
    ///         procedure for its result schema, which is the stronger form of the same assertion.
    ///     </para>
    /// </remarks>
    internal static class ChatSyncSqlText
    {
        /// <summary>
        /// The columns that decide whether a group is a chat channel. A second copy of the rule, so
        /// that a test can hold the stamping statement and the staging query to the same words.
        /// </summary>
        public static readonly string[] QualificationColumns = new[]
        {
            "ChatChannelFirstEnabledDateTime",
            "IsChatAllowed",
            "IsChatEnabledOverride",
            "IsChatEnabledForAllGroups"
        };

        private static readonly Regex _projectedAlias = new Regex( @"(?i:\bAS)\s+\[(?<name>[a-z0-9_]+)\]", RegexOptions.Compiled );

        /// <summary>
        /// The columns a section's query returns, in the order it returns them.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>The column names.</returns>
        public static IList<string> SectionColumns( string section )
        {
            return _projectedAlias.Matches( ChatPlatformSync.GetSectionSql( section ) )
                .Cast<Match>()
                .Select( m => m.Groups["name"].Value )
                .ToList();
        }
    }
}
