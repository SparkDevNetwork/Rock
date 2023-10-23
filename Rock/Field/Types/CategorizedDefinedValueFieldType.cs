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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
#endif
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using TreeNode = Rock.Web.UI.Controls.TreeNode;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a selection from a Defined Type that supports categorized values.
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( "3217C31F-85B6-4E0D-B6BE-2ADB0D28588D" )]
    public class CategorizedDefinedValueFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "DefinedType";
        private const string DEFINED_TYPES_KEY = "DefinedTypes";
        private const string DEFINED_TYPE_VALUES_KEY = "DefinedTypeValues";
        private const string SELECTABLE_VALUES_KEY = "SelectableDefinedValues";
        private const string CONFIGURATION_MODE_KEY = "ConfigurationMode";

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
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldDefinedType = oldPrivateConfigurationValues.GetValueOrNull( DEFINED_TYPE_KEY ) ?? string.Empty;
            var newDefinedType = newPrivateConfigurationValues.GetValueOrNull( DEFINED_TYPE_KEY ) ?? string.Empty;

            if ( oldDefinedType != newDefinedType )
            {
                return true;
            }

            var oldSelectableValues = oldPrivateConfigurationValues.GetValueOrNull( SELECTABLE_VALUES_KEY ) ?? string.Empty;
            var newSelectableValues = newPrivateConfigurationValues.GetValueOrNull( SELECTABLE_VALUES_KEY ) ?? string.Empty;

            if ( oldSelectableValues != newSelectableValues )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Edit Control

        private const string AllCategoriesListItemText = "All Categories";

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var editValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( editValue != null )
            {
                var definedValue = DefinedValueCache.Get( editValue.Value );
                return definedValue?.Id.ToString() ?? string.Empty;
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) && int.TryParse( privateValue, out int definedValueId ) )
            {
                var definedValue = DefinedValueCache.Get( definedValueId );

                if ( definedValue != null )
                {
                    return new ListItemBag()
                    {
                        Text = definedValue.Value,
                        Value = definedValue.Guid.ToString(),
                        Category = CategoryCache.Get( definedValue.CategoryId ?? 0 )?.Name ?? AllCategoriesListItemText
                    }.ToCamelCaseJson( false, true );
                }
            }

            return base.GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicValue(string privateValue, Dictionary<string, string> privateConfigurationValues)
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( privateConfigurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
            {
                var definedTypeValue = publicConfigurationValues[DEFINED_TYPE_KEY].FromJsonOrNull<ListItemBag>();
                if ( definedTypeValue != null && Guid.TryParse( definedTypeValue.Value, out Guid definedTypeGuid ) )
                {
                    var definedType = DefinedTypeCache.Get( definedTypeGuid );
                    if ( definedType != null )
                    {
                        privateConfigurationValues[DEFINED_TYPE_KEY] = definedType.Id.ToString();
                    }
                }
            }

            if ( privateConfigurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) )
            {
                var selectedValueGuids = privateConfigurationValues[SELECTABLE_VALUES_KEY].FromJsonOrNull<List<Guid>>();
                privateConfigurationValues[SELECTABLE_VALUES_KEY] = selectedValueGuids.ConvertAll( i => DefinedValueCache.Get( i )?.Id ).AsDelimited( "," );
            }

            privateConfigurationValues.Remove( DEFINED_TYPES_KEY );
            privateConfigurationValues.Remove( DEFINED_TYPE_VALUES_KEY );
            privateConfigurationValues.Remove( CONFIGURATION_MODE_KEY );

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );
            var definedTypes = new List<DefinedTypeCache>();

            if ( publicConfigurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) )
            {
                var selectedValueIds = publicConfigurationValues[SELECTABLE_VALUES_KEY].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList();
                publicConfigurationValues[SELECTABLE_VALUES_KEY] = selectedValueIds.ConvertAll( i => DefinedValueCache.Get( i )?.Guid ).ToCamelCaseJson( false, true );
            }

            if ( publicConfigurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
            {
                var definedTypeValue = publicConfigurationValues[DEFINED_TYPE_KEY];
                if ( int.TryParse( definedTypeValue, out int definedTypeId ) )
                {
                    var definedType = DefinedTypeCache.Get( definedTypeId );
                    if ( definedType != null )
                    {
                        publicConfigurationValues[DEFINED_TYPE_KEY] = new ListItemBag()
                        {
                            Text = definedType.Name,
                            Value = definedType.Guid.ToString()
                        }.ToCamelCaseJson( false, true );

                        // If in Edit mode add CategorizedDefinedTypes if any so we get its DefinedValues.
                        if ( usage == ConfigurationValueUsage.Edit && definedType != null )
                        {
                            definedTypes.Add( definedType );
                        }
                    }
                }
            }

            // If in Configure mode get all CategorizedDefinedTypes so we can get their DefinedValues
            if ( usage == ConfigurationValueUsage.Configure )
            {
                definedTypes = DefinedTypeCache.All()
                    .Where( x => x.CategorizedValuesEnabled ?? false )
                    .OrderBy( d => d.Order )
                    .ToList();

                publicConfigurationValues[DEFINED_TYPES_KEY] = definedTypes.ConvertAll( dt => new ListItemBag()
                {
                    Text = dt.Name,
                    Value = dt.Guid.ToString()
                } ).ToCamelCaseJson( false, true );
            }

            var definedValues = new Dictionary<string, string>();
            foreach ( var definedType in definedTypes )
            {
                var definedValueValues = definedType.DefinedValues
                    .ConvertAll( g => new ListItemBag()
                    {
                        Text = g.Value,
                        Value = g.Guid.ToString(),
                        Category = CategoryCache.Get( g.CategoryId ?? 0 )?.Name ?? "All Categories"
                    } );
                definedValues.Add( definedType.Guid.ToString(), definedValueValues.ToCamelCaseJson( false, true ) );
            }

            publicConfigurationValues.Add( DEFINED_TYPE_VALUES_KEY, definedValues.ToCamelCaseJson( false, true ) );

            publicConfigurationValues[CONFIGURATION_MODE_KEY] = usage.ToString();

            return publicConfigurationValues;
        }
        #endregion

        #region Filter Control

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

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var settings = new Settings( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) );

            var values = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToArray() ?? new string[0];
            values = values.Select( s => HttpUtility.UrlDecode( s ) ).ToArray();

            var referencedEntities = new List<ReferencedEntity>();
            if ( settings.DefinedTypeId != null )
            {
                for ( int i = 0; i < values.Length; i++ )
                {
                    var definedValue = DefinedValueCache.Get( values[i].AsInteger() );
                    if ( definedValue != null )
                    {
                        referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<DefinedValue>().Value, definedValue.Id ) );
                    }
                }
            }

            return referencedEntities;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

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

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var settings = new Settings( privateConfigurationValues.ToDictionary( k => k.Key, k => new ConfigurationValue( k.Value ) ) );

            var values = privateValue?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToArray() ?? new string[0];
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

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
                var allCategoriesNode = childCategoryNodes.FirstOrDefault( x => x.Value.Text == AllCategoriesListItemText );

                if ( allCategoriesNode == null )
                {
                    var allCategoriesItem = new DefinedValueTreeNode
                    {
                        Key = parentNode.Value.Key + "_all",
                        Text = AllCategoriesListItemText,
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

        /// <inheritdoc/>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

#endif
        #endregion
    }
}