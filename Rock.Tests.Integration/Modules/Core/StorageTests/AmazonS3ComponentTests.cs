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
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.StorageTests
{
    [Ignore( "These tests require Amazon Cloud configuration." )]
    [TestClass]
    public class AmazonCloudComponentTests : BaseStorageComponentTests
    {
        static AmazonCloudComponentTests()
        {
            _assetStorageProviderServiceGuid = "A3000000-8CF7-4441-A3FA-FB45AD1FF9B9".AsGuid();
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

            var accessKey = testContext.Properties["AWSAccessKey"].ToStringSafe();
            var secretKey = testContext.Properties["AWSSecretKey"].ToStringSafe();

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var assetStorageProviderService = new AssetStorageProviderService( rockContext );
                    assetStorageProvider = assetStorageProviderService.Get( _assetStorageProviderServiceGuid );

                    if ( assetStorageProvider == null )
                    {
                        var entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.STORAGE_ASSETSTORAGE_AMAZONS3.AsGuid() );

                        assetStorageProvider = new AssetStorageProvider();
                        assetStorageProvider.Name = "TEST Amazon S3 AssetStorageProvider";
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
                    assetStorageProvider.SetAttributeValue( "GenerateSingedURLs", "False" ); // The attribute Key has that typo.
                    assetStorageProvider.SetAttributeValue( "AWSRegion", testContext.Properties["AWSRegion"].ToStringSafe() );
                    assetStorageProvider.SetAttributeValue( "Bucket", testContext.Properties["AWSBucket"].ToStringSafe() );
                    assetStorageProvider.SetAttributeValue( "Expiration", "525600" );
                    assetStorageProvider.SetAttributeValue( "RootFolder", testContext.Properties["UnitTestRootFolder"].ToStringSafe() );
                    assetStorageProvider.SetAttributeValue( "AWSProfileName", testContext.Properties["AWSProfileName"].ToStringSafe() );
                    assetStorageProvider.SetAttributeValue( "AWSAccessKey", accessKey );
                    assetStorageProvider.SetAttributeValue( "AWSSecretKey", secretKey );

                    assetStorageProvider.SaveAttributeValues( rockContext );
                }
            }
            catch ( System.Exception ex )
            {
                Assert.That.Inconclusive( $"Unable to get the Amazon S3 Asset Storage Provider ({ex.Message})." );
            }
        }
    }
}
