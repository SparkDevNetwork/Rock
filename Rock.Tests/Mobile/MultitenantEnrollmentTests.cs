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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Mobile;

namespace Rock.Tests.Mobile
{
    [TestClass]
    public class MultitenantEnrollmentTests
    {
        #region ValidateDirectoryDetails

        [TestMethod]
        public void ValidateDirectoryDetails_WithValidValues_ReturnsNull()
        {
            Assert.IsNull( MultitenantEnrollment.ValidateDirectoryDetails( "Grace Community Church", "#2E6BE6" ) );
        }

        [TestMethod]
        public void ValidateDirectoryDetails_WithLowercaseColor_ReturnsNull()
        {
            Assert.IsNull( MultitenantEnrollment.ValidateDirectoryDetails( "Grace", "#2e6be6" ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        public void ValidateDirectoryDetails_WithMissingName_ReturnsError( string name )
        {
            Assert.IsNotNull( MultitenantEnrollment.ValidateDirectoryDetails( name, "#2E6BE6" ) );
        }

        [TestMethod]
        public void ValidateDirectoryDetails_WithNameAtLimit_ReturnsNull()
        {
            var name = new string( 'a', MultitenantEnrollment.MaximumNameLength );

            Assert.IsNull( MultitenantEnrollment.ValidateDirectoryDetails( name, "#2E6BE6" ) );
        }

        [TestMethod]
        public void ValidateDirectoryDetails_WithNameOverLimit_ReturnsError()
        {
            var name = new string( 'a', MultitenantEnrollment.MaximumNameLength + 1 );

            Assert.IsNotNull( MultitenantEnrollment.ValidateDirectoryDetails( name, "#2E6BE6" ) );
        }

        [TestMethod]
        public void ValidateDirectoryDetails_WithSurroundingSpacesOnly_CountsTrimmedLength()
        {
            var name = $"  {new string( 'a', MultitenantEnrollment.MaximumNameLength )}  ";

            Assert.IsNull( MultitenantEnrollment.ValidateDirectoryDetails( name, "#2E6BE6" ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "red" )]
        [DataRow( "#FFF" )]
        [DataRow( "2E6BE6" )]
        [DataRow( "#2E6BE6FF" )]
        [DataRow( "rgba(46,107,230,1)" )]
        public void ValidateDirectoryDetails_WithInvalidColor_ReturnsError( string brandColor )
        {
            Assert.IsNotNull( MultitenantEnrollment.ValidateDirectoryDetails( "Grace", brandColor ) );
        }

        #endregion

        #region BuildConfigurationRequest

        [TestMethod]
        public void BuildConfigurationRequest_SendsCampusesExactlyAsSerialized()
        {
            var campusesJson = "[{\"name\":\"Tempe\",\"city\":\"Tempe\",\"state\":\"AZ\",\"lat\":33.4255,\"long\":-111.94}]";

            var request = MultitenantEnrollment.BuildConfigurationRequest( "Grace", "#2E6BE6", null, "https://rock.example.org/api", "key", campusesJson );
            var json = JsonSerializer.Serialize( request, ConnectedServicesProvider.JsonOptions );

            // The campus hash is taken over this array, so its fields and values must reach the wire unchanged.
            StringAssert.Contains( json, $"\"campuses\":{campusesJson}" );
        }

        [TestMethod]
        public void BuildConfigurationRequest_WithoutLogo_OmitsLogoUrl()
        {
            var request = MultitenantEnrollment.BuildConfigurationRequest( "Grace", "#2E6BE6", null, "https://rock.example.org/api", "key", "[]" );
            var json = JsonSerializer.Serialize( request, ConnectedServicesProvider.JsonOptions );

            // An absent logo must be absent in the body, which tells the directory to remove it.
            Assert.DoesNotContain( "logoUrl", json );
        }

        [TestMethod]
        public void BuildConfigurationRequest_UsesDirectoryFieldNames()
        {
            var request = MultitenantEnrollment.BuildConfigurationRequest( "Grace", "#2E6BE6", "https://rock.example.org/GetImage.ashx?guid=1", "https://rock.example.org/api", "key", "[]" );
            var json = JsonSerializer.Serialize( request, ConnectedServicesProvider.JsonOptions );

            Assert.AreEqual( "{\"name\":\"Grace\",\"branding\":{\"brandColor\":\"#2E6BE6\",\"logoUrl\":\"https://rock.example.org/GetImage.ashx?guid=1\"},\"connection\":{\"apiUrl\":\"https://rock.example.org/api\",\"apiKey\":\"key\"},\"campuses\":[]}", json );
        }

        #endregion

        #region FakeMobileAppGateway

        [TestMethod]
        public async Task FakeMobileAppGateway_ReturnsSameCodeOnEveryCall()
        {
            var gateway = new FakeMobileAppGateway( new Guid( "3F2C0D7E-8B1A-4C55-9E6F-1A2B3C4D5E6F" ) );

            var first = await gateway.SetConfigurationAsync( null, CancellationToken.None );
            var second = await gateway.SetConfigurationAsync( null, CancellationToken.None );

            Assert.IsTrue( first.IsSuccess );
            Assert.AreEqual( first.Data.ChurchCode, second.Data.ChurchCode );
        }

        [TestMethod]
        public void FakeMobileAppGateway_DifferentInstances_GetDifferentCodes()
        {
            var first = new FakeMobileAppGateway( new Guid( "3F2C0D7E-8B1A-4C55-9E6F-1A2B3C4D5E6F" ) ).GetChurchCode();
            var second = new FakeMobileAppGateway( new Guid( "9A8B7C6D-5E4F-4A3B-8C2D-1E0F9A8B7C6D" ) ).GetChurchCode();

            Assert.AreNotEqual( first, second );
        }

        [TestMethod]
        public async Task FakeMobileAppGateway_LinkUsesShellTestHost()
        {
            var gateway = new FakeMobileAppGateway( new Guid( "3F2C0D7E-8B1A-4C55-9E6F-1A2B3C4D5E6F" ) );

            var result = await gateway.SetConfigurationAsync( null, CancellationToken.None );

            // The Debug shell recognizes church links on this host only.
            Assert.AreEqual( $"https://argus.spark.test/c/{result.Data.ChurchCode}", result.Data.Link );
        }

        [TestMethod]
        public void FakeMobileAppGateway_CodeHasEightUnambiguousCharacters()
        {
            var code = new FakeMobileAppGateway( new Guid( "3F2C0D7E-8B1A-4C55-9E6F-1A2B3C4D5E6F" ) ).GetChurchCode();

            Assert.AreEqual( 8, code.Length );
            Assert.IsTrue( System.Text.RegularExpressions.Regex.IsMatch( code, "^[2-9A-HJKMNP-TV-Z]{8}$" ), $"Unexpected character in {code}." );
        }

        #endregion
    }
}
