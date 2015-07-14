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
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "RegistrationRegistrantFee" )]
    [DataContract]
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
        /// Gets or sets the registration template fee identifier.
        /// </summary>
        /// <value>
        /// The registration template fee identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateFeeId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        [DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the option.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration registrant.
        /// </summary>
        /// <value>
        /// The registration registrant.
        /// </value>
        public virtual RegistrationRegistrant RegistrationRegistrant { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee.
        /// </summary>
        /// <value>
        /// The registration template fee.
        /// </value>
        [LavaInclude]
        public virtual RegistrationTemplateFee RegistrationTemplateFee { get; set; }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalCost
        {
            get { return Quantity * Cost; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return RegistrationTemplateFee != null ? RegistrationTemplateFee.Name : "Fee";
        }

        #endregion

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
        }
    }

    #endregion

}
