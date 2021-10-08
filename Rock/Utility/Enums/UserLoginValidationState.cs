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

namespace Rock.Utility
{
    /// <summary>
    /// The result of a request to verify and validate a UserLogin. This can
    /// be used anywhere the state of a login is needed, such as when trying
    /// to authenticate.
    /// </summary>
    internal enum UserLoginValidationState
    {
        /// <summary>
        /// The request succeeded and is considered valid.
        /// </summary>
        Valid = 0,

        /// <summary>
        /// The Username is not valid or is not found.
        /// </summary>
        InvalidUsername = 1,

        /// <summary>
        /// The Password is not valid or is incorrect.
        /// </summary>
        InvalidPassword = 2,

        /// <summary>
        /// The account is not confirmed.
        /// </summary>
        NotConfirmed = 3,

        /// <summary>
        /// The account is locked out.
        /// </summary>
        LockedOut = 4
    }
}
