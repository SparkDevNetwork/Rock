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
namespace Rock.SystemGuid
{
    /// <summary>
    /// System Groups
    /// </summary>
    public static class Group
    {
        /// <summary>
        /// RSR - Rock Administration
        /// </summary>
        public const string GROUP_ADMINISTRATORS = "628C51A8-4613-43ED-A18D-4A6FB999273E";

        /// <summary>
        /// RSR - Benevolence Group Guid
        /// </summary>
        public const string GROUP_BENEVOLENCE = "02FA0881-3552-42B8-A519-D021139B800F";

        /// <summary>
        /// RSR - Event Registration Administration Guid
        /// </summary>
        public const string GROUP_EVENT_REGISTRATION_ADMINISTRATORS = "2A92086B-DFF0-4B9C-46CB-4DAD805615AF";

        /// <summary>
        /// RSR - Mobile Application Users
        /// </summary>
        public const string GROUP_MOBILE_APPLICATION_USERS = "42175217-1BA4-401B-AA4E-21EC4F1F0AB4";

        /// <summary>
        /// The Calendar Administrators Group guid
        /// </summary>
        public const string GROUP_CALENDAR_ADMINISTRATORS = "FDA9D63F-B0B1-43E8-8B82-0255E5D99F26";

        /// <summary>
        /// The Communication Administrators Group guid
        /// </summary>
        public const string GROUP_COMMUNICATION_ADMINISTRATORS = "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B";

        /// <summary>
        /// The Connection Administrators Group guid
        /// </summary>
        public const string GROUP_CONNECTION_ADMINISTRATORS = "060971D2-EAF9-4C0D-B6F6-F01725CAA5AC";

        /// <summary>
        /// The Finance Administrators Group guid
        /// </summary>
        public const string GROUP_FINANCE_ADMINISTRATORS = "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559";

        /// <summary>
        /// The Finance Users Group guid
        /// </summary>
        public const string GROUP_FINANCE_USERS = "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9";

        /// <summary>
        /// The security group used by mobile app
        /// </summary>
        /// <remarks>Use <see cref="GROUP_MOBILE_APPLICATION_USERS"/></remarks>
        [RockObsolete( "1.15" )]
        public const string GROUP_MOBILE_APP = "EDD336D5-1429-41D9-8D41-2581A05F0E16";

        /// <summary>
        /// Get the photo request application group
        /// </summary>
        public const string GROUP_PHOTO_REQUEST = "2108EF9C-10DC-4466-973D-D25AAB7818BE";

        /// <summary>
        /// Gets the staff member group guid (Staff Users)
        /// </summary>
        public const string GROUP_STAFF_MEMBERS = "2C112948-FF4C-46E7-981A-0257681EADF4";

        /// <summary>
        /// Gets the staff-like member group guid (Staff Users)
        /// </summary>
        public const string GROUP_STAFF_LIKE_MEMBERS = "300BA2C8-49A3-44BA-A82A-82E3FD8C3745";

        /// <summary>
        /// The group of communication approvers
        /// </summary>
        public const string GROUP_COMMUNICATION_APPROVERS = "74B1B26E-1955-49A7-4C59-ABCD7543FF71";

        /// <summary>
        /// Group of people who are responsible for the safety and security of staff and members
        /// </summary>
        public const string GROUP_SAFETY_SECURITY = "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB";

        /// <summary>
        /// Group of people who are responsible for the integrity of the data.
        /// </summary>
        public const string GROUP_DATA_INTEGRITY_WORKER = "40517E10-0F2D-4C61-AA8D-BDE36D58C63A";

        /// <summary>
        /// WEB - Admistration
        /// </summary>
        public const string GROUP_WEB_ADMINISTRATORS = "1918E74F-C00D-4DDD-94C4-2E7209CE12C3";

        /// <summary>
        /// RSR - Prayer Access
        /// </summary>
        public const string GROUP_RSR_PRAYER_ACCESS = "9E17621E-F559-44E9-8C40-E8CF44CF8FCF";

        /// <summary>
        /// A parent Group for all Sign-up Groups
        /// </summary>
        public const string GROUP_SIGNUP_GROUPS = "D649638A-EF91-42D8-9B38-32172D614A5F";
    }
}