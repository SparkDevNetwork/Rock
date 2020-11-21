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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
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
            return EntityTypeCache.Get( entityType ).FriendlyName + " Fields";
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
            List<string> values;

            // First value is the field id (property of attribute), remaining values are the field type's filter values
            try
            {
                values = JsonConvert.DeserializeObject<List<string>>( selection );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                return "Error";
            }

            if ( values.Count >= 1 )
            {
                // First value in array is always the name of the entity field being filtered
                string fieldSelection = values[0];

                var entityField = EntityHelper.FindFromFilterSelection( entityType, fieldSelection );

                if ( entityField != null )
                {
                    result = entityField.FormattedFilterDescription( FixDelimination( values.Skip( 1 ).ToList() ) );
                }
                else
                {
                    result = $"Unknown Property: {fieldSelection}";
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the selection label.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public string GetSelectionLabel( Type entityType, string selection )
        {
            List<string> values;

            // First value is the field id (property of attribute), remaining values are the field type's filter values
            try
            {
                values = JsonConvert.DeserializeObject<List<string>>( selection );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                return "Error";
            }

            if ( values.Count >= 1 )
            {
                // First value in array is always the name of the entity field being filtered
                string fieldSelection = values[0];

                var entityField = EntityHelper.FindFromFilterSelection( entityType, fieldSelection );
                if ( entityField != null )
                {
                    return entityField.TitleWithoutQualifier;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            var containerControl = new DynamicControlsPanel();
            containerControl.ID = string.Format( "{0}_containerControl", filterControl.ID );
            containerControl.CssClass = "js-container-control";
            filterControl.Controls.Add( containerControl );

            // Create the field selection dropdown
            var ddlEntityField = new RockDropDownList();
            ddlEntityField.ID = string.Format( "{0}_ddlProperty", filterControl.ID );
            ddlEntityField.ClientIDMode = ClientIDMode.Predictable;

            var entityTypeCache = EntityTypeCache.Get( entityType, true );
            ddlEntityField.Attributes["EntityTypeId"] = entityTypeCache?.Id.ToString();

            containerControl.Controls.Add( ddlEntityField );

            // add Empty option first
            ddlEntityField.Items.Add( new ListItem() );
            var rockBlock = filterControl.RockBlock();


            var entityFields = EntityHelper.GetEntityFields( entityType );
            foreach ( var entityField in entityFields.OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.FieldKind != FieldKind.Property ).ThenBy( a => a.Title ) )
            {
                bool isAuthorized = true;
                bool includeField = true;
                if ( entityField.FieldKind == FieldKind.Attribute && entityField.AttributeGuid.HasValue )
                {
                    if ( entityType == typeof( Rock.Model.Workflow ) && !string.IsNullOrWhiteSpace( entityField.AttributeEntityTypeQualifierName ) )
                    {
                        // Workflows can contain tons of Qualified Attributes, so let the WorkflowAttributeFilter take care of those
                        includeField = false;
                    }

                    var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );

                    // Don't include the attribute if it isn't active
                    if ( attribute.IsActive == false )
                    {
                        includeField = false;
                    }

                    if ( includeField && attribute != null && rockBlock != null )
                    {
                        // only show the Attribute field in the drop down if they have VIEW Auth to it
                        isAuthorized = attribute.IsAuthorized( Rock.Security.Authorization.VIEW, rockBlock.CurrentPerson );
                    }
                }

                if ( isAuthorized && includeField )
                {
                    var listItem = new ListItem( entityField.Title, entityField.UniqueName );

                    if ( entityField.IsPreviewable )
                    {
                        listItem.Attributes["optiongroup"] = "Common";
                    }
                    else if ( entityField.FieldKind == FieldKind.Attribute )
                    {
                        listItem.Attributes["optiongroup"] = string.Format( "{0} Attributes", entityType.Name );
                    }
                    else
                    {
                        listItem.Attributes["optiongroup"] = string.Format( "{0} Fields", entityType.Name );
                    }

                    ddlEntityField.Items.Add( listItem );
                }
            }

            ddlEntityField.AutoPostBack = true;

            // grab the currently selected value off of the request params since we are creating the controls after the Page Init
            var selectedValue = ddlEntityField.Page.Request.Params[ddlEntityField.UniqueID];
            if ( selectedValue != null )
            {
                ddlEntityField.SelectedValue = selectedValue;
                ddlEntityField_SelectedIndexChanged( ddlEntityField, new EventArgs() );
            }

            ddlEntityField.SelectedIndexChanged += ddlEntityField_SelectedIndexChanged;

            return new Control[] { containerControl };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlEntityField_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlEntityField = sender as RockDropDownList;
            var containerControl = ddlEntityField.FirstParentControlOfType<DynamicControlsPanel>();
            FilterField filterControl = ddlEntityField.FirstParentControlOfType<FilterField>();

            int? entityTypeId = ddlEntityField.Attributes["EntityTypeId"]?.AsIntegerOrNull();
            if ( !entityTypeId.HasValue )
            {
                // shouldn't happen;
                return;
            }

            var entityType = EntityTypeCache.Get( entityTypeId.Value ).GetEntityType();

            var entityFields = EntityHelper.GetEntityFields( entityType );

            var entityField = entityFields.FirstOrDefault( a => a.UniqueName == ddlEntityField.SelectedValue );
            if ( entityField != null )
            {
                string controlId = string.Format( "{0}_{1}", containerControl.ID, entityField.UniqueName );
                if ( !containerControl.Controls.OfType<Control>().Any( a => a.ID == controlId ) )
                {
                    var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true, filterControl.FilterMode );
                    if ( control != null )
                    {
                        // Add the filter controls of the selected field
                        containerControl.Controls.Add( control );
                    }
                }
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="filterMode"></param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls, FilterMode filterMode )
        {
            if ( controls.Length > 0 )
            {
                var containerControl = controls[0] as DynamicControlsPanel;

                var ddlEntityField = containerControl.Controls[0] as DropDownList;
                var entityFields = EntityHelper.GetEntityFields( entityType );
                RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlEntityField, containerControl.Controls.OfType<Control>().ToList(), containerControl.ID, filterMode );
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls, FilterMode filterMode )
        {
            var values = new List<string>();

            if ( controls.Length > 0 )
            {
                var containerControl = controls[0] as DynamicControlsPanel;

                DropDownList ddlProperty = containerControl.Controls[0] as DropDownList;
                ddlEntityField_SelectedIndexChanged( ddlProperty, new EventArgs() );

                var uniqueName = ddlProperty.SelectedValue;
                var entityField = EntityHelper.GetEntityField( entityType, uniqueName );
                if ( entityField != null )
                {
                    var control = containerControl.Controls.OfType<Control>().ToList().FirstOrDefault( c => c.ID.EndsWith( "_" + entityField.UniqueName ) );
                    if ( control != null )
                    {
                        values.Add( ddlProperty.SelectedValue );
                        entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, filterMode ).ForEach( v => values.Add( v ) );
                    }
                }
            }

            string result = values.ToJson();
            return result;
        }

        /// <summary>
        /// Optional: The Entity that should be used when determining which PropertyFields and Attributes to show (instead of just basing it off of EntityType)
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [RockObsolete( "1.12" )]
        [Obsolete( "Not Supported. Could cause inconsistent results." )]
        public IEntity Entity
        {
            get
            {
                if ( HttpContext.Current != null )
                {
                    return HttpContext.Current.Items[$"{this.GetType().FullName}:Entity"] as IEntity;
                }

                return _nonHttpContextEntity;
            }

            set
            {
                if ( HttpContext.Current != null )
                {
                    HttpContext.Current.Items[$"{this.GetType().FullName}:Entity"] = value;
                }
                else
                {
                    _nonHttpContextEntity = value;
                }
            }
        }

        /// <summary>
        /// Thread safe storage of property when HttpContext.Current is null
        /// NOTE: ThreadStatic is per thread, but ASP.NET threads are ThreadPool threads, so they will be used again.
        /// see https://www.hanselman.com/blog/ATaleOfTwoTechniquesTheThreadStaticAttributeAndSystemWebHttpContextCurrentItems.aspx
        /// So be careful and only use the [ThreadStatic] trick if absolutely necessary
        /// </summary>
        [ThreadStatic]
        [RockObsolete( "1.12" )]
        [Obsolete( "Not Supported. Could cause inconsistent results." )]
        private static IEntity _nonHttpContextEntity;

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        /// <param name="filterMode"></param>
        public override void SetSelection( Type entityType, Control[] controls, string selection, FilterMode filterMode )
        {
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );
                var containerControl = controls[0] as DynamicControlsPanel;
                var ddlEntityField = containerControl.Controls[0] as DropDownList;

                var entityFields = EntityHelper.GetEntityFields( entityType );

                // set the selected Field, but not the filter values yet
                var entityFieldControls = containerControl.Controls.OfType<Control>().ToList();
                SetEntityFieldSelection( entityFields, ddlEntityField, values, entityFieldControls, false );

                // build the Field specific filter controls
                ddlEntityField_SelectedIndexChanged( ddlEntityField, new EventArgs() );

                // update the list of entityFieldControls since ddlEntityField_SelectedIndexChanged added more
                entityFieldControls = containerControl.Controls.OfType<Control>().ToList();

                // set the selected Field and filter values
                SetEntityFieldSelection( entityFields, ddlEntityField, values, entityFieldControls, true );
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
            /* 2020-08-17 MDP
               'selection' should be a deserialized List<string> where list parts are
               [0] - PropertyName or Attribute to filter on
               [1] - Comparison Type
               [2+] - Parts that the GetPropertyExpression or GetAttributeExpression interpret
             */


            /* 2020-08-17 MDP
            * If it isn't fully configured we will ignore it and won't filter. We can detect if the filter isn't configured if..
            * 
            *   1) There isn't a selection specified (null or whitespace)
            *   2) Deserialized Selection (filterValues) is an empty list
            *   3) There are less than 2 items in the filterValues (we need a PropertyName/Attribute, and a comparison type)
            *   4) A comparisontype isn't specified (null, "" or "0" means not specified) 
            *   5) A property/attribute is not specified
            *   
            *   If we have any of the above cases, we'll return Expression.Const(true), which means we won't filter on this)
            *   
            * An exception is returned if
            *   1) A Property is specified, but the Property doesn't exist
            *   2) An attribute is specified, but the Attribute doesn't exist
            *   3) A Property is specified, but the property is [HideFromReporting]
            */

            if ( string.IsNullOrWhiteSpace( selection ) )
            {
                // if the Property Filter hasn't been configured, don't use it to filter the results.
                return Expression.Constant( true );
            }


            var filterValues = JsonConvert.DeserializeObject<List<string>>( selection );

            if ( !filterValues.Any() )
            {
                // if the Property/Attribute Filter hasn't been configured, don't use it to filter the results.
                return Expression.Constant( true );
            }

            if ( filterValues.Count < 2 )
            {
                // a Property/Attribute filter needs at least 2 parameters. If it doesn't, don't filter
                return Expression.Constant( true );
            }

            string selectedPropertyOrAttribute = filterValues[0];
            if ( selectedPropertyOrAttribute.IsNullOrWhiteSpace() )
            {
                // A property/attribute is not specified
                return Expression.Constant( true );
            }

            var entityField = EntityHelper.FindFromFilterSelection( entityType, selectedPropertyOrAttribute );
            if ( entityField != null )
            {
                /* 2020-08-17 MDP
                  filterValues[0] is the PropertyName or Attribute, we used that to determine which property or attribute to filter on.
                  filterValues[1]+ are the filterValues that GetPropertyExpression or GetAttributeExpression use, so we do a Skip(1) when passing those
                */

                if ( entityField.FieldKind == FieldKind.Property )
                {
                    return GetPropertyExpression( serviceInstance, parameterExpression, entityField, FixDelimination( filterValues.Skip( 1 ).ToList() ) );
                }
                else
                {
                    return GetAttributeExpression( serviceInstance, parameterExpression, entityField, FixDelimination( filterValues.Skip( 1 ).ToList() ) );
                }
            }
            else
            {
                /* 2020-08-17 MDP
                 Pre-v12 would ignore this situation. However, ignoring this will return incorrect results. So, v12+ will throw an exception instead.
                 Here is an example where the Pre-v12 behavior would return incorrect results
                   -- A 'FavoriteColor' attribute exists on Person, and they configure a DataView to filter on 'FavoriteColor is Blue'. DataView returns only people whose favorite color is blue
                   -- Somebody removes the 'FavoriteColor' attribute
                   -- Pre-v12 would return everybody since the FavoriteColor attribute no longer exists
                   -- v12 will throw an exception to prevent unexpected results after an attribute is deleted.
                 */


                string formattedSelection;

                try
                {
                    formattedSelection = this.FormatSelection( entityType, selection );
                }
                catch
                {
                    formattedSelection = "Property";
                }

                throw new RockDataViewFilterExpressionException( $"{formattedSelection} filter refers to a property or attribute that doesn't exist. Selection: {selection}" );
            }
        }

        /// <summary>
        /// Builds an expression for a property field
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="entityField">The property.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public Expression GetPropertyExpression( IService serviceInstance, ParameterExpression parameterExpression, EntityField entityField, List<string> values )
        {
            Expression trueValue = Expression.Constant( true );
            MemberExpression propertyExpression = Expression.Property( parameterExpression, entityField.Name );

            return entityField.FieldType.Field.PropertyFilterExpression( entityField.FieldConfig, values, parameterExpression, entityField.Name, entityField.PropertyType );
        }

        #endregion
    }
}