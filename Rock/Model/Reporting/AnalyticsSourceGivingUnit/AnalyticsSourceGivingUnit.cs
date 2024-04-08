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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Analytics table for the Giving Units involved in <see cref="Rock.Jobs.GivingAutomation"/>. 
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceGivingUnit" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "05103BCB-B164-4591-9129-F949A58C04B1")]
    public class AnalyticsSourceGivingUnit : Entity<AnalyticsSourceGivingUnit>
    {
        #region Entity Properties

        /// <summary>
        /// See <see cref="Person.GivingId"/>.
        /// </summary>
        /// <value>
        /// The giving identifier.
        /// </value>
        [DataMember]
        [MaxLength( 20 )]
        public string GivingId { get; set; }

        /// <summary>
        /// See <seealso cref="Person.GivingLeaderId"/>.
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        [DataMember]
        public int GivingLeaderPersonId { get; set; }

        /// <summary>
        /// See <seealso cref="Group.GroupSalutation"/>
        /// </summary>
        [DataMember]
        public string GivingSalutation { get; set; }

        /// <summary>
        /// See <seealso cref="Group.GroupSalutationFull"/>
        /// </summary>
        [DataMember]
        public string GivingSalutationFull { get; set; }

        /// <summary>
        /// The bin that this person's giving habits fall within.
        /// The logic for this is in <see cref="Rock.Jobs.GivingAutomation" />
        /// </summary>
        /// <value>
        /// The giving bin.
        /// </value>
        [DataMember]
        public int GivingBin { get; set; }

        /// <summary>
        /// Giving Percentile - Number - This will be rounded to the nearest percent and stored as a whole number (15 vs .15)
        /// </summary>
        [DataMember]
        public int GivingPercentile { get; set; }

        /// <summary>
        /// The median gift amount given in the past 12 months.
        /// </summary>
        /// <value>
        /// The gift amount median.
        /// </value>
        [DataMember]
        public decimal GiftAmountMedian { get; set; }

        /// <summary>
        /// The gift amount interquartile range calculated from the past 12 months of giving.
        /// </summary>
        /// <value>
        /// The gift amount IQR.
        /// </value>
        [DataMember]
        public decimal GiftAmountIqr { get; set; }

        /// <summary>
        /// The mean days between gifts given in the past 12 months.
        /// </summary>
        /// <value>
        /// The gift frequency mean.
        /// </value>
        [DataMember]
        public decimal GiftFrequencyMean { get; set; }

        /// <summary>
        /// The standard deviation for the number of days between gifts given in the past 12 months.
        /// </summary>
        /// <value>
        /// The gift frequency standard deviation.
        /// </value>
        [DataMember]
        public decimal GiftFrequencyStandardDeviation { get; set; }

        /// <summary>
        /// The percent of gifts in the past 12 months that have been part of a scheduled transaction. Note that this is stored as an integer. Ex: 15% is stored as 15.
        /// </summary>
        /// <value>
        /// The percent gifts scheduled.
        /// </value>
        [DataMember]
        public int PercentGiftsScheduled { get; set; }

        /// <summary>
        /// The frequency that this person typically has given in the past 12 months.
        /// <see cref="Rock.Financial.FinancialGivingAnalyticsFrequencyLabel" />
        /// Possible values include: Weekly, Bi-Weekly, Monthly, Quarterly, Erratic, Variable
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string Frequency { get; set; }

        /// <summary>
        /// The DefinedValueId of <see cref="PreferredCurrency"/>
        /// </summary>
        /// <value>
        /// The preferred currency value identifier.
        /// </value>
        [DataMember]
        public int PreferredCurrencyValueId { get; set; }

        /// <summary>
        /// The most used means of giving that this person employed in the past 12 months.
        /// This would be the DefinedValue.Value of <seealso cref="PreferredCurrencyValueId"/>
        /// </summary>
        /// <value>
        /// The preferred currency.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string PreferredCurrency { get; set; }

        /// <summary>
        /// The DefinedValueId of <see cref="PreferredSource"/>
        /// </summary>
        /// <value>
        /// The preferred source value identifier.
        /// </value>
        [DataMember]
        public int PreferredSourceValueId { get; set; }

        /// <summary>
        /// The most used giving source (kiosk, app, web) that this person employed in the past 12 months.
        /// This would be the DefinedValue.Value of <seealso cref="PreferredSourceValueId"/>
        /// </summary>
        /// <value>
        /// The preferred source.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string PreferredSource { get; set; }

        #endregion Entity Properties
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceGivingUnit Configuration Class
    /// </summary>
    public partial class AnalyticsSourceGivingUnitConfiguration : EntityTypeConfiguration<AnalyticsSourceGivingUnit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceGivingUnitConfiguration"/> class.
        /// </summary>
        public AnalyticsSourceGivingUnitConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
