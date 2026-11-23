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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Model;
using Rock.Security;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Tests that a connection request status history row is secured by its
    /// connection request.
    /// </summary>
    [TestClass]
    public class ConnectionRequestStatusHistoryTests
    {
        /// <summary>
        /// Should use the connection request as the parent authority when the request is set.
        /// </summary>
        [TestMethod]
        public void ParentAuthority_WithRequest_IsTheRequest()
        {
            var connectionRequest = new ConnectionRequest();
            var history = new ConnectionRequestStatusHistory
            {
                ConnectionRequest = connectionRequest,
                ConnectionStatus = new ConnectionStatus()
            };

            Assert.AreSame( connectionRequest, history.ParentAuthority );
        }

        /// <summary>
        /// Should fall back to the connection status as the parent authority when there is no request.
        /// </summary>
        [TestMethod]
        public void ParentAuthority_WithoutRequest_IsTheStatus()
        {
            var connectionStatus = new ConnectionStatus();
            var history = new ConnectionRequestStatusHistory
            {
                ConnectionStatus = connectionStatus
            };

            Assert.AreSame( connectionStatus, history.ParentAuthority );
        }

        /// <summary>
        /// Should deny access when the connection request denies it.
        /// </summary>
        [TestMethod]
        public void IsAuthorized_WithRequest_DeniedWhenRequestDenies()
        {
            var person = new Person { Id = 10 };
            var requestMock = new Mock<ConnectionRequest>();
            requestMock.Setup( m => m.IsAuthorized( Authorization.VIEW, person ) ).Returns( false );

            var history = new ConnectionRequestStatusHistory
            {
                ConnectionRequest = requestMock.Object,
                ConnectionStatus = new ConnectionStatus()
            };

            Assert.IsFalse( history.IsAuthorized( Authorization.VIEW, person ) );
            requestMock.Verify( m => m.IsAuthorized( Authorization.VIEW, person ), Times.Once );
        }

        /// <summary>
        /// Should allow access when the connection request allows it.
        /// </summary>
        [TestMethod]
        public void IsAuthorized_WithRequest_AllowedWhenRequestAllows()
        {
            var person = new Person { Id = 10 };
            var requestMock = new Mock<ConnectionRequest>();
            requestMock.Setup( m => m.IsAuthorized( Authorization.EDIT, person ) ).Returns( true );

            var history = new ConnectionRequestStatusHistory
            {
                ConnectionRequest = requestMock.Object,
                ConnectionStatus = new ConnectionStatus()
            };

            Assert.IsTrue( history.IsAuthorized( Authorization.EDIT, person ) );
            requestMock.Verify( m => m.IsAuthorized( Authorization.EDIT, person ), Times.Once );
        }
    }
}
