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
using Rock.Net;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.ConnectionRequest
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Select all the requests where the requester is the same person as the people returned from this other data view." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Data View" )]
    [Rock.SystemGuid.EntityTypeGuid( "8C05C3F9-4AB4-41D1-9311-214A9AD6BCE0" )]
    public class PersonDataViewFilter : DataFilterComponent, IRelatedChildDataView
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
            get { return typeof( Model.ConnectionRequest ).FullName; }
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

        /// <inheritdoc/>
        public override string ObsidianFileUrl => "~/Obsidian/Reporting/DataFilters/ConnectionRequest/personDataViewFilter.obs";

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var settings = new SelectSettings( selection );
            var data = new Dictionary<string, string>();

            if ( !settings.IsValid )
            {
                return data;
            }

            var dataView = new DataViewService( rockContext ).Get( settings.DataViewGuid.GetValueOrDefault() );

            data.AddOrReplace( "dataView", dataView?.ToListItemBag().ToCamelCaseJson( false, true ) );

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var dataView = "";
            var dataViewJson = data.GetValueOrNull( "dataView" );

            if ( dataViewJson != null )
            {
                var dataViewBag = dataViewJson.FromJsonOrNull<ListItemBag>();
                dataView = dataViewBag.Value ?? string.Empty;
            }

            return dataView;
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
            return "People - matched to the Requester";
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
    var result = 'People - matched to the Requestor';

    var dataViewName = $('.js-data-view-picker', $content).find('.js-item-name-value').val().trim();
    result += ' ""' + dataViewName + '""';
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

                result = string.Format( "People - matched to the Requestor \"{0}\"", dataView != null ? dataView.ToString() : string.Empty );
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

            var context = ( RockContext ) serviceInstance.Context;

            //
            // Evaluate the Data View that defines the candidate Groups.
            //
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            var personService = new PersonService( context );

            var personQuery = personService.Queryable();

            if ( dataView != null )
            {
                personQuery = DataComponentSettingsHelper.FilterByDataView( personQuery, dataView, personService );
            }

            var personKeys = personQuery.Select( x => x.Id );

            //
            // Construct the Query to return the list of Group Members matching the filter conditions.
            //
            var connectionRequestQuery = new ConnectionRequestService( context ).Queryable();

            // Filter By Person.
            connectionRequestQuery = connectionRequestQuery.Where( p => personKeys.Contains( p.PersonAlias.PersonId ) );

            var selectExpression = FilterExpressionExtractor.Extract<Model.ConnectionRequest>( connectionRequestQuery, parameterExpression, "p" );

            return selectExpression;
        }

        private const string _CtlDataView = "dvpDataView";

#if WEBFORMS

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
            // Define Control: Person Data View Picker
            var dvpDataView = new DataViewItemPicker();
            dvpDataView.ID = filterControl.GetChildControlInstanceName( _CtlDataView );
            dvpDataView.CssClass = "js-data-view-picker";
            dvpDataView.Label = "People from Data View";
            dvpDataView.Help = "A Data View that filters the People included in the result.";
            filterControl.Controls.Add( dvpDataView );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Get( typeof( Model.Person ) ).Id;
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

        /// <summary>
        /// Gets the related data view identifier.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public int? GetRelatedDataViewId( Control[] controls )
        {
            if ( controls == null )
            {
                return null;
            }

            var ddlDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            if ( ddlDataView == null )
            {
                return null;
            }

            return ddlDataView.SelectedValueAsId();
        }

#endif

        #endregion

        #region Settings

        /// <summary>
        ///     Settings for the Data Select Component "Group Member/Group Data View".
        /// </summary>
        private class SelectSettings : SettingsStringBase
        {
            public Guid? DataViewGuid { get; set; }

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