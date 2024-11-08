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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class BinaryFileTests : DatabaseTestsBase
    {
        #region Setup
        private Dictionary<string, Guid> _fileGuids;

        /// <summary>
        /// Tests the initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _fileGuids = new Dictionary<string, Guid>
            {
                { nameof( AddANewFileFromAStream ), Guid.NewGuid() },
                { nameof( AddANewFileFromAStreamWithMaxHeightGreaterThanWidthSet ), Guid.NewGuid() },
                { nameof( AddANewFileFromAStreamWithMaxHeightSet ), Guid.NewGuid() },
                { nameof( AddANewFileFromAStreamWithMaxWidthGreaterThanHeightSet ), Guid.NewGuid() },
                { nameof( AddANewFileFromAStreamWithMaxWidthSet ), Guid.NewGuid() }
            };
        }

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                var guids = _fileGuids.Values.Select( v => $"'{v}'" ).JoinStrings( "," );

                string sql = $@"
                    DECLARE @binaryFileId INT = (SELECT [Id] FROM [BinaryFile] WHERE [Guid] IN ({guids}))
                    DELETE FROM [BinaryFileData] WHERE [ID] = @binaryFileId
                    DELETE FROM [BinaryFile] WHERE [ID] = @binaryFileId";

                rockContext.Database.ExecuteSqlCommand( sql );
            }
        }

        #endregion Setup

        #region CreateFile

        [TestMethod]
        public void AddANewFileFromAStream()
        {
            using ( var stream = new FileStream( @"TestData\test.jpg", FileMode.Open ) )
            {
                string mimeType = "image/jpeg";
                long contentLength = stream.Length;// 59007
                Guid? imageGuid = _fileGuids[nameof( AddANewFileFromAStream )];
                string fileName = $"test-{imageGuid}.jpg";
                string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

                stream.Position = 0;

                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );

                    var binaryFile = binaryFileService.AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                    Assert.IsNotNull( binaryFile );
                    Assert.IsTrue( binaryFile?.Id > 0 );

                }
            }
        }

        /// <summary>
        /// Defines the test method AddANewFileFromAStreamWithMaxWidthSet. Based on settings specified in <seealso cref="Rock.Model.BinaryFile.SaveHook"/>
        /// </summary>
        [TestMethod]
        public void AddANewFileFromAStreamWithMaxWidthSet()
        {
            var stream = new FileStream( @"TestData\BinaryFileWidthTests\test_mW.jpg", FileMode.Open );
            string mimeType = "image/jpeg";
            long contentLength = stream.Length;// 59007
            Guid? imageGuid = _fileGuids[nameof( AddANewFileFromAStreamWithMaxWidthSet )];
            string fileName = $"test_mW-{imageGuid}.jpg";
            string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

            stream.Position = 0;

            using ( var rockContext = new RockContext() )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var fileType = binaryFileTypeService.Get( binaryFileTypeGuid.AsGuid() );

                const int maxWidth = 100;

                // Set Test dimensions
                fileType.MaxWidth = maxWidth;
                rockContext.SaveChanges();

                var binaryFileService = new BinaryFileService( rockContext );

                var binaryFile = binaryFileService.AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                Assert.IsNotNull( binaryFile );
                Assert.IsTrue( binaryFile?.Id > 0 );
                Assert.IsTrue( binaryFile.Width == maxWidth );
                Assert.IsTrue( binaryFile.Height == maxWidth );
            }
        }

        /// <summary>
        /// Defines the test method AddANewFileFromAStreamWithMaxHeightSet. Based on settings specified in <seealso cref="Rock.Model.BinaryFile.SaveHook"/>
        /// </summary>
        [TestMethod]
        public void AddANewFileFromAStreamWithMaxHeightSet()
        {
            var stream = new FileStream( @"TestData\BinaryFileWidthTests\test_mH.jpg", FileMode.Open );
            string mimeType = "image/jpeg";
            long contentLength = stream.Length;// 59007
            Guid? imageGuid = _fileGuids[nameof( AddANewFileFromAStreamWithMaxHeightSet )];
            string fileName = $"test_mH-{imageGuid}.jpg";
            string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

            stream.Position = 0;

            using ( var rockContext = new RockContext() )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var fileType = binaryFileTypeService.Get( binaryFileTypeGuid.AsGuid() );

                const int maxHeight = 100;

                // Set Test dimensions
                fileType.MaxHeight = maxHeight;
                rockContext.SaveChanges();

                var binaryFileService = new BinaryFileService( rockContext );

                var binaryFile = binaryFileService.AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                Assert.IsNotNull( binaryFile );
                Assert.IsTrue( binaryFile?.Id > 0 );
                Assert.IsTrue( binaryFile.Height == maxHeight );
                Assert.IsTrue( binaryFile.Width == maxHeight );
            }
        }

        /// <summary>
        /// Defines the test method AddANewFileFromAStreamWithMaxHeightGreaterThanWidthSet. Based on settings specified in <seealso cref="Rock.Model.BinaryFile.SaveHook"/>
        /// </summary>
        [TestMethod]
        public void AddANewFileFromAStreamWithMaxHeightGreaterThanWidthSet()
        {
            var stream = new FileStream( @"TestData\BinaryFileWidthTests\test_mHgtMw.jpg", FileMode.Open );
            string mimeType = "image/jpeg";
            long contentLength = stream.Length;// 59007
            Guid? imageGuid = _fileGuids[nameof( AddANewFileFromAStreamWithMaxHeightGreaterThanWidthSet )];
            string fileName = $"test_mHgtMw-{imageGuid}.jpg";
            string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

            stream.Position = 0;

            using ( var rockContext = new RockContext() )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var fileType = binaryFileTypeService.Get( binaryFileTypeGuid.AsGuid() );

                const int maxHeight = 200;
                const int maxWidth = 100;

                // Set Test dimensions
                fileType.MaxHeight = maxHeight;
                fileType.MaxWidth = maxWidth;

                rockContext.SaveChanges();

                var binaryFileService = new BinaryFileService( rockContext );

                var binaryFile = binaryFileService.AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                Assert.IsNotNull( binaryFile );
                Assert.IsTrue( binaryFile?.Id > 0 );
                Assert.IsTrue( binaryFile.Height == maxHeight );
                Assert.IsTrue( binaryFile.Width == maxHeight );
            }
        }

        /// <summary>
        /// Defines the test method AddANewFileFromAStreamWithMaxWidthGreaterThanHeightSet. Based on settings specified in <seealso cref="Rock.Model.BinaryFile.SaveHook"/>
        /// </summary>
        [TestMethod]
        public void AddANewFileFromAStreamWithMaxWidthGreaterThanHeightSet()
        {
            var stream = new FileStream( @"TestData\BinaryFileWidthTests\test_mWgtMh.jpg", FileMode.Open );
            string mimeType = "image/jpeg";
            long contentLength = stream.Length;// 59007
            Guid? imageGuid = _fileGuids[nameof( AddANewFileFromAStreamWithMaxWidthGreaterThanHeightSet )];
            string fileName = $"test_mWgtMh-{imageGuid}.jpg";
            string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

            stream.Position = 0;

            using ( var rockContext = new RockContext() )
            {
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var fileType = binaryFileTypeService.Get( binaryFileTypeGuid.AsGuid() );

                const int maxHeight = 100;
                const int maxWidth = 200;

                // Set Test dimensions
                fileType.MaxHeight = maxHeight;
                fileType.MaxWidth = maxWidth;

                rockContext.SaveChanges();

                var binaryFileService = new BinaryFileService( rockContext );

                var binaryFile = binaryFileService.AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                Assert.IsNotNull( binaryFile );
                Assert.IsTrue( binaryFile?.Id > 0 );
                Assert.IsTrue( binaryFile.Height == maxWidth );
                Assert.IsTrue( binaryFile.Width == maxWidth );
            }
        }

        #endregion CreateFile

    }
}