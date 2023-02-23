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
using Rock.Enums.Blocks.Engagement.SignUp;

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
        /// Gets or sets whether to display the "send reminder using" option.
        /// </summary>
        /// <value>
        /// Whether to display the "send reminder using" option.
        /// </value>
        public bool DisplaySendReminderOption { get; set; }

        /// <summary>
        /// Gets or sets whether to require that a value be entered for email when registering in Anonymous mode.
        /// </summary>
        /// <value>
        /// Whether to require that a value be entered for email when registering in Anonymous mode.
        /// </value>
        public bool RequireEmail { get; set; }

        /// <summary>
        /// Gets or sets whether to require that a value be entered for email when registering in Anonymous mode.
        /// </summary>
        /// <value>
        /// Whether to require that a value be entered for email when registering in Anonymous mode.
        /// </value>
        public bool RequireMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the list of existing or possible registrants.
        /// <para>
        /// Each <see cref="SignUpRegistrantBag.WillAttend"/> indicates whether they're already registered (<see langword="true" />) or available to be registered (<see langword="false" />).
        /// </para>
        /// </summary>
        /// <value>
        /// The list of existing or possible registrants.
        /// </value>
        public List<SignUpRegistrantBag> Registrants { get; set; }
    }
}
