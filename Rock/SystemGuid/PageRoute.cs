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
    /// Class PageRoute.
    /// </summary>
    public static class PageRoute
    {
        /// <summary>
        /// The CheckinManager route guid
        /// The /checkinmanager route guid
        /// </summary>
        public const string CHECK_IN_MANAGER = "A2B6EA1C-9E46-42C8-ABE4-0FD32D562B48";

        /// <summary>
        /// The /checkinmanager/attendance-detail route guid
        /// </summary>
        public const string CHECK_IN_MANAGER_ATTENDANCE_DETAIL = "FB89DA07-CB29-4786-85FF-8814F43511B2";

        /// <summary>
        /// Gets the Campus Detail > Group Member Detail page route guid
        /// </summary>
        public const string GROUP_MEMBER_DETAIL_CAMPUS_DETAIL = "9660B9FB-C90F-4AFE-9D58-C0EC271C1377";

        /// <summary>
        /// Gets the Text To Give Setup page route guid
        /// </summary>
        public const string TEXT_TO_GIVE_SETUP = "58592279-6CD7-47FD-BD0C-E35784FF69FF";

        /// <summary>
        /// Gets the Registration Template Placement page route guid
        /// </summary>
        public const string REGISTRATION_TEMPLATE_PLACEMENT = "1F51EC08-7287-4A4C-BF36-2CCB57A02EE4";

        /// <summary>
        /// Gets the RSVP Response page route guid.
        /// </summary>
        public const string RSVP = "6BE4EE11-694A-4D3E-B3A7-F6B2946012B9";

        /// <summary>
        /// The phone number lookup
        /// </summary>
        public const string PHONE_NUMBER_LOOKUP = "1FB5A224-9E26-47E6-9A20-5B5A59B5C7CF";

        /// <summary>
        /// The oidc authorize route.
        /// </summary>
        public const string OIDC_AUTHORIZE = "E35CD82E-C162-444E-AC5F-E42F20DA79F3";

        /// <summary>
        /// The oidc logout route.
        /// </summary>
        public const string OIDC_LOGOUT = "5137F15D-61EA-4935-A9E1-16534959AEFA";

        /// <summary>
        /// The edit person route '/Person/{PersonId}/Edit'
        /// </summary>
        public const string EDIT_PERSON_ROUTE = "FCC0CCFF-8E18-48D8-A5EB-3D0F81D68280";

        /// <summary>
        /// The financial account search route 'Account/Search/name/?SearchTerm='
        /// </summary>
        public const string FINANCIAL_ACCOUNT_SEARCH = "76A96F05-5B89-407B-A72E-5CB4FA64A11A";

        /// <summary>
        /// The system communication preview route 'Admin/Communications/System/Preview'
        /// </summary>
        public const string SYSTEM_COMMUNICATION_PREVIEW = "AAC42941-8B2C-4F20-923D-E74146D2E103";

        /// <summary>
        /// The Step Program Flow route 'steps/program/{ProgramId}/flow'
        /// </summary>
        public const string STEP_FLOW = "4F75872B-EBE0-43FA-A8F3-ED716B45A1A6";

        /// <summary>
        /// The external site Workflow Entry route 'WorkflowEntry/{WorkflowTypeGuid}/{WorkflowGuid}'.
        /// </summary>
        public const string EXTERNAL_WORKFLOW_ENTRY_WITH_WORKFLOW = "D8031879-92FD-4782-9AEB-715D6D290434";

        /// <summary>
        /// The external site Workflow Entry route 'WorkflowEntry/{WorkflowTypeGuid}'.
        /// </summary>
        public const string EXTERNAL_WORKFLOW_ENTRY = "ABDBED7B-93F5-4341-8B38-5E96F3009A1E";

        /// <summary>
        /// The library viewer route 'admin/cms/content-library'.
        /// </summary>
        public const string LIBRARY_VIEWER = "36648CBD-A1F6-4DF4-81FB-D36DB0932919";
    }
}
