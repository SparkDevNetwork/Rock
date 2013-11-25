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
    /// Represents an bank or debit/credit card that a <see cref="Rock.Model.Person"/> has saved to RockChMS for future reuse. Please
    /// note that account number is not actually stored here. The reference/profile number is stored here as well as a masked 
    /// version of the account number.
    /// </summary>
    [Table( "FinancialPersonSavedAccount" )]
    [DataContract]
    public partial class FinancialPersonSavedAccount : Model<FinancialPersonSavedAccount>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the account owner.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is the account holder.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the FinancialTransactionId of the <see cref="Rock.Model.FinancialTransaction"/> that originated the save request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represented the TransactionId.
        /// </value>
        [DataMember]
        public int FinancialTransactionId { get; set; }

        /// <summary>
        /// Gets or sets a reference identifier needed by the payment provider to initiate a future transaction
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the reference identifier to initiate a future transaction.
        /// </value>
        [DataMember]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the saved account. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the account.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a masked version of the account number. This is a value with "*" and a partial account number (usually the last 4 digits).
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the masked account number.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string MaskedAccountNumber { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who is the account owner.
        /// </summary>
        /// <value>
        /// The Account Owner's <see cref="Rock.Model.Person"/> entity.
        /// </value>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction where the account was saved.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> where the account was saved.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction FinancialTransaction { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialPersonSavedAccount.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialPersonSavedAccount.
        /// </returns>
        public override string ToString()
        {
            return this.MaskedAccountNumber.ToString();
        }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// FinancialPersonSavedAccount Configuration class.
    /// </summary>
    public partial class FinancialPersonSavedAccountConfiguration : EntityTypeConfiguration<FinancialPersonSavedAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonSavedAccountConfiguration"/> class.
        /// </summary>
        public FinancialPersonSavedAccountConfiguration()
        {
            this.HasRequired( t => t.Person ).WithMany().HasForeignKey( t => t.PersonId ).WillCascadeOnDelete( true );
            this.HasRequired( t => t.FinancialTransaction ).WithMany().HasForeignKey( t => t.FinancialTransactionId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}