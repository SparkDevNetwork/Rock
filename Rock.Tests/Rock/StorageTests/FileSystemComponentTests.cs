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
        private AssetStorageProvider GetAssetStorageProvider()
        {
            var assetStorageService = new AssetStorageProviderService( new Data.RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageService.Get( 3 );// need mock
            assetStorageProvider.LoadAttributes();
            assetStorageProvider.SetAttributeValue( "RootFolder", "TestFolder" );

            return assetStorageProvider;
        }

        [Fact( Skip = "Need to mock HttpContext.Current")]
        private void TestCreateFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();

            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Name = "TestFolder";
            asset.Type = AssetType.Folder;

            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestCreateFoldersInTestFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderA", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TetFolder/TestFolderA/A1", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderB", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderC", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Name = "TestFolderD", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E1/E1a", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E2/E2a", Type = AssetType.Folder } ) );
            Assert.True( fileSystemComponent.CreateFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderE/E3/E3a", Type = AssetType.Folder } ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestUploadObjectByKey()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = ( "TestFolder/TestFolderA/TestUploadObjectByKey.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            Assert.True( fileSystemComponent.UploadObject( assetStorageProvider, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestUploadObjectByName()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Name = ( "TestFolderA/TestUploadObjectByName.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            Assert.True( fileSystemComponent.UploadObject( assetStorageProvider, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListFoldersInFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListFoldersInFolder( assetStorageProvider, new Asset { Key = "~/TestFolder", Type = AssetType.Folder } );

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
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListFilesInFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

            Assert.Contains( assets, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assets, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.DoesNotContain( assets, a => a.Name == "A1" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestListObjects()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListObjects( assetStorageProvider );

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
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var assets = fileSystemComponent.ListObjectsInFolder( assetStorageProvider, new Asset { Key = "TestFolder/TestFolderA", Type = AssetType.Folder } );

            Assert.Contains( assets, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assets, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assets, a => a.Name == "A1" );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestRenameAsset()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKey.jpg";

            Assert.True( fileSystemComponent.RenameAsset( assetStorageProvider, asset, "TestUploadObjectByKeyRenamed.jpg" ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestGetObject()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            var downloadedAsset = fileSystemComponent.GetObject( assetStorageProvider, asset, false );

            using ( FileStream fs = new FileStream( @"C:\temp\TestGetObjectDownloaded.jpg", FileMode.Create ) )
            using ( downloadedAsset.AssetStream )
            {
                downloadedAsset.AssetStream.CopyTo( fs );
            }
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestCreateDownloadLink()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            string link = fileSystemComponent.CreateDownloadLink( assetStorageProvider, asset );

            Uri uri = null;

            Assert.True( Uri.TryCreate( link, UriKind.Absolute, out uri ) );
            Assert.NotNull( uri );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestDeleteAssetFile()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "TestFolder/TestFolderA/TestUploadObjectByKeyRenamed.jpg";

            Assert.True( fileSystemComponent.DeleteAsset( assetStorageProvider, asset ) );
        }

        [Fact( Skip = "Need to mock HttpContext.Current" )]
        private void TestDeleteAssetFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var fileSystemComponent = assetStorageProvider.GetAssetStorageComponent();
            fileSystemComponent.FileSystemCompontHttpContext = HttpContext.Current;

            var asset = new Asset();
            asset.Type = AssetType.Folder;
            asset.Key = "TestFolder";

            Assert.True( fileSystemComponent.DeleteAsset( assetStorageProvider, asset ) );
        }
    }

}
