using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

using Rock.Model;
using Rock.Extension;
using Rock.Field;

namespace Rock.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FilterComponent : IComponent
    {
        /// <summary>
        /// Gets the prompt.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        public abstract string Prompt { get; }

        /// <summary>
        /// Gets the supported comparison types.
        /// </summary>
        /// <value>
        /// The supported comparison types.
        /// </value>
        public abstract FilterComparisonType SupportedComparisonTypes { get; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public abstract Rock.Web.Cache.FieldTypeCache FieldType { get; }

        /// <summary>
        /// Gets the field configuration values.
        /// </summary>
        /// <value>
        /// The field configuration values.
        /// </value>
        public abstract Dictionary<string, string> FieldConfigurationValues { get; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="fieldTypeValue">The field type value.</param>
        /// <returns></returns>
        public abstract Expression GetExpression( Expression parameterExpression, FilterComparisonType comparisonType, string fieldTypeValue );


        /// <summary>
        /// Gets the comparison expression.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Expression ComparisonExpression( FilterComparisonType comparisonType, Expression property, Expression value )
        {
            if ( comparisonType == FilterComparisonType.Contains )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == FilterComparisonType.DoesNotContain )
            {
                return Expression.Not( Expression.Call( property, typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } ), value ) );
            }

            if ( comparisonType == FilterComparisonType.EndsWith )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } ), value );
            }

            if ( comparisonType == FilterComparisonType.Equal )
            {
                return Expression.Equal( property, value );
            }

            if ( comparisonType == FilterComparisonType.GreaterThan )
            {
                return Expression.GreaterThan( property, value );
            }

            if ( comparisonType == FilterComparisonType.GreaterThanOrEqual )
            {
                return Expression.GreaterThanOrEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.IsBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ));
                Expression emtpyString = Expression.Constant(string.Empty);
                return Expression.Equal( trimmed, value );
            }

            if ( comparisonType == FilterComparisonType.IsNotBlank )
            {
                Expression trimmed = Expression.Call( property, typeof( string ).GetMethod( "Trim", System.Type.EmptyTypes ) );
                Expression emtpyString = Expression.Constant( string.Empty );
                return Expression.NotEqual( trimmed, value );
            }

            if ( comparisonType == FilterComparisonType.LessThan )
            {
                return Expression.LessThan( property, value );
            }

            if ( comparisonType == FilterComparisonType.LessThanOrEqual )
            {
                return Expression.LessThanOrEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.NotEqual )
            {
                return Expression.NotEqual( property, value );
            }

            if ( comparisonType == FilterComparisonType.StartsWith )
            {
                return Expression.Call( property, typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ) } ), value );
            }

            return null;
        }
    }


}