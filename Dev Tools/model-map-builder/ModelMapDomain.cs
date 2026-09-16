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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A single Rock domain (e.g. "CRM", "Finance") and the models that belong
    /// to it.
    /// </summary>
    internal class ModelMapDomain
    {
        /// <summary>
        /// Gets or sets the domain name, or "Other" when a model has no
        /// <c>RockDomainAttribute</c>.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the full detail of every model in this domain.
        /// </summary>
        public List<ModelMapEntry> Models { get; set; }
    }
}
