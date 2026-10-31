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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// A name Rock holds as blank never reaches the far side as blank.
    /// </summary>
    /// <remarks>
    /// The far side refuses an empty nick name, last name or badge name, and one refused row fails
    /// the whole church's submission on every cycle. Rock's own person save can store an empty nick
    /// name (a nick name that was only emoji is stripped after the blank one is filled), and an import
    /// or a direct edit can store any of the three. Each value here is written straight to its
    /// column, because that is how the rows this protects against get there.
    /// <para>
    /// A name is blank when nothing in it would show: only spaces, tabs and line breaks, or only the
    /// control characters the sync's writer takes out before the body is sent, which would otherwise
    /// leave an empty string on the wire. The projection and the writer must agree on that, so each
    /// value here goes through both.
    /// </para>
    /// </remarks>
    [TestClass]
    public class ProjectionBlankNameTests : DatabaseTestsBase
    {
        #region Person names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "\u0001\u0002" )]
        [DataRow( "\0" )]
        [DataRow( "\t" )]
        [DataRow( "\r\n" )]
        [DataRow( "\u0001 " )]
        public void ABlankNickName_TravelsAsNothing( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "BlankNick" );
                fixture.SetNickNameDirectly( personId, blank );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( JTokenType.Null, payload.Value( "aliases", row, "nick_name" ).Type,
                    "a blank nick name was sent, and the far side refuses the whole church over it" );
                Assert.AreEqual( "BlankNick", ( string ) payload.Value( "aliases", row, "last_name" ),
                    "the last name was lost along with the blank nick name" );
            }
        }

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "\u0001\u0002" )]
        [DataRow( "\0" )]
        [DataRow( "\t" )]
        [DataRow( "\r\n" )]
        [DataRow( "\u0001 " )]
        public void ABlankLastName_TravelsAsNothing( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "BlankLast" );
                fixture.SetLastNameDirectly( personId, blank );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( JTokenType.Null, payload.Value( "aliases", row, "last_name" ).Type,
                    "a blank last name was sent, and the far side refuses the whole church over it" );
                Assert.AreNotEqual( JTokenType.Null, payload.Value( "aliases", row, "nick_name" ).Type,
                    "the nick name was lost along with the blank last name" );
            }
        }

        /// <summary>
        /// Only a blank name is changed. Anything else is sent exactly as Rock holds it, spaces and all.
        /// </summary>
        [TestMethod]
        public void ANameWithSpacesAroundIt_TravelsAsRockHoldsIt()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var personId = EnrolledPerson( fixture, "Spaced" );
                fixture.SetNickNameDirectly( personId, " Ann " );

                var payload = fixture.Project();
                var row = PrimaryRow( payload, personId );

                Assert.AreEqual( " Ann ", ( string ) payload.Value( "aliases", row, "nick_name" ) );
            }
        }

        #endregion Person names

        #region Channel names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "\u0001\u0002" )]
        [DataRow( "\0" )]
        [DataRow( "\t" )]
        [DataRow( "\r\n" )]
        [DataRow( "\u0001 " )]
        public void AChannelWithABlankName_IsProjectedWithTheFallbackName( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Named on the way in" );
                fixture.SetChannelNameDirectly( channelGuid, blank );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.IsNotNull( row, "the channel was not projected at all" );

                var name = ( string ) payload.Value( "channels", row, "name" );

                Assert.IsTrue( name.StartsWith( "Channel " ),
                    "a channel with no visible name was sent as " + JsonConvert.ToString( name ) + ", and the far side refuses the whole church over it" );
            }
        }

        /// <summary>
        /// Only a blank name is replaced. A name with spaces at its edges loses them, as it always has.
        /// </summary>
        [TestMethod]
        public void AChannelNameWithSpacesAroundIt_TravelsTrimmed()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Named on the way in" );
                fixture.SetChannelNameDirectly( channelGuid, "  Youth  " );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.AreEqual( "Youth", ( string ) payload.Value( "channels", row, "name" ) );
            }
        }

        #endregion Channel names

        #region Group type names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "\u0001\u0002" )]
        [DataRow( "\0" )]
        [DataRow( "\t" )]
        [DataRow( "\r\n" )]
        [DataRow( "\u0001 " )]
        public void AGroupTypeWithABlankName_TravelsAsNothing( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Typed" );
                fixture.SetGroupTypeNameDirectly( fixture.SharedGroupTypeId, blank );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.AreEqual( JTokenType.Null, payload.Value( "channels", row, "group_type_name" ).Type,
                    "a group type name that shows nothing was sent as text" );
            }
        }

        /// <summary>
        /// Only a blank name is dropped. A name with spaces at its edges loses them, as it always has.
        /// </summary>
        [TestMethod]
        public void AGroupTypeNameWithSpacesAroundIt_TravelsTrimmed()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, "Typed" );
                fixture.SetGroupTypeNameDirectly( fixture.SharedGroupTypeId, "  Small groups  " );

                var payload = fixture.Project();
                var row = payload.Row( "channels", "channel_id", channelGuid );

                Assert.AreEqual( "Small groups", ( string ) payload.Value( "channels", row, "group_type_name" ) );
            }
        }

        #endregion Group type names

        #region Badge names

        [TestMethod]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "\u0001\u0002" )]
        [DataRow( "\0" )]
        [DataRow( "\t" )]
        [DataRow( "\r\n" )]
        [DataRow( "\u0001 " )]
        public void ABadgeWithABlankName_IsProjectedWithAName( string blank )
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var badgeGuid = fixture.AddBadge( "Named on the way in" );
                fixture.SetBadgeNameDirectly( badgeGuid, blank );

                var payload = fixture.Project();
                var row = payload.Row( "badges", "badge_key", badgeGuid );

                Assert.IsNotNull( row, "the badge was not projected at all" );
                Assert.IsFalse( string.IsNullOrWhiteSpace( ( string ) payload.Value( "badges", row, "name" ) ),
                    "a badge with no name fails the whole submission rather than that one row" );
            }
        }

        #endregion Badge names

        #region Support

        private static int EnrolledPerson( ChatSyncProjectionFixture fixture, string lastName )
        {
            var channelGuid = fixture.AddChannel( fixture.SharedGroupTypeId, lastName + " channel" );
            var personId = fixture.AddPerson( lastName );
            fixture.AddMember( channelGuid, personId );

            return personId;
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
