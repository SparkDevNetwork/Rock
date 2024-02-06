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
using System.IO;
using System.Linq;

using Http.TestLibrary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.StorageTests
{
    /// <summary>
    /// This class tests the FileSystem storage component. It currently requires
    /// that your storage provider be defined in Rock with ID #3.
    /// </summary>
    /// <remarks>
    /// Some data under the TestData folder is needed for these tests.  These are auto-magically copied
    /// from the build output directory to the deployment directory as described here:
    /// https://stackoverflow.com/questions/3738819/do-mstest-deployment-items-only-work-when-present-in-the-project-test-settings-f?rq=1
    /// </remarks>
    [TestClass]
    [DeploymentItem( @"TestData\", "TestData" )]
    public class FileSystemComponentTests : DatabaseTestsBase
    {
        private static string webContentFolder = string.Empty;
        private static string appDataTempFolder = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            // Set up a fake webContentFolder that we will use with the Asset Storage component
            webContentFolder = Path.Combine( TestContext.DeploymentDirectory, "TestData", "Content" );
            EnsureFolder( webContentFolder );

            appDataTempFolder = Path.Combine( TestContext.DeploymentDirectory, "App_Data", $"{System.Guid.NewGuid()}" );
            EnsureFolder( appDataTempFolder );
        }

        [ClassCleanup]
        public static void CleanupFolder()
        {
            // WARNING: Only delete if this is the TestFolder/Content folder we setup.
            if ( webContentFolder.EndsWith( "\\TestData\\Content" ) )
            {
                Directory.Delete( webContentFolder, recursive: true );
            }

            if ( !string.IsNullOrEmpty( appDataTempFolder ) )
            {
                Directory.Delete( appDataTempFolder, recursive: true );
            }
        }

        private AssetStorageProvider GetAssetStorageProvider()
        {
            var assetStorageService = new AssetStorageProviderService( new RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageService.Get( 1 ); // this is the stock, local file system provider
            assetStorageProvider.LoadAttributes();
            assetStorageProvider.SetAttributeValue( "RootFolder", "TestFolder" );

            return assetStorageProvider;
        }

        [TestMethod]
        public void TestCreateFolder()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();

                Asset asset = new Asset();
                asset.Name = "TestFolder";
                asset.Type = AssetType.Folder;

                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, asset ) );
            }
        }

        [TestMethod]
        public void TestCreateFoldersInTestFolder()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();

                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderA", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderA/A1", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderB", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderC", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderD", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E1/E1a", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E2/E2a", Type = AssetType.Folder } ) );
                Assert.That.IsTrue( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E3/E3a", Type = AssetType.Folder } ) );
            }
        }

        [TestMethod]
        public void TestUploadObjectByKey()
        {
            TestCreateFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                Asset asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = ( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );

                Assert.That.IsTrue( fileSystemComponent.UploadObject( assetStorageProvider, asset ) );
            }
        }

        [TestMethod]
        public void TestUploadObjectByName()
        {
            TestCreateFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                Asset asset = new Asset();
                asset.Type = AssetType.File;
                asset.Name = ( "TestFolderA/TestUploadObjectByName.jpg" );
                asset.AssetStream = new FileStream( @"TestData\test.jpg", FileMode.Open );

                Assert.That.IsTrue( fileSystemComponent.UploadObject( assetStorageProvider, asset ) );
            }
        }

        [TestMethod]
        public void TestListFoldersInFolder()
        {
            TestCreateFoldersInTestFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var assets = fileSystemComponent.ListFoldersInFolder( assetStorageProvider, new Asset { Key = "~/TestFolder", Type = AssetType.Folder } );

                Assert.That.IsTrue( assets.Any( a => a.Key == "~/TestFolder/TestFolderA/" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderB" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderC" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderD" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderE" ) );
                Assert.That.IsFalse( assets.Any( a => a.Name == "A1" ) );
                Assert.That.IsFalse( assets.Any( a => a.Name == "E1" ) );
                Assert.That.IsFalse( assets.Any( a => a.Name == "E1a" ) );
            }
        }

        [TestMethod]
        public void TestListFilesInFolder()
        {
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByName.jpg" );
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            TestUploadObjectByName();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var assets = fileSystemComponent.ListFilesInFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsFalse( assets.Any( a => a.Name == "A1" ) );
            }
        }

        [TestMethod]
        public void TestListObjects()
        {
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByName.jpg" );
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            TestCreateFoldersInTestFolder();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var assets = fileSystemComponent.ListObjects( assetStorageProvider );

                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderA" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderB" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderC" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderD" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestFolderE" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "A1" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E1" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E2" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E3" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E1a" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E2a" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "E3a" ) );
            }
        }

        [TestMethod]
        public void TestListObjectsInFolder()
        {
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByName.jpg" );
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            EnsureFolder( "TestFolder/TestFolderA/A1" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var assets = fileSystemComponent.ListObjectsInFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByName.jpg" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "TestUploadObjectByKey.jpg" || a.Name == "TestUploadObjectByKeyRenamed.jpg" ) );
                Assert.That.IsTrue( assets.Any( a => a.Name == "A1" ) );
            }
        }

        [TestMethod]
        public void TestRenameAsset()
        {
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            EnsureFileNotExists( "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKey.jpg";

                Assert.That.IsTrue( fileSystemComponent.RenameAsset( assetStorageProvider, asset, "TestUploadObjectByKeyRenamed.jpg" ) );
            }
        }

        [TestMethod]
        public void TestGetObject()
        {
            EnsureFile( "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

                var downloadedAsset = fileSystemComponent.GetObject( assetStorageProvider, asset, false );

                using ( FileStream fs = new FileStream( Path.Combine( appDataTempFolder, @"TestGetObjectDownloaded.jpg" ), FileMode.Create ) )
                using ( downloadedAsset.AssetStream )
                {
                    downloadedAsset.AssetStream.CopyTo( fs );
                }
            }
        }

        [TestMethod]
        public void TestCreateDownloadLink()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

                string link = fileSystemComponent.CreateDownloadLink( assetStorageProvider, asset );

                Uri uri = null;

                Assert.That.IsTrue( Uri.TryCreate( link, UriKind.Absolute, out uri ) );
                Assert.That.IsNotNull( uri );
            }
        }

        [TestMethod]
        public void TestDeleteAssetFile()
        {
            EnsureFile( "TestFolder/DELETEFILE.jpg" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var asset = new Asset();
                asset.Type = AssetType.File;
                asset.Key = "TestFolder/DELETEFILE.jpg";

                Assert.That.IsTrue( fileSystemComponent.DeleteAsset( assetStorageProvider, asset ) );
            }
        }

        [TestMethod]
        public void TestDeleteAssetFolder()
        {
            EnsureFolder( "TestFolder/TestFolderDELETEME" );

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
                
                var asset = new Asset();
                asset.Type = AssetType.Folder;
                asset.Key = "TestFolder/TestFolderDELETEME";

                Assert.That.IsTrue( fileSystemComponent.DeleteAsset( assetStorageProvider, asset ) );
            }
        }

        #region Utility Methods to help us fake the HttpContext

        /// <summary>
        /// Ensures the path and file exists for the upcoming test.
        /// </summary>
        /// <param name="file">The file.</param>
        private static void EnsureFile( string file )
        {
            var pathAndFile = Path.Combine( webContentFolder, file );
            var path = Path.GetDirectoryName( pathAndFile );
            var filename = Path.GetFileName( pathAndFile );
            Directory.CreateDirectory( path );
            if ( !string.IsNullOrEmpty( filename ) )
            {
                File.Copy( @"TestData\test.jpg", pathAndFile, true );
            }
        }

        /// <summary>
        /// Ensures the file does not exist for the upcoming test.
        /// </summary>
        /// <param name="file">The file.</param>
        private static void EnsureFileNotExists( string file )
        {
            var pathAndFile = Path.Combine( webContentFolder, file );
            if ( File.Exists( pathAndFile ) )
            {
                File.Delete( pathAndFile );
            }
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
