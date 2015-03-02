// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given at the specified <see cref="PledgeFrequencyValue"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the amount to be pledged at the specified frequency.
        /// </value>
        /// <remarks>
        /// An example is that a person pledges $100.00 to be given monthly for the next year. This value will be $100.00 and the grand total of the pledge would be $1,200.00
        /// </remarks>
        [DataMember]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the pledge frequency <see cref="Rock.Model.DefinedValue" /> representing how often the pledgor is promising to give the pledge amount.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> or account that the pledge is being directed toward.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> or account that the pledge is being directed toward.
        /// </value>
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
            this.HasOptional( p => p.Account ).WithMany().HasForeignKey( p => p.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PledgeFrequencyValue ).WithMany().HasForeignKey( p => p.PledgeFrequencyValueId ).WillCascadeOnDelete( false );
        }

    }

    #endregion

}