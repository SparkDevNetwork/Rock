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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video file field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.VIDEO_FILE )]
    public class VideoFileFieldType : FileFieldType, IEntityReferenceFieldType
    {

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
    src='{0}}'
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
                    return string.Format( htmlFormat, filePath, binaryFileInfo.MimeType );
                }
            }

            return string.Empty;
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

#endif
        #endregion
    }
}