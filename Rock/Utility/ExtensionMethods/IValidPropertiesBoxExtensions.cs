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
using System.Linq;

using Rock.ViewModels.Utility;

namespace Rock
{
    /// <summary>
    /// 
    /// </summary>
    public static class IValidPropertiesBoxExtensions
    {
        /// <summary>
        /// Checks if the box contains the property in the list of Valid Properties.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool IsValidProperty( this IValidPropertiesBox box, string propertyName )
        {
            return box.ValidProperties.Contains( propertyName, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Perform the action if property name is present in the list of Valid Properties on the box
        /// </summary>
        /// <param name="box"></param>
        /// <param name="propertyName"></param>
        /// <param name="executeIfValid"></param>
        public static void IfValidProperty( this IValidPropertiesBox box, string propertyName, Action executeIfValid )
        {
            if ( IsValidProperty( box, propertyName ) )
            {
                executeIfValid();
            }
        }

        /// <summary>
        /// Perform the action if property name is present in the list of Valid Properties on the box
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="box"></param>
        /// <param name="propertyName"></param>
        /// <param name="executeIfValid"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TReturn IfValidProperty<TReturn>( this IValidPropertiesBox box, string propertyName, Func<TReturn> executeIfValid, TReturn defaultValue )
        {
            if ( IsValidProperty( box, propertyName ) )
            {
                return executeIfValid();
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
