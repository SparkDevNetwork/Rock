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

using Rock.Communication.Chat.Platform.Doors;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The door behind a profile card's contact details and the church's template: whom it answers
    /// for, what it reads from Rock, and what of a template's output reaches another person's page.
    /// </summary>
    /// <remarks>
    /// The browser names a person and nothing else, so each case is one a person could ask for by
    /// passing someone's alias guid. The scene's church shows details by default.
    /// </remarks>
    [TestClass]
    public class ChatProfileDoorTests : DatabaseTestsBase
    {
        #region Whom it answers for

        [TestMethod]
        public void AViewerChatRefusesIsToldNothing()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                scene.AddToBanList( viewer );

                AssertNothingShown( Details( scene, viewer, scene.Alias( target ) ) );
            }
        }

        [TestMethod]
        public void NobodySignedInIsToldNothing()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var target = AddReachablePerson( scene, "Target" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AssertNothingShown( ChatDoorHelper.GetProfileDetails( null, scene.Alias( target ), scene.Configuration, null, rockContext ) );
                }
            }
        }

        [TestMethod]
        public void SomeoneShowingDetailsIsShown()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );

                var details = Details( scene, viewer, scene.Alias( target ) );

                Assert.IsTrue( details.IsShown );
                Assert.AreEqual( "target@example.org", details.Email );
                StringAssert.Contains( details.MobilePhone, "555-0142" );
                Assert.AreEqual( "tel:+16025550142", details.MobilePhoneUri );
            }
        }

        [TestMethod]
        public void SomeoneWhoTurnedDetailsOffIsNotShownWhateverTheChurchSays()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                SetProfilePublic( target, false );

                AssertNothingShown( Details( scene, viewer, scene.Alias( target ) ) );
            }
        }

        [TestMethod]
        public void SomeoneWhoNeverChoseFollowsTheChurchBothWays()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                SetProfilePublic( target, null );

                Assert.IsTrue( Details( scene, viewer, scene.Alias( target ) ).IsShown, "the church shows details by default" );

                scene.Configuration.AreChatProfilesVisible = false;
                AssertNothingShown( Details( scene, viewer, scene.Alias( target ) ) );

                SetProfilePublic( target, true );
                Assert.IsTrue( Details( scene, viewer, scene.Alias( target ) ).IsShown, "their own choice wins over the church's" );
            }
        }

        [TestMethod]
        public void SomeoneChatHasNeverSeenIsNotShown()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );

                // In Rock, with contact details, and in no group chat reads.
                var stranger = scene.Fixture.AddPerson( "Stranger" );
                GiveContactDetails( stranger );

                AssertNothingShown( Details( scene, viewer, scene.Alias( stranger ) ) );
            }
        }

        [TestMethod]
        public void SomeoneChatRefusesIsNotShown()
        {
            using ( var scene = new ChatDoorScene( minimumAge: 13 ) )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                scene.SetAge( viewer, 30 );

                var banned = AddReachablePerson( scene, "Banned" );
                scene.SetAge( banned, 30 );
                scene.AddToBanList( banned );

                var deceased = AddReachablePerson( scene, "Deceased" );
                scene.SetAge( deceased, 30 );
                scene.MakeDeceased( deceased );

                var inactive = AddReachablePerson( scene, "Inactive" );
                scene.SetAge( inactive, 30 );
                scene.MakeInactive( inactive );

                var young = AddReachablePerson( scene, "Young" );
                scene.SetAge( young, 12 );

                // Chat cannot tell how old someone with no birthdate is, so it shows them to nobody.
                var unknownAge = AddReachablePerson( scene, "UnknownAge" );

                var adult = AddReachablePerson( scene, "Adult" );
                scene.SetAge( adult, 30 );

                AssertNothingShown( Details( scene, viewer, scene.Alias( banned ) ), "on the Ban List" );
                AssertNothingShown( Details( scene, viewer, scene.Alias( deceased ) ), "deceased" );
                AssertNothingShown( Details( scene, viewer, scene.Alias( inactive ) ), "inactive" );
                AssertNothingShown( Details( scene, viewer, scene.Alias( young ) ), "under the church's minimum age" );
                AssertNothingShown( Details( scene, viewer, scene.Alias( unknownAge ) ), "no birthdate where there is a minimum age" );
                Assert.IsTrue( Details( scene, viewer, scene.Alias( adult ) ).IsShown, "an adult in the same scene is shown" );
            }
        }

        [TestMethod]
        public void AnAliasMergedAwayFindsThePersonItBelongsToNow()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                var mergedAlias = scene.Fixture.AddExtraAlias( target );

                var details = Details( scene, viewer, mergedAlias );

                Assert.IsTrue( details.IsShown );
                Assert.AreEqual( "target@example.org", details.Email );
            }
        }

        [TestMethod]
        public void AnAliasThatIsNobodysIsToldNothing()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );

                AssertNothingShown( Details( scene, viewer, Guid.NewGuid() ) );
            }
        }

        [TestMethod]
        public void OneViewerReadsOnlySoManyDifferentPeopleAMinute()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );

                // Names that are nobody count as people read, so guessing is bounded too.
                for ( var i = 0; i < ChatDoorHelper.MaxProfilesReadPerMinute - 1; i++ )
                {
                    Details( scene, viewer, Guid.NewGuid() );
                }

                Assert.IsTrue( Details( scene, viewer, scene.Alias( target ) ).IsShown, "the last person allowed" );
                Assert.IsTrue( Details( scene, viewer, scene.Alias( target ) ).IsShown, "the same person again" );

                var another = AddReachablePerson( scene, "Another" );
                AssertNothingShown( Details( scene, viewer, scene.Alias( another ) ), "one more person in the same minute" );

                var otherViewer = scene.AddChatPerson( "OtherViewer" );
                Assert.IsTrue( Details( scene, otherViewer, scene.Alias( another ) ).IsShown, "another viewer has a minute of their own" );
            }
        }

        #endregion Whom it answers for

        #region What it reads

        [TestMethod]
        public void AnUnlistedMobileIsLeftOutAndAHomePhoneIsNeverShown()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = scene.AddChatPerson( "Target" );
                SetEmail( target, "target@example.org", isActive: true, EmailPreference.EmailAllowed );
                AddPhone( target, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE, "6025550142", isUnlisted: true );
                AddPhone( target, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME, "6025550199", isUnlisted: false );

                var details = Details( scene, viewer, scene.Alias( target ) );

                Assert.IsTrue( details.IsShown );
                Assert.IsNull( details.MobilePhone );
                Assert.IsNull( details.MobilePhoneUri );
            }
        }

        [TestMethod]
        public void AnEmailRockKnowsIsDeadIsLeftOutAndDoNotEmailIsStillShown()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );

                var dead = scene.AddChatPerson( "Dead" );
                SetEmail( dead, "dead@example.org", isActive: false, EmailPreference.EmailAllowed );

                var doNotEmail = scene.AddChatPerson( "DoNotEmail" );
                SetEmail( doNotEmail, "quiet@example.org", isActive: true, EmailPreference.DoNotEmail );

                Assert.IsNull( Details( scene, viewer, scene.Alias( dead ) ).Email );
                Assert.AreEqual( "quiet@example.org", Details( scene, viewer, scene.Alias( doNotEmail ) ).Email, "do not email governs bulk mail, not being reached one to one" );
            }
        }

        #endregion What it reads

        #region The church's template

        [TestMethod]
        public void TheTemplateSeesThePersonOnTheCardAndThePersonLooking()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                scene.Configuration.ProfileCardLavaTemplate = "<p>{{ Person.LastName }} for {{ CurrentPerson.LastName }}</p>";

                Assert.AreEqual( "<p>Target for Viewer</p>", Details( scene, viewer, scene.Alias( target ) ).LavaHtml );
            }
        }

        [TestMethod]
        public void NoTemplateRunsForSomeoneNotShown()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                SetProfilePublic( target, false );
                scene.Configuration.ProfileCardLavaTemplate = "<p>{{ Person.LastName }}</p>";

                Assert.IsNull( Details( scene, viewer, scene.Alias( target ) ).LavaHtml );
            }
        }

        [TestMethod]
        public void ACommandRunsOnlyWhenTheChurchEnabledIt()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                scene.Configuration.ProfileCardLavaTemplate = "{% sql %}SELECT 'RAN' + 'MARKER' AS [V]{% endsql %}{% for row in results %}<b>{{ row.V }}</b>{% endfor %}";

                var refused = Details( scene, viewer, scene.Alias( target ) ).LavaHtml ?? string.Empty;
                Assert.IsFalse( refused.Contains( "RANMARKER" ), refused );

                scene.Configuration.ProfileCardEnabledLavaCommands = new List<string> { "Sql" };
                Assert.AreEqual( "<b>RANMARKER</b>", Details( scene, viewer, scene.Alias( target ) ).LavaHtml );
            }
        }

        [TestMethod]
        public void ABrokenTemplateShowsNoErrorAndTheRestStillShows()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                scene.Configuration.ProfileCardLavaTemplate = "{% if %}<p>never</p>";

                var details = Details( scene, viewer, scene.Alias( target ) );

                Assert.IsTrue( details.IsShown );
                Assert.AreEqual( "target@example.org", details.Email );
                Assert.IsNull( details.LavaHtml, "an error's text is for the template's author, not everyone who opens a card" );
            }
        }

        [TestMethod]
        public void APersonsOwnMarkupCannotScriptThePeopleWhoOpenTheirCard()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var viewer = scene.AddChatPerson( "Viewer" );
                var target = AddReachablePerson( scene, "Target" );
                // Short enough for the column, and a script link a blocklist would read as plain text.
                scene.Fixture.SetNickNameDirectly( target, "<a href=java&#9;script:x onclick=y>Ann</a>" );
                scene.Configuration.ProfileCardLavaTemplate = "<p>{{ Person.NickName }}</p>";

                Assert.AreEqual( "<p><a rel=\"noopener noreferrer\">Ann</a></p>", Details( scene, viewer, scene.Alias( target ) ).LavaHtml );
            }
        }

        #endregion The church's template

        #region Who chat has seen

        [TestMethod]
        public void WhoChatHasSeenAgreesWithTheProjectionOnEveryArm()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                fixture.EnsureChatPeopleGroup();

                // The marker counts in any state; enrolment is never withdrawn.
                var marked = fixture.AddPerson( "Marked" );
                fixture.AddMember( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE.AsGuid(), marked, m => m.GroupMemberStatus = GroupMemberStatus.Inactive );

                var administrator = fixture.AddPerson( "Administrator" );
                scene.MakeChatAdministrator( administrator );

                var inactiveAdministrator = fixture.AddPerson( "InactiveAdministrator" );
                fixture.AddMember( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS.AsGuid(), inactiveAdministrator, m => m.GroupMemberStatus = GroupMemberStatus.Inactive );

                var banned = fixture.AddPerson( "Banned" );
                scene.AddToBanList( banned );

                // Chat is on for the group's type and the group has never been marked a channel.
                var unmarkedRoom = fixture.AddChannel( fixture.SharedGroupTypeId, "Unmarked" );
                ClearChannelMark( unmarkedRoom );
                var inUnmarkedRoom = fixture.AddPerson( "InUnmarkedRoom" );
                fixture.AddMember( unmarkedRoom, inUnmarkedRoom );

                var archivedMember = fixture.AddPerson( "ArchivedMember" );
                fixture.AddMember( unmarkedRoom, archivedMember, m => m.IsArchived = true );

                // A group chat is off for, never marked.
                var offRoom = fixture.AddChannel( fixture.SharedGroupTypeId, "Off", g => g.IsChatEnabledOverride = false );
                ClearChannelMark( offRoom );
                var inOffRoom = fixture.AddPerson( "InOffRoom" );
                fixture.AddMember( offRoom, inOffRoom );

                var nobody = fixture.AddPerson( "Nobody" );

                var people = new[] { marked, administrator, inactiveAdministrator, banned, inUnmarkedRoom, archivedMember, inOffRoom, nobody };
                var expected = new Dictionary<int, bool>
                {
                    [marked] = true,
                    [administrator] = true,
                    [inactiveAdministrator] = false,
                    [banned] = true,
                    [inUnmarkedRoom] = true,
                    [archivedMember] = false,
                    [inOffRoom] = false,
                    [nobody] = false
                };

                // Asked before the projection runs, since a run marks the groups that are channels.
                Dictionary<int, bool> answered;
                using ( var rockContext = new RockContext() )
                {
                    answered = people.ToDictionary( id => id, id => ChatDoorHelper.IsEnrolledInChat( id, rockContext ) );
                }

                var payload = fixture.Project();

                foreach ( var id in people )
                {
                    var projected = payload.Row( "aliases", "person_alias_guid", scene.Alias( id ) ) != null;
                    Assert.AreEqual( expected[id], answered[id], $"the door on {scene.Name( id )}" );
                    Assert.AreEqual( projected, answered[id], $"the door and the projection on {scene.Name( id )}" );
                }
            }
        }

        #endregion Who chat has seen

        #region Helpers

        private static ChatProfileDetailsBag Details( ChatDoorScene scene, int viewerId, Guid personAliasGuid )
        {
            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                var viewer = new PersonService( rockContext ).Get( viewerId );
                var mergeFields = LavaHelper.GetCommonMergeFields( null, viewer );

                return ChatDoorHelper.GetProfileDetails( viewer, personAliasGuid, scene.Configuration, mergeFields, rockContext );
            }
        }

        private static void AssertNothingShown( ChatProfileDetailsBag details, string because = null )
        {
            Assert.IsFalse( details.IsShown, because );
            Assert.IsNull( details.Email, because );
            Assert.IsNull( details.MobilePhone, because );
            Assert.IsNull( details.MobilePhoneUri, because );
            Assert.IsNull( details.LavaHtml, because );
        }

        /// <summary>
        /// A chat person with an email and a listed mobile phone.
        /// </summary>
        private static int AddReachablePerson( ChatDoorScene scene, string lastName )
        {
            var personId = scene.AddChatPerson( lastName );
            GiveContactDetails( personId );
            return personId;
        }

        private static void GiveContactDetails( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var lastName = new PersonService( rockContext ).Get( personId ).LastName.ToLowerInvariant();
                SetEmail( personId, lastName + "@example.org", isActive: true, EmailPreference.EmailAllowed );
            }

            AddPhone( personId, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE, "6025550142", isUnlisted: false );
        }

        private static void SetEmail( int personId, string email, bool isActive, EmailPreference preference )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "UPDATE [Person] SET [Email] = @p1, [IsEmailActive] = @p2, [EmailPreference] = @p3 WHERE [Id] = @p0", personId, email, isActive, ( int ) preference );
            }
        }

        private static void SetProfilePublic( int personId, bool? isPublic )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand(
                    "UPDATE [Person] SET [IsChatProfilePublic] = @p1 WHERE [Id] = @p0", personId, ( object ) isPublic ?? DBNull.Value );
            }
        }

        private static void AddPhone( int personId, string typeGuid, string number, bool isUnlisted )
        {
            using ( var rockContext = new RockContext() )
            {
                new PhoneNumberService( rockContext ).Add( new PhoneNumber
                {
                    PersonId = personId,
                    NumberTypeValueId = DefinedValueCache.GetId( typeGuid.AsGuid() ),
                    CountryCode = "1",
                    Number = number,
                    IsUnlisted = isUnlisted
                } );
                rockContext.SaveChanges();
            }
        }

        private static void ClearChannelMark( Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "UPDATE [Group] SET [ChatChannelFirstEnabledDateTime] = NULL WHERE [Guid] = @p0", groupGuid );
            }
        }

        #endregion Helpers
    }
}
