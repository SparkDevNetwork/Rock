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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Tests.Communication.Chat.Platform.Blocks
{
    /// <summary>
    /// What the chat card on the Connected Services page shows, and when it may offer
    /// to set the organization up. The question the card asks is the point of these:
    /// the settings can say both "live on the platform" and "cannot chat" at once, and
    /// the card must read the first.
    /// </summary>
    [TestClass]
    public class ChatCardPolicyTests
    {
        private const string PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}";

        #region A church whose stored key this installation cannot read

        [TestMethod]
        public void ToBag_ForAChurchWhoseKeyCannotBeRead_StillSaysEnabled()
        {
            // A database restored onto an installation with a different encryption key.
            // Reading the settings returns no private key, because decryption fails and
            // is swallowed, so everything else is intact and the organization is still
            // live on the chat platform.
            var bag = ChatCardPolicy.ToBag( RestoredOntoAnotherInstallation() );

            Assert.IsTrue( bag.IsEnabled, "a church already live on the platform is shown as not enabled, so the card offers to set it up again" );
            Assert.AreEqual( "https://example.supabase.co", bag.ProjectUrl );
        }

        [TestMethod]
        public void MayEnable_ForAChurchWhoseKeyCannotBeRead_IsFalse()
        {
            // Enabling again mints a second signing key and orphans the first, and
            // nothing in Rock or on the platform can rotate or retire one.
            Assert.IsFalse( ChatCardPolicy.MayEnable( RestoredOntoAnotherInstallation() ), "the card would strand the tenant this organization already has" );
        }

        #endregion A church whose stored key this installation cannot read

        #region A church that was never set up

        [TestMethod]
        public void ToBag_ForAChurchThatWasNeverSetUp_SaysNotEnabledAndNamesNothing()
        {
            var bag = ChatCardPolicy.ToBag( new ChatPlatformConfiguration() );

            Assert.IsFalse( bag.IsEnabled );
            Assert.IsNull( bag.TenantId );
            Assert.IsNull( bag.ProjectUrl );
        }

        [TestMethod]
        public void MayEnable_ForAChurchThatWasNeverSetUp_IsTrue()
        {
            Assert.IsTrue( ChatCardPolicy.MayEnable( new ChatPlatformConfiguration() ) );
        }

        [TestMethod]
        public void MayEnable_WithNothingStoredAtAll_IsTrueRatherThanThrowing()
        {
            Assert.IsTrue( ChatCardPolicy.MayEnable( null ) );
        }

        #endregion A church that was never set up

        #region A church that is set up and can chat

        [TestMethod]
        public void ToBag_ForAChurchThatCanChat_SaysEnabledAndNamesTheOrganization()
        {
            var stored = Complete();

            var bag = ChatCardPolicy.ToBag( stored );

            Assert.IsTrue( bag.IsEnabled );
            Assert.AreEqual( stored.TenantId.ToString(), bag.TenantId );
            Assert.AreEqual( stored.ProjectUrl, bag.ProjectUrl );
        }

        [TestMethod]
        public void MayEnable_ForAChurchThatCanChat_IsFalse()
        {
            Assert.IsFalse( ChatCardPolicy.MayEnable( Complete() ) );
        }

        #endregion A church that is set up and can chat

        #region Pins

        [TestMethod]
        public void ToBag_CarriesNoSigningKey()
        {
            var serialized = ChatCardPolicy.ToBag( Complete() ).ToJson();

            Assert.IsFalse( serialized.Contains( "secret-part" ), "the card's bag carries the church signing key" );
            Assert.IsFalse( serialized.Contains( "\"kty\"" ), "the card's bag carries the church signing key" );
        }

        #endregion Pins

        #region Helpers

        /// <summary>
        /// Everything Enable Chat wrote, read back on an installation that cannot decrypt
        /// the signing key. <see cref="ChatPlatformConfigurationService.Read"/> returns a
        /// null key in that case rather than throwing, and leaves the rest as stored.
        /// </summary>
        private static ChatPlatformConfiguration RestoredOntoAnotherInstallation()
        {
            var stored = Complete();
            stored.PrivateKey = null;

            return stored;
        }

        private static ChatPlatformConfiguration Complete()
        {
            return new ChatPlatformConfiguration
            {
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
