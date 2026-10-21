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
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Extension;
using Rock.Lava;
using Rock.Model;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// Base class for merge template types (i.e. Word Document, HTML, etc) 
    /// </summary>
    public abstract class MergeTemplateType : Component
    {
        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public abstract List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public abstract BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields );

        /// <summary>
        /// Creates the merged document so it can be sent straight to the browser
        /// without leaving a saved file behind. The default implementation calls
        /// <see cref="CreateDocument(MergeTemplate, List{object}, Dictionary{string, object})"/>,
        /// reads the file it created and then deletes that file when it is temporary.
        /// Merge template types that can build the document in memory should
        /// override this method so no file is ever saved.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns>The merged document, or <c>null</c> if it could not be created.</returns>
        [Rock.Attribute.RockInternal( "17.12" )]
        public virtual MergeTemplateDocument CreateDocumentForDownload( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            var binaryFile = CreateDocument( mergeTemplate, mergeObjectList, globalMergeFields );
            if ( binaryFile == null )
            {
                return null;
            }

            /*
                10/21/2026 - MSE

                Merge output used to be left as a temporary file under the unsecured
                default file type with no parent entity, so GetFile.ashx served it to
                anyone who guessed the Id until Rock Cleanup removed it. For merge
                template types that only implement CreateDocument we read the bytes
                back and delete the temporary file right away, so the output only
                exists for the length of this request. A file the merge type saved as
                not temporary belongs to that type and is left alone.

                Reason: Merged documents must not be downloadable by Id.
            */
            if ( binaryFile.Id == 0 )
            {
                // The merge type returned a file it never saved, so there is nothing to delete.
                return CreateDocumentFromUnsavedFile( binaryFile );
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var savedFile = binaryFileService.Get( binaryFile.Id );
                if ( savedFile == null )
                {
                    return null;
                }

                byte[] bytes;

                try
                {
                    /*
                        10/21/2026 - MSE

                        The FileSystem storage provider returns an open FileStream that
                        does not allow delete sharing. The stream must be closed before
                        the file is deleted, or the delete fails and leaves the merged
                        file on disk.

                        Reason: Close the stored content before deleting the file.
                    */
                    using ( var contentStream = savedFile.ContentStream )
                    {
                        bytes = contentStream?.ReadBytesToEnd() ?? new byte[0];
                    }
                }
                finally
                {
                    /*
                        10/21/2026 - MSE

                        The delete runs in a finally so a temporary file is still removed
                        when reading its content throws. Any read error then continues
                        up to the caller unchanged.

                        Reason: Never leave merge output behind when the read fails.
                    */
                    if ( savedFile.IsTemporary )
                    {
                        TryDeleteTemporaryFile( rockContext, binaryFileService, savedFile );
                    }
                }

                return new MergeTemplateDocument( new MemoryStream( bytes ), savedFile.FileName, savedFile.MimeType );
            }
        }

        /// <summary>
        /// Creates the download document from a file that the merge template type
        /// returned without saving it to the database.
        /// </summary>
        /// <param name="binaryFile">The unsaved binary file.</param>
        /// <returns>The document, or <c>null</c> if the file content could not be read.</returns>
        private static MergeTemplateDocument CreateDocumentFromUnsavedFile( BinaryFile binaryFile )
        {
            Stream contentStream;

            try
            {
                contentStream = binaryFile.ContentStream;
            }
            catch
            {
                /*
                    10/21/2026 - MSE

                    If a third party CreateDocument disposed its stream before returning,
                    the ContentStream getter tries to reopen it through the storage
                    provider, which an unsaved file usually does not have, so the getter
                    throws. Treat that the same as a missing stream so the caller shows
                    its normal "could not be loaded" message instead of an error.

                    Reason: An unreadable unsaved file returns null, not an exception.
                */
                contentStream = null;
            }

            if ( contentStream == null || !contentStream.CanRead )
            {
                return null;
            }

            return new MergeTemplateDocument( new MemoryStream( contentStream.ReadBytesToEnd() ), binaryFile.FileName, binaryFile.MimeType );
        }

        /// <summary>
        /// Deletes a temporary merge output file when nothing else depends on it.
        /// Failures are logged and never thrown.
        /// </summary>
        /// <param name="rockContext">The context that loaded the file.</param>
        /// <param name="binaryFileService">The binary file service that loaded the file.</param>
        /// <param name="binaryFile">The temporary binary file to delete.</param>
        private static void TryDeleteTemporaryFile( RockContext rockContext, BinaryFileService binaryFileService, BinaryFile binaryFile )
        {
            try
            {
                // Match Rock Cleanup: only delete a temporary file nothing else depends on.
                if ( binaryFileService.CanDelete( binaryFile, out _ ) )
                {
                    binaryFileService.Delete( binaryFile );
                    rockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                // Intentionally not rethrown: the delete is best effort. Rock Cleanup removes
                // any temporary file left behind, and a delete error must not hide the
                // document from the caller or replace an earlier read error.
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Gets the lava debug information.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public virtual string GetLavaDebugInfo( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            return GetDefaultLavaDebugInfo( mergeObjectList, globalMergeFields );
        }

        /// <summary>
        /// Gets the supported file extensions 
        /// Returns NULL if the file extension doesn't matter or doesn't apply
        /// Rock will use this to warn the user if the file extension isn't supported
        /// </summary>
        /// <value>
        /// The supported file extensions.
        /// </value>
        public virtual IEnumerable<string> SupportedFileExtensions
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the default lava debug information.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <param name="preText">The pre text.</param>
        /// <returns></returns>
        public static string GetDefaultLavaDebugInfo( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields, string preText = null )
        {
            var debugMergeFields = new LavaDataDictionary();

            if ( mergeObjectList.Count >= 1 )
            {
                debugMergeFields.Add( "Row", mergeObjectList[0] );
            }

            foreach ( var mergeField in globalMergeFields )
            {
                debugMergeFields.Add( mergeField.Key, mergeField.Value );
            }

            return debugMergeFields.lavaDebugInfo( null, preText );
        }

        /// <summary>
        /// The RegEx for finding the "next" delimiter/indicator
        /// </summary>
        protected readonly Regex nextRecordRegEx = new Regex( @"{%\s*\bnext\b\s*%}", RegexOptions.IgnoreCase );

        /// <summary>
        /// The RegEx for finding the "next_empty" delimiter/indicator
        /// </summary>
        protected readonly Regex nextEmptyRecordRegEx = new Regex( @"{%\s*\bnext_empty\b\s*%}", RegexOptions.IgnoreCase );

        /// <summary>
        /// The RegEx of "." that matches anything
        /// </summary>
        protected readonly Regex regExDot = new Regex( "." );

        /// <summary>
        /// The RegEx to detect if the text has {{ }} tags in it
        /// </summary>
        protected readonly Regex lavaRegEx = new Regex( @"\{\{.+?\}\}", RegexOptions.Multiline );
    }
}
