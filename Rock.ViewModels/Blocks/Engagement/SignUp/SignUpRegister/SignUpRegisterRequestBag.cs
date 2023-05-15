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
    /// The information about registrants to be registered, updated or unregistered when registering for a sign-up project occurrence.
    /// </summary>
    public class SignUpRegisterRequestBag
    {
        /// <summary>
        /// Gets or sets the registrants to be registered, updated or unregistered.
        /// </summary>
        /// <value>
        /// The registrants to be registered, updated or unregistered.
        /// </value>
        public List<SignUpRegistrantBag> Registrants { get; set; }
    }
}
