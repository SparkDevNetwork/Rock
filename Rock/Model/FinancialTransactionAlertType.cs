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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility.Enums;

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionAlertType" )]
    [DataContract]
    [System.Diagnostics.DebuggerDisplay( "Name:{Name}, AlertType:{AlertType}, AmountSensitivityScale:{AmountSensitivityScale}, FrequencySensitivityScale:{FrequencySensitivityScale}" )]
    public class FinancialTransactionAlertType : Model<FinancialTransactionAlertType>, IOrdered
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
        [Index]
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

        #region Virtual Properties

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

        #endregion Virtual Properties

        #region Methods

        /// <summary>
        /// Gets a description of what <see cref="AmountSensitivityScale"/> means depending on the AlertType.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <returns>string.</returns>
        public static string GetAmountSensitivityDescription( AlertType alertType )
        {
            /* 11-19-2021 MDP

           AlertType drives the logic of how sensitivity values are used. (See notes and logic here https://github.com/SparkDevNetwork/Rock/blob/6dacabe84dcaf041450c3bc075164c7580151390/Rock/Jobs/GivingAutomation.cs#L1602)

           Follow-Up uses sensitivity to look for 'worse than usual':
               - Gifts with amounts that are significantly smaller than usual. For example, a $50 gift for somebody that usually gives $300 a week.

           Gratitude uses sensitivity to look for 'better than usual':
               - Gifts with amounts that are significantly larger than usual. For example, a $1200 gift for somebody that usually gives $300 a week.

           In both cases, a positive value for sensitivity should be used.
           A negative sensitivity could be entered if they really wanted to, but it'll do weird things such as express gratitude for a gift over $180 for a person that normally gives $200.

            */

            /* Amount Sensitivity */
            if ( alertType == AlertType.Gratitude )
            {
                // Amount Sensitivity Help for Gratitude.
                // Gratitude. Larger amount than usual.
                return @"
 <p>
    For gratitude alerts, the amount sensitivity scale determines when a 'larger amount than usual' gift triggers the alert.
</p>
<p>
    Typical Values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when a gift was more than 2 times the interquartile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $530 was received.</li>
        <li>3 (Normal) - This would alert when a gift was more than 3 times the interquartile range (IQR) from their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $595 was received.</li>
    </ul>
</p>
<p>
    In the event that there is a very consistent giver — every gift is the exact same amount — we use a fallback value. The fallback amount sensitivity is calculated as 15% of the median gift amount.
</p>";
            }
            else
            {
                // Amount Sensitivity Help for Follow-Up
                // Follow-Up. The logic is flipped so the sensitivity is used to determine smaller than usual amounts."
                return @"
 <p>
    For follow-up alerts, the amount sensitivity scale determines when a 'smaller amount than usual' gift triggers the alert.
</p>
<p>
    Typical Values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when a gift is 2 times or more of the interquartile range (IQR) smaller than their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $270 was received.</li>
        <li>3 (Normal) - This would alert when a gift is 3 times or more of the interquartile range (IQR) smaller than their median gift amount. For a bi-weekly giver with a median gift of $400 and an IQR of $65, this alert would be generated if a gift of $205 was received.</li>
    </ul>
</p>
<p>
    In the event that there is a very consistent giver — every gift is the exact same amount — we use a fallback value. The fallback amount sensitivity is calculated as 15% of the median gift amount.
</p>";
            }
        }

        /// <summary>
        /// Gets a description of what <see cref="FrequencySensitivityScale"/> means depending on the AlertType.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <returns>string.</returns>
        public static string GetFrequencySensitivityDescription( AlertType alertType )
        {
            /* 11-19-2021 MDP

            AlertType drives the logic of how sensitivity values are used. (See notes and logic here https://github.com/SparkDevNetwork/Rock/blob/6dacabe84dcaf041450c3bc075164c7580151390/Rock/Jobs/GivingAutomation.cs#L1602)

            Follow-Up uses sensitivity to look for 'worse than usual':
                - Gifts that are significantly late. For example: a bi-weekly giver than hasn't given for several weeks

            Gratitude uses sensitivity to look for 'better than usual':
                - Gifts that are significantly early. For example: a monthly giver that gave 20 days earlier than usual.

            In both cases, a positive value for sensitivity should be used.
            A negative sensitivity could be entered if they really wanted to, but it'll do weird things such as create a follow up (a late gift alert)
            for a consistent weekly giver if it has been 4 or more days since their last gift.

            */

            /* Frequency Sensitivity */
            if ( alertType == AlertType.FollowUp )
            {
                // Follow-Up because gift is late or later than usual.
                // Aggressive is 'a little later than usual'
                // Normal is 'significantly later than usual'
                return @"
<p>
    For a follow-up alert, the frequency sensitivity scale determines when to generate an alert if a gift hasn't been received for a while, or was received later than usual.
</p>
<p>
    Typical Values are shown below.
    <ul>
                             
        <li>2 (Aggressive) - This would alert when the frequency of a gift is later than 2 times of the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, this alert would be generated if no gift was received within 22 days since their last gift.</li>
        <li>3 (Normal) - This would alert when the frequency of a gift is later than 3 times of the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, this alert would be generated if no gift was received within 26 days since their last gift.</li>
    </ul>
</p>
<p>
    In the event that there is a very consistent giver — every gift is the same number of days apart — we use a fallback value. The fallback frequency sensitivity is calculated as 15% of the average days between gifts. If that value is less than 3 days, then we again fallback to 3 days.
</p>
";
            }
            else
            {
                // Gratitude for Earlier than Usual
                // Aggressive is 'a little bit earlier than usual'
                // Normal is 'significantly earlier than usual'
                return @"
<p>
    For a gratitude alert, the frequency sensitivity scale determines when to generate an alert if a gift is earlier than usual.
</p>
<p>
    Typical Values are shown below.
    <ul>
        <li>2 (Aggressive) - This would alert when the frequency of a gift is earlier than 2 times the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, an alert would be generated for a gift if it has been fewer than 10 days since their last gift. (4 days early).</li>
        <li>3 (Normal) -     This would alert when the frequency of a gift is earlier than 3 times the standard deviation. For a bi-weekly giver with a mean of 14 days and a standard deviation of 3.8, an alert would be generated for a gift if it has been fewer than 7 days since their last gift. (7 days early).</li>
    </ul>
</p>
<p>
    In the event that there is a very consistent giver — every gift is the same number of days apart — we use a fallback value. The fallback frequency sensitivity is calculated as 15% of the average days between gifts. If that value is less than 3 days, then we again fallback to 3 days.
</p>
";
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name.IsNullOrWhiteSpace() ? base.ToString() : Name;
        }

        #endregion Methods
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

    #region Enumerations

    /// <summary>
    /// Alert Type
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// Gratitude looks for amounts larger than normal, or transactions that are earlier than usual.
        /// </summary>
        [Description( "Gratitude" )]
        Gratitude = 0,

        /// <summary>
        /// Follow Up looks for amounts smaller than normal, or transactions later than usual (or stopped occurring)
        /// </summary>
        [Description( "Follow-up" )]
        FollowUp = 1,
    }

    #endregion
}