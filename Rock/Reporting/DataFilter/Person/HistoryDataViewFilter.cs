﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///     A DataFilter that selects People by their presence in the set of records returned by a History Data View.
    /// </summary>
    [Description( "Select people that are represented in the History entries returned by a specified Data View" )]
    [Export( typeof(DataFilterComponent) )]
    [ExportMetadata( "ComponentName", "History Data View" )]
    public class HistoryDataViewFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        ///     Settings for the Data Select Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public Guid? DataViewGuid;

            public FilterSettings()
            {
                //
            }

            public FilterSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// Indicates if the current settings are valid.
            /// </summary>
            /// <value>
            /// True if the settings are valid.
            /// </value>
            public override bool IsValid
            {
                get { return DataViewGuid.HasValue; }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                DataViewGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();
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

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof(Model.Person).FullName; }
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
            return "History Data View";
        }

        /// <summary>
        ///     Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        ///     will set the description of the filter to whatever is returned by this property.  If including script, the
        ///     controls parent container can be referenced through a '$content' variable that is set by the control before
        ///     referencing this property.
        /// </summary>
        /// <value>
        ///     The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var dataViewName = $('.rock-drop-down-list,select:first', $content).find(':selected').text();
  var result = 'History record is in filter: ' + dataViewName;
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
            var settings = new FilterSettings( selection );

            string result = "Referenced in History";

            if (!settings.IsValid)
            {
                return result;
            }

            using (var context = new RockContext())
            {
                var dataView = new DataViewService( context ).Get( settings.DataViewGuid.GetValueOrDefault() );

                result = string.Format( "Referenced in History filter: {0}",
                                        ( dataView != null ? dataView.ToString() : string.Empty ) );
            }

            return result;
        }

        private const string _CtlDataView = "ddlDataView";

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
            // Define Control: History Data View Picker
            var ddlDataView = new DataViewPicker();
            ddlDataView.ID = filterControl.GetChildControlInstanceName( _CtlDataView );
            ddlDataView.Label = "Connected to History Records";
            ddlDataView.Help = "A Data View that provides the set of History items to which the Person may be connected, either because the history is attached to their record or it affected them in some way.";

            filterControl.Controls.Add( ddlDataView );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Read( typeof(History) ).Id;
            ddlDataView.EntityTypeId = entityTypeId;

            return new Control[] {ddlDataView};
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
            var ddlDataView = controls.GetByName<DataViewPicker>( _CtlDataView );

            var settings = new FilterSettings();

            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( ddlDataView.SelectedValue );

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
            var ddlDataView = controls.GetByName<DataViewPicker>( _CtlDataView );

            var settings = new FilterSettings( selection );

            if (!settings.IsValid)
            {
                return;
            }

            ddlDataView.SelectedValue = DataComponentSettingsHelper.GetDataViewId( settings.DataViewGuid ).ToStringSafe();
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

            var context = (RockContext)serviceInstance.Context;

            // Get the Data View that defines the set of related records from which entities can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the related records.
            var historyService = new HistoryService( context );

            var historyQuery = historyService.Queryable();

            if ( dataView != null )
            {
                historyQuery = DataComponentSettingsHelper.FilterByDataView( historyQuery, dataView, historyService );
            }

            // Select only those History records that are either related to a Person, or affect a Person.
            int personEntityTypeId = EntityTypeCache.GetId( typeof(Model.Person) ).GetValueOrDefault();

            historyQuery = historyQuery.Where( x => x.EntityTypeId == personEntityTypeId );

            // Get all of the People corresponding to the qualifying History records.
            var qry = new PersonService( context ).Queryable()
                                                  .Where(
                                                         p =>
                                                         historyQuery.Any(
                                                                          h =>
                                                                          h.EntityTypeId == personEntityTypeId
                                                                          && h.EntityId == p.Id
                                                                          || ( h.RelatedEntityTypeId == personEntityTypeId && h.RelatedEntityId == p.Id ) ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}