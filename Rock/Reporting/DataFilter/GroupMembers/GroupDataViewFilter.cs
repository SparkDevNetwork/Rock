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

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.GroupMember
{
    /// <summary>
    ///     A Data Filter to select Group Members according to their membership of Groups from a Group Data View.
    /// </summary>
    [Description( "Select Group Members according to their membership of Groups from a Group Data View." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Data View" )]
    public class GroupDataViewFilter : DataFilterComponent
    {
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
            get { return "Related Data Views"; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Group Data View";
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
            return @"
function ()
{    
    var result = 'Members of Groups in Data View';
    
    var dataViewName = $('.rock-drop-down-list,select:first', $content).find(':selected').text();    
    result += ' ""' + dataViewName + '""';

    var groupMemberStatus = $('.js-group-member-status option:selected', $content).text();    
    if (groupMemberStatus)
    {
       result = result + ', with Status:' + groupMemberStatus;
    }

    var groupMemberRole = $('.js-group-member-role option:selected', $content).text();    
    if (groupMemberRole)
    {
       result = result + ', with Role:' + groupMemberRole;
    }

   return result;
}
";
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
            var settings = new SelectSettings( selection );

            string result = GetTitle( null );

            if ( !settings.IsValid )
            {
                return result;
            }

            using ( var context = new RockContext() )
            {
                var dataView = new DataViewService( context ).Get( settings.DataViewGuid.GetValueOrDefault() );

                result = string.Format( "Members of Groups in Data View \"{0}\"", (dataView != null ? dataView.ToString() : string.Empty ));
            }

            return result;
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
            var settings = new SelectSettings( selection );

            var context = (RockContext)serviceInstance.Context;

            //
            // Evaluate the Data View that defines the candidate Groups.
            //
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );
            
            var groupService = new GroupService( context );

            var groupQuery = groupService.Queryable();

            if (dataView != null)
            {
                groupQuery = DataComponentSettingsHelper.FilterByDataView( groupQuery, dataView, groupService );
            }
            else
            {
                // Apply a default Group filter to only show Groups that would be visible in a Group List.
                groupQuery = groupQuery.Where( x => x.GroupType.ShowInGroupList );
            }

            var groupKeys = groupQuery.Select( x => x.Id );

            //
            // Construct the Query to return the list of Group Members matching the filter conditions.
            //
            var groupMemberQuery = new GroupMemberService( context ).Queryable();

            // Filter By Group.
            groupMemberQuery = groupMemberQuery.Where( gm => groupKeys.Contains( gm.GroupId ) );

            var selectExpression = FilterExpressionExtractor.Extract<Model.GroupMember>( groupMemberQuery, parameterExpression, "gm" );

            return selectExpression;
        }

        private const string _CtlDataView = "dvpDataView";

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
            // Define Control: Group Data View Picker
            var dvpDataView = new DataViewItemPicker();
            dvpDataView.ID = filterControl.GetChildControlInstanceName( _CtlDataView );
            dvpDataView.Label = "Is Member of Group from Data View";
            dvpDataView.Help = "A Data View that filters the Groups included in the result. If no value is selected, any Groups that would be visible in a Group List will be included.";
            filterControl.Controls.Add( dvpDataView );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Get( typeof( Model.Group ) ).Id;
            dvpDataView.EntityTypeId = entityTypeId;

            return new Control[] { dvpDataView };
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
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new SelectSettings();

            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( dvpDataView.SelectedValue );

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
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new SelectSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            if ( settings.DataViewGuid.HasValue )
            {
                var dsService = new DataViewService( new RockContext() );

                var dataView = dsService.Get( settings.DataViewGuid.Value );

                if ( dataView != null )
                {
                    dvpDataView.SetValue( dataView );
                }
            }
        }

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Select Component "Group Member/Group Data View".
        /// </summary>
        private class SelectSettings : SettingsStringBase
        {
            public Guid? DataViewGuid;

            public SelectSettings()
            {
                //
            }

            public SelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                DataViewGuid = DataComponentSettingsHelper.GetParameterOrDefault( parameters, 0, string.Empty ).AsGuidOrNull();
            }

            /// <summary>
            /// Gets an ordered set of property values that can be used to construct the
            /// settings string.
            /// </summary>
            /// <returns>
            /// An ordered collection of strings representing the parameter values.
            /// </returns>
            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                settings.Add( DataViewGuid.ToStringSafe() );                

                return settings;
            }
        }

        #endregion
    }
}