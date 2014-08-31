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
        /// Gets the staff member group guid (Staff Users)
        /// </summary>
        public const string GROUP_STAFF_MEMBERS= "2C112948-FF4C-46E7-981A-0257681EADF4";

        /// <summary>
        /// Get the photo request application group
        /// </summary>
        public const string GROUP_PHOTO_REQUEST = "2108EF9C-10DC-4466-973D-D25AAB7818BE";
    }
}