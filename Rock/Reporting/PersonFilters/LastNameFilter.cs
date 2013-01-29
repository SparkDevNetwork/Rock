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
    [Description( "Filter persons on Last Name" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "Last Name Filter" )]
    public class LastNameFilter : FilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "Last Name"; }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, string selection )
        {
            FilterComparisonType comparisonType = FilterComparisonType.StartsWith;
            string value = string.Empty;

            string[] options = selection.Split( '|' );
            if ( options.Length > 0 )
            {
                try { comparisonType = options[0].ConvertToEnum<FilterComparisonType>(); }
                catch { }
            }
            if ( options.Length > 1 )
            {
                value = options[1];
            }

            MemberExpression property = Expression.Property( parameterExpression, "LastName" );
            Expression constant = Expression.Constant( value );
            return ComparisonExpression( comparisonType, property, constant );
        }
    }
}