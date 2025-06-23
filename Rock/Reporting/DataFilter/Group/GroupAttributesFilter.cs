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
using Rock.Enums.Reporting;
using Rock.Field;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
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
    [Rock.SystemGuid.EntityTypeGuid( "0F4D0B55-EC26-43D5-91CE-D7162ABCDCE6" )]
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Group/groupAttributesFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            var groupTypeService = new GroupTypeService( rockContext );

            var groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            var groupTypeGuidsById = new Dictionary<int, Guid>();
            var fieldFilterSourcesByGroupType = new Dictionary<Guid, List<FieldFilterSourceBag>>();

            // Prime the dictionary with empty lists to make it easier to add the entity fields
            foreach ( var groupType in groupTypes )
            {
                groupTypeGuidsById.Add( groupType.Id, groupType.Guid );
                fieldFilterSourcesByGroupType.AddOrReplace( groupType.Guid, new List<FieldFilterSourceBag>() );
            }

            var attributes = AttributeCache.AllForEntityType<Rock.Model.Group>()
                .Where( a => a.IsActive )
                .Where( a => a.FieldType.Field.HasFilterControl() )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name )
                .ToList();
            var allEntityFields = new List<EntityField>();
            EntityHelper.AddEntityFieldsForAttributeList( allEntityFields, attributes );

            // Go through all the entity fields and add them to the dictionary based on content channel type indicated by their entity type qualifier
            foreach ( var attribute in attributes )
            {
                Guid groupTypeGuid = Guid.Empty;

                if ( attribute.EntityTypeQualifierColumn == "GroupTypeId" )
                {
                    groupTypeGuid = groupTypeGuidsById.GetValueOrDefault( attribute.EntityTypeQualifierValue.AsInteger(), Guid.Empty );
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

                // Add to list for specific group type
                if ( groupTypeGuid != Guid.Empty )
                {
                    fieldFilterSourcesByGroupType.GetValueOrNull( groupTypeGuid )?.Add( source );
                }
                // Add to each list, because it's global
                else
                {
                    foreach ( var filterSourceList in fieldFilterSourcesByGroupType )
                    {
                        filterSourceList.Value.Add( source );
                    }
                }

            }

            data.AddOrReplace( "fieldFilterSources", fieldFilterSourcesByGroupType.ToCamelCaseJson( false, true ) );

            if ( !string.IsNullOrWhiteSpace( selection ) || selection.IsNotNullOrWhiteSpace() )
            {
                var values = selection.FromJsonOrNull<List<string>>();
                if ( values == null || values.Count == 0 )
                {
                    return data;
                }

                var groupType = groupTypeService.Get( values[0]?.AsGuid() ?? Guid.Empty );
                data.AddOrReplace( "groupType", groupType?.ToListItemBag().ToCamelCaseJson( false, true ) ?? "" );

                if ( values.Count > 1 )
                {
                    var entityField = allEntityFields.ToList().FindFromFilterSelection( values[1] );

                    if ( entityField == null )
                    {
                        return data;
                    }

                    var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );

                    if ( values.Count == 3 )
                    {
                        var filterRule = FieldVisibilityRule.GetPublicRuleBag( attribute, 0, values[2] );

                        data.AddOrReplace( "filterRule", filterRule.ToCamelCaseJson( false, true ) );
                    }
                    else if ( values.Count == 4 )
                    {
                        var filterRule = FieldVisibilityRule.GetPublicRuleBag( attribute, ( ComparisonType ) ( values[2].AsInteger() ), values[3] );

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

            var groupType = data.GetValueOrNull( "groupType" )?.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
            if ( groupType.IsNullOrWhiteSpace() )
            {
                return selectionValues.ToJson();
            }
            selectionValues.Add( groupType );

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
            return this.GetSelectedFieldName( selection );
        }

        /// <summary>
        /// Gets the name of the selected field.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string GetSelectedFieldName( string selection )
        {
            string result = "Group Property";

            // First value is group type, second value is attribute, remaining values are the field type's filter values
            var values = JsonConvert.DeserializeObject<List<string>>( selection );
            if ( values.Count >= 2 )
            {
                var groupType = GroupTypeCache.Get( values[0].AsGuid() );
                if ( groupType != null )
                {
                    var entityFields = GetGroupAttributes( groupType.Id );
                    var entityField = entityFields.FindFromFilterSelection( values[1] );
                    if ( entityField != null )
                    {
                        result = entityField.FormattedFilterDescription( values.Skip( 2 ).ToList() );
                    }
                }

            }

            return result;
        }

#if REVIEW_WEBFORMS
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
            var containerControl = new DynamicControlsPanel();
            containerControl.ID = string.Format( "{0}_containerControl", filterControl.ID );
            containerControl.CssClass = "js-container-control";
            filterControl.Controls.Add( containerControl );

            GroupTypePicker groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.ID + "_groupTypePicker";
            groupTypePicker.AddCssClass( "js-group-type-picker" );
            groupTypePicker.Label = "Group Type";
            groupTypePicker.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().ToList();
            groupTypePicker.SelectedIndexChanged += groupTypePicker_SelectedIndexChanged;
            groupTypePicker.AutoPostBack = true;
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // we still need to render the control in order to get the selected GroupTypeId on postback, so just hide it instead
                groupTypePicker.Style[HtmlTextWriterStyle.Display] = "none";
            }

            containerControl.Controls.Add( groupTypePicker );

            // set the GroupTypePicker selected value now so we can create the other controls the depending on know the groupTypeid
            if ( filterControl.Page.IsPostBack )
            {
                // since we just created the GroupTypePicker, we'll have to sniff the GroupTypeId from Request.Params
                int? groupTypeId = filterControl.Page.Request.Params[groupTypePicker.UniqueID].AsIntegerOrNull();
                groupTypePicker.SelectedGroupTypeId = groupTypeId;
                EnsureSelectedGroupTypeControls( groupTypePicker );
            }

            return new Control[] { containerControl };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the groupTypePicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void groupTypePicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            GroupTypePicker groupTypePicker = sender as GroupTypePicker;
            EnsureSelectedGroupTypeControls( groupTypePicker );
        }

        /// <summary>
        /// Ensures that the controls that are created based on the GroupType have been created
        /// </summary>
        /// <param name="groupTypePicker">The group type picker.</param>
        private void EnsureSelectedGroupTypeControls( GroupTypePicker groupTypePicker )
        {
            DynamicControlsPanel containerControl = groupTypePicker.Parent as DynamicControlsPanel;
            FilterField filterControl = containerControl.FirstParentControlOfType<FilterField>();

            var entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );

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
                containerControl.Controls.Add( ddlProperty );
            }

            // update the list of items just in case the GroupType changed
            ddlProperty.Items.Clear();

            // add Empty option first
            ddlProperty.Items.Add( new ListItem() );
            foreach ( var entityField in entityFields )
            {
                // Add the field to the dropdown of available fields
                ddlProperty.Items.Add( new ListItem( entityField.TitleWithoutQualifier, entityField.UniqueName ) );
            }

            if ( groupTypePicker.Page.IsPostBack )
            {
                ddlProperty.SetValue( groupTypePicker.Page.Request.Params[ddlProperty.UniqueID] );
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
            var groupTypePicker = filterControl.ControlsOfTypeRecursive<GroupTypePicker>().Where( a => a.HasCssClass( "js-group-type-picker" ) ).FirstOrDefault();

            var entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );

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
                    GroupTypePicker groupTypePicker = containerControl.Controls[0] as GroupTypePicker;
                    groupTypePicker.RenderControl( writer );

                    DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                    var entityFields = GetGroupAttributes( groupTypePicker.SelectedGroupTypeId );

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
            var values = new List<string>();

            if ( controls.Length > 0 )
            {
                var containerControl = controls[0] as DynamicControlsPanel;
                if ( containerControl.Controls.Count >= 1 )
                {
                    // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
                    GroupTypePicker groupTypePicker = containerControl.Controls[0] as GroupTypePicker;
                    Guid groupTypeGuid = Guid.Empty;
                    var groupType = GroupTypeCache.Get( groupTypePicker.SelectedGroupTypeId ?? 0 );
                    if ( groupType != null )
                    {
                        if ( containerControl.Controls.Count > 1 )
                        {
                            DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;

                            var entityFields = GetGroupAttributes( groupType.Id );
                            var entityField = entityFields.FirstOrDefault( f => f.UniqueName == ddlProperty.SelectedValue );
                            if ( entityField != null )
                            {
                                var panelControls = new List<Control>();
                                panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                                var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( entityField.UniqueName ) );
                                if ( control != null )
                                {
                                    values.Add( groupType.Guid.ToString() );
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
                    var groupType = GroupTypeCache.Get( values[0].AsGuid() );
                    if ( groupType != null )
                    {
                        var containerControl = controls[0] as DynamicControlsPanel;
                        if ( containerControl.Controls.Count > 0 )
                        {
                            GroupTypePicker groupTypePicker = containerControl.Controls[0] as GroupTypePicker;
                            groupTypePicker.SelectedGroupTypeId = groupType.Id;
                            EnsureSelectedGroupTypeControls( groupTypePicker );
                        }

                        if ( containerControl.Controls.Count > 1 && values.Count > 1 )
                        {
                            DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                            var entityFields = GetGroupAttributes( groupType.Id );

                            var panelControls = new List<Control>();
                            panelControls.AddRange( containerControl.Controls.OfType<Control>() );
                            SetEntityFieldSelection( entityFields, ddlProperty, values.Skip( 1 ).ToList(), panelControls );
                        }
                    }
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
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( selection );

                if ( values.Count >= 3 )
                {
                    var groupType = GroupTypeCache.Get( values[0].AsGuid() );
                    if ( groupType != null )
                    {
                        string selectedProperty = values[1];

                        var entityFields = GetGroupAttributes( groupType.Id );
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
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private List<EntityField> GetGroupAttributes( int? groupTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();

            if ( groupTypeId.HasValue )
            {
                var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId.Value };
                Rock.Attribute.Helper.LoadAttributes( fakeGroup );

                var attributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();
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

        #endregion
    }
}