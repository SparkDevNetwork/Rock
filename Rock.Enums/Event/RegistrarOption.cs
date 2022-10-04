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

namespace Rock.Model
{
    /// <summary>
    /// How registrar information should be collected.
    /// </summary>
    [Enums.EnumDomain( "Event" )]
    public enum RegistrarOption
    {
        /// <summary>
        /// Prompt for registrar
        /// </summary>
        PromptForRegistrar = 0,

        /// <summary>
        /// Prefill first registrant
        /// </summary>
        PrefillFirstRegistrant = 1,

        /// <summary>
        /// Use first registrant
        /// </summary>
        UseFirstRegistrant = 2,

        /// <summary>
        /// Use the LoggedIn person and keep fields readonly, except for fields that haven't been collected yet
        /// For example, if EmailAddress wasn't known, Email would be prompted vs readonly.
        /// </summary>
        UseLoggedInPerson = 3
    }
}
