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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpRegister
{
    /// <summary>
    /// The information about registrants that were successfully registered, unregistered or unable to be registered, for a given register request.
    /// </summary>
    public class SignUpRegisterResponseBag
    {
        /// <summary>
        /// Gets or sets the full names of individuals who were successfully registered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were successfully registered for a sign-up project.
        /// </value>
        public List<string> RegisteredRegistrantNames { get; set; }

        /// <summary>
        /// Gets or sets the full names of individuals who were unregistered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were unregistered for a sign-up project.
        /// </value>
        public List<string> UnregisteredRegistrantNames { get; set; }

        /// <summary>
        /// Gets or sets the full names of individuals who were unable to be registered for a sign-up project.
        /// </summary>
        /// <value>
        /// The full names of individuals who were unable to be registered for a sign-up project.
        /// </value>
        public List<string> UnsuccessfulRegistrantNames { get; set; }

        /// <summary>
        /// Gets or sets the warning message for this registration attempt.
        /// </summary>
        /// <value>
        /// The warning message for this registration attempt.
        /// </value>
        public string WarningMessage { get; set; }
    }
}
