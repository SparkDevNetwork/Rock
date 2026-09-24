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

using Rock.Communication.Chat.Platform.Configuration;
using Rock.ViewModels.Blocks.Communication.Chat.ChatConfiguration;
using Rock.ViewModels.Utility;

namespace Rock.Tests.Communication.Chat.Platform.Blocks
{
    /// <summary>
    /// What the Chat Configuration block may show the browser, and what it may accept
    /// back from it.
    /// </summary>
    [TestClass]
    public class ChatConfigurationBlockTests
    {
        private const string PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}";

        #region What the browser is sent

        [TestMethod]
        public void ToBag_CarriesNoPrivateKey_ButSaysWhetherOneIsPresent()
        {
            var bag = ChatConfigurationPolicy.ToBag( Stored() );

            Assert.IsTrue( bag.IsChurchKeyPresent );

            var serialized = bag.ToJson();
            Assert.IsFalse( serialized.Contains( "secret-part" ), "the bag carries the church signing key" );
            Assert.IsFalse( serialized.Contains( "\"kty\"" ), "the bag carries the church signing key" );
        }

        [TestMethod]
        public void ToBag_WithNoKeyStored_SaysSo()
        {
            var stored = Stored();
            stored.PrivateKey = null;

            Assert.IsFalse( ChatConfigurationPolicy.ToBag( stored ).IsChurchKeyPresent );
        }

        [TestMethod]
        public void ToBag_CarriesEveryChurchOwnedSetting()
        {
            var stored = Stored();

            var bag = ChatConfigurationPolicy.ToBag( stored );

            Assert.IsTrue( bag.AreChatProfilesVisible );
            Assert.IsTrue( bag.IsOpenDirectMessagingAllowed );
            Assert.AreEqual( 13, bag.MinimumAge );
            Assert.AreEqual( stored.DirectMessageAccessDataViewGuid.ToString(), bag.DirectMessageAccessDataView.Value );
            Assert.AreEqual( 1, bag.ChatBadgeDataViews.Count );
        }

        #endregion What the browser is sent

        #region What the browser may send back

        [TestMethod]
        public void Save_ReturnsTheChurchHalfOnly_SoTheScreenCannotWriteThePlatformHalfOrTheKey()
        {
            // The screen owns the church half and nothing else. The platform half and the
            // signing key belong to Enable Chat, which writes them by name, so what comes
            // out of here carries neither: there is nothing to take away and nothing to
            // overwrite, whatever a modified client sends back.
            var stored = Stored();
            var bag = ChatConfigurationPolicy.ToBag( stored );
            bag.ProjectUrl = "https://attacker.example";
            bag.PublishableKey = "sb_publishable_attacker";
            bag.TenantId = Guid.NewGuid().ToString();
            bag.Kid = "attacker-kid";

            var result = ChatConfigurationPolicy.Save( bag, isAuthorizedToEdit: true );

            Assert.IsTrue( result.IsSaved );
            Assert.IsNull( result.Configuration.ProjectUrl );
            Assert.IsNull( result.Configuration.PublishableKey );
            Assert.IsNull( result.Configuration.TenantId );
            Assert.IsNull( result.Configuration.Kid );
            Assert.IsNull( result.Configuration.PrivateKey );
        }

        [TestMethod]
        public void Save_WithAnEmptyBadgeEntry_IgnoresItRatherThanThrowing()
        {
            var stored = Stored();
            var bag = ChatConfigurationPolicy.ToBag( stored );
            bag.ChatBadgeDataViews = new List<ListItemBag> { null };

            var result = ChatConfigurationPolicy.Save( bag, isAuthorizedToEdit: true );

            Assert.AreEqual( 0, result.Configuration.ChatBadgeDataViewGuids.Count );
        }

        [TestMethod]
        public void Save_AppliesEveryChurchOwnedSetting()
        {
            var stored = Stored();
            var newDataView = Guid.NewGuid();
            var bag = ChatConfigurationPolicy.ToBag( stored );
            bag.AreChatProfilesVisible = false;
            bag.IsOpenDirectMessagingAllowed = false;
            bag.MinimumAge = 16;
            bag.DirectMessageAccessDataView = new ListItemBag { Value = newDataView.ToString(), Text = "Members" };
            bag.ChatBadgeDataViews = new List<ListItemBag>();

            var result = ChatConfigurationPolicy.Save( bag, isAuthorizedToEdit: true );

            Assert.IsFalse( result.Configuration.AreChatProfilesVisible );
            Assert.IsFalse( result.Configuration.IsOpenDirectMessagingAllowed );
            Assert.AreEqual( 16, result.Configuration.MinimumAge );
            Assert.AreEqual( newDataView, result.Configuration.DirectMessageAccessDataViewGuid );
            Assert.AreEqual( 0, result.Configuration.ChatBadgeDataViewGuids.Count );
        }

        [TestMethod]
        public void Save_WhenNotAuthorizedToEdit_Refuses()
        {
            var stored = Stored();
            var bag = ChatConfigurationPolicy.ToBag( stored );
            bag.MinimumAge = 99;

            var result = ChatConfigurationPolicy.Save( bag, isAuthorizedToEdit: false );

            Assert.IsFalse( result.IsSaved );
            Assert.IsNull( result.Configuration );
        }

        #endregion What the browser may send back

        #region Pins

        [TestMethod]
        public void Bag_NamesNoGifVendorKey()
        {
            var named = typeof( ChatConfigurationBag ).GetProperties()
                .Select( p => p.Name )
                .Where( n => n.IndexOf( "klipy", StringComparison.OrdinalIgnoreCase ) >= 0
                    || n.IndexOf( "gif", StringComparison.OrdinalIgnoreCase ) >= 0 )
                .ToList();

            Assert.AreEqual( 0, named.Count, "GIF search is hosted by Spark, so no church ever holds that key: " + string.Join( ", ", named ) );
        }

        #endregion Pins

        #region A church enabled whose credentials cannot be read

        [TestMethod]
        public void IsCredentialUnreadable_EnabledAndMissingAnythingChatNeeds_IsTrue()
        {
            var noKey = Stored();
            noKey.PrivateKey = null;
            var noProjectUrl = Stored();
            noProjectUrl.ProjectUrl = " ";
            var noPublishableKey = Stored();
            noPublishableKey.PublishableKey = null;

            Assert.IsTrue( ChatConfigurationPolicy.IsCredentialUnreadable( noKey ), "the signing key cannot be read" );
            Assert.IsTrue( ChatConfigurationPolicy.IsCredentialUnreadable( noProjectUrl ), "no project address" );
            Assert.IsTrue( ChatConfigurationPolicy.IsCredentialUnreadable( noPublishableKey ), "no publishable key" );
        }

        [TestMethod]
        public void IsCredentialUnreadable_NeverEnabledOrReadyToChat_IsFalse()
        {
            var neverEnabled = Stored();
            neverEnabled.TenantId = null;
            neverEnabled.PrivateKey = null;

            Assert.IsFalse( ChatConfigurationPolicy.IsCredentialUnreadable( neverEnabled ), "never enabled" );
            Assert.IsFalse( ChatConfigurationPolicy.IsCredentialUnreadable( new ChatPlatformConfiguration() ), "nothing stored" );
            Assert.IsFalse( ChatConfigurationPolicy.IsCredentialUnreadable( null ), "no settings at all" );
            Assert.IsFalse( ChatConfigurationPolicy.IsCredentialUnreadable( Stored() ), "ready to chat" );
        }

        #endregion A church enabled whose credentials cannot be read

        #region Helpers

        private static ChatPlatformConfiguration Stored()
        {
            return new ChatPlatformConfiguration
            {
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                MinimumAge = 13,
                DirectMessageAccessDataViewGuid = Guid.Parse( "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb" ),
                ChatBadgeDataViewGuids = new List<Guid> { Guid.Parse( "cccccccc-cccc-4ccc-8ccc-cccccccccccc" ) },
                PrivateKey = PrivateKey,
                TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                ProjectUrl = "https://example.supabase.co",
                PublishableKey = "sb_publishable_test",
                Kid = "platform-kid-1"
            };
        }

        #endregion Helpers
    }
}
