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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// A generic valid properties bag that uses "Bag" as the main property name.
    /// </summary>
    /// <typeparam name="TPropertyBag">The type of the property bag.</typeparam>
    public class ValidPropertiesBox<TPropertyBag> : IValidPropertiesBox
        where TPropertyBag : class
    {
        /// <summary>
        /// Gets or sets the bag that contains the properties.
        /// </summary>
        /// <value>The bag that contains the properties.</value>
        public TPropertyBag Bag { get; set; }

        /// <summary>
        /// Gets or sets the valid properties.
        /// </summary>
        /// <value>The valid properties.</value>
        public List<string> ValidProperties { get; set; }
    }
}
