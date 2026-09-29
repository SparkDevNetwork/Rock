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

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// An existing attribute that can be selected as a registrant form field.
    /// </summary>
    public class RegistrationTemplateAttributeItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the attribute.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key of the attribute.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of the field type of the attribute.
        /// </summary>
        public string FieldTypeName { get; set; }
    }
}
