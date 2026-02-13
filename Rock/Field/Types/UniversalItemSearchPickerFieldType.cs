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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Reporting;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// General purpose picker field type that allows one or more items to
    /// be picked by the person.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
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
                    ["searchUrl"] = GetSearchUrl( privateConfigurationValues ),
                    ["context"] = GetStandardContext( privateConfigurationValues )
                };
            }

            return base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );
        }

        /// <summary>
        /// Gets the context to send to the UI. If we are using the standard
        /// URL then we need to provide some additional context data.
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>Either the custom context data or the subclass context data.</returns>
        private string GetStandardContext( Dictionary<string, string> privateConfigurationValues )
        {
            var fieldTypeGuid = GetType().GetCustomAttribute<FieldTypeGuidAttribute>()?.Guid;
            var methodInfo = GetType().GetMethod( nameof( GetSearchUrl ), BindingFlags.NonPublic | BindingFlags.Instance );

            // If they haven't overridden GetRootRestUrl() then we need to
            // provide custom context for the standard implementation.
            if ( fieldTypeGuid.HasValue && methodInfo != null && methodInfo.GetBaseDefinition() != methodInfo )
            {
                var data = new ContextData
                {
                    ConfigurationValues = privateConfigurationValues,
                    Context = GetContext( privateConfigurationValues ),
                    FieldTypeGuid = fieldTypeGuid.Value
                };

                return Encryption.EncryptString( data.ToJson() );
            }

            return GetContext( privateConfigurationValues );
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
            return "ti ti-folder-open";
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
        protected virtual string GetSearchUrl( Dictionary<string, string> privateConfigurationValues )
        {
            return "/api/v2/controls/UniversalItemSearchPickerGetItems";
        }

        /// <summary>
        /// Gets the tree items to display in the tree view. This will be
        /// called if you do not override <see cref="GetSearchUrl(Dictionary{string, string})"/>
        /// to get the items to display.
        /// </summary>
        /// <param name="options">The options that describe this request and provide additional data.</param>
        /// <returns>A list of <see cref="TreeItemBag"/> objects that describe the items to be rendered.</returns>
        internal List<UniversalItemSearchPickerItemBag> GetSearchItemsInternal( UniversalItemSearchPickerGetItemsOptions options )
        {
            return GetSearchItems( options );
        }

        /// <summary>
        /// Gets the tree items to display in the tree view. This will be
        /// called if you do not override <see cref="GetSearchUrl(Dictionary{string, string})"/>
        /// to get the items to display.
        /// </summary>
        /// <param name="options">The options that describe this request and provide additional data.</param>
        /// <returns>A list of <see cref="TreeItemBag"/> objects that describe the items to be rendered.</returns>
        protected virtual List<UniversalItemSearchPickerItemBag> GetSearchItems( UniversalItemSearchPickerGetItemsOptions options )
        {
            return new List<UniversalItemSearchPickerItemBag>();
        }

        /// <summary>
        /// Gets the context string to include with all API requests. This can
        /// be information you might need in handling the API request so that
        /// you can return the correct data. We also recommend you encode any
        /// context data as a JSON string to future proof yourself from changes
        /// you might need to make.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <returns>A custom string.</returns>
        protected virtual string GetContext( Dictionary<string, string> privateConfigurationValues )
        {
            return null;
        }

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
                SearchUrl = GetSearchUrl( privateConfigurationValues ),
                UrlContext = GetStandardContext( privateConfigurationValues )
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

        internal class ContextData
        {
            public Dictionary<string, string> ConfigurationValues { get; set; }

            public string Context { get; set; }

            public Guid FieldTypeGuid { get; set; }
        }
    }
}
