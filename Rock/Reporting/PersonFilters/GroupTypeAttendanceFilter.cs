using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;
using Rock.Model;

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter persons on whether they have attended a group type a specific number of times" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "Group Type Filter" )]
    public class GroupTypeAttendanceFilter : FilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Group Type Attendance"; }
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( string selection )
        {
            GroupType groupType = null;
            FilterComparisonType comparisonType = FilterComparisonType.None;
            int attended = 0;
            int weeks = 0;

            string[] options = selection.Split( '|' );
            if ( options.Length != 4  )
            {
                return string.Empty;
            }

            int groupTypeId = 0;
            if (int.TryParse(options[0], out groupTypeId))
            {
                groupType = new GroupTypeService().Get(groupTypeId);
            }

            try { comparisonType= options[1].ConvertToEnum<FilterComparisonType>(); }
            catch {}

            if (!int.TryParse(options[2], out attended))
                attended = 0;

            if (!int.TryParse(options[3], out weeks))
                weeks = 0;

            return string.Format("Attended {0} {1} {2} times in the last {3} week(s)",
                groupType != null ? groupType.Name : "?",
                comparisonType != FilterComparisonType.None ? comparisonType.ConvertToString() : string.Empty,
                attended, weeks);
        }

        /// <summary>
        /// Gets the selection controls
        /// </summary>
        /// <param name="setSelection"></param>
        /// <param name="selection"></param>
        public override void AddControls( Control parentControl, bool setSelection, string selection )
        {
            var controls = new List<Control>();

            DropDownList ddlGroupType = new DropDownList();
            ddlGroupType.ID = parentControl.ID + "_ddlGroupType";
            parentControl.Controls.Add(ddlGroupType);

            foreach ( GroupType groupType in new GroupTypeService().Queryable() )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            parentControl.Controls.Add(new LiteralControl("<br/><br/>Attended "));

            DropDownList ddlComparison = ComparisonControl( NumericFilterComparisonTypes );
            ddlComparison.ID = parentControl.ID + "_ddlComparison";
            parentControl.Controls.Add(ddlComparison);

            parentControl.Controls.Add(new LiteralControl(" "));

            TextBox tbAttendance = new TextBox();
            tbAttendance.ID = parentControl.ID + "_tbAttendance";
            parentControl.Controls.Add(tbAttendance);

            parentControl.Controls.Add(new LiteralControl(" times in the last "));

            TextBox tbWeeks = new TextBox();
            tbWeeks.ID = parentControl.ID + "_tbWeeks";
            parentControl.Controls.Add(tbWeeks);

            parentControl.Controls.Add(new LiteralControl(" weeks."));

            if ( setSelection )
            {
                string[] options = selection.Split( '|' );
                if ( options.Length != 4 )
                {
                    ddlGroupType.SelectedValue = options[0];
                    ddlComparison.SelectedValue = options[1];
                    tbAttendance.Text = options[2];
                    tbWeeks.Text = options[3];
                }
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public override string GetSelection( Control parentControl )
        {
            DropDownList ddlGroupType = parentControl.FindControl( parentControl.ID + "_ddlGroupType" ) as DropDownList;
            DropDownList ddlComparison = parentControl.FindControl( parentControl.ID + "_ddlComparison" ) as DropDownList;
            TextBox tbAttendance = parentControl.FindControl( parentControl.ID + "_tbAttendance" ) as TextBox;
            TextBox tbWeeks = parentControl.FindControl( parentControl.ID + "_tbWeeks" ) as TextBox;

            return string.Format("{0}|{1}|{2}|{3}",
                ddlGroupType != null ? ddlGroupType.SelectedValue : string.Empty,
                ddlComparison != null ? ddlComparison.SelectedValue : string.Empty,
                tbAttendance.Text, tbWeeks.Text);
        }


        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length != 4 )
            {
                return null;
            }

            FilterComparisonType comparisonType = FilterComparisonType.None;
            int attended = 0;
            int weeks = 0;

            int groupTypeId = 0;
            if ( !int.TryParse( options[0], out groupTypeId ) )
                groupTypeId = 0;

            try { comparisonType = options[1].ConvertToEnum<FilterComparisonType>(); }
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