//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Person Saved Account POCO class.
    /// </summary>
    [Table( "FinancialPersonSavedAccount" )]
    [DataContract]
    public partial class FinancialPersonSavedAccount : Model<FinancialPersonSavedAccount>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the gateway id.
        /// </summary>
        /// <value>
        /// The gateway id.
        /// </value>
        [DataMember]
        public int? GatewayId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [DataMember]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        /// <value>
        /// The masked account number.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MaskedAccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string TransactionCode { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway Gateway { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.MaskedAccountNumber.ToString();
        }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// Transaction Configuration class.
    /// </summary>
    public partial class FinancialPersonSavedAccountConfiguration : EntityTypeConfiguration<FinancialPersonSavedAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonSavedAccountConfiguration"/> class.
        /// </summary>
        public FinancialPersonSavedAccountConfiguration()
        {
            this.HasRequired( t => t.Person ).WithMany().HasForeignKey( t => t.PersonId ).WillCascadeOnDelete( true );
            this.HasOptional( t => t.Gateway ).WithMany().HasForeignKey( t => t.GatewayId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The type of payment (Credit card or ACH)
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Credit Card
        /// </summary>
        CreditCard = 0,

        /// <summary>
        /// ACH
        /// </summary>
        ACH = 1,
    }

    #endregion
}