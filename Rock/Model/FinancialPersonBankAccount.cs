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
    /// Represents a relationship between a person and a bank account in Rock. A person can be related to multiple bank accounts
    /// but a bank account can only be related to an individual person in Rock.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialPersonBankAccount" )]
    [DataContract]
    public partial class FinancialPersonBankAccount : Model<FinancialPersonBankAccount>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets hash of the Checking Account AccountNumber.  Stored as a SHA1 hash so that it can be matched without being known
        /// Enables a match of a Check Account to Person ( or Persons if multiple persons share a checking account) can be made
        /// </summary>
        /// <value>
        /// AccountNumberSecured.
        /// </value>
        [Required]
        [MaxLength( 128 )]
        [HideFromReporting]
        public string AccountNumberSecured { get; set; }

        /// <summary>
        /// Gets or sets the Masked Account Number (Last 4 of Account Number prefixed with 12 *'s)
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        [Required]
        public string AccountNumberMasked { get; set; }

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
            return this.AccountNumberMasked;
        }

        /// <summary>
        /// Encodes the account number.
        /// </summary>
        /// <param name="routingNumber">The routing number.</param>
        /// <param name="accountNumber">The account number.</param>
        /// <returns></returns>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Account encoding requires a 'PasswordKey' app setting</exception>
        public static string EncodeAccountNumber( string routingNumber, string accountNumber )
        {
            string toHash = string.Format( "{0}|{1}", routingNumber.Trim(), accountNumber.Trim() );
            return Rock.Security.Encryption.GetSHA1Hash( toHash );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialPersonBankAccount Configuration class.
    /// </summary>
    public partial class FinancialPersonBankAccountConfiguration : EntityTypeConfiguration<FinancialPersonBankAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonBankAccountConfiguration"/> class.
        /// </summary>
        public FinancialPersonBankAccountConfiguration()
        {
            this.HasRequired( b => b.PersonAlias ).WithMany().HasForeignKey( b => b.PersonAliasId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}