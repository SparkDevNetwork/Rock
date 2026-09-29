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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The projection read for the few keys one save touched, which is what the immediate sync
    /// pushes after the save commits.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The immediate sync uses the same procedure as the full sync, so the two can never
    ///         disagree about what a chat channel is or what a row looks like. What differs is the
    ///         scope, and the one thing a scope needs that a whole church does not: a key that was
    ///         asked about and no longer qualifies has to be named, because the platform stamps
    ///         absent only what a push lists.
    ///     </para>
    ///     <para>
    ///         A membership always travels with its channel and its alias, because the platform
    ///         refuses one whose channel or alias it does not hold, and a group can become a channel
    ///         through its group type, which no save hook sees.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ProjectionScopeTests : DatabaseTestsBase
    {
        [TestMethod]
        public void APersonScopeCarriesThatPersonsAliasesAndNothingElse()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Person scope channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                var adaExtraAlias = fixture.AddExtraAlias( adaId );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.PersonIds.Add( adaId );

                var push = fixture.ProjectChanges( changes );

                var adaPrimary = fixture.PrimaryAliasGuid( adaId );
                CollectionAssert.AreEquivalent( new[] { adaPrimary, adaExtraAlias }, Keys( push, "aliases" ),
                    "a person's save carries every alias of that person, and nobody else's" );
                Assert.AreEqual( 0, Rows( push, "channels" ).Count, "a person's save carries no channel" );
                Assert.AreEqual( 0, Rows( push, "members" ).Count, "a person's save carries no membership" );
                Assert.AreEqual( 0, Rows( push, "badges" ).Count, "a push never carries badges" );
                Assert.AreEqual( 0, Absent( push, "channels" ).Count );
                Assert.AreEqual( 0, Absent( push, "members" ).Count );
                Assert.AreEqual( 2, push.RowCount, "two alias rows and nothing else: the row count agrees with the sections" );
            }
        }

        [TestMethod]
        public void AMembershipTravelsWithItsChannelAndItsPersonsAliases()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Member scope channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.MemberKeys.Add( (channel, adaId) );

                var push = fixture.ProjectChanges( changes );

                var adaPrimary = fixture.PrimaryAliasGuid( adaId );
                var members = Rows( push, "members" );
                Assert.AreEqual( 1, members.Count, "only the membership the save touched" );
                Assert.AreEqual( channel, ( Guid ) members[0][0] );
                Assert.AreEqual( adaPrimary, ( Guid ) members[0][1] );
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( push, "channels" ),
                    "the membership's channel comes with it, so the platform can take the row into an empty mirror" );
                CollectionAssert.AreEquivalent( new[] { adaPrimary }, Keys( push, "aliases" ),
                    "and so does the person's alias, and not the other member's" );
                Assert.IsFalse( Keys( push, "aliases" ).Contains( Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid() ),
                    "chat's own author row belongs to the full sync" );
                Assert.AreEqual( 0, Absent( push, "members" ).Count );
            }
        }

        [TestMethod]
        public void AMembershipThatWasRemovedIsListedAbsentUnderThePersonsPrimaryAlias()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Removed member channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId, member => member.GroupMemberStatus = Rock.Model.GroupMemberStatus.Inactive );
                fixture.DeleteMember( channel, adaId );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.MemberKeys.Add( (channel, adaId) );
                changes.MemberKeys.Add( (channel, boId) );

                var push = fixture.ProjectChanges( changes );

                Assert.AreEqual( 0, Rows( push, "members" ).Count, "neither membership is live" );
                CollectionAssert.AreEquivalent(
                    new[] { channel + " " + fixture.PrimaryAliasGuid( adaId ), channel + " " + fixture.PrimaryAliasGuid( boId ) },
                    Absent( push, "members" ).Select( k => ( Guid ) k[0] + " " + ( Guid ) k[1] ).ToList(),
                    "a deleted membership and an inactive one are both named, so the platform can stamp them absent" );
                CollectionAssert.AreEquivalent( new[] { channel }, Keys( push, "channels" ),
                    "the channel itself is still a channel and still travels" );
            }
        }

        [TestMethod]
        public void AGroupThatWasArchivedShipsItsChannelAndListsEveryFormerMemberAbsent()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Archived scope channel" );
                var adaId = fixture.AddPerson( "Ada" );
                var boId = fixture.AddPerson( "Bo" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( channel, boId );

                // Marked first, as any run before the archive would have left it.
                fixture.MarkChannelDirectly( channel );
                fixture.EditChannel( channel, group => group.IsArchived = true );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.GroupGuids.Add( channel );

                var push = fixture.ProjectChanges( changes );

                var channels = Rows( push, "channels" );
                Assert.AreEqual( 1, channels.Count, "an archived channel keeps its row" );
                Assert.AreEqual( false, ( bool ) channels[0][4], "and loses its reach" );
                Assert.AreEqual( 0, Rows( push, "members" ).Count, "an archived channel takes no members" );
                CollectionAssert.AreEquivalent(
                    new[] { fixture.PrimaryAliasGuid( adaId ), fixture.PrimaryAliasGuid( boId ) },
                    Absent( push, "members" ).Where( k => ( Guid ) k[0] == channel ).Select( k => ( Guid ) k[1] ).ToList(),
                    "a group's save names every one of its members who is no longer in the channel" );
            }
        }

        [TestMethod]
        public void AChannelScopeCarriesTheChannelRowAloneAndNamesANonChannelAbsent()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Channel scope channel" );
                var notChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Not a channel", group => group.IsChatEnabledOverride = false );
                var adaId = fixture.AddPerson( "Ada" );
                fixture.AddMember( channel, adaId );
                fixture.AddMember( notChannel, adaId );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.ChannelGuids.Add( channel );
                changes.ChannelGuids.Add( notChannel );

                var push = fixture.ProjectChanges( changes );

                CollectionAssert.AreEquivalent( new[] { channel }, Keys( push, "channels" ),
                    "a channel scope carries the channel's own row" );
                Assert.AreEqual( 0, Rows( push, "members" ).Count, "and none of its memberships" );
                Assert.AreEqual( 0, Rows( push, "aliases" ).Count, "and no member's alias" );
                Assert.AreEqual( 0, Absent( push, "members" ).Count, "and names no member absent, since it read none" );
                CollectionAssert.AreEquivalent( new[] { notChannel }, Absent( push, "channels" ).Select( k => ( Guid ) k ).ToList(),
                    "a group in the scope that is not a chat channel is named absent, as a whole group scope names it" );
                Assert.IsNotNull( fixture.ChannelMark( channel ), "and the channel is marked, as any scoped read marks the groups it asks about" );
                Assert.AreEqual( 1, push.RowCount );
            }
        }

        [TestMethod]
        public void AGroupThatNoLongerExistsIsListedAbsentByItsGuid()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var deleted = Guid.NewGuid();

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.GroupGuids.Add( deleted );

                var push = fixture.ProjectChanges( changes );

                Assert.AreEqual( 0, Rows( push, "channels" ).Count );
                CollectionAssert.AreEquivalent( new[] { deleted }, Absent( push, "channels" ).Select( k => ( Guid ) k ).ToList(),
                    "a deleted group has no row to read, so the guid its save recorded is what names it absent" );
            }
        }

        [TestMethod]
        public void AScopedReadMarksOnlyTheGroupsInItsScope()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var inScope = fixture.AddChannel( fixture.SharedGroupTypeId, "Marked by the push" );
                var outOfScope = fixture.AddChannel( fixture.SharedGroupTypeId, "Left for the full sync" );

                var changes = new ChatPlatformSyncHelper.ImmediateChanges();
                changes.GroupGuids.Add( inScope );

                var push = fixture.ProjectChanges( changes );

                Assert.IsNotNull( fixture.ChannelMark( inScope ),
                    "a group that starts qualifying through a save is marked by that save's push" );
                Assert.IsNull( fixture.ChannelMark( outOfScope ),
                    "and a push marks nothing it was not asked about" );
                Assert.IsTrue( Math.Abs( ( push.ReadAtUtc - DateTime.UtcNow ).TotalMinutes ) < 5,
                    "the read time is the database's own UTC time" );
                Assert.AreEqual( DateTimeKind.Utc, push.ReadAtUtc.Kind );
            }
        }

        #region Support

        private static List<JArray> Rows( ChatPlatformSyncHelper.PushBody push, string section )
        {
            return ( ( JArray ) push.Body[section] ).Cast<JArray>().ToList();
        }

        /// <summary>
        /// The first column of every row of a section, which is its key.
        /// </summary>
        private static List<Guid> Keys( ChatPlatformSyncHelper.PushBody push, string section )
        {
            return Rows( push, section ).Select( r => ( Guid ) r[0] ).ToList();
        }

        private static List<JToken> Absent( ChatPlatformSyncHelper.PushBody push, string kind )
        {
            return ( ( JArray ) push.Body["absent"][kind] ).ToList();
        }

        #endregion Support
    }
}
