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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetActiveChildren API action of
    /// the LocationPicker control.
    /// </summary>
    public class LocationItemPickerGetActiveChildrenOptionsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the parent location whose
        /// children are to be enumerated.
        /// </summary>
        /// <value>The unique identifier of the parent location.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the root location unique identifier. This is used if <see cref="Guid"/>
        /// is empty to specify the root location to limit the tree to.
        /// </summary>
        /// <value>The root location unique identifier.</value>
        public Guid RootLocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }

        /// <summary>
        /// Gets or sets the values that need to be expanded to. This is used
        /// when opening the tree view with an already selected value. Each
        /// selected value is included in this property. When getting the list
        /// of root items, you should automatically expand your results until
        /// each of these values is reached.
        /// </summary>

        public List<string> ExpandToValues { get; set; }
    }
}
