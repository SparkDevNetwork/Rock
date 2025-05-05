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
using Rock.Utility;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.GroupMember
{
    /// <summary>
    /// Filter Group Members by their Attribute Values
    /// </summary>
    [Description( "Filter Group Members by their Attribute Values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Attributes Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "5ABED6E0-BD45-455A-B28C-72F5648A28AD" )]
    public class GroupMemberAttributesFilter : EntityFieldFilter
    {
        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public List<string> AttributeFilterSettings;
            public string AttributeKey;
            public int? GroupTypeId;

            public FilterSettings()
            {
                AttributeFilterSettings = new List<string>();
            }

            public FilterSettings( string settingsString )
                : this()
            {
                FromSelectionString( settingsString );
            }

            public override bool IsValid
            {
                get
                {
                    if ( string.IsNullOrWhiteSpace( AttributeKey ) )
                    {
                        return false;
                    }

                    return true;
                }
            }

            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Attribute Key
                AttributeKey = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 );

                // Parameter 2+: Remaining Parameters represent settings that are specific to the filter for the selected Attribute.
                AttributeFilterSettings = new List<string>();

                AttributeFilterSettings.AddRange( parameters.Skip( 1 ) );

                // Derive GroupTypeId from AttributeKey
                var attributeGuid = AttributeKey.Split( '_' ).LastOrDefault().AsGuidOrNull();
                AttributeCache attribute = null;
                if ( attributeGuid.HasValue )
                {
                    attribute = AttributeCache.Get( attributeGuid.Value );
                }

                this.GroupTypeId = null;
                if ( attribute != null && attribute.EntityTypeQualifierColumn == "GroupTypeId" )
                {
                    this.GroupTypeId = attribute.EntityTypeQualifierValue.AsIntegerOrNull();
                }
            }

            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( AttributeKey.ToStringSafe() );

                settings.AddRange( AttributeFilterSettings );

                return settings;
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
            get { return typeof( Rock.Model.GroupMember ).FullName; }
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

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/GroupMember/groupMemberAttributesFilter.obs";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            var groupTypeService = new GroupTypeService( rockContext );

            var groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();

            var groupTypeGuidsById = new Dictionary<int, Guid>();
            var fieldFilterSourcesByGroupType = new Dictionary<Guid, List<FieldFilterSourceBag>>();

            // Prime the dictionary with empty lists to make it easier to add the entity fields
            foreach ( var groupTypeItem in groupTypes )
            {
                groupTypeGuidsById.Add( groupTypeItem.Id, groupTypeItem.Guid );
                fieldFilterSourcesByGroupType.AddOrReplace( groupTypeItem.Guid, new List<FieldFilterSourceBag>() );
            }

            // Add a list for when no group type is selected
            fieldFilterSourcesByGroupType.AddOrReplace( Guid.Empty, new List<FieldFilterSourceBag>() );

            var attributes = AttributeCache.AllForEntityType<Rock.Model.GroupMember>().Where( a => a.IsActive ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
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
                // Add to each list, because it's global (not specific to a group type)
                else
                {
                    foreach ( var filterSourceList in fieldFilterSourcesByGroupType )
                    {
                        filterSourceList.Value.Add( source );
                    }
                }

            }

            data.AddOrReplace( "fieldFilterSources", fieldFilterSourcesByGroupType.ToCamelCaseJson( false, true ) );

            if ( selection.IsNullOrWhiteSpace() )
            {
                return data;
            }

            var config = new FilterSettings( selection );

            if ( config == null )
            {
                return data;
            }

            var groupType = groupTypeService.Get( config.GroupTypeId ?? 0 );
            data.AddOrReplace( "groupType", groupType?.ToListItemBag().ToCamelCaseJson( false, true ) ?? "" );

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
            var config = new FilterSettings();

            var groupTypeGuid = data.GetValueOrNull( "groupType" )?.FromJsonOrNull<ListItemBag>()?.Value ?? string.Empty;
            var groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid.AsGuid() );
            config.GroupTypeId = groupType?.Id;

            var filterRule = data.GetValueOrNull( "filterRule" ).FromJsonOrNull<FieldFilterRuleBag>();
            if ( filterRule == null || filterRule.AttributeGuid == null )
            {
                return config.ToSelectionString();
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

            return config.ToSelectionString();
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
            return "Group Member Attribute Values";
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
            return "GroupMemberPropertySelection( $content )";
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
            string result = "Member Property";
            if ( !string.IsNullOrWhiteSpace( selection ) )
            {
                var settings = new FilterSettings( selection );
                var entityField = GetGroupMemberAttributes( settings.GroupTypeId ).FirstOrDefault( f => f.UniqueName == settings.AttributeKey );
                if ( entityField != null )
                {
                    result = entityField.FormattedFilterDescription( settings.AttributeFilterSettings );
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Updates the selection from page parameters if there is a page parameter for the selection
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public override string UpdateSelectionFromPageParameters( string selection, Rock.Web.UI.RockBlock rockBlock )
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
            groupTypePicker.Label = "Group Type";
            groupTypePicker.AddCssClass( "js-group-type-picker" );
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

            var entityFields = GetGroupMemberAttributes( groupTypePicker.SelectedGroupTypeId );

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

            var entityFields = GetGroupMemberAttributes( groupTypePicker.SelectedGroupTypeId );

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
                    var entityFields = GetGroupMemberAttributes( groupTypePicker.SelectedGroupTypeId );

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
            var settings = new FilterSettings();

            if ( controls.Length > 0 )
            {
                var containerControl = controls[0] as DynamicControlsPanel;
                if ( containerControl.Controls.Count >= 1 )
                {
                    // note: since this datafilter creates additional controls outside of CreateChildControls(), we'll use our _controlsToRender instead of the controls parameter
                    GroupTypePicker groupTypePicker = containerControl.Controls[0] as GroupTypePicker;
                    Guid groupTypeGuid = Guid.Empty;
                    var groupTypeId = groupTypePicker.SelectedGroupTypeId;

                    if ( containerControl.Controls.Count > 1 )
                    {
                        DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                        settings.AttributeKey = ddlProperty.SelectedValue;
                        var entityFields = GetGroupMemberAttributes( groupTypeId );
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

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            if ( controls.Length > 0 )
            {
                int? groupTypeId = settings.GroupTypeId;

                var containerControl = controls[0] as DynamicControlsPanel;
                if ( containerControl.Controls.Count > 0 )
                {
                    GroupTypePicker groupTypePicker = containerControl.Controls[0] as GroupTypePicker;
                    groupTypePicker.SelectedGroupTypeId = groupTypeId;
                    EnsureSelectedGroupTypeControls( groupTypePicker );
                }

                if ( containerControl.Controls.Count > 1 )
                {
                    DropDownList ddlProperty = containerControl.Controls[1] as DropDownList;
                    var entityFields = GetGroupMemberAttributes( groupTypeId );

                    var panelControls = new List<Control>();
                    panelControls.AddRange( containerControl.Controls.OfType<Control>() );

                    var parameters = new List<string> { settings.AttributeKey };

                    parameters.AddRange( settings.AttributeFilterSettings );

                    SetEntityFieldSelection( entityFields, ddlProperty, parameters, panelControls );
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
            var settings = new FilterSettings( selection );

            if ( settings.IsValid && settings.AttributeFilterSettings.Any() )
            {
                var entityFields = GetGroupMemberAttributes( settings.GroupTypeId );
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
        /// Gets the properties and attributes for the entity
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private List<EntityField> GetGroupMemberAttributes( int? groupTypeId )
        {
            List<EntityField> entityAttributeFields = new List<EntityField>();

            var fakeGroupMember = new Rock.Model.GroupMember();
            fakeGroupMember.Group = new Rock.Model.Group();
            if ( groupTypeId.HasValue )
            {
                fakeGroupMember.Group.GroupTypeId = groupTypeId.Value;
            }
            else
            {
                //// if no GroupTypeId was specified, just set the GroupTypeId to 0
                //// NOTE: There could be GroupMember Attributes that are not specific to a GroupType
                fakeGroupMember.Group.GroupTypeId = 0;
            }

            Rock.Attribute.Helper.LoadAttributes( fakeGroupMember );
            var attributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();
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