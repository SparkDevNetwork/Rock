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

namespace Rock.DataFilters.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter persons on whether they have attended a group type a specific number of times" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Type Attendance Filter" )]
    public class GroupTypeAttendanceFilter : DataFilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Recent Attendance"; }
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

        /// <summary>
        /// Gets the name of the filtered entity type.
        /// </summary>
        /// <value>
        /// The name of the filtered entity type.
        /// </value>
        public override string FilteredEntityTypeName
        {
            get { return "Rock.Model.Person"; }
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
        public override string ClientFormatSelection
        {
            get
            {
                return "'Attended ' + " + 
                    "'\\'' + $('select:first', $content).find(':selected').text() + '\\' ' + " +
                    "$('select:last', $content).find(':selected').text() + ' ' + " + 
                    "$('input:first', $content).val() + ' times in the last ' + " +
                    "$('input:last', $content).val() + ' week(s)'";
            }
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
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
        public override Control[] CreateChildControls()
        {
            DropDownList ddlGroupType = new DropDownList();
            foreach ( GroupType groupType in new GroupTypeService().Queryable() )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            var controls = new Control[4] {
                ddlGroupType,  ComparisonControl( NumericFilterComparisonTypes ),
                new TextBox(), new TextBox() };

            SetSelection( controls, string.Format( "{0}|{1}|4|16",
                ddlGroupType.Items.Count > 0 ? ddlGroupType.Items[0].Value : "0",
                ComparisonType.GreaterThanOrEqualTo.ConvertToInt().ToString() ) );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( HtmlTextWriter writer, Control[] controls )
        {
            controls[0].RenderControl( writer );
            writer.WriteBreak();
            writer.Write( "Attended " );
            controls[1].RenderControl( writer );
            writer.Write( " " );
            controls[2].RenderControl( writer );
            writer.Write( " Times in the Last " );
            controls[3].RenderControl( writer );
            writer.Write( " Week(s)." );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        public override string GetSelection( Control[] controls )
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
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Control[] controls, string selection )
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
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
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

            ParameterExpression attendanceParameter = Expression.Parameter( typeof( Attendance ), "a" );

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
                Expression.Lambda<Func<Attendance, bool>>(groupTypeIdAndStartAndDidAttend, new ParameterExpression[] { attendanceParameter });

            Expression attendanceCount = Expression.Call(typeof(Enumerable), "Count", 
                new Type[] { typeof(Attendance) }, Expression.PropertyOrField(parameterExpression, "Attendances"), attendanceLambda);

            Expression timesAttendedConstant = Expression.Constant( attended );
            Expression timesAttendedComparison = Expression.GreaterThanOrEqual(attendanceCount, timesAttendedConstant);

            return timesAttendedComparison;
        }
    }
}