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
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video field type
    /// </summary>
    [Serializable]
    public class VideoFieldType : FieldType
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( value.Trim() != string.Empty )
            {
                if (condensed)
                {
                    return base.FormatValue( parentControl, value, configurationValues, true );
                }

                string poster = string.Empty;
                string video = value;

                if ( value.Contains( "|" ) )
                {
                    string[] values = value.Split( '|' );
                    if ( values.Length >= 2 )
                    {
                        poster = string.Format( "poster='{0}'", values[0] );
                        video = values[1];
                    }
                }

                return string.Format( @"
    <video id='{0}_video' class='video-js vjs-default-skin' controls width='640' height='264' {1} preload='auto'>
        <source type='video/mp4' src='{2}'/>
    </video>
    <script>
        var myPlayer = _V_('{0}_video');
    </script>
", parentControl.ID, poster, video );
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Adds any required CSS or Script Links to the current page
        /// </summary>
        /// <param name="page">The page.</param>
        public static void AddLinks( Page page )
        {
            Rock.Web.UI.RockPage.AddCSSLink( page, "http://vjs.zencdn.net/c/video-js.css", false );
            Rock.Web.UI.RockPage.AddScriptLink( page, "http://vjs.zencdn.net/c/video.js", false );
        }
    }

}