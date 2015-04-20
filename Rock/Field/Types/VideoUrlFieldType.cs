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
using System.Collections.Generic;
using System.Web.UI;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video Url field type
    /// Stored as URL
    /// </summary>
    public class VideoUrlFieldType : FieldType
    {

        #region Formatting

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
            string videoUrl = value;
            if ( !string.IsNullOrEmpty( videoUrl ) )
            {
                if ( condensed )
                {
                    return videoUrl;
                }
                else
                {
                    string htmlFormat = @"
<video
    src='{0}'
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
                    string typeTag = string.Empty;
                    if (videoUrl.Contains("youtube.com"))
                    {
                        typeTag = "type='video/youtube' width=640 height=480";
                    }
                    
                    var html = string.Format( htmlFormat, videoUrl, typeTag );
                    return html;
                }

            }

            // value was null
            return null;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            // This fieldtype does not support filtering
            return null;
        }

        #endregion
      
    }
}