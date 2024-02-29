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

using Rock.Enums.Controls;
using Rock.Utility;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Converts the <see cref="RockCacheability"/> entity into a <see cref="RockCacheabilityBag"/>.
        /// </summary>
        /// <param name="rockCacheability">The rock cacheability.</param>
        /// <returns>Null if the <paramref name="rockCacheability"/> is null, else <see cref="RockCacheabilityBag"/></returns>
        public static RockCacheabilityBag ToCacheabilityBag( this RockCacheability rockCacheability )
        {
            if ( rockCacheability == null )
            {
                return null;
            }

            var cacheabilitybag = new RockCacheabilityBag()
            {
                MaxAge = rockCacheability.MaxAge.ToTimeIntervalBag(),
                RockCacheabilityType = ( RockCacheabilityType ) rockCacheability.RockCacheablityType,
                SharedMaxAge = rockCacheability.SharedMaxAge.ToTimeIntervalBag()
            };

            return cacheabilitybag;
        }

        /// <summary>
        /// Converts the <see cref="RockCacheabilityBag"/> entity into a <see cref="RockCacheability"/>.
        /// </summary>
        /// <param name="rockCacheabilityBag">The rock cacheability bag.</param>
        /// <returns>Null if the <paramref name="rockCacheabilityBag"/> is null, else <see cref="RockCacheability"/></returns>
        public static RockCacheability ToCacheability( this RockCacheabilityBag rockCacheabilityBag )
        {
            if ( rockCacheabilityBag == null )
            {
                return null;
            }

            var cacheabilityType = ( RockCacheablityType ) rockCacheabilityBag.RockCacheabilityType;
            var cacheability = new RockCacheability
            {
                RockCacheablityType = ( RockCacheablityType ) rockCacheabilityBag.RockCacheabilityType,
                MaxAge = cacheabilityType == RockCacheablityType.Public || cacheabilityType == RockCacheablityType.Private ? rockCacheabilityBag.MaxAge.ToTimeInterval() : null,
                SharedMaxAge = cacheabilityType == RockCacheablityType.Public ? rockCacheabilityBag.SharedMaxAge.ToTimeInterval() : null
            };

            return cacheability;
        }

        /// <summary>
        /// Converts the <see cref="TimeIntervalBag"/> entity into a <see cref="TimeInterval"/>.
        /// </summary>
        /// <param name="timeIntervalBag">The time interval bag.</param>
        /// <returns>Null if the <paramref name="timeIntervalBag"/> is null, else <see cref="TimeIntervalBag"/></returns>
        public static TimeIntervalBag ToTimeIntervalBag( this TimeInterval timeIntervalBag )
        {
            if ( timeIntervalBag == null )
            {
                return null;
            }

            var timeInterval = new TimeIntervalBag
            {
                Unit = timeIntervalBag.Unit,
                Value = timeIntervalBag.Value,
            };

            return timeInterval;
        }

        /// <summary>
        /// Converts the <see cref="TimeIntervalBag"/> entity into a <see cref="TimeInterval"/>.
        /// </summary>
        /// <param name="timeIntervalBag">The time interval bag.</param>
        /// <returns>Null if the <paramref name="timeIntervalBag"/> is null, else <see cref="TimeIntervalBag"/></returns>
        public static TimeInterval ToTimeInterval( this TimeIntervalBag timeIntervalBag )
        {
            if ( timeIntervalBag == null )
            {
                return null;
            }

            var timeInterval = new TimeInterval
            {
                Unit = timeIntervalBag.Unit,
                Value = timeIntervalBag.Value,
            };

            return timeInterval;
        }
    }
}
