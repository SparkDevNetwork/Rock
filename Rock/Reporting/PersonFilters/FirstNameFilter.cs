//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq.Expressions;

using Rock.Model;

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter persons on First Name" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "First Name Filter" )]
    public class FirstNameFilter : FilterComponent
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public override string Title
        {
            get { return "First Name"; }
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

            MemberExpression gnProperty = Expression.Property( parameterExpression, "GivenName" );
            MemberExpression nnProperty = Expression.Property( parameterExpression, "NickName" );
            Expression property = Expression.Coalesce( nnProperty, gnProperty );
            Expression constant = Expression.Constant( value );
            return ComparisonExpression( comparisonType, property, constant );
        }
    }
}