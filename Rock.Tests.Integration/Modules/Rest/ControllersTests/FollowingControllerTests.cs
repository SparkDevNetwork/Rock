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
using System.Net.Http;
using System.Web.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Rest.Controllers;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Rest.ControllersTests
{
    [TestClass]
    public class FollowingControllerTests : DatabaseTestsBase
    {
        [TestMethod]
        [IsolatedTestDatabase]
        public void Follow_AddFollowForCurrentUser_AddsFollowing()
        {
            var personType = EntityTypeCache.Get( typeof( Rock.Model.Person ) );
            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );
            var personBillMarble = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BillMarble );

            var controller = new FollowingsController();
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            // Set the current user as Ted Decker.
            controller.Request.Properties["Person"] = personTedDecker;

            // Add a Following: Ted Decker following Bill Marble.

            // Remove existing Followings.
            controller.Delete( personType.Id, personBillMarble.Id, "Test" );
            controller.Delete( personType.Guid, personBillMarble.Guid, "Test" );

            // Test 1: Add a Following by Id.
            var followResponse1 = controller.Follow( personType.Id, personBillMarble.Id, "Test" );
            Assert.IsTrue( followResponse1.TryGetContentValue<int>( out var followId1 ) );
            Assert.IsTrue( followId1 != 0 );

            // Remove the Following by Id.
            controller.Delete( personType.Id, personBillMarble.Id, "Test" );

            // Test 2: Add a Following by Guid
            var followResponse2 = controller.Follow( personType.Guid, personBillMarble.Guid, "Test" );
            Assert.IsTrue( followResponse2.TryGetContentValue<int>( out var followId2 ) );
            Assert.IsTrue( followId2 != 0 );

            // Remove the Following by Guid.
            controller.Delete( personType.Guid, personBillMarble.Guid, "Test" );
        }

        [TestMethod]
        public void Follow_WithInvalidEntityTypeParameter_ReturnsExpectedError()
        {
            var personBillMarble = TestDataHelper.GetTestPerson( TestGuids.TestPeople.BillMarble );
            Follow_WithInvalidParameter_ReturnsExpectedError( Guid.Empty, personBillMarble.Guid );
        }

        [TestMethod]
        public void Follow_WithInvalidEntityParameter_ReturnsExpectedError()
        {
            var personType = EntityTypeCache.Get( typeof( Rock.Model.Person ) );
            Follow_WithInvalidParameter_ReturnsExpectedError( personType.Guid, Guid.Empty );
        }

        private void Follow_WithInvalidParameter_ReturnsExpectedError( Guid entityTypeGuid, Guid entityGuid )
        {
            var personTedDecker = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var controller = new FollowingsController();
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            // Set the current user as Ted Decker.
            controller.Request.Properties["Person"] = personTedDecker;

            HttpResponseException exception = null;
            string errorMessage;

            // Attempt to add a Following with an invalid parameter.
            try
            {
                var followResponse1 = controller.Follow( entityTypeGuid, entityGuid, "Test" );
            }
            catch ( System.Web.Http.HttpResponseException ex )
            {
                exception = ex;
            }

            // Verify that an invalid parameter error is returned.
            Assert.That.IsTrue( exception?.Response?.StatusCode == System.Net.HttpStatusCode.BadRequest );
            errorMessage = exception.Response.Content.ReadAsStringAsync().Result;
            Assert.That.IsTrue( errorMessage.Contains( "Invalid parameter value." ) );
            Assert.That.IsTrue( errorMessage.Contains( Guid.Empty.ToString() ) );
        }
    }
}
