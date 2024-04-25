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
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
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
        /// Concatenate the items into a Delimited string
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
        /// Concatenate the items into a Delimited string an optionally htmlencode the strings
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
        /// Adds the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="item">The item.</param>
        /// <param name="IgnoreIfExists">if set to <c>true</c> [ignore if exists].</param>
        public static void Add<T>( this List<T> items, T item, bool IgnoreIfExists )
        {
            if ( !IgnoreIfExists || !items.Contains( item ) )
            {
                items.Add( item );
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
            return items.Select( a => a.AsIntegerOrNull() ).AsIntegerList();
        }

        /// <summary>
        /// Converts a List&lt;int?&gt; to List&lt;int&gt; only returning items that have a value.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static List<int> AsIntegerList( this IEnumerable<int?> items )
        {
            return items.Where( a => a.HasValue ).Select( a => a.Value ).ToList();
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{T}"/> of <see cref="string"/> values into
        /// their enumeration type. Only returns values that can be converted
        /// to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The enumeration type to be converted to.</typeparam>
        /// <param name="items">The items to be converted.</param>
        /// <returns>A list of <typeparamref name="T"/> enumeration values.</returns>
        public static List<T> AsEnumList<T>( this IEnumerable<string> items )
            where T : struct
        {
            return items.Select( a => a.ConvertToEnumOrNull<T>() )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();
        }

        /// <summary>
        /// Joins a dictionary of items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="delimter">The delimiter.</param>
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
        /// Then Orders the list by a property in descending order.
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

            return ( IOrderedQueryable<T> ) result;
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
            if ( sortProperty.Property.StartsWith( "attribute:" ) && source.Any() )
            {
                var itemType = typeof( T );
                var attributeCache = AttributeCache.Get( sortProperty.Property.Substring( 10 ).AsInteger() );
                if ( attributeCache != null && typeof( IModel ).IsAssignableFrom( typeof( T ) ) )
                {
                    List<int> ids = new List<int>();
                    var models = new List<IModel>();
                    source.ToList().ForEach( i => models.Add( i as IModel ) );

                    using ( var rockContext = new RockContext() )
                    {
                        //Check if Attribute Entity Type is same as Source Entity Type
                        var type = models.First().GetType();
                        EntityTypeCache modelEntity = EntityTypeCache.Get( type, false );
                        PropertyInfo modelProperty = null;
                        //Same Entity Type
                        if ( modelEntity != null && modelEntity.Id == attributeCache.EntityTypeId )
                        {
                            ids = models.Select( m => m.Id ).ToList();
                        }
                        //Different Entity Types
                        else if ( modelEntity != null )
                        {
                            //Search the entity properties for a matching entity and save the property information and primary key name
                            var propertiesWithAttributes = type.GetProperties().Where( a => typeof( IHasAttributes ).IsAssignableFrom( a.PropertyType ) ).ToList();
                            foreach ( var propertyInfo in propertiesWithAttributes )
                            {
                                var propertyEntity = EntityTypeCache.Get( propertyInfo.PropertyType, false );
                                if ( propertyEntity != null && propertyEntity.Id == attributeCache.EntityTypeId )
                                {
                                    Object obj = models.First().GetPropertyValue( propertyInfo.Name );
                                    var prop = obj.GetType().GetProperty( "Id" );
                                    modelProperty = propertyInfo;
                                    ids = models.Select( m => Int32.Parse( prop.GetValue( m.GetPropertyValue( propertyInfo.Name ) ).ToString() ) ).ToList();
                                }
                            }
                        }
                        var field = attributeCache.FieldType.Field;

                        foreach ( var attributeValue in new AttributeValueService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.AttributeId == attributeCache.Id &&
                                v.EntityId.HasValue &&
                                ids.Contains( v.EntityId.Value ) )
                            .ToList() )
                        {
                            IModel model = null;
                            if ( modelEntity != null && modelEntity.Id == attributeCache.EntityTypeId )
                            {
                                model = models.FirstOrDefault( m => m.Id == attributeValue.EntityId.Value );
                            }
                            else if ( modelEntity != null )
                            {
                                //Use the model property and primary key name to get the foreign key EntityId refers to
                                model = models.FirstOrDefault( m =>
                                {
                                    var obj = m.GetPropertyValue( modelProperty.Name );
                                    PropertyInfo prop = obj.GetType().GetProperty( "Id" );
                                    string val = prop.GetValue( obj ).ToString();
                                    return Int32.Parse( val ) == attributeValue.EntityId.Value;
                                } );
                            }
                            else
                            {
                                //Handle not finding an Entity type
                                model = null;
                            }
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
        public static IQueryable<T> WhereAttributeValue<T>( this IQueryable<T> source, RockContext rockContext, string attributeKey, string attributeValue ) where T : Entity<T>, new()
        {
            int entityTypeId = EntityTypeCache.GetId( typeof( T ) ) ?? 0;

            var avs = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == attributeKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                .Where( a => a.Value == attributeValue )
                .Select( a => a.EntityId );

            var result = source.Where( a => avs.Contains( ( a as T ).Id ) );
            return result;
        }

        /// <summary>
        /// Filters a Query to rows that have matching attribute values that meet the condition
        /// NOTE: Make sure your predicate references 'Attribute.Key' and not 'AttributeKey'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static IQueryable<T> WhereAttributeValue<T>( this IQueryable<T> source, RockContext rockContext, Expression<Func<AttributeValue, bool>> predicate ) where T : Entity<T>, new()
        {
            /*
              Example: 
              var qryPerson = new PersonService( rockContext ).Queryable().Where( a => a.FirstName == "Bob" )
                .WhereAttributeValue( rockContext, a => a.Attribute.Key == "IsAwesome" && a.ValueAsBoolean == true );
            */

            int entityTypeId = EntityTypeCache.GetId( typeof( T ) ) ?? 0;

            var avs = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                .Where( predicate )
                .Select( a => a.EntityId );

            var result = source.Where( a => avs.Contains( ( a as T ).Id ) );
            return result;
        }

        /// <summary>
        /// Wheres the campus.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        public static IQueryable<T> WhereCampus<T>( this IQueryable<T> source, RockContext rockContext, int campusId ) where T : Entity<T>, new()
        {
            int entityTypeId = EntityTypeCache.GetId( typeof( T ) ) ?? 0;

            var entityCampusFilterService = new EntityCampusFilterService( rockContext )
                .Queryable()
                .Where( e => e.CampusId == campusId )
                .Where( e => e.EntityTypeId == entityTypeId )
                .Select( e => e.EntityId );

            var result = source.Where( s => entityCampusFilterService.Contains( ( s as T ).Id ) );
            return result;
        }

        /// <summary>
        /// Filters the query to only those values that match the specified time
        /// period. The filtering is applied to a TimeSpan value that represents
        /// the time of day.
        /// </summary>
        /// <typeparam name="T">The type of the queryable.</typeparam>
        /// <param name="source">The query to be filtered.</param>
        /// <param name="timePeriod">The time period.</param>
        /// <param name="predicate">The predicate to access the <see cref="TimeSpan"/> value.</param>
        /// <returns>A queryable filtered to only items matching the time period.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timePeriod</exception>
        /// <example>
        /// This method takes a predicate that directs it to the TimeSpan
        /// value:
        /// <code>
        /// var query = new GroupService( rockContext ).Queryable().Where( g => g.Schedule.WeeklyTimeOfDay.HasValue );
        /// query = qry.WhereTimePeriodIs( TimePeriodOfDay.Morning, g => g.Schedule.WeeklyTimeOfDay.Value );
        /// </code>
        /// </example>
        public static IQueryable<T> WhereTimePeriodIs<T>( this IQueryable<T> source, TimePeriodOfDay timePeriod, Expression<Func<T, TimeSpan>> predicate )
        {
            Expression timePeriodExpression;
            var hourExpression = Expression.Property( predicate.Body, "Hours" );

            switch ( timePeriod )
            {
                case TimePeriodOfDay.Morning:
                    // hour < 12pm
                    timePeriodExpression = Expression.LessThan( hourExpression, Expression.Constant( 12 ) );
                    break;

                case TimePeriodOfDay.Afternoon:
                    // hour >= 12pm && hour < 5pm
                    timePeriodExpression = Expression.And(
                        Expression.GreaterThanOrEqual( hourExpression, Expression.Constant( 12 ) ),
                        Expression.LessThan( hourExpression, Expression.Constant( 17 ) ) );
                    break;

                case TimePeriodOfDay.Evening:
                    // hour >= 6pm
                    timePeriodExpression = Expression.GreaterThanOrEqual( hourExpression, Expression.Constant( 17 ) );
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof( timePeriod ) );
            }

            return source.Where( predicate.Parameters[0], timePeriodExpression );
        }

        /// <summary>
        /// Filters the query to only those values that match one of the time
        /// periods. The filtering is applied to a TimeSpan value that represents
        /// the time of day.
        /// </summary>
        /// <typeparam name="T">The type of the queryable.</typeparam>
        /// <param name="source">The query to be filtered.</param>
        /// <param name="timePeriods">The time periods.</param>
        /// <param name="predicate">The predicate to access the <see cref="TimeSpan"/> value.</param>
        /// <returns>A queryable filtered to only items matching the time periods.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timePeriods</exception>
        /// <example>
        /// This method takes a predicate that directs it to the TimeSpan
        /// value:
        /// <code>
        /// var query = new GroupService( rockContext ).Queryable().Where( g => g.Schedule.WeeklyTimeOfDay.HasValue );
        /// query = qry.WhereTimePeriodIsOneOf( new[] { TimePeriodOfDay.Morning }, g => g.Schedule.WeeklyTimeOfDay.Value )
        /// </code>
        /// </example>
        public static IQueryable<T> WhereTimePeriodIsOneOf<T>( this IQueryable<T> source, IEnumerable<TimePeriodOfDay> timePeriods, Expression<Func<T, TimeSpan>> predicate )
        {
            Expression timePeriodExpression = null;
            var hourExpression = Expression.Property( predicate.Body, "Hours" );

            foreach ( var timePeriod in timePeriods )
            {
                Expression expr;

                switch ( timePeriod )
                {
                    case TimePeriodOfDay.Morning:
                        // hour < 12pm
                        expr = Expression.LessThan( hourExpression, Expression.Constant( 12 ) );
                        break;

                    case TimePeriodOfDay.Afternoon:
                        // hour >= 12pm && hour < 5pm
                        expr = Expression.And(
                            Expression.GreaterThanOrEqual( hourExpression, Expression.Constant( 12 ) ),
                            Expression.LessThan( hourExpression, Expression.Constant( 17 ) ) );
                        break;

                    case TimePeriodOfDay.Evening:
                        // hour >= 5pm
                        expr = Expression.GreaterThanOrEqual( hourExpression, Expression.Constant( 17 ) );
                        break;

                    default:
                        throw new ArgumentOutOfRangeException( nameof( timePeriods ) );
                }

                timePeriodExpression = timePeriodExpression != null ? Expression.Or( timePeriodExpression, expr ) : expr;
            }

            if ( timePeriodExpression != null )
            {
                return source.Where( predicate.Parameters[0], timePeriodExpression );
            }
            else
            {
                return source;
            }
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

        /// <summary>
        /// Gets the underlying <see cref="ObjectQuery{T}"/> that represents the provided <see cref="IQueryable{T}"/>.
        /// <para>
        /// This is useful to gain access to the actual SQL query and parameters that will be executed against the database.
        /// </para>
        /// </summary>
        /// <remarks>
        /// https://www.stevefenton.co.uk/blog/2015/07/getting-the-sql-query-from-an-entity-framework-iqueryable/
        /// </remarks>
        /// <typeparam name="T">The type of the source query.</typeparam>
        /// <param name="source">The source query.</param>
        /// <returns>The underlying <see cref="ObjectQuery{T}"/> that represents the provided <see cref="IQueryable{T}"/>.</returns>
        internal static ObjectQuery<T> ToObjectQuery<T>( this IQueryable<T> source )
        {
            try
            {
                var internalQueryField = source
                    .GetType()
                    .GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( f => f.Name.Equals( "_internalQuery" ) )
                    .FirstOrDefault();

                var internalQuery = internalQueryField.GetValue( source );

                var objectQueryField = internalQuery
                    .GetType()
                    .GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( f => f.Name.Equals( "_objectQuery" ) )
                    .FirstOrDefault();

                return objectQueryField.GetValue( internalQuery ) as ObjectQuery<T>;
            }
            catch
            {
                return null;
            }
        }

        #endregion IQueryable extensions

        #region Expression extensions

        /// <summary>
        /// Replaces a parameter in the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="parameterName">Name of the parameter to be replaced.</param>
        /// <param name="parameterExpression">The parameter expression to use in the replacement.</param>
        /// <returns>A new Expression if the original expression was modified; otherwise the original expression.</returns>
        public static Expression ReplaceParameter( this Expression expression, string parameterName, ParameterExpression parameterExpression )
        {
                var filterExpressionVisitor = new ParameterExpressionVisitor( parameterExpression, parameterName );

                return filterExpressionVisitor.Visit( expression );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Helps rewrite the expression by replacing the parameter expression
        /// in the expression with another parameterExpression.
        /// </summary>
        private class ParameterExpressionVisitor : ExpressionVisitor
        {
            #region Fields

            /// <summary>
            /// The new parameter expression to use when replacing existing ones.
            /// </summary>
            private readonly ParameterExpression _parameterExpression;

            /// <summary>
            /// The name of the parameter expression to be replaced.
            /// </summary>
            private readonly string _parameterName;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterExpressionVisitor"/> class.
            /// </summary>
            /// <param name="parameterExpression">The parameter expression to use in replacement.</param>
            /// <param name="parameterName">Name of the parameter to be replaced.</param>
            public ParameterExpressionVisitor( ParameterExpression parameterExpression, string parameterName )
            {
                this._parameterExpression = parameterExpression;
                this._parameterName = parameterName;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Visits the parameter.
            /// </summary>
            /// <param name="p">The application.</param>
            /// <returns></returns>
            protected override Expression VisitParameter( ParameterExpression p )
            {
                if ( p.Name == _parameterName )
                {
                    p = _parameterExpression;
                }

                return base.VisitParameter( p );
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Provides additional logical operations to simplify the process of constructing Linq predicates.
    /// </summary>
    /// <remarks>
    /// Adapted from https://petemontgomery.wordpress.com/2011/02/10/a-universal-predicatebuilder/.
    /// </remarks>
    public static class LinqPredicateBuilder
    {
        /// <summary>
        /// Creates a predicate that evaluates to true.
        /// </summary>
        public static Expression<Func<T, bool>> True<T>() { return param => true; }

        /// <summary>
        /// Creates a predicate that evaluates to false.
        /// </summary>
        public static Expression<Func<T, bool>> False<T>() { return param => false; }

        /// <summary>
        /// Creates a predicate expression from the specified lambda expression.
        /// </summary>
        public static Expression<Func<T, bool>> Create<T>( Expression<Func<T, bool>> predicate ) { return predicate; }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public static Expression<Func<T, bool>> And<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.AndAlso );
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.OrElse );
        }

        /// <summary>
        /// Negates the predicate.
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>( this Expression<Func<T, bool>> expression )
        {
            var negated = Expression.Not( expression.Body );
            return Expression.Lambda<Func<T, bool>>( negated, expression.Parameters );
        }

        /// <summary>
        /// Combines the first expression with the second using the specified merge function.
        /// </summary>
        static Expression<T> Compose<T>( this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge )
        {
            // Map expression parameters of second to parameters of first.
            var map = first.Parameters
                .Select( ( f, i ) => new { f, s = second.Parameters[i] } )
                .ToDictionary( p => p.s, p => p.f );

            // Replace parameters in the second lambda expression with the parameters in the first.
            var secondBody = ParameterRebinder.ReplaceParameters( map, second.Body );

            // Create a merged lambda expression with parameters from the first expression.
            return Expression.Lambda<T>( merge( first.Body, secondBody ), first.Parameters );
        }

        /// <summary>
        /// An Expression Visitor that replaces one parameter with another in an Expression.
        /// </summary>
        private class ParameterRebinder : ExpressionVisitor
        {
            readonly Dictionary<ParameterExpression, ParameterExpression> map;

            ParameterRebinder( Dictionary<ParameterExpression, ParameterExpression> map )
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters( Dictionary<ParameterExpression, ParameterExpression> map, Expression exp )
            {
                return new ParameterRebinder( map ).Visit( exp );
            }

            protected override Expression VisitParameter( ParameterExpression p )
            {
                ParameterExpression replacement;

                if ( map.TryGetValue( p, out replacement ) )
                {
                    p = replacement;
                }

                return base.VisitParameter( p );
            }
        }
    }
}