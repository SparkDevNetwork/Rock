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
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Sync Now finds the restatement job by its well-known identifier and
    /// refuses with a reason when it cannot.
    /// </summary>
    [TestClass]
    public class ChatSyncNowTests
    {
        [TestMethod]
        public void CanRequest_WithoutTheJob_IsFalse()
        {
            Assert.IsFalse( ChatSyncNow.CanRequest( EmptyContext(), Complete() ) );
        }

        [TestMethod]
        public void CanRequest_WithTheJobAndAConfiguredChurch_IsTrue()
        {
            Assert.IsTrue( ChatSyncNow.CanRequest( ContextWithJob(), Complete() ) );
        }

        [TestMethod]
        public void TryRequest_WithoutTheJob_ReturnsAReasonAndDoesNotQueue()
        {
            var queued = 0;
            string error;

            var ok = ChatSyncNow.TryRequest( EmptyContext(), Complete(), out error, id => queued = id );

            Assert.IsFalse( ok );
            Assert.AreEqual( "The chat restatement job is not registered.", error );
            Assert.AreEqual( 0, queued );
        }

        [TestMethod]
        public void TryRequest_WithTheJob_QueuesThatJob()
        {
            var queued = 0;
            string error;

            var ok = ChatSyncNow.TryRequest( ContextWithJob(), Complete(), out error, id => queued = id );

            Assert.IsTrue( ok );
            Assert.IsNull( error );
            Assert.AreEqual( 9, queued );
        }

        [TestMethod]
        public void TryRequest_WhenChatIsNotConfigured_DoesNotLookForTheJob()
        {
            var queued = 0;
            string error;

            var ok = ChatSyncNow.TryRequest( ContextWithJob(), new ChatPlatformConfiguration(), out error, id => queued = id );

            Assert.IsFalse( ok );
            Assert.AreEqual( "Chat is not configured.", error );
            Assert.AreEqual( 0, queued );
        }

        private static RockContext EmptyContext()
        {
            return MockDatabaseHelper.CreateRockContextMock().Object;
        }

        private static RockContext ContextWithJob()
        {
            var rockContext = MockDatabaseHelper.CreateRockContextMock().Object;
            rockContext.Set<ServiceJob>().Add( new ServiceJob
            {
                Id = 9,
                Guid = Guid.Parse( Rock.SystemGuid.ServiceJob.CHAT_PLATFORM_SYNC ),
                Name = "Chat Restatement",
                Class = "Rock.Jobs.ChatPlatformSyncJob"
            } );
            return rockContext;
        }

        private static ChatPlatformConfiguration Complete()
        {
            return new ChatPlatformConfiguration
            {
                TenantId = Guid.Parse( "11111111-1111-4111-8111-111111111111" ),
                PrivateKey = "{\"kty\":\"EC\",\"crv\":\"P-256\",\"kid\":\"kid-1\",\"d\":\"secret-part\"}",
                ProjectUrl = "http://example.test",
                PublishableKey = "sb_publishable_test",
                Kid = "kid-1"
            };
        }
    }
}
