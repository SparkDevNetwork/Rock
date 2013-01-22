using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

using Rock.Field;
using Rock.Model;

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter persons on days since last attendance" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "Last Attended Days Filter" )]
    public class LastAttendedDaysFilter : FilterComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public override string Prompt
        {
            get { return "Last Name"; }
        }

        /// <summary>
        /// Gets the supported comparison types.
        /// </summary>
        /// <value>
        /// The supported comparison types.
        /// </value>
        public override FilterComparisonType SupportedComparisonTypes
        {
            get
            {
                return
                    FilterComparisonType.Equal &
                    FilterComparisonType.NotEqual &
                    FilterComparisonType.GreaterThan &
                    FilterComparisonType.GreaterThanOrEqual &
                    FilterComparisonType.LessThan &
                    FilterComparisonType.LessThanOrEqual;
            }
        }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public override Rock.Web.Cache.FieldTypeCache FieldType
        {
            get
            {
                return Rock.Web.Cache.FieldTypeCache.Read( SystemGuid.FieldType.INTEGER );
            }
        }

        /// <summary>
        /// Gets the field configuration values.
        /// </summary>
        /// <value>
        /// The field configuration values.
        /// </value>
        public override Dictionary<string, string> FieldConfigurationValues
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, FilterComparisonType comparisonType, string fieldTypeValue )
        {
            //ParameterExpression attendance = Expression.Parameter( typeof(Attendance), "a");
            //MemberExpression startDateTime = Expression.Property(attendance, "StartDateTime");

            //Expression attendanceDates = Expression.Lambda<Func<Attendance, DateTime>>(startDateTime, attendance);
            //Expression exp = Expression.Call(typeof(Enumerable), "Select", 

            //int numDays = 0;
            //if (int.TryParse(fieldTypeValue, out numDays))
            //{
            //    Expression attendances = Expression.Property( parameterExpression, "Attendances");
            //    Expression select = Expression.

            //MemberExpression property = Expression.Property( parameterExpression, "LastName" );
            //Expression value = Expression.Constant( fieldTypeValue );
            //return ComparisonExpression( comparisonType, property, value );

            return null;
        }
    }
}