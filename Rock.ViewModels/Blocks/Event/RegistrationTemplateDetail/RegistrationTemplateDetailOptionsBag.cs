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

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// Additional options for the <c>RegistrationTemplateDetail</c> block.
    /// </summary>
    public class RegistrationTemplateDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the currency settings of the organization. Used to format the
        /// costs, fees and discount amounts that are shown as text.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the group types that can be selected
        /// for the template and for placement configurations. Only group types that
        /// are shown in navigation are included.
        /// </summary>
        public List<Guid> GroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the signature document templates that can be required for
        /// registrations of this template. Inactive templates are only included when
        /// they are currently selected.
        /// </summary>
        public List<ListItemBag> SignatureDocumentTemplates { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the signature document templates that
        /// use a legacy external provider. Selecting any other template disables the
        /// external registration update option.
        /// </summary>
        public List<Guid> LegacySignatureDocumentTemplateGuids { get; set; }

        /// <summary>
        /// Gets or sets all field types. Used to display the field type name of
        /// registrant attributes that have not been saved yet.
        /// </summary>
        public List<ListItemBag> FieldTypes { get; set; }

        /// <summary>
        /// Gets or sets the school grade options for the eligibility grade range. The
        /// value is the grade offset and the text is the grade abbreviation.
        /// </summary>
        public List<ListItemBag> GradeOptions { get; set; }
    }
}
