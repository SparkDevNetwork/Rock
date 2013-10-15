//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataTransform.Person
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
        ///   </value>
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
                var groupType = new GroupTypeService().Get( int.Parse( options[0] ) );

                ComparisonType comparisonType = ComparisonType.GreaterThanOrEqualTo;
                try { comparisonType= options[0].ConvertToEnum<ComparisonType>(); }
                catch {}

                s = string.Format( "Attended '{0}' {1} {2} times in the last {3} week(s)",
                    groupType != null ? groupType.Name : "?",
                    comparisonType.ConvertToString(),
                    options[2], options[3] );
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

            foreach ( Rock.Model.GroupType groupType in new GroupTypeService().Queryable() )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            var ddl = ComparisonControl( NumericFilterComparisonTypes );
            ddl.ID = filterControl.ID + "_1";
            filterControl.Controls.Add( ddl );

            var tb = new RockTextBox();
            tb.ID = filterControl.ID + "_2";
            filterControl.Controls.Add( tb );

            var tb2 = new RockTextBox();
            tb2.ID = filterControl.ID + "_3";
            filterControl.Controls.Add( tb );

            var controls = new Control[4] { ddlGroupType, ddl, tb, tb2 };

            SetSelection( entityType, controls, string.Format( "{0}|{1}|4|16",
                ddlGroupType.Items.Count > 0 ? ddlGroupType.Items[0].Value : "0",
                ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

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
            string groupTypeId = ( (DropDownList)controls[0] ).SelectedValue;
            string comparisonType = ( (DropDownList)controls[1] ).SelectedValue;
            string attended = ( (TextBox)controls[2] ).Text;
            string weeks = ( (TextBox)controls[3] ).Text;
            return string.Format( "{0}|{1}|{2}|{3}",
                groupTypeId, comparisonType, attended, weeks );
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
        public override Expression GetExpression( Type entityType, object serviceInstance, Expression parameterExpression, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length != 4 )
            {
                return null;
            }

            ComparisonType comparisonType = ComparisonType.GreaterThanOrEqualTo;
            int attended = 0;
            int weeks = 0;

            int groupTypeId = 0;
            if ( !int.TryParse( options[0], out groupTypeId ) )
                groupTypeId = 0;

            try { comparisonType = options[1].ConvertToEnum<ComparisonType>(); }
            catch { }

            if ( !int.TryParse( options[2], out attended ) )
                attended = 0;

            if ( !int.TryParse( options[3], out weeks ) )
                weeks = 0;

            DateTime startDate = DateTime.Now.AddDays( 0 - (7 * weeks));

            // Build expressions for this type of linq statement:
            //var result = new PersonService().Queryable()
            //    .Where( p =>
            //        ( p.Attendances.Count( a =>
            //            (
            //                (
            //                    ( a.Group.GroupTypeId == groupTypeId ) &&
            //                    ( a.StartDateTime >= startDate )
            //                ) &&
            //                ( a.DidAttend == true )
            //            )
            //        ) >= attended ) );

            ParameterExpression attendanceParameter = Expression.Parameter( typeof( Rock.Model.Attendance ), "a" );

            MemberExpression groupProperty = Expression.Property( attendanceParameter, "Group" );
            MemberExpression groupTypeIdProperty = Expression.Property( groupProperty, "GroupTypeId" );
            Expression groupTypeIdConstant = Expression.Constant( groupTypeId );
            Expression groupTypeIdComparison = Expression.Equal( groupTypeIdProperty, groupTypeIdConstant );

            MemberExpression startProperty = Expression.Property( attendanceParameter, "StartDateTime" );
            Expression startConstant = Expression.Constant( startDate );
            Expression startComparison = Expression.GreaterThanOrEqual( startProperty, startConstant );

            MemberExpression didAttendProperty = Expression.Property( attendanceParameter, "DidAttend" );
            Expression didAttendConstant = Expression.Constant( true );
            Expression didAttendComparison = Expression.Equal( didAttendProperty, didAttendConstant );

            Expression groupTypeIdAndStart = Expression.AndAlso( groupTypeIdComparison, startComparison );
            Expression groupTypeIdAndStartAndDidAttend = Expression.AndAlso( groupTypeIdAndStart, didAttendComparison );

            LambdaExpression attendanceLambda = 
                Expression.Lambda<Func<Rock.Model.Attendance, bool>>(groupTypeIdAndStartAndDidAttend, new ParameterExpression[] { attendanceParameter });

            Expression attendanceCount = Expression.Call(typeof(Enumerable), "Count", 
                new Type[] { typeof(Rock.Model.Attendance) }, Expression.PropertyOrField(parameterExpression, "Attendances"), attendanceLambda);

            Expression timesAttendedConstant = Expression.Constant( attended );
            Expression timesAttendedComparison = Expression.GreaterThanOrEqual(attendanceCount, timesAttendedConstant);

            return timesAttendedComparison;
        }

        #endregion

    }

}