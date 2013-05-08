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
    /// Person Bank Account POCO class.
    /// </summary>
    [Table( "FinancialPersonBankAccount" )]
    [DataContract]
    public partial class FinancialPersonBankAccount : Model<FinancialPersonBankAccount>
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
        /// Gets or sets hash of the Checking Account AccountNumber.  Stored as a SHA1 hash (always 40 chars) so that it can be matched without being known
        /// Must be Unique (AlternateKey) so that a match of a Check Account to a Person can be made
        /// </summary>
        /// <value>
        /// AccountNumberSecured.
        /// </value>
        [Required]
        [MaxLength( 40 )]
        [AlternateKey]
        public string AccountNumberSecured { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person { get; set; }

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
            return this.AccountNumberSecured.ToString();
        }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// Transaction Configuration class.
    /// </summary>
    public partial class FinancialPersonBankAccountConfiguration : EntityTypeConfiguration<FinancialPersonBankAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonBankAccountConfiguration"/> class.
        /// </summary>
        public FinancialPersonBankAccountConfiguration()
        {
            this.HasRequired( b => b.Person ).WithMany().HasForeignKey( b => b.PersonId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}