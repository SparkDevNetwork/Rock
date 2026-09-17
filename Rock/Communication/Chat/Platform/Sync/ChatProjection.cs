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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Contract;
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Runs the three projection texts and the C# badge list. The SQL is the
    /// only definition of "is a chat channel"; badges are C# because the colour
    /// pair is floating-point HSL with no T-SQL form.
    /// </summary>
    internal static class ChatProjection
    {
        #region Constants

        private const string ChannelsResource = "Rock.Communication.Chat.Platform.Sql.channels.sql";
        private const string MembersResource = "Rock.Communication.Chat.Platform.Sql.members.sql";
        private const string AliasesResource = "Rock.Communication.Chat.Platform.Sql.aliases.sql";

        #endregion

        #region Methods

        /// <summary>
        /// The channels SQL text, for tests that pin the channel predicate and
        /// the DM invariants without a database.
        /// </summary>
        internal static string ChannelsSql
        {
            get { return ReadResource( ChannelsResource ); }
        }

        /// <summary>
        /// The members SQL text.
        /// </summary>
        internal static string MembersSql
        {
            get { return ReadResource( MembersResource ); }
        }

        /// <summary>
        /// The aliases SQL text.
        /// </summary>
        internal static string AliasesSql
        {
            get { return ReadResource( AliasesResource ); }
        }

        /// <summary>
        /// Stamps currently-enabled groups that have never been stamped, then
        /// reads the four sections.
        /// </summary>
        internal static IDictionary<string, IList<IDictionary<string, object>>> Read(
            RockContext rockContext,
            ChatPlatformConfiguration configuration,
            int commandTimeoutSeconds )
        {
            var connection = rockContext.Database.Connection;
            var opened = false;
            if ( connection.State != ConnectionState.Open )
            {
                connection.Open();
                opened = true;
            }

            try
            {
                StampFirstEnabled( connection, commandTimeoutSeconds );
                var parameters = Parameters( configuration );

                var sections = new Dictionary<string, IList<IDictionary<string, object>>>
                {
                    { "channels", ReadSql( connection, ChannelsSql, parameters, commandTimeoutSeconds ) },
                    { "members", ReadSql( connection, MembersSql, parameters, commandTimeoutSeconds ) },
                    { "aliases", ReadAliases( connection, parameters, commandTimeoutSeconds ) },
                    { "badges", ReadBadges( configuration ) }
                };

                return sections;
            }
            finally
            {
                if ( opened )
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Identity high-water values keyed by the artifact's marks keys.
        /// </summary>
        internal static IDictionary<string, long> ReadIdentityMarks( RockContext rockContext )
        {
            var map = new Dictionary<string, string>( StringComparer.Ordinal )
            {
                { "person", "Person" },
                { "person_alias", "PersonAlias" },
                { "group", "Group" },
                { "group_member", "GroupMember" }
            };

            var marks = new Dictionary<string, long>();
            var connection = rockContext.Database.Connection;
            var opened = false;
            if ( connection.State != ConnectionState.Open )
            {
                connection.Open();
                opened = true;
            }

            try
            {
                foreach ( var key in ChatWireContract.MarksKeys )
                {
                    string table;
                    if ( !map.TryGetValue( key, out table ) )
                    {
                        marks[key] = 0;
                        continue;
                    }

                    using ( var command = connection.CreateCommand() )
                    {
                        command.CommandText = string.Format( "SELECT CAST(ISNULL(IDENT_CURRENT('{0}'), 0) AS bigint);", table );
                        var raw = command.ExecuteScalar();
                        marks[key] = raw == null || raw is DBNull ? 0L : Convert.ToInt64( raw, CultureInfo.InvariantCulture );
                    }
                }
            }
            finally
            {
                if ( opened )
                {
                    connection.Close();
                }
            }

            return marks;
        }

        #endregion

        #region Private Methods

        private static void StampFirstEnabled( System.Data.Common.DbConnection connection, int commandTimeoutSeconds )
        {
            using ( var command = connection.CreateCommand() )
            {
                command.CommandTimeout = commandTimeoutSeconds;
                command.CommandText = @"
UPDATE g
   SET ChatChannelFirstEnabledDateTime = SYSUTCDATETIME()
  FROM dbo.[Group] g
  JOIN dbo.GroupType gt ON gt.Id = g.GroupTypeId
 WHERE g.ChatChannelFirstEnabledDateTime IS NULL
   AND gt.IsChatAllowed = 1
   AND COALESCE(g.IsChatEnabledOverride, gt.IsChatEnabledForAllGroups) = 1;";
                command.ExecuteNonQuery();
            }
        }

        private static List<IDictionary<string, object>> ReadSql(
            System.Data.Common.DbConnection connection,
            string sql,
            IList<IDbDataParameter> parameters,
            int commandTimeoutSeconds )
        {
            using ( var command = connection.CreateCommand() )
            {
                command.CommandTimeout = commandTimeoutSeconds;
                command.CommandText = sql;
                foreach ( var parameter in parameters )
                {
                    var copy = command.CreateParameter();
                    copy.ParameterName = parameter.ParameterName;
                    copy.DbType = parameter.DbType;
                    copy.Value = parameter.Value ?? DBNull.Value;
                    command.Parameters.Add( copy );
                }

                var rows = new List<IDictionary<string, object>>();
                using ( var reader = command.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        var row = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );
                        for ( var i = 0; i < reader.FieldCount; i++ )
                        {
                            var value = reader.IsDBNull( i ) ? null : reader.GetValue( i );
                            row[reader.GetName( i )] = value;
                        }

                        rows.Add( row );
                    }
                }

                return rows;
            }
        }

        private static List<IDictionary<string, object>> ReadAliases(
            System.Data.Common.DbConnection connection,
            IList<IDbDataParameter> parameters,
            int commandTimeoutSeconds )
        {
            var rows = ReadSql( connection, AliasesSql, parameters, commandTimeoutSeconds );
            foreach ( var row in rows )
            {
                object raw;
                if ( row.TryGetValue( "badge_keys", out raw ) && raw is string text && !string.IsNullOrWhiteSpace( text ) )
                {
                    row["badge_keys"] = text.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                        .Select( s => Guid.Parse( s ) )
                        .ToList();
                }
                else if ( raw == null )
                {
                    row["badge_keys"] = null;
                }
                else
                {
                    row["badge_keys"] = new List<Guid>();
                }
            }

            return rows;
        }

        private static List<IDictionary<string, object>> ReadBadges( ChatPlatformConfiguration configuration )
        {
            var rows = new List<IDictionary<string, object>>();
            var guids = configuration.ChatBadgeDataViewGuids ?? new List<Guid>();
            for ( var i = 0; i < guids.Count; i++ )
            {
                var dataView = DataViewCache.Get( guids[i] );
                if ( dataView == null || !dataView.IsPersisted() )
                {
                    continue;
                }

                string bg = null;
                string fg = null;
                if ( !string.IsNullOrWhiteSpace( dataView.HighlightColor ) )
                {
                    var pair = RockColor.CalculateColorPair( new RockColor( dataView.HighlightColor ) );
                    bg = pair?.BackgroundColor?.ToHex();
                    fg = pair?.ForegroundColor?.ToHex();
                }

                rows.Add( new Dictionary<string, object>
                {
                    { "badge_key", dataView.Guid },
                    { "name", dataView.Name },
                    { "icon_css", dataView.IconCssClass },
                    { "bg_color", bg },
                    { "fg_color", fg },
                    { "sort_order", i }
                } );
            }

            return rows;
        }

        private static IList<IDbDataParameter> Parameters( ChatPlatformConfiguration configuration )
        {
            var root = GlobalAttributesCache.Value( "PublicApplicationRoot" ) ?? string.Empty;
            if ( !root.EndsWith( "/" ) && root.Length > 0 )
            {
                root += "/";
            }

            var active = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
            var badgeXml = new XElement( "root" );
            foreach ( var guid in ( configuration.ChatBadgeDataViewGuids ?? new List<Guid>() ).Distinct() )
            {
                badgeXml.Add( new XElement( "g", guid.ToString( "D" ) ) );
            }

            return new IDbDataParameter[]
            {
                Param( "@Root", DbType.String, root ),
                Param( "@DmTypeGuid", DbType.Guid, Guid.Parse( Rock.SystemGuid.GroupType.GROUPTYPE_CHAT_DIRECT_MESSAGE ) ),
                Param( "@LivestreamTypeGuid", DbType.Guid, Guid.Empty ),
                Param( "@ShowProfileDefault", DbType.Boolean, configuration.AreChatProfilesVisible ),
                Param( "@OpenDmDefault", DbType.Boolean, configuration.IsOpenDirectMessagingAllowed ),
                Param( "@ChatPeopleGuid", DbType.Guid, Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE ) ),
                Param( "@BanListGuid", DbType.Guid, Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST ) ),
                Param( "@ChatAdminsGuid", DbType.Guid, Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS ) ),
                Param( "@RecordStatusActiveId", DbType.Int32, active != null ? ( object ) active.Id : 0 ),
                Param( "@SystemAliasGuid", DbType.Guid, Guid.Parse( Rock.SystemGuid.Person.CHAT_SYSTEM ) ),
                Param( "@BadgeGuidsXml", DbType.Xml, badgeXml.ToString( SaveOptions.DisableFormatting ) )
            };
        }

        private static IDbDataParameter Param( string name, DbType type, object value )
        {
            return new SqlParameter( name, type ) { Value = value ?? DBNull.Value };
        }

        private static string ReadResource( string name )
        {
            var assembly = typeof( ChatProjection ).Assembly;
            using ( var stream = assembly.GetManifestResourceStream( name ) )
            {
                if ( stream == null )
                {
                    throw new InvalidOperationException( string.Format( "The chat projection {0} is not embedded.", name ) );
                }

                using ( var reader = new StreamReader( stream, Encoding.UTF8 ) )
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
