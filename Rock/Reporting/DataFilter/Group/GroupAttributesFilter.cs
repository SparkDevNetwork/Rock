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

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// Filter group on any of its attribute values
    /// </summary>
    [Description( "Filter group on its attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Attributes Filter" )]
    public class GroupAttributesFilter : EntityFieldFilter
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
            get { return typeof( Rock.Model.Group ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
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
            return "Group Attribute Values";
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
            return "GroupPropertySelection( $content )";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Group Property";

            // First value is group type, second value is attribute, remaining values are the field type's filter values
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count >= 2 )
            {
                var groupType = GroupTypeCache.Read( values[0].AsGuid() );
                if ( groupType != null )
                {
                    var entityFields = GetGroupAttributes( groupType.Id );
                    var entityField = entityFields.FirstOrDefault( f => f.Name == values[1] );
                    if ( entityField != null )
                    {
                        result = entityField.FormattedFilterDescription( values.Skip( 2 ).ToList() );
                    }
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
            Panel pnlGroupAttributeFilterControls = new Panel();
            pnlGroupAttributeFilterControls.ViewStateMode = ViewStateMode.Disabled;
            pnlGroupAttributeFilterControls.ID = filterControl.ID + "_pnlGroupAttributeFilterControls";
            filterControl.Controls.Add( pnlGroupAttributeFilterControls );

            GroupTypePicker groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.ID + "_groupTypePicker";
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            pnlGroupAttributeFilterControls.Controls.Add( groupTypePicker );

            // set the GroupTypePicker selected value now so we can create the other controls the depending on know the groupTypeid
            int? groupTypeId = filterControl.Page.Request.Params[groupTypePicker.UniqueID].AsIntegerOrNull();
            groupTypePicker.SelectedGroupTypeId = groupTypeId;
            groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );

            return new Control[] { pnlGroupAttributeFilterControls };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            GroupTypePicker groupTypePicker = sender as GroupTypePicker;
            Panel pnlGroupAttributeFilterControls = groupTypePicker.Parent as Panel;

            pnlGroupAttributeFilterControls.Controls.Clear();
            pnlGroupAttributeFilterControls.Controls.Add( groupTypePicker );

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = string.Format( "{0}_ddlProperty", pnlGroupAttributeFilterControls.ID );
            pnlGroupAttributeFilterControls.Controls.Add( ddlProperty );

            // add Empty option first
            ddlProperty.Items.Add( new ListItem() );

            foreach ( var entityField in GetGroupAttributes( groupTypePicker.SelectedGroupTypeId ) )
            {
                string controlId = string.Format( "{0}_{1}", pnlGroupAttributeFilterControls.ID, entityField.Name );
                var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true, FilterMode.AdvancedFilter );
                if ( control != null )
                {
                    // Add the field to the dropdown of available fields
                    ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );
                    pnlGroupAttributeFilterControls.Controls.Add( control );
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
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls, FilterMode filterMode )
        {
            if ( controls.Length > 0 )
            {
                Panel pnlGroupAttributeFilterControls = controls[0] as Panel;
                if ( pnlGroupAttributeFilterControls.Controls.Count >= 2 )
                {
                    GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;
                    groupTypePicker.RenderControl( writer );

                    DropDownList ddlEntityField = pnlGroupAttributeFilterControls.Controls[1] as DropDownList;
                    var entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );

                    var panelControls = new List<Control>();
                    panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );

                    RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlEntityField, panelControls, pnlGroupAttributeFilterControls.ID, filterMode );
                }
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
                Panel pnlGroupAttributeFilterControls = controls[0] as Panel;
                if ( pnlGroupAttributeFilterControls.Controls.Count >= 1 )
                {
                    // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
                    GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;
                    Guid groupTypeGuid = Guid.Empty;
                    var groupType = GroupTypeCache.Read( groupTypePicker.SelectedGroupTypeId ?? 0 );
                    if ( groupType != null )
                    {
                        if ( pnlGroupAttributeFilterControls.Controls.Count == 1 )
                        {
                            groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );
                        }

                        if ( pnlGroupAttributeFilterControls.Controls.Count > 1 )
                        {
                            DropDownList ddlProperty = pnlGroupAttributeFilterControls.Controls[1] as DropDownList;

                            var entityFields = GetGroupAttributes( groupType.Id );
                            var entityField = entityFields.FirstOrDefault( f => f.Name == ddlProperty.SelectedValue );
                            if ( entityField != null )
                            {
                                var panelControls = new List<Control>();
                                panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );

                                var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( entityField.Name ) );
                                if ( control != null )
                                {
                                    values.Add( groupType.Guid.ToString() );
                                    values.Add( ddlProperty.SelectedValue );
                                    entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig ).ForEach( v => values.Add( v ) );
                                }
                            }
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
                if ( controls.Length > 0 && values.Count > 0 )
                {
                    var groupType = GroupTypeCache.Read( values[0].AsGuid() );
                    if ( groupType != null )
                    {
                        Panel pnlGroupAttributeFilterControls = controls[0] as Panel;
                        if ( pnlGroupAttributeFilterControls.Controls.Count > 0 )
                        {
                            GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;
                            groupTypePicker.SelectedGroupTypeId = groupType.Id;
                            groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );
                        }

                        if ( pnlGroupAttributeFilterControls.Controls.Count > 1 && values.Count > 1 )
                        {
                            DropDownList ddlProperty = pnlGroupAttributeFilterControls.Controls[1] as DropDownList;
                            var entityFields = GetGroupAttributes( groupType.Id );

                            var panelControls = new List<Control>();
                            panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );
                            SetEntityFieldSelection( entityFields, ddlProperty, values.Skip( 1 ).ToList(), panelControls );
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

                if ( values.Count >= 3 )
                {
                    var groupType = GroupTypeCache.Read( values[0].AsGuid() );
                    if ( groupType != null )
                    {
                        string selectedProperty = values[1];

                        var entityFields = GetGroupAttributes( groupType.Id );
                        var entityField = entityFields.FirstOrDefault( f => f.Name == selectedProperty );
                        if ( entityField != null )
                        {
                            return GetAttributeExpression( serviceInstance, parameterExpression, entityField, values.Skip( 2 ).ToList() );
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
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private List<EntityField> GetGroupAttributes( int? groupTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();

            if ( groupTypeId.HasValue )
            {
                var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId.Value };
                Rock.Attribute.Helper.LoadAttributes( fakeGroup );
                foreach ( var attribute in fakeGroup.Attributes.Select( a => a.Value ) )
                {
                    EntityHelper.AddEntityFieldForAttribute( entityAttributeFields, attribute );
                }
            }

            int index = 0;
            var sortedFields = new List<EntityField>();
            foreach ( var entityProperty in entityAttributeFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityProperty.Index = index;
                index++;
                sortedFields.Add( entityProperty );
            }

            return sortedFields;
        }

        #endregion
    }
}