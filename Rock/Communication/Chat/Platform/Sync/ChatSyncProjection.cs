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
    internal static class ChatSyncProjection
    {
        #region Fields

        internal static readonly string[] QualificationColumns = new[]
        {
            "ChatChannelFirstEnabledDateTime",
            "IsChatAllowed",
            "IsChatEnabledOverride",
            "IsChatEnabledForAllGroups"
        };

        private static readonly Dictionary<string, string> _resourceNamesBySection = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
        {
            { "aliases", "ChatSyncAliases.sql" },
            { "channels", "ChatSyncChannels.sql" },
            { "members", "ChatSyncMembers.sql" },
            { "badges", "ChatSyncBadges.sql" }
        };

        private static readonly ConcurrentDictionary<string, string> _sqlByResourceName = new ConcurrentDictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        private static readonly Regex _projectedAlias = new Regex( @"(?i:\bAS)\s+\[(?<name>[a-z0-9_]+)\]", RegexOptions.Compiled );

        #endregion

        #region Methods

        public static string GetStagingSql()
        {
            return ReadSql( "ChatSyncStage.sql" );
        }

        public static string GetStampSql()
        {
            return ReadSql( "ChatSyncStampChannels.sql" );
        }

        public static string GetSectionSql( string section )
        {
            string resourceName;

            if ( section == null || !_resourceNamesBySection.TryGetValue( section, out resourceName ) )
            {
                throw new InvalidOperationException( string.Format( "no chat projection query ships for the {0} section", section ?? "(none)" ) );
            }

            return ReadSql( resourceName );
        }

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
