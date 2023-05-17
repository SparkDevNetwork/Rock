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
using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The results from the ValidateAddress API action of the LocationAddressPicker control.
    /// </summary>
    public class AddressControlValidateAddressResultsBag
    {
        /// <summary>
        /// If invalid, this is the message stating what is wrong with the given address
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Whether the given address is valid or not
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// If the address is valid, this is an HTML string of the address
        /// </summary>
        public string AddressString { get; set; }

        /// <summary>
        /// If the address is valid, this is an HTML string of the address
        /// </summary>
        public AddressControlBag Address { get; set; }
    }
}
