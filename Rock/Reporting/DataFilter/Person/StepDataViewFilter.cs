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

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// A DataFilter to select people that are represented in the entries returned by a specified Step Data View.
    /// </summary>
    [Description( "Select people that are represented in the Step entries returned by a specified Data View" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Step Filter" )]
    public class StepDataViewFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        /// Settings for the Data Select Component.
        /// </summary>
        public class FilterSettings : SettingsStringBase
        {
            /// <summary>
            /// The data view unique identifier
            /// </summary>
            public Guid? DataViewGuid;

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            public FilterSettings()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FilterSettings"/> class.
            /// </summary>
            /// <param name="settingsString">The settings string.</param>
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
            get { return "Related Data Views"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Step Data View";
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
            return @"
function() {
  var dataViewName = $('.rock-drop-down-list,select:first', $content).find(':selected').text();
  var result = 'Step is in filter: ' + dataViewName;
  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var settings = new FilterSettings( selection );

            string result = "In Step Data View";

            if ( !settings.IsValid )
            {
                return result;
            }

            using ( var context = new RockContext() )
            {
                var dataView = new DataViewService( context ).Get( settings.DataViewGuid.GetValueOrDefault() );

                result = string.Format( "In Step Data View filter: {0}",
                                        ( dataView != null ? dataView.ToString() : string.Empty ) );
            }

            return result;
        }

        private const string _CtlDataView = "dvpDataView";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Define Control: Data View Picker
            var dvpDataView = new DataViewItemPicker();
            dvpDataView.ID = filterControl.GetChildControlInstanceName( _CtlDataView );
            dvpDataView.Label = "In Step Data View";
            dvpDataView.Help = "A Step Data View that provides the set of possible step participants.";

            filterControl.Controls.Add( dvpDataView );

            // Populate the Data View Picker
            int entityTypeId = EntityTypeCache.Get( typeof( Model.Step ) ).Id;
            dvpDataView.EntityTypeId = entityTypeId;

            return new Control[] { dvpDataView };
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
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new FilterSettings();

            settings.DataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( dvpDataView.SelectedValue );

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );

            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            dvpDataView.SetValue( DataComponentSettingsHelper.GetDataViewId( settings.DataViewGuid ) );
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            var context = ( RockContext ) serviceInstance.Context;

            // Get the Data View that defines the set of related records from which entities can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.DataViewGuid, context );

            // Evaluate the Data View that defines the related records.
            var relatedEntityService = new StepService( context );

            var relatedEntityQuery = relatedEntityService.Queryable();

            if ( dataView != null )
            {
                relatedEntityQuery = DataComponentSettingsHelper.FilterByDataView( relatedEntityQuery, dataView, relatedEntityService );
            }

            // Get all of the People corresponding to the qualifying related records.
            var personService = new PersonService( ( RockContext ) serviceInstance.Context );

            var qry = personService.Queryable()
                        .Where( p => relatedEntityQuery.Any( xx => xx.PersonAlias.PersonId == p.Id ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}