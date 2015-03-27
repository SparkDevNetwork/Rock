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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using OpenXmlPowerTools;
using Rock.Data;
using Rock.Model;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// 
    /// </summary>
    [System.ComponentModel.Description( "An HTML Document merge template" )]
    [Export( typeof( MergeTemplateType ) )]
    [ExportMetadata( "ComponentName", "HTML Document" )]
    public class HtmlMergeTemplateType : MergeTemplateType
    {
        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectsList">The merge objects list.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<Dictionary<string, object>> mergeObjectsList )
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

                sourceTemplateStream.Seek( 0, SeekOrigin.Begin );

                bool? allSameParent = null;

                HtmlDocument outputDoc = new HtmlDocument();
                outputDoc.Load( outputDocStream );

                {
                    var outputBodyNode = outputDoc.DocumentNode.ChildNodes.FindFirst( "body" );
                    outputBodyNode.RemoveAllChildren();

                    int recordIndex = 0;
                    int recordCount = mergeObjectsList.Count();
                    while ( recordIndex < recordCount )
                    {
                        using ( var tempMergeTemplateStream = new MemoryStream() )
                        {
                            sourceTemplateStream.Position = 0;
                            sourceTemplateStream.CopyTo( tempMergeTemplateStream );
                            tempMergeTemplateStream.Position = 0;
                            var tempMergeWordDoc = new HtmlDocument();
                            tempMergeWordDoc.Load( tempMergeTemplateStream );
                            var tempMergeTemplateX = tempMergeWordDoc;
                            var tempMergeTemplateBodyNode = tempMergeTemplateX.DocumentNode.ChildNodes.FindFirst( "body" );

                            // find all the Nodes that have a {% next %}.  
                            List<HtmlNode> nextIndicatorNodes = new List<HtmlNode>();

                            foreach ( var node in tempMergeTemplateBodyNode.DescendantsAndSelf() )
                            {
                                if ( this.nextRecordRegEx.IsMatch( node.InnerHtml ) )
                                {
                                    nextIndicatorNodes.Add( node );
                                }
                            }

                            allSameParent = allSameParent ?? nextIndicatorNodes.Count > 1 && nextIndicatorNodes.Select( a => a.ParentNode ).Distinct().Count() == 1;

                            List<HtmlNode> recordContainerNodes = new List<HtmlNode>();

                            foreach ( var nextIndicatorNodeParent in nextIndicatorNodes.Select( a => a.ParentNode ).Where( a => a != null ) )
                            {
                                HtmlNode recordContainerNode = nextIndicatorNodeParent;
                                if ( !allSameParent.Value )
                                {
                                    // go up the parent nodes until we have more than one "Next" descendent so that we know what to consider our record container
                                    while ( recordContainerNode.ParentNode != null )
                                    {
                                        if ( this.nextRecordRegEx.Matches( recordContainerNode.ParentNode.InnerHtml ).Count == 1 )
                                        {
                                            // still just the one "next" indicator, so go out another parent
                                            recordContainerNode = recordContainerNode.ParentNode;
                                        }
                                        else
                                        {
                                            // we went too far up the parents and found multiple "next" children, so use this node as the recordContainerNode
                                            break;
                                        }

                                    }
                                }

                                recordContainerNodes.Add( recordContainerNode );
                            }

                            foreach ( var recordContainerNode in recordContainerNodes )
                            {
                                // loop thru each of the recordContainerNodes
                                // If we have more records than nodes, we'll jump out to the outer "while" and append another template and keep going

                                HtmlNode mergedXRecord;

                                var recordContainerNodeXml = recordContainerNode.OuterHtml;

                                if ( recordIndex >= recordCount )
                                {
                                    // out of records, so clear out any remaining template nodes that haven't been merged
                                    string xml = recordContainerNodeXml;
                                    mergedXRecord = HtmlNode.CreateNode( xml );
                                    
                                    //TODO
                                    

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
                                                    mergedXml += xml.ResolveMergeFields( mergeObjectsList[recordIndex], true, true ); ;
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

                                    mergedXRecord = HtmlNode.CreateNode( mergedXml );
                                }

                                // remove the orig nodes and replace with merged nodes
                                recordContainerNode.RemoveAllChildren();
                                recordContainerNode.AppendChildren( mergedXRecord.ChildNodes );

                                var mergedRecordContainer = HtmlNode.CreateNode( recordContainerNode.OuterHtml );
                                if ( recordContainerNode.ParentNode != null )
                                {
                                    // the recordContainerNode is some child/descendent of <body>
                                    recordContainerNode.ParentNode.ReplaceChild(recordContainerNode, mergedRecordContainer );
                                }
                                else
                                {
                                    // the recordContainerNode is the <body>
                                    recordContainerNode.RemoveAllChildren();
                                    recordContainerNode.AppendChildren( mergedRecordContainer.ChildNodes );
                                }
                            }

                            outputBodyNode.AppendChildren( tempMergeTemplateBodyNode.ChildNodes );
                        }
                    }

                    // remove all the 'next' delimiters
                    // TODO
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
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public override List<Exception> Exceptions { get; set; }
    }
}
