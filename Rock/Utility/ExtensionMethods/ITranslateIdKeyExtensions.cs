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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock
{
    /// <summary>
    /// Extension methods for working with <see cref="ITranslateIdKey"/> instances.
    /// </summary>
    public static class ITranslateIdKeyExtensions
    {
        /// <summary>
        /// Translates the identifier to identifier key for the instance. If
        /// the instance has an Id value, it will be converted to an IdKey and
        /// the Id property will be set to null.
        /// </summary>
        /// <param name="instance">The instance to translate.</param>
        public static void TranslateIdToIdKey( this ITranslateIdKey instance )
        {
            if ( instance != null && instance.Id.HasValue )
            {
                instance.IdKey = instance.Id.Value.AsIdKey();
                instance.Id = null;
            }
        }

        /// <summary>
        /// Translates the identifier to identifier key for the set of
        /// instances. If the instance has an Id value, it will be converted
        /// to an IdKey and the Id property will be set to null.
        /// </summary>
        /// <param name="instances">The instances to translate.</param>
        public static void TranslateIdToIdKey( this IEnumerable<ITranslateIdKey> instances )
        {
            if ( instances == null )
            {
                return;
            }

            foreach ( var instance in instances )
            {
                if ( instance.Id.HasValue )
                {
                    instance.IdKey = instance.Id.Value.AsIdKey();
                    instance.Id = null;
                }
            }
        }
    }
}
