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
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an image value
    /// Stored as BinaryFile.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M5.16,4.27A1.32,1.32,0,1,0,6.48,5.59,1.32,1.32,0,0,0,5.16,4.27Zm8.09-2.41H2.73A1.76,1.76,0,0,0,1,3.62v8.76a1.75,1.75,0,0,0,1.73,1.76H13.25A1.75,1.75,0,0,0,15,12.38V3.62A1.74,1.74,0,0,0,13.25,1.86Zm.44,10.34L9.94,7.11a.45.45,0,0,0-.39-.21.51.51,0,0,0-.42.21L6.21,11.05l-1-1.26a.5.5,0,0,0-.4-.19.53.53,0,0,0-.41.19L2.32,12.36h0V3.62a.44.44,0,0,1,.44-.44H13.27a.44.44,0,0,1,.44.44V12.2Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.IMAGE )]
    public class ImageFieldType : BinaryFileFieldType
    {
        // NOTE: We are not implemented IReferenceEntityFieldType interface because
        // there is no UI support for the file name to change.

        #region Configuration

        /// <summary>
        /// if true, wrap the image with a an href to the full size image
        /// </summary>
        protected const string FORMAT_AS_LINK = "formatAsLink";

        /// <summary>
        /// The img tag template
        /// </summary>
        protected const string IMG_TAG_TEMPLATE = "img_tag_template";

        /// <summary>
        /// The default image tag template
        /// </summary>
        protected const string DefaultImageTagTemplate = "<img src='{{ ImageUrl }}' class='img-responsive' />";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var imageGuid = privateValue.AsGuidOrNull();

            if ( !imageGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var imageName = new BinaryFileService( rockContext ).GetSelect( imageGuid.Value, bf => bf.FileName );

                return imageName ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var imageGuid = privateValue.AsGuidOrNull();

            if ( !imageGuid.HasValue )
            {
                return string.Empty;
            }

            return GetImageHtml( imageGuid.Value, null, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var imageGuid = privateValue.AsGuidOrNull();

            if ( !imageGuid.HasValue )
            {
                return string.Empty;
            }

            return GetImageHtml( imageGuid.Value, 120, privateConfigurationValues );
        }

        /// <summary>
        /// Gets the image HTML that should be used to display the image one
        /// a web page.
        /// </summary>
        /// <param name="imageGuid">The image unique identifier.</param>
        /// <param name="width">The width to force the image to.</param>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <returns>A string that contains the HTML used to render the image.</returns>
        private static string GetImageHtml( Guid imageGuid, int? width, Dictionary<string, string> privateConfigurationValues )
        {
            var queryParms = string.Empty;

            // Determine image size parameters if they aren't already forced.
            if ( width.HasValue )
            {
                queryParms = $"&width={width}";
            }
            else
            {
                width = privateConfigurationValues.GetValueOrNull( "width" )?.AsIntegerOrNull();
                if ( width.HasValue )
                {
                    queryParms = $"&width={width}";
                }

                var height = privateConfigurationValues.GetValueOrNull( "height" )?.AsIntegerOrNull();
                if ( height.HasValue )
                {
                    queryParms += $"&height={height}";
                }
            }

            var imageUrl = FileUrlHelper.GetImageUrl( imageGuid );
            var imageTagTemplate = privateConfigurationValues.GetValueOrNull( IMG_TAG_TEMPLATE ).ToStringOrDefault( DefaultImageTagTemplate );
            var formatAsLink = privateConfigurationValues.GetValueOrNull( FORMAT_AS_LINK ).AsBoolean();
            var mergeFields = new Dictionary<string, object>()
            {
                ["ImageUrl"] = imageUrl + queryParms,
                ["ImageGuid"] = imageGuid
            };

            var imageTag = imageTagTemplate.ResolveMergeFields( mergeFields );

            return formatAsLink
                ? $"<a href='{imageUrl}'>{imageTag}</a>"
                : imageTag;
        }


        #endregion

        #region Edit Control

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldWidth = oldPrivateConfigurationValues.GetValueOrNull( "width" ) ?? string.Empty;
            var oldHeight = oldPrivateConfigurationValues.GetValueOrNull( "height" ) ?? string.Empty;
            var oldImgTagTemplate = oldPrivateConfigurationValues.GetValueOrNull( IMG_TAG_TEMPLATE ) ?? string.Empty;
            var oldFormatAsLink = oldPrivateConfigurationValues.GetValueOrNull( FORMAT_AS_LINK ) ?? string.Empty;
            var newWidth = newPrivateConfigurationValues.GetValueOrNull( "width" ) ?? string.Empty;
            var newHeight = newPrivateConfigurationValues.GetValueOrNull( "height" ) ?? string.Empty;
            var newImgTagTemplate = newPrivateConfigurationValues.GetValueOrNull( IMG_TAG_TEMPLATE ) ?? string.Empty;
            var newFormatAsLink = newPrivateConfigurationValues.GetValueOrNull( FORMAT_AS_LINK ) ?? string.Empty;

            if ( oldWidth != newWidth )
            {
                return true;
            }

            if ( oldHeight != newHeight )
            {
                return true;
            }

            if ( oldImgTagTemplate != newImgTagTemplate )
            {
                return true;
            }

            if ( oldFormatAsLink != newFormatAsLink )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( FORMAT_AS_LINK );
            configKeys.Add( IMG_TAG_TEMPLATE );
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
            controls.Add( cbFormatAsLink );

            var codeEditorImageTabTemplate = new CodeEditor();
            codeEditorImageTabTemplate.Label = "Image Tag Template";
            codeEditorImageTabTemplate.Help = "The Lava template to use when rendering as an html img tag.";
            codeEditorImageTabTemplate.EditorHeight = "100";
            codeEditorImageTabTemplate.EditorMode = CodeEditorMode.Lava;
            codeEditorImageTabTemplate.EditorTheme = CodeEditorTheme.Rock;
            controls.Add( codeEditorImageTabTemplate );

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
            configurationValues.Add( IMG_TAG_TEMPLATE, new ConfigurationValue( "Image Tag Template", "The Lava template to use when rendering as an html img tag", DefaultImageTagTemplate ) );

            if ( controls != null && controls.Count > 2 )
            {
                var cbFormatAsLink = controls[1] as RockCheckBox;
                if ( cbFormatAsLink != null )
                {
                    configurationValues[FORMAT_AS_LINK].Value = cbFormatAsLink.Checked.ToTrueFalse();
                }

                var codeEditorImageTabTemplate = controls[2] as CodeEditor;
                if ( codeEditorImageTabTemplate != null )
                {
                    configurationValues[IMG_TAG_TEMPLATE].Value = codeEditorImageTabTemplate.Text;
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

            if ( controls != null && controls.Count > 2 && configurationValues != null )
            {
                var cbFormatAsLink = controls[1] as RockCheckBox;
                if ( cbFormatAsLink != null && configurationValues.ContainsKey( FORMAT_AS_LINK ) )
                {
                    cbFormatAsLink.Checked = configurationValues[FORMAT_AS_LINK].Value.AsBooleanOrNull() ?? false;
                }

                var codeEditorImageTabTemplate = controls[2] as CodeEditor;
                if ( codeEditorImageTabTemplate != null && configurationValues.ContainsKey( IMG_TAG_TEMPLATE ) )
                {
                    codeEditorImageTabTemplate.Text = configurationValues[IMG_TAG_TEMPLATE].Value;
                    if ( codeEditorImageTabTemplate.Text.IsNullOrWhiteSpace() )
                    {
                        codeEditorImageTabTemplate.Text = DefaultImageTagTemplate;
                    }
                }
            }
        }

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
            return !condensed
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
                    using ( var rockContext = new RockContext() )
                    {
                        var binaryFileGuid = new BinaryFileService( rockContext ).GetGuid( id.Value );

                        return binaryFileGuid?.ToString() ?? string.Empty;
                    }
                }
            }

            return null;
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
                Guid? guid = value.AsGuidOrNull();
                int? binaryFileId = null;

                // if there is a Value as Guid, get the Id of the BinaryFile
                if ( guid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        binaryFileId = new BinaryFileService( rockContext ).GetId( guid.Value );
                    }
                }

                // set the picker's selected BinaryFileId (or set it to null if setting the value to null or emptystring)
                picker.BinaryFileId = binaryFileId;
            }
        }

#endif
        #endregion
    }
}