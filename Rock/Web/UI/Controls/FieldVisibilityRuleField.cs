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

using System;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This class is used to specify the information needed by the Field Visibility Rules Editor.
    /// </summary>
    public class FieldVisibilityRuleField
    {
        /// <summary>
        /// The Guid of the field that the rule applies to.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// The attribute the rule applies to.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public Model.Attribute Attribute { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public RegistrationFieldSource FieldSource { get; set; }
    }
}
