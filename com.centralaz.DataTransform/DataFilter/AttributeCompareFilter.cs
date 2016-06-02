// <copyright>
// Copyright by Central Christian Church
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Reporting.DataFilter;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.centralaz.DataTransform.DataFilter
{
    /// <summary>
    /// Filter entities on a compare of any two of its property or attribute values
    /// </summary>
    [Description( "Filter entities on a compare of any two of its attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Compare Filter" )]
    public class AttributeCompareFilter : EntityFieldFilter
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Compare " + EntityTypeCache.Read( entityType ).FriendlyName + " Attributes";
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
            return @"
function() {
    var firstName = $(""input[id$='_ddlFirstProperty']"", $content).val();
    var secondName = $(""input[id$='_ddlSecondProperty']"", $content).val();
    
    var boolValue = $(""input[id$='_ddlBool']"", $content).text() 

   return firstName  + ' ' + boolValue + ' ' + secondName;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Compare Fields";
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 3 )
            {
                var attributeService = new AttributeService( new RockContext() );
                var firstName = selectionValues[0];
                var secondName = selectionValues[1];
                var boolValue = selectionValues[2].AsBoolean();
                if ( firstName != null && secondName != null )
                {
                    result = string.Format(
                            "{0} {1} {2}",
                            firstName,
                            boolValue ? "is equal to" : "is not equal to",
                            secondName );
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
            var controls = new List<Control>();
            var entityFields = EntityHelper.GetEntityFields( entityType );

            // Create the first field selection dropdown
            var ddlFirstProperty = new RockDropDownList();
            ddlFirstProperty.ID = string.Format( "{0}_ddlFirstProperty", filterControl.ID );
            ddlFirstProperty.ClientIDMode = ClientIDMode.Predictable;

            // Create the second field selection dropdown
            var ddlSecondProperty = new RockDropDownList();
            ddlSecondProperty.ID = string.Format( "{0}_ddlSecondProperty", filterControl.ID );
            ddlSecondProperty.ClientIDMode = ClientIDMode.Predictable;

            // fill in the property dropdowns. We add the second dropdown later, but populate it here along the first to save on processing time.
            ddlFirstProperty.Items.Add( new ListItem() );
            ddlSecondProperty.Items.Add( new ListItem() );
            var rockBlock = filterControl.RockBlock();

            foreach ( var entityField in entityFields )
            {
                bool isAuthorized = false;
                if ( entityField.FieldKind == FieldKind.Attribute && entityField.AttributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Read( entityField.AttributeGuid.Value );
                    if ( attribute != null && rockBlock != null )
                    {
                        // only show the Attribute field in the drop down if they have VIEW Auth to it
                        isAuthorized = attribute.IsAuthorized( Rock.Security.Authorization.VIEW, rockBlock.CurrentPerson );
                    }
                }

                if ( isAuthorized )
                {
                    ddlFirstProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );
                    ddlSecondProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );
                }
            }

            filterControl.Controls.Add( ddlFirstProperty );
            controls.Add( ddlFirstProperty );

            // Add boolean dropdown.
            var ddlBool = new RockDropDownList();
            ddlBool.ID = string.Format( "{0}_ddlBool", filterControl.ID );
            ddlBool.AddCssClass( "js-filter-control" );

            ddlBool.Items.Clear();

            // add blank item as first item
            ddlBool.Items.Add( new ListItem() );

            ddlBool.Items.Add( new ListItem( "is equal to", "True" ) );
            ddlBool.Items.Add( new ListItem( "is not equal to", "False" ) );

            filterControl.Controls.Add( ddlBool );
            controls.Add( ddlBool );

            // Add second property dropdown
            filterControl.Controls.Add( ddlSecondProperty );
            controls.Add( ddlSecondProperty );

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
            var ddlFirstProperty = controls[0] as DropDownList;
            var ddlSecondProperty = controls[2] as DropDownList;
            var ddlBool = controls[1] as DropDownList;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlFirstProperty.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlBool.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlSecondProperty.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var firstName = ( controls[0] as DropDownList ).SelectedValue;
            var secondName = ( controls[2] as DropDownList ).SelectedValue;
            var boolValue = ( controls[1] as DropDownList ).SelectedValue.AsBoolean();

            return string.Format( "{0}|{1}|{2}", firstName, secondName, boolValue );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var entityFields = EntityHelper.GetEntityFields( entityType );

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 3 )
            {
                var ddlFirstProperty = controls[0] as DropDownList;
                var ddlSecondProperty = controls[2] as DropDownList;
                var ddlBool = controls[1] as DropDownList;

                var firstName = selectionValues[0];
                var secondName = selectionValues[1];
                var boolValue = selectionValues[2].AsBoolean();

                ddlFirstProperty.SelectedValue = firstName;
                ddlSecondProperty.SelectedValue = secondName;
                ddlBool.SelectedValue = boolValue.ToTrueFalse();
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var rockContext = (RockContext)serviceInstance.Context;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 3 )
            {
                var entityFields = EntityHelper.GetEntityFields( entityType );
                var firstEntityField = entityFields.FirstOrDefault( f => f.Name.Equals( selectionValues[0] ) );
                var secondEntityField = entityFields.FirstOrDefault( f => f.Name.Equals( selectionValues[1] ) );
                var boolValue = selectionValues[2].AsBoolean();

                if ( firstEntityField != null && secondEntityField != null )
                {
                    var attributeValueService = new AttributeValueService( rockContext );

                    var firstQry = attributeValueService.Queryable()
                        .Where( av => av.Attribute.Guid == firstEntityField.AttributeGuid.Value );

                    var joinedQry = attributeValueService.Queryable()
                        .Where( av => av.Attribute.Guid == secondEntityField.AttributeGuid.Value )
                        .Join( firstQry, first => first.EntityId, second => second.EntityId, ( first, second ) => new { first, second } );

                    if ( boolValue )
                    {
                        joinedQry = joinedQry.Where( j => j.first.Value == j.second.Value );
                    }
                    else
                    {
                        joinedQry = joinedQry.Where( j => j.first.Value != j.second.Value );
                    }

                    IQueryable<int> entityIdQry = joinedQry.Select( j => j.first.EntityId.Value );

                    //----------------------------------------------------------------------------

                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "Id" );
                    ConstantExpression idsExpression = Expression.Constant( entityIdQry.AsQueryable(), typeof( IQueryable<int> ) );
                    Expression expression = Expression.Call( typeof( Queryable ), "Contains", new Type[] { typeof( int ) }, idsExpression, propertyExpression );

                    // If we have used an inverted comparison type for the evaluation, invert the Expression so that it excludes the matching Entities.

                    return expression;

                }
            }

            return null;
        }

        #endregion
    }
}