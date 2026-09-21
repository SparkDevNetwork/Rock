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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The queries that read a church's chat rows out of Rock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is one staging query and one query per payload section, and they run in that order on
    /// one connection, because the staging query leaves its results in temporary tables that the
    /// other four read.
    /// </para>
    /// <para>
    /// Splitting it that way is not only tidiness. The question "is this group a chat channel" is
    /// asked once, in the staging query, so there is one definition of it rather than four. Rock
    /// disagrees with itself about that question in two places today, and the answer decides which
    /// rows exist at all.
    /// </para>
    /// <para>
    /// It also closes a failure the four queries would otherwise share. They are separate
    /// statements seconds apart, and the far side keys a membership to both its channel and its
    /// person. A group that starts qualifying, or a person who joins a group, between two of those
    /// statements puts a membership in the payload whose channel or whose person is in no other
    /// section of it, which fails the whole submission rather than that one row, on every retry.
    /// Reading the frozen sets makes the containment hold by construction.
    /// </para>
    /// </remarks>
    internal static class ChatSyncProjection
    {
        #region Fields

        /// <summary>
        /// The columns that decide whether a group is a chat channel. They belong to the staging
        /// query and to nothing else.
        /// </summary>
        internal static readonly string[] QualificationColumns = new[]
        {
            "ChatChannelFirstEnabledDateTime",
            "IsChatAllowed",
            "IsChatEnabledOverride",
            "IsChatEnabledForAllGroups"
        };

        /// <summary>
        /// The file each payload section's query ships in.
        /// </summary>
        private static readonly Dictionary<string, string> _resourceNamesBySection = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            { "aliases", "ChatSyncAliases.sql" },
            { "channels", "ChatSyncChannels.sql" },
            { "members", "ChatSyncMembers.sql" },
            { "badges", "ChatSyncBadges.sql" }
        };

        /// <summary>
        /// Each query is read out of the assembly once.
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _sqlByResourceName = new ConcurrentDictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Matches a projected column's alias in a section query.
        /// </summary>
        /// <remarks>
        /// Every wire column is lower case with underscores, which is the contract's own naming, and
        /// every table alias in these queries is upper case. Matching the name case sensitively is
        /// therefore what separates the two, and it is why a table may not be aliased in lower case
        /// in a section query: doing so would put a name into this list that the wire never carries,
        /// and the order check would fail with nothing actually wrong.
        /// </remarks>
        private static readonly Regex _projectedAlias = new Regex( @"(?i:\bAS)\s+\[(?<name>[a-z0-9_]+)\]", RegexOptions.Compiled );

        #endregion

        #region Methods

        /// <summary>
        /// The query that stages the sets the section queries read.
        /// </summary>
        /// <returns>The query text.</returns>
        public static string GetStagingSql()
        {
            return ReadSql( "ChatSyncStage.sql" );
        }

        /// <summary>
        /// The statement that marks the groups that are chat channels right now.
        /// </summary>
        /// <returns>The statement text.</returns>
        public static string GetStampSql()
        {
            return ReadSql( "ChatSyncStampChannels.sql" );
        }

        /// <summary>
        /// The query that reads one payload section.
        /// </summary>
        /// <param name="section">The payload section, as the wire contract names it.</param>
        /// <returns>The query text.</returns>
        public static string GetSectionSql( string section )
        {
            string resourceName;

            if ( section == null || !_resourceNamesBySection.TryGetValue( section, out resourceName ) )
            {
                throw new InvalidOperationException( string.Format( "no chat projection query ships for the {0} section", section ?? "(none)" ) );
            }

            return ReadSql( resourceName );
        }

        /// <summary>
        /// Reads one query out of this assembly's manifest.
        /// </summary>
        /// <param name="fileName">The query's file name.</param>
        /// <returns>The query text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the query is not packaged into the assembly, which is a build failure rather
        /// than a runtime condition: without it this church cannot restate at all, so it says so
        /// here rather than sending a payload missing a section.
        /// </exception>
        private static string ReadSql( string fileName )
        {
            return _sqlByResourceName.GetOrAdd( fileName, name =>
            {
                var assembly = typeof( ChatSyncProjection ).Assembly;
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault( n => n.EndsWith( "." + name, StringComparison.OrdinalIgnoreCase ) );

                if ( resourceName == null )
                {
                    throw new InvalidOperationException( string.Format( "the chat projection query {0} is not embedded in this assembly", name ) );
                }

                using ( var stream = assembly.GetManifestResourceStream( resourceName ) )
                using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
                {
                    return reader.ReadToEnd();
                }
            } );
        }

        /// <summary>
        /// The column aliases a section query returns, in the order it returns them.
        /// </summary>
        /// <param name="section">The payload section.</param>
        /// <returns>The aliases.</returns>
        public static IList<string> GetSectionColumns( string section )
        {
            var sql = GetSectionSql( section );

            // Read out of the query rather than written down beside it, so the list cannot drift
            // from what the query actually returns. A second copy of the order would just be a
            // third place for the order to be wrong.
            return _projectedAlias.Matches( sql )
                .Cast<Match>()
                .Select( m => m.Groups["name"].Value )
                .ToList();
        }

        #endregion
    }
}
