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
    [Table( "RegistrationTemplateDiscount" )]
    [DataContract]
    public partial class RegistrationTemplateDiscount : Model<RegistrationTemplateDiscount>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [DataMember]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets the discount string.
        /// </summary>
        /// <value>
        /// The discount string.
        /// </value>
        [NotMapped]
        public virtual string DiscountString
        {
            get
            {
                if ( DiscountAmount != 0.0m )
                {
                    return DiscountAmount.ToString( "C2" );
                }
                else if ( DiscountPercentage != 0.0m )
                {
                    return DiscountPercentage.ToString( "P0" );
                }
                return string.Empty;
            }
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
            return Code;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplateDiscountConfiguration : EntityTypeConfiguration<RegistrationTemplateDiscount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateDiscountConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateDiscountConfiguration()
        {
            this.HasRequired( d => d.RegistrationTemplate ).WithMany( t => t.Discounts ).HasForeignKey( d => d.RegistrationTemplateId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
