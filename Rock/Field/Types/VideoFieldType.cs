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
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Field.Types
{
    /// <summary>
    /// Video field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    public class VideoFieldType : FileFieldType
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
                    string controlId = string.Format( "player_{0}", Guid.NewGuid().ToString( "N" ) );
                    
                    var htmlFormat = @"
<video 
    src='{0}?guid={1}' 
    type='{2}' 
    height='240'
    width='320'
    controls=true
>
</video>
<script>
    $(document).ready(function() {{
        
        $('video').mediaelementplayer({{
            success: function(player, node) {{
                $('#' + node.id + '-mode').html('mode: ' + player.pluginType);
            }}
        }});
    }});
</script>
<span id='{3}-mode' display=''></span>
";

                    if ( binaryFile != null )
                    {
                        var html = string.Format( htmlFormat, filePath, binaryFile.Guid, binaryFile.MimeType, controlId );
                        return html;
                    }
                }
            }

            // value or binaryfile was null
            return null;
        }
    }
}