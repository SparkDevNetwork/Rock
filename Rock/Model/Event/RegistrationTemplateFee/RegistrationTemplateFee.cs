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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationTemplateFee" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "2DB3A441-6CA1-49D1-BB25-C744E2FFA457")]
    public partial class RegistrationTemplateFee : Model<RegistrationTemplateFee>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplate"/> identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [DataMember]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the fee type ( single option vs multiple options ).
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [DataMember]
        public RegistrationFeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets the cost(s) of the fee. Value is stored like: single = 20, multiple = L|20,XL|20,XXL|25 or Small^10|Medium^20|Large^30|XXL^40
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [MaxLength( 400 )]
        [DataMember]
        [RockObsolete( "1.9" )]
        [Obsolete( "Use FeeItems instead", true )]
        public string CostValue { get; set; }

        /// <summary>
        /// Discount codes apply to this fee
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public bool DiscountApplies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if registrant can select multiple values for this fee.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide when none remaining].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide when none remaining]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HideWhenNoneRemaining { get; set; } = false;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplate"/>.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        [LavaVisible]
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the fee items.
        /// </summary>
        /// <value>
        /// The fee items.
        /// </value>
        [LavaVisible]
        [DataMember]
        public virtual ICollection<RegistrationTemplateFeeItem> FeeItems { get; set; } = new List<RegistrationTemplateFeeItem>();

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplateFeeConfiguration : EntityTypeConfiguration<RegistrationTemplateFee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateFeeConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateFeeConfiguration()
        {
            this.HasRequired( f => f.RegistrationTemplate ).WithMany( t => t.Fees ).HasForeignKey( f => f.RegistrationTemplateId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
