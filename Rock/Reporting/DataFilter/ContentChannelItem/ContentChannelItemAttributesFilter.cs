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
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.ContentChannelItem
{
    /// <summary>
    /// Filter Content Channel Items based on any of its attribute values
    /// </summary>
    [Description( "Filter Content Channel Items based on any of its attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Item Attributes Filter" )]
    public class ContentChannelItemAttributesFilter : EntityFieldFilter
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
            get { return typeof( Rock.Model.ContentChannelItem ).FullName; }
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
            return "Content Channel Item Attribute Values";
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
            return "ContentChannelItemPropertySelection( $content )";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Content Channel Item Property";

            // First value is content channel type, second value is attribute, remaining values are the field type's filter values
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count >= 2 )
            {
                var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( values[0].AsGuid() );
                if ( contentChannelType != null )
                {
                    var entityFields = GetContentChannelItemAttributes( contentChannelType.Id );
                    var entityField = entityFields.FindFromFilterSelection( values[1] );
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
        /// Implement this version of CreateChildControls if your DataFilterComponent supports different FilterModes
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="filterControl"></param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            var containerControl = new DynamicControlsPanel();
            containerControl.ID = string.Format( "{0}_containerControl", filterControl.ID );
            containerControl.CssClass = "js-container-control";
            filterControl.Controls.Add( containerControl );

            RockDropDownList contentChannelTypePicker = new RockDropDownList();
            contentChannelTypePicker.ID = filterControl.ID + "_contentChannelTypePicker";
            contentChannelTypePicker.Label = "Content Channel Type";

            contentChannelTypePicker.Items.Clear();
            var contentChannelTypeList = new ContentChannelTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            foreach ( var contentChannelType in contentChannelTypeList )
            {
                contentChannelTypePicker.Items.Add( new ListItem( contentChannelType.Name, contentChannelType.Id.ToString() ) );
            }

            contentChannelTypePicker.SelectedIndexChanged += contentChannelTypePicker_SelectedIndexChanged;
            contentChannelTypePicker.AutoPostBack = true;
            contentChannelTypePicker.Visible = filterMode == FilterMode.AdvancedFilter;
            containerControl.Controls.Add( contentChannelTypePicker );

            // set the contentChannelTypePicker selected value now so we can create the other controls that depend on knowing the contentChannelTypeId
            int? contentChannelTypeId = filterControl.Page.Request.Params[contentChannelTypePicker.UniqueID].AsIntegerOrNull();
            contentChannelTypePicker.SelectedValue = contentChannelTypeId.ToString();
            contentChannelTypePicker_SelectedIndexChanged( contentChannelTypePicker, new EventArgs() );

            return new Control[] { containerControl };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the contentChannelTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void contentChannelTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockDropDownList contentChannelTypePicker = sender as RockDropDownList;
            DynamicControlsPanel containerControl = contentChannelTypePicker.Parent as DynamicControlsPanel;
            FilterField filterControl = containerControl.FirstParentControlOfType<FilterField>();

            containerControl.Controls.Clear();
            containerControl.Controls.Add( contentChannelTypePicker );

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = string.Format( "{0}_{1}_ddlProperty", containerControl.ID, contentChannelTypePicker.SelectedValue.AsInteger() );
            containerControl.Controls.Add( ddlProperty );

            // add Empty option first
            ddlProperty.Items.Add( new ListItem() );

            this.entityFields = GetContentChannelItemAttributes( contentChannelTypePicker.SelectedValue.AsIntegerOrNull() );
            foreach ( var entityField in this.entityFields )
            {
                string controlId = string.Format( "{0}_{1}", containerControl.ID, entityField.UniqueName );
                var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true, filterControl.FilterMode );
                if ( control != null )
                {
                    // Add the field to the dropdown of available fields
                    if ( AttributeCache.Get( entityField.AttributeGuid.Value )?.EntityTypeQualifierColumn == "ContentChannelTypeId" )
                    {
                        ddlProperty.Items.Add( new ListItem( entityField.TitleWithoutQualifier, entityField.UniqueName ) );
                    }
                    else
                    {
                        ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.UniqueName ) );
                    }

                    containerControl.Controls.Add( control );
                }
            }

            ddlProperty.AutoPostBack = true;

            // grab the currently selected value off of the request params since we are creating the controls after the Page Init
            var selectedValue = ddlProperty.Page.Request.Params[ddlProperty.UniqueID];
            if ( selectedValue != null )
            {
                ddlProperty.SelectedValue = selectedValue;
                ddlProperty_SelectedIndexChanged( ddlProperty, new EventArgs() );
            }

            ddlProperty.SelectedIndexChanged += ddlProperty_SelectedIndexChanged;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlProperty_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlEntityField = sender as RockDropDownList;
            var containerControl = ddlEntityField.FirstParentControlOfType<DynamicControlsPanel>();
            FilterField filterControl = ddlEntityField.FirstParentControlOfType<FilterField>();

            var entityField = this.entityFields.FirstOrDefault( a => a.UniqueName == ddlEntityField.SelectedValue );
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

        private List<EntityField> entityFields = null;

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
                if ( containerControl.Controls.Count >= 2 )
                {
                    RockDropDownList contentChannelTypePicker = containerControl.Controls[0] as RockDropDownList;
                    contentChannelTypePicker.RenderControl( writer );

                    DropDownList ddlEntityField = containerControl.Controls[1] as DropDownList;
                    var entityFields = GetContentChannelItemAttributes( contentChannelTypePicker.SelectedValue.AsIntegerOrNull() );

                    var panelControls = new List<Control>();
                    panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                    RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlEntityField, panelControls, containerControl.ID, filterMode );
                }
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
                if ( containerControl.Controls.Count >= 1 )
                {
                    // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
                    RockDropDownList contentChannelTypePicker = containerControl.Controls[0] as RockDropDownList;
                    var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( contentChannelTypePicker.SelectedValue.AsInteger() );
                    if ( contentChannelType != null )
                    {
                        if ( containerControl.Controls.Count == 1 )
                        {
                            contentChannelTypePicker_SelectedIndexChanged( contentChannelTypePicker, new EventArgs() );
                        }

                        if ( containerControl.Controls.Count > 1 )
                        {
                            DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;

                            var entityFields = GetContentChannelItemAttributes( contentChannelType.Id );
                            var entityField = entityFields.FirstOrDefault( f => f.UniqueName == ddlProperty.SelectedValue );
                            if ( entityField != null )
                            {
                                var panelControls = new List<Control>();
                                panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                                var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( entityField.UniqueName ) );
                                if ( control != null )
                                {
                                    values.Add( contentChannelType.Guid.ToString() );
                                    values.Add( ddlProperty.SelectedValue );
                                    entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, filterMode ).ForEach( v => values.Add( v ) );
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
                    var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( values[0].AsGuid() );
                    if ( contentChannelType != null )
                    {
                        var containerControl = controls[0] as DynamicControlsPanel;
                        if ( containerControl.Controls.Count > 0 )
                        {
                            RockDropDownList contentChannelTypePicker = containerControl.Controls[0] as RockDropDownList;
                            contentChannelTypePicker.SelectedValue = contentChannelType.Id.ToString();
                            contentChannelTypePicker_SelectedIndexChanged( contentChannelTypePicker, new EventArgs() );
                        }

                        if ( containerControl.Controls.Count > 1 && values.Count > 1 )
                        {
                            DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                            var entityFields = GetContentChannelItemAttributes( contentChannelType.Id );

                            var panelControls = new List<Control>();
                            panelControls.AddRange( containerControl.Controls.OfType<Control>() );
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
                    var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( values[0].AsGuid() );
                    if ( contentChannelType != null )
                    {
                        string selectedProperty = values[1];

                        var entityFields = GetContentChannelItemAttributes( contentChannelType.Id );
                        var entityField = entityFields.FindFromFilterSelection( selectedProperty );
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
        /// <param name="contentChannelTypeId">The content channel type identifier.</param>
        /// <returns></returns>
        private List<EntityField> GetContentChannelItemAttributes( int? contentChannelTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();
            if ( contentChannelTypeId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var contentChannelService = new ContentChannelService( rockContext );
                    var allEntityAttributeFields = EntityHelper.GetEntityFields( typeof( Rock.Model.ContentChannelItem ) ).Where( a => a.FieldKind == FieldKind.Attribute );
                    foreach ( var entityAttributeField in allEntityAttributeFields )
                    {
                        var attributeCache = AttributeCache.Get( entityAttributeField.AttributeGuid.Value );
                        if ( attributeCache.EntityTypeQualifierColumn == "ContentChannelTypeId" && attributeCache.EntityTypeQualifierValue == contentChannelTypeId.ToString() )
                        {
                            entityAttributeFields.Add( entityAttributeField );
                        }
                        else if ( attributeCache.EntityTypeQualifierColumn == "ContentChannelId" )
                        {
                            int contentChannelId = attributeCache.EntityTypeQualifierValue.AsInteger();
                            var contentChannel = contentChannelService.Queryable().Where( a => a.Id == contentChannelId ).FirstOrDefault();
                            if ( contentChannel?.ContentChannelTypeId == contentChannelTypeId.Value )
                            {
                                entityAttributeFields.Add( entityAttributeField );
                            }
                        }
                    }
                }
            }

            int index = 0;
            var sortedFields = new List<EntityField>();
            foreach ( var entityProperty in entityAttributeFields.OrderBy( p => p.TitleWithoutQualifier ).ThenBy( p => p.Name ) )
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