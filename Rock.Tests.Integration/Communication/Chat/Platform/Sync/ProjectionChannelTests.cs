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

        #region Icon

        [TestMethod]
        public void AChannelIcon_IsLinked()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Channel with an icon" );
                var iconGuid = fixture.SetChannelIcon( channelGuid, false );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );
                StringAssert.Contains( ( string ) payload.Value( "channels", row, "icon_url" ), iconGuid.ToString(),
                    "a channel icon anyone may view was not linked" );
            }
        }

        [TestMethod]
        public void AChannelIconWhoseFileTypeRequiresViewSecurity_IsNotLinked()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Channel with a secured icon" );
                fixture.SetChannelIcon( channelGuid, true );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );
                Assert.AreEqual( Newtonsoft.Json.Linq.JTokenType.Null, payload.Value( "channels", row, "icon_url" ).Type,
                    "an icon behind view security was linked, and the far side serves that link to every member of the channel" );
            }
        }

        #endregion Icon

        #region Description and group type

        [TestMethod]
        public void AChannelDescription_IsSentWithoutTheSpacesAndLineBreaksAtItsEdges()
        {
            var description = ProjectedDescription( "\r\n  Where the youth team plans each week.\n\t " );

            Assert.AreEqual( "Where the youth team plans each week.", description,
                "a description typed into a text box keeps its edges, and the far side shows them" );
        }

        [TestMethod]
        public void AChannelDescriptionPast500Characters_IsCutTo500()
        {
            var description = ProjectedDescription( new string( 'a', 600 ) );

            Assert.AreEqual( new string( 'a', 500 ), description,
                "a description longer than the far side takes fails the church's whole submission" );
        }

        [TestMethod]
        public void AChannelDescription_IsCutShortOfACharacterThatTakesTwoUnitsRatherThanThroughIt()
        {
            // The 500th unit is the first half of the emoji, so a cut at 500 would leave half a
            // character the far side cannot read.
            var description = ProjectedDescription( new string( 'a', 499 ) + "\U0001F600" + "bbb" );

            Assert.AreEqual( new string( 'a', 499 ), description );
        }

        [TestMethod]
        public void AChannelDescriptionOfOnlyWhitespace_IsSentAsNone()
        {
            Assert.IsNull( ProjectedDescription( "  \r\n\t " ), "the far side refuses a blank description" );
        }

        [TestMethod]
        public void AChannelWithNoDescription_IsSentWithNone()
        {
            Assert.IsNull( ProjectedDescription( null ) );
        }

        [TestMethod]
        public void AChannel_CarriesItsGroupTypesName()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Typed channel" );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );
                Assert.AreEqual( GroupTypeName( fixture.SharedGroupTypeId ), ( string ) payload.Value( "channels", row, "group_type_name" ) );
            }
        }

        [TestMethod]
        public void ADirectMessage_CarriesNoDescriptionAndNoGroupTypeName()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.DirectMessageGroupTypeId, "Described direct message", group =>
                {
                    group.Description = "A description set on a direct message";
                } );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the direct message was not projected at all" );
                Assert.AreEqual( Newtonsoft.Json.Linq.JTokenType.Null, payload.Value( "channels", row, "description" ).Type,
                    "a direct message is titled by its people and drawn with no description" );
                Assert.AreEqual( Newtonsoft.Json.Linq.JTokenType.Null, payload.Value( "channels", row, "group_type_name" ).Type,
                    "a direct message's type is Rock's own, not a name anyone chose to show" );
            }
        }

        #endregion Description and group type

        #region Support

        /// <summary>
        /// The description a shared channel with the given one is projected with.
        /// </summary>
        /// <param name="groupDescription">The group's description as Rock stores it.</param>
        /// <returns>The description on the wire, or null when it carries none.</returns>
        private static string ProjectedDescription( string groupDescription )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Described channel", group =>
                {
                    group.Description = groupDescription;
                } );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );

                return ( string ) payload.Value( "channels", row, "description" );
            }
        }

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

        /// <summary>
        /// A group type's name as Rock stores it.
        /// </summary>
        /// <param name="groupTypeId">The type.</param>
        /// <returns>Its name.</returns>
        internal static string GroupTypeName( int groupTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupTypeService( rockContext ).Get( groupTypeId ).Name;
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
