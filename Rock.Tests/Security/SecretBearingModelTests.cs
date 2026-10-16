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

using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Provider;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Tests that models holding sign-in codes or provider secrets are not
    /// exposed through the generic v1 REST controllers, serialization or Lava.
    /// </summary>
    [TestClass]
    public class SecretBearingModelTests
    {
        [DataTestMethod]
        [DataRow( typeof( IdentityVerification ) )]
        [DataRow( typeof( IdentityVerificationCode ) )]
        [DataRow( typeof( AIProvider ) )]
        [DataRow( typeof( RemoteAuthenticationSession ) )]
        public void Model_ExcludesDefaultRestController( Type modelType )
        {
            var attribute = modelType.GetCustomAttribute<CodeGenExcludeAttribute>();

            Assert.IsNotNull( attribute );
            Assert.IsTrue( attribute.ExcludedFeatures.HasFlag( CodeGenFeature.DefaultRestController ) );
        }

        [DataTestMethod]
        [DataRow( "Rock.Rest.Controllers.IdentityVerificationsController" )]
        [DataRow( "Rock.Rest.Controllers.IdentityVerificationCodesController" )]
        [DataRow( "Rock.Rest.Controllers.AIProvidersController" )]
        [DataRow( "Rock.Rest.Controllers.RemoteAuthenticationSessionsController" )]
        public void RestAssembly_DoesNotContainV1Controller( string controllerTypeName )
        {
            var restAssembly = typeof( Rock.Rest.ApiControllerBase ).Assembly;

            Assert.IsNull( restAssembly.GetType( controllerTypeName ) );
        }

        [TestMethod]
        public void IdentityVerificationCode_Code_IsHidden()
        {
            var property = typeof( IdentityVerificationCode ).GetProperty( nameof( IdentityVerificationCode.Code ) );

            Assert.IsFalse( HasAttribute( property, "System.Runtime.Serialization.DataMemberAttribute" ) );
            Assert.IsTrue( HasAttribute( property, "System.Runtime.Serialization.IgnoreDataMemberAttribute" ) );
            Assert.IsNotNull( property.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>() );
            Assert.IsNotNull( property.GetCustomAttribute<HideFromReportingAttribute>() );
            Assert.IsFalse( LavaHelper.IsLavaProperty( property ) );
        }

        [TestMethod]
        public void IdentityVerificationCode_ToJson_DoesNotIncludeCode()
        {
            var code = new IdentityVerificationCode { Code = "482913" };

            Assert.IsFalse( code.ToJson().Contains( "482913" ) );
        }

        [TestMethod]
        public void DecryptString_PlainTextSecretKey_ReturnsNull()
        {
            // Plain text keys rely on this to fall back to the stored value
            // and to be picked up for encryption at start-up.
            Assert.IsNull( Encryption.DecryptString( "sk-proj-TH164_test-key" ) );
        }

        [TestMethod]
        public void DecryptString_EncryptedSecretKey_RoundTrips()
        {
            var plainText = "sk-proj-TH164_test-key";
            var encrypted = Encryption.EncryptString( plainText );

            Assert.AreNotEqual( plainText, encrypted );
            Assert.AreEqual( plainText, Encryption.DecryptString( encrypted ) );
        }

        [TestMethod]
        public void IsRockCiphertextShape_EncryptedValue_ReturnsTrue()
        {
            var encrypted = Encryption.EncryptString( "sk-proj-TH164_test-key" );

            Assert.IsTrue( AIProviderContainer.IsRockCiphertextShape( encrypted ) );
        }

        [TestMethod]
        public void IsRockCiphertextShape_ValueEncryptedWithUnknownKey_IsSkipped()
        {
            // A value encrypted with a key this server does not have must
            // not decrypt to the original text, but must still be recognized
            // so start-up does not encrypt it a second time. A wrong key
            // usually fails the padding check and returns null, but it can
            // pass by chance and return garbage, so only check that the
            // original text does not come back.
            const string plainText = "sk-proj-TH164_test-key";
#pragma warning disable CS0618 // Type or member is obsolete
            var encrypted = Encryption.EncryptString( plainText, Encryption.GenerateEncryptionKey( 128 ) );
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.AreNotEqual( plainText, Encryption.DecryptString( encrypted ) );
            Assert.IsTrue( AIProviderContainer.IsRockCiphertextShape( encrypted ) );
        }

        [DataTestMethod]
        [DataRow( "sk-proj-TH164_test-key" )]
        [DataRow( "sk-abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKL" )]
        [DataRow( "c2stcHJvai1USDE2NF90ZXN0LWtleQ==" )]
        [DataRow( "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" )]
        [DataRow( "" )]
        [DataRow( null )]
        public void IsRockCiphertextShape_PlainTextValue_ReturnsFalse( string value )
        {
            Assert.IsFalse( AIProviderContainer.IsRockCiphertextShape( value ) );
        }

        private static bool HasAttribute( PropertyInfo property, string attributeTypeName )
        {
            foreach ( var attribute in property.GetCustomAttributes( true ) )
            {
                if ( attribute.GetType().FullName == attributeTypeName )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
