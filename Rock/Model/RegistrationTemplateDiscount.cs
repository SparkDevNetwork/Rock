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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
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

        /// <summary>
        /// Gets or sets the maximum number of registrations that can use this discount code.
        /// </summary>
        /// <value>
        /// The maximum usage.
        /// </value>
        [DataMember]
        public int? MaxUsage { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of registrants per registration that the discount code can used for.
        /// </summary>
        /// <value>
        /// The maximum registrants.
        /// </value>
        [DataMember]
        public int? MaxRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of registrants a registration is required to have in order to be able to use this discount code.
        /// </summary>
        /// <value>
        /// The minimum registrants.
        /// </value>
        [DataMember]
        public int? MinRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the first day that the discount code can be used.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the last day that the discount code can be used
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the discount applies automatically.
        /// </summary>
        /// <value>
        /// <c>true</c> if this discount applies automatically; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AutoApplyDiscount { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        [LavaInclude]
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets the discount string.
        /// </summary>
        /// <value>
        /// The discount string.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string DiscountString
        {
            get
            {
                if ( DiscountAmount != 0.0m )
                {
                    return DiscountAmount.FormatAsCurrency();
                }
                else if ( DiscountPercentage != 0.0m )
                {
                    return DiscountPercentage.ToString( "P0" );
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// String representation of any discount limits.
        /// </summary>
        /// <value>
        /// The discount limits string.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string DiscountLimitsString
        {
            get
            {
                var limits = new List<string>();

                if ( MaxUsage.HasValue )
                {
                    limits.Add( string.Format( "Max Usage: {0}", MaxUsage.Value ) );
                }
                if ( MaxRegistrants.HasValue )
                {
                    limits.Add( string.Format( "Max Registrants: {0}", MaxRegistrants.Value ) );
                }
                if ( MinRegistrants.HasValue )
                {
                    limits.Add( string.Format( "Min Registrants: {0}", MinRegistrants.Value ) );
                }
                if ( StartDate.HasValue )
                {
                    limits.Add( string.Format( "Effective: {0}", StartDate.Value.ToShortDateString() ) );
                }
                if ( EndDate.HasValue )
                {
                    limits.Add( string.Format( "Expires: {0}", EndDate.Value.ToShortDateString() ) );
                }

                return limits.AsDelimited( "; " );
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
