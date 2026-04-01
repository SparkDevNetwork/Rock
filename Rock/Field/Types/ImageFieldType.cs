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
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display an image value
    /// Stored as BinaryFile.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M5.16,4.27A1.32,1.32,0,1,0,6.48,5.59,1.32,1.32,0,0,0,5.16,4.27Zm8.09-2.41H2.73A1.76,1.76,0,0,0,1,3.62v8.76a1.75,1.75,0,0,0,1.73,1.76H13.25A1.75,1.75,0,0,0,15,12.38V3.62A1.74,1.74,0,0,0,13.25,1.86Zm.44,10.34L9.94,7.11a.45.45,0,0,0-.39-.21.51.51,0,0,0-.42.21L6.21,11.05l-1-1.26a.5.5,0,0,0-.4-.19.53.53,0,0,0-.41.19L2.32,12.36h0V3.62a.44.44,0,0,1,.44-.44H13.27a.44.44,0,0,1,.44.44V12.2Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.IMAGE )]
    public class ImageFieldType : BinaryFileFieldType
    {
        // NOTE: We are not implemented IReferenceEntityFieldType interface because
        // there is no UI support for the file name to change.

        #region Configuration

        private static class ConfigurationKey
        {
            // FYI: Add new configuration keys here. They should be camelCase for consistency.
            public const string FormatAsLink = FORMAT_AS_LINK;
            public const string ImageTagTemplate = IMG_TAG_TEMPLATE;
            public const string ImageUrl = IMAGE_URL;
            public const string BinaryFileType = "binaryFileType";
            public const string EnableCrop = "enableCrop";
            public const string TargetWidth = "targetWidth";
            public const string TargetHeight = "targetHeight";
            public const string MinimumWidth = "minimumWidth";
            public const string MinimumHeight = "minimumHeight";
        }

        #region New configuration keys should be added to ConfigurationKey above.

        /// <summary>
        /// if true, wrap the image with a an href to the full size image
        /// </summary>
        protected const string FORMAT_AS_LINK = "formatAsLink";

        /// <summary>
        /// The img tag template0
        /// </summary>
        protected const string IMG_TAG_TEMPLATE = "img_tag_template";

        /// <summary>
        /// The image URL generated from the image guid.
        /// </summary>
        protected const string IMAGE_URL = "imageUrl";

        #endregion

        #region Configuration Value Defaults

        /// <summary>
        /// The default image tag template
        /// </summary>
        protected const string DefaultImageTagTemplate = "<img src='{{ ImageUrl }}' class='img-responsive' />";

        #endregion

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicValues = new Dictionary<string, string>( base.GetPublicConfigurationValues( privateConfigurationValues, usage, value ) );

            var imageTagTemplate = publicValues.GetValueOrNull( ConfigurationKey.ImageTagTemplate );
            if ( imageTagTemplate.IsNullOrWhiteSpace() )
            {
                publicValues.AddOrReplace( ConfigurationKey.ImageTagTemplate, DefaultImageTagTemplate );
            }

            publicValues[ConfigurationKey.ImageUrl] = FileUrlHelper.GetImageUrl( value.AsGuid() );

            return publicValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateValues = new Dictionary<string, string>( base.GetPrivateConfigurationValues( publicConfigurationValues ) );

            privateValues.Remove( ConfigurationKey.ImageUrl );

            return privateValues;
        }

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

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var imageGuid = privateValue.AsGuidOrNull();

            if ( !imageGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var imageName = new BinaryFileService( rockContext ).GetSelect( imageGuid.Value, bf => bf.FileName );

                return new ListItemBag()
                {
                    Value = imageGuid.ToString(),
                    Text = imageName,
                }.ToCamelCaseJson( false, true );
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
            var imageTagTemplate = privateConfigurationValues.GetValueOrNull( ConfigurationKey.ImageTagTemplate ).ToStringOrDefault( DefaultImageTagTemplate );
            var formatAsLink = privateConfigurationValues.GetValueOrNull( ConfigurationKey.FormatAsLink ).AsBoolean();
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
            var oldImgTagTemplate = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.ImageTagTemplate ) ?? string.Empty;
            var oldFormatAsLink = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.FormatAsLink ) ?? string.Empty;
            var newWidth = newPrivateConfigurationValues.GetValueOrNull( "width" ) ?? string.Empty;
            var newHeight = newPrivateConfigurationValues.GetValueOrNull( "height" ) ?? string.Empty;
            var newImgTagTemplate = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.ImageTagTemplate ) ?? string.Empty;
            var newFormatAsLink = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.FormatAsLink ) ?? string.Empty;

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

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            var imageGuid = privateValue.AsGuidOrNull();

            if ( !imageGuid.HasValue )
            {
                return PersistedValues.Empty();
            }

            var textValue = GetTextValue( privateValue, privateConfigurationValues );

            return new PersistedValues
            {
                TextValue = textValue,
                HtmlValue = GetHtmlValue( privateValue, privateConfigurationValues ),
                CondensedTextValue = textValue.Truncate( CondensedTruncateLength ),
                CondensedHtmlValue = GetCondensedHtmlValue( privateValue, privateConfigurationValues ),
            };
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
            configKeys.Add( ConfigurationKey.FormatAsLink );
            configKeys.Add( ConfigurationKey.ImageTagTemplate );
            configKeys.Add( ConfigurationKey.EnableCrop );
            configKeys.Add( ConfigurationKey.TargetWidth );
            configKeys.Add( ConfigurationKey.TargetHeight );
            configKeys.Add( ConfigurationKey.MinimumWidth );
            configKeys.Add( ConfigurationKey.MinimumHeight );
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
            controls.Add( codeEditorImageTabTemplate );

            var cbCropEnabled = new RockCheckBox();
            cbCropEnabled.Label = "Enable Crop";
            cbCropEnabled.Help = "Enable this to allow cropping of the image when uploading.<br/>Only supported by blocks using Obsidian.";
            controls.Add( cbCropEnabled );

            var nbTargetWidth = new NumberBox();
            nbTargetWidth.Label = "Target Width";
            nbTargetWidth.Help = "The width that the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian.";
            controls.Add( nbTargetWidth );

            var nbTargetHeight = new NumberBox();
            nbTargetHeight.Label = "Target Height";
            nbTargetHeight.Help = "The height that the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian.";
            controls.Add( nbTargetHeight );

            var nbMinimumWidth = new NumberBox();
            nbMinimumWidth.Label = "Minimum Width";
            nbMinimumWidth.Help = "The minimum width required for the image to be uploaded.<br/>Only supported by blocks using Obsidian.";
            controls.Add( nbMinimumWidth );

            var nbMinimumHeight = new NumberBox();
            nbMinimumHeight.Label = "Minimum Height";
            nbMinimumHeight.Help = "The minimum height required for the image to be uploaded.<br/>Only supported by blocks using Obsidian.";
            controls.Add( nbMinimumHeight );

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
            configurationValues.Add( ConfigurationKey.FormatAsLink, new ConfigurationValue( "Format Image as Link", "Enable this to navigate to a full size image when the image is clicked", string.Empty ) );
            configurationValues.Add( ConfigurationKey.ImageTagTemplate, new ConfigurationValue( "Image Tag Template", "The Lava template to use when rendering as an html img tag", DefaultImageTagTemplate ) );
            configurationValues.Add( ConfigurationKey.EnableCrop, new ConfigurationValue( "Enable Crop", "Enable this to allow cropping of the image when uploading.<br/>Only supported by blocks using Obsidian.", string.Empty ) );
            configurationValues.Add( ConfigurationKey.TargetWidth, new ConfigurationValue( "Target Width", "The width that the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian.", string.Empty ) );
            configurationValues.Add( ConfigurationKey.TargetHeight, new ConfigurationValue( "Target Height", "The height that the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian.", string.Empty ) );
            configurationValues.Add( ConfigurationKey.MinimumWidth, new ConfigurationValue( "Minimum Width", "The minimum width required for the image to be uploaded.<br/>Only supported by blocks using Obsidian.", string.Empty ) );
            configurationValues.Add( ConfigurationKey.MinimumHeight, new ConfigurationValue( "Minimum Height", "The minimum height required for the image to be uploaded.<br/>Only supported by blocks using Obsidian.", string.Empty ) );

            if ( controls != null )
            {
                if ( controls.ElementAtOrDefault( 1 ) is RockCheckBox cbFormatAsLink )
                {
                    configurationValues[ConfigurationKey.FormatAsLink].Value = cbFormatAsLink.Checked.ToTrueFalse();
                }

                if ( controls.ElementAtOrDefault( 2 ) is CodeEditor codeEditorImageTabTemplate )
                {
                    configurationValues[ConfigurationKey.ImageTagTemplate].Value = codeEditorImageTabTemplate.Text;
                }

                if ( controls.ElementAtOrDefault( 3 ) is RockCheckBox cbEnableCrop )
                {
                    configurationValues[ConfigurationKey.EnableCrop].Value = cbEnableCrop.Checked.ToTrueFalse();
                }

                if ( controls.ElementAtOrDefault( 4 ) is NumberBox nbTargetWidth )
                {
                    configurationValues[ConfigurationKey.TargetWidth].Value = nbTargetWidth.Text;
                }

                if ( controls.ElementAtOrDefault( 5 ) is NumberBox nbTargetHeight )
                {
                    configurationValues[ConfigurationKey.TargetHeight].Value = nbTargetHeight.Text;
                }

                if ( controls.ElementAtOrDefault( 6 ) is NumberBox nbMinimumWidth )
                {
                    configurationValues[ConfigurationKey.MinimumWidth].Value = nbMinimumWidth.Text;
                }

                if ( controls.ElementAtOrDefault( 7 ) is NumberBox nbMinimumHeight )
                {
                    configurationValues[ConfigurationKey.MinimumHeight].Value = nbMinimumHeight.Text;
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

            if ( controls != null && configurationValues != null )
            {
                if ( controls.ElementAtOrDefault( 1 ) is RockCheckBox cbFormatAsLink
                     && configurationValues.TryGetValue( ConfigurationKey.FormatAsLink, out var formatAsLink ) )
                {
                    cbFormatAsLink.Checked = formatAsLink.Value.AsBooleanOrNull() ?? false;
                }

                if ( controls.ElementAtOrDefault( 2 ) is CodeEditor codeEditorImageTabTemplate
                     && configurationValues.TryGetValue( ConfigurationKey.ImageTagTemplate, out var imageTagTemplate ) )
                {
                    if ( imageTagTemplate.Value.IsNotNullOrWhiteSpace() )
                    {
                        codeEditorImageTabTemplate.Text = imageTagTemplate.Value;
                    }
                    else
                    {
                        codeEditorImageTabTemplate.Text = DefaultImageTagTemplate;
                    }
                }

                if ( controls.ElementAtOrDefault( 3 ) is RockCheckBox cbEnableCrop
                     && configurationValues.TryGetValue( ConfigurationKey.EnableCrop, out var enableCrop ) )
                {
                    cbEnableCrop.Checked = enableCrop.Value.AsBooleanOrNull() ?? false;
                }

                if ( controls.ElementAtOrDefault( 4 ) is NumberBox nbTargetWidth
                     && configurationValues.TryGetValue( ConfigurationKey.TargetWidth, out var targetWidth ) )
                {
                    nbTargetWidth.Text = targetWidth.Value;
                }

                if ( controls.ElementAtOrDefault( 5 ) is NumberBox nbTargetHeight
                     && configurationValues.TryGetValue( ConfigurationKey.TargetHeight, out var targetHeight ) )
                {
                    nbTargetHeight.Text = targetHeight.Value;
                }

                if ( controls.ElementAtOrDefault( 6 ) is NumberBox nbMinimumWidth
                     && configurationValues.TryGetValue( ConfigurationKey.MinimumWidth, out var minimumWidth ) )
                {
                    nbMinimumWidth.Text = minimumWidth.Value;
                }

                if ( controls.ElementAtOrDefault( 7 ) is NumberBox nbMinimumHeight
                     && configurationValues.TryGetValue( ConfigurationKey.MinimumHeight, out var minimumHeight ) )
                {
                    nbMinimumHeight.Text = minimumHeight.Value;
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
            if ( configurationValues != null )
            {
                if ( configurationValues.TryGetValue( ConfigurationKey.BinaryFileType, out var binaryFileType ) )
                {
                    control.BinaryFileTypeGuid = binaryFileType.Value.AsGuid();
                }

                // The WebForms control does not support cropping, target dimensions, and minimum dimensions.
                // Those are only supported by the Obsidian control.
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