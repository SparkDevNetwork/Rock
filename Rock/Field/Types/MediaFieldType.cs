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
    /// Video field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    public abstract class MediaFieldType : FileFieldType
    {
        /// <summary>
        /// 
        /// </summary>
        public enum MediaTypeTag
        {
            /// <summary>
            /// The audio
            /// </summary>
            audio,
            
            /// <summary>
            /// The video
            /// </summary>
            video
        }

        /// <summary>
        /// Gets the media tag.
        /// </summary>
        /// <value>
        /// The media tag.
        /// </value>
        public abstract MediaTypeTag MediaTag { get; }
        
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
            var binaryFileGuid = value.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                var binaryFileService = new BinaryFileService( new RockContext() );
                var binaryFile = binaryFileService.Get( binaryFileGuid.Value );

                if ( condensed )
                {
                    if ( binaryFile != null )
                    {
                        return binaryFile.FileName;
                    }
                }
                else
                {
                    var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" );
                    var cssPath = System.Web.VirtualPathUtility.ToAbsolute( "~/Scripts/mediaelementjs/mediaelementplayer.min.css");

                    string controlId = string.Format( "player_{0}", Guid.NewGuid().ToString( "N" ) );

                    string htmlFormat = string.Empty;
                    if (this.MediaTag == MediaTypeTag.video)
                    {
                        htmlFormat += @"
<video
    src='{0}?guid={1}'
    class='img img-responsive' 
    type='{2}' 
    id='{3}'
    controls='true'
>
</video>";
                    }
                    else if (this.MediaTag == MediaTypeTag.audio)
                    {
                        htmlFormat += @"
<audio
    src='{0}?guid={1}' 
    class='img img-responsive'
    type='{2}' 
    id='{3}'
    controls='true'
>
</audio>";
                    }
                    
                    htmlFormat += @"
<script>
    $(document).ready(function() {{
        // ensure that css for mediaelementplayers is added to page
        if (!$('#mediaElementCss').length) {{
            $('head').append(""<link id='mediaElementCss' href='{4}' type='text/css' rel='stylesheet' />"");
        }}

        $('#{3}').mediaelementplayer();
    }});
</script>
";

                    if ( binaryFile != null )
                    {
                        var html = string.Format( htmlFormat, filePath, binaryFile.Guid, binaryFile.MimeType, controlId, cssPath );
                        return html;
                    }
                }
            }

            // value or binaryfile was null
            return null;
        }
    }
}