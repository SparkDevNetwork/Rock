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
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Communication.Chat.Platform.Doors;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// A chat ban outlives the person's removal from the group: Rock archives the member instead of
    /// deleting them, the projection carries the archived row as a ban, and joining again cannot step
    /// around it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Rock deletes a removed member outright unless the group type keeps history and the
    ///         member has history to keep, so without this a banned member removed by staff could
    ///         join a public room again at once and read it. The fixture's group type keeps no
    ///         history, which is the case that used to delete.
    ///     </para>
    ///     <para>
    ///         An archived or inactive row is no membership, so the platform hears of it only as a
    ///         ban: banned with no end, granting nothing, until a later reading no longer carries it.
    ///         The platform reads a ban whose end has passed as an ordinary membership, and an
    ///         archived row must never become one.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ChatBanRemovalTests : DatabaseTestsBase
    {
        #region The removal

        [TestMethod]
        public void RemovingAMemberUnderABanInForceArchivesTheRowAndKeepsTheBan()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var room = fixture.AddChannel( fixture.SharedGroupTypeId, "Ban kept room" );
                var open = fixture.AddPerson( "OpenEnded" );
                var timed = fixture.AddPerson( "Timed" );
                var until = RockDateTime.Now.AddDays( 3 );
                fixture.AddMember( room, open, m => m.IsChatBanned = true );
                fixture.AddMember( room, timed, m => { m.IsChatBanned = true; m.ChatBannedUntil = until; } );

                DeleteThroughRock( room, open );
                DeleteThroughRock( room, timed );

                var openRow = Rows( room, open ).Single();
                Assert.IsTrue( openRow.IsArchived, "a banned member removed in a group that keeps no history is archived, not deleted" );
                Assert.IsTrue( openRow.IsChatBanned && !openRow.ChatBannedUntil.HasValue, "with the ban as it was" );

                var timedRow = Rows( room, timed ).Single();
                Assert.IsTrue( timedRow.IsArchived && timedRow.IsChatBanned );
                Assert.AreEqual( until.ToString( "s" ), timedRow.ChatBannedUntil.Value.ToString( "s" ), "and its end as it was" );
            }
        }

        [TestMethod]
        public void RemovingAMemberWithNoBanOrABanThatHasEndedDeletesTheRowAsBefore()
        {
            // A guard: only a ban still in force changes what removing a member does.
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var room = fixture.AddChannel( fixture.SharedGroupTypeId, "Ordinary removal room" );
                var plain = fixture.AddPerson( "Plain" );
                var ended = fixture.AddPerson( "Ended" );
                fixture.AddMember( room, plain );
                fixture.AddMember( room, ended, m => { m.IsChatBanned = true; m.ChatBannedUntil = RockDateTime.Now.AddHours( -1 ); } );

                DeleteThroughRock( room, plain );
                DeleteThroughRock( room, ended );

                Assert.AreEqual( 0, Rows( room, plain ).Count, "a member with no ban is deleted" );
                Assert.AreEqual( 0, Rows( room, ended ).Count, "and so is one whose ban has ended" );
            }
        }

        #endregion The removal

        #region The projection

        [TestMethod]
        public void AnArchivedOrInactiveRowUnderABanInForceIsProjectedAsABanWithNoEndThatGrantsNothing()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var leader = fixture.AddRole( fixture.SharedGroupTypeId, "Leader", true );
                var room = fixture.AddChannel( fixture.SharedGroupTypeId, "Archived ban room" );
                var archived = fixture.AddPerson( "Archived" );
                var inactive = fixture.AddPerson( "Inactive" );
                var until = RockDateTime.Now.AddDays( 3 );

                fixture.AddMember( room, archived, m => { m.GroupRoleId = leader; m.IsChatBanned = true; m.ChatBannedUntil = until; m.IsArchived = true; } );
                fixture.AddMember( room, inactive, m => { m.GroupRoleId = leader; m.IsChatBanned = true; m.GroupMemberStatus = GroupMemberStatus.Inactive; } );

                var payload = fixture.Project();

                foreach ( var person in new[] { archived, inactive } )
                {
                    var row = MembershipOf( payload, room, fixture.PrimaryAliasGuid( person ) );
                    Assert.IsNotNull( row, "a ban on a row that is no membership still keeps the person out, person " + person );
                    Assert.IsTrue( ( bool ) payload.Value( "members", row, "is_banned" ), "carried as banned, person " + person );

                    // Past its end the platform would read the row as a membership, so it carries none.
                    Assert.AreEqual( JTokenType.Null, payload.Value( "members", row, "ban_expires_at" ).Type, "with no end on the wire, person " + person );
                    Assert.IsFalse( ( bool ) payload.Value( "members", row, "is_leader" ), "and leading nothing, person " + person );
                }
            }
        }

        [TestMethod]
        public void AnArchivedRowWhoseBanHasEndedIsNotProjectedAndALiveRowBesideOneDecidesAlone()
        {
            // A guard on both edges of the rule: an ended ban is no ban, and a person staff added
            // back with a new row is the member that row says, as the doors treat them.
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var room = fixture.AddChannel( fixture.SharedGroupTypeId, "Ended ban room" );
                var ended = fixture.AddPerson( "Ended" );
                var back = fixture.AddPerson( "Back" );

                fixture.AddMember( room, ended, m => { m.IsChatBanned = true; m.ChatBannedUntil = RockDateTime.Now.AddHours( -1 ); m.IsArchived = true; } );
                fixture.AddMember( room, back, m => { m.IsChatBanned = true; m.IsArchived = true; } );
                fixture.AddMember( room, back );

                var payload = fixture.Project();

                Assert.IsNull( MembershipOf( payload, room, fixture.PrimaryAliasGuid( ended ) ), "an archived row whose ban has ended is nothing to carry" );

                var backRow = MembershipOf( payload, room, fixture.PrimaryAliasGuid( back ) );
                Assert.IsNotNull( backRow );
                Assert.IsFalse( ( bool ) payload.Value( "members", backRow, "is_banned" ), "the live row decides" );
            }
        }

        #endregion The projection

        #region Through the push and the doors

        [TestMethod]
        public void ABannedMemberRemovedByStaffStaysBannedOnThePlatformAndCannotJoinAgainUntilTheBanIsLifted()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var bea = scene.AddChatPerson( "Bea" );
                var room = scene.Fixture.AddChannel( scene.Fixture.SharedGroupTypeId, "Open room" );
                scene.Fixture.AddMember( room, bea, m => m.IsChatBanned = true );
                scene.WaitForPushes();

                using ( ChatSyncProjectionFixture.InsideRequest() )
                {
                    DeleteThroughRock( room, bea );
                }

                var pushes = scene.WaitForPushes();
                var alias = scene.Alias( bea );
                Assert.IsFalse( pushes.SelectMany( p => p.AbsentMembers ).Contains( (room, alias) ), "the platform is not told the membership is gone" );
                Assert.IsTrue( pushes.Last( p => p.Members.Any( r => ( Guid ) r[0] == room && ( Guid ) r[1] == alias ) )
                    .Members.Single( r => ( Guid ) r[0] == room && ( Guid ) r[1] == alias )[5].Value<bool>(), "it is told the ban stands" );

                Assert.AreEqual( "door.not_allowed", scene.Join( bea, room ).Code, "joining again does not step around the ban" );
                Assert.IsTrue( scene.Memberships( room, bea ).Single().IsArchived, "and brings nothing back" );

                // Staff lift the ban on the archived row, and the room is open to her again.
                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var row = new GroupMemberService( rockContext ).AsNoFilter().Single( m => m.Group.Guid == room && m.PersonId == bea );
                    row.IsChatBanned = false;
                    rockContext.SaveChanges();
                }

                Assert.IsTrue( scene.WaitForPushes().SelectMany( p => p.AbsentMembers ).Contains( (room, alias) ), "once lifted, the archived row is no membership at all" );
                Assert.AreEqual( "ok", scene.Join( bea, room ).Code );
            }
        }

        #endregion Through the push and the doors

        #region Support

        /// <summary>
        /// Removes a person's rows from a group the way Rock's own screens do, through the member
        /// service's delete.
        /// </summary>
        internal static void DeleteThroughRock( Guid groupGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new GroupMemberService( rockContext );
                foreach ( var member in service.Queryable().Where( m => m.Group.Guid == groupGuid && m.PersonId == personId ).ToList() )
                {
                    service.Delete( member );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Every row a person holds in a group, archived ones included.
        /// </summary>
        private static List<GroupMember> Rows( Guid groupGuid, int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupMemberService( rockContext ).AsNoFilter().AsNoTracking()
                    .Where( m => m.Group.Guid == groupGuid && m.PersonId == personId )
                    .ToList();
            }
        }

        /// <summary>
        /// The membership row a channel carries for one person, or null when it carries none.
        /// </summary>
        private static JArray MembershipOf( ProjectedPayload payload, Guid channelGuid, Guid aliasGuid )
        {
            var channelText = ( string ) payload.Value( "channels", payload.Row( "channels", "channel_id", channelGuid ), "channel_id" );

            return payload.Rows( "members" )
                .Where( r => string.Equals( ( string ) payload.Value( "members", r, "channel_id" ), channelText, StringComparison.Ordinal )
                    && Guid.Parse( ( string ) payload.Value( "members", r, "person_alias_guid" ) ) == aliasGuid )
                .SingleOrDefault();
        }

        #endregion Support
    }
}
