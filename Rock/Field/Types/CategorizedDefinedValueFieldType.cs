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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using TreeNode = Rock.Web.UI.Controls.TreeNode;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a selection from a Defined Type that supports categorized values.
    /// </summary>
    [Serializable]
    public class CategorizedDefinedValueFieldType : FieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "DefinedType";
        private const string SELECTABLE_VALUES_KEY = "SelectableDefinedValues";

        /// <summary>
        /// The settings for this Field Type.
        /// </summary>
        internal class Settings : FieldTypeConfigurationSettings
        {
            public Settings()
            {
                Add( DEFINED_TYPE_KEY, "Defined Type", "The Defined Type to select values from", string.Empty );
                Add( SELECTABLE_VALUES_KEY, "Selectable Values", "Specify the values eligible for this control. If none are specified then all will be displayed.", string.Empty );
            }

            public Settings( Dictionary<string, ConfigurationValue> configurationValues )
                : this()
            {
                SetConfigurationValues( configurationValues );
            }

            public int? DefinedTypeId
            {
                get
                {
                    return TryGetValue( DEFINED_TYPE_KEY ).AsIntegerOrNull();
                }
                set
                {
                    TrySetValue( DEFINED_TYPE_KEY, value.ToStringSafe() );
                }
            }

            public List<string> SelectableValueIdList
            {
                get
                {
                    return TryGetValue( SELECTABLE_VALUES_KEY ).SplitDelimitedValues( ",", StringSplitOptions.RemoveEmptyEntries ).ToList();
                }
                set
                {
                    TrySetValue( SELECTABLE_VALUES_KEY, value.AsDelimited( "," ) );
                }
            }
        }

        /// <inheritdoc/>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( SELECTABLE_VALUES_KEY );
            return configKeys;
        }

        /// <inheritdoc/>
        public override bool HasDefaultControl => false;

        /// <inheritdoc/>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Get the Defined Types that are configured for categorized values.
            var definedTypes = DefinedTypeCache.All()
                .Where( x => ( x.CategorizedValuesEnabled ?? false ) )
                .OrderBy( d => d.Order )
                .Select( v => new ListItem { Text = v.Name, Value = v.Id.ToString() } )
                .ToList();
            definedTypes.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            var ddlDefinedType = new RockDropDownList()
            {
                AutoPostBack = true,
                Label = "Defined Type",
                Help = "A Defined Type that is configured to support categorized values.",
                EnhanceForLongLists = true
            };
            ddlDefinedType.SelectedIndexChanged += OnQualifierUpdated;

            ddlDefinedType.Items.AddRange( definedTypes.ToArray() );

            var definedValues = GetDefinedValueListItems( ddlDefinedType.SelectedValue.AsInteger() );

            var cblSelectableValues = new RockCheckBoxList
            {
                RepeatDirection = RepeatDirection.Horizontal,
                Label = "Selectable Values",
            };
            cblSelectableValues.Items.AddRange( definedValues.ToArray() );

            controls.Add( ddlDefinedType );
            controls.Add( cblSelectableValues );

            return controls;
        }

        /// <inheritdoc/>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var settings = new Settings();
            GetSettingsFromConfigurationControls( settings, controls );

            return settings.GetConfigurationValues();
        }

        /// <inheritdoc/>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls == null )
            {
                return;
            }

            var settings = new Settings( configurationValues );
            ApplySettingsToConfigurationControls( settings, controls );
        }

        /// <summary>
        /// Update the configuration settings from the values stored in the edit controls.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="controls"></param>
        private void GetSettingsFromConfigurationControls( Settings settings, List<Control> controls )
        {
            if ( controls == null )
            {
                return;
            }

            // Defined Type.
            if ( controls.Count > 0 && controls[0] is RockDropDownList rdd )
            {
                settings.DefinedTypeId = rdd.SelectedValue.AsIntegerOrNull();
            }

            // Selectable Values.
            if ( controls.Count > 1 && controls[1] is RockCheckBoxList cblSelectableValues )
            {
                var selectableValues = new List<string>( cblSelectableValues.SelectedValues );

                var definedValues = GetDefinedValueListItems( settings.DefinedTypeId );
                cblSelectableValues.Items.Clear();
                cblSelectableValues.Items.AddRange( definedValues.ToArray() );

                if ( selectableValues != null && selectableValues.Any() )
                {
                    foreach ( ListItem listItem in cblSelectableValues.Items )
                    {
                        listItem.Selected = selectableValues.Contains( listItem.Value );
                    }
                }

                settings.SelectableValueIdList = cblSelectableValues.SelectedValues;

                cblSelectableValues.Visible = ( definedValues.Count > 0 );
            }

        }

        private void ApplySettingsToConfigurationControls( Settings settings, List<Control> controls )
        {
            if ( controls == null )
            {
                return;
            }

            // Defined Type.
            if ( controls.Count > 0 && controls[0] is RockDropDownList ddl )
            {
                ddl.SelectedValue = settings.DefinedTypeId.ToStringSafe();
            }

            // Selectable Values.
            if ( controls.Count > 1 && controls[1] is RockCheckBoxList cblSelectableValues )
            {
                if ( settings.DefinedTypeId != null )
                {
                    // Show the Defined Values
                    var definedValues = GetDefinedValueListItems( settings.DefinedTypeId );
                    cblSelectableValues.Items.Clear();
                    cblSelectableValues.Items.AddRange( definedValues.ToArray() );

                    // Set the selected values.
                    if ( settings.SelectableValueIdList != null && settings.SelectableValueIdList.Any() )
                    {
                        foreach ( ListItem listItem in cblSelectableValues.Items )
                        {
                            listItem.Selected = settings.SelectableValueIdList.Contains( listItem.Value );
                        }
                    }
                }
            }

        }

        private List<ListItem> GetDefinedValueListItems( int? definedTypeId )
        {
            var definedValues = new List<ListItem>();

            if ( definedTypeId != null )
            {
                var definedType = DefinedTypeCache.Get( definedTypeId.Value );

                if ( definedType != null )
                {
                    definedValues = definedType.DefinedValues
                        .Select( v => new ListItem { Text = v.Value, Value = v.Id.ToString() } )
                        .ToList();
                }
            }

            return definedValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var settings = new Settings( configurationValues );

            var values = value?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToArray() ?? new string[0];
            values = values.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            if ( settings.DefinedTypeId != null )
            {
                for ( int i = 0; i < values.Length; i++ )
                {
                    var definedValue = DefinedValueCache.Get( values[i].AsInteger() );
                    if ( definedValue != null )
                    {
                        values[i] = definedValue.Value;
                    }
                }
            }

            return values.ToList().AsDelimited( ", " );
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var settings = new Settings( configurationValues );

            var control = new CategorizedValuePicker { ID = id };
            control.ValueTree = GetSelectionTreeForDefinedType( settings.DefinedTypeId, settings.SelectableValueIdList );

            return control;
        }

        /// <inheritdoc/>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is CategorizedValuePicker cvp )
            {
                return cvp.SelectedValue;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control is CategorizedValuePicker cvp )
            {
                cvp.SelectedValue = value;
            }
        }

        /// <summary>
        /// Create a tree data structure to store the hierarchy of categories and values available for selection.
        /// </summary>
        /// <param name="definedTypeId"></param>
        /// <param name="selectableValueKeys"></param>
        /// <returns></returns>
        private TreeNode<CategorizedValuePickerItem> GetSelectionTreeForDefinedType( int? definedTypeId, List<string> selectableValueKeys )
        {
            var listItems = new List<DefinedValueTreeNode>();
            var rockContext = new RockContext();

            // Get the Defined Type and associated values.
            var definedType = DefinedTypeCache.Get( definedTypeId.GetValueOrDefault( 0 ) );
            if ( definedType == null || !definedType.IsActive )
            {
                return null;
            }

            var definedValueService = new DefinedValueService( rockContext );
            var definedValues = definedValueService.GetByDefinedTypeId( definedTypeId.Value )
                .Where( x => x.IsActive )
                .OrderBy( x => x.Order )
                .ToList();

            // Filter the selectable values.
            if ( selectableValueKeys != null
                 && selectableValueKeys.Any() )
            {
                definedValues = definedValues.Where( x => selectableValueKeys.Contains( x.Id.ToString() ) ).ToList();
            }

            if ( !definedValues.Any() )
            {
                return null;
            }

            // Get a list of the Categories associated with the Defined Values.
            var categories = new Dictionary<int, Category>();
            var definedValueCategoryIdList = new List<int>();

            foreach ( var definedValue in definedValues )
            {
                if ( definedValue.CategoryId != null )
                {
                    if ( !definedValueCategoryIdList.Contains( definedValue.CategoryId.Value ) )
                    {
                        definedValueCategoryIdList.Add( definedValue.CategoryId.Value );
                    }
                }
            }

            // Retrieve the Category details, including any parent categories required to build the selection tree.
            var categoryService = new CategoryService( rockContext );

            foreach ( var categoryId in definedValueCategoryIdList )
            {
                // If this category already exists in the categories list, ignore it as an ancestor of a previous category.
                if ( categories.ContainsKey( categoryId ) )
                {
                    continue;
                }

                var ancestors = categoryService.GetAllAncestors( categoryId ).ToList();
                foreach ( var ancestor in ancestors )
                {
                    if ( !categories.ContainsKey( ancestor.Id ) )
                    {
                        categories.Add( ancestor.Id, ancestor );
                    }
                }
            }

            // Create a selection tree structure from the Categories.
            // Categories are created with a placeholder label which will be replaced by applying the naming rules.
            foreach ( var category in categories.Values )
            {
                var listItem = new DefinedValueTreeNode
                {
                    Key = $"C{category.Id}",
                    Text = category.Name,
                    CategoryLabel = "*"
                };

                if ( category.ParentCategoryId != null )
                {
                    listItem.ParentKey = $"C{category.ParentCategoryId}";
                }

                listItems.Add( listItem );
            }

            var rootNodes = TreeNode.BuildTree( listItems, cv => cv.Key, cv => cv.ParentKey );

            // Add the Defined Type as the root of the selection tree.
            var rootKey = $"T{definedType.Id}";
            var definedTypeItem = new DefinedValueTreeNode
            {
                Key = rootKey,
                Text = definedType.Name,
                CategoryLabel = "*"
            };
            var rootNode = new TreeNode<DefinedValueTreeNode>( definedTypeItem );
            rootNode.AddChildren( rootNodes );

            // Now that the tree structure is built, convert the tree nodes to picker items.
            var definedTypeNode = TreeNode.Convert( rootNode, x => ( CategorizedValuePickerItem ) x );

            // Add the Defined Values to the selection tree by applying the following rules:
            // * Defined Values can only be added to Value selection nodes, not Category selection nodes.
            // * Defined Values are added to all Value nodes that are descendants of the Category to which the Defined Value belongs.
            // * Defined Values having no parent Category are added to all Value selection nodes.
            var nodeMapByKey = definedTypeNode.Flatten()
                .ToDictionary( k => k.Value.Key, v => v );

            foreach ( var definedValue in definedValues )
            {
                // Get the parent node.
                string categoryKey = null;
                TreeNode<CategorizedValuePickerItem> parentNode = definedTypeNode;

                if ( definedValue.CategoryId != null )
                {
                    categoryKey = $"C{definedValue.CategoryId}";
                    nodeMapByKey.TryGetValue( categoryKey, out parentNode );
                }

                // Add the Defined Value to the parent category, and every child category.
                AddDefinedValueToCategoryAndChildCategories( definedValue, parentNode );
            }

            // Finally, apply the node naming rules to the selection tree.
            // 1. Top-level node is given the name of the Defined Type with "Category" appended.
            // 2. Intermediate nodes are given the name of the Category with "Category" appended.
            // 3. Value selection nodes are given the name of the Defined Type.
            definedTypeNode.Value.CategoryLabel = $"{definedType.Name} Category";

            foreach ( var node in nodeMapByKey.Values )
            {
                if ( node.Children.Any( x => x.Value.CategoryLabel != null ) )
                {
                    // The node has child categories, so it is an intermediate selection node.
                    node.Value.CategoryLabel = $"{node.Value.Text} Category";
                }
                else
                {
                    // The node has no child categories, so it is a final value node.
                    node.Value.CategoryLabel = definedType.Name;
                }
            }

            return definedTypeNode;
        }

        private void AddDefinedValueToCategoryAndChildCategories( DefinedValue definedValue, TreeNode<CategorizedValuePickerItem> parentNode )
        {
            // Add the Defined Value to the parent category, and every child category.
            if ( parentNode == null )
            {
                return;
            }

            // If this node contains child categories, add an "All Categories" node to provide access to
            // values defined by an ancestor category.
            var childCategoryNodes = parentNode.Children.Where( x => x.Value.IsCategory() );

            if ( childCategoryNodes.Any() )
            {
                var allCategoriesNode = childCategoryNodes.FirstOrDefault( x => x.Value.Text == "(All Categories)" );

                if ( allCategoriesNode == null )
                {
                    var allCategoriesItem = new DefinedValueTreeNode
                    {
                        Key = parentNode.Value.Key + "_all",
                        Text = "(All Categories)",
                        CategoryLabel = definedValue.DefinedType.Name,
                        IsDefaultSelection = true
                    };
                    allCategoriesNode = parentNode.InsertChild( 0, allCategoriesItem );
                }
                parentNode = allCategoriesNode;
            }

            // Determine the key to uniquely identify this node.
            var ancestors = parentNode.GetAncestors();
            ancestors.Insert( 0, parentNode );

            var keyPrefix = string.Empty;
            foreach ( var ancestor in ancestors )
            {
                keyPrefix = keyPrefix + ancestor.Value.Key + "_";
            }
            var listItem = new DefinedValueTreeNode
            {
                Key = keyPrefix + definedValue.Id.ToString(),
                Value = definedValue.Id.ToString(),
                Text = definedValue.Value,
            };

            if ( !parentNode.Children.Any( x => x.Value.Value == listItem.Value ) )
            {
                parentNode.AddChild( listItem );
            }

            foreach ( var childNode in childCategoryNodes )
            {
                AddDefinedValueToCategoryAndChildCategories( definedValue, childNode );
            }
        }

        #endregion

        #region Filter Control

        /// <inheritdoc/>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <inheritdoc/>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A base class to provide commonly-used functions for handling Field Type configuration settings.
        /// </summary>
        internal abstract class FieldTypeConfigurationSettings
        {
            Dictionary<string, ConfigurationValue> _configurationValues = new Dictionary<string, ConfigurationValue>();

            /// <summary>
            /// Add a configuration value definition.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="name"></param>
            /// <param name="description"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public ConfigurationValue Add( string key, string name, string description, string value )
            {
                var newValue = new ConfigurationValue( name, description, value );

                _configurationValues.Add( key, newValue );

                return newValue;
            }

            /// <summary>
            /// Attempt to set a configuration value.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool TrySetValue( string key, string value )
            {
                if ( !_configurationValues.ContainsKey( key ) )
                {
                    return false;
                }

                _configurationValues[key].Value = value;
                return true;
            }

            /// <summary>
            /// Attempt to get a configuration value, or a default value if undefined.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public string TryGetValue( string key, string defaultValue = null )
            {
                if ( !_configurationValues.ContainsKey( key ) )
                {
                    return defaultValue;
                }

                return _configurationValues[key].Value;
            }

            /// <summary>
            /// Gets the list of defined configuration keys.
            /// </summary>
            /// <returns></returns>
            public List<string> GetConfigurationKeys()
            {
                return _configurationValues.Select( x => x.Key ).ToList();
            }

            /// <summary>
            /// Gets a dictionary of defined configuration settings and their values.
            /// </summary>
            /// <returns></returns>
            public Dictionary<string, ConfigurationValue> GetConfigurationValues()
            {
                var values = _configurationValues.ToDictionary( k => k.Key, v => new ConfigurationValue( v.Value.Name, v.Value.Description, v.Value.Value ) );
                return values;
            }

            /// <summary>
            /// Sets defined configuration settings from the list of supplied values.
            /// </summary>
            /// <param name="configurationValues"></param>
            public void SetConfigurationValues( Dictionary<string, ConfigurationValue> configurationValues )
            {
                if ( configurationValues == null )
                {
                    return;
                }
                foreach ( var value in configurationValues )
                {
                    TrySetValue( value.Key, value.Value.Value );
                }
            }
        }

        private class DefinedValueTreeNode : CategorizedValuePickerItem
        {
            public string ParentKey { get; set; }
        }

        #endregion
    }
}