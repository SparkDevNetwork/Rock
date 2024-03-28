using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Rest.Controllers;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Rest.ControllersTests
{
    [TestClass]
    public class AuthControllerTests : DatabaseTestsBase
    {
        [TestMethod]
        public async Task PinAuthenticationShouldFail()
        {
            var expectedMessage = "Invalid login type.";

            var exception = Assert.That.ThrowsException<HttpResponseException>( () =>
            {
                var controller = new AuthController
                {
                    Request = new HttpRequestMessage(),
                    RequestContext = new HttpRequestContext()
                };

                controller.Login( new Rock.Security.LoginParameters
                {
                    Username = "7777",
                    Password = "7777"
                } );
            } );

            var responseContent = await exception.Response.Content.ReadAsStringAsync();
            var error = responseContent.FromJsonOrThrow<HttpError>();

            Assert.That.AreEqual( expectedMessage, error.Message );
        }
    }
}
