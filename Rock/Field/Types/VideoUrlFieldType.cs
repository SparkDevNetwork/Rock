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
using System.Web.UI;
using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video Url field type
    /// Stored as URL
    /// </summary>
    public class VideoUrlFieldType : FieldType
    {
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
    class='img img-responsive' 
    controls
    preload='none'
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
    }
}