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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockFormFieldViewModel
    /// </summary>
    public sealed class RegistrationEntryFormFieldBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public int FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public int PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public PublicAttributeBag Attribute { get; set; }

        /// <summary>
        /// Gets or sets the type of the visibility rule.
        /// </summary>
        /// <value>
        /// The type of the visibility rule.
        /// </value>
        public int VisibilityRuleType { get; set; }

        /// <summary>
        /// Gets or sets the visibility rules.
        /// </summary>
        /// <value>
        /// The visibility rules.
        /// </value>
        public List<RegistrationEntryVisibilityBag> VisibilityRules { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show on wait list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show on wait list]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnWaitList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shared value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared value; otherwise, <c>false</c>.
        /// </value>
        public bool IsSharedValue { get; set; }
    }
}
