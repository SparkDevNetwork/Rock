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
    /// Filter entities on any of it's property or attribute values
    /// </summary>
    [Description( "Filter entities on any of it's property or attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Property Filter" )]
    public class PropertyFilter : DataFilterComponent
    {
        #region Private Fields

        private string _clientFormatSelection = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return int.MinValue;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return EntityTypeCache.Read( entityType ).FriendlyName + " Fields";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return _clientFormatSelection;
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var values = JsonConvert.DeserializeObject<List<string>>( selection );

            if ( values.Count > 0 )
            {
                // First value in array is always the name of the entity field being filtered
                string entityFieldName = values[0];

                var entityField = GetEntityFields( entityType ).Where( p => p.Name == entityFieldName ).FirstOrDefault();
                if ( entityField != null )
                {
                    // If there is just one additional value then there's no comparison value
                    if ( values.Count == 2 )
                    {
                        if ( entityField.FilterFieldType == SystemGuid.FieldType.MULTI_SELECT )
                        {
                            var selectedValues = JsonConvert.DeserializeObject<List<string>>( values[1] );
                            var selectedTexts = new List<string>();

                            if ( entityField.DefinedTypeId.HasValue )
                            {
                                foreach ( string selectedValue in selectedValues )
                                {
                                    int valueId = int.MinValue;
                                    if ( int.TryParse( selectedValue, out valueId ) )
                                    {
                                        var definedValue = DefinedValueCache.Read( valueId );
                                        if ( definedValue != null )
                                        {
                                            selectedTexts.Add( definedValue.Name );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                selectedTexts = selectedValues.ToList();
                            }

                            return string.Format( "{0} is {1}", entityField.Title, selectedTexts.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " ) );
                        }
                        else
                        {
                            return string.Format( "{0} is {1}", entityField.Title, values[1] );
                        }
                    }
                    else if ( values.Count == 3 )
                    {
                        // If two more values, then it is a comparison and a value
                        ComparisonType comparisonType = values[1].ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                        if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                        {
                            return string.Format( "{0} {1}", entityField.Title, comparisonType.ConvertToString() );
                        }
                        else
                        {
                            return string.Format( "{0} {1} '{2}'", entityField.Title, comparisonType.ConvertToString(), values[2] );
                        }
                    }
                }
            }

            return entityType.Name.SplitCase() + " Property";
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            filterControl.Controls.Add( ddlProperty );
            controls.Add( ddlProperty );

            var entityFields = GetEntityFields( entityType );

            foreach ( var entityField in entityFields )
            {
                // Add the field to the dropdown of availailable fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );

                switch ( entityField.FilterFieldType )
                {
                    case SystemGuid.FieldType.DATE:

                        var ddlDateCompare = ComparisonControl( DateFilterComparisonTypes );
                        ddlDateCompare.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        ddlDateCompare.AddCssClass( "js-filter-compare" );
                        filterControl.Controls.Add( ddlDateCompare );
                        controls.Add( ddlDateCompare );

                        var dtPicker = new DatePicker();
                        dtPicker.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        dtPicker.AddCssClass( "js-filter-control" );
                        filterControl.Controls.Add( dtPicker );
                        controls.Add( dtPicker );

                        break;

                    case SystemGuid.FieldType.INTEGER:

                        var ddlIntegerCompare = ComparisonControl( NumericFilterComparisonTypes );
                        ddlIntegerCompare.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        ddlIntegerCompare.AddCssClass( "js-filter-compare" );
                        filterControl.Controls.Add( ddlIntegerCompare );
                        controls.Add( ddlIntegerCompare );

                        var numberBox = new NumberBox();
                        numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        numberBox.AddCssClass( "js-filter-control" );
                        filterControl.Controls.Add( numberBox );
                        controls.Add( numberBox );

                        numberBox.FieldName = entityField.Title;

                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:

                        var cblMultiSelect = new RockCheckBoxList();
                        cblMultiSelect.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( cblMultiSelect );
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
                            else if ( entityField.DefinedTypeId.HasValue )
                            {
                                // Defined Value Properties
                                var definedType = DefinedTypeCache.Read( entityField.DefinedTypeId.Value );
                                if ( definedType != null )
                                {
                                    foreach ( var definedValue in definedType.DefinedValues )
                                    {
                                        cblMultiSelect.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
                                    }
                                }
                            }
                        }
                        else
                        {
                            var attribute = AttributeCache.Read( entityField.AttributeId.Value );
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
                        ddlSingleSelect.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddlSingleSelect );
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
                            var attribute = AttributeCache.Read( entityField.AttributeId.Value );
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

                    case SystemGuid.FieldType.TEXT:

                        var ddl = ComparisonControl( StringFilterComparisonTypes );
                        ddl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        ddl.AddCssClass( "js-filter-compare" );
                        filterControl.Controls.Add( ddl );
                        controls.Add( ddl );

                        var tb = new RockTextBox();
                        tb.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        tb.AddCssClass( "js-filter-control" );
                        filterControl.Controls.Add( tb );
                        controls.Add( tb );

                        break;
                }
            }

            _clientFormatSelection = string.Format( "{0}PropertySelection( $content )", entityType.Name );

            return controls.ToArray();
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            string selectedEntityField = ( (DropDownList)controls[0] ).SelectedValue;

            writer.AddAttribute( "class", "row js-filter-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ( (WebControl)controls[0] ).AddCssClass( "entity-property-selection" );
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            var groupedControls = GroupControls( entityType, controls );

            StringBuilder sb = new StringBuilder();
            int i = 0;

            writer.AddAttribute( "class", "col-md-9" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            foreach ( var entityField in GetEntityFields( entityType ) )
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
                    case SystemGuid.FieldType.DATE:
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('input', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).filter(':visible').val()  + '\\'') : '' )", entityField.Title );
                        break;

                    case SystemGuid.FieldType.INTEGER:
                    case SystemGuid.FieldType.TEXT:
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('input', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).filter(':visible').val()  + '\\'') : '' )", entityField.Title );
                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:
                        clientFormatSelection = string.Format( "var selectedItems = ''; $('input:checked', $selectedContent).each(function() {{ selectedItems += selectedItems == '' ? '' : ' or '; selectedItems += '\\'' + $(this).parent().text() + '\\'' }}); result = '{0} is ' + selectedItems ", entityField.Title );
                        break;

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
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var values = new List<string>();

            DropDownList ddlProperty = controls[0] as DropDownList;
            if ( ddlProperty != null )
            {
                string selectedProperty = ddlProperty.SelectedValue;
                values.Add( selectedProperty );

                var groupedControls = GroupControls( entityType, controls );

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
                                values.Add( dtp.SelectedTime.Value.ToTimeString() );
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

            return values.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count > 0 )
                {
                    string selectedProperty = values[0];

                    DropDownList ddlProperty = controls[0] as DropDownList;
                    if ( ddlProperty != null )
                    {
                        ddlProperty.SelectedValue = selectedProperty;
                    }

                    var groupedControls = GroupControls( entityType, controls );
                    bool hideFilterControl = false;

                    if ( groupedControls.ContainsKey( selectedProperty ) )
                    {
                        for ( int i = 0; i < groupedControls[selectedProperty].Count; i++ )
                        {
                            if ( values.Count >= i + 1 )
                            {
                                Control control = groupedControls[selectedProperty][i];
                                if (control is WebControl)
                                {
                                    if ((control as WebControl).CssClass.Contains("js-filter-control"))
                                    {
                                        if ( hideFilterControl )
                                        {
                                            ( control as WebControl ).Style[HtmlTextWriterStyle.Display] = "none";
                                        }
                                    }
                                }

                                if ( control is DatePicker )
                                {
                                    var dt = DateTime.MinValue;
                                    if ( DateTime.TryParse( values[i + 1], out dt ) )
                                    {
                                        ( (DatePicker)control ).SelectedDate = dt.Date;
                                    }
                                }
                                else if ( control is DateTimePicker )
                                {
                                    var dt = DateTime.MinValue;
                                    if ( DateTime.TryParse( values[i + 1], out dt ) )
                                    {
                                        ( (DateTimePicker)control ).SelectedDateTime = dt;
                                    }
                                }
                                else if ( control is TimePicker )
                                {
                                    var dt = DateTime.MinValue;
                                    if ( DateTime.TryParse( values[i + 1], out dt ) )
                                    {
                                        ( (TimePicker)control ).SelectedTime = dt.TimeOfDay;
                                    }
                                }
                                else if ( control is TextBox )
                                {
                                    ( (TextBox)control ).Text = values[i + 1];
                                }
                                else if ( control is DropDownList )
                                {
                                    DropDownList ddlControl = control as DropDownList;
                                    ddlControl.SelectedValue = values[i + 1];

                                    if ( ddlControl.CssClass.Contains( "js-filter-compare" ) )
                                    {
                                        ComparisonType comparisonType = ddlControl.SelectedValue.ConvertToEnum<ComparisonType>(ComparisonType.EqualTo);
                                        hideFilterControl = comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank;
                                    }
                                }
                                else if ( control is CheckBoxList )
                                {
                                    CheckBoxList cbl = (CheckBoxList)control;
                                    List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[i + 1] );
                                    foreach ( string selectedValue in selectedValues )
                                    {
                                        ListItem li = cbl.Items.FindByValue( selectedValue );
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
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 2 )
                {
                    string selectedProperty = values[0];

                    var entityField = GetEntityFields( entityType ).Where( p => p.Name == selectedProperty ).FirstOrDefault();
                    if ( entityField != null )
                    {
                        if ( entityField.FieldKind == FieldKind.Property )
                        {
                            return GetPropertyExpression( serviceInstance, parameterExpression, entityField, values.Skip( 1 ).ToList() );
                        }
                        else
                        {
                            return GetAttributeExpression( serviceInstance, parameterExpression, entityField, values.Skip( 1 ).ToList() );
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the properties and attributes for the entity
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private List<EntityField> GetEntityFields( Type entityType )
        {
            return EntityHelper.GetEntityFields( entityType );
        }

        /// <summary>
        /// Builds an expression for a property field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private Expression GetPropertyExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            Expression trueValue = Expression.Constant( true );
            MemberExpression propertyExpression = Expression.Property( parameterExpression, entityField.Name );

            switch ( entityField.FilterFieldType )
            {
                // Date Properties
                case SystemGuid.FieldType.DATE:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>(ComparisonType.EqualTo);
                        DateTime dateValue = values[1].AsDateTime() ?? DateTime.MinValue;
                        ConstantExpression constantExpression = Expression.Constant(dateValue);

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            if ( entityField.PropertyType == typeof( DateTime? ) )
                            {
                                // special case for Nullable Type
                                MemberExpression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                MemberExpression valueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression comparisonExpression = ComparisonExpression( comparisonType, valueExpression, constantExpression );
                                return Expression.AndAlso( hasValue, comparisonExpression );
                            }
                        }

                        return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;

                // Number properties
                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>(ComparisonType.EqualTo);
                        int intValue = values[1].AsInteger(false) ?? int.MinValue;
                        ConstantExpression constantExpression = Expression.Constant(intValue);

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            if ( entityField.PropertyType == typeof( int? ) )
                            {
                                // special case for Nullable Type
                                MemberExpression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                MemberExpression valueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression comparisonExpression = ComparisonExpression( comparisonType, valueExpression, constantExpression );
                                return Expression.AndAlso( hasValue, comparisonExpression );
                            }
                        }

                        return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;

                // Enumerations and Defined Value properties
                case SystemGuid.FieldType.MULTI_SELECT:

                    if ( values.Count == 1 )
                    {
                        List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[0] );
                        if ( selectedValues.Any() )
                        {
                            if ( entityField.PropertyType.IsEnum )
                            {
                                ConstantExpression constantExpression = Expression.Constant(Enum.Parse(entityField.PropertyType, selectedValues[0].Replace(" ", string.Empty)));
                                Expression comparison = Expression.Equal( propertyExpression, constantExpression );

                                foreach ( string selectedValue in selectedValues.Skip( 1 ) )
                                {
                                    constantExpression = Expression.Constant( Enum.Parse( entityField.PropertyType, selectedValue.Replace( " ", string.Empty ) ) );
                                    comparison = Expression.Or( comparison, Expression.Equal( propertyExpression, constantExpression ) );
                                }

                                return comparison;
                            }
                            else if ( entityField.DefinedTypeId.HasValue )
                            {
                                List<int> selectedIds = selectedValues.Select( v => int.Parse( v ) ).ToList();
                                ConstantExpression constantExpression = Expression.Constant(selectedIds, typeof(List<int>));

                                if ( entityField.PropertyType == typeof( int? ) )
                                {
                                    // special case for Nullable Type
                                    MemberExpression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                    MemberExpression valueExpression = Expression.Property( propertyExpression, "Value" );
                                    MethodCallExpression containsExpression = Expression.Call( constantExpression, "Contains", new Type[] { }, valueExpression );
                                    return Expression.AndAlso( hasValue, containsExpression );
                                }
                                else
                                {
                                    return Expression.Call( constantExpression, "Contains", new Type[] { }, propertyExpression );
                                }
                            }
                        }
                    }

                    break;

                // Boolean Properties
                case SystemGuid.FieldType.SINGLE_SELECT:

                    if ( values.Count == 1 )
                    {
                        if ( entityField.PropertyType == typeof( bool ) || entityField.PropertyType == typeof( bool? ) )
                        {
                            ConstantExpression constantExpression = Expression.Constant(bool.Parse(values[0]));
                            ComparisonType comparisonType = ComparisonType.EqualTo;

                            if ( entityField.PropertyType == typeof( bool? ) )
                            {
                                // special case for Nullable Type
                                MemberExpression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                MemberExpression valueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression compareExpression = ComparisonExpression( comparisonType, valueExpression, constantExpression );
                                return Expression.AndAlso( hasValue, compareExpression );
                            }
                            else
                            {
                                return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                            }
                        }
                    }

                    break;

                // String Properties
                case SystemGuid.FieldType.TEXT:

                    if ( values.Count == 2 )
                    {
                        ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>(ComparisonType.EqualTo);
                        ConstantExpression constantExpression = Expression.Constant(values[1]);

                        return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }

                    break;
            }

            return null;
        }

        /// <summary>
        /// Builds an expression for an attribute field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="property">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private Expression GetAttributeExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField property, List<string> values )
        {
            IEnumerable<int> ids = null;

            ComparisonType comparisonType = ComparisonType.EqualTo;

            var service = new AttributeValueService( (RockContext)serviceInstance.Context );
            var attributeValues = service.Queryable().Where( v =>
                v.AttributeId == property.AttributeId &&
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

                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );

                        if ( !( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            int intValue = values[1].AsInteger( false ) ?? int.MinValue;
                            switch ( comparisonType )
                            {
                                case ComparisonType.EqualTo:
                                case ComparisonType.NotEqualTo:
                                    ids = attributeValues.Where( v => intValue.CompareTo( Convert.ToInt32( v.Value ) ) == 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThan:
                                    ids = attributeValues.Where( v => intValue.CompareTo( Convert.ToInt32( v.Value ) ) <= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.GreaterThanOrEqualTo:
                                    ids = attributeValues.Where( v => intValue.CompareTo( Convert.ToInt32( v.Value ) ) < 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThan:
                                    ids = attributeValues.Where( v => intValue.CompareTo( Convert.ToInt32( v.Value ) ) >= 0 ).Select( v => v.EntityId.Value );
                                    break;
                                case ComparisonType.LessThanOrEqualTo:
                                    ids = attributeValues.Where( v => intValue.CompareTo( Convert.ToInt32( v.Value ) ) > 0 ).Select( v => v.EntityId.Value );
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

        /// <summary>
        /// Groups all the controls for each field
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        private Dictionary<string, List<Control>> GroupControls( Type entityType, Control[] controls )
        {
            var groupedControls = new Dictionary<string, List<Control>>();

            foreach ( var property in GetEntityFields( entityType ) )
            {
                if ( !groupedControls.ContainsKey( property.Name ) )
                {
                    groupedControls.Add( property.Name, new List<Control>() );
                }

                for ( int i = property.Index; i < property.Index + property.ControlCount; i++ )
                {
                    groupedControls[property.Name].Add( controls[i] );
                }
            }

            return groupedControls;
        }

        #endregion
    }
}