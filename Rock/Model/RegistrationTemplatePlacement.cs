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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationTemplatePlacement" )]
    [DataContract]
    public partial class RegistrationTemplatePlacement : Model<RegistrationTemplatePlacement>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the registration template placement.
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
        [Required]
        [DataMember( IsRequired = true )]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/> that this registration template placement is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/> that this registration template placement is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the sort and display order of the registration template placement. This is an ascending order, so the lower the value the higher the sort priority.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the sort order of the registration template placement.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class that is defined for the RegistrationTemplatePlacement.
        /// Use <see cref="GetIconCssClass"/> to get the IconCssClass to use since that GroupType.IconCssClass should be used if this isn't defined
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple placements].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple placements]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultiplePlacements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is limited to administration purposes.
        /// </summary>
        /// <value>
        /// <c>true</c> if this registration template placement is internal; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsInternal { get; set; } = true;

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal? Cost { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationTemplate"/>.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        [DataMember]
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this registration template placement is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/> that this registration template placement is associated with.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the Icon CSS class from either <see cref="IconCssClass"/> or from <see cref="GroupType.IconCssClass" />
        /// </summary>
        /// <returns></returns>
        public string GetIconCssClass()
        {
            if ( this.IconCssClass.IsNotNullOrWhiteSpace() )
            {
                return this.IconCssClass;
            }
            else
            {
                return GroupTypeCache.Get( this.GroupTypeId ).IconCssClass;
            }
        }

        #endregion Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplatePlacementConfiguration : EntityTypeConfiguration<RegistrationTemplatePlacement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplatePlacementConfiguration"/> class.
        /// </summary>
        public RegistrationTemplatePlacementConfiguration()
        {
            this.HasRequired( i => i.RegistrationTemplate ).WithMany( t => t.Placements ).HasForeignKey( i => i.RegistrationTemplateId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany().HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
