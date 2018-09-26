using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Storage;
using Rock.Storage.AssetStorage;
using Rock.Model;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Xunit;

namespace Rock.Tests.Rock.StorageTests
{
    public class AmazonS3ComponentTests
    {
        //private string AWSAccessKey = "";
        //private string AWSSecretKey = @"";
        //private RegionEndpoint AWSRegion = RegionEndpoint.USWest1;
        //private string Bucket = "rockphotostest0";
        //private string UnitTestRootFolder = "UnitTestFolder";

        private AssetStorageSystem GetAssetStorageSystem()
        {
            var assetStorageService = new AssetStorageSystemService( new Data.RockContext() );
            AssetStorageSystem assetStorageSystem = assetStorageService.Get( 1 );// need mock
            assetStorageSystem.LoadAttributes();
            assetStorageSystem.SetAttributeValue( "RootFolder", "UnitTestFolder" );

            return assetStorageSystem;
        }

        //[Fact]
        //public void TestListRootBucket()
        //{
        //    var assetStorageSystem = GetAssetStorageSystem();
        //    var s3Component = assetStorageSystem.GetAssetStorageComponent();

        //    var asset = new Asset {
        //        Key = "UnitTestFolder/",
        //        Type = AssetType.Folder
        //    };
            
        //    var assets = s3Component.ListFoldersInFolder( assetStorageSystem, asset );
        //}

        /// <summary>
        /// Create a folder in the bucket using a key (the full name);
        /// This folder is used for other tests.
        /// </summary>
        [Fact]
        public void TestAWSCreateRootFolderUsingKey()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = assetStorageSystem.GetAttributeValue( "RootFolder" );
            asset.Type = AssetType.Folder;

            Assert.True( s3Component.CreateFolder( assetStorageSystem, asset ) );
        }

        /// <summary>
        /// Create folders using RootFolder and Asset.Name
        /// These folders are used for other tests.
        /// Requires TestAWSCreateFolderByKey
        /// </summary>
        [Fact]
        public void TestAWSCreateFolderByName()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Name = "SubFolder1/";
            asset.Type = AssetType.Folder;
            Assert.True( s3Component.CreateFolder( assetStorageSystem, asset ) );

            asset = new Asset();
            asset.Name = "SubFolder1/SubFolder1a/";
            asset.Type = AssetType.Folder;
            Assert.True( s3Component.CreateFolder( assetStorageSystem, asset ) );
        }

        /// <summary>
        /// Upload a file using RootFolder and Asset.Name.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [Fact]
        public void TestUploadObjectByName()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Name = ( "SubFolder1/TestUploadObjectByName.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            bool hasUploaded = s3Component.UploadObject( assetStorageSystem, asset );
            Assert.True( hasUploaded );
        }

        /// <summary>
        /// Upload a file using Asset.Key.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [Fact]
        public void TestUploadObjectByKey()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = ( "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            bool hasUploaded = s3Component.UploadObject( assetStorageSystem, asset );
            Assert.True( hasUploaded );
        }

        /// <summary>
        /// Get a recursive list of objects using Asset.Key
        /// </summary>
        [Fact]
        public void TestListObjectsByKey()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = ( "UnitTestFolder/" );
            
            var assetList = s3Component.ListObjects( assetStorageSystem, asset );
            Assert.Contains( assetList, a => a.Name == "UnitTestFolder" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1a" );
        }

        /// <summary>
        /// Get a list of files and folders in a single folder using RootFolder
        /// </summary>
        [Fact]
        public void TestListObjectsInFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListObjectsInFolder( assetStorageSystem, asset );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1a" );
        }

        /// <summary>
        /// Upload > 2K objects. Used to test listing that requries more than one request.
        /// </summary>
        [Fact]
        public void TestUpload2kObjects()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();
            
            var subFolder = new Asset();
            subFolder.Name = "TwoThousandObjects/";
            subFolder.Type = AssetType.Folder;

            s3Component.CreateFolder( assetStorageSystem, subFolder );

            int i = 0;
            while ( i < 10 )
            {
                subFolder = new Asset();
                subFolder.Name = $"TwoThousandObjects/TestFolder-{i}/";
                subFolder.Type = AssetType.Folder;

                s3Component.CreateFolder( assetStorageSystem, subFolder );
                i++;
            }

            FileStream fs;
            i = 0;
            while ( i < 2000 )
            {
                using ( fs = new FileStream( @"C:\temp\TextDoc.txt", FileMode.Open ) )
                {
                    Asset asset = new Asset { Name = $"TwoThousandObjects/TestFile-{i}.txt", AssetStream = fs };
                    s3Component.UploadObject( assetStorageSystem, asset );
                    i++;
                }
            }
        }

        /// <summary>
        /// Get a list of keys that requires more than one request.
        /// </summary>
        [Fact]
        public void TestList2KObjectsInFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/TwoThousandObjects/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListObjectsInFolder( assetStorageSystem, asset );
            Assert.Contains( assetList, a => a.Name == "TestFile-0.txt" );
            Assert.Contains( assetList, a => a.Name == "TestFile-1368.txt" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-0" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-6" );
        }

        /// <summary>
        /// Create a download link for an asset on the fly.
        /// </summary>
        [Fact]
        public void TestCreateDownloadLink()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";

            string url = s3Component.CreateDownloadLink( assetStorageSystem, asset );
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

            Assert.True( valid );
        }

        /// <summary>
        /// Get a file from storage.
        /// </summary>
        [Fact]
        public void TestGetObject()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";

            bool valid = true;

            try
            {
                var responseAsset = s3Component.GetObject( assetStorageSystem, asset );
                using ( responseAsset.AssetStream)
                using ( FileStream fs = new FileStream( $@"C:\temp\{responseAsset.Name}", FileMode.Create ) )
                {
                    responseAsset.AssetStream.CopyTo( fs );
                }
            }
            catch
            {
                valid = false;
            }

            Assert.True( valid );
        }

        /// <summary>
        /// List only the folders.
        /// </summary>
        [Fact]
        public void TestListFolders()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/TwoThousandObjects/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListFoldersInFolder( GetAssetStorageSystem(), asset );

            Assert.Contains( assetList, a => a.Name == "TestFolder-0" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-1" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-2" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-3" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-4" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-5" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-6" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-7" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-8" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-9" );
            Assert.DoesNotContain( assetList, a => a.Name == "TestFile-0.txt" );
            Assert.DoesNotContain( assetList, a => a.Name == "TestFile-1368.txt" );
        }

        /// <summary>
        /// List only the files.
        /// </summary>
        [Fact]
        public void TestListFiles()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListFilesInFolder( assetStorageSystem, asset );

            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.DoesNotContain( assetList, a => a.Name == "SubFolder1a" );

        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        [Fact]
        public void TestRenameAsset()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg";

            Assert.True( s3Component.RenameAsset( assetStorageSystem, asset, "TestUploadObjectByKeyRenamed.jpg" ) );

            asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey";

            var assetList = s3Component.ListObjects( assetStorageSystem, asset );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKeyRenamed.jpg" );
            Assert.DoesNotContain( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
        }

        /// <summary>
        /// Delete a single file.
        /// </summary>
        [Fact]
        public void TestDeleteFile()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";
            asset.Type = AssetType.File;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageSystem, asset );
            Assert.True( hasDeleted );
        }

        /// <summary>
        /// Delete all of the test data.
        /// </summary>
        [Fact]
        public void TestDeleteFolder()
        {
            var assetStorageSystem = GetAssetStorageSystem();
            var s3Component = assetStorageSystem.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = ( "UnitTestFolder/" );
            asset.Type = AssetType.Folder;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageSystem, asset );
            Assert.True( hasDeleted );
        }
    }
}
