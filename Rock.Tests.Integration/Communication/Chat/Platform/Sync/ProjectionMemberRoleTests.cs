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

using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// One membership row for a person in a channel, however many roles they hold in its group.
    /// </summary>
    /// <remarks>
    /// Rock lets one person hold several roles in one group, each its own group member. The far side
    /// keys a membership by channel and alias, so two rows for the same pair cannot both be stored,
    /// and the whole church's submission fails on every cycle over it.
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

        #region Support

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
