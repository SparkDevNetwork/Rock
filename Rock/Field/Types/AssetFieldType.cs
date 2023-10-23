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

using System.Collections.Generic;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "4E4E8692-23B4-49EA-88B4-2AB07899E0EE" )]
    public class AssetFieldType : FieldType
    {
        /// <summary>
        /// The picker button template
        /// </summary>
        /// <returns></returns>
        private readonly string pickerButtonTemplate = @"
{% assign iconPath = SelectedValue | FromJSON | Property:'IconPath' %}
{% assign fileName = SelectedValue | FromJSON | Property:'Name' %}
{% if iconPath != '' and fileName != '' %}
    {% assign escFileName = fileName | UrlEncode %}
    {% assign imageTypeUrl = iconPath | Replace: fileName, escFileName %}
{% endif %}

<div class='fileupload-thumbnail{% if imageTypeUrl contains '/Assets/Icons/FileTypes/' %} fileupload-thumbnail-icon{% endif %}' {% if fileName != '' %}style='background-image:url({{ imageTypeUrl }}) !important;' title='{{ fileName }}'{% endif %}>
    {% if fileName != '' %}<span class='file-link' style='background-color: transparent'>{{ fileName }}</span>{% else %}<span class='file-link file-link-default'></span>{% endif %}
</div>
<div class='imageupload-dropzone'>
    <span>
        Select Asset
    </span>
</div>";

        #region Edit Control

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Storage.AssetStorage.Asset asset = GetAssetInfoFromValue( privateValue );

            if ( asset == null )
            {
                return string.Empty;
            }

            // Find in cache first before performing expensive operations.
            // (Every asset should have a provider id and a key.)
            var cacheKey = $"Rock.Field.Types.AssetFieldType:{asset.AssetStorageProviderId}:{asset.Key}";
            var uri = RockCache.Get( cacheKey, true ) as string;
            if ( uri != null )
            {
                return uri;
            }

            if ( asset.AssetStorageProviderId <= 0)
            {
                return string.Empty;
            }

            var assetStorageProviderCache = AssetStorageProviderCache.Get( asset.AssetStorageProviderId );

            var component = assetStorageProviderCache?.AssetStorageComponent;
            if ( component == null )
            {
                return string.Empty;
            }

            uri = component.CreateDownloadLink( assetStorageProviderCache.ToEntity(), asset );

            // Cache for 60 seconds
            RockCache.AddOrUpdate( cacheKey, null, uri, RockDateTime.Now.AddSeconds( 60 ) );

            return uri;
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // Don't truncate the value.
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var url = GetTextValue( privateValue, privateConfigurationValues );

            if ( url.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var encodedUrl = url.EncodeHtml();

            return $"<a href=\"{encodedUrl}\">{encodedUrl}</a>";
        }

        #endregion

        /// <summary>
        /// Gets the asset information from value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static Storage.AssetStorage.Asset GetAssetInfoFromValue( string value )
        {
            Storage.AssetStorage.Asset asset = null;

            if ( !value.IsNullOrWhiteSpace() )
            {
                asset = JsonConvert.DeserializeObject<Storage.AssetStorage.Asset>( value );
                asset.Type = Storage.AssetStorage.AssetType.File;
            }

            return asset;
        }

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueVolatile( Dictionary<string, string> privateConfigurationValues )
        {
            // The download links expire so we need to keep them up to date.
            return true;
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var pickerControl = new ItemFromBlockPicker
            {
                ID = id,
                BlockTypePath = "~/Blocks/CMS/AssetManager.ascx",
                ShowInModal = true,
                SelectControlCssClass = "imageupload-group",
                CssClass = "picker-asset",
                ModalSaveButtonText = "Select",
                ModalSaveButtonCssClass = "js-singleselect aspNetDisabled",
                ModalCssClass = "js-AssetManager-modal",
                ButtonTextTemplate = "Select Asset",
                PickerButtonTemplate = pickerButtonTemplate,
                ModalTitle = "Asset Manager"
            };

            return pickerControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = ( ItemFromBlockPicker ) control;
            if ( picker != null )
            {
                return picker.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = ( ItemFromBlockPicker ) control;
            if ( picker != null )
            {
                picker.SelectedValue = value;
            }
        }

        /// <summary>
        /// Overridden to take JSON input of AssetStorageID and Key and create a URL. If the asset is using Amazon then a presigned URL is
        /// created.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // Original implementation always returns non-condensed and did not HTML format.
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

#endif
        #endregion
    }
}
