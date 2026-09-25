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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// A badge Data View that Rock counts as persisted is sent, and its members carry its key.
    /// </summary>
    /// <remarks>
    /// Rock persists a Data View either on an interval or on a named schedule, and the picker on
    /// Chat Configuration offers both. A badge that is dropped on the way out is not refused by
    /// anything: the church syncs, and the badge simply never appears.
    /// </remarks>
    [TestClass]
    public class ProjectionBadgeTests : DatabaseTestsBase
    {
        #region Persistence

        [TestMethod]
        public void ABadgePersistedOnAnInterval_IsProjectedAndCarried()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddBadge( "Interval badge" );

                AssertProjectedAndCarried( fixture, badgeGuid, "IntervalBadge" );
            }
        }

        [TestMethod]
        public void ABadgePersistedOnANamedSchedule_IsProjectedAndCarried()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddBadgeOnSchedule( "Scheduled badge" );

                AssertProjectedAndCarried( fixture, badgeGuid, "ScheduledBadge" );
            }
        }

        #endregion Persistence

        #region The badge list as stored

        [TestMethod]
        public void ABadgeListedTwice_IsProjectedOnceAndCarriedOnce()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddBadge( "Repeated badge" );
                fixture.RepeatBadge( badgeGuid );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Repeated badge channel" );
                var personId = fixture.AddPerson( "RepeatedBadge" );
                fixture.AddMember( channelGuid, personId );
                fixture.GiveBadge( badgeGuid, personId );

                var payload = fixture.Project();

                var keyIndex = payload.ColumnIndex( "badges", "badge_key" );
                var badgeRows = payload.Rows( "badges" )
                    .Count( r => string.Equals( ( string ) r[keyIndex], badgeGuid.ToString(), StringComparison.OrdinalIgnoreCase ) );

                Assert.AreEqual( 1, badgeRows, "the badge was not projected exactly once" );

                var badgeKeys = ( JArray ) payload.Value( "aliases", PrimaryRow( payload, personId ), "badge_keys" );

                Assert.AreEqual( 1, badgeKeys.Count( k => string.Equals( ( string ) k, badgeGuid.ToString(), StringComparison.OrdinalIgnoreCase ) ),
                    "the person does not carry the badge's key exactly once" );
            }
        }

        [TestMethod]
        public void AGroupDataViewOnTheBadgeList_IsNotProjectedAndNobodyCarriesIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddGroupDataViewBadge( "Group badge" );
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Group badge channel" );
                var personId = fixture.AddPerson( "GroupBadge" );
                fixture.AddMember( channelGuid, personId );

                // A group whose id happens to equal this person's id.
                fixture.GiveBadge( badgeGuid, personId );

                var payload = fixture.Project();

                Assert.IsNull( payload.Row( "badges", "badge_key", badgeGuid ), "a Data View of groups was projected as a badge" );

                var badgeKeys = payload.Value( "aliases", PrimaryRow( payload, personId ), "badge_keys" ) as JArray;

                Assert.IsFalse( badgeKeys != null && badgeKeys.Any( k => string.Equals( ( string ) k, badgeGuid.ToString(), StringComparison.OrdinalIgnoreCase ) ),
                    "a person carries a badge because their id equals a group's id" );
                Assert.AreEqual( "Badge 'Group badge' was skipped: its Data View does not list people.", payload.Result.BadgeWarning );
            }
        }

        #endregion The badge list as stored

        #region Badges left out

        /// <summary>
        /// A badge whose Data View is not persisted cannot be sent, and the run says so by name
        /// rather than leaving the badge to quietly never appear.
        /// </summary>
        [TestMethod]
        public void ABadgeWhoseDataViewIsNotPersisted_IsNamedInTheWarning()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddUnpersistedBadge( "Unpersisted badge" );

                var payload = fixture.Project();

                Assert.IsNull( payload.Row( "badges", "badge_key", badgeGuid ), "a badge with no persisted holders was projected" );
                Assert.AreEqual( "Badge 'Unpersisted badge' was skipped: its Data View is not persisted.", payload.Result.BadgeWarning );
            }
        }

        [TestMethod]
        public void ABadgeWhoseDataViewNoLongerExists_IsNamedInTheWarning()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddMissingBadge();

                var payload = fixture.Project();

                Assert.AreEqual( "Badge '" + badgeGuid + "' was skipped: its Data View no longer exists.", payload.Result.BadgeWarning );
            }
        }

        #endregion Badges left out

        #region Support

        private static void AssertProjectedAndCarried( ChatSyncProjectionFixture fixture, Guid badgeGuid, string lastName )
        {
            var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, lastName + " channel" );
            var personId = fixture.AddPerson( lastName );
            fixture.AddMember( channelGuid, personId );
            fixture.GiveBadge( badgeGuid, personId );

            var payload = fixture.Project();

            Assert.IsNotNull( payload.Row( "badges", "badge_key", badgeGuid ), "the badge was not projected" );
            Assert.IsNull( payload.Result.BadgeWarning, "a badge that was sent was warned about" );

            var badgeKeys = ( JArray ) payload.Value( "aliases", PrimaryRow( payload, personId ), "badge_keys" );

            Assert.IsTrue( badgeKeys.Any( k => string.Equals( ( string ) k, badgeGuid.ToString(), StringComparison.OrdinalIgnoreCase ) ),
                "the person in the badge's Data View does not carry its key" );
        }

        private static JArray PrimaryRow( ProjectedPayload payload, int personId )
        {
            Guid primaryAliasGuid;

            using ( var rockContext = new RockContext() )
            {
                primaryAliasGuid = new PersonService( rockContext ).Queryable()
                    .Where( p => p.Id == personId )
                    .Select( p => p.Aliases.FirstOrDefault( a => a.Id == p.PrimaryAliasId ).Guid )
                    .First();
            }

            var row = payload.Row( "aliases", "person_alias_guid", primaryAliasGuid );

            Assert.IsNotNull( row, "the person's primary alias was not projected" );

            return row;
        }

        #endregion Support
    }
}
