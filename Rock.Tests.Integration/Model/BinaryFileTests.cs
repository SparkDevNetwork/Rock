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
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using Rock.Tests.Shared;
using System.IO;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class BinaryFileTests
    {
        #region Setup

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                string sql = @"
                    DECLARE @binaryFileId INT = (SELECT [Id] FROM [BinaryFile] WHERE [Guid] = 'a1dbaf65-b61a-4b63-9dce-46de5912e455')
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
            var stream = new FileStream( @"TestData\test.jpg", FileMode.Open );
            string mimeType = "image/jpeg";
            long contentLength = stream.Length;// 59007
            Guid? imageGuid = new Guid( "a1dbaf65-b61a-4b63-9dce-46de5912e455" );
            string fileName = $"test-{imageGuid}.jpg";
            string binaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.DEFAULT;

            stream.Position = 0;

            using ( var rockContext = new RockContext() )
            {
                var binaryFile = new BinaryFileService( rockContext).AddFileFromStream( stream, mimeType, contentLength, fileName, binaryFileTypeGuid, imageGuid );

                Assert.IsNotNull( binaryFile );
            }
        }

        #endregion CreateFile

    }
}
