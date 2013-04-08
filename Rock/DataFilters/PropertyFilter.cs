//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
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

namespace Rock.DataFilters
{
    /// <summary>
    /// Abstract class for filtering a model based on any of it's property or attribute values
    /// </summary>
    public abstract class PropertyFilter<T> : DataFilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return EntityTypeCache.Read( typeof( T ) ).FriendlyName + " Fields"; }
        }

        /// <summary>
        /// Gets the name of the filtered entity type.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public override string FilteredEntityTypeName
        {
            get { return typeof( T ).FullName; }
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

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string ClientFormatSelection
        {
            get
            {
                return _clientFormatSelection;
            }
            protected set
            {
                _clientFormatSelection = value;
            }
        }
        private string _clientFormatSelection = string.Empty;

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        private List<EntityProperty> Properties
        {
            get
            {
                if ( _properties == null )
                {
                    _properties = GetProperties();
                }
                return _properties;
            }
            set { _properties = value; }
        }
        private List<EntityProperty> _properties = null;

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            var values = JsonConvert.DeserializeObject<List<string>>( selection );

            if ( values.Count > 0 )
            {
                string selectedProperty = values[0];

                var property = Properties.Where( p => p.PropertyName == selectedProperty ).FirstOrDefault();
                if ( property != null )
                {
                    switch ( property.SystemFieldType )
                    {
                        case SystemGuid.FieldType.INTEGER:
                        case SystemGuid.FieldType.TEXT:

                            if ( values.Count == 3 )
                            {
                                ComparisonType comparisonType = ComparisonType.StartsWith;
                                try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); }
                                catch { }
                                return string.Format( "{0} {1} '{2}'", property.Title, comparisonType.ConvertToString(), values[2] );
                            }

                            break;

                        case SystemGuid.FieldType.BOOLEAN:

                            if ( values.Count == 2 )
                            {
                                return string.Format( "{0} is '{1}'", property.Title, values[1] == "1" ? "True" : "False" );
                            }

                            break;

                        case SystemGuid.FieldType.SINGLE_SELECT:

                            if ( values.Count == 2 )
                            {
                                return string.Format( "{0} is '{1}'", property.Title, values[1] );
                            }

                            break;

                        case SystemGuid.FieldType.MULTI_SELECT:

                            if ( values.Count == 2 )
                            {
                                List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[1] );
                                List<string> selectedTexts = new List<string>();

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

                                return string.Format( "{0} is {1}", property.Title,
                                    selectedTexts.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " ) );
                            }

                            break;
                    }
                }
            }

            return typeof( T ).Name.SplitCase() + " Property";

        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( FilterField filterControl )
        {
            var controls = new List<Control>();

            DropDownList ddlProperty = new DropDownList();
            ddlProperty.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            filterControl.Controls.Add( ddlProperty );
            controls.Add( ddlProperty );

            Type type = typeof( T );

            foreach ( var entityProperty in Properties )
            {
                ddlProperty.Items.Add( new ListItem( entityProperty.Title, entityProperty.PropertyName ) );

                if ( entityProperty.PropertyOrAttribute == PropOrAttribute.Property )
                {
                    var propInfo = type.GetProperty( entityProperty.PropertyName );
                    if ( propInfo != null && propInfo.PropertyType.IsEnum )
                    {
                        DropDownList ddl = new DropDownList();
                        ddl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddl );
                        controls.Add( ddl );

                        foreach ( var value in Enum.GetValues( propInfo.PropertyType ) )
                        {
                            ddl.Items.Add( new ListItem( Enum.GetName( propInfo.PropertyType, value ).SplitCase() ) );
                        }
                    }

                    else if ( propInfo.PropertyType == typeof( string ) )
                    {
                        var ddl =  ComparisonControl( StringFilterComparisonTypes );
                        ddl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddl );
                        controls.Add( ddl );

                        var tb = new TextBox();
                        tb.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( tb );
                        controls.Add( tb );
                    }

                    else if ( propInfo.PropertyType == typeof( bool ) || propInfo.PropertyType == typeof( bool? ) )
                    {
                        DropDownList ddl = new DropDownList();
                        ddl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                        filterControl.Controls.Add( ddl );
                        controls.Add( ddl );

                        ddl.Items.Add( new ListItem( "True", "1" ) );
                        ddl.Items.Add( new ListItem( "False", "0" ) );
                    }

                    else if ( propInfo.PropertyType == typeof( int ) || propInfo.PropertyType == typeof( int? ) )
                    {
                        var definedValueAttribute = propInfo.GetCustomAttributes( typeof( Rock.Data.DefinedValueAttribute ), true ).FirstOrDefault();
                        if ( definedValueAttribute != null )
                        {
                            // Defined Value Properties
                            CheckBoxList cbl = new CheckBoxList();
                            cbl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( cbl );
                            controls.Add( cbl );

                            cbl.RepeatDirection = RepeatDirection.Horizontal;

                            var definedType = DefinedTypeCache.Read( ( (Rock.Data.DefinedValueAttribute)definedValueAttribute ).DefinedTypeGuid );
                            if ( definedType != null )
                            {
                                foreach ( var definedValue in definedType.DefinedValues )
                                    cbl.Items.Add( new ListItem( definedValue.Name, definedValue.Id.ToString() ) );
                            }

                        }
                        else
                        {
                            // Numerical
                            var ddl = ComparisonControl( NumericFilterComparisonTypes );
                            ddl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( ddl );
                            controls.Add( ddl );

                            var numberBox = new NumberBox();
                            numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( numberBox );
                            controls.Add( numberBox );

                            numberBox.FieldName = entityProperty.Title;
                        }
                    }
                }

                else
                {
                    var attribute = AttributeCache.Read( entityProperty.AttributeId.Value );

                    switch ( attribute.FieldType.Guid.ToString() )
                    {
                        case SystemGuid.FieldType.BOOLEAN:
                            
                            var ddlBool = new DropDownList();
                            ddlBool.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( ddlBool );
                            controls.Add( ddlBool );

                            ddlBool.Items.Add( new ListItem( "True", "1" ) );
                            ddlBool.Items.Add( new ListItem( "False", "0" ) );

                            break;

                        case SystemGuid.FieldType.INTEGER:
                            
                            var ddlInt = ComparisonControl( NumericFilterComparisonTypes );
                            ddlInt.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( ddlInt );
                            controls.Add( ddlInt );

                            var numberBox = attribute.CreateControl() as NumberBox;
                            numberBox.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( numberBox );
                            controls.Add( numberBox );

                            numberBox.FieldName = attribute.Name;
                            
                            break;
                        
                        case SystemGuid.FieldType.TEXT:

                            var ddlText =  ComparisonControl( StringFilterComparisonTypes );
                            ddlText.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( ddlText );
                            controls.Add( ddlText );

                            var textControl = attribute.CreateControl();
                            textControl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( textControl );
                            controls.Add( textControl );

                            break;
                        
                        case SystemGuid.FieldType.MULTI_SELECT:
                        case SystemGuid.FieldType.SINGLE_SELECT:
                        
                            var selectControl = attribute.CreateControl();
                            selectControl.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
                            filterControl.Controls.Add( selectControl );
                            controls.Add( selectControl );

                            break;
                    }
                }
            }

            ClientFormatSelection = string.Format( "{0}PropertySelection( $content )", typeof( T ).Name );
            
            return controls.ToArray();        
        }


        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            string selectedProperty = ( (DropDownList)controls[0] ).SelectedValue;

            writer.AddAttribute( "class", "entity-property-selection" );
            controls[0].RenderControl( writer );
            writer.WriteBreak();

            var groupedControls = GroupControls( controls );

            StringBuilder sb = new StringBuilder();
            int i = 0;

            foreach ( var property in Properties )
            {
                var propertyControls = groupedControls[property.PropertyName];

                if ( property.PropertyName != selectedProperty )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Controls
                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                string clientFormatSelection = string.Empty;
                switch ( property.SystemFieldType )
                {
                    case SystemGuid.FieldType.INTEGER:
                    case SystemGuid.FieldType.TEXT:
                        propertyControls[0].RenderControl( writer );
                        writer.Write( " " );
                        propertyControls[1].RenderControl( writer );
                        clientFormatSelection = string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ' \\'' + $('input', $selectedContent).val() + '\\''", property.Title );
                        break;

                    case SystemGuid.FieldType.BOOLEAN:
                    case SystemGuid.FieldType.SINGLE_SELECT:
                        writer.Write( "is " );
                        propertyControls[0].RenderControl( writer );
                        clientFormatSelection = string.Format( "result = '{0} is ' + '\\'' + $('select', $selectedContent).find(':selected').text() + '\\''", property.Title );
                        break;

                    case SystemGuid.FieldType.MULTI_SELECT:
                        writer.Write( "is " );
                        propertyControls[0].RenderControl( writer );
                        clientFormatSelection = string.Format( "var selectedItems = ''; $('input:checked', $selectedContent).each(function() {{ selectedItems += selectedItems == '' ? '' : ' or '; selectedItems += '\\'' + $(this).parent().text() + '\\'' }}); result = '{0} is ' + selectedItems ", property.Title );
                        break;

                }

                if ( clientFormatSelection != string.Empty )
                {
                    sb.AppendFormat( @"
            case {0}: {1}; break;
", i, clientFormatSelection );
                }
                i++;

                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            string script = string.Format( @"
    function {0}PropertySelection($content){{
        var sIndex = $('select.entity-property-selection', $content).find(':selected').index();
        var $selectedContent = $('div.control-group', $content).eq(sIndex);
        var result = '';
        switch(sIndex) {{
            {1}
        }}
        return result;
    }}
", typeof( T ).Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), typeof( T ).Name + "-property-selection", script, true );

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
        public override string GetSelection( Control[] controls )
        {
            var values = new List<string>();

            DropDownList ddlProperty = controls[0] as DropDownList;
            if ( ddlProperty != null )
            {
                string selectedProperty = ddlProperty.SelectedValue;
                values.Add( selectedProperty );

                var groupedControls = GroupControls( controls );

                if ( groupedControls.ContainsKey( selectedProperty ) )
                {
                    foreach ( Control control in groupedControls[selectedProperty] )
                    {
                        if ( control is TextBox )
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
        public override void SetSelection( Control[] controls, string selection )
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

                    var groupedControls = GroupControls( controls );

                    if ( groupedControls.ContainsKey( selectedProperty ) )
                    {
                        for ( int i = 0; i < groupedControls[selectedProperty].Count; i++ )
                        {
                            if ( values.Count >= i + 1 )
                            {
                                Control control = groupedControls[selectedProperty][i];

                                if ( control is TextBox )
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
        public override Expression GetExpression( object serviceInstance, Expression parameterExpression, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 2 )
                {
                    string selectedProperty = values[0];

                    var property = Properties.Where( p => p.PropertyName == selectedProperty ).FirstOrDefault();
                    if ( property != null )
                    {
                        Expression trueValue = Expression.Constant( true );
                        MemberExpression propertyExpression = Expression.Property( parameterExpression, selectedProperty );
                        Expression constantExpression = null;
                        ComparisonType comparisonType = ComparisonType.EqualTo;

                        switch ( property.SystemFieldType )
                        {
                            case SystemGuid.FieldType.BOOLEAN:

                                if ( values.Count == 2 )
                                {
                                    constantExpression = Expression.Constant( values[1] == "1" );

                                    if ( property.PropertyType == typeof( bool ) )
                                    {
                                        return Expression.Equal( propertyExpression, constantExpression );
                                    }
                                    else // bool?
                                    {
                                        Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                        Expression equalExpression = Expression.Equal( hasValue, trueValue );
                                        Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                        Expression comparisonExpression = Expression.Equal( ValueExpression, constantExpression );
                                        return Expression.AndAlso( equalExpression, comparisonExpression );
                                    }
                                }

                                break;

                            case SystemGuid.FieldType.INTEGER:

                                if ( values.Count == 3 )
                                {
                                    int intValue = int.MinValue;
                                    if ( int.TryParse( values[2], out intValue ) )
                                    {
                                        try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); }
                                        catch { }
                                        constantExpression = Expression.Constant( intValue );

                                        if ( property.PropertyType == typeof( int ) )
                                        {
                                            return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                                        }
                                        else  // int?
                                        {
                                            Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                            Expression equalExpression = Expression.Equal( hasValue, trueValue );
                                            Expression ValueExpression = Expression.Property( propertyExpression, "Value" );
                                            Expression comparisonExpression = ComparisonExpression( comparisonType, ValueExpression, constantExpression );
                                            return Expression.AndAlso( equalExpression, comparisonExpression );
                                        }

                                    }
                                }

                                break;

                            case SystemGuid.FieldType.TEXT:

                                if ( values.Count == 3 )
                                {
                                    try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); }
                                    catch { }
                                    constantExpression = Expression.Constant( values[2] );

                                    return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                                }

                                break;

                            case SystemGuid.FieldType.SINGLE_SELECT:

                                if ( values.Count == 2 )
                                {
                                    if ( property.PropertyType.IsEnum )
                                    {
                                        constantExpression = Expression.Constant( Enum.Parse( property.PropertyType, values[1].Replace( " ", "" ) ) );
                                    }
                                    else
                                    {
                                        constantExpression = Expression.Constant( values[1] );
                                    }

                                    return ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                                }

                                break;

                            case SystemGuid.FieldType.MULTI_SELECT:

                                if ( values.Count == 2 )
                                {
                                    if ( property.PropertyType == typeof( int? ) )
                                    {
                                        List<string> selectedValues = JsonConvert.DeserializeObject<List<string>>( values[1] );
                                        List<int> selectedIds = selectedValues.Select( v => int.Parse( v ) ).ToList();

                                        Expression hasValue = Expression.Property( propertyExpression, "HasValue" );
                                        Expression value = Expression.Constant( true );
                                        Expression equalExpression = Expression.Equal( hasValue, value );

                                        Expression ValueExpression = Expression.Property( propertyExpression, "Value" );

                                        constantExpression = Expression.Constant( selectedIds, typeof( List<int> ) );
                                        MethodCallExpression containsExpression = Expression.Call( constantExpression, "Contains", new Type[] { }, ValueExpression );

                                        return Expression.AndAlso( equalExpression, containsExpression );
                                    }
                                }

                                break;
                        }

                    }
                }
            }

            return null;
        }

        private List<EntityProperty> GetProperties()
        {
            var properties = new List<EntityProperty>();

            // Get Properties
            foreach ( var property in typeof( T ).GetProperties() )
            {
                if ( !property.GetGetMethod().IsVirtual || property.Name == "Id" || property.Name == "Guid" || property.Name == "Order" )
                {
                    EntityProperty entityProperty = null;


                    // Enum Properties
                    if ( property.PropertyType.IsEnum )
                    {
                        entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, 1 );
                        entityProperty.SystemFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                    }

                    // Boolean properties
                    if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( bool? ) )
                    {
                        entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, 1 );
                        entityProperty.SystemFieldType = SystemGuid.FieldType.BOOLEAN;
                    }

                    // Text Properties
                    else if ( property.PropertyType == typeof( string ) )
                    {
                        entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, 2 );
                        entityProperty.SystemFieldType = SystemGuid.FieldType.TEXT;
                    }

                    // Integer Properties
                    else if ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( int? ) )
                    {
                        var definedValueAttribute = property.GetCustomAttributes( typeof( Rock.Data.DefinedValueAttribute ), true ).FirstOrDefault();

                        if ( definedValueAttribute != null )
                        {
                            // Defined Value Properties
                            entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, 1 );
                            var definedType = DefinedTypeCache.Read( ( (Rock.Data.DefinedValueAttribute)definedValueAttribute ).DefinedTypeGuid );
                            entityProperty.Title = definedType != null ? definedType.Name : property.Name.Replace( "ValueId", "" ).SplitCase();
                            entityProperty.SystemFieldType = SystemGuid.FieldType.MULTI_SELECT;
                        }
                        else
                        {
                            entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, 2 );
                            entityProperty.SystemFieldType = SystemGuid.FieldType.INTEGER;
                        }
                    }

                    if ( entityProperty != null )
                    {
                        properties.Add( entityProperty );
                    }
                }
            }

            // Get Attributes
            int entityTypeId = EntityTypeCache.Read( typeof( T ) ).Id;
            foreach ( var attribute in new AttributeService().Get( entityTypeId, string.Empty, string.Empty ) )
            {
                // Ensure prop name is unique
                string propName = attribute.Name;
                int i = 1;
                while ( properties.Any( p => p.PropertyName.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
                {
                    propName = attribute.Name + i++.ToString();
                }

                EntityProperty entityProperty = null;

                // For now only do text and single-select attributes
                switch ( attribute.FieldType.Guid.ToString() )
                {
                    case SystemGuid.FieldType.INTEGER:
                    case SystemGuid.FieldType.TEXT:
                        entityProperty = new EntityProperty( attribute.Name, PropOrAttribute.Attribute, null, 2, attribute.Id );
                        break;
                    case SystemGuid.FieldType.BOOLEAN:
                    case SystemGuid.FieldType.MULTI_SELECT:
                    case SystemGuid.FieldType.SINGLE_SELECT:
                        entityProperty = new EntityProperty( attribute.Name, PropOrAttribute.Attribute, null, 1, attribute.Id );
                        break;
                }

                if ( entityProperty != null )
                {
                    entityProperty.SystemFieldType = attribute.FieldType.Guid.ToString();
                    properties.Add( entityProperty );
                }

            }

            int index = 1;
            var orderedProperties = new List<EntityProperty>();
            foreach( var entityProperty in properties.OrderBy( p => p.Title ).ThenBy( p => p.PropertyName ))
            {
                entityProperty.Index = index;
                index += entityProperty.ControlCount;
                orderedProperties.Add(entityProperty);
            }

            return orderedProperties;
        }

        private Dictionary<string, List<Control>> GroupControls( Control[] controls )
        {
            var groupedControls = new Dictionary<string, List<Control>>();

            foreach ( var property in Properties )
            {
                groupedControls.Add( property.PropertyName, new List<Control>() );
                for ( int i = property.Index; i < property.Index + property.ControlCount; i++ )
                {
                    groupedControls[property.PropertyName].Add( controls[i] );
                }
            }

            return groupedControls;
        }

        class EntityProperty
        {
            public string PropertyName { get; set; }
            public string Title { get; set; }
            public PropOrAttribute PropertyOrAttribute { get; set; }
            public Type PropertyType { get; set; }
            public int Index { get; set; }
            public int ControlCount { get; set; }
            public int? AttributeId { get; set; }
            public string SystemFieldType { get; set; }

            public EntityProperty( string name, PropOrAttribute propertyOrAttribute, Type propertyType, int controlCount, int? attributeId = null )
            {
                PropertyName = name;
                Title = name.SplitCase();
                PropertyOrAttribute = propertyOrAttribute;
                PropertyType = propertyType;
                ControlCount = controlCount;
                AttributeId = attributeId;
            }
        }

        enum PropOrAttribute
        {
            Property,
            Attribute,
        }
    }
}