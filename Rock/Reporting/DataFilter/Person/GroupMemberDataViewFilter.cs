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
    ///     A Data Filter to select People by their inclusion in a Group Member Data View.
    /// </summary>
    [Description( "Select people according to their inclusion in a Group Member Data View." )]
    [Export( typeof(DataFilterComponent) )]
    [ExportMetadata( "ComponentName", "Group Member Data View" )]
    public class GroupMemberDataViewFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        ///     Settings for the Data Filter Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public Guid? GroupMemberDataViewGuid;
            public int? MemberCount;
            public ComparisonType? MemberCountComparison = ComparisonType.GreaterThan;

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
                    if (!GroupMemberDataViewGuid.HasValue)
                    {
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Group Member Data View
                GroupMemberDataViewGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();

                // Parameter 2: Group Member Count Comparison Type
                MemberCountComparison = DataComponentSettingsHelper.GetParameterAsEnum( parameters, 1, ComparisonType.GreaterThan );

                // Parameter 3: Group Member Count
                MemberCount = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 2 ).AsIntegerOrNull();
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

                settings.Add( GroupMemberDataViewGuid.ToStringSafe() );
                settings.Add( MemberCountComparison.ToStringSafe() );
                settings.Add( MemberCount.ToStringSafe() );

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
            return "Group Member Data View";
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
    var dataViewName = $('.data-view-picker', $content).find(':selected').text();
    var comparisonName = $('.js-filter-compare', $content).find(':selected').text();
    var comparisonCount = $('.js-member-count', $content).val();

    result = 'Group Memberships in Data View';
    result += ' ""' + dataViewName + '""';
    result += ' is ' + comparisonName;
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
                var dataView = new DataViewService( context ).Get( settings.GroupMemberDataViewGuid.GetValueOrDefault() );

                result = string.Format( "Group Memberships in Data View \"{0}\" is {1} {2}",
                                        ( dataView != null ? dataView.ToString() : string.Empty ),
                                        settings.MemberCountComparison.ConvertToString(),
                                        settings.MemberCount );
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
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField parentControl )
        {
            // Define Control: Group Member Data View Picker
            var dvpDataView = new DataViewItemPicker();
            dvpDataView.ID = parentControl.GetChildControlInstanceName( _CtlDataView );
            dvpDataView.Label = "Has Group Memberships in this Data View";
            dvpDataView.Help = "A Group Member Data View that provides the set of possible Group Members.";
            parentControl.Controls.Add( dvpDataView );

            var ddlCompare = ComparisonHelper.ComparisonControl( CountComparisonTypesSpecifier, false );
            ddlCompare.Label = "where the number of matching Group Memberships is";
            ddlCompare.ID = parentControl.GetChildControlInstanceName( _CtlComparison );
            ddlCompare.AddCssClass( "js-filter-compare" );
            parentControl.Controls.Add( ddlCompare );

            var nbCount = new NumberBox();
            nbCount.Label = "&nbsp;";
            nbCount.ID = parentControl.GetChildControlInstanceName( _CtlMemberCount );
            nbCount.AddCssClass( "js-filter-control js-member-count" );
            nbCount.FieldName = "Membership Count";
            parentControl.Controls.Add( nbCount );

            // Populate the Data View Picker
            dvpDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.GroupMember) ).Id;

            return new Control[] {dvpDataView, ddlCompare, nbCount};
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
            var ddlCompare = controls.GetByName<RockDropDownList>( _CtlComparison );
            var nbValue = controls.GetByName<NumberBox>( _CtlMemberCount );

            var settings = new FilterSettings();

            settings.GroupMemberDataViewGuid = DataComponentSettingsHelper.GetDataViewGuid( dvpDataView.SelectedValue );
            settings.MemberCountComparison = ddlCompare.SelectedValueAsEnum<ComparisonType>( ComparisonType.GreaterThan );
            settings.MemberCount = nbValue.Text.AsInteger();

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
            var ddlCompare = controls.GetByName<RockDropDownList>( _CtlComparison );
            var nbValue = controls.GetByName<NumberBox>( _CtlMemberCount );

            var settings = new FilterSettings( selection );

            if (!settings.IsValid)
            {
                return;
            }

            dvpDataView.SetValue( DataComponentSettingsHelper.GetDataViewId( settings.GroupMemberDataViewGuid ) );
            ddlCompare.SelectedValue = settings.MemberCountComparison.ConvertToInt().ToString();
            nbValue.Text = settings.MemberCount.ToString();
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
            // Define Candidate Group Members.
            //

            // Get the Group Member Data View that defines the set of candidates from which matching Group Members can be selected.
            var dataView = DataComponentSettingsHelper.GetDataViewForFilterComponent( settings.GroupMemberDataViewGuid, context );

            var memberService = new GroupMemberService( context );

            var memberQuery = memberService.Queryable();

            if (dataView != null)
            {
                memberQuery = DataComponentSettingsHelper.FilterByDataView( memberQuery, dataView, memberService );
            }

            //
            // Construct the Query to return the list of People matching the filter conditions.
            //

            var personQuery = new PersonService( context ).Queryable();

            BinaryExpression result;

            if (settings.MemberCountComparison.HasValue
                && settings.MemberCount.HasValue)
            {
                var comparisonType = settings.MemberCountComparison.Value;
                int memberCountValue = settings.MemberCount.Value;

                var memberKeys = memberQuery.Select( x => x.Id );

                var memberCountEqualQuery = personQuery.Where( p => p.Members.Count( gm => memberKeys.Contains( gm.Id ) ) == memberCountValue );

                var compareEqualExpression = FilterExpressionExtractor.Extract<Model.Person>( memberCountEqualQuery, parameterExpression, "p" ) as BinaryExpression;

                result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, 0 );
            }
            else
            {
                personQuery = personQuery.Where( p => memberQuery.Any(m => m.PersonId == p.Id ));

                result = FilterExpressionExtractor.Extract<Model.Person>( personQuery, parameterExpression, "p" ) as BinaryExpression;
            }

            return result;
        }

        #endregion
    }
}