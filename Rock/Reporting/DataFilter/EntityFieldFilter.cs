// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter
{
    /// <summary>
    /// Abstract class that is used by DataFilters that let a user select a field/attribute of an entity
    /// </summary>
    public abstract class EntityFieldFilter : DataFilterComponent
    {
        /// <summary>
        /// Adds the field type controls.
        /// </summary>
        /// <param name="parentControl">The filter control.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="entityField">The entity field.</param>
        protected void AddFieldTypeControls( Control parentControl, List<Control> controls, EntityField entityField )
        {
            string controlIdPrefix = string.Format( "{0}_{1}", parentControl.ID, entityField.FieldKind == FieldKind.Attribute ? entityField.AttributeGuid.Value.ToString("n") : entityField.Name );
            switch ( entityField.FilterFieldType )
            {
                case SystemGuid.FieldType.DATE:

                    var ddlDateCompare = ComparisonControl( DateFilterComparisonTypes );
                    ddlDateCompare.ID = string.Format( "{0}_ddlDateCompare", controlIdPrefix );
                    ddlDateCompare.AddCssClass( "js-filter-compare" );
                    parentControl.Controls.Add( ddlDateCompare );
                    controls.Add( ddlDateCompare );

                    var datePicker = new DatePicker();
                    datePicker.ID = string.Format( "{0}_dtPicker", controlIdPrefix );
                    datePicker.AddCssClass( "js-filter-control" );
                    parentControl.Controls.Add( datePicker );
                    controls.Add( datePicker );

                    break;

                case SystemGuid.FieldType.TIME:

                    var ddlTimeCompare = ComparisonControl( DateFilterComparisonTypes );
                    ddlTimeCompare.ID = string.Format( "{0}_ddlTimeCompare", controlIdPrefix );
                    ddlTimeCompare.AddCssClass( "js-filter-compare" );
                    parentControl.Controls.Add( ddlTimeCompare );
                    controls.Add( ddlTimeCompare );

                    var timePicker = new TimePicker();
                    timePicker.ID = string.Format( "{0}_timePicker", controlIdPrefix );
                    timePicker.AddCssClass( "js-filter-control" );
                    parentControl.Controls.Add( timePicker );
                    controls.Add( timePicker );

                    break;

                case SystemGuid.FieldType.INTEGER:
                case SystemGuid.FieldType.DECIMAL:

                    var ddlNumberCompare = ComparisonControl( NumericFilterComparisonTypes );
                    ddlNumberCompare.ID = string.Format( "{0}_ddlNumberCompare", controlIdPrefix );
                    ddlNumberCompare.AddCssClass( "js-filter-compare" );
                    parentControl.Controls.Add( ddlNumberCompare );
                    controls.Add( ddlNumberCompare );

                    var numberBox = new NumberBox();
                    numberBox.ID = string.Format( "{0}_numberBox", controlIdPrefix );
                    numberBox.AddCssClass( "js-filter-control" );
                    parentControl.Controls.Add( numberBox );
                    controls.Add( numberBox );

                    numberBox.FieldName = entityField.Title;

                    break;

                case SystemGuid.FieldType.MULTI_SELECT:

                    var cblMultiSelect = new RockCheckBoxList();
                    cblMultiSelect.ID = string.Format( "{0}_cblMultiSelect", controlIdPrefix );
                    parentControl.Controls.Add( cblMultiSelect );
                    cblMultiSelect.RepeatDirection = RepeatDirection.Horizontal;
                    controls.Add( cblMultiSelect );

                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        if ( entityField.PropertyType.IsEnum )
                        {
                            // Enumeration property
                            foreach ( var value in Enum.GetValues( entityField.PropertyType ) )
                            {
                                cblMultiSelect.Items.Add( new ListItem( Enum.GetName( entityField.PropertyType, value ).SplitCase() ) );
                            }
                        }
                        else if ( entityField.DefinedTypeGuid.HasValue )
                        {
                            // Defined Value Properties
                            var definedType = DefinedTypeCache.Read( entityField.DefinedTypeGuid.Value );
                            if ( definedType != null )
                            {
                                foreach ( var definedValue in definedType.DefinedValues )
                                {
                                    cblMultiSelect.Items.Add( new ListItem( definedValue.Value, definedValue.Guid.ToString() ) );
                                }
                            }
                        }
                    }
                    else
                    {
                        var attribute = AttributeCache.Read( entityField.AttributeGuid.Value );
                        if ( attribute != null )
                        {
                            switch ( attribute.FieldType.Guid.ToString().ToUpper() )
                            {
                                case SystemGuid.FieldType.SINGLE_SELECT:
                                    //TODO get attribute values qualifier to populate cbl
                                    break;
                                case SystemGuid.FieldType.MULTI_SELECT:
                                    //TODO get attribute values qualifier to populate cbl
                                    break;
                            }
                        }
                    }

                    break;

                case SystemGuid.FieldType.SINGLE_SELECT:

                    var ddlSingleSelect = new RockDropDownList();
                    ddlSingleSelect.ID = string.Format( "{0}_ddlSingleSelect", controlIdPrefix );
                    parentControl.Controls.Add( ddlSingleSelect );
                    controls.Add( ddlSingleSelect );

                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        if ( entityField.PropertyType == typeof( bool ) || entityField.PropertyType == typeof( bool? ) )
                        {
                            ddlSingleSelect.Items.Add( new ListItem( "True", "True" ) );
                            ddlSingleSelect.Items.Add( new ListItem( "False", "False" ) );
                        }
                    }
                    else
                    {
                        var attribute = AttributeCache.Read( entityField.AttributeGuid.Value );
                        if ( attribute != null )
                        {
                            switch ( attribute.FieldType.Guid.ToString().ToUpper() )
                            {
                                case SystemGuid.FieldType.BOOLEAN:
                                    ddlSingleSelect.Items.Add( new ListItem( "True", "True" ) );
                                    ddlSingleSelect.Items.Add( new ListItem( "False", "False" ) );
                                    break;
                            }
                        }
                    }

                    break;

                case SystemGuid.FieldType.DAY_OF_WEEK:
                    var dayOfWeekPicker = new DayOfWeekPicker();
                    dayOfWeekPicker.Label = string.Empty;
                    dayOfWeekPicker.ID = string.Format( "{0}_dayOfWeekPicker", controlIdPrefix );
                    dayOfWeekPicker.AddCssClass( "js-filter-control" );
                    parentControl.Controls.Add( dayOfWeekPicker );
                    controls.Add( dayOfWeekPicker );

                    break;

                case SystemGuid.FieldType.TEXT:

                    var ddlText = ComparisonControl( StringFilterComparisonTypes );
                    ddlText.ID = string.Format( "{0}_ddlText", controlIdPrefix );
                    ddlText.AddCssClass( "js-filter-compare" );
                    parentControl.Controls.Add( ddlText );
                    controls.Add( ddlText );

                    var tbText = new RockTextBox();
                    tbText.ID = string.Format( "{0}_tbText", controlIdPrefix );
                    tbText.AddCssClass( "js-filter-control" );
                    parentControl.Controls.Add( tbText );
                    controls.Add( tbText );

                    break;
            }
        }

        /// <summary>
        /// Gets the entity field format selection.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="entityField">The entity field.</param>
        /// <returns></returns>
        public string GetEntityFieldFormatSelection( List<string> values, EntityField entityField )
        {
            string entityFieldResult = null;
            if ( entityField != null )
            {
                // If there is just one additional value then there's no comparison value
                if ( values.Count == 2 )
                {
                    if ( entityField.FilterFieldType == SystemGuid.FieldType.MULTI_SELECT )
                    {
                        var selectedValues = JsonConvert.DeserializeObject<List<string>>( values[1] );
                        var selectedTexts = new List<string>();

                        if ( entityField.DefinedTypeGuid.HasValue )
                        {
                            foreach ( string selectedValue in selectedValues )
                            {
                                Guid? definedValueGuid = selectedValue.AsGuidOrNull();

                                if ( definedValueGuid.HasValue )
                                {
                                    var definedValue = DefinedValueCache.Read( definedValueGuid.Value );
                                    if ( definedValue != null )
                                    {
                                        selectedTexts.Add( definedValue.Value );
                                    }
                                }
                            }
                        }
                        else
                        {
                            selectedTexts = selectedValues.ToList();
                        }

                        entityFieldResult = string.Format( "{0} is {1}", entityField.Title, selectedTexts.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " ) );
                    }
                    else if ( entityField.FilterFieldType == SystemGuid.FieldType.DAY_OF_WEEK )
                    {
                        DayOfWeek dayOfWeek = (DayOfWeek)( values[1].AsInteger() );

                        entityFieldResult = string.Format( "{0} is {1}", entityField.Title, dayOfWeek.ConvertToString() );
                    }
                    else
                    {
                        entityFieldResult = string.Format( "{0} is {1}", entityField.Title, values[1] );
                    }
                }
                else if ( values.Count == 3 )
                {
                    // If two more values, then it is a comparison and a value
                    ComparisonType comparisonType = values[1].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                    if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                    {
                        entityFieldResult = string.Format( "{0} {1}", entityField.Title, comparisonType.ConvertToString() );
                    }
                    else
                    {
                        Field.IFieldType fieldType = FieldTypeCache.Read( entityField.FilterFieldType.AsGuid() ).Field;
                        entityFieldResult = string.Format( "{0} {1} '{2}'", entityField.Title, comparisonType.ConvertToString(), fieldType.FormatValue(null, values[2], null, false) );
                    }
                }
            }

            return entityFieldResult;
        }

        /// <summary>
        /// Renders the entity fields controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="groupedControls">The grouped controls.</param>
        /// <param name="ddlEntityField">The DDL entity field.</param>
        public void RenderEntityFieldsControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, List<EntityField> entityFields, Dictionary<string, List<Control>> groupedControls, DropDownList ddlEntityField )
        {
            string selectedEntityField = ddlEntityField.SelectedValue;

            writer.AddAttribute( "class", "row js-filter-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlEntityField.AddCssClass( "entity-property-selection" );
            ddlEntityField.RenderControl( writer );
            writer.RenderEndTag();

            StringBuilder sb = new StringBuilder();
            int i = 0;

            writer.AddAttribute( "class", "col-md-9" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            foreach ( var entityField in entityFields )
            {
                if ( entityField.Name != selectedEntityField )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }

                writer.AddAttribute( "class", "row field-criteria" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                var propertyControls = groupedControls[entityField.Name];
                if ( propertyControls.Count == 1 )
                {
                    writer.AddAttribute( "class", "col-md-1" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "<span class='data-view-filter-label'>is</span>" );
                    writer.RenderEndTag();

                    writer.AddAttribute( "class", "col-md-11" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    propertyControls[0].RenderControl( writer );
                    writer.RenderEndTag();
                }
                else if ( propertyControls.Count == 2 )
                {
                    writer.AddAttribute( "class", "col-md-4" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    propertyControls[0].RenderControl( writer );
                    writer.RenderEndTag();

                    writer.AddAttribute( "class", "col-md-8" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    propertyControls[1].RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();  // row

                string clientFormatSelection = string.Empty;
                switch ( entityField.FilterFieldType )
                {
                    case SystemGuid.FieldType.TIME:
                    case SystemGuid.FieldType.DATE:
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('input', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).filter(':visible').val()  + '\\'') : '' )", entityField.Title );
                        break;

                    case SystemGuid.FieldType.DECIMAL:
                    case SystemGuid.FieldType.INTEGER:
                    case SystemGuid.FieldType.TEXT:
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('input', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).filter(':visible').val()  + '\\'') : '' )", entityField.Title );
                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:
                        clientFormatSelection = string.Format( "var selectedItems = ''; $('input:checked', $selectedContent).each(function() {{ selectedItems += selectedItems == '' ? '' : ' or '; selectedItems += '\\'' + $(this).parent().text() + '\\'' }}); result = '{0} is ' + selectedItems ", entityField.Title );
                        break;

                    case SystemGuid.FieldType.DAY_OF_WEEK:
                    case SystemGuid.FieldType.SINGLE_SELECT:
                        clientFormatSelection = string.Format( "result = '{0} is ' + '\\'' + $('select', $selectedContent).find(':selected').text() + '\\''", entityField.Title );
                        break;
                }

                if ( clientFormatSelection != string.Empty )
                {
                    string lineFormat = @"
            case {0}: {1}; break;";

                    sb.AppendFormat( lineFormat, i, clientFormatSelection );
                }

                i++;
            }

            writer.RenderEndTag();  // col-md-9

            writer.RenderEndTag();  // row

            string scriptFormat = @"
    function {0}PropertySelection($content){{
        var sIndex = $('select.entity-property-selection', $content).find(':selected').index();
        var $selectedContent = $('div.field-criteria', $content).eq(sIndex);
        var result = '';
        switch(sIndex) {{
            {1}
        }}
        return result;
    }}
";

            string script = string.Format( scriptFormat, entityType.Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), entityType.Name + "-property-selection", script, true );

            script = @"
    $('select.entity-property-selection').change(function(){
        var $parentRow = $(this).closest('.js-filter-row');
        $parentRow.find('div.field-criteria').hide();
        $parentRow.find('div.field-criteria').eq($(this).find(':selected').index()).show();
    });";

            // only need this script once per page
            ScriptManager.RegisterStartupScript( filterControl.Page, filterControl.Page.GetType(), "entity-property-selection-change-script", script, true );

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets the selection values for property.
        /// </summary>
        /// <param name="selectedProperty">The selected property.</param>
        /// <param name="groupedControls">The grouped controls.</param>
        /// <param name="values">The values.</param>
        public void GetSelectionValuesForProperty( string selectedProperty, Dictionary<string, List<Control>> groupedControls, List<string> values )
        {
            values.Add( selectedProperty );

            if ( groupedControls.ContainsKey( selectedProperty ) )
            {
                foreach ( Control control in groupedControls[selectedProperty] )
                {
                    if ( control is DatePicker )
                    {
                        var dtp = control as DatePicker;
                        if ( dtp != null && dtp.SelectedDate.HasValue )
                        {
                            values.Add( dtp.SelectedDate.Value.ToShortDateString() );
                        }
                        else
                        {
                            values.Add( string.Empty );
                        }
                    }
                    else if ( control is DateTimePicker )
                    {
                        var dtp = control as DateTimePicker;
                        if ( dtp != null && dtp.SelectedDateTime.HasValue )
                        {
                            values.Add( dtp.SelectedDateTime.Value.ToShortDateString() );
                        }
                        else
                        {
                            values.Add( string.Empty );
                        }
                    }
                    else if ( control is TimePicker )
                    {
                        var dtp = control as TimePicker;
                        if ( dtp != null && dtp.SelectedTime.HasValue )
                        {
                            values.Add( dtp.SelectedTime.Value.ToString() );
                        }
                        else
                        {
                            values.Add( string.Empty );
                        }
                    }
                    else if ( control is TextBox )
                    {
                        values.Add( ( (TextBox)control ).Text );
                    }
                    else if ( control is DropDownList )
                    {
                        values.Add( ( (DropDownList)control ).SelectedValue );
                    }
                    else if ( control is CheckBoxList )
                    {
                        var selectedValues = new List<string>();
                        foreach ( ListItem item in ( (CheckBoxList)control ).Items )
                        {
                            if ( item.Selected )
                            {
                                selectedValues.Add( item.Value );
                            }
                        }

                        values.Add( selectedValues.ToJson() );
                    }
                }
            }
        }

        /// <summary>
        /// Sets the entity field selection.
        /// </summary>
        /// <param name="ddlProperty">The DDL property.</param>
        /// <param name="values">The values.</param>
        /// <param name="groupedControls">The grouped controls.</param>
        public void SetEntityFieldSelection( DropDownList ddlProperty, List<string> values, Dictionary<string, List<Control>> groupedControls )
        {
            if ( values.Count > 0 )
            {
                string selectedProperty = values[0];

                if ( ddlProperty != null )
                {
                    ddlProperty.SelectedValue = selectedProperty;
                }

                bool hideFilterControl = false;

                if ( groupedControls.ContainsKey( selectedProperty ) )
                {
                    for ( int i = 0; i < groupedControls[selectedProperty].Count; i++ )
                    {
                        if ( values.Count >= i + 1 )
                        {
                            string selectedValue = values[i + 1];
                            Control control = groupedControls[selectedProperty][i];
                            WebControl webControl = control as WebControl;
                            if ( webControl != null )
                            {
                                if ( webControl.CssClass.Contains( "js-filter-control" ) )
                                {
                                    if ( hideFilterControl )
                                    {
                                        webControl.Style[HtmlTextWriterStyle.Display] = "none";
                                    }
                                }
                            }

                            if ( control is DatePicker )
                            {
                                var dateTime = selectedValue.AsDateTime();
                                if ( dateTime.HasValue )
                                {
                                    ( control as DatePicker ).SelectedDate = dateTime.Value.Date;
                                }
                            }
                            else if ( control is DateTimePicker )
                            {
                                ( control as DateTimePicker ).SelectedDateTime = selectedValue.AsDateTime();
                            }
                            else if ( control is TimePicker )
                            {
                                ( control as TimePicker ).SelectedTime = selectedValue.AsTimeSpan();
                            }
                            else if ( control is TextBox )
                            {
                                ( control as TextBox ).Text = selectedValue;
                            }
                            else if ( control is DropDownList )
                            {
                                DropDownList ddlControl = control as DropDownList;
                                ddlControl.SelectedValue = selectedValue;

                                if ( ddlControl.CssClass.Contains( "js-filter-compare" ) )
                                {
                                    ComparisonType comparisonType = ddlControl.SelectedValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                                    hideFilterControl = comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank;
                                }
                            }
                            else if ( control is CheckBoxList )
                            {
                                CheckBoxList cbl = (CheckBoxList)control;
                                List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( selectedValue );
                                foreach ( string val in selectedValues )
                                {
                                    ListItem li = cbl.Items.FindByValue( val );
                                    if ( li != null )
                                    {
                                        li.Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="property">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public Expression GetAttributeExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField property, List<string> values )
        {
            IEnumerable<int> ids = null;

            ComparisonType comparisonType = ComparisonType.EqualTo;

            var service = new AttributeValueService( (RockContext)serviceInstance.Context );
            var attributeValues = service.Queryable().Where( v =>
                v.Attribute.Guid == property.AttributeGuid &&
                v.EntityId.HasValue &&
                v.Value != string.Empty ).ToList();

            switch ( property.FilterFieldType )
            {
                case SystemGuid.FieldType.DATE:

                    if ( values.Count == 2 )
                    {
                        comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            DateTime dateValue = values[1].AsDateTime() ?? DateTime.MinValue;
                            switch ( comparisonType )
                            {
                                case ComparisonType.EqualTo:
                                case ComparisonType.NotEqualTo:
                                    ids = attributeValues.Where( v => dateValue.CompareTo( Convert.ToDateTime( v.Value ) ) == 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThan:
                                    ids = attributeValues.Where( v => dateValue.CompareTo( Convert.ToDateTime( v.Value ) ) <= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThanOrEqualTo:
                                    ids = attributeValues.Where( v => dateValue.CompareTo( Convert.ToDateTime( v.Value ) ) < 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThan:
                                    ids = attributeValues.Where( v => dateValue.CompareTo( Convert.ToDateTime( v.Value ) ) >= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThanOrEqualTo:
                                    ids = attributeValues.Where( v => dateValue.CompareTo( Convert.ToDateTime( v.Value ) ) > 0 ).Select( v => v.EntityId.Value );
                                    break;
                            }
                        }
                        else
                        {
                            ids = attributeValues.Select( v => v.EntityId.Value );
                        }
                    }

                    break;

                case SystemGuid.FieldType.TIME:

                    if ( values.Count == 2 )
                    {
                        comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            TimeSpan timeValue = values[1].AsTimeSpan() ?? TimeSpan.MinValue;
                            switch ( comparisonType )
                            {
                                case ComparisonType.EqualTo:
                                case ComparisonType.NotEqualTo:
                                    ids = attributeValues.Where( v => timeValue.CompareTo( v.Value.AsTimeSpan() ) == 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThan:
                                    ids = attributeValues.Where( v => timeValue.CompareTo( v.Value.AsTimeSpan() ) <= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThanOrEqualTo:
                                    ids = attributeValues.Where( v => timeValue.CompareTo( v.Value.AsTimeSpan() ) < 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThan:
                                    ids = attributeValues.Where( v => timeValue.CompareTo( v.Value.AsTimeSpan() ) >= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThanOrEqualTo:
                                    ids = attributeValues.Where( v => timeValue.CompareTo( v.Value.AsTimeSpan() ) > 0 ).Select( v => v.EntityId.Value );
                                    break;
                            }
                        }
                        else
                        {
                            ids = attributeValues.Select( v => v.EntityId.Value );
                        }
                    }

                    break;
                
                case SystemGuid.FieldType.DECIMAL:
                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            double numericValue = values[1].AsDoubleOrNull() ?? int.MinValue;
                            switch ( comparisonType )
                            {
                                case ComparisonType.EqualTo:
                                case ComparisonType.NotEqualTo:
                                    ids = attributeValues.Where( v => numericValue.CompareTo( v.Value.AsDouble() ) == 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThan:
                                    ids = attributeValues.Where( v => numericValue.CompareTo( v.Value.AsDouble() ) <= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThanOrEqualTo:
                                    ids = attributeValues.Where( v => numericValue.CompareTo( v.Value.AsDouble() ) < 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThan:
                                    ids = attributeValues.Where( v => numericValue.CompareTo( v.Value.AsDouble() ) >= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThanOrEqualTo:
                                    ids = attributeValues.Where( v => numericValue.CompareTo( v.Value.AsDouble() ) > 0 ).Select( v => v.EntityId.Value );
                                    break;
                            }
                        }
                        else
                        {
                            ids = attributeValues.Select( v => v.EntityId.Value );
                        }
                    }

                    break;

                case SystemGuid.FieldType.TEXT:

                    if ( values.Count == 2 )
                    {
                        comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                        switch ( comparisonType )
                        {
                            case ComparisonType.Contains:
                            case ComparisonType.DoesNotContain:
                                ids = attributeValues.Where( v => v.Value.ToUpper().Contains( values[1].ToUpper() ) ).Select( v => v.EntityId.Value );
                                break;
                            case ComparisonType.EqualTo:
                            case ComparisonType.NotEqualTo:
                                ids = attributeValues.Where( v => v.Value.Equals( values[1], StringComparison.CurrentCultureIgnoreCase ) ).Select( v => v.EntityId.Value );
                                break;
                            case ComparisonType.IsBlank:
                            case ComparisonType.IsNotBlank:
                                ids = attributeValues.Select( v => v.EntityId.Value );
                                break;
                            case ComparisonType.StartsWith:
                                ids = attributeValues.Where( v => v.Value.StartsWith( values[1], StringComparison.CurrentCultureIgnoreCase ) ).Select( v => v.EntityId.Value );
                                break;
                            case ComparisonType.EndsWith:
                                ids = attributeValues.Where( v => v.Value.EndsWith( values[1], StringComparison.CurrentCultureIgnoreCase ) ).Select( v => v.EntityId.Value );
                                break;
                        }
                    }

                    break;

                case SystemGuid.FieldType.DAY_OF_WEEK:
                case SystemGuid.FieldType.SINGLE_SELECT:

                    if ( values.Count == 1 )
                    {
                        ids = attributeValues.Where( v => v.Value == values[0] ).Select( v => v.EntityId.Value );
                    }

                    break;

                case SystemGuid.FieldType.MULTI_SELECT:

                    if ( values.Count == 1 )
                    {
                        List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[0] );
                        ids = attributeValues.Where( v => selectedValues.Contains( v.Value ) ).Select( v => v.EntityId.Value );
                    }

                    break;
            }

            if ( ids != null )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Id" );
                ConstantExpression idsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );
                Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );
                if ( comparisonType == ComparisonType.NotEqualTo ||
                    comparisonType == ComparisonType.DoesNotContain ||
                    comparisonType == ComparisonType.IsBlank )
                {
                    return Expression.Not( expression );
                }
                else
                {
                    return expression;
                }
            }

            return null;
        }
    }
}