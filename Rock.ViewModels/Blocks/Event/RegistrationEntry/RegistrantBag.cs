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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Registrant Info
    /// </summary>
    public sealed class RegistrantBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is on wait list.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is on wait list; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid? FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public Dictionary<Guid, object> FieldValues { get; set; }

        /// <summary>
        /// The registration cost for the registrant at the time of the registration.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the fee item quantities.
        /// </summary>
        /// <value>
        /// The fee item quantities.
        /// </value>
        public Dictionary<Guid, int> FeeItemQuantities { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of a previously-signed signature document that should be reused for this registrant.
        /// </summary>
        /// <value>
        /// The unique identifier of a previously-signed signature document that should be reused for this registrant.
        /// </value>
        public Guid? ExistingSignatureDocumentGuid { get; set; }

        /// <summary>
        /// Gets or sets the encoded signature data.
        /// </summary>
        /// <value>
        /// The encoded signature data.
        /// </value>
        public string SignatureData { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }
    }
}
