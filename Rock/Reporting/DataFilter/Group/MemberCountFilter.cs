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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter groups based on the number of members with status or leader criteria" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Member Count (Advanced)" )]
    public class MemberCountFilter : DataFilterComponent
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

        /// <summary>
        /// Gets a value indicating whether [simple member count mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [simple member count mode]; otherwise, <c>false</c>.
        /// </value>
        internal virtual bool SimpleMemberCountMode
        {
            get
            {
                return false;
            }
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
            return "Member Count (Advanced)";
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
function () {
    var result = 'Number of';
    result += ' ' + $('.js-member-status', $content).find(':selected').text();
    result += ' ' + $('.js-member-is-leader', $content).find(':selected').text();
    if (result.trim() == 'Number of')
    {
        result = 'Number of members';
    }

    result += ' ' + $('.js-filter-compare', $content).find(':selected').text();
    var countText = $('.js-member-count', $content).filter(':visible').length ? $('.js-member-count', $content).filter(':visible').val() : '';
    result += ' ' + countText;
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
            string result = "Member Count";
            if ( values.Length == 4 )
            {
                ComparisonType comparisonType = values[0].ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                int? memberCountValue = values[1].AsIntegerOrNull();
                bool? isLeader = values[2].AsBooleanOrNull();
                GroupMemberStatus? memberStatusValue = (GroupMemberStatus?)values[3].AsIntegerOrNull();

                result = "Number of";

                if ( memberStatusValue.HasValue )
                {
                    result += " " + memberStatusValue.Value.ConvertToString();
                }

                if ( isLeader.HasValue )
                {
                    result += isLeader.Value ? " Leader" : " Not Leader";
                }

                if (result.Trim() == "Number of")
                {
                    result = "Number of members";
                }

                string countText = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? string.Empty : memberCountValue.ToString();
                result += " " + comparisonType.ConvertToString() + " " + countText;
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.Label = "Count";
            ddlIntegerCompare.ID = string.Format( "{0}_ddlIntegerCompare", filterControl.ID );
            ddlIntegerCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlIntegerCompare );

            var nbMemberCount = new NumberBox();
            nbMemberCount.Label = "&nbsp;";
            nbMemberCount.ID = string.Format( "{0}_nbMemberCount", filterControl.ID );
            nbMemberCount.AddCssClass( "js-filter-control js-member-count" );
            nbMemberCount.FieldName = "Member Count";
            filterControl.Controls.Add( nbMemberCount );

            RockDropDownList ddlLeader = new RockDropDownList();
            ddlLeader.ID = string.Format( "{0}_ddlMemberType", filterControl.ID );
            ddlLeader.AddCssClass( "js-filter-control js-member-is-leader" );
            ddlLeader.Label = "Member Type";
            ddlLeader.Items.Add( new ListItem( string.Empty, string.Empty ) );
            ddlLeader.Items.Add( new ListItem( "Leader", "true" ) );
            ddlLeader.Items.Add( new ListItem( "Not Leader", "false" ) );
            filterControl.Controls.Add( ddlLeader );
            ddlLeader.Style[HtmlTextWriterStyle.Display] = this.SimpleMemberCountMode ? "none" : string.Empty;

            RockDropDownList ddlMemberStatus = new RockDropDownList();
            ddlMemberStatus.ID = string.Format( "{0}_ddlMemberStatus", filterControl.ID );
            ddlMemberStatus.AddCssClass( "js-filter-control js-member-status" );
            ddlMemberStatus.Label = "Member Status";
            ddlMemberStatus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( GroupMemberStatus memberStatus in Enum.GetValues( typeof( GroupMemberStatus ) ) )
            {
                ddlMemberStatus.Items.Add( new ListItem( memberStatus.ConvertToString(), memberStatus.ConvertToInt().ToString() ) );
            }

            filterControl.Controls.Add( ddlMemberStatus );

            ddlLeader.Visible = !this.SimpleMemberCountMode;
            ddlMemberStatus.Visible = !this.SimpleMemberCountMode;

            return new Control[] { ddlIntegerCompare, nbMemberCount, ddlLeader, ddlMemberStatus };
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
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            DropDownList ddlLeader = controls[2] as DropDownList;
            DropDownList ddlMemberStatus = controls[3] as DropDownList;

            // Member Status
            ddlMemberStatus.RenderControl( writer );

            // Is Leader
            ddlLeader.RenderControl( writer );

            // Comparison Row
            writer.AddAttribute( "class", "row form-row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Comparison Type
            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlCompare.RenderControl( writer );
            writer.RenderEndTag();

            ComparisonType comparisonType = (ComparisonType)( ddlCompare.SelectedValue.AsInteger() );
            nbValue.Style[HtmlTextWriterStyle.Display] = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? "none" : string.Empty;

            // Comparison Value
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            nbValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            RegisterFilterCompareChangeScript( filterControl );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            DropDownList ddlLeader = controls[2] as DropDownList;
            DropDownList ddlMemberStatus = controls[3] as DropDownList;

            if ( this.SimpleMemberCountMode )
            {
                return string.Format( "{0}|{1}|{2}|{3}", ddlCompare.SelectedValue, nbValue.Text, null, null );
            }
            else
            {
                return string.Format( "{0}|{1}|{2}|{3}", ddlCompare.SelectedValue, nbValue.Text, ddlLeader.SelectedValue, ddlMemberStatus.SelectedValue );
            }
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

            DropDownList ddlCompare = controls[0] as DropDownList;
            NumberBox nbValue = controls[1] as NumberBox;
            DropDownList ddlLeader = controls[2] as DropDownList;
            DropDownList ddlMemberStatus = controls[3] as DropDownList;

            if ( values.Length >= 4 )
            {
                ComparisonType comparisonType = (ComparisonType)( values[0].AsInteger() );
                ddlCompare.SelectedValue = comparisonType.ConvertToInt().ToString();
                nbValue.Text = values[1];
                ddlLeader.SelectedValue = values[2];
                ddlMemberStatus.SelectedValue = values[3];
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
            int? memberCountValue = values[1].AsIntegerOrNull();
            GroupMemberStatus? memberStatusValue = (GroupMemberStatus?)values[3].AsIntegerOrNull();
            bool? isLeader = values[2].AsBooleanOrNull();

            var memberCountQuery = new GroupService( (RockContext)serviceInstance.Context ).Queryable();
            var memberCountEqualQuery = memberCountQuery.Where( p => p.Members.Count( a =>
                            ( !memberStatusValue.HasValue || a.GroupMemberStatus == memberStatusValue )
                            && ( !isLeader.HasValue || ( a.GroupRole.IsLeader == isLeader.Value ) ) )
                            == memberCountValue );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Group>( memberCountEqualQuery, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, 0 );

            return result;
        }

        #endregion
    }
}