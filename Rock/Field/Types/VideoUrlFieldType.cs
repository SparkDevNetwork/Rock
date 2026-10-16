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
#endif
using Rock.Attribute;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video Url field type
    /// Stored as URL
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.VIDEO_URL )]
    public class VideoUrlFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var videoUrl = privateValue;

            /*
                10/16/2026 - MSE

                The stored value can come from forms that do not check its
                format. Only an http or https (or relative) URL is placed in
                the player's src, and it is encoded so a quote can't end the
                attribute. Anything else is shown as plain encoded text with
                no player.

                Reason: Stored values were rendered as raw HTML.
            */
            var safeUrl = SocialMediaAccountFieldType.GetSafeUrl( videoUrl );

            if ( safeUrl.IsNullOrWhiteSpace() )
            {
                return videoUrl.EncodeHtml();
            }

            var htmlFormat = @"
<video
    src='{0}'
    class='js-media-video'
    controls='controls'
    preload='auto'
    style='width:100%;height:100%;'
    width='100%'
    height='100%'
    preload='auto'    
    {1}
>
</video>
                    
<script>
    Rock.controls.mediaPlayer.initialize();
</script>
";
            var typeTag = string.Empty;
            if ( videoUrl.Contains( "youtube.com" ) )
            {
                typeTag = "type='video/youtube' width=640 height=480";
            }

            return string.Format( htmlFormat, safeUrl.EncodeHtml(), typeTag );
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var encodedUrl = privateValue.EncodeHtml();

            /*
                10/16/2026 - MSE

                Only an http or https (or relative) URL becomes a link, so a
                value with the javascript scheme can't be clicked in a grid.

                Reason: Any URL scheme was allowed in the href.
            */
            var safeUrl = SocialMediaAccountFieldType.GetSafeUrl( privateValue );

            if ( safeUrl == null )
            {
                return encodedUrl;
            }

            return $"<a href=\"{safeUrl.EncodeHtml()}\">{encodedUrl}</a>";
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
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
            // Original implementation simply returns the text value for condensed.
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

                FormatValue returns the raw URL when condensed because Lava,
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

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}