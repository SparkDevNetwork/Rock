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

namespace Rock.ViewModels.Blocks.Core.ComponentList
{
    /// <summary>
    /// The data bag returned when editing a component's attributes in the
    /// Component List block.
    /// </summary>
    public class ComponentListEditBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the component being edited.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public attribute definitions for the component,
        /// keyed by attribute key.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the public attribute values for the component,
        /// keyed by attribute key.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the SMTP warning.
        /// </summary>
        public bool IsSmtpTransport { get; set; }
    }
}
