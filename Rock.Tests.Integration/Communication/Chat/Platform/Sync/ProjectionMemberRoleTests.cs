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
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// One membership row for a person in a channel, however many roles they hold in its group.
    /// </summary>
    /// <remarks>
    /// Rock lets one person hold several roles in one group, each its own group member. The far side
    /// keys a membership by channel and alias, so two rows for the same pair cannot both be stored,
    /// and the whole church's submission fails on every cycle over it. What the roles allow in chat
    /// is folded onto that one row the same way: a person may do what any of their roles allows.
    /// </remarks>
    [TestClass]
    public class ProjectionMemberRoleTests : DatabaseTestsBase
    {
        [TestMethod]
        public void APersonWhoIsBothMemberAndLeader_IsOneMembershipAndALeader()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var leaderRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Leader", true );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Two roles channel" );
                var personId = fixture.AddPerson( "TwoRoles" );

                fixture.AddMember( channelGuid, personId );
                fixture.AddMember( channelGuid, personId, member => member.GroupRoleId = leaderRoleId );

                var payload = fixture.Project();
                var membership = SingleMembership( payload, channelGuid );

                Assert.IsTrue( ( bool ) payload.Value( "members", membership, "is_leader" ), "a person who leads the group is not a leader of its channel" );
                Assert.IsFalse( ( bool ) payload.Value( "members", membership, "is_banned" ), "a person banned in neither role is banned" );
            }
        }

        [TestMethod]
        public void APersonBannedInOneRoleOnly_IsBannedUntilThatRolesExpiry()
        {
            // A winter date, so the offset applied on the way out does not depend on when this runs.
            var expiresAt = new DateTime( 2027, 1, 15, 9, 30, 0, DateTimeKind.Unspecified );

            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var otherRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Helper", false );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Half banned channel" );
                var personId = fixture.AddPerson( "HalfBanned" );

                fixture.AddMember( channelGuid, personId );
                fixture.AddMember( channelGuid, personId, member =>
                {
                    member.GroupRoleId = otherRoleId;
                    member.IsChatBanned = true;
                    member.ChatBannedUntil = expiresAt;
                } );

                var payload = fixture.Project();
                var membership = SingleMembership( payload, channelGuid );

                Assert.IsTrue( ( bool ) payload.Value( "members", membership, "is_banned" ), "a ban held in one role was lifted by the other role" );

                var written = ( string ) payload.Value( "members", membership, "ban_expires_at" );

                Assert.IsNotNull( written, "the ban has no end on the wire, so the far side would treat it as permanent" );
                Assert.AreEqual(
                    TimeZoneInfo.ConvertTimeToUtc( expiresAt, RockDateTime.OrgTimeZoneInfo ),
                    DateTimeOffset.Parse( written, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind ).UtcDateTime,
                    "the ban ends at a different moment than the banned role says" );
            }
        }

        [TestMethod]
        public void APersonBannedPermanentlyInOneRoleAndUntilADateInAnother_IsBannedPermanently()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var otherRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Helper", false );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Twice banned channel" );
                var personId = fixture.AddPerson( "TwiceBanned" );

                fixture.AddMember( channelGuid, personId, member => member.IsChatBanned = true );
                fixture.AddMember( channelGuid, personId, member =>
                {
                    member.GroupRoleId = otherRoleId;
                    member.IsChatBanned = true;
                    member.ChatBannedUntil = new DateTime( 2027, 1, 15, 9, 30, 0, DateTimeKind.Unspecified );
                } );

                var payload = fixture.Project();
                var membership = SingleMembership( payload, channelGuid );

                Assert.IsTrue( ( bool ) payload.Value( "members", membership, "is_banned" ), "a person banned in both roles is not banned" );
                Assert.AreEqual( JTokenType.Null, payload.Value( "members", membership, "ban_expires_at" ).Type,
                    "a permanent ban was shortened to the other role's expiry" );
            }
        }

        [TestMethod]
        public void EachCapabilityComesFromTheMembersRole_AndNeitherStandsInForTheOther()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var mentionRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Mentions everyone", false );
                var announceRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Announces", false );
                fixture.SetRoleCapabilitiesDirectly( mentionRoleId, true, false );
                fixture.SetRoleCapabilitiesDirectly( announceRoleId, false, true );

                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Capabilities channel" );
                var mentionerId = fixture.AddPerson( "Mentioner" );
                var announcerId = fixture.AddPerson( "Announcer" );

                fixture.AddMember( channelGuid, mentionerId, member => member.GroupRoleId = mentionRoleId );
                fixture.AddMember( channelGuid, announcerId, member => member.GroupRoleId = announceRoleId );

                var payload = fixture.Project();
                var mentioner = MembershipOf( payload, channelGuid, fixture.PrimaryAliasGuid( mentionerId ) );
                var announcer = MembershipOf( payload, channelGuid, fixture.PrimaryAliasGuid( announcerId ) );

                Assert.IsTrue( ( bool ) payload.Value( "members", mentioner, "can_mention_all" ), "a role that may mention everyone did not say so" );
                Assert.IsFalse( ( bool ) payload.Value( "members", mentioner, "can_post_announcements" ), "a role that may only mention everyone was let post announcements" );
                Assert.IsFalse( ( bool ) payload.Value( "members", announcer, "can_mention_all" ), "a role that may only post announcements was let mention everyone" );
                Assert.IsTrue( ( bool ) payload.Value( "members", announcer, "can_post_announcements" ), "a role that may post announcements did not say so" );
            }
        }

        [TestMethod]
        public void APersonInTwoRoles_TakesEachCapabilityEitherRoleGrants()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var mentionRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Mentions everyone", false );
                var announceRoleId = fixture.AddRole( fixture.SharedGroupTypeId, "Announces", false );
                fixture.SetRoleCapabilitiesDirectly( mentionRoleId, true, false );
                fixture.SetRoleCapabilitiesDirectly( announceRoleId, false, true );

                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Two capable roles channel" );
                var personId = fixture.AddPerson( "TwoCapableRoles" );

                fixture.AddMember( channelGuid, personId, member => member.GroupRoleId = mentionRoleId );
                fixture.AddMember( channelGuid, personId, member => member.GroupRoleId = announceRoleId );

                var payload = fixture.Project();
                var membership = SingleMembership( payload, channelGuid );

                Assert.IsTrue( ( bool ) payload.Value( "members", membership, "can_mention_all" ), "the role that may mention everyone was outvoted by the other role" );
                Assert.IsTrue( ( bool ) payload.Value( "members", membership, "can_post_announcements" ), "the role that may post announcements was outvoted by the other role" );
            }
        }

        [TestMethod]
        public void ARolesChangedCapabilities_AreOnTheNextReadingForEveryMemberOfIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var roleId = fixture.AddRole( fixture.SharedGroupTypeId, "Changing", false );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Changed role channel" );
                var firstId = fixture.AddPerson( "ChangedFirst" );
                var secondId = fixture.AddPerson( "ChangedSecond" );
                var aliases = new[] { fixture.PrimaryAliasGuid( firstId ), fixture.PrimaryAliasGuid( secondId ) };

                fixture.AddMember( channelGuid, firstId, member => member.GroupRoleId = roleId );
                fixture.AddMember( channelGuid, secondId, member => member.GroupRoleId = roleId );

                var before = fixture.Project();
                foreach ( var alias in aliases )
                {
                    var row = MembershipOf( before, channelGuid, alias );
                    Assert.IsFalse( ( bool ) before.Value( "members", row, "can_mention_all" ), "a new role may mention everyone before anyone said so" );
                    Assert.IsFalse( ( bool ) before.Value( "members", row, "can_post_announcements" ), "a new role may post announcements before anyone said so" );
                }

                fixture.SetRoleCapabilitiesDirectly( roleId, true, true );

                var granted = fixture.Project();
                foreach ( var alias in aliases )
                {
                    var row = MembershipOf( granted, channelGuid, alias );
                    Assert.IsTrue( ( bool ) granted.Value( "members", row, "can_mention_all" ), "a member of the role was left out of mentioning everyone after the role was given it" );
                    Assert.IsTrue( ( bool ) granted.Value( "members", row, "can_post_announcements" ), "a member of the role was left out of announcements after the role was given them" );
                }

                fixture.SetRoleCapabilitiesDirectly( roleId, false, false );

                var withdrawn = fixture.Project();
                foreach ( var alias in aliases )
                {
                    var row = MembershipOf( withdrawn, channelGuid, alias );
                    Assert.IsFalse( ( bool ) withdrawn.Value( "members", row, "can_mention_all" ), "a member kept mentioning everyone after the role lost it" );
                    Assert.IsFalse( ( bool ) withdrawn.Value( "members", row, "can_post_announcements" ), "a member kept posting announcements after the role lost it" );
                }
            }
        }

        [TestMethod]
        public void TheChatRolesRockShipsWith_StartWithTheCapabilitiesTheirChatRoleHad()
        {
            // The six roles Rock seeds on its two chat group types: an administrator and a
            // moderator, who held chat's elevated roles, and a member, who did not.
            var elevated = new[]
            {
                new Guid( "0F85C980-1A45-4FE2-BD2B-3E3A051FFDDF" ),
                new Guid( "78CA32C4-F34A-4056-AE42-D49BCD39B027" ),
                new Guid( "A04C6E1D-89EA-48D1-856F-E91F9AB6ACF3" ),
                new Guid( "E0BC063F-ECAA-4C4F-9A87-A4752030AED7" )
            };

            var ordinary = new[]
            {
                new Guid( "EC36D21C-FF61-4A4E-8164-5A1145ABBD85" ),
                new Guid( "E133E458-9785-4C59-B844-35E0C3B686D9" )
            };

            using ( var rockContext = new RockContext() )
            {
                var roles = rockContext.Database.SqlQuery<SeededRole>(
                    "SELECT [Guid], [ChatRole], [CanMentionAll], [CanPostAnnouncements] FROM [GroupTypeRole] "
                    + "WHERE [Guid] IN ( @p0, @p1, @p2, @p3, @p4, @p5 )",
                    elevated[0], elevated[1], elevated[2], elevated[3], ordinary[0], ordinary[1] )
                    .ToList()
                    .ToDictionary( r => r.Guid );

                Assert.AreEqual( 6, roles.Count, "the database is missing a chat role Rock seeds" );

                foreach ( var guid in elevated )
                {
                    Assert.AreNotEqual( 0, roles[guid].ChatRole, $"the seeded role {guid} was expected to hold an elevated chat role" );
                    Assert.IsTrue( roles[guid].CanMentionAll, $"the seeded role {guid}, a chat moderator or administrator, may not mention everyone" );
                    Assert.IsTrue( roles[guid].CanPostAnnouncements, $"the seeded role {guid}, a chat moderator or administrator, may not post announcements" );
                }

                foreach ( var guid in ordinary )
                {
                    Assert.AreEqual( 0, roles[guid].ChatRole, $"the seeded role {guid} was expected to hold the ordinary chat role" );
                    Assert.IsFalse( roles[guid].CanMentionAll, $"the seeded member role {guid} may mention everyone" );
                    Assert.IsFalse( roles[guid].CanPostAnnouncements, $"the seeded member role {guid} may post announcements" );
                }
            }
        }

        #region Support

        /// <summary>
        /// The columns of a seeded role this file reads.
        /// </summary>
        private sealed class SeededRole
        {
            public Guid Guid { get; set; }

            public int ChatRole { get; set; }

            public bool CanMentionAll { get; set; }

            public bool CanPostAnnouncements { get; set; }
        }

        /// <summary>
        /// The one membership row a channel carries for one person.
        /// </summary>
        private static JArray MembershipOf( ProjectedPayload payload, Guid channelGuid, Guid aliasGuid )
        {
            var channelText = ( string ) payload.Value( "channels", payload.Row( "channels", "channel_id", channelGuid ), "channel_id" );

            var rows = payload.Rows( "members" )
                .Where( r => string.Equals( ( string ) payload.Value( "members", r, "channel_id" ), channelText, StringComparison.Ordinal )
                    && Guid.Parse( ( string ) payload.Value( "members", r, "person_alias_guid" ) ) == aliasGuid )
                .ToList();

            Assert.AreEqual( 1, rows.Count,
                string.Format( "the channel carries {0} membership rows for one person, and the far side can store only one", rows.Count ) );

            return rows[0];
        }

        /// <summary>
        /// The one membership row a channel carries.
        /// </summary>
        private static JArray SingleMembership( ProjectedPayload payload, Guid channelGuid )
        {
            var channelText = ( string ) payload.Value( "channels", payload.Row( "channels", "channel_id", channelGuid ), "channel_id" );

            var rows = payload.Rows( "members" )
                .Where( r => string.Equals( ( string ) payload.Value( "members", r, "channel_id" ), channelText, StringComparison.Ordinal ) )
                .ToList();

            Assert.AreEqual( 1, rows.Count,
                string.Format( "the channel carries {0} membership rows for one person, and the far side can store only one", rows.Count ) );

            return rows[0];
        }

        #endregion Support
    }
}
