using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

using Rock;
using Rock.Field;
using Rock.Model;

namespace Rock.Reporting.PersonFilter
{
    /// <summary>
    /// 
    /// </summary>
    [Description("Filter persons on Gender")]
    [Export(typeof(FilterComponent))]
    [ExportMetadata("ComponentName", "Gender Filter")]
    public class GenderFilter : FilterComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public override string Prompt
        {
            get { return "Gender"; }
        }

        /// <summary>
        /// Gets the supported comparison types.
        /// </summary>
        /// <value>
        /// The supported comparison types.
        /// </value>
        public override FilterComparisonType SupportedComparisonTypes
        {
            get { return FilterComparisonType.None; }
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
                return Rock.Web.Cache.FieldTypeCache.Read( SystemGuid.FieldType.SINGLE_SELECT );
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
                var configValues = new Dictionary<string, string>();
                configValues.Add( "values", "Male, Female");
                configValues.Add( "fieldtype", "rb" );
                return configValues;
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <returns></returns>
        public override Expression GetExpression( Expression parameterExpression, FilterComparisonType comparisonType, string fieldTypeValue )
        {
            Gender gender = fieldTypeValue.ConvertToEnum<Gender>();

            Expression property = Expression.Property( parameterExpression, "Gender" );
            Expression value = Expression.Constant( gender );
            return ComparisonExpression( comparisonType, property, value );
        }
    }
}