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

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Display data for the person or business credited with a financial transaction.
    /// </summary>
    public class AuthorizedPersonBag : ITranslateIdKey
    {
        /// <summary>
        /// Gets or sets the person's unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the person's integer identifier.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated IdKey used for building person profile links.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of addresses associated with the person's family group.
        /// </summary>
        public List<AddressBag> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's primary campus.
        /// </summary>
        public string Campus { get; set; }
    }
}
