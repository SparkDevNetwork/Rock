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

namespace Rock.ViewModel.Controls
{
    /// <summary>
    /// The model value used by an electronic signature control to provide all
    /// the data entered by the individual.
    /// </summary>
    public class ElectronicSignatureValueViewModel
    {
        /// <summary>
        /// Gets or sets the drawn signature data.
        /// </summary>
        /// <value>
        /// The drawn signature data.
        /// </value>
        public string SignatureData { get; set; }

        /// <summary>
        /// Gets or sets the name of the person that signed the document.
        /// </summary>
        /// <value>
        /// The name of the person the signed the document.
        /// </value>
        public string SignedByName { get; set; }

        /// <summary>
        /// Gets or sets the contact email address provided by the signer.
        /// </summary>
        /// <value>
        /// The contact email address provided by the signer.
        /// </value>
        public string SignedByEmail { get; set; }
    }
}
