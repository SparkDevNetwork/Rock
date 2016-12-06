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
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Linq, List, Dictionary, etc extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region GenericCollection Extensions

        /// <summary>
        /// Concatonate the items into a Delimited string
        /// </summary>
        /// <example>
        /// FirstNamesList.AsDelimited(",") would be "Ted,Suzy,Noah"
        /// FirstNamesList.AsDelimited(", ", " and ") would be "Ted, Suzy and Noah"
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="finalDelimiter">The final delimiter. Set this if the finalDelimiter should be a different delimiter</param>
        /// <returns></returns>
        public static string AsDelimited<T>( this List<T> items, string delimiter, string finalDelimiter = null )
        {
            return AsDelimited<T>( items, delimiter, finalDelimiter, false );
        }

        /// <summary>
        /// Concatonate the items into a Delimited string an optionally htmlencode the strings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="finalDelimiter">The final delimiter.</param>
        /// <param name="HtmlEncode">if set to <c>true</c> [HTML encode].</param>
        /// <returns></returns>
        public static string AsDelimited<T>( this List<T> items, string delimiter, string finalDelimiter, bool HtmlEncode )
        {

            List<string> strings = new List<string>();
            foreach ( T item in items )
            {
                if ( item != null )
                {
                    string itemString = item.ToString();
                    if ( HtmlEncode )
                    {
                        itemString = HttpUtility.HtmlEncode( itemString );
                    }
                    strings.Add( itemString );
                }
            }

            if ( finalDelimiter != null && strings.Count > 1 )
            {
                return String.Join( delimiter, strings.Take( strings.Count - 1 ).ToArray() ) + string.Format( "{0}{1}", finalDelimiter, strings.Last() );
            }
            else
            {
                return String.Join( delimiter, strings.ToArray() );
            }
        }

        /// <summary>
        /// Converts a List&lt;string&gt; to List&lt;guid&gt; only returning items that could be converted to a guid.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<Guid> AsGuidList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsGuidOrNull() ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
        }

        /// <summary>
        /// Converts a List&lt;string&gt; to List&lt;guid&gt; return a null for items that could not be converted to a guid
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<Guid?> AsGuidOrNullList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsGuidOrNull() ).ToList();
        }

        /// <summary>
        /// Converts a List&lt;string&gt; to List&lt;int&gt; only returning items that could be converted to a int.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<int> AsIntegerList( this IEnumerable<string> items )
        {
            return items.Select( a => a.AsIntegerOrNull() ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();
        }

        /// <summary>
        /// Joins a dictionary of items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="delimter">The delimter.</param>
        /// <returns></returns>
        public static string Join( this Dictionary<string, string> items, string delimter )
        {
            List<string> parms = new List<string>();
            foreach ( var item in items )
                parms.Add( string.Join( "=", new string[] { item.Key, item.Value } ) );
            return string.Join( delimter, parms.ToArray() );
        }

        /// <summary>
        /// Recursively flattens the specified source.
        /// http://stackoverflow.com/questions/5422735/how-do-i-select-recursive-nested-entities-using-linq-to-entity/5423024#5423024
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="childSelector">The child selector.</param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>( this IEnumerable<T> source, Func<T, IEnumerable<T>> childSelector )
        {
            // Flatten all the items without recursion or potential memory overflow
            var itemStack = new Stack<T>( source );
            while ( itemStack.Count > 0 )
            {
                T item = itemStack.Pop();
                yield return item;

                foreach ( T child in childSelector( item ) )
                {
                    itemStack.Push( child );
                }
            }
        }

        /// <summary>
        /// Takes the last n items from a List.
        /// http://stackoverflow.com/questions/3453274/using-linq-to-get-the-last-n-elements-of-a-collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="N">The n.</param>
        /// <returns></returns>
        public static IEnumerable<T> TakeLast<T>( this IEnumerable<T> source, int N )
        {
            return source.Skip( Math.Max( 0, source.Count() - N ) );
        }

        #endregion GenericCollection Extensions

        #region IQueryable extensions

        #region Where

        /// <summary>
        /// Queries a list of items that match the specified expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Where( queryable, parameterExpression, whereExpression, null, null );
        }

        /// <summary>
        /// Queries a list of items that match the specified expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            return Where( queryable, parameterExpression, whereExpression, sortProperty, null );
        }

        /// <summary>
        /// Queries the specified parameter expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="fetchTop">The fetch top.</param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>( this IQueryable<T> queryable, ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty, int? fetchTop = null )
        {
            if ( parameterExpression != null && whereExpression != null )
            {
                var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
                queryable = queryable.Where( lambda );
            }

            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }

            if ( fetchTop.HasValue )
            {
                queryable = queryable.Take( fetchTop.Value );
            }

            return queryable;
        }

        #endregion Where

        /// <summary>
        /// Orders the list by the name of a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>( this IQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "OrderBy" );
        }

        /// <summary>
        /// Distincts the by.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        /// http://stackoverflow.com/questions/489258/linq-distinct-on-a-particular-property
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            ( this IEnumerable<TSource> source, Func<TSource, TKey> keySelector )
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach ( TSource element in source )
            {
                if ( seenKeys.Add( keySelector( element ) ) )
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Orders the list by the name of a property in descending order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>( this IQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "OrderByDescending" );
        }

        /// <summary>
        /// Then Orders the list by the name of a property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>( this IOrderedQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "ThenBy" );
        }

        /// <summary>
        /// Then Orders the list by a a property in descending order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The type of object.</param>
        /// <param name="property">The property to order by.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>( this IOrderedQueryable<T> source, string property )
        {
            return ApplyOrder<T>( source, property, "ThenByDescending" );
        }

        private static IOrderedQueryable<T> ApplyOrder<T>( IQueryable<T> source, string property, string methodName )
        {
            string[] props = property.Split( '.' );
            Type type = typeof( T );
            ParameterExpression arg = Expression.Parameter( type, "x" );
            Expression expr = arg;
            foreach ( string prop in props )
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty( prop );
                if ( pi != null )
                {
                    expr = Expression.Property( expr, pi );
                    type = pi.PropertyType;
                }
            }
            Type delegateType = typeof( Func<,> ).MakeGenericType( typeof( T ), type );
            LambdaExpression lambda = Expression.Lambda( delegateType, expr, arg );

            object result = typeof( Queryable ).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2 )
                    .MakeGenericMethod( typeof( T ), type )
                    .Invoke( null, new object[] { source, lambda } );
            return (IOrderedQueryable<T>)result;
        }

        /// <summary>
        /// Sorts the object by the specified sort property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> Sort<T>( this IQueryable<T> source, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            if ( sortProperty.Property.StartsWith( "attribute:" ) )
            {
                var itemType = typeof( T );
                var attributeCache = AttributeCache.Read( sortProperty.Property.Substring( 10 ).AsInteger() );
                if ( attributeCache != null && typeof( IModel ).IsAssignableFrom( typeof( T ) ) )
                {
                    var entityIds = new List<int>();

                    var models = new List<IModel>();
                    source.ToList().ForEach( i => models.Add( i as IModel ) );
                    var ids = models.Select( m => m.Id ).ToList();

                    var field = attributeCache.FieldType.Field;

                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var attributeValue in new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.AttributeId == attributeCache.Id &&
                                v.EntityId.HasValue &&
                                ids.Contains( v.EntityId.Value ) )
                            .ToList() )
                        {
                            var model = models.FirstOrDefault( m => m.Id == attributeValue.EntityId.Value );
                            if ( model != null )
                            {
                                model.CustomSortValue = field.SortValue( null, attributeValue.Value, attributeCache.QualifierValues );
                            }
                        }
                    }

                    var result = new List<T>();
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        models.OrderBy( m => m.CustomSortValue ).ToList().ForEach( m => result.Add( (T)m ) );
                    }
                    else
                    {
                        models.OrderByDescending( m => m.CustomSortValue ).ToList().ForEach( m => result.Add( (T)m ) );
                    }

                    return result.AsQueryable().OrderBy( r => 0 );
                }
            }

            string[] columns = sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            IOrderedQueryable<T> qry = null;

            for ( int columnIndex = 0; columnIndex < columns.Length; columnIndex++ )
            {
                string column = columns[columnIndex].Trim();

                var direction = sortProperty.Direction;
                if ( column.ToLower().EndsWith( " desc" ) )
                {
                    column = column.Left( column.Length - 5 );
                    direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                }

                if ( direction == SortDirection.Ascending )
                {
                    qry = ( columnIndex == 0 ) ? source.OrderBy( column ) : qry.ThenBy( column );
                }
                else
                {
                    qry = ( columnIndex == 0 ) ? source.OrderByDescending( column ) : qry.ThenByDescending( column );
                }
            }

            return qry;
            
        }

        /// <summary>
        /// Filters a Query to rows that have matching attribute value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        /// <example>
        /// var test = new PersonService( rockContext ).Queryable().Where( a =&gt; a.FirstName == "Bob" ).WhereAttributeValue( rockContext, "BaptizedHere", "True" ).ToList();
        ///   </example>
        public static IQueryable<T> WhereAttributeValue<T>( this IQueryable<T> source, RockContext rockContext, string attributeKey, string attributeValue ) where T : Rock.Data.Model<T>, new()
        {
            int entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( typeof( T ) ) ?? 0;

            var avs = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == attributeKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                .Where( a => a.Value == attributeValue )
                .Select( a => a.EntityId );

            var result = source.Where( a => avs.Contains( ( a as T ).Id ) );
            return result;
        }

        /// <summary>
        /// Forces an Inner Join to the Person table using the specified key selector expression.
        /// Handy for optimizing a query that would have normally done an outer join 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qry">The qry.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<T> InnerJoinPerson<T>( this IQueryable<T> qry, Expression<Func<T, int>> keySelector, RockContext rockContext ) where T : IEntity
        {
            var qryPerson = new PersonService( rockContext ).Queryable( true, true );
            qry = qry.Join( qryPerson, keySelector, p => p.Id, ( t, p ) => t );
            return qry;
        }

        #endregion IQueryable extensions

        #region Dictionary<TKey, TValue> extension methods

        /// <summary>
        /// Adds or replaces an item in a Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrReplace<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
            else
            {
                dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an item to a Dictionary if it doesn't already exist in Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrIgnore<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
        }

        /// <summary>
        /// Gets value for the specified key, or null if the dictionary doesn't contain the key
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TValue GetValueOrNull<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return default( TValue );
            }
        }

        /// <summary>
        /// Gets ConfigurationValue's Value for the specified key, or null if the dictionary doesn't contain the key or the ConfigurationValue is null
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetValueOrNull<TKey>( this IDictionary<TKey, Rock.Field.ConfigurationValue> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) && dictionary[key] != null )
            {
                return dictionary[key].Value;
            }
            else
            {
                return null;
            }
        }

        #endregion Dictionary<TKey, TValue> extension methods
    }
}
