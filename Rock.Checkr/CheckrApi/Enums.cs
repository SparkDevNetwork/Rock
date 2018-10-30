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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rock.Checkr.CheckrApi
{
    internal class Enums
    {
        /// <summary>
         /// Webhook invitation events.
         /// </summary>
        [JsonConverter( typeof( StringEnumConverter ) )]
        public enum WebhookTypes
        {
            /// <summary>
            /// Invitation created.
            /// </summary>
            [EnumMember( Value = "invitation.created" )]
            InvitationCreated,

            /// <summary>
            /// Invitation completed.
            /// </summary>
            [EnumMember( Value = "invitation.completed" )]
            InvitationCompleted,

            /// <summary>
            /// Invitation expired.
            /// </summary>
            [EnumMember( Value = "invitation.expired" )]
            InvitationExpired,

            /// <summary>
            /// Report created.
            /// </summary>
            [EnumMember( Value = "report.created" )]
            ReportCreated,

            /// <summary>
            /// Report upgraded.
            /// </summary>
            [EnumMember( Value = "report.upgraded" )]
            ReportUpgraded,

            /// <summary>
            /// Report suspended.
            /// </summary>
            [EnumMember( Value = "report.suspended" )]
            ReportSuspended,

            /// <summary>
            /// Report resumed.
            /// </summary>
            [EnumMember( Value = "report.resumed" )]
            ReportResumed,

            /// <summary>
            /// Report disputed.
            /// </summary>
            [EnumMember( Value = "report.disputed" )]
            ReportDisputed,

            /// <summary>
            /// Report pre-adverse action.
            /// </summary>
            [EnumMember( Value = "report.pre_adverse_action" )]
            ReportPreAdverseAction,

            /// <summary>
            /// Report post-adverse action.
            /// </summary>
            [EnumMember( Value = "report.post_adverse_action" )]
            ReportPostAdverseAction,

            /// <summary>
            /// Report engaged.
            /// </summary>
            [EnumMember( Value = "report.engaged" )]
            ReportEngaged,
            
            /// <summary>
            /// Report created.
            /// </summary>
            [EnumMember( Value = "report.completed" )]
            ReportCompleted,

            /// <summary>
            /// Candidate created.
            /// </summary>
            [EnumMember( Value = "candidate.created" )]
            candidateCreated,

            /// <summary>
            /// Candidate ID required.
            /// </summary>
            [EnumMember( Value = "candidate.id_required" )]
            candidateIdRequired,

            /// <summary>
            /// Candidate driver license required.
            /// </summary>
            [EnumMember( Value = "candidate.driver_license_required" )]
            candidateDriverLicenseRequired,

            /// <summary>
            /// Adverse action notice not delivered.
            /// </summary>
            [EnumMember( Value = "adverse_action.notice_not_delivered" )]
            AdverseActNoticeNotDelivered
        }
    }
}
