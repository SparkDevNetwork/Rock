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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// General purpose picker field type that allows one or more items to
    /// be picked by the person.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [UniversalFieldTypeGuid( "c5b32713-fb46-41c0-8bbc-9bd4142f841a" )]
    public abstract class UniversalItemSearchPickerFieldType : UniversalItemFieldType
    {
        /// <inheritdoc/>
        protected sealed override bool IsMultipleSelection => false;

        /// <inheritdoc/>
        internal sealed override bool IsMultipleFilterSelection => false;

        /// <inheritdoc/>
        public sealed override bool HasFilterControl()
        {
            return true;
        }

        /// <inheritdoc/>
        public sealed override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            if ( usage == ConfigurationValueUsage.View )
            {
                return new Dictionary<string, string>();
            }
            else if ( usage == ConfigurationValueUsage.Edit )
            {
                return new Dictionary<string, string>
                {
                    ["areDetailsAlwaysVisible"] = AreDetailsAlwaysVisible( privateConfigurationValues ) ? "true" : "false",
                    ["iconCssClass"] = GetItemIconCssClass( privateConfigurationValues ),
                    ["isIncludeInactiveVisible"] = IsIncludeInactiveVisible( privateConfigurationValues ) ? "true" : "false",
                    ["searchUrl"] = GetSearchUrl( privateConfigurationValues )
                };
            }

            return base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );
        }

        #region Protected Methods

        /// <summary>
        /// Determines if the item details will always be visible in the search
        /// results or only after the individual clicks on a result.
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns><c>true</c> if the details are always visible; otherwise <c>false</c>.</returns>
        protected virtual bool AreDetailsAlwaysVisible( Dictionary<string, string> privateConfigurationValues )
        {
            return false;
        }

        /// <summary>
        /// Determines if the "include inactive" search filter will be visible
        /// in the search panel.
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns><c>true</c> if the include inactive option is visible; otherwise <c>false</c>.</returns>
        protected virtual bool IsIncludeInactiveVisible( Dictionary<string, string> privateConfigurationValues )
        {
            return false;
        }

        /// <summary>
        /// Gets the CSS icon class to use on the picker when it is clsoed.
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string that represents the icon class.</returns>
        protected virtual string GetItemIconCssClass( Dictionary<string, string> privateConfigurationValues )
        {
            return "fa fa-folder-open";
        }

        /// <summary>
        /// <para>
        /// Gets the URL that will be used when sending the POST request to search
        /// for results matching the value entered by the individual.
        /// </para>
        /// <para>
        /// The API will receive a POST request with a JSON body payload of
        /// <see cref="Rock.ViewModels.Rest.Controls.UniversalItemSearchPickerOptionsBag"/>
        /// and should return an array of
        /// <see cref="Rock.ViewModels.Controls.UniversalItemSearchPickerItemBag"/>
        /// objects.
        /// </para>
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string that represents the URL to use.</returns>
        protected abstract string GetSearchUrl( Dictionary<string, string> privateConfigurationValues );

        #endregion

#if WEBFORMS

        #region WebForms - Edit Controls

        /// <inheritdoc/>
        public sealed override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is UniversalItemSearchPicker picker )
            {
                var privateConfigurationValues = configurationValues.ToDictionary( v => v.Key, v => v.Value.Value );

                var bags = GetItemBags( GetValueAsList( value ), privateConfigurationValues );

                if ( bags.Count > 0 )
                {
                    picker.SetValue( bags[0] );
                }
            }
        }

        /// <inheritdoc/>
        public sealed override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is UniversalItemSearchPicker picker )
            {
                return picker.SelectedValue;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value );

            return new UniversalItemSearchPicker
            {
                ID = id,
                AreDetailsAlwaysVisible = AreDetailsAlwaysVisible( privateConfigurationValues ),
                IconCssClass = GetItemIconCssClass( privateConfigurationValues ),
                IsIncludeInactiveVisible = IsIncludeInactiveVisible( privateConfigurationValues ),
                SearchUrl = GetSearchUrl( privateConfigurationValues )
            };
        }

        #endregion

        #region WebForms - Filter Controls

        /// <inheritdoc/>
        public sealed override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var picker = ( UniversalItemSearchPicker ) EditControl( configurationValues, id );

            picker.Required = required;
            picker.AddCssClass( "js-filter-control" );

            return picker;
        }

        /// <inheritdoc/>
        public sealed override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValue( control, configurationValues );
        }

        /// <inheritdoc/>
        public sealed override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            SetEditValue( control, configurationValues, value );
        }

        /// <inheritdoc/>
        public sealed override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value );

            return GetTextValue( value, privateConfigurationValues );
        }

        #endregion

#endif
    }
}
