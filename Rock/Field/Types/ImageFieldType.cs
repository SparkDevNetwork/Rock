﻿// <copyright>
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
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an image value
    /// Stored as BinaryFile.Guid
    /// </summary>
    public class ImageFieldType : BinaryFileFieldType
    {
        #region Configuration

        /// <summary>
        /// if true, wrap the image with a an href to the full size image
        /// </summary>
        protected const string FORMAT_AS_LINK = "formatAsLink";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( FORMAT_AS_LINK );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            var cbFormatAsLink = new RockCheckBox();
            cbFormatAsLink.Label = "Format as Link";
            cbFormatAsLink.Help = "Enable this to navigate to a full size image when the image is clicked";
            cbFormatAsLink.Text = "Yes";
            controls.Add( cbFormatAsLink );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );
            configurationValues.Add( FORMAT_AS_LINK, new ConfigurationValue( "Format Image as Link", "Enable this to navigate to a full size image when the image is clicked", string.Empty ) );

            if ( controls != null && controls.Count > 1 )
            {
                var cbFormatAsLink = controls[1] as RockCheckBox;
                if ( cbFormatAsLink != null )
                {
                    configurationValues[FORMAT_AS_LINK].Value = cbFormatAsLink.Checked.ToTrueFalse();
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count > 1 && configurationValues != null )
            {
                var cbFormatAsLink = controls[1] as RockCheckBox;
                if ( cbFormatAsLink != null && configurationValues.ContainsKey( FORMAT_AS_LINK ) )
                {
                    cbFormatAsLink.Checked = configurationValues[FORMAT_AS_LINK].Value.AsBooleanOrNull() ?? false;
                }
            }
        }

        #endregion

        #region Formatting

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
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var imagePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetImage.ashx" );

                // create querystring parms
                string queryParms = string.Empty;
                if ( condensed )
                {
                    queryParms = "&width=120"; // for grids hardcode to 100px wide
                }
                else
                {
                    // determine image size parameters
                    // width
                    if ( configurationValues != null &&
                        configurationValues.ContainsKey( "width" ) &&
                        !string.IsNullOrWhiteSpace( configurationValues["width"].Value ) )
                    {
                        queryParms = "&width=" + configurationValues["width"].Value;
                    }

                    // height
                    if ( configurationValues != null &&
                        configurationValues.ContainsKey( "height" ) &&
                        !string.IsNullOrWhiteSpace( configurationValues["height"].Value ) )
                    {
                        queryParms += "&height=" + configurationValues["height"].Value;
                    }
                }

                Guid? imageGuid = value.AsGuidOrNull();
                if ( imageGuid.HasValue )
                {
                    string imageUrl = string.Format( "{0}?guid={1}", imagePath, imageGuid );
                    var imageTag = string.Format( "<img src='{0}{1}' class='img-responsive' />", imageUrl, queryParms );

                    if ( configurationValues != null &&
                        configurationValues.ContainsKey( FORMAT_AS_LINK ) &&
                        ( configurationValues[FORMAT_AS_LINK].Value.AsBooleanOrNull() ?? false ) )
                    {
                        return string.Format( "<a href='{0}'>{1}</a>", imageUrl, imageTag );
                    }
                    else
                    {
                        return imageTag;
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

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
            var control = new Web.UI.Controls.ImageUploader { ID = id };
            if ( configurationValues != null && configurationValues.ContainsKey( "binaryFileType" ) )
            {
                control.BinaryFileTypeGuid = configurationValues["binaryFileType"].Value.AsGuid();
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as ImageUploader;

            if ( picker != null )
            {
                int? id = picker.BinaryFileId;
                if ( id.HasValue )
                {
                    var binaryFile = new BinaryFileService( new RockContext() ).Get( id.Value );

                    if ( binaryFile != null )
                    {
                        return binaryFile.Guid.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ImageUploader;

            if ( picker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var item = new BinaryFileService( new RockContext() ).Get( guid );
                if ( item != null )
                {
                    picker.BinaryFileId = item.Id;
                }
            }
        }

        #endregion
    }
}