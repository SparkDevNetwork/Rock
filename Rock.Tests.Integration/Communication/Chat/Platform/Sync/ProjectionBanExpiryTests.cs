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

using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// When a ban ends, said in a way the far side reads as the same moment.
    /// </summary>
    /// <remarks>
    /// Rock stores this in the organisation's own time zone, and the column it lands in on the far
    /// side is cast without one, which reads whatever it is given as UTC. A church outside UTC would
    /// have every ban expiry shifted by its own offset, and for a church behind UTC that lifts bans
    /// early. The organisation's zone is moved to one with a real offset for the length of this
    /// test, so the assertion is about a shift rather than about a machine that happened to be set
    /// to UTC.
    /// </remarks>
    [TestClass]
    public class ProjectionBanExpiryTests : DatabaseTestsBase
    {
        [TestMethod]
        public void ABanExpiry_TravelsWithAnExplicitOffsetAndNamesTheSameInstant()
        {
            var organizationZone = RockDateTime.OrgTimeZoneInfo;

            try
            {
                var eastern = TimeZoneInfo.FindSystemTimeZoneById( "Eastern Standard Time" );
                RockDateTime.Initialize( eastern );

                // A winter date, so the offset is the standard one and not the daylight one, which
                // keeps the arithmetic below independent of when this test is run.
                var expiresAt = new DateTime( 2027, 1, 15, 9, 30, 0, DateTimeKind.Unspecified );

                using ( var fixture = new ChatSyncProjectionFixture() )
                {
                    var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Ban expiry channel" );
                    var personId = fixture.AddPerson( "BanExpiry" );

                    fixture.AddMember( channelGuid, personId, member =>
                    {
                        member.IsChatBanned = true;
                        member.ChatBannedUntil = expiresAt;
                    } );

                    var payload = fixture.Project();
                    var channelText = payload.Value( "channels", payload.Row( "channels", "channel_id", channelGuid ), "channel_id" ).ToString();

                    var member = payload.Rows( "members" )
                        .FirstOrDefault( r => string.Equals( ( string ) payload.Value( "members", r, "channel_id" ), channelText, StringComparison.Ordinal ) );

                    Assert.IsNotNull( member, "the banned membership was not projected" );

                    var written = ( string ) payload.Value( "members", member, "ban_expires_at" );

                    Assert.IsNotNull( written, "the ban has no end on the wire, so the far side would treat it as permanent" );
                    Assert.IsTrue( written.EndsWith( "Z", StringComparison.Ordinal ) || written.Contains( "+" ) || written.LastIndexOf( '-' ) > 9,
                        string.Format( "'{0}' carries no offset, so the far side reads it in its own zone", written ) );

                    var parsed = DateTimeOffset.Parse( written, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );

                    Assert.AreEqual( TimeZoneInfo.ConvertTimeToUtc( expiresAt, eastern ), parsed.UtcDateTime,
                        "the ban ends at a different moment than the one Rock stored" );

                    Assert.AreNotEqual( expiresAt, parsed.UtcDateTime,
                        "the stored value was sent as though it were already UTC, which shifts every ban by this church's own offset" );
                }
            }
            finally
            {
                RockDateTime.Initialize( organizationZone );
            }
        }
    }
}
