// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using Rock.Data;
using Rock.Model;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// 
    /// </summary>
    [System.ComponentModel.Description( "A Word Document merge template" )]
    [Export( typeof( MergeTemplateType ) )]
    [ExportMetadata( "ComponentName", "Word Document" )]
    public class WordDocumentMergeTemplateType : MergeTemplateType
    {
        public override List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public override BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            this.Exceptions = new List<Exception>();
            BinaryFile outputBinaryFile = null;

            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            var templateBinaryFile = binaryFileService.Get( mergeTemplate.TemplateBinaryFileId );
            if ( templateBinaryFile == null )
            {
                return null;
            }

            var sourceTemplateStream = templateBinaryFile.ContentStream;

            // Start by creating a new document with the contents of the Template (so that Styles, etc get included)
            using ( MemoryStream outputDocStream = new MemoryStream() )
            {
                sourceTemplateStream.CopyTo( outputDocStream );
                outputDocStream.Seek( 0, SeekOrigin.Begin );

                // now that we have the outputdoc started, simplify the sourceTemplate
                var simplifiedDoc = WordprocessingDocument.Open( sourceTemplateStream, true );
                MarkupSimplifier.SimplifyMarkup( simplifiedDoc, this.simplifyMarkupSettingsAll );

                //// simplify any nodes that have Lava in it that might not have been caught by the MarkupSimplifier
                //// MarkupSimplifier only merges superfluous runs that are children of a paragraph
                var simplifiedDocX = simplifiedDoc.MainDocumentPart.GetXDocument();
                OpenXmlRegex.Match(
                                simplifiedDocX.Elements(),
                                this.lavaRegEx,
                                ( x, m ) =>
                                {
                                    foreach ( var nonParagraphRunsParent in x.DescendantNodes().OfType<XElement>().Where( a => a.Parent != null && a.Name != null )
                                .Where( a => ( a.Name.LocalName == "r" ) ).Select( a => a.Parent ).Distinct().ToList() )
                                    {
                                        if ( lavaRegEx.IsMatch( nonParagraphRunsParent.Value ) )
                                        {
                                            var tempParent = XElement.Parse( new Paragraph().OuterXml );
                                            tempParent.Add( nonParagraphRunsParent.Nodes() );
                                            tempParent = MarkupSimplifier.MergeAdjacentSuperfluousRuns( tempParent );
                                            nonParagraphRunsParent.ReplaceNodes( tempParent.Nodes() );
                                        }
                                    }
                                } );

                simplifiedDoc.MainDocumentPart.PutXDocument();

                sourceTemplateStream.Seek( 0, SeekOrigin.Begin );

                bool? allSameParent = null;

                using ( WordprocessingDocument outputDoc = WordprocessingDocument.Open( outputDocStream, true ) )
                {
                    var xdoc = outputDoc.MainDocumentPart.GetXDocument();
                    var outputBodyNode = xdoc.DescendantNodes().OfType<XElement>().FirstOrDefault( a => a.Name.LocalName.Equals( "body" ) );
                    outputBodyNode.RemoveNodes();

                    int recordIndex = 0;
                    int? lastRecordIndex = null;
                    int recordCount = mergeObjectList.Count();
                    while ( recordIndex < recordCount )
                    {
                        if ( lastRecordIndex.HasValue && lastRecordIndex == recordIndex )
                        {
                            // something went wrong, so throw to avoid spinning infinately
                            throw new Exception( "Unexpected unchanged recordIndex" );
                        }

                        lastRecordIndex = recordIndex;
                        using ( var tempMergeTemplateStream = new MemoryStream() )
                        {
                            sourceTemplateStream.Position = 0;
                            sourceTemplateStream.CopyTo( tempMergeTemplateStream );
                            tempMergeTemplateStream.Position = 0;
                            var tempMergeWordDoc = WordprocessingDocument.Open( tempMergeTemplateStream, true );
                            var tempMergeTemplateX = tempMergeWordDoc.MainDocumentPart.GetXDocument();
                            var tempMergeTemplateBodyNode = tempMergeTemplateX.DescendantNodes().OfType<XElement>().FirstOrDefault( a => a.Name.LocalName.Equals( "body" ) );

                            // find all the Nodes that have a {% next %}.  
                            List<XElement> nextIndicatorNodes = new List<XElement>();

                            OpenXmlRegex.Match(
                                tempMergeTemplateX.Elements(),
                                this.nextRecordRegEx,
                                ( x, m ) =>
                                {
                                    nextIndicatorNodes.Add( x );
                                } );

                            allSameParent = allSameParent ?? nextIndicatorNodes.Count > 1 && nextIndicatorNodes.Select( a => a.Parent ).Distinct().Count() == 1;

                            List<XContainer> recordContainerNodes = new List<XContainer>();
                            if ( !nextIndicatorNodes.Any() )
                            {
                                // if there aren't any Next indicators, create one (which means the entire template doc is a record)
                                var addedNode = XElement.Parse( new Paragraph( new Run( new Text( " {% next %} " ) ) ).OuterXml );
                                tempMergeTemplateBodyNode.Add( addedNode );
                                nextIndicatorNodes.Add( addedNode );
                            }

                            foreach ( var nextIndicatorNodeParent in nextIndicatorNodes.Select( a => a.Parent ).Where( a => a != null ) )
                            {
                                XContainer recordContainerNode = nextIndicatorNodeParent;
                                if ( !allSameParent.Value )
                                {
                                    // go up the parent nodes until we have more than one "Next" descendent so that we know what to consider our record container
                                    while ( recordContainerNode.Parent != null )
                                    {
                                        if ( this.nextRecordRegEx.Matches( recordContainerNode.Parent.Value ).Count == 1 )
                                        {
                                            // still just the one "next" indicator, so go out another parent
                                            recordContainerNode = recordContainerNode.Parent;
                                        }
                                        else
                                        {
                                            // we went too far up the parents and found multiple "next" children, so use this node as the recordContainerNode
                                            break;
                                        }
                                    }
                                }

                                if ( !recordContainerNodes.Contains( recordContainerNode ) )
                                {
                                    recordContainerNodes.Add( recordContainerNode );
                                }
                            }

                            foreach ( var recordContainerNode in recordContainerNodes )
                            {
                                //// loop thru each of the recordContainerNodes
                                //// If we have more records than nodes, we'll jump out to the outer "while" and append another template and keep going

                                XContainer mergedXRecord;

                                var recordContainerNodeXml = recordContainerNode.ToString( SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces ).ReplaceWordChars();

                                if ( recordIndex >= recordCount )
                                {
                                    // out of records, so clear out any remaining template nodes that haven't been merged
                                    string xml = recordContainerNodeXml;
                                    mergedXRecord = XElement.Parse( xml ) as XContainer;
                                    OpenXmlRegex.Replace( mergedXRecord.Nodes().OfType<XElement>(), this.regExDot, string.Empty, ( a, b ) => { return true; } );

                                    recordIndex++;
                                }
                                else
                                {
                                    List<string> xmlChunks = new List<string>();

                                    if ( allSameParent.Value )
                                    {
                                        // if all the nextRecord nodes have the same parent, just split the XML for each record and reassemble it when done
                                        xmlChunks.AddRange( this.nextRecordRegEx.Split( recordContainerNodeXml ) );
                                    }
                                    else
                                    {
                                        string xmlChunk = nextRecordRegEx.Replace( recordContainerNodeXml, string.Empty );
                                        xmlChunks.Add( xmlChunk );
                                    }

                                    string mergedXml = string.Empty;

                                    foreach ( var xml in xmlChunks )
                                    {
                                        if ( xml.HasMergeFields() )
                                        {
                                            if ( recordIndex < recordCount )
                                            {
                                                try
                                                {
                                                    DotLiquid.Hash wordMergeObjects = new DotLiquid.Hash();
                                                    wordMergeObjects.Add( "Row", mergeObjectList[recordIndex] );

                                                    foreach ( var field in globalMergeFields )
                                                    {
                                                        wordMergeObjects.Add( field.Key, field.Value );
                                                    }

                                                    mergedXml += xml.ResolveMergeFields( wordMergeObjects, true, true );
                                                }
                                                catch ( Exception ex )
                                                {
                                                    // if ResolveMergeFields failed, log the exception, then just return the orig xml
                                                    this.Exceptions.Add( ex );
                                                    mergedXml += xml;
                                                }

                                                recordIndex++;
                                            }
                                            else
                                            {
                                                // out of records, so just keep it as the templated xml
                                                mergedXml += xml;
                                            }
                                        }
                                        else
                                        {
                                            mergedXml += xml;
                                        }
                                    }

                                    mergedXRecord = XElement.Parse( mergedXml ) as XContainer;
                                }

                                // remove the orig nodes and replace with merged nodes
                                recordContainerNode.RemoveNodes();
                                recordContainerNode.Add( mergedXRecord.Nodes().OfType<XElement>() );

                                var mergedRecordContainer = XElement.Parse( recordContainerNode.ToString( SaveOptions.DisableFormatting ) );
                                if ( recordContainerNode.Parent != null )
                                {
                                    // the recordContainerNode is some child/descendent of <body>
                                    recordContainerNode.ReplaceWith( mergedRecordContainer );
                                }
                                else
                                {
                                    // the recordContainerNode is the <body>
                                    recordContainerNode.RemoveNodes();
                                    recordContainerNode.Add( mergedRecordContainer.Nodes() );

                                    if ( recordIndex < recordCount )
                                    {
                                        // add page break
                                        var pageBreakXml = new Paragraph( new Run( new Break() { Type = BreakValues.Page } ) ).OuterXml;
                                        var pageBreak = XElement.Parse( pageBreakXml, LoadOptions.None );
                                        var lastParagraph = recordContainerNode.Nodes().OfType<XElement>().Where( a => a.Name.LocalName == "p" ).LastOrDefault();
                                        if ( lastParagraph != null )
                                        {
                                            lastParagraph.AddAfterSelf( pageBreak );
                                        }
                                    }
                                }
                            }

                            outputBodyNode.Add( tempMergeTemplateBodyNode.Nodes() );
                        }
                    }

                    // remove all the 'next' delimiters
                    OpenXmlRegex.Replace( outputBodyNode.Nodes().OfType<XElement>(), this.nextRecordRegEx, string.Empty, ( xx, mm ) => { return true; } );

                    // remove all but the last SectionProperties element (there should only be one per section (body))
                    var sectPrItems = outputBodyNode.Nodes().OfType<XElement>().Where( a => a.Name.LocalName == "sectPr" );
                    foreach ( var extra in sectPrItems.Where( a => a != sectPrItems.Last() ).ToList() )
                    {
                        extra.Remove();
                    }

                    // renumber all the ids to make sure they are unique
                    var idAttrs = xdoc.DescendantNodes().OfType<XElement>().Where( a => a.HasAttributes ).Select( a => a.Attribute( "id" ) ).Where( s => s != null );
                    int lastId = 1;
                    foreach ( var attr in idAttrs )
                    {
                        attr.Value = lastId.ToString();
                        lastId++;
                    }

                    // remove the last pagebreak
                    MarkupSimplifier.SimplifyMarkup( outputDoc, new SimplifyMarkupSettings { RemoveLastRenderedPageBreak = true } );

                    // If you want to see validation errors
                    /*
                    var validator = new OpenXmlValidator();
                    var errors = validator.Validate( outputDoc ).ToList();
                    */
                }

                outputBinaryFile = new BinaryFile();
                outputBinaryFile.IsTemporary = true;
                outputBinaryFile.ContentStream = outputDocStream;
                outputBinaryFile.FileName = "MergeTemplateOutput" + Path.GetExtension( templateBinaryFile.FileName );
                outputBinaryFile.MimeType = templateBinaryFile.MimeType;
                outputBinaryFile.BinaryFileTypeId = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id;

                binaryFileService.Add( outputBinaryFile );
                rockContext.SaveChanges();
            }

            return outputBinaryFile;
        }

        /// <summary>
        /// The simplify markup settings all
        /// </summary>
        private SimplifyMarkupSettings simplifyMarkupSettingsAll = new SimplifyMarkupSettings
        {
            NormalizeXml = true,
            RemoveWebHidden = true,
            RemoveBookmarks = true,
            RemoveGoBackBookmark = true,
            RemoveMarkupForDocumentComparison = true,
            RemoveComments = true,
            RemoveContentControls = true,
            RemoveEndAndFootNotes = true,
            RemoveFieldCodes = true,
            RemoveLastRenderedPageBreak = true,
            RemovePermissions = true,
            RemoveProof = true,
            RemoveRsidInfo = true,
            RemoveSmartTags = true,
            RemoveSoftHyphens = true,
            ReplaceTabsWithSpaces = false
        };
    }
}
