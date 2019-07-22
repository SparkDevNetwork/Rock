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

namespace Rock.Tests.Integration.Utility
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
            return items[_rng.Next( 0, items.Count )];
        }

        #endregion

    }
}
