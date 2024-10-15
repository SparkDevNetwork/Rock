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

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Communication Templates
    /// </summary>
    public static class SystemCommunication
    {
        /// <summary>
        /// Gets the template guid for the Attendance communication
        /// </summary>
        public const string ATTENDANCE_NOTIFICATION = "CA794BD8-25C5-46D9-B7C2-AD8190AC27E6";

        /// <summary>
        /// Gets the template guid for the Exception Notification communication
        /// </summary>
        public const string CONFIG_EXCEPTION_NOTIFICATION = "75CB0A4A-B1C5-4958-ADEB-8621BD231520";

        /// <summary>
        /// Gets the template guid for the Job Notification communication
        /// </summary>
        public const string CONFIG_JOB_NOTIFICATION = "691FEA1B-E5C4-4BF8-A7CD-C588F5C63CA8";

        /// <summary>
        /// Gets the template guid for finance pledge confirmation communication
        /// </summary>
        public const string FINANCE_PLEDGE_CONFIRMATION = "73E8D035-61BB-495A-A87F-39007B98834C";

        /// <summary>
        /// The financial transaction alert notification summary.
        /// <see cref="Rock.Model.FinancialTransactionAlertType.AlertSummaryNotificationGroupId"/>
        /// </summary>
        public const string FINANCIAL_TRANSACTION_ALERT_NOTIFICATION_SUMMARY = "FDF323F5-31FA-4F98-9B4C-A8C26A10840E";

        /// <summary>
        /// Gets the template guid for group attendance reminder
        /// </summary>
        public const string GROUP_ATTENDANCE_REMINDER = "ED567FDE-A3B4-4827-899D-C2740DF3E5DA";

        /// <summary>
        /// Gets the template guid for the Prayer Comments Notification
        /// </summary>
        public const string PRAYER_REQUEST_COMMENTS_NOTIFICATION = "FAEA9DE5-62CE-4EEE-960B-C06103E97AA9";

        /// <summary>
        /// Gets the template guid for event registration confirmation
        /// </summary>
        public const string REGISTRATION_CONFIRMATION = "7B0F4F06-69BD-4CB4-BD04-8DA3779D5259";

        /// <summary>
        /// Gets the template guid for event registration notification
        /// </summary>
        public const string REGISTRATION_NOTIFICATION = "158607D1-0772-4947-ADD6-EA31AB6ABC2F";

        /// <summary>
        /// Gets the template guid for the Account Created communication
        /// </summary>
        public const string SECURITY_ACCOUNT_CREATED = "84e373e9-3aaf-4a31-b3fb-a8e3f0666710";

        /// <summary>
        /// Gets the template guid for the Confirm Account communication
        /// </summary>
        public const string SECURITY_CONFIRM_ACCOUNT = "17aaceef-15ca-4c30-9a3a-11e6cf7e6411";

        /// <summary>
        /// Gets the template guid for the Forgot Username communication
        /// </summary>
        public const string SECURITY_FORGOT_USERNAME = "113593ff-620e-4870-86b1-7a0ec0409208";

        /// <summary>
        /// Gets the template guid for Passwordless Login Confirmation communication
        /// </summary>
        public const string SECURITY_CONFIRM_LOGIN_PASSWORDLESS = "A7AD9FD5-A343-4ADA-868D-A3528D650143";

        /// <summary>
        /// Gets the template guid for Confirm Account (Passwordless) communication
        /// </summary>
        public const string SECURITY_CONFIRM_ACCOUNT_PASSWORDLESS = "543B7C09-80C0-4DAB-8487-10569474D9C7";

        /// <summary>
        /// The scheduling response communication
        /// </summary>
        public const string SCHEDULING_RESPONSE = "D095F78D-A5CF-4EF6-A038-C7B07E250611";

        /// <summary>
        /// The scheduling reminder communication
        /// </summary>
        public const string SCHEDULING_REMINDER = "8A20FE79-B73C-447A-82B1-416F9B50C038";

        /// <summary>
        /// The scheduling confirmation communication
        /// </summary>
        public const string SCHEDULING_CONFIRMATION = "F8E4CE07-68F5-4169-A865-ECE915CF421C";

        /// <summary>
        /// Gets the template guid for workflow form notifications
        /// </summary>
        public const string WORKFLOW_FORM_NOTIFICATION = "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D";

        /// <summary>
        /// Gets the template guid for digital signature invite
        /// </summary>
        [RockObsolete( "1.14" )]
        [Obsolete( "No longer used" )]
        public const string DIGITAL_SIGNATURE_INVITE = "791F2DE4-5A59-60AE-4F2F-FDC3EBC4FFA9";

        /// <summary>
        /// Gets the template guid for the system Electronic Signature Receipt
        /// </summary>
        public const string SYSTEM_ELECTRONIC_SIGNATURE_RECEIPT = "224A0E80-069B-463C-8187-E13682F8A550";

        /// <summary>
        /// Gets the template guid for a notewatch notification
        /// </summary>
        public const string NOTE_WATCH_NOTIFICATION = "21B92DE2-6825-45F3-BD27-43B47FE490D8";

        /// <summary>
        /// Gets the template guid for a note approval notification
        /// </summary>
        public const string NOTE_APPROVAL_NOTIFICATION = "B2E3D75F-681E-430F-82C9-D0D681040FAF";

        /// <summary>
        /// Gets the template guid for a Spark Data notification
        /// </summary>
        public const string SPARK_DATA_NOTIFICATION = "CBCBE0F0-67FB-6393-4D9C-592C839A2E54";

        /// <summary>
        /// Gets the template guid for a Assessment Request system communication
        /// </summary>
        public const string ASSESSMENT_REQUEST = "41FF4269-7B48-40CD-81D4-C11370A13DED";

        #region Communication

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string COMMUNICATION_QUEUE = "2FC7D3E3-D85B-4265-8983-970345215DEA";

        /// <summary>
        /// Gets the template guid for the communication approval email
        /// </summary>
        public const string COMMUNICATION_APPROVAL_EMAIL = "11D325F4-DAF3-4579-A9D8-6347CCC02341";

        /// <summary>
        /// Gets the template guid for the Email Metrics Reminder system communication 
        /// </summary>
        public const string COMMUNICATION_EMAIL_METRICS_REMINDER = "EAE6A169-C01C-4570-92A5-E465B566CFEF";

        #endregion

        #region Finance

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string FINANCE_EXPIRING_CREDIT_CARD = "C07ACD2E-7B9D-400A-810F-BC0EBB9A60DD";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string FINANCE_FAILED_PAYMENT = "449232B5-9C6B-480E-A881-E317D0BC307E";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string FINANCE_GIVING_RECEIPT = "7DBF229E-7DEE-A684-4929-6C37312A0039";

        #endregion

        #region Following

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string FOLLOWING_EVENT = "CA7576CD-0A10-4ADA-A068-62EE598178F5";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string FOLLOWING_SUGGESTION = "8F5A9400-AED2-48A4-B5C8-C9B5D5669F4C";

        #endregion

        #region Groups

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string GROUP_MEMBER_ABSENCE = "8747131E-3EDA-4FB0-A484-C2D2BE3918BA";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string GROUP_REQUIREMENTS = "91EA23C3-2E16-2597-4EAF-27C40D3A66D8";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string GROUP_PENDING_MEMBERS = "18521B26-1C7D-E287-487D-97D176CA4986";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string GROUP_SYNC_WELCOME = "F66D7DAE-89C1-E8BC-48F8-A0D6B849615F";

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string GROUP_SYNC_EXIT = "9AC62F1B-16E0-3886-4CE6-2B9290D6B135";

        /// <summary>
        /// Gets the template guid for the group attendance digest
        /// </summary>
        public const string GROUP_ATTENDANCE_DIGEST = "345CD403-11D2-4B74-A467-ADD15572DD4F";

        #endregion

        /// <summary>
        /// Gets the template guid for 
        /// </summary>
        public const string KIOSK_INFO_UPDATE = "BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881";

        /// <summary>
        /// Gets the template guid for Reminder Notifications.
        /// </summary>
        public const string REMINDER_NOTIFICATION = "7899958C-BC2F-499E-A5CC-11DE1EF8DF20";

        #region Sign-Up Group

        /// <summary>
        /// Gets the template guid for Sign-Up Group Reminders.
        /// </summary>
        public const string SIGNUP_GROUP_REMINDER = "530ADF1E-C2D9-4A67-BADF-5EF65222CA7E";

        /// <summary>
        /// Gets the template guid for Sign-Up Group Registration Confirmations.
        /// </summary>
        public const string SIGNUP_GROUP_REGISTRATION_CONFIRMATION = "B546C11D-6C92-400F-BA56-AAA22D7BAC01";

        #endregion

        /// <summary>
        /// Gets the template guid for a Login Confirmation Alert.
        /// </summary>
        public const string LOGIN_CONFIRMATION_ALERT = "90D986D4-F3B5-4B28-B731-5A3F35172BA9";
    }
}
