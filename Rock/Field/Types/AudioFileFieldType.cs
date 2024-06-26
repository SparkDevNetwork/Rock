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
using System.Data.Entity;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Audio file field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.AUDIO_FILE )]
    public class AudioFileFieldType : FileFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetFileName( privateValue );
        }

        /// <inheritdoc />
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var binaryFileInfo = binaryFileService.Queryable().AsNoTracking().Where( a => a.Guid == binaryFileGuid.Value )
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
                        string htmlFormat = @"
<audio
    src='{0}' 
    class='img img-responsive js-media-audio'
    type='{1}' 
    controls
>
</audio>

<script>
    Rock.controls.mediaPlayer.initialize();
</script>
";

                        var html = string.Format( htmlFormat, filePath, binaryFileInfo.MimeType );
                        return html;
                    }
                }
            }

            // value or binaryfile was null
            return string.Empty;
        }

        /// <inheritdoc />
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return string.Empty;
            }

            return $"<a href=\"{FileUrlHelper.GetFileUrl( binaryFileGuid.Value )}\">{GetFileName( privateValue ).EncodeHtml()}</a>";
        }

        /// <summary>
        /// Get the filename of the binary file.
        /// </summary>
        /// <param name="privateValue"></param>
        /// <returns></returns>
        private static string GetFileName( string privateValue )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var filename = binaryFileService
                        .Queryable().AsNoTracking()
                        .Where( a => a.Guid == binaryFileGuid.Value )
                        .Select( s => s.FileName )
                        .FirstOrDefault();

                    if ( filename.IsNotNullOrWhiteSpace() )
                    {
                        return filename;
                    }
                }
            }

            // value or binaryfile was null
            return string.Empty;
        }

        #endregion

        #region IEntityReferenceFieldType

        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var binaryFileGuid = privateValue.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileId = new BinaryFileService( rockContext ).GetId( binaryFileGuid.Value );

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

        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
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
            // For compatibility reasons, the condensed version is just the filename.
            return !condensed
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )?.EncodeHtml();
        }

#endif
        #endregion

    }
}