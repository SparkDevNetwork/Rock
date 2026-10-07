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

using Rock.Blocks.Group;
using Rock.Communication.Chat;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Integration.Communication.Chat.Platform.Sync;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Blocks
{
    /// <summary>
    /// Group Type Detail carries a role's two chat capabilities to the screen and back, and tells
    /// the screen whether the Stream provider is the one running.
    /// </summary>
    /// <remarks>
    /// These drive the block's own load and save, not the role service, because the copy between
    /// the role and its bag is written out by hand one property at a time: a line left out or two
    /// lines crossed compiles and saves without complaint, and an administrator's change is lost.
    /// Each role here holds one capability and not the other, so crossing the two is seen too.
    /// </remarks>
    [TestClass]
    public class GroupTypeDetailChatRoleTests : DatabaseTestsBase
    {
        #region Load

        [TestMethod]
        public void LoadingTheGroupType_ShowsEachRolesCapabilitiesAsStored()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                var mentionOnly = AddRole( fixture, "Mention Only", canMentionAll: true, canPostAnnouncements: false );
                var announceOnly = AddRole( fixture, "Announce Only", canMentionAll: false, canPostAnnouncements: true );

                using ( var rockContext = new RockContext() )
                {
                    var block = CreateBlock( rockContext, fixture.SharedGroupTypeId );

                    var box = ( DetailBlockBox<GroupTypeBag, GroupTypeDetailOptionsBag> ) block.GetObsidianBlockInitialization();

                    Assert.IsNull( box.ErrorMessage, box.ErrorMessage );

                    var mentionOnlyBag = box.Entity.Roles.Single( r => r.Guid == mentionOnly );
                    Assert.IsTrue( mentionOnlyBag.CanMentionAll, "a role that may mention everyone shows that it may" );
                    Assert.IsFalse( mentionOnlyBag.CanPostAnnouncements, "a role that may not post announcements shows that it may not" );

                    var announceOnlyBag = box.Entity.Roles.Single( r => r.Guid == announceOnly );
                    Assert.IsFalse( announceOnlyBag.CanMentionAll, "a role that may not mention everyone shows that it may not" );
                    Assert.IsTrue( announceOnlyBag.CanPostAnnouncements, "a role that may post announcements shows that it may" );
                }
            }
        }

        [TestMethod]
        public void LoadingTheGroupType_SaysWhetherStreamIsTheChatProvider()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                // The Stream setting is the whole church's, so it goes back as it was found.
                var originalStreamConfiguration = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION );

                try
                {
                    ChatHelper.SaveChatConfiguration( new ChatConfiguration() );
                    Assert.IsFalse( LoadOptions( fixture.SharedGroupTypeId ).IsStreamChatEnabled, "Stream not configured, so the screen hides what only Stream reads" );

                    ChatHelper.SaveChatConfiguration( new ChatConfiguration { ApiKey = "stream-key", ApiSecret = "stream-secret" } );
                    Assert.IsTrue( LoadOptions( fixture.SharedGroupTypeId ).IsStreamChatEnabled, "Stream configured, so the screen shows what only Stream reads" );
                }
                finally
                {
                    Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.CHAT_CONFIGURATION, originalStreamConfiguration );
                }
            }
        }

        #endregion Load

        #region Save

        [TestMethod]
        public void SavingTheGroupType_StoresEachRolesCapabilitiesAsSent()
        {
            using ( var fixture = new ChatSyncProjectionFixture() )
            {
                // Each role is sent the opposite of what it holds, so a value the save did not copy
                // stays behind where the assertions can see it.
                var mentionOnly = AddRole( fixture, "Mention Only", canMentionAll: true, canPostAnnouncements: false );
                var announceOnly = AddRole( fixture, "Announce Only", canMentionAll: false, canPostAnnouncements: true );

                using ( var rockContext = new RockContext() )
                {
                    var block = CreateBlock( rockContext, fixture.SharedGroupTypeId );
                    var groupTypeKey = GroupTypeCache.Get( fixture.SharedGroupTypeId ).IdKey;

                    // The screen saves the box it was given to edit, so the save starts from that.
                    var edit = block.Edit( groupTypeKey );
                    Assert.AreEqual( HttpStatusCode.OK, edit.StatusCode, edit.Error );
                    var box = ( ValidPropertiesBox<GroupTypeBag> ) edit.Content;

                    var mentionOnlyBag = box.Bag.Roles.Single( r => r.Guid == mentionOnly );
                    mentionOnlyBag.CanMentionAll = false;
                    mentionOnlyBag.CanPostAnnouncements = true;

                    var announceOnlyBag = box.Bag.Roles.Single( r => r.Guid == announceOnly );
                    announceOnlyBag.CanMentionAll = true;
                    announceOnlyBag.CanPostAnnouncements = false;

                    var save = block.Save( box );
                    Assert.AreEqual( HttpStatusCode.OK, save.StatusCode, save.Error );
                }

                var stored = StoredCapabilities( mentionOnly, announceOnly );

                Assert.IsFalse( stored[mentionOnly].CanMentionAll, "mention everyone, turned off, is stored off" );
                Assert.IsTrue( stored[mentionOnly].CanPostAnnouncements, "post announcements, turned on, is stored on" );
                Assert.IsTrue( stored[announceOnly].CanMentionAll, "mention everyone, turned on, is stored on" );
                Assert.IsFalse( stored[announceOnly].CanPostAnnouncements, "post announcements, turned off, is stored off" );
            }
        }

        #endregion Save

        #region Support

        /// <summary>
        /// Adds a role to the fixture's group type holding the two capabilities given.
        /// </summary>
        /// <returns>The role's guid, the key the block's bags carry.</returns>
        private static Guid AddRole( ChatSyncProjectionFixture fixture, string name, bool canMentionAll, bool canPostAnnouncements )
        {
            var roleId = fixture.AddRole( fixture.SharedGroupTypeId, name, false );
            fixture.SetRoleCapabilitiesDirectly( roleId, canMentionAll, canPostAnnouncements );

            using ( var rockContext = new RockContext() )
            {
                return new GroupTypeRoleService( rockContext ).GetSelect( roleId, r => r.Guid );
            }
        }

        /// <summary>
        /// Reads the two capabilities of each role from its row, past every cache.
        /// </summary>
        private static Dictionary<Guid, GroupTypeRole> StoredCapabilities( params Guid[] roleGuids )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupTypeRoleService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( r => roleGuids.Contains( r.Guid ) )
                    .ToDictionary( r => r.Guid );
            }
        }

        /// <summary>
        /// Loads the group type through the block and returns the options it sends the screen.
        /// </summary>
        private static GroupTypeDetailOptionsBag LoadOptions( int groupTypeId )
        {
            using ( var rockContext = new RockContext() )
            {
                var box = ( DetailBlockBox<GroupTypeBag, GroupTypeDetailOptionsBag> ) CreateBlock( rockContext, groupTypeId ).GetObsidianBlockInitialization();

                return box.Options;
            }
        }

        /// <summary>
        /// Builds the block as a page request does, for a Rock administrator viewing the group type.
        /// </summary>
        /// <param name="rockContext">The context the block reads and saves through.</param>
        /// <param name="groupTypeId">The group type named in the page's address.</param>
        /// <returns>The block, ready for its load or an action.</returns>
        /// <remarks>
        /// The page and block instance only answer the block's own settings and the page's site, so
        /// any placed block will do; the Group Type Detail one is preferred when the page exists.
        /// </remarks>
        private static GroupTypeDetail CreateBlock( RockContext rockContext, int groupTypeId )
        {
            var blockTypeGuid = typeof( GroupTypeDetail ).GetCustomAttribute<Rock.SystemGuid.BlockTypeGuidAttribute>().Guid;

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
            requestContext.SetPageParameters( new Dictionary<string, string> { ["GroupTypeId"] = groupTypeId.ToString() } );

            return new GroupTypeDetail
            {
                RockContext = rockContext,
                RequestContext = requestContext,
                BlockCache = blockCache,
                PageCache = blockCache.Page
            };
        }

        #endregion Support
    }
}
