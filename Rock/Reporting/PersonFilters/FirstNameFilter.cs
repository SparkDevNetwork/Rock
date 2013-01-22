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
    [Description( "Filter persons on First Name" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "First Name Filter" )]
    public class FirstNameFilter : FilterComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public override string Prompt
        {
            get { return "First Name"; }
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
                    FilterComparisonType.Contains &
                    FilterComparisonType.DoesNotContain &
                    FilterComparisonType.Equal &
                    FilterComparisonType.IsBlank &
                    FilterComparisonType.IsNotBlank &
                    FilterComparisonType.NotEqual &
                    FilterComparisonType.StartsWith &
                    FilterComparisonType.EndsWith;
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
                return Rock.Web.Cache.FieldTypeCache.Read( SystemGuid.FieldType.TEXT );
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
            MemberExpression gnProperty = Expression.Property( parameterExpression, "GivenName" );
            MemberExpression nnProperty = Expression.Property( parameterExpression, "NickName" );
            Expression property = Expression.Coalesce( nnProperty, gnProperty );
            Expression value = Expression.Constant( fieldTypeValue );
            return ComparisonExpression( comparisonType, property, value );
        }
    }
}