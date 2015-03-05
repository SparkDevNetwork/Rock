using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility
{
    public static class MergeDocHelper
    {
        /// <summary>
        /// Makes the document returning the binaryFileId of the output file
        /// </summary>
        /// <param name="templateBinaryFileId">The template binary file identifier.</param>
        /// <param name="mergeObjectsList">The merge objects list.</param>
        /// <returns></returns>
        public static int? MakeDocument( int templateBinaryFileId, List<Dictionary<string, object>> mergeObjectsList )
        {
            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            var templateBinaryFile = binaryFileService.Get( templateBinaryFileId );
            if ( templateBinaryFile == null )
            {
                return null;
            }

            var letterTemplateStream = templateBinaryFile.ContentStream;

            MemoryStream outputDocStream = new MemoryStream();
            letterTemplateStream.CopyTo( outputDocStream );
            outputDocStream.Seek( 0, SeekOrigin.Begin );

            using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
            {
                var newDocBody = outputDoc.MainDocumentPart.Document.Body;

                // start with a clean body
                newDocBody.RemoveAllChildren();

                // loop thru each merge item, using the template
                foreach ( var mergeObjects in mergeObjectsList )
                {

                    var tempMergeDocStream = new MemoryStream();
                    letterTemplateStream.Position = 0;
                    letterTemplateStream.CopyTo( tempMergeDocStream );
                    tempMergeDocStream.Position = 0;
                    var tempMergeDoc = WordprocessingDocument.Open( tempMergeDocStream, true );

                    var xdoc = tempMergeDoc.MainDocumentPart.GetXDocument();
                    foreach ( var r in mergeObjects )
                    {
                        OpenXmlRegex.Match( xdoc.Nodes().OfType<XElement>(), new Regex( r.Key, RegexOptions.Multiline ), ( x, m ) =>
                        {
                            string todo = "hello";
                        } );

                        OpenXmlRegex.Replace( xdoc.Nodes().OfType<XElement>(), new Regex( r.Key ), r.Value.ToString(), ( x, m ) =>
                                        {
                                            return true;
                                        } );
                    }

                    tempMergeDoc.MainDocumentPart.PutXDocument();

                    foreach ( var childBodyItem in tempMergeDoc.MainDocumentPart.Document.Body )
                    {
                        var clonedChild = childBodyItem.CloneNode( true );
                        newDocBody.AppendChild( clonedChild );
                    }

                    // add page break
                    newDocBody.AppendChild( new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page } );
                }

                // remove last page break
                var lastBr = newDocBody.LastChild as DocumentFormat.OpenXml.Wordprocessing.Break;
                if ( lastBr != null )
                {
                    newDocBody.RemoveChild( lastBr );
                }
            }

            BinaryFile outputFile = new BinaryFile();
            // TODO

            outputFile.ContentStream = outputDocStream;
            binaryFileService.Add( outputFile );
            rockContext.SaveChanges();
            return outputFile.Id;
        }

        /// <summary>
        /// Makes the labels.
        /// </summary>
        /// <param name="templateBinaryFileId">The template binary file identifier.</param>
        /// <param name="mergeObjectsList">The merge objects list.</param>
        /// <returns></returns>
        public static int? MakeLabels( int templateBinaryFileId, List<Dictionary<string, object>> mergeObjectsList )
        {
            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            var templateBinaryFile = binaryFileService.Get( templateBinaryFileId );
            if ( templateBinaryFile == null )
            {
                return null;
            }

            var letterTemplateStream = templateBinaryFile.ContentStream;
            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            MemoryStream outputDocStream = new MemoryStream();
            letterTemplateStream.CopyTo( outputDocStream );
            outputDocStream.Seek( 0, SeekOrigin.Begin );

            int mergeItemIndex = 0;
            int mergeItemCount = mergeObjectsList.Count();


            using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
            {
                var newDocBody = outputDoc.MainDocumentPart.Document.Body;
                var tables = newDocBody.ChildElements.OfType<DocumentFormat.OpenXml.Wordprocessing.Table>();

                foreach ( var table in tables )
                {
                    foreach ( var row in table.ChildElements.OfType<TableRow>() )
                    {
                        foreach ( var cell in row.ChildElements.OfType<TableCell>().Where( a => a.InnerText.Contains( "{{" ) ).ToList() )
                        {
                            if ( mergeItemIndex >= ( mergeItemCount - 1 ) )
                            {
                                // out of data so hide cell contents
                                var emptyCell = new TableCell( new DocumentFormat.OpenXml.Wordprocessing.Paragraph[] { new DocumentFormat.OpenXml.Wordprocessing.Paragraph() } );
                                cell.Parent.ReplaceChild( emptyCell, cell );
                            }
                            else
                            {
                                XElement[] xe = new XElement[] { XElement.Parse( cell.OuterXml ) };

                                foreach ( var r in mergeObjectsList[mergeItemIndex] )
                                {
                                    OpenXmlRegex.Match( xe, new Regex( r.Key, RegexOptions.Multiline ), ( x, m ) =>
                                    {
                                        // TODO
                                        string todo = "hello";
                                    } );


                                    OpenXmlRegex.Replace( xe, new Regex( r.Key ), r.Value.ToString(), ( x, m ) =>
                                    {
                                        return true;
                                    } );
                                }

                                XDocument xdoc = XDocument.Parse( cell.OuterXml );

                                var newCell = new TableCell( xe[0].ToString() );

                                cell.Parent.ReplaceChild( newCell, cell );

                                mergeItemIndex++;
                            }


                        }
                    }
                }
            }

            BinaryFile outputFile = new BinaryFile();
            // TODO

            outputFile.ContentStream = outputDocStream;
            binaryFileService.Add( outputFile );
            rockContext.SaveChanges();
            return outputFile.Id;
        }
    }
}
