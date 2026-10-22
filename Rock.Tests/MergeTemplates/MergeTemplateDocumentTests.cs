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
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.MergeTemplates;
using Rock.Model;

namespace Rock.Tests.MergeTemplates
{
    /// <summary>
    /// Checks that <see cref="MergeTemplateDocument"/> and the default
    /// <see cref="MergeTemplateType.CreateDocumentForDownload"/> produce a
    /// document that can be sent to the browser.
    /// </summary>
    [TestClass]
    public class MergeTemplateDocumentTests
    {
        #region MergeTemplateDocument

        [TestMethod]
        public void Constructor_WithStreamAtEnd_SeeksToStart()
        {
            var stream = new MemoryStream( Encoding.UTF8.GetBytes( "merged" ) );
            stream.Seek( 0, SeekOrigin.End );

            var document = new MergeTemplateDocument( stream, "MergeTemplateOutput.html", "text/html" );

            Assert.AreEqual( 0, document.ContentStream.Position );
            Assert.AreSame( stream, document.ContentStream );
        }

        [TestMethod]
        public void Constructor_WithBlankFileName_UsesDefaultFileName()
        {
            var document = new MergeTemplateDocument( new MemoryStream(), " ", "text/html" );

            Assert.AreEqual( "MergeTemplateOutput", document.FileName );
        }

        [TestMethod]
        public void Constructor_WithFileName_KeepsFileName()
        {
            var document = new MergeTemplateDocument( new MemoryStream(), "MergeTemplateOutput.docx", "text/html" );

            Assert.AreEqual( "MergeTemplateOutput.docx", document.FileName );
        }

        [DataTestMethod]
        [DataRow( null, "application/octet-stream" )]
        [DataRow( "", "application/octet-stream" )]
        [DataRow( "   ", "application/octet-stream" )]
        [DataRow( "text/html", "text/html" )]
        [DataRow( " text/html ", "text/html" )]
        [DataRow( "text/html; charset=utf-8", "text/html" )]
        [DataRow( "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" )]
        [DataRow( "; charset=utf-8", "application/octet-stream" )]
        [DataRow( "text", "application/octet-stream" )]
        [DataRow( "not a mime type", "application/octet-stream" )]
        public void Constructor_NormalizesMimeType( string mimeType, string expectedMimeType )
        {
            var document = new MergeTemplateDocument( new MemoryStream(), "MergeTemplateOutput.html", mimeType );

            Assert.AreEqual( expectedMimeType, document.MimeType );
        }

        [DataTestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "text/html; charset=utf-8" )]
        [DataRow( "text" )]
        public void Constructor_MimeType_IsAcceptedByContentTypeHeader( string mimeType )
        {
            var document = new MergeTemplateDocument( new MemoryStream(), "MergeTemplateOutput.html", mimeType );

            // FileBlockActionResult builds the Content-Type header this way, which throws for an invalid value.
            var header = new MediaTypeHeaderValue( document.MimeType );

            Assert.AreEqual( document.MimeType, header.MediaType );
        }

        #endregion MergeTemplateDocument

        #region CreateDocumentForDownload

        [TestMethod]
        public void CreateDocumentForDownload_WithUnsavedFile_ReturnsSameContent()
        {
            var content = Encoding.UTF8.GetBytes( "<p>TH172 Tester A</p>" );
            var mergeTemplateType = CreateTestMergeTemplateType();
            mergeTemplateType.OutputFile = new BinaryFile
            {
                ContentStream = new MemoryStream( content ),
                FileName = "MergeTemplateOutput.html",
                MimeType = "text/html; charset=utf-8"
            };

            var document = mergeTemplateType.CreateDocumentForDownload( new MergeTemplate(), new List<object>(), new Dictionary<string, object>() );

            Assert.IsNotNull( document );
            CollectionAssert.AreEqual( content, document.ContentStream.ReadBytesToEnd() );
            Assert.AreEqual( "MergeTemplateOutput.html", document.FileName );
            Assert.AreEqual( "text/html", document.MimeType );
        }

        [TestMethod]
        public void CreateDocumentForDownload_WhenCreateDocumentReturnsNull_ReturnsNull()
        {
            var mergeTemplateType = CreateTestMergeTemplateType();
            mergeTemplateType.OutputFile = null;

            var document = mergeTemplateType.CreateDocumentForDownload( new MergeTemplate(), new List<object>(), new Dictionary<string, object>() );

            Assert.IsNull( document );
        }

        [TestMethod]
        public void CreateDocumentForDownload_WithUnsavedFileAndNoStream_ReturnsNull()
        {
            var mergeTemplateType = CreateTestMergeTemplateType();
            mergeTemplateType.OutputFile = new BinaryFile
            {
                FileName = "MergeTemplateOutput.html",
                MimeType = "text/html"
            };

            var document = mergeTemplateType.CreateDocumentForDownload( new MergeTemplate(), new List<object>(), new Dictionary<string, object>() );

            Assert.IsNull( document );
        }

        [TestMethod]
        public void CreateDocumentForDownload_WithUnsavedFileAndDisposedStream_ReturnsNull()
        {
            var stream = new MemoryStream( Encoding.UTF8.GetBytes( "<p>TH172 Tester A</p>" ) );
            var mergeTemplateType = CreateTestMergeTemplateType();
            mergeTemplateType.OutputFile = new BinaryFile
            {
                ContentStream = stream,
                FileName = "MergeTemplateOutput.html",
                MimeType = "text/html"
            };
            stream.Dispose();

            var document = mergeTemplateType.CreateDocumentForDownload( new MergeTemplate(), new List<object>(), new Dictionary<string, object>() );

            Assert.IsNull( document );
        }

        #endregion CreateDocumentForDownload

        #region Support Classes

        /// <summary>
        /// Creates the test merge template type without running the Component
        /// constructor, which loads attributes from the database.
        /// </summary>
        /// <returns>A new test merge template type.</returns>
        private static TestMergeTemplateType CreateTestMergeTemplateType()
        {
            return ( TestMergeTemplateType ) FormatterServices.GetUninitializedObject( typeof( TestMergeTemplateType ) );
        }

        /// <summary>
        /// A merge template type that only implements CreateDocument, like a
        /// third party merge template type, and returns a file that was not saved.
        /// </summary>
        private class TestMergeTemplateType : MergeTemplateType
        {
            public BinaryFile OutputFile { get; set; }

            public override List<Exception> Exceptions { get; set; }

            public override BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
            {
                return OutputFile;
            }
        }

        #endregion Support Classes
    }
}
