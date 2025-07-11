﻿// <copyright>
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
using Rock.Enums.Reporting;
using Rock.Field;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// Filter Person records on the attribute values of a Step they have participated in.
    /// </summary>
    [Description( "Filter Person records on the attribute values of a Step they have participated in" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Step Participants By Attribute Value Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "A8A108B7-6713-444F-AA02-A65C1D354B96" )]
    public class StepParticipantsByAttributeValueFilter : EntityFieldFilter
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
            get { return typeof( Rock.Model.Person ).FullName; }
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
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Person/stepParticipantsByAttributeValueFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            //var channelService = new ContentChannelService( rockContext );
            var stepProgramService = new StepProgramService( rockContext );
            var stepTypeService = new StepTypeService( rockContext );

            var stepTypes = stepTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            var stepTypeGuidsById = new Dictionary<int, Guid>();
            var fieldFilterSourcesByStepType = new Dictionary<Guid, List<FieldFilterSourceBag>>();

            // Prime the dictionary with empty lists to make it easier to add the entity fields
            foreach ( var stepType in stepTypes )
            {
                stepTypeGuidsById.Add( stepType.Id, stepType.Guid );
                fieldFilterSourcesByStepType.AddOrReplace( stepType.Guid, new List<FieldFilterSourceBag>() );
            }

            var attributes = AttributeCache.AllForEntityType<Rock.Model.Step>()
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
                Guid stepTypeGuid = Guid.Empty;

                if ( attribute.EntityTypeQualifierColumn == "StepTypeId" )
                {
                    stepTypeGuid = stepTypeGuidsById.GetValueOrDefault( attribute.EntityTypeQualifierValue.AsInteger(), Guid.Empty );
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

                // Add to list for specific step type
                if ( stepTypeGuid != Guid.Empty )
                {
                    fieldFilterSourcesByStepType.GetValueOrNull( stepTypeGuid )?.Add( source );
                }
                // Add to each list, because it's global
                else
                {
                    foreach ( var filterSourceList in fieldFilterSourcesByStepType )
                    {
                        filterSourceList.Value.Add( source );
                    }
                }

            }

            data.AddOrReplace( "fieldFilterSources", fieldFilterSourcesByStepType.ToCamelCaseJson( false, true ) );

            if ( !string.IsNullOrWhiteSpace( selection ) || selection.IsNotNullOrWhiteSpace() )
            {
                var values = selection.FromJsonOrNull<List<string>>();
                if ( values == null || values.Count == 0 )
                {
                    return data;
                }

                var stepProgram = stepProgramService.Get( values[0]?.AsGuid() ?? Guid.Empty );
                data.AddOrReplace( "stepProgram", stepProgram?.ToListItemBag().ToCamelCaseJson( false, true ) ?? "" );

                var stepType = stepTypeService.Get( values[1]?.AsGuid() ?? Guid.Empty );
                data.AddOrReplace( "stepType", stepType?.ToListItemBag().ToCamelCaseJson( false, true ) ?? "" );

                if ( values.Count > 2 )
                {

                    var entityField = allEntityFields.ToList().FindFromFilterSelection( values[2] );

                    if ( entityField == null )
                    {
                        return data;
                    }

                    var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );

                    if ( values.Count == 4 )
                    {
                        var filterRule = FieldVisibilityRule.GetPublicRuleBag( attribute, 0, values[3] );

                        data.AddOrReplace( "filterRule", filterRule.ToCamelCaseJson( false, true ) );
                    }
                    else if ( values.Count == 5 )
                    {
                        var filterRule = FieldVisibilityRule.GetPublicRuleBag( attribute, ( ComparisonType ) ( values[3].AsInteger() ), values[4] );

                        data.AddOrReplace( "filterRule", filterRule.ToCamelCaseJson( false, true ) );
                    }
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionValues = new List<string>();

            var stepProgram = data.GetValueOrNull( "stepProgram" )?.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
            if ( stepProgram.IsNullOrWhiteSpace() )
            {
                return selectionValues.ToJson();
            }
            selectionValues.Add( stepProgram );

            var stepType = data.GetValueOrNull( "stepType" )?.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
            if ( stepType.IsNullOrWhiteSpace() )
            {
                return selectionValues.ToJson();
            }
            selectionValues.Add( stepType );

            var filterRule = data.GetValueOrNull( "filterRule" ).FromJsonOrNull<FieldFilterRuleBag>();
            if ( filterRule == null || filterRule.AttributeGuid == null )
            {
                return selectionValues.ToJson();
            }

            var attribute = AttributeCache.Get( filterRule.AttributeGuid.Value );
            var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );
            var comparisonValue = new ComparisonValue
            {
                ComparisonType = filterRule.ComparisonType,
                Value = filterRule.Value
            };

            var filterValue = attribute.FieldType.Field.GetPrivateFilterValue( comparisonValue, attribute.ConfigurationValues ).FromJsonOrNull<List<string>>();

            if ( filterValue.Count >= 2 && filterValue[0] == "0" )
            {
                filterValue = filterValue.Skip( 1 ).ToList();
            }

            selectionValues.Add( entityField.UniqueName );
            selectionValues = selectionValues.Concat( filterValue ).ToList();

            return selectionValues.ToJson();
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
            return "Step Participants By Attribute Value";
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
            return "PersonPropertySelection( $content )";
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
            string result = "Step Attribute";

            // First value is StepProgram Guid, second value is StepType Guid, third value is Attribute,
            // remaining values are the field type's filter values
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count >= 3 )
            {
                var stepProgram = GetStepProgram( values[0].AsGuid() );
                var stepType = GetStepType( values[1].AsGuid() );
                if ( stepProgram != null && stepType != null )
                {
                    var entityFields = GetStepAttributes( stepType.Id );
                    var entityField = entityFields.FindFromFilterSelection( values[2] );
                    if ( entityField != null )
                    {
                        result = entityField.FormattedFilterDescription( values.Skip( 3 ).ToList() );
                        //string propertyDescription = entityField.FormattedFilterDescription( values.Skip( 3 ).ToList() );
                        //result = $"Step Attribute: {stepProgram.Name}: {stepType.Name}: {propertyDescription}";
                    }
                }

            }

            return result;
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
            // Create a container to hold our dynamic controls and add it to the FilterField.
            var containerControl = new DynamicControlsPanel
            {
                ID = string.Format( "{0}_containerControl", filterControl.ID ),
                CssClass = "js-container-control"
            };
            filterControl.Controls.Add( containerControl );

            // Create the StepProgramPicker.
            StepProgramPicker stepProgramPicker = new StepProgramPicker
            {
                ID = filterControl.ID + "_stepProgramPicker",
                Label = "Step Program",
                Required = true
            };
            stepProgramPicker.SelectedIndexChanged += stepProgramPicker_SelectedIndexChanged;
            stepProgramPicker.AutoPostBack = true;
            StepProgramPicker.LoadDropDownItems( stepProgramPicker, true );
            containerControl.Controls.Add( stepProgramPicker );

            // Create the StepTypePicker.
            StepTypePicker stepTypePicker = new StepTypePicker
            {
                ID = filterControl.ID + "_stepTypePicker",
                CssClass = "js-step-type-picker",
                Label = "Step Type",
                Required = true
            };
            stepTypePicker.SelectedIndexChanged += stepTypePicker_SelectedIndexChanged;
            stepTypePicker.AutoPostBack = true;
            containerControl.Controls.Add( stepTypePicker );

            if ( filterMode == FilterMode.SimpleFilter )
            {
                // we still need to render the control in order to get the selected StepProgramId on postback, so just hide it instead
                stepProgramPicker.Style[HtmlTextWriterStyle.Display] = "none";
                stepTypePicker.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if ( filterControl.Page.IsPostBack )
            {
                // If the Step Program has already been selected, make sure it retains that value, and
                // set the StepProgramId of the StepTypePicker.
                var stepProgramId = filterControl.Page.Request.Params[stepProgramPicker.UniqueID];
                stepProgramPicker.SelectedValue = stepProgramId;
                stepTypePicker.StepProgramId = stepProgramId.AsIntegerOrNull();

                // If the Step Type has already been selected, make sure it retains that value.
                var stepTypePickerId = filterControl.Page.Request.Params[stepTypePicker.UniqueID];
                stepTypePicker.SelectedValue = stepTypePickerId;

                // Ensure that the attribute filter controls are updated to reflect the current
                // Step Type selection.
                EnsureSelectedStepTypeControls( stepTypePicker );
            }

            return new Control[] { containerControl };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the StepProgramPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void stepProgramPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            // This sets the StepProgramId on the StepTypePicker so that it will update the list items
            // to match the items on the selected StepProgram.
            var stepProgramPicker = sender as StepProgramPicker;
            var stepTypePicker = stepProgramPicker.Parent.Controls[1] as StepTypePicker;
            stepTypePicker.StepProgramId = stepProgramPicker.SelectedValueAsId();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the StepTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void stepTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            // This triggers an update on the attribute filter controls so that they will reflect the
            // attributes associated with the selected StepType.
            var stepTypePicker = sender as StepTypePicker;
            EnsureSelectedStepTypeControls( stepTypePicker );
        }

        /// <summary>
        /// Ensures that the correct attribute filter controls are created based on the selected <see cref="StepType"/>.
        /// </summary>
        /// <param name="stepTypePicker">The <see cref="StepTypePicker"/>.</param>
        private void EnsureSelectedStepTypeControls( StepTypePicker stepTypePicker )
        {
            DynamicControlsPanel containerControl = stepTypePicker.Parent as DynamicControlsPanel;
            FilterField filterControl = containerControl.FirstParentControlOfType<FilterField>();

            // Get the EntityFields for the attributes associated with the selected StepType.
            var entityFields = GetStepAttributes( stepTypePicker.SelectedValueAsId() );

            // Create the attribute selection dropdown.
            string propertyControlId = string.Format( "{0}_ddlProperty", containerControl.ID );
            RockDropDownList ddlProperty = containerControl.Controls.OfType<RockDropDownList>().FirstOrDefault( a => a.ID == propertyControlId );
            if ( ddlProperty == null )
            {
                ddlProperty = new RockDropDownList();
                ddlProperty.ID = propertyControlId;
                ddlProperty.AutoPostBack = true;
                ddlProperty.SelectedIndexChanged += ddlProperty_SelectedIndexChanged;
                ddlProperty.AddCssClass( "js-property-dropdown" );
                containerControl.Controls.Add( ddlProperty );
            }

            // Clear the list of items.  We will rebuild them to match the selected StepType.
            ddlProperty.Items.Clear();

            // Add an empty option.
            ddlProperty.Items.Add( new ListItem() );

            // Add a ListItem for each of the attributes.
            foreach ( var entityField in entityFields )
            {
                ddlProperty.Items.Add( new ListItem( entityField.TitleWithoutQualifier, entityField.UniqueName ) );
            }

            if ( stepTypePicker.Page.IsPostBack )
            {
                // If the attribute has been selected, make sure that value is retained.
                ddlProperty.SetValue( stepTypePicker.Page.Request.Params[ddlProperty.UniqueID] );
            }

            // Add the filter controls (comparison type and value).
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
            StepTypePicker stepTypePicker = filterControl.ControlsOfTypeRecursive<StepTypePicker>().Where( a => a.HasCssClass( "js-step-type-picker" ) ).FirstOrDefault();
            var entityFields = GetStepAttributes( stepTypePicker.SelectedValueAsId() );

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
        /// <param name="entityType"><see cref="Type"/> of the entity.</param>
        /// <param name="filterControl">The <see cref="FilterField"/> control.</param>
        /// <param name="writer">The <see cref="HtmlTextWriter"/>.</param>
        /// <param name="controls">The collection of <see cref="Control"/>s.</param>
        /// <param name="filterMode">The <see cref="FilterMode"/> setting.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls, FilterMode filterMode )
        {
            if ( controls.Length < 1 )
            {
                return;
            }

            var containerControl = controls[0] as DynamicControlsPanel;
            if ( containerControl.Controls.Count < 3 )
            {
                return;
            }

            var stepProgramPicker = containerControl.Controls[0] as StepProgramPicker;
            stepProgramPicker.RenderControl( writer );

            var stepTypePicker = containerControl.Controls[1] as StepTypePicker;
            stepTypePicker.RenderControl( writer );

            DropDownList ddlProperty = containerControl.Controls[2] as DropDownList;
            var entityFields = GetStepAttributes( stepTypePicker.SelectedValueAsId() );

            var panelControls = new List<Control>();
            panelControls.AddRange( containerControl.Controls.OfType<Control>() );

            RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlProperty, panelControls, containerControl.ID, filterMode );
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

            if ( controls.Length < 1 )
            {
                return values.ToJson();
            }

            var containerControl = controls[0] as DynamicControlsPanel;
            if ( containerControl.Controls.Count < 3 )
            {
                return values.ToJson();
            }

            var stepProgramPicker = containerControl.Controls[0] as StepProgramPicker;
            var stepTypePicker = containerControl.Controls[1] as StepTypePicker;
            var stepProgram = GetStepProgram( stepProgramPicker.SelectedValueAsId() );
            var stepType = GetStepType( stepTypePicker.SelectedValueAsId() );

            if ( stepProgram == null || stepType == null )
            {
                return values.ToJson();
            }

            DropDownList ddlProperty = containerControl.Controls[2] as DropDownList;

            var entityFields = GetStepAttributes( stepType.Id );
            var entityField = entityFields.FirstOrDefault( f => f.UniqueName == ddlProperty.SelectedValue );
            if ( entityField == null )
            {
                return values.ToJson();
            }

            var panelControls = new List<Control>();
            panelControls.AddRange( containerControl.Controls.OfType<Control>() );

            var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( entityField.UniqueName ) );
            if ( control == null )
            {
                return values.ToJson();
            }

            values.Add( stepProgram.Guid.ToString() );
            values.Add( stepType.Guid.ToString() );
            values.Add( ddlProperty.SelectedValue );
            entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, filterMode ).ForEach( v => values.Add( v ) );

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
            if ( string.IsNullOrWhiteSpace( selection ) || controls.Length <= 0 )
            {
                return;
            }

            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count <= 0 )
            {
                return;
            }

            var stepType = GetStepType( values[1].AsGuid() );
            if ( stepType == null )
            {
                return;
            }

            var containerControl = controls[0] as DynamicControlsPanel;
            if ( containerControl.Controls.Count > 0 )
            {
                StepProgramPicker stepProgramPicker = containerControl.Controls[0] as StepProgramPicker;
                stepProgramPicker.SelectedValue = stepType.StepProgramId.ToString();
            }

            if ( containerControl.Controls.Count > 1 )
            {
                StepTypePicker stepTypePicker = containerControl.Controls[1] as StepTypePicker;
                stepTypePicker.StepProgramId = stepType.StepProgramId;
                stepTypePicker.SelectedValue = stepType.Id.ToString();

                EnsureSelectedStepTypeControls( stepTypePicker );
            }

            if ( containerControl.Controls.Count > 2 && values.Count > 2 )
            {
                DropDownList ddlProperty = containerControl.Controls[2] as DropDownList;
                var entityFields = GetStepAttributes( stepType.Id );

                var panelControls = new List<Control>();
                panelControls.AddRange( containerControl.Controls.OfType<Control>() );
                SetEntityFieldSelection( entityFields, ddlProperty, values.Skip( 2 ).ToList(), panelControls );
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
            if ( string.IsNullOrWhiteSpace( selection ) )
            {
                return null;
            }

            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count < 3 )
            {
                return null;
            }

            var stepProgramGuid = values[0].AsGuid();
            var stepTypeGuid = values[1].AsGuid();
            var selectedProperty = values[2];

            var stepProgram = GetStepProgram( stepProgramGuid );
            if ( stepProgram == null )
            {
                return null;
            }

            var stepType = GetStepType( stepTypeGuid );
            if ( stepType == null )
            {
                return null;
            }

            var rockContext = ( RockContext ) serviceInstance.Context;

            var entityFields = GetStepAttributes( stepType.Id );
            var entityField = entityFields.FindFromFilterSelection( selectedProperty );
            if ( entityField == null )
            {
                return null;
            }

            // Find matchings Steps.
            var stepService = new StepService( rockContext );
            var stepParameterExpression = stepService.ParameterExpression;
            var attributeFilterValues = values.Skip( 3 ).ToList();
            var attributeWhereExpression = GetAttributeExpression( stepService, stepParameterExpression, entityField, attributeFilterValues );
            var stepQuery = stepService.Queryable()
                .Where( stepParameterExpression, attributeWhereExpression );

            // Get Person records associated with the Steps.
            var personService = new PersonService( rockContext );
            var personQuery = personService.Queryable()
                .Where( p => stepQuery.Any( x => x.PersonAlias.PersonId == p.Id ) );

            // Extract the expression.
            var dataFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( personQuery, parameterExpression, "p" );
            return dataFilterExpression;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the properties and attributes for the entity
        /// </summary>
        /// <param name="stepTypeId">The step type identifier.</param>
        /// <returns></returns>
        private List<EntityField> GetStepAttributes( int? stepTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();

            if ( stepTypeId.HasValue )
            {
                var fakeStep = new Rock.Model.Step { StepTypeId = stepTypeId.Value };
                Rock.Attribute.Helper.LoadAttributes( fakeStep );

                var attributeList = fakeStep.Attributes.Select( a => a.Value ).ToList();
                EntityHelper.AddEntityFieldsForAttributeList( entityAttributeFields, attributeList );
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

        /// <summary>
        /// Gets the specified <see cref="StepProgram"/> by Id.
        /// </summary>
        /// <param name="stepProgramId">The identifier of the <see cref="StepProgram"/>.</param>
        /// <returns>A <see cref="StepProgram"/> or null.</returns>
        private StepProgram GetStepProgram( int? stepProgramId )
        {
            if ( !stepProgramId.HasValue || stepProgramId.Value == 0 )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var stepTypeService = new StepProgramService( rockContext );
                return stepTypeService.Get( stepProgramId.Value );
            }
        }

        /// <summary>
        /// Gets the specified <see cref="StepProgram"/> by Id.
        /// </summary>
        /// <param name="stepProgramGuid">The identifier of the <see cref="StepProgram"/>.</param>
        /// <returns>A <see cref="StepProgram"/> or null.</returns>
        private StepProgram GetStepProgram( Guid? stepProgramGuid )
        {
            if ( !stepProgramGuid.HasValue || stepProgramGuid.Value == Guid.Empty )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var stepTypeService = new StepProgramService( rockContext );
                return stepTypeService.Get( stepProgramGuid.Value );
            }
        }

        /// <summary>
        /// Gets the specified <see cref="StepType"/> by Id.
        /// </summary>
        /// <param name="stepTypeId">The identifier of the <see cref="StepType"/>.</param>
        /// <returns>A <see cref="StepType"/> or null.</returns>
        private StepType GetStepType( int? stepTypeId )
        {
            if ( !stepTypeId.HasValue || stepTypeId.Value == 0 )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var stepTypeService = new StepTypeService( rockContext );
                return stepTypeService.Get( stepTypeId.Value );
            }
        }

        /// <summary>
        /// Gets the specified <see cref="StepType"/> by Guid.
        /// </summary>
        /// <param name="stepTypeGuid">The Guid of the <see cref="StepType"/>.</param>
        /// <returns>A <see cref="StepType"/> or null.</returns>
        private StepType GetStepType( Guid? stepTypeGuid )
        {
            if ( !stepTypeGuid.HasValue || stepTypeGuid.Value == Guid.Empty )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var stepTypeService = new StepTypeService( rockContext );
                return stepTypeService.Get( stepTypeGuid.Value );
            }
        }

        #endregion
    }
}