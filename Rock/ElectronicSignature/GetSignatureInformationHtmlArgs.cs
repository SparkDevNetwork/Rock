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

using Rock.Model;

namespace Rock.ElectronicSignature
{
    /// <summary>
    /// Class GetSignatureInformationHtmlArgs. This class cannot be inherited.
    /// </summary>
    public sealed class GetSignatureInformationHtmlArgs
    {
        /// <summary>
        /// Gets or sets the type of the signature.
        /// </summary>
        /// <value>The type of the signature.</value>
        public SignatureType SignatureType { get; set; }

        /// <summary>
        /// Gets or sets the typed signature when SignatureType is <see cref="SignatureType.Typed"/>
        /// or full legal name when SignatureType is <see cref="SignatureType.Drawn"/>
        /// </summary>
        /// <value>The typed signature.</value>
        public string SignedName { get; set; }

        /// <summary>
        /// Gets or sets the drawn signature data URL.
        /// </summary>
        /// <value>The drawn signature data URL.</value>
        public string DrawnSignatureDataUrl { get; set; }

        /// <summary>
        /// Gets or sets the signed by person.
        /// </summary>
        /// <value>The signed by person.</value>
        public Person SignedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the signed date time.
        /// </summary>
        /// <value>The signed date time.</value>
        public DateTime? SignedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the client ip address.
        /// </summary>
        /// <value>The client ip address.</value>
        public string SignedClientIp { get; set; }

        /// <summary>
        /// Gets or sets the signature hash.
        /// </summary>
        /// <value>The signature hash.</value>
        public string SignatureVerificationHash { get; set; }
    }
}
