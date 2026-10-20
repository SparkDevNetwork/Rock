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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The details of a single registrant shown below the registration.
    /// </summary>
    public class RegistrantPanelBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the registrant.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the full name of the registrant.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the first name of the registrant.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person profile of the registrant, or null when the
        /// registrant has no person.
        /// </summary>
        public string PersonProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registrant is on the wait list.
        /// </summary>
        public bool IsOnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the formatted discounted total cost shown in the panel header,
        /// or null when it is zero.
        /// </summary>
        public string CostLabelText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group row is shown.
        /// </summary>
        public bool IsGroupVisible { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the group member the registrant was
        /// placed as, or null when the registrant has not been placed in the group.
        /// </summary>
        public string GroupMemberIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the group the registrant was placed in.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group member has been archived.
        /// </summary>
        public bool IsGroupMemberArchived { get; set; }

        /// <summary>
        /// Gets or sets the form field values of the registrant.
        /// </summary>
        public List<RegistrantFieldValueBag> Fields { get; set; }

        /// <summary>
        /// Gets or sets the formatted cost of the registrant, or null when there is no cost.
        /// </summary>
        public string CostText { get; set; }

        /// <summary>
        /// Gets or sets the formatted discounted cost of the registrant, or null when
        /// it is the same as the cost.
        /// </summary>
        public string DiscountedCostText { get; set; }

        /// <summary>
        /// Gets or sets the fees of the registrant.
        /// </summary>
        public List<RegistrantFeeRowBag> Fees { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the signature document status is shown.
        /// </summary>
        public bool IsSignatureDocumentVisible { get; set; }

        /// <summary>
        /// Gets or sets the name of the required signature document template.
        /// </summary>
        public string SignatureDocumentTemplateName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the signature document of the registrant has been signed.
        /// </summary>
        public bool IsSignatureDocumentSigned { get; set; }

        /// <summary>
        /// Gets or sets the URL of the signed document.
        /// </summary>
        public string SignatureDocumentUrl { get; set; }

        /// <summary>
        /// Gets or sets the formatted date the document was signed, or null when unknown.
        /// </summary>
        public string SignedOnText { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who signed the document.
        /// </summary>
        public string SignedByName { get; set; }
    }
}
