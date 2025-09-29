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
namespace Rock.SystemKey
{
    /// <summary>
    /// The Global Attribute Key class
    /// </summary>
    public class GlobalAttributeKey
    {
        #region Organization

        /// <summary>
        /// The organization abbreviation.
        /// </summary>
        public const string ORGANIZATION_ABBREVATION = "OrganizationAbbreviation";

        /// <summary>
        /// The organization email.
        /// </summary>
        public const string ORGANIZATION_EMAIL = "OrganizationEmail";

        /// <summary>
        /// The organization name.
        /// </summary>
        public const string ORGANIZATION_NAME = "OrganizationName";

        /// <summary>
        /// The organization phone.
        /// </summary>
        public const string ORGANIZATION_PHONE = "OrganizationPhone";

        #endregion Organization

        /// <summary>
        /// List of Exceptions to be filtered out from the Exception Log
        /// </summary>
        public const string EXCEPTION_LOG_FILTER = "core_ExceptionLogFilter";
    }
}
