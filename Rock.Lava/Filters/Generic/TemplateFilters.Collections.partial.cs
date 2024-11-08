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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        static Random _randomNumberGenerator = new Random();

        /// <summary>
        /// Determines whether the input collection contains the specified value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="containValue">The search value.</param>
        /// <returns>
        ///   <c>true</c> if the input collection contains the specified search value; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains( object input, object containValue )
        {
            var inputList = ( input as IList );

            if ( inputList != null )
            {
                return inputList.Contains( containValue );
            }

            return false;
        }

        /// <summary>
        /// Takes a collection and returns distinct values in that collection.
        /// </summary>
        /// <param name="input">A collection of objects.</param>
        /// <returns>A collection of objects with no repeating elements.</returns>
        /// <example>
        ///     {{ 'hello,test,one,hello,two,one,three' | Split:',' | Distinct | ToJSON }}    
        /// </example>
        public static IEnumerable Distinct( object input )
        {
            return Distinct( input, null );
        }

        /// <summary>
        /// Takes a collection and returns distinct values in that collection based on the property.
        /// </summary>
        /// <param name="input">A collection of objects.</param>
        /// <returns>A collection of objects with no repeating elements.</returns>
        /// <example><![CDATA[
        /// {% assign items = '[{"PersonId":1,"Title":"Mr"},{"PersonId":2,"Title","Mrs"},{"PersonId":1,"Title":"Dr"}]' | FromJSON %}
        /// {{ items | DistinctBy:'PersonId' | ToJSON }}
        /// 
        /// {% groupmember where:'PersonId == "1"' %}
        ///     {{ groupmemberItems | DistinctBy:'Person.FirstName' | Select:'Id' | ToJSON }}
        /// {% endgroupmember %}
        /// ]]></example>
        public static IEnumerable Distinct( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() )
            {
                return e;
            }

            if ( string.IsNullOrWhiteSpace( property ) )
            {
                // Materialize the enumerable to a List, determine the distinct values, then convert the
                // output of that operation to a List to ensure that it can be processed by downstream filters.
                return e.ToList().Distinct().ToList();
            }
            else
            {
                return e.GroupBy( d => d.GetPropertyValue( property ) ).Select( x => x.FirstOrDefault() );
            }
        }

        /// <summary>
        /// Takes a collection and groups it by the specified property tree.
        /// </summary>
        /// <param name="input">A collection of objects to be grouped.</param>
        /// <param name="property">The property to use when grouping the objects.</param>
        /// <returns>A dictionary of group keys and value collections.</returns>
        /// <example><![CDATA[
        /// {% assign members = 287635 | GroupById | Property:'Members' | GroupBy:'GroupRole.Name' %}
        /// <ul>
        /// {% for member in members %}
        ///     {% assign parts = member | PropertyToKeyValue %}
        ///     <li>{{ parts.Key }}</li>
        ///     <ul>
        ///         {% for m in parts.Value %}
        ///             <li>{{ m.Person.FullName }}</li>
        ///         {% endfor %}
        ///     </ul>
        /// {% endfor %}
        /// </ul>
        /// ]]></example>
        public static object GroupBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            if ( !e.Any() )
            {
                return new Dictionary<string, List<object>>();
            }

            if ( string.IsNullOrWhiteSpace( property ) )
            {
                throw new Exception( "Must provide a property to group by." );
            }

            var groupedList = e.AsQueryable()
                .GroupBy( x => x.GetPropertyValue( property ) )
                .ToDictionary( g => g.Key != null ? g.Key.ToString() : string.Empty, g => (object)g.ToList() );

            return groupedList;
        }

        /// <summary>
        /// Takes an enumerable and returns a new enumerable with the object appended.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <param name="newObject">The new object to append.</param>
        /// <returns>A new enumerable that contains the old objects and the new one.</returns>
        /// <example><![CDATA[
        /// {% assign array = '' | AddToArray:'one' %}
        /// {% assign array = array | AddToArray:'two' | AddToArray:'three' %}
        /// {% assign array = array | RemoveFromArray:'one' %}
        /// {{ array | ToJSON }}
        /// ]]></example>
        public static IEnumerable AddToArray( object input, object newObject )
        {
            List<object> array = new List<object>();

            if ( input == null || ( input is string && string.IsNullOrEmpty( (string)input ) ) )
            {
                /* Intentionally left blank, start with empty array. */
            }
            else if ( input is IEnumerable )
            {
                foreach ( object item in input as IEnumerable )
                {
                    array.Add( item );
                }
            }
            else
            {
                array.Add( input );
            }

            array.Add( newObject );

            return array;
        }

        /// <summary>
        /// Takes an enumerable and returns a new enumerable with all instances of the specified object removed.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <param name="removeObject">The object to remove.</param>
        /// <returns>A new enumerable that contains the old objects without any instances of the specified object.</returns>
        /// <example><![CDATA[
        /// {% assign array = '' | AddToArray:'one' %}
        /// {% assign array = array | AddToArray:'two' | AddToArray:'three' %}
        /// {% assign array = array | RemoveFromArray:'one' %}
        /// {{ array | ToJSON }}
        /// ]]></example>
        public static IEnumerable RemoveFromArray( object input, object removeObject )
        {
            List<object> array = new List<object>();

            if ( input == null || ( input is string && string.IsNullOrEmpty( (string)input ) ) )
            {
                /* Intentionally left blank, start with empty array. */
            }
            else if ( input is IEnumerable )
            {
                foreach ( object item in input as IEnumerable )
                {
                    array.Add( item );
                }
            }
            else
            {
                array.Add( input );
            }

            // Remove single instances until none remain in the collection.
            while ( array.Remove( removeObject ) );

            return array;
        }

        /// <summary>
        /// Extracts a single item from an array.
        /// </summary>
        /// <param name="input">The input object to extract one element from.</param>
        /// <param name="index">The index number of the object to extract.</param>
        /// <returns>The single object from the array or null if not found.</returns>
        public static object Index( object input, object index )
        {
            if ( input == null || index == null )
            {
                return input;
            }

            IList inputList;

            if ( ( input is IList ) )
            {
                inputList = input as IList;
            }
            else if ( ( input is IEnumerable ) )
            {
                inputList = ( input as IEnumerable ).Cast<object>().ToList();
            }
            else
            {
                return input;
            }

            var indexInt = index.ToString().AsIntegerOrNull();
            if ( !indexInt.HasValue || indexInt.Value < 0 || indexInt.Value >= inputList.Count )
            {
                return null;
            }

            return inputList[indexInt.Value];
        }

        /// <summary>
        /// Orders a collection of elements by the specified property (or properties)
        /// and returns a new collection in that order.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property">The property or properties to order the collection by.</param>
        /// <returns>A new collection sorted in the requested order.</returns>
        /// <example>
        ///     {% assign members = 287635 | GroupById | Property:'Members' | OrderBy:'GroupRole.IsLeader desc,Person.FullNameReversed' %}
        ///    <ul>
        ///    {% for member in members %}
        ///        <li>{{ member.Person.FullName }} - {{ member.GroupRole.Name }}</li>
        ///    {% endfor %}
        ///    </ul>
        /// </example>
        public static IEnumerable OrderBy( object input, string property )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object> { input };

            if ( !e.Any() || string.IsNullOrWhiteSpace( property ) )
            {
                return e;
            }

            //
            // Create a list of order by objects for the field to order by
            // and the ascending/descending flag.
            //
            var orderBy = property
                .Split( ',' )
                .Select( s => s.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries ) )
                .Select( a => new { Property = a[0], Descending = a.Length >= 2 && "desc" == a[1].ToLower() } )
                .ToList();

            //
            // Do initial ordering of first property requested.
            //
            IOrderedQueryable<object> qry;
            if ( orderBy[0].Descending )
            {
                qry = e.Cast<object>().AsQueryable().OrderByDescending( d => d.GetPropertyValue( orderBy[0].Property ) );
            }
            else
            {
                qry = e.Cast<object>().AsQueryable().OrderBy( d => d.GetPropertyValue( orderBy[0].Property ) );
            }

            //
            // For the rest use ThenBy and ThenByDescending.
            //
            for ( int i = 1; i < orderBy.Count; i++ )
            {
                var propertyName = orderBy[i].Property; // This can't be inlined. -dsh

                if ( orderBy[i].Descending )
                {
                    qry = qry.ThenByDescending( d => d.GetPropertyValue( propertyName ) );
                }
                else
                {
                    qry = qry.ThenBy( d => d.GetPropertyValue( propertyName ) );
                }
            }

            return qry.ToList();
        }

        /// <summary>
        /// Selects the set of values of a named property from the items in a collection.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="selectKey">The select key.</param>
        /// <returns></returns>
        public static object Select( object input, string selectKey )
        {
            if ( input == null )
            {
                return input;
            }

            var enumerableInput = input as IEnumerable;

            if ( enumerableInput == null )
            {
                return input;
            }

            var result = new List<object>();

            foreach ( var value in enumerableInput )
            {
                /* TODO: Reinstate after test.
                                if ( value is ILiquidizable )
                                {
                                    var liquidObject = value as ILiquidizable;
                                    if ( liquidObject.ContainsKey( selectKey ) )
                                    {
                                        result.Add( liquidObject[selectKey] );
                                    }
                                }
                                else
                */
                if ( value is IDictionary<string, object> )
                {
                    var dictionaryObject = value as IDictionary<string, object>;
                    if ( dictionaryObject.ContainsKey( selectKey ) )
                    {
                        result.Add( dictionaryObject[selectKey] );
                    }
                }
                else
                {
                    result.Add( value.GetPropertyValue( selectKey ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the size of an array or the length of a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int Size( object input )
        {
            if ( input is string )
            {
                return ( (string)input ).Length;
            }

            if ( input is IEnumerable )
            {
                return ( (IEnumerable)input ).Cast<object>().Count();
            }

            return 0;
        }

        /// <summary>
        /// Rearranges an array in a random order
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object Shuffle( object input )
        {
            if ( input == null )
            {
                return input;
            }

            if ( !( input is IList ) )
            {
                return input;
            }

            var inputList = input as IList;
            int n = inputList.Count;
            while ( n > 1 )
            {
                n--;
                int k = _randomNumberGenerator.Next( n + 1 );
                var value = inputList[k];
                inputList[k] = inputList[n];
                inputList[n] = value;
            }

            return inputList;
        }

        /// <summary>
        /// Takes an enumerable and returns the sum of all the values.
        /// </summary>
        /// <param name="input">The existing enumerable.</param>
        /// <returns>A sum of all values in the input.</returns>
        /// <example><![CDATA[
        /// Total: {{ '3,5,7' | Split:',' | Sum }}
        /// ]]></example>
        public static object Sum( object input )
        {
            IEnumerable<object> e = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );
            var array = e.ToList();

            if ( input == null || !e.Any() )
            {
                return 0;
            }

            bool isDouble = false;
            array.ForEach( a => isDouble = isDouble || a is double );

            bool isDecimal = false;
            array.ForEach( a => isDecimal = isDecimal || a is decimal );

            bool isInteger = false;
            array.ForEach( a => isInteger = isInteger || a is int );

            if ( isDouble )
            {
                return array.Select( a => Convert.ToDouble( a ) ).Sum();
            }
            else if ( isDecimal )
            {
                return array.Select( a => Convert.ToDecimal( a ) ).Sum();
            }
            else if ( isInteger )
            {
                return array.Select( a => Convert.ToInt32( a ) ).Sum();
            }
            else
            {
                var result = array.Select( a => a.ToString().AsDouble() ).Sum();

                return result == Math.Truncate( result ) ? Convert.ToInt32( result ) : result;
            }
        }
    }
}
