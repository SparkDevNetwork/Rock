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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// A single form field value of a registrant. Person fields are sent as text and
    /// attribute fields reference a shared attribute definition.
    /// </summary>
    public class RegistrantFieldValueBag
    {
        /// <summary>
        /// Gets or sets the label of the field.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the plain text value of a person field, or null for an attribute field.
        /// </summary>
        public string TextValue { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the attribute definition, or null for a person field.
        /// </summary>
        public Guid? AttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the public value of an attribute field.
        /// </summary>
        public string AttributeValue { get; set; }
    }
}
