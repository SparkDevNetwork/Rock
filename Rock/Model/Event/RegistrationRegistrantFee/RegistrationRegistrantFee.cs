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

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationRegistrantFee" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "AB66F63A-3E79-438F-8C52-C9A8C70A0511")]
    public partial class RegistrationRegistrantFee : Model<RegistrationRegistrantFee>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration registrant identifier.
        /// </summary>
        /// <value>
        /// The registration registrant identifier.
        /// </value>
        [DataMember]
        public int RegistrationRegistrantId { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee identifier. See also <seealso cref="RegistrationTemplateFeeItemId"/>.
        /// </summary>
        /// <value>
        /// The registration template fee identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateFeeId { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee item identifier.
        /// </summary>
        /// <value>
        /// The registration template fee item identifier.
        /// </value>
        [DataMember]
        public int? RegistrationTemplateFeeItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        [DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the name of the FeeItem that was selected at the time of the registration. See also <seealso cref="RegistrationTemplateFeeItem"/>.
        /// </summary>
        /// <value>
        /// The option.
        /// </value>
        [DataMember]
        public string Option { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the registration registrant.
        /// </summary>
        /// <value>
        /// The registration registrant.
        /// </value>
        [LavaVisible]
        public virtual RegistrationRegistrant RegistrationRegistrant { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee. See also <seealso cref="RegistrationTemplateFeeItem"/>.
        /// </summary>
        /// <value>
        /// The registration template fee.
        /// </value>
        [LavaVisible]
        public virtual RegistrationTemplateFee RegistrationTemplateFee { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee item.
        /// </summary>
        /// <value>
        /// The registration template fee item.
        /// </value>
        [LavaVisible]
        public virtual RegistrationTemplateFeeItem RegistrationTemplateFeeItem { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationRegistrantFeeConfiguration : EntityTypeConfiguration<RegistrationRegistrantFee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationRegistrantFeeConfiguration"/> class.
        /// </summary>
        public RegistrationRegistrantFeeConfiguration()
        {
            this.HasRequired( f => f.RegistrationRegistrant ).WithMany( t => t.Fees ).HasForeignKey( f => f.RegistrationRegistrantId ).WillCascadeOnDelete( true );
            this.HasRequired( f => f.RegistrationTemplateFee ).WithMany().HasForeignKey( f => f.RegistrationTemplateFeeId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.RegistrationTemplateFeeItem ).WithMany().HasForeignKey( f => f.RegistrationTemplateFeeItemId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
