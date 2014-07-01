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
    /// Filter group on any of it's attribute values
    /// </summary>
    [Description( "Filter group on it's attribute values" )]
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
            return string.Format( "{0}PropertySelection( $content )", "Group" );
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
            List<string> selectionValues = JsonConvert.DeserializeObject<List<string>>( selection );
            List<string> propertyFilterValues = new List<string>();
            propertyFilterValues.AddRange( selectionValues );
            propertyFilterValues.RemoveAt( 0 );
            int? groupTypeId = null;

            if ( selectionValues.Count > 1 )
            {
                groupTypeId = GetGroupTypeIdFromSelection( selectionValues );
            }

            string entityFieldName = propertyFilterValues[0];

            if ( propertyFilterValues.Count > 0 )
            {
                var entityField = GetGroupAttributes( groupTypeId ).Where( a => a.Name == entityFieldName ).FirstOrDefault();
                string entityFieldResult = GetEntityFieldFormatSelection( propertyFilterValues, entityField );

                result = entityFieldResult ?? result;
            }

            return result;
        }

        /// <summary>
        /// Gets the group type identifier from selection.
        /// </summary>
        /// <param name="selectionValues">The selection values.</param>
        /// <returns></returns>
        private static int? GetGroupTypeIdFromSelection( List<string> selectionValues )
        {
            Guid groupTypeGuid = selectionValues[0].AsGuid();
            
            var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeGuid );
            if ( groupType != null )
            {
                return groupType.Id;
            }
            
            return null;
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

            var entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );

            foreach ( var entityField in entityFields )
            {
                // Add the field to the dropdown of available fields
                ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.Name ) );

                var dummyControls = new List<Control>();

                AddFieldTypeControls( pnlGroupAttributeFilterControls, dummyControls, entityField );
            }
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
            Panel pnlGroupAttributeFilterControls = controls[0] as Panel;
            GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;
            groupTypePicker.RenderControl( writer );

            if ( pnlGroupAttributeFilterControls.Controls.Count < 2 )
            {
                return;
            }

            List<EntityField> entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );
            var groupedControls = GroupControls( entityType, pnlGroupAttributeFilterControls.Controls.OfType<Control>().ToArray() );
            DropDownList ddlEntityField = pnlGroupAttributeFilterControls.Controls[1] as DropDownList;
            string selectedEntityField = ddlEntityField.SelectedValue;

            RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, groupedControls, ddlEntityField );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            Panel pnlGroupAttributeFilterControls = controls[0] as Panel;
            var values = new List<string>();

            // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
            GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;

            int? groupTypeId = groupTypePicker.SelectedGroupTypeId;
            Guid groupTypeGuid = Guid.Empty;
            var groupType = new GroupTypeService(new RockContext()).Get( groupTypeId ?? 0 );
            if (groupType != null)
            {
                groupTypeGuid = groupType.Guid;
            }

            values.Add( groupTypeGuid.ToString() );

            if ( pnlGroupAttributeFilterControls.Controls.Count == 1 )
            {
                groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );
            }

            if ( pnlGroupAttributeFilterControls.Controls.Count > 1 )
            {
                var groupedControls = GroupControls( entityType, pnlGroupAttributeFilterControls.Controls.OfType<Control>().ToArray() );
                DropDownList ddlProperty = pnlGroupAttributeFilterControls.Controls[1] as DropDownList;

                GetSelectionValuesForProperty( ddlProperty.SelectedValue, groupedControls, values );
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
                var selectionValues = JsonConvert.DeserializeObject<List<string>>( selection );

                List<string> propertyFilterValues = new List<string>();
                propertyFilterValues.AddRange( selectionValues );
                propertyFilterValues.RemoveAt( 0 );
                Panel pnlGroupAttributeFilterControls = null;

                if ( selectionValues.Count > 0 )
                {
                    pnlGroupAttributeFilterControls = controls[0] as Panel;

                    int? groupTypeId = GetGroupTypeIdFromSelection( selectionValues );
                    GroupTypePicker groupTypePicker = pnlGroupAttributeFilterControls.Controls[0] as GroupTypePicker;
                    groupTypePicker.SelectedGroupTypeId = groupTypeId;
                    groupTypePicker_SelectedIndexChanged( groupTypePicker, new EventArgs() );
                }

                var propertyFieldControls = new List<Control>();
                propertyFieldControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );
                propertyFieldControls.RemoveAt( 0 );

                DropDownList ddlProperty = propertyFieldControls[0] as DropDownList;

                var groupedControls = GroupControls( entityType, pnlGroupAttributeFilterControls.Controls.OfType<Control>().ToArray() );

                SetEntityFieldSelection( ddlProperty, propertyFilterValues, groupedControls );
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
                    int? groupTypeId = GetGroupTypeIdFromSelection( values );
                    string selectedProperty = values[1];

                    var entityField = GetGroupAttributes( groupTypeId ).Where( p => p.Name == selectedProperty ).FirstOrDefault();
                    if ( entityField != null )
                    {
                        return GetAttributeExpression( serviceInstance, parameterExpression, entityField, values.Skip( 2 ).ToList() );
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

            // start at 2 since the first two controls will be grouptype and property selection
            int index = 2;
            var sortedFields = new List<EntityField>();
            foreach ( var entityProperty in entityAttributeFields.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityProperty.Index = index;
                index += entityProperty.ControlCount;
                sortedFields.Add( entityProperty );
            }

            return sortedFields;
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
            var groupTypePicker = controls[0] as GroupTypePicker;

            foreach ( var property in GetGroupAttributes( groupTypePicker.SelectedGroupTypeId ) )
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