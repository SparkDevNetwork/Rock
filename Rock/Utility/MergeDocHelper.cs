using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
        public int? MakeDocument( int templateBinaryFileId, List<Dictionary<string, object>> mergeObjectsList )
        {
            // see HowTos at https://msdn.microsoft.com/EN-US/library/office/cc850849.aspx

            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            MemoryStream outputDocStream = new MemoryStream();

            var templateBinaryFile = binaryFileService.Get( templateBinaryFileId );
            if ( templateBinaryFile == null )
            {
                return null;
            }

            var letterTemplateStream = templateBinaryFile.ContentStream;
            letterTemplateStream.CopyTo( outputDocStream );
            outputDocStream.Seek( 0, SeekOrigin.Begin );

            using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
            {
                var newDocBody = outputDoc.MainDocumentPart.Document.Body;

                // for each Merge Item, we want to start with the Body of the orig template, 
                // which was copied into the new document (above)
                // Do this by cloning the template body...
                var templateBody = newDocBody.CloneNode( true );

                // ...then removing the contents of it from our new document
                newDocBody.RemoveAllChildren();

                // loop thru each merge item, using the template
                foreach ( var mergeObjects in mergeObjectsList )
                {

                    // make a copy of each of the child elements of the template for the merge item
                    foreach ( var childEl in templateBody.ChildElements )
                    {
                        var clone = childEl.CloneNode( true );

                        // recursivly find any Text nodes, replacing the MergeField with the Value
                        ResolveMergeFieldsRecursive( clone, mergeObjects );
                        newDocBody.AppendChild( clone );
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
            outputFile.ContentStream = outputDocStream;
            binaryFileService.Add( outputFile );
            rockContext.SaveChanges();
            return outputFile.Id;
        }

        /// <summary>
        /// Searches the and replace recursive.
        /// </summary>
        /// <param name="el">The el.</param>
        /// <param name="replacements">The replacements.</param>
        public void ResolveMergeFieldsRecursive( OpenXmlElement el, Dictionary<string, object> mergeObjects )
        {
            if ( el is Text )
            {
                string newText = ( el as Text ).InnerText;
                newText = newText.ResolveMergeFields( mergeObjects );

                Text newTextNode = new Text
                {
                    Text = newText,
                    Space = SpaceProcessingModeValues.Preserve
                };

                foreach ( var child in el.ChildElements )
                {
                    newTextNode.AppendChild( child.CloneNode( true ) );
                }

                el.Parent.ReplaceChild( newTextNode, el );

                el = newTextNode;
            }

            foreach ( var child in el.ChildElements )
            {
                ResolveMergeFieldsRecursive( child, mergeObjects );
            }
        }
    }
}
