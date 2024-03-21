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
using Rock.Enums.Controls;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// General purpose picker field type that allows one or more items to
    /// be picked by the person.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [UniversalFieldTypeGuid( "b69b5a61-6fcd-4e3b-bb45-5f6802514953" )]
    public abstract class UniversalItemPickerFieldType : UniversalItemFieldType
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
                return new Dictionary<string, string>
                {
                    ["columnCount"] = GetColumnCount( privateConfigurationValues ).ToStringSafe(),
                    ["displayStyle"] = GetDisplayStyle( privateConfigurationValues ).ConvertToInt().ToStringSafe(),
                    ["enhanceForLongLists"] = GetEnhanceForLongLists( privateConfigurationValues ).ToString(),
                    ["isMultiple"] = IsMultipleSelection.ToString(),
                    ["items"] = GetListItems( privateConfigurationValues ).ToCamelCaseJson( false, true )
                };
            }

            return base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );
        }

        #region Protected Methods

        /// <summary>
        /// Gets the number of columns to use when displaying the values in list
        /// display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>An integer that contains the number of columns to use or <c>null</c> to use the default.</returns>
        protected virtual int? GetColumnCount( Dictionary<string, string> privateConfigurationValues )
        {
            return null;
        }

        /// <summary>
        /// Gets the display style to use when rendering the edit control.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>The style to display the edit control in.</returns>
        protected virtual UniversalItemValuePickerDisplayStyle GetDisplayStyle( Dictionary<string, string> privateConfigurationValues )
        {
            return UniversalItemValuePickerDisplayStyle.Auto;
        }

        /// <summary>
        /// Gets a value that indicates if the edit control should be rendered
        /// with enhanced selection mode. This provides search capabilities to
        /// the list of items in condensed display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns><c>true</c> if the list of items should provide search capabilities; otherwise <c>false</c>.</returns>
        protected virtual bool GetEnhanceForLongLists( Dictionary<string, string> privateConfigurationValues )
        {
            return false;
        }

        /// <summary>
        /// Gets the list of items to be displayed in the picker.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe the field type.</param>
        /// <returns>A list of item bags that will be rendered in the picker.</returns>
        protected virtual List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ListItemBag>();
        }

        #endregion

#if WEBFORMS

        #region WebForms - Edit Controls

        /// <inheritdoc/>
        public sealed override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is RockDropDownList ddl )
            {
                ddl.SetValue( value );
            }
            else if ( control is RockListBox rlb )
            {
                rlb.SetValues( GetValueAsList( value ) );
            }
            else if ( control is RockCheckBoxList cbl )
            {
                cbl.SetValues( GetValueAsList( value ) );
            }
            else if ( control is RockRadioButtonList rbl )
            {
                rbl.SetValue( value );
            }
        }

        /// <inheritdoc/>
        public sealed override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is RockDropDownList ddl )
            {
                return ddl.SelectedValue;
            }
            else if ( control is RockListBox rlb )
            {
                return rlb.SelectedValues.JoinStrings( "," );
            }
            else if ( control is RockCheckBoxList cbl )
            {
                return cbl.SelectedValues.JoinStrings( "," );
            }
            else if ( control is RockRadioButtonList rbl )
            {
                return rbl.SelectedValue;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( k => k.Key, v => v.Value.Value );
            var displayMode = GetDisplayStyle( privateConfigurationValues );

            if ( displayMode == UniversalItemValuePickerDisplayStyle.List )
            {
                return !IsMultipleSelection
                    ? GetSingleSelectionListEditControl( privateConfigurationValues, id )
                    : GetMultipleSelectionListEditControl( privateConfigurationValues, id );
            }
            else
            {
                return !IsMultipleSelection
                    ? GetSingleSelectionCondensedEditControl( privateConfigurationValues, id )
                    : GetMultipleSelectionCondensedEditControl( privateConfigurationValues, id );
            }
        }

        /// <summary>
        /// Creates a Multiple Selection edit control in the List display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe this field type.</param>
        /// <param name="id">The control identifier.</param>
        /// <returns>A control that provides edit functionality.</returns>
        private WebControl GetMultipleSelectionListEditControl( Dictionary<string, string> privateConfigurationValues, string id )
        {
            var columnCount = GetColumnCount( privateConfigurationValues ) ?? 4;

            var cbl = new RockCheckBoxList
            {
                ID = id,
                RepeatDirection = RepeatDirection.Horizontal,
                RepeatColumns = columnCount
            };

            foreach ( var item in GetListItems( privateConfigurationValues ) )
            {
                cbl.Items.Add( new ListItem( item.Text, item.Value ) );
            }

            return cbl;
        }

        /// <summary>
        /// Creates a Single Selection edit control in the List display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe this field type.</param>
        /// <param name="id">The control identifier.</param>
        /// <returns>A control that provides edit functionality.</returns>
        private WebControl GetSingleSelectionListEditControl( Dictionary<string, string> privateConfigurationValues, string id )
        {
            var columnCount = GetColumnCount( privateConfigurationValues ) ?? 4;

            var rbl = new RockRadioButtonList
            {
                ID = id,
                RepeatDirection = RepeatDirection.Horizontal,
                RepeatColumns = columnCount
            };

            foreach ( var item in GetListItems( privateConfigurationValues ) )
            {
                rbl.Items.Add( new ListItem( item.Text, item.Value ) );
            }

            return rbl;
        }

        /// <summary>
        /// Creates a Multiple Selection edit control in the Condensed display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe this field type.</param>
        /// <param name="id">The control identifier.</param>
        /// <returns>A control that provides edit functionality.</returns>
        private WebControl GetMultipleSelectionCondensedEditControl( Dictionary<string, string> privateConfigurationValues, string id )
        {
            var rlb = new RockListBox
            {
                ID = id,
                DisplayDropAsAbsolute = true
            };

            foreach ( var item in GetListItems( privateConfigurationValues ) )
            {
                rlb.Items.Add( new ListItem( item.Text, item.Value ) );
            }

            return rlb;
        }

        /// <summary>
        /// Creates a Single Selection edit control in the Condensed display mode.
        /// </summary>
        /// <param name="privateConfigurationValues">The configuration values that describe this field type.</param>
        /// <param name="id">The control identifier.</param>
        /// <returns>A control that provides edit functionality.</returns>
        private WebControl GetSingleSelectionCondensedEditControl( Dictionary<string, string> privateConfigurationValues, string id )
        {
            var ddl = new RockDropDownList
            {
                ID = id,
                DisplayEnhancedAsAbsolute = true,
                EnhanceForLongLists = GetEnhanceForLongLists( privateConfigurationValues )
            };

            ddl.Items.Add( new ListItem() );
            foreach ( var item in GetListItems( privateConfigurationValues ) )
            {
                ddl.Items.Add( new ListItem( item.Text, item.Value ) );
            }

            return ddl;
        }

        #endregion

        #region WebForms - Filter Controls

        /// <inheritdoc/>
        public sealed override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( k => k.Key, v => v.Value.Value );
            id = $"{id ?? string.Empty}_ctlCompareValue";

            var control = !IsMultipleSelection
                ? GetMultipleSelectionListEditControl( privateConfigurationValues, id )
                : GetSingleSelectionCondensedEditControl( privateConfigurationValues, id );

            control.AddCssClass( "js-filter-control" );

            return control;
        }

        /// <inheritdoc/>
        public sealed override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is RockDropDownList ddl )
            {
                return ddl.SelectedValue;
            }
            else if ( control is RockCheckBoxList cbl )
            {
                return cbl.SelectedValues.JoinStrings( "," );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is RockDropDownList ddl )
            {
                ddl.SetValue( value );
            }
            else if ( control is RockCheckBoxList cbl )
            {
                cbl.SetValues( GetValueAsList( value ) );
            }
        }

        /// <inheritdoc/>
        public sealed override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var privateConfigurationValues = configurationValues.ToDictionary( k => k.Key, v => v.Value.Value );
            var items = GetListItems( privateConfigurationValues );
            var textValues = new List<string>();

            var values = GetValueAsList( value );

            foreach ( string key in values )
            {
                var item = items.FirstOrDefault( i => i.Value == key );

                if ( item != null )
                {
                    textValues.Add( item.Text );
                }
            }

            return AddQuotes( textValues.ToList().AsDelimited( "' OR '" ) );
        }

        #endregion

#endif
    }
}
