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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Blocks;
using Rock.Blocks.Group;
using Rock.Communication.Chat;
using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Blocks
{
    /// <summary>
    /// Group Member Detail's chat settings: a ban, open-ended or until a date and time, shown and
    /// saved for a church on the chat platform, and Chat Muted only where the provider Rock shipped
    /// before the platform still reads it.
    /// </summary>
    /// <remarks>
    /// These drive the block's own load and save, because the copy between the member and its bag
    /// is written out by hand one property at a time, and the rule for showing the section is asked
    /// twice, once to build the screen and once to save it: a section shown but not saved would
    /// take a ban and drop it.
    /// </remarks>
    [TestClass]
    public class GroupMemberDetailChatBanTests : DatabaseTestsBase
    {
        #region Load

        [TestMethod]
        public void AChurchOnThePlatformAloneSeesTheBanAndItsEndButNotChatMuted()
        {
            using ( var scene = new BanScene( isPlatformConfigured: true, isPreviousProviderConfigured: false ) )
            {
                var until = new DateTime( 2027, 1, 15, 9, 30, 0 );
                var memberId = scene.AddMember( m => { m.IsChatBanned = true; m.ChatBannedUntil = until; } );

                var box = scene.Load( memberId );

                Assert.IsTrue( box.Options.AreChatPreferencesShown, "a church on the platform alone can ban" );
                Assert.IsFalse( box.Options.IsChatMutedShown, "the platform has no mute, so the box would do nothing" );
                Assert.IsTrue( box.Entity.IsChatBanned );
                Assert.AreEqual( until, box.Entity.ChatBannedUntil?.DateTime, "the end is shown as stored, in the organization's time" );
            }
        }

        [TestMethod]
        public void ChatMutedIsShownWhileThePreviousProviderIsOnAndNothingIsShownWithNeither()
        {
            using ( var scene = new BanScene( isPlatformConfigured: false, isPreviousProviderConfigured: true ) )
            {
                var box = scene.Load( scene.AddMember() );

                Assert.IsTrue( box.Options.AreChatPreferencesShown );
                Assert.IsTrue( box.Options.IsChatMutedShown, "the previous provider reads Chat Muted" );
            }

            using ( var scene = new BanScene( isPlatformConfigured: false, isPreviousProviderConfigured: false ) )
            {
                Assert.IsFalse( scene.Load( scene.AddMember() ).Options.AreChatPreferencesShown, "with no chat at all there is nothing to set" );
            }
        }

        #endregion Load

        #region Save

        [TestMethod]
        public void ABanUntilADateAndTimeIsSavedInTheOrganizationsTime()
        {
            using ( var scene = new BanScene( isPlatformConfigured: true, isPreviousProviderConfigured: false ) )
            {
                var memberId = scene.AddMember();
                var until = RockDateTime.Now.AddDays( 2 ).Date.AddHours( 17 );

                var save = scene.Save( memberId, bag =>
                {
                    bag.IsChatBanned = true;
                    bag.ChatBannedUntil = until.ToRockDateTimeOffset();
                } );

                Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                var stored = scene.Stored( memberId );
                Assert.IsTrue( stored.IsChatBanned, "the ban is saved for a church on the platform alone" );
                Assert.AreEqual( until, stored.ChatBannedUntil, "and its end, as the organization's clock reads it" );
            }
        }

        [TestMethod]
        public void UntickingChatBannedClearsItsEndSoAnOldDateNeverComesBackWithANewBan()
        {
            using ( var scene = new BanScene( isPlatformConfigured: true, isPreviousProviderConfigured: false ) )
            {
                var memberId = scene.AddMember( m => { m.IsChatBanned = true; m.ChatBannedUntil = RockDateTime.Now.AddDays( 5 ); } );

                var save = scene.Save( memberId, bag => bag.IsChatBanned = false );

                Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                var stored = scene.Stored( memberId );
                Assert.IsFalse( stored.IsChatBanned );
                Assert.IsNull( stored.ChatBannedUntil, "a lifted ban keeps no end" );
            }
        }

        [TestMethod]
        public void AnEndAlreadyPastIsRefusedAndAnOldEndLeftAsItWasIsNot()
        {
            using ( var scene = new BanScene( isPlatformConfigured: true, isPreviousProviderConfigured: false ) )
            {
                var memberId = scene.AddMember();

                var refused = scene.Save( memberId, bag =>
                {
                    bag.IsChatBanned = true;
                    bag.ChatBannedUntil = RockDateTime.Now.AddMinutes( -5 ).ToRockDateTimeOffset();
                } );

                Assert.AreEqual( HttpStatusCode.BadRequest, refused.StatusCode, "a ban that ended before it was set is a mistake" );
                Assert.IsFalse( scene.Stored( memberId ).IsChatBanned, "and nothing is saved" );

                // A ban whose end has since passed does not stop the member being edited.
                var ended = RockDateTime.Now.AddDays( -1 );
                var endedId = scene.AddMember( m => { m.IsChatBanned = true; m.ChatBannedUntil = ended; } );
                var save = scene.Save( endedId, bag => bag.Note = "Edited after the ban ended" );

                Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                Assert.AreEqual( "Edited after the ban ended", scene.Stored( endedId ).Note );
            }
        }

        [TestMethod]
        public void WithNoChatConfiguredTheBanIsNotSaved()
        {
            // A guard: the save asks the rule the screen asked, so a section not shown saves nothing.
            using ( var scene = new BanScene( isPlatformConfigured: false, isPreviousProviderConfigured: false ) )
            {
                var memberId = scene.AddMember();

                var save = scene.Save( memberId, bag => bag.IsChatBanned = true );

                Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                Assert.IsFalse( scene.Stored( memberId ).IsChatBanned );
            }
        }

        [TestMethod]
        public void StaffLiftABanOnAMemberTheyRemovedFromTheArchivedRow()
        {
            using ( var scene = new BanScene( isPlatformConfigured: true, isPreviousProviderConfigured: false ) )
            {
                var memberId = scene.AddMember( m => { m.IsChatBanned = true; m.IsArchived = true; } );

                var box = scene.Load( memberId );
                Assert.IsTrue( box.Options.IsArchived, "the archived member opens on the screen" );
                Assert.IsTrue( box.Entity.IsChatBanned, "with the ban that keeps them out" );

                var save = scene.Save( memberId, bag => bag.IsChatBanned = false );

                Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                Assert.IsFalse( scene.Stored( memberId ).IsChatBanned, "and the ban is lifted there" );
            }
        }

        #endregion Save

        #region Support

        /// <summary>
        /// Loads a member through the block, as opening the page does.
        /// </summary>
        /// <param name="memberId">The membership, archived or not.</param>
        /// <returns>The box the screen is given.</returns>
        internal static DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag> LoadThroughBlock( int memberId )
        {
            using ( var rockContext = new RockContext() )
            {
                var box = ( DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag> ) CreateBlock( rockContext, memberId ).GetObsidianBlockInitialization();
                Assert.IsNull( box.ErrorMessage, box.ErrorMessage );

                return box;
            }
        }

        /// <summary>
        /// Saves a member through the block from the bag the screen was given, changed as the
        /// caller says, naming every property the screen sends that these tests read.
        /// </summary>
        /// <param name="memberId">The membership, archived or not.</param>
        /// <param name="change">What the person editing changes.</param>
        /// <returns>The block's answer.</returns>
        internal static BlockActionResult SaveThroughBlock( int memberId, Action<GroupMemberBag> change )
        {
            var bag = LoadThroughBlock( memberId ).Entity;
            change( bag );

            var box = new ValidPropertiesBox<GroupMemberBag>
            {
                Bag = bag,
                ValidProperties = new List<string>
                {
                    nameof( GroupMemberBag.GroupRoleId ),
                    nameof( GroupMemberBag.GroupMemberStatus ),
                    nameof( GroupMemberBag.Note ),
                    nameof( GroupMemberBag.IsChatMuted ),
                    nameof( GroupMemberBag.IsChatBanned ),
                    nameof( GroupMemberBag.ChatBannedUntil )
                }
            };

            using ( var rockContext = new RockContext() )
            {
                return CreateBlock( rockContext, memberId ).Save( box, false, true );
            }
        }

        /// <summary>
        /// Builds the block as a page request does, for a Rock administrator viewing the member.
        /// </summary>
        /// <remarks>
        /// The page and block instance only answer the block's own settings and the page's site,
        /// so any placed block will do; the Group Member Detail one is preferred when it exists.
        /// </remarks>
        private static GroupMemberDetail CreateBlock( RockContext rockContext, int memberId )
        {
            var blockTypeGuid = typeof( GroupMemberDetail ).GetCustomAttribute<Rock.SystemGuid.BlockTypeGuidAttribute>().Guid;

            var blockId = new BlockService( rockContext ).Queryable()
                .Where( b => b.PageId.HasValue )
                .OrderByDescending( b => b.BlockType.Guid == blockTypeGuid )
                .ThenBy( b => b.Id )
                .Select( b => b.Id )
                .First();

            var blockCache = BlockCache.Get( blockId );

            var administratorsGuid = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();
            var administrator = new GroupMemberService( rockContext ).Queryable()
                .Where( m => m.Group.Guid == administratorsGuid )
                .OrderBy( m => m.Id )
                .Select( m => m.Person )
                .First();

            var requestContext = new RockRequestContext
            {
                CurrentUser = new UserLogin { Person = administrator, PersonId = administrator.Id }
            };
            requestContext.SetPageParameters( new Dictionary<string, string> { ["GroupMemberId"] = memberId.ToString() } );

            return new GroupMemberDetail
            {
                RockContext = rockContext,
                RequestContext = requestContext,
                BlockCache = blockCache,
                PageCache = blockCache.Page
            };
        }

        /// <summary>
        /// A group in chat scope and the chat providers set as a test needs them, put back when
        /// disposed.
        /// </summary>
        private sealed class BanScene : IDisposable
        {
            private readonly ChatSyncProjectionFixture _fixture = new ChatSyncProjectionFixture();

            private readonly string _previousProvider = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION );

            private readonly string _platform = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CHAT_PLATFORM_CONFIGURATION );

            private readonly Guid _room;

            public BanScene( bool isPlatformConfigured, bool isPreviousProviderConfigured )
            {
                ChatHelper.SaveChatConfiguration( isPreviousProviderConfigured
                    ? new ChatConfiguration { ApiKey = "stream-key", ApiSecret = "stream-secret" }
                    : new ChatConfiguration() );

                if ( isPlatformConfigured )
                {
                    const string kid = "kid-member-detail";
                    _fixture.StoreConfiguration( new ChatPlatformConfiguration
                    {
                        TenantId = Guid.NewGuid(),
                        ProjectUrl = "https://example.supabase.co",
                        PublishableKey = "sb_publishable_test",
                        Kid = kid,
                        PrivateKey = ChatSyncProjectionFixture.CreateSigningKey( kid ).PrivateJwk
                    } );
                }
                else
                {
                    Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CHAT_PLATFORM_CONFIGURATION, string.Empty );
                }

                _room = _fixture.AddChannel( _fixture.SharedGroupTypeId, "Member detail room" );
            }

            /// <summary>
            /// Adds a person to the room, with anything else set before it is saved.
            /// </summary>
            /// <returns>The membership's id.</returns>
            public int AddMember( Action<GroupMember> edit = null )
            {
                var personId = _fixture.AddPerson( "MemberDetail" );
                _fixture.AddMember( _room, personId, edit );

                using ( var rockContext = new RockContext() )
                {
                    return new GroupMemberService( rockContext ).AsNoFilter()
                        .Where( m => m.Group.Guid == _room && m.PersonId == personId )
                        .Select( m => m.Id )
                        .Single();
                }
            }

            public DetailBlockBox<GroupMemberBag, GroupMemberDetailOptionsBag> Load( int memberId ) => LoadThroughBlock( memberId );

            public BlockActionResult Save( int memberId, Action<GroupMemberBag> change ) => SaveThroughBlock( memberId, change );

            /// <summary>
            /// The membership as stored, past every cache.
            /// </summary>
            public GroupMember Stored( int memberId )
            {
                using ( var rockContext = new RockContext() )
                {
                    return new GroupMemberService( rockContext ).AsNoFilter().AsNoTracking().Single( m => m.Id == memberId );
                }
            }

            public void Dispose()
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION, _previousProvider );
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CHAT_PLATFORM_CONFIGURATION, _platform );
                _fixture.Dispose();
            }
        }

        #endregion Support
    }
}
