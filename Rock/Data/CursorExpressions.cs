// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rock.Data
{
    /// <summary>
    /// Handles building where clause expressions for use in cursor pagination.
    /// These are complex expressions because they need to handle multiple
    /// ordering fields, each of which may be ascending or descending. It also
    /// needs to take into account null values.
    /// </summary>
    internal static class CursorExpressions
    {
        /// <summary>
        /// Builds a LINQ expression that implements multi-field cursor-based
        /// pagination for the specified entity type. The generated predicate
        /// supports cursor-based pagination across multiple ordered fields,
        /// enabling efficient 'seek' pagination in LINQ queries.
        /// </summary>
        /// <typeparam name="T">The type of the entity to which the predicate will be applied.</typeparam>
        /// <param name="parts">A read-only list of ordering information describing the fields and sort directions.</param>
        /// <param name="partValues">A dictionary mapping property paths to their corresponding cursor values.</param>
        /// <returns>An expression representing a predicate that evaluates to true for entities positioned after the <paramref name="partValues"/>.</returns>
        public static Expression<Func<T, bool>> BuildCursorPredicate<T>( IReadOnlyList<CursorOrderInfo> parts, Dictionary<string, object> partValues )
            where T : class
        {
            var param = Expression.Parameter( typeof( T ), "x" );

            Expression finalOr = null;

            // Builds a LINQ expression for cursor pagination that combines
            // multiple ordering fields. For each ordering field, it generates
            // a predicate that matches all previous fields exactly and checks
            // if the current field is "after" the cursor value (according to
            // ascending/descending). The final expression is an OR of all such
            // predicates, allowing for multi-field cursor pagination.
            //
            // Exmaple: OrderByDescending( x => x.CreatedDateTime ).ThenByDescending( x => x.Id )
            //
            // Will translate to something like:
            // x => (
            //   (x.CreatedDateTime < cursorCreatedDateTime)
            //     OR
            //   (x.CreatedDateTime == cursorCreatedDateTime AND x.Id < cursorId)
            // )
            for ( int i = 0; i < parts.Count; i++ )
            {
                var part = parts[i];

                // Rebind the key selector to our shared parameter.
                var key = ReplaceParameter( part.KeySelector, param );

                // Build Equal(x => key, cursorValue) for all previous fields
                Expression andChain = null;
                for ( int j = 0; j < i; j++ )
                {
                    var prev = parts[j];
                    var prevKey = ReplaceParameter( prev.KeySelector, param );

                    var equalExpr = BuildEqualDynamic<T>( prevKey, partValues[prev.PropertyPath] );

                    andChain = andChain == null
                        ? equalExpr
                        : Expression.AndAlso( andChain, equalExpr );
                }

                // Build After(x => key, cursorValue, ascending)
                var afterExpr = BuildAfterDynamic<T>( key, partValues[part.PropertyPath], !part.Descending );

                // Combine: (previous equalities) AND (after)
                Expression levelExpr = andChain == null
                    ? afterExpr
                    : Expression.AndAlso( andChain, afterExpr );

                // OR it into the final predicate
                finalOr = finalOr == null
                    ? levelExpr
                    : Expression.OrElse( finalOr, levelExpr );
            }

            return Expression.Lambda<Func<T, bool>>( finalOr, param );
        }

        /// <summary>
        /// Builds a predicate expression that tests whether the value selected
        /// by the specified key selector is equal to the provided cursor value,
        /// handling both nullable and non-nullable types appropriately.
        /// </summary>
        /// <typeparam name="T">The type of the object being tested in the predicate.</typeparam>
        /// <typeparam name="TKey">The type of the key value to compare.</typeparam>
        /// <param name="keySelector">An expression that selects the key from an object of type <typeparamref name="T"/>. This determines which property or field is compared.</param>
        /// <param name="cursorValue">The value to compare against the selected key. May be null if <typeparamref name="TKey"/> is a nullable or reference type.</param>
        /// <returns>
        /// An expression representing a predicate that evaluates to <see langword="true"/>
        /// if the selected key is equal to <paramref name="cursorValue"/>. For
        /// nullable or reference types, the predicate also evaluates to
        /// <see langword="true"/> if both the selected key and
        /// <paramref name="cursorValue"/> are null.
        /// </returns>
        private static Expression<Func<T, bool>> BuildEqual<T, TKey>( Expression<Func<T, TKey>> keySelector, TKey cursorValue )
        {
            var param = keySelector.Parameters[0];
            var member = keySelector.Body;
            var keyType = typeof( TKey );

            // If the key type is not nullable (value type without Nullable<>), just use ==
            if ( !IsNullableType( keyType ) )
            {
                var constant = Expression.Constant( cursorValue, keyType );
                var body = Expression.Equal( member, constant );

                return Expression.Lambda<Func<T, bool>>( body, param );
            }

            // Nullable or reference type:
            // (FieldValue == CursorValue) OR (FieldValue == null AND CursorValue == null)

            var constantValue = Expression.Constant( cursorValue, keyType );
            var nullConstant = Expression.Constant( null, keyType );

            // FieldValue == CursorValue
            var equalNonNull = Expression.Equal( member, constantValue );

            // FieldValue == null
            var memberNull = Expression.Equal( member, nullConstant );

            // CursorValue == null
            var cursorNull = Expression.Equal( constantValue, nullConstant );

            // (FieldValue == null AND CursorValue == null)
            var bothNull = Expression.AndAlso( memberNull, cursorNull );

            // (FieldValue == CursorValue) OR (FieldValue == null AND CursorValue == null)
            var bodyNullable = Expression.OrElse( equalNonNull, bothNull );

            return Expression.Lambda<Func<T, bool>>( bodyNullable, param );
        }

        /// <summary>
        /// Dynamically calls the BuildEqual method with the appropriate
        /// generic type arguments based on the return type of the provided
        /// key selector expression. This allows for building equality
        /// expressions for keys of unknown types at compile time, including
        /// handling nullable and non-nullable types correctly.
        /// </summary>
        /// <typeparam name="T">The type of the object being tested in the predicate.</typeparam>
        /// <param name="keySelector">An expression that selects the key from an object of type <typeparamref name="T"/>. This determines which property or field is compared.</param>
        /// <param name="cursorValue">The value to compare against the selected key.</param>
        /// <returns>
        /// An expression representing a predicate that evaluates to <see langword="true"/>
        /// if the selected key is equal to <paramref name="cursorValue"/>. For
        /// nullable or reference types, the predicate also evaluates to
        /// <see langword="true"/> if both the selected key and
        /// <paramref name="cursorValue"/> are null.
        /// </returns>
        private static Expression BuildEqualDynamic<T>( LambdaExpression keySelector, object cursorValue )
        {
            var keyType = keySelector.ReturnType;
            var typedValue = ConvertValue( cursorValue, keyType );

            // Call BuildEqual<T, keyType>
            var method = typeof( CursorExpressions )
                .GetMethod( nameof( BuildEqual ), BindingFlags.NonPublic | BindingFlags.Static )
                .MakeGenericMethod( typeof( T ), keyType );

            var lambda = ( LambdaExpression ) method.Invoke( null, new object[] { keySelector, typedValue } );

            return lambda.Body;
        }

        /// <summary>
        /// <para>
        /// Builds a predicate expression that filters entities occurring after
        /// a specified cursor value, based on the provided key selector and
        /// sort direction.
        /// </para>
        /// <para>
        /// For nullable key types, the predicate handles null values according
        /// to common database ordering conventions: in ascending order, nulls
        /// are considered first; in descending order, nulls are considered last.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to filter.</typeparam>
        /// <typeparam name="TKey">The type of the key used for cursor-based comparison.</typeparam>
        /// <param name="keySelector">An expression that selects the key from the entity. This key is used to determine the position relative to the cursor value.</param>
        /// <param name="cursorValue">The value of the key that represents the current cursor position. Entities after this value will be included in the result.</param>
        /// <param name="ascending">true to filter in ascending order; false to filter in descending order.</param>
        /// <returns>An expression that evaluates to true for entities occurring after the specified cursor value according to the sort direction.</returns>
        private static Expression<Func<T, bool>> BuildAfter<T, TKey>( Expression<Func<T, TKey>> keySelector, TKey cursorValue, bool ascending )
        {
            var param = keySelector.Parameters[0];
            var member = keySelector.Body;
            var keyType = typeof( TKey );

            var constant = Expression.Constant( cursorValue, keyType );

            bool isNullable = IsNullableType( keyType );

            // If non-nullable, we can use simple > or <.
            if ( !isNullable )
            {
                Expression comparison;

                if ( keyType == typeof( bool ) )
                {
                    var cursorBool = ( bool ) ( object ) cursorValue;

                    if ( ascending )
                    {
                        // In ascending, if the cursor value is false, then only
                        // true values remain. If the cursor value is true, then
                        // nothing remains.
                        comparison = cursorBool == false
                            ? ( Expression ) Expression.Equal( member, Expression.Constant( true ) )
                            : Expression.Constant( false );
                    }
                    else // descending
                    {
                        // In descending, if the cursor value is true, then only
                        // false values remain. If the cursor value is false, then
                        // nothing remains.
                        comparison = cursorBool == true
                            ? ( Expression ) Expression.Equal( member, Expression.Constant( false ) )
                            : Expression.Constant( false );
                    }
                }
                else
                {
                    comparison = ascending
                        ? Expression.GreaterThan( member, constant )
                        : Expression.LessThan( member, constant );
                }

                return Expression.Lambda<Func<T, bool>>( comparison, param );
            }

            // NULLABLE CASE
            //
            // Ascending (nulls first):
            //   (CursorValue == null AND FieldValue != null)
            //   OR
            //   (CursorValue != null AND FieldValue != null AND FieldValue > CursorValue)
            //
            // Descending (nulls last):
            //   (CursorValue != null AND FieldValue == null)
            //   OR
            //   (C ursorValue!= null AND FieldValue != null AND FieldValue < CursorValue)

            var nullConst = Expression.Constant( null, keyType );

            var cursorIsNull = Expression.Equal( constant, nullConst );
            var cursorNotNull = Expression.NotEqual( constant, nullConst );

            var memberIsNull = Expression.Equal( member, nullConst );
            var memberNotNull = Expression.NotEqual( member, nullConst );

            // Strings don't directly support GreaterThan/LessThan in LINQ
            // so we need to translate to string.Compare(a, b).
            if ( keyType == typeof( string ) )
            {
                // string.Compare(member, constant) > 0 (ascending)
                // string.Compare(member, constant) < 0 (descending)
                var compareMethod = typeof( string ).GetMethod( "Compare", new[] { typeof( string ), typeof( string ) } );
                var compareCall = Expression.Call( compareMethod, member, constant );

                var comparison = ascending
                    ? Expression.GreaterThan( compareCall, Expression.Constant( 0 ) )
                    : Expression.LessThan( compareCall, Expression.Constant( 0 ) );

                // (CursorValue == null AND FieldValue != null)
                var case1 = Expression.AndAlso( cursorIsNull, memberNotNull );

                // (CursorValue != null AND FieldValue != null AND string.Compare(FieldValue, CursorValue) >/< 0)
                var case2 = Expression.AndAlso( cursorNotNull, Expression.AndAlso( memberNotNull, comparison ) );

                var body = Expression.OrElse( case1, case2 );

                return Expression.Lambda<Func<T, bool>>( body, param );
            }

            if ( ascending )
            {
                // (CursorValue == null AND FieldValue != null)
                var case1 = Expression.AndAlso( cursorIsNull, memberNotNull );

                // (CursorValue != null AND FieldValue != null AND FieldValue > CursorValue)
                var greaterThan = Expression.GreaterThan( member, constant );
                var case2 = Expression.AndAlso(
                    cursorNotNull,
                    Expression.AndAlso( memberNotNull, greaterThan )
                );

                var body = Expression.OrElse( case1, case2 );

                return Expression.Lambda<Func<T, bool>>( body, param );
            }
            else
            {
                // (CursorValue != null AND FieldValue == null)
                var case1 = Expression.AndAlso( cursorNotNull, memberIsNull );

                // (CursorValue != null AND FieldValue != null AND FieldValue < CursorValue)
                var lessThan = Expression.LessThan( member, constant );
                var case2 = Expression.AndAlso(
                    cursorNotNull,
                    Expression.AndAlso( memberNotNull, lessThan )
                );

                var body = Expression.OrElse( case1, case2 );

                return Expression.Lambda<Func<T, bool>>( body, param );
            }
        }

        /// <summary>
        /// <para>
        /// Dynamically calls the BuildAfter method with the appropriate generic
        /// type arguments.
        /// </para>
        /// <para>
        /// For nullable key types, the predicate handles null values according
        /// to common database ordering conventions: in ascending order, nulls
        /// are considered first; in descending order, nulls are considered last.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to filter.</typeparam>
        /// <param name="keySelector">An expression that selects the key from the entity. This key is used to determine the position relative to the cursor value.</param>
        /// <param name="cursorValue">The value of the key that represents the current cursor position. Entities after this value will be included in the result.</param>
        /// <param name="ascending">true to filter in ascending order; false to filter in descending order.</param>
        /// <returns>An expression that evaluates to true for entities occurring after the specified cursor value according to the sort direction.</returns>
        private static Expression BuildAfterDynamic<T>( LambdaExpression keySelector, object cursorValue, bool ascending )
        {
            var keyType = keySelector.ReturnType;
            var typedValue = ConvertValue( cursorValue, keyType );

            // Call BuildAfter<T, keyType>
            var method = typeof( CursorExpressions )
                .GetMethod( nameof( BuildAfter ), BindingFlags.NonPublic | BindingFlags.Static )
                .MakeGenericMethod( typeof( T ), keyType );

            var lambda = ( LambdaExpression ) method.Invoke( null, new object[] { keySelector, typedValue, ascending } );

            return lambda.Body;
        }

        /// <summary>
        /// Converts the specified value to the given target type, handling
        /// nullable types as needed. This only handles simple conversions for
        /// now, such as long to int. It does not handle complex conversions
        /// such as string to Guid.
        /// </summary>
        /// <param name="value">The value to convert. If null, the method returns null.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <returns>An object of the specified target type with the converted value, or null if the input value is null.</returns>
        private static object ConvertValue( object value, Type targetType )
        {
            if ( value == null )
            {
                return null;
            }

            var underlying = Nullable.GetUnderlyingType( targetType );

            if ( underlying != null )
            {
                return Convert.ChangeType( value, underlying );
            }

            return Convert.ChangeType( value, targetType );
        }

        /// <summary>
        /// Determines whether the specified type can represent null values.
        /// </summary>
        /// <param name="type">The type to evaluate for nullability.</param>
        /// <returns>true if the type is a reference type or a nullable value type; otherwise, false.</returns>
        private static bool IsNullableType( Type type )
        {
            // Reference types are always nullable.
            if ( !type.IsValueType )
            {
                return true;
            }

            return Nullable.GetUnderlyingType( type ) != null;
        }

        /// <summary>
        /// Creates a new lambda expression by replacing the parameter of the
        /// specified expression with a new parameter. This allows us to re-use
        /// expressions that have different parameters.
        /// </summary>
        /// <param name="expr">The original lambda expression whose parameter is to be replaced.</param>
        /// <param name="newParam">The new parameter expression to use in place of the original parameter.</param>
        /// <returns>A new LambdaExpression that is functionally equivalent to the original.</returns>
        private static LambdaExpression ReplaceParameter( LambdaExpression expr, ParameterExpression newParam )
        {
            var replacer = new ReplaceVisitor( expr.Parameters[0], newParam );
            var newBody = replacer.Visit( expr.Body );

            return Expression.Lambda( newBody, newParam );
        }

        /// <summary>
        /// An expression visitor that replaces all occurrences of a specified
        /// parameter expression with another parameter expression within an
        /// expression tree.
        /// </summary>
        private sealed class ReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceVisitor( ParameterExpression oldParam, ParameterExpression newParam )
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter( ParameterExpression node )
            {
                return node == _oldParam ? _newParam : node;
            }
        }
    }
}
