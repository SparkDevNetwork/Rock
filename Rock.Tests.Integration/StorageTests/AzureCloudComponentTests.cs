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
using Rock.Security;
using Rock.Storage.AssetStorage;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.StorageTests
{
    /// <summary>
    /// This class tests the Azure cloud storage component.
    /// It requires that you set your Test > Test Settings to use a Test Setting File
    /// to have the following parameters:
    ///
    ///    <!-- Azure Cloud Storage Provider tests -->
    //<Parameter name = "StorageAccountName" value="" />
    //<Parameter name = "AccountAccessKey" value="" />
    //<Parameter name = "DefaultContainerName" value="" />
    //<Parameter name = "CustomDomain" value="" />
    //<Parameter name = "UnitTestRootFolder" value="UnitTestFolder" />

    /// </summary>
    /// <remarks>
    /// Some data under the TestData folder is needed for these tests.  These are auto-magically copied
    /// from the build output directory to the deployment directory as described here:
    /// https://stackoverflow.com/questions/3738819/do-mstest-deployment-items-only-work-when-present-in-the-project-test-settings-f?rq=1
    /// </remarks>
    [TestClass]
    [DeploymentItem( @"TestData\", "TestData" )]
    public class AzureCloudComponentTests
    {
        /// <summary>
        /// The azure cloud test unique identifier (used only for tests).
        /// </summary>
        private static readonly System.Guid _AzureCloudTestGuid = "E7C23582-DB82-4912-AED0-7CDFBE3C452B".AsGuid();

        private static string webContentFolder = string.Empty;
        private static string appDataTempFolder = string.Empty;

        [ClassInitialize()]
        public static void ClassInit( TestContext context )
        {
            CreateAssetStorageProvider( context );

            // Set up a fake webContentFolder that we will use with the Asset Storage component BECAUSE
            // there is a file-system dependency to determine the asset's file type (via GetFileTypeIcon)

            webContentFolder = Path.Combine( context.DeploymentDirectory, "TestData", "Content" );
            EnsureFolder( webContentFolder );

            appDataTempFolder = Path.Combine( context.DeploymentDirectory, "App_Data", $"{System.Guid.NewGuid()}" );
            EnsureFolder( appDataTempFolder );

            SeedIntialData();
        }

        [TestInitialize]
        public void Initialize()
        {

        }

        /// <summary>
        /// Cleanups this any mess made by these tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if ( !string.IsNullOrEmpty( appDataTempFolder ) )
            {
                Directory.Delete( appDataTempFolder, recursive: true );
            }
        }

        /// <summary>
        /// Create folders using RootFolder and Asset.Name
        /// These folders are used for other tests.
        /// </summary>
        [TestMethod]
        public void TestAzureCreateFolderByName()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Name = "SampleFolder";
            asset.Type = AssetType.Folder;
            var isFolderCreated = azureComponent.CreateFolder( assetStorageProvider, asset );
            Assert.That.IsTrue( isFolderCreated );

            var assets = azureComponent.ListFoldersInFolder( assetStorageProvider );
            Assert.That.IsNotEmpty( assets );
            Assert.That.IsTrue( assets.Any( a => a.Type == AssetType.Folder && a.Key == assetStorageProvider.GetAttributeValue( "RootFolder" ) + "/SampleFolder/" ) );

            bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, asset );
            Assert.That.IsTrue( isSuccess, "New Created Folder can't be deleted in TestAzureCreateFolderByName" );
        }

        /// <summary>
        /// Upload a file using Asset.Name.
        /// </summary>
        [TestMethod]
        public void TestUploadObjectByName()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var azureComponent = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Name = ( "ParentFolder/TestFolder-1/TestUploadObjectByName.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );
                asset.Type = AssetType.File;

                bool hasUploaded = azureComponent.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );

                var getAsset = azureComponent.GetObject( assetStorageProvider, asset );
                Assert.That.IsNotNull( getAsset );
                Assert.That.Equal( "TestUploadObjectByName.jpg", getAsset.Name );

                bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, getAsset );
                Assert.That.IsTrue( isSuccess, "New uploaded file can't be deleted in TestUploadObjectByName" );
            }
        }

        /// <summary>
        /// Upload a file using Asset.Key.
        /// </summary>
        [TestMethod]
        public void TestUploadObjectByKey()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var azureComponent = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Key = ( assetStorageProvider.GetAttributeValue( "RootFolder" ) + "/ParentFolder/TestUploadObjectByKey.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );

                bool hasUploaded = azureComponent.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );

                var getAsset = azureComponent.GetObject( assetStorageProvider, asset );
                Assert.That.IsNotNull( getAsset );
                Assert.That.Equal( "TestUploadObjectByKey.jpg", getAsset.Name );

                bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, getAsset );
                Assert.That.IsTrue( isSuccess, "New uploaded file can't be deleted in TestUploadObjectByKey" );
            }
        }

        /// <summary>
        /// Get a recursive list of objects using Asset.Key
        /// </summary>
        [TestMethod]
        public void TestListObjects()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var azureComponent = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();

                var assetList = azureComponent.ListObjects( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "ParentFolder" ) );
            }
        }

        /// <summary>
        /// Get a list of files and folders in a single folder
        /// </summary>
        [TestMethod]
        public void TestListObjectsInFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Name = "ParentFolder/";
            asset.Type = AssetType.Folder;

            var assetList = azureComponent.ListObjectsInFolder( assetStorageProvider, asset );
            Assert.That.AreEqual( 10, assetList.Where( a => a.Type == AssetType.File ).Count() );
            for ( int i = 0; i < 10; i++ )
            {
                Assert.That.IsTrue( assetList.Any( a => a.Name == $"TestFile-{i}.txt" ) );
            }
        }

        /// <summary>
        /// List only the folders.  This represents a simple case where there are only a handful of
        /// files and folders under the UnitTestFolder/SimpleFolder/ folder.
        /// </summary>
        [TestMethod]
        public void TestListFoldersOfSimpleFolder()
        {
            // Make sure some simple files and folders are up there...
            TestUploadFewSimpleObjects();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var component = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();
                asset.Key = "UnitTestFolder/SimpleFolder/";
                asset.Type = AssetType.Folder;

                var assetList = component.ListFoldersInFolder( assetStorageProvider, asset );

                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-0" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-1" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-2" ) );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "TestFolder-3" ) );
                Assert.That.IsFalse( assetList.Any( a => a.Name == "TestFile-0.txt" ) );
                Assert.That.IsFalse( assetList.Any( a => a.Name == "TestFile-7.txt" ) );
            }
        }

        /// <summary>
        /// CRUD > 2K objects. Used to test listing that requires more than one request.
        /// </summary>
        [TestMethod]
        public void TestCrud2kObjects()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var subFolder = new Asset();
                subFolder.Name = "TwoThousandObjects";
                subFolder.Type = AssetType.Folder;

                azureComponent.CreateFolder( assetStorageProvider, subFolder );

                // Create 2000 files
                FileStream fs;
                int i = 0;
                while ( i < 2000 )
                {
                    using ( fs = new FileStream( @"TestData\TextDoc.txt", FileMode.Open ) )
                    {
                        Asset asset = new Asset { Name = $"TwoThousandObjects/TestFile-{i}.txt", AssetStream = fs };
                        azureComponent.UploadObject( assetStorageProvider, asset );
                        i++;
                    }
                }

                // Create 10 child folders (AFTER the 2000 files so we can try to cause some components
                // to fail the ListFoldersInFolder test due to the way they could be written).
                i = 0;
                Asset childFolder;
                while ( i < 10 )
                {
                    childFolder = new Asset();
                    childFolder.Name = $"TwoThousandObjects/TestFolder-{i}/";
                    childFolder.Type = AssetType.Folder;

                    azureComponent.CreateFolder( assetStorageProvider, childFolder );
                    i++;
                }

                subFolder = new Asset();
                subFolder.Name = "TwoThousandObjects/"; // <- why the trailing slash? Is that how all the providers behave?
                subFolder.Type = AssetType.Folder;

                // Check for all 2000 files:
                var assets = azureComponent.ListFilesInFolder( assetStorageProvider, subFolder );
                Assert.That.IsTrue( assets.Where( a => a.Name.Contains( "TestFile-" ) ).Count() == 2000, "Did not find all 2000 files." );

                // Check for all 10 sub folders:
                var foldersList = azureComponent.ListFoldersInFolder( assetStorageProvider, subFolder );
                Assert.That.IsTrue( foldersList.Where( a => a.Name.Contains( "TestFolder-" ) ).Count() == 10, "Did not find all 10 folders." );

                // Delete the whole TwoThousandObjects folder.
                var isDeleteSuccess = azureComponent.DeleteAsset( assetStorageProvider, subFolder );
                Assert.That.IsTrue( isDeleteSuccess );
            }
        }

        /// <summary>
        /// Create a download link for an asset on the fly.
        /// </summary>
        [TestMethod]
        public void TestCreateDownloadLink()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Asset asset = new Asset();
                asset.Name = ( "ParentFolder/TestFolder-1/TestDownloadLink.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );
                asset.Type = AssetType.File;

                bool hasUploaded = azureComponent.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );

                var getAsset = azureComponent.GetObject( assetStorageProvider, asset );
                Assert.That.IsNotNull( getAsset );

                string url = azureComponent.CreateDownloadLink( assetStorageProvider, asset );
                bool valid = false;

                try
                {
                    System.Net.HttpWebRequest request = System.Net.WebRequest.Create( url ) as System.Net.HttpWebRequest;
                    request.Method = "GET";
                    System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse;
                    response.Close();
                    valid = response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
                }
                catch ( System.Net.WebException e )
                {
                    using ( System.Net.WebResponse response = e.Response )
                    {
                        System.Net.HttpWebResponse httpResponse = ( System.Net.HttpWebResponse ) response;
                        if ( httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden )
                        {
                            Assert.That.Inconclusive( $"File ({asset.Key}) was not forbidden from viewing." );
                        }
                    }
                }

                Assert.That.IsTrue( valid );

                bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, getAsset );
                Assert.That.IsTrue( isSuccess, "Download Link Test file can't be deleted in TestUploadObjectByName" );
            }
        }

        /// <summary>
        /// Get a file from storage.
        /// </summary>
        [TestMethod]
        public void TestGetObject()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Asset asset = new Asset();
                asset.Name = ( "ParentFolder/TestFolder-1/GetOject.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );
                asset.Type = AssetType.File;

                bool hasUploaded = azureComponent.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );

                bool valid = true;

                var getAsset = azureComponent.GetObject( assetStorageProvider, asset );
                Assert.That.IsNotNull( getAsset );
                Assert.That.Equal( "GetOject.jpg", getAsset.Name );

                try
                {
                    using ( getAsset.AssetStream )
                    using ( FileStream fs = new FileStream( Path.Combine( appDataTempFolder, getAsset.Name ), FileMode.Create ) )
                    {
                        getAsset.AssetStream.CopyTo( fs );
                    }
                }
                catch
                {
                    valid = false;
                }

                Assert.That.IsTrue( valid );

                bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, getAsset );
                Assert.That.IsTrue( isSuccess, "Get object file can't be deleted in TestUploadObjectByName" );
            }
        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        [TestMethod]
        public void TestRenameAsset()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Asset asset = new Asset();
                asset.Name = "ParentFolder/TestFolder-1/RenameObject.jpg" ;
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );
                asset.Type = AssetType.File;

                bool hasUploaded = azureComponent.UploadObject( assetStorageProvider, asset );
                Assert.That.IsTrue( hasUploaded );

                Asset folderAsset = new Asset();
                folderAsset.Type = AssetType.Folder;
                folderAsset.Name = "ParentFolder/TestFolder-1/";
                var fileList = azureComponent.ListFilesInFolder( assetStorageProvider, folderAsset );
                Assert.That.IsNotNull( fileList );
                Assert.That.IsTrue( fileList.Any( a => a.Name == "RenameObject.jpg" ) );
                Assert.That.IsFalse( fileList.Any( a => a.Name == "NewNameObject.jpg" ) );

                var isRenameSuccess = azureComponent.RenameAsset( assetStorageProvider, asset, "NewNameObject.jpg" );
                Assert.That.IsTrue( isRenameSuccess );

                fileList = azureComponent.ListFilesInFolder( assetStorageProvider, folderAsset );
                Assert.That.IsNotNull( fileList );
                Assert.That.IsTrue( fileList.Any( a => a.Name == "NewNameObject.jpg" ) );
                Assert.That.IsFalse( fileList.Any( a => a.Name == "RenameObject.jpg" ) );

                asset = new Asset();
                asset.Type = AssetType.File;
                asset.Name = "ParentFolder/TestFolder-1/NewNameObject.jpg";
                bool isSuccess = azureComponent.DeleteAsset( assetStorageProvider, asset );
                Assert.That.IsTrue( isSuccess, "Rename file can't be deleted" );
            }
        }

        /// <summary>
        /// Delete all of the test data.
        /// </summary>
        [TestMethod]
        public void TestDeleteFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            // Create a folder so we can test the delete 
            var assetSetup = new Asset();
            assetSetup.Name = "DELETE_FOLDER/";
            assetSetup.Type = AssetType.Folder;
            Assert.That.IsTrue( azureComponent.CreateFolder( assetStorageProvider, assetSetup ) );

            var folders = azureComponent.ListFoldersInFolder( assetStorageProvider );
            Assert.That.True( folders.Any( a => a.Name == "DELETE_FOLDER" ) );

            // Now we can run our test...
            Asset asset = new Asset();
            asset.Key = ( "DELETE_FOLDER/" );
            asset.Type = AssetType.Folder;

            bool hasDeleted = azureComponent.DeleteAsset( assetStorageProvider, asset );
            Assert.That.IsTrue( hasDeleted );

            folders = azureComponent.ListFoldersInFolder( assetStorageProvider );
            Assert.That.True( folders.Any( a => a.Name == "DELETE_FOLDER" ) );
        }

        /// <summary>
        /// Deletes everything that was created during the unit tests.
        /// </summary>
        [ClassCleanup()]
        public static void ClassCleanup()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            var assetStorageProvider = assetStorageProviderService.Get( _AzureCloudTestGuid );
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            var parentFolder = new Asset();
            parentFolder.Name = "ParentFolder";
            parentFolder.Type = AssetType.Folder;

            azureComponent.DeleteAsset( assetStorageProvider, parentFolder );
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

        #region Private Methods
        /// <summary>
        /// Add a test Azure Storage Provider
        /// if it could not be found.
        /// </summary>
        private static void CreateAssetStorageProvider( TestContext testContext )
        {
            AssetStorageProvider assetStorageProvider = null;

            var storageAccountName = testContext.Properties["AzureStorageAccountName"].ToStringSafe();
            var accountAccessKey = testContext.Properties["AzureAccountAccessKey"].ToStringSafe();
            var defaultContainerName = testContext.Properties["AzureDefaultContainerName"].ToStringSafe();

            // Just Assert Inconclusive if the AzureStorageAccountName and AzureAccountAccessKey are blank
            if ( string.IsNullOrEmpty( storageAccountName ) || string.IsNullOrEmpty( accountAccessKey ) || string.IsNullOrEmpty( defaultContainerName ) )
            {
                Assert.That.Inconclusive( $"The AzureStorageAccountName, AzureAccountAccessKey and AzureDefaultContainerName must be set up in your Test > Test Settings > Test Setting File in order to run these tests." );
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var assetStorageProviderService = new AssetStorageProviderService( rockContext );
                    assetStorageProvider = assetStorageProviderService.Get( _AzureCloudTestGuid );

                    if ( assetStorageProvider == null )
                    {
                        // This is the registered Guid for the 'Rock.Storage.AssetStorage.AzureCloudStorageComponent' entity type
                        var entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.STORAGE_ASSETSTORAGE_AZURECLOUD.AsGuid() );

                        assetStorageProvider = new AssetStorageProvider();
                        assetStorageProvider.Name = "TEST Azure Cloud AssetStorageProvider";
                        assetStorageProvider.Guid = _AzureCloudTestGuid;
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
                    assetStorageProvider.SetAttributeValue( "StorageAccountName", storageAccountName );
                    assetStorageProvider.SetAttributeValue( "AccountAccessKey", Encryption.EncryptString( accountAccessKey ) );
                    assetStorageProvider.SetAttributeValue( "RootFolder", testContext.Properties["UnitTestRootFolder"].ToStringSafe() );
                    assetStorageProvider.SetAttributeValue( "DefaultContainerName", defaultContainerName );
                    assetStorageProvider.SetAttributeValue( "CustomDomain", testContext.Properties["AzureCustomDomain"].ToStringSafe() );
                    assetStorageProvider.SaveAttributeValues( rockContext );
                }
            }
            catch ( System.Exception ex )
            {
                Assert.That.Inconclusive( $"Unable to get the Azure Cloud Asset Storage Provider ({ex.Message})." );
            }
        }

        /// <summary>
        /// Seed Initial Data
        /// </summary>
        private static void SeedIntialData()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            var assetStorageProvider = assetStorageProviderService.Get( _AzureCloudTestGuid );
            assetStorageProvider.LoadAttributes();
            var azureComponent = assetStorageProvider.GetAssetStorageComponent();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var rootAsset = new Asset();
                rootAsset.Key = assetStorageProvider.GetAttributeValue( "RootFolder" );
                rootAsset.Type = AssetType.Folder;

                bool isSuccess = azureComponent.CreateFolder( assetStorageProvider, rootAsset );
                if ( !isSuccess )
                {
                    Assert.That.Inconclusive( $"Unable to create root folder while seeding data" );
                }

                var parentFolder = new Asset();
                parentFolder.Name = "ParentFolder";
                parentFolder.Type = AssetType.Folder;

                isSuccess = azureComponent.CreateFolder( assetStorageProvider, parentFolder );

                int i = 0;
                while ( i < 5 )
                {
                    parentFolder = new Asset();
                    parentFolder.Name = $"ParentFolder/TestFolder-{i}/";
                    parentFolder.Type = AssetType.Folder;
                    azureComponent.CreateFolder( assetStorageProvider, parentFolder );
                    i++;
                }

                FileStream fs;
                i = 0;
                while ( i < 10 )
                {
                    using ( fs = new FileStream( @"TestData\TextDoc.txt", FileMode.Open ) )
                    {
                        Asset asset = new Asset { Name = $"ParentFolder/TestFile-{i}.txt", AssetStream = fs };
                        azureComponent.UploadObject( assetStorageProvider, asset );
                        i++;
                    }
                }
            }
        }

        private AssetStorageProvider GetAssetStorageProvider()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageProviderService.Get( _AzureCloudTestGuid );
            assetStorageProvider.LoadAttributes();
            return assetStorageProvider;
        }

        /// <summary>
        /// Upload a small set of folders and files.  Used for simple folder/file listing that requires ONLY one request.
        /// </summary>
        private void TestUploadFewSimpleObjects()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var s3Component = assetStorageProvider.GetAssetStorageComponent();

                var subFolder = new Asset();
                subFolder.Name = "SimpleFolder/";
                subFolder.Type = AssetType.Folder;

                s3Component.CreateFolder( assetStorageProvider, subFolder );

                int i = 0;
                while ( i < 5 )
                {
                    subFolder = new Asset();
                    subFolder.Name = $"SimpleFolder/TestFolder-{i}/";
                    subFolder.Type = AssetType.Folder;

                    s3Component.CreateFolder( assetStorageProvider, subFolder );
                    i++;
                }

                FileStream fs;
                i = 0;
                while ( i < 10 )
                {
                    using ( fs = new FileStream( @"TestData\TextDoc.txt", FileMode.Open ) )
                    {
                        Asset asset = new Asset { Name = $"SimpleFolder/TestFile-{i}.txt", AssetStream = fs };
                        s3Component.UploadObject( assetStorageProvider, asset );
                        i++;
                    }
                }
            }
        }
        #endregion Private Methods

    }
}
