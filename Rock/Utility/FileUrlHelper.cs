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
using Rock.Model;
using Rock.Security;

namespace Rock.Utility
{
    /// <summary>
    /// Helper class for generating file URLs
    /// </summary>
    public static class FileUrlHelper
    {
        /// <summary>
        /// Gets the file URL for the specified file ID.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="endpointType">The endpoint type.</param>
        /// <param name="publicAppRoot">The public application root.</param>
        /// <param name="fileName">The file name (optional).</param>
        /// <param name="width">The width (optional).</param>
        /// <param name="height">The height (optional).</param>
        /// <returns></returns>
        public static string GetFileUrl( int fileId, string endpointType = "GetFile", string publicAppRoot = null, string fileName = null, int? width = null, int? height = null )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            bool disablePredictableIds;
            switch ( endpointType )
            {
                case "GetFile":
                case "GetImage":
                case "GetAvatar":
                    disablePredictableIds = securitySettings.DisablePredictableIds;
                    break;
                default:
                    throw new ArgumentException( "Invalid endpoint type. Supported types are 'GetFile', 'GetImage', and 'GetAvatar'." );
            }

            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.AppendFormat( "{0}{1}.ashx?", System.Web.VirtualPathUtility.ToAbsolute( "~" ), endpointType );

            if ( disablePredictableIds )
            {
                urlBuilder.AppendFormat( "fileIdKey={0}", IdHasher.Instance.GetHash( fileId ) );
            }
            else
            {
                urlBuilder.AppendFormat( "id={0}", fileId );
            }

            if ( !string.IsNullOrWhiteSpace( fileName ) )
            {
                urlBuilder.AppendFormat( "&fileName={0}", fileName );
            }

            if ( width.HasValue )
            {
                urlBuilder.AppendFormat( "&width={0}", width.Value );
            }

            if ( height.HasValue )
            {
                urlBuilder.AppendFormat( "&height={0}", height.Value );
            }

            return publicAppRoot != null ? publicAppRoot + urlBuilder.ToString() : urlBuilder.ToString();
        }


        /// <summary>
        /// Gets the file URL for the specified file ID.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="endpointType">The endpoint type.</param>
        /// <param name="publicAppRoot">The public application root.</param>
        /// <param name="fileName">The file name (optional).</param>
        /// <param name="width">The width (optional).</param>
        /// <param name="height">The height (optional).</param>
        /// <returns></returns>
        public static string GetFileUrl( int? fileId, string endpointType = "GetFile", string publicAppRoot = null, string fileName = null, int? width = null, int? height = null )
        {
            return fileId.HasValue ? GetFileUrl( fileId.Value, endpointType, publicAppRoot, fileName, width, height ) : string.Empty;
        }

        /// <summary>
        /// Gets the file URL for the specified file GUID.
        /// </summary>
        /// <param name="fileGuid">The file GUID.</param>
        /// <param name="endpointType">The endpoint type.</param>
        /// <param name="publicAppRoot">The public application root.</param>
        /// <param name="fileName">The file name (optional).</param>
        /// <param name="width">The width (optional).</param>
        /// <param name="height">The height (optional).</param>
        /// <returns></returns>
        public static string GetFileUrl( Guid fileGuid, string endpointType = "GetFile", string publicAppRoot = null, string fileName = null, int? width = null, int? height = null )
        {
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.AppendFormat( "{0}{1}.ashx?guid={2}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), endpointType, fileGuid );

            if ( !string.IsNullOrWhiteSpace( fileName ) )
            {
                urlBuilder.AppendFormat( "&fileName={0}", fileName );
            }

            if ( width.HasValue )
            {
                urlBuilder.AppendFormat( "&width={0}", width.Value );
            }

            if ( height.HasValue )
            {
                urlBuilder.AppendFormat( "&height={0}", height.Value );
            }

            return publicAppRoot != null ? publicAppRoot + urlBuilder.ToString() : urlBuilder.ToString();
        }
    }
}