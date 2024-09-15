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
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Base logic provider for field types that have shared UI comonents
    /// on the client to handle working with the field values.
    /// </summary>
    public abstract class UniversalItemFieldType : FieldType
    {
        #region Properties

        /// <inheritdoc/>
        public sealed override string AttributeValueFieldName => base.AttributeValueFieldName;

        /// <inheritdoc/>
        public sealed override Type AttributeValueFieldType => base.AttributeValueFieldType;

        /// <inheritdoc/>
        public sealed override Model.ComparisonType FilterComparisonType
        {
            get => IsMultipleSelection
                ? ComparisonHelper.ContainsFilterComparisonTypes
                : ComparisonHelper.BinaryFilterComparisonTypes;
        }

        /// <inheritdoc/>
        public sealed override bool HasDefaultControl => true;

        /// <summary>
        /// Gets a value to determine if this field type supports multiple
        /// selection or only single selection.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field type supports selecting multiple items.
        /// </value>
        protected virtual bool IsMultipleSelection => false;

        /// <summary>
        /// Gets a value to determine if this field type supports multiple
        /// selection on the filter control.
        /// </summary>
        /// <value>
        /// <c>true</c> if this field type supports selecting multiple items in the filter.
        /// </value>
        internal virtual bool IsMultipleFilterSelection => !IsMultipleSelection;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the value as a list of string values. If the field type is
        /// configured for single selection then this returns an array of
        /// either 0 or 1 items. If the field type is configured for multiple
        /// selection then this will return an array that contains all the
        /// selected values.
        /// </summary>
        /// <param name="privateValue">The private value stored in the database.</param>
        /// <returns>A list of strings that represent the individual values.</returns>
        protected List<string> GetValueAsList( string privateValue )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            return privateValue.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
        }

        /// <summary>
        /// Gets the item bags for the values. If an item is not found
        /// (for example, no longer exists), then it should not be included
        /// in the returned list.
        /// </summary>
        /// <param name="values">The individual values that should be retrieved.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects that have the <see cref="ListItemBag.Value"/> and <see cref="ListItemBag.Text"/> properties filled in.</returns>
        protected abstract List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues );

        #endregion

        #region Sealed Methods

        /// <inheritdoc/>
        public sealed override List<string> ConfigurationKeys()
        {
            return GetConfigurationAttributes().Select( a => a.Key ).ToList();
        }

        /// <inheritdoc/>
        public sealed override ConstantExpression AttributeConstantExpression( string value )
        {
            return base.AttributeConstantExpression( value );
        }

        /// <inheritdoc/>
        public sealed override object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            return base.ConvertValueToPropertyType( value, propertyType, isNullableType );
        }

        /// <inheritdoc/>
        public sealed override string GetCopyValue( string originalValue, RockContext rockContext )
        {
            return base.GetCopyValue( originalValue, rockContext );
        }

        /// <inheritdoc/>
        public sealed override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( !IsMultipleSelection && filterValues.Count == 1 )
            {
                var selectedValues = GetValueAsList( filterValues[0] );
                var propertyExpression = Expression.Property( parameterExpression, "Value" );

                if ( selectedValues.Count == 0 )
                {
                    // No Value specified, so return NoAttributeFilterExpression ( which means don't filter )
                    return new NoAttributeFilterExpression();
                }
                else if ( selectedValues.Count == 1 )
                {
                    // only one value, so do an Equal instead of Contains which might compile a little bit faster
                    return ComparisonHelper.ComparisonExpression( Rock.Model.ComparisonType.EqualTo, propertyExpression, AttributeConstantExpression( selectedValues[0] ) );
                }
                else
                {
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                }
            }

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        /// <inheritdoc/>
        public sealed override string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            return base.FormatFilterValues( configurationValues, filterValues );
        }

        /// <inheritdoc/>
        public sealed override string GetEqualToCompareValue()
        {
            if ( !IsMultipleSelection )
            {
                return null;
            }

            return base.GetEqualToCompareValue();
        }

        /// <inheritdoc/>
        public sealed override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            return base.GetFilterFormatScript( configurationValues, title );
        }

        /// <inheritdoc/>
        public sealed override string GetPrivateFilterValue( ComparisonValue publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            return base.GetPrivateFilterValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue.FromJsonOrNull<List<string>>();

            if ( values == null || values.Count == 0 )
            {
                return new ComparisonValue
                {
                    Value = string.Empty
                };
            }
            else if ( values.Count == 1 )
            {
                return new ComparisonValue
                {
                    Value = GetPublicEditValue( values[0], privateConfigurationValues, IsMultipleFilterSelection )
                };
            }
            else
            {
                return new ComparisonValue
                {
                    ComparisonType = values[0].ConvertToEnumOrNull<ComparisonType>(),
                    Value = GetPublicEditValue( values[1], privateConfigurationValues, IsMultipleFilterSelection )
                };
            }
        }

        /// <inheritdoc/>
        public sealed override bool IsComparedToValue( List<string> filterValues, string value )
        {
            return base.IsComparedToValue( filterValues, value );
        }

        /// <inheritdoc/>
        public sealed override bool IsEqualToValue( List<string> filterValues, string value )
        {
            return base.IsEqualToValue( filterValues, value );
        }

        /// <inheritdoc/>
        public sealed override bool IsSensitive()
        {
            return false;
        }

        /// <inheritdoc/>
        public sealed override bool IsValid( string value, bool required, out string message )
        {
            return base.IsValid( value, required, out message );
        }

        /// <inheritdoc/>
        public sealed override object ValueAsFieldType( string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value;
        }

        /// <inheritdoc/>
        public sealed override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            return base.PropertyFilterExpression( configurationValues, filterValues, parameterExpression, propertyName, propertyType );
        }

        /// <inheritdoc/>
        public sealed override string GetPersistedValuePlaceholder( Dictionary<string, string> privateConfigurationValues )
        {
            return base.GetPersistedValuePlaceholder( privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetPublicEditValue( privateValue, privateConfigurationValues, IsMultipleSelection );
        }

        /// <summary>
        /// Gets the value that will be sent to remote devices. This value is
        /// used for custom formatting as well as device-side editing.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <param name="isMultipleAllowed"><c>true</c> if multiple values are allowed.</param>
        /// <returns>A string of text to send to the remote device.</returns>
        private string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues, bool isMultipleAllowed )
        {
            var values = GetValueAsList( privateValue );

            if ( values.Count == 0 )
            {
                return string.Empty;
            }

            var bags = GetItemBags( values, privateConfigurationValues );

            if ( bags.Count == 0 )
            {
                return string.Empty;
            }

            if ( isMultipleAllowed )
            {
                return bags.ToCamelCaseJson( false, true );
            }
            else
            {
                return bags[0].ToCamelCaseJson( false, true );
            }
        }

        /// <inheritdoc/>
        public sealed override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var singleBag = publicValue.FromJsonOrNull<ListItemBag>();

            if ( singleBag != null )
            {
                return singleBag.Value;
            }

            var multiBag = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( multiBag != null )
            {
                return multiBag.Select( b => b.Value ).JoinStrings( "," );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public sealed override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = GetValueAsList( privateValue );

            if ( values.Count == 0 )
            {
                return string.Empty;
            }

            return GetItemBags( values, privateConfigurationValues )
                ?.Select( b => b.Text )
                .JoinStrings( ", " )
                ?? string.Empty;
        }

        /// <inheritdoc/>
        public sealed override string GetCondensedTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            var textValue = GetTextValue( privateValue, privateConfigurationValues );

            return new PersistedValues
            {
                TextValue = textValue,
                HtmlValue = textValue,
                CondensedTextValue = textValue,
                CondensedHtmlValue = textValue
            };
        }

        /// <inheritdoc/>
        public sealed override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            return base.IsPersistedValueInvalidated( oldPrivateConfigurationValues, newPrivateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues )
        {
            return base.IsPersistedValueSupported( privateConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override bool IsPersistedValueVolatile( Dictionary<string, string> privateConfigurationValues )
        {
            return base.IsPersistedValueVolatile( privateConfigurationValues );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the field attributes that define the configuration for this
        /// field type.
        /// </summary>
        /// <returns>A list of <see cref="FieldAttribute"/> instances.</returns>
        private List<FieldAttribute> GetConfigurationAttributes()
        {
            return GetType()
                .GetCustomAttributes( true )
                .Where( a => typeof( FieldAttribute ).IsAssignableFrom( a.GetType() ) )
                .Cast<FieldAttribute>()
                .OrderBy( a => a.Order )
                .ToList();
        }

        #endregion

        #region Configuration Methods

        /// <inheritdoc/>
        public sealed override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            return base.GetPrivateConfigurationValues( publicConfigurationValues );
        }

        /// <inheritdoc/>
        public sealed override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var fieldTypeAttributes = GetConfigurationAttributes();
            int order = 0;
            var attributeBags = new List<PublicAttributeBag>();

            // Build a list of all the field type attributes defined on this
            // instance. These are transformed into a fake attribute so that
            // the client can present them with standard logic.
            foreach ( var fieldTypeAttribute in fieldTypeAttributes )
            {
                var fieldTypeCache = FieldTypeCache.All().FirstOrDefault( c => c.Class == fieldTypeAttribute.FieldTypeClass );
                if ( fieldTypeCache == null || fieldTypeCache.Field == null )
                {
                    continue;
                }

                var configurationValues = fieldTypeAttribute.FieldConfigurationValues
                    .ToDictionary( k => k.Key, k => k.Value.Value );

                var bag = new PublicAttributeBag
                {
                    FieldTypeGuid = fieldTypeCache.ControlFieldTypeGuid,
                    AttributeGuid = Guid.NewGuid(),
                    Name = fieldTypeAttribute.Name,
                    Order = order++,
                    Key = fieldTypeAttribute.Key,
                    IsRequired = fieldTypeAttribute.IsRequired,
                    Description = fieldTypeAttribute.Description,
                    ConfigurationValues = fieldTypeCache.Field.GetPublicConfigurationValues( configurationValues, ConfigurationValueUsage.Edit, null ),
                };

                attributeBags.Add( bag );
            }

            return new Dictionary<string, string>
            {
                ["Attributes"] = attributeBags.ToCamelCaseJson( false, true )
            };
        }

        #endregion

#if WEBFORMS

        #region WebForms - General Sealed

        /// <inheritdoc/>
        public sealed override bool HasChangeHandler( Control editControl )
        {
            return base.HasChangeHandler( editControl );
        }

        /// <inheritdoc/>
        public sealed override void AddChangeHandler( Control editControl, Action action )
        {
            base.AddChangeHandler( editControl, action );
        }

        /// <inheritdoc/>
        public sealed override IQueryable<T> ApplyAttributeQueryFilter<T>( IQueryable<T> qry, Control filterControl, AttributeCache attribute, IService serviceInstance, FilterMode filterMode )
        {
            return base.ApplyAttributeQueryFilter( qry, filterControl, attribute, serviceInstance, filterMode );
        }

        /// <inheritdoc/>
        public sealed override HorizontalAlign AlignValue => base.AlignValue;

        #endregion

        #region WebForms - Configuration Controls

        /// <inheritdoc/>
        public sealed override List<Control> ConfigurationControls()
        {
            var controls = new List<Control>();
            var fieldTypeAttributes = GetConfigurationAttributes();

            for ( int i = 0; i < fieldTypeAttributes.Count; i++ )
            {
                var fieldTypeAttribute = fieldTypeAttributes[i];
                var field = Helper.InstantiateFieldType( fieldTypeAttribute.FieldTypeAssembly, fieldTypeAttribute.FieldTypeClass );

                if ( field != null )
                {
                    var control = field.EditControl( fieldTypeAttribute.FieldConfigurationValues, $"cfg_{i}" );

                    if ( control is IRockControl rockControl )
                    {
                        rockControl.Required = fieldTypeAttribute.IsRequired;
                        rockControl.Label = fieldTypeAttribute.Name;
                        rockControl.Help = fieldTypeAttribute.Description;
                    }

                    AddChangeHandler( control, () => OnQualifierUpdated( control, new EventArgs() ) );

                    controls.Add( control );
                }
                else
                {
                    controls.Add( new Literal { ID = $"cfg_{i}" } );
                }
            }

            return controls;
        }

        /// <inheritdoc/>
        public sealed override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>();
            var fieldTypeAttributes = GetConfigurationAttributes();

            for ( int i = 0; i < fieldTypeAttributes.Count; i++ )
            {
                var fieldTypeAttribute = fieldTypeAttributes[i];
                var field = Helper.InstantiateFieldType( fieldTypeAttribute.FieldTypeAssembly, fieldTypeAttribute.FieldTypeClass );

                if ( field != null && controls.Count > i )
                {
                    var value = field.GetEditValue( controls[i], fieldTypeAttribute.FieldConfigurationValues );
                    configurationValues.TryAdd( fieldTypeAttribute.Key, new ConfigurationValue( value ) );
                }
            }

            return configurationValues;
        }

        /// <inheritdoc/>
        public sealed override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var fieldTypeAttributes = GetConfigurationAttributes();

            for ( int i = 0; i < fieldTypeAttributes.Count; i++ )
            {
                var fieldTypeAttribute = fieldTypeAttributes[i];
                var field = Helper.InstantiateFieldType( fieldTypeAttribute.FieldTypeAssembly, fieldTypeAttribute.FieldTypeClass );

                if ( field != null && controls.Count > i )
                {
                    var value = configurationValues.ContainsKey( fieldTypeAttribute.Key )
                        ? configurationValues[fieldTypeAttribute.Key].Value
                        : null;

                    field.SetEditValue( controls[i], fieldTypeAttribute.FieldConfigurationValues, value );
                }
            }
        }

        #endregion

        #region WebForms - Formatting Controls

        /// <inheritdoc/>
        public sealed override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value;
        }

        /// <inheritdoc/>
        public sealed override object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value;
        }

        /// <inheritdoc/>
        public sealed override string FormatValue( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return condensed
                ? GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) )
                : GetTextValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) );
        }

        /// <inheritdoc/>
        public sealed override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return condensed
                ? GetCondensedTextValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) )
                : GetTextValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) );
        }

        /// <inheritdoc/>
        public sealed override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return condensed
                ? GetCondensedHtmlValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) )
                : GetHtmlValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) );
        }

        /// <inheritdoc/>
        public sealed override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return condensed
                ? GetCondensedHtmlValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) )
                : GetHtmlValue( value, configurationValues.ToDictionary( k => k.Key, v => v.Value.Value ) );
        }

        #endregion

        #region WebForms - Filter Controls

        /// <inheritdoc/>
        public sealed override void SetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            base.SetFilterValues( filterControl, configurationValues, filterValues );
        }

        /// <inheritdoc/>
        public sealed override void SetFilterCompareValue( Control control, string value )
        {
            if ( IsMultipleSelection )
            {
                base.SetFilterCompareValue( control, value );
            }
        }

        /// <inheritdoc/>
        public sealed override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            if ( control is Label )
            {
                return null;
            }

            return base.GetFilterCompareValue( control, filterMode );
        }

        /// <inheritdoc/>
        public sealed override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            return base.GetFilterValues( filterControl, configurationValues, filterMode );
        }

        /// <inheritdoc/>
        public sealed override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            return base.FilterControl( configurationValues, id, required );
        }

        /// <inheritdoc/>
        public sealed override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            return base.FilterControl( configurationValues, id, required, filterMode );
        }

        /// <inheritdoc/>
        public sealed override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            if ( !IsMultipleSelection )
            {
                var lbl = new Label
                {
                    ID = string.Format( "{0}_lIs", id ),
                    Text = "Is",
                    Visible = filterMode != FilterMode.SimpleFilter
                };

                lbl.AddCssClass( "data-view-filter-label" );

                return lbl;
            }

            return base.FilterCompareControl( configurationValues, id, required, filterMode );
        }

        #endregion

#endif
    }
}
