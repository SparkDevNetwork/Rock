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
using System.Web.UI;

using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    public class AssetFieldType : FieldType
    {
        /// <summary>
        /// The picker button template
        /// </summary>
        /// <returns></returns>
        private readonly string pickerButtonTemplate = @"
{% assign imageTypeUrl = '/Assets/Images/no-asset.svg' %}
{% assign selectedFileName = SelectedValue | FromJSON | Property:'Key' | Escape %}
{% if selectedFileName != '' %}
  {% assign imageType = selectedFileName | Split:'.' | Last | Trim %}
  {% capture imageTypeUrl %}/Assets/Icons/FileTypes/{{ imageType }}.png{% endcapture %}
{% endif %}

<div class='imageupload-thumbnail-image' style='height:100px; width:100px; background-image:url({{ imageTypeUrl }}); background-size:40%; background-position:50%; background-repeat: no-repeat;'>
    <span class='file-link'><p style='width: 95px; overflow: hidden' title='{{ selectedFileName }}'>{{ selectedFileName }}</p></span>
</div>
<div class='imageupload-dropzone'>
    <span>
        Select Asset
    </span>
</div>";

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
                SelectControlCssClass = "btn btn-xs btn-default imageupload-group",
                CssClass = "picker-asset",
                ModalSaveButtonText = "Select",
                ModalSaveButtonCssClass = "js-singleselect aspNetDisabled",
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
            Storage.AssetStorage.Asset asset = GetAssetInfoFromValue( value );

            if ( asset == null )
            {
                return string.Empty;
            }

            AssetStorageProvider assetStorageProvider = new AssetStorageProvider();
            int? assetStorageId = asset.AssetStorageProviderId;

            if ( assetStorageId != null )
            {
                var assetStorageService = new AssetStorageProviderService( new RockContext() );
                assetStorageProvider = assetStorageService.Get( assetStorageId.Value );
                assetStorageProvider.LoadAttributes();
            }

            var component = assetStorageProvider.GetAssetStorageComponent();

            string uri = component.CreateDownloadLink( assetStorageProvider, asset );

            return uri;
        }

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
    }
}
