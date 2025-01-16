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
    [Table( "RegistrationTemplateFeeItem" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "CC1DB3FF-A145-49CE-ADC0-B8960EBF37D4")]
    public partial class RegistrationTemplateFeeItem : Model<RegistrationTemplateFeeItem>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplateFee"/> identifier.
        /// </summary>
        /// <value>
        /// The registration template fee identifier.
        /// </value>
        [DataMember]
        public int? RegistrationTemplateFeeId { get; set; }

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
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times this fee item can be used per registration instance.
        /// </summary>
        /// <value>
        /// The maximum available.
        /// </value>
        [DataMember]
        public int? MaximumUsageCount { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplateFee"/>.
        /// </summary>
        /// <value>
        /// The registration template fee.
        /// </value>
        [LavaVisible]
        public virtual RegistrationTemplateFee RegistrationTemplateFee { get; set; }

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
    public partial class RegistrationTemplateFeeItemConfiguration : EntityTypeConfiguration<RegistrationTemplateFeeItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateFeeItemConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateFeeItemConfiguration()
        {
            this.HasRequired( f => f.RegistrationTemplateFee ).WithMany( t => t.FeeItems ).HasForeignKey( f => f.RegistrationTemplateFeeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
