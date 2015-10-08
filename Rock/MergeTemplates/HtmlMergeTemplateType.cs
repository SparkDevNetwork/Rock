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
using HtmlAgilityPack;
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
                return new string[] { "htm", "html" };
            }
        }
        
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

            string templateHtml = templateBinaryFile.ContentsToString();
            var htmlMergeObjects = GetHtmlMergeObjects( mergeObjectList, globalMergeFields );
            string outputHtml = templateHtml.ResolveMergeFields( htmlMergeObjects );
            HtmlDocument outputDoc = new HtmlDocument();
            outputDoc.LoadHtml( outputHtml );
            var outputStream = new MemoryStream();
            outputDoc.Save( outputStream );

            outputBinaryFile = new BinaryFile();
            outputBinaryFile.IsTemporary = true;
            outputBinaryFile.ContentStream = outputStream;
            outputBinaryFile.FileName = "MergeTemplateOutput" + Path.GetExtension( templateBinaryFile.FileName );
            outputBinaryFile.MimeType = templateBinaryFile.MimeType;
            outputBinaryFile.BinaryFileTypeId = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id;

            binaryFileService.Add( outputBinaryFile );
            rockContext.SaveChanges();

            return outputBinaryFile;
        }

        /// <summary>
        /// Gets the HTML merge objects.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetHtmlMergeObjects( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            var htmlMergeObjects = new Dictionary<string, object>();
            htmlMergeObjects.Add( "Rows", mergeObjectList );
            
            foreach (var mergeField in globalMergeFields)
            {
                htmlMergeObjects.Add( mergeField.Key, mergeField.Value );
            }
            
            return htmlMergeObjects;
        }

        /// <summary>
        /// Gets the lava debug information.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public override string GetLavaDebugInfo( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            return GetHtmlMergeObjects( mergeObjectList, globalMergeFields ).lavaDebugInfo();
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
