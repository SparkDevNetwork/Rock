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

using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Communication.Chat.Platform.Doors;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// What a person merge leaves for chat, through the real merge procedure.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When two people in the same chat room are merged, the procedure keeps one of their
    ///         two memberships and deletes the other. A ban or a mute on the deleted one must not be
    ///         lost with it, whichever of the two the procedure keeps, or a merge would silently let
    ///         a banned person back in.
    ///     </para>
    ///     <para>
    ///         When each of the two people had a conversation with the same third person, both
    ///         conversations hold the same people once they are merged. Chat keeps the oldest, the
    ///         one the direct message door already reopens, and archives the rest, so the platform
    ///         moves their history into it.
    ///     </para>
    /// </remarks>
    [TestClass]
    public class PersonMergeTests : DatabaseTestsBase
    {
        #region Bans and mutes

        [TestMethod]
        public void ABanOnlyTheMergedAwayPersonHeldStaysOnTheMembershipTheProcedureKeeps()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge ban" );
                var survivorId = fixture.AddPerson( "Survivor" );
                var loserId = fixture.AddPerson( "Merged" );
                fixture.AddMember( channel, survivorId );
                fixture.AddMember( channel, loserId, m => m.IsChatBanned = true );

                Merge( loserId, survivorId );

                var kept = scene.Memberships( channel, survivorId ).Single();
                Assert.IsTrue( kept.IsChatBanned, "a ban on either membership survives the merge" );
                Assert.IsNull( kept.ChatBannedUntil, "and it is still open-ended" );
            }
        }

        [TestMethod]
        public void AnOpenEndedBanOutlastsADatedOneAndTheLaterOfTwoDatesWins()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                var openChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge open ban" );
                var datedChannel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge dated ban" );
                var survivorId = fixture.AddPerson( "Survivor" );
                var loserId = fixture.AddPerson( "Merged" );
                var earlier = RockDateTime.Now.AddDays( 3 ).Date;
                var later = RockDateTime.Now.AddDays( 30 ).Date;

                // The survivor's dated ban and the merged-away person's open one; then two dated
                // bans, the merged-away person's the later.
                fixture.AddMember( openChannel, survivorId, m => { m.IsChatBanned = true; m.ChatBannedUntil = earlier; } );
                fixture.AddMember( openChannel, loserId, m => m.IsChatBanned = true );
                fixture.AddMember( datedChannel, survivorId, m => { m.IsChatBanned = true; m.ChatBannedUntil = earlier; } );
                fixture.AddMember( datedChannel, loserId, m => { m.IsChatBanned = true; m.ChatBannedUntil = later; } );

                Merge( loserId, survivorId );

                Assert.IsNull( scene.Memberships( openChannel, survivorId ).Single().ChatBannedUntil,
                    "an open-ended ban on either membership leaves the kept one open-ended" );
                Assert.AreEqual( later, scene.Memberships( datedChannel, survivorId ).Single().ChatBannedUntil,
                    "of two dated bans the kept membership carries the later" );
            }
        }

        [TestMethod]
        public void ABanOnTheSurvivorsOwnMembershipSurvivesWhenTheProcedureKeepsTheOtherOne()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge kept other" );
                var survivorId = fixture.AddPerson( "Survivor" );
                var loserId = fixture.AddPerson( "Merged" );

                // The procedure keeps the more active membership, so here it keeps the merged-away
                // person's and deletes the survivor's banned, inactive one.
                fixture.AddMember( channel, survivorId, m => { m.GroupMemberStatus = GroupMemberStatus.Inactive; m.IsChatBanned = true; } );
                fixture.AddMember( channel, loserId );

                Merge( loserId, survivorId );

                var kept = scene.Memberships( channel, survivorId ).Single();
                Assert.AreEqual( GroupMemberStatus.Active, kept.GroupMemberStatus, "the procedure kept the active membership" );
                Assert.IsTrue( kept.IsChatBanned, "and the ban the survivor held is on it" );
            }
        }

        [TestMethod]
        public void AMuteOnlyTheMergedAwayPersonHeldStaysOnTheMembershipTheProcedureKeeps()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge mute" );
                var survivorId = fixture.AddPerson( "Survivor" );
                var loserId = fixture.AddPerson( "Merged" );
                fixture.AddMember( channel, survivorId );
                fixture.AddMember( channel, loserId, m => m.IsChatMuted = true );

                Merge( loserId, survivorId );

                Assert.IsTrue( scene.Memberships( channel, survivorId ).Single().IsChatMuted, "a mute on either membership survives the merge" );
            }
        }

        #endregion Bans and mutes

        #region Twin conversations

        [TestMethod]
        public void TwinConversationsLeaveOnlyTheOldestLive()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var survivorId = scene.AddChatPerson( "Survivor", isOpenDmAllowed: true );
                var loserId = scene.AddChatPerson( "Merged", isOpenDmAllowed: true );
                var otherId = scene.AddChatPerson( "Other", isOpenDmAllowed: true );
                var fourthId = scene.AddChatPerson( "Fourth", isOpenDmAllowed: true );

                // Oldest first: the merged-away person's conversation with the other person, the
                // survivor's, then a group of all three, which after the merge is the same two
                // people. The survivor's conversation with a fourth person has no twin.
                var oldest = scene.Start( otherId, scene.Alias( loserId ) ).ChannelGuid.Value;
                var survivors = scene.Start( otherId, scene.Alias( survivorId ) ).ChannelGuid.Value;
                var group = scene.Start( otherId, scene.Alias( loserId ), scene.Alias( survivorId ) ).ChannelGuid.Value;
                var unrelated = scene.Start( fourthId, scene.Alias( survivorId ) ).ChannelGuid.Value;

                Merge( loserId, survivorId );

                CollectionAssert.AreEqual( new List<Guid> { oldest }, scene.DirectMessagesWithExactly( survivorId, otherId ),
                    "one conversation of the two people is left, the oldest, which the door reopens" );
                Assert.IsTrue( scene.AnyGroup( survivors ).IsArchived, "the survivor's own twin is archived" );
                Assert.IsTrue( scene.AnyGroup( group ).IsArchived, "and so is the group that became the same two people" );
                Assert.IsFalse( scene.AnyGroup( unrelated ).IsArchived, "a conversation with no twin is left alone" );
            }
        }

        [TestMethod]
        public void TheMergePushNamesTheArchivedTwinsMembersAsGone()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var survivorId = scene.AddChatPerson( "Survivor", isOpenDmAllowed: true );
                var loserId = scene.AddChatPerson( "Merged", isOpenDmAllowed: true );
                var otherId = scene.AddChatPerson( "Other", isOpenDmAllowed: true );

                scene.Start( otherId, scene.Alias( loserId ) );
                var twin = scene.Start( otherId, scene.Alias( survivorId ) ).ChannelGuid.Value;
                var before = scene.WaitForPushes().Count;

                Merge( loserId, survivorId );

                var merge = scene.WaitForPushes().Skip( before ).ToList();
                Assert.AreEqual( 1, merge.Count, "the merge pushes once, after it commits" );
                // An archived group keeps its channel and loses its members, which is what tells the
                // platform the conversation is now empty and its history belongs to the one kept.
                CollectionAssert.IsSubsetOf(
                    new[] { (twin, scene.Alias( survivorId )), (twin, scene.Alias( otherId )) },
                    merge[0].AbsentMembers,
                    "every member of the archived twin is named gone in the push, at once" );
            }
        }

        #endregion Twin conversations

        #region Merge paths

        [TestMethod]
        public void ANamelessMergeTellsChatAboutTheSurvivor()
        {
            using ( var scene = new ChatDoorScene() )
            {
                var fixture = scene.Fixture;
                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Merge nameless" );
                var existingId = fixture.AddPerson( "Existing" );
                var namelessId = fixture.AddPerson( "Nameless" );
                fixture.AddMember( channel, existingId );
                fixture.AddMember( channel, namelessId );
                var namelessAlias = fixture.PrimaryAliasGuid( namelessId );
                MakeNameless( namelessId );
                var before = scene.WaitForPushes().Count;

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    personService.MergeNamelessPersonToExistingPerson( personService.Get( namelessId ), personService.Get( existingId ) );
                }

                var pushes = scene.WaitForPushes().Skip( before ).ToList();
                Assert.IsTrue(
                    pushes.Any( p => p.Aliases.Any( a => ( Guid ) a[0] == namelessAlias && ( Guid ) a[1] == fixture.PrimaryAliasGuid( existingId ) ) ),
                    "the nameless record's alias reaches chat under the person it was merged into, without waiting for the full sync" );
            }
        }

        #endregion Merge paths

        #region Support

        /// <summary>
        /// Merges one person into another as the Person Merge block does: the procedure and chat's
        /// record of the merge inside one transaction, inside a request.
        /// </summary>
        private static void Merge( int loserId, int survivorId )
        {
            using ( ChatSyncProjectionFixture.InsideRequest() )
            using ( var rockContext = new RockContext() )
            {
                rockContext.WrapTransaction( () =>
                {
                    rockContext.Database.ExecuteSqlCommand( "EXEC dbo.spCrm_PersonMerge @p0, @p1", loserId, survivorId );
                    ChatPlatformSyncHelper.RecordPersonMerge( rockContext, survivorId );
                } );
            }
        }

        /// <summary>
        /// Turns a person into a nameless record, which is how Rock holds a text message sender it
        /// cannot yet name: a mobile number and nothing else.
        /// </summary>
        private static void MakeNameless( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var namelessTypeId = Rock.Web.Cache.DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
                var mobileTypeId = Rock.Web.Cache.DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
                var person = new PersonService( rockContext ).Get( personId );

                person.RecordTypeValueId = namelessTypeId;
                person.PhoneNumbers.Add( new PhoneNumber { NumberTypeValueId = mobileTypeId, Number = "6235550100", IsMessagingEnabled = true } );
                rockContext.SaveChanges();
            }
        }

        #endregion Support
    }
}
