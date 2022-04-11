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

/**
 * The signature value view model that will be passed to and emitted from the
 * electronic signature control.
 */
export type ElectronicSignatureValue = {
    /** The drawn signature data encoded as a URI. */
    signatureData?: string | null;

    /** The name of the person that signed the document. */
    signedByName?: string | null;

    /** The email provided to contact the signer. */
    signedByEmail?: string | null;
};
