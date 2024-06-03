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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Rock" )]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "a2b98b90-6dcb-4049-ad04-353c9b46a113" )]

[assembly: InternalsVisibleTo( "Rock.Blocks" )]
[assembly: InternalsVisibleTo( "Rock.CodeGeneration" )]
[assembly: InternalsVisibleTo( "Rock.Migrations" )]
[assembly: InternalsVisibleTo( "Rock.RealTime.Dynamic" )]
[assembly: InternalsVisibleTo( "Rock.Rest" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Shared" )]
[assembly: InternalsVisibleTo( "Rock.Tests.UnitTests" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Integration" )]
[assembly: InternalsVisibleTo( "Rock.Tests.Performance" )]
[assembly: InternalsVisibleTo( "Rock.Update" )]
[assembly: InternalsVisibleTo( "Rock.WebStartup" )]
[assembly: InternalsVisibleTo( "Rock.AI.OpenAI" )]
[assembly: InternalsVisibleTo( "DynamicProxyGenAssembly2" )] // Used by Moq

// The following type forwardings were setup in Rock 1.13.0
[assembly: TypeForwardedTo( typeof( Rock.RockObsolete ) )]
[assembly: TypeForwardedTo( typeof( Rock.RockDateTime ) )]
[assembly: TypeForwardedTo( typeof( Rock.Utility.RockColor ) )]

// The following type forwardings were setup in Rock 1.14.0
[assembly: TypeForwardedTo( typeof( Rock.Model.Gender ) )]

// The following type forwardings were setup in Rock 1.15.0
[assembly: TypeForwardedTo( typeof( Rock.Model.CommunicationStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.BlockLocation ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ContentControlType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ContentChannelItemStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ContentChannelDateType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.TagType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.DisplayInNavWhen ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.PersistedDatasetDataFormat ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.PersistedDatasetScriptType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SiteType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.CommunicationRecipientStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SegmentCriteria ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.NotificationClassification ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ConnectionRequestViewModelSortProperty ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ConnectionTypeViewMode ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ConnectionWorkflowTriggerType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.GroupRequirementsFilter ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AddressInvalidReason ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AuditType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.CameraBarcodeConfiguration ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ChangeType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ColorDepth ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.FollowingSuggestedStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.Format ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.JobNotificationStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.KioskType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MatchFlag ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MoveType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.NcoaType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.NoteApprovalStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.PrintFrom ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.PrintTo ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.Processed ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.Resolution ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SignatureDocumentStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SignatureType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SpecialRole ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.UpdatedAddressType ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AgeClassification) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AssessmentRequestStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AttendanceGraphBy) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AuthenticationServiceType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.EmailPreference) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.PersonalizationType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrantsSameFamily) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrarOption) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrationCostSummaryType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrationFeeType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrationFieldSource) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RegistrationPersonFieldType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RSVP) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SessionStatus) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SignatureDocumentAction) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.StreakOccurrenceFrequency) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.StreakStructureType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AlertType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.BatchStatus) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.BenevolenceWorkflowTriggerType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.FinancialScheduledTransactionStatus) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MICRStatus) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.TransactionGraphBy) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AppliesToAgeClassification) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AttendanceRecordRequiredForCheckIn) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.AttendanceRule) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.DueDateType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.FilterExpressionType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.GroupCapacityRule) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.GroupMemberStatus) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MeetsGroupRequirement) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MetricNumericDataType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.MetricValueType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ParticipationType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ReportFieldType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.RequirementCheckType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.UnitType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.WorkflowActionFormPersonEntryOption) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.WorkflowLoggingLevel) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.WorkflowTriggerType) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.WorkflowTriggerValueChangeType) )]
[assembly: TypeForwardedTo( typeof( Rock.Utility.TimeIntervalUnit ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ScheduledAttendanceItemMatchesPreference ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.ScheduledAttendanceItemStatus ) )]
[assembly: TypeForwardedTo( typeof( Rock.Model.SchedulerResourceGroupMemberFilterType ) )]

// The following type forwardings were setup in Rock 1.16.6
[assembly: TypeForwardedTo( typeof( Rock.Model.ComparisonType ) )]
