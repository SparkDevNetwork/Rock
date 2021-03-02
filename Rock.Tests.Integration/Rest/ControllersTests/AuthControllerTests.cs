using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Rest.ControllersTests
{
    [TestClass]
    public class AuthControllerTests
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            StartIISExpress();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            StopIISExpress();
        }

        public class CookieAwareWebClient : WebClient
        {
            public CookieAwareWebClient()
            {
                CookieContainer = new CookieContainer();
            }
            public CookieContainer CookieContainer { get; private set; }

            protected override WebRequest GetWebRequest( Uri address )
            {
                var request = ( HttpWebRequest ) base.GetWebRequest( address );
                request.CookieContainer = CookieContainer;
                return request;
            }
        }

        [TestMethod]
        public void PinAuthenticationShouldFail()
        {
            var client = new CookieAwareWebClient();
            client.BaseAddress = "http://localhost:9090/";
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            Assert.That.ThrowsExceptionWithMessage<WebException>( () => client.UploadString( "api/Auth/Login", "{'Username': '7777','Password': '7777'}" ), "The remote server returned an error: (401) Unauthorized." );
        }

        [TestMethod]
        public void AuthenticatedUserShouldReturnData()
        {
            var client = new CookieAwareWebClient();
            client.BaseAddress = "http://localhost:9090/";
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            var result = client.UploadString( "api/Auth/Login", "{'Username': 'admin','Password': 'admin'}" );
            Assert.That.IsTrue( result.IsNullOrWhiteSpace() );

            result = client.DownloadString( $"/api/Badges/InGroupOfType/{1}/{System.Guid.NewGuid()}" );
            Assert.That.IsTrue( result.IsNotNullOrWhiteSpace() );
        }

        [TestMethod]
        public void AuthenticatedPenUserShouldFailAuthCheck()
        {
            var client = new CookieAwareWebClient();
            client.BaseAddress = "http://localhost:9090/";
            var result = client.DownloadString( "/" );
            Assert.That.IsTrue( result.IsNotNullOrWhiteSpace() );

            client.CookieContainer.Add( new Cookie( ".ROCK", "9373243FAEE9455CCFEF205114261C4CAAA451ABEBA8F27713DC015CDE99B3B796A683204D1F7D3FDF949483926A8AC5EDE93AA1655DB1B0AAF7E50062A9F4ACD557C645FD113ECDAEA82BAC152EE09D98EC4696", "/", "localhost" ) );
            Assert.That.ThrowsExceptionWithMessage<WebException>( () => client.DownloadString( $"/api/Badges/InGroupOfType/{1}/{System.Guid.NewGuid()}" ), "The remote server returned an error: (401) Unauthorized." );
        }

        private static Process _iisExpressProcess = null;

        private static void StartIISExpress()
        {
            var webSitePath = Path.GetFullPath( Directory.GetCurrentDirectory() + "..\\..\\..\\..\\RockWeb" );
            var port = "9090";
            var arguments = $"/path:\"{webSitePath}\" /port:{port}";

            var startInfo = new ProcessStartInfo( "\\Program Files\\IIS Express\\iisexpress.exe" )
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = arguments
            };

            _iisExpressProcess = Process.Start( startInfo );
        }

        private static void StopIISExpress()
        {
            if ( _iisExpressProcess.HasExited == false )
            {
                _iisExpressProcess.Kill();
                _iisExpressProcess.Dispose();
                _iisExpressProcess = null;
            }
        }
    }
}
