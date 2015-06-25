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
    [Table( "RegistrationTemplateFee" )]
    [DataContract]
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
        /// Gets or sets the registration template identifier.
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
        /// Gets or sets the cost(s) of the fee. Value is stored like: single = 20, multiple = L|20,XL|20,XXL|25
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [MaxLength(400)]
        [DataMember]
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

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
            return Name;
        }

        #endregion

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

    #endregion

    #region Enumerations

    /// <summary>
    /// Flag for how person details should be displayed/required by user
    /// </summary>
    public enum RegistrationFeeType
    {
        /// <summary>
        /// There is one only one option for this fee
        /// </summary>
        Single = 0,

        /// <summary>
        /// There are multiple options available for this fee
        /// </summary>
        Multiple = 1,
    }

    #endregion

}
