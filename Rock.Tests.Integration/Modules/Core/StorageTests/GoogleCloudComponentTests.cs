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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.StorageTests
{
    [Ignore( "These tests require Google Cloud configuration." )]
    [TestClass]
    public class GoogleCloudComponentTests : BaseStorageComponentTests
    {
        static GoogleCloudComponentTests()
        {
            _assetStorageProviderServiceGuid = "16B3AC07-1433-477B-9D27-2D20D926CAC8".AsGuid();
        }

        [ClassInitialize()]
        public static void ClassInit( TestContext context )
        {
            CreateAssetStorageProvider( context );

            ClassInitialize( context );
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            InternalClassCleanup();
        }

        private static void CreateAssetStorageProvider( TestContext testContext )
        {
            AssetStorageProvider assetStorageProvider = null;

            var bucketName = testContext.Properties["GoogleBucketName"].ToStringSafe();
            var apiKey = testContext.Properties["GoogleServiceAccountJsonKey"].ToStringSafe();

            // Just Assert Inconclusive if the GoogleStorageAccountName and GoogleAccountAccessKey are blank
            if ( string.IsNullOrEmpty( bucketName ) || string.IsNullOrEmpty( apiKey ) )
            {
                Assert.That.Inconclusive( $"The GoogleStorageAccountName and GoogleAccountAccessKey must be set up in your Test > Test Settings > Test Setting File in order to run these tests." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var assetStorageProviderService = new AssetStorageProviderService( rockContext );
                    assetStorageProvider = assetStorageProviderService.Get( _assetStorageProviderServiceGuid );

                    if ( assetStorageProvider == null )
                    {
                        // This is the registered Guid for the 'Rock.Storage.AssetStorage.GoogleCloudStorageComponent' entity type
                        var entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.STORAGE_ASSETSTORAGE_GOOGLECLOUD.AsGuid() );

                        assetStorageProvider = new AssetStorageProvider();
                        assetStorageProvider.Name = "TEST Google Cloud AssetStorageProvider";
                        assetStorageProvider.Guid = _assetStorageProviderServiceGuid;
                        assetStorageProvider.EntityTypeId = entityType.Id;
                        assetStorageProvider.IsActive = true;

                        assetStorageProviderService.Add( assetStorageProvider );

                        rockContext.SaveChanges();

                        var assetStorageProviderComponentEntityType = EntityTypeCache.Get( assetStorageProvider.EntityTypeId.Value );
                        var assetStorageProviderEntityType = EntityTypeCache.Get<Rock.Model.AssetStorageProvider>();

                        Helper.UpdateAttributes(
                                assetStorageProviderComponentEntityType.GetEntityType(),
                                assetStorageProviderEntityType.Id,
                                "EntityTypeId",
                                assetStorageProviderComponentEntityType.Id.ToString(),
                                rockContext );

                    }

                    assetStorageProvider.LoadAttributes();
                    assetStorageProvider.SetAttributeValue( "BucketName", bucketName );
                    assetStorageProvider.SetAttributeValue( "ApiKey", Encryption.EncryptString( apiKey ) );
                    assetStorageProvider.SetAttributeValue( "RootFolder", testContext.Properties["UnitTestRootFolder"].ToStringSafe() );
                    assetStorageProvider.SaveAttributeValues( rockContext );
                }
            }
            catch ( System.Exception ex )
            {
                Assert.That.Inconclusive( $"Unable to get the Google Cloud Asset Storage Provider ({ex.Message})." );
            }
        }


    }
}
