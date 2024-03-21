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

using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.StorageTests
{
    public abstract class BaseStorageComponentTests : DatabaseTestsBase
    {
        protected static System.Guid _assetStorageProviderServiceGuid;

        private static string webContentFolder = string.Empty;
        private static byte[] _testTextFileBytes = File.ReadAllBytes( @"TestData\TextDoc.txt" );
        private static byte[] _testJpgFileBytes = File.ReadAllBytes( @"TestData\test.jpg" );

        protected static void ClassInitialize( TestContext context )
        {
            webContentFolder = Path.Combine( context.DeploymentDirectory, "TestData", "Content" );
            EnsureFolder( webContentFolder );

            SeedIntialData();
        }

        protected static void InternalClassCleanup()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            var assetStorageProvider = assetStorageProviderService.Get( _assetStorageProviderServiceGuid );
            var storageComponent = assetStorageProvider.GetAssetStorageComponent();

            var unitTestFolder = new Asset
            {
                Key = assetStorageProvider.GetAttributeValue( "RootFolder" ),
                Type = AssetType.Folder
            };

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var items = storageComponent.ListObjects( assetStorageProvider, unitTestFolder );
                foreach ( var item in items )
                {
                    if ( item.Key == unitTestFolder.Key )
                    {
                        continue;
                    }
                    storageComponent.DeleteAsset( assetStorageProvider, item );
                }
            }

            if ( !string.IsNullOrEmpty( webContentFolder ) )
            {
                try
                {
                    Directory.Delete( webContentFolder, recursive: true );
                }
                catch
                {
                    // do nothing.
                }
            }
        }

        protected AssetStorageProvider GetAssetStorageProvider()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            AssetStorageProvider assetStorageProvider = assetStorageProviderService.Get( _assetStorageProviderServiceGuid );
            assetStorageProvider.LoadAttributes();
            return assetStorageProvider;
        }

        private static void EnsureFolder( string foldername )
        {
            var path = Path.Combine( webContentFolder, foldername );
            if ( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
        }

        private static void SeedIntialData()
        {
            var assetStorageProviderService = new AssetStorageProviderService( new RockContext() );
            var assetStorageProvider = assetStorageProviderService.Get( _assetStorageProviderServiceGuid );
            assetStorageProvider.LoadAttributes();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var rootAsset = new Asset
                {
                    Key = assetStorageProvider.GetAttributeValue( "RootFolder" ),
                    Type = AssetType.Folder
                };

                bool isSuccess = assetStorageComponent.CreateFolder( assetStorageProvider, rootAsset );
                if ( !isSuccess )
                {
                    Assert.That.Inconclusive( $"Unable to create root folder while seeding data" );
                }

                var parentFolder = new Asset();
                parentFolder.Name = "ParentFolder";
                parentFolder.Type = AssetType.Folder;

                isSuccess = assetStorageComponent.CreateFolder( assetStorageProvider, parentFolder );

                for ( var i = 1; i < 11; i++ )
                {
                    var testFolder = new Asset
                    {
                        Name = $"{parentFolder.Name}/TestFolder-{i}",
                        Type = AssetType.Folder
                    };

                    isSuccess = assetStorageComponent.CreateFolder( assetStorageProvider, testFolder );

                    assetStorageComponent.UploadObject( assetStorageProvider, new Asset
                    {
                        Name = $"{parentFolder.Name}/TestFile-{i}.txt",
                        AssetStream = new MemoryStream( _testTextFileBytes )
                    } );

                    assetStorageComponent.UploadObject( assetStorageProvider, new Asset
                    {
                        Name = $"{testFolder.Name}/TestFile-{i}.txt",
                        AssetStream = new MemoryStream( _testTextFileBytes )
                    } );
                }
            }
        }

        [TestMethod]
        public void GetObjectUsingAssetNameShouldReturnRequestedObject()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var expectedFileName = "TestFile-2.txt";
            var expectedFilePath = $"ParentFolder/TestFolder-2/{expectedFileName}";



            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var expectedAsset = new Asset
                {
                    Name = expectedFilePath,
                    Type = AssetType.File
                };

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );
                Assert.That.Equal( expectedFileName, actualAsset.Name );

                var actualBytes = new byte[0];
                try
                {
                    using ( var actualStream = actualAsset.AssetStream )
                    {
                        var actualFile = new MemoryStream();
                        actualStream.CopyTo( actualFile );
                        actualBytes = actualFile.ReadBytesToEnd();
                    }
                }
                catch ( Exception ex )
                {
                    Assert.That.Fail( $"An exception occurred while trying to downloaded file. {ex.Message}" );
                }

                Assert.That.AreEqual( _testTextFileBytes.AsEnumerable<byte>(), actualBytes.AsEnumerable<byte>() );
            }
        }

        [TestMethod]
        public void GetObjectUsingAssetKeyShouldReturnRequestedObject()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var expectedFileName = "TestFile-1.txt";
            var expectedFilePath = $"{assetStorageProvider.GetAttributeValue( "RootFolder" )}/ParentFolder/TestFolder-1/{expectedFileName}";

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var expectedAsset = new Asset
                {
                    Key = expectedFilePath,
                    Type = AssetType.File
                };

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );
                Assert.That.AreEqual( expectedFileName, actualAsset.Name );

                var actualBytes = new byte[0];
                try
                {
                    using ( var actualStream = actualAsset.AssetStream )
                    {
                        var actualFile = new MemoryStream();
                        actualStream.CopyTo( actualFile );
                        actualBytes = actualFile.ReadBytesToEnd();
                    }
                }
                catch ( Exception ex )
                {
                    Assert.That.Fail( $"An exception occurred while trying to downloaded file. {ex.Message}" );
                }

                Assert.That.AreEqual( _testTextFileBytes.AsEnumerable<byte>(), actualBytes.AsEnumerable<byte>() );
            }
        }

        [TestMethod]
        public void CreateFolderUsingAssetNameCreatesCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var rootAsset = new Asset
            {
                Key = assetStorageProvider.GetAttributeValue( "RootFolder" ),
                Type = AssetType.Folder
            };

            var expectedAsset = new Asset
            {
                Name = System.Guid.NewGuid().ToString(),
                Type = AssetType.Folder
            };

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var isFolderCreated = assetStorageComponent.CreateFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isFolderCreated );

                var assets = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.IsNotEmpty( assets );
                Assert.That.IsTrue( assets.Any( a => a.Type == AssetType.Folder && a.Name == expectedAsset.Name ) );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isSuccess, "New Created Folder can't be deleted in TestGoogleCreateFolderByName" );
            }
        }

        [TestMethod]
        public void CreateFolderUsingAssetKeyCreatesCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var expectedAsset = new Asset
            {
                Key = $"FolderCreateTest/{System.Guid.NewGuid().ToString()}",
                Type = AssetType.Folder
            };

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var isFolderCreated = assetStorageComponent.CreateFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isFolderCreated );

                var assets = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, new Asset { Key = "FolderCreateTest/", Type = AssetType.Folder } );
                Assert.That.IsNotEmpty( assets );
                Assert.That.IsTrue( assets.Any( a => a.Type == AssetType.Folder && a.Key == expectedAsset.Key ) );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isSuccess, "New Created Folder can't be deleted in TestGoogleCreateFolderByName" );
            }
        }

        [TestMethod]
        public void UploadObjectByNameShouldUploadCorrectly()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

                var expectedFileName = $"{Guid.NewGuid().ToString()}.jpg";
                var expectedAsset = new Asset
                {
                    Name = $"ParentFolder/TestFolder-1/{expectedFileName}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasUploaded );

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );
                Assert.That.Equal( expectedFileName, actualAsset.Name );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, actualAsset );
                Assert.That.IsTrue( isSuccess, "New uploaded file can't be deleted in TestUploadObjectByName" );
            }
        }

        [TestMethod]
        public void UploadObjectByKeyShouldUploadCorrectly()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

                var expectedFileName = $"{Guid.NewGuid().ToString()}.jpg";
                var expectedAsset = new Asset
                {
                    Key = $"ParentFolder/TestFolder-1/{expectedFileName}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasUploaded );

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );
                Assert.That.Equal( expectedFileName, actualAsset.Name );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, actualAsset );
                Assert.That.IsTrue( isSuccess, "New uploaded file can't be deleted in TestUploadObjectByName" );
            }
        }

        [TestMethod]
        public void ListObjectsShouldReturnAllObjectsFroRootFolderIfNoNameOrKeyIsPassed()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset();

                var assetList = assetStorageComponent.ListObjects( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == "ParentFolder" ) );
            }
        }

        [TestMethod]
        public void ListObjectsUsingAssetNameShouldReturnCorrectData()
        {
            var expectedCount = 10;
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

                var asset = new Asset { Name = "ParentFolder", Type = AssetType.Folder };

                var assetList = assetStorageComponent.ListObjects( assetStorageProvider, asset );
                Assert.AreEqual( expectedCount, assetList.Count( f => f.Name.StartsWith( "TestFolder-" ) ) );
                Assert.AreEqual( expectedCount, assetList.Count( f => f.Name.StartsWith( "TestFile-" ) ) );
            }
        }

        [TestMethod]
        public void ListObjectsUsingAssetKeyShouldReturnCorrectData()
        {
            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetStorageProvider = GetAssetStorageProvider();
                var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
                var expectedFolderName = assetStorageProvider.GetAttributeValue( "RootFolder" );
                var asset = new Asset { Key = "/" };

                var assetList = assetStorageComponent.ListObjects( assetStorageProvider, asset );
                Assert.That.IsTrue( assetList.Any( a => a.Name == expectedFolderName ) );
            }
        }

        [TestMethod]
        public void ListObjectsInFolderUsingKeyShouldReturnCorrectData()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset
            {
                Key = $"{assetStorageProvider.GetAttributeValue( "RootFolder" )}/ParentFolder/",
                Type = AssetType.Folder
            };

            var assetList = assetStorageComponent.ListObjectsInFolder( assetStorageProvider, asset );
            Assert.That.IsTrue( assetList.Where( a => a.Type == AssetType.File ).Count() >= 10 );
            for ( int i = 1; i <= 10; i++ )
            {
                Assert.That.IsTrue( assetList.Any( a => a.Name == $"TestFile-{i}.txt" ) );
            }

            Assert.That.IsTrue( assetList.Where( a => a.Type == AssetType.Folder ).Count() >= 10 );
            for ( int i = 1; i <= 10; i++ )
            {
                Assert.That.IsTrue( assetList.Any( a => a.Name == $"TestFolder-{i}" ) );
            }
        }

        [TestMethod]
        public void ListObjectsInFolderUsingNameShouldReturnCorrectData()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var asset = new Asset
            {
                Name = "ParentFolder",
                Type = AssetType.Folder
            };

            var assetList = assetStorageComponent.ListObjectsInFolder( assetStorageProvider, asset );
            Assert.That.IsTrue( assetList.Where( a => a.Type == AssetType.File ).Count() >= 10 );
            for ( int i = 1; i <= 10; i++ )
            {
                Assert.That.IsTrue( assetList.Any( a => a.Name == $"TestFile-{i}.txt" ) );
            }

            Assert.That.IsTrue( assetList.Where( a => a.Type == AssetType.Folder ).Count() >= 10 );
            for ( int i = 1; i <= 10; i++ )
            {
                Assert.That.IsTrue( assetList.Any( a => a.Name == $"TestFolder-{i}" ) );
            }
        }

        [TestMethod]
        public void ListFoldersInFolderUsingKeyShouldReturnCorrectData()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var expectedFolderCount = 10;

            var asset = new Asset
            {
                Key = $"{assetStorageProvider.GetAttributeValue( "RootFolder" )}/ParentFolder/",
                Type = AssetType.Folder
            };

            var assetList = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, asset );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count() );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count( f => f.Name.StartsWith( "TestFolder-" ) ) );
        }

        [TestMethod]
        public void ListFoldersInFolderUsingNameShouldReturnCorrectData()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var expectedFolderCount = 10;

            var asset = new Asset
            {
                Name = "ParentFolder",
                Type = AssetType.Folder
            };

            var assetList = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, asset );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count() );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count( f => f.Name.StartsWith( "TestFolder-" ) ) );
        }

        [TestMethod]
        public void ListFoldersInFolderWithoutNameOrKeyShouldReturnCorrectData()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var expectedFolderCount = 1;

            var assetList = assetStorageComponent.ListFoldersInFolder( assetStorageProvider );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count() );
            Assert.That.AreEqual( expectedFolderCount, assetList.Count( f => f.Name.StartsWith( "ParentFolder" ) ) );
        }

        [TestMethod]
        public void RenameAssetUsingNameShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var originalFilename = $"{Guid.NewGuid().ToString()}.jpg";
            var newFilename = $"{Guid.NewGuid().ToString()}.jpg";
            var parentFolder = "ParentFolder";

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetToRename = new Asset
                {
                    Name = $"{parentFolder}/{originalFilename}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, assetToRename );
                Assert.That.IsTrue( hasUploaded );

                var isRenameSuccess = assetStorageComponent.RenameAsset( assetStorageProvider, assetToRename, newFilename );
                Assert.That.IsTrue( isRenameSuccess );


                var fileList = assetStorageComponent.ListFilesInFolder( assetStorageProvider, new Asset { Name = parentFolder, Type = AssetType.Folder } );
                Assert.That.IsNotNull( fileList );

                var expectedAsset = fileList.FirstOrDefault( a => a.Name == newFilename );
                Assert.That.IsNotNull( expectedAsset );

                Assert.That.IsTrue( fileList.Any( a => a.Name == newFilename ) );
                Assert.That.IsFalse( fileList.Any( a => a.Name == originalFilename ) );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isSuccess, "Rename file can't be deleted" );
            }
        }

        [TestMethod]
        public void RenameAssetUsingKeyShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var originalFilename = $"{Guid.NewGuid().ToString()}.jpg";
            var newFilename = $"{Guid.NewGuid().ToString()}.jpg";
            var parentFolder = $"{assetStorageProvider.GetAttributeValue( "RootFolder" )}/ParentFolder";

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var assetToRename = new Asset
                {
                    Key = $"{parentFolder}/{originalFilename}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, assetToRename );
                Assert.That.IsTrue( hasUploaded );

                var isRenameSuccess = assetStorageComponent.RenameAsset( assetStorageProvider, assetToRename, newFilename );
                Assert.That.IsTrue( isRenameSuccess );


                var fileList = assetStorageComponent.ListFilesInFolder( assetStorageProvider, new Asset { Key = parentFolder, Type = AssetType.Folder } );
                Assert.That.IsNotNull( fileList );

                var expectedAsset = fileList.FirstOrDefault( a => a.Name == newFilename );
                Assert.That.IsNotNull( expectedAsset );

                Assert.That.IsTrue( fileList.Any( a => a.Name == newFilename ) );
                Assert.That.IsFalse( fileList.Any( a => a.Name == originalFilename ) );

                bool isSuccess = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( isSuccess, "Rename file can't be deleted" );
            }
        }

        [TestMethod]
        public void DeleteAssetUsingKeyShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var rootPath = assetStorageProvider.GetAttributeValue( "RootFolder" );
            var folderPath = $"{rootPath}/{Guid.NewGuid().ToString()}/";

            var rootAsset = new Asset
            {
                Key = rootPath,
                Type = AssetType.Folder
            };

            var expectedAsset = new Asset
            {
                Key = folderPath,
                Type = AssetType.Folder
            };

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Assert.That.IsTrue( assetStorageComponent.CreateFolder( assetStorageProvider, expectedAsset ) );

                var expectedFileAsset = new Asset
                {
                    Key = $"{folderPath}/{Guid.NewGuid().ToString()}.jpg",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                Assert.That.IsTrue( assetStorageComponent.UploadObject( assetStorageProvider, expectedFileAsset ) );

                var folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.True( folders.Any( a => a.Key == expectedAsset.Key ) );

                bool hasDeleted = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasDeleted );

                folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.IsFalse( folders.Any( a => a.Key == expectedAsset.Key ) );

                var files = assetStorageComponent.ListFilesInFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsFalse( folders.Any( a => a.Key == expectedFileAsset.Key ) );
            }
        }

        [TestMethod]
        public void DeleteAssetUsingNameShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var rootPath = assetStorageProvider.GetAttributeValue( "RootFolder" );
            var folderPath = $"{Guid.NewGuid().ToString()}";

            var rootAsset = new Asset
            {
                Key = rootPath,
                Type = AssetType.Folder
            };

            var expectedAsset = new Asset
            {
                Name = folderPath,
                Type = AssetType.Folder
            };

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Assert.That.IsTrue( assetStorageComponent.CreateFolder( assetStorageProvider, expectedAsset ) );

                var expectedFileAsset = new Asset
                {
                    Name = $"{folderPath}/{Guid.NewGuid().ToString()}.jpg",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                Assert.That.IsTrue( assetStorageComponent.UploadObject( assetStorageProvider, expectedFileAsset ) );

                var folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.True( folders.Any( a => a.Name == expectedAsset.Name ) );

                bool hasDeleted = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasDeleted );

                folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.IsFalse( folders.Any( a => a.Name == expectedAsset.Name ) );

                var files = assetStorageComponent.ListFilesInFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsFalse( folders.Any( a => a.Name == expectedFileAsset.Name ) );
            }
        }

        [TestMethod]
        public void DeleteAssetWithSubfoldersShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var rootPath = assetStorageProvider.GetAttributeValue( "RootFolder" );
            var folderPath = $"{Guid.NewGuid().ToString()}";

            var rootAsset = new Asset
            {
                Key = rootPath,
                Type = AssetType.Folder
            };

            var expectedAsset = new Asset
            {
                Name = folderPath,
                Type = AssetType.Folder
            };

            var subfolderAsset = new Asset
            {
                Name = $"{folderPath}/{Guid.NewGuid().ToString()}",
                Type = AssetType.Folder
            };


            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Assert.That.IsTrue( assetStorageComponent.CreateFolder( assetStorageProvider, expectedAsset ) );
                Assert.That.IsTrue( assetStorageComponent.CreateFolder( assetStorageProvider, subfolderAsset ) );

                var expectedFileAsset = new Asset
                {
                    Name = $"{subfolderAsset.Name}/{Guid.NewGuid().ToString()}.jpg",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                Assert.That.IsTrue( assetStorageComponent.UploadObject( assetStorageProvider, expectedFileAsset ) );

                var folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.True( folders.Any( a => a.Name == expectedAsset.Name ) );

                folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, expectedAsset );
                Assert.That.True( folders.Any( a => a.Key.Contains( subfolderAsset.Name ) ) );

                bool hasDeleted = assetStorageComponent.DeleteAsset( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasDeleted );

                folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, rootAsset );
                Assert.That.IsFalse( folders.Any( a => a.Key.Contains( expectedAsset.Name ) ) );

                folders = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsFalse( folders.Any( a => a.Key.Contains( subfolderAsset.Name ) ) );

                var files = assetStorageComponent.ListFilesInFolder( assetStorageProvider, expectedAsset );
                Assert.That.IsFalse( folders.Any( a => a.Name == expectedFileAsset.Name ) );
            }
        }


        [TestMethod]
        public void ListFilesInFolderShouldWorkWith2kFiles()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();
            var expectedFolderCount = 10;
            var expectedFileCount = 2000;

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                var subFolder = new Asset
                {
                    Name = "TwoThousandObjects",
                    Type = AssetType.Folder
                };

                assetStorageComponent.CreateFolder( assetStorageProvider, subFolder );

                // Create 2000 files
                int i = 0;
                while ( i < expectedFileCount )
                {
                    Asset asset = new Asset
                    {
                        Name = $"TwoThousandObjects/TestFile-{i}.txt",
                        AssetStream = new MemoryStream( _testTextFileBytes )
                    };

                    assetStorageComponent.UploadObject( assetStorageProvider, asset );
                    i++;
                }

                // Create 10 child folders (AFTER the 2000 files so we can try to cause some components
                // to fail the ListFoldersInFolder test due to the way they could be written).
                i = 0;
                Asset childFolder;
                while ( i < expectedFolderCount )
                {
                    childFolder = new Asset
                    {
                        Name = $"TwoThousandObjects/TestFolder-{i}/",
                        Type = AssetType.Folder
                    };

                    assetStorageComponent.CreateFolder( assetStorageProvider, childFolder );
                    i++;
                }

                // Check for all 2000 files:
                var assets = assetStorageComponent.ListFilesInFolder( assetStorageProvider, subFolder );
                var actualFileCount = assets.Where( a => a.Name.Contains( "TestFile-" ) ).Count();

                // Check for all 10 sub folders:
                var foldersList = assetStorageComponent.ListFoldersInFolder( assetStorageProvider, subFolder );
                var actualFolderCount = foldersList.Where( a => a.Name.Contains( "TestFolder-" ) ).Count();

                // Delete the whole TwoThousandObjects folder.
                while ( assetStorageComponent.ListFilesInFolder( assetStorageProvider, subFolder ).Count > 0 )
                {
                    assetStorageComponent.DeleteAsset( assetStorageProvider, subFolder );
                }

                Assert.That.AreEqual( expectedFileCount, actualFileCount, "Did not find all 2000 files." );
                Assert.That.AreEqual( expectedFolderCount, expectedFolderCount, "Did not find all 10 folders." );
            }
        }

        [TestMethod]
        public void CreateDownloadLinkUsingKeyShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var folderPath = $"{assetStorageProvider.GetAttributeValue( "RootFolder" )}/";
            var filename = $"{Guid.NewGuid().ToString()}.jpg";

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Asset expectedAsset = new Asset
                {
                    Key = $"{folderPath}{filename}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasUploaded );

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );

                string url = assetStorageComponent.CreateDownloadLink( assetStorageProvider, expectedAsset );
                bool valid = false;
                var actualBytes = new byte[0];
                try
                {
                    System.Net.HttpWebRequest request = System.Net.WebRequest.Create( url ) as System.Net.HttpWebRequest;
                    request.Method = "GET";
                    System.Net.HttpWebResponse response = request.GetResponse() as System.Net.HttpWebResponse;

                    actualBytes = response.GetResponseStream().ReadBytesToEnd();

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
                            Assert.That.Inconclusive( $"File ({expectedAsset.Key}) was forbidden from viewing." );
                        }
                        if ( httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized )
                        {
                            Assert.That.Inconclusive( $"Anonymous download is not allowed for ({expectedAsset.Key})." );
                        }
                    }
                }
                finally
                {
                    assetStorageComponent.DeleteAsset( assetStorageProvider, actualAsset );
                }

                Assert.That.IsTrue( valid );
                Assert.That.AreEqual( _testJpgFileBytes.AsEnumerable(), actualBytes.AsEnumerable() );
            }
        }

        [TestMethod]
        public void CreateDownloadLinkUsingNameShouldWorkCorrectly()
        {
            var assetStorageProvider = GetAssetStorageProvider();
            var assetStorageComponent = assetStorageProvider.GetAssetStorageComponent();

            var folderPath = $"/";
            var filename = $"{Guid.NewGuid().ToString()}.jpg";

            using ( new HttpSimulator( "/", webContentFolder ).SimulateRequest() )
            {
                Asset expectedAsset = new Asset
                {
                    Name = $"{folderPath}{filename}",
                    AssetStream = new MemoryStream( _testJpgFileBytes ),
                    Type = AssetType.File
                };

                bool hasUploaded = assetStorageComponent.UploadObject( assetStorageProvider, expectedAsset );
                Assert.That.IsTrue( hasUploaded );

                var actualAsset = assetStorageComponent.GetObject( assetStorageProvider, expectedAsset );
                Assert.That.IsNotNull( actualAsset );

                string url = assetStorageComponent.CreateDownloadLink( assetStorageProvider, expectedAsset );
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
                            Assert.That.Inconclusive( $"File ({expectedAsset.Key}) was forbidden from viewing." );
                        }
                        if ( httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized )
                        {
                            Assert.That.Inconclusive( $"Anonymous download is not allowed for ({expectedAsset.Key})." );
                        }
                    }
                }
                finally
                {
                    assetStorageComponent.DeleteAsset( assetStorageProvider, actualAsset );
                }
            }
        }
    }
}
