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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Enums.Reporting;
using Rock.Field;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Workflow
{
    /// <summary>
    /// Filter Workflows by their Attribute Values
    /// </summary>
    [Description( "Filter Workflows by their Attribute Values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Attributes Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "F0DF027F-65F5-49DE-BA26-25610B83879A" )]
    public class WorkflowAttributesFilter : EntityFieldFilter
    {
        #region Settings

        /// <summary>
        /// 
        /// </summary>
        private class SelectionConfig
        {
            public int? WorkflowTypeId { get; set; }

            public List<string> AttributeFilterSettings { get; set; } = new List<string>();

            public string AttributeKey { get; set; }

            /// <summary>
            /// Parses the specified selection.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();

                if ( selectionConfig == null )
                {
                    // see if it is in an older format
                    var values = selection.FromJsonOrNull<List<string>>() ?? new List<string>();
                    selectionConfig = new SelectionConfig();
                    if ( values.Count > 0 )
                    {
                        var workflowTypeId = WorkflowTypeCache.GetId( values[0].AsGuid() );
                        selectionConfig.WorkflowTypeId = workflowTypeId;
                    }

                    if ( values.Count > 1 )
                    {
                        selectionConfig.AttributeKey = values[1];
                    }

                    selectionConfig.AttributeFilterSettings = values.Skip( 1 ).ToList();
                }

                selectionConfig.AttributeFilterSettings = selectionConfig.AttributeFilterSettings ?? new List<string>();

                return selectionConfig;
            }
        }

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
            get { return typeof( Rock.Model.Workflow ).FullName; }
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Workflow/workflowAttributesFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            var workflowTypeService = new WorkflowTypeService( rockContext );

            var workflowTypes = workflowTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            var workflowTypeGuidsById = new Dictionary<int, Guid>();
            var fieldFilterSourcesByWorkflowType = new Dictionary<Guid, List<FieldFilterSourceBag>>();

            // Prime the dictionary with empty lists to make it easier to add the entity fields
            foreach ( var workflowTypeItem in workflowTypes )
            {
                workflowTypeGuidsById.Add( workflowTypeItem.Id, workflowTypeItem.Guid );
                fieldFilterSourcesByWorkflowType.AddOrReplace( workflowTypeItem.Guid, new List<FieldFilterSourceBag>() );
            }

            // Add a list for when no workflow type is selected
            fieldFilterSourcesByWorkflowType.AddOrReplace( Guid.Empty, new List<FieldFilterSourceBag>() );

            var attributes = AttributeCache.AllForEntityType<Rock.Model.Workflow>()
                .Where( a => a.IsActive )
                .Where( a => a.FieldType.Field.HasFilterControl() )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name )
                .ToList();
            var allEntityFields = new List<EntityField>();
            EntityHelper.AddEntityFieldsForAttributeList( allEntityFields, attributes );

            // Go through all the entity fields and add them to the dictionary based on content channel type indicated by their entity type qualifier
            foreach ( var attribute in attributes )
            {
                //var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );
                Guid workflowTypeGuid = Guid.Empty;

                if ( attribute.EntityTypeQualifierColumn == "WorkflowTypeId" )
                {
                    workflowTypeGuid = workflowTypeGuidsById.GetValueOrDefault( attribute.EntityTypeQualifierValue.AsInteger(), Guid.Empty );
                }

                var fieldType = attribute.FieldType;

                var source = new FieldFilterSourceBag
                {
                    Guid = attribute.Guid,
                    Type = FieldFilterSourceType.Attribute,
                    Attribute = new PublicAttributeBag
                    {
                        AttributeGuid = attribute.Guid,
                        ConfigurationValues = fieldType.Field.GetPublicConfigurationValues( attribute.ConfigurationValues, Field.ConfigurationValueUsage.Edit, null ),
                        Description = attribute.Description,
                        FieldTypeGuid = fieldType.Guid,
                        Name = attribute.Name
                    }
                };

                // Add to list for specific workflow type
                if ( workflowTypeGuid != Guid.Empty )
                {
                    fieldFilterSourcesByWorkflowType.GetValueOrNull( workflowTypeGuid )?.Add( source );
                }
                // Add to each list, because it's global (not specific to a workflow type)
                else
                {
                    foreach ( var filterSourceList in fieldFilterSourcesByWorkflowType )
                    {
                        filterSourceList.Value.Add( source );
                    }
                }

            }

            data.AddOrReplace( "fieldFilterSources", fieldFilterSourcesByWorkflowType.ToCamelCaseJson( false, true ) );

            if ( selection.IsNullOrWhiteSpace() )
            {
                return data;
            }

            var config = SelectionConfig.Parse( selection );

            if ( config == null )
            {
                return data;
            }

            var workflowType = workflowTypeService.Get( config.WorkflowTypeId ?? 0 );
            data.AddOrReplace( "workflowType", workflowType?.ToListItemBag().ToCamelCaseJson( false, true ) ?? "" );

            if ( config.AttributeKey.IsNullOrWhiteSpace() )
            {
                return data;
            }

            var entityField = allEntityFields.ToList().FindFromFilterSelection( config.AttributeKey );
            var filterAttribute = AttributeCache.Get( entityField?.AttributeGuid ?? Guid.Empty );
            FieldFilterRuleBag filterRule = null;

            if ( entityField == null || filterAttribute == null )
            {
                return data;
            }

            if ( config.AttributeFilterSettings.Count == 0 )
            {
                filterRule = FieldVisibilityRule.GetPublicRuleBag( filterAttribute, 0, null );
            }
            else if ( config.AttributeFilterSettings.Count == 1 )
            {
                filterRule = FieldVisibilityRule.GetPublicRuleBag( filterAttribute, 0, config.AttributeFilterSettings[0] );
            }
            else if ( config.AttributeFilterSettings.Count == 2 )
            {
                filterRule = FieldVisibilityRule.GetPublicRuleBag(
                    filterAttribute,
                    ( ComparisonType ) ( config.AttributeFilterSettings[0].AsInteger() ),
                    config.AttributeFilterSettings[1] );
            }

            data.AddOrReplace( "filterRule", filterRule?.ToCamelCaseJson( false, true ) );

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var config = new SelectionConfig();

            var workflowTypeGuid = data.GetValueOrNull( "workflowType" )?.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
            var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeGuid.AsGuid() );
            config.WorkflowTypeId = workflowType?.Id;

            var filterRule = data.GetValueOrNull( "filterRule" ).FromJsonOrNull<FieldFilterRuleBag>();
            if ( filterRule == null || filterRule.AttributeGuid == null )
            {
                return config.ToJson();
            }

            var attribute = AttributeCache.Get( filterRule.AttributeGuid.Value );
            var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );
            var comparisonValue = new ComparisonValue
            {
                ComparisonType = filterRule.ComparisonType,
                Value = filterRule.Value
            };

            var filterValue = attribute.FieldType.Field.GetPrivateFilterValue( comparisonValue, attribute.ConfigurationValues ).FromJsonOrNull<List<string>>();

            config.AttributeKey = entityField.UniqueName;
            config.AttributeFilterSettings = filterValue;

            return config.ToJson();
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
            return "Workflow Attribute Values";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the FilterField control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "WorkflowPropertySelection( $content )";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return GetSelectedFieldName( selection );
        }

        /// <summary>
        /// Gets the name of the selected field.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string GetSelectedFieldName( string selection )
        {
            string result = "Workflow Property";
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var settings = SelectionConfig.Parse( selection );
                var entityField = GetWorkflowAttributes( settings.WorkflowTypeId ).FirstOrDefault( f => f.UniqueName == settings.AttributeKey );
                if ( entityField != null )
                {
                    result = entityField.FormattedFilterDescription( settings.AttributeFilterSettings );
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Updates the selection from parameters on the request.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="requestContext">The rock request context.</param>
        /// <param name="rockContext">The rock database context.</param>
        /// <returns></returns>
        public override string UpdateSelectionFromRockRequestContext( string selection, Rock.Net.RockRequestContext requestContext, RockContext rockContext )
        {
            // don't modify the selection for the Filter based on PageParameters
            return selection;
        }

#if WEBFORMS

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

            WorkflowTypePicker workflowTypePicker = new WorkflowTypePicker();
            workflowTypePicker.ID = filterControl.ID + "_workflowTypePicker";
            workflowTypePicker.Label = "Workflow Type";
            workflowTypePicker.AddCssClass( "js-workflow-type-picker" );
            workflowTypePicker.ValueChanged += workflowTypePicker_ValueChanged;
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // we still need to render the control in order to get the selected WorkflowTypeId on PostBack, so just hide it instead
                workflowTypePicker.Style[HtmlTextWriterStyle.Display] = "none";
            }

            containerControl.Controls.Add( workflowTypePicker );

            // set the WorkflowTypePicker selected value now so we can create the other controls the depending on know the workflowTypeId
            if ( filterControl.Page.IsPostBack )
            {
                var hiddenField = workflowTypePicker.ControlsOfTypeRecursive<HiddenFieldWithClass>().Where( a => a.CssClass.Contains( "js-item-id-value" ) ).FirstOrDefault();

                // since we just created the WorkflowTypePicker, we'll have to sniff the WorkflowTypeId from Request.Params
                int? workflowTypeId = filterControl.Page.Request.Params[hiddenField.UniqueID].AsIntegerOrNull();
                workflowTypePicker.SetValue( workflowTypeId );
                EnsureSelectedWorkflowTypeControls( workflowTypePicker );
            }

            return new Control[] { containerControl };
        }

        /// <summary>
        /// Handles the ValueChanged event of the workflowTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void workflowTypePicker_ValueChanged( object sender, EventArgs e )
        {
            DynamicControlsPanel containerControl = ( sender as Control ).FirstParentControlOfType<DynamicControlsPanel>();
            WorkflowTypePicker workflowTypePicker = containerControl.ControlsOfTypeRecursive<WorkflowTypePicker>().Where( a => a.CssClass.Contains( "js-workflow-type-picker" ) ).FirstOrDefault();
            EnsureSelectedWorkflowTypeControls( workflowTypePicker );
        }

        /// <summary>
        /// Ensures that the controls that are created based on the WorkflowType have been created
        /// </summary>
        /// <param name="workflowTypePicker">The workflow type picker.</param>
        private void EnsureSelectedWorkflowTypeControls( WorkflowTypePicker workflowTypePicker )
        {
            DynamicControlsPanel containerControl = workflowTypePicker.Parent as DynamicControlsPanel;
            FilterField filterControl = containerControl.FirstParentControlOfType<FilterField>();

            var entityFields = GetWorkflowAttributes( workflowTypePicker.SelectedValueAsId() );

            // Create the field selection dropdown
            string propertyControlId = string.Format( "{0}_ddlProperty", containerControl.ID );
            RockDropDownList ddlProperty = containerControl.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID == propertyControlId );
            if ( ddlProperty == null )
            {
                ddlProperty = new RockDropDownList();
                ddlProperty.ID = propertyControlId;
                ddlProperty.AutoPostBack = true;
                ddlProperty.SelectedIndexChanged += ddlProperty_SelectedIndexChanged;
                ddlProperty.AddCssClass( "js-property-dropdown" );
                ddlProperty.Attributes["EntityTypeId"] = EntityTypeCache.GetId<Rock.Model.Workflow>().ToString();
                containerControl.Controls.Add( ddlProperty );
            }

            // update the list of items just in case the WorkflowType changed
            ddlProperty.Items.Clear();

            // add Empty option first
            ddlProperty.Items.Add( new ListItem() );
            foreach ( var entityField in entityFields )
            {
                // Add the field to the dropdown of available fields
                ddlProperty.Items.Add( new ListItem( entityField.TitleWithoutQualifier, entityField.UniqueName ) );
            }

            if ( workflowTypePicker.Page.IsPostBack )
            {
                ddlProperty.SetValue( workflowTypePicker.Page.Request.Params[ddlProperty.UniqueID] );
            }

            foreach ( var entityField in entityFields )
            {
                string controlId = string.Format( "{0}_{1}", containerControl.ID, entityField.UniqueName );
                if ( !containerControl.Controls.OfType<Control>().Any( a => a.ID == controlId ) )
                {
                    var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true, filterControl.FilterMode );
                    if ( control != null )
                    {
                        containerControl.Controls.Add( control );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlProperty_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlProperty = sender as RockDropDownList;
            var containerControl = ddlProperty.FirstParentControlOfType<DynamicControlsPanel>();
            FilterField filterControl = ddlProperty.FirstParentControlOfType<FilterField>();
            var workflowTypePicker = filterControl.ControlsOfTypeRecursive<WorkflowTypePicker>().Where( a => a.HasCssClass( "js-workflow-type-picker" ) ).FirstOrDefault();

            var entityFields = GetWorkflowAttributes( workflowTypePicker.SelectedValueAsId() );

            var entityField = entityFields.FirstOrDefault( a => a.UniqueName == ddlProperty.SelectedValue );

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
                if ( containerControl.Controls.Count >= 2 )
                {
                    WorkflowTypePicker workflowTypePicker = containerControl.Controls[0] as WorkflowTypePicker;
                    workflowTypePicker.RenderControl( writer );

                    DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                    var entityFields = GetWorkflowAttributes( workflowTypePicker.SelectedValueAsId() );

                    var panelControls = new List<Control>();
                    panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                    RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlProperty, panelControls, containerControl.ID, filterMode );
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
            var settings = new SelectionConfig();

            if ( controls.Length > 0 )
            {
                var containerControl = controls[0] as DynamicControlsPanel;
                if ( containerControl.Controls.Count >= 1 )
                {
                    // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
                    WorkflowTypePicker workflowTypePicker = containerControl.Controls[0] as WorkflowTypePicker;
                    var workflowTypeId = workflowTypePicker.SelectedValueAsId();

                    if ( containerControl.Controls.Count > 1 )
                    {
                        DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                        settings.AttributeKey = ddlProperty.SelectedValue;
                        settings.WorkflowTypeId = workflowTypeId;
                        var entityFields = GetWorkflowAttributes( workflowTypeId );
                        var entityField = entityFields.FirstOrDefault( f => f.UniqueName == ddlProperty.SelectedValue );
                        if ( entityField != null )
                        {
                            var panelControls = new List<Control>();
                            panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                            var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( entityField.UniqueName ) );
                            if ( control != null )
                            {
                                entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, filterMode ).ForEach( v => settings.AttributeFilterSettings.Add( v ) );
                            }
                        }
                    }
                }
            }

            return settings.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var settings = SelectionConfig.Parse( selection );

            if ( controls.Length > 0 )
            {
                int? workflowTypeId = settings.WorkflowTypeId;

                var containerControl = controls[0] as DynamicControlsPanel;
                if ( containerControl.Controls.Count > 0 )
                {
                    WorkflowTypePicker workflowTypePicker = containerControl.Controls[0] as WorkflowTypePicker;
                    workflowTypePicker.SetValue( workflowTypeId );
                    EnsureSelectedWorkflowTypeControls( workflowTypePicker );
                }

                if ( containerControl.Controls.Count > 1 )
                {
                    DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                    var entityFields = GetWorkflowAttributes( workflowTypeId );

                    var panelControls = new List<Control>();
                    panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                    var parameters = new List<string> { settings.AttributeKey };

                    parameters.AddRange( settings.AttributeFilterSettings );

                    SetEntityFieldSelection( entityFields, ddlProperty, parameters, panelControls );
                }
            }
        }

#endif

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
            var settings = SelectionConfig.Parse( selection );

            if ( settings.AttributeFilterSettings.Any() )
            {
                var entityFields = GetWorkflowAttributes( settings.WorkflowTypeId );
                var entityField = entityFields.FindFromFilterSelection( settings.AttributeKey );
                if ( entityField != null )
                {
                    return GetAttributeExpression( serviceInstance, parameterExpression, entityField, settings.AttributeFilterSettings );
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the workflow attributes.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <returns></returns>
        private static List<EntityField> GetWorkflowAttributes( int? workflowTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();

            var fakeWorkflow = new Rock.Model.Workflow();
            if ( workflowTypeId.HasValue )
            {
                fakeWorkflow.WorkflowTypeId = workflowTypeId.Value;
            }
            else
            {
                //// if no WorkflowTypeId was specified, just set the WorkflowTypeId to 0
                //// NOTE: There could be Workflow Attributes that are not specific to a WorkflowTypeId
                fakeWorkflow.WorkflowTypeId = 0;
            }

            Rock.Attribute.Helper.LoadAttributes( fakeWorkflow );
            var attributeList = fakeWorkflow.Attributes.Select( a => a.Value ).ToList();
            EntityHelper.AddEntityFieldsForAttributeList( entityAttributeFields, attributeList );

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