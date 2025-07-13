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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Communication;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Communication Flow in Rock.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationFlow" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_FLOW )]
    public partial class CommunicationFlow : Model<CommunicationFlow>, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication Flow (maximum 100 characters).
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active Communication Flow.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Category identifier.
        /// </summary>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how this Communication Flow is triggered.
        /// </summary>
        /// <remarks>
        /// Valid values:
        /// <list type="bullet">
        ///   <item>
        ///     <term>Recurring</term>
        ///     <description>The flow runs on a scheduled pattern (e.g., weekly, monthly).</description>
        ///   </item>
        ///   <item>
        ///     <term>OnDemand</term>
        ///     <description>The flow is triggered externally, such as by a workflow or manual event.</description>
        ///   </item>
        ///   <item>
        ///     <term>OneTime</term>
        ///     <description>The flow is configured to run only once at a scheduled date and time.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        [DataMember]
        public CommunicationFlowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Data View that defines the initial target audience for this Communication Flow.
        /// </summary>
        /// <remarks>
        ///     <para>If specified, the Data View is evaluated once when the Communication Flow initially executes to generate the recipient list.</para>
        ///     <para>If null (typically for On-Demand flows), initial recipients must be provided dynamically by an external component, such as a Workflow.</para>
        ///     <para>Follow-up communications and goal tracking metrics are based on this initial set of recipients.</para>
        ///     <para>Recipients who unsubscribe from the flow are excluded from further communications.</para>
        ///     <para>Conversion progress for recipients is periodically evaluated against this original audience.</para>
        /// </remarks>
        [DataMember]
        public int? TargetAudienceDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the Schedule identifier.
        /// </summary>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the conversion goal type for this Communication Flow.
        /// </summary>
        /// <remarks>
        ///     <para>If specified, the Communication Flow will track the progress of recipients toward this goal.</para>
        ///     <para>If null, the Communication Flow will not track any conversion goals; no goal is desired.</para>
        ///     <para>Additional goal settings (Group Type, etc.) will be stored in <see cref="AdditionalSettingsJson"/>.</para>
        /// </remarks>
        [DataMember]
        public ConversionGoalType? ConversionGoalType { get; set; }

        /// <summary>
        /// Gets or sets the percentage of recipients expected to complete the conversion goal.
        /// </summary>
        /// <value>
        /// The target conversion percent which should be a value between 0 and 100, inclusively.
        /// </value>
        [DataMember]
        public decimal? ConversionGoalTargetPercent { get; set; }

        /// <summary>
        /// Gets or sets the timeframe (in days) for achieving the conversion goal.
        /// </summary>
        /// <remarks>
        /// The conversion goal is met if the target percentage of recipients completes the action (defined by the goal type) within this number of days from the flow's start.
        /// </remarks>
        [DataMember]
        public int? ConversionGoalTimeframeInDays { get; set; }

        /// <summary>
        /// Gets or sets the condition for when a recipient no longer receives messages from this Communication Flow.
        /// </summary>
        [DataMember]
        public ExitConditionType ExitConditionType { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe message (maximum 500 characters).
        /// </summary>
        /// <remarks>
        /// Displayed in the unsubscribe block when a recipient opts out of this Communication Flow.
        /// Applies only to Email or SMS messages.
        /// </remarks>
        [MaxLength( 500 )]
        [DataMember]
        public string UnsubscribeMessage { get; set; }

        /// <inheritdoc />
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region IHasAdditionalSettings Models

        /// <summary>
        /// Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class ConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the "Completed Form" conversion goal settings.
            /// </summary>
            public CompletedFormConversionGoalSettings CompletedFormSettings { get; set; }

            /// <summary>
            /// Gets or sets the "Joined Group Of Type" conversion goal settings.
            /// </summary>
            public JoinedGroupTypeConversionGoalSettings JoinedGroupTypeSettings { get; set; }

            /// <summary>
            /// Gets or sets the "Joined Specific Group" conversion goal settings.
            /// </summary>
            public JoinedGroupConversionGoalSettings JoinedGroupSettings { get; set; }

            /// <summary>
            /// Gets or sets the "Registered" conversion goal settings.
            /// </summary>
            public RegisteredConversionGoalSettings RegisteredSettings { get; set; }

            /// <summary>
            /// Gets or sets the "Completed Step" conversion goal settings.
            /// </summary>
            public TookStepConversionGoalSettings TookStepSettings { get; set; }

            /// <summary>
            /// Gets or sets the "Entered Data View" conversion goal settings.
            /// </summary>
            public EnteredDataViewConversionGoalSettings EnteredDataViewSettings { get; set; }
        }

        /// <summary>
        /// "Completed Form" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class CompletedFormConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Workflow Type used for this conversion goal.
            /// </summary>
            public Guid WorkflowTypeGuid { get; set; }
        }

        /// <summary>
        /// "Joined Group Of Type" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class JoinedGroupTypeConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Group Type used for this conversion goal.
            /// </summary>
            public Guid GroupTypeGuid { get; set; }
        }

        /// <summary>
        /// "Joined Specific Group" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class JoinedGroupConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Group used for this conversion goal.
            /// </summary>
            public Guid GroupGuid { get; set; }
        }

        /// <summary>
        /// "Registered" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class RegisteredConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Registration Instance used for this conversion goal.
            /// </summary>
            public Guid RegistrationInstanceGuid { get; set; }
        }

        /// <summary>
        /// "Completed Step" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class TookStepConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Step Type used for this conversion goal.
            /// </summary>
            public Guid StepTypeGuid { get; set; }
        }

        /// <summary>
        /// "Entered Data View" Conversion Goal Settings
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "18.0" )]
        public class EnteredDataViewConversionGoalSettings
        {
            /// <summary>
            /// Gets or sets the unique identifier of the Data View used for this conversion goal.
            /// </summary>
            public Guid DataViewGuid { get; set; }
        }

        #endregion IHasAdditionalSettings Models

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the Data View used to define the initial target audience for this Communication Flow.
        /// </summary>
        [DataMember]
        public virtual DataView TargetAudienceDataView { get; set; }

        /// <summary>
        /// Gets or sets the Schedule for this Communication Flow.
        /// </summary>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the instances for this Communication Flow.
        /// </summary>
        [DataMember]
        public virtual ICollection<CommunicationFlowInstance> CommunicationFlowInstances
        {
            get
            {
                return _communicationFlowInstances ?? ( _communicationFlowInstances = new Collection<CommunicationFlowInstance>() );
            }
            set
            {
                _communicationFlowInstances = value;
            }
        }

        private ICollection<CommunicationFlowInstance> _communicationFlowInstances;

        /// <summary>
        /// Gets or sets the communications for this Communication Flow.
        /// </summary>
        [DataMember]
        public virtual ICollection<CommunicationFlowCommunication> CommunicationFlowCommunications
        {
            get
            {
                return _communicationFlowCommunications ?? ( _communicationFlowCommunications = new Collection<CommunicationFlowCommunication>() );
            }
            set
            {
                _communicationFlowCommunications = value;
            }
        }

        private ICollection<CommunicationFlowCommunication> _communicationFlowCommunications;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Flow Configuration class.
    /// </summary>
    public partial class CommunicationFlowConfiguration : EntityTypeConfiguration<CommunicationFlow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationFlowConfiguration"/> class.
        /// </summary>
        public CommunicationFlowConfiguration()
        {
            this.HasOptional( c => c.Category ).WithMany().HasForeignKey( c => c.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.Schedule ).WithMany().HasForeignKey( c => c.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.TargetAudienceDataView ).WithMany().HasForeignKey( c => c.TargetAudienceDataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}