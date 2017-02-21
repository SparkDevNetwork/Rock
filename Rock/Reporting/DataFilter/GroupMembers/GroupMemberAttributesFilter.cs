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
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.GroupMember
{
    /// <summary>
    /// Filter Group Members by their Attribute Values
    /// </summary>
    [Description( "Filter Group Members by their Attribute Values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Attributes Filter" )]
    public class GroupMemberAttributesFilter : EntityFieldFilter
    {
        #region Private Methods

        /// <summary>
        /// Gets the Attributes for a Group Member of a specific Group Type.
        /// </summary>
        /// <returns></returns>
        private List<EntityField> GetGroupMemberAttributes()
        {
            var entityAttributeFields = new Dictionary<string, EntityField>();
            var context = new RockContext();

            var attributeService = new AttributeService( context );
            var groupTypeService = new GroupTypeService( context );

            var groupMemberEntityTypeId = EntityTypeCache.GetId( typeof( Model.GroupMember ) );

            var groupMemberAttributes = attributeService.Queryable()
                                                        .AsNoTracking()
                                                        .Where( a => a.EntityTypeId == groupMemberEntityTypeId )
                                                        .Join( groupTypeService.Queryable(), a => a.EntityTypeQualifierValue, gt => gt.Id.ToString(),
                                                               ( a, gt ) =>
                                                               new
                                                               {
                                                                   Attribute = a,
                                                                   AttributeKey = a.Key,
                                                                   FieldTypeName = a.FieldType.Name,
                                                                   a.FieldTypeId,
                                                                   AttributeName = a.Name,
                                                                   GroupTypeName = gt.Name
                                                               } )
                                                        .GroupBy( x => x.AttributeName )
                                                        .ToList();

            foreach ( var attributesByName in groupMemberAttributes )
            {
                var attributeNameAndTypeGroups = attributesByName.GroupBy( x => x.FieldTypeId ).ToList();

                bool requiresTypeQualifier = ( attributeNameAndTypeGroups.Count > 1 );

                foreach ( var attributeNameAndTypeGroup in attributeNameAndTypeGroups )
                {
                    foreach ( var attribute in attributeNameAndTypeGroup )
                    {
                        string fieldKey;
                        string fieldName;

                        if ( requiresTypeQualifier )
                        {
                            fieldKey = attribute.AttributeName + "_" + attribute.FieldTypeId;

                            fieldName = string.Format( "{0} [{1}]", attribute.AttributeName, attribute.FieldTypeName );
                        }
                        else
                        {
                            fieldName = attribute.AttributeName;
                            fieldKey = attribute.AttributeName;
                        }

                        if ( entityAttributeFields.ContainsKey( fieldKey ) )
                        {
                            continue;
                        }

                        var attributeCache = AttributeCache.Read( attribute.Attribute );

                        var entityField = EntityHelper.GetEntityFieldForAttribute( attributeCache );

                        if ( entityField != null )
                        {
                            entityField.Title = fieldName;
                            entityField.AttributeGuid = null;

                            entityAttributeFields.Add( fieldKey, entityField );
                        }
                    }
                }
            }

            int index = 0;
            var sortedFields = new List<EntityField>();
            foreach ( var entityProperty in entityAttributeFields.Values.OrderBy( p => p.Title ).ThenBy( p => p.Name ) )
            {
                entityProperty.Index = index;
                index++;
                sortedFields.Add( entityProperty );
            }

            return sortedFields;
        }

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public List<string> AttributeFilterSettings;
            public string AttributeKey;

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
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.GroupMember ).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Group Member Attributes";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "GroupPropertySelection( $content )";
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A string containing the user-friendly description of the settings.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Member Property";

            var settings = new FilterSettings( selection );

            var entityField = GetGroupMemberAttributes().FirstOrDefault( f => f.Name == settings.AttributeKey );
            if ( entityField != null )
            {
                result = entityField.FormattedFilterDescription( settings.AttributeFilterSettings );
            }

            return result;
        }

        private const string _CtlGroup = "pnlGroupAttributeFilterControls";
        private const string _CtlProperty = "ddlProperty";

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// Implement this version of CreateChildControls if your DataFilterComponent works the same in all filter modes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var pnlGroupAttributeFilterControls = new DynamicControlsPanel();
            pnlGroupAttributeFilterControls.ID = filterControl.GetChildControlInstanceName( _CtlGroup );
            pnlGroupAttributeFilterControls.CssClass = "js-container-control";
            filterControl.Controls.Add( pnlGroupAttributeFilterControls );

            pnlGroupAttributeFilterControls.Controls.Clear();

            // Create the field selection dropdown
            var ddlProperty = new RockDropDownList();
            ddlProperty.ID = pnlGroupAttributeFilterControls.GetChildControlInstanceName( _CtlProperty );

            pnlGroupAttributeFilterControls.Controls.Add( ddlProperty );

            // Add empty selection as first item.
            ddlProperty.Items.Add( new ListItem() );

            foreach ( var entityField in GetGroupMemberAttributes() )
            {
                string controlId = pnlGroupAttributeFilterControls.GetChildControlInstanceName( entityField.UniqueName );
                var control = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, controlId, true, filterControl.FilterMode );
                if ( control != null )
                {
                    // Add the field to the dropdown of available fields
                    ddlProperty.Items.Add( new ListItem( entityField.Title, entityField.UniqueName ) );
                    pnlGroupAttributeFilterControls.Controls.Add( control );
                }
            }

            ddlProperty.AutoPostBack = true;

            return new Control[] { pnlGroupAttributeFilterControls };
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// Implement this version of RenderControls if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var pnlGroupAttributeFilterControls = controls.GetByName<DynamicControlsPanel>( _CtlGroup );
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );

            if ( pnlGroupAttributeFilterControls == null )
            {
                return;
            }

            var panelControls = new List<Control>();
            panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );

            var entityFields = GetGroupMemberAttributes();

            RenderEntityFieldsControls( entityType, filterControl, writer, entityFields, ddlProperty, panelControls, pnlGroupAttributeFilterControls.ID, FilterMode.AdvancedFilter );
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            // Get selection control instances.
            var pnlGroupAttributeFilterControls = controls.GetByName<DynamicControlsPanel>( _CtlGroup );
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );

            if ( pnlGroupAttributeFilterControls == null )
            {
                return null;
            }

            var settings = new FilterSettings();
            settings.AttributeKey = ddlProperty.SelectedValue;

            var entityFields = GetGroupMemberAttributes();
            var entityField = entityFields.FirstOrDefault( f => f.UniqueName == ddlProperty.SelectedValue );
            if ( entityField != null )
            {
                var panelControls = new List<Control>();
                panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );

                var control = panelControls.FirstOrDefault( c => c.ID.EndsWith( "_" + entityField.UniqueName ) );

                if ( control != null )
                {
                    entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, FilterMode.AdvancedFilter ).ForEach( v => settings.AttributeFilterSettings.Add( v ) );
                }
            }

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            // Get selection control instances.
            var pnlGroupAttributeFilterControls = controls.GetByName<DynamicControlsPanel>( _CtlGroup );
            var ddlProperty = controls.GetByName<DropDownList>( _CtlProperty );

            if ( pnlGroupAttributeFilterControls == null )
            {
                return;
            }

            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            if ( settings.AttributeFilterSettings.Any() )
            {
                var entityFields = GetGroupMemberAttributes();

                var panelControls = new List<Control>();

                panelControls.AddRange( pnlGroupAttributeFilterControls.Controls.OfType<Control>() );

                var parameters = new List<string> { settings.AttributeKey };

                parameters.AddRange( settings.AttributeFilterSettings );

                SetEntityFieldSelection( entityFields, ddlProperty, parameters, panelControls );
            }
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            if ( settings.IsValid
                && settings.AttributeFilterSettings.Any() )
            {
                var entityFields = GetGroupMemberAttributes();
                var entityField = entityFields.FirstOrDefault( f => f.Name == settings.AttributeKey );
                if ( entityField != null )
                {
                    return GetAttributeExpression( serviceInstance, parameterExpression, entityField, settings.AttributeFilterSettings );
                }
            }

            return null;
        }

        #endregion
    }
}