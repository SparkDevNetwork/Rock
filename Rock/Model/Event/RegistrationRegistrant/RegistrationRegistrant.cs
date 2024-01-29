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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
    [Table( "RegistrationRegistrant" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "8A25E5CE-1B4F-4825-BCEA-216167836305" )]
    public partial class RegistrationRegistrant : Model<RegistrationRegistrant>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Registration"/> identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        [DataMember]
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMember"/> identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplate"/> identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registrant is on a wait list.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on wait list]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool OnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the registration's discount code applies to this registrant.
        /// </summary>
        /// <value>
        /// The discount applies.
        /// </value>
        [DataMember]
        public bool DiscountApplies
        {
            get { return _discountApplies; }
            set { _discountApplies = value; }
        }

        private bool _discountApplies = true;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Registration"/>.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [LavaVisible]
        public virtual Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMember"/>.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        [LavaVisible]
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [NotMapped]
        public virtual int? PersonId
        {
            get
            {
                return PersonAlias != null ? PersonAlias.PersonId : ( int? ) null;
            }
        }

        /// <summary>
        /// Gets or sets the optional <see cref="Rock.Model.SignatureDocument"/> that may be associated with the Registrant.
        /// </summary>
        /// <value>
        /// The signature document.
        /// </value>
        public virtual SignatureDocument SignatureDocument { get; set; }

        /// <summary>
        /// Gets or sets the id of the optional <see cref="Rock.Model.SignatureDocument"/> that may be associated with the Registrant.
        /// </summary>
        /// <value>
        /// The signature document.
        /// </value>
        [DataMember]
        public int? SignatureDocumentId { get; set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrantFee> Fees
        {
            get
            {
                return _fees ?? ( _fees = new Collection<RegistrationRegistrantFee>() );
            }

            set
            {
                _fees = value;
            }
        }

        private ICollection<RegistrationRegistrantFee> _fees;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationRegistrantConfiguration : EntityTypeConfiguration<RegistrationRegistrant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationRegistrantConfiguration"/> class.
        /// </summary> 
        public RegistrationRegistrantConfiguration()
        {
            this.HasRequired( r => r.Registration ).WithMany( t => t.Registrants ).HasForeignKey( r => r.RegistrationId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.GroupMember ).WithMany().HasForeignKey( r => r.GroupMemberId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.SignatureDocument ).WithMany().HasForeignKey( r => r.SignatureDocumentId ).WillCascadeOnDelete ( false );
        }
    }

    #endregion
}
