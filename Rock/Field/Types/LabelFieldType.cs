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
using System.Data.Entity;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of label files
    /// Stored as BinaryFile's Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.LABEL )]
    public class LabelFieldType : BinaryFileFieldType
    {
        #region Configuration

        private const string FILE_PATH = "filePath";
        private const string ENCODED_FILENAME = "encodedFileName";

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guidValue = privateValue.AsGuidOrNull();
            if ( guidValue.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileInfo = new BinaryFileService( rockContext ).GetSelect( guidValue.Value, f => new ListItemBag()
                    {
                        Text = f.FileName,
                        Value = f.Guid.ToString(),
                    } );

                    if ( binaryFileInfo == null )
                    {
                        return string.Empty;
                    }

                    return binaryFileInfo.ToCamelCaseJson( false, true );
                }
            }

            return base.GetPublicValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var configurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            var binaryFileGuid = value.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    var fileName = binaryFileService.GetSelect( binaryFileGuid.Value, s => s.FileName );

                    if ( !string.IsNullOrWhiteSpace( fileName ) )
                    {
                        configurationValues[FILE_PATH] = System.Web.VirtualPathUtility.ToAbsolute( "~/GetFile.ashx" );
                        configurationValues[ENCODED_FILENAME] = System.Web.HttpUtility.HtmlEncode( fileName );
                    }
                }
            }

            return configurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            configurationValues.Remove( FILE_PATH );
            configurationValues.Remove( ENCODED_FILENAME );

            return configurationValues;
        }

        #endregion Edit Control

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            return new List<Control>();
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var fileType = new ConfigurationValue( "File Type", "The type of files to list", Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL );
            return new Dictionary<string, ConfigurationValue>() { { "binaryFileType", fileType } };
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var picker = base.EditControl( configurationValues, id );
            if ( picker is BinaryFilePicker )
            {
                ( picker as BinaryFilePicker ).BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid();
            }

            return picker;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
        }

#endif
        #endregion

    }
}