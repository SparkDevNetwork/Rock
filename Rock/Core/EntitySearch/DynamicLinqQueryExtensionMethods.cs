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

using Rock.Data;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Definitions of extension methods that are available for use inside
    /// Dynamic LINQ queries.
    /// </summary>
    static class DynamicLinqQueryExtensionMethods
    {
        /// <summary>
        /// Determines if the entity is listed inside the dataview.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns><c>true</c> if the data view contains the entity, <c>false</c> otherwise.</returns>
        public static bool IsInDataView( this IEntity entity, int dataViewId )
        {
            throw new NotSupportedException( "Calling entity search query extension methods directly is not supported." );
        }
    }
}
