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

namespace Rock.Tests.Shared
{
    public static class ArrayExtensions
    {
        #region Random Item Selection

        private static Random _rng = new Random();

        /// <summary>
        /// Return a random item from an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>( this T[] items )
        {
            if ( items == null
                 || items.Length == 0 )
            {
                return default( T );
            }

            return items[_rng.Next( 0, items.Length )];
        }

        /// <summary>
        /// Return a random item from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T GetRandomElement<T>( this List<T> items )
        {
            if ( items == null
                 || items.Count == 0 )
            {
                return default( T );
            }

            return items[_rng.Next( 0, items.Count )];
        }

        /// <summary>
        /// Return a randomized list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<T> GetRandomizedList<T>( this IList<T> items )
        {
            if ( items == null )
            {
                return null;
            }

            return items.GetRandomizedList( items.Count );
        }

        /// <summary>
        /// Creates a randomized list of items from a source list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="maximumItems">The maximum number of items to select from the list.</param>
        /// <returns></returns>
        public static List<T> GetRandomizedList<T>( this IList<T> items, int maximumItems )
        {
            if ( items == null )
            {
                return null;
            }

            var randomizedList = new List<T>();

            // Create a set of index numbers representing each of the items in the original list.
            var indexes = new List<int>();

            var totalItemCount = items.Count;

            for ( int i = 0; i < totalItemCount; i++ )
            {
                indexes.Add( i );
            }

            if ( maximumItems > totalItemCount )
            {
                maximumItems = totalItemCount;
            }

            // Select a random sequence of index numbers and the items they represent to a new collection.
            var itemsRemaining = totalItemCount;

            for ( var i = 0; i < maximumItems; i++ )
            {
                // Pick a random index number, and add that item to the new list.
                // Note that for the random range, lower bound is inclusive and upper bound is exclusive.
                var randomIndex = _rng.Next( 0, itemsRemaining );

                var randomItemIndex = indexes[randomIndex];

                randomizedList.Add( items[randomItemIndex] );

                // Remove the item index number from the list of available items.
                indexes.Remove( randomItemIndex );

                itemsRemaining--;
            }

            return randomizedList;
        }

        #endregion
    }
}
