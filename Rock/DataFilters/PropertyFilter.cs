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
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.DataFilters
{
    /// <summary>
    /// 
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
            get { return EntityTypeCache.Read(typeof(T)).FriendlyName +  " Property"; }
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

                var property = Properties.Where( p => p.Name == selectedProperty ).FirstOrDefault();
                if ( property != null )
                {
                    switch ( property.SystemFieldType )
                    {
                        case SystemGuid.FieldType.TEXT:

                            if ( values.Count == 3 )
                            {
                                ComparisonType comparisonType = ComparisonType.StartsWith;
                                try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); } catch { }
                                return string.Format( "{0} {1} '{2}'", selectedProperty.SplitCase(), comparisonType.ConvertToString(), values[2] );
                            }
                            
                            break;

                        case SystemGuid.FieldType.SINGLE_SELECT:

                            if ( values.Count == 2 )
                            {
                                return string.Format( "{0} is '{1}'", selectedProperty.SplitCase(), values[1] );
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
            var controls = new Control[Properties.Sum( p => p.ControlCount ) + 1];

            DropDownList ddlProperty = new DropDownList();
            controls[0] = ddlProperty;

            Type type = typeof( T );

            int i = 1;
            foreach ( var entityProperty in Properties )
            {
                ddlProperty.Items.Add( new ListItem( entityProperty.Name.SplitCase(), entityProperty.Name ) );

                if ( entityProperty.PropertyOrAttribute == PropOrAttribute.Property )
                {
                    var propInfo = type.GetProperty( entityProperty.Name );
                    if ( propInfo != null && propInfo.PropertyType.IsEnum )
                    {
                        DropDownList ddl = new DropDownList();
                        foreach ( var value in Enum.GetValues( propInfo.PropertyType ) )
                        {
                            ddl.Items.Add( new ListItem( Enum.GetName( propInfo.PropertyType, value ).SplitCase() ) );
                        }
                        controls[i++] = ddl;
                    }
                    else
                    {
                        controls[i++] = ComparisonControl( StringFilterComparisonTypes );
                        controls[i++] = new TextBox();
                    }
                }

                else
                {
                    var attribute = AttributeCache.Read( entityProperty.AttributeId.Value );

                    switch ( attribute.FieldType.Guid.ToString() )
                    {
                        case SystemGuid.FieldType.TEXT:
                            controls[i++] = ComparisonControl( StringFilterComparisonTypes );
                            controls[i++] = attribute.CreateControl();
                            break;
                        case SystemGuid.FieldType.SINGLE_SELECT:
                            controls[i++] = attribute.CreateControl();
                            break;
                    }
                }
            }

            ClientFormatSelection = string.Format( "{0}PropertySelection( $content )", typeof(T).Name );
            return controls;
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
                string clientFormatSelection = string.Empty;
                switch ( property.SystemFieldType )
                {
                    case SystemGuid.FieldType.TEXT:
                        clientFormatSelection = string.Format( "'{0} ' + $('select', $selectedContent).find(':selected').text() + ' \\'' + $('input', $selectedContent).val() + '\\''", property.Name.SplitCase() );
                        break;
                    case SystemGuid.FieldType.SINGLE_SELECT:
                        clientFormatSelection = string.Format( "'{0} is ' + '\\'' + $('select', $selectedContent).find(':selected').text() + '\\''", property.Name.SplitCase() );
                        break;
                }

                if ( clientFormatSelection != string.Empty )
                {
                    sb.AppendFormat( @"
            case {0}: result = {1}; break;
", i, clientFormatSelection );
                }
                i++;

                if ( property.Name != selectedProperty )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Label
                writer.AddAttribute( "class", "control-label" );
                writer.RenderBeginTag( HtmlTextWriterTag.Label );
                writer.Write( property.Name.SplitCase() );
                writer.RenderEndTag();

                // Controls
                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                foreach ( Control control in groupedControls[property.Name] )
                {
                    control.RenderControl( writer );
                    writer.Write( " " );
                }

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
", typeof(T).Name, sb.ToString() );
            ScriptManager.RegisterStartupScript( filterControl, typeof( FilterField ), typeof(T).Name + "-property-selection", script, true );

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

                if (values.Count > 0)
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
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 2 )
                {
                    string selectedProperty = values[0];

                    var property = Properties.Where( p => p.Name == selectedProperty ).FirstOrDefault();
                    if ( property != null )
                    {
                        MemberExpression expr = Expression.Property( parameterExpression, selectedProperty );
                        ComparisonType comparisonType = ComparisonType.EqualTo;
                        Expression constant = null;

                        switch ( property.SystemFieldType )
                        {
                            case SystemGuid.FieldType.TEXT:

                                if ( values.Count == 3 )
                                {
                                    try { comparisonType = values[1].ConvertToEnum<ComparisonType>(); }
                                    catch { }
                                    constant = Expression.Constant( values[2] );
                                }

                                break;

                            case SystemGuid.FieldType.SINGLE_SELECT:

                                if ( values.Count == 2 )
                                {
                                    if ( property.PropertyType.IsEnum )
                                    {
                                        constant = Expression.Constant( Enum.Parse( property.PropertyType, values[1].Replace( " ", "" ) ) );
                                    }
                                    else
                                    {
                                        constant = Expression.Constant( values[1] );
                                    }
                                }

                                break;
                        }

                        if ( constant != null )
                        {
                            return ComparisonExpression( comparisonType, expr, constant );
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

                    // For now, only do enums and string properties
                    if ( property.PropertyType.IsEnum )
                    {
                        entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, properties.Sum( p => p.ControlCount ) + 1, 1 );
                        entityProperty.SystemFieldType = SystemGuid.FieldType.SINGLE_SELECT;
                    }
                    else if ( property.PropertyType == typeof( string ) )
                    {
                        entityProperty = new EntityProperty( property.Name, PropOrAttribute.Property, property.PropertyType, properties.Sum( p => p.ControlCount ) + 1, 2 );
                        entityProperty.SystemFieldType = SystemGuid.FieldType.TEXT;
                    }

                    if (entityProperty != null)
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
                while ( properties.Any( p => p.Name.Equals( propName, StringComparison.CurrentCultureIgnoreCase ) ) )
                {
                    propName = attribute.Name + i++.ToString();
                }

                EntityProperty entityProperty = null;

                // For now only do text and single-select attributes
                switch ( attribute.FieldType.Guid.ToString() )
                {
                    case SystemGuid.FieldType.TEXT:
                        entityProperty = new EntityProperty( attribute.Name, PropOrAttribute.Attribute, null, properties.Sum( p => p.ControlCount ) + 1, 2, attribute.Id );
                        break;
                    case SystemGuid.FieldType.SINGLE_SELECT:
                        entityProperty = new EntityProperty( attribute.Name, PropOrAttribute.Attribute, null, properties.Sum( p => p.ControlCount ) + 1, 1, attribute.Id );
                        break;
                }

                if (entityProperty != null)
                {
                    entityProperty.SystemFieldType = attribute.FieldType.Guid.ToString();
                    properties.Add( entityProperty );
                }

            }

            return properties;
        }

        private Dictionary<string, List<Control>> GroupControls( Control[] controls )
        {
            var groupedControls = new Dictionary<string, List<Control>>();

            foreach ( var property in Properties )
            {
                groupedControls.Add( property.Name, new List<Control>() );
                for ( int i = property.Index; i < property.Index + property.ControlCount; i++ )
                {
                    groupedControls[property.Name].Add( controls[i] );
                }
            }

            return groupedControls;
        }

        class EntityProperty
        {
            public string Name { get; set; }
            public PropOrAttribute PropertyOrAttribute { get; set; }
            public Type PropertyType { get; set; }
            public int Index { get; set; }
            public int ControlCount { get; set; }
            public int? AttributeId { get; set; }
            public string SystemFieldType { get; set; }

            public EntityProperty( string name, PropOrAttribute propertyOrAttribute, Type propertyType, int index, int controlCount, int? attributeId = null )
            {
                Name = name;
                PropertyOrAttribute = propertyOrAttribute;
                PropertyType = propertyType;
                Index = index;
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