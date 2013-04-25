//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Rock;
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
        private List<EntityField> _entityFields = null;

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

                            return string.Format( "{0} is {1}", entityField.Title,
                                selectedTexts.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " ) );
                        }
                        else
                        {
                            return string.Format( "{0} is {1}", entityField.Title, values[1] );
                        }
                    }

                    // If two more values, then it is a comparison and a value
                    else if ( values.Count == 3 )
                    {
                        ComparisonType comparisonType = ComparisonType.StartsWith;
                        try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); }
                        catch { }
                        return string.Format( "{0} {1} '{2}'", entityField.Title, comparisonType.ConvertToString(), values[2] );
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
            DropDownList ddlProperty = new DropDownList();
            ddlProperty.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            filterControl.Controls.Add( ddlProperty );
            controls.Add( ddlProperty );

            foreach ( var entityField in GetEntityFields(entityType) )
            {
                // Add the field to the dropdown of availailable fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );

                switch ( entityField.FilterFieldType )
                {
                    case SystemGuid.FieldType.DATE:

                        var ddlDate = ComparisonControl( DateFilterComparisonTypes );
                        ddlDate.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddlDate );
                        controls.Add( ddlDate );

                        var dtPicker = new DateTimePicker();
                        dtPicker.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( dtPicker );
                        controls.Add( dtPicker );

                        dtPicker.DatePickerType = DateTimePickerType.Date;

                        break;

                    case SystemGuid.FieldType.INTEGER:

                        var ddlInteger = ComparisonControl( NumericFilterComparisonTypes );
                        ddlInteger.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddlInteger );
                        controls.Add( ddlInteger );

                        var numberBox = new NumberBox();
                        numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( numberBox );
                        controls.Add( numberBox );

                        numberBox.FieldName = entityField.Title;

                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:

                        var cblMultiSelect = new CheckBoxList();
                        cblMultiSelect.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( cblMultiSelect );
                        cblMultiSelect.RepeatDirection = RepeatDirection.Horizontal;
                        controls.Add( cblMultiSelect );

                        if ( entityField.FieldKind == FieldKind.Property )
                        {
                            // Enumeration property
                            if ( entityField.PropertyType.IsEnum )
                            {
                                foreach ( var value in Enum.GetValues( entityField.PropertyType ) )
                                {
                                    cblMultiSelect.Items.Add( new ListItem( Enum.GetName( entityField.PropertyType, value ).SplitCase() ) );
                                }
                            }

                            // Defined Value Properties
                            else if ( entityField.DefinedTypeId.HasValue )
                            {
                                var definedType = DefinedTypeCache.Read( entityField.DefinedTypeId.Value );
                                if ( definedType != null )
                                {
                                    foreach ( var definedValue in definedType.DefinedValues )
                                        cblMultiSelect.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
                                }
                            }
                        }
                        else
                        {
                            var attribute = AttributeCache.Read( entityField.AttributeId.Value );
                            if (attribute != null)
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

                        var ddlSingleSelect = new DropDownList();
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
                        filterControl.Controls.Add( ddl );
                        controls.Add( ddl );

                        var tb = new TextBox();
                        tb.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
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
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            string selectedEntityField = ( (DropDownList)controls[0] ).SelectedValue;

            writer.AddAttribute( "class", "entity-property-selection" );
            controls[0].RenderControl( writer );
            writer.WriteBreak();

            var groupedControls = GroupControls( entityType, controls );

            StringBuilder sb = new StringBuilder();
            int i = 0;

            foreach ( var entityField in GetEntityFields(entityType) )
            {
                var propertyControls = groupedControls[entityField.Name];

                if ( entityField.Name != selectedEntityField )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Controls
                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( propertyControls.Count == 1 )
                {
                    writer.Write( "is " );
                    propertyControls[0].RenderControl( writer );
                }
                else if (propertyControls.Count == 2)
                {
                    propertyControls[0].RenderControl( writer );
                    writer.Write( " " );
                    propertyControls[1].RenderControl( writer );
                }

                string clientFormatSelection = string.Empty;
                switch ( entityField.FilterFieldType )
                {
                    case SystemGuid.FieldType.DATE:
                        clientFormatSelection = string.Format( "result = '... a date value ...'" );
                        break;

                    case SystemGuid.FieldType.INTEGER:
                    case SystemGuid.FieldType.TEXT:
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ' \\'' + $('input', $selectedContent).val() + '\\''", entityField.Title );
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
                    sb.AppendFormat( @"
            case {0}: {1}; break;", i, clientFormatSelection );
                }
                i++;

                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            string script = string.Format( @"
    function {0}PropertySelection($content){{
        var sIndex = $('select.entity-property-selection', $content).find(':selected').index();
        var $selectedContent = $content.children('div.control-group').eq(sIndex);
        var result = '';
        switch(sIndex) {{
            {1}
        }}
        return result;
    }}
", entityType.Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), entityType.Name + "-property-selection", script, true );

            script = @"
    $('select.entity-property-selection').change(function(){
        $(this).siblings('div.control-group').hide();
        $(this).siblings('div.control-group').eq($(this).find(':selected').index()).show();
    });
";
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), "property-filter-script", script, true );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
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
                        if ( control is DateTimePicker )
                        {
                            var dtp = control as DateTimePicker;
                            if ( dtp != null && dtp.SelectedDate.HasValue )
                            {
                                values.Add( dtp.SelectedDate.Value.ToShortDateString() );
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

                    if ( groupedControls.ContainsKey( selectedProperty ) )
                    {
                        for ( int i = 0; i < groupedControls[selectedProperty].Count; i++ )
                        {
                            if ( values.Count >= i + 1 )
                            {
                                Control control = groupedControls[selectedProperty][i];

                                if ( control is DateTimePicker )
                                {
                                    var dt = DateTime.MinValue;
                                    if ( DateTime.TryParse( values[i + 1], out dt ) )
                                    {
                                        ( (DateTimePicker)control ).SelectedDate = dt;
                                    }
                                }
                                else if ( control is TextBox )
                                {
                                    ( (TextBox)control ).Text = values[i + 1];
                                }
                                else if ( control is DropDownList )
                                {
                                    ( (DropDownList)control ).SelectedValue = values[i + 1];
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
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, object serviceInstance, Expression parameterExpression, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 2 )
                {
                    string selectedProperty = values[0];

                    var entityField = GetEntityFields(entityType).Where( p => p.Name == selectedProperty ).FirstOrDefault();
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
        /// <returns></returns>
        private List<EntityField> GetEntityFields(Type entityType)
        {
            if ( _entityFields == null )
            {
                var entityFields = new List<EntityField>();

                // Get Properties
                foreach ( var property in entityType.GetProperties() )
                {
                    if ( !property.GetGetMethod().IsVirtual || property.Name == "Id" || property.Name == "Guid" || property.Name == "Order" )
                    {
                        EntityField entityProperty = null;

                        // Enum Properties
                        if ( property.PropertyType.IsEnum )
                        {
                            entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                        }

                        // Boolean properties
                        if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( bool? ) )
                        {
                            entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                        }

                        // Date properties
                        if ( property.PropertyType == typeof( DateTime ) || property.PropertyType == typeof( DateTime? ) )
                        {
                            entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
                        }

                        // Text Properties
                        else if ( property.PropertyType == typeof( string ) )
                        {
                            entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                        }

                        // Integer Properties
                        else if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( int? ) )
                        {
                            var definedValueAttribute = property.GetCustomAttributes( typeof( Rock.Data.DefinedValueAttribute ), true ).FirstOrDefault();

                            if ( definedValueAttribute != null )
                            {
                                // Defined Value Properties
                                entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 1 );
                                var definedType = DefinedTypeCache.Read( ( (Rock.Data.DefinedValueAttribute)definedValueAttribute ).DefinedTypeGuid );
                                entityProperty.Title = definedType != null ? definedType.Name : property.Name.Replace( "ValueId", "" ).SplitCase();
                                entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                                entityProperty.DefinedTypeId = definedType.Id;
                            }
                            else
                            {
                                entityProperty = new EntityField( property.Name, FieldKind.Property, property.PropertyType, 2 );
                                entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                            }
                        }

                        if ( entityProperty != null )
                        {
                            entityFields.Add( entityProperty );
                        }
                    }
                }

                // Get Attributes
                int entityTypeId = EntityTypeCache.Read( entityType ).Id;
                foreach ( var attribute in new AttributeService().Get( entityTypeId, string.Empty, string.Empty ) )
                {
                    // Ensure prop name is unique
                    string propName = attribute.Name;
                    int i = 1;
                    while ( entityFields.Any( p => p.Name.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
                    {
                        propName = attribute.Name + i++.ToString();
                    }

                    EntityField entityProperty = null;

                    switch ( attribute.FieldType.Guid.ToString().ToUpper() )
                    {
                        case SystemGuid.FieldType.BOOLEAN:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                            break;

                        case SystemGuid.FieldType.DATE:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.DATE;
                            break;

                        case SystemGuid.FieldType.INTEGER:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.INTEGER;
                            break;

                        case SystemGuid.FieldType.MULTI_SELECT:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                            break;

                        case SystemGuid.FieldType.SINGLE_SELECT:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 1, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
                            break;

                        case SystemGuid.FieldType.TEXT:
                            entityProperty = new EntityField( attribute.Name, FieldKind.Attribute, null, 2, attribute.Id );
                            entityProperty.FilterFieldType = SystemGuid.FieldType.TEXT;
                            break;
                    }

                    if ( entityProperty != null )
                    {
                        entityFields.Add( entityProperty );
                    }

                }

                int index = 1;
                _entityFields = new List<EntityField>();
                foreach ( var entityProperty in entityFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
                {
                    entityProperty.Index = index;
                    index += entityProperty.ControlCount;
                    _entityFields.Add( entityProperty );
                }
            }

            return _entityFields;
        }
 
        /// <summary>
        /// Builds an expression for a property field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        private Expression GetPropertyExpression( object serviceInstance, Expression parameterExpression, EntityField entityField, List<string> values )
        {
            Expression trueValue = Expression.Constant( true );
            MemberExpression propertyExpression = Expression.Property( parameterExpression, entityField.Name );
            Expression constantExpression = null;
            ComparisonType comparisonType = ComparisonType.EqualTo;

            switch ( entityField.FilterFieldType )
            {
                // Date Properties
                case SystemGuid.FieldType.DATE:

                    if ( values.Count == 2 )
                    {
                        DateTime dateValue = DateTime.MinValue;
                        if ( DateTime.TryParse( values[1], out dateValue ) )
                        {
                            try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                            catch { }
                            constantExpression = Expression.Constant( dateValue );

                            if ( entityField.PropertyType == typeof( DateTime? ) )
                            {
                                Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression comparisonExpression = ComparisonExpression( comparisonType, ValueExpression, constantExpression );
                                return Expression.AndAlso( hasValue, comparisonExpression );
                            }
                            else 
                            {
                                return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                            }

                        }
                    }

                    break;

                // Number properties
                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        int intValue = int.MinValue;
                        if ( int.TryParse( values[1], out intValue ) )
                        {
                            try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                            catch { }
                            constantExpression = Expression.Constant( intValue );

                            if ( entityField.PropertyType == typeof( int? ) )
                            {
                                Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression comparisonExpression = ComparisonExpression( comparisonType, ValueExpression, constantExpression );
                                return Expression.AndAlso( hasValue, comparisonExpression );
                            }
                            else 
                            {
                                return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                            }

                        }
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
                                constantExpression = Expression.Constant( Enum.Parse( entityField.PropertyType, selectedValues[0].Replace( " ", "" ) ) );
                                Expression comparison = Expression.Equal( propertyExpression, constantExpression );

                                foreach ( string selectedValue in selectedValues.Skip( 1 ) )
                                {
                                    constantExpression = Expression.Constant( Enum.Parse( entityField.PropertyType, selectedValue.Replace( " ", "" ) ) );
                                    comparison = Expression.Or( comparison, Expression.Equal( propertyExpression, constantExpression ) );
                                }

                                return comparison;
                            }

                            else if ( entityField.DefinedTypeId.HasValue )
                            {
                                List<int> selectedIds = selectedValues.Select( v => int.Parse( v ) ).ToList();
                                constantExpression = Expression.Constant( selectedIds, typeof( List<int> ) );

                                if ( entityField.PropertyType == typeof( int? ) )
                                {
                                    Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                    Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                    MethodCallExpression containsExpression = Expression.Call( constantExpression, "Contains", new Type[] { }, ValueExpression );
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
                            constantExpression = Expression.Constant( bool.Parse( values[0] ) );
                            
                            if ( entityField.PropertyType == typeof( bool? ) )
                            {
                                Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                Expression compareExpression = ComparisonExpression( comparisonType, ValueExpression, constantExpression );
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
                        try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                        catch { }
                        constantExpression = Expression.Constant( values[1] );

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
        private Expression GetAttributeExpression( object serviceInstance, Expression parameterExpression, EntityField property, List<string> values )
        {
            IEnumerable<int> ids = null;

            ComparisonType comparisonType = ComparisonType.EqualTo;

            var service = new AttributeValueService();
            var attributeValues = service.Queryable().Where( v =>
                v.AttributeId == property.AttributeId &&
                v.EntityId.HasValue &&
                v.Value != string.Empty ).ToList();

            switch ( property.FilterFieldType )
            {
                case SystemGuid.FieldType.DATE:

                    if ( values.Count == 2 )
                    {
                        DateTime dateValue = DateTime.MinValue;
                        if ( DateTime.TryParse( values[1], out dateValue ) )
                        {
                            try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                            catch { }

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
                    }

                    break;

                case SystemGuid.FieldType.INTEGER:

                    if ( values.Count == 2 )
                    {
                        int intValue = int.MinValue;
                        if ( int.TryParse( values[1], out intValue ) )
                        {
                            try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                            catch { }

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
                    }

                    break;

                case SystemGuid.FieldType.TEXT:

                    if ( values.Count == 2 )
                    {
                        try { comparisonType = values[0].ConvertToEnum<ComparisonType>(); }
                        catch { }

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
                ConstantExpression IdsExpression = Expression.Constant( ids.AsQueryable(), typeof( IQueryable<int> ) );
                Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, IdsExpression, propertyExpression );
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
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        private Dictionary<string, List<Control>> GroupControls( Type entityType, Control[] controls )
        {
            var groupedControls = new Dictionary<string, List<Control>>();

            foreach ( var property in GetEntityFields(entityType) )
            {
                groupedControls.Add( property.Name, new List<Control>() );
                for ( int i = property.Index; i < property.Index + property.ControlCount; i++ )
                {
                    groupedControls[property.Name].Add( controls[i] );
                }
            }

            return groupedControls;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class for saving information about each property and attribute of an entity
        /// Note: the type of a field or attribute does not neccesarily determine the ui rendered for filtering.   For example, a Single-Select attribute
        /// will use a multi-select ui so that user can filter on one or more values.  The FilterFieldType property determines the UI rendered for filtering
        /// and not the type of field.
        /// 
        /// Entity Property Types and their renderd filter field type
        ///     string              ->  TEXT
        ///     bool or bool?       ->  SINGLE_SELECT
        ///     date or date?       ->  DATE
        ///     int or int?
        ///         Defined Values  ->  MULTI_SELECT
        ///         otherwise       ->  INTEGER
        ///     enumeration         ->  MULTI_SELECT
        /// 
        /// Attribute types and their rendered filter field type
        ///     MULTI_SELECT        ->  MULTI_SELECT
        ///     SINGLE_SELECT       ->  MULTI_SELECT
        ///     BOOLEAN             ->  SINGLE_SELECT (True or False)
        ///     DATE                ->  DATE
        ///     INTEGER             ->  INTEGER
        ///     TEXT                ->  TEXT
        ///     
        /// </summary>
        class EntityField
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public FieldKind FieldKind { get; set; }
            public Type PropertyType { get; set; }
            public int Index { get; set; }
            public int ControlCount { get; set; }
            public int? AttributeId { get; set; }
            public string FilterFieldType { get; set; }
            public int? DefinedTypeId { get; set; }

            public EntityField( string name, FieldKind fieldKind, Type propertyType, int controlCount, int? attributeId = null )
            {
                Name = name;
                Title = name.SplitCase();
                FieldKind = fieldKind;
                PropertyType = propertyType;
                ControlCount = controlCount;
                AttributeId = attributeId;
            }
        }

        #endregion

        #region Private Enumerations

        enum FieldKind
        {
            Property,
            Attribute,
        }

        #endregion
    }
}