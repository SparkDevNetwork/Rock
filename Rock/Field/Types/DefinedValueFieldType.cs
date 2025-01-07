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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ServiceBus.Messaging;

#endif

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
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type.
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple).
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M14.12,10.62V2.31A1.31,1.31,0,0,0,12.81,1H4.06A2.19,2.19,0,0,0,1.88,3.19v9.62A2.19,2.19,0,0,0,4.06,15h9.41a.66.66,0,0,0,0-1.31h-.22V11.86A1.32,1.32,0,0,0,14.12,10.62Zm-2.18,3.07H4.06a.88.88,0,0,1,0-1.75h7.88Zm.87-3.07H4.06a2.13,2.13,0,0,0-.87.19V3.19a.87.87,0,0,1,.87-.88h8.75Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.DEFINED_VALUE )]
    public class DefinedValueFieldType : FieldType, IEntityFieldType, IEntityQualifierFieldType, ICachedEntitiesFieldType, IEntityReferenceFieldType, ISplitMultiValueFieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string DISPLAY_DESCRIPTION = "displaydescription";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string ALLOW_ADDING_NEW_VALUES_KEY = "AllowAddingNewValues";
        private const string REPEAT_COLUMNS_KEY = "RepeatColumns";
        private const string SELECTABLE_VALUES_KEY = "SelectableDefinedValuesId";
        private const string VALUES_PUBLIC_KEY = "values";
        private const string SELECTABLE_VALUES_PUBLIC_KEY = "selectableValues";

        private const string DEFINED_TYPES_PROPERTY_KEY = "definedTypes";
        private const string DEFINED_VALUES_PROPERTY_KEY = "definedValues";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            var configurationProperties = new Dictionary<string, string>();

            // Determine if we need to display the description instead of the
            // value name.
            var displayDescription = privateConfigurationValues.GetValueOrDefault( DISPLAY_DESCRIPTION, "False" ).AsBoolean();

            // Determine if we need to include inactive defined values.
            var includeInactive = privateConfigurationValues.GetValueOrDefault( INCLUDE_INACTIVE_KEY, "False" ).AsBoolean();

            // Get the defined types that are available to be selected.
            var definedTypes = DefinedTypeCache.All()
                .OrderBy( t => t.Name )
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name
                } )
                .ToList();

            configurationProperties[DEFINED_TYPES_PROPERTY_KEY] = definedTypes.ToCamelCaseJson( false, true );

            // Get the currently selected defined type identifier.
            var definedTypeId = privateConfigurationValues.GetValueOrDefault( DEFINED_TYPE_KEY, "" ).AsIntegerOrNull();
            var definedTypeCache = definedTypeId.HasValue ? DefinedTypeCache.Get( definedTypeId.Value ) : null;

            if ( !definedTypeId.HasValue )
            {
                definedTypeCache = DefinedTypeCache.All().OrderBy( t => t.Name ).FirstOrDefault();
            }

            if ( definedTypeCache != null && definedTypes.Any( t => t.Value == definedTypeCache.Guid.ToString() ) )
            {
                // Get the defined values that are available to be selected.
                var definedValues = definedTypeCache
                    .DefinedValues
                    .Where( v => v.IsActive || includeInactive )
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                configurationProperties[DEFINED_VALUES_PROPERTY_KEY] = definedValues.ToCamelCaseJson( false, true );
            }

            return configurationProperties;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var definedTypeId = publicConfigurationValues.GetValueOrDefault( DEFINED_TYPE_KEY, string.Empty ).AsIntegerOrNull();
            var definedType = definedTypeId.HasValue ? DefinedTypeCache.Get( definedTypeId.Value ) : null;

            if ( usage == ConfigurationValueUsage.View )
            {
                publicConfigurationValues.Remove( DEFINED_TYPE_KEY );
                publicConfigurationValues.Remove( SELECTABLE_VALUES_KEY );
            }

            // This will be converted later if needed.
            if ( publicConfigurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) )
            {
                publicConfigurationValues.Remove( SELECTABLE_VALUES_KEY );
            }

            // Convert the defined type from an integer value to a guid.
            if ( usage == ConfigurationValueUsage.Edit || usage == ConfigurationValueUsage.Configure )
            {
                if ( definedType == null )
                {
                    definedType = DefinedTypeCache.All().OrderBy( t => t.Name ).FirstOrDefault();
                }

                publicConfigurationValues[DEFINED_TYPE_KEY] = definedType?.Guid.ToString();
            }

            if ( usage == ConfigurationValueUsage.Configure )
            {
                // If in configure mode, get the selectable value options that
                // have been set.
                if ( privateConfigurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) )
                {
                    var selectableValues = ConvertDelimitedIdsToGuids( privateConfigurationValues[SELECTABLE_VALUES_KEY], id => DefinedValueCache.Get( id )?.Guid );
                    publicConfigurationValues[SELECTABLE_VALUES_PUBLIC_KEY] = selectableValues;
                }
            }

            // Get the list of values that can be selected.
            if ( definedType != null )
            {
                int[] selectableValues = privateConfigurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) && privateConfigurationValues[SELECTABLE_VALUES_KEY].IsNotNullOrWhiteSpace()
                    ? privateConfigurationValues[SELECTABLE_VALUES_KEY].Split( ',' ).Select( int.Parse ).ToArray()
                    : null;

                var includeInactive = privateConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                var definedValues = definedType.DefinedValues
                    .Where( v => ( includeInactive || v.IsActive )
                        && ( selectableValues == null || selectableValues.Contains( v.Id ) ) );

                if ( usage == ConfigurationValueUsage.View )
                {
                    var selectedValues = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
                    definedValues = definedValues.Where( dv => selectedValues.Contains( dv.Guid ) );
                }

                publicConfigurationValues[VALUES_PUBLIC_KEY] = definedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new
                    {
                        Value = v.Guid,
                        Text = v.Value,
                        v.Description
                    } )
                    .ToCamelCaseJson( false, true );
            }
            else
            {
                publicConfigurationValues[VALUES_PUBLIC_KEY] = "[]";
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            // Convert the selectable values from unique identifiers into
            // integer identifiers that can be stored in the database.
            var selectableValues = publicConfigurationValues.GetValueOrDefault( SELECTABLE_VALUES_PUBLIC_KEY, string.Empty );
            selectableValues = ConvertDelimitedGuidsToIds( selectableValues, v => DefinedValueCache.Get( v )?.Id );

            privateConfigurationValues[SELECTABLE_VALUES_KEY] = selectableValues;

            // Convert the defined type value from a guid to an integer.
            var definedTypeGuid = privateConfigurationValues.GetValueOrDefault( DEFINED_TYPE_KEY, string.Empty ).AsGuidOrNull();
            privateConfigurationValues.Remove( DEFINED_TYPE_KEY );

            if ( definedTypeGuid.HasValue )
            {
                var definedTypeCache = DefinedTypeCache.Get( definedTypeGuid.Value );

                if ( definedTypeCache != null )
                {
                    privateConfigurationValues[DEFINED_TYPE_KEY] = definedTypeCache.Id.ToString();
                }
            }

            return privateConfigurationValues;
        }

        #endregion

        #region EntityQualifierConfiguration

        /// <summary>
        /// Gets the configuration values for this field using the EntityTypeQualiferColumn and EntityTypeQualifierValues
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public Dictionary<string, Rock.Field.ConfigurationValue> GetConfigurationValuesFromEntityQualifier( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple defined type values to be selected.", string.Empty ) );
            configurationValues.Add( DISPLAY_DESCRIPTION, new ConfigurationValue( "Display Descriptions", "When set, the defined value descriptions will be displayed instead of the values.", string.Empty ) );
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );
            configurationValues.Add( ALLOW_ADDING_NEW_VALUES_KEY, new ConfigurationValue( "Allow Adding New Values", "When set the defined type picker can be used to add new defined types.", string.Empty ) );
            configurationValues.Add( REPEAT_COLUMNS_KEY, new ConfigurationValue( "Repeat Columns", "Select how many columns the list should use before going to the next row, if not set 4 is used. This setting has no effect if 'Enhance For Long Lists' is selected since that will not use a checkbox list.", string.Empty ) );
            configurationValues.Add( SELECTABLE_VALUES_KEY, new ConfigurationValue( "Selectable Values", "Specify the values eligible for this control. If none are specified then all will be displayed.", string.Empty ) );

            if ( entityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) )
            {
                configurationValues[DEFINED_TYPE_KEY].Value = entityTypeQualifierValue;
            }

            return configurationValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool useDescription = configurationValues?.ContainsKey( DISPLAY_DESCRIPTION ) ?? false
                    ? configurationValues[DISPLAY_DESCRIPTION].AsBoolean()
                    : false;

                var names = new List<string>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        names.Add( useDescription && definedValue.Description.IsNotNullOrWhiteSpace() ? definedValue.Description : definedValue.Value );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            return formattedValue;
        }

        /// <inheritdoc/>
        public override string GetCondensedTextValue( string value, Dictionary<string, string> configurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool useDescription = configurationValues?.ContainsKey( DISPLAY_DESCRIPTION ) ?? false
                    ? configurationValues[DISPLAY_DESCRIPTION].AsBoolean()
                    : false;

                var names = new List<string>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        names.Add( useDescription && definedValue.Description.IsNotNullOrWhiteSpace() ? definedValue.Description : definedValue.Value );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            return formattedValue.Truncate( CondensedTruncateLength );
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = privateValue.SplitDelimitedValues().AsGuidList();
            bool useDescription = privateConfigurationValues?.ContainsKey( DISPLAY_DESCRIPTION ) ?? false
                ? privateConfigurationValues[DISPLAY_DESCRIPTION].AsBoolean()
                : false;

            var definedValues = new List<DefinedValueCache>();
            foreach ( var guid in guids )
            {
                var definedValue = DefinedValueCache.Get( guid );
                if ( definedValue != null )
                {
                    definedValues.Add( definedValue );
                }
            }

            return new PublicValue
            {
                Value = privateValue,
                Text = definedValues.Select( v => v.Value ).JoinStrings( ", " ),
                Description = useDescription ? definedValues.Select( v => v.Description ).JoinStrings( ", " ) : string.Empty
            }.ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var value = publicValue.FromJsonOrNull<PublicValue>();

            return value?.Value ?? string.Empty;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            bool useDescription = false;
            if ( configurationValues != null &&
                configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) &&
                configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean() )
            {
                useDescription = true;
            }

            var values = new List<string>();
            foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
            {
                var definedValue = DefinedValueCache.Get( guid );
                if ( definedValue != null )
                {
                    values.Add( useDescription ? definedValue.Description : definedValue.Value );
                }
            }

            return AddQuotes( values.ToList().AsDelimited( "' OR '" ) );
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple )
            {
                return base.GetFilterFormatScript( configurationValues, title );
            }

            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForDefinedValueField('{0}', $selectedContent);";
            return string.Format( format, titleJs );
        }


        /// <inheritdoc/>
        public override ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue.FromJsonOrNull<List<string>>();
            if ( values?.Count == 2 )
            {
                return new ComparisonValue
                {
                    ComparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains ),
                    Value = GetPublicEditValue( values[1], privateConfigurationValues )
                };
            }
            else if ( values?.Count == 1 )
            {
                return new ComparisonValue
                {
                    ComparisonType = ComparisonType.Contains,
                    Value = GetPublicEditValue( values[0], privateConfigurationValues )
                };
            }
            else
            {
                return new ComparisonValue
                {
                    Value = string.Empty
                };
            }
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( selectedValues.Any() )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                var type = propertyType;
                bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
                if ( isNullableType )
                {
                    type = Nullable.GetUnderlyingType( type );
                    propertyExpression = Expression.Property( propertyExpression, "Value" );
                }

                Type genericListType = typeof( List<> );
                Type specificListType = genericListType.MakeGenericType( type );
                object specificList = Activator.CreateInstance( specificListType );

                foreach ( string value in selectedValues )
                {
                    string tempValue = value;

                    // if this is not for an attribute value, look up the id for the defined value
                    if ( propertyName != "Value" || propertyType != typeof( string ) )
                    {
                        var dv = DefinedValueCache.Get( value.AsGuid() );
                        tempValue = dv != null ? dv.Id.ToString() : string.Empty;
                    }

                    if ( !string.IsNullOrWhiteSpace( tempValue ) )
                    {
                        object obj = Convert.ChangeType( tempValue, type );
                        specificListType.GetMethod( "Add" ).Invoke( specificList, new object[] { obj } );
                    }
                }

                ConstantExpression constantExpression = Expression.Constant( specificList, specificListType );
                return Expression.Call( constantExpression, specificListType.GetMethod( "Contains", new Type[] { type } ), propertyExpression );
            }

            return null;
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            List<string> selectedValues;
            if ( allowMultiple || filterValues.Count != 1 )
            {
                ComparisonType comparisonType = filterValues[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains );

                // if it isn't either "Contains" or "Not Contains", just use the base AttributeFilterExpression
                if ( !( new ComparisonType[] { ComparisonType.Contains, ComparisonType.DoesNotContain } ).Contains( comparisonType ) )
                {
                    return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
                }

                //// OR up the where clauses for each of the selected values
                //// and make sure to wrap commas around things so we don't collide with partial matches
                //// so it'll do something like this:
                ////
                //// WHERE ',' + Value + ',' like '%,bacon,%'
                //// OR ',' + Value + ',' like '%,lettuce,%'
                //// OR ',' + Value + ',' like '%,tomato,%'

                if ( filterValues.Count > 1 )
                {
                    selectedValues = filterValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                }
                else
                {
                    selectedValues = new List<string>();
                }

                Expression comparison = null;

                foreach ( var selectedValue in selectedValues )
                {
                    var searchValue = "," + selectedValue + ",";
                    var qryToExtract = new AttributeValueService( new Data.RockContext() ).Queryable().Where( a => ( "," + a.Value + "," ).Contains( searchValue ) );
                    var valueExpression = FilterExpressionExtractor.Extract<AttributeValue>( qryToExtract, parameterExpression, "a" );

                    if ( comparisonType != ComparisonType.Contains )
                    {
                        valueExpression = Expression.Not( valueExpression );
                    }

                    if ( comparison == null )
                    {
                        comparison = valueExpression;
                    }
                    else
                    {
                        comparison = Expression.Or( comparison, valueExpression );
                    }
                }

                if ( comparison == null )
                {
                    // No Value specified, so return NoAttributeFilterExpression ( which means don't filter )
                    return new NoAttributeFilterExpression();
                }
                else
                {
                    return comparison;
                }
            }

            selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            int valueCount = selectedValues.Count();
            MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
            if ( valueCount == 0 )
            {
                // No Value specified, so return NoAttributeFilterExpression ( which means don't filter )
                return new NoAttributeFilterExpression();
            }
            else if ( valueCount == 1 )
            {
                // only one value, so do an Equal instead of Contains which might compile a little bit faster
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( selectedValues[0] ) );
            }
            else
            {
                ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new DefinedValueService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region ICachedEntitiesFieldType Members
        /// <summary>
        /// Gets the cached defined values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var definedValues = new List<IEntityCache>();

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        definedValues.Add( definedValue );
                    }
                }
            }

            return definedValues;
        }
        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldDisplayDescription = oldPrivateConfigurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ) ?? string.Empty;
            var newDisplayDescription = newPrivateConfigurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ) ?? string.Empty;

            if ( oldDisplayDescription != newDisplayDescription )
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var definedValueEntityTypeId = EntityTypeCache.GetId<DefinedValue>().Value;

            return privateValue
                .Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                .AsGuidList()
                .Select( g => DefinedValueCache.Get( g ) )
                .Where( dv => dv != null )
                .Select( dv => new ReferencedEntity( definedValueEntityTypeId, dv.Id ) )
                .ToList();
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Value and Description properties of
            // a DefinedValue and should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Value ) ),
                new ReferencedProperty( EntityTypeCache.GetId<DefinedValue>().Value, nameof( DefinedValue.Description ) )
            };
        }

        #endregion

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return privateValue.Split( ',' );
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( DISPLAY_DESCRIPTION );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( ALLOW_ADDING_NEW_VALUES_KEY );
            configKeys.Add( REPEAT_COLUMNS_KEY );
            configKeys.Add( SELECTABLE_VALUES_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of defined types (the one that gets selected is
            // used to build a list of defined values)
            var ddlDefinedType = new RockDropDownList
            {
                AutoPostBack = true,
                Label = "Defined Type",
                Help = "The Defined Type to select values from."
            };

            ddlDefinedType.SelectedIndexChanged += OnQualifierUpdated;

            var definedTypeService = new DefinedTypeService( new RockContext() );
            ddlDefinedType.Items.Add( new ListItem() );
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Name ) )
            {
                ddlDefinedType.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            // Add checkbox for deciding if the defined values list is rendered as a drop down list or a checkbox list.
            var cbAllowMultipleValues = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Allow Multiple Values",
                Help = "When set, allows multiple defined type values to be selected."
            };

            cbAllowMultipleValues.CheckedChanged += OnQualifierUpdated;

            // option for Display Descriptions
            var cbDescription = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Display Descriptions",
                Help = "When set, the defined value descriptions will be displayed instead of the values."
            };

            cbDescription.CheckedChanged += OnQualifierUpdated;

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnhanced = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Enhance For Long Lists",
                Help = "When set, will render a searchable selection of options."
            };

            cbEnhanced.CheckedChanged += OnQualifierUpdated;

            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Include Inactive",
                Help = "When set, inactive defined values will be included in the list."
            };

            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;

            // Checkbox to indicate if new defined types can be added via the field type.
            var cbAllowAddingNewValues = new RockCheckBox
            {
                AutoPostBack = true,
                Label = "Allow Adding New Values",
                Help = "When set the defined type picker can be used to add new defined types."
            };

            cbAllowAddingNewValues.CheckedChanged += OnQualifierUpdated;

            var tbRepeatColumns = new NumberBox
            {
                AutoPostBack = true,
                Label = "Repeat Columns",
                Help = "Select how many columns the list should use before going to the next row. If 0 then the options are put next to each other and wrap around. If blank then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.",
                MinimumValue = "0"
            };

            tbRepeatColumns.TextChanged += OnQualifierUpdated;

            List<(string, string)> definedValues = new List<(string, string)>();
            if ( ddlDefinedType.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                definedValues = DefinedTypeCache.Get( ddlDefinedType.SelectedValue.AsInteger() ).DefinedValues.Select( v => (Text: v.Value, Value: v.Id.ToString()) ).ToList();
            }

            var cblSelectableDefinedValues = new RockCheckBoxList
            {
                AutoPostBack = true,
                RepeatDirection = RepeatDirection.Horizontal,
                Label = "Selectable Values",
                DataTextField = "Text",
                DataValueField = "Value",
                DataSource = definedValues,
                Visible = definedValues.Any()
            };

            cblSelectableDefinedValues.DataBind();

            controls.Add( ddlDefinedType );
            controls.Add( cbAllowMultipleValues );
            controls.Add( cbDescription );
            controls.Add( cbEnhanced );
            controls.Add( cbIncludeInactive );
            controls.Add( cbAllowAddingNewValues );
            controls.Add( tbRepeatColumns );
            controls.Add( cblSelectableDefinedValues );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple defined type values to be selected.", string.Empty ) );
            configurationValues.Add( DISPLAY_DESCRIPTION, new ConfigurationValue( "Display Descriptions", "When set, the defined value descriptions will be displayed instead of the values.", string.Empty ) );
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive defined values will be included in the list.", string.Empty ) );
            configurationValues.Add( ALLOW_ADDING_NEW_VALUES_KEY, new ConfigurationValue( "Allow Adding New Values", "When set the defined type picker can be used to add new defined types.", string.Empty ) );
            configurationValues.Add( REPEAT_COLUMNS_KEY, new ConfigurationValue( "Repeat Columns", "Select how many columns the list should use before going to the next row, if not set 4 is used. This setting has no effect if 'Enhance For Long Lists' is selected since that will not use a checkbox list.", string.Empty ) );
            configurationValues.Add( SELECTABLE_VALUES_KEY, new ConfigurationValue( "Selectable Values", "Specify the values eligible for this control. If none are specified then all will be displayed.", string.Empty ) );

            if ( controls != null )
            {
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbAllowMultipleValues = controls.Count > 1 ? controls[1] as CheckBox : null;
                CheckBox cbDescription = controls.Count > 2 ? controls[2] as CheckBox : null;
                CheckBox cbEnhanced = controls.Count > 3 ? controls[3] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 4 ? controls[4] as CheckBox : null;
                CheckBox cbAllowAddNewValues = controls.Count > 5 ? controls[5] as CheckBox : null;
                NumberBox nbRepeatColumns = controls.Count > 6 ? controls[6] as NumberBox : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 7 ? controls[7] as RockCheckBoxList : null;

                if ( ddlDefinedType != null )
                {
                    configurationValues[DEFINED_TYPE_KEY].Value = ddlDefinedType.SelectedValue;
                }

                if ( cbAllowMultipleValues != null )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = cbAllowMultipleValues.Checked.ToString();
                }

                if ( cbDescription != null )
                {
                    configurationValues[DISPLAY_DESCRIPTION].Value = cbDescription.Checked.ToString();
                }

                if ( cbEnhanced != null )
                {
                    configurationValues[ENHANCED_SELECTION_KEY].Value = cbEnhanced.Checked.ToString();
                }

                if ( cbIncludeInactive != null )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive.Checked.ToString();
                }

                if ( cbAllowAddNewValues != null )
                {
                    configurationValues[ALLOW_ADDING_NEW_VALUES_KEY].Value = cbAllowAddNewValues.Checked.ToString();
                }

                if ( nbRepeatColumns != null )
                {
                    configurationValues[REPEAT_COLUMNS_KEY].Value = nbRepeatColumns.Text;
                }

                if ( cblSelectableValues != null )
                {
                    var selectableValues = new List<string>( cblSelectableValues.SelectedValues );
                    var includeInactive = cbIncludeInactive?.Checked ?? false;

                    var definedValues = includeInactive ?
                        DefinedTypeCache.Get( ddlDefinedType.SelectedValue.AsInteger() )?.DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } ) :
                        DefinedTypeCache.Get( ddlDefinedType.SelectedValue.AsInteger() )?.DefinedValues.Where( v => v.IsActive ).Select( v => new { Text = v.Value, Value = v.Id } );
                    cblSelectableValues.DataSource = definedValues;
                    cblSelectableValues.DataBind();
                    cblSelectableValues.Visible = definedValues?.Any() ?? false;

                    if ( selectableValues != null && selectableValues.Any() )
                    {
                        foreach ( ListItem listItem in cblSelectableValues.Items )
                        {
                            listItem.Selected = selectableValues.Contains( listItem.Value );
                        }
                    }

                    configurationValues[SELECTABLE_VALUES_KEY].Value = cblSelectableValues.SelectedValues.AsDelimited( "," );
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
            if ( controls != null && configurationValues != null )
            {
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbAllowMultipleValues = controls.Count > 1 ? controls[1] as CheckBox : null;
                CheckBox cbDescription = controls.Count > 2 ? controls[2] as CheckBox : null;
                CheckBox cbEnhanced = controls.Count > 3 ? controls[3] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 4 ? controls[4] as CheckBox : null;
                CheckBox cbAllowAddNewValues = controls.Count > 5 ? controls[5] as CheckBox : null;
                NumberBox nbRepeatColumns = controls.Count > 6 ? controls[6] as NumberBox : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 7 ? controls[7] as RockCheckBoxList : null;

                if ( ddlDefinedType != null )
                {
                    ddlDefinedType.SelectedValue = configurationValues.GetValueOrNull( DEFINED_TYPE_KEY );
                }

                if ( cbAllowMultipleValues != null )
                {
                    cbAllowMultipleValues.Checked = configurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbDescription != null )
                {
                    cbDescription.Checked = configurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBooleanOrNull() ?? false;
                }

                if ( cbEnhanced != null )
                {
                    cbEnhanced.Checked = configurationValues.GetValueOrNull( ENHANCED_SELECTION_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbIncludeInactive != null )
                {
                    cbIncludeInactive.Checked = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbAllowAddNewValues != null )
                {
                    cbAllowAddNewValues.Checked = configurationValues.GetValueOrNull( ALLOW_ADDING_NEW_VALUES_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( nbRepeatColumns != null )
                {
                    nbRepeatColumns.Text = configurationValues.GetValueOrNull( REPEAT_COLUMNS_KEY );
                }

                if ( cblSelectableValues != null )
                {
                    var includeInactive = cbIncludeInactive?.Checked ?? false;

                    var definedValues = includeInactive ?
                        DefinedTypeCache.Get( ddlDefinedType.SelectedValue.AsInteger() )?.DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } ) :
                        DefinedTypeCache.Get( ddlDefinedType.SelectedValue.AsInteger() )?.DefinedValues.Where( v => v.IsActive ).Select( v => new { Text = v.Value, Value = v.Id } );
                    cblSelectableValues.DataSource = definedValues;
                    cblSelectableValues.DataBind();

                    var selectableValues = configurationValues.GetValueOrNull( SELECTABLE_VALUES_KEY )?.SplitDelimitedValues( false );
                    if ( selectableValues != null && selectableValues.Any() )
                    {
                        foreach ( ListItem listItem in cblSelectableValues.Items )
                        {
                            listItem.Selected = selectableValues.Contains( listItem.Value );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool useDescription = false;
                if ( configurationValues != null &&
                     configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) &&
                     configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean() )
                {
                    useDescription = true;
                }

                // if there are multiple defined values, just pick the first one as the sort value
                Guid guid = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList().FirstOrDefault();
                var definedValue = DefinedValueCache.Get( guid );
                if ( definedValue != null )
                {
                    // sort by Order then Description/Value (using a padded string)
                    var sortValue = definedValue.Order.ToString().PadLeft( 10 ) + "," + ( useDescription && definedValue.Description.IsNotNullOrWhiteSpace() ? definedValue.Description : definedValue.Value );
                    return sortValue;
                }
            }

            return base.SortValue( parentControl, value, configurationValues );
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
            if ( configurationValues == null )
            {
                return null;
            }

            Control editControl;

            bool useDescription = configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) && configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean();
            int? definedTypeId = configurationValues.ContainsKey( DEFINED_TYPE_KEY ) ? configurationValues[DEFINED_TYPE_KEY].Value.AsIntegerOrNull() : null;
            int repeatColumns = ( configurationValues.ContainsKey( REPEAT_COLUMNS_KEY ) ? configurationValues[REPEAT_COLUMNS_KEY].Value.AsIntegerOrNull() : null ) ?? 4;
            bool allowAdd = configurationValues.ContainsKey( ALLOW_ADDING_NEW_VALUES_KEY ) && configurationValues[ALLOW_ADDING_NEW_VALUES_KEY].Value.AsBoolean();
            bool enhanceForLongLists = configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean();
            bool allowMultiple = configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            bool includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();

            int[] selectableValues = configurationValues.ContainsKey( SELECTABLE_VALUES_KEY ) && configurationValues[SELECTABLE_VALUES_KEY].Value.IsNotNullOrWhiteSpace()
                ? configurationValues[SELECTABLE_VALUES_KEY].Value.Split( ',' ).Select( int.Parse ).ToArray()
                : null;

            if ( allowMultiple )
            {
                if ( allowAdd )
                {
                    editControl = new DefinedValuePickerWithAddMultipleSelect
                    {
                        ID = id,
                        DisplayDescriptions = useDescription,
                        DefinedTypeId = definedTypeId,
                        RepeatColumns = repeatColumns,
                        IsAllowAddDefinedValue = allowAdd,
                        EnhanceForLongLists = enhanceForLongLists,
                        SelectableDefinedValuesId = selectableValues,
                        IncludeInactive = includeInactive
                    };
                }
                else
                {
                    if ( enhanceForLongLists )
                    {
                        editControl = new DefinedValuesPickerEnhanced
                        {
                            ID = id,
                            DisplayDescriptions = useDescription,
                            DefinedTypeId = definedTypeId,
                            SelectableDefinedValuesId = selectableValues,
                            IncludeInactive = includeInactive
                        };
                    }
                    else
                    {
                        editControl = new DefinedValuesPicker
                        {
                            ID = id,
                            DisplayDescriptions = useDescription,
                            DefinedTypeId = definedTypeId,
                            RepeatColumns = repeatColumns,
                            SelectableDefinedValuesId = selectableValues,
                            IncludeInactive = includeInactive
                        };
                    }
                }
            }
            else
            {
                // TODO: The add versions of the controls are not working with AttributeValuesContainer, so keep the old ones for now
                if ( allowAdd )
                {
                    editControl = new DefinedValuePickerWithAddSingleSelect
                    {
                        ID = id,
                        DisplayDescriptions = useDescription,
                        DefinedTypeId = definedTypeId,
                        IsAllowAddDefinedValue = allowAdd,
                        EnhanceForLongLists = enhanceForLongLists,
                        SelectableDefinedValuesId = selectableValues,
                        IncludeInactive = includeInactive
                    };
                }
                else
                {
                    editControl = new DefinedValuePicker
                    {
                        ID = id,
                        DisplayDescriptions = useDescription,
                        DefinedTypeId = definedTypeId,
                        EnhanceForLongLists = enhanceForLongLists,
                        SelectableDefinedValuesId = selectableValues,
                        IncludeInactive = includeInactive
                    };
                }
            }

            if ( definedTypeId.HasValue )
            {
                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var definedValueIdList = new List<int>();

            var definedValuePicker = control as IDefinedValuePicker;
            var definedValuePickerWithAdd = control as DefinedValuePickerWithAdd;

            if ( definedValuePicker != null )
            {
                definedValueIdList = definedValuePicker.SelectedDefinedValuesId.ToList();
            }
            else if ( definedValuePickerWithAdd != null )
            {
                definedValueIdList = definedValuePickerWithAdd.SelectedDefinedValuesId.ToList();
            }

            var guids = new List<Guid>();

            foreach ( int definedValueId in definedValueIdList )
            {
                var definedValue = DefinedValueCache.Get( definedValueId );
                if ( definedValue != null )
                {
                    guids.Add( definedValue.Guid );
                }
            }

            return guids.AsDelimited( "," );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                var definedValuePicker = control as IDefinedValuePicker;
                var definedValuePickerWithAdd = control as DefinedValuePickerWithAdd;

                if ( definedValuePicker == null && definedValuePickerWithAdd == null )
                {
                    return;
                }

                var ids = new List<int>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        ids.Add( definedValue.Id );
                    }
                }

                if ( definedValuePicker != null )
                {
                    definedValuePicker.SelectedDefinedValuesId = ids.ToArray();
                }
                else if ( definedValuePickerWithAdd != null )
                {
                    definedValuePickerWithAdd.SelectedDefinedValuesId = ids.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple )
            {
                return base.FilterCompareControl( configurationValues, id, required, filterMode );
            }
            else
            {
                var lbl = new Label();
                lbl.ID = string.Format( "{0}_lIs", id );
                lbl.AddCssClass( "data-view-filter-label" );
                lbl.Text = "Is";

                // hide the compare control when in SimpleFilter mode
                lbl.Visible = filterMode != FilterMode.SimpleFilter;
                return lbl;
            }
        }

        /// <summary>
        /// Filters the value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();

            var overrideConfigValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var keyVal in configurationValues )
            {
                overrideConfigValues.Add( keyVal.Key, keyVal.Value );
            }

            overrideConfigValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new ConfigurationValue( true.ToString() ) );

            return base.FilterValueControl( overrideConfigValues, id, required, filterMode );
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            var values = new List<string>();

            if ( filterControl != null )
            {
                bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();

                try
                {
                    if ( allowMultiple )
                    {
                        var filterValues = base.GetFilterValues( filterControl, configurationValues, filterMode );
                        if ( filterValues != null )
                        {
                            filterValues.ForEach( v => values.Add( v ) );
                        }
                    }
                    else
                    {
                        values.Add( GetEditValue( filterControl.Controls[1].Controls[0], configurationValues ) );
                    }
                }
                catch
                {
                    // intentionally ignore
                }
            }

            return values;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            string value = base.GetFilterValueValue( control, configurationValues );
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple && string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }

            return value;
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = DefinedValueCache.Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            DefinedValueCache item = null;
            if ( id.HasValue )
            {
                item = DefinedValueCache.Get( id.Value );
            }

            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion

        private class PublicValue
        {
            public string Value { get; set; }

            public string Text { get; set; }

            public string Description { get; set; }
        }
    }
}