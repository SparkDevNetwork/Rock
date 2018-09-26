using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Xunit;

using Rock;
using Rock.Storage;
using Rock.Storage.AssetStorage;
using Rock.Model;

namespace Rock.Tests.Rock.StorageTests
{
    /// <summary>
    /// Can't do these until we mock HttpContext.Current
    /// </summary>
    public class FileSystemComponentTests
    {
        private AssetStorageSystem GetAssetStorageSystem()
        {
            var assetStorageService = new AssetStorageSystemService( new Data.RockContext() );
            AssetStorageSystem assetStorageSystem = assetStorageService.Get( 3 );// need mock
            assetStorageSystem.LoadAttributes();
            assetStorageSystem.SetAttributeValue( "RootFolder", "TestFolder" );

            return assetStorageSystem;
        }

        [Fact( Skip = "Need to mock HttpContext.Current")]
        private void TestCreateFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();

            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Name = "TestFolder";
            asset.Type = AssetType.Folder;

            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestCreateFoldersInTestFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Name = "TestFolderA", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Key = "TetFolder/TestFolderA/A1", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Name = "TestFolderB", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Name = "TestFolderC", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Name = "TestFolderD", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Key = "TestFolder/TestFolderE/E1/E1a", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Key = "TestFolder/TestFolderE/E2/E2a", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageSystem, new Asset { Key = "TestFolder/TestFolderE/E3/E3a", Type = AssetType.Folder } ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestUploadObjectByKey()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = ( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            Assert.True( fileSystemComponent.UploadObject( assetStorageSystem, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestUploadObjectByName()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Name = ( "TestFolderA/TestUploadObjectByName.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            Assert.True( fileSystemComponent.UploadObject( assetStorageSystem, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListFoldersInFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListFoldersInFolder( assetStorageSystem, new Asset { Key = "~/TestFolder", Type = AssetType.Folder } );

            Assert.Contains( assets, a => a.Key == "TestFolderA" );
            Assert.Contains( assets, a => a.Name == "TestFolderB" );
            Assert.Contains( assets, a => a.Name == "TestFolderC" );
            Assert.Contains( assets, a => a.Name == "TestFolderD" );
            Assert.Contains( assets, a => a.Name == "TestFolderE" );
            Assert.DoesNotContain( assets, a => a.Name == "A1" );
            Assert.DoesNotContain( assets, a => a.Name == "E1" );
            Assert.DoesNotContain( assets, a => a.Name == "E1a" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListFilesInFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListFilesInFolder( assetStorageSystem, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

            Assert.Contains( assets, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assets, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.DoesNotContain( assets, a => a.Name == "A1" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListObjects()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListObjects( assetStorageSystem );

            Assert.Contains( assets, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assets, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assets, a => a.Name == "TestFolderA" );
            Assert.Contains( assets, a => a.Name == "TestFolderB" );
            Assert.Contains( assets, a => a.Name == "TestFolderC" );
            Assert.Contains( assets, a => a.Name == "TestFolderD" );
            Assert.Contains( assets, a => a.Name == "TestFolderE" );
            Assert.Contains( assets, a => a.Name == "A1" );
            Assert.Contains( assets, a => a.Name == "E1" );
            Assert.Contains( assets, a => a.Name == "E2" );
            Assert.Contains( assets, a => a.Name == "E3" );
            Assert.Contains( assets, a => a.Name == "E1a" );
            Assert.Contains( assets, a => a.Name == "E2a" );
            Assert.Contains( assets, a => a.Name == "E3a" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListObjectsInFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListObjectsInFolder( assetStorageSystem, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

            Assert.Contains( assets, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assets, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assets, a => a.Name == "A1" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestRenameAsset()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKey.jpg";

            Assert.True( fileSystemComponent.RenameAsset( assetStorageSystem, asset, "TestUploadObjectByKeyRenamed.jpg" ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestGetObject()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            var downloadedAsset = fileSystemComponent.GetObject( assetStorageSystem, asset );

            using ( FileStream fs = new FileStream( @"C:\temp\TestGetObjectDownloaded.jpg", FileMode.Create ) )
            using ( downloadedAsset.AssetStream )
            {
                downloadedAsset.AssetStream.CopyTo( fs );
            }
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestCreateDownloadLink()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            string link = fileSystemComponent.CreateDownloadLink( assetStorageSystem, asset );

            Uri uri = null;

            Assert.True( Uri.TryCreate( link, UriKind.Absolute, out uri ) );
            Assert.NotNull( uri );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestDeleteAssetFile()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            Assert.True( fileSystemComponent.DeleteAsset( assetStorageSystem, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestDeleteAssetFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var fileSystemComponent = assetStorageSystem.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.Folder;
            asset.Key = "TestFolder";

            Assert.True( fileSystemComponent.DeleteAsset( assetStorageSystem, asset ) );
        }
    }

}
