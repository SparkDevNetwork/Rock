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

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What a channel looks like by the time it reaches the chat platform.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Five settings on a direct message and one on every channel are forced rather than
    ///         read. The far side refuses a direct message that is public, always shown, search
    ///         indexed or campused, and refuses any channel with no name, and Rock stops an
    ///         administrator from doing none of those things. Without the forcing, one edited
    ///         direct message fails that church's whole submission on every cycle, for as long as
    ///         it takes somebody to find it.
    ///     </para>
    ///     <para>
    ///         An archived or deactivated channel is still sent. That is what makes turning chat off
    ///         archive a conversation rather than lose it: the channel keeps its row and loses its
    ///         reach.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class ProjectionChannelTests : DatabaseTestsBase
    {
        #region Direct message invariants

        [TestMethod]
        public void ADirectMessageEditedToBreakEveryInvariant_IsProjectedWithNoneOfThemBroken()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var campusId = FirstCampusId();

                var channelGuid = fixture.AddChannel( fixture.DirectMessageGroupTypeId, "Edited direct message", group =>
                {
                    group.IsChatChannelPublicOverride = true;
                    group.IsChatChannelAlwaysShownOverride = true;
                    group.IsChatSearchIndexedOverride = true;
                    group.CampusId = campusId;
                } );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the direct message was not projected at all" );
                Assert.AreEqual( "dm", ( string ) payload.Value( "channels", row, "channel_type" ) );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "is_public" ) );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "always_shown" ) );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "is_search_indexed" ) );
                Assert.AreEqual( Newtonsoft.Json.Linq.JTokenType.Null, payload.Value( "channels", row, "campus_id" ).Type,
                    "a direct message carried a campus, which the far side refuses" );
                Assert.AreEqual( true, ( bool ) payload.Value( "channels", row, "can_view_members" ),
                    "a direct message has no roster to hide" );
            }
        }

        [TestMethod]
        public void AChannelWithABlankName_IsProjectedWithAName()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                // Written straight to the column, because Rock's own validation refuses a group
                // with no name and the row this protects against is one that got there another way:
                // an import, a plugin, or a direct edit.
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Named on the way in" );
                fixture.SetChannelNameDirectly( channelGuid, "   " );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );

                var name = ( string ) payload.Value( "channels", row, "name" );

                Assert.IsFalse( string.IsNullOrWhiteSpace( name ),
                    "a channel with no name fails the whole submission rather than that one row" );
            }
        }

        #endregion Direct message invariants

        #region Archived and deactivated

        [TestMethod]
        public void ADeactivatedChannel_IsStillProjectedAndHasNoReach()
        {
            AssertChannelIsProjectedWithoutReach( group => group.IsActive = false );
        }

        [TestMethod]
        public void AnArchivedChannel_IsStillProjectedAndHasNoReach()
        {
            AssertChannelIsProjectedWithoutReach( group => group.IsArchived = true );
        }

        #endregion Archived and deactivated

        #region Support

        private static void AssertChannelIsProjectedWithoutReach( Action<Group> makeUnreachable )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                // Public, always shown and indexed on the way in, so the projection has something
                // to turn off rather than a set of values that were already false.
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Retired channel", group =>
                {
                    group.IsChatChannelPublicOverride = true;
                    group.IsChatChannelAlwaysShownOverride = true;
                    group.IsChatSearchIndexedOverride = true;
                } );

                fixture.EditChannel( channelGuid, makeUnreachable );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "a channel that went quiet was dropped, which loses its conversation instead of archiving it" );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "is_public" ) );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "always_shown" ) );
                Assert.AreEqual( false, ( bool ) payload.Value( "channels", row, "is_search_indexed" ) );
            }
        }

        private static int? FirstCampusId()
        {
            using ( var rockContext = new RockContext() )
            {
                return new CampusService( rockContext ).Queryable()
                    .OrderBy( c => c.Id )
                    .Select( c => ( int? ) c.Id )
                    .FirstOrDefault();
            }
        }

        #endregion Support
    }
}
