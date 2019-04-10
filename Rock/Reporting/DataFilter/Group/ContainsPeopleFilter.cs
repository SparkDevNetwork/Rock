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

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    ///     A Data Filter to select Groups based on the number of group members that also exist in a Person Data View.
    /// </summary>
    [Description( "Filter groups based on the number of group members that also exist in a filtered set of people." )]
    [Export( typeof(DataFilterComponent) )]
    [ExportMetadata( "ComponentName", "Contains People" )]
    public class ContainsPeopleFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public int PersonCount;
            public ComparisonType PersonCountComparison = ComparisonType.GreaterThan;
            public Guid? PersonDataViewGuid;

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
                get
                {
                    return PersonDataViewGuid.HasValue;
                }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Person Data View
                PersonDataViewGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();

                // Parameter 2: Person Count Comparison
                PersonCountComparison = DataComponentSettingsHelper.GetParameterAsEnum( parameters, 1, ComparisonType.GreaterThan );

                // Parameter 3: Person Count
                PersonCount = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 2 ).AsInteger();
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

                settings.Add( PersonDataViewGuid.ToStringSafe() );
                settings.Add( PersonCountComparison.ToStringSafe() );
                settings.Add( PersonCount.ToStringSafe() );

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
            get { return typeof(Model.Group).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Member Filters"; }
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
            return "Contains People";
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
    var dataViewName = $('.rock-drop-down-list,select:first', $content).find(':selected').text();
    var comparisonName = $('.js-filter-compare', $content).find(':selected').text();
    var comparisonCount = $('.js-member-count', $content).val();

    result = 'Members matching Person filter';
    result += ' ""' + dataViewName + '""';
    result += ' ' + comparisonName;
    result += ' ' + comparisonCount;
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

            string result = GetTitle( null );

            if (!settings.IsValid)
            {
                return result;
            }

            using (var context = new RockContext())
            {
                var dataView = new DataViewService( context ).Get( settings.PersonDataViewGuid.GetValueOrDefault() );

                result = string.Format( "Members matching Person filter \"{0}\" is {1} {2}",
                                        ( dataView != null ? dataView.ToString() : string.Empty ),
                                        settings.PersonCountComparison.ConvertToString(),
                                        settings.PersonCount );
            }

            return result;
        }

        private const string _CtlDataView = "dvpDataView";
        private const string _CtlComparison = "ddlComparison";
        private const string _CtlMemberCount = "nbMemberCount";

        private const ComparisonType CountComparisonTypesSpecifier =
            ComparisonType.EqualTo |
            ComparisonType.NotEqualTo |
            ComparisonType.GreaterThan |
            ComparisonType.GreaterThanOrEqualTo |
            ComparisonType.LessThan |
            ComparisonType.LessThanOrEqualTo;

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
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
            dvpDataView.Label = "Contains People from this Data View";
            dvpDataView.Help = "A Person Data View that provides the set of possible Group Members.";
            filterControl.Controls.Add( dvpDataView );

            var ddlCompare = ComparisonHelper.ComparisonControl( CountComparisonTypesSpecifier );
            ddlCompare.Label = "where the number of matching Group Members is";
            ddlCompare.ID = filterControl.GetChildControlInstanceName( _CtlComparison );
            ddlCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlCompare );

            var nbCount = new NumberBox();
            nbCount.Label = "&nbsp;";
            nbCount.ID = filterControl.GetChildControlInstanceName( _CtlMemberCount );
            nbCount.AddCssClass( "js-filter-control js-member-count" );
            nbCount.FieldName = "Member Count";
            filterControl.Controls.Add( nbCount );

            // Populate the Data View Picker
            dvpDataView.EntityTypeId = EntityTypeCache.Get( typeof(Model.Person) ).Id;

            return new Control[] {dvpDataView, ddlCompare, nbCount};
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            var ddlCompare = controls.GetByName<RockDropDownList>( _CtlComparison );
            var nbValue = controls.GetByName<NumberBox>( _CtlMemberCount );

            dvpDataView.RenderControl( writer );

            // Comparison Row
            writer.AddAttribute( "class", "row form-row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Comparison Type
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            // Comparison Value
            writer.AddAttribute( "class", "col-md-8 vertical-align-bottom" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets a formatted string representing the current filter control values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            var ddlCompare = controls.GetByName<RockDropDownList>( _CtlComparison );
            var nbValue = controls.GetByName<NumberBox>( _CtlMemberCount );

            var settings = new FilterSettings();

            settings.PersonDataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( dvpDataView.SelectedValue );
            settings.PersonCountComparison = ddlCompare.SelectedValueAsEnum<ComparisonType>( ComparisonType.GreaterThan );
            settings.PersonCount = nbValue.Text.AsInteger();

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the filter control values from a formatted string.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var dvpDataView = controls.GetByName<DataViewItemPicker>( _CtlDataView );
            var ddlCompare = controls.GetByName<RockDropDownList>( _CtlComparison );
            var nbValue = controls.GetByName<NumberBox>( _CtlMemberCount );

            var settings = new FilterSettings( selection );

            if (!settings.IsValid)
            {
                return;
            }

            dvpDataView.SetValue( DataComponentSettingsHelper.GetDataViewId( settings.PersonDataViewGuid ) );
            ddlCompare.SelectedValue = settings.PersonCountComparison.ConvertToInt().ToString();
            nbValue.Text = settings.PersonCount.ToString();
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

            //
            // Define Candidate People.
            //

            // Get the Person Data View that defines the set of candidates from which matching Group Members can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.PersonDataViewGuid, context );

            var personService = new PersonService( context );

            var personQuery = personService.Queryable();

            if (dataView != null)
            {
                personQuery = DataComponentSettingsHelper.FilterByDataView( personQuery, dataView, personService );
            }

            var personKeys = personQuery.Select( x => x.Id );

            //
            // Construct the Query to return the list of Groups matching the filter conditions.
            //
            var comparisonType = settings.PersonCountComparison;
            int memberCountValue = settings.PersonCount;

            var memberCountQuery = new GroupService( context ).Queryable();

            var memberCountEqualQuery = memberCountQuery.Where( g => g.Members.Count( gm => personKeys.Contains( gm.PersonId ) ) == memberCountValue );

            var compareEqualExpression = FilterExpressionExtractor.Extract<Model.Group>( memberCountEqualQuery, parameterExpression, "g" ) as BinaryExpression;
            var result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, 0 );

            return result;
        }

        #endregion
    }
}