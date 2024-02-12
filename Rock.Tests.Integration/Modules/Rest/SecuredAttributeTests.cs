using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Rock.Tests.Integration.Modules.Rest
{
    [TestClass]
    public class SecuredAttributeTests : DatabaseTestsBase
    {
        [TestMethod]
        public void AuthenticateWithoutPinAuthenticationActive()
        {
            // Set PIN authentication Active status to false
            var activeAttributeGuid = Guid.Parse( "f8926e80-1cd1-4dfd-ac1f-28b5dc75b207" );
            var attributeService = new AttributeService( new RockContext() );
            var activeAttribute = attributeService.Queryable().FirstOrDefault( a => a.Guid == activeAttributeGuid );

            var pinAuthentication = AuthenticationContainer.GetComponent( typeof( Rock.Security.Authentication.PINAuthentication ).FullName );
            pinAuthentication.AttributeValues["Active"] = new AttributeValueCache { AttributeId = activeAttribute.Id, EntityId = pinAuthentication.Id, Value = "False" };

            // Add signed in user to trigger PIN authentication check
            var principal = new GenericPrincipal( new GenericIdentity( "tdecker" ), null );
            var requestContext = new HttpRequestContext() { Principal = principal };
            var request = new HttpRequestMessage();
            request.SetUserPrincipal( principal );

            // Categories controller because it is authenticated and will activate SecuredAttribute filter and has an endpoint which us called to populate a tree-view
            var descriptor = new HttpControllerDescriptor( new HttpConfiguration(), nameof( CategoriesController ), typeof( CategoriesController ) );
            var controller = new CategoriesController() { Request = request, RequestContext = requestContext, User = principal };
            var actionFilter = new SecuredAttribute();
            var httpContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext( requestContext, request, descriptor, controller ),
                ActionDescriptor = new ReflectedHttpActionDescriptor() { ControllerDescriptor = descriptor, MethodInfo = descriptor.ControllerType.GetMethod( "GetChildren" ) },
                Response = new HttpResponseMessage( HttpStatusCode.OK )
            };

            actionFilter.OnActionExecuting( httpContext );

            Assert.That.AreEqual( HttpStatusCode.OK, httpContext.Response.StatusCode );
        }
    }
}