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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The response that will be returned by the SearchForFamilies check-in
    /// REST endpoint.
    /// </summary>
    public class SearchForFamiliesResponseBag
    {
        /// <summary>
        /// Gets or sets the families that matched the search request.
        /// </summary>
        /// <value>The families that matched the search request.</value>
        public List<FamilyBag> Families { get; set; }
    }
}
