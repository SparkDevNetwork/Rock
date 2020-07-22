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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial pledge that an individual has made to be given to the specified <see cref="Rock.Model.FinancialAccount"/>/account.  This includes
    /// the account that the pledge is directed to, the amount, the pledge frequency and the time period for the pledge.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialPledge" )]
    [DataContract]
    public partial class FinancialPledge : Model<FinancialPledge>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// If a person belongs to one or more groups a particular type (i.e. Family), this field 
        /// is used to distinguish which group the pledge should be associated with.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the total amount to be pledged.
        /// </value>
        /// <remarks>
        /// An example is that a person pledges $100.00 to be given monthly for the next year. This value will be $1,200.00 to indicate the total amount that is expected.
        /// </remarks>
        [DataMember]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the pledge frequency <see cref="Rock.Model.DefinedValue" /> representing how often the pledgor is promising to give a portion of the pledge amount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the pledge frequency <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_FREQUENCY )]
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date of the pledge period.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the end date of the pledge period.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets the start date key.
        /// </summary>
        /// <value>
        /// The start date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int StartDateKey
        {
            get => StartDate.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        /// <summary>
        /// Gets the end date key.
        /// </summary>
        /// <value>
        /// The end date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int EndDateKey
        {
            get => EndDate.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> or account that the pledge is being directed toward.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> or account that the pledge is being directed toward.
        /// </value>
        [LavaInclude]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the pledge frequency <see cref="Rock.Model.DefinedValue"/>. This is how often the <see cref="Rock.Model.Person"/> who is 
        /// making the pledge promises to give the <see cref="TotalAmount"/>
        /// </summary>
        /// <value>
        /// The frequency of the pledge
        /// </value>
        [DataMember]
        public virtual DefinedValue PledgeFrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the start source date.
        /// </summary>
        /// <value>
        /// The start source date.
        /// </value>
        [DataMember]
        public AnalyticsSourceDate StartSourceDate { get; set; }

        /// <summary>
        /// Gets or sets the end source date.
        /// </summary>
        /// <value>
        /// The end source date.
        /// </value>
        [DataMember]
        public AnalyticsSourceDate EndSourceDate { get; set; }
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this pledge.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this pledge.
        /// </returns>
        public override string ToString()
        {
            return this.TotalAmount.ToStringSafe();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result && TotalAmount < 0 )
                {
                    this.ValidationResults.Add( new ValidationResult( "Total Amount can't be negative." ) );
                    return false;
                }

                return result;
            }
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Pledge Configuration class.
    /// </summary>
    public partial class FinancialPledgeConfiguration : EntityTypeConfiguration<FinancialPledge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPledgeConfiguration"/> class.
        /// </summary>
        public FinancialPledgeConfiguration()
        {
            this.HasOptional( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Account ).WithMany().HasForeignKey( p => p.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PledgeFrequencyValue ).WithMany().HasForeignKey( p => p.PledgeFrequencyValueId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( r => r.StartSourceDate ).WithMany().HasForeignKey( r => r.StartDateKey ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.EndSourceDate ).WithMany().HasForeignKey( r => r.EndDateKey ).WillCascadeOnDelete( false );
        }

    }

    #endregion

}