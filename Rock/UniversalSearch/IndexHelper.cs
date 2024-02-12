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

using Rock.Attribute;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Helper that contains logic or methods related to indexing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal("1.17")]
    public static class IndexHelper
    {
        /// <summary>
        /// Gets a list of index types that relate to the specified <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="requestedType">The Type of the <see cref="Rock.Model.EntityType.IndexModelType"/> requested.</param>
        /// <returns>The requested <see cref="Rock.Model.EntityType.IndexModelType"/> and any related IndexModelTypes.</returns>
        public static List<Type> GetRelatedIndexes( Type requestedType )
        {
            if ( requestedType == typeof( PersonIndex ) )
            {
                return new List<Type> { typeof( PersonIndex ), typeof( BusinessIndex ) };
            }
            else
            {
                return new List<Type> { requestedType };
            }
        }
    }
}
