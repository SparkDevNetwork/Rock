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
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Model
{
    /// <summary>
    /// This suite checks the client lookups on <see cref="AuthClientService"/>.
    /// </summary>
    [TestClass]
    public class AuthClientServiceLookupTests
    {
        [TestMethod]
        public void GetByClientId_WithActiveClient_ReturnsClient()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );
            rockContextMock.SetupDbSet( new List<AuthClient> { new AuthClient { Id = 1, ClientId = "active-client", IsActive = true } } );

            var authClient = new AuthClientService( rockContextMock.Object ).GetByClientId( "active-client" );

            Assert.IsNotNull( authClient );
            Assert.AreEqual( 1, authClient.Id );
        }

        [TestMethod]
        public void GetByClientId_WithInactiveClient_ReturnsNull()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );
            rockContextMock.SetupDbSet( new List<AuthClient> { new AuthClient { Id = 1, ClientId = "inactive-client", IsActive = false } } );

            var authClient = new AuthClientService( rockContextMock.Object ).GetByClientId( "inactive-client" );

            Assert.IsNull( authClient );
        }
    }
}
