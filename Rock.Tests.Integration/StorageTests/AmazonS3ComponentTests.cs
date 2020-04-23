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
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.StorageTests
{
    /// <summary>
    /// This class tests the AmazonS3 storage component.
    /// It requires that you set your Test > Test Settings to use a Test Setting File
    /// to have the following parameters:
    ///
    ///    <!-- AWS Amazon S3 Storage Provider tests -->
    ///    <Parameter name = "AWSAccessKey" value="xxxxxxxxxxx" />
    ///    <Parameter name = "AWSSecretKey" value="xxxxxxxxxxx" />
    ///    <Parameter name = "AWSProfileName" value="xxxxxxxxxxx" />
    ///    <Parameter name = "AWSBucket" value="xxxxxxxxxxx" />
    ///    <Parameter name = "AWSRegion" value="xxxxxxxxxxx" />
    ///    <Parameter name = "UnitTestRootFolder" value="UnitTestFolder" />

    /// </summary>
    /// <remarks>
    /// Some data under the TestData folder is needed for these tests.  These are auto-magically copied
    /// from the build output directory to the deployment directory as described here:
    /// https://stackoverflow.com/questions/3738819/do-mstest-deployment-items-only-work-when-present-in-the-project-test-settings-f?rq=1
    /// </remarks>
    [TestClass]
    [DeploymentItem( @"TestData\", "TestData" )]
    public class AmazonS3ComponentTests
    {
        /// <summary>
        /// The amazon s3 test unique identifier (used only for tests).
        /// </summary>
        private static readonly System.Guid _AmazonS3TestGuid = "A3000000-8CF7-4441-A3FA-FB45AD1FF9B9".AsGuid();

        private static string webContentFolder = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            // Set up a fake webContentFolder that we will use with the Asset Storage component BECAUSE
            // there is a file-system dependency to determine the asset's file type (via GetFileTypeIcon)

            webContentFolder = Path.Combine( TestContext.DeploymentDirectory, "TestData", "Content" );
            EnsureFolder( webContentFolder );
        }

        /// <summary>
        /// Gets the asset storage provider, but note, it will add a test S3 Storage Provider
        /// if it could not be found.
        /// </summary>
        /// <returns>A S3 storage provider</returns>
        private AssetStorageProvider GetAssetStorageProvider()
        {
            AssetStorageProvider assetStorageProvider = null;
            using ( var rockContext = new RockContext() )
            {
                var assetStorageProviderService = new AssetStorageProviderService( rockContext );
                assetStorageProvider = assetStorageProviderService.Get( _AmazonS3TestGuid );
                var isNew = false;

                if ( assetStorageProvider == null )
                {
                    isNew = true;
                    var entityType = EntityTypeCache.Get( "FFE9C4A0-7AB7-48CA-8938-EC73DEC134E8".AsGuid() );

                    assetStorageProvider = new AssetStorageProvider();
                    assetStorageProvider.Name = "TEST Amazon S3 AssetStorageProvider";
                    assetStorageProvider.Guid = _AmazonS3TestGuid;
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
                assetStorageProvider.SetAttributeValue( "AWSRegion", TestContext.Properties["AWSRegion"].ToStringSafe() );
                assetStorageProvider.SetAttributeValue( "Bucket", TestContext.Properties["AWSBucket"].ToStringSafe() );
                assetStorageProvider.SetAttributeValue( "Expiration", "525600" );
                assetStorageProvider.SetAttributeValue( "RootFolder", TestContext.Properties["UnitTestRootFolder"].ToStringSafe() );
                assetStorageProvider.SetAttributeValue( "AWSProfileName", TestContext.Properties["AWSProfileName"].ToStringSafe() );
                assetStorageProvider.SetAttributeValue( "AWSAccessKey", TestContext.Properties["AWSAccessKey"].ToStringSafe() );
                assetStorageProvider.SetAttributeValue( "AWSSecretKey", TestContext.Properties["AWSSecretKey"].ToStringSafe() );
                
                if ( isNew )
                {
                    assetStorageProvider.SaveAttributeValues( rockContext );
                }
            }

            return assetStorageProvider;
        }

        //[TestMethod]
        //public void TestListRootBucket()
        //{
        //    var assetStorageProvider = GetAssetStorageProvider();
        //    var s3Component = assetStorageProvider.GetAssetStorageComponent();

        //    var asset = new Asset {
        //        Key = "UnitTestFolder/",
        //        Type = AssetType.Folder
        //    };

        //    var assets = s3Component.ListFoldersInFolder( assetStorageProvider, asset );
        //}

        /// <summary>
        /// Create a folder in the bucket using a key (the full name);
        /// This folder is used for other tests.
        /// </summary>
        [TestMethod]
        public void TestAWSCreateRootFolderUsingKey()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = assetStorageProvider.GetAttributeValue( "RootFolder" );
            asset.Type = AssetType.Folder;

            Assert.That.IsTrue( s3Component.CreateFolder( assetStorageProvider, asset ) );
        }

        /// <summary>
        /// Create folders using RootFolder and Asset.Name
        /// These folders are used for other tests.
        /// Requires TestAWSCreateFolderByKey
        /// </summary>
        [TestMethod]
        public void TestAWSCreateFolderByName()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Name = "SubFolder1/";
            asset.Type = AssetType.Folder;
            Assert.That.IsTrue( s3Component.CreateFolder( assetStorageProvider, asset ) );

            asset = new Asset();
            asset.Name = "SubFolder1/SubFolder1a/";
            asset.Type = AssetType.Folder;
            Assert.That.IsTrue( s3Component.CreateFolder( assetStorageProvider, asset ) );
        }

        /// <summary>
        /// Upload a file using RootFolder and Asset.Name.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [TestMethod]
        public void TestUploadObjectByName()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Name = ( "SubFolder1/TestUploadObjectByName.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );

                bool hasUploaded = s3Component.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );
            }
        }

        /// <summary>
        /// Upload a file using Asset.Key.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [TestMethod]
        public void TestUploadObjectByKey()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Key = ( "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );

                bool hasUploaded = s3Component.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );
            }
        }

        /// <summary>
        /// Get a recursive list of objects using Asset.Key
        /// </summary>
        [TestMethod]
        public void TestListObjectsByKey()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = ( "UnitTestFolder/" );

                var assetList = s3Component.ListObjects( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "UnitTestFolder" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "SubFolder1" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "SubFolder1a" ) );
            }
        }

        /// <summary>
        /// Get a list of files and folders in a single folder using RootFolder
        /// </summary>
        [TestMethod]
        public void TestListObjectsInFolder()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = "UnitTestFolder/SubFolder1/";
                asset.Type = AssetType.Folder;

                var assetList = s3Component.ListObjectsInFolder( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "SubFolder1" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "SubFolder1a" ) );
            }
        }

        /// <summary>
        /// Upload > 2K objects. Used to test listing that requires more than one request.
        /// </summary>
        [TestMethod]
        public void TestUpload2kObjects()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var subFolder = new Asset();
                subFolder.Name = "TwoThousandObjects/";
                subFolder.Type = AssetType.Folder;

                s3Component.CreateFolder( assetStorageProvider, subFolder );

                int i = 0;
                while ( i < 10 )
                {
                    subFolder = new Asset();
                    subFolder.Name = $"TwoThousandObjects/TestFolder-{i}/";
                    subFolder.Type = AssetType.Folder;

                    s3Component.CreateFolder( assetStorageProvider, subFolder );
                    i++;
                }

                FileStream fs;
                i = 0;
                while ( i < 2000 )
                {
                    using ( fs = new FileStream( @"TestData\TextDoc.txt", FileMode.Open ) )
                    {
                        Asset asset = new Asset { Name = $"TwoThousandObjects/TestFile-{i}.txt", AssetStream = fs };
                        s3Component.UploadObject( assetStorageProvider, asset );
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Get a list of keys that requires more than one request.
        /// </summary>
        [TestMethod]
        public void TestList2KObjectsInFolder()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = "UnitTestFolder/TwoThousandObjects/";
                asset.Type = AssetType.Folder;

                var assetList = s3Component.ListObjectsInFolder( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFile-0.txt" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFile-1368.txt" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-0" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-6" ) );
            }
        }

        /// <summary>
        /// Create a download link for an asset on the fly.
        /// </summary>
        [TestMethod]
        public void TestCreateDownloadLink()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";

                string url = s3Component.CreateDownloadLink( assetStorageProvider, asset );
                bool valid = false;

                try
                {
                    System.Net.HttpWebRequest request = System.Net.WebRequest.Create( url ) as System.Net.HttpWebRequest;
                    request.Method = "GET";
                    System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse;
                    response.Close();
                    valid = response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
                }
                catch
                {
                    valid = false;
                }

                Assert.That.IsTrue( valid );
            }
        }

        /// <summary>
        /// Get a file from storage.
        /// </summary>
        [TestMethod]
        public void TestGetObject()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                TestUploadObjectByName();

                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";

                bool valid = true;

                try
                {
                    var responseAsset = s3Component.GetObject( assetStorageProvider, asset, false );
                    using ( responseAsset.AssetStream )
                    using ( FileStream fs = new FileStream( $@"C:\temp\{responseAsset.Name}", FileMode.Create ) )
                    {
                        responseAsset.AssetStream.CopyTo( fs );
                    }
                }
                catch
                {
                    valid = false;
                }

                Assert.That.IsTrue( valid );
            }
        }

        /// <summary>
        /// List only the folders.
        /// </summary>
        [TestMethod]
        public void TestListFolders()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = "UnitTestFolder/TwoThousandObjects/";
                asset.Type = AssetType.Folder;

                var assetList = s3Component.ListFoldersInFolder( GetAssetStorageProvider(), asset );

                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-0" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-1" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-2" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-3" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-4" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-5" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-6" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-7" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-8" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-9" ) );
                Assert.That.IsFalse( assetList.Any( a => a.Name == "TestFile-0.txt" ) );
                Assert.That.IsFalse( assetList.Any( a => a.Name == "TestFile-1368.txt" ) );
            }
        }

        /// <summary>
        /// List only the files.
        /// </summary>
        [TestMethod]
        public void TestListFiles()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = "UnitTestFolder/SubFolder1/";
                asset.Type = AssetType.Folder;

                var assetList = s3Component.ListFilesInFolder( assetStorageProvider, asset );

                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsFalse( assetList.Any( a => a.Name == "SubFolder1a" ) );
            }
        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        [TestMethod]
        public void TestRenameAsset()
        {
            // Make sure the file is there so we can rename it
            TestUploadObjectByKey();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg";

                var origAssetList = s3Component.ListObjects( assetStorageProvider, asset );

                if ( origAssetList.Count == 1 )
                {
                    Assert.That.IsTrue( s3Component.RenameAsset( assetStorageProvider, asset, "TestUploadObjectByKeyRenamed.jpg" ) );

                    asset = new Asset();
                    asset.Type = AssetType.File;
                    asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey";

                    var assetList = s3Component.ListObjects( assetStorageProvider, asset );
                    Assert.That.IsTrue( assetList.Any( a => a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                    Assert.That.IsFalse( assetList.Any( a => a.Name == "TestUploadObjectByKey.jpg" ) );
                }
                else
                {
                    Assert.That.Inconclusive( "File (UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg) was not present for 'rename' test." );
                }
            }
        }

        /// <summary>
        /// Delete a single file.
        /// </summary>
        [TestMethod]
        public void TestDeleteFile()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";
            asset.Type = AssetType.File;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageProvider, asset );
            Assert.That.IsTrue( hasDeleted );
        }

        /// <summary>
        /// Delete all of the test data.
        /// </summary>
        [TestMethod]
        public void TestDeleteFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            // Create a folder so we can test the delete 
            var assetSetup = new Asset();
            assetSetup.Name = "DELETE_FOLDER/";
            assetSetup.Type = AssetType.Folder;
            Assert.That.IsTrue( s3Component.CreateFolder( assetStorageProvider, assetSetup ) );

            // Now we can run our test...
            Asset asset = new Asset();
            asset.Key = ( "DELETE_FOLDER/" );
            asset.Type = AssetType.Folder;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageProvider, asset );
            Assert.That.IsTrue( hasDeleted );
        }

        #region Utility Methods to help us fake the HttpContext

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        /// <value>
        /// The test context.
        /// </value>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        /// <summary>
        /// Ensures the folder exists for the upcoming test.
        /// </summary>
        /// <param name="foldername">The foldername.</param>
        private static void EnsureFolder( string foldername )
        {
            var path = Path.Combine( webContentFolder, foldername );
            Directory.CreateDirectory( path );
        }
        #endregion

    }
}
