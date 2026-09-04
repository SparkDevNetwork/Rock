using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.MergeTemplates
{
    [TestClass]
    public class WordDocumentMergeTemplateTypeTests
    {
        /// <summary>
        /// Any lava in the header and footer should render in a Word document.
        /// At one point an update to the DocumentFormat.OpenXml library caused
        /// the way we were rendering headers and footers to stop working.
        /// </summary>
        [TestMethod]
        public void LavaInHeaderAndFooter_ShouldRenderLava()
        {
            var contentTypesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
  <Override PartName=""/word/header1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml""/>
  <Override PartName=""/word/footer1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml""/>
</Types>";

            var relsXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
</Relationships>";

            var documentXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:t>Hello World</w:t>
      </w:r>
    </w:p>
    <w:sectPr>
      <w:headerReference w:type=""default"" r:id=""rId2"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""/>
      <w:footerReference w:type=""default"" r:id=""rId3"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships""/>
    </w:sectPr>
  </w:body>
</w:document>";

            var documentXmlRels = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/header"" Target=""header1.xml""/>
  <Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer"" Target=""footer1.xml""/>
</Relationships>";

            var headerXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:hdr xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:p>
    <w:r>
      <w:t>1{{ HeaderValue }}1</w:t>
    </w:r>
  </w:p>
</w:hdr>";

            var footerXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:ftr xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:p>
    <w:r>
      <w:t>2{{ FooterValue }}2</w:t>
    </w:r>
  </w:p>
</w:ftr>";

            using ( var scope = TestHelper.CreateScopedRockApp() )
            {
                var memoryStream = new System.IO.MemoryStream();

                using ( var archive = new ZipArchive( memoryStream, ZipArchiveMode.Create, true ) )
                {
                    using ( var entryStream = archive.CreateEntry( "[Content_Types].xml" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( contentTypesXml ), 0, contentTypesXml.Length );
                    }
                    using ( var entryStream = archive.CreateEntry( "_rels/.rels" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( relsXml ), 0, relsXml.Length );
                    }
                    using ( var entryStream = archive.CreateEntry( "word/document.xml" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( documentXml ), 0, documentXml.Length );
                    }
                    using ( var entryStream = archive.CreateEntry( "word/_rels/document.xml.rels" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( documentXmlRels ), 0, documentXmlRels.Length );
                    }
                    using ( var entryStream = archive.CreateEntry( "word/header1.xml" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( headerXml ), 0, headerXml.Length );
                    }
                    using ( var entryStream = archive.CreateEntry( "word/footer1.xml" ).Open() )
                    {
                        entryStream.Write( Encoding.UTF8.GetBytes( footerXml ), 0, footerXml.Length );
                    }
                }

                var rockContext = scope.App.CreateRockContext();

                var binaryFile = new BinaryFile
                {
                    Id = 1,
                    ContentStream = memoryStream,
                    FileName = "template.docx",
                };

                var mergeTemplate = new MergeTemplate
                {
                    Id = 1,
                    TemplateBinaryFileId = binaryFile.Id,
                    TemplateBinaryFile = binaryFile,
                };

                rockContext.Set<BinaryFile>().Add( binaryFile );
                rockContext.Set<MergeTemplate>().Add( mergeTemplate );

                var template = new Rock.MergeTemplates.WordDocumentMergeTemplateType();
                var mergeFields = new Dictionary<string, object>
                {
                    { "HeaderValue", "Rock" },
                    { "FooterValue", "Roll" }
                };

                try
                {
                    LavaService.SetCurrentEngine( typeof( FluidEngine ) );

                    var outputStream = template.GetMergedDocumentOutputStream( mergeTemplate, new List<object>(), mergeFields );
                    outputStream.Position = 0;

                    using ( var archive = new ZipArchive( outputStream, ZipArchiveMode.Read ) )
                    {
                        var headerEntry = archive.GetEntry( "word/header1.xml" );
                        using ( var reader = new System.IO.StreamReader( headerEntry.Open() ) )
                        {
                            var headerContent = reader.ReadToEnd();
                            Assert.Contains( "1Rock1", headerContent, "Header Lava did not render correctly." );
                        }

                        var footerEntry = archive.GetEntry( "word/footer1.xml" );
                        using ( var reader = new System.IO.StreamReader( footerEntry.Open() ) )
                        {
                            var footerContent = reader.ReadToEnd();
                            Assert.Contains( "2Roll2", footerContent, "Footer Lava did not render correctly." );
                        }
                    }
                }
                finally
                {
                    LavaService.SetCurrentEngine( null, null );
                }
            }
        }
    }
}
