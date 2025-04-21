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
using Rock.SystemGuid;

namespace Rock.Utility
{
    /// <summary>
    /// Helper class for generating file URLs
    /// </summary>
    public static class FileUrlHelper
    {
        #region GetFileUrl Methods

        /// <summary>
        /// Method used to get the URL for a file given its binary file ID
        /// </summary>
        /// <param name="binaryFileId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetFileUrl( int binaryFileId, GetFileUrlOptions options = null )
        {
            return GetFileUrlInternal( GetFileIdentifierParameter( binaryFileId ), options );
        }

        /// <summary>
        /// Method used to get the URL for a file given its binary file ID
        /// </summary>
        /// <param name="binaryFileId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetFileUrl( int? binaryFileId, GetFileUrlOptions options = null )
        {
            if ( !binaryFileId.HasValue )
            {
                return string.Empty;
            }

            return GetFileUrlInternal( GetFileIdentifierParameter( binaryFileId.Value ), options );
        }

        /// <summary>
        /// Method used to get the URL for a file given its binary file GUID
        /// </summary>
        /// <param name="binaryFileGuid"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetFileUrl( Guid binaryFileGuid, GetFileUrlOptions options = null )
        {
            return GetFileUrlInternal( GetFileIdentifierParameter( binaryFileGuid ), options );
        }

        /// <summary>
        /// Internal method used to get the URL for a file given its file identifier parameter
        /// </summary>
        /// <param name="fileIdentifierParameter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string GetFileUrlInternal( string fileIdentifierParameter, GetFileUrlOptions options = null )
        {
            options = options ?? new GetFileUrlOptions();
            var urlBuilder = new System.Text.StringBuilder();

            if ( System.Web.Hosting.HostingEnvironment.VirtualPathProvider != null )
            {
                urlBuilder.Append( $"{System.Web.VirtualPathUtility.ToAbsolute( "~" )}GetFile.ashx?{fileIdentifierParameter}" );
            }
            else
            {
                // The hosting environment is not configured to resolve the file path.
                urlBuilder.Append( $"~/GetFile.ashx?{fileIdentifierParameter}" );
            }

            if ( options.FileName.IsNotNullOrWhiteSpace() )
            {
                urlBuilder.Append( $"&fileName={options.FileName}" );
            }

            return options.PublicAppRoot != null ? options.PublicAppRoot + urlBuilder.ToString() : urlBuilder.ToString();
        }

        #endregion

        #region GetImageUrl Methods

        /// <summary>
        /// Method used to get the URL for an image given its binary file ID
        /// </summary>
        /// <param name="binaryFileId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetImageUrl( int binaryFileId, GetImageUrlOptions options = null )
        {
            return GetImageUrlInternal( GetFileIdentifierParameter( binaryFileId ), options );
        }

        /// <summary>
        /// Method used to get the URL for an image given its binary file ID
        /// </summary>
        /// <param name="binaryFileId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetImageUrl( int? binaryFileId, GetImageUrlOptions options = null )
        {
            if ( !binaryFileId.HasValue )
            {
                return string.Empty;
            }
            return GetImageUrlInternal( GetFileIdentifierParameter( binaryFileId.Value ), options );
        }

        /// <summary>
        /// Method used to get the URL for an image given its binary file GUID
        /// </summary>
        /// <param name="binaryFileGuid"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetImageUrl( Guid binaryFileGuid, GetImageUrlOptions options = null )
        {
            return GetImageUrlInternal( GetFileIdentifierParameter( binaryFileGuid ), options );
        }

        /// <summary>
        /// Internal method used to get the URL for an image given its file identifier parameter
        /// </summary>
        /// <param name="fileIdentifierParameter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string GetImageUrlInternal( string fileIdentifierParameter, GetImageUrlOptions options = null )
        {
            options = options ?? new GetImageUrlOptions();
            var urlBuilder = new System.Text.StringBuilder();

            if ( System.Web.Hosting.HostingEnvironment.VirtualPathProvider != null )
            {
                string baseUrl = Rock.Configuration.RockApp.Current.HostingSettings.VirtualRootPath;
                if (fileIdentifierParameter.StartsWith("Assets/"))
                {
                    // If it's a relative path, just append it to the base URL
                    urlBuilder.Append( $"{baseUrl}{fileIdentifierParameter}" );
                }
                else
                {
                    // Otherwise, use the GetImage.ashx handler
                    urlBuilder.Append( $"{baseUrl}GetImage.ashx?{fileIdentifierParameter}" );
                }
            }
            else
            {
                // The hosting environment is not configured to resolve the file path.
                if ( fileIdentifierParameter.StartsWith( "Assets/" ) )
                {
                    urlBuilder.Append( $"/" + fileIdentifierParameter );
                }
                else
                {
                    urlBuilder.Append( $"/GetImage.ashx?{fileIdentifierParameter}" );
                }

            }
            
            if ( options.FileName.IsNotNullOrWhiteSpace() )
            {
                urlBuilder.Append( $"&fileName={options.FileName}" );
            }

            if ( options.Width.HasValue )
            {
                urlBuilder.Append( $"&width={options.Width.Value}" );
            }

            if ( options.Height.HasValue )
            {
                urlBuilder.Append( $"&height={options.Height.Value}" );
            }

            if ( options.MaxWidth.HasValue )
            {
                urlBuilder.Append( $"&maxwidth={options.MaxWidth.Value}" );
            }

            if ( options.MaxHeight.HasValue )
            {
                urlBuilder.Append( $"&maxheight={options.MaxHeight.Value}" );
            }

            return options.PublicAppRoot != null ? options.PublicAppRoot + urlBuilder.ToString() : urlBuilder.ToString();
        }

        #endregion

        #region GetAvatarUrl Methods

        /// <summary>
        /// Method used to get the URL for an avatar given its binary file ID
        /// </summary>
        /// <param name="binaryFileId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetAvatarUrl( int binaryFileId, GetAvatarUrlOptions options = null )
        {
            return GetAvatarUrlInternal( GetFileIdentifierParameter( binaryFileId ), options );
        }

        /// <summary>
        /// Method used to get the URL for an avatar given its binary file GUID
        /// </summary>
        /// <param name="binaryFileGuid"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetAvatarUrl( Guid binaryFileGuid, GetAvatarUrlOptions options = null )
        {
            return GetAvatarUrlInternal( GetFileIdentifierParameter( binaryFileGuid ), options );
        }

        /// <summary>
        /// Internal method used to get the URL for an avatar given its file identifier parameter
        /// </summary>
        /// <param name="fileIdentifierParameter"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string GetAvatarUrlInternal( string fileIdentifierParameter, GetAvatarUrlOptions options = null )
        {
            options = options ?? new GetAvatarUrlOptions();
            var urlBuilder = new System.Text.StringBuilder();

            urlBuilder.Append( $"{System.Web.VirtualPathUtility.ToAbsolute( "~" )}GetAvatar.ashx?{fileIdentifierParameter}" );

            if ( options.Width.HasValue )
            {
                urlBuilder.Append( $"&width={options.Width.Value}" );
            }

            if ( options.Height.HasValue )
            {
                urlBuilder.Append( $"&height={options.Height.Value}" );
            }

            // TODO: Add other avatar-specific parameters here

            return options.PublicAppRoot != null ? options.PublicAppRoot + urlBuilder.ToString() : urlBuilder.ToString();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method used to get the file identifier parameter for an integer ID
        /// </summary>
        /// <param name="fileIdentifier"></param>
        /// <returns></returns>
        private static string GetFileIdentifierParameter( int fileIdentifier )
        {
            var securitySettings = new SecuritySettingsService().SecuritySettings;
            var disablePredictableIds = securitySettings.DisablePredictableIds;

            if ( disablePredictableIds )
            {
                return $"fileIdKey={IdHasher.Instance.GetHash( fileIdentifier )}";
            }
            else
            {
                return $"id={fileIdentifier}";
            }
        }

        /// <summary>
        /// Helper method used to get the file identifier parameter for a GUID
        /// </summary>
        /// <param name="fileIdentifier"></param>
        /// <returns></returns>
        private static string GetFileIdentifierParameter( Guid fileIdentifier )
        {
            return $"guid={fileIdentifier}";
        }

        #endregion
    }

    #region Option Classes

    /// <summary>
    /// Class used to store options for getting a file URL
    /// </summary>
    public class GetFileUrlOptions
    {
        /// <summary>
        /// Gets or sets the public application root.
        /// </summary>
        public string PublicAppRoot { get; set; }
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
    }

    /// <summary>
    /// Class used to store options for getting an image URL
    /// </summary>
    public class GetImageUrlOptions : GetFileUrlOptions
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// Gets or sets the max width.
        /// </summary>
        public int? MaxWidth { get; set; }
        /// <summary>
        /// Gets or sets the max height.
        /// </summary>
        public int? MaxHeight { get; set; }
    }

    /// <summary>
    /// Class used to store options for getting an avatar URL
    /// </summary>
    public class GetAvatarUrlOptions : GetFileUrlOptions
    {
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the age classification.
        /// </summary>
        public AgeClassification? AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }
    }

    #endregion
}
