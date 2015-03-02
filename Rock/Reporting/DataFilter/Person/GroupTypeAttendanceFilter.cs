// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether they have attended a group type a specific number of times" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Type Attendance Filter" )]
    public class GroupTypeAttendanceFilter : DataFilterComponent
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
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Group Attendance"; }
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
            return "Recent Attendance";
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
            return "'Attended ' + " +
                "'\\'' + $('select:first', $content).find(':selected').text() + '\\' ' + " +
                "$('select:last', $content).find(':selected').text() + ' ' + " +
                "$('input:first', $content).val() + ' times in the last ' + " +
                "$('input:last', $content).val() + ' week(s)'";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Group Type Attendance";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                var groupType = Rock.Web.Cache.GroupTypeCache.Read( options[0].AsGuid() );

                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

                s = string.Format(
                    "Attended '{0}' {1} {2} times in the last {3} week(s)",
                    groupType != null ? groupType.Name : "?",
                    comparisonType.ConvertToString(),
                    options[2],
                    options[3] );
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var ddlGroupType = new RockDropDownList();
            ddlGroupType.ID = filterControl.ID + "_0";
            filterControl.Controls.Add( ddlGroupType );

            foreach ( Rock.Model.GroupType groupType in new GroupTypeService( new RockContext() ).Queryable() )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Guid.ToString() ) );
            }

            var ddlIntegerCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.NumericFilterComparisonTypes );
            ddlIntegerCompare.ID = filterControl.ID + "_ddlIntegerCompare";
            filterControl.Controls.Add( ddlIntegerCompare );

            var tbAttendedCount = new RockTextBox();
            tbAttendedCount.ID = filterControl.ID + "_2";
            filterControl.Controls.Add( tbAttendedCount );

            var tbInLastWeeksCount = new RockTextBox();
            tbInLastWeeksCount.ID = filterControl.ID + "_tbInLastWeeksCount";
            filterControl.Controls.Add( tbInLastWeeksCount );

            var controls = new Control[4] { ddlGroupType, ddlIntegerCompare, tbAttendedCount, tbInLastWeeksCount };

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|4|16", ddlGroupType.Items.Count > 0 ? ddlGroupType.Items[0].Value : "0", ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

            return controls;
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
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='data-view-filter-label'>Attended</span>" );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[0].RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[1].RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[2].RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='data-view-filter-label'>Times in the Last</span>" );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            controls[3].RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='data-view-filter-label'>Week(s)</span>" );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            string groupTypeGuid = ( (DropDownList)controls[0] ).SelectedValue;
            string comparisonType = ( (DropDownList)controls[1] ).SelectedValue;
            string attended = ( (TextBox)controls[2] ).Text;
            string weeks = ( (TextBox)controls[3] ).Text;
            return string.Format( "{0}|{1}|{2}|{3}", groupTypeGuid, comparisonType, attended, weeks );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                ( (DropDownList)controls[0] ).SelectedValue = options[0];
                ( (DropDownList)controls[1] ).SelectedValue = options[1];
                ( (TextBox)controls[2] ).Text = options[2];
                ( (TextBox)controls[3] ).Text = options[3];
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
            string[] options = selection.Split( '|' );
            if ( options.Length != 4 )
            {
                return null;
            }

            Guid groupTypeGuid = options[0].AsGuid();
            ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );
            int attended = options[2].AsInteger();
            int weeks = options[3].AsInteger();

            var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeGuid );
            int? groupTypeId = groupType != null ? groupType.Id : (int?)null;

            DateTime startDate = RockDateTime.Now.AddDays( 0 - ( 7 * weeks ) );

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceQry = new AttendanceService( rockContext ).Queryable();

            var qry = new PersonService( rockContext ).Queryable()
                  .Where( p => attendanceQry.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count( a =>
                            (
                                (
                                    ( a.Group.GroupTypeId == groupTypeId ) &&
                                    ( a.StartDateTime >= startDate )
                                ) &&
                                ( a.DidAttend.HasValue && a.DidAttend.Value )
                            )
                        ) == attended );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );

            return result;
        }

        #endregion
    }
}