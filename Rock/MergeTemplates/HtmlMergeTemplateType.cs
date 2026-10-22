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
using System.ComponentModel.Composition;
using System.IO;
using System.Web;
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
    [Rock.SystemGuid.EntityTypeGuid( "5FBFF041-9EDC-41A3-8A92-D7AC4FF88221")]
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
            BinaryFile outputBinaryFile = null;

            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );

            var outputStream = GetMergedDocumentOutputStream( binaryFileService, mergeTemplate, mergeObjectList, globalMergeFields, out var templateBinaryFile );
            if ( outputStream == null )
            {
                return null;
            }

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

        /// <inheritdoc/>
        [Rock.Attribute.RockInternal( "17.12" )]
        public override MergeTemplateDocument CreateDocumentForDownload( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            /*
                10/21/2026 - MSE

                Return the merged HTML stream directly instead of saving it as a
                temporary BinaryFile under the unsecured default file type, which
                GetFile.ashx would serve to anyone by Id.

                Reason: Merged documents must not be saved where they can be downloaded by Id.
            */
            var binaryFileService = new BinaryFileService( new RockContext() );

            var outputStream = GetMergedDocumentOutputStream( binaryFileService, mergeTemplate, mergeObjectList, globalMergeFields, out var templateBinaryFile );
            if ( outputStream == null )
            {
                return null;
            }

            return new MergeTemplateDocument( outputStream,
                "MergeTemplateOutput" + Path.GetExtension( templateBinaryFile.FileName ),
                templateBinaryFile.MimeType );
        }

        /// <summary>
        /// Resolves the template's Lava and returns the merged HTML document as a stream.
        /// The stream is left positioned at its end, as HtmlDocument.Save leaves it.
        /// </summary>
        /// <param name="binaryFileService">The binary file service used to load the template file.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <param name="templateBinaryFile">The template file, or <c>null</c> when it could not be found.</param>
        /// <returns>The merged document, or <c>null</c> when the template file could not be found.</returns>
        private MemoryStream GetMergedDocumentOutputStream( BinaryFileService binaryFileService, MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields, out BinaryFile templateBinaryFile )
        {
            this.Exceptions = new List<Exception>();

            templateBinaryFile = binaryFileService.Get( mergeTemplate.TemplateBinaryFileId );
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

            return outputStream;
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
        public override List<Exception> Exceptions
        {
            get
            {
                if ( HttpContext.Current != null )
                {
                    return HttpContext.Current.Items[$"{this.GetType().FullName}:Exceptions"] as List<Exception>;
                }

                return _nonHttpContextExceptions;
            }

            set
            {
                if ( HttpContext.Current != null )
                {
                    HttpContext.Current.Items[$"{this.GetType().FullName}:Exceptions"] = value;
                }
                else
                {
                    _nonHttpContextExceptions = value;
                }
            }
        }

        /// <summary>
        /// Thread safe storage of property when HttpContext.Current is null
        /// NOTE: ThreadStatic is per thread, but ASP.NET threads are ThreadPool threads, so they will be used again.
        /// see https://www.hanselman.com/blog/ATaleOfTwoTechniquesTheThreadStaticAttributeAndSystemWebHttpContextCurrentItems.aspx
        /// So be careful and only use the [ThreadStatic] trick if absolutely necessary
        /// </summary>
        [ThreadStatic]
        private static List<Exception> _nonHttpContextExceptions = null;
    }
}
