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

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class DataViewFilterOverride
    {
        /// <summary>
        /// Gets or sets the data filter unique identifier.
        /// </summary>
        /// <value>
        /// The data filter unique identifier.
        /// </value>
        public Guid DataFilterGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include filter]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeFilter { get; set; }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        public string Selection { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{DataFilterGuid}]  [{IncludeFilter}] Selection='{Selection}'";
        }
    }
}
