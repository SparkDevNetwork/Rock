//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Payment Gateway POCO class.
    /// </summary>
    [Table( "PaymentGateway" )]
    [DataContract( IsReference = true )]
    public partial class PaymentGateway : Model<PaymentGateway>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [MaxLength(500)]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the API URL.
        /// </summary>
        /// <value>
        /// The API URL.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret.
        /// </summary>
        /// <value>
        /// The API secret.
        /// </value>
        [MaxLength(100)]
        [DataMember]
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransaction> Transactions { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Payment Gateway Configuration class.
    /// </summary>
    public partial class PaymentGatewayConfiguration : EntityTypeConfiguration<PaymentGateway>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentGatewayConfiguration"/> class.
        /// </summary>
        public PaymentGatewayConfiguration()
        {
        }
    }
}