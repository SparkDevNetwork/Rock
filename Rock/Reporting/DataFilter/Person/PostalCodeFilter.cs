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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people based on the zipcode of their family." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Postal Filter" )]
    public class PostalCodeFilter : DataFilterComponent
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
            return "Postal Code";
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
    var result = 'Postal Code';
    var compareTypeText = $('.js-filter-compare :selected', $content).text();
    if ( $('.js-filter-control', $content).is(':visible') ) {
        var compareValueSingle = $('.js-filter-control', $content).val()
        result += ' ' + compareTypeText + ' ' + (compareValueSingle || '');
    }
    else {
        result += ' ' + compareTypeText;
    }

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
            var values = selection.Split( '|' );
            if ( values.Length >= 2 )
            {
                string locationType = "";
                if ( values.Length >= 3 && values[0].AsInteger() != 0 )
                {
                    var groupLocationType = DefinedTypeCache.Get( SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() )
                        .DefinedValues.FirstOrDefault( dv => dv.Id == values[2].AsInteger() );
                    if ( groupLocationType != null )
                    {
                        locationType = groupLocationType.Value;
                    }
                }

                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                switch ( comparisonType )
                {
                    case ComparisonType.EqualTo:
                    case ComparisonType.NotEqualTo:
                    case ComparisonType.StartsWith:
                    case ComparisonType.Contains:
                    case ComparisonType.DoesNotContain:
                    case ComparisonType.EndsWith:
                        return string.Format( "{0} Postal Code {1} {2}", locationType, comparisonType.ConvertToString(), values[1] );
                    case ComparisonType.IsBlank:
                    case ComparisonType.IsNotBlank:
                        return string.Format( "{0} Postal Code {1}", locationType, comparisonType.ConvertToString() );
                    default:
                        break;
                }
            }
            return "Postal code filter";
        }

        /// <summary>
        /// The GroupPicker
        /// </summary>
        private RockTextBox tbPostalCode = null;
        private RockDropDownList ddlStringFilterComparison = null;
        private DefinedValuePicker dvpLocationType = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            ddlStringFilterComparison = ComparisonHelper.ComparisonControl( ComparisonHelper.StringFilterComparisonTypes );
            ddlStringFilterComparison.ID = string.Format( "{0}_{1}", filterControl.ID, controls.Count() );
            ddlStringFilterComparison.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlStringFilterComparison );
            controls.Add( ddlStringFilterComparison );

            tbPostalCode = new RockTextBox();
            tbPostalCode.ID = filterControl.ID + "_tbPostalCode";
            tbPostalCode.AddCssClass( "js-filter-control" );
            filterControl.Controls.Add( tbPostalCode );
            controls.Add( tbPostalCode );

            dvpLocationType = new DefinedValuePicker();
            dvpLocationType.ID = filterControl.ID + "_ddlLocationType";
            dvpLocationType.Label = "Location Type";
            dvpLocationType.DataValueField = "Id";
            dvpLocationType.DataTextField = "Value";
            DefinedTypeCache locationDefinedType = DefinedTypeCache.Get( SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() );
            dvpLocationType.DefinedTypeId = locationDefinedType.Id;
            dvpLocationType.Items.Insert( 0, new ListItem( "(All Location Types)", "" ) );
            filterControl.Controls.Add( dvpLocationType );
            controls.Add( dvpLocationType );

            return controls.ToArray();
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
            if ( controls.Count() >= 3 )
            {
                RockDropDownList ddlCompare = controls[0] as RockDropDownList;
                RockTextBox tbPostalCode = controls[1] as RockTextBox;
                writer.AddAttribute( "class", "row form-row field-criteria" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( "class", "col-md-4" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                ddlCompare.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( "class", "col-md-8" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                tbPostalCode.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();  // row
                RegisterFilterCompareChangeScript( filterControl );

                ( controls[2] as RockDropDownList ).RenderControl( writer );
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            RockDropDownList ddlCompare = controls[0] as RockDropDownList;
            RockTextBox nbPostalBox = controls[1] as RockTextBox;
            var locationTypeId = ( controls[2] as RockDropDownList ).SelectedValue;

            return string.Format( "{0}|{1}|{2}", ddlCompare.SelectedValue, nbPostalBox.Text, locationTypeId );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var values = selection.Split( '|' );
            if ( values.Length >= 2 )
            {
                ( controls[0] as RockDropDownList ).SelectedValue = values[0];
                ( controls[1] as RockTextBox ).Text = values[1];
            }
            if ( values.Length >= 3 )
            {
                ( controls[2] as RockDropDownList ).SelectedValue = values[2];
            }
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
            var values = selection.Split( '|' );

            ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
            string postalCode = values[1];

            var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();

            var groupLocationQry = new GroupLocationService( ( RockContext ) serviceInstance.Context ).Queryable();

            switch ( comparisonType )
            {
                case ComparisonType.EqualTo:
                    groupLocationQry = groupLocationQry.Where( gl => gl.Location.PostalCode == postalCode );
                    break;
                case ComparisonType.NotEqualTo:
                    groupLocationQry = groupLocationQry.Where( gl => gl.Location.PostalCode != postalCode );
                    break;
                case ComparisonType.StartsWith:
                    groupLocationQry = groupLocationQry.Where( gl => gl.Location.PostalCode.StartsWith( postalCode ) );
                    break;
                case ComparisonType.Contains:
                    groupLocationQry = groupLocationQry.Where( gl => gl.Location.PostalCode.Contains( postalCode ) );
                    break;
                case ComparisonType.DoesNotContain:
                    groupLocationQry = groupLocationQry.Where( gl => !gl.Location.PostalCode.Contains( postalCode ) );
                    break;
                case ComparisonType.IsBlank:
                    groupLocationQry = groupLocationQry
                        .Where( gl => gl.Location.PostalCode == null || gl.Location.PostalCode == string.Empty );
                    break;
                case ComparisonType.IsNotBlank:
                    groupLocationQry = groupLocationQry
                        .Where( gl => gl.Location.PostalCode != null && gl.Location.PostalCode != string.Empty );
                    break;
                case ComparisonType.EndsWith:
                    groupLocationQry = groupLocationQry.Where( gl => gl.Location.PostalCode.EndsWith( postalCode ) );
                    break;
                default:
                    break;
            }

            IQueryable<Rock.Model.Person> qry;

            //Limit by location type if applicable
            if ( values.Length >= 3 && values[2].AsInteger() != 0 )
            {
                int locationTypeId = values[2].AsInteger();
                groupLocationQry = groupLocationQry.Where( gl => gl.GroupLocationTypeValueId == locationTypeId );
            }

            var groupMemberQry = groupLocationQry.Select( gl => gl.Group )
                .Where( g => g.GroupType.Guid == familyGroupTypeGuid )
                .SelectMany( g => g.Members );


            // Families which do not have locations need to be added separately
            if ( comparisonType == ComparisonType.IsBlank
                || comparisonType == ComparisonType.DoesNotContain
                || comparisonType == ComparisonType.NotEqualTo )
            {
                IQueryable<Model.GroupMember> noLocationGroupMembersQry;

                if ( values.Length >= 3 && values[2].AsInteger() != 0 )
                {
                    int locationTypeId = values[2].AsInteger();
                    noLocationGroupMembersQry = new GroupService( ( RockContext ) serviceInstance.Context ).Queryable()
                        .Where( g =>
                            g.GroupType.Guid == familyGroupTypeGuid
                            && !g.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == locationTypeId ) )
                        .SelectMany( g => g.Members );
                }
                else
                {
                    noLocationGroupMembersQry = new GroupService( ( RockContext ) serviceInstance.Context ).Queryable()
                        .Where( g => g.GroupType.Guid == familyGroupTypeGuid && !g.GroupLocations.Any() )
                        .SelectMany( g => g.Members );
                }


                qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => groupMemberQry.Any( xx => xx.PersonId == p.Id ) || noLocationGroupMembersQry.Any( xx => xx.PersonId == p.Id ) );
            }
            else
            {
                qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => groupMemberQry.Any( xx => xx.PersonId == p.Id ) );
            }

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}