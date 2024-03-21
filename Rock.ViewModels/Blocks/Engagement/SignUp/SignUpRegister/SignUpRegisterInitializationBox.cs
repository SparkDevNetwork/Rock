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

using Rock.Enums.Blocks.Engagement.SignUp;
using Rock.ViewModels.Utility;

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// The box that contains all the initialization information for the Sign-Up Register block.
    /// </summary>
    public class SignUpRegisterInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the registration mode the block is in.
        /// </summary>
        /// <value>
        /// The registration mode the block is in.
        /// </value>
        public RegisterMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the optional title to display above the register form.
        /// </summary>
        /// <value>
        /// The optional title to display above the register form.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets whether this project has required group requirements.
        /// </summary>
        /// <value>
        /// Whether this project has required group requirements.
        /// </value>
        public bool ProjectHasRequiredGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets the communication preference items available for the registrant to select.
        /// </summary>
        /// <value>
        /// The communication preference items available for the registrant to select.
        /// </value>
        public List<ListItemBag> CommunicationPreferenceItems { get; set; }

        /// <summary>
        /// Gets or sets whether to require that a value be entered for email when registering in Anonymous mode.
        /// </summary>
        /// <value>
        /// Whether to require that a value be entered for email when registering in Anonymous mode.
        /// </value>
        public bool RequireEmail { get; set; }

        /// <summary>
        /// Gets or sets whether to require that a value be entered for mobile phone when registering in Anonymous mode.
        /// </summary>
        /// <value>
        /// Whether to require that a value be entered for mobile phone when registering in Anonymous mode.
        /// </value>
        public bool RequireMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the list of existing or possible registrants, including the registrar.
        /// <para>
        /// Each <see cref="SignUpRegistrantBag.WillAttend"/> value indicates whether they're already registered (<see langword="true" />) or available to be registered (<see langword="false" />).
        /// </para>
        /// </summary>
        /// <value>
        /// The list of existing or possible registrants, including the registrar.
        /// </value>
        public List<SignUpRegistrantBag> Registrants { get; set; }

        /// <summary>
        /// Gets or sets the registrant member attributes.
        /// </summary>
        /// <value>
        /// The registrant member attributes.
        /// </value>
        public Dictionary<string, PublicAttributeBag> MemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the registrant member opportunity attributes.
        /// </summary>
        /// <value>
        /// The registrant member opportunity attributes.
        /// </value>
        public Dictionary<string, PublicAttributeBag> MemberOpportunityAttributes { get; set; }
    }
}
