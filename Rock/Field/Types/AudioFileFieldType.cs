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
using System.Linq;
using System.Web.UI;

using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Audio file field type
    /// Stored as BinaryFile.Guid
    /// </summary>
    public class AudioFileFieldType : FileFieldType
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
            var binaryFileGuid = value.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                var binaryFileService = new BinaryFileService( new RockContext() );
                var binaryFileInfo = binaryFileService.Queryable().Where( a => a.Guid == binaryFileGuid.Value )
                    .Select( s => new
                    {
                        s.FileName,
                        s.MimeType,
                        s.Guid
                    } )
                    .FirstOrDefault();

                if ( binaryFileInfo != null )
                {
                    if ( condensed )
                    {
                        return binaryFileInfo.FileName;
                    }
                    else
                    {
                        var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" );
                        string htmlFormat = @"
<audio
    src='{0}?guid={1}' 
    class='img img-responsive'
    type='{2}' 
    controls
>
</audio>

<script>
    Rock.controls.mediaPlayer.initialize();
</script>
";

                        var html = string.Format( htmlFormat, filePath, binaryFileInfo.Guid, binaryFileInfo.MimeType );
                        return html;
                    }
                }
            }

            // value or binaryfile was null
            return null;
        }

        #endregion

    }
}