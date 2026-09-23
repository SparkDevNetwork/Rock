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

using Rock.Data;
using Rock.Enums.Security;
using Rock.Lava;
using Rock.Security;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a kind of content module, defined by Lava templates, that can be placed on a page.
    /// </summary>
    /// <remarks>
    /// The settings a module of this type can be configured with are attributes of
    /// <see cref="Rock.Model.ModuleInstance"/>, qualified by <see cref="Rock.Model.ModuleInstance.ModuleTypeId"/>.
    /// </remarks>
    [RockDomain( "CMS" )]
    [Table( "ModuleType" )]
    [DataContract]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Do not generate a v1 API controller.
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MODULE_TYPE )]
    public partial class ModuleType : Model<ModuleType>, ICategorized, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the module type.
        /// </summary>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [StringValidation( StringValidationProfile.Name )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown for the module type.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        [StringValidation( StringValidationProfile.PlainText )]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Category"/> the module type belongs to.
        /// </summary>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a module of this type holds a list of items, such as slides or cards.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool AreItemsSupported { get; set; }

        /// <summary>
        /// Gets or sets the term used for this module type's items, such as Slides, Links, or Cards.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        [StringValidation( StringValidationProfile.PlainText )]
        public string ItemTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a module of this type can be personalized for different audiences.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsPersonalizationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Lava template that renders a module of this type on the web.
        /// </summary>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string WebLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Lava template that renders a module of this type in a mobile application.
        /// </summary>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string MobileLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the additional settings JSON.
        /// </summary>
        [DataMember]
        [StringValidation( StringValidationProfile.Unrestricted )]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> the module type belongs to.
        /// </summary>
        [DataMember]
        [LavaVisible]
        public virtual Category Category { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Returns the name of the module type.
        /// </summary>
        /// <returns>The name of the module type.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// ModuleType Configuration class.
    /// </summary>
    public partial class ModuleTypeConfiguration : EntityTypeConfiguration<ModuleType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleTypeConfiguration"/> class.
        /// </summary>
        public ModuleTypeConfiguration()
        {
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
