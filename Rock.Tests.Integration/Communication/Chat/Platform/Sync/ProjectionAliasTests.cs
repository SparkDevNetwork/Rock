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
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The two shapes an alias row comes in, and the form its identifiers travel in.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A person's primary alias row carries everything about them. Their other alias rows
    ///         carry the two identifiers and nothing else, which is what lets a client still holding
    ///         an old alias resolve it to whoever owns it now. A merge in Rock heals itself here,
    ///         with nothing tracking that it happened, because every alias is restated every cycle.
    ///     </para>
    ///     <para>
    ///         The guid cell is about a real hazard of this wire. SQL Server hands a uniqueidentifier
    ///         back with its first three groups byte swapped relative to the way it prints, and
    ///         Postgres parses a uuid literally. Anything that moved those bytes, or upper cased
    ///         them, would key a person to a different row on the far side and nothing in Rock would
    ///         look wrong.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ProjectionAliasTests : DatabaseTestsBase
    {
        #region The two row shapes

        [TestMethod]
        public void ANonPrimaryAliasRow_CarriesTheTwoIdentifiersAndNothingElse()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Alias shape channel" );
                var personId = fixture.AddPerson( "AliasShape" );
                fixture.AddMember( channelGuid, personId );

                var primaryAliasGuid = PrimaryAliasGuid( personId );
                var extraAliasGuid = fixture.AddExtraAlias( personId );

                var payload = fixture.Project();

                var primary = payload.Row( "aliases", "person_alias_guid", primaryAliasGuid );
                var extra = payload.Row( "aliases", "person_alias_guid", extraAliasGuid );

                Assert.IsNotNull( primary, "the person's primary alias was not projected" );
                Assert.IsNotNull( extra, "the person's second alias was not projected, so a client holding it could not resolve it" );

                Assert.AreEqual( primaryAliasGuid.ToString(), ( string ) payload.Value( "aliases", extra, "primary_person_alias_guid" ),
                    "the second alias does not point at the person who owns it now" );

                foreach ( var column in new[] { "nick_name", "last_name", "avatar_url", "campus_id",
                    "show_profile_details", "is_open_dm_allowed", "is_globally_banned", "is_inactive" } )
                {
                    Assert.AreEqual( JTokenType.Null, payload.Value( "aliases", extra, column ).Type,
                        string.Format( "the second alias row carries {0}, which belongs on the primary row alone", column ) );
                }

                // Badges are the one position that is an empty list rather than nothing. The column
                // they land in cannot hold absent, and the far side turns a missing value and an
                // empty list into the same thing, so the two forms are the same row. The empty list
                // is what the shaper sends, and it is asserted here rather than left unchecked.
                Assert.AreEqual( JTokenType.Array, payload.Value( "aliases", extra, "badge_keys" ).Type );
                Assert.AreEqual( 0, ( ( JArray ) payload.Value( "aliases", extra, "badge_keys" ) ).Count,
                    "the second alias row claims badges of its own" );

                Assert.AreEqual( "AliasShape", ( string ) payload.Value( "aliases", primary, "last_name" ) );
                Assert.AreEqual( primaryAliasGuid.ToString(), ( string ) payload.Value( "aliases", primary, "primary_person_alias_guid" ) );
                Assert.AreNotEqual( JTokenType.Null, payload.Value( "aliases", primary, "show_profile_details" ).Type );
                Assert.AreNotEqual( JTokenType.Null, payload.Value( "aliases", primary, "is_inactive" ).Type );
            }
        }

        /// <summary>
        /// Chat writes its own messages about a conversation, and the author of those messages is
        /// not a person in anybody's Rock.
        /// </summary>
        [TestMethod]
        public void TheSystemAuthorIsProjectedAsAnAliasOfItsOwn()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var payload = fixture.Project();
                var author = payload.Row( "aliases", "person_alias_guid", Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid() );

                Assert.IsNotNull( author, "nothing on the far side could attribute a system message" );
                Assert.AreEqual( "Rock", ( string ) payload.Value( "aliases", author, "nick_name" ) );
                Assert.AreEqual( "Chat", ( string ) payload.Value( "aliases", author, "last_name" ) );
            }
        }

        #endregion The two row shapes

        #region The form a guid travels in

        [TestMethod]
        public void EveryIdentifierOnTheWire_IsLowerCaseHyphenatedAndUnswapped()
        {
            var uuid = new Regex( "^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$" );

            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Guid round trip channel" );
                var personId = fixture.AddPerson( "GuidRoundTrip" );
                fixture.AddMember( channelGuid, personId );

                var payload = fixture.Project();
                var channel = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( channel, "the channel was not projected at all" );

                var projected = ( string ) payload.Value( "channels", channel, "channel_id" );

                Assert.IsTrue( uuid.IsMatch( projected ),
                    string.Format( "'{0}' is not a form Postgres reads as the same uuid", projected ) );

                // The guid as SQL Server itself prints it, so a byte swap anywhere between the
                // column and the wire shows up as a different value rather than as a format.
                Assert.AreEqual( SqlServerGuidText( channelGuid ), projected,
                    "the identifier on the wire is not the one the database holds" );

                var member = payload.Rows( "members" )
                    .FirstOrDefault( r => string.Equals( ( string ) payload.Value( "members", r, "channel_id" ), projected, StringComparison.Ordinal ) );

                Assert.IsNotNull( member, "the membership was not projected" );
                Assert.IsTrue( uuid.IsMatch( ( string ) payload.Value( "members", member, "person_alias_guid" ) ) );
            }
        }

        #endregion The form a guid travels in

        #region Support

        private static Guid PrimaryAliasGuid( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new PersonService( rockContext ).Queryable()
                    .Where( p => p.Id == personId )
                    .Select( p => p.Aliases.FirstOrDefault( a => a.Id == p.PrimaryAliasId ).Guid )
                    .First();
            }
        }

        /// <summary>
        /// The guid as SQL Server itself renders it, taken from SQL Server rather than from .NET.
        /// </summary>
        private static string SqlServerGuidText( Guid value )
        {
            using ( var rockContext = new RockContext() )
            {
                return rockContext.Database
                    .SqlQuery<string>( "SELECT LOWER( CAST( [Guid] AS VARCHAR( 36 ) ) ) FROM [Group] WHERE [Guid] = @p0", value )
                    .First();
            }
        }

        #endregion Support
    }
}
