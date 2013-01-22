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
    [Description( "Filter persons on whether they have a picture or not" )]
    [Export( typeof( FilterComponent ) )]
    [ExportMetadata( "ComponentName", "Has Picture Filter" )]
    public class HasPictureFilter : FilterComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public override string Prompt
        {
            get { return "Has Picture"; }
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
                    FilterComparisonType.None;
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
                return Rock.Web.Cache.FieldTypeCache.Read( SystemGuid.FieldType.BOOLEAN );
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
            bool hasPicture = true;
            if ( Boolean.TryParse( fieldTypeValue, out hasPicture ) )
            {
                MemberExpression property = Expression.Property( parameterExpression, "PhotoId" );
                Expression hasValue = Expression.Property( property, "HasValue");
                Expression value = Expression.Constant( hasPicture );
                return Expression.Equal( hasValue, value);
            }
            return null;
        }
    }
}