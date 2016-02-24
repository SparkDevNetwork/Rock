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
using System.IO;
using System.Linq;
using System.Web;
using Rock.Utility;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class ImageUploader : FileUploader
    {
        /// <summary>
        /// Gets the file bytes.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <returns></returns>
        public override Stream GetFileContentStream( HttpContext context, HttpPostedFile uploadedFile )
        {
            var enableResize = context.Request.QueryString["enableResize"] != null;
            return ImageUtilities.GetFileContentStream( uploadedFile, enableResize );
        }

        /// <summary>
        /// Validates the type of the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <exception cref="WebFaultException{System.String}">Filetype now allowed</exception>
        public override void ValidateFileType( HttpContext context, HttpPostedFile uploadedFile )
        {
            base.ValidateFileType( context, uploadedFile );

            var globalAttributesCache = GlobalAttributesCache.Read();
            IEnumerable<string> contentImageFileTypeWhiteList = ( globalAttributesCache.GetValue( "ContentImageFiletypeWhitelist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );

            // clean up list
            contentImageFileTypeWhiteList = contentImageFileTypeWhiteList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            string fileExtension = Path.GetExtension( uploadedFile.FileName ).ToLower().TrimStart( new char[] { '.' } );
            if ( !contentImageFileTypeWhiteList.Contains( fileExtension ) )
            {
                throw new Rock.Web.FileUploadException( "Image filetype not allowed", System.Net.HttpStatusCode.NotAcceptable );
            }
        }
    }
}