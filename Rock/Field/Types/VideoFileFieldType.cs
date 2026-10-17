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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video file field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.VIDEO_FILE )]
    public class VideoFileFieldType : FileFieldType, IEntityReferenceFieldType
    {
        private const string FILE_PATH = "filePath";
        private const string MIME_TYPE = "mimeType";
        private const string FILE_NAME = "fileName";
        private const string FILE_GUID = "fileGuid";

        /// <summary>
        /// Matches a plain video MIME type, such as video/mp4, with no
        /// parameters or other characters.
        /// </summary>
        private static readonly Regex VideoMimeTypePattern = new Regex( @"^video/[a-z0-9][a-z0-9.+-]{0,126}\z", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant );

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileName = binaryFileService.GetSelect( binaryFileGuid.Value, bf => bf.FileName );

                return binaryFileName ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return new BinaryFileService( new Data.RockContext() )
                .Get( privateValue.AsGuid() )
                .ToListItemBag()
                .ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Extract the raw value.
            return publicValue.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return publicConfigurationValues;
            }

            // Configuration to help display the HTML value on the remote device.
            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileInfo = binaryFileService.GetSelect( binaryFileGuid.Value, bf => new
                {
                    bf.FileName,
                    bf.MimeType
                } );

                publicConfigurationValues[FILE_NAME] = binaryFileInfo.FileName;
                publicConfigurationValues[MIME_TYPE] = binaryFileInfo.MimeType;
                publicConfigurationValues[FILE_PATH] = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" );
                publicConfigurationValues[FILE_GUID] = binaryFileGuid.ToString();
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            privateConfigurationValues.Remove( FILE_NAME );
            privateConfigurationValues.Remove( MIME_TYPE );
            privateConfigurationValues.Remove( FILE_PATH );
            privateConfigurationValues.Remove( FILE_GUID );

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileInfo = binaryFileService
                    .Queryable()
                    .Where( a => a.Guid == binaryFileGuid.Value )
                    .Select( s => new
                    {
                        s.FileName,
                        s.MimeType,
                        s.Guid
                    } )
                    .FirstOrDefault();

                if ( binaryFileInfo != null )
                {
                    var filePath = FileUrlHelper.GetFileUrl( binaryFileInfo.Guid );

                    // NOTE: Flash and Silverlight might crash if we don't set width and height. However, that makes responsive stuff not work
                    string htmlFormat = @"
<video 
    src='{0}'
    class='js-media-video'
    type='{1}'
    controls='controls'
    style='width:100%;height:100%;'
    width='100%'
    height='100%'
    preload='auto'
>
</video>
                    
<script>
    Rock.controls.mediaPlayer.initialize();
</script>
";
                    /*
                        10/16/2026 - MSE

                        The stored MimeType comes from the Content-Type the
                        client sent with the upload. Only a plain video type
                        is placed in the type attribute, and it is encoded,
                        so the value can't end the attribute.

                        Reason: The stored MimeType was rendered as raw HTML.
                    */
                    return string.Format( htmlFormat, filePath.EncodeHtml(), GetSafeVideoMimeType( binaryFileInfo.MimeType ).EncodeHtml() );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the MIME type if it is a plain video type, such as video/mp4.
        /// </summary>
        /// <param name="mimeType">The stored MIME type of the file.</param>
        /// <returns>The MIME type, or an empty string if it is not a plain video type.</returns>
        internal static string GetSafeVideoMimeType( string mimeType )
        {
            return mimeType != null && VideoMimeTypePattern.IsMatch( mimeType ) ? mimeType : string.Empty;
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileName = binaryFileService.GetSelect( binaryFileGuid.Value, bf => bf.FileName );

                if ( binaryFileName == null )
                {
                    return string.Empty;
                }

                return $"<a href=\"{FileUrlHelper.GetFileUrl( binaryFileGuid.Value )}\">{binaryFileName.EncodeHtml()}</a>";
            }
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileId = new BinaryFileService( rockContext ).GetId( guid.Value );

                if ( !binaryFileId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<BinaryFile>().Value, binaryFileId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the FileName property of a BinaryFile and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<BinaryFile>().Value, nameof( BinaryFile.FileName ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // This intentionally deviates from the pattern to maintain compatibility.
            // If we are not condensed then get the full HTML content. If we are
            // condensed this method used to return just the filename.
            return !condensed
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            var formattedValue = FormatValue( parentControl, value, configurationValues, condensed );

            /*
                10/16/2026 - MSE

                FormatValue returns the raw file name when condensed because Lava,
                workflows and other plain text consumers use it. Grids place
                this value directly in HTML, so it is encoded here instead.

                Reason: The condensed value was rendered as raw HTML.
            */
            return condensed
                ? formattedValue?.EncodeHtml()
                : formattedValue;
        }

        /// <inheritdoc/>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return FormatValueAsHtml( parentControl, value, configurationValues, condensed );
        }

#endif
        #endregion
    }
}