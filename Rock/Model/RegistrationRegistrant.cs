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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "RegistrationRegistrant" )]
    [DataContract]
    public partial class RegistrationRegistrant : Model<RegistrationRegistrant>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        [DataMember]
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        [DataMember]
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        public virtual Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group member.
        /// </summary>
        /// <value>
        /// The group member.
        /// </value>
        [LavaInclude]
        public virtual GroupMember GroupMember { get; set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrantFee> Fees
        {
            get { return _fees ?? ( _fees = new Collection<RegistrationRegistrantFee>() ); }
            set { _fees = value; }
        }
        private ICollection<RegistrationRegistrantFee> _fees;

        /// <summary>
        /// Gets the cost with fees.
        /// </summary>
        /// <value>
        /// The cost with fees.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalCost
        {
            get
            {
                var cost = Cost;
                if ( Fees != null )
                {
                    cost += Fees
                        .Sum( f => f.TotalCost );
                }
                return cost;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Discounteds the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedCost( decimal discountPercent, decimal discountAmount )
        {
            var discountedCost = Cost - ( Cost * discountPercent );

            if ( Fees != null )
            {
                foreach( var fee in Fees )
                {
                    discountedCost += fee.DiscountedCost( discountPercent );
                }
            }

            discountedCost = discountedCost - discountAmount;

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PersonAlias != null && PersonAlias.Person != null )
            {
                return PersonAlias.Person.ToString();
            }
            else
            {
                return "Registrant";
            }
        }

        #endregion

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
        }
    }

    #endregion

}
