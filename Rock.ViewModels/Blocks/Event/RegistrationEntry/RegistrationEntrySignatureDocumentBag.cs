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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Defines the data required to display a signature document on a single
    /// registrant form.
    /// </summary>
    public sealed class RegistrationEntrySignatureDocumentBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of a previously-signed signature document that should be reused for this registrant.
        /// </summary>
        /// <value>
        /// The unique identifier of a previously-signed signature document that should be reused for this registrant.
        /// </value>
        public Guid? ExistingSignatureDocumentGuid { get; set; }

        /// <summary>
        /// Gets or sets the document HTML to be displayed on the form. This should
        /// be displayed inside an IFrame element to ensure no other CSS on the
        /// page interferes.
        /// </summary>
        /// <value>
        /// The document HTML to be displayed on the form.
        /// </value>
        public string DocumentHtml { get; set; }

        /// <summary>
        /// Gets or sets the security token that will be used to authenticate the
        /// document after it has been signed.
        /// </summary>
        /// <value>
        /// The security token that will be used to authenticate the document.
        /// </value>
        public string SecurityToken { get; set; }
    }
}
