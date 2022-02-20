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

namespace Rock.ViewModel.NonEntities
{
    /// <summary>
    /// Describes the data sent to and from remote systems to allow editing of
    /// attribute filter values.
    /// </summary>
    public class PublicFilterableAttributeViewModel
    {
        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>The attribute unique identifier.</value>
        public Guid AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>The field type unique identifier.</value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the display name of the attribute.
        /// </summary>
        /// <value>The display name of the attribute.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the help text that describes this attribute.
        /// </summary>
        /// <value>The help text that describes this attribute.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the configuration values for how to display and edit
        /// values.
        /// </summary>
        /// <value>The configuration values for how to display and edit values.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
