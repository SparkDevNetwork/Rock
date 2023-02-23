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
using Rock.Enums.Blocks.Security.Login;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the required information to render a login block's passwordless section.
    /// </summary>
    public class PasswordlessLoginOptionsBag
    {
        /// <summary>
        /// The passwordless authentication state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The passwordless authentication code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Indicating whether this automatic verification is required.
        /// </summary>
        public bool IsAutomaticVerificationRequired { get; set; }

        /// <summary>
        /// The passwordless authentication step.
        /// </summary>
        public PasswordlessLoginStep Step { get; set; }

        /// <summary>
        /// Indicates whether person selection is required.
        /// </summary>
        public bool IsPersonSelectionRequired { get; set; }

        /// <summary>
        /// The people matching the email or phone number.
        /// </summary>
        /// <remarks>Only set when multiple matches are found.</remarks>
        public List<ListItemBag> MatchingPeople { get; set; }
    }
}
