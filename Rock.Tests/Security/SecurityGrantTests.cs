using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.Tests.Shared;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Security
{
    [TestClass]
    public class SecurityGrantTests
    {
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            // Security grants read their lifetime from the system settings.
            // Provide empty settings so the defaults are used without a database.
            RockCache.AddOrUpdate( "Rock:SystemSettings", new SystemSettings() );
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Removing an item publishes a message that needs a RockApp.
            using ( TestHelper.CreateScopedRockApp() )
            {
                RockCache.Remove( "Rock:SystemSettings" );
            }
        }

        private static SecurityGrant CreateGrant()
        {
            return new SecurityGrant()
                .AddRule( new GroupPickerSecurityGrantRule() );
        }

        [TestMethod]
        public void FromTokenReturnsGrantCreatedByToToken()
        {
            var token = CreateGrant().ToToken();

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( GroupPickerSecurityGrantRule.AccessInstance, Authorization.VIEW ) );
        }

        [TestMethod]
        public void FromTokenReturnsGrantCreatedByRawToToken()
        {
            var token = CreateGrant().ToToken( true );

            var grant = SecurityGrant.FromToken( token );

            Assert.IsNotNull( grant );
            Assert.IsTrue( grant.IsAccessGranted( GroupPickerSecurityGrantRule.AccessInstance, Authorization.VIEW ) );
        }

        [TestMethod]
        public void FromTokenRejectsGrantEncryptedWithGeneralKey()
        {
            // Text encrypted by Encryption.EncryptString must never be
            // accepted as a security grant token, even if something that
            // looks like an authentication code is added to it.
            var json = JsonConvert.SerializeObject( CreateGrant() );
            var encrypted = Encryption.EncryptString( json );
            var fakeCode = System.Convert.ToBase64String( new byte[32] );

            Assert.IsNull( SecurityGrant.FromToken( encrypted ) );
            Assert.IsNull( SecurityGrant.FromToken( $"1;2099-01-01T00:00:00.0000000-07:00;{encrypted}" ) );
            Assert.IsNull( SecurityGrant.FromToken( $"{encrypted}.{fakeCode}" ) );
        }

        [TestMethod]
        public void FromTokenRejectsModifiedToken()
        {
            var token = CreateGrant().ToToken();
            var segments = token.Split( ';' );
            var payload = segments[2];
            var modifiedPayload = payload.Substring( 0, 5 ) + ( payload[5] == 'A' ? 'B' : 'A' ) + payload.Substring( 6 );

            Assert.IsNull( SecurityGrant.FromToken( $"{segments[0]};{segments[1]};{modifiedPayload}" ) );
        }

        [TestMethod]
        public void FromTokenReturnsNullForEmptyToken()
        {
            Assert.IsNull( SecurityGrant.FromToken( null ) );
            Assert.IsNull( SecurityGrant.FromToken( string.Empty ) );
        }
    }
}
