// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Groups
    /// </summary>
    public static class Group
    {
        /// <summary>
        /// Gets the administrator group guid (Rock Administrators)
        /// </summary>
        public const string GROUP_ADMINISTRATORS= "628C51A8-4613-43ED-A18D-4A6FB999273E";

        /// <summary>
        /// The Communication Administrators Group guid
        /// </summary>
        public const string GROUP_COMMUNICATION_ADMINISTRATORS = "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B";

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
    }
}