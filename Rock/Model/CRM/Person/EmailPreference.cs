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

namespace Rock.Model
{
    /// <summary>
    /// The person's email preference
    /// </summary>
    public enum EmailPreference
    {
        /// <summary>
        /// Emails can be sent to person
        /// </summary>
        EmailAllowed = 0,

        /// <summary>
        /// No Mass emails should be sent to person
        /// </summary>
        NoMassEmails = 1,

        /// <summary>
        /// No emails should be sent to person
        /// </summary>
        DoNotEmail = 2
    }
}
