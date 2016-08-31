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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Email Templates
    /// </summary>
    public static class SystemEmail
    {
        /// <summary>
        /// Gets the template guid for the Exception Notification email
        /// </summary>
        public const string CONFIG_EXCEPTION_NOTIFICATION = "75CB0A4A-B1C5-4958-ADEB-8621BD231520";

        /// <summary>
        /// Gets the template guid for finance pledge confirmation email
        /// </summary>
        public const string FINANCE_PLEDGE_CONFIRMATION = "73E8D035-61BB-495A-A87F-39007B98834C";

        /// <summary>
        /// Gets the template guid for group attendance reminder
        /// </summary>
        public const string GROUP_ATTENDANCE_REMINDER = "ED567FDE-A3B4-4827-899D-C2740DF3E5DA";

        /// <summary>
        /// Gets the template guid for event registration confirmation
        /// </summary>
        public const string REGISTRATION_CONFIRMATION = "7B0F4F06-69BD-4CB4-BD04-8DA3779D5259";

        /// <summary>
        /// Gets the template guid for event registration notification
        /// </summary>
        public const string REGISTRATION_NOTIFICATION = "158607D1-0772-4947-ADD6-EA31AB6ABC2F";

        /// <summary>
        /// Gets the template guid for the Account Created email
        /// </summary>
        public const string SECURITY_ACCOUNT_CREATED = "84e373e9-3aaf-4a31-b3fb-a8e3f0666710";

        /// <summary>
        /// Gets the template guid for the Confirm Account email
        /// </summary>
        public const string SECURITY_CONFIRM_ACCOUNT = "17aaceef-15ca-4c30-9a3a-11e6cf7e6411";

        /// <summary>
        /// Gets the template guid for the Forgot Username email
        /// </summary>
        public const string SECURITY_FORGOT_USERNAME = "113593ff-620e-4870-86b1-7a0ec0409208";

        /// <summary>
        /// Gets the template guid for workflow form notifications
        /// </summary>
        public const string WORKFLOW_FORM_NOTIFICATION = "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D";

        /// <summary>
        /// Gets the template guid for workflow form notifications
        /// </summary>
        public const string DIGITAL_SIGNATURE_INVITE  = "791F2DE4-5A59-60AE-4F2F-FDC3EBC4FFA9";

    }
}