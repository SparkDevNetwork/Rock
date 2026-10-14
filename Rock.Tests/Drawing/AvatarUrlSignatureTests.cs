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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Drawing.Avatar;
using Rock.Security;

namespace Rock.Tests.Drawing
{
    [TestClass]
    public class AvatarUrlSignatureTests
    {
        private const string FileIdKey = "Z9NB6v3Bo0";

        /// <summary>
        /// Fails every test in the class when the test-side signer no longer produces the same URL as the production signer.
        /// </summary>
        /// <remarks>
        /// The expired-URL test relies on the test-side signer, so without this check a format drift would make it pass
        /// for the wrong reason (a bad token instead of an expired one).
        /// </remarks>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            // Reading the day on both sides of the signing call keeps a run that crosses midnight from failing.
            var todayBefore = RockDateTime.Today;
            var productionUrl = Sign( Parameter( "fileIdKey", FileIdKey ) );
            var todayAfter = RockDateTime.Today;

            var isMatch = productionUrl == SignWithExpiration( FileIdKey, todayBefore.AddDays( 7 ) )
                || productionUrl == SignWithExpiration( FileIdKey, todayAfter.AddDays( 7 ) );

            Assert.IsTrue( isMatch, "The test-side signer no longer matches AvatarUrlSignature.GetSignedQueryString, so its signed URLs can't be trusted." );
        }

        #region Valid URLs

        [TestMethod]
        public void TryValidate_SignedUrl_IsValidUntilSevenDaysFromToday()
        {
            var todayBefore = RockDateTime.Today;
            var queryString = GetSignedQueryString();
            var todayAfter = RockDateTime.Today;

            var isValid = AvatarUrlSignature.TryValidate( queryString, out var expiresDateTime );

            Assert.IsTrue( isValid );
            Assert.IsTrue( expiresDateTime == todayBefore.AddDays( 7 ) || expiresDateTime == todayAfter.AddDays( 7 ), $"Unexpected expiration {expiresDateTime:s}." );
        }

        [TestMethod]
        public void TryValidate_AppendedDisplayParameters_IsValid()
        {
            var signed = Sign( Parameter( "fileIdKey", FileIdKey ), Parameter( "Size", "400" ) );
            var queryString = HttpUtility.ParseQueryString( $"{signed}&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA&width=60&Radius=circle" );

            Assert.IsTrue( AvatarUrlSignature.TryValidate( queryString, out _ ) );
        }

        [TestMethod]
        public void TryValidate_ReorderedAndRecasedKeys_IsValid()
        {
            var queryString = GetSignedQueryString();
            var reordered = $"t={queryString["t"]}&E={HttpUtility.UrlEncode( queryString["e"] )}&FILEIDKEY={queryString["fileIdKey"]}";

            Assert.IsTrue( AvatarUrlSignature.TryValidate( HttpUtility.ParseQueryString( reordered ), out _ ) );
        }

        [TestMethod]
        public void TryValidate_NoPhotoUrl_IsValid()
        {
            var signed = Sign( Parameter( "Text", "TM" ) );

            Assert.IsTrue( AvatarUrlSignature.TryValidate( HttpUtility.ParseQueryString( signed ), out _ ) );
        }

        #endregion Valid URLs

        #region Rejected URLs

        [TestMethod]
        public void TryValidate_ExpiredUrl_IsInvalid()
        {
            var queryString = HttpUtility.ParseQueryString( SignWithExpiration( FileIdKey, RockDateTime.Now.AddMinutes( -1 ) ) );

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_EditedFileIdKey_IsInvalid()
        {
            var queryString = GetSignedQueryString();
            queryString["fileIdKey"] = "Z9NB6v3Bo1";

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_ExtendedExpiration_IsInvalid()
        {
            var queryString = GetSignedQueryString();
            queryString["e"] = RockDateTime.Today.AddDays( 30 ).ToString( "s" );

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_AddedFileIdKeyToNoPhotoUrl_IsInvalid()
        {
            var signed = Sign( Parameter( "Text", "TM" ) );
            var queryString = HttpUtility.ParseQueryString( $"{signed}&fileIdKey={FileIdKey}" );

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_AppendedSecondFileIdKey_IsInvalid()
        {
            var signed = Sign( Parameter( "fileIdKey", FileIdKey ) );
            var queryString = HttpUtility.ParseQueryString( $"{signed}&fileIdKey=Z9NB6v3Bo1" );

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_MissingToken_IsInvalid()
        {
            var queryString = GetSignedQueryString();
            queryString.Remove( "t" );

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_MalformedExpiration_IsInvalid()
        {
            var queryString = GetSignedQueryString();
            queryString["e"] = "next-week";

            AssertIsRejected( queryString );
        }

        [TestMethod]
        public void TryValidate_UnsignedUrl_IsInvalid()
        {
            var queryString = HttpUtility.ParseQueryString( $"fileIdKey={FileIdKey}&Text=TD" );

            AssertIsRejected( queryString );
        }

        #endregion Rejected URLs

        #region Helpers

        private static KeyValuePair<string, string> Parameter( string key, string value )
        {
            return new KeyValuePair<string, string>( key, value );
        }

        private static string Sign( params KeyValuePair<string, string>[] parameters )
        {
            return AvatarUrlSignature.GetSignedQueryString( parameters );
        }

        private static void AssertIsRejected( NameValueCollection queryString )
        {
            var isValid = AvatarUrlSignature.TryValidate( queryString, out var expiresDateTime );

            Assert.IsFalse( isValid );
            Assert.AreEqual( DateTime.MinValue, expiresDateTime );
        }

        private static NameValueCollection GetSignedQueryString()
        {
            return HttpUtility.ParseQueryString( Sign( Parameter( "fileIdKey", FileIdKey ), Parameter( "Text", "TD" ), Parameter( "Size", "400" ) ) );
        }

        /// <summary>
        /// Signs a URL with an arbitrary expiration by building the documented canonical string here in the test,
        /// so production code needs no way to mint a URL with a chosen expiration.
        /// </summary>
        private static string SignWithExpiration( string fileIdKey, DateTime expiresDateTime )
        {
            var expires = expiresDateTime.ToString( "s" );
            var canonical = $"fileidkey={Uri.EscapeDataString( fileIdKey )}&e={Uri.EscapeDataString( expires )}";
            var signature = Encryption.ComputePurposeHmacSha256( Encoding.UTF8.GetBytes( canonical ), new[] { "Rock.Avatar.UrlSignature" } );
            var token = Base64UrlEncoder.Encode( signature );

            return $"fileIdKey={fileIdKey}&e={Uri.EscapeDataString( expires )}&t={token}";
        }

        #endregion Helpers
    }
}
