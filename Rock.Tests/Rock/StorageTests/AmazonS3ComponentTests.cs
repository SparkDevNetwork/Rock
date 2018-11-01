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
        private AssetStorageProvider GetAssetStorageProvider()
        {
            var assetStorageService = new AssetStorageProviderService( new Data.RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageService.Get( 1 );// need mock
            assetStorageProvider.LoadAttributes();
            assetStorageProvider.SetAttributeValue( "RootFolder", "UnitTestFolder" );

            return assetStorageProvider;
        }

        //[Fact]
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
        [Fact(Skip ="Need mock")]
        public void TestAWSCreateRootFolderUsingKey()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = assetStorageProvider.GetAttributeValue( "RootFolder" );
            asset.Type = AssetType.Folder;

            Assert.True( s3Component.CreateFolder( assetStorageProvider, asset ) );
        }

        /// <summary>
        /// Create folders using RootFolder and Asset.Name
        /// These folders are used for other tests.
        /// Requires TestAWSCreateFolderByKey
        /// </summary>
        [Fact( Skip = "Need mock" )]
        public void TestAWSCreateFolderByName()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Name = "SubFolder1/";
            asset.Type = AssetType.Folder;
            Assert.True( s3Component.CreateFolder( assetStorageProvider, asset ) );

            asset = new Asset();
            asset.Name = "SubFolder1/SubFolder1a/";
            asset.Type = AssetType.Folder;
            Assert.True( s3Component.CreateFolder( assetStorageProvider, asset ) );
        }

        /// <summary>
        /// Upload a file using RootFolder and Asset.Name.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [Fact( Skip = "Need mock" )]
        public void TestUploadObjectByName()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Name = ( "SubFolder1/TestUploadObjectByName.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            bool hasUploaded = s3Component.UploadObject( assetStorageProvider, asset );
            Assert.True( hasUploaded );
        }

        /// <summary>
        /// Upload a file using Asset.Key.
        /// Requires TestAWSCreateFolderByKey, TestAWSCreateFolderByName
        /// </summary>
        [Fact( Skip = "Need mock" )]
        public void TestUploadObjectByKey()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = ( "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg" );
            asset.AssetStream = new FileStream( @"C:\temp\test.jpg", FileMode.Open );

            bool hasUploaded = s3Component.UploadObject( assetStorageProvider, asset );
            Assert.True( hasUploaded );
        }

        /// <summary>
        /// Get a recursive list of objects using Asset.Key
        /// </summary>
        [Fact( Skip = "Need mock" )]
        public void TestListObjectsByKey()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = ( "UnitTestFolder/" );
            
            var assetList = s3Component.ListObjects( assetStorageProvider, asset );
            Assert.Contains( assetList, a => a.Name == "UnitTestFolder" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1a" );
        }

        /// <summary>
        /// Get a list of files and folders in a single folder using RootFolder
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestListObjectsInFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListObjectsInFolder( assetStorageProvider, asset );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1" );
            Assert.Contains( assetList, a => a.Name == "SubFolder1a" );
        }

        /// <summary>
        /// Upload > 2K objects. Used to test listing that requries more than one request.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestUpload2kObjects()
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
                using ( fs = new FileStream( @"C:\temp\TextDoc.txt", FileMode.Open ) )
                {
                    Asset asset = new Asset { Name = $"TwoThousandObjects/TestFile-{i}.txt", AssetStream = fs };
                    s3Component.UploadObject( assetStorageProvider, asset );
                    i++;
                }
            }
        }

        /// <summary>
        /// Get a list of keys that requires more than one request.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestList2KObjectsInFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/TwoThousandObjects/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListObjectsInFolder( assetStorageProvider, asset );
            Assert.Contains( assetList, a => a.Name == "TestFile-0.txt" );
            Assert.Contains( assetList, a => a.Name == "TestFile-1368.txt" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-0" );
            Assert.Contains( assetList, a => a.Name == "TestFolder-6" );
        }

        /// <summary>
        /// Create a download link for an asset on the fly.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestCreateDownloadLink()
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

            Assert.True( valid );
        }

        /// <summary>
        /// Get a file from storage.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestGetObject()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";

            bool valid = true;

            try
            {
                var responseAsset = s3Component.GetObject( assetStorageProvider, asset, false );
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
        [Fact(Skip ="Need mock")]
        public void TestListFolders()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/TwoThousandObjects/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListFoldersInFolder( GetAssetStorageProvider(), asset );

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
        [Fact(Skip ="Need mock")]
        public void TestListFiles()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/";
            asset.Type = AssetType.Folder;

            var assetList = s3Component.ListFilesInFolder( assetStorageProvider, asset );

            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByName.jpg" );
            Assert.DoesNotContain( assetList, a => a.Name == "SubFolder1a" );

        }

        /// <summary>
        /// Rename an existing file.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestRenameAsset()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey.jpg";

            Assert.True( s3Component.RenameAsset( assetStorageProvider, asset, "TestUploadObjectByKeyRenamed.jpg" ) );

            asset = new Asset();
            asset.Type = AssetType.File;
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByKey";

            var assetList = s3Component.ListObjects( assetStorageProvider, asset );
            Assert.Contains( assetList, a => a.Name == "TestUploadObjectByKeyRenamed.jpg" );
            Assert.DoesNotContain( assetList, a => a.Name == "TestUploadObjectByKey.jpg" );
        }

        /// <summary>
        /// Delete a single file.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestDeleteFile()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = "UnitTestFolder/SubFolder1/TestUploadObjectByName.jpg";
            asset.Type = AssetType.File;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageProvider, asset );
            Assert.True( hasDeleted );
        }

        /// <summary>
        /// Delete all of the test data.
        /// </summary>
        [Fact(Skip ="Need mock")]
        public void TestDeleteFolder()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var s3Component = assetStorageProvider.GetAssetStorageComponent();

            Asset asset = new Asset();
            asset.Key = ( "UnitTestFolder/" );
            asset.Type = AssetType.Folder;

            bool hasDeleted = s3Component.DeleteAsset( assetStorageProvider, asset );
            Assert.True( hasDeleted );
        }
    }
}
