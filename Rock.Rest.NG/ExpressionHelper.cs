using System;
using System.Linq.Expressions;

namespace Rock.Rest
{
    internal static class ExpressionHelper
    {
        /// <summary>
        /// Creates an expression that can be used to compare the property value
        /// with the passed value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="typeParameter">The type parameter.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns></returns>
        public static Expression ComparisonExpression( Type type, Expression typeParameter, string propertyName, object value, int comparisonType )
        {
            var property = type.GetProperty( propertyName );
            var propertyAccess = Expression.MakeMemberAccess( typeParameter, property );

            Expression logicExpression;

            if ( property.PropertyType.IsPrimitive )
            {
                // Primitives are easy. They understand the standard expression
                // methods so we can just use those.
                if ( comparisonType < 0 )
                {
                    logicExpression = Expression.LessThan( propertyAccess, Expression.Constant( value ) );
                }
                else if ( comparisonType > 0 )
                {
                    logicExpression = Expression.GreaterThan( propertyAccess, Expression.Constant( value ) );
                }
                else
                {
                    logicExpression = Expression.Equal( propertyAccess, Expression.Constant( value ) );
                }
            }
            else
            {
                // Non-primitives take more work. First we need to deal with
                // situations where it is a nullable.
                var propertyType = property.PropertyType;

                if ( propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                    propertyAccess = Expression.MakeMemberAccess( propertyAccess, property.PropertyType.GetProperty( "Value" ) );
                }

                // Then we need to call the CompareTo() method on the underlying
                // object to get a comparison result.
                var mi = propertyType.GetMethod( "CompareTo", new[] { propertyType } );
                var compareTo = Expression.Call( propertyAccess, mi, Expression.Constant( value ) );

                // Then we can compare that against 0 to determine the result
                // of the comparison.
                if ( comparisonType < 0 )
                {
                    logicExpression = Expression.LessThan( compareTo, Expression.Constant( 0 ) );
                }
                else if ( comparisonType > 0 )
                {
                    logicExpression = Expression.GreaterThan( compareTo, Expression.Constant( 0 ) );
                }
                else
                {
                    logicExpression = Expression.Equal( compareTo, Expression.Constant( 0 ) );
                }
            }

            return logicExpression;
        }

        public static Expression IsNullExpression( Type type, Expression typeParameter, string propertyName )
        {
            var property = type.GetProperty( propertyName );
            var propertyAccess = Expression.MakeMemberAccess( typeParameter, property );

            return Expression.Equal( propertyAccess, Expression.Constant( null ) );
        }

        public static Expression IsNotNullExpression( Type type, Expression typeParameter, string propertyName )
        {
            var property = type.GetProperty( propertyName );
            var propertyAccess = Expression.MakeMemberAccess( typeParameter, property );

            return Expression.NotEqual( propertyAccess, Expression.Constant( null ) );
        }

    }
}
