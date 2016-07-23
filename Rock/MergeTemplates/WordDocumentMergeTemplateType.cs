﻿// <copyright>
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
    [ExportMetadata( "ComponentName", "Word" )]
    public class WordDocumentMergeTemplateType : MergeTemplateType
    {
        /// <summary>
        /// Gets the supported file extensions
        /// Returns NULL if the file extension doesn't matter or doesn't apply
        /// Rock will use this to warn the user if the file extension isn't supported
        /// </summary>
        /// <value>
        /// The supported file extensions.
        /// </value>
        public override IEnumerable<string> SupportedFileExtensions
        {
            get
            {
                return new string[] { "docx" };
            }
        }

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
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

                XElement lastLavaNode = simplifiedDocX.DescendantNodes().OfType<XElement>().LastOrDefault( a => lavaRegEx.IsMatch( a.Value ) );

                // ensure there is a { Next } indicator after the last lava node in the template
                if (lastLavaNode != null)
                {
                    var nextRecordMatch = nextRecordRegEx.Match( lastLavaNode.Value );
                    if ( nextRecordMatch == null || !nextRecordMatch.Success )
                    {
                        // if the last lava node doesn't have a { next }, append to the end
                        lastLavaNode.Value += " {% next %} ";
                    }
                    else
                    {
                        if ( !lastLavaNode.Value.EndsWith( nextRecordMatch.Value ) )
                        {
                            // if the last lava node does have a { next }, but there is stuff after it, add it (just in case)
                            lastLavaNode.Value += " {% next %} ";
                        }
                    }
                }

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
                                    //// just in case they have shared parent node, or if there is trailing {{ next }} after the last lava 
                                    //// on the page, split the XML for each record and reassemble it when done
                                    List<string> xmlChunks = this.nextRecordRegEx.Split( recordContainerNodeXml ).ToList();

                                    string mergedXml = string.Empty;

                                    foreach ( var xml in xmlChunks )
                                    {
                                        bool incRecordIndex = true;
                                        if ( lavaRegEx.IsMatch(xml) )
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

                                                    var resolvedXml = xml.ResolveMergeFields( wordMergeObjects, true, true );
                                                    mergedXml += resolvedXml;
                                                    if (resolvedXml == xml)
                                                    {
                                                        // there weren't any MergeFields after all, so don't move to the next record
                                                        incRecordIndex = false;
                                                    }
                                                }
                                                catch ( Exception ex )
                                                {
                                                    // if ResolveMergeFields failed, log the exception, then just return the orig xml
                                                    this.Exceptions.Add( ex );
                                                    mergedXml += xml;
                                                }

                                                if ( incRecordIndex )
                                                {
                                                    recordIndex++;
                                                }
                                            }
                                            else
                                            {
                                                // out of records, so put a special '{% next_empty %}' that we can use to clear up unmerged parts of the template
                                                mergedXml += " {% next_empty %} " + xml;
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

                    // find all the 'next_empty' delimiters that we might have added and clear out the content in the paragraph nodes that follow
                    OpenXmlRegex.Match(
                        outputBodyNode.Nodes().OfType<XElement>(),
                        this.nextEmptyRecordRegEx,
                        ( xx, mm ) =>
                        {
                            var afterSiblings = xx.ElementsAfterSelf().ToList();
                            
                            // get all the paragraph elements after the 'next_empty' node and clear out the content
                            var nodesToClean = afterSiblings.Where( a => a.Name.LocalName == "p" ).ToList();
                            
                            // if the next_empty node has lava, clean that up too
                            var xxContent = xx.ToString();
                            if ( lavaRegEx.IsMatch(xxContent) )
                            {
                                nodesToClean.Add( xx );
                            }
                            
                            foreach (var node in nodesToClean)
                            {
                                // remove all child nodes from each paragraph node
                                if (node.HasElements)
                                {
                                    node.RemoveNodes();
                                }
                            }

                        } );

                    // remove all the 'next_empty' delimiters
                    OpenXmlRegex.Replace( outputBodyNode.Nodes().OfType<XElement>(), this.nextEmptyRecordRegEx, string.Empty, ( xx, mm ) => { return true; } );

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

                    DotLiquid.Hash globalMergeHash = new DotLiquid.Hash();
                    foreach ( var field in globalMergeFields )
                    {
                        globalMergeHash.Add( field.Key, field.Value );
                    }

                    HeaderFooterGlobalMerge( outputDoc, globalMergeHash );

                    // sweep thru any remaining un-merged body parts for any Lava having to do with Global merge fields
                    foreach ( var bodyTextPart in outputDoc.MainDocumentPart.Document.Body.Descendants<Text>() )
                    {
                        string nodeText = bodyTextPart.Text.ReplaceWordChars();
                        if ( lavaRegEx.IsMatch( nodeText ) )
                        {
                            bodyTextPart.Text = nodeText.ResolveMergeFields( globalMergeHash, true, true );
                        }
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
        /// Merges global merge fields into the Header and Footer parts of the document
        /// </summary>
        /// <param name="outputDoc">The output document.</param>
        /// <param name="globalMergeHash">The global merge hash.</param>
        private void HeaderFooterGlobalMerge( WordprocessingDocument outputDoc, DotLiquid.Hash globalMergeHash )
        {
            // make sure that all proof codes get removed so that the lava can be found
            MarkupSimplifier.SimplifyMarkup( outputDoc, new SimplifyMarkupSettings { RemoveProof = true } );

            // update the doc headers and footers for any Lava having to do with Global merge fields
            // from http://stackoverflow.com/a/19012057/1755417
            foreach( var headerFooterPart in outputDoc.MainDocumentPart.HeaderParts.OfType<OpenXmlPart>().Union(outputDoc.MainDocumentPart.FooterParts))
            {
                foreach (var currentParagraph in headerFooterPart.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    foreach ( var currentRun in currentParagraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>())
                    {
                        foreach ( var currentText in currentRun.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>() )
                        {
                            string nodeText = currentText.Text.ReplaceWordChars();
                            if ( lavaRegEx.IsMatch( nodeText ) )
                            {
                                currentText.Text = nodeText.ResolveMergeFields( globalMergeHash, true, true );
                            }
                        }
                    }
                }
            }
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
