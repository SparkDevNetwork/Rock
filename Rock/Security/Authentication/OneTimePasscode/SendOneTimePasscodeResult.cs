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
using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// The result from sending a one-time passcode.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class SendOneTimePasscodeResult
    {
        /// <summary>
        /// Indicates whether the passwordless login start step was successful.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the passwordless login start step was successful; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// The error message if the passwordless login start step was unsuccessful.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The auto-generated state value that should be sent during the passwordless login verify step.
        /// </summary>
        public string State { get; set; }
    }
}
