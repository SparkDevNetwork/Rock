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

using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// That a payload holds together on its own.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The far side keys a membership to both a channel and an alias, and refuses the whole
    ///         submission rather than that one row when either is missing. So a membership whose
    ///         channel or whose person is in no other section of the same payload does not cost one
    ///         row: it fails the church on all four tables, and every retry reproduces it.
    ///     </para>
    ///     <para>
    ///         This is why the qualifying set is staged once and every section reads that set rather
    ///         than the tables it came from. Read straight from the tables, the sections would be
    ///         four moments rather than one, and a group that starts qualifying, or a person who
    ///         joins and leaves, in between is exactly the row nothing else would catch. The second cell below changes the database
    ///         while the projection is reading. It cannot guarantee it lands in the window, so it is
    ///         a probe rather than a proof, and the assertion it makes is true of any payload.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ProjectionStagedSetTests : DatabaseTestsBase
    {
        [TestMethod]
        public void EveryMembershipNamesAChannelAndAnAliasThatAreInTheSamePayload()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Staged set channel" );
                var stayingPersonId = fixture.AddPerson( "Staying" );
                fixture.AddMember( channelGuid, stayingPersonId );

                var payload = fixture.Project();

                AssertPayloadHoldsTogether( payload );
            }
        }

        /// <summary>
        /// The same assertion against a database that changes while the projection runs. A group
        /// starts qualifying and a person joins and leaves, in the window the four sections span.
        /// </summary>
        [TestMethod]
        public void APayloadHoldsTogetherEvenWhileTheChurchIsChangingUnderIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var settledChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Settled channel" );
                var settledPersonId = fixture.AddPerson( "Settled" );
                fixture.AddMember( settledChannel, settledPersonId );

                // A group that is not a chat channel yet, and a person who is about to join it.
                var latecomerChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Latecomer channel",
                    group => group.IsChatEnabledOverride = false );
                var latecomerPersonId = fixture.AddPerson( "Latecomer" );

                var churn = new System.Threading.Thread( () =>
                {
                    // Chat is turned on for the group, and somebody joins it, while the projection
                    // is partway through its sections.
                    fixture.EditChannel( latecomerChannel, group => group.IsChatEnabledOverride = true );
                    fixture.AddMember( latecomerChannel, latecomerPersonId );
                } );

                churn.Start();
                var payload = fixture.Project();
                churn.Join();

                AssertPayloadHoldsTogether( payload );
            }
        }

        #region Support

        /// <summary>
        /// Every membership in this payload names a channel this payload carries, and an alias this
        /// payload carries.
        /// </summary>
        private static void AssertPayloadHoldsTogether( ProjectedPayload payload )
        {
            var channels = new HashSet<string>(
                payload.Rows( "channels" ).Select( r => ( string ) payload.Value( "channels", r, "channel_id" ) ),
                StringComparer.OrdinalIgnoreCase );

            var aliases = new HashSet<string>(
                payload.Rows( "aliases" ).Select( r => ( string ) payload.Value( "aliases", r, "person_alias_guid" ) ),
                StringComparer.OrdinalIgnoreCase );

            foreach ( var membership in payload.Rows( "members" ) )
            {
                var channelId = ( string ) payload.Value( "members", membership, "channel_id" );
                var aliasGuid = ( string ) payload.Value( "members", membership, "person_alias_guid" );

                Assert.IsTrue( channels.Contains( channelId ),
                    string.Format( "a membership names channel {0}, which is in no other section of this payload, so the far side refuses the whole submission", channelId ) );

                Assert.IsTrue( aliases.Contains( aliasGuid ),
                    string.Format( "a membership names alias {0}, which is in no other section of this payload, so the far side refuses the whole submission", aliasGuid ) );
            }
        }

        #endregion Support
    }
}
