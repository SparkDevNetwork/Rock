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
#endif

using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Abstract field type class used for enum fields
    /// </summary>
    [Serializable]
    public abstract class EnumFieldType<T> : FieldType where T : struct
    {
        private const string REPEAT_COLUMNS = "repeatColumns";
        private const string OPTIONS = "options";

        private Dictionary<int, string> _EnumValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumFieldType{T}"/> class.
        /// </summary>
        public EnumFieldType() : this( null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumFieldType{T}"/> class.
        /// </summary>
        public EnumFieldType( T[] includedEnums )
        {
            SetAvailableValues( includedEnums, null );
        }

        /// <summary>
        /// Set the enumeration values that are available for selection in this field.
        /// </summary>
        /// <param name="includedValues"></param>
        protected void SetAvailableValues( IEnumerable<T> includedValues )
        {
            SetAvailableValues( includedValues, null );
        }

        /// <summary>
        /// Set the enumeration values that are available for selection in this field.
        /// </summary>
        /// <param name="includedValues"></param>
        protected void SetAvailableValues( Dictionary<T, string> includedValues )
        {
            _EnumValues = new Dictionary<int, string>();

            foreach ( var value in Enum.GetValues( typeof( T ) ) )
            {
                var key = ( T ) value;

                if ( includedValues.ContainsKey( key ) )
                {
                    _EnumValues.Add( ( int ) value, includedValues[key] );
                }
            }
        }

        /// <summary>
        /// Set the enumeration values that are available for selection in this field.
        /// </summary>
        /// <param name="includedValues"></param>
        /// <param name="excludedValues"></param>
        protected void SetAvailableValues( IEnumerable<T> includedValues, IEnumerable<T> excludedValues )
        {
            _EnumValues = new Dictionary<int, string>();

            foreach ( var value in Enum.GetValues( typeof( T ) ) )
            {
                if ( ( includedValues == null || includedValues.Contains( ( T ) value ) )
                     && ( excludedValues == null || !excludedValues.Contains( ( T ) value ) ) )
                {
                    _EnumValues.Add( ( int ) value, value.ToString().SplitCase() );
                }
            }

        }

        #region Configuration

        #endregion Configuration

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var intValue = privateValue.AsIntegerOrNull();

            if ( intValue.HasValue && _EnumValues.ContainsKey( intValue.Value ) )
            {
                return _EnumValues[intValue.Value];
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( privateConfigurationValues.ContainsKey( OPTIONS ) )
            {
                privateConfigurationValues.Remove( OPTIONS );
            }

            return privateConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            var enumValues = new List<ListItemBag>()
            {
                new ListItemBag() { Text = "None", Value = "" }
            };

            foreach ( var enumValue in _EnumValues )
            {
                enumValues.Add( new ListItemBag()
                {
                    Text = enumValue.Value,
                    Value = enumValue.Key.ToString()
                } );
            }

            publicConfigurationValues[OPTIONS] = enumValues.ToCamelCaseJson( false, true );

            return publicConfigurationValues;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns></returns>
        public override string GetEqualToCompareValue()
        {
            return null;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList().AsIntegerList();
            return AddQuotes( _EnumValues
                .Where( v => selectedValues.Contains( v.Key ) )
                .Select( v => v.Value )
                .ToList()
                .AsDelimited( "' OR '" ) );
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
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForSelectSingleField('{0}', $selectedContent);";
            return string.Format( format, titleJs );
        }

        /// <inheritdoc/>
        public override ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue.FromJsonOrNull<List<string>>();

            if ( values?.Count == 1 )
            {
                var selectedValues = values[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                if ( selectedValues.Count == 0 )
                {
                    return new ComparisonValue
                    {
                        Value = string.Empty
                    };
                }
                else
                {
                    return new ComparisonValue
                    {
                        ComparisonType = ComparisonType.Contains,
                        Value = selectedValues.Select( v => GetPublicEditValue( v, privateConfigurationValues ) ).JoinStrings( "," )
                    };
                }
            }
            else
            {
                return base.GetPublicFilterValue( privateValue, privateConfigurationValues );
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
        /// <remarks>
        /// Used only by enums ( See the EntityHelper.GetEntityFields() method )
        /// </remarks>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( selectedValues.Any() )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                object constantValue;
                if ( propertyType.IsEnum )
                {
                    constantValue = Enum.Parse( propertyType, selectedValues[0] );
                }
                else
                {
                    constantValue = selectedValues[0] as string;
                }

                ConstantExpression constantExpression = Expression.Constant( constantValue );
                Expression comparison = Expression.Equal( propertyExpression, constantExpression );

                foreach ( string selectedValue in selectedValues.Skip( 1 ) )
                {
                    constantExpression = Expression.Constant( Enum.Parse( propertyType, selectedValue ) );
                    comparison = Expression.Or( comparison, Expression.Equal( propertyExpression, constantExpression ) );
                }

                return comparison;
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
            if ( filterValues.Count == 1 )
            {
                List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                if ( selectedValues.Any() )
                {
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                }
            }

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Get a serialized representation of a value for this field type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetSerializedValue( T value )
        {
            return ( ( int ) ( object ) value ).ToString();
        }

        /// <summary>
        /// Get a value for this field type from a serialized representation, or return the specified default value.
        /// </summary>
        /// <param name="serialized"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetDeserializedValue( string serialized, T defaultValue )
        {
            T enumValue;

            var isValid = Enum.TryParse( serialized, out enumValue );

            if ( isValid )
            {
                return enumValue;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( REPEAT_COLUMNS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = base.ConfigurationControls();

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            string description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            if ( controls != null && controls.Count > 0 )
            {
                var tbRepeatColumns = controls[0] as NumberBox;
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : string.Empty;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count > 0 && configurationValues != null )
            {
                var tbRepeatColumns = controls[0] as NumberBox;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
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
            // Don't ever truncate the value even in condensed mode.
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsIntegerOrNull();
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
            if ( configurationValues != null )
            {
                var editControl = new RockRadioButtonList { ID = id };
                editControl.RepeatDirection = RepeatDirection.Horizontal;

                if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                {
                    ( ( RockRadioButtonList ) editControl ).RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                }

                foreach ( var keyVal in _EnumValues )
                {
                    editControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key.ToString() ) );
                }

                if ( editControl.Items.Count > 0 )
                {
                    return editControl;
                }
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
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                return editControl.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                editControl.SetValue( value );
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
            var lbl = new Label();
            lbl.ID = string.Format( "{0}_lIs", id );
            lbl.AddCssClass( "data-view-filter-label" );
            lbl.Text = "Is";

            // hide the compare control when in SimpleFilter mode
            lbl.Visible = filterMode != FilterMode.SimpleFilter;

            return lbl;
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var cbList = new RockCheckBoxList();
            cbList.ID = string.Format( "{0}_cbList", id );
            cbList.AddCssClass( "js-filter-control" );
            cbList.RepeatDirection = RepeatDirection.Horizontal;

            foreach ( var keyVal in _EnumValues )
            {
                cbList.Items.Add( new ListItem( keyVal.Value, keyVal.Key.ToString() ) );
            }

            if ( cbList.Items.Count > 0 )
            {
                return cbList;
            }

            return null;
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            if ( control != null && control is CheckBoxList )
            {
                CheckBoxList cbl = ( CheckBoxList ) control;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }
            }

            return values.AsDelimited( "," );
        }

        /// <summary>
        /// Sets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterCompareValue( Control control, string value )
        {
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is CheckBoxList && value != null )
            {
                var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                CheckBoxList cbl = ( CheckBoxList ) control;
                foreach ( ListItem li in cbl.Items )
                {
                    li.Selected = values.Contains( li.Value );
                }
            }
        }

#endif
        #endregion

    }
}