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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility.Enums;

namespace Rock.Model
{
    /// <summary>
    /// Financial Transaction Alert Type
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionAlertType" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [System.Diagnostics.DebuggerDisplay( "Name:{Name}, AlertType:{AlertType}, AmountSensitivityScale:{AmountSensitivityScale}, FrequencySensitivityScale:{FrequencySensitivityScale}" )]
    [Rock.SystemGuid.EntityTypeGuid( "2E237B04-5B2A-40F1-8CD3-52673C104305")]
    public partial class FinancialTransactionAlertType : Model<FinancialTransactionAlertType>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the alert type.
        /// </summary>
        /// <value>
        /// The alert type.
        /// </value>
        [DataMember]
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Gets or sets the financial account identifier.
        /// </summary>
        /// <value>The financial account identifier.</value>
        [DataMember]
        public int? FinancialAccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include child financial accounts].
        /// </summary>
        /// <value><c>true</c> if [include child financial accounts]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IncludeChildFinancialAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [continue if matched].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [continue if matched]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ContinueIfMatched { get; set; }

        /// <summary>
        /// Gets or sets the repeat prevention duration (days).
        /// </summary>
        /// <value>
        /// The repeat prevention duration.
        /// </value>
        [DataMember]
        public int? RepeatPreventionDuration { get; set; }

        /// <summary>
        /// Gets or sets the frequency sensitivity scale.
        /// This determines the point where a transaction is considered
        /// significantly later or earlier than usual.
        /// <para>See notes on <see cref="AlertType">Alert Type</see> to
        /// see how this value is used for Gratitude vs Follow-Up alert types.</para>
        /// </summary>
        /// <value>
        /// The frequency sensitivity scale.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 1 )]
        public decimal? FrequencySensitivityScale { get; set; }

        /// <summary>
        /// Gets or sets the amount sensitivity scale.
        /// This determines the point where a transaction amount is considered
        /// significantly larger or smaller than usual.
        /// <para>See notes on <see cref="AlertType">Alert Type</see> to
        /// see how this value is used for Gratitude vs Follow-Up alert types.</para>
        /// </summary>
        /// <value>
        /// The amount sensitivity scale.
        /// </value>
        [DataMember]
        [DecimalPrecision( 6, 2 )]
        public decimal? AmountSensitivityScale { get; set; }

        /// <summary>
        /// Gets or sets the minimum gift amount.
        /// </summary>
        /// <value>
        /// The minimum gift amount.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? MinimumGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum gift amount.
        /// </summary>
        /// <value>
        /// The maximum gift amount.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? MaximumGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the minimum median gift amount.
        /// </summary>
        /// <value>
        /// The minimum median gift amount.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? MinimumMedianGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum median gift amount.
        /// </summary>
        /// <value>
        /// The maximum median gift amount.
        /// </value>
        [DataMember]
        [DecimalPrecision( 18, 2 )]
        public decimal? MaximumMedianGiftAmount { get; set; }

        /// <summary>
        /// Gets or sets the data view identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [DataMember]
        public int? WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SystemCommunication" /> that will be sent to the Donor (<see cref="FinancialTransaction.AuthorizedPersonAlias"/>).
        /// </summary>
        /// <value>
        /// The system communication identifier.
        /// </value>
        [DataMember]
        public int? SystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.SystemCommunication" /> that will be sent to any Account Participants.
        /// <para>Account Participants are stored as <see cref="RelatedEntity" /> with <see cref="RelatedEntity.PurposeKey" />
        /// of <see cref="RelatedEntityPurposeKey.FinancialAccountGivingAlert"/>.</para>
        /// </summary>
        /// <value>The account participant system communication identifier.</value>
        public int? AccountParticipantSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send bus event].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send bus event]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendBusEvent { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
#if REVIEW_WEBFORMS
        [Index]
#endif
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the maximum days since last gift.
        /// </summary>
        /// <value>
        /// The maximum days since last gift.
        /// </value>
        [DataMember]
        public int? MaximumDaysSinceLastGift { get; set; }

        /// <summary>
        /// Gets or sets the run days for this alert type.
        /// Null means all days of the week are run days.
        /// </summary>
        /// <value>
        /// The run days.
        /// </value>
        [DataMember]
        public DayOfWeekFlag? RunDays { get; set; }

        /// <summary>
        /// Gets or sets the alert summary notification group identifier.
        /// </summary>
        /// <value>
        /// The alert summary notification group identifier.
        /// </value>
        [DataMember]
        public int? AlertSummaryNotificationGroupId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the alert summary notification group.
        /// </summary>
        /// <value>
        /// The alert summary notification group.
        /// </value>
        [DataMember]
        public virtual Group AlertSummaryNotificationGroup { get; set; }

        /// <summary>
        /// Gets or sets the campus that this financial transaction alert type is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that the financial transaction alert type is associated with.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the financial account.
        /// </summary>
        /// <value>The financial account.</value>
        [DataMember]
        public virtual FinancialAccount FinancialAccount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DataView"/> that this financial transaction alert type is based on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DataView"/> that this financial transaction alert type is based on.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the connection opportunity.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        /// <inheritdoc cref="SystemCommunicationId"/>
        [DataMember]
        public virtual SystemCommunication SystemCommunication { get; set; }

        /// <inheritdoc cref="AccountParticipantSystemCommunicationId"/>
        [DataMember]
        public virtual SystemCommunication AccountParticipantSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction alerts.
        /// </summary>
        /// <value>
        /// The financial transaction alerts.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransactionAlert> FinancialTransactionAlerts { get; set; } = new Collection<FinancialTransactionAlert>();

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialTransactionAlertType Configuration class.
    /// </summary>
    public partial class FinancialTransactionAlertTypeConfiguration : EntityTypeConfiguration<FinancialTransactionAlertType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAlertTypeConfiguration"/> class.
        /// </summary>
        public FinancialTransactionAlertTypeConfiguration()
        {
            this.HasOptional( t => t.AlertSummaryNotificationGroup ).WithMany().HasForeignKey( t => t.AlertSummaryNotificationGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Campus ).WithMany().HasForeignKey( t => t.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialAccount ).WithMany().HasForeignKey( t => t.FinancialAccountId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.DataView ).WithMany().HasForeignKey( t => t.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.WorkflowType ).WithMany().HasForeignKey( t => t.WorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.ConnectionOpportunity ).WithMany().HasForeignKey( t => t.ConnectionOpportunityId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SystemCommunication ).WithMany().HasForeignKey( t => t.SystemCommunicationId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.AccountParticipantSystemCommunication ).WithMany().HasForeignKey( t => t.AccountParticipantSystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}