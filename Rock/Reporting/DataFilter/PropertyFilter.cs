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
    /// Filter entities on any of its property or attribute values
    /// </summary>
    [Description( "Filter entities on any of its property or attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Property Filter" )]
    public class PropertyFilter : EntityFieldFilter
    {
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
            return string.Format( "{0}PropertySelection( $content )", entityType.Name );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = entityType.Name.SplitCase() + " Property";

            // First value is the field id (property of attribute), remaining values are the field type's filter values
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count >= 1 )
            {
                // First value in array is always the name of the entity field being filtered
                string entityFieldName = values[0].Replace( " ", "" );   // Prior to v1.1 attribute.Name was used instead of attribute.Key, because of that, strip spaces to attempt matching key

                var entityFields = EntityHelper.GetEntityFields( entityType );
                var entityField = entityFields.FirstOrDefault( p => p.Name == entityFieldName );
                if ( entityField != null )
                {
                    result = entityField.FormattedFilterDescription( FixDelimination( values.Skip( 1 ).ToList() ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            containerControl = new Panel();
            containerControl.ID = string.Format( "{0}_containerControl", filterControl.ID );
            filterControl.Controls.Add( containerControl );

            // Create the field selection dropdown
            ddlEntityField = new RockDropDownList();
            ddlEntityField.ID = string.Format( "{0}_ddlProperty", filterControl.ID );
            containerControl.Controls.Add( ddlEntityField );

            // add Empty option first
            ddlEntityField.Items.Add( new ListItem() );

            this.entityFields = EntityHelper.GetEntityFields( entityType );
            foreach ( var entityField in entityFields )
            {
                ddlEntityField.Items.Add( new ListItem( entityField.Title, entityField.Name ) );
            }

            ddlEntityField.AutoPostBack = true;
            ddlEntityField.SelectedIndexChanged += ddlEntityField_SelectedIndexChanged;

            return new Control[] { containerControl };
        }

        void ddlEntityField_SelectedIndexChanged( object sender, EventArgs e )
        {
            var entityField = this.entityFields.FirstOrDefault( a => a.Name == ddlEntityField.SelectedValue );
            if ( entityField != null )
            {
                string controlId = string.Format( "{0}_{1}", containerControl.ID, entityField.Name );
                if ( !containerControl.Controls.OfType<Control>().Any( a => a.ID == controlId ) )
                {
                    var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true );
                    if ( control != null )
                    {
                        // Add the field to the dropdown of available fields
                        containerControl.Controls.Add( control );
                    }
                }
            }
        }

        private Panel containerControl = null;
        private DropDownList ddlEntityField = null;
        private List<EntityField> entityFields = null;

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            if ( controls.Length > 0 )
            {
                var container = controls[0] as Panel;
                ddlEntityField = container.Controls[0] as DropDownList;
                var entityFields = EntityHelper.GetEntityFields( entityType );
                RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlEntityField, container.Controls.OfType<Control>().ToList(), container.ID );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [hide entity field picker].
        /// </summary>
        /// <value>
        /// <c>true</c> if [hide entity field picker]; otherwise, <c>false</c>.
        /// </value>
        public void HideEntityFieldPicker()
        {
            if ( ddlEntityField != null )
            {
                ddlEntityField.Style[HtmlTextWriterStyle.Display] = "none";
            }
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

            if ( controls.Length > 0 )
            {
                var container = controls[0] as Panel;
                DropDownList ddlProperty = container.Controls[0] as DropDownList;
                ddlEntityField_SelectedIndexChanged( ddlProperty, new EventArgs() );

                var entityFields = EntityHelper.GetEntityFields( entityType );
                var entityField = entityFields.FirstOrDefault( f => f.Name == ddlProperty.SelectedValue );
                if ( entityField != null )
                {
                    var control = container.Controls.OfType<Control>().ToList().FirstOrDefault( c => c.ID.EndsWith( entityField.Name ) );
                    if ( control != null )
                    {
                        values.Add( ddlProperty.SelectedValue );
                        entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig ).ForEach( v => values.Add( v ) );
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
                var container = controls[0] as Panel;
                DropDownList ddlProperty = container.Controls[0] as DropDownList;
                ddlEntityField_SelectedIndexChanged( ddlProperty, new EventArgs() );
                var entityFields = EntityHelper.GetEntityFields( entityType );
                SetEntityFieldSelection( entityFields, ddlProperty, values, container.Controls.OfType<Control>().ToList() );
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
                    string selectedProperty = values[0].Replace( " ", "" );   // Prior to v1.1 attribute.Name was used instead of attribute.Key, because of that, strip spaces to attempt matching key

                    var entityFields = EntityHelper.GetEntityFields( entityType );
                    var entityField = entityFields.FirstOrDefault( f => f.Name == selectedProperty );
                    if ( entityField != null )
                    {
                        if ( entityField.FieldKind == FieldKind.Property )
                        {
                            return GetPropertyExpression( serviceInstance, parameterExpression, entityField, FixDelimination( values.Skip( 1 ).ToList() ) );
                        }
                        else
                        {
                            return GetAttributeExpression( serviceInstance, parameterExpression, entityField, FixDelimination( values.Skip( 1 ).ToList() ) );
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

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

            return entityField.FieldType.Field.PropertyFilterExpression( entityField.FieldConfig, values, parameterExpression, entityField.Name, entityField.PropertyType );
        }

        #endregion
    }
}