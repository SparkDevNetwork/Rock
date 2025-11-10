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
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// General purpose picker field type that allows one or more items to
    /// be picked by the person in a tree list.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [UniversalFieldTypeGuid( "c7485f3f-0c10-4db6-9574-c10b195617e4" )]
    public abstract class UniversalItemTreePickerFieldType : UniversalItemFieldType
    {
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
                var values = new Dictionary<string, string>
                {
                    ["iconCssClass"] = GetItemIconCssClass( privateConfigurationValues ),
                    ["isMultiple"] = IsMultipleSelection.ToString(),
                    ["rootRestUrl"] = GetRootRestUrl( privateConfigurationValues ),
                    ["context"] = GetStandardContext( privateConfigurationValues )
                };

                var itemTypes = GetSelectableItemTypes( privateConfigurationValues );

                if ( itemTypes != null )
                {
                    values["selectableItemTypes"] = itemTypes.ToCamelCaseJson( false, true );
                }

                return values;
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
            var methodInfo = GetType().GetMethod( nameof( GetRootRestUrl ), BindingFlags.NonPublic | BindingFlags.Instance );

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
        /// <para>
        /// Gets the URL to use when retrieving the root items of the tree view.
        /// </para>
        /// <para>
        /// The URL must accept a POST request. It will receive JSON data in the
        /// body of <see cref="Rock.ViewModels.Rest.Controls.UniversalItemTreePickerOptionsBag"/>
        /// and should return an array of <see cref="Rock.ViewModels.Utility.TreeItemBag"/>
        /// objects.
        /// </para>
        /// </summary>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string that represents the URL.</returns>
        protected virtual string GetRootRestUrl( Dictionary<string, string> privateConfigurationValues )
        {
            return "/api/v2/controls/UniversalItemTreePickerGetItems";
        }

        /// <summary>
        /// Gets the tree items to display in the tree view. This will be
        /// called if you do not override <see cref="GetRootRestUrl(Dictionary{string, string})"/>
        /// to get the items to display.
        /// </summary>
        /// <param name="options">The options that describe this request and provide additional data.</param>
        /// <returns>A list of <see cref="TreeItemBag"/> objects that describe the items to be rendered.</returns>
        internal List<TreeItemBag> GetTreeItemsInternal( UniversalItemTreePickerGetItemsOptions options )
        {
            return GetTreeItems( options );
        }

        /// <summary>
        /// Gets the tree items to display in the tree view. This will be
        /// called if you do not override <see cref="GetRootRestUrl(Dictionary{string, string})"/>
        /// to get the items to display.
        /// </summary>
        /// <param name="options">The options that describe this request and provide additional data.</param>
        /// <returns>A list of <see cref="TreeItemBag"/> objects that describe the items to be rendered.</returns>
        protected virtual List<TreeItemBag> GetTreeItems( UniversalItemTreePickerGetItemsOptions options )
        {
            return new List<TreeItemBag>();
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
        /// Specifies the tree item types that are selectable. If <c>null</c>
        /// is returned then all item types will be selectable. An empty set
        /// means no item types are selectable.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <returns>A list of item types.</returns>
        protected virtual List<string> GetSelectableItemTypes( Dictionary<string, string> privateConfigurationValues )
        {
            return null;
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
            if ( control is UniversalItemTreePicker picker )
            {
                var privateConfigurationValues = configurationValues.ToDictionary( v => v.Key, v => v.Value.Value );
                var values = GetItemBags( GetValueAsList( value ), privateConfigurationValues ) ?? new List<ListItemBag>();

                picker.ItemIds = values.Select( v => v.Value );
                picker.ItemNames = values.Select( v => v.Text );
            }
        }

        /// <inheritdoc/>
        public sealed override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is UniversalItemTreePicker picker )
            {
                return picker.ItemIds.Where( id => id != "0" ).JoinStrings( "," );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( k => k.Key, v => v.Value.Value );

            var picker = new UniversalItemTreePicker
            {
                ID = id,
                IconCssClass = GetItemIconCssClass( privateConfigurationValues ),
                AllowMultiSelect = IsMultipleSelection
            };

            picker.SetItemRestUrl( GetRootRestUrl( privateConfigurationValues ) );
            picker.UrlContext = GetStandardContext( privateConfigurationValues );

            return picker;
        }

        #endregion

        #region WebForms - Filter Controls

        /// <inheritdoc/>
        public sealed override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            id = $"{id ?? string.Empty}_ctlCompareValue";

            var control = ( UniversalItemTreePicker ) EditControl( configurationValues, $"{id ?? string.Empty}_ctlCompareValue" );

            control.AddCssClass( "js-filter-control" );

            // Invert multiple selection on the filter control. We don't
            // allow a many-to-many style filtering.
            control.AllowMultiSelect = !IsMultipleSelection;

            return control;
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
            var privateConfigurationValues = configurationValues.ToDictionary( v => v.Key, v => v.Value.Value );
            var textValues = GetItemBags( GetValueAsList( value ), privateConfigurationValues )
                ?.Select( b => b.Text )
                .ToList()
                ?? new List<string>();

            return AddQuotes( textValues.AsDelimited( "' OR '" ) );
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
